using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Lever : MonoBehaviour
{
    public bool useTimer;
    public float timeActive;
    public Transform stickTransform;

    public bool activated;
    bool animationPlaying;

    SCR_TongueTarget targetComponent;
    Animator anim;

    void Awake()
    {
        targetComponent = GetComponentInChildren<SCR_TongueTarget>();
        anim = GetComponent<Animator>();

        targetComponent.transform.eulerAngles = Vector3.zero;
    }

    void Update()
    {
        if (targetComponent.eventTriggered && !activated)
        {
            StartCoroutine(Activate());
        }
        print("Stick angle is" + stickTransform.localRotation.eulerAngles);
    }

    IEnumerator Activate()
    {
        targetComponent.eventTriggered = false;
        targetComponent.enabled = false;
        SCR_ObjectReferenceManager.Instance.player.GetComponent<SCR_Tongue>().TerminateGrab();


        //int i = 0;
        //while (stickTransform.localEulerAngles.x > 45.1f)
        //{
        //    i++;
        //    //print("looped through function " + i + " times. Angle is " + (stickTransform.localEulerAngles.x));  

        //    yield return null;
        //    stickTransform.localRotation = Quaternion.Slerp(stickTransform.localRotation, Quaternion.Euler(45, 0, 0), 2 * Time.deltaTime);
        //}

        anim.SetTrigger("FlipA");
        animationPlaying = true;
        yield return new WaitUntil(()=> animationPlaying == false);

        //stickTransform.localEulerAngles = new Vector3(45, 0, 0);

        activated = true;
        targetComponent.enabled = true;
        targetComponent.transform.eulerAngles = Vector3.zero;

    }

    public void flipDone()
    {
        animationPlaying = false;
    }
}
