using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{

    BattleBot controller;
    float throttle = 300f; //hardcoded max torque (Nm)
    float steer = 50f; //hardcoded max steer angle (degrees)

    //assigned wheels
    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steerWheels;

    //movable centre of mass
    public Transform centreOfMass;

    public Rigidbody mass; //object repesenting the mass of the bot
    
    //set the centre of mass to stabilise the bot
    void Awake()
    {
        controller = transform.GetComponentInParent<BattleBot>();
        mass.centerOfMass = centreOfMass.localPosition;

    }

    // apply torque and steering based on BattleBot values
    void Update()
    {
        
        foreach(WheelCollider c in throttleWheels)
        {
            c.motorTorque = Mathf.Clamp(controller.GetThrottleState(), -1f, 1f) * throttle;
            Vector3 pos = c.gameObject.transform.position;
            Quaternion rot = c.gameObject.transform.rotation;
            c.GetWorldPose(out pos, out rot);
            c.gameObject.transform.rotation = rot;
        }

        foreach (WheelCollider c in steerWheels)
        {
            c.steerAngle = Mathf.Clamp(controller.GetSteerState(), -1f, 1f) * steer;
        }
    }
}
