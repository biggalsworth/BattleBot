using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotCompetitor
{
    public GameObject prefabLink;
    public BattleBot instanceLink;
    public int score;
    public uint credits;
    public int uid;
    public static int id = 0;

    public BotCompetitor(GameObject prefabLink)
    {
        this.prefabLink = prefabLink;

        uid = id;
        id++;
        score = 0;
    }
}
