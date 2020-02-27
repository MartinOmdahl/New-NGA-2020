using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayerAnimation : MonoBehaviour
{
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    public Animator anim;
    public MeshRenderer eyesRenderer;

    public Texture eyesForward, eyesLeft, eyesRight;

    InputControls controls;
    SCR_PlayerV3 movement;
    SCR_Tongue tongue;

    private void Awake()
    {
        controls = new InputControls();
        movement = GetComponent<SCR_PlayerV3>();
        tongue = GetComponent<SCR_Tongue>();
    }

    void Update()
    {
        anim.SetBool("Walking",
            controls.Player.Movement.ReadValue<Vector2>().magnitude > 0.05f
            && movement.touchingGround);

        anim.SetFloat("WalkSpeed", 1.4f * controls.Player.Movement.ReadValue<Vector2>().magnitude);

        EyeAnimation();
    }

    void EyeAnimation()
    {
        if (tongue.currentTarget != null)
        {
            // Get direction between player and target (ignoring height difference)
            Vector3 targetDirection = (tongue.currentTarget.transform.position - new Vector3(transform.position.x, tongue.currentTarget.transform.position.y, transform.position.z)).normalized;

            // Get angle of target direction relative to player's rotation
            float angleToTarget = Vector3.Angle(transform.forward, targetDirection);

            if (angleToTarget > 30)
            {
                // Check if object is to the left or right of player
                if (transform.InverseTransformDirection(targetDirection).x > 0)
                {
                    eyesRenderer.material.mainTexture = eyesRight;
                }
                else
                {
                    eyesRenderer.material.mainTexture = eyesLeft;
                }
            }
            else
            {
                eyesRenderer.material.mainTexture = eyesForward;
            }
        }
        else if (eyesRenderer.material.mainTexture != eyesForward)
        {
            eyesRenderer.material.mainTexture = eyesForward;
        }
    }
}
