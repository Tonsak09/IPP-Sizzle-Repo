using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class HiddenDesign : Chargeable
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
            if (chargeOrigins[i] == null)
            {
                chargeOrigins.RemoveAt(i);
                i--;

                continue;
            }

            string name = "_ChargeOrigin" + "_" + i;
            hiddenDesignMaterial.SetVector(name, chargeOrigins[i].position);
        }
    }

    /// <summary>
    /// Add a charge source to our code 
    /// </summary>
    /// <param name="charge"></param>
    public void AddCharge(Transform charge)
    {
        if (chargeOrigins.Count + 1 >= MAX_CHARGES)
            return;

        if (chargeOrigins.Contains(charge)) 
            return;

        chargeOrigins.Add(charge);
    }

    public override void Charge(Transform source)
    {
        AddCharge(source);
    }

    private void ManagerCharges()
    {
        // Check if null or out of range 
    }
}
