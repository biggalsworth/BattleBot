using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reticle : MonoBehaviour
{

    //visualisation of the targetting reticle - VFX only

    [SerializeField]
    GameObject reticleA, reticleB;
    float reticleSpeed = 10f;
    public GameObject target;
    public Color color;
    
    void Start()
    {
      
        target = transform.parent.gameObject;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasColor("_EmissiveColor"))
                    {
                        m.SetColor("_EmissiveColor", color*10f);
                    }
                }
            }

            
            float rng = Random.Range(0f, 0.5f);
            transform.localScale += new Vector3(rng, rng, rng);
            //else spawn a few reticle cubes at a random spot to create that 'lock on' feeling
            reticleA.transform.position = target.transform.position + Random.insideUnitSphere*1f;
            reticleA.transform.rotation = Random.rotation;
           
            
            reticleB.transform.position = target.transform.position + Random.insideUnitSphere*1f;
            reticleB.transform.rotation = Random.rotation;
           
            StartCoroutine(Despawn());
       
       
    }

    // Update is called once per frame
    void Update()
    {
        //lerp the reticles towards the target and align
        if (target)
        {
            reticleA.transform.position = Vector3.Lerp(reticleA.transform.position, target.transform.position, Time.deltaTime * reticleSpeed);
            reticleB.transform.position = Vector3.Lerp(reticleB.transform.position, target.transform.position, Time.deltaTime * reticleSpeed);
            reticleA.transform.rotation = Quaternion.Slerp(reticleA.transform.rotation, target.transform.rotation, Time.deltaTime * reticleSpeed);
            reticleB.transform.rotation = Quaternion.Slerp(reticleB.transform.rotation, target.transform.rotation, Time.deltaTime * reticleSpeed);
        }
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(4f);
        Destroy(gameObject);
    }
}
