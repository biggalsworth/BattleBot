using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Poolable : MonoBehaviour
{

    //abstract class for anything that can go into an object pool.
    //abstract means you can't instance it directly. you can only make subclasses of it, and they're required to implement these methods:
    public abstract void OnInitialise(); //when it's spawned into the pool
    public abstract void OnExitPool(); //when it's taken from the pool and used
    public abstract void OnEnterPool(); //when it's reclaimed into the pool
    public abstract void OnReclaimWarning(); //a 'heads up' it will be reclaimed soon, so should start fading etc.
}
