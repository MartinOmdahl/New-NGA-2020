using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

// General script for all tongue-interactable objects
public class SCR_TongueTarget : MonoBehaviour
{
    // On target types
    // Swing: Frog hangs and swings from target.
    // Eat: Frog instantly eats target.
    // Grab: Frog can grab target and pull it.
    // Deflect: Frog tongue immediately retracts after hitting target.
    public enum TargetType { Swing, Eat, Grab, Deflect}
    public TargetType targetType = TargetType.Swing;

    // Swing variables
    [Tooltip("Whether player can start swing while grounded")]
    public bool canSwingFromGround = true;

    // Eat variables
    [Tooltip("Whether player gets something after eating this object")]
    public bool HasReward;
    [HideInInspector]
    public bool isBeingEaten;

    public Vector3 targetIconOffset = new Vector3(0, 0.5f, 0);
    void Start()
    {
        // Add self to list of all tongue targets
        // Need to be updated to remove if destroyed!
        SCR_ObjectReferenceManager.Instance.tongueTargets.Add(this);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize target icon position
        Gizmos.DrawIcon(transform.position + targetIconOffset, "SPR_TargetTriangle128.png", false, Color.white);
    }
}

// Editor class to show different variables based on target type
[CustomEditor(typeof(SCR_TongueTarget))]
public class DynamicVariables : Editor
{
    public override void OnInspectorGUI()
    {
        SCR_TongueTarget tongueTarget = target as SCR_TongueTarget;

        tongueTarget.targetType = (SCR_TongueTarget.TargetType)EditorGUILayout.EnumPopup("Target Type", tongueTarget.targetType);

        switch (tongueTarget.targetType)
        {
            case SCR_TongueTarget.TargetType.Swing:
                tongueTarget.canSwingFromGround = EditorGUILayout.Toggle("Can Swing From Ground", tongueTarget.canSwingFromGround);
                break;

            case SCR_TongueTarget.TargetType.Eat:
                tongueTarget.HasReward = EditorGUILayout.Toggle("Has Reward", tongueTarget.HasReward);
                break;

            case SCR_TongueTarget.TargetType.Grab:

                break;

            case SCR_TongueTarget.TargetType.Deflect:

                break;

            default:
                break;
        }
        EditorGUILayout.Space();

        tongueTarget.targetIconOffset = EditorGUILayout.Vector3Field("Target Icon Offset", tongueTarget.targetIconOffset);
    }
}
