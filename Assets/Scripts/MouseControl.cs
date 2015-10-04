using UnityEngine;
using System.Collections;

public class MouseControl : MonoBehaviour {

    GameObject selectedObj;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
        if (hit)
        {
            
            GameObject hitObj = hitInfo.transform.gameObject;
            //Debug.Log("Hit " + hitObj);
            if (hitObj.tag == "InteractableObject")
            {

                if (hitInfo.transform.gameObject != selectedObj)
                {
                    //Switch selected object
                    if (selectedObj)
                    {
                        selectedObj.GetComponent<ObjectSelection>().UnselectObject();
                    }
                    selectedObj = hitInfo.transform.gameObject;
                    selectedObj.GetComponent<ObjectSelection>().SelectObject();
                }
            }
            else //If no hit, unselect previous object
            {
                if (selectedObj)
                {
                    selectedObj.GetComponent<ObjectSelection>().UnselectObject();
                    selectedObj = null;
                }

            }
        }

        else //If no hit, unselect previous object
        {
            if (selectedObj)
            {
                selectedObj.GetComponent<ObjectSelection>().UnselectObject();
                selectedObj = null;
            }
            
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedObj)
            {
                selectedObj.GetComponent<ObjectSelection>().ActionOnObject();
            }
        }

	}
}
