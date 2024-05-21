using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorBattery : Chargeable
{
    private bool holdUnlocked; 

    void Update()
    {
        // Only at unlocked event not continous 
        if(IsUnlocked() && holdUnlocked == false)
        {
            // Play sound 

            // Animation 
        }

        holdUnlocked = IsUnlocked();
    }

    
}
