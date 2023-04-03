using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ActionTrigger))]
public class ActionTriggerEditor : Editor
{

    SerializedProperty triggerType;
    SerializedProperty moveToArea;
    SerializedProperty reveal;
    SerializedProperty hide;
    SerializedProperty audioClip;
    SerializedProperty scene;
    SerializedProperty oneshot;
    SerializedProperty toggleAudio;

    void OnEnable()
    {
        triggerType = serializedObject.FindProperty("triggerType");
        moveToArea = serializedObject.FindProperty("moveToArea");
        reveal = serializedObject.FindProperty("reveal");
        hide = serializedObject.FindProperty("hide");
        audioClip = serializedObject.FindProperty("audioClip");
        scene = serializedObject.FindProperty("scene");
        oneshot = serializedObject.FindProperty("oneshot");
        toggleAudio = serializedObject.FindProperty("toggleAudio");
    }

    public override void OnInspectorGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        ActionTrigger trigger = (ActionTrigger)target;
        /*if (GUILayout.Button("Add Collider"))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("circle"), false, OnAddCollider, "circle");
            menu.AddItem(new GUIContent("square"), false, OnAddCollider, "square");
            menu.ShowAsContext();
        }*/
        RenderTriggerType(trigger);
        GUILayout.Label("Trigger Settings");
        GUI.skin = null;
        switch (trigger.triggerType)
        {
            case TriggerType.GoToArea:
                moveToArea.objectReferenceValue = EditorGUILayout.ObjectField("Area to move to: ", moveToArea.objectReferenceValue, typeof(Area), true);
                break;
            case TriggerType.RevealAndHideObjects:
                reveal.objectReferenceValue = EditorGUILayout.ObjectField("Object to reveal: ", reveal.objectReferenceValue, typeof(GameObject), true);
                hide.objectReferenceValue = EditorGUILayout.ObjectField("Object to hide: ", hide.objectReferenceValue, typeof(GameObject), true);
                oneshot.boolValue = EditorGUILayout.Toggle("Show/hide art only the first time you enter the trigger?", oneshot.boolValue);
                break;
            case TriggerType.PlaySound:
                audioClip.objectReferenceValue = EditorGUILayout.ObjectField("Sound to play: ", audioClip.objectReferenceValue, typeof(AudioClip), false);
                oneshot.boolValue = EditorGUILayout.Toggle("Play sound only the first time you enter the trigger?", oneshot.boolValue);
                if (!oneshot.boolValue)
                {
                    toggleAudio.boolValue = EditorGUILayout.Toggle("Loop the sound when inside the trigger?", toggleAudio.boolValue);
                }
                break;
            case TriggerType.LoadUnityScene:
                scene.stringValue = EditorGUILayout.TextField("Scene to move to: ", scene.stringValue);
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void RenderTriggerType(ActionTrigger trigger)
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        //left
        GUILayout.Label("Trigger Type: ", GUILayout.ExpandWidth(false));
        if (GUILayout.Button("", GUI.skin.customStyles[3]))
        {
            int enumVal = (int)trigger.triggerType;
            enumVal--;
            if(enumVal < 0)
            {
                enumVal = Enum.GetValues(typeof(TriggerType)).Length - 1;
            }
            triggerType.enumValueIndex = enumVal;
        }
        GUILayout.Label(trigger.triggerType.ToString());
        //right
        if (GUILayout.Button("", GUI.skin.customStyles[4]))
        {
            int enumVal = (int)trigger.triggerType;
            enumVal++;
            if (enumVal >= Enum.GetValues(typeof(TriggerType)).Length)
            {
                enumVal = 0;
            }
            triggerType.enumValueIndex = enumVal;
        }
        GUILayout.EndHorizontal();
    }

    private void OnAddCollider(object objectType)
    {
        string input = (string)objectType;
        ActionTrigger trigger = (ActionTrigger)target;
        switch (input) {
            case "circle":
                CircleCollider2D ccol = trigger.gameObject.AddComponent<CircleCollider2D>();
                ccol.isTrigger = true;
                break;
            case "square":
                BoxCollider2D bcol = trigger.gameObject.AddComponent<BoxCollider2D>();
                bcol.isTrigger = true;
                break;
            default:
                break;
        }
    }
}

