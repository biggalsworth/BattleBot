using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotCam : ArenaCam
{

    bool firstPersonMode = true;

    //transform to parent the camera to in third person
    GameObject thirdPersonAnchor;

    //transform to smoothly move the actual 3rd person anchor towards to prevent jitter
    GameObject thirdPersonAnchorTarget;
    

    float thirdPersonFollowDistance = 7f; //target follow distance
    float thirdPersonFollowSpeed = 3f; //tracking speed
    public void Awake()
    {
        base.Awake();
        //spawn anchors for 3rd person mode behind the target
        thirdPersonAnchor = new GameObject();
        thirdPersonAnchorTarget = new GameObject();
        thirdPersonAnchor.name = "Third Person Camera Anchor";
        thirdPersonAnchorTarget.name = "Third Person Camera Target";
        thirdPersonAnchorTarget.transform.parent = transform;
        thirdPersonAnchorTarget.transform.localPosition = new Vector3(0f, 2f, -1f*thirdPersonFollowDistance);
        thirdPersonAnchorTarget.transform.LookAt(transform);
        thirdPersonAnchor.transform.position = thirdPersonAnchorTarget.transform.position;
        thirdPersonAnchor.transform.rotation = thirdPersonAnchorTarget.transform.rotation;
        cameraType = CameraType.Bot;
    }


    //GetAnchor is called when the camera manager requests control of this camera
    public override Transform GetAnchor()
    {
       
        //slightly hacky toggle implementation - if we request the same camera again, it'll toggle between 1st/3rd person
        firstPersonMode = !firstPersonMode;

      
        if (firstPersonMode)
        {
            return transform;
        }
        else
        {
            return thirdPersonAnchor.transform;
        }
    }


    private void LateUpdate()
    {
   
        //I wish unity would simplify the way you construct layer masks in code to avoid this bitshift stuff!
        RaycastHit hit;
        int mask = 0;
        mask |= 1 << LayerMask.NameToLayer("NotNavigable");
   
        //This is clip prevention for the camera - if a raycast from the focus (bot head) towards the camera's target position hits a wall, move the target position to the point it hits

        if (Physics.Raycast(transform.position, thirdPersonAnchorTarget.transform.position - transform.position, out hit, thirdPersonFollowDistance+2f, mask))
        {
            thirdPersonAnchor.transform.position = Vector3.Lerp(thirdPersonAnchor.transform.position, hit.point, Time.deltaTime * thirdPersonFollowSpeed);    
        }
        else
        {
            thirdPersonAnchor.transform.position = Vector3.Lerp(thirdPersonAnchor.transform.position, thirdPersonAnchorTarget.transform.position, Time.deltaTime * thirdPersonFollowSpeed);   
        }
        thirdPersonAnchor.transform.rotation = Quaternion.Slerp(thirdPersonAnchor.transform.rotation, thirdPersonAnchorTarget.transform.rotation, Time.deltaTime * thirdPersonFollowSpeed);
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
