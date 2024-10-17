using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flares : CounterMeasure
{

    List<Flare> allFlares;
    bool onCooldown = false;
    BattleBot owner;
 
    // Start is called before the first frame update
    void Start()
    {
        allFlares = new List<Flare>();
        allFlares.AddRange(GetComponentsInChildren<Flare>());
        owner = GetComponentInParent<BattleBot>();
    }

    public override bool Deploy()
    {
        if (!onCooldown)
        {
            StartCoroutine(Launch());
            return true;
        }
        else return false;
    }

    IEnumerator Launch()
    {
        onCooldown = true;
        owner.isFlared = true;
        foreach(Flare flare in allFlares)
        {
            flare.Launch();
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(5f);
        owner.isFlared = false;
        yield return new WaitForSeconds(10f);
        onCooldown = false;
      
    }

   

}
