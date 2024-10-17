using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour
{

    //simple rotator simply rotates! (fans, etc.)

    [SerializeField]
    Vector3 axis; //rotation axis

    [SerializeField]
    public float speed; //rpm
   
    void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}
