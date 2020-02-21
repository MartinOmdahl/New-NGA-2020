using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_VarManager : MonoBehaviour
{
    #region Singleton
    public static SCR_VarManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("Player")]
    public int currentCoins;
    public int currentStars;
    public int currentHealth;

    [Header("Gameplay")]
    public bool GamePaused = false;
}
