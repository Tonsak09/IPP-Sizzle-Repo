using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Chargeable : MonoBehaviour
{
    [Header("Chargeable")]
    [SerializeField] float maxChargeAmount;
    [SerializeField] float thresholdChargeAmount;
    [SerializeField] float depleteRate;

    private float charge = 0.0f; 

    public virtual void Charge(Transform source)
    {
        charge += Time.deltaTime;
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
