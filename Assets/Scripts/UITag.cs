using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UITag : MonoBehaviour
{

    //an individual UI 'tag' that shows on the right in playmode

    public BattleBot linkedBot; //bot whose stats are displayed
    public static List<UITag> allTags; //all active tags for convenience
    public int linkedID; //linked unique ID - we need this to ensure consistency when a bot respawns
    int cameraState = 0;

    //text field assignments in prefab
    [SerializeField]
    TMP_Text botNameField,
    botInfoField,
    botScoreField,
    botWeaponsField,
    botKillerField,
    botRespawnTimerField,
    botCreditField;

    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
        if (allTags == null)
        {
            allTags = new List<UITag>();
        }
        allTags.Add(this);
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
       
        StartCoroutine(UpdateTag());
    }

    void UpdateColor()
    {
        //recolour all shaders that have an exposed field for recolouration based on bot color etc.

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (m.HasColor("_BotColor"))
                {
                    m.SetColor("_BotColor", linkedBot.GetStats().defaultColor);
                }

                if (m.HasFloat("_Energy"))
                {
                    m.SetFloat("_Energy", linkedBot.GetEnergy()/100f);
                }

                if (m.HasFloat("_Health"))
                {
                    m.SetFloat("_Health", linkedBot.GetHealth() / 100f);
                }
            }
        }

    }

    public void DoDeathAnimation(string killerName)
    {
        StartCoroutine(DeathAnimation(killerName));
    }

    IEnumerator DeathAnimation(string killerName)
    {
        //masks the tag using the animator and shows killer etc.

        float timer = GameManager.instance.GetRespawnTimer();
        botKillerField.text = "Killed by " + killerName + "!";
        anim.SetBool("showDeath", true);
        botRespawnTimerField.text = "";
        yield return new WaitUntil(()=>linkedBot==null); //wait until the bot fully despawns (explodes)
        while (timer > 0f)
        {
            botRespawnTimerField.text = "Respawning in " + Mathf.Round(timer) + "...";
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }
        anim.SetBool("showDeath", false);
    }

    IEnumerator UpdateTag()
    {
        //keep the tag updated with the in-game values of the linked bot
        while (true)
        {
            if (linkedBot != null) //this will be null in the interval between explosion and respawn
            {
                linkedID = linkedBot.competitorLink.uid;
                UpdateColor();
                botNameField.text = linkedBot.GetStats().name;
                botInfoField.text = linkedBot.GetStats().creatorName;
                botScoreField.text = linkedBot.competitorLink.score.ToString();
                botCreditField.text = linkedBot.GetCredits().ToString() + "CR";
                string weaponFieldString = "";
                if (linkedBot.GetStats().leftWeapon)
                {
                    weaponFieldString += linkedBot.GetStats().leftWeapon.name;
                }
                else
                {
                    weaponFieldString += "Empty";
                }

                weaponFieldString += " / ";

                if (linkedBot.GetStats().rightWeapon)
                {
                    weaponFieldString += linkedBot.GetStats().rightWeapon.name;
                }
                else
                {
                    weaponFieldString += "Empty";
                }

                weaponFieldString += " / ";

                if (linkedBot.GetStats().counterMeasure)
                {
                    weaponFieldString += linkedBot.GetStats().counterMeasure.name;
                }
                else
                {
                    weaponFieldString += "Empty";
                }

                botWeaponsField.text = weaponFieldString;
            }
            yield return new WaitForSeconds(0.5f);

        }
    }
  
    public void OnClick()
    {
        cameraState++;
        if (cameraState < 3)
        {
            if (linkedBot != null)
            {
                CameraManager.instance.CameraSwitch(linkedBot);
                CameraManager.instance.mode = CameraManager.Mode.Disabled;
            }
        }
        else
        {
            CameraManager.instance.CameraSwitch();
            CameraManager.instance.mode = CameraManager.Mode.AI;
            cameraState = 0;
        }


        
    }
   
}
