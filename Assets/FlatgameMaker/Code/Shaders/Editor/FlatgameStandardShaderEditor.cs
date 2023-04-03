using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class FlatgameStandardShaderEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        bool distortion = Array.IndexOf(targetMat.shaderKeywords, "Distortion") != -1;
        EditorGUI.BeginChangeCheck();
        distortion = EditorGUILayout.Toggle("Distortion Enabled", distortion);
        if (EditorGUI.EndChangeCheck())
        {
            if (distortion)
                targetMat.EnableKeyword("Distortion");
            else
                targetMat.DisableKeyword("Distortion");
        }
        base.OnGUI(materialEditor, properties);
    }
}
