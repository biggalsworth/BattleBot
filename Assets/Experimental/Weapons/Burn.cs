using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : MonoBehaviour
{

    public BotCompetitor owner;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BurnRoutine(owner));
    }

    IEnumerator BurnRoutine(BotCompetitor owner)
    {
        float ticks = 5;
       
        yield return new WaitForFixedUpdate();
       
        while (ticks > 0)
        {
            
            List<Shootable> botsAlreadyHit = new List<Shootable>();
            if (GetComponentInParent<Shootable>() != null)
            {
                if (!botsAlreadyHit.Contains(GetComponentInParent<Shootable>()))
                {
                    GetComponentInParent<Shootable>().TakeHit(2, owner);
                    botsAlreadyHit.Add(GetComponentInParent<Shootable>());
                }
            }
            yield return new WaitForSeconds(1f);
            ticks--;
        }
    }

}
