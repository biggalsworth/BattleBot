using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lance : Weapon
{

    //energy lance weapon type

    [SerializeField]
    GameObject sparks; //impact effect

    [SerializeField]
    public Transform tip; //lance tip

    [SerializeField]
    float charge=0f; //replenishing charge
   

    public override bool Fire()
    {
        return false; //there is no fire action for the lance
    }

    bool Strike()
    {

        //perform a lance strike if the lance is charged

        if (canFire)
        {
            if (charge > 2f)
            {
                anim.SetTrigger("Strike");
                GameObject spark = Instantiate(sparks, tip.transform.position, tip.transform.rotation);
                spark.transform.parent = tip.transform;
                charge = 0f;
                return true;
            }
            return false;
        }
        return false;
        
    }


    //detect a lance strike
    private void OnTriggerEnter(Collider other)
    {

        if (!other.isTrigger)
        {
           
            if (Strike())
            {
                if (other.GetComponentInParent<Shootable>() != null)
                {
                    other.GetComponentInParent<Shootable>().TakeHit((uint)(Mathf.Clamp(Mathf.RoundToInt(GetComponentInParent<BattleBot>().GetStats().velocity.magnitude * 2f + 10), 0f, 50f)), GetComponentInParent<BattleBot>().competitorLink);
                }
            }
        }
      
        
    }

    //replenish charge over time
    void Update()
    {
        charge += Time.deltaTime;
        if (charge > 3f) charge = 3f;
    }
}
