using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Area))]
public class AreaEditor : Editor
{
    bool newStart;
    SerializedProperty boundsType;
    SerializedProperty boundsOffset;
    SerializedProperty areaName;
    SerializedProperty startArea;
    SerializedProperty startMusic;
    SerializedProperty bounds;
    void OnEnable()
    {
        boundsType = serializedObject.FindProperty("boundsCollision");
        boundsOffset = serializedObject.FindProperty("boundsCollisionOffset");
        areaName = serializedObject.FindProperty("areaName");
        startArea = serializedObject.FindProperty("startArea");
        startMusic = serializedObject.FindProperty("playAudioFromStart");
        bounds = serializedObject.FindProperty("bounds");
    }
    public override void OnInspectorGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        Area area = (Area)target;
        Rect r = bounds.rectValue;
        r.size = EditorGUILayout.Vector2Field("Bounds size:", r.size);
        r.center = bounds.rectValue.center;
        bounds.rectValue = r;
        EditorGUILayout.PropertyField(boundsType);
        boundsOffset.floatValue = EditorGUILayout.FloatField("Bounds collision border", boundsOffset.floatValue);
        areaName.stringValue = EditorGUILayout.TextField("Area Name: ", areaName.stringValue);
        startMusic.boolValue = EditorGUILayout.Toggle("Start music on entering area: ", startMusic.boolValue);
        newStart = EditorGUILayout.Toggle("Is Starting Area: ",area.startArea);
        if(startArea.boolValue != newStart)
        {
            startArea.boolValue = newStart;
            AreaManager.Instance.UpdateStartArea(area);
        }
        if(GUILayout.Button("Select Area Player"))
        {
            AreaManager.Instance.MoveTo(area.AreaPlayer.transform);
        }
        if(GUILayout.Button("Calculate Bounds from contents"))
        {
            area.RecalculateBoundsFromContents();
        }
        if(area.GetComponent<AudioSource>() == null)
        {
            if (GUILayout.Button("Add Music"))
            {
                int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<AudioClip>(null, false, "", controlID);
            }
        }
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            AudioClip clip = (AudioClip)EditorGUIUtility.GetObjectPickerObject();
            if(clip != null)
            {
                area.AddMusic(clip);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
