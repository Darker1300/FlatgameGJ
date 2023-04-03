using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class PlayerEditor : GeneralEditor
{

    SerializedProperty flip;
    SerializedProperty turn;
    SerializedProperty up;
    SerializedProperty down;
    SerializedProperty left;
    SerializedProperty right;
    SerializedProperty movementSpeed;
    SerializedProperty normaliseMovement;
    SerializedProperty useAxes;
    SerializedProperty vertical;
    SerializedProperty horizontal;

    private void OnEnable()
    {
        flip = serializedObject.FindProperty("flipWithMovement");
        turn = serializedObject.FindProperty("turnWithMovement");
        up = serializedObject.FindProperty("upKey");
        down = serializedObject.FindProperty("downKey");
        left = serializedObject.FindProperty("leftKey");
        right = serializedObject.FindProperty("rightKey");
        movementSpeed = serializedObject.FindProperty("movementSpeed");
        normaliseMovement = serializedObject.FindProperty("normaliseMovement");
        useAxes = serializedObject.FindProperty("useUnityAxisInput");
        vertical = serializedObject.FindProperty("verticalAxis");
        horizontal = serializedObject.FindProperty("horizontalAxis");
    }

    public override void OnInspectorGUI()
    {
        
        Player player = (Player)target;
        base.OnInspectorGUI();
        GUI.skin = GUIUtils.DefaultSkin;
        EditorGUILayout.LabelField("Movement Settings Settings");
        EditorGUILayout.PropertyField(movementSpeed);
        EditorGUILayout.PropertyField(normaliseMovement);
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Input Settings",EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useAxes);

        if (player.useUnityAxisInput)
        {
            EditorGUILayout.PropertyField(vertical);
            EditorGUILayout.PropertyField(horizontal);
        }
        else
        {
            EditorGUILayout.PropertyField(up);
            EditorGUILayout.PropertyField(down);
            EditorGUILayout.PropertyField(left);
            EditorGUILayout.PropertyField(right);
        }


        EditorGUILayout.PropertyField(flip);
        EditorGUILayout.PropertyField(turn);

        serializedObject.ApplyModifiedProperties();
    }

}
