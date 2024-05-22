using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeableSet : MonoBehaviour
{
    [SerializeField] List<Chargeable> chargeables;

    /// <summary>
    /// Gets whether all the cheargeables are unlocked or not 
    /// </summary>
    /// <returns></returns>
    protected bool AllAmberUnlocked()
    {
        for (int i = 0; i < chargeables.Count; i++)
        {
            if (chargeables[i].IsUnlocked() == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets count of chargeables that are unlocked 
    /// </summary>
    /// <returns></returns>
    protected int GetChargeablesUnlockedCount()
    {
        int count = 0;
        for (int i = 0; i < chargeables.Count; i++)
        {
            if (chargeables[i].IsUnlocked())
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected int GetChargeablesCount()
    {
        return chargeables.Count;
    }
}
