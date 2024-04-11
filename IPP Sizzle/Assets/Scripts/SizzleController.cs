using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SizzleController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] KeyCode forward;
    [SerializeField] KeyCode backward;
    [SerializeField] KeyCode right;
    [SerializeField] KeyCode left;
    [SerializeField] KeyCode dash;
    [SerializeField] MouseButton spark;
 

    [Header("Turning")]
    [SerializeField] float turnTime;

    [Header("Running")]
    [SerializeField] float moveSpeed;

    [Header("Dashing")]
    [SerializeField] float dashTime;
    [SerializeField] float dashSpeed;
    [SerializeField] AnimationCurve dashCurve;

    [Header("Falling")]
    [SerializeField] float fallMaxSpeed;
    [SerializeField] AnimationCurve fallCurve;

    [Header("Sparks")]
    [SerializeField] GameObject sparkEmitter;
    [SerializeField] Vector3 emitterOffset; // TODO: Attach to head bone but face same direction always 

    [Header("Collision Checking")]
    [SerializeField] float unpassBodyCheckRadius; // Used to see if the body can fully fit in an area 
    [SerializeField] float unpassBodyCheckOffsetA;
    [SerializeField] float unpassBodyCheckOffsetB;
    [SerializeField] float unpassTurnCheckDistance; // Use when checking ahead if there is an unpassable 
    [SerializeField] float unpassTurnCheckRadius;
    [SerializeField] LayerMask unpassable;
    [SerializeField] LayerMask ground;
    [SerializeField] float groundCheckDistance;
    [SerializeField] float groundCheckRadius;

    // Theses two variables range from -1 to 1 and
    // represent the current direction of movement 
    private int forwAxis = 1;
    private int sideAxis = 0;

    private TurnType turn; // Used for animation manager
    private bool isTurning; // Whether a turn coroutine is active 
    private bool isDashing; // Whether a dash coroutine is active 

    private bool isAirborne; 

    void Start()
    {
        isTurning = false;
    }

    void Update()
    {
        if (isAirborne)
            return;

        SparkLogic();
        ProcessUserInput();
    }

    private void ProcessUserInput()
    {

        DashLogic();

        // Can't do anything else until dash is finished 
        if (isDashing)
            return;

        int currForw;
        int currSide;

        bool isForward = Input.GetKey(forward);
        bool isBackward = Input.GetKey(backward);

        if (isForward == isBackward)
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


        // Check if area to turn has unpassables if to turn 
        if (Physics.CheckSphere(this.transform.position + dir * unpassTurnCheckDistance, unpassTurnCheckRadius, unpassable))
        {
            return;
        }

        TryMove(dir, nextPos);

        if (isTurning)
            return;

        OrientationLogic(currForw, currSide);

        forwAxis = currForw;
        sideAxis = currSide;
    }

    #region Running

    /// <summary>
    /// Logic to check if a desiered position can be set to 
    /// </summary>
    /// <param name="dir">Direction of movement</param>
    /// <param name="nextPos">Next desired position</param>
    private bool TryMove(Vector3 dir, Vector3 nextPos)
    {
        bool hasGround = Physics.CheckSphere(this.transform.position + dir * groundCheckDistance, groundCheckRadius, ground);
        bool noUnpassable =
            !Physics.CheckSphere(nextPos + dir * unpassBodyCheckOffsetA, unpassBodyCheckRadius, unpassable) &&
            !Physics.CheckSphere(nextPos + dir * unpassBodyCheckOffsetB, unpassBodyCheckRadius, unpassable);


        if (hasGround && noUnpassable)
        {
            this.transform.position = nextPos;
            return true; 
        }

        return false;
    }

    #endregion

    #region Turning 

    private void OrientationLogic(int currForw, int currSide)
    {
        // Compare to previous input to see if there needs
        // to be rotation

        if (currForw != forwAxis || currSide != sideAxis)
        {
            if (!isTurning)
            {
                // Note: Differences in how much we need to turn range 
                //       from 1 to 4 as a difference 

                int forwDifference = currForw - forwAxis;
                int sideDifference = currSide - sideAxis;

                if (currForw == -forwAxis && currSide == -sideAxis)
                {
                    // Flipped direction 
                    turn = TurnType.BIG_JUMP;
                }
                else
                {
                    // Figure how much of a turn based on following equation 
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
        StartCoroutine(RotateToOrientationCo(this.transform.forward, target));
    }

    private IEnumerator RotateToOrientationCo(Vector3 origin, Vector3 target)
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

    #region Dashing

    private void DashLogic()
    {
        if (isDashing)
            return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isDashing = true;
            StartCoroutine(DashCo());
        }
    }

    private IEnumerator DashCo()
    {
        Vector3 dir = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;

        float timer = 0.0f;
        while(timer <= dashTime)
        {
            float scale = dashSpeed * dashCurve.Evaluate(timer / dashTime) * Time.deltaTime;
            if(!TryMove(dir, this.transform.position + dir * scale))
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    #endregion

    #region Sparks

    private void SparkLogic()
    {
        if(Input.GetMouseButtonDown((int)spark))
        {
            GameObject emitter = Instantiate(sparkEmitter, this.transform.TransformPoint(emitterOffset), this.transform.rotation);
            emitter.transform.forward = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Vector3 dir = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;

        Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetA, unpassBodyCheckRadius);
        Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetB, unpassBodyCheckRadius);

        Gizmos.DrawWireSphere(this.transform.position + dir * groundCheckDistance, groundCheckRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position + dir * unpassTurnCheckDistance, unpassTurnCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.transform.TransformPoint(emitterOffset), 0.05f);

        Gizmos.color = Color.white;
        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.DrawSphere(new Vector3(0, 0, 0), 0.1f);
    }

}
