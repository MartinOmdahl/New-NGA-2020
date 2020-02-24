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

    InputControls controls;
    SCR_PlayerV3 movement;

    private void Awake()
    {
        controls = new InputControls();
        movement = GetComponent<SCR_PlayerV3>();
    }

    void Update()
    {
        anim.SetBool("Walking", 
            controls.Player.Movement.ReadValue<Vector2>().magnitude > 0.05f
            && movement.touchingGround);

        anim.SetFloat("WalkSpeed", 1.4f * controls.Player.Movement.ReadValue<Vector2>().magnitude);
    }
}
