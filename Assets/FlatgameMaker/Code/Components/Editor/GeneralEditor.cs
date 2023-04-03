using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GeneralEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        MonoBehaviour targetBehaviour = (MonoBehaviour)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(GUIUtils.UpButton))
        {
            MoveInHeirarchy(targetBehaviour.transform, -1);
        }
        if (GUILayout.Button(GUIUtils.DownButton))
        {
            MoveInHeirarchy(targetBehaviour.transform, 1);
        }
        GUILayout.EndHorizontal();


    }

    private void MoveInHeirarchy(Transform trans, int delta)
    {
        int startIndex = trans.GetSiblingIndex();
        trans.SetSiblingIndex(startIndex + delta);
        int endIndex = trans.GetSiblingIndex();
        if (endIndex - startIndex == 0)
        {
            trans.SetAsFirstSibling();
        }
    }

    public void BaseInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
