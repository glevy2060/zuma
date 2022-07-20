using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointsManager : MonoBehaviour
{

    public static PointsManager instance;
    public Text scoreText;
    public Text highScoreText;

    int score = 0;
    int highScore = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("highScore", 0);
        scoreText.text = score.ToString() + " POINTS";
        highScoreText.text = "HIGHSCORE: " + highScore.ToString();
    }

    public void AddPoints(int amount)
    {
        score += amount;
        scoreText.text = score.ToString() + " POINTS";
        if (highScore < score)
            PlayerPrefs.SetInt("highScore", score);
    }

    public void RemovePoints(int amount)
    {
        if (score - amount < 0)
        {
            score = 0;
            scoreText.text = score.ToString() + " POINTS";
        }
        else
        {
            score -= amount;
            scoreText.text = score.ToString() + " POINTS";
        }

    }

    public int getScore()
    {
        return score;
    }
}
