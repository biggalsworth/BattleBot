using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMarker : MonoBehaviour
{

    public BattleBot linkedBot;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (m.HasColor("_Color"))
                {
                    m.SetColor("_Color", linkedBot.GetStats().defaultColor);
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Minimap.WorldCoordToMinimapCoord(linkedBot.GetStats().position);
        transform.localRotation = Quaternion.LookRotation(linkedBot.GetStats().forwardVector);
        
        
    }
}
