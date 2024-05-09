using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private Transform sizzle;
    private Transform aim;          // Where the camera is looking 
    private Transform target;       // Currently is the focus of the camera's aim 

    private float standardLense;
    private bool isLenseSizeChanging = false;
    private Coroutine lenseChangeCO;

    [SerializeField] CameraState camState;
    [SerializeField] AnimationCurve defaultCurve;

    private void Awake()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        sizzle = GameObject.FindGameObjectWithTag("Sizzle").GetComponent<Transform>();

        if(sizzle == null )
            Debug.LogError("Sizzle not found in scene");

        target = sizzle;

        aim = new GameObject("Camera Target").GetComponent<Transform>();
        aim.position = sizzle.position;
        cam.Follow = aim;
        cam.LookAt = aim;


        standardLense = cam.m_Lens.OrthographicSize;
    }

    private void Update()
    {
        CamStateMachine();
    }

    private void CamStateMachine()
    {
        switch (camState)
        {
            case CameraState.LOCKED:
                UpdateAim();
                break;
            case CameraState.TRANSITIONING_TARGET:
                break;
        }
    }


    /// <summary>
    /// Updates camera aim's position to target 
    /// </summary>
    private void UpdateAim()
    {
        aim.position = target.position;
    }

    /// <summary>
    /// Interpolates the aim from the old target to the new one 
    /// </summary>
    /// <param name="idealTarget"></param>
    /// <param name="travelTime"></param>
    /// <param name="curve"></param>
    /// <returns></returns>
    private IEnumerator TransitionAimCo(Transform idealTarget, float travelTime, AnimationCurve curve)
    {
        Vector3 initial = aim.position;

        float timer = 0.0f;
        while(timer <= travelTime)
        {
            float lerp = curve.Evaluate(timer / travelTime);
            aim.position = Vector3.Lerp(initial, target.position, lerp);

            timer += Time.deltaTime;
            yield return null;
        }

        target = idealTarget;
        camState = CameraState.LOCKED;
    }

    private IEnumerator TransitionLenseSizeCo(float size, float time, AnimationCurve curve)
    {

        float initial = cam.m_Lens.OrthographicSize;

        float timer = 0.0f;
        while (timer <= time)
        {
            float lerp = curve.Evaluate(timer / time);
            cam.m_Lens.OrthographicSize = Mathf.LerpUnclamped(initial, size, lerp);

            timer += Time.deltaTime;
            yield return null;
        }


        isLenseSizeChanging = false;
    }

    #region PUBLIC_FUNCTIONS

    /// <summary>
    /// Sets target back to Sizzle 
    /// </summary>
    public void LockToTransformStandard(float time)
    {
        LockToTransformStandard(time, defaultCurve);
    }

    /// <summary>
    /// Sets target back to Sizzle 
    /// </summary>
    public void LockToTransformStandard(float time, AnimationCurve curve)
    {
        LockToTransform(sizzle, time, curve);
    }


    /// <summary>
    /// Interpolates camera target to the give target 
    /// </summary>
    public void LockToTransform(Transform nextTarget, float travelTime)
    {
        LockToTransform(nextTarget, travelTime, defaultCurve);
    }

    /// <summary>
    /// Interpolates camera target to the give target 
    /// </summary>
    public void LockToTransform(Transform nextTarget, float travelTime, AnimationCurve curve)
    {
        // Don't continue unless doing follow logic 
        if (camState != CameraState.LOCKED)
            return;

        camState = CameraState.TRANSITIONING_TARGET;
        StartCoroutine(TransitionAimCo(nextTarget, travelTime, curve));
    }


    /// <summary>
    /// Changes the orth lense size back to its standrard size 
    /// </summary>
    public void LerpOrthSizeToStandard(float time)
    {
        LerpOrthSizeToStandard(time, defaultCurve);
    }

    /// <summary>
    /// Changes the orth lense size back to its standrard size 
    /// </summary>
    public void LerpOrthSizeToStandard(float time, AnimationCurve curve)
    {
        LerpOrthoSize(standardLense, time, curve);
    }

    /// <summary>
    /// Change the ortho lense size to a desired value 
    /// </summary>
    public void LerpOrthoSize(float size, float time)
    {
        LerpOrthoSize(size, time, defaultCurve);
    }

    /// <summary>
    /// Change the ortho lense size to a desired value 
    /// </summary>
    public void LerpOrthoSize(float size, float time, AnimationCurve curve)
    {
        // Don't continue unless not already chaning lense size 
        if (isLenseSizeChanging)
        {
            StopCoroutine(lenseChangeCO);
        }
            

        isLenseSizeChanging = true;
        lenseChangeCO = StartCoroutine(TransitionLenseSizeCo(size, time, curve));
    }


    #endregion


    private enum CameraState
    {
        LOCKED,         // Locked to the target 
        TRANSITIONING_TARGET,  // Chaning from one target to the next 
    }
}
