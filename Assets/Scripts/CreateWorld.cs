using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateWorld : MonoBehaviour {

    ////Tunnel Variables and Objects////
    public GameObject[] floorPieces = new GameObject[6];
    public GameObject[] ceilingPieces = new GameObject[6];
    public float[] yOffsets = new float[6];
    float clearanceWidth = 0f; //Used to account for clearance between piece sections. 0.2 if needed
    GameObject lastFloor;
    GameObject lastCeiling;
    public LayerMask terrainLayer;
    enum TunnelTendency { up, down, normal };

    ////Shaft Variables and Objects////
    public GameObject[] leftShaftPieces = new GameObject[1];
    public GameObject[] rightShaftPieces = new GameObject[1];
    public float[] yShaftOffsets = new float[2];
    bool shaftUp = false;
    bool shaftDown = false;
    int shaftCooldown = 3;  //Shafts can't be generated within this many iterations of eachother
    
    ////Branch Variables////
    int branchCooldown = 3;  //Branches can't be generated within this many iterations of eachother
    int branchDir = 0;
    int branchCount = 0;

    ////Biome Variables and Objects////
    public Material[] biomeMaterials = new Material[3];


    ////Behavior Variables////
    // Set general procedural characteristics//

    //Branch Characteristics
    float branchFrequency = 0.1f;    //Set chance to create a branchfloat branchFrequency = 0.1f;    //Set chance to create a branch
    Vector3 tunnelTendDist = new Vector3(0.10f, 0.6f, 1.0f); //Set distribution of tunnels between up and down and normal. Set as breakpoints between 0 and 1
    int maxBranches = 20;
    float tunnelHeight = 1f;
    int minTunnelLength = 50;
    int maxTunnelLength = 400;

    //Shaft characteristics
    float shaftFrequency = 0.1f;    //Set chance to create a shaft
    int maxShaftWidth = 1;
    int minShaft = 2;
    int maxShaft = 15;

    public GameObject marker;
    public GameObject line;

    CreateBackground createBackground;
    CreateWaterMesh createWater;

    float waterElevation;

	// Use this for initialization
	void Start () {
        createBackground = this.GetComponent<CreateBackground>();
        createWater = this.GetComponent<CreateWaterMesh>();
        waterElevation = createWater.globalWaterElevation;

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
        int floorInd = 2;   //Should be flat/neutral piece index
        int ceilingInd = 2;
        Vector3 floorPos;
        Vector3 ceilingPos;
        Vector3 prevFloorPos;
        Vector3 prevCeilingPos;

        floorPos = startPos;
        floorPos.x += 0.5f * direction;
        ceilingPos = floorPos;
        ceilingPos.y += 1f;

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
            prevFloorPos = floorPos;
            prevCeilingPos = ceilingPos;

            //Determine start position for next floor piece
            floorPos.y += yOffsets[floorInd] * direction;   //This is y position of end point for previous piece
            //Determine start position for next ceiling piece
            ceilingPos.y += yOffsets[ceilingInd] * direction;   //Account for offset of last piece that was placed. Gives y position at end of piece

            if (!shaftDown)
            {
                //Select next piece type
                if (i == 0)
                {
                    floorInd = 2;
                }
                else
                {
                    floorInd = SelectPieceIndex(vertTend);
                    floorPos.y += yOffsets[floorInd] * direction;   //Account for offset of next piece. Should now be y pos of where center of next piece should go
                }
            }

            else //If shaftDown
            {
                floorInd = 0;
            }


            if (!shaftUp)
            {
                //Select next piece type
                if (i == 0)
                {
                    ceilingInd = 2;
                    ceilingPos = floorPos;
                    ceilingPos.y += tunnelHeight;
                }
                else
                {
                    float yPosTest = 0;
                    bool passed = false;
                    int iter = 0;
                    while (!passed)    //Enforce that ceiling must be at least minimum height above floor
                    {
                        yPosTest = ceilingPos.y;
                        ceilingInd = SelectPieceIndex(vertTend);
                        yPosTest += yOffsets[ceilingInd] * direction; //Account for offset of next piece. Should now be y pos of where center of next piece should go


                        float ht = yPosTest + yOffsets[ceilingInd] * direction - (floorPos.y + yOffsets[floorInd] * direction); //Calculate clearance height

                        if (ht > 0.9f || iter >= 20)
                        {
                            passed = true;
                            if (iter == 20) //If solution not reached, there might not be a solution. Next ceiling piece should be angled up as much as possible to try to create enough space between floor and ceiling
                            {
                                Debug.Log("Passed on iter " + iter + "; Height of " + ht);
                                ceilingInd = 0;
                                yPosTest = ceilingPos.y;
                                yPosTest += yOffsets[ceilingInd] * direction;
                            }
                        }
                        iter++;
                    }
                    ceilingPos.y = yPosTest;
                }
            }

            else //If shaftUp
            {
                ceilingInd = 0;
            }

            //For constant height tunnel
            //ceilingPos = floorPos;
            //ceilingPos.y += tunnelHeight;
            //ceilingInd = floorInd;          
            
            //First check for interference
            //Check for overlap before placing anything

            Vector3 floorStart = floorPos; //Line 1 spans floor piece
            floorStart.x -= (0.5f - clearanceWidth/2) * direction;  //Account for clearance of previous piece (previous vertical pieces can get collided with otherwise) 
            floorStart.y -= yOffsets[floorInd] * direction + clearanceWidth / 2; //Use outside edge accounting for clearance
            Vector3 floorEnd = floorPos;
            floorEnd.x += (0.5f + clearanceWidth / 2) * direction;
            floorEnd.y += yOffsets[floorInd] * direction - clearanceWidth / 2;

            Vector3 ceilingStart = ceilingPos; //Line 2 spans ceiling piece
            ceilingStart.x -= (0.5f - clearanceWidth/2) * direction;
            ceilingStart.y -= yOffsets[ceilingInd] * direction - clearanceWidth / 2;
            Vector3 ceilingEnd = ceilingPos;
            ceilingEnd.x += (0.5f + clearanceWidth / 2) * direction;
            ceilingEnd.y += yOffsets[ceilingInd] * direction + clearanceWidth / 2;

            //Debug Lines
            //GameObject ln = GameObject.Instantiate(line);
            //ln.GetComponent<LineRenderer>().SetPosition(0, floorStart);
            //ln.GetComponent<LineRenderer>().SetPosition(1, floorEnd);
            //ln.GetComponent<LineRenderer>().SetPosition(2, ceilingEnd);
            //ln.GetComponent<LineRenderer>().SetPosition(3, ceilingStart);


            if (CheckForOverlap(floorStart, floorEnd, ceilingStart, ceilingEnd, floorEnd, ceilingEnd))
            {
                Debug.Log("Found interference with tunnel placement #" + branchNum);
                
                goto EndTunnelActions;

            }

            
            //Keep track of water area using line positions from above
            //Vector4 quad = new Vector4(floorStart.y, ceilingStart.y, floorEnd.y, ceilingEnd.y);
            if (floorStart.y < waterElevation || floorEnd.y < waterElevation)
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

                if (ceilingStart.y < waterElevation && ceilingEnd.y > waterElevation)
                {
                    startFreeSurface = true;
                    numberOfQuads = 2;
                    //First Quad
                    waterTopEnd.y = waterElevation;
                    waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(waterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y));
                    waterBottomEnd.x = waterTopEnd.x;
                    //waterBottomEnd.y = /////Need to finish

                    //Second Quad
                    waterBottomStart2.x = waterTopStart2.x = waterTopEnd.x;
                    waterTopStart2.y = waterTopEnd2.y = waterElevation;
                    waterBottomStart.y = waterBottomEnd.y;
                    

                }
                else if (ceilingStart.y > waterElevation && ceilingEnd.y < waterElevation)
                {
                    endFreeSurface = true;
                    numberOfQuads = 2;
                    //First Quad
                    waterTopStart.y = waterTopEnd.y = waterElevation;
                    waterTopEnd.x = ceilingEnd.x - (Mathf.Abs(waterElevation - ceilingEnd.y) * Mathf.Abs(ceilingEnd.x - ceilingStart.x) / Mathf.Abs(ceilingEnd.y - ceilingStart.y));
                    waterBottomEnd.x = waterTopEnd.x;
                    //waterBottomEnd.y = /////Need to finish
                    //Second Quad
                    waterBottomStart2.x = waterTopStart2.x = waterTopEnd.x;
                    waterTopStart2.y = waterElevation;
                    waterBottomStart.y = waterBottomEnd.y;
                }

                else if (ceilingStart.y > waterElevation && ceilingEnd.y > waterElevation)
                {
                    waterTopStart.y = Mathf.Min(ceilingStart.y, waterElevation);    //Don't allow water to be above water elevation
                    waterTopEnd.y = Mathf.Min(ceilingEnd.y, waterElevation);
                }

                if (floorStart.y > waterElevation && floorEnd.y < waterElevation)
                {
                    startWater = true;
                    waterBottomStart.x = floorEnd.x - (Mathf.Abs(waterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y));
                    waterTopStart.x = waterBottomStart.x;
                    //waterTopStart.y = waterElevation;
                }
                else if (floorStart.y < waterElevation && floorEnd.y > waterElevation)
                {
                    endWater = true;
                    endFreeSurface = true;
                    waterBottomEnd.x = floorEnd.x - (Mathf.Abs(waterElevation - floorEnd.y) * Mathf.Abs(floorEnd.x - floorStart.x) / Mathf.Abs(floorEnd.y - floorStart.y));
                    waterTopEnd.x = waterBottomStart.x;
                    waterTopEnd.y = waterElevation;
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
                    GameObject nothingObj2 = new GameObject();
                    createWater.CreateWaterBody(waterQuads, waterSurface, direction, nothingObj2);
                }
            }

            if (shaftDown)
            {
                floorPos = prevFloorPos;
            }
            if (shaftUp)
            {
                ceilingPos = prevCeilingPos;
            }

            //Place Background
            Vector3 bottomPos = floorPos;
            Vector3 topPos = ceilingPos;
            int bottomInd = floorInd;
            int topInd = ceilingInd;

            if (shaftDown)
            {
                bottomPos.y += yOffsets[floorInd] * direction; //Account for offset of last piece that was placed
                bottomInd = 2;
                Instantiate(marker, bottomPos, Quaternion.identity);
            }

            if (shaftUp)
            {
                topPos.y += yOffsets[ceilingInd] * direction; //Account for offset of last piece that was placed
                topInd = 2;
                Instantiate(marker, topPos, Quaternion.identity);
            }

            createBackground.CoverHeight(bottomPos, topPos, bottomInd, topInd, biomeInd);


            //Create floor if not doing a shaft
            if (!shaftDown)
            {
                //Place floor
                lastFloor = (GameObject)Instantiate(floorPieces[floorInd], floorPos, Quaternion.identity);
                Renderer[] rendArray = lastFloor.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in rendArray)
                {
                    rend.material = biomeMat;
                }
            }
            else //Create an down shaft
            {
                Vector3 shaftStart = floorPos;
                shaftStart.x -= 0.5f * direction;
                shaftStart.y += yOffsets[floorInd] * direction; //Account for offset of last piece that was placed
                //Debug.Log("StartPos " + shaftStart);
                GenerateShaft(shaftStart, -1, direction, biomeInd);
            }

            //Create ceiling if not doing a shaft
            if (!shaftUp)
            {
                //Place ceiling
                lastCeiling = (GameObject)Instantiate(ceilingPieces[ceilingInd], ceilingPos, Quaternion.identity);
                Renderer[] rendArray = lastCeiling.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in rendArray)
                {
                    rend.material = biomeMat;
                }
            }

            else //Create an up shaft
            {
                Vector3 shaftStart = ceilingPos;
                shaftStart.x -= 0.5f * direction;
                shaftStart.y += yOffsets[ceilingInd] * direction; //Account for offset of last piece that was placed
                GenerateShaft(shaftStart, 1, direction, biomeInd);
            }

            //Place background pieces
            //float highestElev = ceilingPos.y + Mathf.Abs(yOffsets[ceilingInd]);
            //float lowestElev = floorPos.y - Mathf.Abs(yOffsets[floorInd]);
            //Vector3 lowestPoint = floorPos;
            //lowestPoint.y = lowestElev;
            //float coverHeight = highestElev - lowestElev;

            floorPos.x += 1f * direction;
            ceilingPos.x += 1f * direction;
            
        }
    EndTunnelActions:
        GameObject nothingObj = new GameObject();
        createWater.CreateWaterBody(waterQuads, waterSurface, direction, nothingObj);
        
        return;
	}

    //Generate a vertical shafts
    void GenerateShaft(Vector3 startPos, int vertDir, int horizDir, int biomeInd)
    {
        //Initialize
        bool branchAllowed = false;
        int branchCoolTimer = 0;

        int leftShaftInd = 0;
        int rightShaftInd = 0;
        Vector3 leftShaftPos = new Vector3();
        Vector3 rightShaftPos = new Vector3();

        Material biomeMat = biomeMaterials[biomeInd];
        
        //Determine shaft characteristics
        int shaftLength = Random.Range(minShaft, maxShaft+1);
        int shaftWidth = Random.Range(1, maxShaftWidth + 1);

        for (int i = 0; i < shaftLength; i++)
        {
            //Determine shaft positions
            if (horizDir == 1)
            {
                leftShaftPos = startPos;
                leftShaftPos.y += (0.5f + i) * vertDir;
                rightShaftPos = leftShaftPos;
                rightShaftPos.x += shaftWidth * horizDir;
            }
            else
            {
                rightShaftPos = startPos;
                rightShaftPos.y += (0.5f + i) * vertDir;
                leftShaftPos = rightShaftPos;
                leftShaftPos.x += shaftWidth * horizDir;
            }

            //Check for overlap before placing anything
            Vector3 lineStart1 = leftShaftPos;
            lineStart1.y -= 0.4f * vertDir;
            Vector3 lineEnd1 = lineStart1;
            lineEnd1.y += 1.1f * vertDir;
            Vector3 lineStart2 = rightShaftPos;
            lineStart2.y -= 0.4f * vertDir;
            Vector3 lineEnd2 = lineStart2;
            lineEnd2.y += 1.1f * vertDir;

            Vector3 lineStart3 = leftShaftPos;      //3rd line is in center of shaft
            float avgXPos = (leftShaftPos.x + rightShaftPos.x)/2;
            lineStart3.x = avgXPos;
            lineStart3.y -= 0.4f * vertDir;
            Vector3 lineEnd3 = lineStart3;
            lineEnd3.y += 1.1f * vertDir;

            if (CheckForOverlap(lineStart1, lineEnd1, lineStart2, lineEnd2, lineStart3, lineEnd3))
            {
                Debug.Log("Found interference with shaft placement");
                //Rollback to previous positions
                i--;
                //Determine shaft positions
                if (horizDir == 1)
                {
                    leftShaftPos = startPos;
                    leftShaftPos.y += (0.5f + i) * vertDir;
                    rightShaftPos = leftShaftPos;
                    rightShaftPos.x += shaftWidth * horizDir;
                }
                else
                {
                    rightShaftPos = startPos;
                    rightShaftPos.y += (0.5f + i) * vertDir;
                    leftShaftPos = rightShaftPos;
                    leftShaftPos.x += shaftWidth * horizDir;
                }

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
            float middleX = (leftShaftPos.x + rightShaftPos.x) / 2f;
            Vector3 backgroundPos = leftShaftPos;
            backgroundPos.x = middleX;

            createBackground.CoverShaft(backgroundPos, biomeInd);

            //Place left side
            if (branchDir != -1)    //If not branching left, place shaft side
            {
                GameObject thisObj = (GameObject)Instantiate(leftShaftPieces[leftShaftInd], leftShaftPos, Quaternion.identity);
                Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in rendArray)
                {
                    rend.material = biomeMat;
                }
            }
            else                    //If branching
            {
                Vector3 tunnelPos = leftShaftPos;   //Determine tunnel floor starting position
                tunnelPos.y -= 0.5f * vertDir;
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
                GameObject thisObj = (GameObject)Instantiate(rightShaftPieces[rightShaftInd], rightShaftPos, Quaternion.identity);
                Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in rendArray)
                {
                    rend.material = biomeMat;
                }
            }
            else                    //If branching
            {
                Vector3 tunnelPos = rightShaftPos;
                tunnelPos.y -= 0.5f * vertDir;
                if (vertDir == -1)
                {
                    tunnelPos.y -= 1f;  //Need to go one more unit to get to the floor position if this is a down shaft
                }
                GenerateTunnel(tunnelPos, branchDir, SelectTunnelTendency());
            }
            
        }
        
        EndShaftActions:
            //Place shaft end
            int ind = 2;    //Use flat pieces for end
            for (int i = 0; i < shaftWidth; i++)
            {
                Vector3 endPos = leftShaftPos;
                //endPos.x += 0.5f * i + 1;
                endPos.x += 0.5f;
                endPos.y += 0.5f * vertDir;
            
                if (vertDir == 1)   //If up shaft then end will be ceiling piece
                {
                    GameObject thisObj = (GameObject)GameObject.Instantiate(ceilingPieces[ind], endPos, Quaternion.identity);
                    Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer rend in rendArray)
                    {
                        rend.material = biomeMat;
                    }
                }
                else //If down shaft then end will be floor pieces
                {
                    GameObject thisObj = (GameObject)GameObject.Instantiate(floorPieces[ind], endPos, Quaternion.identity);
                    Renderer[] rendArray = thisObj.gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer rend in rendArray)
                    {
                        rend.material = biomeMat;
                    }
                }
            }
    
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
    int SelectPieceIndex(TunnelTendency tend)
    {
        int ind = 0;
        switch (tend)
        {
            case TunnelTendency.up: //For up tunnel, more likely to select index 0 or 1
                if (Random.Range(0f, 1f) < 0.25)    //Ex. If value is set to 0.25, there is a 75% chance that we go to the else statement and select an upward sloping tunnel
                {
                    ind = Random.Range(2, 5);
                }
                else
                {
                    ind = Random.Range(0, 2);
                }
                break;

            case TunnelTendency.down:   //For up tunnel, more likely to select index 3 or 4
                if (Random.Range(0f, 1f) < 0.25)    
                {
                    ind = Random.Range(0, 3);
                }
                else
                {
                    ind = Random.Range(3, 5);
                }
                break;

            default:       //For normal tunnel, all pieces have equal chance of being selected
                ind = Random.Range(0, 5);
                break;
        }

        return ind;
    }

    int SelectBiomeMaterial()
    {
        int index = 0;

        index = Random.Range(0,3);

        return index;
    }
}
