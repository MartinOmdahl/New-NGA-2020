using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_StaticSwingObject : MonoBehaviour
{
    public Transform mesh;

    ConfigurableJoint joint;
    SCR_TongueTarget targetComponent;

    void Awake()
    {
        joint = GetComponentInChildren<ConfigurableJoint>();
        targetComponent = GetComponent<SCR_TongueTarget>();
    }

    void Update()
    {

        float YPos = transform.position.y + Mathf.Sin(Time.time) * 2;


        mesh.position = new Vector3(transform.position.x, 0, transform.position.z);



    }


}
