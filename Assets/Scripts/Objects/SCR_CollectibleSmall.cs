using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_CollectibleSmall : MonoBehaviour
{
    [Tooltip("How many coins will player get from this object?")]
    public int coinValue = 1;
    
    bool useTrigger = true;

    SCR_TongueTarget targetComponent;
    SCR_VarManager varManager;

    void Start()
    {
        targetComponent = GetComponent<SCR_TongueTarget>();
        varManager = SCR_VarManager.Instance;
    }


    void Update()
    {
        // Disable trigger when player eats coin, so it won't be collected twice
        if (targetComponent.isBeingEaten)
        {
            useTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && useTrigger)
        {
            GetCollected();
        }
    }

    void GetCollected()
    {
        varManager.currentCoins += coinValue;
        // [Play effect]
        Destroy(gameObject);
    }
}
