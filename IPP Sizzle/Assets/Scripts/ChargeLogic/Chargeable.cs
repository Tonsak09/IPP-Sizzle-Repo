using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chargeable : MonoBehaviour
{
    [Header("Chargeable")]
    [Tooltip("The max amount of charge that this object can hold")]
    [SerializeField] protected float maxChargeAmount;
    [Tooltip("Threshold for this chargeable's logic to be considered charged")]
    [SerializeField] protected float thresholdChargeAmount;
    [SerializeField] float chargeRate;
    [SerializeField] float depleteRate;

    [SerializeField] protected float charge = 0.0f;
    

    public virtual void Charge(Transform source)
    {
        float delta = chargeRate * Time.deltaTime;
        if(charge + delta > maxChargeAmount)
        {
            charge = maxChargeAmount;
            return;
        }

        charge += delta;
    }

    protected virtual void DepleteCharge()
    {
        charge -= depleteRate * Time.deltaTime;
    }

    private enum FinishChargeEvent
    {
        CLAMP_CHARGE,       // Don't allow charge to go past limit 
        OVERFLOW_CHARGE,    // Let charge go past limit 
        STOP_CHARGE         // Once threshold is met stop charging logic 
    }
}
