using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_CamControl : MonoBehaviour
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
	public Transform lookTarget;
	InputControls controls;
    #endregion

    #region Public variables
    [Tooltip("Collision layers camera tries to avoid clipping into")]
    public LayerMask clippingDetectMask;
    #endregion

    #region Local Variables
    float yaw, pitch;
    float targetDistance;
	Vector3 RotationSmoothVelocity, currentRotation;
    float distanceSmoothVelocity;
	#endregion


	private void Awake()
	{
		controls = new InputControls();
        //transform.SetParent(null);
	}

    private void Start()
    {
        SCR_ObjectReferenceManager.Instance.playerCamera = GetComponent<Camera>();

        targetDistance = variables.camDistanceMinMax.y;
        transform.position = lookTarget.position - transform.forward * targetDistance;
    }

    private void LateUpdate()
	{
		Vector2 input = controls.Player.Camera.ReadValue<Vector2>();
		yaw += input.x * variables.camSensitivity * Time.deltaTime;
		pitch -= input.y * variables.camSensitivity * Time.deltaTime;
		pitch = Mathf.Clamp(pitch, variables.camPitchMinMax.x, variables.camPitchMinMax.y);

		currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref RotationSmoothVelocity, variables.camTurnSpeed);
		transform.eulerAngles = currentRotation;

        AvoidClipping();

        float smoothDistance = Mathf.SmoothDamp(Vector3.Distance(transform.position, lookTarget.position), targetDistance, ref distanceSmoothVelocity, Time.deltaTime, 60);
		transform.position = lookTarget.position - transform.forward * smoothDistance;
	}

    void AvoidClipping()
    {
        // Do a raycast from player to camera to find any obstructions, and move the camera closer if there are any.

        Vector3 clipRayDirection = (transform.position - lookTarget.position).normalized;
        if (Physics.Raycast(lookTarget.position, clipRayDirection, out RaycastHit clipHit, variables.camDistanceMinMax.y, clippingDetectMask, QueryTriggerInteraction.Ignore))
        {
            targetDistance = Vector3.Distance(lookTarget.position, clipHit.point) -0.5f;
        }
        else
        {
            targetDistance = variables.camDistanceMinMax.y;
        }

        targetDistance = Mathf.Clamp(targetDistance, variables.camDistanceMinMax.x, variables.camDistanceMinMax.y);
    }
}
