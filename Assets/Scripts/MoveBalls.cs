﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct ActiveBallList
{
	List<GameObject> ballList;
	bool isMoving;
	bool isInTransition;
}

public enum BallType
{
	red,
	green,
	blue,
	yellow,
    bomb
}

public class MoveBalls : MonoBehaviour
{
	public GameObject redBall;
	public GameObject greenBall;
	public GameObject blueBall;
	public GameObject yellowBall;
    public GameObject bombBall;

	public float pathSpeed;
	public float mergeSpeed;
	public int ballCount;

    public Ease easeType;
	public Ease mergeEaseType;
   
    public GameOverScreen GameOverScreen;
	
    // Private
	private List<GameObject> ballList;
	private GameObject ballsContainerGO;
	private GameObject removedBallsContainer;

	private BGCurve bgCurve;
	private float distance= 0;
	private int headballIndex;
	private SectionData sectionData;
	[SerializeField]
	private int addBallIndex;
	private int touchedBallIndex;
	private float ballRadius;
    private static Stack<int> ballsStack = new Stack<int>();
    private bool doStartOverFlag = false;
    public int speedIncrement = 1;
    public static int numberOfLives = 3;
    public Text livesText;
    // Use this for initialization
    private void Start ()
	{
		ballRadius = redBall.transform.localScale.x;
		headballIndex = 0;
		addBallIndex = -1;
        pathSpeed += speedIncrement;
        ballCount += 2;
		bgCurve = GetComponent<BGCurve>();
		ballList = new List<GameObject>();

		ballsContainerGO = new GameObject();
		ballsContainerGO.name = "Balls Container";

		removedBallsContainer = new GameObject();
		removedBallsContainer.name = "Removed Balls Container";

		for (int i=0; i < ballCount; i++)
        {
            if (!doStartOverFlag)
                CreateNewBall();
            else
                CreateNewBallFromBeginning();
        }
      

		sectionData = new SectionData();
	}

	// Update is called once per frame
	private void Update ()
	{


        if (sectionData.ballSections.Count > 1 && addBallIndex != -1 && addBallIndex < headballIndex)
			MoveStopeedBallsAlongPath();

		if (ballList.Count > 0)
			MoveAllBallsAlongPath();

		if (headballIndex != 0)
			JoinStoppedSections(headballIndex, headballIndex - 1);

		if (addBallIndex != -1)
			AddNewBallAndDeleteMatched();

		if (CheckIfActiveEndsMatch())
			MergeActiveEnds();

        if (ballList.Count == 0)
            StartOver();

		MergeIfStoppedEndsMatch();
	}

	/*
	 * Public Section
	 * =============
	 */
	public void AddNewBallAt(GameObject go, int index, int touchedBallIdx)
	{
		addBallIndex = index;
		touchedBallIndex = touchedBallIdx;

		if (index > ballList.Count)
			ballList.Add(go);
		else
			ballList.Insert(index, go);

		go.transform.parent = ballsContainerGO.transform;
		go.transform.SetSiblingIndex(index);

		if(touchedBallIdx < headballIndex)
			headballIndex++;

		sectionData.OnAddModifySections(touchedBallIdx);

		// adjust distance for headBall or the position of the front in the added section
		PushSectionForwardByUnit();
	}

	/*
	 * Static Section
	 * =============
	 */
     public  void updateNumberOfLives()
    {
        numberOfLives--;
        if (numberOfLives == 0)
            GameOver();
        else
            StartOverFromTheSameLevel();

    }

	public static BallType GetRandomBallColor()
	{
		int rInt = Random.Range(0, 3);

        //Prevent 3 balls in a row of the same kind
        if (ballsStack.Count > 1)
        {
            int last = ballsStack.Pop();
            int beforeLast = ballsStack.Pop();
            bool sameType = last == beforeLast;
            while (sameType && rInt == last)
                rInt = Random.Range(0, 3);
            ballsStack.Push(last);
        }
        ballsStack.Push(rInt);
		return (BallType)rInt;
	}

    /*
	 * Private Section
	 * =============
	 */



    private void CreateNewBall()
	{
		switch (GetRandomBallColor())
		{
			case BallType.red:
				InstatiateBall(redBall);
				break;

			case BallType.green:
				InstatiateBall(greenBall);
				break;

			case BallType.blue:
				InstatiateBall(blueBall);
				break;

			case BallType.yellow:
				InstatiateBall(yellowBall);
				break;
		}
	}

    private void StartOverFromTheSameLevel()
    {
        LivesManager.instance.ChangeLives(numberOfLives);
        RemoveBalls(headballIndex, ballList.Count);
        doStartOverFlag = true;
        Start();
    }
    private void StartOver()
    {
        doStartOverFlag = true;
        speedIncrement += 2;
        LevelManager.instance.ChangeLevel();
        Start();
    }
    
    private void CreateNewBallFromBeginning()
    {
        switch (GetRandomBallColor())
        {
            case BallType.red:
                InstatiateBallFromBeginning(redBall);
                break;

            case BallType.green:
                InstatiateBallFromBeginning(greenBall);
                break;

            case BallType.blue:
                InstatiateBallFromBeginning(blueBall);
                break;

            case BallType.yellow:
                InstatiateBallFromBeginning(yellowBall);
                break;
        }
    }

    private void InstatiateBallFromBeginning(GameObject ballGameObject)
    {
        Vector3 test =new Vector3((float)-0.3390636, (float)2.14003, (float)4.850493);
        GameObject go = Instantiate(ballGameObject, test, Quaternion.identity, ballsContainerGO.transform);
        go.SetActive(false);
        ballList.Add(go.gameObject);
    }

    private void InstatiateBall(GameObject ballGameObject)
	{
		GameObject go = Instantiate(ballGameObject,  bgCurve[0].PositionWorld, Quaternion.identity, ballsContainerGO.transform);
		go.SetActive(false);
		ballList.Add(go.gameObject);
	}

	// When a new Ball is added to the one of the stopped sections move the balls to their correct positions
	private void MoveStopeedBallsAlongPath()
	{
		int sectionKey = sectionData.GetSectionKey(addBallIndex);
		int sectionKeyVal = sectionData.ballSections[sectionKey];
		int movingBallCount = 1;

		float sectionHeadDist;
		GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out sectionHeadDist);

		Vector3 tangent;
		Vector3 trailPos;
		float currentBallDist ;
		for (int i = sectionKeyVal + 1; i <= sectionKey; i++)
		{
			currentBallDist = sectionHeadDist - movingBallCount * ballRadius;
			trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);

			if (i == addBallIndex && addBallIndex != -1)
				ballList[i].transform.DOMove(trailPos, 0.5f)
					.SetEase(easeType);
			else
				ballList[i].transform.DOMove(trailPos, 1);

			movingBallCount++;
		}
	}

    private bool IsGameOver(Vector3 ballPos)
    {
        BGCurvePointI lastPoint = bgCurve.Points[bgCurve.Points.GetLength(0) - 1];
        bool isTheSameXPosition = lastPoint.PositionLocal.x == ballPos.x;
        
        if (isTheSameXPosition)
        {
            return true;
        }
        return false;
    }

    private void GameOver()
    {
        print("game over!");
        numberOfLives = 3;
        SceneManager.LoadScene("Over");

    }

	// Move the active section of balls along the path
	private void MoveAllBallsAlongPath()
	{
		Vector3 tangent;
		int movingBallCount = 1;
		distance += pathSpeed * Time.deltaTime;

		// use a head index value which leads the balls on the path
		// This value will be changed when balls are delected from the path
		Vector3 headPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(distance, out tangent);

        //if user finished all balls
        if (doStartOverFlag)
        {
            distance = 0;
            headPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(distance, out tangent);
            doStartOverFlag = false;
        }

		ballList[headballIndex].transform.DOMove(headPos, 1);
		ballList[headballIndex].transform.rotation = Quaternion.LookRotation(tangent);

		if (!ballList[headballIndex].activeSelf)
			ballList[headballIndex].SetActive(true);


		for (int i = headballIndex + 1; i < ballList.Count; i++)
		{
			float currentBallDist = distance - movingBallCount * ballRadius;
			Vector3 trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);

            if (IsGameOver(trailPos))  
                updateNumberOfLives();
  
            if (i == addBallIndex && addBallIndex != -1)
				ballList[i].transform.DOMove(trailPos, 0.5f)
					.SetEase(easeType);
			else
				ballList[i].transform.DOMove(trailPos, 1);

			ballList[i].transform.rotation = Quaternion.LookRotation(tangent);

			if (!ballList[i].activeSelf)
				ballList[i].SetActive(true);

			movingBallCount++;
		}
	}

	// When using a different tween speed, there has to be a reset to the tween speed when the added balls reaches it correct position
	private bool CheckIfPushNeeded()
	{
		if (addBallIndex != ballList.Count)
		{
			Vector3 ABallPos = ballList[addBallIndex].transform.position;

			int neighbourBall = addBallIndex == touchedBallIndex? addBallIndex + 1: addBallIndex - 1;
			Vector3 TBallPos = ballList[neighbourBall].transform.position;

			if (Vector3.Distance(ABallPos, ballList[addBallIndex + 1].transform.position) <= 3)
				return true;
		}

		return false;
	}

	// Move the section head a unit forward along the path when new ball is added
	private void PushSectionForwardByUnit()
	{
		int sectionKey = sectionData.GetSectionKey(addBallIndex);
		int sectionKeyVal = sectionData.ballSections[sectionKey];

		float modifiedDistance;
		Vector3 tangent;

		GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out modifiedDistance);
		modifiedDistance += ballRadius;

		if (addBallIndex >= headballIndex)
			distance = modifiedDistance;

		Vector3 movedPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(modifiedDistance, out tangent);
		ballList[sectionKeyVal].transform.DOMove(movedPos, 1);
		ballList[sectionKeyVal].transform.rotation = Quaternion.LookRotation(tangent);
	}

	// Join the sections which were divided when balls were removed
	// Just check the current head with the next value if they are close
	private void JoinStoppedSections(int currentIdx, int nextSectionIdx)
	{
		float nextSecdist;
		GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[nextSectionIdx].transform.position, out nextSecdist);

		if (nextSecdist - distance <= ballRadius)
		{
			int nextSectionKeyVal;
			sectionData.ballSections.TryGetValue(nextSectionIdx, out nextSectionKeyVal);
			headballIndex = nextSectionKeyVal;

			MergeSections(currentIdx, nextSectionKeyVal);
			RemoveMatchedBalls(nextSectionIdx, ballList[nextSectionIdx]);

			if (ballList.Count > 0)
			{
				GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out nextSecdist);
				distance = nextSecdist;
			}
		}
	}

	private void MergeSections(int currentIdx, int nextSectionKeyVal)
	{
   
		sectionData.ballSections.Remove(currentIdx - 1);
		sectionData.ballSections[int.MaxValue] = nextSectionKeyVal;
	}

	// Check if the added new ball has reached its correct position on the path along the curve
	// Also remove the match ball upon reaching the position
	// set a flag to know if the ball has reached its correct position
	private void AddNewBallAndDeleteMatched()
	{
		// check if the ball position is on the path
		int sectionKey = sectionData.GetSectionKey(addBallIndex);
		int sectionKeyVal = sectionData.ballSections[sectionKey];

		float neighbourDist = 0;
		Vector3 currentTangent;
		Vector3 actualPos = Vector3.zero;
		Vector3 neighbourPos = Vector3.zero;

		int end = sectionKey == int.MaxValue? ballList.Count - 1: sectionKey;
		if (addBallIndex == end)
		{
			neighbourPos = ballList[addBallIndex - 1].transform.position;
			GetComponent<BGCcMath>().CalcPositionByClosestPoint(neighbourPos, out neighbourDist);
			actualPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(neighbourDist - ballRadius, out currentTangent);
		}
		else
		{
			neighbourPos = ballList[addBallIndex + 1].transform.position;
			GetComponent<BGCcMath>().CalcPositionByClosestPoint(neighbourPos, out neighbourDist);
			actualPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(neighbourDist + ballRadius, out currentTangent);
		}

		Vector2 currentPos = new Vector2(ballList[addBallIndex].transform.position.x, ballList[addBallIndex].transform.position.y);
		float isNear = Vector2.Distance(actualPos, currentPos);

		if (isNear <= 0.1f)
		{
			RemoveMatchedBalls(addBallIndex, ballList[addBallIndex]);
			addBallIndex = -1;
		}
	}

	// Called by the collided new ball on collision with the active balls on the path
	private void RemoveMatchedBalls(int index, GameObject go)
	{
		int front = index;
		int back = index;

		Color ballColor = go.GetComponent<Renderer>().material.GetColor("_Color");

		int sectionKey = sectionData.GetSectionKey(index);
		int sectionKeyVal;
		sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);
        PointsManager.instance.AddPoints(1);

        // Check if any same color balls towards the front side
        for (int i = index - 1; i >= sectionKeyVal; i--)
		{
			Color currrentBallColor = ballList[i].GetComponent<Renderer>().material.GetColor("_Color");
			if(ballColor == currrentBallColor)
            {
				front = i;
                PointsManager.instance.AddPoints(1);
            }
			else
				break;
		}

		// Check if any same color balls towards the back side
		int end  = sectionKey == int.MaxValue ? ballList.Count - 1: sectionKey;
		for (int i = index + 1; i <= end ; i++)
		{
			Color currrentBallColor = ballList[i].GetComponent<Renderer>().material.GetColor("_Color");
			if(ballColor == currrentBallColor)
				back = i;
			else
				break;
		}

		// If atleast 3 balls in a row are found at the new balls position
		if (back - front >= 2)
		{
			// Modify the headballIndex only if the remove section is in the moving section
			if (back > headballIndex)
			{
				// whole back section will be removed change the headIndex to the front section value
				if (front == sectionKeyVal && back == ballList.Count - 1)
				{
					if (sectionData.ballSections.Count > 1)
					{
						int nextSectionFront;
						sectionData.ballSections.TryGetValue(front - 1, out nextSectionFront);
						headballIndex = nextSectionFront;
					}
				}
				// if the remove section is less that the back i.e front and middle part of the moving section
				else
				{
					if(front >= sectionKeyVal && back != ballList.Count - 1)
					{
						headballIndex = front;
					}
				}
			}
			else
			{
				headballIndex -= (back - front + 1);
			}


			RemoveBalls(front, back - front + 1);

			if (back > headballIndex && ballList.Count > 0)
				GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out distance);
		}
	}

	// Remove balls from the list also from the ballsContainer in scene
	private void RemoveBalls(int atIndex, int range)
	{
		for (int i = 0; i < range; i++)
		{
			ballList[atIndex + i].transform.parent = removedBallsContainer.transform;
			ballList[atIndex + i].SetActive(false);
		}
		ballList.RemoveRange(atIndex, range);

		OnDeleteModifySections(atIndex, range);
	}

	private void OnDeleteModifySections(int atIndex, int range)
	{
		int sectionKey = sectionData.GetSectionKey(atIndex);

		int sectionKeyVal;
		sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);

		// completely remove the section
		if (atIndex == sectionKeyVal && atIndex + range == ballList.Count + range)
		{
			sectionData.DeleteEntireSection(atIndex, range, sectionKey, ballList.Count);
		}
		else
		{
			sectionData.DeletePartialSection(atIndex, range, sectionKey, sectionKeyVal, ballList.Count);
		}
	}

	// Check if the active section front ball matches with the back of the next section
	private bool CheckIfActiveEndsMatch()
	{
		if (sectionData.ballSections.Count <= 1)
			return false;

		Color headBallColor = ballList[headballIndex].GetComponent<Renderer>().material.GetColor("_Color");
		Color nextSectionEndColor = ballList[headballIndex - 1].GetComponent<Renderer>().material.GetColor("_Color");

		if (headBallColor == nextSectionEndColor)
			return true;
        
       
     
		return false;
	}

	// Move the section to the front of the active section
	private void MergeActiveEnds ()
	{
		int sectionKey = headballIndex - 1;
		int sectionKeyVal = sectionData.ballSections[sectionKey];

		int movingBallCount = 1;
		float sectionHeadDist;
		GetComponent<BGCcMath>().CalcPositionByClosestPoint(ballList[sectionKey].transform.position, out sectionHeadDist);
		sectionHeadDist -= mergeSpeed * Time.deltaTime;

		Vector3 tangent;
		Vector3 trailPos =  GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(sectionHeadDist, out tangent);
		ballList[sectionKey].transform.DOMove(trailPos, 0.3f)
			.SetEase(mergeEaseType);

		for (int i = sectionKey - 1; i >= sectionKeyVal; i--)
		{
			float currentBallDist = sectionHeadDist + movingBallCount * ballRadius;
			trailPos = GetComponent<BGCcMath>().CalcPositionAndTangentByDistance(currentBallDist , out tangent);

			ballList[i].transform.DOMove(trailPos, 0.3f)
				.SetEase(easeType);
			ballList[i].transform.rotation = Quaternion.LookRotation(tangent);

			movingBallCount++;
		}
	}

	// Merge the Stopped End balls
	private void MergeIfStoppedEndsMatch()
	{

	}
}
