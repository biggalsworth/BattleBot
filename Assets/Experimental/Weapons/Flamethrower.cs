using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : Weapon
{
    [SerializeField]
    ParticleSystem flameParticles;

    [SerializeField]
    GameObject flame;

    [SerializeField]
    GameObject ammoPointerNeedle;

    [SerializeField]
    Transform muzzle;

    Quaternion needleTargetRotation;

    uint ammo = 50;
    uint maxAmmo = 50;

    float maxValuePointer;
    float minValuePointer;

    bool canShoot = true;
    public override bool Fire()
    {
        if (canShoot && ammo>0)
        {
            StartCoroutine(Shoot());
            return true;
        }
        else return false;
    }

    void Update()
    {
        if (autoFire && canShoot)
        {
            Fire();
        }

        needleTargetRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(minValuePointer, maxValuePointer, (float)ammo / (float)maxAmmo), 0f, 0f));
        ammoPointerNeedle.transform.localRotation = Quaternion.Slerp(ammoPointerNeedle.transform.localRotation, needleTargetRotation, Time.deltaTime);
    }

    public override void ReplenishAmmo()
    {
        ammo = maxAmmo;
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        ammo--;
        GameObject flameInstance = Instantiate(flame, muzzle.position, muzzle.rotation, null);
        flameInstance.GetComponent<Flame>().botThatFiredThisFlame = GetComponentInParent<BattleBot>().competitorLink;
        flameParticles.Play();
        yield return new WaitForSeconds(0.4f);
        canShoot = true;
        flameParticles.Stop();
    }

    // Start is called before the first frame update
    void Start()
    {
        maxValuePointer = -20f;
        minValuePointer = 250f;
        flameParticles.Stop();
    }

   
}
