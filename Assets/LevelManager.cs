using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    public static LevelManager instance;
    public Text levelText;
    int needToChange;
    private float _TimeChecker = 0;
    private float _BlinkTime = 0;
    Color prevColor;


    int level = 1;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        levelText.text ="LEVEL "+ level.ToString();

    }


    private void Update()
    {
        if(needToChange == 1)
        {
            _TimeChecker += Time.deltaTime;
            if (_TimeChecker < _BlinkTime)
            {
                levelText.color = Color.red;
                levelText.fontStyle = FontStyle.Normal;
            }

            else
            {
                levelText.color = prevColor;
                levelText.fontStyle = FontStyle.Bold;

                needToChange = 0;
            }
        }
    }

    public void ChangeLevel()
    {
        level += 1;
        levelText.text = "LEVEL " + level.ToString();
        needToChange = 1;
        _TimeChecker = Time.deltaTime;
        _BlinkTime = Time.deltaTime + 0.15f;
        prevColor = levelText.color;
    }



}
