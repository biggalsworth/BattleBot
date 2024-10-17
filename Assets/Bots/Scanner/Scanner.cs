using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    //the bot's scanner. most of the scan logic is in ScanPulse, attached to the pulse object the scanner spawns.

    [SerializeField]
    GameObject pulse;
       
    public void EmitPulse(BattleBot source)
    {
        GameObject scanPulse = Instantiate(pulse.gameObject, transform.position, transform.rotation);
        scanPulse.GetComponentInChildren<ScanPulse>().source = source;
        scanPulse.GetComponentInChildren<ScanPulse>().color = source.GetStats().defaultColor;
    }

    public void EmitParentedPulse(BattleBot source)
    {
        GameObject scanPulse = Instantiate(pulse.gameObject, transform.position, transform.rotation, transform);
        scanPulse.GetComponentInChildren<ScanPulse>().source = source;
        scanPulse.GetComponentInChildren<ScanPulse>().color = source.GetStats().defaultColor;
    }
}
