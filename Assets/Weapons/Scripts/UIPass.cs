using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UIPass : DrawRenderersCustomPass
{
    private Matrix4x4 originalProjectionMatrix;


    protected override void Execute(CustomPassContext ctx)
    {
        float targetFOV = 60f;
        Matrix4x4 projMat = Camera.main.projectionMatrix;
        float aspectRatio = (float)Screen.width / Screen.height;
        float fovRad = targetFOV * Mathf.Deg2Rad;
        float top = Mathf.Tan(0.5f * fovRad);
        float bottom = -top;
        float right = top * aspectRatio;
        float left = -right;

        projMat.m00 = 2 / (right - left);
        projMat.m11 = 2 / (top - bottom);
        projMat.m02 = (right + left) / (right - left);
        projMat.m12 = (top + bottom) / (top - bottom);

        Camera.main.projectionMatrix = projMat;
        base.Execute(ctx);
        Camera.main.ResetProjectionMatrix();
        Camera.main.fieldOfView = 60f;
       
    }

    protected override void Cleanup()
    {
        base.Cleanup();
        
    }
 
}