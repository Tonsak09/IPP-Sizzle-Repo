using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    [SerializeField] TextMeshPro textMesh;
    [SerializeField] float appearSpeed = 1.0f;
    [SerializeField] AnimationCurve alphaCurve;
    [SerializeField] Vector3 targetPos;
    [SerializeField] AnimationCurve posCurve;
    [Space]
    [SerializeField] float idleLifetime;
    [Space]
    [SerializeField] float dissapearSpeed;

    private float timer = 0.0f;
    private Material wobbleMat;

    private Vector3 startPos;

    private NotificationState state;

    // Start is called before the first frame update
    void Start()
    {
        wobbleMat = renderer.material;
        startPos = this.transform.localPosition;

        state = NotificationState.APPEAR;
    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            case NotificationState.APPEAR:
                Appear();
                break;
            case NotificationState.IDLE:
                Idle();
                break;
            case NotificationState.DISSAPEAR:
                Dissappear();
                break;
        }
        
    }

    /// <summary>
    /// Animates the notification appearing 
    /// </summary>
    private void Appear()
    {
        timer += appearSpeed * Time.deltaTime;

        Vector4 color = wobbleMat.color;
        color.w = alphaCurve.Evaluate(timer);
        Vector4 fontColor = textMesh.color;
        fontColor.w = alphaCurve.Evaluate(timer);

        wobbleMat.SetVector("_Color", color);
        textMesh.color = fontColor;

        this.transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, posCurve.Evaluate(timer));

        if(timer >= 1.0f)
        {
            timer = 0.0f;
            state = NotificationState.IDLE;
        }
    }

    /// <summary>
    /// Keeps notification alive for given amount of time 
    /// </summary>
    private void Idle()
    {
        timer += Time.deltaTime;

        if(timer >= idleLifetime)
        {
            timer = 0.0f;
            state = NotificationState.DISSAPEAR;
        }
    }

    /// <summary>
    /// Animates notification dissapearing and then destroys it 
    /// </summary>
    private void Dissappear()
    {
        timer += dissapearSpeed * Time.deltaTime;

        Vector4 color = wobbleMat.color;
        color.w = alphaCurve.Evaluate(1.0f - timer);
        Vector4 fontColor = textMesh.color;
        fontColor.w = alphaCurve.Evaluate(1.0f - timer);

        wobbleMat.SetVector("_Color", color);
        textMesh.color = fontColor;

        //this.transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, posCurve.Evaluate(timer));

        if (timer >= 1.0f)
        {
            Destroy(this.gameObject);
        }
    }

    private enum NotificationState
    {
        APPEAR,
        IDLE,
        DISSAPEAR
    }
}
