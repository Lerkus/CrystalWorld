using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour {
    private ObjectPool mr;
    private GameObject playerObject;
    private GameObject camObject;
    private GameObject pauseMenue;
    private LevelManager levelManager;
   
    public static GameMaster gameMaster;

    public static GameMaster getGameMaster()
    {
        return gameMaster;
    }

    void Awake()
    {
        gameMaster = this;
    }


    void Start () {
        mr = ObjectPool.getObjectPool();
        playerObject = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.player);
        camObject = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.camera);
        camObject.GetComponent<camShowDmg>().showStart();
        pauseMenue = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.Pause_menue);
        pauseMenue.SetActive(false);

        playerObject.SetActive(true);
        camObject.SetActive(true);

        levelManager = LevelManager.getLevelManager();

        LoadGameStats();

        StartLevel();
    }

    private void StartLevel()
    {   
        levelManager.GenerateMap();

        //Set Player on Start Isle
        playerObject.GetComponent<CharacterController>().enabled = false;

        Isle startIsle = levelManager.startIsle.IsleObj;
        levelManager.currentIsle = startIsle.isleAbstract;
        playerObject.transform.position = startIsle.PlayerStartPoint.transform.position;
        playerObject.transform.rotation = startIsle.PlayerStartPoint.transform.rotation;
        playerObject.GetComponent<NavMeshTarget>().IslePosition = startIsle.transform.position;

        levelManager.currentIsle.IsleObj.StartIsle();
        levelManager.currentIsle.IsleObj.AddBorders();

        // activate Character Controller
        playerObject.GetComponent<CharacterController>().enabled = true;

        // show UI (inclusive Mini-Map)
        UI_Canvas ui = mr.getObject(ObjectPool.categorie.essential, (int)ObjectPool.essential.UI).GetComponent<UI_Canvas>();
        ui.ShowMiniMap();
        Stats stats = playerObject.GetComponent<Stats>();
        ui.UpdateLive(stats.health, stats.maxHealth);
        ui.UpdateKeys(playerObject.GetComponent<Player>().NumberSmallKeys);
    }

    private void LoadGameStats()
    {
        if (GameStats.LoadLevelSettings == true)
        {
            // load Level Settings

            int currentLevel = GameStats.Level;

            GameStats.UpdateLevelSettings(currentLevel);

            levelManager.WorldWidth = GameStats.LvlWorldWidht;
            levelManager.WorldHeight = GameStats.LvlWorldHeight;
            levelManager.IsleDensity = GameStats.LvlIsleDensity;

        }

        if (GameStats.LoadCharStats == true)
        {
            // load Player Stats

            Player player = playerObject.GetComponent<Player>();
            GameStats.ReadCharStats(player);
            
        }

    }

    public void BackToMenue()
    {
        StartCoroutine(BackToMenueHandler());
    }

    IEnumerator BackToMenueHandler()
    {

        yield return new WaitForSeconds(3);

        StopAllCoroutines();

        SceneManager.LoadScene("Scenes/Main_Menue");
    }
    
    public void ShowPauseMenue()
    {
        Time.timeScale = 0;

        pauseMenue.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Cancel") == 1)
        {
            ShowPauseMenue();
        }

        // Cheat mode to next Level
        /*if (Input.GetKeyDown(KeyCode.Backspace))
        {
            levelManager.bossIsle.IsleObj.gameObject.GetComponent<PortalIsle>().teleport();
        }*/
    }

}
