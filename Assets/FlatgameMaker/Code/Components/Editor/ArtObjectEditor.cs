using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System;

[CustomEditor(typeof(ArtObject))]
public class ArtObjectEditor : GeneralEditor
{
    [SerializeField]
    bool editShader;
    [SerializeField]
    bool editBase;
    [SerializeField]
    bool editDistortion;
    [SerializeField]
    bool editColour;
    [SerializeField]
    StandardShaderWrapper matVals;

    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();
        ArtObject art = (ArtObject)target;
        if (art.transform.parent != null && art.transform.parent.GetComponentInParent<Animation>() == null && art.transform.parent.GetComponentInParent<Player>() == null)
        {
            CommonEditorElements.MakePlayerButton(art.gameObject);
        }
        GUILayout.Label("Art Options");
        if (GUILayout.Button("Match Image Proportions"))
        {
            art.ResizeFromTexture();
        }
        if (GUILayout.Button("Scale To Fill Area"))
        {
            art.ResizeToBounds();
        }
        if (art.transform.parent != null && art.transform.parent.GetComponentInParent<Animation>() == null)
        {
            if (GUILayout.Button("Make Animation"))
            {
                GameObject newAnimation = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.animationPrefabPath);
                newAnimation.name = art.gameObject.name;
                newAnimation.transform.parent = art.transform.parent;
                int siblingIndex = art.transform.GetSiblingIndex();
                art.transform.parent = newAnimation.transform;
                newAnimation.transform.SetSiblingIndex(siblingIndex);
                Selection.activeGameObject = newAnimation;
            }
        }
        if (art.gameObject.GetComponent<ActionTrigger>() == null)
        {
            if (GUILayout.Button("Make Trigger"))
            {
                art.gameObject.AddComponent<ActionTrigger>();
            }
        }
        if (art.gameObject.GetComponent<Juice>() == null)
        {
            if (GUILayout.Button("Add Juice"))
            {
                art.gameObject.AddComponent<Juice>();
            }
        }
        MeshRenderer mr = art.gameObject.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Material mat = mr.sharedMaterial;
            if (mat.shader.name == "Flatgame/StandardShader")
            {
                editShader = GUIUtils.DropdownButton("Edit Graphics", editShader);
                if (editShader)
                {
                    if(matVals == null)
                    {
                        matVals = new StandardShaderWrapper(mat);
                    }
                    editBase = GUIUtils.DropdownButton("Base properties", editBase);
                    if (editBase)
                    {
                        DrawBaseProperties(mat);
                    }
                    GUIUtils.EndDropdown();
                    editDistortion = GUIUtils.DropdownButton("Distortion", editDistortion);
                    if (editDistortion)
                    {
                        DrawDistortionProperties(mat);
                    }
                    GUIUtils.EndDropdown();
                    editColour = GUIUtils.DropdownButton("Modify Colours", editColour);
                    if (editColour)
                    {
                        DrawColourProperties(mat);
                    }
                    GUIUtils.EndDropdown();
                }
                GUIUtils.EndDropdown();
            }
        }
    }

    void DrawBaseProperties(Material mat)
    {
        matVals.MainTex = GUIUtils.TextureField(mat, matVals.MainTex, "_MainTex", "Main Texture");
        matVals.WorldSpaceMain = MatToggle(mat, matVals.WorldSpaceMain, "WorldSpaceMain", "Use World Space tiling");
        matVals.Offset = MatToggle(mat, matVals.Offset, "UVOffset", "Use texture Panning");
        matVals.OffsetSpeed = GUIUtils.Vector2Field("Panning Speed", matVals.OffsetSpeed, 200);
        matVals.OffsetFPS = GUIUtils.FloatSliderField("Panning FPS", matVals.OffsetFPS, 0, 30, 200, 150);
        GUILayout.Space(10);
    }

    void DrawDistortionProperties(Material mat)
    {
        matVals.Distortion = MatToggle(mat, matVals.Distortion, "Distortion", "Use Distortion");
        matVals.DistortionTex = GUIUtils.TextureField(mat, matVals.DistortionTex, "_DistortionTex", "Distortion Texture");
        matVals.WorldSpaceDist = MatToggle(mat, matVals.WorldSpaceDist, "WorldSpaceDist", "Use World Space Tiling");
        Vector4 distVals = matVals.DistortionVals;
        Vector2 distSpeed = new Vector2(distVals.x, distVals.y);
        Vector2 distAmt = new Vector2(distVals.z, distVals.w);
        distSpeed = GUIUtils.Vector2Field("Pan Speed", distSpeed, 200);
        distAmt = GUIUtils.Vector2Field("Distortion Amount", distAmt, 200);
        matVals.DistortionVals = new Vector4(distSpeed.x, distSpeed.y, distAmt.x, distAmt.y);
        matVals.DistortionFPS = GUIUtils.FloatSliderField("Distortion FPS", matVals.DistortionFPS, 0, 30, 200, 150);
        GUILayout.Space(10);
    }

    void DrawColourProperties(Material mat)
    {
        matVals.Color = GUIUtils.ColorField("Tint Colour", matVals.Color);
        matVals.HSVShift = MatToggle(mat, matVals.HSVShift, "HSV_Shift", "Use HSV Colour Shift");
        matVals.Hue = GUIUtils.FloatSliderField("Hue", matVals.Hue, -0.5f, 0.5f, 300, 120);
        matVals.Sat = GUIUtils.FloatSliderField("Saturation", matVals.Sat, -0.5f, 0.5f, 300, 120);
        matVals.Val = GUIUtils.FloatSliderField("Value", matVals.Val, -0.5f, 0.5f, 300, 120);
        matVals.Remap = MatToggle(mat, matVals.Remap, "Remap", "Use Black-White Remapping");
        matVals.BlVal = GUIUtils.FloatSliderField("Black Value", matVals.BlVal, 0, 1, 300, 120);
        matVals.BlackColor = GUIUtils.ColorField("Black Remap Colour", matVals.BlackColor);
        matVals.WhVal = GUIUtils.FloatSliderField("White Value", matVals.WhVal, 0, 1, 300, 120);
        matVals.WhiteColor = GUIUtils.ColorField("White Remap Colour", matVals.WhiteColor);
        matVals.Clipping = MatToggle(mat, matVals.Clipping, "Clipping", "Use Transparency Clipping");
        matVals.AlphaCutoff = GUIUtils.FloatSliderField("Transparency Clip Value", matVals.AlphaCutoff,0,1, 180, 240);
        GUILayout.Space(10);
    }

    private bool MatToggle(Material mat,bool val, string key, string name)
    {
        val = GUILayout.Toggle(val, name);
        if (mat.IsKeywordEnabled(key) != val)
        {
            if (val)
            {
                mat.EnableKeyword(key);
            }
            else
            {
                mat.DisableKeyword(key);
            }
        }
        return val;
    }
}

class StandardShaderWrapper
{
    Material mat;
    public bool HSVShift
    {
        get
        {
            return mat.GetFloat("_HSVShift") > 0;
        }
        set
        {
            mat.SetFloat("_HSVShift", value ? 1.0f : 0.0f);
        }
    }

    public bool Distortion
    {
        get
        {
            return mat.GetFloat("_Distortion") > 0;
        }
        set
        {
            mat.SetFloat("_Distortion", value ? 1 : 0);
        }
    }

    public bool Remap
    {
        get
        {
            return mat.GetFloat("_Remap") > 0;
        }
        set
        {
            mat.SetFloat("_Remap", value ? 1 : 0);
        }
    }

    public bool Offset
    {
        get
        {
            return mat.GetFloat("_Offset") != 0;
        }
        set
        {
            mat.SetFloat("_Offset", value ? 1 : 0);
        }
    }

    public bool Clipping
    {
        get
        {
            return mat.GetFloat("_Clipping") != 0;
        }
        set
        {
            mat.SetFloat("_Clipping", value ? 1 : 0);
        }
    }

    public bool WorldSpaceMain
    {
        get
        {
            return mat.GetFloat("_WorldSpaceMain") != 0;
        }
        set
        {
            mat.SetFloat("_WorldSpaceMain", value ? 1 : 0);
        }
    }

    public bool WorldSpaceDist
    {
        get
        {
            return mat.GetFloat("_WorldSpaceDist") != 0;
        }
        set
        {
            mat.SetFloat("_WorldSpaceDist", value ? 1 : 0);
        }
    }

    public Color Color
    {
        get
        {
            return mat.GetColor("_Color");
        }
        set
        {
            mat.SetColor("_Color", value);
        }
    }

    public Texture2D MainTex
    {
        get
        {
            return (Texture2D)mat.GetTexture("_MainTex");
        }
        set
        {
            mat.SetTexture("_MainTex", value);
        }
    }

    public float AlphaCutoff
    {
        get
        {
            return mat.GetFloat("_AlphaCutoff");
        }
        set
        {
            mat.SetFloat("_AlphaCutoff", value);
        }
    }

    public float Hue
    {
        get
        {
            return mat.GetFloat("_Hue");
        }
        set
        {
            mat.SetFloat("_Hue", value);
        }
    }

    public float Sat
    {
        get
        {
            return mat.GetFloat("_Sat");
        }
        set
        {
            mat.SetFloat("_Sat", value);
        }
    }

    public float Val
    {
        get
        {
            return mat.GetFloat("_Val");
        }
        set
        {
            mat.SetFloat("_Val", value);
        }
    }

    public Texture2D DistortionTex
    {
        get
        {
            return (Texture2D)mat.GetTexture("_DistortionTex");
        }
        set
        {
            mat.SetTexture("_DistortionTex", value);
        }
    }
    public Vector4 DistortionVals
    {
        get
        {
            return mat.GetVector("_DistortionVals");
        }
        set
        {
            mat.SetVector("_DistortionVals", value);
        }
    }
    public float DistortionFPS
    {
        get
        {
            return mat.GetFloat("_DistortionFPS");
        }
        set
        {
            mat.SetFloat("_DistortionFPS", value);
        }
    }

    public Color BlackColor
    {
        get
        {
            return mat.GetColor("_BlackColor");
        }
        set
        {
            mat.SetColor("_BlackColor", value);
        }
    }

    public Color WhiteColor
    {
        get
        {
            return mat.GetColor("_WhiteColor");
        }
        set
        {
            mat.SetColor("_WhiteColor", value);
        }
    }

    public float BlVal
    {
        get
        {
            return mat.GetFloat("_BlVal");
        }
        set
        {
            mat.SetFloat("_BlVal", value);
        }
    }

    public float WhVal
    {
        get
        {
            return mat.GetFloat("_WhVal");
        }
        set
        {
            mat.SetFloat("_WhVal", value);
        }
    }

    public Vector2 OffsetSpeed
    {
        get
        {
            return new Vector2(mat.GetFloat("_OffsetSpeedX"), mat.GetFloat("_OffsetSpeedY"));
        }
        set
        {
            mat.SetFloat("_OffsetSpeedX", value.x);
            mat.SetFloat("_OffsetSpeedY", value.y);
        }
    }

    public float OffsetFPS
    {
        get
        {
            return mat.GetFloat("_OffsetFPS");
        }
        set
        {
            mat.SetFloat("_OffsetFPS", value);
        }
    }

    public StandardShaderWrapper(Material mat)
    {
        this.mat = mat;
    }
}
