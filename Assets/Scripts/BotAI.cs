using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BotAI : MonoBehaviour
{

    public BattleBot myBot;
    public virtual void GenerateNavigationData() { }
    public virtual void OnStart() { }
    public virtual void OnTakeDamage(Vector3 direction) { }
    public virtual void OnRecieveScanData(BattleBotStats data) { }
    public virtual void OnRecieveScanData(DropPodStats data) { }
    public virtual void OnDeath() { } 
}
