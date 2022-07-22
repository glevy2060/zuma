using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class onBoarding : MonoBehaviour {

    public Text changingText;
    public Text clickToContinue;
    static string message1 = "Welcome to ###!";
    static string message2 = "Your goal is to prevent the balls from getting to your canon";
    static string message3 = "Click to use your canon and shoot the ball";
    static string message4 = "Lets try it!";
    static string message5 = "Cool!";
    static string message6 = "Now lets try to\nshoot those balls";
    static string message7 = "Hooray!";
    static string message8 = "Let's try this again,\nwith two colors this time";
    static string message9 = "";



    public static string[] messages = { message1, message2, message3, message4, message5, message6 };
    public static string[] messages2 = { message7, message8, message9 };
    public GameObject ground;
    public GameObject levelContainer;
    private int ClickCounter = 0;
    static LinkedList<string> messagesList1 = new LinkedList<string>(messages);
    static LinkedList<string> messagesList2 = new LinkedList<string>(messages2);


    // Use this for initialization
    void Start () {
        changingText.text = messagesList1.First.Value;
        messagesList1.RemoveFirst();

    }
	
	// Update is called once per frame
	void Update () {
        if (changingText.text == message2)
            changingText.fontSize = 30;


        if (changingText.text == message4)
        {
            changingText.alignment = TextAnchor.MiddleLeft;
            clickToContinue.enabled = false;
            ground.SetActive(true);
            levelContainer.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Mouse0))
                ClickCounter++;
            if (ClickCounter > 2)
            {
                changingText.text = messagesList1.First.Value;
                messagesList1.RemoveFirst();
            }

        }
        else if (changingText.text == message5)
        {
            clickToContinue.enabled = true;
            clickToContinue.alignment = TextAnchor.UpperLeft;
            MoveToNextMessage();

        }
        else if (changingText.text == message6)
            MoveBallsOnBoarding.instance.setOnBallFlag(true);



        else if (messagesList1.Count != 0)
            MoveToNextMessage();


        if (MoveBallsOnBoarding.finishPhaseOneFlag)
            MoveToNextMessage2();

        if(messagesList2.Count == 0)
        {
            //levelContainer.SetActive(false);
            //clickToContinue.enabled = false;
            SceneManager.LoadScene("Are you ready");

        }


    }

    private void MoveToNextMessage()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (messagesList1.Count != 0)
            {
                changingText.text = messagesList1.First.Value;
                messagesList1.RemoveFirst();
            }
        }
    }

    private void MoveToNextMessage2()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) | messagesList2.Count == messages2.Length)
        {
            if (messagesList2.Count != 0)
            {
                changingText.text = messagesList2.First.Value;
                messagesList2.RemoveFirst();
                if(changingText.text == message8)
                    MoveBallsOnBoarding.restartFlag = true;

            }
        }
    }
}
