using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] float dashOccupiedCheckOffset;
    [SerializeField] Vector3 dashOccupiedCheckRect;
    [SerializeField] float dashOccupiedCheckRadius;
    [Tooltip("X is forward, Y is up")]
    [SerializeField] Vector2 groundForwardOffset;
    [SerializeField] float groundForwardCheckRadius;

    [Header("Gizmos")]
    [SerializeField] SizzleGizmosType gizmosDisplay;

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

    private bool isAirborne; 

    private MovementLocks movementLocks;

    public bool IsAirborne { get { return isAirborne; } }

    private void Awake()
    {
        movementLocks = new MovementLocks(true, true);
    }

    void Start()
    {
        isTurning = false;

        UpdateIsAirborne();

        if(isAirborne)
        {
            StartCoroutine(FallCo());
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (isAirborne)
            return;

        SparkLogic();
        ProcessUserInput();

        UpdateIsAirborne();

        if (isAirborne)
        {
            StartCoroutine(FallCo());
        }
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
        // Maintain lock
        if (!movementLocks.CanTranslate)
            return false;

        bool hasGround = ignoreGround ? true : Physics.CheckCapsule(this.transform.position + dir * groundOriginCheckDistance, this.transform.position + dir * groundTargetCheckDistance, groundCheckRadius, ground);

        bool noUnpassable = 
            !Physics.CheckSphere(nextPos + dir * unpassBodyCheckOffsetA, unpassBodyCheckRadius, unpassable) &&
            !Physics.CheckSphere(nextPos + dir * unpassBodyCheckOffsetB, unpassBodyCheckRadius, unpassable);

        // Seperate check to see if there is ground
        // in front of Sizzle. This could be another plateform 
        // which we do not want to consider unpassable 
        bool groundInfront = Physics.CheckSphere(
            this.transform.position + dir * groundForwardOffset.x + Vector3.up * groundForwardOffset.y, 
            groundForwardCheckRadius, ground);

        if (hasGround && noUnpassable && !groundInfront)
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
        // Maintain lock but not during an animation 
        if (!isTurning && movementLocks.CanRotate == false)
            return;


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
        isAirborne = !Physics.CheckSphere(this.transform.position + Vector3.up * groundCheckRadius * 0.5f, groundCheckRadius, ground);
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

        // Dashing off edge needs a boost 
        if ((!fallOffHasSpace && correctHasSpace) )
        {
            // Add a little to the distance dashed 
            target += dir * (falloffDiff.x);
        }

        // Check if area is where you can completely fit 

        // If completely good then dash and ignore ground
        // Otherwise dash but include ground check 

        //  target + dir * (dashOccupiedCheckOffset), 
        bool canSizzleLand = DoesSizzleFitForDash(
            target + dir * (dashOccupiedCheckOffset),
            dashOccupiedCheckRect,
            Quaternion.FromToRotation(-Vector3.right, dir));
        //print("Fine for dash: " + canSizzleLand);

        float timer = 0.0f;
        while(timer <= dashTime)
        {
            float lerp = dashCurve.Evaluate(timer / dashTime);
            Vector3 nextPos = Vector3.Lerp(holdPos, target, lerp);

            // Fully air or fully land to ignore ground 
            if (!TryMove(dir, nextPos, correctHasSpace || canSizzleLand))
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

    bool DoesSizzleFitForDash(Vector3 center, Vector3 size, Quaternion rotation)
    {
        Vector3 halfSize = size * 0.5f;

        Vector3[] points = new Vector3[4];
        points[0] = center + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        points[1] = center + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        points[2] = center + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
        points[3] = center + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        foreach(Vector3 point in points)
        {
            // Is there open air here or an unpassable 
            if(!Physics.CheckSphere(point, dashOccupiedCheckRadius, ground) || Physics.CheckSphere(point, dashOccupiedCheckRadius, unpassable))
            {
                return false; 
            }
        }

        // All spots have ground and no unpassable 
        return true;
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

            AkSoundEngine.PostEvent("Play_Sparks", this.gameObject);
        }
    }

    #endregion

    #region OutsideManipulators

    public void SetMovementLocks(MovementLocks locks)
    {
        movementLocks = locks;
    }

    public struct MovementLocks
    {
        public bool CanTranslate;
        public bool CanRotate;

        public MovementLocks(bool canTrans, bool canRot)
        {
            CanTranslate = canTrans;
            CanRotate = canRot;
        }
    }


    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 dir = new Vector3(-forwAxis, 0.0f, sideAxis).normalized;

        switch (gizmosDisplay)
        {
            case SizzleGizmosType.NONE:
                break;
            case SizzleGizmosType.UNPASSABLE_WALK_CHECK:
                Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetA, unpassBodyCheckRadius);
                Gizmos.DrawWireSphere(this.transform.position + dir * unpassBodyCheckOffsetB, unpassBodyCheckRadius);
                break;
            case SizzleGizmosType.UNPASSABLE_TURN_CHECK:
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(this.transform.position + dir * unpassTurnCheckDistance, unpassTurnCheckRadius);
                break;
            case SizzleGizmosType.GROUND_CHECK:
                Gizmos.DrawSphere(this.transform.position + dir * groundOriginCheckDistance + Vector3.up * groundCheckRadius, groundCheckRadius);
                Gizmos.DrawSphere(this.transform.position + dir * groundTargetCheckDistance + Vector3.up * groundCheckRadius, groundCheckRadius);
                break;
            case SizzleGizmosType.DASH_TARGET:
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(this.transform.position + dir * dashTarget, new Vector3(0.1f, 0.1f, 0.1f));
                break;
            case SizzleGizmosType.FALL_RAY:
                Gizmos.DrawLine(this.transform.position, this.transform.position + Vector3.down * fallCheckDistance);
                break;
            case SizzleGizmosType.FALL_ZONE:
                DashOccupiedCheck(dir);
                break;
            case SizzleGizmosType.DASH_OCCUPIED_CHECK:
                DashOccupiedCheck(dir);
                break;
            case SizzleGizmosType.DASH_OFF_ZONE:
                DashOffGizmos(dir);
                break;
            case SizzleGizmosType.SPARK_EMITTER:
                break;
            case SizzleGizmosType.FORWARD_GROUND_CHECK:
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(this.transform.position + dir * groundForwardOffset.x + Vector3.up * groundForwardOffset.y, groundForwardCheckRadius);
                break;
        }
    }

    private void DashOccupiedCheck(Vector3 dir)
    {
        /*Matrix4x4 mat = Matrix4x4.identity;
        mat = Matrix4x4.Translate(this.transform.position + dir * dashTarget);
        mat *= Matrix4x4.Rotate(Quaternion.FromToRotation(-Vector3.right, dir));
        Gizmos.matrix = mat;*/

        Vector3 target = this.transform.position + (dir * dashTarget);

        Gizmos.color = Color.blue;
        DrawOrientedCubeGizmo(
            target + dir * (dashOccupiedCheckOffset), 
            dashOccupiedCheckRect, 
            Quaternion.FromToRotation(-Vector3.right, dir));


        
    }

    private void DashOffGizmos(Vector3 dir)
    {
        Vector3 target = this.transform.position + (dir * dashTarget);

        // Check area is full air 
        Gizmos.color = Color.green;
        Vector3 falloffDiff = SizzleFallZoneRect - SizzleFallFixRect; // Correction zone 
        DrawOrientedCubeGizmo(target + dir * (falloffDiff.x / 2.0f), SizzleFallFixRect, Quaternion.FromToRotation(-Vector3.right, dir));
        Gizmos.color = Color.red;
        DrawOrientedCubeGizmo(target, SizzleFallZoneRect, Quaternion.FromToRotation(-Vector3.right, dir));
    }

    // Draws an oriented cube gizmo
    // Assisted by ChatGPT
    void DrawOrientedCubeGizmo(Vector3 center, Vector3 size, Quaternion rotation)
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
        DrawSphereOnArray(corners, dashOccupiedCheckRadius);

        // Draw the cube using Gizmos.DrawLine
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }
    }


    void DrawSphereOnArray(Vector3[] points, float radius)
    {
        foreach(Vector3 point in points) 
        { 
            Gizmos.DrawSphere(point, radius);
        }

    }

    private enum SizzleGizmosType
    {
        NONE,
        UNPASSABLE_WALK_CHECK,
        UNPASSABLE_TURN_CHECK,
        GROUND_CHECK,
        DASH_TARGET,
        FALL_RAY,
        FALL_ZONE,
        DASH_OCCUPIED_CHECK,
        DASH_OFF_ZONE,
        SPARK_EMITTER,
        FORWARD_GROUND_CHECK,
    }

    #endregion

}
