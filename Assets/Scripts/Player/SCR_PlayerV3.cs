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
	float rotationVelocity, speedVelocity;
	float targetRotation;
    int collisionCount;
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

        //Move the Player:
        Vector3 velocity = transform.forward * currentSpeed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
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
            yield return new WaitForEndOfFrame();
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
                touchingGround = true;
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
