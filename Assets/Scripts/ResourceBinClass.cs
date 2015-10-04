using UnityEngine;
using System.Collections;

public class ResourceBinClass : MonoBehaviour {

    public GameObject resourceObj;
    public int totalResources = 3;
    public bool renewable = false;
    public float renewCycleTime = 60f;
    public float spawnRangeX = 0.75f;
    public float spawnRangeZ = 0.2f;

    Vector3[] spawnLocations;
    GameObject[] resourceObjList;
    bool[] resPresentList;

    int resourceCount = 0;

	// Use this for initialization
	void Awake () {
        spawnLocations = new Vector3[totalResources];
        resourceObjList = new GameObject[totalResources];
        resPresentList = new bool[totalResources];
        //InitializeBin();
	}

    public void InitializeBin()
    {
        //Set spawn points and spawn resources
        for (int i = 0; i < totalResources; i++)
        {
            float xLoc = Random.Range(-spawnRangeX, spawnRangeX);
            float zLoc = Random.Range(-spawnRangeZ, spawnRangeZ);
            spawnLocations[i] = new Vector3(xLoc, 0f, zLoc);

            GameObject res = (GameObject)Instantiate(resourceObj);
            ResourceBehavior resBehavior = res.GetComponent<ResourceBehavior>();

            
            resBehavior.InitializeProps(i);
            resourceObjList[i] = res;
            resPresentList[i] = true;

            res.transform.parent = this.transform;
            res.transform.localPosition = spawnLocations[i];

            resourceCount++;
        }

    }

    public void ResourceHarvested(int resID)
    {
        resourceCount--;
        resPresentList[resID] = false;
        if (resourceCount <= 0)
        {
            this.GetComponent<Light>().enabled = false; //Specific to phosphorescent resources
        }
    }
	
}
