using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BotTrail : MonoBehaviour
{
    Rigidbody rb;
    VisualEffect trail;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        trail = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        trail.SetFloat("Rate", rb.velocity.magnitude);
    }
}
