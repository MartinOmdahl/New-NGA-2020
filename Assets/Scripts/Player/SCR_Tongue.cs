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
    public Transform tongueCollider;
    InputControls controls;
    SCR_PlayerV3 movementScript;
    Rigidbody rb;
    #endregion

    #region Local variables
    SCR_TongueTarget currentTarget;
    bool lockedOnTarget;
    bool holdingTongueButton;
    enum TongueState { Retracted, attacking, Attached}
    TongueState tongueState = TongueState.Retracted;
    #endregion

    void Awake()
    {
        controls = new InputControls();
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<SCR_PlayerV3>();

        InputActions();
    }

    void Update()
    {
        if (tongueState == TongueState.Retracted)
        {
            if (lockedOnTarget)
                LockedTarget();
            else
                AutoTarget();
        }

        if (controls.Player.TongueButtonPress.triggered
            && tongueState == TongueState.Retracted
            && currentTarget != null)
        {
            StartCoroutine(TongueAttack(currentTarget));
            tongueState = TongueState.attacking;
        }
    }

    #region Targeting
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

    #endregion

    #region Tongue attack & hold
    IEnumerator TongueAttack(SCR_TongueTarget target)
    {
        float TongueDistance = 0;
        while (TongueDistance < 1)
        {
            yield return new WaitForEndOfFrame();
            TongueDistance += 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, target.transform.position, TongueDistance);
        }

        StartCoroutine(TongueHold(target));
    }

    IEnumerator TongueHold(SCR_TongueTarget target)
    {
        ConfigurableJoint targetJoint = target.GetComponent<ConfigurableJoint>();
        //targetJoint.connectedAnchor = tongueParent.position;
        targetJoint.connectedBody = rb;
        movementScript.usingNormalMovement = false;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
       
        while (holdingTongueButton && Vector3.Distance(tongueParent.position, target.transform.position) < variables.maxTargetDistance)
        {
            yield return new WaitForEndOfFrame();

            if (controls.Player.Jump.triggered)
            {
                break;
            }

            tongueCollider.position = target.transform.position;
        }
        targetJoint.connectedBody = null;
        movementScript.usingNormalMovement = true;
       
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        StartCoroutine(TongueRetract(target));
    }

    IEnumerator TongueRetract(SCR_TongueTarget target)
    {
        float TongueDistance = 1;
        while (TongueDistance > 0)
        {
            yield return new WaitForEndOfFrame();
            TongueDistance -= 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, target.transform.position, TongueDistance);
        }

        tongueState = TongueState.Retracted;
    }
    #endregion

    #region Input actions
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

        controls.Player.TongueButtonPress.performed += ctx => holdingTongueButton = true;
        controls.Player.TongueButtonRelease.performed += ctx => holdingTongueButton = false;

    }
    #endregion

    #region Gizmos
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
    #endregion
}
