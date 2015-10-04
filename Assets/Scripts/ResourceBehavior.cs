using UnityEngine;
using System.Collections;

public class ResourceBehavior : MonoBehaviour {

    public int myID;
    public bool stackable = true;
    public float minScale = 0.5f;
    public float maxScale = 1.2f;
    public bool visible = true;
    public bool harvestable = true;

    public ParticleSystem gatherEffect;

    ResourceBinClass myBin;

	// Use this for initialization
	void Start () {
        myBin = this.GetComponentInParent<ResourceBinClass>();
	}

    public void InitializeProps(int ID)
    {
        float scale = Random.Range(minScale, maxScale);
        this.transform.localScale = new Vector3(scale, scale, scale);
        this.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        myID = ID;
    }

    public void Harvest()
    {
        //this.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.SetActive(false);
        visible = false;
        harvestable = false;
        myBin.ResourceHarvested(myID);
    }

    public void Regenerate()
    {
        //this.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.SetActive(true);
        visible = true;
        harvestable = true;
    }
	
}
