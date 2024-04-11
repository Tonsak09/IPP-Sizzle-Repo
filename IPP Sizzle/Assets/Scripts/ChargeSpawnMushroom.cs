using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSpawnMushroom : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] float minXRot;
    [SerializeField] float maxXRot;
    [SerializeField] float minYRot;
    [SerializeField] float maxYRot;
    [SerializeField] float minZRot;
    [SerializeField] float maxZRot;

    [Header("Animation")]
    [SerializeField] Vector3 startScaleMin;
    [SerializeField] Vector3 startScaleMax;
    [SerializeField] Vector3 targetScaleMin;
    [SerializeField] Vector3 targetScaleMax;
    [SerializeField] float timeToGrow;
    [SerializeField] AnimationCurve ySpawnCurve;
    [SerializeField] AnimationCurve xzSpawnCurve;

    [Tooltip("Time in which this mushroom is not animated")]
    [SerializeField] float normalLifeTime; 

    [SerializeField] float timeToShrink;
    [SerializeField] AnimationCurve yShrinkCurve;
    [SerializeField] AnimationCurve xzShrinkCurve;

    private Vector3 startScale;
    private Vector3 targetScale;

    // Start is called before the first frame update
    void Start()
    {
        startScale = RandomVec(startScaleMin, startScaleMax);
        targetScale = RandomVec(targetScaleMin, targetScaleMax);

        this.transform.eulerAngles = new Vector3(
            Random.Range(minXRot, maxXRot),
            Random.Range(minYRot, maxYRot),
            Random.Range(minZRot, maxZRot));

        StartCoroutine(LifeCycle());
    }

    private IEnumerator LifeCycle()
    {
        float growthTimer = 0.0f;
        while (growthTimer <= timeToGrow)
        {
            float y = Mathf.LerpUnclamped(startScale.y, targetScale.y, ySpawnCurve.Evaluate(growthTimer / timeToGrow));
            float xz = Mathf.LerpUnclamped(startScale.x, targetScale.x, xzSpawnCurve.Evaluate(growthTimer / timeToGrow));

            this.transform.localScale = new Vector3(xz, y, xz);

            growthTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(normalLifeTime);

        float decayTimer = 0.0f;
        while (decayTimer <= timeToShrink)
        {
            float y = Mathf.LerpUnclamped(targetScale.y, startScale.y, yShrinkCurve.Evaluate(decayTimer / timeToShrink));
            float xz = Mathf.LerpUnclamped(targetScale.x, startScale.x, xzShrinkCurve.Evaluate(decayTimer / timeToShrink));

            this.transform.localScale = new Vector3(xz, y, xz);

            decayTimer += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }

    private Vector3 RandomVec(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }
}
