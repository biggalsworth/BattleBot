using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCam : ArenaCam
{
    //pivots for the various bits. anchor is where the main camera fixes to.
    [SerializeField]
    Transform anchor;

    public float targetDistance;
    public float fov = 60f;
    float offset;


    GameObject camanchor;
    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        camanchor = Instantiate(new GameObject());
        camanchor.name = "Drone camera anchor";
        offset = Random.Range(0f, 90f);
        cameraType = CameraType.Drone;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
       
        anchor.transform.LookAt(target.transform);
        anchor.transform.Rotate(15f, 0f, 0f);
       

        camanchor.transform.position = Vector3.Lerp(camanchor.transform.position, anchor.transform.position, Time.deltaTime * 5f);
        camanchor.transform.rotation = Quaternion.Slerp(camanchor.transform.rotation, anchor.transform.rotation, Time.deltaTime * 5f);
       
    }
    protected override float GetRotationAngle()
    {
        return 0.5f;
    }

    protected override Transform GetMountTransform()
    {
        return anchor.transform;
    }

    public override Transform GetAnchor()
    {
        return anchor;
    }

    /*
    * each camera intermittently checks what it can see to generate a priority value
    * this is used by the manager to select which camera to switch to during play
    * 
    * TODO - a proper, balanced fuzzy logic calculation
    */
   
}