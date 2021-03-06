﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Isle : MonoBehaviour
{
    public IsleAbstract isleAbstract;
    
    public Vector3 NavMeshPosition;

    public GameObject PlayerStartPoint;

    public Portal PortalUp;
    public Portal PortalUpRight;
    public Portal PortalDownRight;
    public Portal PortalDown;
    public Portal PortalDownLeft;
    public Portal PortalUpLeft;

    public List<ItemPoint> ItemPoints;
    public List<EnemyPoint> EnemyPoints;

    private List<Item> listBoxes; 

    [HideInInspector]
    public List<GameObject> ListEnemies;

    [HideInInspector]
    public Portal[] Portals;

    private GameObject[] borders;

    private ObjectPool mr;
    private LevelManager lvlManager;
    private GameObject playerObject;

    private Coroutine enemySpawnRoutine;
    private Coroutine levelCheckRoutine;

    private System.Random rnd;
    
    public void Initialize(IsleAbstract isle)
    {
        isleAbstract = isle;

        mr = ObjectPool.getObjectPool();
        lvlManager = LevelManager.getLevelManager();
        playerObject = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.player);

        rnd = mr.random;

        ListEnemies = new List<GameObject>();
        listBoxes = new List<Item>();

        Portals = new Portal[6];
        Portals[0] = PortalUp;
        Portals[1] = PortalUpRight;
        Portals[2] = PortalDownRight;
        Portals[3] = PortalDown;
        Portals[4] = PortalDownLeft;
        Portals[5] = PortalUpLeft;

        borders = new GameObject[4];

        // prepare Enemy Spawns
        for (int i = 0; i < EnemyPoints.Count; i++)
        {
            EnemyPoints[i].IslePosition = transform.position;
            EnemyPoints[i].GetComponent<Renderer>().enabled = false;
        }
        
        // hide Player Start Point
        if (PlayerStartPoint != null)
        {
            PlayerStartPoint.SetActive(false);
        }

        // hide Item Spawns
        for (int i = 0; i < ItemPoints.Count; i++)
        {
            ItemPoints[i].GetComponent<Renderer>().enabled = false;
        }

        // Portal stuff
        for (int i = 0; i < 6; i++)
        {
            // hide Portal-Tempaltes
            Portals[i].gameObject.SetActive(false);

            // show real Portals (and remove old ones
            
            if (isleAbstract.Portals[i] != null)
            {
                Portal realPortal = mr.getObject(ObjectPool.categorie.structures, (int)ObjectPool.structures.portal).GetComponent<Portal>();
                realPortal.transform.position = Portals[i].transform.position;
                realPortal.transform.rotation = Portals[i].transform.rotation;
                realPortal.spawnPoint.transform.position = Portals[i].spawnPoint.transform.position;
                realPortal.spawnPoint.SetActive(false);
                realPortal.portalSpiral.gameObject.SetActive(false);
                realPortal.Direction = i;
                isleAbstract.Portals[i].portalObj = realPortal;
                realPortal.portalAbstract = isleAbstract.Portals[i];
                Portals[i] = realPortal;
                realPortal.transform.SetParent(gameObject.transform);
            }
        }

        // set items ---------------------

        int keyPoint =-1;

        if (isleAbstract.isleObjectType == IsleAbstract.IsleObjectType.key)
        {
            keyPoint = rnd.Next(0, ItemPoints.Count);
        }

        for (int i = 0; i < ItemPoints.Count; i++)
        {
            // check for key
            if (i == keyPoint)
            {
                // place key
                GameObject key = null;
                switch (isleAbstract.keyNumber)
                {
                    case 1: key = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.key1);
                        break;
                    case 2: key = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.key2);
                        break;
                    case 3: key = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.key3);
                        break;
                    default: print("Error Key-Isle, but no valid key-Number. " + isleAbstract.keyNumber);
                        break;
                }

                key.GetComponent<Item>().initialize();
                key.transform.position = ItemPoints[i].transform.position;
            }
            else
            {
                // place boxes
                GameObject box = null;
                if (rnd.Next(0, 101) > lvlManager.ChancesBigBoxes)
                {
                    // small box
                    box = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.smallBox);

                }else{
                    // big box
                    box = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.bigBox);
                }

                box.transform.position = ItemPoints[i].transform.position;
                box.GetComponent<Item>().initialize();
     
                listBoxes.Add(box.GetComponent<Item>());
            }
        }
       
    }

    public void StartIsle()
    {
        // clear list
        for (int i = 0; i < ListEnemies.Count; i++)
        {
            mr.returnObject(ListEnemies[i].GetComponent<GhostCopy>().ghost.gameObject);
            mr.returnObject(ListEnemies[i]);
        }
        ListEnemies.Clear();

        for (int i = 0; i < EnemyPoints.Count; i++)
        {
            EnemyPoints[i].Initialize();

            if (EnemyPoints[i].CanCreateEnemy == true)
            {
                ExplosionScript expl = mr.getObject(ObjectPool.categorie.explosion, (int)ObjectPool.explosion.enemySpawn).GetComponent<ExplosionScript>();
                expl.Initialize(EnemyPoints[i].transform.position);
            }
        }

        enemySpawnRoutine = StartCoroutine(EnemySpawnHandler());

        UI_Canvas ui = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>();
        ui.ShowMessage("Destroy all enemies!");
    }

    public IEnumerator EnemySpawnHandler()
    {
        yield return new WaitForSeconds(1);

        // create enemies
        for (int i = 0; i < EnemyPoints.Count; i++)
        {
            GameObject enemy = EnemyPoints[i].createEnemy();
            if (enemy != null)
            {
                enemy.GetComponent<Enemy>().Initialize(EnemyPoints[i], transform.position, NavMeshPosition, playerObject.GetComponent<NavMeshTarget>());

                ListEnemies.Add(enemy);
            }

            EnemyPoints[i].gameObject.SetActive(false);

        }

        playerObject.GetComponent<NavMeshTarget>().IslePosition = transform.position;

        levelCheckRoutine = StartCoroutine(LevelCheckHandler());

        StopCoroutine(enemySpawnRoutine);
    }

    public IEnumerator LevelCheckHandler()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (ListEnemies.Count <= 0)
            {
                // Level finished

                UI_Canvas ui = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>();
                ui.ShowMessage("Isle cleared!\nPortals are open now!");

                UnlockPortals();

                UnlockSmallBoxes();

                isleAbstract.finished = true;

                StopCoroutine(levelCheckRoutine);

                yield return null;
            }
        }
    }

    public void LockPortals()
    {
        for (int i = 0; i < 6; i++)
        {
            if (isleAbstract.Portals[i] != null)
            {
                Portals[i].PortalActivated = false;
                Portals[i].portalSpiral.gameObject.SetActive(false);
            }
        }
    }

    public void UnlockPortals()
    {
        for (int i = 0; i < 6; i++)
        {
            if (isleAbstract.Portals[i] != null)
            {
                Portals[i].PortalActivated = true;
                Portals[i].portalSpiral.gameObject.SetActive(true);
            }
        }
    }

    public void UnlockSmallBoxes()
    {
        for(int i = 0; i < listBoxes.Count; i++)
        {
            if (listBoxes[i].Type == Item.ItemType.SmallBox)
            {
                Item box = listBoxes[i];

                box.OpenSmallBox();

            }
        }
    }

    public void AddBorders()
    {
        RemoveBorders();

        // upper side
        borders[0] = mr.getObject(ObjectPool.categorie.planes, (int)ObjectPool.planes.borderPlane);
        borders[0].transform.position = transform.position + new Vector3(0, 0, lvlManager.Fieldwidth / 2);
        borders[0].transform.rotation = Quaternion.Euler(0, 0, 0);
        borders[0].GetComponent<Renderer>().material.color = Color.blue;
        // right sight
        borders[1] = mr.getObject(ObjectPool.categorie.planes, (int)ObjectPool.planes.borderPlane);
        borders[1].transform.position = transform.position + new Vector3(lvlManager.Fieldwidth / 2, 0, 0);
        borders[1].transform.rotation = Quaternion.Euler(0, 90, 0);
        borders[1].GetComponent<Renderer>().material.color = Color.red;
        // bottom side
        borders[2] = mr.getObject(ObjectPool.categorie.planes, (int)ObjectPool.planes.borderPlane);
        borders[2].transform.position = transform.position + new Vector3(0, 0, - (lvlManager.Fieldwidth / 2));
        borders[2].transform.rotation = Quaternion.Euler(0, 0, 0);
        borders[2].GetComponent<Renderer>().material.color = Color.yellow;
        // left side
        borders[3] = mr.getObject(ObjectPool.categorie.planes, (int)ObjectPool.planes.borderPlane);
        borders[3].transform.position = transform.position + new Vector3(-(lvlManager.Fieldwidth / 2), 0, 0);
        borders[3].transform.rotation = Quaternion.Euler(0, 90, 0);
        borders[3].GetComponent<Renderer>().material.color = Color.green;
    }

    public void RemoveBorders()
    {
        for (int i = 0; i < borders.Length; i++)
        {
            if (borders[i] != null)
            {
                mr.returnObject(borders[i]);
            }
            borders[i] = null;

        }
    }
}
