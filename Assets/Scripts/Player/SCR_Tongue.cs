using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Tongue : MonoBehaviour
{
    #region Enable & Disable
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
    #endregion

    #region References
    public SCR_Variables variables;
    public Transform tongueParent;
    InputControls controls;
    #endregion

    #region Local variables
    SCR_TongueTarget currentTarget;
    bool lockedOnTarget;
    #endregion


    void Awake()
    {
        controls = new InputControls();

        InputActions();
    }

    void Update()
    {
        if (lockedOnTarget)
            LockedTarget();
        else
            AutoTarget();
    }

    void AutoTarget()
    {
        currentTarget = null;

        foreach(var target in SCR_ObjectReferenceManager.Instance.tongueTargets)
        {
            float targetDistance = Vector3.Distance(tongueParent.position, target.transform.position);
            bool targetInRange = targetDistance <= variables.maxTargetDistance;
            float angleToTarget = Quaternion.Angle(tongueParent.rotation, Quaternion.LookRotation((target.transform.position - tongueParent.position).normalized, Vector3.up));

            if (targetInRange 
                && angleToTarget < variables.maxTargetAngle
                && (currentTarget == null || targetDistance < Vector3.Distance(tongueParent.position, currentTarget.transform.position)))
            {
                currentTarget = target;
            }
        }
    }

    void PressTarget()
    {
        // If locked: unlock targeting.
        // If unlocked: Lock on target.

        if (lockedOnTarget)
        {
            UnlockTargeting();
        }
        else
        {
            InitiateLockOn();
        }
    }

    void InitiateLockOn()
    {
        if(currentTarget != null)
        {
            lockedOnTarget = true;
        }
    }

    void UnlockTargeting()
    {
        lockedOnTarget = false;
        currentTarget = null;
    }

    void LockedTarget()
    {
        float targetDistance = Vector3.Distance(tongueParent.position, currentTarget.transform.position);

        if (targetDistance <= variables.maxTargetDistance && currentTarget != null)
        {
            // do targeting stuff
        }
        else
        {
            UnlockTargeting();
        }
    }


    void InputActions()
    {
        // Set Hold targeting / Press targeting
        if (variables.holdToTarget)
        {

            controls.Player.HoldtoTargetPress.performed += ctx => InitiateLockOn();
            controls.Player.HoldtoTargetRelease.performed += ctx => UnlockTargeting();
        }
        else
            controls.Player.PresstoTarget.performed += ctx => PressTarget();
    }

    float targetSphereSize = 0;
    Vector3 targetIconPosition;
    private void OnDrawGizmos()
    {
        Color targetingColor;
        if (lockedOnTarget)
            targetingColor = new Color(1, .6f, .6f);
        else
            targetingColor = new Color(1f, 0, 0);


        Gizmos.color = Color.red;

        if(currentTarget != null)
        {
            targetIconPosition = Vector3.Lerp(targetIconPosition, currentTarget.transform.position, 20 * Time.deltaTime);
            targetSphereSize = Mathf.MoveTowards(targetSphereSize, 0.5f, 4 * Time.deltaTime);
            Gizmos.DrawIcon(targetIconPosition + currentTarget.targetIconOffset, "SPR_TargetTriangle128.png", false, targetingColor);
        }
        else
        {
            targetSphereSize = Mathf.MoveTowards(targetSphereSize, 0.0f, 4 * Time.deltaTime);
        }
            Gizmos.DrawWireSphere(targetIconPosition, targetSphereSize);
    }
}
