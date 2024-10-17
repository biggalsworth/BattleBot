using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGrid : MonoBehaviour
{

    /* You're welcome to tinker with and expand this script, however, note all final bots should share the same navgrid, therefore this is not a modifiable component.
     * 
     * If you do want to explore this, I recommend you save frequently.
     * This has a high 'big-O', and a typo (e.g. asking for a 2560x2560 grid) will likely hang Unity for a long time
     * 
     * If you think you have a better/more efficient way of doing things, do let us know and we can consider factoring it into the main branch.
     * 
     */



    [SerializeField]
    bool visualiseGrid = false; //should I visualise impassable cells? (performance intensive)

    [SerializeField]
    GameObject gridCell; //prefab to use to visualise those cells

    public bool[,] data; //core navgrid data. cell at n,m is true if passable, false if not

    int gridScale = 4; //hardcoded grid scale
    

    void Start()
    {
        StartCoroutine(generateGrid()); //this is in a couroutine to allow execution over time, though currently it runs immediately in Start() (there is no yield for the next frame).
    }

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
                if (visualiseGrid&&!data[n,m])
                {
                    GameObject cell = Instantiate(gridCell, transform.position + new Vector3(gridScale*n, 0, gridScale*m), Quaternion.identity);
                    cell.transform.localScale = new Vector3(gridScale, 2f, gridScale);
                    cell.transform.parent = gameObject.transform;
                }
                
                
                

            }
        
        }
        yield return null; //amend to endofframe or seconds to generate the navgrid and spread the performance hit across frames/time.
    }

}
