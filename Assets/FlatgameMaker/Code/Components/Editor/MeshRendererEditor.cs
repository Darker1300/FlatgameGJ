using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshRenderer renderer = (MeshRenderer)target;
        GUILayout.Label("Render queue: " + renderer.sharedMaterial.renderQueue);
    }
}
