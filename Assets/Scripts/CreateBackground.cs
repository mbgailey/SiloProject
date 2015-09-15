using UnityEngine;
using System.Collections;

public class CreateBackground : MonoBehaviour {

    public GameObject[] squareTiles = new GameObject[3];
    public GameObject[] angleTiles = new GameObject[5];
    public float[] elevOffsets = new float[5];
    float defaultDepth = 0.22f;
    float pieceHeight = 1f;
    //float edgePadding = 0.05f;
    CreateWorld createWorld;

	// Use this for initialization
	void Awake () {
        createWorld = this.GetComponent<CreateWorld>();
	}
	
	public void PlaceBackground (Vector3 pos, int biomeInd) {
        pos.z = defaultDepth;
        int sel = Random.Range(0,3);
        GameObject thisObj = (GameObject)Instantiate(squareTiles[sel], pos, Quaternion.Euler(180f, 0f, 0f));
        ApplyBiomeMaterial(thisObj, biomeInd);
	}

    public void PlaceScaledBackground(Vector3 pos, float yScale, int biomeInd)
    {
        pos.z = defaultDepth;
        int sel = Random.Range(0, 3);
        GameObject thisObj = (GameObject)Instantiate(squareTiles[sel], pos, Quaternion.Euler(180f, 0f, 0f));
        thisObj.transform.localScale = new Vector3(1.05f, yScale + 0.15f, 1f);
        ApplyBiomeMaterial(thisObj, biomeInd);
    }

    public void PlaceFloorAngle(Vector3 pos, int index, int biomeInd)
    {
        pos.z = defaultDepth;
        Vector3 rot = new Vector3(0f, 180f, 90f);
        GameObject thisObj = (GameObject)Instantiate(angleTiles[index], pos, Quaternion.Euler(rot));
        ApplyBiomeMaterial(thisObj, biomeInd);
    }

    public void PlaceCeilingAngle(Vector3 pos, int index, int biomeInd)
    {
        pos.z = defaultDepth;
        Vector3 rot = new Vector3(0f, 180f, -90f);
        GameObject thisObj = (GameObject)Instantiate(angleTiles[index], pos, Quaternion.Euler(rot));
        ApplyBiomeMaterial(thisObj, biomeInd);
    }

    public void CoverHeight(Vector3 floorPos, Vector3 ceilingPos, int floorInd, int ceilingInd, int biomeInd)
    {
        //float highestElev = lowestPoint.y + totalHeight;

        //int pieceCount = Mathf.RoundToInt((totalHeight - elevOffsets[floorInd] - elevOffsets[ceilingInd])/ pieceHeight);
        //Debug.Log("total ht: " + totalHeight + "pieceCount: " + pieceCount);

        //Debug.Log("floorPos" + floorPos);
        //Debug.Log("ceilingPos" + ceilingPos);

        Vector3 placePos = floorPos;
        float coverElev = floorPos.y;        //Keep track of how much elevation has been covered by background
        
        //Place floor piece background. If angled piece then place angle tile
        if (floorInd != 2)
        {
            //placePos.y += Mathf.Abs(createWorld.yOffsets[floorInd]) + elevOffsets[floorInd];
            placePos.y += elevOffsets[floorInd];
            PlaceFloorAngle(placePos, floorInd, biomeInd);
            coverElev += Mathf.Abs(createWorld.yOffsets[floorInd]);
        }

        placePos.y += pieceHeight / 2; //Next piece will be a square

        Vector3 topPos = ceilingPos;
        topPos.y -= elevOffsets[ceilingInd];
        //Place ceiling piece background 
        if (ceilingInd != 2)
        {
            PlaceCeilingAngle(topPos, ceilingInd, biomeInd);
        }

        float topElev = ceilingPos.y - Mathf.Abs(createWorld.yOffsets[ceilingInd]);
        
        //Place all square pieces
        //for (int i = 0; i < pieceCount ; i++)

        float leftToCover = Mathf.Abs(topElev - coverElev);
        //Debug.Log("topElev " + topElev);
        //Debug.Log("coverElev " + coverElev);
        //Debug.Log("leftToCover " + leftToCover);

        int iter = 0;
        while (leftToCover > 0.01f)
        {
            //Debug.Log("leftToCover " + leftToCover);
            
            if (leftToCover < 1f)
            {
                placePos.y = (topElev + coverElev) / 2;
                //Debug.Log("Placing scaled background at " + leftToCover);
                PlaceScaledBackground(placePos, leftToCover, biomeInd);
                leftToCover = 0f;
                return;
            }
            else
            {
                PlaceBackground(placePos, biomeInd);
                coverElev = placePos.y + pieceHeight / 2;
                
                placePos.y += pieceHeight;
            }
            leftToCover = Mathf.Abs(topElev - coverElev);

            //Break out of loop after x iterations to avoid endless loop
            iter++;
            if (iter > 50)
            {
                leftToCover = 0;
                return;
            }

        }

        //If needed, place one more piece to cover gap
        
        //float coveredElev = placePos.y - pieceHeight / 2;
        //if (coveredElev < topPos.y)
        //{
        //    placePos.y = topPos.y - pieceHeight / 2;
        //    PlaceBackground(placePos);
        //}
        

    }

    public void CoverShaft(Vector3 placePos, int biomeInd)
    {
        placePos.z = defaultDepth;
        int sel = Random.Range(0, 3);
        GameObject thisObj = (GameObject)Instantiate(squareTiles[sel], placePos, Quaternion.Euler(180f, 0f, 0f));
        //back.transform.localScale.Set(1.1f, 1.1f, 1f);    //Stretch background tile in the y direction slightly to cover edges
        ApplyBiomeMaterial(thisObj, biomeInd);
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
