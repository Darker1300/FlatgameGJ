using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUIUtils : MonoBehaviour
{
    public static string guiArtPath = "Assets/FlatgameMaker/Resources/Art/GUI/";
    public static string guiSkinPath = "Assets/FlatgameMaker/Resources/GUISkin/Default.guiskin";
    private static GUISkin defaultSkin;
    public static GUISkin DefaultSkin
    {
        get
        {
            if (defaultSkin == null)
            {
                #if UNITY_EDITOR
                defaultSkin = (GUISkin)AssetDatabase.LoadAssetAtPath(guiSkinPath, typeof(GUISkin));
                #endif
            }
            return defaultSkin;
        }
    }

    private static GUIContent upButton;
    public static GUIContent UpButton
    {
        get
        {
            if (upButton == null)
            {
                upButton = new GUIContent("Move Up", GetGUITexture("UpArrow"));
            }
            return upButton;
        }
    }

    private static GUIContent downButton;
    public static GUIContent DownButton
    {
        get
        {
            if (downButton == null)
            {
                downButton = new GUIContent("Move Down", GetGUITexture("DownArrow"));
            }
            return downButton;
        }
    }

    private static GUIContent addButton;
    public static GUIContent AddButton
    {
        get
        {
            if (addButton == null)
            {
                addButton = new GUIContent("Add Something New", GetGUITexture("Plus"));
            }
            return addButton;
        }
    }

    private static GUIContent shaderUButton;
    public static GUIContent ShaderUButton
    {
        get
        {
            if (shaderUButton == null)
            {
                shaderUButton = new GUIContent("Edit Shader", GetGUITexture("RightArrow"));
            }
            return shaderUButton;
        }
    }

    private static GUIContent shaderDButton;
    public static GUIContent ShaderDButton
    {
        get
        {
            if (shaderDButton == null)
            {
                shaderDButton = new GUIContent("Edit Shader", GetGUITexture("DownArrow"));
            }
            return shaderDButton;
        }
    }

    public static bool DropdownButton(string label,bool down, float width = 0)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        if (width == 0) {
            if (GUILayout.Button(DropdownButtonContent(label, down)))
            {
                down = !down;
            }
        }
        else
        {
            if (GUILayout.Button(DropdownButtonContent(label, down),GUILayout.Width(width)))
            {
                down = !down;
            }
        }
        return down;
    }

    public static void EndDropdown()
    {
        GUILayout.EndVertical();
    }

    static GUIContent DropdownButtonContent(string label, bool down)
    {
        return new GUIContent(label, down? GetGUITexture("DownArrow"): GetGUITexture("RightArrow"));
    }

    public static Texture2D GetGUITexture(string name)
    {
        #if UNITY_EDITOR
        return (Texture2D)AssetDatabase.LoadAssetAtPath(guiArtPath + name + ".png", typeof(Texture2D));
        #endif
        return null;
    }

#if UNITY_EDITOR
    public static Color ColorField(string label, Color col)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUI.skin = null;
        col = EditorGUILayout.ColorField(col, GUILayout.Height(32), GUILayout.Width(64));
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.EndHorizontal();
        return col;
    }

    public static Texture2D TextureField(Material mat, Texture2D mainTex, string texName, string label)
    {
        GUILayout.Label(label);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        mat.SetTextureScale(texName, Vector2Field("Scale", mat.GetTextureScale(texName), 80));
        mat.SetTextureOffset(texName, Vector2Field("Offset", mat.GetTextureOffset(texName), 80));
        GUILayout.EndVertical();
        GUI.skin = null;
        mainTex = (Texture2D)EditorGUILayout.ObjectField(mainTex, typeof(Texture2D), false, GUILayout.Height(64), GUILayout.Width(64));
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.EndHorizontal();
        return mainTex;
    }

    public static Vector2 Vector2Field(string label, Vector2 value, float labelwidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(labelwidth));
        value.x = EditorGUILayout.FloatField(value.x, GUI.skin.textField);
        value.y = EditorGUILayout.FloatField(value.y, GUI.skin.textField);
        if (GUILayout.Button("X2"))
        {
            value.x *= 2;
            value.y *= 2;
        }
        if (GUILayout.Button("/2"))
        {
            value.x /= 2;
            value.y /= 2;
        }
        GUILayout.EndHorizontal();
        return value;
    }

    public static float FloatSliderField(string label, float val, float min, float max, float sliderWidth, float labelWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        val = GUILayout.HorizontalSlider(val, min, max, GUILayout.MaxWidth(sliderWidth));
        val = EditorGUILayout.FloatField(val, GUI.skin.textField, GUILayout.Width(80));
        val = Mathf.Clamp(val, min, max);
        GUILayout.EndHorizontal();
        return val;
    }
#endif

}
