using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreventFrustrumCullingInChildren : MonoBehaviour
{
    // Start is called before the first frame update
    
    void LateUpdate()
    {
      
            foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            {
                Bounds adjustedBounds = r.bounds;
                adjustedBounds.center = Camera.main.transform.position + (Camera.main.transform.forward * (Camera.main.farClipPlane - Camera.main.nearClipPlane) * 0.5f);
                adjustedBounds.extents = new Vector3(500f, 500f, 500f);
                r.bounds = adjustedBounds;
            }
            foreach (TextMeshPro tm in GetComponentsInChildren<TextMeshPro>())
            {
                Bounds adjustedBounds = tm.bounds;
                adjustedBounds.center = Camera.main.transform.position + (Camera.main.transform.forward * (Camera.main.farClipPlane - Camera.main.nearClipPlane) * 0.5f);
                adjustedBounds.extents = new Vector3(500f, 500f, 500f);
                tm.renderer.bounds = adjustedBounds;
                //  tm.extraPadding = true;
                //  tm.UpdateMeshPadding();
            }
       

    }

}
