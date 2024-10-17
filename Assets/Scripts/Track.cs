using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Track : Poolable
{
    //an instance of a tyre track. projects a decal onto the floor.
    //uses the object pool system (default 1024 decals at runtime - fortunately HDRPs decal system is optimised for this provided they share a material)

    DecalProjector projector;
    Coroutine fade;
   
    public override void OnInitialise()
    {
        projector = GetComponent<DecalProjector>(); 
    }


    //fade decal if pool is reaching capacity
    public override void OnReclaimWarning() 
    {
        fade = StartCoroutine(Fade());
    }

    //disable when entering object pool
    public override void OnEnterPool()
    {
        projector.enabled = false;
        StopCoroutine(fade);

    }

    //enable when exiting pool
    public override void OnExitPool()
    {
        projector.enabled = true;
        projector.fadeFactor = 1f;

    }

    IEnumerator Fade()
        {
            while (projector.fadeFactor > 0)
            {
                yield return new WaitForSeconds(0.1f);
                projector.fadeFactor -= Time.deltaTime * 10f;
            }

        projector.enabled = false;
    }
}
