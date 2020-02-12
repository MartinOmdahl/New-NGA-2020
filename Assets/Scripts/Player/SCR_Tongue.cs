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

    #region Public variables
    #endregion

    #region Local variables
    SCR_TongueTarget currentTarget;
    bool lockedOnTarget;
    bool holdingTongueButton;
    bool grabbedOnTarget;
    bool grabTerminated;
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
    /*
     * This region contains the targeting behavior used to determine which object Tongue Attack should target.
     * 
     * FUNCTIONS IN THIS REGION:
     * Auto Target
     * Press Target
     * Initiate Lock-On
     * Unlock Targeting
     * Locked Target
     */

    void AutoTarget()
    {
        currentTarget = null;

        // Go through each target in scene. If it is within range & angle of player, and no other target is closer, set it as current target.
        foreach (var target in SCR_ObjectReferenceManager.Instance.tongueTargets)
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

    #region Tongue attack & retract
    /*
     * This region contains the Tongue Attack and Retract that constitute the beginning and end of every tongue interaction.
     * 
     * FUNCTIONS IN THIS REGION:
     * Tongue Attack
     * Tongue Retract
     */
    IEnumerator TongueAttack(SCR_TongueTarget target)
    {
        float TongueDistance = 0;
        while (TongueDistance < 1)
        {
            yield return null;

            // Linearly interpolate tongue's position from body to target
            // (promote speed to variable later)
            TongueDistance += 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, target.transform.position, variables.tongueAttackCurve.Evaluate(TongueDistance));
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
                if (movement.touchingGround)
                    StartCoroutine(TongueGrab(target));
                else
                    StartCoroutine(TongueRetract(target.transform.position, 1));
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

    IEnumerator TongueRetract(Vector3 targetPos, float startingDistance)
    {
        float TongueDistance = startingDistance;
        while (TongueDistance > 0)
        {
            yield return null;

            // Linearly interpolate tongue's position from target back to body 
            // (promote speed to variable later)
            TongueDistance -= 10 * Time.deltaTime;
            tongueCollider.position = Vector3.Lerp(tongueParent.position, targetPos, TongueDistance);
        }
        tongueCollider.position = tongueParent.position;

        tongueState = TongueState.Retracted;
    }
    #endregion

    #region Tongue interactions
    /*
     * This region contains the various interactions that can be performed once tongue has hit a target.
     * The type of interaction performed is determined by checking the targetType of the target object.
     * 
     * FUNCTIONS IN THIS REGION:
     * Tongue Swing
     * Tongue Eat
     * Tongue Grab
     * Terminate Grab
     */

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
        movement.overrideNormalMovement = true;
        movement.overrideJump = true;
        movement.overrideRotation = true;
        movement.canMidairJump = false;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

        float swingTime = 0;
        while (holdingTongueButton || swingTime < 0.65f)
        {
            yield return null;
            swingTime += Time.deltaTime;

            // Add some forward rotation at the beginning of swing
            if (swingTime < 0.1f)
            {
                targetJointRb.AddRelativeTorque(-400 * Time.deltaTime, 0, 0, ForceMode.Impulse);
            }

            tongueCollider.position = target.transform.position;
        }
        // Break connection to joint
        targetJoint.connectedBody = null;

        // Set velocity on swing end. Change this in future to convert linear velocity to angular
        rb.velocity = rb.velocity.normalized * targetJointRb.angularVelocity.magnitude * 2;

        // Re-activate normal movement
        movement.overrideNormalMovement = false;
        movement.overrideJump = false;
        movement.overrideRotation = false;
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

        // Slow down time for a split second (experimental)
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        float time = 0;
        while (time < 0.08f)
        {
            yield return null;
            time += Time.deltaTime / Time.timeScale;
            tongueCollider.position = target.transform.position;
        }
        Time.timeScale = 1;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        // Attach target to tongue
        target.transform.SetParent(tongueCollider);
        target.transform.localScale = target.transform.localScale * 0.75f;

        // Start tongue retraction, wait until it finishes
        StartCoroutine(TongueRetract(target.transform.position, 1));
        yield return new WaitUntil(() => tongueState == TongueState.Retracted);

        if (target.HasReward)
        {
            // Give player reward
        }

        // Destroy target
        Destroy(target.gameObject);
    }

    IEnumerator TongueGrab(SCR_TongueTarget target)
    {
        grabbedOnTarget = true;
        
        // Stop targeting
        currentTarget = null;
        ConfigurableJoint targetJoint = target.GetComponentInChildren<ConfigurableJoint>();

        // Temporarily turn player towards target to set up correct joint anchor position
        Quaternion playerStartingRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(target.transform.position.x, tongueParent.position.y, target.transform.position.z) - tongueParent.position);
        transform.rotation = targetRotation;
        targetJoint.transform.rotation = targetRotation;

        // Connect player to target joint
        targetJoint.connectedBody = rb;

        // Set player's rotation back to where it was
        transform.rotation = playerStartingRotation;

        // get distance to grabbed object (this is used to determine if player is pulling on it)
        float maxTargetDistance = Vector3.Distance(tongueParent.position, targetJoint.transform.position);

        // Disable normal movement
        movement.overrideRotation = true;
        movement.overrideJump = true;

        float timeObjectPulled = 0;
        while (holdingTongueButton && !grabTerminated && movement.touchingGround)
        {
            yield return null;

            // Turn player towards target
            targetRotation = Quaternion.LookRotation(new Vector3(target.transform.position.x, tongueParent.position.y, target.transform.position.z) - tongueParent.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20 * Time.deltaTime);
            targetJoint.transform.rotation = targetRotation;
            
            if(Vector3.Distance(tongueParent.position, targetJoint.transform.position) <= maxTargetDistance + 0.2f)
            {
                // Behavior when not pulling on target (player is closer than when it was grabbed)

                // Allow free movement by updating joint's target distance
                targetJoint.targetPosition = new Vector3(0, 0, maxTargetDistance-Vector3.Distance(tongueParent.position, targetJoint.transform.position));

                // Tell target that it isn't being pulled
                target.isBeingPulled = false;
                timeObjectPulled = 0;
            }
            else 
            {
                // Behavior when pulling on target (player is farther away than when it was grabbed)

                // restrict player from moving any farther away
                targetJoint.targetPosition = new Vector3(0, 0, 0);

                target.isBeingPulled = true;

                // If pulling should trigger an event on target, trigger it
                timeObjectPulled += Time.deltaTime;
                if(timeObjectPulled >= target.timeToPull && target.eventOnPull)
                {
                    target.eventTriggered = true;
                }
            }

            tongueCollider.position = target.transform.position;
        }
        // Break connection to joint
        targetJoint.connectedBody = null;

        // Enable normal movement
        movement.overrideRotation = false;
        movement.overrideJump = false;

        target.isBeingPulled = false;
        grabTerminated = false;
        grabbedOnTarget = false;

        StartCoroutine(TongueRetract(target.transform.position, 1));
    }

    public void TerminateGrab()
    {
        if (grabbedOnTarget)
        {
            grabTerminated = true;
        }
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
