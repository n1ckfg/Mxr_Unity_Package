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
        bool allowSceneObjects = !EditorUtility.IsPersistent(this.target);
        EditorGUILayout.BeginVertical("box", new GUILayoutOption[] {});
        this.target.interaxial = EditorGUILayout.IntSlider(new GUIContent("Interaxial (mm)", "Distance (in millimeters) between cameras."), (int) this.target.interaxial, 0, 1000, new GUILayoutOption[] {});
        this.target.zeroPrlxDist = EditorGUILayout.Slider(new GUIContent("Zero Prlx Dist (M)", "Distance (in meters) at which left and right images converge."), (float) this.target.zeroPrlxDist, 0.1f, 100, new GUILayoutOption[] {});
        this.target.cameraSelect = (cams_3D) EditorGUILayout.EnumPopup(new GUIContent("Camera Order", "Swap cameras for cross-eyed free-viewing."), this.target.cameraSelect, new GUILayoutOption[] {});
        this.target.H_I_T = EditorGUILayout.Slider(new GUIContent("H I T", "Horizontal Image Transform (default 0)"), (float) this.target.H_I_T, -25, 25, new GUILayoutOption[] {});
        this.target.sideBySideSqueezed = EditorGUILayout.Toggle(new GUIContent("Squeezed", "For 3DTV frame-compatible format"), this.target.sideBySideSqueezed, new GUILayoutOption[] {});
        this.target.usePhoneMask = EditorGUILayout.Toggle(new GUIContent("Use Phone Mask", "Mask for side-by-side mobile phone formats"), this.target.usePhoneMask, new GUILayoutOption[] {});
        if (this.target.usePhoneMask != null)
        {
            EditorGUI.indentLevel = 1;
            this.target.leftViewRect = EditorGUILayout.Vector4Field("Left View Rect (left bottom width height)", this.target.leftViewRect, new GUILayoutOption[] {});
            this.target.rightViewRect = EditorGUILayout.Vector4Field("Right View Rect (left bottom width height)", this.target.rightViewRect, new GUILayoutOption[] {});
            EditorGUI.indentLevel = 0;
        }
        EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this.target);
        }
    }

}