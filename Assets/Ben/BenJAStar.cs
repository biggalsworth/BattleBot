using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenJAStar
{
    BenJNodeGraph graph;
    //public Transform startPos;
    //public Transform targPos;


    public List<BenJVertex> A_Star(Vector3 startPos, Vector3 targetPostion, BenJNodeGraph exGraph)
    {
        float h = hueristic(startPos, targetPostion);

        graph = exGraph;
        BenJVertex start = graph.GetNearestVertex(startPos);
        BenJVertex goal = graph.GetNearestVertex(targetPostion);

        // The set of discovered nodes that may need to be (re-)expanded.
        // Initially, only the start node is known.
        // This is usually implemented as a min-heap or priority queue rather than a hash-set.
        List<BenJVertex> openSet = new List<BenJVertex>();
        openSet.Add(start);

        // For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from the start
        // to n currently known.
        Dictionary<BenJVertex, BenJVertex> cameFrom = new Dictionary<BenJVertex, BenJVertex>();

        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<BenJVertex, float> gScore = new Dictionary<BenJVertex, float>(); //:= map with default value of Infinity
        gScore.Add(start, 0);


        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<BenJVertex, float> fScore = new Dictionary<BenJVertex, float>();
        fScore.Add(start, h);

        while(openSet.Count > 0)
        {
            // This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue
            BenJVertex current = openSet[0];
            if(current.position == goal.position)
            {
                return reconstruct_path(cameFrom, current);
            }

            openSet.Remove(current);
            //for each neighbor of current
            for(int i = 0; i < current.edges.Count; i++)
            {
                BenJVertex neighbour = current.edges[i].end;
                if(neighbour == current)
                {
                    neighbour = current.edges[i].start;
                    //continue;
                }
                // d(current,neighbor) is the weight of the edge from current to neighbor
                // tentative_gScore is the distance from start to the neighbor through current
                gScore.TryGetValue(current, out float value);
                float tentative_gScore = value + Vector3.Distance(current.position, neighbour.position);
                if (tentative_gScore < gScore.GetValueOrDefault(neighbour, float.PositiveInfinity))
                {
                    // This path to neighbor is better than any previous one. Record it!
                    //cameFrom[neighbour] = current;
                    cameFrom[neighbour] = current;

                    gScore[neighbour] = tentative_gScore;
                    fScore[neighbour] = tentative_gScore + hueristic(neighbour.position, targetPostion);
                    if(!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }

            }
        }


        // Open set is empty but goal was never reached
        return new List<BenJVertex>();
    }
    public float hueristic(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    List<BenJVertex> reconstruct_path(Dictionary<BenJVertex, BenJVertex> cameFrom, BenJVertex current)
    {
        List<BenJVertex> total_path = new List<BenJVertex>();
        total_path.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            total_path.Add(current);
        }
        //so far this draws from the target to the player, this will make the list start from the nearest to the player, to the destination
        total_path.Reverse();
        return total_path;
    }

}
