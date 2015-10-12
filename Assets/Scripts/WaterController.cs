using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterController : MonoBehaviour {

    public List<GameObject> waterObjList = new List<GameObject>();
    TideController tideController;
    GameObject activeWater;

	// Use this for initialization
	void Start () {
        tideController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TideController>();
        //waterObjList = new GameObject[tideController.meshCount];
	}

    public void InitializeWater()
    {
        foreach (GameObject obj in waterObjList)
        {
            obj.SetActive(false);
        }
        waterObjList[0].SetActive(true);
        activeWater = waterObjList[0];
    }
	
	public void SetTideIndex (int tideIndex) 
    {
        activeWater.SetActive(false);
        waterObjList[tideIndex].SetActive(true);

        activeWater = waterObjList[tideIndex];
	}

    public void MoveWaterBy(float elev)
    {
        activeWater.transform.Translate(0f, elev, 0f);
    }
}
