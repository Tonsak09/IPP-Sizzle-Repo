using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenObject : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Renderer ditherRenderer;
    private Material ditherMaterial;

    // Start is called before the first frame update
    void Start()
    {
        ditherMaterial = ditherRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        ditherMaterial.SetVector("_Target", target.position);
    }
}
