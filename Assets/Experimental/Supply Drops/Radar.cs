using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : Powerup, Shootable
{
    [SerializeField]
    GameObject deathExplosion;

    Scanner scanner;

    float health = 30f;
    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.RADAR;
    }
    // Start is called before the first frame update
    void Start()
    {
        scanner = GetComponentInChildren<Scanner>();
        StartCoroutine(Scan());
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (m.HasColor("_BotColor"))
                {
                    m.SetColor("_BotColor", linkedPod.owner.GetColor());
                }
            }
        }
    }

    IEnumerator Scan()
    {
        while (true)
        {
            
           

            scanner.EmitPulse(linkedPod.owner);
            yield return new WaitForSeconds(2f);


        }
    }

    public void TakeHit(uint damage, BotCompetitor shooter)
    {
        health -= damage;
        if (health < 0f)
        {
            Instantiate(deathExplosion, transform.position, transform.rotation);
            Destroy(gameObject);
            if(linkedPod) linkedPod.Despawn();
          
        }
    }
}
