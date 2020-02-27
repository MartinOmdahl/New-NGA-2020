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
            Quaternion angleToTarget = Quaternion.LookRotation((tongue.currentTarget.transform.position - transform.position).normalized, Vector3.up);

            float f_AngleBetween = Vector3.Angle(transform.forward, (tongue.currentTarget.transform.position - transform.position).normalized);

            //print(f_AngleBetween);

            if (f_AngleBetween > 50)
            {
                print(transform.InverseTransformDirection(tongue.currentTarget.transform.position - transform.position).normalized.x);

                if (transform.InverseTransformDirection(tongue.currentTarget.transform.position - transform.position).normalized.x > 0)
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
