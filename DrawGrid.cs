using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DrawGrid : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Get the main camera and its position for use in the grid drawing script
        Vector3 camPos = getMainCameraPos();
        float camPosX = camPos[0];
        float camPosZ = camPos[2];

        // Draw a 10x10 grid in use for the A* algorithm, based on the main camera's position
		/*for (int i = 0; i<10; i++) {
			for (int j = 0; j < 10; j++) {
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
				cube.transform.position = new Vector3 (camPosX + i * 10f, 0.5f, camPosZ + j * 10f);
				cube.transform.localScale = new Vector3 (9.9f, 1f, 9.9f);
			}
		}*/
        //Debug.DrawLine(new Vector3(10, 10, 10), new Vector3(0, 10, 50), Color.black, 100f);
        computePath();
    }
	
	// Update is called once per frame
	void Update () {
	}

    void computePath()
    {
        // get the main camera's position
        Vector3 camPos = getMainCameraPos();
        float camPosX = camPos[0];
        float camPosZ = camPos[2];

        // start and end positions of the algorithm
        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 endPos = new Vector3(0, 0, 80);

        List<Vector3> grid = new List<Vector3>();
        for (int i=0; i<10; i++)
        {
            for (int j=0; j<10; j++)
            {
                float posX = camPosX + i * 10;
                float posZ = camPosZ + j * 10;
                //float posY = Terrain.activeTerrain.SampleHeight(new Vector3(posX, 0, posZ));
                grid.Add(new Vector3(posX, 0, posZ));
            }
        }


        /*for (int i=0; i<10; i++)
        {
            for (int j=0; j<10; j++)
            {
                Debug.Log(grid[i, j]);
            }
        }*/

        List<Vector3> finalPath = aSharp(endPos, startPos, grid);
        // drawPath(finalPath);
    }

    /*private void drawPath(List<Vector3> finalPath)
    {

    }*/

    private List<Vector3> aSharp(Vector3 goal, Vector3 start, List<Vector3> grid)
    {
        // create lists of frontier and explored nodes
        List<Vector3> frontier = new List<Vector3>();
        List<Vector3> explored = new List<Vector3>();
        List<Vector3> finalPath = new List<Vector3>();

        // dictionary of each node and where it can be reached from
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

        // cost of getting to the node from the start node
        Dictionary<Vector3, float> gFunction = new Dictionary<Vector3, float>();
        // add the start node with a cost of 0 to go to itself
        gFunction.Add(start, 0);

        // total cost of getting from the start node to the goal
        Dictionary<Vector3, float> fFunction = new Dictionary<Vector3, float>();
        fFunction.Add(start, h(start, goal));

        // add the start node
        frontier.Add(start);

        // start the loop
        while (frontier.Count != 0)
        {
            Vector3 current = getLowestinDict(fFunction, frontier);

            // Debug.Log(current);

            if (current == goal)
            {
                finalPath = makePath(cameFrom, goal);
            }

            // remove current from the frontier
            frontier.Remove(current);
            explored.Add(current);

            // get list of adjacent nodes
            List<Vector3> neighbors = getAdjacent(current);

            // Debug.Log(neighbors[3]);

            // go through each neighbor
            while (neighbors.Count != 0)
            {
                Vector3 neighbor = neighbors[0];
                neighbors.RemoveAt(0);
                if (!grid.Contains(neighbor) || explored.Contains(neighbor))
                {
                    continue;
                }
                float tempGScore = gFunction[current] + getGScore(current, neighbor);
                if (grid.Contains(neighbor) && !frontier.Contains(neighbor)) {
                    frontier.Add(neighbor);
                }
                else if (tempGScore >= gFunction[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                // Debug.Log(cameFrom[neighbor]);
                gFunction[neighbor] = tempGScore;
                // Debug.Log(tempGScore);
                // Debug.Log(gFunction[neighbor]);
                fFunction[neighbor] = gFunction[neighbor] + h(neighbor, goal);
                // Debug.Log(fFunction[neighbor]);
            }
        }

        return finalPath;
    }

    private float getGScore(Vector3 start, Vector3 end)
    {
        Vector3 final = new Vector3(end[0] - start[0], 0, end[2] - start[2]);
        //float posY = Terrain.activeTerrain.SampleHeight(new Vector3(posX, 0, posZ));
        float startPosY = Terrain.activeTerrain.SampleHeight(new Vector3(start[0], 0, start[2]));
        float endPosY = Terrain.activeTerrain.SampleHeight(new Vector3(end[0], 0, end[2]));
        final[2] = endPosY - startPosY;

        // 3 dimensional distance formula
        float gScore = (float)(Math.Sqrt((final[0] * final[0]) + (final[1] * final[1]) + (final[2] * final[2])));

        return gScore;
    }

    private List<Vector3> getAdjacent(Vector3 current)
    {
        List<Vector3> finalList = new List<Vector3>();
        finalList.Add(new Vector3(current[0] + 10, current[1], current[2]));
        finalList.Add(new Vector3(current[0] - 10, current[1], current[2]));
        finalList.Add(new Vector3(current[0], current[1], current[2] + 10));
        finalList.Add(new Vector3(current[0], current[1], current[2] - 10));

        return finalList;
    }

    private List<Vector3> makePath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
    {
        Debug.Log("Hello");
        List<Vector3> finalPath = new List<Vector3>();
        finalPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            Vector3 temp = current;
            current = cameFrom[current];
            Debug.Log(temp);
            Debug.Log(current);
            float tempPosY = Terrain.activeTerrain.SampleHeight(new Vector3(temp[0], 0, temp[2]));
            float currentPosY = Terrain.activeTerrain.SampleHeight(new Vector3(current[0], 0, current[2]));
            Debug.DrawLine(new Vector3(temp[0], tempPosY, temp[2]), new Vector3(current[0], currentPosY, current[2]), Color.black, 10000f);
            finalPath.Add(current);
        }
        return finalPath;
    }

    private Vector3 getLowestinDict(Dictionary<Vector3, float> dict, List<Vector3> frontier)
    {
        float min = 10000000;
        Vector3 minVector = new Vector3(0, 0, 0);
        foreach (KeyValuePair<Vector3, float> kvp in dict)
        {
            if (kvp.Value < min && frontier.Contains(kvp.Key))
            {
                min = kvp.Value;
                minVector = kvp.Key;
            }
        }
        return minVector;
    }

    private float h(Vector3 goal, Vector3 start) 
    {
        // distance formula
        float result = (float)Math.Sqrt((goal[0] + start[0]) * (goal[0] + start[0]) + (goal[2] + start[2]) * (goal[2] + start[2]));

        return result;
    }

    private Vector3 getMainCameraPos()
    {
        Camera mainCam = Camera.main;
        Vector3 camPos = Camera.main.gameObject.transform.position;

        return camPos;
    }

}
