using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotAnim : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float speed;

    private Vector3 origin;
    private Vector3 target;
    float lerp;

    // Start is called before the first frame update
    void Start()
    {
        origin = transform.eulerAngles;
        target = transform.eulerAngles + offset;
    }

    // Update is called once per frame
    void Update()
    {
        if (lerp < 1.0)
        {
            this.transform.eulerAngles = Vector3.Lerp(origin, target, curve.Evaluate(lerp));
            lerp += Time.deltaTime * speed;
        }
    }
}
