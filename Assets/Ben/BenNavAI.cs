using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BenNavAI : MonoBehaviour
{
    [SerializeField]
    bool visualiseGrid = false; //should I visualise impassable cells? (performance intensive)

    [SerializeField]
    GameObject gridCell; //prefab to use to visualise those cells

    public bool[,] data; //core navgrid data. cell at n,m is true if passable, false if not

    [HideInInspector]
    public int gridScale = 4; //hardcoded grid scale


    void Start()
    {
        StartCoroutine(generateGrid()); //this is in a couroutine to allow execution over time, though currently it runs immediately in Start() (there is no yield for the next frame).
    }

    //responsible for calculating where the bot is unable to travel throught
    IEnumerator generateGrid()
    {

        data = new bool[256 / gridScale, 256 / gridScale]; //hardcoded number of cells (after testing, to check logical 'passable' routes made sense whilst minimising the size of the data)

        //for each row - we're at a big O of O(n) right now
        for (int n = 0; n < 256 / gridScale; n += 1)
        {

            //for each column - now our big-O is O(n*m)
            for (int m = 0; m < 256 / gridScale; m += 1)
            {

                data[n, m] = true;
                Collider[] overlappingColliders;
                overlappingColliders = Physics.OverlapBox(new Vector3(transform.position.x + gridScale * n, transform.position.y, transform.position.z + gridScale * m),
                                                            new Vector3(gridScale / 2f, 1f, gridScale / 2f));

                //we also need to sift all the colliders to see if they're in 'default', and should not be navigable. O(n*m*c) - ouch!
                foreach (Collider c in overlappingColliders)
                {
                    if (c.gameObject.layer == 10)
                    {
                        data[n, m] = false;
                    }
                }

                //if we're visualising and it's impassable, spawn a cube. This is very performance heavy. A better way would be to pass it as a matrix to a shader to render in a single draw call.
                if (visualiseGrid && !data[n, m])
                {
                    GameObject cell = Instantiate(gridCell, transform.position + new Vector3(gridScale * n, 0, gridScale * m), Quaternion.identity);
                    cell.transform.localScale = new Vector3(gridScale, 2f, gridScale);
                    cell.transform.parent = gameObject.transform;
                }
            }
        }
        yield return null; //amend to endofframe or seconds to generate the navgrid and spread the performance hit across frames/time.
    }


}

//The class that is responsible for storing all the vertices of a graph.
public class BenJVertex
{
    //stores the position of this vertex
    public Vector3 position;
    //each vertex should have neighbour vertices, this edge lines are what connects them.
    public List<Edge> edges = new List<Edge>();

    public BenJVertex(Vector3 pos)
    {
        position = pos;
    }

}

//The edge is responsible for connecting each vertex.
public class Edge
{
    //an edge will store the two vertices that they connect.
    public BenJVertex start;
    public BenJVertex end;
    public float weight;
    //when creating the edge, attach each vertex and assign a weight.
    public Edge(BenJVertex startVertex, BenJVertex endVertex, float w)
    {
        start = startVertex;
        end = endVertex;
        weight = w;
    }
}

//create the node graph for the entire map.
//this is what the AI will travel along.
//using vertices as checkpoints of where to travel to, and the edges are how to get to each vertex.
public class BenJNodeGraph
{
    public List<BenJVertex> allVertices;

    BenJAStar AStar;

    public void GenerateGraph(bool[,] sourceData, float sourceGridscale)
    {
        //first, we'll create an array of nodes that mirror the source data array
        allVertices = new List<BenJVertex>();
        BenJVertex[,] vertexArray = new BenJVertex[sourceData.GetLength(0), sourceData.GetLength(1)];

        for (int n = 0; n < sourceData.GetLength(0); n++)
        {
            for (int m = 0; m < sourceData.GetLength(1); m++)
            {
                //make a vertex with a world position based on the grid scale.
                // Note this relies on the grid starting at 0,0,0 in world space - if it doesn't, you need to add an offset vector3.
                vertexArray[n, m] = new BenJVertex(new Vector3(sourceGridscale * n, 0, sourceGridscale * m));
            }
        }


        for (int n = 1; n < sourceData.GetLength(0) - 1; n++)
        {
            for (int m = 1; m < sourceData.GetLength(1) - 1; m++)
            {
                if (sourceData[n, m]) //create a vertex only if the selected cell is passable
                {
                    //check adjacency and create edges
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (!(x == 0 && y == 0)) //don't create an edge to ourself
                            {
                                if (sourceData[x + n, y + m]) //create edges only to passable vertices
                                {
                                    vertexArray[n, m].edges.Add(new Edge(vertexArray[n, m], vertexArray[x + n, y + m], Vector3.Distance(vertexArray[n, m].position, vertexArray[x + n, y + m].position)));
                                }
                            }
                        }
                    }
                }
                allVertices.Add(vertexArray[n, m]);
            }
        }
    }

    /// <summary>
    /// Get the nearest vertex.
    /// <param name="position">Postion: Player Position</param>
    /// </summary>
    /// <returns>
    /// The nearest vertex
    /// </returns>
    public BenJVertex GetNearestVertex(Vector3 position)
    {
        float distance = Mathf.Infinity;
        BenJVertex returnVal = new BenJVertex(Vector3.zero);
        foreach (BenJVertex v in allVertices)
        {
            if (Vector3.Distance(position, v.position) < distance)
            {
                returnVal = v;
                distance = Vector3.Distance(position, v.position);
            }
        }
        if (returnVal.position == Vector3.zero)
        {
            Debug.LogWarning("Could not find a nearest node");
            return null;
        }
        return returnVal;
    }


    public List<BenJVertex> GetAllConnectedVertices(Vector3 start)
    {
        return GetAllConnectedVertices(GetNearestVertex(start));
    }
    public List<BenJVertex> GetAllConnectedVertices(BenJVertex start)
    {
        List<BenJVertex> visited = new List<BenJVertex>();
        Queue<BenJVertex> queue = new Queue<BenJVertex>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            BenJVertex vertex = queue.Dequeue();
            foreach (Edge edge in vertex.edges)
            {
                if (!visited.Contains(edge.end))
                {
                    queue.Enqueue(edge.end);
                    visited.Add(edge.end);
                }
            }
        }
        return visited;
    }


    //Finding the path
    public List<BenJVertex> FindGreedyPath(Vector3 startPosition, Vector3 endPosition)
    {
        BenJVertex vertex = GetNearestVertex(startPosition);
        BenJVertex target = GetNearestVertex(endPosition);

        Dictionary<BenJVertex, float> distances = new Dictionary<BenJVertex, float>();
        Dictionary<BenJVertex, BenJVertex> previousVertices = new Dictionary<BenJVertex, BenJVertex>();
        List<BenJVertex> unvisited = new List<BenJVertex>();

        foreach (BenJVertex v in GetAllConnectedVertices(vertex))
        {
            distances[v] = Mathf.Infinity;
            previousVertices[v] = null;
            unvisited.Add(v);
        }

        distances[vertex] = 0;

        while (unvisited.Count > 0)
        {

            BenJVertex currentVertex = unvisited.OrderBy(node => distances[node]).First();

            if (currentVertex == null) break;
            if (currentVertex == target) break;

            unvisited.Remove(currentVertex);

            foreach (Edge edge in currentVertex.edges)
            {
                float tentativeDistance = distances[currentVertex] + edge.weight;

                if (tentativeDistance < distances[edge.end])
                {
                    distances[edge.end] = tentativeDistance;
                    previousVertices[edge.end] = currentVertex;
                }

            }
        }

        return ReconstructPath(vertex, target, previousVertices);

    }

    List<BenJVertex> ReconstructPath(BenJVertex start, BenJVertex end, Dictionary<BenJVertex, BenJVertex> previousVertices)
    {
        List<BenJVertex> path = new List<BenJVertex>();
        BenJVertex currentVertex = end;
        while (currentVertex != null)
        {
            path.Insert(0, currentVertex);
            currentVertex = previousVertices[currentVertex];

        }
        return path;
    }

}



