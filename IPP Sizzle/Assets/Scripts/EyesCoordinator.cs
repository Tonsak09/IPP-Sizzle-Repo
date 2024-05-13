using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyesCoordinator : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] List<Renderer> eyeRenderers;
    [SerializeField] Vector3 eyesCenterOffset;
    [SerializeField] Transform eyesCenter;

    private Material eyeMaterial;
    float mag;

    private void Start()
    {
        eyeMaterial = eyeRenderers[0].material;
        mag = eyeMaterial.GetVector("_PupilOrigin").magnitude;
    }

    void Update()
    {
        if(!target)
            return;

        foreach(Renderer r in eyeRenderers)
        {
            ProcessEye(r);
        }
    }

    private void ProcessEye(Renderer eye)
    {
        Vector3 dir = eye.transform.InverseTransformDirection(target.position - eye.transform.position).normalized;
        eye.material.SetVector("_PupilOrigin", dir * mag);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(eyesCenter.position + eyesCenterOffset, 0.05f);
    }
}
