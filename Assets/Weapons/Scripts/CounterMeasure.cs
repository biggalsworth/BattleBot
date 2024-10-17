using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CounterMeasure : MonoBehaviour
{

    //abstract base class for all countermeasures

    public enum CounterMeasureType { FLARES, SHIELD, TURBO, SCANNER}

    public CounterMeasureType counterMeasureType;
    public abstract bool Deploy();

    
   
}
