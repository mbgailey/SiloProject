using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterController : MonoBehaviour {

    public List<GameObject> waterObjList = new List<GameObject>();
    TideController tideController;
    GameObject activeWater;

    Vector3[] tideInPosList;
    Vector3[] tideOutPosList;


	// Use this for initialization
	void Awake () {
        tideController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TideController>();
        //waterObjList = new GameObject[tideController.meshCount];
	}

    public void InitializeWater()
    {
        tideInPosList = new Vector3[waterObjList.Count];
        tideOutPosList = new Vector3[waterObjList.Count];

        int i = 0;
        foreach (GameObject obj in waterObjList)
        {
            obj.SetActive(false);
            Vector3 pos = obj.transform.position;
            tideInPosList[i] = pos;
            pos.y -= tideController.deltaElev;
            tideOutPosList[i] = pos;
            i++;
        }
        waterObjList[0].SetActive(true);
        activeWater = waterObjList[0];
    }
	
	public void SetTideIndexOut (int tideIndex) 
    {
        SetLowPosition(tideIndex);
        activeWater.SetActive(false);
        waterObjList[tideIndex].SetActive(true);

        activeWater = waterObjList[tideIndex];
	}

    public void SetTideIndexIn(int tideIndex)
    {
        SetHighPosition(tideIndex);
        activeWater.SetActive(false);
        waterObjList[tideIndex].SetActive(true);

        activeWater = waterObjList[tideIndex];
    }

    public void MoveWaterBy(float elev)
    {
        activeWater.transform.Translate(0f, elev, 0f);
    }

    void SetLowPosition(int ind)
    {
        activeWater.transform.position = tideOutPosList[ind];
    }

    void SetHighPosition(int ind)
    {
        activeWater.transform.position = tideInPosList[ind];
    }
}
