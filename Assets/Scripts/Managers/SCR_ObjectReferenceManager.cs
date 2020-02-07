using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ObjectReferenceManager : MonoBehaviour
{
    #region Singleton
    public static SCR_ObjectReferenceManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            transform.parent = null;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public List<SCR_TongueTarget> tongueTargets = new List<SCR_TongueTarget>();

    public Camera playerCamera;

    public GameObject player;
}
