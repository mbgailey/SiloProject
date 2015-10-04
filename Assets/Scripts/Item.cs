using UnityEngine;
using System.Collections;

public class Item {

    public GameObject itemObj;
    public string itemName;
    //public int itemID;
    public string itemDescription;
    public int itemCount;
    public bool stackable;
    public enum itemType { blueMushroom}
    public itemType myType;

	// Use this for initialization
	public Item (string name, int count, itemType type) 
    {
        itemName = name;
        //itemID = id;
        itemCount = count;
        //itemObj = Resources.Load<GameObject>("Prefabs/Items/" + name);
        itemObj = (GameObject)Resources.Load("Prefabs/Items/" + name);
        stackable = true;
        myType = type;
	}
	
}
