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

    [Header("Collision Checking")]
    [SerializeField] float unpassCheckRadius;
    [SerializeField] LayerMask unpassable;
    [SerializeField] LayerMask ground;
    [SerializeField] float groundCheckDistance;
    [SerializeField] float groundCheckRadius;

    // Theses two variables range from -1 to 1 and
    // represent the current direction of movement 
    private int forwAxis = 1;
    private int sideAxis = 0;

    private TurnType turn;
    private bool isTurning; // Whether a coroutine is active 

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

        Vector3 dir = new Vector3(-currForw, 0.0f, currSide).normalized;
        Vector3 nextPos = this.transform.position + dir * moveSpeed * Time.deltaTime;

        bool hasGround = Physics.CheckSphere(this.transform.position + dir, groundCheckRadius, ground);
        /*foreach (Vector3 point in groundChecks)
        {
            if (!Physics.CheckSphere(this.transform.TransformPoint(point), groundCheckRadius, ground))
            {
                hasGround = false;
                break;
            }
        }*/

        if (hasGround && !Physics.CheckSphere(nextPos, unpassCheckRadius, unpassable))
        {
            this.transform.position = nextPos;
        }


        if (isTurning)
            return;

        OrientationLogic(currForw, currSide);

        forwAxis = currForw; 
        sideAxis = currSide;
    }



    #region Turning 

    private void OrientationLogic(int currForw, int currSide)
    {
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
                    int totalDiff = System.Math.Abs(forwDifference) + System.Math.Abs(sideDifference);
                    turn = (TurnType)totalDiff;
                }

                AdjustOrientation(new Vector3(currSide, 0.0f, currForw));

            }
        }
        else
        {
            // Don't turn 
            turn = TurnType.NONE;
        }
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

        while (timer <= turnTime)
        {
            this.transform.forward = Vector3.Slerp(origin, target, timer / turnTime);

            timer += Time.deltaTime;
            yield return null;
        }

        this.transform.forward = target;
        isTurning = false;
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, unpassCheckRadius);
        Gizmos.DrawWireSphere(this.transform.position + new Vector3(-forwAxis, 0.0f, sideAxis).normalized * groundCheckDistance, groundCheckRadius);

        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.DrawSphere(new Vector3(0, 0, 0), 0.1f);
    }

}
