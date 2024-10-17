using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarImpact : MonoBehaviour
{
    public BattleBot botThatFiredThisMortar;
    int damage = 50;
    
    [SerializeField] AnimationCurve damageDropOff;
    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 10f);

        //the collider might hit multiple colliders on the same bot, but we only want to apply damage once
        //so we need to save what's been hit and check it doesn't belong to the same bot before applying damage.
        List<Shootable> botsAlreadyHit = new List<Shootable>();

        foreach (Collider c in hits)
        {
            //is the thing we've hit a bot?
            if (c.GetComponentInParent<Shootable>() != null)
            {
                //is the bot already one we've hit?
                if (!botsAlreadyHit.Contains(c.GetComponentInParent<Shootable>()))
                {
                    float distance = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
                    
                    //if not, apply damage and add to the list of the ones we've hit
                 
                    c.GetComponentInParent<Shootable>().TakeHit((uint)Mathf.RoundToInt(damage*damageDropOff.Evaluate(distance)), botThatFiredThisMortar.competitorLink);
                    botsAlreadyHit.Add(c.GetComponentInParent<Shootable>());
                }

            }
        }

        //destroy after visual plays out
        Destroy(gameObject, 5f);

    }

    
}
