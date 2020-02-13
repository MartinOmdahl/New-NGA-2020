using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_StaticSwingObject : MonoBehaviour
{
    public Transform mesh;

    ConfigurableJoint joint;
    SCR_TongueTarget targetComponent;

    float randomAnimationOffset;

    void Awake()
    {
        joint = GetComponentInChildren<ConfigurableJoint>();
        targetComponent = GetComponent<SCR_TongueTarget>();

        randomAnimationOffset = Random.Range(-4, 4);
    }

    void Update()
    {
        if (targetComponent.isBeingSwung)
        {
            mesh.position = Vector3.Lerp(mesh.position, transform.position, 10 * Time.deltaTime);
        }
        else
        {
            float YPos = transform.position.y + Mathf.Sin(Time.time * 4 + randomAnimationOffset) * 0.05f;
            mesh.position = new Vector3(transform.position.x, YPos, transform.position.z);
        }
    }
}
