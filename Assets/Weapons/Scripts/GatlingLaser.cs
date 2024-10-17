using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingLaser : Weapon
{

    //gatling laser weapon behaviour

    [SerializeField]
    GameObject projectile; //what to spawn on shot

    [SerializeField]
    Transform muzzle; //muzzle location

    [SerializeField]
    Transform barrel; //barrel location (for barrel spin)

    float targetRotation;
    float timeSinceLastShot;

    //how fast the laser fires
    float refireRate = 0.2f;
    uint damage = 10;
   

    private void Update()
    {
        //rotate barrels if firing
        if (targetRotation > 0f)
        {
            barrel.transform.Rotate(0f, 0f, Time.deltaTime * 500f);
            targetRotation -= Time.deltaTime * 500f;
        }

        
        timeSinceLastShot += Time.deltaTime;


        //fire if autofire enabled
        if (autoFire&&timeSinceLastShot>refireRate)
        {
            Fire();
            timeSinceLastShot = 0f;
        }
    }
    public override bool Fire()
    {
        //fire a single shot
        if (canFire)
        {
            GameObject laser = ObjectPoolManager.instance.SpawnFromPool<Laser>(muzzle.position, muzzle.rotation);
            laser.GetComponent<Rigidbody>().velocity = muzzle.transform.forward * 30f;
            laser.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(muzzle.transform.forward);
            BattleBot shooter = GetComponentInParent<BattleBot>();
            if (shooter != null)
            {
                shooter.SpendEnergy(1);
                foreach (Collider c in shooter.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(laser.GetComponentInChildren<Collider>(), c, true);
                }
            }


            else
            {
                if (GetComponentInParent<Powerup>())
                {
                    shooter = GetComponentInParent<Powerup>().linkedPod.owner;
                }

            }

               
                if (GetComponentInParent<Rigidbody>())
                {
                    laser.GetComponent<Rigidbody>().velocity += GetComponentInParent<Rigidbody>().velocity;
                }

                
            
            
            
            laser.GetComponent<Laser>().botThatFiredThisLaser = shooter;
            laser.GetComponent<Laser>().damage = damage;
            laser.GetComponent<Rigidbody>().isKinematic = false;
            
            
          
          targetRotation += 120f;
            return true;
        }
        return false;
    }


    
}
