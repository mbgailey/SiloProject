using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldResources : MonoBehaviour {

    public enum resourceType { blueMushroom, greenMushroom, ironOre };
    public GameObject[] ResourceBinPrefabs = new GameObject[1];
    public float[] chanceToSpawn = new float[1];
    float offset = 0.2f;


	// Use this for initialization
	void Start () {
	
	}
	
	public void PopulateTunnel (GameObject tunnelObj, resourceType resType, int tunnelDirection) 
    {
	    Transform floor = tunnelObj.transform.FindChild("FloorPieces");

        GameObject Resources = new GameObject("Resources");
        Resources.transform.SetParent(tunnelObj.transform);

        foreach (Transform child in floor)
        {
            float roll = Random.Range(0f, 1f);
            if (roll < chanceToSpawn[(int)resType])
            {
                float angle = child.gameObject.GetComponent<CreatePieceMesh>().angle * tunnelDirection;
                //Vector3 position = child.position;
                GameObject bin = (GameObject)Instantiate(ResourceBinPrefabs[(int)resType], child.position, Quaternion.Euler(0f, 0f, angle));
                bin.transform.Translate(new Vector3(0f, offset)); //Move to slightly above floor
                bin.transform.SetParent(Resources.transform);
                bin.GetComponent<ResourceBinClass>().InitializeBin();

                
            }
        }
    }
}
