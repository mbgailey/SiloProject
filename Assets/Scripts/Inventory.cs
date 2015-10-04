using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    //List<Item> InventoryList = new List<Item>();
    Item[] InventoryList = new Item[10];
    public RectTransform[] Slots = new RectTransform[10];
    GameObject[] InventoryObjects = new GameObject[10];
    bool[] Occupied = new bool[10];
    int currentSlot = 0;

    public int activeSlot = -1;
    float rotateSpeed = 20f;
    

	void Update () 
    {
        if (activeSlot != -1 && InventoryObjects[activeSlot] != null)
        {
            RotateItem(activeSlot);
        }
	}

    public void PickUp(Item item)
    {
        Item existingItem = item; //Initialize as the input item just because I'm lazy. Will be replaced by inventory item if match is found
        bool isNew = true;
        int index = 0;
        for (int i = 0; i < InventoryList.Length; i++)
        {
            if (Occupied[i])
            {
                if (InventoryList[i].myType == item.myType)
                {
                    isNew = false;
                    index = i;
                    break;
                }
            }
        }

        if (isNew)
        {
            AddNewToInventory(item);
        }
        else
        {
            AddExistingToInventory(InventoryList[index], item, index);
        }
    }

    void AddNewToInventory(Item item)
    {
        //InventoryList.Add(item);
        InventoryList[currentSlot] = item;
        Occupied[currentSlot] = true;
        GameObject obj = (GameObject)Instantiate(item.itemObj);
        InventoryObjects[currentSlot] = obj;
        obj.transform.parent = Slots[currentSlot].transform;
        obj.transform.localPosition = new Vector3(0f, 0f, -1f);
        obj.isStatic = false;
        obj.layer = LayerMask.NameToLayer("3dGUI");
        obj.transform.localScale = new Vector3(55f, 55f, 55f);
        currentSlot++;

    }

    void AddExistingToInventory(Item currentItem, Item newItem, int slotNum)
    {
        currentItem.itemCount += newItem.itemCount;
        Slots[slotNum].gameObject.GetComponent<InventorySlot>().UpdateCountDisplay(currentItem.itemCount);
    }


    public void RemoveFromInventory(Item item)
    {
        //InventoryList.Remove(item);
    }

    void UpdateGUI()
    {

    }

    public void RotateItem(int slotID)
    {
        Transform trans = InventoryObjects[slotID].transform;
        trans.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}
