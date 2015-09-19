using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateShaftBackground : MonoBehaviour {

    //Create curved background for shaft section
    float striationWidth = 0.4f;
    float striationAngle = 0f;

    float pieceLength = 1f;
    int nodesPerLength = 3;
    float zPos = 0.5f;
    [SerializeField]
    int nodesAcrossCount;   //Assume this is constant for entire shaft
    [SerializeField]
    int yPosCount;

    [SerializeField]
    List<Vector3> verts = new List<Vector3>();
    [SerializeField]
    List<int> tris = new List<int>();

    CreateWorld2 createWorld;

	// Use this for initialization
	void Awake () 
    {
        createWorld = GameObject.Find("WorldCreator").GetComponent<CreateWorld2>();
        
	}

    public void PlaceBackground(Vector3 startPos, int horizDir, int vertDir, int shaftLength, float shaftWidth, int biomeInd)
    {

        this.gameObject.AddComponent<MeshFilter>();
        this.gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        
        this.gameObject.isStatic = true;
        this.gameObject.transform.position = startPos;
        
        float yDist = pieceLength / nodesPerLength;
        float yPosition = 0f;
        float xPosition = 0f;

        yPosCount = shaftLength * nodesPerLength + 1;

        float padding = 0.3f;

        float leftSide = xPosition - padding;
        float rightSide = xPosition + shaftWidth + padding;

        float radius = Mathf.Abs(rightSide - leftSide) / 2;

        List<float> xPosList = new List<float>();
        List<float> zPosList = new List<float>();
        
        //Make X positions vector
        int xIndex = 0;
        float xPos = leftSide;
        while (xPos < rightSide)
        {
            if (xIndex == 0)
            {
                xPosList.Add(xPos); //Add two nodes for end points
            }
            else
            {
                xPosList.Add(xPos); //Add four nodes for end points
                xPosList.Add(xPos);
            }
            xPos += striationWidth;
            //Debug.Log("xPos " + xPos);
            xIndex++;
            //Debug.Log("xIndex " + xIndex);
            nodesAcrossCount = xIndex + 1;
        }
        xPosList.Add(rightSide); //Add two nodes for end points

        //Make Z positions vector
        float center = shaftWidth / 2;
        int count = 0;
        foreach (float xPt in xPosList)
        {
            float x = Mathf.Abs(center - xPt);

            float z = zPos;
            if (count == 0 || count == xPosList.Count - 1 || x == radius)
            {
                z = zPos;
            }
            else
            {
                z = zPos + Mathf.Sqrt(radius * radius - x * x);
            }
            zPosList.Add(z);
            count++;
        }

        //Create nodes at each height
        for (int yIndex = 0; yIndex < yPosCount; yIndex++)
        {
            yPosition = yIndex * yDist;
            
            List<Vector3> nodes = new List<Vector3>();
            nodes = CreateNodesBetween(xPosList, yPosition, zPosList);
            
            foreach (Vector3 node in nodes)
            {
                verts.Add(node);
            }
        }

        tris = CreateTriangles(yPosCount);
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ApplyBiomeMaterial(this.gameObject, biomeInd);

    }

    List<Vector3> CreateNodesBetween(List<float> xList, float yPos, List<float> zList)
    {
        List<Vector3> nodesList = new List<Vector3>();
        List<float> xListMod = new List<float>(xList);
        List<float> zListMod = new List<float>(zList);

        int listLength = xListMod.Count;
        Debug.Log(listLength);
        //Add random variation to all middle nodes. Keep end nodes the same
        for (int k = 1; k < listLength - 2; k = k + 2)
        {
            float xBump = SelectBumpiness(0.1f);
            float zBump = SelectBumpiness(0.3f);

            xListMod[k] += xBump;
            zListMod[k] += zBump;
            xListMod[k + 1] += xBump;
            zListMod[k + 1] += zBump;
        }

        //Repeat lists because we need two rows of nodes at each ht
        for (int j = 0; j < listLength ; j++)
        {
            xListMod.Add(xListMod[j]);
            zListMod.Add(zListMod[j]);
        }

        for (int i = 0; i < xListMod.Count; i++)
        {
            Vector3 pos;
            if (i == 0 || i == xListMod.Count - 1)
            {
                pos = new Vector3(xListMod[i], yPos, zListMod[i]);
            }
            else
            {
                pos = new Vector3(xListMod[i], yPos, zListMod[i]);
            }

            nodesList.Add(pos);
        }

        return nodesList;
    }

    List<int> CreateTriangles(int length)
    {
        List<int> triangles = new List<int>();

        int extNodes = 2 + (nodesAcrossCount - 2) * 2;  //Since multiple nodes are added for inner nodes, use the extended node count for indexing

        int refA;   //Left most index on lower side of quad
        int refB;   //Left most index on upper side of quad

        for (int i = 0; i < length - 1; i++)    //Going up
        {
            refA = extNodes + extNodes * (i * 2);    //Left most index on lower side of quad
            refB = refA + extNodes; //Left most index on upper side of quad

            for (int j = 0; j < nodesAcrossCount - 1; j++)  //Going across
            {
                int ind1 = refA;
                triangles.Add(ind1);

                int ind2 = refB;
                triangles.Add(ind2);

                int ind3 = refA + 1;
                triangles.Add(ind3);
                /////
                triangles.Add(ind2);

                int ind4 = ind2 + 1;
                triangles.Add(ind4);

                triangles.Add(ind3);

                refA += 2;
                refB += 2;
                //Debug.Log("j = " + j);
            }
            //Debug.Log("length " + length);
            //Debug.Log("i = " + i);
            
        }
         
        return triangles;
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
