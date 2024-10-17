using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//spawns tyre tracks when a bot moves
public class TyreTracks : MonoBehaviour
{

    public GameObject tracks;
    float distanceTravelled;
    Vector3 lastPosition;


    void Start()
    {
        lastPosition = transform.position;
    }



    void Update()
    {
        //spawn decals over distance

        distanceTravelled += Vector3.Distance(transform.position, lastPosition);
     
        if (distanceTravelled > 0.3f)
        {
            //orient the decal. I use the parent of the wheel's up vector. Raycast would be more precise, but also very expensive considering the rate we make decals.
            GameObject go  = ObjectPoolManager.instance.SpawnFromPool<Track>(transform.position, Quaternion.LookRotation(transform.parent.parent.up * -1f, (transform.position - lastPosition).normalized));
           
           
            distanceTravelled = 0f;
        }


        lastPosition = transform.position;
    }
}
