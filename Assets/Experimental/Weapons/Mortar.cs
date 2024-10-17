using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mortar : Weapon
{
    [SerializeField]
    GameObject ammoChainLink;

    [SerializeField]
    Transform barrel;

    [SerializeField]
    GameObject mortarShell;

    List<Animator> chainLinks;
    Dictionary<GameObject, int> chainPosition;
    bool canShoot=true;
    uint ammo;
    uint maxAmmo = 12;
    int chainLength = 11;
    float animationDuration;
    void Start()
    {
        ammo = maxAmmo;
        
        //spawn the ammo feed
        chainLinks = new List<Animator>();
        chainPosition = new Dictionary<GameObject, int>();
        
        for(int n=0; n<chainLength; n++)
        {
            
            GameObject link = Instantiate(ammoChainLink, transform.position, transform.rotation, transform);
            link.GetComponent<Animator>().speed = 0;
            link.GetComponent<Animator>().Play("mortarfeed", 0, (float)n / chainLength);
            chainLinks.Add(link.GetComponent<Animator>());
            chainPosition.Add(link, n);
        }

        AnimatorClipInfo[] clips = chainLinks[0].GetCurrentAnimatorClipInfo(0);
        animationDuration = clips[0].clip.length;

    }

    public override bool Fire()
    {
        
        if (canShoot)
        {
            StartCoroutine(Shoot());
            return true;
        }
        else return false;
    }

    IEnumerator Shoot()
    {
        
        canShoot = false;
       
        foreach(Animator anim in chainLinks)
        {
            anim.speed = 1;
        }
        
        yield return new WaitForSeconds(animationDuration/chainLinks.Count);
        foreach (Animator anim in chainLinks)
        {
            anim.speed = 0;
            chainPosition[anim.gameObject]++;
            if (chainPosition[anim.gameObject] > chainLength) chainPosition[anim.gameObject] = 0;
        }
        if (ammo > 0)
        {
            ammo--;
            if (ammo < chainLength)
            {
                foreach (KeyValuePair<GameObject, int> kvp in chainPosition)
                {
                    if (kvp.Value == 0)
                    {
                        kvp.Key.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
            float shotPower = 0f;
            while (autoFire && shotPower<2f)
            {
                shotPower += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            LaunchShot(shotPower);
        }
        yield return new WaitForSeconds(0.5f);
        canShoot = true;
       
    }
    
    public override void ReplenishAmmo()
    {
        ammo = maxAmmo;
        foreach (KeyValuePair<GameObject, int> kvp in chainPosition)
        {
            kvp.Key.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        }
    }
    void LaunchShot(float shotpower)
    {
        GameObject shell = Instantiate(mortarShell, barrel.transform.position, barrel.transform.rotation, null);
        shell.GetComponent<MortarShell>().botThatFiredThisShell = GetComponentInParent<BattleBot>();
        foreach(Collider c in GetComponentInParent<BattleBot>().GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(c, shell.GetComponent<Collider>());
        }

        if (GetComponentInParent<Rigidbody>() != null)
        {
            shell.GetComponent<Rigidbody>().velocity = GetComponentInParent<Rigidbody>().velocity;
        }

        shell.GetComponent<Rigidbody>().AddForce(barrel.transform.forward * (50f + 50f * shotpower), ForceMode.Impulse);
    }

    void Update()
    {
        if (autoFire && canShoot)
        {
            Fire();
        }
    }
}
