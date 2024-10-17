using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{

    /* This is my own take on a generic object pool system for Unity
     * I don't use the in-built methods (though they're definitely perhaps better in many cases!).
     * I find it useful to 'tell' something in a pool it will be reclaimed by the pool soon, so it can take action (e.g. fade) to avoid any jarring visualsl.
     * e.g. for the tyre tracks - it's neat to not have them immediately fade so they linger if a bot doesn't move. But it's also necessary to prompt them to fade if the pool is reaching capacity.
     * I also find it easier to make the pool with any subclass of 'Poolable' (see Poolable.cs), and have it be fairly ambivalent to what the actual pool content is.
     * A generic method lets another coder request anything of <type>, if it's in the pool.
     * There are 2 public variables, the list of objects to make pools for, and the list of pool sizes. This could definitely be tidied into a single struct for a clearer inspector interaction
     * The goal really was an endpoint where another dev can just call ObjectPoolManager.instance.SpawnFromPool<TypeIWant>(WhereIWantIt), as this was the cleanest way I could think of 
     * to provide another user with object pool functionality that didn't require them to edit the manager to add new item types.
     */


    /*best thing about this? it's pretty much entirely cosmetic! I made it primarily for the tyre tracks. For those to be smooth, we need a lot of decal projectors.
     * HDRP is very good at handling many decals with one material in a draw call
     * But spawning those decals every time bots move a bit to make a track, is still expensive Instantiate calls
     * The object pool avoids that by moving the oldest track to the base of the tyre, when a new one is needed.
     * It's also used for projectiles etc. Whilst this isn't a multiplayer game per-se, in a multiplayer game...
     * ...it's really important to minimise framerate hits when the bullets (literally) start flying. Object pools help a lot with that.
     */



    public GameObject[] pools; //objects to pool (prefabs)
    public int[] sizes; //size of each pool
    public static ObjectPoolManager instance;
    

    private void Awake()
    {

        //singleton pattern to allow for quick requests to spawn from pool in codebase
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
  
        //populate all pools immediately to avoid instantiate calls at runtime
        int poolCounter = 0;
        foreach (GameObject go in pools)
        {
            GameObject poolParent = Instantiate(new GameObject(), transform.position, transform.rotation, transform);
            poolParent.name = go.GetComponent<Poolable>().GetType().ToString();
            for(int n=0; n<sizes[poolCounter]; n++)
            {
                GameObject pooledObject = Instantiate(go, transform.position, transform.rotation, poolParent.transform);
                pooledObject.GetComponent<Poolable>().OnInitialise();
            }
            poolCounter++;
        }
    }


    //handle requests to spawn from pool anything of a specified Type T
    //this could perhaps be amended/improved to actually create a pool of Type T, if it doesn't exist, but at the moment it returns null if it doesn't exist.
    //this is also an example of a 'generic method' in C#, which you often use (e.g. GetComponent<Type>, but rarely write!)

    //note I use the transform's order of children as a data structure (first index is 'back of the queue' when it comes to requesting a new object from the pool).
    //This avoids the overhead of making an explicit one (e.g. a List or Queue) but also has a few drawbacks
    //I've not performance tested using a List or Queue instead of simply the child order of the transform; this would be interesting to do!
    public GameObject SpawnFromPool<T>(Vector3 position, Quaternion rotation)
    {
       
       foreach (Transform t in transform)
        {
            if (t.GetChild(0).GetComponent<T>()!=null)
            {
                Transform poolObject = t.GetChild(t.childCount-1);
                t.GetChild(t.childCount - 256).GetComponent<Poolable>().OnReclaimWarning(); //warn the object 200 objects above it should prep and return itself. If it's not returned by the time it's needed, it will effectively be forced.
                poolObject.GetComponent<Poolable>().OnExitPool(); //tell the object to do it's 'i've just spawned' action
                poolObject.GetComponent<Poolable>().transform.position = position;
                poolObject.GetComponent<Poolable>().transform.rotation = rotation;
                poolObject.SetAsFirstSibling(); //shuffle it up to the start of the children, putting it at the back of the queue
                return poolObject.gameObject;
            }
        }
        return null;
    }

    //handle the return to pool of an object

    public void ReturnToPool(GameObject go)
    {
        go.GetComponent<Poolable>().OnEnterPool();
        go.transform.SetAsFirstSibling();
    }

    
}
