using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    List<Item> InventoryList = new List<Item>();
    

	// Use this for initialization
	void Start () 
    {
	
	}

    public void AddToInventory(Item item)
    {
        InventoryList.Add(item);
    }


    public void RemoveFromInventory(Item item)
    {
        InventoryList.Remove(item);
    }

    void UpdateGUI()
    {

    }
}
