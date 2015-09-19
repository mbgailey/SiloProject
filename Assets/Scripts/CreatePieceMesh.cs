using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatePieceMesh : MonoBehaviour {

    public float angle = 0f;
    public Vector3 startPos = new Vector3(0f, 0f, 0f);
    public float pieceLength = 1f;
    public int xDirection = 1; //1 = left to right, -1 = right to left
    public int yDirection = -1; // -1 = floor piece, 1 = ceiling piece
    public int numberTris = 5;  //Number of tris on front face
    public float startThickness = 0.2f;
    public float endThickness = 0.4f;

    public float floorDepth = 2f;
    public int numberfloorTris = 3;
    float floorSpacing;

    float xSpacing;
    float angleRad;
    float slope; //Rise over run
    float secondarySlope; //Rise over run

    float frontSurfZ = -0.5f;
    float backEdgeZ = 1.5f;

    public float maxBumpiness = 0.05f;

    public Material material;
    public Material curtainMaterial;

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    //List<Vector2> uvs = new List<Vector2>();
    //List<Vector3> normals = new List<Vector3>();

    List<Vector3> floorVerts = new List<Vector3>();
    List<int> floorTris = new List<int>();    
    List<Vector3> curtainVerts = new List<Vector3>();
    List<int> curtainTris = new List<int>(); 

	// Use this for initialization
	public void InitializePiece (float ang, Vector3 start, int xDir, int yDir, float startThick, float endThick, float depth, Material biomeMat) 
    {
        angle = ang;
        startPos = start;
        xDirection = xDir; //1 = left to right, -1 = right to left
        yDirection = yDir; // -1 = floor piece, 1 = ceiling piece
        startThickness = startThick;
        endThickness = endThick;
        floorDepth = depth;
        
        
        xSpacing = pieceLength / (numberTris - 1) * xDirection;
        angleRad = angle * Mathf.Deg2Rad;
        slope = Mathf.Tan(angleRad);
        secondarySlope = ((slope * pieceLength + endThickness * yDirection) - startThickness * yDirection) / pieceLength;  //Slope of non collider side of the piece. Varies depending on thickness change
        
        floorSpacing = floorDepth / numberfloorTris;
        material = biomeMat;
        

        CreateMeshes();
        CreateColliders(yDir);
    }

    void CreateColliders(int yDirection)
    {
        BoxCollider coll = gameObject.GetComponentInChildren<BoxCollider>();

        float length = pieceLength / Mathf.Cos(angleRad);
        coll.size = new Vector3(length, 0.1f, floorDepth);
        coll.center = new Vector3(0f, 0.05f * yDirection, 0.5f);
        float colliderAngle = angle * xDirection;
        coll.transform.localRotation = Quaternion.Euler(0f, 0f, colliderAngle);
    }

    void CreateCurtain()
    {
    //Create black mesh on the off side of pieces so that other things are blocked
        GameObject curtain = new GameObject("Curtain");
        curtain.AddComponent<MeshFilter>();
        curtain.AddComponent<MeshRenderer>();
        Mesh curtainMesh = curtain.GetComponent<MeshFilter>().mesh;
        

        List<Vector3> tempList = new List<Vector3>(curtainVerts);
        int edgeVertCount = tempList.Count;

        foreach (Vector3 secVert in tempList)
        {
            Vector3 backPos = secVert;
            backPos.z = backEdgeZ;
            curtainVerts.Add(backPos);
        }

        bool switchNormals;

        if (xDirection == -1 && yDirection == 1)
        {
            switchNormals = true;
        }
        else if (xDirection == -1 && yDirection == -1)
        {
            switchNormals = false;
        }
        else if (xDirection == 1 && yDirection == 1)
        {
            switchNormals = false;
        }
        else
        {
            switchNormals = true;
        }

        for (int j = 0; j < edgeVertCount - 1; j++)
        {
            if (!switchNormals)
            {
                curtainTris.Add(j);
                curtainTris.Add(j + edgeVertCount);
                curtainTris.Add(j + 1);

                curtainTris.Add(j + 1);
                curtainTris.Add(j + edgeVertCount);
                curtainTris.Add(j + 1 + edgeVertCount);
            }
            else
            {
                curtainTris.Add(j);
                curtainTris.Add(j + 1);
                curtainTris.Add(j + edgeVertCount);

                curtainTris.Add(j + 1);
                curtainTris.Add(j + 1 + edgeVertCount);
                curtainTris.Add(j + edgeVertCount);
            }
        }

        curtainMesh.vertices = curtainVerts.ToArray();
        curtainMesh.triangles = curtainTris.ToArray();

        curtainMesh.RecalculateNormals();
        curtainMesh.RecalculateBounds();

        curtain.transform.position = this.transform.position;
        curtain.transform.SetParent(this.transform);
        curtain.gameObject.GetComponent<MeshRenderer>().material = curtainMaterial;

    }


	void CreateMeshes () 
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        //mesh.subMeshCount = 2;

        //Create first two verts
        Vector3 pos1 = startPos;    //Primary edge (one with collider)
        pos1.z = frontSurfZ;

        Vector3 pos0 = pos1;
        pos0.y += yDirection * startThickness;  //Secondary edge
        
        verts.Add(pos0);
        verts.Add(pos1);
        curtainVerts.Add(pos0);

        for (int i = 1; i < numberTris - 1; i = i + 2)
        {
            //Secondary face vert (would be bottom for floor piece or top side for ceiling piece)
            Vector3 posSecondary = pos0;
            posSecondary.x = pos0.x + i * xSpacing + SelectBumpiness();
            posSecondary.y = pos0.y + secondarySlope * Mathf.Abs(posSecondary.x - pos0.x) + SelectBumpiness();
            posSecondary.z = pos0.z + SelectBumpiness();
            verts.Add(posSecondary);
            curtainVerts.Add(posSecondary);

            if (i != numberTris - 2)
            {
                //Primary face vert 
                Vector3 posPrimary = pos1;
                posPrimary.x = pos1.x + (i + 1) * xSpacing;
                posPrimary.y = pos1.y + slope * Mathf.Abs(posPrimary.x - pos1.x);
                posPrimary.z = pos1.z + SelectBumpiness();
                verts.Add(posPrimary);
            }
        }

        //Final primary face vert 
        Vector3 posFin1 = pos1;
        posFin1.x = pos1.x + pieceLength * xDirection;
        posFin1.y = pos1.y + slope * + pieceLength;
        verts.Add(posFin1);

        //Final secondary face vert
        Vector3 posFin0 = pos0;
        posFin0.x = pos0.x + pieceLength * xDirection;
        posFin0.y = pos0.y + secondarySlope * pieceLength;
        verts.Add(posFin0);
        curtainVerts.Add(posFin0);

        bool switchNormals;

        if (xDirection == -1 && yDirection == 1)
        {
            switchNormals = false;
        }
        else if (xDirection == -1 && yDirection == -1)
        {
            switchNormals = true;
        }
        else if (xDirection == 1 && yDirection == 1)
        {
            switchNormals = true;
        }
        else
        {
            switchNormals = false;
        }

        for (int j = 0; j < verts.Count - 2; j++)
        {
            if (!switchNormals)
            {
                tris.Add(j);
                tris.Add(j + 1);
                tris.Add(j + 2);
                switchNormals = true;
            }
            else
            {
                tris.Add(j);
                tris.Add(j + 2);
                tris.Add(j + 1);
                switchNormals = false;
            }
        }

        int frontVertCount = verts.Count;
        //Debug.Log("tris length " + tris.Count);
        //Debug.Log("tris last " + tris[tris.Count-1]);
        //Debug.Log("tris list " + tris);
        Vector3[] primaryEdge = new Vector3[(numberTris + 1) / 2];
        int edgeInd = 0;
        int vertInd = 0;
        bool indEven = true;
        foreach (Vector3 vert in verts)
        {
            if (!indEven)   //Primary edge will be all odd indices
            {
                primaryEdge[edgeInd] = verts[vertInd];
                edgeInd++;
            }
            if (indEven) indEven = false;
            else indEven = true;
            vertInd++;
        }

        CreateTopVerts(primaryEdge);
        CreateTopTris(primaryEdge);

        //Combine verts and tris arrays for vertical and horizontal surfaces

        foreach (Vector3 v in floorVerts)
        {
            verts.Add(v);
        }
        foreach (int t in floorTris)
        {
            tris.Add(t);
        }

        mesh.vertices = verts.ToArray();
        //mesh.normals = normals.ToArray();
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetComponent<MeshRenderer>().material = material;

        CreateCurtain();

	}

    void CreateTopVerts(Vector3[] primaryEdge)
    {
        int j = 0;
        foreach (Vector3 pt in primaryEdge)
        {
            for (int i = 1; i < numberfloorTris + 1; i++)
            {
                Vector3 pos = pt;
                pos.z = frontSurfZ + i * floorSpacing;
                if (j == 0 || j == primaryEdge.Length-1) //If this is a point on the left or right edge
                {
                    //don't apply any bumpiness
                }
                else
                {
                    pos.y = pt.y + SelectBumpiness();
                }
                floorVerts.Add(pos);
            }
            j++;
        }
    }

    void CreateTopTris(Vector3[] primaryEdge)
    {
        int frontVertCount = verts.Count;

        bool switchNormals = false;

        if (xDirection == -1 && yDirection == 1)
        {
            switchNormals = false;
        }
        else if (xDirection == -1 && yDirection == -1)
        {
            switchNormals = true;
        }
        else if (xDirection == 1 && yDirection == 1)
        {
            switchNormals = true;
        }
        else
        {
            switchNormals = false;
        }

        bool frontEdge = true;

        for (int i = 0; i < primaryEdge.Length - 1; i++)
        {

            for (int j = 0; j < numberfloorTris; j++)
            {
                if (j == 0)    //First triangle of each column is connected to front edge
                {
                    frontEdge = true;
                }
                else
                {
                    frontEdge = false;
                }

                //LeftSide Tri
                if (frontEdge)
                {
                    floorTris.Add(i * 2 + 1);
                    if (switchNormals)
                    {
                        floorTris.Add((i + 1) * 2 + 1);
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                    }
                    else
                    {
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                        floorTris.Add((i + 1) * 2 + 1);
                    }
                }
                else
                {
                    floorTris.Add(frontVertCount + i * numberfloorTris + (j - 1));
                    if (switchNormals)
                    {
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + (j - 1));
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                    }
                    else
                    {
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + (j - 1));
                    }
                }

                //RightSide Tri
                if (frontEdge)
                {
                    floorTris.Add((i + 1) * 2 + 1);
                    if (switchNormals)
                    {
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + j);
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                    }
                    else
                    {
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + j);
                    }
                }
                else
                {
                    floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + (j - 1));
                    if (switchNormals)
                    {
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + j);
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                    }
                    else
                    {
                        floorTris.Add(frontVertCount + i * numberfloorTris + j);
                        floorTris.Add(frontVertCount + (i + 1) * numberfloorTris + j);
                    }
                }
            }
        }
    }

    float SelectBumpiness()
    {
        float bump = 0f;
        bump = Random.Range(-maxBumpiness, maxBumpiness);

        return bump;
    }	
}
