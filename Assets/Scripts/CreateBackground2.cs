using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateBackground2 : MonoBehaviour {

    float defaultDepth = 0.22f;
    float pieceHeight = 1f;
    float pieceLength = 1.002f;
    int nodesPerHeight = 5;
    int nodesPerLength = 3;
    float zPos = 1.2f;
    //float edgePadding = 0.05f;

    float maxBumpiness = 0.1f;

    CreateWorld2 createWorld;

    public LayerMask backgroundLayer;

	// Use this for initialization
	void Awake () {
        createWorld = this.GetComponent<CreateWorld2>();
	}
	
	public void PlaceBackground (Vector3 floorPos, Vector3 ceilingPos, float floorAngle, float ceilingAngle, int horizDir, int biomeInd, GameObject parentObj) 
    {
        floorPos.y -= 0.15f; //Give some extra coverage to not allow spaces between parts
        ceilingPos.y += 0.15f;
        floorPos.x -= 0.001f * horizDir;

        GameObject backObj = new GameObject("Background");
        backObj.AddComponent<MeshFilter>();
        backObj.AddComponent<MeshRenderer>();
        Mesh mesh = backObj.GetComponent<MeshFilter>().mesh;
        //backObj.layer = (int)backgroundLayer;
        backObj.isStatic = true;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();


        backObj.transform.position = floorPos;
        backObj.transform.SetParent(parentObj.transform);

        float heightToCover = ceilingPos.y - floorPos.y;
        int nodesUp = (int)heightToCover * nodesPerHeight;
        //Debug.Log("nodesUp " + nodesUp);
        float xDist = pieceLength/ nodesPerLength;
        float yDist = heightToCover / nodesUp;
        //Debug.Log("xDist " + xDist + "  yDist " + yDist);
        Vector3 pos = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < nodesPerLength + 1; i++)
        {
            float xPos = xDist * i * horizDir;
            float yStartPos = Mathf.Tan(floorAngle * Mathf.Deg2Rad) * xPos;
            float yPos;
            float yEndPos = heightToCover + Mathf.Tan(ceilingAngle * Mathf.Deg2Rad) * xPos;
            
            for (int j = 0; j < nodesUp+1; j++)
            {
                yPos = yStartPos + yDist * j;
                yPos = Mathf.Min(yPos, yEndPos);
                if (j == nodesUp) //Last position should coincide with ceiling
                {
                    yPos = yEndPos;
                }
                if (i == 0 || i == nodesPerLength)
                {
                    pos = new Vector3(xPos, yPos + SelectBumpiness(), zPos); //Fix x and z position for edges to avoid spaces between background pieces
                }
                else
                {
                    pos = new Vector3(xPos + SelectBumpiness(), yPos + SelectBumpiness(), zPos + SelectBumpiness());
                }

                verts.Add(pos);
            }
        }

        bool leftSide = true;

        for (int n = 1; n < nodesPerLength+1; n++)
        {
            for (int m = 0; m < nodesUp; m++)
            {
                tris.Add(m);
                tris.Add(m + 1);
                tris.Add(m + n * (nodesUp +1));

                tris.Add(m + 1);
                tris.Add(m + 1 + n * (nodesUp +1));
                tris.Add(m + n * (nodesUp +1));
            }
                
               
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ApplyBiomeMaterial(backObj, biomeInd);
	}

    float SelectBumpiness()
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
