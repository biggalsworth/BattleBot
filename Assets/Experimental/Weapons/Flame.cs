using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour
{

    [SerializeField]
    public GameObject burning;
    
    public BotCompetitor botThatFiredThisFlame;
    Rigidbody rb;
    List<Shootable> botsAlreadyHit;
    private void Start()
    {
        botsAlreadyHit = new List<Shootable>();
        rb = GetComponent<Rigidbody>();
        Destroy(this, 1f);
        rb.velocity = transform.forward * 20f;
        foreach(Collider c in botThatFiredThisFlame.instanceLink.transform.root.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(c, GetComponent<Collider>());
        }
    
    }

    private void OnTriggerEnter(Collider other)
    {

        GameObject burnInstance = Instantiate(burning, other.ClosestPointOnBounds(transform.position), Quaternion.identity, other.transform);
        burnInstance.GetComponent<Burn>().owner = botThatFiredThisFlame;
        Destroy(gameObject);
    }

   
}
