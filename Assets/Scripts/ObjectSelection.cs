using UnityEngine;
using System.Collections;

public class ObjectSelection : MonoBehaviour {

    public Material selectedMat;
    Material[] selectedMatList;
    Material[] normalMats;
    MeshRenderer rend;
    enum ItemCategory { resource, item, enemy}
    ItemCategory myType;
    
    public bool selected = false;

	// Use this for initialization
	void Start () {
	    rend = this.GetComponent<MeshRenderer>();
        normalMats = rend.materials;
        selectedMatList = new Material[normalMats.Length];
        for (int i = 0; i < normalMats.Length; i++)
        {
            selectedMatList[i] = selectedMat;
        }
        myType = ItemCategory.resource;
	}
	
	public void SelectObject () 
    {
        rend.materials = selectedMatList;
        selected = true;

	}

    public void UnselectObject()
    {
        rend.materials = normalMats;
        selected = false;
    }

    public void ActionOnObject()
    {
        if (myType == ItemCategory.resource)
        {
            this.GetComponent<ResourceBehavior>().Harvest();
        }
    }


}
