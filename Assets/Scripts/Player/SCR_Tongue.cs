﻿using System.Collections;
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
    SCR_PlayerV3 movement;
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
        movement = GetComponent<SCR_PlayerV3>();

        InputActions();
    }

    void Update()
    {
        // Targeting behavior
        if (tongueState == TongueState.Retracted)
        {
            if (lockedOnTarget)
                LockedTarget();
            else
                AutoTarget();
        }

        // Check for tongue attack initiation
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

        // Go through each target in scene. If it is within range & angle of player, and no other target is closer, set it as current target.
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
        // This is only used if "press" targeting is used

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
        // Lock on autotargeted object, if any. Otherwise, do nothing.
        if(currentTarget != null)
        {
            lockedOnTarget = true;
        }
    }

    void UnlockTargeting()
    {
        lockedOnTarget = false;
    }

    void LockedTarget()
    {
        float targetDistance = Vector3.Distance(tongueParent.position, currentTarget.transform.position);

        if (targetDistance <= variables.maxTargetDistance && currentTarget != null)
        {
            // Turn player towards target
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

            // Linearly interpolate tongue's position from body to target
            // (promote speed to variable later)
            TongueDistance += 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, target.transform.position, TongueDistance);
        }

        // Check what type of tongue interaction should be used
        switch (target.targetType)
        {
            case SCR_TongueTarget.TargetType.Swing:
                if (!movement.touchingGround || target.canSwingFromGround)
                {
                    StartCoroutine(TongueSwing(target));
                }
                else
                {
                    StartCoroutine(TongueRetract(target.transform.position, 1));
                }
                break;

            case SCR_TongueTarget.TargetType.Eat:
                StartCoroutine(TongueEat(target));
                break;

            case SCR_TongueTarget.TargetType.Grab:
                // Start Eat behavior
                break;

            case SCR_TongueTarget.TargetType.Deflect:
                // Maybe update this later to add effect
                StartCoroutine(TongueRetract(target.transform.position, 1));
                break;

            default:
                StartCoroutine(TongueRetract(target.transform.position, 1));
                break;
        }
    }

    IEnumerator TongueSwing(SCR_TongueTarget target)
    {
        // Stop targeting
        currentTarget = null;

        ConfigurableJoint targetJoint = target.GetComponentInChildren<ConfigurableJoint>();
        Rigidbody targetJointRb = targetJoint.GetComponent<Rigidbody>();

        // Turn player towards target
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - tongueParent.position);
        transform.rotation = targetRotation;
        targetJoint.transform.rotation = targetRotation;
       
        // Connect player to target joint
        targetJoint.connectedBody = rb;

        // Set target distance while swinging (promote to variable later)
        targetJoint.targetPosition = new Vector3(0, 0, Vector3.Distance(tongueParent.position, targetJoint.transform.position) - variables.swingDistance);

        // Set initial swing velocity. Change this in future to convert linear velocity to angular
        //rb.velocity = rb.velocity.normalized * 7;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Deactivate normal movement
        movement.usingNormalMovement = false;
        movement.canMidairJump = false;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

        float swingTime = 0;
        while (holdingTongueButton || swingTime < 0.65f )
        {
            yield return new WaitForEndOfFrame();
            swingTime += Time.deltaTime;

            // Add some forward rotation at the beginning of swing
            if (swingTime < 0.1f)
            {
                targetJointRb.AddRelativeTorque(-400 * Time.deltaTime, 0, 0, ForceMode.Impulse);
            }

            //// Break tongue contact if player jumps (deprecated for now)
            //if (controls.Player.Jump.triggered)
            //{
            //    break;
            //}

            tongueCollider.position = target.transform.position;
        }
        // Break connection to joint
        targetJoint.connectedBody = null;

        // Set velocity on swing end. Change this in future to convert linear velocity to angular
        rb.velocity = rb.velocity.normalized * targetJointRb.angularVelocity.magnitude * 2;

        // Re-activate normal movement
        movement.usingNormalMovement = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Refresh midair jump after swinging
        movement.canMidairJump = true;

        StartCoroutine(TongueRetract(target.transform.position, 1));
    }
    
    IEnumerator TongueEat(SCR_TongueTarget target)
    {
        // Stop targeting
        currentTarget = null;

        // Politely notify target that it's going to be eaten
        target.isBeingEaten = true;

        // Attach target to tongue
        target.transform.SetParent(tongueCollider);
        target.transform.localScale = target.transform.localScale * 0.75f;

        // Start retracting tongue, wait until it's retracted
        StartCoroutine(TongueRetract(target.transform.position, 1));
        yield return new WaitUntil(() => tongueState == TongueState.Retracted);

        if (target.HasReward)
        {
            // Give player reward
        }

        // Destroy target
        SCR_ObjectReferenceManager.Instance.tongueTargets.Remove(target);
        Destroy(target.gameObject);
    }
    IEnumerator TongueRetract(Vector3 targetPos, float startingDistance)
    {
        float TongueDistance = startingDistance;
        while (TongueDistance > 0)
        {
            yield return new WaitForEndOfFrame();

            // Linearly interpolate tongue's position from target back to body 
            // (promote speed to variable later)
            TongueDistance -= 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, targetPos, TongueDistance);
        }
        tongueCollider.position = tongueParent.position;

        tongueState = TongueState.Retracted;
    }
    #endregion

    #region Input actions
    void InputActions()
    {
        // Set input event functions

        // Set Hold targeting / Press targeting
        if (variables.holdToTarget)
        {
            controls.Player.HoldtoTargetPress.performed += ctx => InitiateLockOn();
            controls.Player.HoldtoTargetRelease.performed += ctx => UnlockTargeting();
        }
        else
        {
            controls.Player.PresstoTarget.performed += ctx => PressTarget();
        }

        controls.Player.TongueButtonPress.performed += ctx => holdingTongueButton = true;
        controls.Player.TongueButtonRelease.performed += ctx => holdingTongueButton = false;
    }
    #endregion

    #region Gizmos
    float targetSphereSize = 0;
    Vector3 targetIconPosition;
    private void OnDrawGizmos()
    {
        #region Highlight Target gizmos
        Gizmos.color = Color.red;

        // Set color of targeting icon to indicate target locking
        Color targetingColor;
        if (lockedOnTarget)
            targetingColor = new Color(1, .6f, .6f);
        else
            targetingColor = new Color(1, 0, 0);

        if (currentTarget != null)
        {
            // Show target icon and wire sphere on current target
            targetIconPosition = Vector3.Lerp(targetIconPosition, currentTarget.transform.position, 20 * Time.deltaTime);
            targetSphereSize = Mathf.MoveTowards(targetSphereSize, 0.5f, 4 * Time.deltaTime);
            Gizmos.DrawIcon(targetIconPosition + currentTarget.targetIconOffset, "SPR_TargetTriangle128.png", false, targetingColor);
        }
        else
        {
            // If there is no target, shrink wire sphere to oblivion
            targetSphereSize = Mathf.MoveTowards(targetSphereSize, 0.0f, 4 * Time.deltaTime);
        }
        Gizmos.DrawWireSphere(targetIconPosition, targetSphereSize);
        #endregion

        #region Show Target Range gizmos
        Gizmos.color = Color.blue;

        // you know what, fuck this whole part

        //if (Application.isPlaying)
        //{
        //    DrawRelativeDirLine(new Vector3(0, variables.maxTargetAngle, 0), tongueParent);
        //    DrawRelativeDirLine(new Vector3(0, -variables.maxTargetAngle, 0), tongueParent);
        //    DrawRelativeDirLine(new Vector3(variables.maxTargetAngle, 0, 0), tongueParent);
        //    DrawRelativeDirLine(new Vector3(-variables.maxTargetAngle, 0, 0), tongueParent);

        //    void DrawRelativeDirLine(Vector3 angle, Transform lineParent)
        //    {
        //        Gizmos.DrawLine(lineParent.position, lineParent.position + ((Quaternion.Euler(angle) * lineParent.forward).normalized * variables.maxTargetDistance));
        //    }
        //}

        #endregion
    }
    #endregion
}
