using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPanel : MonoBehaviour
{
    [SerializeField]
    TMP_Text timeDisplay;

    [SerializeField]
    Renderer circularDisplay;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance)
        {
            timeDisplay.text = GameManager.instance.GetTimeElapsed();
        
       

          foreach (Material m in circularDisplay.materials)
           {
                if (m.HasFloat("_Energy")) m.SetFloat("_Energy", GameManager.instance.GetFractionOfSecond());
                if (m.HasFloat("_Health")) m.SetFloat("_Health", GameManager.instance.GetTimeRemaining());
            }
        }
        else
        {
            timeDisplay.text = "";
        }
    }
}
