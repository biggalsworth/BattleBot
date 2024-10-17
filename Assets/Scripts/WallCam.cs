using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCam : ArenaCam
{
    //pivots for the various bits. anchor is where the main camera fixes to.
    [SerializeField]
    Transform pivot, camera, anchor;

    public float targetDistance;
    public float fov=60f;
    float offset;
    bool isActive;
    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
        offset = Random.Range(0f, 90f);
        cameraType = CameraType.Wall;
      
    }



    // Update is called once per frame
    /* void Update()
     {

         if (target)
         {
             targetDistance = Vector3.Distance(transform.position, target.transform.position);
             camera.transform.LookAt(target.transform);

         }
         else
         {
             Vector3 eulerRot = pivot.transform.localRotation.eulerAngles;
             eulerRot.y = -22.5f + Mathf.PingPong(offset + Time.time * 2f, 45);
             pivot.transform.localRotation = Quaternion.Slerp(pivot.transform.localRotation, Quaternion.Euler(eulerRot), Time.deltaTime*3f);
         }
     }*/

    private void LateUpdate()
    {
  //      Camera.main.fieldOfView = 30f;
        camera.transform.LookAt(target.transform);
    }
    public override Transform GetAnchor()
    {
        return anchor;
    }

    protected override float GetRotationAngle()
    {
        return 0.5f;
    }

    protected override Transform GetMountTransform()
    {
        return transform;
    }



}
