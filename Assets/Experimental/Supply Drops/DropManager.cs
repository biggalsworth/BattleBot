using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> availableDropPods;

    public static DropManager instance;
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public bool RequestPod(DropPodStats.DroppodContentType type, Vector2 coordinates, BattleBot owner)
    {
        foreach(GameObject go in availableDropPods)
        {
            if (go.GetComponentInChildren<DropPod>().contents.GetContentType() == type)
            {

                if (owner.GetCredits() >= go.GetComponentInChildren<DropPod>().cost)
                {

                    EventFeed.Broadcast("A drop pod containing " + type.ToString().ToLower() + " is inbound for " + owner.competitorLink.prefabLink.GetComponent<BattleBot>().GetStats().name) ;
                    GameObject pod = Instantiate(go, new Vector3(coordinates.x, 0f, coordinates.y), Quaternion.identity);
                    owner.SpendCredits(go.GetComponentInChildren<DropPod>().cost);
                    pod.GetComponentInChildren<DropPod>().SetOwner(owner);
                    return true;
                }
                else
                {
                   // Debug.LogWarning("A pod drop of " + go.GetComponentInChildren<DropPod>().contents.GetContentType() + " was requested but " + owner.name + " could not afford it");
                }
            }
        }
        return false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
