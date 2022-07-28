using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour {

    public Text pointsText;

    public void SetUp(int score)
    {
        gameObject.SetActive(true);
        pointsText.text = score.ToString() + " POINTS";
    }

    private void Update()
    {
        pointsText.text = PointsManager.instance.getScore() + " Points";
    }

    public void RestartButton()
    {
        print("here");
        SceneManager.LoadScene("Main");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
