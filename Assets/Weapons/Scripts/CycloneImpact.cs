using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycloneImpact : MonoBehaviour
{

    //logic for the explosion that occurs when a cyclone rocket hits something

    const uint damage = 15; //balance variable
    public BattleBot botThatFiredThisCyclone;
    public AnimationCurve damageDropOff;

    [SerializeField]
    LayerMask impactLayers;
    private void Start()
    {
        
        Collider[] hits = Physics.OverlapSphere(transform.position, 6f, impactLayers);
        
        //the collider might hit multiple colliders on the same bot, but we only want to apply damage once
        //so we need to save what's been hit and check it doesn't belong to the same bot before applying damage.
        List<Shootable> botsAlreadyHit = new List<Shootable>();
        botsAlreadyHit.Add(botThatFiredThisCyclone.GetComponentInParent<Shootable>());


        foreach (Collider c in hits)
        {
            //is the thing we've hit a bot?
            if (c.GetComponentInParent<Shootable>() != null)
            {
                //is the bot already one we've hit?
                if (!botsAlreadyHit.Contains(c.GetComponentInParent<Shootable>()))
                {
                    //if not, apply damage and add to the list of the ones we've hit

                    float distance = Vector3.Distance(c.ClosestPoint(transform.position), transform.position);
                    float damageFloat = damageDropOff.Evaluate(distance);

                    c.GetComponentInParent<Shootable>().TakeHit((uint)Mathf.RoundToInt(damageFloat), botThatFiredThisCyclone.competitorLink);
                    botsAlreadyHit.Add(c.GetComponentInParent<Shootable>());
                }
              
            }
        }

        //destroy after visual plays out
        Destroy(gameObject, 2f);
    }
}
