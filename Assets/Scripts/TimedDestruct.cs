using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestruct : MonoBehaviour
{
    [SerializeField]
    float timeToDestruct;
    void Start()
    {
        Destroy(gameObject, timeToDestruct);
    }

}
