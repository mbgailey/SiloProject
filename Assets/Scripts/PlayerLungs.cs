using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerLungs : MonoBehaviour {

    float lungCapacity = 20f;
    float air;
    float fillRate = 2f;
    CharacterControl characterControl;
    public bool submerged;
    public Image lungGUI;


	// Use this for initialization
	void Start () {
        //characterControl = this.GetComponent<CharacterControl>();
        lungGUI = GameObject.Find("LungsFill").GetComponent<Image>();     //Is there a better way to get link to GUI object?
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
    }

	void UpdateGUI (float fill) 
    {
        lungGUI.fillAmount = fill;
	}
}
