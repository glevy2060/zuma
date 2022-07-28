using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour {
    public static LivesManager instance;
    public Text livesText;
    int needToChange;
    private float _TimeChecker = 0;
    private float _BlinkTime = 0;
    Color prevColor;

    int lives = MoveBalls.numberOfLives;
    // Use this for initialization
    private void Awake()
    {
        instance = this;

    }
    void Start () {
        livesText.text = lives.ToString() + " Lives";

    }

    // Update is called once per frame
    void Update () {
        if (needToChange == 1)
        {
            _TimeChecker += Time.deltaTime;
            if (_TimeChecker < _BlinkTime)
            {
                livesText.color = Color.red;
                livesText.fontStyle = FontStyle.Normal;
            }

            else
            {
                livesText.color = prevColor;
                livesText.fontStyle = FontStyle.Bold;

                needToChange = 0;
            }
        }
        if (lives == 1)
            livesText.color = Color.red;
    }
    public void ChangeLives(int lives)
    {
        this.lives=lives;
        livesText.text = lives.ToString() + " Lives";
        needToChange = 1;
        _TimeChecker = Time.deltaTime;
        _BlinkTime = Time.deltaTime + 0.15f;
        prevColor = livesText.color;
    }
}
