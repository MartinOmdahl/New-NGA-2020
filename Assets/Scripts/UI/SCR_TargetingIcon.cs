using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_TargetingIcon : MonoBehaviour
{
    public SCR_TongueTarget target;

    CanvasGroup cGroup;
    SCR_ObjectReferenceManager objectRefs;

    bool targetHasBeenNull = true;
    float targetScale;

    void Start()
    {
        objectRefs = SCR_ObjectReferenceManager.Instance;
        objectRefs.targetIcon = this;
        cGroup = GetComponent<CanvasGroup>();

        transform.SetParent(null);
    }

    void FixedUpdate()
    {
        transform.LookAt(objectRefs.playerCamera.transform, Vector3.up);

        if(target != null)
        {
            Targeting();
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.7f);

            if (transform.localScale.x < 0.01f)
            {
                transform.localScale = Vector3.zero;
                targetHasBeenNull = true;
            }
        }

        // Destroy self if player stops existing
        if (objectRefs.player == null)
            Destroy(gameObject);
    }

    void Targeting()
    {
        //float maxDistance = objectRefs.variables.distanceFromTarget + objectRefs.variables.maxTargetDistance;

        //targetScale = 1.8f -
        //    Vector3.Distance(objectRefs.playerCamera.transform.position, target.transform.position) / (maxDistance -2);

        //print("targetscale: " + targetScale);


        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.7f);

        Vector3 targetPos = target.transform.position;
        if (targetHasBeenNull)
        {
            transform.position = targetPos;
            targetHasBeenNull = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.7f);
        }
    }

}
