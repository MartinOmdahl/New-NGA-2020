using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_TongueTarget : MonoBehaviour
{
    public Vector3 targetIconOffset = new Vector3(0, 0.5f, 0);

    void Start()
    {
        SCR_ObjectReferenceManager.Instance.tongueTargets.Add(this);
    }

    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawIcon(transform.position + targetIconOffset, "SPR_TargetTriangle128.png", false, Color.white);
    }
}
