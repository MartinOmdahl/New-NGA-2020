using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayerV3 : MonoBehaviour
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
	InputControls controls;
	Rigidbody rb;
	Transform camT;
    #endregion

    #region Public variables
    public bool overrideNormalMovement = false;
    public bool overrideJump = false;
    public bool overrideRotation = false;
    public bool touchingGround;
    public bool canMidairJump = true;
    #endregion

    #region Local Variables
    bool running;
	float currentSpeed;
    Vector3 moveDirection;
	float moveRotationVelocity, moveSpeedVelocity;
	float rotationVelocity, speedVelocity;
	float targetRotation;
    int collisionCount;
    int offGroundFrames;
    float jumpCooldown;
    Vector3 groundNormal;
    #endregion

    #region Debug variables
    [ContextMenu("Show or hide ground normal")]
    void GroundNormalSwitch()
    {
        showGroundNormal = !showGroundNormal;
    }
    [HideInInspector]
    public bool showGroundNormal;

    #endregion

    private void Awake()
	{
		//REF:
        // Whoa change this please
		camT = GameObject.FindGameObjectWithTag("PlayerCam").transform;
		controls = new InputControls();
		rb = GetComponent<Rigidbody>();

		//Functions:
		InputActions();
	}

    private void FixedUpdate()
	{
        //Functions

        if (!overrideNormalMovement)
        {
            if (touchingGround)
            {
                GroundMovement();
            }
            else
            {
                AirMovement();
            }
        }

        if (!overrideRotation)
        {
            NormalRotation();
        }

        GroundDetect();
    }

    #region Horizontal movement

    void GroundMovement()
    {
        /* OK, so this is gonna need some explanation
         * This movement system originally worked by setting player's rotation, then moving them forward.
         * I've changed this to make movement independent of player's rotation. But the code still works mostly the same:
         * by setting a rotation and then moving player along the forward vector of that rotation. 
         */

        //Get Player Input:
        Vector2 input = controls.Player.Movement.ReadValue<Vector2>();

        // Find rotation of direction player should move in (it's weird, but truuuuuuuuust me)
        float targetMoveRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camT.eulerAngles.y;

        float targetSpeed;
        //If Input then set Target Rotation &Smoothly Rotate in Degrees:
        if (input.magnitude > 0.1f)
        {
        //moveDirection = Vector3.up * Mathf.SmoothDampAngle(moveDirection.y, targetMoveRotation, ref moveRotationVelocity, variables.playerTurnSpeed);
        moveDirection = Vector3.up * targetMoveRotation;

            targetSpeed = (running ? variables.runSpeed : variables.walkSpeed) * input.magnitude;
        }
        else
        {
            targetSpeed = 0;
        }

        //Check if Running & Set Speed Accordingly:
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref moveSpeedVelocity, variables.Acceleration);

        //Move the Player:
        Vector3 velocity = (Quaternion.Euler(moveDirection) * Vector3.forward) * currentSpeed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);



        // Should be updated to move along ground normal
        // Player mesh should also be rotated to fit ground normal.

        // Currently missing: Set move rotation in air as well
    }

    void AirMovement()
    {
        //Get Player Input:
        Vector2 input = controls.Player.Movement.ReadValue<Vector2>();

        //If Input then set Target Rotation & Smoothly Rotate in Degrees:
        //if (input.magnitude > 0.1f)
        //{
        //    targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
        //}
        //transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, variables.playerTurnSpeed);

        //Check if Running & Set Speed Accordingly:
        float targetSpeed = (running ? variables.runSpeed : variables.walkSpeed) * input.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, variables.Acceleration);

        //Move the Player

        Vector3 velocity = transform.forward * currentSpeed;
        //rb.AddForce(velocity, ForceMode.Acceleration);

        rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(velocity.x, rb.velocity.y, velocity.z), 0.1f);
    }

    void OnLanding()
    {
        // This function runs when player lands on ground after being in the air.

        moveDirection = transform.eulerAngles;

    }

    #endregion

    #region Jumping

    void JumpCheck()
    {
        // Check what kind of jump should be performed, if any
        if (touchingGround && !overrideJump)
        {
            StartCoroutine(GroundJump());
            StartCoroutine(AirJumpCooldown());
            
        }
        else if (canMidairJump && !overrideJump)
        {
            StartCoroutine(MidairJump(jumpCooldown));
            canMidairJump = false;
        }
    }

    IEnumerator GroundJump()
    {
        yield return null;

        // Change everything about this later, please
        rb.velocity = new Vector3(rb.velocity.x, 6, rb.velocity.z);

        Debug.DrawLine(transform.position, transform.position + Vector3.up, Color.red, 0.1f);
    }

    IEnumerator AirJumpCooldown()
    {
        jumpCooldown = variables.airJumpCooldown;
        while (jumpCooldown > 0)
        {
            yield return null;
            jumpCooldown -= Time.deltaTime;
        }
        jumpCooldown = 0;
    }

    IEnumerator MidairJump(float buffer)
    {
        yield return null;

        // If button was pressed before cooldown ended, buffer the jump
        yield return new WaitForSeconds(buffer);

        if (!overrideNormalMovement && !touchingGround)
        {
            // oh god this hurts to look at
            rb.velocity = new Vector3(rb.velocity.x, 4f, rb.velocity.z);

            Debug.DrawLine(transform.position, transform.position + Vector3.up / 2, Color.blue, 0.1f);
        }
    }

    #endregion

    #region Ground Detection

    void GroundDetect()
    {
        // Keep track of how many frames player has been in air;
        if (!touchingGround)
            offGroundFrames++;

        // Set touchingGround to false before checking for ground collision.
        // This works because FixedUpdate is before OnCollisionStay in execution order.
        touchingGround = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Attach player to moving platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(collision.transform);
        }

        // Keep track of number of colliding bodies
        collisionCount++;
    }

    private void OnCollisionExit(Collision collision)
    {
        // Detach player from moving platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(null);
        }

        // Keep track of number of colliding bodies
        collisionCount--;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Go through each contact point and check for an appropriate ground angle.
        foreach (var contact in collision.contacts)
        {
            float contactAngle = Vector3.Angle(Vector3.up, contact.normal);
            
            if (contactAngle < variables.maxGroundAngle)
            {
                if (offGroundFrames > 1)
                    OnLanding();

                touchingGround = true;
                offGroundFrames = 0;
                groundNormal = contact.normal;
                canMidairJump = true;
            }
        }
    }

    #endregion

    #region Animation

    void NormalRotation()
    {
        //Get Player Input:
        Vector2 input = controls.Player.Movement.ReadValue<Vector2>();

        //If Input then set Target Rotation & Smoothly Rotate in Degrees:
        if (input.magnitude > 0.1f)
        {
            targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
        }
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, variables.playerTurnSpeed);
    }

    #endregion

    #region Input actions

    void InputActions()
	{
		//Set Jump:
		controls.Player.Jump.performed += ctx => JumpCheck();

		//Check and Set Running:
		if (variables.holdToRun)
			controls.Player.HoldtoRun.performed += ctx => running = !running;
		else
			controls.Player.PresstoRun.performed += ctx => running = !running;
	}

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (touchingGround && showGroundNormal)
        {
            Gizmos.DrawLine(transform.position, transform.position + groundNormal);
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }
    }
}
