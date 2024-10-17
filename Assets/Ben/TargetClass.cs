using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TargetClass : MonoBehaviour
{
    public BattleBot parentBot;
    public BattleBotStats data;
    public Vector3 position = Vector3.zero;

    public float ID;
    public float health = 100;

    float timestamp;

    float distance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ID = data.uniqueID;
        health = data.health;
        //position = data.position;
    }

    //Update the data for this target
    public void UpdateDate(BattleBotStats data)
    {
        ID = data.uniqueID;
        position = data.position;
        health = data.health;
        
        //i used to make sure the postion would account for velocity change, but it made it inaccurate
        //position += data.forwardVector + (data.velocity * Time.deltaTime);

        timestamp = Time.time;
    }
}
