using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerLungs : MonoBehaviour {

    float lungCapacity = 10f;
    float air;
    float fillRate = 2f;
    CharacterControl characterControl;
    PlayerHealth playerHealth;
    public bool submerged;
    public Image lungGUI;
    float damageRate = 1.0f;    //Number of seconds between taking damage when out of air
    float damageAmt = 10f;      //Amount of damage taken when out of air
    float damageTimer = 0f;

	// Use this for initialization
	void Start () {
        //characterControl = this.GetComponent<CharacterControl>();
        lungGUI = GameObject.Find("LungsFill").GetComponent<Image>();     //Is there a better way to get link to GUI object?
        playerHealth = this.GetComponent<PlayerHealth>();
        air = lungCapacity;

	}

    void Update()
    {
        if (submerged)
        {
            air -= Time.deltaTime;
            air = Mathf.Max(air, 0f);
            UpdateGUI(air / lungCapacity);
        }

        if (!submerged && air < lungCapacity)
        {
            air += Time.deltaTime * fillRate;
            air = Mathf.Min(air, lungCapacity);
            UpdateGUI(air / lungCapacity);
        }

        if (air <= 0f)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageRate)
            {
                playerHealth.TakeDamage(damageAmt);
                damageTimer = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }

    }

	void UpdateGUI (float fill) 
    {
        lungGUI.fillAmount = fill;
	}
}
