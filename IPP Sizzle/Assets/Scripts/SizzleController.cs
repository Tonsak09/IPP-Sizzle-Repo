using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class SizzleController : MonoBehaviour
{
    // BUGS: Can walk through ground that acts
    //       should like a wall. 

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
    [SerializeField] float dashTarget; 
    [SerializeField] AnimationCurve dashCurve;

    [Header("Falling")]
    [SerializeField] float fallStartSpeed;
    [SerializeField] float fallMaxSpeed;
    [SerializeField] float timeToReachMaxFallSpeed;
    [SerializeField] AnimationCurve fallSpeedCurve;
    [SerializeField] float maxFallDis;
    [SerializeField] Vector3 fallRotOffSpeed;

    [Header("Sparks")]
    [SerializeField] GameObject sparkEmitter;
    [SerializeField] Vector3 emitterOffset; // TODO: Attach to head bone for offset but not direction 

    [Header("Collision Checking")]
    [SerializeField] LayerMask unpassable;
    [SerializeField] LayerMask ground;
    [SerializeField] float unpassBodyCheckRadius; // Used to see if the body can fully fit in an area 
    [SerializeField] float unpassBodyCheckOffsetA;
    [SerializeField] float unpassBodyCheckOffsetB;
    [SerializeField] float unpassTurnCheckDistance; // Use when checking ahead if there is an unpassable 
    [SerializeField] float unpassTurnCheckRadius;
    [SerializeField] float groundOriginCheckDistance;
    [SerializeField] float groundTargetCheckDistance;
    [SerializeField] float groundCheckRadius;
    [SerializeField] float fallCheckDistance;
    [Tooltip("Checks whether Sizzle could fall and is fully off an edge. " +
             "Will stop Sizzle at edge if cannot completely fit off of it ")]
    [SerializeField] float SizzleFallZoneOffset;
    [SerializeField] Vector3 SizzleFallZoneRect;
    [SerializeField] Vector3 SizzleFallFixRect;

    // NOTE: Sometimes Sizzle is within the fall zone but that fall zone may
    //       still have some of the tail in it. Instead of adjusting a more 
    //       strict fall zone we use this zone to increase Sizzle's dash a 
    //       little bit so that their tail does not clip 


    // Theses two variables range from -1 to 1 and
    // represent the current direction of movement 
    private int forwAxis = 1;
    private int sideAxis = 0;

    private TurnType turn; // Used for animation manager 
    private bool isTurning; // Whether a turn coroutine is active 
    private bool isDashing; // Whether a dash coroutine is active 

    public bool isAirborne; 

    void Start()
    {
        isTurning = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            this.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        }

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
    private bool TryMove(Vector3 dir, Vector3 nextPos, bool ignoreGround = false)
    {
        bool hasGround = ignoreGround ? true : Physics.CheckCapsule(this.transform.position + dir * groundOriginCheckDistance, this.transform.position + dir * groundTargetCheckDistance, groundCheckRadius, ground);

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

    #region Dashing&Falling

    private void DashLogic()
    {
        // TODO: Make sure that if dashing off an edge
        //       that there is enough space for the 
        //       full Sizzle body to go off and if not
        //       then stop dash at the edge 

        if (isDashing)
            return;

        if(Input.GetKeyDown(KeyCode.Space))
        {

            isDashing = true;
            StartCoroutine(DashCo());
        }
    }

    private void UpdateIsAirborne()
    {
        // Creates a small check at pivot 
        isAirborne = !Physics.CheckSphere(this.transform.position, groundCheckRadius, ground);
    }

    private IEnumerator DashCo()
    {
        Vector3 dir = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;
        Vector3 target = this.transform.position + dir * dashTarget;
        Vector3 holdPos = this.transform.position;

        // We are checking if sizzle can completely fit off 
        Vector3 falloffDiff = SizzleFallZoneRect - SizzleFallFixRect; // Correction zone 
        bool fallOffHasSpace = !Physics.CheckBox(target + dir * SizzleFallZoneOffset, SizzleFallZoneRect / 2.0f, Quaternion.FromToRotation(-Vector3.right, dir), ground);
        bool correctHasSpace = !Physics.CheckBox(target + dir * (falloffDiff.x / 2.0f) + dir * SizzleFallZoneOffset, SizzleFallFixRect / 2.0f, Quaternion.FromToRotation(-Vector3.right, dir), ground);

        print("Falloff: " + fallOffHasSpace);
        print("Correct: " + correctHasSpace);

        /*
            Vector3 falloffDiff = SizzleFallZoneRect - SizzleFallFixRect; // Correction zone 
            DrawOrientedCubeGizmo(target + dir * (falloffDiff.x / 2.0f), SizzleFallFixRect, Quaternion.FromToRotation(-Vector3.right, dir));
            Gizmos.color = Color.red;
            DrawOrientedCubeGizmo(target, SizzleFallZoneRect, Quaternion.FromToRotation(-Vector3.right, dir));
         */

        if ((!fallOffHasSpace && correctHasSpace) )
        {
            // Add a little to the distance dashed 
            target += dir * (falloffDiff.x);
            print("Added to dash distance");
        }

        float timer = 0.0f;
        while(timer <= dashTime)
        {
            float lerp = dashCurve.Evaluate(timer / dashTime);
            Vector3 nextPos = Vector3.Lerp(holdPos, target, lerp);


            if (!TryMove(dir, nextPos, correctHasSpace))
            {
                break;
            }

           /* if (correctHasSpace)
            {
                // Moving into open air 
                this.transform.position = nextPos;
            }
            else
            {
                // Continue to move forward but stop 
                // if under normal move conditions 
                
            }*/
            
            
            timer += Time.deltaTime;
            yield return null;
        }

        // Begin to fall if in air 
        UpdateIsAirborne();

        if(isAirborne)
        {
            StartCoroutine(FallCo());
        }

        isDashing = false;
    }

    private IEnumerator FallCo()
    {

        // Raycast down to find next ground 

        // Bring player down according to speed curve 

        // If player is below desier position return them 
        // to the desired location and break loop 

        RaycastHit hit;
        Physics.Raycast(this.transform.position, Vector3.down, out hit, fallCheckDistance, ground);


        if(hit.collider == null)
        {
            // Fall off map 
            // Ex: AAAAAAAAHHHHH!!!!!

            float timer = 0.0f;
            while (true)
            {
                float speed = Mathf.Lerp(fallStartSpeed, fallMaxSpeed, fallSpeedCurve.Evaluate(timer / timeToReachMaxFallSpeed)) * Time.deltaTime;

                this.transform.position += Vector3.down * speed;
                this.transform.eulerAngles += fallRotOffSpeed * speed;

                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // Fall animation 

            float timer = 0.0f;
            while (this.transform.position.y > hit.point.y)
            {
                float speed = Mathf.Lerp(fallStartSpeed, fallMaxSpeed, fallSpeedCurve.Evaluate(timer / timeToReachMaxFallSpeed)) * Time.deltaTime;

                this.transform.position += Vector3.down * speed;

                timer += Time.deltaTime;
                yield return null;
            }

            this.transform.position = hit.point;
        }
        UpdateIsAirborne();
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

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 dir = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;

        //Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetA, unpassBodyCheckRadius);
        //Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetB, unpassBodyCheckRadius);

        /*Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.transform.position + dir * groundOriginCheckDistance, groundCheckRadius);
        Gizmos.DrawSphere(this.transform.position + dir * groundTargetCheckDistance, groundCheckRadius);*/

        /*Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position + dir * unpassTurnCheckDistance, unpassTurnCheckRadius);*/

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(this.transform.position + dir * dashTarget, new Vector3(0.1f, 0.1f, 0.1f));

        Gizmos.DrawLine(this.transform.position, this.transform.position + Vector3.down * fallCheckDistance);

        FallZoneGizmos(dir);


        //Gizmos.DrawSphere(this.transform.TransformPoint(emitterOffset), 0.05f);



        /*Gizmos.color = Color.white;
        Gizmos.matrix = this.transform.localToWorldMatrix;
        Gizmos.DrawSphere(new Vector3(0, 0, 0), 0.1f);*/
    }

    private void FallZoneGizmos(Vector3 dir)
    {
        /*Matrix4x4 mat = Matrix4x4.identity;
        mat = Matrix4x4.Translate(this.transform.position + dir * dashTarget);
        mat *= Matrix4x4.Rotate(Quaternion.FromToRotation(-Vector3.right, dir));
        Gizmos.matrix = mat;*/

        Vector3 target = this.transform.position + (dir * dashTarget) + (dir * SizzleFallZoneOffset);

        Gizmos.color = Color.green;
        Vector3 falloffDiff = SizzleFallZoneRect - SizzleFallFixRect; // Correction zone 
        DrawOrientedCubeGizmo(target + dir * (falloffDiff.x / 2.0f), SizzleFallFixRect, Quaternion.FromToRotation(-Vector3.right, dir));
        Gizmos.color = Color.red;
        DrawOrientedCubeGizmo(target, SizzleFallZoneRect, Quaternion.FromToRotation(-Vector3.right, dir));

        Gizmos.matrix = Matrix4x4.identity;
    }

    // Draws an oriented cube gizmo
    // Assisted by ChatGPT
    public static void DrawOrientedCubeGizmo(Vector3 center, Vector3 size, Quaternion rotation)
    {
        // Calculate half sizes of the cube
        Vector3 halfSize = size * 0.5f;

        // Calculate the eight corners of the cube
        Vector3[] corners = new Vector3[8];
        corners[0] = center + rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        corners[1] = center + rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        corners[2] = center + rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        corners[3] = center + rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        corners[4] = center + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        corners[5] = center + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        corners[6] = center + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
        corners[7] = center + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        // Draw the cube using Gizmos.DrawLine
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }
    }

    #endregion

}
