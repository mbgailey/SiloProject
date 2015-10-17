using UnityEngine;
using System.Collections;

public class WaterSplashController : MonoBehaviour {

    public GameObject splashPrefab;
    float maxLifetime = 0.75f;
    float maxSpeed = 7f;
    float particleVelocityMultiplier = 0.75f;
    float maxEmissionRate = 150;
    //float globalWaterElevation = -2.0f;
    TideController tideController;
    CharacterControl playerController;

	// Use this for initialization
	void Start () {
        tideController = GameObject.FindGameObjectWithTag("GameController").GetComponent<TideController>();
	}
	
	// Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("TRIGGERED");
        if (other.tag == "Player")
        {
            
            if (!playerController)
            {
                playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterControl>();
            }
            Vector3 vel = playerController._velocity;

            Debug.Log("vel: " + vel);

            if (vel.y < -0.05f)
            {
                float scale = Mathf.Min(Mathf.Abs(vel.y), maxSpeed); //Velocity scale doesn't work yet with new character controller
                //float scale = 5f;
                Debug.Log("CREATESPLASH; scale: " + scale);
                CreateSplash(scale, other.transform.position);
            }
        }
	}

    void CreateSplash(float scale, Vector3 loc)
    {
        Vector3 tempLoc = loc;
        tempLoc.y = tideController.globalWaterElevation;
        GameObject splash = (GameObject)Instantiate(splashPrefab, tempLoc, Quaternion.Euler(270f, 0f, 0f));
        ParticleSystem splashParticles = splash.GetComponent<ParticleSystem>();

        //Set parameters
        splashParticles.startSpeed = scale * particleVelocityMultiplier;
        splashParticles.startLifetime = maxLifetime * (scale / maxSpeed);
        splashParticles.emissionRate = maxEmissionRate * (scale / maxSpeed);



    }
}
