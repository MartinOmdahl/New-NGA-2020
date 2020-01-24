using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// General script for all tongue-interactable objects
public class SCR_TongueTarget : MonoBehaviour
{
    public Vector3 targetIconOffset = new Vector3(0, 0.5f, 0);
    void Start()
    {
        // Add self to list of all tongue targets
        SCR_ObjectReferenceManager.Instance.tongueTargets.Add(this);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize target icon position
        Gizmos.DrawIcon(transform.position + targetIconOffset, "SPR_TargetTriangle128.png", false, Color.white);
    }
}
