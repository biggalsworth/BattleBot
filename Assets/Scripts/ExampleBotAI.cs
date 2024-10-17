using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/* Instructions:
 * 
 * Duplicate this script and rename the class (and filename to match) to create your own AI
 * 
 * Important - to include multiple bots from different people in the arena, the class names must not match.
 * Create an original class name (or namespace) with a unique identifier (e.g. your full name or unique nickname, etc.)
 * Avoid calling it something generic (e.g. myAI) as if two people do this, it will need fixing.
 * 
 * Attach your new script to an empty gameobject and save as a prefab.
 * Then slot the prefab into the BotAI field of a BattleBot prefab (in bots/prefabs).
 * 
 */

public class ExampleBotAI : BotAI
{
    BattleBotStats testData;
    public override void OnStart()
    {
        /* OnStart() runs when your bot spawns (including after each death)
         * 
         * Use it to launch appropriate coroutines to handle the AI
         */


        myBot.SwitchCountermeasure(CounterMeasure.CounterMeasureType.FLARES);

        StartCoroutine(RandomThrottle());
        StartCoroutine(RandomSteer());
        StartCoroutine(RandomShoot());
        StartCoroutine(RandomCountermeasures());
        StartCoroutine(RandomScanner());
        StartCoroutine(RandomSupplyDrops());
    }

    public override void GenerateNavigationData()
    {
       
        /* This method runs only once when the game starts. It does not run on respawn.
         * 
         * Important - this script instance will be destroyed when the bot dies.
         * 
         * This is partly for your benefit, since if your script crashes in a certain edge-case, it'll start over when you die :)
         * 
         * This means you *must* use a *static* class/struct (or other means) to 
         * contain your navigation data to ensure it persists between spawns 
         */
    }

    

    /* The below couroutines demonstrate the various method calls to control your bot
     * 
     * You'll obviously need to improve on this completely random behaviour!
     */

    IEnumerator RandomThrottle()
    {
    
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            myBot.SetThrottleState(Random.Range(-1f, 1f));
        }
    }

    IEnumerator RandomSteer()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            myBot.SetSteerState(Random.Range(-1f, 1f));
           

        }

    }

    IEnumerator RandomShoot()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            bool randomBool;
            if (Random.Range(0, 2) == 0) randomBool = false; else randomBool = true;
            myBot.AutoFireWeapon(randomBool, BattleBot.Mount.LEFT);
            if (Random.Range(0, 2) == 0) randomBool = false; else randomBool = true;
            myBot.AutoFireWeapon(randomBool, BattleBot.Mount.RIGHT);
        }
    }

    IEnumerator RandomCountermeasures()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 20f));
            myBot.DeployCountermeasures();
        }
    }

    IEnumerator RandomSupplyDrops()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 20f));
            myBot.RequestSupplyDrop((DropPodStats.DroppodContentType)Random.Range(0, 6), new Vector2(Random.Range(0f, 200f), Random.Range(0f, 200f)));
        }
    }
    IEnumerator RandomScanner()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            myBot.DoScanPulse(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
    }

    //OnTakeDamage() is called whenever another bot hits you. Direction is the vector of the incoming attack.
    public override void OnTakeDamage(Vector3 direction)
    {
       // Debug.Log(myBot.GetStats().name + " took damage along " + direction);
    }

    //OnRecieveScanData() is called every time you hit another bot with a scan. Access BattleBotStats fields to get information on the target.
    public override void OnRecieveScanData(BattleBotStats data)
    {
        //Debug.Log(myBot.GetStats().name + " got a scanner hit on " + data.name + " at " + data.position);
    }

    public override void OnRecieveScanData(DropPodStats data)
    {
      //  Debug.Log(myBot.GetStats().name + " got a scanner hit on a supply drop containing " + data.contents + " at " + data.position);
    }

    public override void OnDeath()
    {
        //weapon switch requests are applied the next time you spawn
        myBot.SwitchWeapon(BattleBot.Mount.LEFT, (Weapon.WeaponType)Random.Range(0, 6));
        myBot.SwitchWeapon(BattleBot.Mount.RIGHT, (Weapon.WeaponType)Random.Range(0, 6));
        myBot.SwitchCountermeasure((CounterMeasure.CounterMeasureType)Random.Range(0, 3));
    }
}
