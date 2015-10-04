using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour {

    public int mySlotID;
    Inventory inventory;
    public bool occupied = false;
    Text itemCountDisplay;

	// Use this for initialization
	void Start () {
        inventory = this.GetComponentInParent<Inventory>();
        itemCountDisplay = this.GetComponentInChildren<Text>();
	}
	
	public void HoverOver () 
    {
        inventory.activeSlot = mySlotID;
	}
    public void HoverOff()
    {
        inventory.activeSlot = -1;
    }
    void Selected()
    {

    }

    public void UpdateCountDisplay(int count)
    {
        itemCountDisplay.text = count.ToString();
    }
}
