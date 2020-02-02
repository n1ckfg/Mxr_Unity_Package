using UnityEngine;
using UnityEditor;
using System.Collections;

/* s3dGyroCamSBSEditor.js - revised 3/11/13
 * Please direct any bugs/comments/suggestions to hoberman@usc.edu.
 * FOV2GO for Unity Copyright (c) 2011-13 Perry Hoberman & MxR Lab. All rights reserved.
 * Usage: Put this script in the Editor folder. It provides a custom inspector for s3dGyroCamSBS.js
 */
[System.Serializable]
[UnityEditor.CustomEditor(typeof(s3dGyroCamSBS))]
public class s3dGyroCamSBSEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical("box", new GUILayoutOption[] {});
        this.target.touchRotatesHeading = EditorGUILayout.Toggle(new GUIContent("Touch Rotates Heading", "Horizontal Swipe Rotates Heading"), this.target.touchRotatesHeading, new GUILayoutOption[] {});
        this.target.setZeroToNorth = EditorGUILayout.Toggle(new GUIContent("Set Zero Heading to North", "Face Compass North on Startup"), this.target.setZeroToNorth, new GUILayoutOption[] {});
        this.target.checkForAutoRotation = EditorGUILayout.Toggle(new GUIContent("Check For Auto Rotation", "Leave off unless Auto Rotation is on"), this.target.checkForAutoRotation, new GUILayoutOption[] {});
        EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this.target);
        }
    }

}