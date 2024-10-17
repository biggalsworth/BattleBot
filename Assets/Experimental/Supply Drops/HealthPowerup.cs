using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerup : Powerup
{

    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.HEALTH;
    }
    protected override void ApplyBuff(BattleBot bot)
    {
        bot.Heal(100);
        linkedPod.Despawn();
        Destroy(gameObject);
    }
}
