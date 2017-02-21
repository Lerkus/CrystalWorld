using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public void StartGame()
    {
        GameStats.LoadLevelSettings = true;
        GameStats.Level = 1;

        GameStats.LoadCharStats = false;

        ChangeScene("Scenes/World");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        ChangeScene("Scenes/Main");
    }

    public void ExitPauseMenue()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}