using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayerSystem : MonoBehaviour
{
    public SCR_Variables variables;

    SCR_ObjectReferenceManager objectRefs;
    SCR_VarManager varManager;


    void Start()
    {
        SCR_ObjectReferenceManager.Instance.player = gameObject;

        varManager = SCR_VarManager.Instance;
        objectRefs = SCR_ObjectReferenceManager.Instance;

        varManager.currentHealth = variables.maxHealth;
    }


    void Update()
    {
        if(varManager.currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Disable player behavior
        // Play animation

        // Bring up menu to reset game

        print("YOU DIED");
    }
}
