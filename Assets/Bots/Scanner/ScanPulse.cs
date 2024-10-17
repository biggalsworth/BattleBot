using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ScanPulse : MonoBehaviour
{
    //detect scan hits based on trigger volume
    //note the scan runs as an animation, not code, attached to the same object as this script, so how it scales/moves is not visible here

    //targeting reticle prefab for visualisation
    [SerializeField]
    GameObject targetingReticle;

    [SerializeField]
    AnimationCurve fadeEffect;

    float fadeTimer;

    //bot initiating the scan
    public BattleBot source;
    public Color color;
    Rigidbody rb;

    private void Awake()
    {
        
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        fadeTimer += Time.deltaTime;
        foreach (DecalProjector r in GetComponentsInChildren<DecalProjector>())
        {

            if (r.material.HasFloat("_Fade"))
            {
                r.material.SetFloat("_Fade", 1f-fadeEffect.Evaluate(fadeTimer));
            }
        }

    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(rb) rb.velocity = transform.forward * 50f;
        foreach (DecalProjector r in GetComponentsInChildren<DecalProjector>())
        {
            r.material = new Material(r.material);
                if (r.material.HasColor("_Color"))
                {
                    r.material.SetColor("_Color", color);
                }
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        
      
       BattleBot botHit = other.GetComponentInParent<BattleBot>();
       DropPod droppodHit = other.transform.parent.GetComponentInChildren<DropPod>();

        if (droppodHit && source)
        {
            //scanner hit a drop pod
            GameObject reticle = Instantiate(targetingReticle);
            reticle.transform.parent = other.transform;
            reticle.GetComponent<Reticle>().color = color;
            source.ReceiveScanData(droppodHit.GetStats());
        }

       if (botHit&&source)
            {
           
             if (source.GetInstanceID() != botHit.GetInstanceID())
              {
                //scanner hit a bot
       
                if (source)
                {
                    if (!botHit.isFlared)
                    {
                        GameObject reticle = Instantiate(targetingReticle);
                        reticle.transform.parent = other.transform;
                        reticle.GetComponent<Reticle>().color = color;
                        source.ReceiveScanData(botHit.GetStats());
                    }
                }
               
              }
              
            }
    }

 
}
