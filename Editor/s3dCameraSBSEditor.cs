using UnityEngine;
using UnityEditor;
using System.Collections;

/* s3dCameraSBSEditor.js - revised 2/12/13
 * Please direct any bugs/comments/suggestions to hoberman@usc.edu.
 * FOV2GO for Unity Copyright (c) 2011-13 Perry Hoberman & MxR Lab. All rights reserved.
 * Usage: Put this script in the Editor folder. It provides a custom inspector for s3dCameraSBS.js
 */
[System.Serializable]
[UnityEditor.CustomEditor(typeof(s3dCameraSBS))]
public class s3dCameraSBSEditor : Editor
{

    private s3dCameraSBS target;

    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(110, 30);
        bool allowSceneObjects = !EditorUtility.IsPersistent(target);
        EditorGUILayout.BeginVertical("box", new GUILayoutOption[] {});
        target.interaxial = EditorGUILayout.IntSlider(new GUIContent("Interaxial (mm)", "Distance (in millimeters) between cameras."), (int) target.interaxial, 0, 1000, new GUILayoutOption[] {});
        target.zeroPrlxDist = EditorGUILayout.Slider(new GUIContent("Zero Prlx Dist (M)", "Distance (in meters) at which left and right images converge."), (float) target.zeroPrlxDist, 0.1f, 100, new GUILayoutOption[] {});
        target.cameraSelect = (cams_3D) EditorGUILayout.EnumPopup(new GUIContent("Camera Order", "Swap cameras for cross-eyed free-viewing."), target.cameraSelect, new GUILayoutOption[] {});
        target.H_I_T = EditorGUILayout.Slider(new GUIContent("H I T", "Horizontal Image Transform (default 0)"), (float) target.H_I_T, -25, 25, new GUILayoutOption[] {});
        target.sideBySideSqueezed = EditorGUILayout.Toggle(new GUIContent("Squeezed", "For 3DTV frame-compatible format"), target.sideBySideSqueezed, new GUILayoutOption[] {});
        target.usePhoneMask = EditorGUILayout.Toggle(new GUIContent("Use Phone Mask", "Mask for side-by-side mobile phone formats"), target.usePhoneMask, new GUILayoutOption[] {});
        if (target.usePhoneMask != null)
        {
            EditorGUI.indentLevel = 1;
            target.leftViewRect = EditorGUILayout.Vector4Field("Left View Rect (left bottom width height)", target.leftViewRect, new GUILayoutOption[] {});
            target.rightViewRect = EditorGUILayout.Vector4Field("Right View Rect (left bottom width height)", target.rightViewRect, new GUILayoutOption[] {});
            EditorGUI.indentLevel = 0;
        }
        EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}