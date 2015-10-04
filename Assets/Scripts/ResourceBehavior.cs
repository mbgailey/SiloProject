using UnityEngine;
using System.Collections;

public class ResourceBehavior : MonoBehaviour {

    public int myID;
    public bool stackable = true;
    public float minScale = 0.5f;
    public float maxScale = 1.2f;
    public bool visible = true;
    public bool harvestable = true;
    public Item.itemType resType;

    public ParticleSystem gatherEffect;

    ResourceBinClass myBin;
    Inventory inventory;

	// Use this for initialization
	void Start () {
        myBin = this.GetComponentInParent<ResourceBinClass>();
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>(); ;
	}

    public void InitializeProps(int ID, Item.itemType type)
    {
        float scale = Random.Range(minScale, maxScale);
        this.transform.localScale = new Vector3(scale, scale, scale);
        this.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        myID = ID;
        resType = type;
    }

    public void Harvest()
    {
        //this.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.SetActive(false);
        visible = false;
        harvestable = false;
        myBin.ResourceHarvested(myID);

        Item item = new Item("BlueMushroom", 1, Item.itemType.blueMushroom);    //Create item with item constructor
        inventory.PickUp(item);

    }

    public void Regenerate()
    {
        //this.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.SetActive(true);
        visible = true;
        harvestable = true;
    }
	
}
