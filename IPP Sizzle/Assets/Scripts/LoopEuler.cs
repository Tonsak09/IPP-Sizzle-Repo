using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopEuler : MonoBehaviour
{
    [SerializeField] Vector3 offsetA;
    [SerializeField] Vector3 offsetB;
    [SerializeField] AnimationCurve rotCurve;
    [SerializeField] float rotSpeed;

    private Vector3 rotA;
    private Vector3 rotB;
    private bool aToB = true;
    private float rotLerp;

    // Start is called before the first frame update
    void Start()
    {
        rotA = this.transform.eulerAngles + offsetA;
        rotB = this.transform.eulerAngles + offsetB;
    }

    // Update is called once per frame
    void Update()
    {
        if (aToB)
        {
            this.transform.eulerAngles = Vector3.Slerp(rotA, rotB, rotCurve.Evaluate(rotLerp));
        }
        else
        {
            this.transform.eulerAngles = Vector3.Slerp(rotB, rotA, rotCurve.Evaluate(rotLerp));
        }

        rotLerp += Time.deltaTime * rotSpeed;

        if (rotLerp >= 1.0)
        {
            rotLerp = 0.0f;
            aToB = !aToB;
        }
    }
}
