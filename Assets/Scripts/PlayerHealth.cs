using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

    float maxHealth = 100f;
    public float health;
    HealthGUI healthGUI;

	// Use this for initialization
	void Start () {
        healthGUI = GameObject.Find("StatusPanel").GetComponent<HealthGUI>();
        health = maxHealth;
	}

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
        {
            TakeDamage(10f);
        }
    }

	public void TakeDamage (float dam) 
    {
        health -= dam;
        health = Mathf.Max(health, 0f);
        healthGUI.SetHealth(health / maxHealth);
	}

    public void HealDamage(float heal)
    {
        health += heal;
        health = Mathf.Max(health, maxHealth);
        healthGUI.SetHealth(health / maxHealth);
    }
}
