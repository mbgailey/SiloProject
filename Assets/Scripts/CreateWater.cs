using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateWater : MonoBehaviour {
    TideController tideController;
    public GameObject waterPrefab;

    float edgeBuffer = 0.3f;

	// Use this for initialization
	void Start () {
        tideController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TideController>();
	}

    public GameObject SetupWaterObjects()
    {
        GameObject waterGroup;
        GameObject parentObj;
        WaterController waterController;

        parentObj = new GameObject("WaterParent");
        waterController = parentObj.AddComponent<WaterController>();
        tideController.AllWaterControllers.Add(waterController);

        foreach (float globalWaterElevation in tideController.waterMeshElevList)
        {
            waterGroup = new GameObject("WaterGroup");
            waterGroup.transform.SetParent(parentObj.transform);
            waterController.waterObjList.Add(waterGroup);
        }

        return parentObj;
    }


	public GameObject CreateTunnelWater (Vector3 floorStartPos, Vector3 ceilingStartPos, List<Vector3> floorEndPoints, List<float> floorSlopes, List<Vector3> ceilingEndPoints, List<float> ceilingSlopes, int horizDir, GameObject parentObj) {

        GameObject waterObj;
        Transform waterGroup;
        //GameObject parentObj;
        //WaterController waterController;

        bool startFreeSurface = false;
        bool endFreeSurface = false;
        bool startedFreeSurface = false;

        List<Vector2[]> waterQuads = new List<Vector2[]>();
        List<Vector2> waterSurface = new List<Vector2>();

        //Add start point to the end point list. List now contains every point for floor and ceiling
        floorEndPoints.Insert(0, floorStartPos);
        ceilingEndPoints.Insert(0, ceilingStartPos);
        floorSlopes.Add(0f);    //Pad the end so that slope list is same length as endpoint list
        ceilingSlopes.Add(0f);    //Pad the end so that slope list is same length as endpoint list

        Vector3 floorStart;
        Vector3 floorEnd;
        Vector3 ceilingStart;
        Vector3 ceilingEnd;

        //Debug.Log("floor list count: " + floorEndPoints.Count);

        //parentObj = new GameObject("WaterParent");   
        WaterController waterController = parentObj.GetComponent<WaterController>();
        //tideController.AllWaterControllers.Add(waterController);

        int meshIndex = 0;
        foreach (float globalWaterElevation in tideController.waterMeshElevList)
        {
            waterQuads.Clear();
            waterSurface.Clear();
            waterGroup = parentObj.transform.GetChild(meshIndex);
            //waterGroup.transform.SetParent(parentObj.transform);
            //waterController.waterObjList.Add(waterGroup);

            for (int i = 0; i < floorEndPoints.Count - 1; i++)
            {
                //Debug.Log("floor i: " + i);
                floorStart = floorEndPoints[i];
                floorEnd = floorEndPoints[i + 1];
                ceilingStart = ceilingEndPoints[i];
                ceilingEnd = ceilingEndPoints[i + 1];

                //Keep track of water area using line positions from above
                //If any part of the floor elevation is below the water line then we need to make water at this location

                //Vector4 quad = new Vector4(floorStart.y, ceilingStart.y, floorEnd.y, ceilingEnd.y);
                if (floorStart.y < globalWaterElevation || floorEnd.y < globalWaterElevation)
                {
                    bool startWater = false;
                    bool endWater = false;

                    int numberOfQuads = 1;

                    Vector2 waterBottomStart = floorStart;
                    Vector2 waterBottomEnd = floorEnd;
                    Vector2 waterTopStart = ceilingStart;
                    Vector2 waterTopEnd = ceilingEnd;

                    //Add buffer to top and bottom to overlap with terrain pieces
                    waterBottomStart.y -= edgeBuffer;
                    waterBottomEnd.y -= edgeBuffer;
                    waterTopStart.y += edgeBuffer;
                    waterTopEnd.y += edgeBuffer;

                    Vector2 waterBottomStart2 = waterBottomStart;
                    Vector2 waterBottomEnd2 = waterBottomEnd;
                    Vector2 waterTopStart2 = waterTopStart;
                    Vector2 waterTopEnd2 = waterTopEnd;



                    if (i == 0 && ceilingStart.y > globalWaterElevation && floorStart.y < globalWaterElevation)
                    {
                        startFreeSurface = true;
                    }

                    //If ceiling piece starts below water line and goes above
                    if (ceilingStart.y < globalWaterElevation && ceilingEnd.y > globalWaterElevation)
                    {
                        startFreeSurface = true;
                        numberOfQuads = 2;
                        //First Quad
                        waterTopEnd.y = globalWaterElevation;
                        waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(globalWaterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y)) * horizDir;
                        waterBottomEnd.x = waterTopEnd.x;
                        //waterBottomEnd.y = /////Need to finish

                        //Second Quad
                        waterBottomStart2.x = waterTopStart2.x = waterTopEnd.x;
                        waterTopStart2.y = waterTopEnd2.y = globalWaterElevation;
                        waterBottomStart.y = waterBottomEnd.y;


                    }
                    //If ceiling piece starts above water line and goes below
                    else if (ceilingStart.y > globalWaterElevation && ceilingEnd.y < globalWaterElevation)
                    {
                        endFreeSurface = true;
                        numberOfQuads = 2;
                        //First Quad
                        waterTopStart.y = waterTopEnd.y = globalWaterElevation;
                        waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(globalWaterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y)) * horizDir;
                        waterBottomEnd.x = waterTopEnd.x;
                        //waterBottomEnd.y = /////Need to finish
                        //Second Quad
                        waterBottomStart2.x = waterTopStart2.x = waterTopEnd.x;
                        waterTopStart2.y = globalWaterElevation;
                        waterBottomStart.y = waterBottomEnd.y;
                    }

                    //If both ends of ceiling piece are above the water line
                    else if (ceilingStart.y > globalWaterElevation && ceilingEnd.y > globalWaterElevation)
                    {
                        waterTopStart.y = Mathf.Min(ceilingStart.y, globalWaterElevation);    //Don't allow water to be above water elevation
                        waterTopEnd.y = Mathf.Min(ceilingEnd.y, globalWaterElevation);
                    }
                    //If floor piece starts above water line and goes below
                    if (floorStart.y > globalWaterElevation && floorEnd.y < globalWaterElevation)
                    {
                        startWater = true;
                        startFreeSurface = true;
                        waterBottomStart.x = floorEnd.x - (Mathf.Abs(globalWaterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y)) * horizDir;
                        waterTopStart.x = waterBottomStart.x;
                        //waterTopStart.y = waterElevation;
                    }
                    //If floor piece starts below water line and goes above
                    else if (floorStart.y < globalWaterElevation && floorEnd.y > globalWaterElevation)
                    {
                        endWater = true;
                        endFreeSurface = true;
                        waterBottomEnd.x = floorEnd.x - (Mathf.Abs(globalWaterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y)) * horizDir;
                        waterTopEnd.x = waterBottomStart.x;
                        waterTopEnd.y = globalWaterElevation;
                    }

                    //waterTopStart.y = Mathf.Min(ceilingStart.y, waterElevation);    //Don't allow water to be above water elevation
                    //waterTopEnd.y = Mathf.Min(ceilingEnd.y, waterElevation);

                    if (startWater)
                    {
                        waterQuads.Clear();
                        waterSurface.Clear();
                    }

                    Vector2[] quad = new Vector2[4];
                    quad[0] = waterBottomStart;
                    quad[1] = waterTopStart;
                    quad[2] = waterBottomEnd;
                    quad[3] = waterTopEnd;
                    waterQuads.Add(quad);

                    if (numberOfQuads == 2)
                    {
                        Vector2[] quad2 = new Vector2[4];
                        quad2[0] = waterBottomStart2;
                        quad2[1] = waterTopStart2;
                        quad2[2] = waterBottomEnd2;
                        quad2[3] = waterTopEnd2;
                        waterQuads.Add(quad2);
                    }
                    //Debug.Log("Quad0 " + quad[0] + " Quad1 " + quad[1] + " Quad2 " + quad[2] + " Quad3 " + quad[3]);

                    if (startFreeSurface)
                    {
                        if (numberOfQuads == 2)
                        {
                            waterSurface.Add(waterTopStart2);
                            //Instantiate(marker, waterTopStart2, Quaternion.identity);
                        }
                        else
                        {
                            waterSurface.Add(waterTopStart);
                            //Instantiate(marker, waterTopStart, Quaternion.identity);
                        }
                        startFreeSurface = false;
                        startedFreeSurface = true;
                    }
                    if (startedFreeSurface && endFreeSurface)
                    {
                        waterSurface.Add(waterTopEnd);
                        startFreeSurface = endFreeSurface = false; //Reset flags
                        //Instantiate(marker, waterTopEnd, Quaternion.identity);
                        startedFreeSurface = false;

                    }



                    if (endWater)
                    {
                        //If free surface hasn't been ended yet
                        if (startedFreeSurface)
                        {
                            waterSurface.Add(waterTopEnd);
                            startedFreeSurface = false;
                            startFreeSurface = endFreeSurface = false;
                        }

                        if (waterQuads.Count != 0)
                        {
                            waterObj = Instantiate(waterPrefab);
                            waterObj.GetComponent<CreateWaterMesh>().CreateWaterBody(waterQuads, waterSurface, horizDir, waterObj);
                            waterObj.transform.SetParent(waterGroup);
                        }
                    }
                }

            }

            if (waterQuads.Count != 0)
            {
                //If free surface hasn't been ended yet
                if (startedFreeSurface)
                {
                    Vector2 waterTopEnd = ceilingEndPoints[ceilingEndPoints.Count - 1];
                    waterTopEnd.y = Mathf.Min(waterTopEnd.y, globalWaterElevation);
                    waterSurface.Add(waterTopEnd);
                    startedFreeSurface = false;
                }

                waterObj = Instantiate(waterPrefab);
                waterObj.GetComponent<CreateWaterMesh>().CreateWaterBody(waterQuads, waterSurface, horizDir, waterObj);
                waterObj.transform.SetParent(waterGroup);
            }

            meshIndex++;
        }

        waterController.InitializeWater();

        return parentObj;

	}

    public void CreateShaftWater(Vector3 bottomLeftPos, float shaftHeight, float shaftWidth, int vertDir, GameObject parentObj)
    {
        GameObject waterObj;
        Transform waterGroup;
        List<Vector2[]> waterQuads = new List<Vector2[]>();
        List<Vector2> waterSurface = new List<Vector2>();

        WaterController controller = parentObj.GetComponent<WaterController>();

        int meshIndex = 0;
        foreach (float globalWaterElevation in tideController.waterMeshElevList)
        {
            waterQuads.Clear();
            waterSurface.Clear();
            waterGroup = parentObj.transform.GetChild(meshIndex);


            if (bottomLeftPos.y < globalWaterElevation)
            {
                Vector2 waterBottomStart = bottomLeftPos;
                Vector2 waterBottomEnd = bottomLeftPos;
                waterBottomEnd.x += shaftWidth;
                Vector2 waterTopStart = bottomLeftPos;
                waterTopStart.y += shaftHeight;
                Vector2 waterTopEnd = waterBottomEnd;
                waterTopEnd.y += shaftHeight;

                //Add edge buffers
                if (vertDir == 1)   //Up shaft
                {
                    waterBottomStart.y += edgeBuffer;
                    waterBottomEnd.y += edgeBuffer;
                }
                else
                {
                    waterTopStart.y -= edgeBuffer;
                    waterTopEnd.y -= edgeBuffer;
                }

                if (waterTopStart.y > globalWaterElevation)
                {
                    waterTopStart.y = globalWaterElevation;
                    waterTopEnd.y = globalWaterElevation;
                    waterSurface.Add(waterTopStart);
                    waterSurface.Add(waterTopEnd);
                }
                else //Lower top of quad slightly to avoid interference with tunnel water quad above. It causes a weird material thing otherwise
                {
                    //waterTopStart.y -= 0.05f;
                    //waterTopEnd.y -= 0.05f;
                }
                //waterBottomStart.y += 0.05f; //Lower top of quad slightly to avoid interference with tunnel water quad below. It causes a weird material thing otherwise
                //waterBottomEnd.y += 0.05f; //Maybe only need to do these two for upward shafts. Check if coverage is enough for downward shafts

                Vector2[] quad = new Vector2[4];
                quad[0] = waterBottomStart;
                quad[1] = waterTopStart;
                quad[2] = waterBottomEnd;
                quad[3] = waterTopEnd;
                waterQuads.Add(quad);
            }

            if (waterQuads.Count != 0)
            {
                waterObj = Instantiate(waterPrefab);
                waterObj.GetComponent<CreateWaterMesh>().CreateWaterBody(waterQuads, waterSurface, 1, waterObj);
                //Transform myParent = controller.waterObjList[meshIndex].transform;
                waterObj.transform.SetParent(waterGroup);
            }

            meshIndex++;
        }
    }

}
