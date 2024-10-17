using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{

    Animator anim;
    static List<BotMarker> activeMarkers;

    public float mapTransformFactor = 1f;
    public Vector2 mapOffsetVector;

    [SerializeField]
    BotMarker botMarkerPrefab;

    public static Minimap instance;

    // Start is called before the first frame update
    void Start()
    {
        activeMarkers = new List<BotMarker>();
        if(instance == null)
        {
            instance = this;
        }
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {

             anim.SetBool("Show", !anim.GetBool("Show"));

        }
    }

    public static Vector3 WorldCoordToMinimapCoord(Vector3 worldCoord)
    {
        float x = worldCoord.x / instance.mapTransformFactor + instance.mapOffsetVector.x;
        float y = 0f;
        float z = worldCoord.z / instance.mapTransformFactor + instance.mapOffsetVector.y;
        return new Vector3(x, y, z);
    }

    public void AddMapMarker(BattleBot owner)
    {
        GameObject marker = Instantiate(botMarkerPrefab.gameObject, transform);
        marker.GetComponent<BotMarker>().linkedBot = owner;
        activeMarkers.Add(marker.GetComponent<BotMarker>());
    }

    public void RemoveMapMarker(BattleBot owner)
    {
        int index = 0;
        for (int i = 0; i < activeMarkers.Count; i++)
        {
            if (activeMarkers[i].GetComponent<BotMarker>().linkedBot == owner)
            {
                index = i;
                Destroy(activeMarkers[i].gameObject);
            }
        }
        activeMarkers.RemoveAt(index);

    }


}
