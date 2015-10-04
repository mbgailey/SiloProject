using UnityEngine;
using System.Collections;

public class KillParticleSystem : MonoBehaviour {

    float killTime = 1.5f;
    float startTime;

	// Use this for initialization
	void Awake () {
        startTime = Time.timeSinceLevelLoad;
	}
	
	// Update is called once per frame
	void Update () {
        float aliveTime = Time.timeSinceLevelLoad - startTime;
        if (aliveTime >= killTime)
        {
            Destroy(this.gameObject);
        }
	}
}
