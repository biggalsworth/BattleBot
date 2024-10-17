using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crater : MonoBehaviour
{
    [SerializeField]
    LayerMask deleteOnLayerContact;


    void Start()
    {
        
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f));
        transform.parent = null;
        Collider[] cols = Physics.OverlapSphere(transform.position, 2f, deleteOnLayerContact);
        if (cols.Length > 0) Destroy(gameObject);
        Destroy(gameObject, 30f);
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
