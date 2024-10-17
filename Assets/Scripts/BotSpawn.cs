using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawn : MonoBehaviour
{
   
    GameObject botToSpawn;

    
    [SerializeField]
    Transform spawnPoint;

    [SerializeField]
    GameObject addToAllSpawns;

    [SerializeField]
    GameObject flameToAdd;

    Animator anim;
    public bool padClear = true;

    // Start is called before the first frame update

    public void Spawn(BotCompetitor competitor, bool generateNavData,  float delay)
    {
        botToSpawn = competitor.prefabLink;
        StartCoroutine(DoSpawn(competitor, generateNavData, delay));
    }
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
   

    private void OnTriggerStay(Collider other)
    {
        padClear = false;
    }

    private void OnTriggerExit(Collider other)
    {
        padClear = true;
    }
    IEnumerator DoSpawn(BotCompetitor competitor, bool GenerateNavData,  float delay)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(delay);
        anim.SetTrigger("Restart");
       
        anim.ResetTrigger("Close");
     
        GameObject bot = Instantiate(competitor.prefabLink, spawnPoint.position, spawnPoint.rotation);
        bot.GetComponent<BattleBot>().deathFlame = flameToAdd;
        if (addToAllSpawns)
        {
            GameObject addition = Instantiate(addToAllSpawns, bot.transform.position, bot.transform.rotation, bot.GetComponentInChildren<Rigidbody>().transform);
        }
        bot.GetComponent<BattleBot>().competitorLink = competitor;
        bot.GetComponent<BattleBot>().score = competitor.score;
        bot.GetComponent<BattleBot>().GiveCredits(competitor.credits);
        if (GenerateNavData)
        {
            bot.GetComponent<BattleBot>().OnInitialSpawn();
        }
        bot.transform.parent = spawnPoint;
        yield return new WaitForSeconds(2f);
        if (bot.GetComponent<BattleBot>().GetAI())
        {
            bot.GetComponent<BattleBot>().GetAI().OnStart();
        }

        yield return new WaitUntil(() => padClear);
        anim.SetTrigger("Close");
        anim.ResetTrigger("Restart");
        if (bot) bot.transform.parent = null;
        yield return new WaitForSeconds(2f);
 
        //anim.speed = 0;
      //  yield return new WaitUntil(() => padClear);
      //  anim.speed = 1;
     //   yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("botspawn"));
      //  StartCoroutine(DoSpawn());
    }


}
