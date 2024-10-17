using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : Powerup {

    public override DropPodStats.DroppodContentType GetContentType()
    {
        return DropPodStats.DroppodContentType.WEAPONSWAP;
    }
    protected override void ApplyBuff(BattleBot bot)
    {
        bot.HotswapWeapons();
        linkedPod.Despawn();
        Destroy(gameObject);
    }


}
