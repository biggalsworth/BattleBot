using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    //generic destroyer of a particle system once the effect ends
    void Start()
    {
        ParticleSystem ps = this.GetComponent<ParticleSystem>();
        Destroy(this.gameObject, ps.main.duration);
    }

   
}
