using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDesign : MonoBehaviour
{
    /// <summary>
    /// Number of spark charges a hidden design can handle 
    /// </summary>
    private const int MAX_CHARGES = 6;

    [SerializeField] List<Transform> chargeOrigins;
    [SerializeField] Renderer hiddenDesignRenderer;
    private Material hiddenDesignMaterial;

    void Start()
    {
        hiddenDesignMaterial = hiddenDesignRenderer.material;
    }

    void Update()
    {
        for (int i = 0; i < chargeOrigins.Count; i++)
        {
            string name = "_ChargeOrigin" + "_" + i;
            hiddenDesignMaterial.SetVector(name, chargeOrigins[i].position);
        }
    }

    public void AddCharge(Transform charge)
    {
        if (chargeOrigins.Count + 1 >= MAX_CHARGES)
            return;

        chargeOrigins.Add(charge);
    }

    private void ManagerCharges()
    {
        // Check if null or out of range 
    }
}
