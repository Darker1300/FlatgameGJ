using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public class WebcamImporter : EditorWindow
{
    WebCamTexture webcam;
    float textureScale = 0.6f;
    TextureByte.Sprite mask;
    TextureByte.Sprite brush;
    int brushSize = 64;
    Rect webcamRect;
    Rect renderRect;
    Rect screenRect;
    string renderMatPath = "Assets/FlatgameMaker/Resources/Materials/WebcamMat.mat";
    Material renderMat;
    Color overlayColour = new Color(1, 1, 1, 0.5f);
    Color brushColour = new Color(1, 1, 1, 0.5f);
    bool drawing;
    bool dragging;
    string saveName = "";
    bool drawn;
    int webcamCount;
    int currentCam;
    Vector2 prev;

    public ArtCreation artCreation;

    void OnEnable()
    {
        renderMat = (Material)AssetDatabase.LoadAssetAtPath(renderMatPath, typeof(Material));
        wantsMouseMove = true;
        brush = TextureByte.Draw.Circle(brushSize, 255);
        brush.Apply();
        UpdateWebcamDevice(0);
        saveName = GetUniqueName();
    }

    private string GetUniqueName()
    {
        string baseName = "WebcamImage";
        int fileNum = 0;
        string savePath = Path.Combine(Application.dataPath + "/Art", baseName + ".png");
        string retName = baseName;
        while (File.Exists(savePath))
        {
            retName = baseName + fileNum;
            savePath = Path.Combine(Application.dataPath + "/Art", retName + ".png");
            fileNum++;
        }
        return retName;
    }

    void UpdateWebcamDevice(int camId)
    {
        webcamCount = WebCamTexture.devices.Length;
        if(camId >= webcamCount)
        {
            camId = 0;
        }else if(camId < 0)
        {
            camId = webcamCount-1;
        }
        if(webcam != null)
        {
            webcam.Stop();
        }
        currentCam = camId;
        if (webcamCount > 0)
        {
            webcam = new WebCamTexture(WebCamTexture.devices[currentCam].name);
            webcam.Play();
            webcamRect = new Rect(0, 0, webcam.width * textureScale, webcam.height * textureScale);
        }
    }

    void OnGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.BeginVertical(GUI.skin.box);
        if(webcamCount == 0)
        {
            GUILayout.Label("No available webcam, please plug in a webcam to use this feature.",GUILayout.Width(900));
        }
        else if (drawing)
        {

            if (GUILayout.Button("Retry"))
            {
                RetryPhoto();
            }
            using (new EditorGUI.DisabledScope(!drawn))
            {
                if (GUILayout.Button("Accept"))
                {
                    AcceptPhoto();
                    Close();
                }
            }
            if (GUILayout.Button("Accept Full Image"))
            {
                AcceptFullPhoto();
                Close();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Image Save Name:", GUILayout.ExpandWidth(false));
            saveName = EditorGUILayout.TextField(saveName,GUIUtils.DefaultSkin.textField);
            GUILayout.EndHorizontal();
        }
        else
        {

            if (GUILayout.Button("Take Photo"))
            {
                TakePhoto();
            }
            if(webcamCount > 1)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("", GUI.skin.customStyles[3]))
                {
                    UpdateWebcamDevice(currentCam - 1);
                }
                GUILayout.Label(webcam.deviceName);
                if (GUILayout.Button("", GUI.skin.customStyles[4]))
                {
                    UpdateWebcamDevice(currentCam + 1);
                }
                GUILayout.EndHorizontal();
            }
        }
        if (webcamCount > 0)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Zoom = " + (int)(textureScale * 100) + " ", GUILayout.ExpandWidth(false));
            using (new EditorGUI.DisabledScope(textureScale < 0.2f))
            {
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    textureScale -= 0.1f;
                    webcamRect = new Rect(0, 0, webcam.width * textureScale, webcam.height * textureScale);
                }
            }

            using (new EditorGUI.DisabledScope(textureScale >= 2))
            {
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    textureScale += 0.1f;
                    webcamRect = new Rect(0, 0, webcam.width * textureScale, webcam.height * textureScale);
                }
            }

            if (drawing)
            {

                GUILayout.Label("Brush size : ", GUILayout.ExpandWidth(false));
                //brushSize = EditorGUILayout.IntSlider(brushSize, 1, 256, GUILayout.Width(128));
                brushSize = (int)GUILayout.HorizontalSlider(brushSize, 1, 256, GUILayout.Width(128));
                brushSize = EditorGUILayout.IntField(brushSize, GUI.skin.textField, GUILayout.Width(64));
                brush = TextureByte.Draw.Circle(brushSize, 255);
                brush.Apply();
                /*GUILayout.Label("Brush Render Color : ", GUILayout.ExpandWidth(false));
                brushColour = EditorGUILayout.ColorField(brushColour, GUILayout.Height(32));
                GUILayout.Label("Overlay Render Color : ", GUILayout.ExpandWidth(false));
                overlayColour = EditorGUILayout.ColorField(overlayColour, GUILayout.Height(32));*/
                if(GUILayout.Button("Invert overlayColour"))
                {
                    overlayColour = Color.white - overlayColour;
                }
            }

            GUILayout.EndHorizontal();

            if (drawing)
            {
                GUILayout.Label("Click to start drawing transparency mask (left click to make area solid, right click to make transparent)");
            }

            renderRect = GUILayoutUtility.GetRect(webcamRect.width, webcamRect.height, GUILayout.ExpandWidth(false));
            screenRect = GUIUtility.GUIToScreenRect(renderRect);
            GUI.DrawTexture(renderRect, webcam);
            if (drawing)
            {
                renderMat.color = overlayColour;
                EditorGUI.DrawPreviewTexture(renderRect, mask.uSprite.texture, renderMat);
            }



            if (mask != null)
            {
                doPainting(Event.current);
            }
        }
        GUILayout.Space(10);
        GUILayout.EndVertical();
    }

    private void doPainting(Event e)
    {
        Vector2 mouse = GUIUtility.GUIToScreenPoint(e.mousePosition);
        if (!screenRect.Contains(mouse))
        {
            return;
        }
        Texture2D brushTex = brush.uSprite.texture;
        Rect brushRect = new Rect(
            mouse.x - ((brushSize/2)*textureScale), 
            mouse.y - ((brushTex.height - (brushSize/2)) * textureScale),
            brushTex.width * textureScale,
            brushTex.height * textureScale);

        brushRect = GUIUtility.ScreenToGUIRect(brushRect);
        renderMat.color = brushColour;
        EditorGUI.DrawPreviewTexture(brushRect, brushTex,renderMat);
        if (e.button < 2 && e.isMouse)
        {
            

            Vector2 next = new Vector2(
                (mouse.x - screenRect.position.x) / screenRect.width,
                (1 - (mouse.y - screenRect.position.y) / screenRect.height));

            next = Rect.NormalizedToPoint(mask.rect, next);

            if (e.type == EventType.MouseDrag)
            {
                var sweep = TextureByte.Draw.Sweep(brush, prev, next, TextureByte.mask);
                if (e.button == 0)
                {
                    mask.Blend(sweep, BlendAdd);
                }
                else
                {
                    mask.Blend(sweep, BlendSub);
                }
                mask.Apply();

                TextureByte.Draw.FreeSprite(sweep);
                drawn = true;
            }

            prev = next;

            dragging = true;
        }
        else
        {
            dragging = false;
        }
    }

    private void Update()
    {
        Repaint();
    }

    void OnDestroy()
    {
        if (webcamCount > 0)
        {
            webcam.Stop();
        }
    }

    public void TakePhoto()
    {
        webcam.Pause();

        drawing = true;

        mask = TextureByte.Draw.GetSprite(webcam.width, webcam.height);
        mask.SetPixelsPerUnit(100);
        mask.Clear(224);

        mask.Apply();
    }

    public void RetryPhoto()
    {
        Reset();
        webcam.Play();
    }
    public void Reset()
    {
        drawing = false;

        drawn = false;
        if (mask != null)
        {
            TextureByte.Draw.FreeSprite(mask);
            mask = null;
        }
    }

    public void AcceptPhoto()
    {
        
        Color32 clear = Color.clear;

        var pix = webcam.GetPixels32();

        int min_x = webcam.width, min_y = webcam.height, max_x = 0, max_y = 0;

        for (int y = 0; y < webcam.height; ++y)
        {
            for (int x = 0; x < webcam.width; ++x)
            {
                int i = y * webcam.width + x;

                if (mask.mTexture.pixels[i] <= 128)
                {
                    min_x = Mathf.Min(x, min_x);
                    min_y = Mathf.Min(y, min_y);
                    max_x = Mathf.Max(x, max_x);
                    max_y = Mathf.Max(y, max_y);
                }
            }
        }
        if(max_x - min_x <= 1 || max_y - min_y <= 1)
        {
            AcceptFullPhoto();
            return;
        }
        //add a pixel of transparency around cutout
        min_x = Mathf.Max(0, min_x - 1);
        min_y = Mathf.Max(0, min_y - 1);
        max_x = Mathf.Min(webcam.width - 1, max_x + 1);
        max_y = Mathf.Min(webcam.height - 1, max_y + 1);
        for (int i = 0; i < pix.Length; ++i)
        {
            if (mask.mTexture.pixels[i] > 128)
            {
                pix[i] = clear;
            }
        }

        var next = new Texture2D(max_x - min_x + 1,
                                 max_y - min_y + 1,
                                 TextureFormat.ARGB32,
                                 false);

        var pixels = next.GetPixels32();

        {
            int ti = 0;

            for (int y = min_y; y <= max_y; ++y)
            {
                for (int x = min_x; x <= max_x; ++x)
                {
                    int i = y * mask.mTexture.width + x;

                    pixels[ti] = pix[i];
                    ti += 1;
                }
            }
        }

        next.SetPixels32(pixels);
        next.Apply();
        SaveTexture(next);
    }

    public void AcceptFullPhoto()
    {

        Color32 clear = Color.clear;

        var pix = webcam.GetPixels32();

        var next = new Texture2D(webcam.width,
                                 webcam.height,
                                 TextureFormat.ARGB32,
                                 false);

        next.SetPixels32(pix);
        next.Apply();
        SaveTexture(next);
    }

    public void SaveTexture(Texture2D tex)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        bool validName = true;
        foreach(char inv in invalid)
        {
            if (saveName.Contains(inv.ToString()))
            {
                validName = false;
                break;
            }
        }
        string name;
        if (saveName == "" || !validName)
        {
            name = GetUniqueName();
        }
        else
        {
            name = saveName;
        }
        if (!AssetDatabase.IsValidFolder("Assets/Art"))
        {
            AssetDatabase.CreateFolder("Assets", "Art");
        }
        name += ".png";
        string root = Application.dataPath + "/Art/";
        System.IO.File.WriteAllBytes(root + name, tex.EncodeToPNG());
        AssetDatabase.Refresh();
        if (artCreation != null)
        {
            artCreation.img = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Art/" + name, typeof(Texture2D));
            artCreation.CreateArt();
        }
    }

    private static byte BlendAdd(byte canvas, byte brush)
    {
        if (canvas > brush)
        {
            return (byte)(canvas - brush);
        }
        else
        {
            return 0;
        }
    }
    private static byte BlendSub(byte canvas, byte brush)
    {
        //Debug.Log(canvas);
        if (canvas > brush)
        {
            return (byte)(canvas - brush);
        }
        else
        {
            return (byte)((brush/225)*224);
        }
    }

}
