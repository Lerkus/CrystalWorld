using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item : MonoBehaviour {

    public enum ItemType { PortalKey1, PortalKey2, PortalKey3, BigBox, SmallBox }
    public ItemType Type;

    public enum ContentTypeSmall { SmallKey, SpeedUpgrade, DamageUpgrade, RateUpgrade, ShotSpeedUpgrade };
    private ContentTypeSmall ContentSmall;

    public enum ContentTypeBig { SpeedUpgrade, DamageUpgrade, RateUpgrade, ShotSpeedUpgrade, Splitter, Bluffer };
    private ContentTypeBig ContentBig;

    [HideInInspector]
    public GameObject ContentObj;

    [HideInInspector]
    public bool collected;
    [HideInInspector]
    public bool opened;

    private ObjectPool mr;
    private LevelManager lvlManager;
    private UI_Canvas ui;

    private Coroutine checkTeleportFinished;
    private Coroutine checkBigBoxFinished;

    private List<lerpInfo> lerpList;

    public Animator theTresure;


    public void initialize()
    {
        mr = ObjectPool.getObjectPool();
        lvlManager = LevelManager.getLevelManager();
        ui = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>();

        lerpList = new List<lerpInfo>();

        collected = false;
        opened = false;

        /*if (Type == ItemType.SmallBox || Type == ItemType.BigBox)
        {
            gameObject.GetComponent<Renderer>().material.color = new Color(0.296f, 0.141f, 0.057f, 1);
        }*/
        if (Type == ItemType.SmallBox)
        {
            generateSmallBoxContent();
            theTresure = gameObject.GetComponentInChildren<Animator>();

        }
        if (Type == ItemType.BigBox)
        {
            generateBigBoxContent();
            theTresure = gameObject.GetComponentInChildren<Animator>();
        }

    }

    private void generateSmallBoxContent()
    {

        if (ContentObj != null)
        {
            mr.returnObject(ContentObj);
            ContentObj = null;
        }

        int tmp = mr.random.Next(0, 5);

        ContentSmall = (ContentTypeSmall)tmp;

        switch (ContentSmall)
        {
            case ContentTypeSmall.SmallKey: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.smallKey);
                break;
            case ContentTypeSmall.SpeedUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeSpeed);
                break;
            case ContentTypeSmall.DamageUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeDamage);
                break;
            case ContentTypeSmall.RateUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeRate);
                break;
            case ContentTypeSmall.ShotSpeedUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeShotSpeed);
                break;
            default: print("Error: No valid content for small box");
                break;
        }

        if (ContentSmall != ContentTypeSmall.SmallKey)
        {
            ContentObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        ContentObj.transform.position = transform.position;

    }

    private void generateBigBoxContent()
    {
        if (ContentObj != null)
        {
            mr.returnObject(ContentObj);
            ContentObj = null;
        }

        int tmp = mr.random.Next(0, 6);

        ContentBig = (ContentTypeBig)tmp;
   
        switch (ContentBig)
        {
            case ContentTypeBig.SpeedUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeSpeed);
                break;
            case ContentTypeBig.DamageUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeDamage);
                break;
            case ContentTypeBig.RateUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeRate);
                break;
            case ContentTypeBig.ShotSpeedUpgrade: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.upgradeShotSpeed);
                break;
            case ContentTypeBig.Splitter: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.splitter);
                break;
            case ContentTypeBig.Bluffer: ContentObj = mr.getObject(ObjectPool.categorie.items, (int)ObjectPool.items.bluffer);
                break;
            default:
                print("Error: No valid content for big box");
                break;
        }

        ContentObj.transform.localScale = new Vector3(1, 1, 1);
        ContentObj.transform.position = transform.position;
    }        

    public void OpenSmallBox()
    {
        opened = true;
        
        theTresure.SetBool("isOpen", true);

        if (ContentObj != null)
        {
            ContentObj.GetComponent<Lerper>().StartLerp(transform.position, transform.position + new Vector3(0, 2, 0), 0.5f);
        }

    }

    public void OpenBigBox()
    {
        opened = true;
        collected = true;

        theTresure.SetBool("IsOpen", true);

        if (ContentObj != null)
        {
            ContentObj.GetComponent<Lerper>().StartLerp(transform.position, transform.position + new Vector3(0, 3, 0), 1.3f);
        }

        checkBigBoxFinished = StartCoroutine(checkBigBoxFinishedHandler());
    }

    public IEnumerator checkBigBoxFinishedHandler()
    {
        while(true)
        {
            if (ContentObj.GetComponent<Lerper>().Lerping == false)
            {
                break;
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        Player player = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.player).GetComponent<Player>();
        Stats playerStats = player.GetComponent<Stats>();

        switch (ContentBig)  
        {
            case ContentTypeBig.SpeedUpgrade:
                playerStats.speed += ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                ui.ShowMessage("Much more Speed!");
                break;
            case ContentTypeBig.DamageUpgrade:
                playerStats.shotStrength += ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                ui.ShowMessage("Much more Damage!");
                break;
            case ContentTypeBig.RateUpgrade:
                playerStats.fireRate *= ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                ui.ShowMessage("Fire rate extremely increased!");
                break;
            case ContentTypeBig.ShotSpeedUpgrade:
                playerStats.shotSpeed += ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                ui.ShowMessage("Shots are much faster!");
                break;
            case ContentTypeBig.Splitter:
                if (player.HasSplitter == false)
                {
                    player.HasSplitter = true;
                    playerStats.possibleShotEffects.Add(new multiplyOnContact());
                    ui.ShowMessage("You have Splitter-Shots!");
                }
                else
                {
                    playerStats.shotStrength += ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                    ui.ShowMessage("Much more Damage!");
                }
                break;
            case ContentTypeBig.Bluffer:
                if (player.HasBluffer == false)
                {
                    player.HasBluffer = true;
                    playerStats.possibleShotEffects.Add(new bluff());
                    ui.ShowMessage("You have Bluffer-Shots!");
                }
                else
                {
                    playerStats.shotStrength += ContentObj.GetComponent<StatUpgrade>().IncreaseBigValue;
                    ui.ShowMessage("Much more Damage!");
                }
                break;
        }

        ExplosionScript itemEffect = mr.getObject(ObjectPool.categorie.explosion, (int)ObjectPool.explosion.itemCollected).GetComponent<ExplosionScript>();
        itemEffect.Initialize(ContentObj.transform.position);

        GameObject heroEffect = mr.getObject(ObjectPool.categorie.explosion, (int)ObjectPool.explosion.itemCollectedHero);
        heroEffect.GetComponent<ExplosionScript>().Initialize(player.transform.position);
        heroEffect.GetComponent<Follow>().Initialize(player.gameObject);

        mr.returnObject(ContentObj);
        ContentObj = null;

        StopCoroutine(checkBigBoxFinished);

        yield return null;
    }

    public void Collect()
    {
        if (Type == ItemType.PortalKey1 || Type ==  ItemType.PortalKey2 || Type == ItemType.PortalKey3)
        {
            teleport();

            lvlManager.currentIsle.isleObjectType = IsleAbstract.IsleObjectType.normal;
            mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>().UpdateMiniMap();
            collected = true;

            PortalIsle portalIsle = lvlManager.bossIsle.IsleObj.GetComponent<PortalIsle>();
            portalIsle.PortalKeys++;

            if (portalIsle.PortalKeys < 3)
            {
                ui.ShowMessage(portalIsle.PortalKeys + " of 3 Portal-Keys collected.");
            }
            else
            {
                ui.ShowMessage("All Portal-Keys collected.\nMain-Portal is open now!");
            }
   

        }
        else if (Type == ItemType.SmallBox)
        {
            if (opened == true)
            {
                Player player = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.player).GetComponent<Player>();
                Stats playerStats = player.GetComponent<Stats>();

                switch(ContentSmall)
                {
                    case ContentTypeSmall.SmallKey: player.NumberSmallKeys++;
                        ui.UpdateKeys(player.NumberSmallKeys);
                        ui.ShowMessage("Small key collected");
                        break;
                    case ContentTypeSmall.SpeedUpgrade: playerStats.speed += ContentObj.GetComponent<StatUpgrade>().IncreaseSmallValue;
                        ui.ShowMessage("More Speed!");
                        break;
                    case ContentTypeSmall.DamageUpgrade: playerStats.shotStrength += ContentObj.GetComponent<StatUpgrade>().IncreaseSmallValue;
                        ui.ShowMessage("More Damage!");
                        break;
                    case ContentTypeSmall.RateUpgrade: playerStats.fireRate *= ContentObj.GetComponent<StatUpgrade>().IncreaseSmallValue;
                        ui.ShowMessage("Fire rate increased!");
                        break;
                    case ContentTypeSmall.ShotSpeedUpgrade: playerStats.shotSpeed += ContentObj.GetComponent<StatUpgrade>().IncreaseSmallValue;
                        ui.ShowMessage("Shots are faster!");
                        break;
                }

                ExplosionScript itemEffect = mr.getObject(ObjectPool.categorie.explosion, (int)ObjectPool.explosion.itemCollected).GetComponent<ExplosionScript>();
                itemEffect.Initialize(ContentObj.transform.position);

                GameObject heroEffect = mr.getObject(ObjectPool.categorie.explosion, (int)ObjectPool.explosion.itemCollectedHero);
                heroEffect.GetComponent<ExplosionScript>().Initialize(player.transform.position);
                heroEffect.GetComponent<Follow>().Initialize(player.gameObject);

                mr.returnObject(ContentObj);
                ContentObj = null;

                collected = true;
            }
        }
        else if (Type == ItemType.BigBox)
        {
            Player player = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.player).GetComponent<Player>();

            if (player.NumberSmallKeys > 0)
            {
                player.NumberSmallKeys--;
                UI_Canvas ui = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>();
                ui.UpdateKeys(player.NumberSmallKeys);

                OpenBigBox();
            }
        }

    }

    private void teleport()
    {
        lerpList.Clear();

        Vector3 podestPosition = Vector3.zero;

        PortalIsle portalIsle = lvlManager.bossIsle.IsleObj.GetComponent<PortalIsle>();

        switch (Type)
        {
            case ItemType.PortalKey1: podestPosition = portalIsle.Podest1.transform.position;
                break;
            case ItemType.PortalKey2: podestPosition = portalIsle.Podest2.transform.position;
                break;
            case ItemType.PortalKey3: podestPosition = portalIsle.Podest3.transform.position;
                break;
            default: print("Error: teleport von Items ohne gütigem Item-Type (nur PortalKey Erlaubt)");
                break;
        }
        lerpList.Add(new lerpInfo(gameObject.transform.position + new Vector3(0, 5, 0), 3));
        lerpList.Add(new lerpInfo(podestPosition + new Vector3(0, 5, 0), 25));
        lerpList.Add(new lerpInfo(podestPosition + new Vector3(0, 1, 0), 7));

        checkTeleportFinished = StartCoroutine(checkTeleportFinishedHandler());
    }

  
    public IEnumerator checkTeleportFinishedHandler()
    {
        while (lerpList.Count > 0)
        {
            Vector3 targetPos = lerpList[0].targetPos;
            float speed = lerpList[0].speed;

            gameObject.GetComponent<Lerper>().StartLerp(gameObject.transform.position, targetPos, speed);

            while (true)
            {
               
                if (gameObject.GetComponent<Lerper>().Lerping == false)
                {
                    break;
                }else{
                    yield return new WaitForSeconds(0.5f);
                }

            }

            lerpList.RemoveAt(0);
        }

        lvlManager.bossIsle.IsleObj.GetComponent<PortalIsle>().KeyArrived();

        StopCoroutine(checkTeleportFinished);

        yield return null;
    }
}

class lerpInfo
{
    public Vector3 targetPos;
    public float speed; 

    public lerpInfo(Vector3 targetPos, float speed)
    {
        this.targetPos = targetPos;
        this.speed = speed;
    }
}

