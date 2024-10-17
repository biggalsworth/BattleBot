using UnityEngine;
using System;

[Serializable]
public struct BattleBotStats {


    //set by creator
    public string name;
    public string creatorName;
    public Weapon leftWeapon;
    public Weapon rightWeapon;
    public CounterMeasure counterMeasure;
    [ColorUsage(true,true)]
    public Color defaultColor;


    //runtime modified
    [HideInInspector]
    public Vector3 forwardVector;
    [HideInInspector]
    public Vector3 upVector;
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 velocity;
    [HideInInspector]
    public Vector3 angularVelocity;
    [HideInInspector]
    public int uniqueID;
    [HideInInspector]
    public int kills;
    [HideInInspector]
    public bool isShielded;
    [HideInInspector]
    public bool isFlared;
    [HideInInspector]
    public float energy;
    [HideInInspector]
    public float health;

    //is this data current or expired in a scan?
 

    public BattleBotStats(BattleBotStats toCopy)
    {
   
        name = toCopy.name;
        uniqueID = toCopy.uniqueID;
        forwardVector = toCopy.forwardVector;
        upVector = toCopy.upVector;
        creatorName = toCopy.creatorName;
        leftWeapon = toCopy.leftWeapon;
        rightWeapon = toCopy.rightWeapon;
        counterMeasure = toCopy.counterMeasure;
        defaultColor = toCopy.defaultColor;
        position = toCopy.position;
        velocity = toCopy.velocity;
        angularVelocity = toCopy.angularVelocity;
        kills = toCopy.kills;
        isShielded = toCopy.isShielded;
        isFlared = toCopy.isFlared;
        energy = toCopy.energy;
        health = toCopy.health;
    }
}
