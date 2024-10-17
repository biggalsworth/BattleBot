using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : CounterMeasure
{
    Animator anim;
    bool isDeployed;
    BattleBot owner;
    // Start is called before the first frame update
    void Start()
    {
        owner = GetComponentInParent<BattleBot>();
        anim = GetComponent<Animator>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            
            if (r.material.HasColor("_Color"))
            {
                r.material.SetColor("_Color", owner.GetStats().defaultColor);
            }

        }
        StartCoroutine(DisableOnLowEnergy());
        StartCoroutine(EnergyDrain());
    }

    IEnumerator DisableOnLowEnergy()
    {
        while (true)
        {
            if (owner.GetEnergy() < 0f && isDeployed)
            {
                Deploy();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
    public override bool Deploy()
    {
        isDeployed = !isDeployed;
        owner.isShielded = isDeployed;
        anim.SetBool("isDeployed", isDeployed);
        return true;
    }

    IEnumerator EnergyDrain()
    {
        while (true)
        {
            if (isDeployed)
            {
                owner.SpendEnergy(1);     
            }
            yield return new WaitForSeconds(1f);
        }
    }
  
  
}
