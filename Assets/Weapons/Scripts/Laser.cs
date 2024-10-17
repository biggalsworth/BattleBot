using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Poolable
{
    //gatling laser projectile 
    [SerializeField]
    GameObject laserImpact;  //impact prefab
    public BattleBot botThatFiredThisLaser;
    Collider col;
    ParticleSystem.EmissionModule p;
    Rigidbody r;
    Light light;

    //per shot damage
    public uint damage;

    
    //process a hit
    private void OnCollisionEnter(Collision collision)
    {
      
        GameObject impact =  Instantiate(laserImpact, collision.GetContact(0).point, Quaternion.identity, collision.GetContact(0).thisCollider.gameObject.transform );
        impact.transform.forward = collision.GetContact(0).normal;

      

         if (collision.gameObject.GetComponentInParent<Shootable>()!=null)
            {
        
                collision.gameObject.GetComponentInParent<Shootable>().TakeHit(damage, botThatFiredThisLaser.competitorLink);
            }

        if (botThatFiredThisLaser != null)
        {
            foreach (Collider c in botThatFiredThisLaser.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(GetComponentInChildren<Collider>(), c, false);
            }
        }

        ObjectPoolManager.instance.ReturnToPool(gameObject);
    }

    
    IEnumerator ReturnToPoolOnTimer()
    {
        yield return new WaitForSeconds(5f);
        ObjectPoolManager.instance.ReturnToPool(gameObject);
    }

    IEnumerator StartParticlesOnDelay()
    {
        yield return new WaitForFixedUpdate();
      //  yield return new WaitForEndOfFrame();
        
        p.enabled = true;
    }

    public override void OnInitialise()
    {
        light = GetComponentInChildren<Light>();
        p = GetComponent<ParticleSystem>().emission;
        col = GetComponent<Collider>();
        r = GetComponent<Rigidbody>();
        col.enabled = false;
        p.enabled = false;
        r.isKinematic = true;
        
    }

    public override void OnExitPool()
    {
        StartCoroutine(StartParticlesOnDelay());
        col.enabled = true;
        r.isKinematic = false;
        light.enabled = true;
       
        
    }

    public override void OnEnterPool()
    {
        light.enabled = false;
        p.enabled = false;
        col.enabled = false;  
        r.isKinematic = true;
    }

    public override void OnReclaimWarning()
    {
        
    }
}
