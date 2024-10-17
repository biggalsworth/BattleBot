using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIAnchor : MonoBehaviour
{
    public float screenWidthFactor;
    public float offsetFactor;
    public float screenHeightFactor;
    

    //scales the in-game UI - I used a world canvas so needed to scale manually
    //to do this I just put the numbers into a guess at an appropriate equation
    //then tested values to get the desired behaviour
    void LateUpdate()
    {
        Vector3 localPos = transform.localPosition;

        localPos.x = 0f - (Screen.width / 1024f) * screenWidthFactor + offsetFactor + (Screen.height/1024f) * screenHeightFactor;
        
        transform.localPosition = localPos;
    }
}
