using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPowerup : Powerup
{

    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.HEALTH;
    }
    protected override void ApplyBuff(BattleBot bot)
    {
        bot.ReplenishAmmo();
        linkedPod.Despawn();
        Destroy(gameObject);
    }
}
