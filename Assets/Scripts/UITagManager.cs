using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITagManager : MonoBehaviour
{

    //manages all UI tags in the HUD

    [SerializeField]
    GameObject tagPrefab; //tag prefab to spawn for each bot

    [SerializeField]
    GameObject toolPrefab; //minimap tool

    List<UITag> activeTags; //all active tags

    private void Awake()
    {
        activeTags = new List<UITag>();
        GameObject map = Instantiate(toolPrefab.gameObject, transform.position, transform.rotation, transform);
        map.transform.localPosition = new Vector3(0f, -1f, 0f);
    }
  

    //add tag is called whenever a bot spawns
    public void AddTag(BattleBot bot)
    {
        //we need to check if the bot is a new spawn, or a respawn
        bool tagAlreadyExists = false;

        foreach(UITag uitag in activeTags)
        {
            if (uitag.linkedID == bot.competitorLink.uid) tagAlreadyExists = true;
        }

        
        if (!tagAlreadyExists) //if it's not a respawn, create a new tag
        {
            GameObject tag = Instantiate(tagPrefab.gameObject, transform.position, transform.rotation, transform);
            tag.GetComponentInChildren<UITag>().linkedBot = bot;
            activeTags.Add(tag.GetComponentInChildren<UITag>());

            //this defines an arbitrary value for how much to vertically move the additional tags down
            tag.transform.localPosition = new Vector3(0f, 0.65f - 0.2f * activeTags.Count, 0f);
        }
    }

   
}
