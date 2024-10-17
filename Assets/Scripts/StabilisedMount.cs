using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilisedMount : MonoBehaviour
{

    //stablises child game objects along the horizontal plane. 
  
    bool automaticStabilisation = true;
    float stabliserSpeed = 10f;
    float maxAngle = 90f;

    Quaternion previousRotation;

    // Update is called once per frame
    void LateUpdate()
    {
       
        
        //disable stabilisation if the parent is falling over
        if (Vector3.Angle(transform.parent.up, Vector3.up) > maxAngle)
        {
            automaticStabilisation = false;
        }
        else automaticStabilisation = true;


        //apply stabilisation or smoothly reset to 0
        if(automaticStabilisation) Stabilise();
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
        }
    }

    //stabilise the mount to always remain level with the horizon
    void Stabilise()
    {
      

        Vector3 target = transform.parent.position + new Vector3(transform.parent.forward.x, 0f, transform.parent.forward.z).normalized * 15f;
        Vector3 targetPosition = new Vector3(target.x, transform.position.y, target.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), Time.deltaTime*stabliserSpeed);

    

    }

  

}
