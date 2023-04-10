using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadMainLevel()
    {
        SceneManager.LoadScene("MainLevel");
    }

    public void LoadSecondLevel()
    {
        SceneManager.LoadScene("SecondLevel");
    }

    public void LoadEndScene()
    {
        SceneManager.LoadScene("EndScene");
    }

    public void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
