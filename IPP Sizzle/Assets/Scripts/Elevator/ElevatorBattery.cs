using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorBattery : Chargeable
{
    private bool holdUnlocked;

    private List<Transform> currentChargeSources;
    private List<Transform> ignoreChargeSources;

    void Update()
    {
        UnlockManagement();
        ChargeSourceCleaup();
    }

    private void UnlockManagement()
    {
        // Only at unlocked event not continous 
        if (IsUnlocked() && holdUnlocked == false)
        {
            // Play sound 

            // Animation 
        }

        holdUnlocked = IsUnlocked();
    }

    /// <summary>
    /// Cleanup any nulls in the charge sources 
    /// </summary>
    private void ChargeSourceCleaup()
    {
        for (int i = 0; i < currentChargeSources.Count; i++)
        {
            if (currentChargeSources[i] == null)
            {
                currentChargeSources.RemoveAt(i);
                i--;
            }    
        }

        for (int i = 0; i < ignoreChargeSources.Count; i++)
        {
            if (ignoreChargeSources[i] == null)
            {
                ignoreChargeSources.RemoveAt(i);
                i--;
            }
        }
    }

    public override void Charge(Transform source)
    {
        if (ignoreChargeSources.Contains(source))
            return;

        base.Charge(source);
    }

    public override void ResetChargeable()
    {
        base.ResetChargeable();

        // Note: We don't want the same charge source
        //       to keep turning a button on especially
        //       if the source's lifetime is longer than
        //       the animation of the elevator 

        for (int i = 0;i < currentChargeSources.Count;i++)
        {
            ignoreChargeSources.Add(currentChargeSources[i]);
        }

        currentChargeSources.Clear();
    }
}
