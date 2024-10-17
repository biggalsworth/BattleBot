using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//abstract base class for all weapons
public abstract class Weapon : MonoBehaviour
{
    public enum WeaponType {LASER, RAILGUN, CYCLONE, LANCE, MORTAR, FLAMETHROWER};

    [SerializeField]
    public WeaponType weaponType;
    //set for constant fire. all derived classes need to implement their own logic to apply this
    protected bool autoFire = false;

    //set to enable/disable firing (e.g. disable on death)
    protected bool canFire;
    
    
    //should contain the fire logic for the individual weapon
    public abstract bool Fire();

    //override if you need a derived weapon to do something on Start()
    protected virtual void OnStart() {}


    //animator to run spawn/despawn animations. Unique to each weapon.
    protected Animator anim;

    public virtual void ReplenishAmmo()
    {
        //override if the weapon uses ammo
    }
    //getter and setter methods for fire control
    public void setAutoFire(bool activateFiring)
    {
        autoFire = activateFiring;
    }

    public bool getAutoFire()
    {
        return autoFire;
    }

 
    private void Start()
    {
        anim = GetComponent<Animator>();
        canFire = true;
        OnStart();
    }

    //runs if the weapon is ejected by the bot
    public void Detach()
    {
        canFire = false;
        autoFire = false;

        foreach(Transform t in transform)
        {
            Destroy(gameObject, 1f);
        }

        if(anim) anim.SetTrigger("Despawn");

      
    }

}
