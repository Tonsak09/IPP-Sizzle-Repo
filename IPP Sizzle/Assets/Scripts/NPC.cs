using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] Vector3 interableOffset;
    [SerializeField] float interactableRadius;

    [Header("Camera Settigns")]
    [SerializeField] float lenseSize;
    [SerializeField] float lenseTime;
    [SerializeField] AnimationCurve lenseCurve;
    [SerializeField] float lenseLeaveTime;
    [SerializeField] AnimationCurve lenseLeaveCurve;
    [Space]
    [SerializeField] Transform camFocus;
    [SerializeField] float focusTime;
    [SerializeField] AnimationCurve focusCurve;
    [SerializeField] float focusLeaveTime;
    [SerializeField] AnimationCurve focusLeaveCurve;

    private Transform sizzle;
    private CameraLogic camLogic;

    private NPCState state;

    private void Awake()
    {
        camLogic = GameObject.FindObjectOfType<CameraLogic>();
        if (camLogic == null)
            Debug.LogError("No camera logic in this scene");

        sizzle = GameObject.FindGameObjectWithTag("Sizzle").GetComponent<Transform>();
        if (sizzle == null)
            Debug.LogError("Sizzle not found in scene");
    }

    private void Update()
    {
        bool isSizzleOutOfRange = Vector3.Distance(sizzle.position, this.transform.position + interableOffset) > interactableRadius;

        // Check if Sizzle is in range 
        if (isSizzleOutOfRange)
        {
            if(state == NPCState.TALKING)
            {
                // Player walked away 
                camLogic.LerpOrthSizeToStandard(lenseLeaveTime, lenseLeaveCurve);
                camLogic.LockToTransformStandard(focusLeaveTime);
                state = NPCState.IDLE;
            }
        }
        else // In Range 
        {
            // Entering range 
            switch(state)
            {
                case NPCState.IDLE:
                    state = NPCState.INTERESTED;
                    break;
                case NPCState.INTERESTED:

                    // Equivelant of Sizzle sparking 
                    if (Input.GetMouseButtonDown(0))
                    {
                        // Begin Conversation 
                        camLogic.LerpOrthoSize(lenseSize, lenseTime, lenseCurve);
                        camLogic.LockToTransform(camFocus, focusTime, focusCurve);
                        state = NPCState.TALKING;
                    }

                    break;
            }
        }
    }

    private enum NPCState
    {
        IDLE,
        INTERESTED,
        TALKING
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position + interableOffset, interactableRadius);
    }
}
