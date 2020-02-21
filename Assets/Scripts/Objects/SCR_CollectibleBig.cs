using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_CollectibleBig : MonoBehaviour
{
    [Tooltip("What's this Star's unique ID?")]
    public int starIndex;
    public GameObject normalMesh, collectedMesh;

    bool useTrigger = true;
    bool alreadyCollected = false;

    SCR_TongueTarget targetComponent;
    SCR_VarManager varManager;

    void Start()
    {
        targetComponent = GetComponent<SCR_TongueTarget>();
        varManager = SCR_VarManager.Instance;

        // Check if this star has been collected before
        foreach(var collIndex in varManager.CollectedStars)
        {
            if(collIndex == starIndex)
            {
                alreadyCollected = true;
            }
        }

        // Set correct mesh to visible
        if (alreadyCollected)
        {
            normalMesh.SetActive(false);
            collectedMesh.SetActive(true);
        }
        else
        {
            normalMesh.SetActive(true);
            collectedMesh.SetActive(false);
        }
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && useTrigger)
        {
            StartCoroutine(GetCollected());
            useTrigger = false;
        }
    }

    IEnumerator GetCollected()
    {
        if (!alreadyCollected)
        {
            varManager.currentStars ++;
            varManager.CollectedStars.Add(starIndex);
        }

        // [Play effect]
        yield return null;

        Destroy(gameObject);
    }
}
