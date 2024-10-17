using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyPowerup : Powerup
{
    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.ENERGY;
    }

    protected override void ApplyBuff(BattleBot bot)
    {
        GetComponent<MeshRenderer>().enabled = false;
        linkedPod.Despawn();
        StartCoroutine(Recharge(bot));
    }


    IEnumerator Recharge(BattleBot bot)
    {
        int ticksRemaining = 10;
        while (ticksRemaining > 0)
        {
            bot.Recharge(10);
            yield return new WaitForSeconds(1f);
            ticksRemaining--;
        }
     
        Destroy(gameObject);
    }
}
