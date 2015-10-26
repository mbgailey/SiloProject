using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateWaterMesh : MonoBehaviour {

    public Material waterMaterial;
    //public Material waterSurfaceMaterial;
    public GameObject waterSidePrefab;
    public GameObject waterTopPrefab;
    //public GameObject waterChurnParticles;
    [SerializeField]
    List<Vector3> troubleShootVerts = new List<Vector3>();

    WaterController waterController;

    float frontSurfZ = -0.4f;
    float backSurfZ = 3f;

    public float globalWaterElevation = -2.0f;


	// Use this for initialization
    void Start()
    {
        //gameObject.AddComponent<MeshFilter>();
        //gameObject.AddComponent<MeshRenderer>();
        //Mesh mesh = GetComponent<MeshFilter>().mesh;
        //mesh.Clear();
        //mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        //mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
        //mesh.triangles = new int[] { 0, 1, 2 };
        //mesh.RecalculateNormals();
        //gameObject.GetComponent<Renderer>().material = waterMaterial;
    }


    public void CreateWaterBody(List<Vector2[]> quadList, List<Vector2> surfaceList, int direction, GameObject parentObj) //Quadlist is list of y coordinates for each section
    {

        if (quadList.Count == 0)
        {
            return;
        }
        
        //Create water front surface
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        
        GameObject waterObj = (GameObject)Instantiate(waterSidePrefab);
        waterObj.AddComponent<MeshFilter>();
        waterObj.AddComponent<MeshRenderer>();
        Mesh mesh = waterObj.GetComponent<MeshFilter>().mesh;
        //mesh.Clear();

        waterObj.transform.parent = parentObj.transform;
        waterController = parentObj.AddComponent<WaterController>();

        float xCoord = quadList[0][0].x;  //Initialize as the first x coordinate in the quad list
        int i = 0;

        //Debug.Log("xCoord" + xCoord);

        foreach (Vector2[] quad in quadList)
        {
            //Debug.Log("quad" + quad);
            
            if (i == 0)
            {
                //Add leftmost verts
                verts.Add(new Vector3(quad[0].x, quad[0].y, frontSurfZ));
                verts.Add(new Vector3(quad[1].x, quad[1].y, frontSurfZ));
                //uvs.Add(new Vector2(i, i)); //Might want to redo UVs to scale to correct height
                //uvs.Add(new Vector2(i, i+1));
                normals.Add(Vector3.back);
                normals.Add(Vector3.back);
            }

            xCoord += 1f * direction;

            verts.Add(new Vector3(quad[2].x, quad[2].y, frontSurfZ));
            verts.Add(new Vector3(quad[3].x, quad[3].y, frontSurfZ));
            //uvs.Add(new Vector2(i + 1, i)); //Might want to redo UVs to scale to correct height
            //uvs.Add(new Vector2(i + 1, i + 1));
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);

            //if (i == 2)
            //{
            //    Debug.Log("VERTS1 " + verts[0]);
            //    Debug.Log("VERTS2 " + verts[1]);
            //    Debug.Log("VERTS3 " + verts[2]);
            //    Debug.Log("VERTS4 " + verts[3]);
            //    Debug.Log("VERTS5 " + verts[4]);
            //    Debug.Log("VERTS6 " + verts[5]);
            //    Debug.Log("VERTS7 " + verts[6]);
            //    Debug.Log("VERTS8 " + verts[7]);
            //    //Debug.Log("VERTS9 " + verts[8]);
            //}

            i++;
            
        }

        //Debug.Log("quadList length " + quadList.Count);
        //Debug.Log("verts length " + verts.Count);
        bool flipNormals = false;
        for (int j = 0; j < verts.Count - 2; j++)
        {
            
            if (direction == 1)
            {
                if (!flipNormals)
                {
                    tris.Add(j);
                    tris.Add(j + 1);
                    tris.Add(j + 2);
                }
                else
                {
                    tris.Add(j);
                    tris.Add(j + 2);
                    tris.Add(j + 1);
                }
            }
            else
            {
                if (!flipNormals)
                {
                    tris.Add(j);
                    tris.Add(j + 2);
                    tris.Add(j + 1);
                }
                else
                {
                    tris.Add(j);
                    tris.Add(j + 1);
                    tris.Add(j + 2);
                }
            }

            flipNormals = !flipNormals;

        }

        //Debug.Log("tris length " + tris.Count);
        //Debug.Log("tris last " + tris[tris.Count-1]);
        //Debug.Log("tris list " + tris);

        mesh.vertices = verts.ToArray();
        //ret.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        //ret.normals = normals.ToArray();
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        waterObj.GetComponent<Renderer>().material = waterMaterial;
        //Debug.Log("mesh length: " + mesh.GetTriangles(0).ToString());

        CreateTopSurfaces(surfaceList, parentObj, direction);

	}

    void CreateTopSurfaces(List<Vector2> surfaceList, GameObject parentObj, int direction)
    {
        int surfaceCount = surfaceList.Count / 2;   //Surface list should come with pairs of vector2s defining the top water surface

        //Debug.Log("SurfaceCount " + surfaceCount);
        
        //Create top water surfaces
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        for (int k = 0; k < surfaceList.Count; k = k + 2)
        {
            verts = new List<Vector3>();
            tris = new List<int>();
            uvs = new List<Vector2>();
            normals = new List<Vector3>();
            
            GameObject waterSurfObj = (GameObject)Instantiate(waterTopPrefab);
            //waterSurfObj.AddComponent<MeshFilter>();
            //waterSurfObj.AddComponent<MeshRenderer>();
            Mesh surfMesh = waterSurfObj.GetComponent<MeshFilter>().mesh;
            surfMesh.Clear();

            waterSurfObj.transform.parent = parentObj.transform;

            verts.Add(new Vector3(surfaceList[k].x, surfaceList[k].y, frontSurfZ));
            verts.Add(new Vector3(surfaceList[k].x, surfaceList[k].y, backSurfZ));
            //uvs.Add(new Vector2(k + 1, k)); //Might want to redo UVs to scale to correct height
            //uvs.Add(new Vector2(k + 1, k + 1));
            //normals.Add(Vector3.up);
            //normals.Add(Vector3.up);

            verts.Add(new Vector3(surfaceList[k + 1].x, surfaceList[k + 1].y, frontSurfZ));
            verts.Add(new Vector3(surfaceList[k + 1].x, surfaceList[k + 1].y, backSurfZ));
            //uvs.Add(new Vector2(k + 1, k)); //Might want to redo UVs to scale to correct height
            //uvs.Add(new Vector2(k + 1, k + 1));
            //normals.Add(Vector3.up);
            //normals.Add(Vector3.up);

            if (direction == 1)
            {
                tris.Add(0); tris.Add(1); tris.Add(2);
                tris.Add(2); tris.Add(1); tris.Add(3);
            }
            else
            {
                tris.Add(0); tris.Add(2); tris.Add(1);
                tris.Add(2); tris.Add(3); tris.Add(1);
            }
            surfMesh.vertices = verts.ToArray();
            surfMesh.triangles = tris.ToArray();
            //surfMesh.RecalculateNormals();
            surfMesh.RecalculateBounds();

            BoxCollider collider = waterSurfObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            LineRenderer surfLine = waterSurfObj.GetComponent<LineRenderer>();
            surfLine.SetPosition(0, new Vector3(surfaceList[k].x, surfaceList[k].y, frontSurfZ));
            surfLine.SetPosition(1, new Vector3(surfaceList[k + 1].x, surfaceList[k + 1].y, frontSurfZ));


            //waterSurfObj.GetComponent<Renderer>().material = waterSurfaceMaterial;
        }

        troubleShootVerts = new List<Vector3>(verts);

    }

}
