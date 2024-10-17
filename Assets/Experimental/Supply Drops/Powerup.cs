using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Powerup : MonoBehaviour
{

    public DropPod linkedPod;

    

    [SerializeField]
    DropPodStats.DroppodContentType contentType = DropPodStats.DroppodContentType.EMPTY;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<BattleBot>())
        {
            ApplyBuff(other.GetComponentInParent<BattleBot>());
          
        }
    }

    public abstract DropPodStats.DroppodContentType GetContentType();
    protected virtual void ApplyBuff(BattleBot bot) { }

   
}
