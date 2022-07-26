using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    bool firstTime = true;
    public void PlayGame()
    {
        if (firstTime)
        {
            firstTime = false;
            SceneManager.LoadScene("First Time Play");
        }
        else
            SceneManager.LoadScene("Main");

    }

    public void OnBoarding()
    {
        SceneManager.LoadScene("OnBoarding");
    }
    public void Credis()
    {
        SceneManager.LoadScene("Credits");
    }
    public void Exit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

}
