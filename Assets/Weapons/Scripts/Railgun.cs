using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Railgun : Weapon
{

   
    //logic for the railgun

    const float warmupTime = 0.5f; //balance variable
    const float refireDelay = 0.1f; //balance variable
    const uint damage = 20; //balance variable

    //assignments for various parts of the weapon and spawned shots
    [SerializeField]
    Transform muzzle;
    [SerializeField]
    GameObject shotPrefab;
    [SerializeField]
    GameObject impactPrefab;

    //layers the weapon hits
    [SerializeField]
    LayerMask hittableLayers;

  


    bool firing = false;
    public override bool Fire()
    {
        anim.speed = 2f / (refireDelay+warmupTime);
        if (!firing)
        {
           GetComponentInParent<BattleBot>().SpendEnergy(2);
           StartCoroutine(Shoot());
           return true;
        }
        else return false;
    }

    private void Update()
    {
        if (autoFire) Fire();
    }

    //perform a shot. raycast is used to determine the hit, line renderer for the visuals.
    public IEnumerator Shoot()
    {
        
        firing = true;
        anim.SetTrigger("Fire");
        yield return new WaitForSeconds(warmupTime);
        GameObject shot;
        RaycastHit hit;
        if(Physics.Raycast(muzzle.position, muzzle.forward, out hit, 500f, hittableLayers, QueryTriggerInteraction.Ignore))
        {
           
            shot = Instantiate(shotPrefab, muzzle.transform.position, muzzle.transform.rotation);
            shot.GetComponent<LineRenderer>().SetPosition(0, muzzle.transform.position);
            shot.GetComponent<LineRenderer>().SetPosition(1, hit.point);
            shot.GetComponent<RailShot>().start = muzzle.transform.position;
            shot.GetComponent<RailShot>().end = hit.point;
            shot.GetComponent<RailShot>().normal = hit.normal;
            Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal), hit.collider.gameObject.transform);

            BattleBot shooter = GetComponentInParent<BattleBot>();
            if (shooter == null)
            {
                if (GetComponentInParent<Powerup>())
                {
                    shooter = GetComponentInParent<Powerup>().linkedPod.owner;
                }
            }

            if (hit.collider.GetComponentInParent<Shootable>()!=null) hit.collider.GetComponentInParent<Shootable>().TakeHit(10, shooter.competitorLink);
        }
        else
        {
            shot = Instantiate(shotPrefab, muzzle.transform.position, muzzle.transform.rotation, muzzle.transform);
            shot.GetComponent<RailShot>().start = muzzle.transform.position;
            shot.GetComponent<RailShot>().end = muzzle.transform.position + muzzle.transform.forward * 200f;
            shot.GetComponent<LineRenderer>().SetPosition(0, muzzle.transform.position);
            shot.GetComponent<LineRenderer>().SetPosition(1, muzzle.transform.position + muzzle.transform.forward*100f);
            shot.GetComponent<RailShot>().normal = muzzle.transform.forward;
        }
        float timer = 0f;
        while (timer < refireDelay)
        {
            timer += Time.deltaTime;
            if (shot) shot.GetComponent<LineRenderer>().SetPosition(0, muzzle.transform.position);
            yield return new WaitForEndOfFrame();
        }

        firing = false;
    }


}
