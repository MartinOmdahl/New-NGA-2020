using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;


public class SCR_DebugMenu : MonoBehaviour
{
    #region Public variables

    public List<KeyCode> debugMenuShortcut = new List<KeyCode>();
    public KeyCode closeMenuButton;
    [Tooltip("How long should shortcut be held?")]
    public float shortcutHoldTime = 1;
    public GameObject playerPositionObject;

    [Header("Menu items")]
    public CanvasGroup debugMenu;
    public Button collectedStarsButton;
    public TextMeshProUGUI collectedStarsText;
    public Button collectedCoinsButton;
    public TextMeshProUGUI collectedCoinsText, incrementby10Text;
    public Button loadSceneButton;
    public TextMeshProUGUI loadSceneText, currentSceneText;
    #endregion

    #region Private variables
    float secondsHeld;
    bool debugMenuOpen = false;
    int sceneToLoad;
    int currentSceneIndex;
    int prevFrameXInput;
    #endregion

    #region References
    SCR_VarManager varManager;
    SCR_ObjectReferenceManager objectRefs;
    #endregion

    void Start()
    {
        varManager = SCR_VarManager.Instance;
        objectRefs = SCR_ObjectReferenceManager.Instance;
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        CloseDebugMenu();
    }

    void Update()
    {
        // If holding debug shortcut: increase "seconds held" timer. 
        // Else: reset timer.
        secondsHeld += Time.deltaTime;
        foreach (var shortcutKey in debugMenuShortcut)
        {
            if (!Input.GetKey(shortcutKey))
                secondsHeld = 0;
        }

        // Open menu if shortcut has been held for 1 second.
        if (secondsHeld >= shortcutHoldTime && !debugMenuOpen)
        {
            OpenDebugMenu();
        }

        if (debugMenuOpen)
            MenuUpdate();

        prevFrameXInput = (int)Input.GetAxisRaw("Horizontal");
    }

    #region Menu & button functionality

    void MenuUpdate()
    {
        // Check for close menu input
        if (Input.GetKeyDown(closeMenuButton))
            CloseDebugMenu();

        // Collected stars button
        collectedStarsText.SetText(varManager.currentStars.ToString());
        int starIncrement = Input.GetKey(KeyCode.Joystick1Button2) ? 10 : 1;
        if (EventSystem.current.currentSelectedGameObject == collectedStarsButton.gameObject)
        {
            // Increment / decrement current stars on joystick left / right
            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == -1)
                varManager.currentStars -= starIncrement;

            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == 1)
                varManager.currentStars += starIncrement;

            if (varManager.currentStars < 0)
                varManager.currentStars = 0;
        }

        // Collected coins button
        collectedCoinsText.SetText(varManager.currentCoins.ToString());
        int coinIncrement = Input.GetKey(KeyCode.Joystick1Button2) ? 10 : 1;
        if (EventSystem.current.currentSelectedGameObject == collectedCoinsButton.gameObject)
        {
            // Increment / decrement current stars on joystick left / right
            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == -1)
                varManager.currentCoins -= coinIncrement;

            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == 1)
                varManager.currentCoins += coinIncrement;

            if (varManager.currentCoins < 0)
                varManager.currentCoins = 0;

            incrementby10Text.enabled = true;
        }
        else
        {
            incrementby10Text.enabled = false;
        }

        // Load scene button

        loadSceneText.SetText(sceneToLoad.ToString());
        if (EventSystem.current.currentSelectedGameObject == loadSceneButton.gameObject)
        {
            // Increment / decrement scene value on joystick left / right
            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == -1 && sceneToLoad > 0)
            {
                sceneToLoad--;
            }

            if (prevFrameXInput == 0 && Input.GetAxisRaw("Horizontal") == 1 && sceneToLoad < SceneManager.sceneCountInBuildSettings - 1)
            {
                sceneToLoad++;
            }

            // Get name of selected scene
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneToLoad);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            // Trim prefix from name
            if(sceneName.Substring(0, 4) == "SCE_")
            sceneName = sceneName.Substring(4);

            // Set "selected scene" text
            currentSceneText.enabled = true;
            currentSceneText.SetText(sceneName);
            if (sceneToLoad == currentSceneIndex)
                currentSceneText.text += "\n(current scene)";

            if (Input.GetButtonDown("Submit") && SceneManager.GetSceneByBuildIndex(sceneToLoad) != null)
            {
                print("Loaded scene index " + sceneToLoad);
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            currentSceneText.enabled = false;
        }

    }

    public void ResetCollectibles()
    {
        while (varManager != null && varManager.CollectedStars.Count > 0)
        {
            varManager.CollectedStars.RemoveAt(0);
        }

        varManager.currentStars = 0;
        varManager.currentCoins = 0;

        print("All collectibles have been reset");
    }
    public void RebootGame()
    {
        Destroy(varManager.gameObject);
        SceneManager.LoadScene(0);
        print("Game has been reset");
    }

    public void SetPlayerPosition()
    {
        if(objectRefs.player != null)
        {
            Instantiate(playerPositionObject, objectRefs.player.transform.position, playerPositionObject.transform.rotation);
            Destroy(objectRefs.player);
            CloseDebugMenu();
        }
        else
        {
            print("Error: Couldn't set player position because no player exists in scene");
        }
    }

    #endregion

    void OpenDebugMenu()
    {
        debugMenuOpen = true;

        // Show debug menu
        debugMenu.alpha = 1;
        debugMenu.interactable = true;
        debugMenu.blocksRaycasts = true;

        // Reset debug menu selections
        collectedStarsButton.Select();
        sceneToLoad = currentSceneIndex;

        // Freeze time
        Time.timeScale = 0;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        // Disable player controls
        if (objectRefs.player != null)
        {
            objectRefs.player.GetComponent<SCR_PlayerV3>().enabled = false;
            objectRefs.player.GetComponent<SCR_Tongue>().enabled = false;
        }

        print("Debug menu activated");
    }

    void CloseDebugMenu()
    {
        debugMenuOpen = false;

        debugMenu.alpha = 0;
        debugMenu.interactable = false;
        debugMenu.blocksRaycasts = false;

        // Unfreeze time
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        // Enable player controls
        if(objectRefs.player != null)
        {
            objectRefs.player.GetComponent<SCR_PlayerV3>().enabled = true;
            objectRefs.player.GetComponent<SCR_Tongue>().enabled = true;
        }

        print("Debug menu closed");
    }

}
