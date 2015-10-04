using UnityEngine;
using System.Collections;

public class WaterSplashController : MonoBehaviour {

    public GameObject splashPrefab;
    float maxLifetime = 0.75f;
    float maxSpeed = 7f;
    float particleVelocityMultiplier = 0.75f;
    float maxEmissionRate = 150;
    float globalWaterElevation = -2.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGERED");
        if (other.tag == "Player")
        {
            Vector3 vel = other.attachedRigidbody.velocity;
            Debug.Log("PLAYER");
            if (vel.y < 0.2f)
            {
                float scale = Mathf.Min(vel.magnitude, maxSpeed);
                Debug.Log("CREATESPLASH");
                CreateSplash(scale, other.transform.position);
            }
        }
	}

    void CreateSplash(float scale, Vector3 loc)
    {
        Vector3 tempLoc = loc;
        tempLoc.y = globalWaterElevation;
        GameObject splash = (GameObject)Instantiate(splashPrefab, tempLoc, Quaternion.Euler(270f, 0f, 0f));
        ParticleSystem splashParticles = splash.GetComponent<ParticleSystem>();

        //Set parameters
        splashParticles.startSpeed = scale * particleVelocityMultiplier;
        splashParticles.startLifetime = maxLifetime * (scale / maxSpeed);
        splashParticles.emissionRate = maxEmissionRate * (scale / maxSpeed);



    }
}
