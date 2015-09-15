using UnityEngine;
using System.Collections;

public class SetEmissive : MonoBehaviour {

    public Renderer lightwall;

	// Use this for initialization
	void Start () {
        lightwall = this.GetComponent<Renderer>();
        Color final = Color.white * Mathf.LinearToGammaSpace(1000);
        //lightwall.material.SetColor("_EmissionColor", final);
        DynamicGI.SetEmissive(lightwall, final);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
