using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FieldOfView))]
/// <summary>
/// Custom Editor script for visualizing the Field of View (FOV) in the Unity Editor Scene view.
/// </summary>
public class FOVEditor : Editor
{
    /// <summary>
    /// Draws the FOV visualization in the Scene view.
    /// </summary>
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.GetFOVRadius());
        Vector3 viewAngleA = fov.DirFromAngle(-fov.GetFOVAngle() / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.GetFOVAngle() / 2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.GetFOVRadius());
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.GetFOVRadius());

        // Draws lines to visible targets
        Handles.color = Color.red;
        foreach (Transform visibleTargets in fov.GetVisibleTargets())
        {
            Handles.DrawLine(fov.transform.position, visibleTargets.position);
        }
    }
}
