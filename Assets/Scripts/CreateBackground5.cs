using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateBackground5 : MonoBehaviour {
    //Create curved background for tunnel section
    float striationHeight = 0.4f;
    float striationAngle = 0f;
    //float localStriationRef = 0.45f;

    float pieceLength = 1f;
    int nodesPerLength = 2;
    float zPos = 0.5f;
    //float edgePadding = 0.05f;

    //float maxBumpiness = 0.2f;

    CreateWorld2 createWorld;

    //Lists are public for troubleshooting
    [SerializeField]
    List<int> publicTriangles;
    [SerializeField]
    List<int> bottomTriangleState = new List<int>();
    [SerializeField]
    List<int> topTriangleState = new List<int>();
    [SerializeField]
    List<int> nodeCounts = new List<int>();
    [SerializeField]
    List<Vector3> newVerts = new List<Vector3>();

    public LayerMask backgroundLayer;

    public GameObject markerPrefab;

	// Use this for initialization
	void Awake () {
        createWorld = GameObject.Find("WorldCreator").GetComponent<CreateWorld2>();
	}

    List<float> CreateMasterStriationList(Vector3 floorStart, Vector3 ceilingStart, List<Vector3> floorEndPoints, List<Vector3> ceilingEndPoints)
    {
        //Create list of striation elevations within the min and max elevations. Return as local elevations with respect to zero
        List<float> striationList = new List<float>();
        float buffer = 5.1f;    //Add buffer to max and min to ensure that all points will be within range
        float zeroElev = floorStart.y;

        float[] points = new float[floorEndPoints.Count+1];
        points[0] = floorStart.y;
        int i = 1;
        foreach (Vector3 pts in floorEndPoints)
        {
            points[i] = pts.y;
            i++;
        }
        float minElev = Mathf.Min(points) - buffer;

        points = new float[ceilingEndPoints.Count+1];
        points[0] = ceilingStart.y;
        i = 1;
        foreach (Vector3 pts in ceilingEndPoints)
        {
            points[i] = pts.y;
            i++;
        }
        float maxElev = Mathf.Max(points) + buffer;

        bool finished = false;
        float elevationPoint = minElev;
        
        while (!finished)
        {
            if (elevationPoint >= maxElev)
            {
                striationList.Add(maxElev);
                finished = true;
            }
            else
            {
                striationList.Add(elevationPoint);
            }
            elevationPoint += striationHeight;
        }

        List<float> localStriationList = new List<float>();
        foreach (float elev in striationList)
        {
            localStriationList.Add(elev - zeroElev);    //Convert list to local coordinate system
        }

        return localStriationList;
    }

    List<Vector3> CreateNodesBetween(Vector3 floorPos, Vector3 ceilingPos, List<float> masterStriationList, float currentPosX, float absoluteElevRef, float absoluteXRef)
    {
        //Debug.Log("NodesBetween: floorPos: " + floorPos + "ceilingPos: " + ceilingPos);
        
        //Node positions returned are with respect to floorPos as 0,0,0
        List<Vector3> nodesList = new List<Vector3>();
        float padding = 0.3f;

        float upperPos = ceilingPos.y - absoluteElevRef + padding;
        float lowerPos = floorPos.y - absoluteElevRef - padding;

        float radius = (upperPos - lowerPos)/2;

        List<float> yPosList = new List<float>();

        //yPosList.Add(lowerPos - absoluteElevRef);
        foreach (float point in masterStriationList)
        {
            if (point < upperPos - 0.05f && point > lowerPos + 0.05f)
            {
                yPosList.Add(point);
            }
        }

        //Make floor and ceiling points next farther striation levels 
        yPosList.Insert(0, yPosList[0] - striationHeight);
        yPosList.Add(yPosList[yPosList.Count-1] + striationHeight);
        

        List<float> zPosList = new List<float>();
        float center = (ceilingPos.y + floorPos.y) / 2 - absoluteElevRef;
        int count = 0;
        foreach (float yPos in yPosList)
        {
            float y = Mathf.Abs(center - yPos);

            float z = zPos;
            if (count == 0 || count == yPosList.Count - 1 || y == radius)
            {
                z = zPos;
            }
            else
            {
                z = zPos + Mathf.Sqrt(radius * radius - y * y) + SelectBumpiness(0.4f);
            }    
            zPosList.Add(z);
            count++;
        }

        for (int i = 0; i < yPosList.Count; i++)
        {
            Vector3 pos;
            if (i == 0 || i == yPosList.Count - 1)
            {
                pos = new Vector3(currentPosX, yPosList[i], zPosList[i]);
            }
            else
            {
                pos = new Vector3(currentPosX, yPosList[i] + SelectBumpiness(0.1f), zPosList[i]);
            }
            
            nodesList.Add(pos);

            //Debug.Log("Node added: " + nodesList[i]);
        }

        return nodesList;
    }

    void CreateTriangles(out List<int> triangles, out List<Vector3> vertsMod, List<Vector3> vertsOrig, List<int> nodeCounts, List<int> bottomState, List<int> topState, bool flipTris)
    {
        triangles = new List<int>();
        vertsMod = new List<Vector3>(vertsOrig);
        int refNodeA = 0; //Lowest node at current index
        int refNodeB = refNodeA + nodeCounts[1]; //Lowest node at next index
        int refNodeAmod = refNodeA; //
        int refNodeBmod = refNodeB;
        int squaresNeeded = nodeCounts[0] - 1;

        //'i' is index of each column of nodes (horizontal index)
        //'j' is index of each row of nodes (vertical index)
        for (int i = 0; i < nodeCounts.Count - 1; i++)
        {
            //Debug.Log("i: " + i + " refNodeA: " + refNodeA + " refNodeB: " + refNodeB);
            
            int triEffect = 0;
 
            if (topState[i + 1] == 1 && bottomState[i + 1] == -1)
            {
                triEffect = 0;
            }
            else if (topState[i + 1] == -1 && bottomState[i + 1] == -1)
            {
                triEffect = -1;
            }
            else if (topState[i + 1] == 1 && bottomState[i + 1] == 1)
            {
                triEffect = -1;
            }
            else
            {
                triEffect = 0;
            }
            
            squaresNeeded = Mathf.Min(nodeCounts[i], nodeCounts[i+1]) - 1 + triEffect;
            
            //squaresNeeded = Mathf.Min(nodeCounts[i], nodeCounts[i + 1]) - Mathf.Abs(bottomState[i + 1]) - Mathf.Abs(topState[i + 1]);

            //squaresNeeded += topState[i + 1]

            int vertIndex = vertsMod.Count;

            if (!flipTris)  //If tunnel going right
            {
                //Make single triangle at bottom if necessary
                if (bottomState[i + 1] == -1)
                {
                    triangles.Add(refNodeA);
                    triangles.Add(refNodeB + 1);
                    triangles.Add(refNodeB);
                    refNodeBmod += 1;
                }
                else if (bottomState[i + 1] == 1)
                {
                    triangles.Add(refNodeA);
                    triangles.Add(refNodeA + 1);
                    triangles.Add(refNodeB);
                    refNodeAmod += 1;
                }

                //Create 2 normal triangles for each square that's needed 
                for (int j = 0; j < squaresNeeded; j++)
                {
                    int ind1 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeAmod + j]);
                    triangles.Add(ind1);
                    vertIndex++;

                    int ind2 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeAmod + j + 1]);
                    triangles.Add(ind2);
                    vertIndex++;

                    int ind3 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeBmod + j]);
                    triangles.Add(ind3);
                    vertIndex++;


                    triangles.Add(ind2);

                    int ind4 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeBmod + j + 1]);
                    triangles.Add(ind4);
                    vertIndex++;

                    triangles.Add(ind3);
                }

                //Make single triangle at top if necessary
                if (topState[i + 1] == -1)
                {
                    triangles.Add(refNodeA + nodeCounts[i] - 1);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 1);
                    triangles.Add(refNodeA + nodeCounts[i] - 2);
                }
                else if (topState[i + 1] == 1)
                {
                    triangles.Add(refNodeA + nodeCounts[i] - 1);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 1);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 2);
                }
            }

            else //If tunnel is going left
            {
                //Make single triangle at bottom if necessary
                if (bottomState[i + 1] == -1)
                {
                    triangles.Add(refNodeA);
                    triangles.Add(refNodeB);
                    triangles.Add(refNodeB + 1);
                    refNodeBmod += 1;
                }
                else if (bottomState[i + 1] == 1)
                {
                    triangles.Add(refNodeA);
                    triangles.Add(refNodeB);
                    triangles.Add(refNodeA + 1);
                    refNodeAmod += 1;
                }

                //Create 2 normal triangles for each square that's needed 
                for (int j = 0; j < squaresNeeded; j++)
                {
                    int ind1 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeAmod + j]);
                    triangles.Add(ind1);
                    vertIndex++;

                    int ind3 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeBmod + j]);
                    triangles.Add(ind3);
                    vertIndex++;

                    int ind2 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeAmod + j + 1]);
                    triangles.Add(ind2);
                    vertIndex++;

                    triangles.Add(ind2);
                    triangles.Add(ind3);

                    int ind4 = vertIndex;
                    vertsMod.Add(vertsMod[refNodeBmod + j + 1]);
                    triangles.Add(ind4);
                    vertIndex++;
                   
                }

                //Make single triangle at top if necessary
                if (topState[i + 1] == -1)
                {
                    triangles.Add(refNodeA + nodeCounts[i] - 1);
                    triangles.Add(refNodeA + nodeCounts[i] - 2);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 1);
                }
                else if (topState[i + 1] == 1)
                {
                    triangles.Add(refNodeA + nodeCounts[i] - 1);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 2);
                    triangles.Add(refNodeB + nodeCounts[i + 1] - 1);
                }
            }

            refNodeA = refNodeB;
            refNodeB += nodeCounts[i+1];

            refNodeAmod = refNodeA;
            refNodeBmod = refNodeB;
        }

        publicTriangles = new List<int>(triangles); //For debugging
    }

    public void PlaceBackground(Vector3 floorStartPos, Vector3 ceilingStartPos, List<Vector3> floorEndPoints, List<float> floorSlopes, List<Vector3> ceilingEndPoints, List<float> ceilingSlopes, int horizDir, int biomeInd, GameObject parentObj) 
    {

        List<float> striationElevations = new List<float>();
        striationElevations = CreateMasterStriationList(floorStartPos, ceilingStartPos, floorEndPoints, ceilingEndPoints);

        GameObject backObj = new GameObject("Background");
        backObj.AddComponent<MeshFilter>();
        backObj.AddComponent<MeshRenderer>();
        Mesh mesh = backObj.GetComponent<MeshFilter>().mesh;
        //backObj.layer = (int)backgroundLayer;
        backObj.isStatic = true;

        //List<int> nodeCounts = new List<int>();
        //List<int> bottomTriangleState = new List<int>();
        //List<int> topTriangleState = new List<int>();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        backObj.transform.position = floorStartPos;
        backObj.transform.SetParent(parentObj.transform);

        //Vector3 floorNode = floorStartPos;
        //Vector3 ceilingNode = ceilingStartPos;
        //float radius = (ceilingNode.y - floorNode.y)/2;
        //Vector3 relFloorPos = new Vector3(0f, 0f, 0f);
        //Vector3 relCeilingPos = new Vector3(0f, radius * 2, 0f);

        float xDist = pieceLength / nodesPerLength;

        float xPos = 0f;
        Vector3 floorPos = floorStartPos;
        Vector3 ceilingPos = ceilingStartPos;

        //Add start point to the end point list. List now contains every point for floor and ceiling
        floorEndPoints.Insert(0, floorStartPos);
        ceilingEndPoints.Insert(0, ceilingStartPos);
        floorSlopes.Add(0f);    //Pad the end so that slope list is same length as endpoint list
        ceilingSlopes.Add(0f);    //Pad the end so that slope list is same length as endpoint list

        Vector3 floorNode = floorStartPos;
        Vector3 ceilingNode = ceilingStartPos;
        Vector3 prevFloorNode = floorNode;
        Vector3 prevCeilingNode = ceilingNode;


        for (int endPtIndex = 0; endPtIndex < floorEndPoints.Count; endPtIndex++)
        {
            floorPos = floorEndPoints[endPtIndex];
            ceilingPos = ceilingEndPoints[endPtIndex];

            for (int xIndex = 0; xIndex < nodesPerLength; xIndex++)
            {
                List<Vector3> nodes = new List<Vector3>();

                floorPos.y += floorSlopes[endPtIndex] * xDist;
                ceilingPos.y += ceilingSlopes[endPtIndex] * xDist;
                //Slope list will be one index ahead of end point index
                //end point list length = slope list length -1
                //At each index the slope is for the piece following that endpoint

                nodes = CreateNodesBetween(floorPos, ceilingPos, striationElevations, xPos, floorStartPos.y, floorStartPos.x);
                nodeCounts.Add(nodes.Count);

                floorNode = nodes[0];
                ceilingNode = nodes[nodes.Count-1];

                if (floorNode.y == prevFloorNode.y)
                {
                    bottomTriangleState.Add(0);
                }
                else if (floorNode.y < prevFloorNode.y)
                {
                    bottomTriangleState.Add(-1);
                }
                else
                {
                    bottomTriangleState.Add(1);
                }

                if (ceilingNode.y == prevCeilingNode.y)
                {
                    topTriangleState.Add(0);
                }
                else if (ceilingNode.y < prevCeilingNode.y)
                {
                    topTriangleState.Add(-1);
                }
                else
                {
                    topTriangleState.Add(1);
                }

                foreach (Vector3 node in nodes)
                {
                    verts.Add(node);
                }

                xPos += xDist * horizDir;  //Advance to next x position
                prevFloorNode = floorNode;
                prevCeilingNode = ceilingNode;
            }
        }

        //Use markers for debug only
        //int ct = 0;
        //foreach (Vector3 vert in verts)
        //{
        //    GameObject marker = (GameObject)Instantiate(markerPrefab, vert, Quaternion.identity);
        //    marker.GetComponentInChildren<TextMesh>().text = ct.ToString();
        //    ct++;
        //}

        bool flip = false;

        if (horizDir == -1)
        {
            flip = true;
        }

        newVerts = new List<Vector3>();

        CreateTriangles(out tris, out newVerts, verts, nodeCounts, bottomTriangleState, topTriangleState, flip);

        mesh.vertices = newVerts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ApplyBiomeMaterial(backObj, biomeInd);
	}

    float SelectBumpiness(float maxBumpiness)
    {
        float bump = 0f;
        bump = Random.Range(-maxBumpiness, maxBumpiness);

        return bump;
    }	

    void ApplyBiomeMaterial(GameObject obj, int biomeInd)
    {
        Material biomeMat = createWorld.biomeMaterials[biomeInd];
        Renderer[] rendArray = obj.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in rendArray)
        {
            rend.material = biomeMat;
        }
    }
}
