using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class GameOverScreen : MonoBehaviour {

    public Text pointsText;

    public void SetUp(int score)
    {
        gameObject.SetActive(true);
        pointsText.text = score.ToString() + " POINTS";
    }

    public void RestartButton()
    {
        EditorSceneManager.LoadScene("Main");
    }

    public void ExitButton()
    {
        EditorSceneManager.LoadScene("Main Menu");
    }
}
