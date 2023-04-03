using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection.Emit;

[CustomEditor(typeof(Animation))]
public class AnimationEditor : GeneralEditor
{
    public override void OnInspectorGUI()
    {
        Animation anim = (Animation)target;
        base.OnInspectorGUI();
        if(anim.transform.parent.GetComponentInParent<Animation>() == null)
        {
            CommonEditorElements.MakePlayerButton(anim.gameObject);
        }
        GUILayout.Label("Animation Options");
        if (GUILayout.Button(anim.animateInEditMode? "Pause" : "Play" ))
        {
            anim.animateInEditMode = !anim.animateInEditMode;
            anim.EditorPlay();
            EditorApplication.QueuePlayerLoopUpdate();
        }
        if(GUILayout.Button("Select Frame"))
        {
            Selection.activeTransform = anim.transform.GetChild(anim.currentFrame);
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            anim.AdvanceFrame(false);
        }
        if (GUILayout.Button(">"))
        {
            anim.AdvanceFrame(true);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Delete Frame"))
        {
            DestroyImmediate(anim.transform.GetChild(anim.currentFrame).gameObject);
            anim.Loop();
            anim.RefreshFrame();
        }
        if (GUILayout.Button("Duplicate Frame"))
        {
            GameObject newFrame = Instantiate(anim.transform.GetChild(anim.currentFrame).gameObject, anim.transform);
            newFrame.transform.SetSiblingIndex(anim.currentFrame + 1);
            newFrame.name = anim.transform.GetChild(anim.currentFrame).name + "_duplicate";
        }
        if (GUILayout.Button("Add Frame"))
        {
            ArtCreation window = CreateInstance<ArtCreation>();
            window.SetCustomParent(anim.transform);
            window.ShowPopup();
        }
        GUILayout.EndHorizontal();
        BaseInspectorGUI();
    }
}
