using UnityEngine;
using System.Collections;

public class ControlTorch : MonoBehaviour
{

    public float multiplier = 0.5f;
    bool torchOn = false;
    float targetMult;

    private void Start()
    {
        targetMult = 0f;
        TurnOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (torchOn) { torchOn = false; targetMult = 0f; TurnOff(); }
            else { torchOn = true; targetMult = multiplier; TurnOn(); }
        }


    }

    void TurnOn()
    {
        var systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in systems)
        {
            system.startSize *= targetMult;
            system.startSpeed *= targetMult;
            system.startLifetime *= Mathf.Lerp(targetMult, 1, 0.5f);
            system.Clear();
            system.Play();
        }
        Light light = GetComponentInChildren<Light>();
        light.enabled = true;
    }

    void TurnOff()
    {
        var systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in systems)
        {
            system.startSize *= targetMult;
            system.startSpeed *= targetMult;
            system.startLifetime *= Mathf.Lerp(targetMult, 1, 0.5f);
            system.Stop();

        }
        Light light = GetComponentInChildren<Light>();
        light.enabled = false;
    }

}
