using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarShell : MonoBehaviour
{
    public GameObject explosion;
    public BattleBot botThatFiredThisShell;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = rb.velocity.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject impact = Instantiate(explosion, transform.position, Quaternion.LookRotation(Vector3.up), null);
        impact.transform.Rotate(90f, 0f, 0f);
        impact.GetComponent<MortarImpact>().botThatFiredThisMortar = botThatFiredThisShell;
        Destroy(gameObject);
    }
}
