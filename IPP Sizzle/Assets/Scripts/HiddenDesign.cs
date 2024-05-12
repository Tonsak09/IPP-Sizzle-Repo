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

    [Header("Hidden Design")]
    [SerializeField] List<Transform> chargeOrigins;
    [SerializeField] Renderer hiddenDesignRenderer;
    private Material hiddenDesignMaterial;

    [Header("Fully charged animation")]
    [SerializeField] float radiusIncreaseRate;
    [SerializeField] float maxRadius;

    void Start()
    {
        hiddenDesignMaterial = hiddenDesignRenderer.material;
    }

    void Update()
    {
        if(charge <= thresholdChargeAmount)
        {
            ManageChargeTrans();
        }
        else
        {
            AnimateRadiusFill();
        }
    }

    private void AnimateRadiusFill()
    {
        float radius = hiddenDesignMaterial.GetFloat("_ChargeRadius");
        radius += radiusIncreaseRate * Time.deltaTime;

        hiddenDesignMaterial.SetFloat("_ChargeRadius", radius);

        if(radius >= maxRadius)
        {
            chargeOrigins.Clear();
            this.enabled = false;
        }
    }

    /// <summary>
    /// Cleanup transforms if null and send their data to shader 
    /// </summary>
    private void ManageChargeTrans()
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
        base.Charge(source);
        AddCharge(source);
    }
}
