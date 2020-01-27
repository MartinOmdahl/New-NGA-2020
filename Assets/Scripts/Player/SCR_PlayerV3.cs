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
    #endregion

    #region Public variables
    public bool usingNormalMovement = true;
    public bool touchingGround;
    public bool canMidairJump = true;
    #endregion

    #region Local Variables
    bool running;
	float currentSpeed;
	float rotationVelocity, speedVelocity;
	float targetRotation;
	Transform camT;
    int collisionCount;
	#endregion

	private void Awake()
	{
		//REF:
		camT = GameObject.FindGameObjectWithTag("PlayerCam").transform;
		controls = new InputControls();
		rb = GetComponent<Rigidbody>();

		//Functions:
		InputActions();
	}

	private void FixedUpdate()
	{
        //Functions

        if (usingNormalMovement)
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

        GroundDetect();
    }

    #region Horizontal movement

    void GroundMovement()
    {
        //Get Player Input:
        Vector2 input = controls.Player.Movement.ReadValue<Vector2>();

        //If Input then set Target Rotation & Smoothly Rotate in Degrees:
        if (input.magnitude > 0.1f)
        {
            targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
        }
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, variables.playerTurnSpeed);

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
        if (input.magnitude > 0.1f)
        {
            targetRotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camT.eulerAngles.y;
        }
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, variables.playerTurnSpeed);

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
        if (touchingGround)
        {
            StartCoroutine(GroundJump());
        }
        else if (canMidairJump)
        {
            StartCoroutine(MidairJump());
        }
    }

    IEnumerator GroundJump()
    {
        // Change everything about this later, please
        rb.velocity = new Vector3(rb.velocity.x, 6, rb.velocity.z);

        // Cooldown for airjumping after jumping (promote to variable later)
        canMidairJump = false;
        yield return new WaitForSeconds(0.5f);
        canMidairJump = true;
    }

    IEnumerator MidairJump()
    {
        yield return null;
        canMidairJump = false;
        
        // oh god this hurts to look at
        rb.velocity = new Vector3(rb.velocity.x, 4f, rb.velocity.z);
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
                canMidairJump = true;
            }
        }
    }

    #endregion

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
}
