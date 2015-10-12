using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TideController : MonoBehaviour {

    public float globalWaterElevation;
    int currentWaterIndex = 0;
    float highTideElev = -2.0f;
    float lowTideElev = -4.0f;

    float tideSpeed = 0.5f;
    public bool tideIn = true;
    public bool tideEligible = true;

    int meshCount = 20;
    float deltaElev;

    public List<WaterController> AllWaterControllers = new List<WaterController>();

    public float[] waterMeshElevList;

	// Use this for initialization
	void Start () {
        
        deltaElev = Mathf.Abs(highTideElev - lowTideElev) / (meshCount - 1);

        waterMeshElevList = new float[meshCount];
        for (int i = 0; i < meshCount; i++)
        {
            waterMeshElevList[i] = highTideElev - deltaElev * i;
        }

        globalWaterElevation = waterMeshElevList[currentWaterIndex];
	}
	
	// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.L) && tideIn && tideEligible)
        {
            tideEligible = false;
            StartCoroutine(SendTideOut());
        }
        if (Input.GetKeyUp(KeyCode.O) && !tideIn && tideEligible)
        {
            tideEligible = false;
            StartCoroutine(SendTideIn());
        }

    }


	IEnumerator SendTideOut () 
    {
        while (tideIn)
        {
            float prevElev = globalWaterElevation;
            globalWaterElevation = Mathf.MoveTowards(globalWaterElevation, lowTideElev, tideSpeed * Time.deltaTime);
            float delta = globalWaterElevation - prevElev;

            foreach (WaterController controller in AllWaterControllers)
            {
                controller.MoveWaterBy(delta);
            }

            for (int i = currentWaterIndex + 1; i < meshCount; i++)
            {
                //Find first elevation in list that water level is greater than
                if (globalWaterElevation > waterMeshElevList[i])
                {
                    if (currentWaterIndex != i)
                    {
                        currentWaterIndex = i - 1;
                        foreach (WaterController controller in AllWaterControllers)
                        {
                            controller.SetTideIndex(currentWaterIndex);
                        }
                    }
                    break;
                }

            }

            if (Mathf.Abs(globalWaterElevation - lowTideElev) < 0.01)
            {
                tideEligible = true;
                tideIn = false;
            }

            yield return null;
        }
	}

    IEnumerator SendTideIn()
    {
        while (!tideIn)
        {
            float prevElev = globalWaterElevation;
            globalWaterElevation = Mathf.MoveTowards(globalWaterElevation, highTideElev, tideSpeed * Time.deltaTime);
            float delta = globalWaterElevation - prevElev;

            foreach (WaterController controller in AllWaterControllers)
            {
                controller.MoveWaterBy(delta);
            }

            for (int i = currentWaterIndex; i >= 0; i--)
            {
                //Find first elevation in list that water level is less than
                if (globalWaterElevation < waterMeshElevList[i])
                {
                    if (currentWaterIndex != i)
                    {
                        currentWaterIndex = i + 1;
                        foreach (WaterController controller in AllWaterControllers)
                        {
                            controller.SetTideIndex(currentWaterIndex);
                        }
                    }
                    break;
                }

            }

            if (Mathf.Abs(globalWaterElevation - highTideElev) < 0.01)
            {
                tideEligible = true;
                tideIn = true;
            }

            yield return null;
        }
    }

    //IEnumerator SendTideIn()
    //{
    //    for (int i = currentWaterIndex - 1; i >= 0; i--)
    //    {
    //        currentWaterIndex = i;
    //        globalWaterElevation = waterMeshElevList[i];

    //        foreach (WaterController controller in AllWaterControllers)
    //        {
    //            controller.SetTideIndex(i);
    //        }

    //        yield return new WaitForSeconds(0.5f);
    //    }
    //}
}
