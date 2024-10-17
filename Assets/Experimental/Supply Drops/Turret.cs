using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Powerup, Shootable
{
    [SerializeField]
    Weapon mountedWeapon;
    [SerializeField]
    Transform weaponMount, pivot;
    [SerializeField]
    GameObject deathExplosion;

    [SerializeField]
    LayerMask targetLayers;

    Weapon leftWeapon, rightWeapon;
    Transform target;

    float health = 30f;
    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.TURRET;
    }
    // Start is called before the first frame update
    void Start()
    {
        target = null;
        StartCoroutine(Initialise());
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (m.HasColor("_BotColor"))
                {
                    m.SetColor("_BotColor", linkedPod.owner.GetColor());
                }
            }
        }
    }

    IEnumerator Initialise()
    {
        yield return new WaitForSeconds(1.5f);
        
        //spawn weapons
        leftWeapon = Instantiate(mountedWeapon, weaponMount.transform.position, weaponMount.transform.rotation);
        leftWeapon.transform.parent = weaponMount.transform;
        rightWeapon = Instantiate(mountedWeapon, weaponMount.transform.position, weaponMount.transform.rotation);
        rightWeapon.transform.parent = weaponMount.transform;
        //flip right weapon
        Vector3 scale = rightWeapon.transform.localScale;
        scale.x = -1f * scale.x;
        rightWeapon.transform.localScale = scale;
        StartCoroutine(Scan());
        
    }

    IEnumerator Scan()
    {
        while (target == null)
        {
            pivot.transform.Rotate(0f, Time.deltaTime*40f, 0f);
            yield return new WaitForFixedUpdate();
            RaycastHit hit;
            if(Physics.Raycast(weaponMount.transform.position, weaponMount.transform.forward, out hit, 20f, targetLayers))
            {
                if (hit.collider.GetComponentInParent<Shootable>()!=null)
                {
                    leftWeapon.setAutoFire(true);
                    rightWeapon.setAutoFire(true);
                    yield return new WaitForSeconds(2f);
                    leftWeapon.setAutoFire(false);
                    rightWeapon.setAutoFire(false);
                    yield return new WaitForSeconds(3f);
                }
            }
        }
        
    }


    // Update is called once per frame

    public void TakeHit(uint damage, BotCompetitor shooter)
    {
        health -= damage;
        if (health < 0f)
        {
            Instantiate(deathExplosion, transform.position, transform.rotation);
            //linkedPod.Despawn();
            Destroy(gameObject);
           
        }
    }

}