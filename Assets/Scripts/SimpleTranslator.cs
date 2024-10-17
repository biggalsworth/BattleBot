using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTranslator : MonoBehaviour
{
    //simple translator simple translates...



    [SerializeField]
    float speed;
    

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(transform.forward * Time.deltaTime * speed);
    }
}
