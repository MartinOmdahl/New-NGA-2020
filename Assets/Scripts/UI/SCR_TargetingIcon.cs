using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_TargetingIcon : MonoBehaviour
{
    SCR_ObjectReferenceManager objectRefs;
    SCR_Tongue playerTongue;
    SCR_TongueTarget target;

    bool targetHasBeenNull = true;

    void Start()
    {
        objectRefs = SCR_ObjectReferenceManager.Instance;
        objectRefs.targetIcon = this;

        transform.SetParent(null);

        // Make icon start invisible
        transform.localScale = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Destroy self if player stops existing
        if (objectRefs.player == null)
            Destroy(gameObject);
        else
        {
            if(playerTongue == null)
                playerTongue = objectRefs.player.GetComponent<SCR_Tongue>();

            // Rotate icon to always face camera
            transform.LookAt(objectRefs.playerCamera.transform, Vector3.up);

            target = playerTongue.currentTarget;
            if (target != null)
            {
                Targeting();
            }
            else
            {
                NotTargeting();
            }
        }
    }

    void Targeting()
    {
        // Grow icon to normal size
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.7f);

        Vector3 targetPos = target.transform.position;
        if (targetHasBeenNull)
        {
            // If NOT moving directly from the previous target, set position instantly
            transform.position = targetPos;
            targetHasBeenNull = false;
        }
        else
        {
            // If moving directly from the previous target, set position with a lerp
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.7f);
        }
    }

    void NotTargeting()
    {
        // Shrink icon
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.7f);

        // If scale is close to zero, set scale to zero
        if (transform.localScale.x < 0.01f)
        {
            transform.localScale = Vector3.zero;
            targetHasBeenNull = true;
        }
    }
}
