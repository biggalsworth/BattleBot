using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DropPod : MonoBehaviour
{
    [SerializeField]
    public Powerup contents;

    [SerializeField]
    public uint cost;

    [SerializeField]
    public DropPodStats stats;

    Rigidbody rb;
    Animator anim;
    bool isDeployed = false;
    int ownerUid;
    public BattleBot owner;
    public BotCompetitor competitor;

    [SerializeField]
    GameObject impactExplosion;
    [SerializeField]
    GameObject marker;
    [SerializeField]
    GameObject missExplosion;
    Powerup contentInstance;
    [SerializeField]
    GameObject smokeTrail;
    [SerializeField]
    GameObject crater;

  
    VisualEffect vfx;
    Vector3 craterPosition;
    public void SetOwner(BattleBot newowner)
    {
        stats.ownerName = newowner.GetStats().name;
        owner = newowner;
        ownerUid = newowner.competitorLink.uid;
    }
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        vfx = GetComponentInChildren<VisualEffect>();
        anim = GetComponent<Animator>();
        rb.velocity = new Vector3(0f, -20f, 0f);
        if(contents) stats.contents = contents.GetContentType();
        stats.position = transform.position;
        stats.hasLanded = false;
        StartCoroutine(RefreshOwner());
    }

    IEnumerator RefreshOwner()
    {
        
        yield return new WaitUntil(() => BattleBot.allBots != null);
        //if there's no owner, assume the manual bot for debugging
        if (owner == null)
        {
            foreach (BattleBot bot in BattleBot.allBots)
            {
                if (bot.GetControlMode() == BattleBot.ControlMode.Manual)
                {
                    owner = bot;
                    competitor = bot.competitorLink;
                    ownerUid = bot.competitorLink.uid;
                }
            }
        }


        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!owner)
            {
      
                foreach (BattleBot b in BattleBot.allBots)
                {
                    if (b.competitorLink.uid == ownerUid) SetOwner(b);
                }
            }
        }
    }
    public DropPodStats GetStats()
    {
        return stats;
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision other)
    {
        vfx.Stop();
        smokeTrail.transform.parent = null;
        if (!isDeployed)
            if(other.collider.gameObject.layer==0)
            {
                StartCoroutine(Deploy());
                
            }
            else
            {
                if (other.collider.GetComponentInParent<Shootable>() != null)
                {
                    other.collider.GetComponentInParent<Shootable>().TakeHit(100, owner.competitorLink);
                }
                else
                {
                    Instantiate(missExplosion, transform.position, transform.rotation);
                    Destroy(transform.parent.gameObject);
                }
            }
        
    }

    public void Despawn()
    {
        StartCoroutine(AnimatedDespawn());
    }
    IEnumerator AnimatedDespawn()
    {
        yield return new WaitUntil(() => contentInstance == null);
        anim.SetTrigger("Despawn");
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    IEnumerator Deploy()
    {
        GameObject impactCrater = Instantiate(crater, transform.position, Quaternion.identity, null);
        impactCrater.transform.localScale = new Vector3(8f, 8f, 8f);
        impactExplosion.SetActive(true);
        yield return new WaitForFixedUpdate();
        Destroy(marker);
        yield return new WaitForSeconds(2f);
        isDeployed = true;
        stats.hasLanded = true;
        rb.isKinematic = true;   
        GetComponentInChildren<Collider>().enabled = false;
        if (contents)
        {
            contentInstance = Instantiate(contents, transform.position, transform.rotation);
            contentInstance.linkedPod = this;
        }

        StartCoroutine(AnimatedDespawn());
        anim.SetTrigger("Deploy");
    }

}
