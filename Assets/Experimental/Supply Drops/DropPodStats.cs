using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DropPodStats 
{
    public enum DroppodContentType {EMPTY, HEALTH, AMMO, ENERGY, RADAR, TURRET, WEAPONSWAP};

    public bool hasLanded; //has the pod landed
    public string ownerName; //which battlebot ordered it?
    public DroppodContentType contents; //what contents are in it?
    public Vector3 position;

}
