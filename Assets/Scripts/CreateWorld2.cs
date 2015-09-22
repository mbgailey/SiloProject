using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateWorld2 : MonoBehaviour {

    ////Tunnel Variables and Objects////
    public GameObject tunnelPiecePrefab;
    
    float clearanceWidth = 0.2f; //Used to account for clearance between piece sections. 0.2 if needed
    GameObject floorObj;
    GameObject ceilingObj;
    float pieceLength = 1f;
    public LayerMask terrainLayer;
    enum TunnelTendency { up, down, normal };

    ////Shaft Variables and Objects////
    bool shaftUp = false;
    bool shaftDown = false;
    int shaftCooldown = 3;  //Shafts can't be generated within this many iterations of eachother
    
    ////Branch Variables////
    int branchCooldown = 3;  //Branches can't be generated within this many iterations of eachother
    int branchDir = 0;
    int branchCount = 0;

    ////Biome Variables and Objects////
    public Material[] biomeMaterials = new Material[3];
    public GameObject waterPrefab;

    ////Behavior Variables////
    // Set general procedural characteristics//

    //Branch Characteristics
    float branchFrequency = 0.1f;    //Set chance to create a branchfloat branchFrequency = 0.1f;    //Set chance to create a branch
    Vector3 tunnelTendDist = new Vector3(0.10f, 0.6f, 1.0f); //Set distribution of tunnels between up and down and normal. Set as breakpoints between 0 and 1
    int maxBranches = 3;
    float tunnelHeight = 0.8f;
    int minTunnelLength = 20;
    int maxTunnelLength = 50;
    float maxAngle = 40f;
    float pieceThickness = 0.4f;

    //Shaft characteristics
    float shaftFrequency = 0.1f;    //Set chance to create a shaft
    int maxShaftWidth = 1;
    int minShaft = 2;
    int maxShaft = 15;

    public GameObject marker;
    public GameObject line;

    //CreateBackground5 createBackground;
    CreateShaftBackground createShaftBackground;
    //CreateWaterMesh createWater;

    float globalWaterElevation = -2.0f;

	// Use this for initialization
	void Start () {
        //createBackground = this.GetComponent<CreateBackground4>();
        createShaftBackground = this.GetComponent<CreateShaftBackground>();
        //createWater = this.GetComponent<CreateWaterMesh>();
        //waterElevation = createWater.globalWaterElevation;

        GenerateTunnel(new Vector3(0f, 0f, 0f), 1, SelectTunnelTendency());
        
	}

    bool CheckForOverlap(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, Vector3 start3, Vector3 end3)
    {
        bool overlap = false;
        bool hit1;
        bool hit2;
        bool hit3;

        hit1 = Physics.Linecast(start1, end1, terrainLayer);
        hit2 = Physics.Linecast(start2, end2, terrainLayer);
        hit3 = Physics.Linecast(start3, end3, terrainLayer);

        if (hit1 || hit2 || hit3) { overlap = true; }

        return overlap;
    }

	// Generate a mostly horizontal tunnel with some vertical shafts
	void GenerateTunnel (Vector3 startPos, int direction, TunnelTendency vertTend) {
        
        //Initialize
        float floorSlope = 0f;
        float ceilingSlope = 0f;
        float floorAngle = 0f;
        float ceilingAngle = 0f;
        Vector3 floorStartPos;
        Vector3 ceilingStartPos;
        Vector3 floorEndPos = new Vector3();
        Vector3 ceilingEndPos = new Vector3();;
        Vector3 tunnelFloorStart;
        Vector3 tunnelCeilingStart;
        List<Vector3> floorEndPoints = new List<Vector3>();
        List<Vector3> ceilingEndPoints = new List<Vector3>();
        List<float> floorSlopeList = new List<float>();
        List<float> ceilingSlopeList = new List<float>();
        GameObject Tunnel = new GameObject("Tunnel");
        GameObject FloorPieces = new GameObject("FloorPieces");
        GameObject CeilingPieces = new GameObject("CeilingPieces");
        GameObject BackgroundPieces = new GameObject("BackgroundPieces");
        FloorPieces.transform.SetParent(Tunnel.transform);
        CeilingPieces.transform.SetParent(Tunnel.transform);
        BackgroundPieces.transform.SetParent(Tunnel.transform);
        BackgroundPieces.AddComponent<CreateBackground5>();
        CreateBackground5 createBackground;
        createBackground = BackgroundPieces.GetComponent<CreateBackground5>();

        GameObject waterObj;

        floorStartPos = startPos;
        //floorPos.x += 0.5f * direction;
        ceilingStartPos = floorStartPos;
        ceilingStartPos.y += tunnelHeight;

        tunnelFloorStart = floorStartPos;
        tunnelCeilingStart = ceilingStartPos;

        List<Vector2[]> waterQuads = new List<Vector2[]>();
        List<Vector2> waterSurface = new List<Vector2>();

        bool upShaftAllowed = false;
        bool downShaftAllowed = false;
        int upCoolTimer = 0;
        int downCoolTimer = 0;
        branchCount++;

        int branchNum = branchCount;

        int biomeInd = SelectBiomeMaterial();
        Material biomeMat = biomeMaterials[biomeInd];

        bool startFreeSurface = false;
        bool endFreeSurface = false;
        bool startedFreeSurface = false;

        //if (branchCount > maxBranches)  //If max branches has been reached, don't continue this function. (Other functions currently executing should still finish)
        //    return;

        //Determine tunnel characteristics
        int tunnelLength = Random.Range(minTunnelLength, maxTunnelLength + 1);

        for (int i = 0; i < tunnelLength; i++)
        {
            
            //Chance to create a shaft
            shaftUp = false;
            shaftDown = false;
            if (upShaftAllowed)
            {
                if (Random.Range(0f, 1f) <= shaftFrequency)
                {
                    //Debug.Log("ShaftUp on iter " + i);
                    shaftUp = true;
                    upShaftAllowed = false;
                }
            }
            else
            {
                upCoolTimer++;
            }
            if (upCoolTimer > shaftCooldown)
            {
                upCoolTimer = 0;
                upShaftAllowed = true;
            }

            if (downShaftAllowed)
            {
                if (Random.Range(0f, 1f) <= shaftFrequency)
                {
                    //Debug.Log("ShaftDown on iter " + i);
                    shaftDown = true;
                    downShaftAllowed = false;
                }
            }
            else
            {
                downCoolTimer++;
            }
            if (downCoolTimer > shaftCooldown)
            {
                downCoolTimer = 0;
                downShaftAllowed = true;
            }
            
            //Determine next piece type and positions of next pieces
            floorEndPos = floorStartPos;    //Initialize
            ceilingEndPos = ceilingStartPos;
            floorEndPos.x += 1f * direction;
            ceilingEndPos.x += 1f * direction;
            //Determine start position for next floor piece
            ////floorPos.y += lastFloorSlope * pieceLength;   //This is y position of end point for previous piece
            //Determine start position for next ceiling piece
            ////ceilingPos.y += lastCeilingSlope * pieceLength;   //Account for offset of last piece that was placed. Gives y position at end of piece

            if (!shaftDown)
            {
                //Select next piece type
                if (i == 0)
                {
                    floorAngle = 0f;
                }
                else
                {
                    floorAngle = SelectNextPieceAngle(vertTend);
                    
                }
                //floorSlope = Mathf.Tan(floorAngle * Mathf.Deg2Rad);
                
            }

            else //If shaftDown
            {
                floorAngle = 0f;
            }
            floorSlope = Mathf.Tan(floorAngle * Mathf.Deg2Rad);
            floorEndPos.y += floorSlope * pieceLength;   //Account for offset of next piece. Should now be y pos of primary surface at end of this piece

            if (!shaftUp)
            {
                //Select next piece type
                if (i == 0)
                {
                    floorAngle = 0f;
                    ceilingStartPos = floorStartPos;       
                    ceilingStartPos.y += tunnelHeight;
                }
                else
                {
                    float endPosTest = 0;
                    bool passed = false;
                    int iter = 0;
                    while (!passed)    //Enforce that ceiling must be at least minimum height above floor
                    {
                        endPosTest = ceilingStartPos.y;
                        ceilingAngle = SelectNextPieceAngle(vertTend);  //Select an angle for this piece
                        ceilingSlope = Mathf.Tan(ceilingAngle * Mathf.Deg2Rad);
                        endPosTest += ceilingSlope * pieceLength; //Should now be y pos of end of ceiling piece
                        //Check where yPosTest is if having problems

                        float ht = endPosTest - floorEndPos.y; //Calculate clearance height. Should be height between end points of floor and ceiling

                        if (ht > 0.9f || iter >= 20)
                        {
                            passed = true;
                            if (iter == 20) //If solution not reached, there might not be a solution. Next ceiling piece should be angled up as much as possible to try to create enough space between floor and ceiling
                            {
                                Debug.Log("Passed on iter " + iter + "; Height of " + ht);
                                ceilingAngle = maxAngle;
                            }
                        }
                        iter++;
                    }
                    //ceilingPos.y = yPosTest;
                }
            }

            else //If shaftUp
            {
                ceilingAngle = 0f;
            }
            ceilingSlope = Mathf.Tan(ceilingAngle * Mathf.Deg2Rad);
            ceilingEndPos.y += ceilingSlope * pieceLength;

            //For constant height tunnel
            //ceilingPos = floorPos;
            //ceilingPos.y += tunnelHeight;
            //ceilingInd = floorInd;          
            
            //First check for interference
            //Check for overlap before placing anything
            
            //At this point floorPos should be end point for current/about to be placed piece. prevFloorPos should be start point for current/about to be placed piece
            
            Vector3 floorStart = floorStartPos; //Line 1 spans floor piece middle to end
            floorStart.x += 0.5f * direction; 
            floorStart.y -= clearanceWidth; //Use outside edge accounting for clearance
            Vector3 floorEnd = floorEndPos;
            floorEnd.x += clearanceWidth * direction;
            floorEnd.y -= clearanceWidth;

            Vector3 ceilingStart = ceilingStartPos; //Line 2 spans ceiling piece middle to end
            ceilingStart.x += 0.5f * direction;
            ceilingStart.y += clearanceWidth;
            Vector3 ceilingEnd = ceilingEndPos;
            ceilingEnd.x += clearanceWidth * direction;
            ceilingEnd.y += clearanceWidth;

            //Debug Lines
            //GameObject ln = GameObject.Instantiate(line);
            //ln.GetComponent<LineRenderer>().SetPosition(0, floorStart);
            //ln.GetComponent<LineRenderer>().SetPosition(1, floorEnd);
            //ln.GetComponent<LineRenderer>().SetPosition(2, ceilingEnd);
            //ln.GetComponent<LineRenderer>().SetPosition(3, ceilingStart);


            if (CheckForOverlap(floorStart, floorEnd, ceilingStart, ceilingEnd, floorEnd, ceilingEnd))
            {
                Debug.Log("Found interference with tunnel placement #" + branchNum);

                //floorEndPoints.RemoveAt(floorEndPoints.Count-1);
                //ceilingEndPoints.RemoveAt(floorEndPoints.Count - 1);
                //floorSlopeList.RemoveAt(floorEndPoints.Count - 1);
                //ceilingSlopeList.RemoveAt(floorEndPoints.Count - 1);

                floorEndPos = floorStartPos;    //Set end point back to start and end tunnel there
                ceilingEndPos = ceilingStartPos;

                goto EndTunnelActions;

            }

            
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
                Vector2 waterBottomStart2 = floorStart;
                Vector2 waterBottomEnd2 = floorEnd;
                Vector2 waterTopStart2 = ceilingStart;
                Vector2 waterTopEnd2 = ceilingEnd;

                

                if (i == 0)
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
                    waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(globalWaterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y));
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
                    waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(globalWaterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y));
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
                    waterBottomStart.x = floorEnd.x - (Mathf.Abs(globalWaterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y));
                    waterTopStart.x = waterBottomStart.x;
                    //waterTopStart.y = waterElevation;
                }
                //If floor piece starts below water line and goes above
                else if (floorStart.y < globalWaterElevation && floorEnd.y > globalWaterElevation)
                {
                    endWater = true;
                    endFreeSurface = true;
                    waterBottomEnd.x = floorEnd.x - (Mathf.Abs(globalWaterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y));
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
                    endFreeSurface = false;
                }
                


                if (endWater)
                {
                    //If free surface hasn't been ended yet
                    if (startedFreeSurface)
                    {
                        waterSurface.Add(waterTopEnd);
                        startedFreeSurface = false;
                    }
                    
                    if (waterQuads.Count != 0)
                    {
                        waterObj = Instantiate(waterPrefab);
                        waterObj.GetComponent<CreateWaterMesh>().CreateWaterBody(waterQuads, waterSurface, direction, waterObj);
                    }
                }
            }

            //if (shaftDown)
            //{
            //    ////floorPos = prevFloorPos;
            //}
            //if (shaftUp)
            //{
            //    ////ceilingPos = prevCeilingPos;
            //}

            ////Place Background
            //Vector3 bottomPos = floorStartPos;
            //Vector3 topPos = ceilingStartPos;
            //float bottomAngle = floorAngle;
            //float topAngle = ceilingAngle;

            //if (shaftDown)
            //{
            //    //bottomPos.y += yOffsets[floorInd] * direction; //Account for offset of last piece that was placed
            //    //bottomInd = 2;
            //    //Instantiate(marker, bottomPos, Quaternion.identity);
            //}

            //if (shaftUp)
            //{
            //    //topPos.y += yOffsets[ceilingInd] * direction; //Account for offset of last piece that was placed
            //    //topInd = 2;
            //    //Instantiate(marker, topPos, Quaternion.identity);
            //}

            //Create floor if not doing a shaft
            if (!shaftDown)
            {
                //Place floor
                Vector3 objLoc = new Vector3(floorStartPos.x + pieceLength / 2f * direction, (floorStartPos.y + floorEndPos.y) / 2f, 0f);
                Vector3 meshStart = new Vector3(-pieceLength / 2f * direction, (floorStartPos.y - floorEndPos.y) / 2f, 0f);
                floorObj = (GameObject)Instantiate(tunnelPiecePrefab, objLoc, Quaternion.identity);
                floorObj.GetComponent<CreatePieceMesh>().InitializePiece(floorAngle, meshStart, direction, -1, pieceThickness, pieceThickness, 2.0f, biomeMat);
                //Renderer[] rendArray = floorObj.gameObject.GetComponentsInChildren<Renderer>();
                //foreach (Renderer rend in rendArray)
                //{
                //    rend.material = biomeMat;
                //}
                floorObj.transform.SetParent(FloorPieces.transform);
            }
            else //Create a down shaft
            {
                Vector3 shaftStart = floorStartPos;
                //shaftStart.x -= 0.5f * direction;
                ////shaftStart.y += yOffsets[floorInd] * direction; //Account for offset of last piece that was placed
                //Debug.Log("StartPos " + shaftStart);
                GenerateShaft(shaftStart, -1, direction, biomeInd);
            }

            //Create ceiling if not doing a shaft
            if (!shaftUp)
            {
                //Place ceiling
                Vector3 objLoc = new Vector3(ceilingStartPos.x + pieceLength / 2f * direction, (ceilingStartPos.y + ceilingEndPos.y) / 2f, 0f);
                Vector3 meshStart = new Vector3(-pieceLength / 2f * direction, (ceilingStartPos.y - ceilingEndPos.y) / 2f, 0f);
                ceilingObj = (GameObject)Instantiate(tunnelPiecePrefab, objLoc, Quaternion.identity);
                ceilingObj.GetComponent<CreatePieceMesh>().InitializePiece(ceilingAngle, meshStart, direction, 1, pieceThickness, pieceThickness, 2.0f, biomeMat);
                //Renderer[] rendArray = ceilingObj.gameObject.GetComponentsInChildren<Renderer>();
                //foreach (Renderer rend in rendArray)
                //{
                //    rend.material = biomeMat;
                //}
                ceilingObj.transform.SetParent(CeilingPieces.transform);
            }

            else //Create an up shaft
            {
                Vector3 shaftStart = ceilingStartPos;
                //shaftStart.x -= 0.5f * direction;
                //shaftStart.y += yOffsets[ceilingInd] * direction; //Account for offset of last piece that was placed
                GenerateShaft(shaftStart, 1, direction, biomeInd);
            }

            //Add to end points list that will be used for background generation
            floorEndPoints.Add(floorEndPos);
            ceilingEndPoints.Add(ceilingEndPos);
            floorSlopeList.Add(floorSlope);
            ceilingSlopeList.Add(ceilingSlope);

            //Place background pieces
            //float highestElev = ceilingPos.y + Mathf.Abs(yOffsets[ceilingInd]);
            //float lowestElev = floorPos.y - Mathf.Abs(yOffsets[floorInd]);
            //Vector3 lowestPoint = floorPos;
            //lowestPoint.y = lowestElev;
            //float coverHeight = highestElev - lowestElev;

            //createBackground.PlaceBackground(floorStartPos, ceilingStartPos, floorAngle, ceilingAngle, direction, biomeInd, BackgroundPieces);

            floorStartPos = floorEndPos;    //Starting point for next piece will be end point for current piece
            ceilingStartPos = ceilingEndPos;
            
        }
        
        EndTunnelActions:
        if (waterQuads.Count != 0)
        {
            //If free surface hasn't been ended yet
            if (startedFreeSurface)
            {
                Vector2 waterTopEnd = ceilingEndPos;
                waterTopEnd.y = Mathf.Min(waterTopEnd.y, globalWaterElevation);
                waterSurface.Add(waterTopEnd);
                startedFreeSurface = false;
            }
            
            waterObj = Instantiate(waterPrefab);
            waterObj.GetComponent<CreateWaterMesh>().CreateWaterBody(waterQuads, waterSurface, direction, waterObj);
        }

        float endHt = ceilingEndPos.y - floorEndPos.y;
        int endPieceCount = (int)(endHt / pieceLength) + 1;

        for (int i = 0; i < endPieceCount; i++)
        {
            Vector3 objLoc = floorEndPos;
            objLoc.y += i * pieceLength + pieceLength / 2 - 0.2f;

            Vector3 meshStart = new Vector3(-pieceLength / 2, 0f, 0f); //Mesh start is relative to instantiated object

            float rotation;
            if (direction == 1)   //If up shaft then end will be ceiling piece
            {
                rotation = -90f;
            }
            else
            {
                rotation = 90f;
            }
            GameObject thisObj = (GameObject)Instantiate(tunnelPiecePrefab, objLoc, Quaternion.identity);
            thisObj.GetComponent<CreatePieceMesh>().InitializePiece(0f, meshStart, 1, 1, 0.2f, 0.2f, 2.0f, biomeMat);    //Create a 0 degree piece with direction of 1 that will be turned by 90 degrees to become vertical piece
            //Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
            //foreach (Renderer rend in rendArray)
            //{
            //    rend.material = biomeMat;
            //}
            thisObj.transform.Rotate(0f, 0f, rotation);

            thisObj.transform.SetParent(FloorPieces.transform);
        }


        createBackground.PlaceBackground(tunnelFloorStart, tunnelCeilingStart, floorEndPoints, floorSlopeList, ceilingEndPoints, ceilingSlopeList, direction, biomeInd, BackgroundPieces);

        //return;
	}

    //Generate a vertical shafts
    void GenerateShaft(Vector3 startPos, int vertDir, int horizDir, int biomeInd)
    {      
        
        //Initialize
        bool branchAllowed = false;
        int branchCoolTimer = 0;

        Vector3 leftShaftStartPos = new Vector3();
        Vector3 rightShaftStartPos = new Vector3();
        Vector3 leftShaftEndPos = new Vector3();
        Vector3 rightShaftEndPos = new Vector3();

        GameObject Shaft = new GameObject("Shaft");
        GameObject ShaftPieces = new GameObject("ShaftPieces");
        ShaftPieces.transform.SetParent(Shaft.transform);
        GameObject Background = new GameObject("Background");
        Background.transform.SetParent(Shaft.transform);
        
        Background.AddComponent<CreateShaftBackground>();
        CreateShaftBackground createBackground;
        createBackground = Background.GetComponent<CreateShaftBackground>();

        Material biomeMat = biomeMaterials[biomeInd];
        
        //Determine shaft characteristics
        int shaftLength = Random.Range(minShaft, maxShaft+1);
        int shaftWidth = Random.Range(1, maxShaftWidth + 1);

        for (int i = 0; i < shaftLength; i++)
        {
            //Determine shaft positions
            if (horizDir == 1)
            {
                leftShaftStartPos = startPos;
                leftShaftStartPos.y += pieceLength * i* vertDir;
                rightShaftStartPos = leftShaftStartPos;
                rightShaftStartPos.x += shaftWidth * horizDir;
            }
            else
            {
                rightShaftStartPos = startPos;
                rightShaftStartPos.y += pieceLength * i* vertDir;
                leftShaftStartPos = rightShaftStartPos;
                leftShaftStartPos.x += shaftWidth * horizDir;
            }

            Vector3 leftObjPos = leftShaftStartPos;
            leftObjPos.y += pieceLength/2 * vertDir;
            Vector3 rightObjPos = rightShaftStartPos;
            rightObjPos.y += pieceLength / 2 * vertDir;

            leftShaftEndPos = leftShaftStartPos;
            leftShaftEndPos.y += pieceLength * vertDir;
            rightShaftEndPos = rightShaftStartPos;
            rightShaftEndPos.y += pieceLength * vertDir;


            //Check for overlap before placing anything
            Vector3 lineStart1 = leftObjPos;
            Vector3 lineEnd1 = leftShaftEndPos;
            lineEnd1.y += clearanceWidth * vertDir;

            Vector3 lineStart2 = rightObjPos;
            Vector3 lineEnd2 = rightShaftEndPos;
            lineEnd2.y += clearanceWidth * vertDir;
            //3rd line is across shaft ends


            if (CheckForOverlap(lineStart1, lineEnd1, lineStart2, lineEnd2, lineEnd1, lineEnd2))
            {
                Debug.Log("Found interference with shaft placement");
                //Rollback to previous positions
                //i--;
                ////Determine shaft positions
                //if (horizDir == 1)
                //{
                //    leftShaftPos = startPos;
                //    leftShaftPos.y += (0.5f + i) * vertDir;
                //    rightShaftPos = leftShaftPos;
                //    rightShaftPos.x += shaftWidth * horizDir;
                //}
                //else
                //{
                //    rightShaftPos = startPos;
                //    rightShaftPos.y += (0.5f + i) * vertDir;
                //    leftShaftPos = rightShaftPos;
                //    leftShaftPos.x += shaftWidth * horizDir;
                //}

                leftShaftEndPos.y -= pieceLength * vertDir;
                rightShaftEndPos.y -= pieceLength * vertDir;

                shaftLength = i;    //Re set shaft length to current iteration and goto end actions

                goto EndShaftActions;
                
            }

            
            //Chance to create a branch
            branchDir = 0;
            if (branchCount < maxBranches) //If max branches has been reached, don't allow more branches
            {

                if (branchAllowed)
                {
                    if (Random.Range(0f, 1f) <= branchFrequency)
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            branchDir = 1;  //Branch right
                        }
                        else
                        {
                            branchDir = -1; //Branch left
                        }
                        branchAllowed = false;
                    }
                }
                else
                {
                    branchCoolTimer++;
                }
                if (branchCoolTimer > branchCooldown)
                {
                    branchCoolTimer = 0;
                    branchAllowed = true;
                }
            }


            //Place background pieces
            float middleX = (leftShaftStartPos.x + rightShaftStartPos.x) / 2f;
            Vector3 backgroundPos = leftShaftStartPos;
            backgroundPos.x = middleX;


            //Place left side
            if (branchDir != -1)    //If not branching left, place shaft side
            {
                Vector3 meshStart = new Vector3(-pieceLength / 2f, 0, 0f); //Mesh start is relative to instantiated object
                GameObject thisObj = (GameObject)Instantiate(tunnelPiecePrefab, leftObjPos, Quaternion.identity);
                thisObj.GetComponent<CreatePieceMesh>().InitializePiece(0f, meshStart, 1, -1, 0.2f, 0.2f, 2.0f, biomeMat);    //Create a 0 degree piece with direction of 1 that will be turned by 90 degrees to become vertical piece
                //Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                //foreach (Renderer rend in rendArray)
                //{
                //    rend.material = biomeMat;
                //}
                thisObj.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);    //Turn piece to make it vertical
                thisObj.transform.SetParent(ShaftPieces.transform);
            }
            else                    //If branching
            {
                Vector3 tunnelPos = leftShaftStartPos;   //Determine tunnel floor starting position

                if (vertDir == -1)
                {
                    tunnelPos.y -= 1f;  //Need to go one more unit to get to the floor position if this is a down shaft
                }
                //GameObject.Instantiate(marker, tunnelPos, Quaternion.identity);
                GenerateTunnel(tunnelPos, branchDir, SelectTunnelTendency());
            }

            //Place right side
            if (branchDir != 1)    //If not branching right, place shaft side
            {

                Vector3 meshStart = new Vector3(-pieceLength / 2f, 0, 0f); //Mesh start is relative to instantiated object
                GameObject thisObj = (GameObject)Instantiate(tunnelPiecePrefab, rightObjPos, Quaternion.identity);
                thisObj.GetComponent<CreatePieceMesh>().InitializePiece(0f, meshStart, 1, -1, 0.2f, 0.2f, 2.0f, biomeMat);    //Create a 0 degree piece with direction of 1 that will be turned by 90 degrees to become vertical piece
                //Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                //foreach (Renderer rend in rendArray)
                //{
                //    rend.material = biomeMat;
                //}
                thisObj.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);    //Turn piece to make it vertical
                thisObj.transform.SetParent(ShaftPieces.transform);
            }
            else                    //If branching
            {
                Vector3 tunnelPos = rightShaftStartPos;
                if (vertDir == -1)
                {
                    tunnelPos.y -= 1f;  //Need to go one more unit to get to the floor position if this is a down shaft
                }
                GenerateTunnel(tunnelPos, branchDir, SelectTunnelTendency());
            }
            
        }
        
        EndShaftActions:
            //Place shaft end
            for (int i = 0; i < shaftWidth; i++)
            {
                Vector3 endPos = leftShaftEndPos;
                endPos.x += pieceLength/2f + pieceLength * i * horizDir;
                endPos.y -= 0.2f * vertDir;
                int pieceVertDir;
                if (vertDir == 1)   //If up shaft then end will be ceiling piece
                {
                    pieceVertDir = 1; //Ceiling piece
                }
                else
                {
                    pieceVertDir = -1;   //Floor piece
                }
                Vector3 meshStart = new Vector3(-pieceLength / 2f * horizDir, 0, 0f); //Mesh start is relative to instantiated object
                GameObject thisObj = (GameObject)Instantiate(tunnelPiecePrefab, endPos, Quaternion.identity);
                thisObj.GetComponent<CreatePieceMesh>().InitializePiece(0f, meshStart, horizDir, pieceVertDir, 0.2f, 0.2f, 2.0f, biomeMat);    //Create a 0 degree piece with direction of 1 that will be turned by 90 degrees to become vertical piece
                //Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                //foreach (Renderer rend in rendArray)
                //{
                //    rend.material = biomeMat;
                //}

                thisObj.transform.SetParent(ShaftPieces.transform);
            }

        Vector3 backgroundStartPos = startPos;
        
        //Always start background in bottom left corner for simplicity
        if (horizDir == -1)
        {
            backgroundStartPos.x -= pieceLength * shaftWidth;
        }

        if (vertDir == -1)
        {
            backgroundStartPos.y -= pieceLength * shaftLength;
        }

        createBackground.PlaceBackground(backgroundStartPos, horizDir, vertDir, shaftLength, (float)shaftWidth * pieceLength, biomeInd);
    
    }

    //Return random tunnel tendency based on set distribution
    TunnelTendency SelectTunnelTendency()
    {
        TunnelTendency tend;
        float roll = Random.Range(0f,1f);
        if (roll <= tunnelTendDist.x)
        {
            tend = TunnelTendency.up;
        }
        else if (roll <= tunnelTendDist.y)
        {
            tend = TunnelTendency.down;
        }
        else
        {
            tend = TunnelTendency.normal;
        }

        return tend;
    }

    //Return random tunnel tendency based on set distribution
    float SelectNextPieceAngle(TunnelTendency tend)
    {
        float ang = 0f;
        switch (tend)
        {
            case TunnelTendency.up: //For up tunnel, more likely to select index 0 or 1
                if (Random.Range(0f, 1f) < 0.25)    //Ex. If value is set to 0.25, there is a 75% chance that we go to the else statement and select an upward sloping tunnel
                {
                    ang = Random.Range(0, maxAngle);
                }
                else
                {
                    ang = Random.Range(-maxAngle, 0);
                }
                break;

            case TunnelTendency.down:   //For up tunnel, more likely to select index 3 or 4
                if (Random.Range(0f, 1f) < 0.25)    
                {
                    ang = Random.Range(0, maxAngle);
                }
                else
                {
                    ang = Random.Range(-maxAngle, 0);
                }
                break;

            default:       //For normal tunnel, all pieces have equal chance of being selected
                ang = Random.Range(-maxAngle, maxAngle);
                break;
        }

        return ang;
    }

    int SelectBiomeMaterial()
    {
        int index = 0;

        index = Random.Range(0,3);

        return index;
    }
}
