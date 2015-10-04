using UnityEngine;
using System.Collections;

public class Item {

    GameObject itemObj;
    public string itemName;
    public int itemID;
    public string itemDescription;
    public int itemCount;


	// Use this for initialization
	public Item (string name, int id, int count) 
    {
        itemName = name;
        itemID = id;
        itemCount = count;

	}
	
}
