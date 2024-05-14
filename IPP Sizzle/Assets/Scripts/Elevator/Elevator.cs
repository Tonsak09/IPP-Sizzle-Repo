using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ElevatorBattery batteryToDown;
    [SerializeField] ElevatorBattery batteryToUp;
    [SerializeField] Transform platform;

    [Header("Animation")]
    [SerializeField] float animSpeed;
    [SerializeField] Vector3 topPos;
    [SerializeField] Vector3 bottomPos;
    [SerializeField] ElevatorState state;

    [Header("Checks")]
    [SerializeField] float checkSizzleRadius;

    [Header("Gizmos")]
    [SerializeField] bool showGizmos;


    private Transform sizzle;

    private void Awake()
    {
        sizzle = GameObject.FindGameObjectWithTag("Sizzle").transform;

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
                Idle(topPos);
                break;
            case ElevatorState.IDLE_BOTTOM:
                Idle(bottomPos);
                break;
            case ElevatorState.MOVE_TO_BOTTOM:
                Move(topPos, bottomPos);
                break;
            case ElevatorState.MOVE_TO_TOP:
                Move(bottomPos, topPos);
                break;
        }
    }


    private void Idle(Vector3 checkOrigin)
    {

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

            switch (state)
            {
                case ElevatorState.MOVE_TO_BOTTOM:
                    state = ElevatorState.IDLE_BOTTOM;
                    break;
                case ElevatorState.MOVE_TO_TOP:
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
            Gizmos.DrawWireSphere(this.transform.position + topPos, checkSizzleRadius);
            Gizmos.DrawSphere(this.transform.position + topPos, 0.05f);
            Gizmos.DrawWireSphere(this.transform.position + bottomPos, checkSizzleRadius);
            Gizmos.DrawSphere(this.transform.position + bottomPos, 0.05f);
        }
    }
}
