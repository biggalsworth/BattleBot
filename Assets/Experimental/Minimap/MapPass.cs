using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Reflection;

//adapted from https://github.com/alelievr/HDRP-Custom-Passes/blob/c1e6f31317fca4960780525fcf35016a3e63f460/Assets/Scenes/CameraDepthBaking/CameraDepthBake.cs#L39
class MapPass : CustomPass
{
    public Camera bakingCamera = null;
    public RenderTexture mapTexture = null;
   
    

    protected override bool executeInSceneView => false;
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        cmd.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.None);
       
    }
    protected override void Execute(CustomPassContext ctx)
    {
        if (ctx.hdCamera.camera == bakingCamera || bakingCamera == null || ctx.hdCamera.camera.cameraType == CameraType.SceneView)
            return;

        if (mapTexture == null)
            return;

        // We need to be careful about the aspect ratio of render textures when doing the culling, otherwise it could result in objects poping:
        if (mapTexture != null)
            bakingCamera.aspect = Mathf.Max(bakingCamera.aspect, mapTexture.width / (float)mapTexture.height);
       
        bakingCamera.TryGetCullingParameters(out var cullingParams);
        
        cullingParams.cullingOptions = CullingOptions.None;

        // Assign the custom culling result to the context
        // so it'll be used for the following operations
        ctx.cullingResults = ctx.renderContext.Cull(ref cullingParams);
   
      
        var overrideDepthTest = new RenderStateBlock(RenderStateMask.Depth) { depthState = new DepthState(true, CompareFunction.LessEqual) };

        // Depth
        if (mapTexture != null)
            CustomPassUtils.RenderDepthFromCamera(ctx, bakingCamera, mapTexture, ClearFlag.All, bakingCamera.cullingMask, overrideRenderState: overrideDepthTest);
       

    }
}