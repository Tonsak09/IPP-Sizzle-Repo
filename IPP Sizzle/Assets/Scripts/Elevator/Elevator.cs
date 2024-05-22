using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("Batteries")]
    [SerializeField] ElevatorBattery batteryToDown;
    [SerializeField] ElevatorBattery batteryToUp;

    [Header("Animation")]
    [SerializeField] Transform platform;
    [SerializeField] float animSpeed;
    [SerializeField] float topOffset;
    [SerializeField] float bottomOffset;
    [SerializeField] ElevatorState state;

    [Header("Checks")]
    [SerializeField] float checkSizzleRadius;

    [Header("Gizmos")]
    [SerializeField] bool showGizmos;


    private Transform sizzle;
    private SizzleController sizzleController;

    // If sizzle is being moved or elevator is being corrected
    private bool isBeckoned; 

    private Vector3 topPos { get { return new Vector3(platform.position.x, this.transform.position.y + topOffset, platform.position.z); } }
    private Vector3 bottomPos { get { return new Vector3(platform.position.x, this.transform.position.y + bottomOffset, platform.position.z); } }

    private void Awake()
    {
        sizzle = GameObject.FindGameObjectWithTag("Sizzle").transform;
        sizzleController = sizzle.GetComponent<SizzleController>();

        if (!sizzle)
            Debug.LogWarning("Sizzle not found in scene");
    }

    private void Update()
    {
        ElevatorStateManager();
    }

    /// <summary>
    /// Manages the different states of the elevator 
    /// </summary>
    private void ElevatorStateManager()
    {
        switch (state)
        {
            case ElevatorState.IDLE_TOP:
                Idle(batteryToDown, batteryToUp, topPos, ElevatorState.MOVE_TO_BOTTOM);
                break;
            case ElevatorState.IDLE_BOTTOM:
                Idle(batteryToUp, batteryToDown, bottomPos, ElevatorState.MOVE_TO_TOP);
                break;
            case ElevatorState.MOVE_TO_BOTTOM:
                Move(topPos, bottomPos);
                break;
            case ElevatorState.MOVE_TO_TOP:
                Move(bottomPos, topPos);
                break;
        }
    }


    private void Idle(
        ElevatorBattery batterySame, ElevatorBattery batteryOpposite, 
        Vector3 checkOrigin,
        ElevatorState nextMoveState)
    {
        // Update charge level visual 

        // Check if right battery is charged and
        // player is in correct area 
        if(batterySame.IsUnlocked())
        {
            // Brings Sizzle to next locationIsAirborne
            if(Vector3.Distance(sizzle.position, checkOrigin) <= checkSizzleRadius && !sizzleController.IsAirborne)
            {
                // Reset batteries 
                batteryOpposite.ResetChargeable();
                batterySame.ResetChargeable();

                // Lock Sizzle position 
                print("Locking Sizzle controller");
                sizzleController.SetMovementLocks(new SizzleController.MovementLocks(false, true));

                isBeckoned = false;

                state = nextMoveState;
                return;
            }
        }

        if(batteryOpposite.IsUnlocked())
        {
            // Beckons platform to correct position 
            batteryOpposite.ResetChargeable();
            batterySame.ResetChargeable();

            isBeckoned = true;

            state = nextMoveState;
        }
    }

    /// <summary>
    /// Animates the platform by interpolating from one pos to the next.
    /// Completes animation by check if next pos is go past target's y
    /// position 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    private void Move(Vector3 origin, Vector3 target)
    {
        Vector3 dir = (target - origin).normalized;
        Vector3 next = platform.position + dir * animSpeed * Time.deltaTime;

        platform.transform.position = next;

        if(!isBeckoned)
            sizzle.position = new Vector3(sizzle.position.x, next.y, sizzle.position.z);

        bool reachedTarget = false;
        switch (state)
        {
            case ElevatorState.MOVE_TO_BOTTOM:
                reachedTarget = next.y < target.y;
                break;
            case ElevatorState.MOVE_TO_TOP:
                reachedTarget = next.y > target.y;
                break;
            default:
                Debug.LogWarning("Invalid State of elevator " + this.gameObject.name + " during animation");
                break;
        }

        if(reachedTarget)
        {
            // Finish animation 
            platform.position = target;

            batteryToDown.ResetChargeable();
            batteryToUp.ResetChargeable();

            print("Unlocking Sizzle controller");
            sizzle.GetComponent<SizzleController>().SetMovementLocks(new SizzleController.MovementLocks(true, true));

            switch (state)
            {
                case ElevatorState.MOVE_TO_BOTTOM:
                    platform.position = bottomPos;
                    state = ElevatorState.IDLE_BOTTOM;
                    break;
                case ElevatorState.MOVE_TO_TOP:
                    platform.position = topPos;
                    state = ElevatorState.IDLE_TOP;
                    break;
            }
        }
        else
        {
            platform.position = next;
        }
    }

    /// <summary>
    /// Call this to see if states are met to animate the elevator
    /// </summary>
    public void ActivateElevator()
    {
        // Check if Sizzle is within range 


        switch(state)
        {
            case ElevatorState.IDLE_TOP:
                break;
            case ElevatorState.IDLE_BOTTOM:
                break;
        }
    }


    private enum ElevatorState
    { 
        IDLE_TOP,
        IDLE_BOTTOM,
        MOVE_TO_BOTTOM,
        MOVE_TO_TOP
    }

    private void OnDrawGizmos()
    {
        if(showGizmos)
        {
            Gizmos.DrawWireSphere( topPos, checkSizzleRadius);
            Gizmos.DrawSphere(topPos, 0.05f);
            Gizmos.DrawWireSphere(bottomPos, checkSizzleRadius);
            Gizmos.DrawSphere(this.transform.position + bottomPos, 0.05f);
        }
    }
}
