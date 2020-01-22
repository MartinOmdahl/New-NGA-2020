using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default_Variables", menuName = "Variables/Game Variables")]
public class SCR_Variables : ScriptableObject
{
	[Header("World Settings")]
	public float gravity = -15f;


	[Header("Player movement")]
	public bool holdToRun = true;
	public float walkSpeed = 4;
	public float runSpeed = 8;

	public float playerTurnSpeed = 0.1f;
	public float Acceleration = 0.1f;

	public float jumpHeight = 1.5f;
	[Range(0,1)]
	public float airControlPercent = .5f;

    [Header("Player tongue")]
    public float maxTargetDistance = 5;
    [Tooltip("Max angle target can be, relative to player." +
        "\n An angle of 0 is directly in front of player.")]
    public float maxTargetAngle = 90;
    public bool holdToTarget = true;


	[Header("Camera")]
	public float sensitivity = 1;
	public float distanceFromTarget = 3;
	public Vector2 pitchMinMax = new Vector2(-40, 85);

	public float camTurnSpeed = 0.1f;
}
