using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [SerializeField] Vector3 offsetA;
    [SerializeField] Vector3 offsetB;
    [SerializeField] AnimationCurve posCurve;
    [SerializeField] float posSpeed;

    private Vector3 posA;
    private Vector3 posB;
    private bool aToB = true;
    private float posLerp;

    [SerializeField] float rotateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        posA = this.transform.position + offsetA;
        posB = this.transform.position + offsetB;
    }

    // Update is called once per frame
    void Update()
    {
        if(aToB)
        {
            this.transform.position = Vector3.Lerp(posA, posB, posCurve.Evaluate(posLerp));
        }
        else
        {
            this.transform.position = Vector3.Lerp(posB, posA, posCurve.Evaluate(posLerp));
        }
        
        posLerp += Time.deltaTime * posSpeed;

        if(posLerp >= 1.0)
        {
            posLerp = 0.0f;
            aToB = !aToB;
        }

        this.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
