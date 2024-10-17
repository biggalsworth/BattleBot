using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDecal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Random.Range(0f, 360f));
        float scale = Random.Range(0.8f, 1.2f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

 
}
