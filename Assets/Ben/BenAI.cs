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

public class BenAI : BotAI
{
    static BenJNodeGraph navGraph;
    BenJAStar AStar;

    static public List<BenJVertex> path;
    public List<BattleBotStats> otherBots;
    BenJVertex currTarg;

    //BattleBotStats target;
    TargetClass target = new TargetClass();

    IEnumerator aimingRoutine;
    static IEnumerator directionRoutine;

    bool aimed = false;
    float scanRange = -1f;

    GameObject indicator;

    BattleBotStats testData;

    //fuzzy logic
    float confidence = 0.0f;
    public AnimationCurve healthCurve;
    public AnimationCurve energy;

    /// <summary>
    /// The state machine of the bot.
    /// <para>Locating is the starting state, finding an enemy</para>
    /// <para>Finding is the checking state, check if any enemy is closer</para>
    /// <para>Tracking is called when my bot is within 30m of the enemy, the bot should begin closely scanning the enemies position</para>
    /// <para>Engaging is the state that begins when close to an enemy, fighting will begin</para>
    /// </summary>
    public enum states
    {
        Locating,
        Finding,
        Tracking,
        Retreat,
        Engaging
    }

    states state;

    public override void OnStart()
    {
        /* OnStart() runs when your bot spawns (including after each death)
         * 
         * Use it to launch appropriate coroutines to handle the AI
         */

        //make sure to assign my weapons
        //will usually be set already but needed to do this to reset it a couple of times
        myBot.SwitchWeapon(BattleBot.Mount.RIGHT, (Weapon.WeaponType)0);
        myBot.SwitchWeapon(BattleBot.Mount.LEFT, (Weapon.WeaponType)0);


        AStar = new BenJAStar();

        state = states.Locating;

        target.parentBot = myBot;

        StartCoroutine(CheckTarget());
        StartCoroutine(FindAStarPath());
        StartCoroutine(Throttle());
        StartCoroutine(DecisionTime());
    }

    //This function gathers all the navigation data we need for pathing
    //It will use data to create a base pathing to build from.
    public override void GenerateNavigationData()
    {
        navGraph = new BenJNodeGraph();
        BenNavAI grid = GameObject.FindObjectOfType<BenNavAI>();
        navGraph.GenerateGraph(grid.data, grid.gridScale);
        navGraph.allVertices = navGraph.GetAllConnectedVertices(myBot.transform.position);
        //GameObject.FindObjectOfType<GraphVisualiser>().VisualiseGraph(navGraph);

    }

    //Old greedy path algorithm.
    IEnumerator FindPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if(path != navGraph.FindGreedyPath(myBot.GetStats().position, target.position))
            {
                path = navGraph.FindGreedyPath(myBot.GetStats().position, target.position);
                currTarg = path[0];
            }
            GameObject.FindObjectOfType<GraphVisualiser>().VisualisePath(path);
        }
    }

    //This function is responsible for finding the path
    IEnumerator FindAStarPath()
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 cachedPos;

        while (true)
        {
            //a lot of conditional statements are utilised in this function because we cannot have a path written every single update, it would be costly to performance.
            //if the bot is locating its target, or shooting, it does not need a path.
            if (state != states.Locating)
            {

                //targPos = target.position;
                //create the path
                path = AStar.A_Star(myBot.GetStats().position, target.position, navGraph);
                
                //set this as the last position used.
                cachedPos = target.position;
                GameObject.FindObjectOfType<GraphVisualiser>().VisualisePath(path);

                // begin the coroutine that handles moving along the path
                directionRoutine = FindDirection(path);
                StartCoroutine(directionRoutine);

                //wait until the target has moved away from their last scanned position.
                //this stops the script from drawing a path every frame, saving on performance.
                yield return new WaitUntil(() => Distance2D(target.position, cachedPos) >= 2.5f);

                StopCoroutine(directionRoutine);
                //StopAllNavigationCoroutines();
                //path.Clear()

                yield return new WaitForFixedUpdate();

            }
            else
            {

                yield return new WaitForFixedUpdate();

            }

        }
    }

    //this co-routine handles finding the next node that the bot needs to move towards
    IEnumerator FindDirection(List<BenJVertex> currPath)
    {
        yield return new WaitUntil(() => currPath != null);
        aimed = false;
        currTarg = null;

        //go along the list nodes
        for (int i = 0; i <= currPath.Count; i++)
        {
            if (state != states.Engaging && state != states.Locating)
            {
                currTarg = currPath[i];

                //function that will begin an aiming routine.
                AimAtPoint(currTarg.position);
                //go to the next node once we have reached the checkpoint.
                yield return new WaitUntil(() => Distance2D(myBot.GetStats().position, currTarg.position) <= 5.0f);
            }
            yield return new WaitForFixedUpdate();

        }
        //clear the old path data now that we have cleared it
        currPath.Clear();
        yield break;
    }



    //this will set up and begin the process to aim
    public void AimAtPoint(Vector3 point)
    {
        //stops all processes that are causing movement 
        StopAllNavigationCoroutines();

        //set the new instace of the aim routine
        IEnumerator aimingRoutine = AimRoutine(point);
        
        StartCoroutine(aimingRoutine);
    }
    public void StopAllNavigationCoroutines()
    {
        if(aimingRoutine != null) StopCoroutine(aimingRoutine);
        myBot.SetSteerState(0f);
        myBot.SetThrottleState(0f);
        aimed = false;

    }

    //this will handle turning until the bot has met a certain heading.
    IEnumerator AimRoutine(Vector3 _currTarg)
    {
        //define the vector to aim along, discarding the y axis

        Vector3 targetVector = (_currTarg - myBot.GetStats().position).normalized;

        while (Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized) < 0.9995f)
        {
            targetVector = (_currTarg - myBot.GetStats().position).normalized;

            //turn
            if (Vector3.Cross(targetVector, myBot.GetStats().forwardVector).y < 0.0f)
            {
                //turn more or less depending on how far off our rotation is
                myBot.SetSteerState(1f - Mathf.Clamp(Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized), 0f, 1f));
            }
            else
            {
                myBot.SetSteerState(-1f + Mathf.Clamp(Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized), -1f, 0f));
            }

            //are we colliding with a wall?
            Collider[] hitColliders = Physics.OverlapSphere(myBot.GetStats().position + myBot.GetStats().forwardVector * 3.5f, 1.0f, 1 << 10);

            if (hitColliders.Length > 0)
            {
                //reverse if colliding with wall
                myBot.SetThrottleState(-1.0f);
                yield return new WaitForSeconds(2.0f);
            }
            //add acceleration on this new rotation
            if (myBot.GetVelocity().magnitude < 5.0f)
            {
                //if the rotation is too great, attempt to reverse and meet the aim quicker
                if (Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized) < 0.5f || hitColliders.Length > 0)
                {
                    myBot.SetSteerState(myBot.GetSteerState() * -1.0f);
                    
                    myBot.SetThrottleState(-1.0f);
                    
                    yield return new WaitUntil(() => Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized) > 0.6f);
                    
                }
                else
                {
                    myBot.SetThrottleState(1.0f);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        //aiming complete so reset the steering
        myBot.SetSteerState(0f);
        if (myBot.GetVelocity().magnitude > 0.5f)
        {
            myBot.SetThrottleState(-1.0f);
        }
        yield return new WaitUntil(() => myBot.GetVelocity().magnitude < 1f);
        myBot.SetThrottleState(0f);

        aimed = true;


    }

    //controlling the throttle of a bot.
    IEnumerator Throttle()
    {
        Collider[] hitColliders;

        while (true)
        {
            //if we are aimed and in a state that allows motion, drive forward.
            yield return new WaitUntil(() => aimed == true);
            if (aimed && state != states.Engaging && state != states.Locating)
            {
                myBot.SetThrottleState(1.0f);
                
            }
            else if (!aimed)
            {
                myBot.SetThrottleState(0f);
            }

            //create a physics sphere
            hitColliders = Physics.OverlapSphere(myBot.GetStats().position + myBot.GetStats().forwardVector * 3.5f, 1.0f, 1 << 10);
            //do we overlap with any object on layer 10 (non navigable layer)
            //if we do, drive backwards.
            if (hitColliders.Length > 0)
            {
                myBot.SetThrottleState(-1.0f);
                yield return new WaitForSeconds(2.0f);
            }

            //braking
            if (state == states.Engaging || state == states.Locating)
            {
                if(myBot.GetVelocity().magnitude > 0.5f)
                {
                    myBot.SetThrottleState(-1.0f);
                }
                yield return new WaitUntil(() => myBot.GetVelocity().magnitude < 2.0f);
              
               myBot.SetThrottleState(0f);
            }

            yield return new WaitForFixedUpdate();
        }

    }

    /// <summary>
    /// Get the distance between two positions
    /// </summary>
    /// <returns></returns>
    public static float Distance2D(Vector3 a, Vector3 b)
    {
        float x = a.x - b.x;
        float z = a.z - b.z;
        return Mathf.Sqrt(x * x + z * z);
    }

    //works as a finite state machine
    IEnumerator DecisionTime()
    {
        yield return new WaitForSeconds(1f);

        Vector3 oldPosition = myBot.GetStats().position;
        float stuckTime = 0;

        while (true)
        {
            yield return new WaitForFixedUpdate();

            //compare health and energy to get our confidence value
            confidence = FuzzyCompare(energy, healthCurve);

            //if we aren't attacking, stop shooting
            if (state != states.Engaging)
            {
                myBot.AutoFireWeapon(false, BattleBot.Mount.LEFT);
                myBot.AutoFireWeapon(false, BattleBot.Mount.RIGHT);
            }

            // if our confidence is too low, begin retreating decisions
            if (confidence < 0.6)
            {
                if (state != states.Retreat)
                {
                    state = states.Retreat;

                    //find the safest space 
                    FindSafeSpace();
                    if (healthCurve.Evaluate(myBot.GetHealth()) <= 0.4)
                    {
                        //activate shield
                        myBot.DeployCountermeasures();
                    }

                }
            }

            if (state == states.Locating)
            {
                myBot.SetThrottleState(0.0f);
                //yield return new WaitUntil(() => state != states.Locating);
            }


            //should i retreat
            if (state == states.Retreat)
            {
                if (confidence < 0.8)
                {
                    //can we buy health
                    //100 is the cost of health
                    if (myBot.GetCredits() > 100 && healthCurve.Evaluate(myBot.GetHealth()) <= 0.4)
                    {
                        myBot.RequestSupplyDrop(DropPodStats.DroppodContentType.HEALTH, FindSafeSpace());
                    }
                    //decided to use a shield instead.
                    /*
                    else if(myBot.GetCredits() < 20 && healthCurve.Evaluate(myBot.GetHealth()) <= 0.4)
                    {
                        //if this bot does not have nearly enough credits, explode instead of waiting.
                        myBot.InitiateSelfDestruct();
                    }
                    */ 
                    
                    state = states.Retreat;
                }
                else
                {
                    //validTarget = false;

                    state = states.Locating;
                }
            }
            //we should only do these type of decisions if we are not being overriden by a priority state.
            else if (state != states.Locating && state != states.Retreat)
            {
                if (target.health == 0)
                {
                    state = states.Locating;

                    continue;
                }

                //Decide action based on how close the bot is
                #region Distance-Based Decisions

                if (Distance2D(myBot.GetStats().position, target.position) <= 35.0f)
                {
                    if (state != states.Engaging)
                    {
                        myBot.SetThrottleState(0f);

                        //StopCoroutine(directionRoutine);
                        //StopAllNavigationCoroutines();

                        state = states.Engaging;

                        AimAtPoint(target.position);

                        yield return new WaitUntil(() => aimed);
                        StopCoroutine(directionRoutine);
                        //StopAllNavigationCoroutines();
                    }
                }
                else if (Distance2D(target.position, myBot.GetStats().position) < 85.0f)
                {
                    state = states.Tracking;
                }
                else if(Distance2D(target.position, myBot.GetStats().position) >= 85.0f)
                {
                    state = states.Finding;
                }
                #endregion

                //Begin shooting
                if (state == states.Engaging)
                {
                    Vector3 targetVector = (target.position - myBot.GetStats().position).normalized;

                    //if(Vector2.Dot(new Vector2(targetVector.x, targetVector.z), new Vector2(myBot.GetStats().forwardVector.x, myBot.GetStats().forwardVector.z).normalized) < 0.99f)
                    //{
                    //    AimAtPoint(target.position);
                    //}

                    myBot.SetThrottleState(0f);

                    myBot.AutoFireWeapon(true, BattleBot.Mount.LEFT);
                    myBot.AutoFireWeapon(true, BattleBot.Mount.RIGHT);
                }
                //if not engaging, enable the ability to spawn drop pods, so we can still go on the defensive.
                else
                {
                    //Do we have more than enough credits to afford a turret?
                    if (myBot.GetCredits() > 500 + 100)
                    {
                        //summon a turret at an enemy
                        myBot.RequestSupplyDrop(DropPodStats.DroppodContentType.TURRET, otherBots[Random.Range(1, otherBots.Count)].position);
                    }
                }

            }

            //if stuck long enough: self destruct.
            if (Distance2D(myBot.GetStats().position, oldPosition) < 2.0f && stuckTime > 15.0f)
            {
                myBot.InitiateSelfDestruct();
            }
            //if stuck but not for long enough: update how much time has changed.
            else if(Distance2D(myBot.GetStats().position, oldPosition) < 2.0f)
            {
                stuckTime = stuckTime + Time.deltaTime;
            }
            //if moved from the stuck position: Reset timer and update the position.
            else
            {
                oldPosition = myBot.GetStats().position;
                stuckTime = 0.0f;
            }
        }
    }

    //handles checking where a target is
    IEnumerator CheckTarget()
    {
        float angle = -90.0f;
        while (true)
        {
            if (state != states.Retreat)
            {
                //if the looking angle ever over 360: start from -360
                if (angle == 360)
                {
                    angle = -360;
                }
                if (state == states.Locating)
                {

                    //scan at an angle from the forward vector.
                    //this angle is increased every run, creating a 
                    myBot.DoScanPulse(Quaternion.Euler(0.0f, angle, 0.0f) * myBot.GetStats().forwardVector);
                    angle += 1.5f; // increase the looking angle
                        
                }
                else
                {
                    if (myBot.GetStats().energy > 20)
                    {
                        if (state == states.Finding)
                        {
                            //if still checking for any nearby target, just scan around randomly.
                            myBot.DoScanPulse(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                            yield return new WaitForSeconds(0.5f);


                        }
                        else if (state == states.Tracking)
                        {
                            //scan toward target.

                            Vector3 scanDirection = target.position - myBot.GetStats().position;
                            //Vector3 scanDirection = myBot.GetStats().forwardVector;

                            // Perform the scan pulse in the calculated direction
                            myBot.DoScanPulse(scanDirection.normalized);
                            yield return new WaitForSeconds(1.0f);

                        }
                        else if (state == states.Engaging)
                        {
                            myBot.DoScanPulse((target.position - myBot.GetStats().position).normalized);
                            yield return new WaitForSeconds(0.5f);
                        }
                        else
                        {
                            yield return new WaitForSeconds(1.50f);
                        }
                    }

                }
            }
            yield return new WaitForFixedUpdate();
        }

    }


    float FuzzyCompare(AnimationCurve data1, AnimationCurve data2 = null)
    {
        float result = 0.0f;
        //what value is the least, therefore the most severely declined
        result = Mathf.Min(data1.Evaluate(myBot.GetEnergy() / 100.0f), data2.Evaluate(myBot.GetHealth() / 100.0f));

        return result;
    }

    Vector3 FindSafeSpace()
    {
        //using each bot that we have scanned, get the midpoint between them all
        Bounds bound = new Bounds(myBot.GetStats().position, Vector3.zero);
        foreach (BattleBotStats bot in otherBots)
        {
            bound.Encapsulate(bot.position);
        }

        //the centre of these bounds is the furthest position between all these bots
        target.position = bound.center;
        return bound.center;   
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

    //OnTakeDamage() is called whenever another bot hits you. Direction is the vector of the incoming attack.
    public override void OnTakeDamage(Vector3 direction)
    {
        Debug.Log(myBot.GetStats().name + " took damage along " + direction);
    }

    //OnRecieveScanData() is called every time you hit another bot with a scan. Access BattleBotStats fields to get information on the target.
    public override void OnRecieveScanData(BattleBotStats data)
    {
        if(state == states.Retreat)
        {
            return;
        }

        bool found = false;
        foreach(BattleBotStats bot in otherBots)
        {
            if (bot.uniqueID == data.uniqueID)
            {
                otherBots.Remove(bot);
                otherBots.Add(data);

                //bot.position = data.position;

                found = true;
                break;
            }
        }
        if (!found)
        {
            otherBots.Add(data);
        }

        if (state == states.Locating && data.health > 0.0f)
        {
            state = states.Finding;
            target.UpdateDate(data);

        }


        if (Vector3.Distance(myBot.GetStats().position, data.position) < Vector3.Distance(myBot.GetStats().position, target.position))
        {
            target.UpdateDate(data);

        }

        if(state == states.Tracking && target.ID == data.uniqueID)
        {
            target.UpdateDate(data);
        }
        if (target.ID == data.uniqueID)
        {
            target.UpdateDate(data);
        }


        //Debug.Log(myBot.GetStats().name + " got a scanner hit on " + data.name + " at " + data.position);
    }

    public override void OnDeath()
    {
        //weapon switch requests are applied the next time you spawn
        //myBot.SwitchWeapon(BattleBot.Mount.LEFT, (Weapon.WeaponType)Random.Range(0, 5));
        myBot.SwitchWeapon(BattleBot.Mount.RIGHT, (Weapon.WeaponType)0);
        myBot.SwitchWeapon(BattleBot.Mount.LEFT, (Weapon.WeaponType)0);
        //myBot.SwitchWeapon(BattleBot.Mount.RIGHT, (Weapon.WeaponType)Random.Range(0, 5));
        myBot.SwitchCountermeasure((CounterMeasure.CounterMeasureType)Random.Range(0, 3));
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow; 
        Gizmos.DrawSphere(myBot.GetStats().position + myBot.GetStats().forwardVector * 3.5f, 1.0f);

    }
}
