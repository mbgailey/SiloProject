using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Camera))]

public class FollowCharacter : MonoBehaviour {

    Camera cam;
    public GameObject character;
    float camDistance;
    float heightDelta = 1f;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        camDistance = cam.transform.position.z;
        
	}
	
	// Update is called once per frame
	void Update () {
        if (character)
        {
            Vector3 pos = character.transform.position;
            pos.z = camDistance;
            pos.y += heightDelta;
            cam.transform.position = pos;

            cam.transform.LookAt(character.transform.position);
        }
	}
}
