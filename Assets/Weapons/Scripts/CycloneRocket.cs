using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class CycloneRocket : MonoBehaviour
{
  
    //code for an individual rocket launched from the cyclone.
    
    [SerializeField]
    GameObject explosion;
    Animator anim;
    Rigidbody rb;
    BattleBot shooter;
    VisualEffect vfx;
    const float respawnTime = 6f; //balance variable
    bool isArmed;
   
    Transform cachedParent;

   public bool isFiring;
    private void Start()
    {
        vfx = GetComponentInChildren<VisualEffect>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        anim = GetComponent<Animator>();
        cachedParent = transform.parent;
        shooter = cachedParent.GetComponentInParent<BattleBot>();
    }

    private void Update()
    {
       
        if (isFiring&&shooter)
        {
            
            foreach (BattleBot b in BattleBot.allBots)
            {
              
                if (b!=shooter&&Vector3.Distance(rb.position, b.GetStats().position) < 4f){
                    if(isArmed) Detonate(Vector3.up);
                }
            }
        }
    }

   

    //fires if requested by Cyclone
    public bool Launch()
    {
       
        if (!isFiring)
        {

         
           
            rb.isKinematic = false;

            GetComponent<SphereCollider>().enabled = true;

            foreach (Collider c in transform.root.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(c, GetComponent<Collider>(), true);
            }

            GetComponent<SphereCollider>().radius = 0.2f;

            isFiring = true;

            anim.ResetTrigger("Impact");
            anim.SetTrigger("Launch");
            transform.parent = null;
            isArmed = true;
            return true;
        }

        else return false;
    }


    void Detonate(Vector3 direction)
    {
        isArmed = false;
        anim.ResetTrigger("Respawn");
        anim.SetTrigger("Impact");
        if (cachedParent)
        {

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;
            GameObject impactExplosion = Instantiate(explosion, transform.position, Quaternion.LookRotation(direction));
                BattleBot shooter = cachedParent.GetComponentInParent<BattleBot>();
                if (shooter == null)
                {
                    if (cachedParent.GetComponentInParent<Powerup>())
                    {

                        shooter = cachedParent.GetComponentInParent<Powerup>().linkedPod.owner;
                    }
                }
                impactExplosion.GetComponent<CycloneImpact>().botThatFiredThisCyclone = shooter;
                
     
        
            rb.isKinematic = true;
            StartCoroutine(Respawn());
        }
    }
    

    //hit something and explode
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 direction = collision.GetContact(0).normal;
        if(isArmed) Detonate(direction);
        
    }

    //cycle the rocket back onto the launcher and reset it (mini object pool pattern!)
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);
        transform.parent = cachedParent;

       
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<SphereCollider>().enabled = false;
        if (transform.parent != null)
        {
            transform.position = transform.parent.position;
            transform.rotation = transform.parent.rotation;
        }
        else Destroy(gameObject);

        yield return new WaitForEndOfFrame();

      
 
        yield return new WaitForSeconds(respawnTime - 2.5f);
        anim.ResetTrigger("Launch");
        anim.SetTrigger("Respawn");
        yield return new WaitForSeconds(0.5f);
        isFiring = false;
       
    }

}
