using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//singleton that manages the game mode
public class GameManager : MonoBehaviour
{
    public enum GameMode {Freeplay, Deathmatch};

    [SerializeField]
    GameMode gameMode;

    [SerializeField]
    List<BattleBot> BotsInRound;
    [SerializeField, ColorUsage(true, true)]
    List<Color> BotColors;
    [SerializeField]
    List<Weapon> availableWeapons;
    [SerializeField]
    List<CounterMeasure> availableCountermeasures;

    [SerializeField]
    float respawnTimer;

    List<BotSpawn> spawns;
    List<BotCompetitor> competitors;

    [SerializeField]
    float roundDurationInMinutes;


   
    private float countdownTime;

    public string GetTimeElapsed()
    {

        if (gameMode == GameMode.Freeplay) return "Test Mode";
        // Format the time elapsed into mm:ss:fff format (minutes:seconds:milliseconds)
        int hundredths = Mathf.FloorToInt((countdownTime - Mathf.Floor(countdownTime)) * 100);

        // Format the remaining time into mm:ss:ff format (minutes:seconds:hundredths)
        string timeRemainingString = string.Format("{0:00}:{1:00}:{2:00}",
                                                   Mathf.Floor(countdownTime / 60),
                                                   Mathf.Floor(countdownTime % 60),
                                                   hundredths);

        // Display the remaining time
        //Debug.Log("Time Remaining: " + timeRemainingString);
        if (countdownTime <= 0)
        {
            return "Round Over";
        }
        else {
            
            return timeRemainingString;
        }
    }

    public float GetTimeRemaining()
    {
        if (gameMode == GameMode.Freeplay) return 1f;
        return Mathf.InverseLerp(0, roundDurationInMinutes*60f, countdownTime);
    }


    public float GetFractionOfSecond()
    {
        if (gameMode == GameMode.Freeplay) return 1f;
        float secondsInCurrentMinute = Mathf.Floor(countdownTime % 60);
        return Mathf.InverseLerp(0, 59, secondsInCurrentMinute);
    }

    public static GameManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {

        if(gameMode==GameMode.Deathmatch) countdownTime -= Time.deltaTime;


        if (countdownTime <= 0)
        {
            SuspendAllBots();
            //Debug.Log("Round over!");
            
        }


    }

    void SuspendAllBots()
    {
        foreach(BattleBot b in BattleBot.allBots)
        {
            b.Suspend();
        }
    }

    void EnableAllBots()
    {
        foreach (BattleBot b in BattleBot.allBots)
        {
            b.Unsuspend();
        }
    }

    void ActivateAllBots()
    {

    }

    void Start()
    {
        competitors = new List<BotCompetitor>();
        for (int n=0; n<BotsInRound.Count; n++)
        {
            BotsInRound[n].SetColor(BotColors[n]);
            competitors.Add(new BotCompetitor(BotsInRound[n].gameObject));
        }

        countdownTime = roundDurationInMinutes * 60f;

        spawns = new List<BotSpawn>();
        
        spawns.AddRange(GameObject.FindObjectsOfType<BotSpawn>());
        if (spawns.Count > 0)
        {
            PerformInitialSpawn();
        }
        else Debug.LogWarning("There are no spawns in the arena");


        if(gameMode == GameMode.Deathmatch)
        {
       //     StartCoroutine(StartDeathmatch());
        }


    }


    IEnumerator StartDeathmatch()
    {
        yield return new WaitForSeconds(0.5f);
        SuspendAllBots();
        yield return new WaitForSeconds(1f);
        foreach (BattleBot b in BattleBot.allBots)
        {
            CameraManager.instance.PerformBotCinematic(b);
            yield return new WaitForSeconds(10.1f);
        }
        EnableAllBots();
        
    }

    public float GetRespawnTimer()
    {
        return respawnTimer;
    }

   
    
   void PerformInitialSpawn()
    {
        int counter = 0;
        foreach(BotCompetitor c in competitors)
        {
            if (spawns.Count > counter)
            {
                spawns[counter].Spawn(c, true, 0f);
                counter++;
            }
            else
            {
                counter = 0;
            }
            
        }
    }

    public void OverrideCountermeasure(BotCompetitor c, CounterMeasure.CounterMeasureType counterMeasureType)
    {
      
                c.prefabLink.GetComponent<BattleBot>().GetStats().counterMeasure = GetCountermeasure(counterMeasureType);
       
    }


    CounterMeasure GetCountermeasure (CounterMeasure.CounterMeasureType counterMeasureType)
    {

        foreach (CounterMeasure c in availableCountermeasures)
        {
            if (c.counterMeasureType == counterMeasureType)
            {
                return c;
            }
        }
        Debug.LogWarning("Could not find requested countermeasure " + counterMeasureType);
        return null;
    }

    public void OverrideLoadout(BotCompetitor c, BattleBot.Mount mount, Weapon.WeaponType weapon)
    {

        if (mount == BattleBot.Mount.LEFT)
        {
            c.prefabLink.GetComponent<BattleBot>().GetStats().leftWeapon = GetWeapon(weapon);
        }
        else
        {
            c.prefabLink.GetComponent<BattleBot>().GetStats().rightWeapon = GetWeapon(weapon);
        }
    }

    Weapon GetWeapon(Weapon.WeaponType weapon)
    {
        foreach (Weapon w in availableWeapons)
        {
            if (w.weaponType == weapon)
            {
                return w;
            }
        }
        Debug.LogWarning("Could not find weapon type " + weapon);
        return null;
    }

    public void RequestRespawn(BotCompetitor c)
    {
      
                spawns[UnityEngine.Random.Range(0, spawns.Count-1)].Spawn(c, false, respawnTimer);
            
      
    }

  
}
