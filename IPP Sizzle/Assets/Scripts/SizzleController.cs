using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizzleController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] KeyCode forward;
    [SerializeField] KeyCode backward;
    [SerializeField] KeyCode right;
    [SerializeField] KeyCode left;

    [Header("Turning")]
    [SerializeField] float turnTime;

    [Header("Running")]
    [SerializeField] float moveSpeed;

    // Theses two variables range from -1 to 1 and
    // represent the current direction of movement 
    public int forwAxis = 1;
    public int sideAxis = 0;

    [SerializeField] private TurnType turn;
    private bool isTurning; // Whether a coroutine is active 
    public bool turnLeft;  // Whether to rotate left or right 

    void Start()
    {
        isTurning = false;
    }

    void Update()
    {
        ProcessUserInput();
    }

    private void ProcessUserInput()
    {
      

        int currForw;
        int currSide;

        bool isForward = Input.GetKey(forward);
        bool isBackward = Input.GetKey(backward);

        if(isForward == isBackward)
        {
            // Cancel each other out 
            currForw = 0;
        }
        else
        {
            currForw = isForward ? 1 : -1;
        }

        bool isRight = Input.GetKey(right);
        bool isLeft = Input.GetKey(left);

        if (isRight == isLeft)
        {
            // Cancel each other out 
            currSide = 0;
        }
        else
        {
            currSide = isRight ? 1 : -1;
        }


        if (currForw == 0 && currSide == 0)
            return;

        // Running code 

        this.transform.position += new Vector3(-currForw, 0.0f, currSide).normalized * moveSpeed * Time.deltaTime;



        if (isTurning)
            return;

        // Compare to previous input to see if there needs
        // to be rotation

        if (currForw != forwAxis || currSide != sideAxis)
        {
            if (!isTurning)
            {
                // Does not match with previous input 

                // Differences in how much we need to turn range 
                // from 1 to 4 as a difference 

                int forwDifference = currForw - forwAxis;
                int sideDifference = currSide - sideAxis;

                if (currForw == -forwAxis && currSide == -sideAxis)
                {
                    // Flipped direction 
                    turn = TurnType.BIG_JUMP;
                }
                else
                {
                    int totalDiff = Math.Abs(forwDifference) + Math.Abs(sideDifference);
                    turn = (TurnType)totalDiff;

                    // Whether to turn left or right 
                    Vector2 curr = new Vector2(currForw, currSide);
                    Vector2 prev = new Vector2(forwAxis, sideAxis);

                    turnLeft = Vector2.Dot(prev, curr) >= 0.0f;

                    print(Vector2.Dot(prev, curr));
                }

            AdjustOrientation(new Vector3(currSide, 0.0f, currForw));

            }
        }
        else
        {
            // Don't turn 
            turn = TurnType.NONE;
        }

        forwAxis = currForw; 
        sideAxis = currSide;
    }

    private void AdjustOrientation(Vector3 target)
    {
        if (isTurning)
            return;

        isTurning = true;
        StartCoroutine(RotateToOrientation(this.transform.forward, target));
    }

    private IEnumerator RotateToOrientation(Vector3 origin, Vector3 target)
    {
        float timer = 0.0f;

        while(timer <= turnTime)
        {
            this.transform.forward = Vector3.Slerp(origin, target, timer / turnTime);

            timer += Time.deltaTime;
            yield return null;
        }

        this.transform.forward = target;
        isTurning = false;
    }

    private void AdjustOrientation()
    {
        switch (turn)
        {
            case TurnType.NONE: // Do nothing 
                break;
            case TurnType.SMALL_STEP:
                isTurning = true;
                StartCoroutine(RotateToAngle(this.transform.eulerAngles, this.transform.eulerAngles + Vector3.up * (turnLeft ? 45.0f : -45.0f)));
                break;
            case TurnType.BIG_STEP:
                //isTurning = true;
                break;
            case TurnType.SMALL_JUMP:
                //isTurning = true;
                break;
            case TurnType.BIG_JUMP:
                //isTurning = true;
                break;
        }
    }

    private IEnumerator RotateToAngle(Vector3 origin, Vector3 target)
    {
        float timer = 0.0f; 
        while(timer <= turnTime)
        {
            this.transform.eulerAngles = Vector3.Slerp(origin, target, timer / turnTime);

            timer += Time.deltaTime;
            yield return null;
        }

        this.transform.eulerAngles = target;
        isTurning = false;
    }

    #region Turning 



    /// <summary>
    /// These are the different kinds of turns Sizzle can take
    /// in either left or right direction 
    /// </summary>
    private enum TurnType
    {
        NONE = 0,
        SMALL_STEP = 1,
        BIG_STEP = 2,
        SMALL_JUMP = 3,
        BIG_JUMP = 4
    }

    #endregion
}
