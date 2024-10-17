using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BattleBot : MonoBehaviour, Shootable
{

    public enum ControlMode {Manual, AI, Disabled, Suspended};

    [SerializeField]
    BotAI botAIToLoad; //which AI prefab to load
   
    [SerializeField]
    ControlMode controlMode; //AI, manual, or disabled controller

    [SerializeField]
    BattleBotStats stats; //struct containing bot stats

    [SerializeField]
    GameObject head; //object for the head

    [SerializeField]
    GameObject torso; //object for the torso

    [SerializeField]
    GameObject deathExplosion; //explosion to spawn on death

    public GameObject deathFlame; //flames that burn before death

    [SerializeField]
    GameObject leftWeaponMount; //parent to mount the left weapon to

    [SerializeField]
    GameObject rightWeaponMount; //parent to mount the right weapon to

    public BotCompetitor competitorLink;

   
    //health and energy variables
    float energy = 100f;
    float health = 100f;
    float postDeathHealth = 100f; //how much more damage to take to go from wrecked > explode
    bool isDestroyed = false;
    const int energyRegenPerTick = 10;
    uint podCredits;
    const uint podCreditsMax = 300;
    

    //gameplay variables
    [HideInInspector]
    public bool isShielded = false;
    [HideInInspector]
    public bool isFlared = false;
    [HideInInspector]
    public int score = 0;
    bool suspendEnergyRegen = false;
    bool canScan = true;
   
    //active weapon instances
    Weapon[] activeWeapons; //[0] = left mount, [1] = right mount
    public enum Mount { LEFT = 0, RIGHT = 1 }; //enum so I don't have to remember which is left and right

    

    //active scanner instance
    Scanner scanner;

    //active countermeasure instance
    CounterMeasure activeCountermeasure;

    //active bot AI
    BotAI botAI;

    //movement control values
    float forward;
    float steer;
   
    //unique id for respawns
    
    //cached components
    Animator anim;

    //list of all active bots for convenience
    public static List<BattleBot> allBots;

    private void Awake()
    {
        //update list of all active bots
        if (allBots == null) allBots = new List<BattleBot>();
        allBots.Add(this);
      
        //activate AI if mode set
        if (botAIToLoad != null && controlMode==ControlMode.AI)
        {
            GameObject AI = Instantiate(botAIToLoad.gameObject, transform.position, transform.rotation, transform);
            botAI = AI.GetComponent<BotAI>();
            botAI.myBot = this;
           
        }
    }

    public BotAI GetAI()
    {
        return botAI;
    }

    void Start()
    {
        EventFeed.Broadcast(GetStats().name + " is spawned in and starting up");
        competitorLink.instanceLink = this;
        //cache components
        anim = GetComponent<Animator>();
        scanner = GetComponentInChildren<Scanner>();

        //update shaders to recolour bot
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (m.HasColor("_BotColor"))
                {
                    m.SetColor("_BotColor", stats.defaultColor);
                }
            }
        }

        //spawn in assigned weapons
        if (stats.leftWeapon)
        {
            GameObject leftWeapon = Instantiate(stats.leftWeapon.gameObject, leftWeaponMount.transform.position, leftWeaponMount.transform.rotation);
            leftWeapon.transform.parent = leftWeaponMount.transform;
        }

        if (stats.rightWeapon)
        {
            GameObject rightWeapon = Instantiate(stats.rightWeapon.gameObject, rightWeaponMount.transform.position, rightWeaponMount.transform.rotation);
            rightWeapon.transform.parent = rightWeaponMount.transform;
            //flip right weapon
            Vector3 scale = rightWeapon.transform.localScale;
            scale.x = -1f * scale.x;
            rightWeapon.transform.localScale = scale;
        }

        if (stats.counterMeasure)
        {
            GameObject counter = Instantiate(stats.counterMeasure.gameObject, torso.transform.position, torso.transform.rotation);
            counter.transform.parent = torso.transform;
            activeCountermeasure = counter.GetComponent<CounterMeasure>();
        }

       

        //cache to activeWeapons[]
        activeWeapons = GetComponentsInChildren<Weapon>();


        //add or update UI element for this bot

        GameObject.FindObjectOfType<UITagManager>().AddTag(this); //addtag checks for duplicates

        if (UITag.allTags != null)
        {
            //if tags exist, check there isn't already one for this bot (a respawn), if so, link it instead
            foreach (UITag tag in UITag.allTags)
            {
                if (tag.linkedID == competitorLink.uid)
                {
                    tag.linkedBot = this;
                }
            }
        }

        if (Minimap.instance)
        {
            Minimap.instance.AddMapMarker(this);
        }

        //start regenerating energy
        StartCoroutine(EnergyRegen());
        StartCoroutine(CreditRegen());
      
    }

    void Update()
    {
 
        //keybinds for controls in Manual mode
        if (controlMode == ControlMode.Manual)
        {
            forward = Input.GetAxis("Vertical");
            steer = Input.GetAxis("Horizontal");
            AutoFireWeapon(Input.GetButton("Fire1"), Mount.LEFT);
            AutoFireWeapon(Input.GetButton("Fire2"), Mount.RIGHT);
            head.transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);

            if (Input.GetKeyDown(KeyCode.E))
            {
                DeployCountermeasures();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                DoScanPulse();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                InitiateSelfDestruct();
            }
        }

        //slow to a halt if disabled
        if (controlMode == ControlMode.Disabled || controlMode == ControlMode.Suspended)
        {
            forward = Mathf.Lerp(forward, 0, Time.deltaTime);
            steer = Mathf.Lerp(steer, 0, Time.deltaTime);
        }

        //update the radial display on the back of the bot (0=empty 1=full)
        anim.SetFloat("Power", energy / 100f);
        anim.SetFloat("Health", health / 100f);

    }

    /*
     * Setter and Getter methods
     */
    public float GetPostDeathHeath()
    {
        return postDeathHealth;
    }

    public void SetColor(Color botColor)
    {
        stats.defaultColor = botColor;
    }
   
    public ref BattleBotStats GetStats()
    {
        return ref stats;
    }



    private void FixedUpdate()
    {
        //update stats based on rigidbody properties and other vars
        stats.position = GetComponentInChildren<Rigidbody>().position;
        stats.velocity = GetComponentInChildren<Rigidbody>().velocity;
        stats.angularVelocity = GetComponentInChildren<Rigidbody>().angularVelocity;
        stats.forwardVector = GetComponentInChildren<Rigidbody>().transform.forward;
        stats.upVector = GetComponentInChildren<Rigidbody>().transform.up;
        stats.energy = energy;
        stats.uniqueID = competitorLink.uid;
        stats.health = health;
        stats.isShielded = isShielded;
        stats.isFlared = isFlared;
        stats.kills = competitorLink.score;
        score = competitorLink.score;
        competitorLink.credits = GetCredits();
    }


    /*
     * Coroutines
     */

    //generates credits over time

    IEnumerator CreditRegen()
    {
        while (true)
        {
            GiveCredits(1);
            yield return new WaitForSeconds(1f);
        }
    }

    

    //regenerates energy for the bot
    IEnumerator EnergyRegen()
    {
        while (true)
        {
            if (controlMode != ControlMode.Disabled)
            {
                //skip this tick if regen is suspended
                if (suspendEnergyRegen)
                {
                    suspendEnergyRegen = false;
                }
                else
                {
                    Recharge(energyRegenPerTick);
                }
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void InitiateSelfDestruct()
    {
        StartCoroutine(InitiateDestruct());
    }

    IEnumerator InitiateDestruct() {
        while (true)
        {
            TakeHit(20, this.competitorLink);
            yield return new WaitForSeconds(1f);
        }
    }

    //power the bot down if energy is critical
    IEnumerator ShutDown()
    {
        if (energy < -5f) energy = -5f;
        foreach (StabilisedMount stabiliser in GetComponentsInChildren<StabilisedMount>())
        {
            stabiliser.enabled = false;
        }
        ControlMode previousMode = controlMode;
        controlMode = ControlMode.Disabled;
        anim.SetBool("Disabled", true);
        activeWeapons[(int)Mount.LEFT].setAutoFire(false);
        activeWeapons[(int)Mount.RIGHT].setAutoFire(false);
        while (energy < 10f)
        {
            yield return new WaitForSeconds(1f);
            energy++;
        }
        anim.SetBool("Disabled", false);

        yield return new WaitForSeconds(1f);
        foreach (StabilisedMount stabiliser in GetComponentsInChildren<StabilisedMount>())
        {
            stabiliser.enabled = true;
        }
        controlMode = previousMode;
    }


    //the 'burn out' phase of a destroyed bot followed by the explosion
    IEnumerator SelfDestruct()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            energy = 0;
            postDeathHealth -= Time.deltaTime * Random.Range(5f, 10f);
            if (postDeathHealth < 0f)
            {
                Instantiate(deathExplosion, transform.GetChild(0).position, Quaternion.identity);
                allBots.Remove(this);
                //if we happen to be hosting the camera, don't destroy it.
                Camera c = GetComponentInChildren<Camera>();
                if (c) c.transform.parent = null;
                GameManager.instance.RequestRespawn(competitorLink);

                if (Minimap.instance)
                {
                    Minimap.instance.RemoveMapMarker(this);
                }
                Destroy(gameObject);
            }
        }
    }

    IEnumerator ScanPulse(Vector2 direction)
    {
        canScan = false;
        Vector3 planarDirection = new Vector3(direction.x, 0f, direction.y);
        float timer = 0.25f;
        while(timer>0f)
        {
            timer -= Time.fixedDeltaTime;
            head.transform.rotation = Quaternion.Slerp(head.transform.rotation, Quaternion.LookRotation(planarDirection), Time.fixedDeltaTime * 5f);
            yield return new WaitForFixedUpdate();
        }
        head.transform.rotation = Quaternion.LookRotation(planarDirection);
        scanner.EmitPulse(this);
        
        timer = 0.25f;
        while (timer > 0f)
        {
            timer -= Time.fixedDeltaTime;
            head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, Quaternion.identity, Time.fixedDeltaTime * 5f);
            yield return new WaitForFixedUpdate();
        }
        head.transform.localRotation = Quaternion.identity;
        canScan = true;

    }

    /*
     *  Methods used by BotAI
     */

    //perform a scan pulse directly forward

    public bool RequestSupplyDrop(DropPodStats.DroppodContentType type, Vector2 position)
    {
        if (controlMode!=ControlMode.Suspended)
        {
            return DropManager.instance.RequestPod(type, position, this);
        }
        else return false;
    }
    public bool DoScanPulse()
    {
        return DoScanPulse(head.transform.forward);
    }

    public bool DoScanPulse(Vector2 direction)
    {
        if (controlMode != ControlMode.Disabled && controlMode!=ControlMode.Suspended)
        {
            if (canScan && scanner && SpendEnergy(1))
            {
                StartCoroutine(ScanPulse(direction));
                return true;
            }
        }
        return false;
    }
    public bool DoScanPulse(Vector3 direction)
    {
        Vector2 removedY = new Vector2(direction.x, direction.z);
        return DoScanPulse(removedY);
    }

    //callback on round start
    public void OnInitialSpawn()
    {
        if(botAI&&controlMode==ControlMode.AI) botAI.GenerateNavigationData();
    }


    //callback on receiving scan data
    public void ReceiveScanData(BattleBotStats data)
    {
        if (controlMode == ControlMode.AI && botAI)
        {
            botAI.OnRecieveScanData(data);
        }  
    }

    public void ReceiveScanData(DropPodStats data)
    {
        if (controlMode == ControlMode.AI && botAI)
        {
            botAI.OnRecieveScanData(data);
        }
    }

    //deploys countermeasures
    public void DeployCountermeasures()
    {
        if (controlMode != ControlMode.Disabled && controlMode != ControlMode.Suspended)
        {
            if (activeCountermeasure != null)
            {
                activeCountermeasure.Deploy();
            }
        }
    }

    //fire a single shot
    public void FireWeapon(Mount mount)
    {
        if (controlMode != ControlMode.Disabled && controlMode != ControlMode.Suspended)
        {
            if(mount == Mount.LEFT)
            {
                activeWeapons[1].Fire();
            }
            if (mount == Mount.RIGHT)
            {
                activeWeapons[0].Fire();
            }

        }
    }

    //enable autofire
    public void AutoFireWeapon(bool autofire, Mount mount)
    {
        if (controlMode != ControlMode.Disabled && controlMode != ControlMode.Suspended)
        {
            if (mount == Mount.LEFT)
            {
                activeWeapons[1].setAutoFire(autofire);
            }
            if (mount == Mount.RIGHT)
            {
                activeWeapons[0].setAutoFire(autofire);
            }
        }
    }

    public bool GetAutoFire(Mount mount)
    {
        if (mount == Mount.LEFT)
        {
            return activeWeapons[1].getAutoFire();
        }
        if (mount == Mount.RIGHT)
        {
            return activeWeapons[0].getAutoFire();
        }
        return false;
    }

    public void HotswapWeapons()
    {
        DetachWeapons();
        stats.leftWeapon = competitorLink.prefabLink.GetComponent<BattleBot>().GetStats().leftWeapon;
        stats.rightWeapon = competitorLink.prefabLink.GetComponent<BattleBot>().GetStats().rightWeapon;

        if (stats.leftWeapon)
        {
            GameObject leftWeapon = Instantiate(stats.leftWeapon.gameObject, leftWeaponMount.transform.position, leftWeaponMount.transform.rotation);
            leftWeapon.transform.parent = leftWeaponMount.transform;
            activeWeapons[0] = leftWeapon.GetComponentInChildren<Weapon>();
        }

        if (stats.rightWeapon)
        {
            GameObject rightWeapon = Instantiate(stats.rightWeapon.gameObject, rightWeaponMount.transform.position, rightWeaponMount.transform.rotation);
            rightWeapon.transform.parent = rightWeaponMount.transform;
            activeWeapons[1] = rightWeapon.GetComponentInChildren<Weapon>();
            //flip right weapon
            Vector3 scale = rightWeapon.transform.localScale;
            scale.x = -1f * scale.x;
            rightWeapon.transform.localScale = scale;
        }

        if (stats.counterMeasure)
        {
            GameObject counter = Instantiate(stats.counterMeasure.gameObject, torso.transform.position, torso.transform.rotation);
            counter.transform.parent = torso.transform;
            activeCountermeasure = counter.GetComponent<CounterMeasure>();
        }

        //cache to activeWeapons[]
      

    }


    public void SwitchWeapon(Mount mount, Weapon.WeaponType weapon)
    {
        GameManager.instance.OverrideLoadout(competitorLink, mount, weapon);
    }

    public void SwitchCountermeasure(CounterMeasure.CounterMeasureType cm)
    {
        GameManager.instance.OverrideCountermeasure(competitorLink, cm);
    }

    //methods to control steering and throttle 

    public float GetThrottleState()
    {
        return forward;
    }

    public float GetSteerState()
    {
        return steer;
    }

    ControlMode previousControlMode;
    public void Suspend()
    {
        AutoFireWeapon(false, Mount.LEFT);
        AutoFireWeapon(false, Mount.RIGHT);
        previousControlMode = controlMode;
        controlMode = ControlMode.Suspended;
    }

    public void Unsuspend()
    {
        controlMode = previousControlMode;
    }


    public void SetThrottleState(float input)
    {
        if (controlMode != ControlMode.Disabled && controlMode != ControlMode.Suspended)
        {
            if ((input > 1f) || (input < -1f))
            {
                Debug.LogWarning("Throttle inputs should be in the range -1f to 1f. The input " + input + " has been clamped to this range");
                input = Mathf.Clamp(input, -1f, 1f);
            }

            forward = input;
        }
    }

    public void SetSteerState(float input)
    {
        if (controlMode != ControlMode.Disabled && controlMode != ControlMode.Suspended)
        {
            if ((input > 1f) || (input < -1f))
            {
                Debug.LogWarning("Steering inputs should be in the range -1f to 1f. The input " + input + " has been clamped to this range");
                input = Mathf.Clamp(input, -1f, 1f);
            }

            steer = input;
        }
    }

   

    //spend energy, called by weapons and countermeasures, true if can spend
    public bool SpendEnergy(uint amount)
    {
        suspendEnergyRegen = true;
        energy -= amount;
        if (energy < 0)
        {
            StartCoroutine(ShutDown());
            return false;
        }
        return true;
    }

    //methods to detach (despawn) weapons
    void DetachWeapons()
    {
        DetachWeapon(Mount.LEFT);
        DetachWeapon(Mount.RIGHT);
    }

    void DetachWeapon(Mount mount)
    {
        if (activeWeapons[(int)mount]) activeWeapons[(int)mount].Detach();
    }

    //method called by weapons that hit the bot to react to incoming damage

    public void Heal(uint amount)
    {
        health += amount;
        if (health > 100f) health = 100f;
    }
    
    public uint GetCredits()
    {
        return podCredits;
    }

    public void SpendCredits(uint amount)
    {
        podCredits -= amount;
        if (podCredits < 0)
        {
            Debug.LogWarning("Credits were spent that could not be afforded - report this bug!");
            podCredits = 0;
        }
    }

    public void ReplenishAmmo()
    {
        foreach(Weapon w in activeWeapons)
        {
            w.ReplenishAmmo();
        }
    }
    public void Recharge(uint amount)
    {
        energy += amount;
        if (energy > 100f) energy = 100f;
    }
    public void TakeHit(uint damage, BotCompetitor shooter)
    {
        
        if (!isDestroyed)
        {

            podCredits += damage;
            if(shooter!=null) shooter.credits+=damage;

            if (botAI)
            {
                if(shooter!=null) botAI.OnTakeDamage((shooter.instanceLink.torso.transform.position - torso.transform.position).normalized);
                else botAI.OnTakeDamage(Vector3.zero);
            }

            if (isShielded)
            {  
                SpendEnergy(damage);
            }
            else
            {
                health -= damage;
                if (health <= 0)
                {
                    foreach (UITag tag in UITag.allTags)
                    {
                        if (tag.linkedBot.competitorLink.uid == competitorLink.uid) {
                            if (shooter != null) tag.DoDeathAnimation(shooter.prefabLink.GetComponent<BattleBot>().GetStats().name);
                            else tag.DoDeathAnimation("Unknown");
                        }
                    }
                    //kill bot
                    if(botAI) botAI.OnDeath();
                    if (shooter != null)
                    {
                        if (shooter != this.competitorLink)
                        {
                            shooter.score++;
                            EventFeed.Broadcast(GetStats().name + " was killed by " + shooter.prefabLink.GetComponent<BattleBot>().GetStats().name);
                        }
                        else
                        {
                            shooter.score--;
                            EventFeed.Broadcast(GetStats().name + " self-destructed");
                        }
                        
                    }
               
                    isDestroyed = true;
                    //if there's a VFX death flame use that, otherwise revert to old animated one
                    if (deathFlame)
                    {
                        Instantiate(deathFlame, transform.GetChild(0).position, transform.GetChild(0).rotation, transform.GetChild(0));
                    }
                    else
                    {
                        anim.SetTrigger("Die");
                    }
                    DetachWeapons();
                    controlMode = ControlMode.Disabled;
                    StartCoroutine(SelfDestruct());
                }
            }
        }
        else
        {
            postDeathHealth -= damage;
        }

    }


    /*
     * get methods for other scripts
     */

    public void GiveCredits(uint numberOfCredits)
    {
        podCredits += numberOfCredits;
        if (podCredits > podCreditsMax)
        {
            podCredits = podCreditsMax;
        }
    }

    public float GetEnergy()
    {
        return energy;
    }
 
    public float GetHealth()
    {
        return health;
    }
   
    public Vector3 GetVelocity()
    {
        if (GetComponentInChildren<Rigidbody>()) return GetComponentInChildren<Rigidbody>().velocity;
        else
        {
            Debug.LogWarning("Get velocity failed - no rigidbody detected. Returning a zero vector.");
            return Vector3.zero;
        }
    }

    public ControlMode GetControlMode()
    {
        return controlMode;
    }
    public Color GetColor()
    {
        return stats.defaultColor;
    }

    public Transform GetHeadTransform()
    {
        return head.transform;
    }

}
