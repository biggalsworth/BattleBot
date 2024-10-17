using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{

    public GameObject Parent;
    public GameObject collidedObj;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(LayerMask.LayerToName(other.transform.gameObject.layer));
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("NotNavigable"))
        {
            collidedObj = other.transform.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("NotNavigable"))
        {
            collidedObj = null;
        }
    }
}
