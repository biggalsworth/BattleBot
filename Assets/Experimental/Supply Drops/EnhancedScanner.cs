using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnhancedScanner : CounterMeasure
{
    public Scanner scanner;
    bool isDeployed = false;
    public override bool Deploy()
    {
        isDeployed = !isDeployed;
        return isDeployed;
    }

    private void Start()
    {
        scanner = GetComponent<Scanner>();
        StartCoroutine(Scanner());
    }

    IEnumerator Scanner()
    {
        while (true)
        {
            if (isDeployed) {
                yield return new WaitForSeconds(1f);
                if (GetComponentInParent<BattleBot>().SpendEnergy(1)) scanner.EmitParentedPulse(GetComponentInParent<BattleBot>()) ;
                
            }
            else {
                scanner.EmitParentedPulse(GetComponentInParent<BattleBot>());
                yield return new WaitForSeconds(3f);
            }
        
            scanner.EmitParentedPulse(GetComponentInParent<BattleBot>());
        }
    }

}