using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class EventBinder : MonoBehaviour
{

    VisualEffect vfx;
    public bool OnPlay;
    public bool Stop;
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if(OnPlay) vfx.SendEvent("OnPlay");
        if(Stop) vfx.SendEvent("Stop");
    }
}
