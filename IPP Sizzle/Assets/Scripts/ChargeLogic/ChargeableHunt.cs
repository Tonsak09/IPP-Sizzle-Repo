using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ChargeableHunt : ChargeableSet
{
    [Tooltip("If not null will output how many chargeables still need to be collected")]
    [SerializeField] TextMeshProUGUI textMesh;

    private void Update()
    {
        if(AllAmberUnlocked())
        {
            textMesh.text = "Complete!";

            // Animate completion 

            // Play sound 

            return;
        }

        textMesh.text = "Secretes Left: " + GetChargeablesUnlockedCount() + " / " + GetChargeablesCount();
    }
}
