using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition;

public class RailShot : MonoBehaviour
{
    VisualEffect vfx;
    HDAdditionalLightData light;
    LineRenderer lr;
    public Vector3 start, end, normal;
    void Start()
    {
        light = GetComponentInChildren<HDAdditionalLightData>();
        lr = GetComponent<LineRenderer>();
        vfx = GetComponent<VisualEffect>();
    }

    
    // Update is called once per frame
    void Update()
    {
        light.transform.position = Vector3.Lerp(start, end, 0.5f);
        light.shapeWidth = Vector3.Distance(start, end);
        vfx.SetVector3("Start", lr.GetPosition(0));
        vfx.SetVector3("End", lr.GetPosition(1));

        vfx.SetVector3("HitNormal",RotateVectorAroundCustomUp(normal, normal));
    }
    Vector3 RotateVectorAroundCustomUp(Vector3 vector, Vector3 customUp)
    {
        // Calculate the rotation axis perpendicular to the input vector and the custom "up" direction
        Vector3 rotationAxis = Vector3.Cross(vector, customUp).normalized;

        // Calculate the rotation angle (90 degrees)
        float rotationAngle = 90f;

        // Create a rotation quaternion for the specified angle and axis
        Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);

        // Rotate the vector by the quaternion
        return rotation * vector;
    }
}
