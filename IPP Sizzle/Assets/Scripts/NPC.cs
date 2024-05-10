using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Distance Checking")]
    [SerializeField] Vector3 interableOffset;
    [SerializeField] float interactableRadius;

    [Header("Camera Settings")]
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

    [Header("Interest")]
    [SerializeField] Transform interestIndicator;
    [SerializeField] float interestIncreaseRate;
    [SerializeField] float interestDecreaseRate;
    [SerializeField] Vector3 interestStartScale;
    [SerializeField] Vector3 interestTargetScale;
    [SerializeField] AnimationCurve interestCurveScaleX;
    [SerializeField] AnimationCurve interestCurveScaleY;
    [SerializeField] Vector3 interestStartPos;
    [SerializeField] Vector3 interestTargetPos;
    [SerializeField] AnimationCurve interestCurvePos;

    [Header("Dialogue")]
    [SerializeField] TextMeshPro textMesh;
    [SerializeField] List<string> text;
    [SerializeField][Range(0.0f, 1.0f)] float showTextThreshold;
    [Space]
    [SerializeField] Transform dialogueTrans;
    [SerializeField] float dialogueIncreaseRate;
    [SerializeField] float dialogueDecreaseRate;
    [SerializeField] Vector3 dialogueStartScale;
    [SerializeField] Vector3 dialogueTargetScale;
    [SerializeField] AnimationCurve dialogueCurveScaleX;
    [SerializeField] AnimationCurve dialogueCurveScaleY;

    public float interest;
    public float dialogueLerp;

    private Transform sizzle;
    private CameraLogic camLogic;

    [Space]
    [SerializeField] private NPCState state;

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
            }

            state = NPCState.IDLE;
        }
        else // In Range 
        {
            // Entering range 
            switch (state)
            {
                case NPCState.IDLE:
                    state = NPCState.INTERESTED;
                    break;
                case NPCState.INTERESTED:

                    interest = Mathf.Clamp01(interest += interestIncreaseRate * Time.deltaTime);

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

        InterestVisual();
        DialogueVisual();
    }

    /// <summary>
    /// Animates the visual the shows if the NPC is 
    /// interested with interacting 
    /// </summary>
    private void InterestVisual()
    {

        // NOTE: The increase of interest is done in the interested state a
        //       that is checked when the player is within range 

        if (state != NPCState.INTERESTED)
        {
            interest = Mathf.Clamp01(interest -= interestDecreaseRate * Time.deltaTime);
        }

        float scaleX = Mathf.LerpUnclamped(interestStartScale.x, interestTargetScale.x, interestCurveScaleX.Evaluate(interest));
        float scaleY = Mathf.LerpUnclamped(interestStartScale.y, interestTargetScale.y, interestCurveScaleY.Evaluate(interest));
        interestIndicator.localScale = new Vector3(scaleX, scaleY, 1.0f);

        interestIndicator.localPosition = Vector3.LerpUnclamped(interestStartPos, interestTargetPos, interestCurvePos.Evaluate(interest));
    }

    /// <summary>
    /// Animates the visual used for dialogue
    /// </summary>
    private void DialogueVisual()
    {
        if (state != NPCState.TALKING)
        {
            dialogueLerp = Mathf.Clamp01(dialogueLerp - dialogueDecreaseRate * Time.deltaTime);
            textMesh.text = "";
        }
        else
        {
            dialogueLerp = Mathf.Clamp01(dialogueLerp + dialogueIncreaseRate * Time.deltaTime);

            if(dialogueLerp >= showTextThreshold)
            {
                textMesh.text = text[0];
            }
        }

        float scaleX = Mathf.LerpUnclamped(dialogueStartScale.x, dialogueTargetScale.x, dialogueCurveScaleX.Evaluate(dialogueLerp));
        float scaleY = Mathf.LerpUnclamped(dialogueStartScale.y, dialogueTargetScale.y, dialogueCurveScaleY.Evaluate(dialogueLerp));
        dialogueTrans.localScale = new Vector3(scaleX, scaleY, 1.0f);
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
