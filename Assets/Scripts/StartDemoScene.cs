using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartDemoScene : MonoBehaviour {

    public bool startGUI = true;
    CreateWorld2 createWorld;
    public Canvas demoMenu;
    public Toggle useSeedToggle;
    public Button startButton;
    public Button resetButton;
    public InputField seedInput;
    public InputField sizeInput;
    public Text currentSeed;

    public GameObject playerPrefab;

    bool worldGenerated = false;



	// Use this for initialization
	void Start () {
        createWorld = GameObject.Find("WorldCreator").GetComponent<CreateWorld2>();
        
        if (!startGUI)
        {
            //demoMenu.enabled = false;
            StartDemo();
        }

        EventSystem.current.SetSelectedGameObject(startButton.gameObject);

	}
	
	// Update is called once per frame
	void Update () {

        if ((Input.GetKeyUp(KeyCode.Escape) || Input.GetButtonUp("Start")) && worldGenerated)
        {
            demoMenu.enabled = !demoMenu.enabled;
            EventSystem.current.sendNavigationEvents = !EventSystem.current.sendNavigationEvents;
        }

	}

    public void StartDemo()
    {
        //First time demo start
        if (startGUI)
        {
            if (int.Parse(seedInput.text) != null && int.Parse(seedInput.text) >= 0)
            {
                createWorld.useRandomSeed = useSeedToggle.isOn;
                createWorld.randomSeed = int.Parse(seedInput.text);
            }

            if (int.Parse(sizeInput.text) != null && int.Parse(sizeInput.text) >= 0)
            {
                createWorld.maxBranches = int.Parse(sizeInput.text);
            }
        }
        
        demoMenu.enabled = false;
        EventSystem.current.sendNavigationEvents = false;
        createWorld.GenerateWorld();
        worldGenerated = true;
        startButton.interactable = false;
        currentSeed.text = "Current Seed: " + createWorld.randomSeed.ToString();
        StartCoroutine(SpawnPlayer());
        
    } 
    public void ResetScene()
    {
        //Restart demo
        Application.LoadLevel(0);
    }

    IEnumerator SpawnPlayer()
    {
        yield return new WaitForSeconds(1.0f);
        GameObject player = (GameObject)Instantiate(playerPrefab);
        //Camera.main.GetComponent<SmoothFollow>().target = player;
    }
}
