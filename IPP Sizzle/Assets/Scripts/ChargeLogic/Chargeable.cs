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
    [SerializeField] FinishChargeEvent finishChargeType;
    [Space]
    [SerializeField] protected float charge = 0.0f;


    public virtual void Charge(Transform source)
    {
        switch (finishChargeType)
        {
            case FinishChargeEvent.CLAMP_CHARGE:
                ClampCharge();
                break;
            case FinishChargeEvent.OVERFLOW_CHARGE:
                OverFlowCharge();
                break;
            case FinishChargeEvent.STOP_CHARGE:
                StopCharge();
                break;
        }
    }

    

    /// <summary>
    /// Only allows charge to reach given max amount
    /// </summary>
    private void ClampCharge()
    {
        float delta = chargeRate * Time.deltaTime;
        if (charge + delta > maxChargeAmount)
        {
            charge = maxChargeAmount;
            return;
        }

        charge += delta;
    }

    /// <summary>
    /// Lets charge overflow max charge amount 
    /// </summary>
    private void OverFlowCharge()
    {
        charge += chargeRate * Time.deltaTime;
    }

    /// <summary>
    /// Once threshold has been met stop charge logic and consider this 
    /// chargeable as unlocked 
    /// </summary>
    private void StopCharge()
    {
        if (IsUnlocked())
            return;

        charge += chargeRate * Time.deltaTime;
    }

    /// <summary>
    /// Reduce chage with minimum of 0 charge 
    /// </summary>
    protected virtual void DepleteCharge()
    {
        // Don't allow depletion if for StopCharge
        // and threshold met 

        if (finishChargeType == FinishChargeEvent.STOP_CHARGE)
        {
            if (IsUnlocked())
                return;
        }

        charge -= depleteRate * Time.deltaTime;
        charge = Mathf.Max(charge, 0); // Clamp at 0
    }

    /// <summary>
    /// Resets this chargeable
    /// </summary>
    public virtual void ResetChargeable()
    {
        charge = 0;
    }

    public bool IsUnlocked()
    {
        return charge >= thresholdChargeAmount;
    }

    private enum FinishChargeEvent
    {
        CLAMP_CHARGE,       // Don't allow charge to go past limit 
        OVERFLOW_CHARGE,    // Let charge go past limit 
        STOP_CHARGE         // Once threshold is met stop charging logic 
    }
}
