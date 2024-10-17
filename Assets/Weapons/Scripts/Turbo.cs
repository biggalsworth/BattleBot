using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turbo : CounterMeasure
{

    //logic for the turbo countermeasure

    Animator anim;
    Rigidbody rb;
    bool onCooldown = false;
   
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponentInParent<Rigidbody>();
    }

    //method called to request a boost
    public override bool Deploy()
    {
        if (!onCooldown)
        {
            StartCoroutine(TurboBoost());
            return true;
        }
        else return false;
    }

    //visual FX of the particle system is driven by the animator
    void Update()
    {
        anim.SetFloat("Throttle", rb.velocity.magnitude*0.1f);
    }

    //perform a turbo boost by adding considerable force
    IEnumerator TurboBoost()
    {
        onCooldown = true;
        float duration = 1f;
        while (duration > 0f)
        {
            Vector3 forceToAdd = transform.forward * 8000f;
            forceToAdd.y = -200f;
            rb.AddForce(forceToAdd);
            yield return new WaitForFixedUpdate();
            duration -= Time.fixedDeltaTime;
        }
        yield return new WaitForSeconds(5f);
        onCooldown = false;
       
    }
}
