using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualiser : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    public GameObject pathNode;
    public GameObject pathEdge;


    public void VisualiseGraph(BenJNodeGraph g)
    {
        foreach (BenJVertex v in g.allVertices)
        {
            Instantiate(nodePrefab, v.position, Quaternion.identity);
            foreach (Edge e in v.edges)
            {
                GameObject line = Instantiate(edgePrefab);
                line.GetComponent<LineRenderer>().SetPosition(0, e.start.position);
                line.GetComponent<LineRenderer>().SetPosition(1, e.end.position);
            }
        }
    }

    public List<GameObject> currentVisualisation;
    public void VisualisePath(List<BenJVertex> path)
    {
        ClearVisualisation();
        foreach (BenJVertex v in path)
        {
            GameObject node = Instantiate(pathNode, v.position, Quaternion.identity);
            currentVisualisation.Add(node);
            foreach (Edge e in v.edges)
            {
                if (path.Contains(e.end))
                {
                    GameObject line = Instantiate(pathEdge);
                    currentVisualisation.Add(line);
                    line.GetComponent<LineRenderer>().SetPosition(0, e.start.position);
                    line.GetComponent<LineRenderer>().SetPosition(1, e.end.position);
                }
            }
        }
    }

    public void ClearVisualisation()
    {
        foreach (GameObject go in currentVisualisation)
        {
            Destroy(go);
        }
    }


}