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

    [Header("Menu items")]
    public CanvasGroup debugMenu;
    public Button collectedStarsButton;
    public TextMeshProUGUI collectedStarsText;
    #endregion

    #region Private variables
    float secondsHeld;
    bool debugMenuOpen = false;
    #endregion

    #region References
    SCR_VarManager varManager;
    SCR_ObjectReferenceManager objectRefs;
    #endregion

    void Start()
    {
        varManager = SCR_VarManager.Instance;
        objectRefs = SCR_ObjectReferenceManager.Instance;

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
        if(secondsHeld >= 1 && !debugMenuOpen)
        {
            OpenDebugMenu();
        }

        if (debugMenuOpen)
            MenuUpdate();
    }

    #region Menu & button functionality

    void MenuUpdate()
    {
        // Check for close menu input
        if (Input.GetKeyDown(closeMenuButton))
            CloseDebugMenu();

        // Collected stars button
        collectedStarsText.SetText(varManager.currentStars.ToString());
        if(EventSystem.current.currentSelectedGameObject == collectedStarsButton.gameObject)
        {
            // Change this condition later to use controller input
            if (Input.GetKeyDown(KeyCode.LeftArrow) && varManager.currentStars > 0)
            {
                varManager.currentStars--;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                varManager.currentStars++;
            }
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ResetCollectibles()
    {
        while(varManager != null && varManager.CollectedStars.Count > 0)
        {
            varManager.CollectedStars.RemoveAt(0);
        }

        varManager.currentStars = 0;
        varManager.currentCoins = 0;
    }
    public void RebootGame()
    {
        Destroy(varManager.gameObject);
        SceneManager.LoadScene(0);
    }

    #endregion


    void OpenDebugMenu()
    {
        debugMenuOpen = true;

        debugMenu.alpha = 1;
        debugMenu.interactable = true;
        debugMenu.blocksRaycasts = true;
        collectedStarsButton.Select();

        print("Debug mode activated");
    }

    void CloseDebugMenu()
    {
        debugMenuOpen = false;

        debugMenu.alpha = 0;
        debugMenu.interactable = false;
        debugMenu.blocksRaycasts = false;
        collectedStarsButton.Select();

        print("Debug menu closed");
    }

}
