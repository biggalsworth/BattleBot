using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cyclone : Weapon
{
    [SerializeField]
    CycloneRocket[] cyclones = new CycloneRocket[6];

    float fireDelay = 0.2f;
    float timeSinceLastShot = 0f;
    float rocketForce = 800f;
    public override bool Fire()
    {
        foreach (CycloneRocket rocket in cyclones)
        {
            rocket.GetComponent<ConstantForce>().relativeForce = new Vector3(0f, 7f, rocketForce);
      
            if (rocket.Launch()) return true;
        }
        return false;
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        if (autoFire && timeSinceLastShot>fireDelay)
        {
            Fire();
            timeSinceLastShot = 0f;
        }
    }

    private void LateUpdate()
    {
        foreach (CycloneRocket rocket in cyclones)
        {
            if (!rocket.isFiring&&rocket.transform.parent!=null)
            {
                rocket.transform.position = rocket.transform.parent.position;
                rocket.transform.rotation = rocket.transform.parent.rotation;
            }
        }
    }
}
