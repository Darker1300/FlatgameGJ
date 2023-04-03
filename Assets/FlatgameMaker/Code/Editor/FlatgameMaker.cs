using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;

public class FlatgameMaker : EditorWindow
{

    public static string artPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/ArtPrefab/ArtPrefab.prefab";
    public static string backgroundPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/BackgroundPrefab/BackgroundPrefab.prefab";
    public static string customBackgroundPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/BackgroundPrefab/CustomBackgroundPrefab.prefab";
    public static string textPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/TextPrefab/TextPrefab.prefab";
    public static string triggerPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/TriggerPrefab/TriggerPrefab.prefab";
    public static string animationPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/AnimationPrefab/AnimationPrefab.prefab";
    public static string groupPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/GroupPrefab/GroupPrefab.prefab";
    public static string positionIndicatorPath = "Assets/FlatgameMaker/Resources/Prefabs/PositionIndicator/PositionIndicator.prefab";
    [SerializeField]
    bool viewSettings = false;
    EditorCoroutine activeCoroutine = null;

    public void OnHierarchyChange()
    {
        if (Application.isPlaying)
        {
            return;
        }
        if (activeCoroutine == null || !activeCoroutine.running)
            activeCoroutine = EditorCoroutine.Start(RecalcImpl());

        Repaint();
    }

    static IEnumerator RecalcImpl()
    {
        Debug.Log("Recalculating");
        if (AreaManager.Instance != null)
        {
            AreaManager.Instance.Recalculate();
        }
        yield return null;
    }

    bool centerSpace = true;
    bool centerObject= true;

    [MenuItem("Flatgame Maker/Flatgame Maker")]
    static void Init()
    {
        FlatgameMaker window = (FlatgameMaker)EditorWindow.GetWindow(typeof(FlatgameMaker));
        window.Show();
    }

    private void OnValidate()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChange;
        EditorApplication.hierarchyChanged += OnHierarchyChange;
    }

    void OnGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.BeginVertical(GUI.skin.box,GUILayout.ExpandWidth(true));
        if (RenderAreaNavigation())
        {
            RenderStartAtCurrentOption();
            RenderCenterOptions();
            RenderArtNavigation();
            //RenderMoveToButton();
        }
        /*if (AreaManager.Instance != null)
        {
            viewSettings = GUIUtils.DropdownButton("Flatgame Settings",viewSettings, position.width - 32);
            if (viewSettings)
            {
                
            }
            GUIUtils.EndDropdown();
        }*/
        GUIUtility.ExitGUI();
    }

    void OnSelectionChange()
    {
        if (Selection.activeObject != null)
        {
            AreaManager.Instance.MoveTo(Selection.activeTransform);
            Repaint();
        }
    }

    private void RenderCenterOptions()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Center : ", GUILayout.ExpandWidth(false));
        centerSpace = GUILayout.Toggle(centerSpace,"Areas ");
        if (position.width < 350)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
        }
        centerObject = GUILayout.Toggle(centerObject, "Objects ");
        if (position.width > 500)
        {
            GUILayout.Label("When Moved To", GUILayout.ExpandWidth(false));
        }
        GUILayout.EndHorizontal();
    }

    private void RenderMoveToButton()
    {
        if (GUILayout.Button("Navigate to selected object",GUILayout.MaxWidth(position.width - 24)))
        {
            Transform selected = Selection.activeTransform;
            AreaManager.Instance.MoveTo(selected);
            if (centerObject)
            {
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
        }
    }

    private bool RenderAreaNavigation()
    {
        if (AreaManager.Instance == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("No AreeaManager present");
            if (GUILayout.Button("Create AreaManager"))
            {
                GameObject newObject = new GameObject("AreaManager");
                newObject.AddComponent<AreaManager>();
            }

            GUILayout.EndHorizontal();
            return false;
        }

        GUILayout.BeginHorizontal();
        if (position.width > 350)
        {
            GUILayout.Label("Current Area:", GUILayout.Width(170));
        }
        //left
        if (GUILayout.Button("", GUI.skin.customStyles[3]))
        {
            AreaManager.Instance.PreviousArea();
            AreaManager.Instance.SelectCurrentArea();
            if (centerSpace)
            {
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
            EditorUtility.SetDirty(AreaManager.Instance);
        }
        //right
        if (GUILayout.Button("", GUI.skin.customStyles[4]))
        {
            AreaManager.Instance.NextArea();
            AreaManager.Instance.SelectCurrentArea();
            if (centerSpace)
            {
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
            EditorUtility.SetDirty(AreaManager.Instance);
        }

        Area currentArea = AreaManager.Instance.GetCurrentArea();
        if (currentArea == null)
        {
            GUILayout.Label("no areas in scene");
        }
        else
        {
            float buttonWidth = position.width - 302;
            if (position.width <= 350)
            {
                buttonWidth += 170;
            }
            if (GUILayout.Button(currentArea.areaName,GUILayout.Width(Mathf.Max(50,buttonWidth))))
            {
                AreaManager.Instance.SelectCurrentArea();
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
        }
        //add
        if (GUILayout.Button(new GUIContent("", "Add new Area"), GUI.skin.customStyles[5]))
        {
            ShowPopup<AreaCreation>();
        }


        GUILayout.EndHorizontal();
        return currentArea != null;
    }

    private void RenderArtNavigation()
    {
        GUILayout.BeginHorizontal();
        if (position.width > 350)
        {
            GUILayout.Label("Current Object:", GUILayout.Width(170));
        }
        Area currentArea = AreaManager.Instance.GetCurrentArea();
        //left
        if (GUILayout.Button("", GUI.skin.customStyles[3]))
        {
            currentArea.PreviousArt();
            if (centerObject)
            {
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
        }
        //right
        if (GUILayout.Button("", GUI.skin.customStyles[4]))
        {
            currentArea.NextArt();
            if (centerObject)
            {
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
        }
        float buttonWidth = position.width - 302;
        if (position.width <= 350)
        {
            buttonWidth += 170;
        }
        if (!currentArea.HasArt())
        {
            GUILayout.Label("no art in current area");
        }
        else
        {
            if (GUILayout.Button(currentArea.GetCurrentArt().name,GUILayout.Width(Mathf.Max(50,buttonWidth))))
            {
                currentArea.SelectCurrentArt();
                EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            }
        }
        //add
        if (GUILayout.Button(new GUIContent("","Add new Object"), GUI.skin.customStyles[5]))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("art"), false, OnAddObject, "art");
            menu.AddItem(new GUIContent("text"), false, OnAddObject, "text");
            menu.AddItem(new GUIContent("trigger"), false, OnAddObject, "trigger");
            menu.AddItem(new GUIContent("audio"), false, OnAddObject, "audio");
            menu.ShowAsContext();
        }

        GUILayout.EndHorizontal();
    }

    private void OnAddObject(object objectType)
    {
        string input = (string)objectType;
        switch(objectType){
            case "art":
                ShowPopup<ArtCreation>();
                break;
            case "text":
                ShowPopup<TextCreation>();
                break;
            case "trigger":
                ShowPopup<TriggerCreation>();
                break;
            case "audio":
                ShowPopup<AudioCreation>();
                break;
            default:
                break;
        }
    }

    private void RenderStartAtCurrentOption()
    {
        AreaManager.Instance.startAtEditorCurrent = GUILayout.Toggle(AreaManager.Instance.startAtEditorCurrent, "Start in current area(Editor Only)", GUILayout.MaxWidth(position.width - 24));
    }

    public static T ShowPopup<T>() where T: EditorWindow
    {
        T window = CreateInstance<T>();
        window.Show();
        return window;
    }

}

internal class AudioCreation : EditorWindow
{
    AudioClip clip;

    void OnGUI()
    {
        clip = (AudioClip)EditorGUILayout.ObjectField("Audio File", clip, typeof(AudioClip), false);
        GUI.skin = GUIUtils.DefaultSkin;
        if(GUILayout.Button("Add As Current Area Song"))
        {
            AreaManager.Instance.GetCurrentArea().AddMusic(clip);
            this.Close();
        }
        if (GUILayout.Button("Add As Default Song"))
        {
            GameObject manager = AreaManager.Instance.gameObject;
            AudioSource src = manager.GetComponent<AudioSource>();
            if (src == null)
            {
                src = manager.AddComponent<AudioSource>();
            }
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = true;
            this.Close();
        }
    }
}

public class ArtCreation : EditorWindow
{

    public Texture2D img;
    bool customParent = false;
    Transform parent;

    public void SetCustomParent(Transform newParent)
    {
        parent = newParent;
        customParent = true;
    }

    void OnGUI()
    {

        //img = (Texture2D)EditorGUILayout.ObjectField("Image", img, typeof(Texture2D), false);
        GUI.skin = GUIUtils.DefaultSkin;
        if(GUILayout.Button("Create From Webcam"))
        {
            WebcamImporter web = FlatgameMaker.ShowPopup<WebcamImporter>();
            web.artCreation = this;
        }
        if(GUILayout.Button("Import From Computer"))
        {
            string importPath = EditorUtility.OpenFilePanel("Import texture", "", "png");
            string filename = importPath.Substring(importPath.LastIndexOf("/"));
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
            {
                AssetDatabase.CreateFolder("Assets","Art");
            }
            string newPath = "Assets/Art/" + filename;
            FileUtil.CopyFileOrDirectory(importPath, newPath);
            AssetDatabase.Refresh();
            img = (Texture2D)AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture2D));
            if (img != null)
            {
                CreateArt();
            }
        }
        if (GUILayout.Button("Add From Project"))
        {
            int controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", controlID);
        }
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            img = (Texture2D)EditorGUIUtility.GetObjectPickerObject();
            if(img != null)
            {
                CreateArt();
            }
        }
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
    }

    public void CreateArt()
    {
        GameObject newArt = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.artPrefabPath);
        newArt.GetComponent<Renderer>().sharedMaterial.mainTexture = img;
        newArt.name = img.name;
        if (!customParent)
        {
            Area parentArea = AreaManager.Instance.GetCurrentArea();
            if (parentArea != null)
            {
                parent = parentArea.transform;
            }
        }
        newArt.transform.parent = parent;
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        newArt.transform.position = Vector3.Scale(sceneCam.transform.position, new Vector3(1, 1, 0));
        Vector3 relativeScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
        newArt.transform.localScale = Vector3.Scale(Vector3.one * sceneCam.orthographicSize, relativeScale);
        newArt.transform.SetAsFirstSibling();
        ArtObject newArtObj = newArt.GetComponent<ArtObject>();
        if (newArtObj != null)
        {
            newArtObj.ResizeFromTexture();
        }
        Selection.activeGameObject = newArt;
        this.Close();
    }
}
public class TextCreation : EditorWindow
{
    string text;
    Color textColor = Color.black;
    bool customParent = false;
    Transform parent;

    public void SetCustomParent(Transform newParent)
    {
        parent = newParent;
        customParent = true;
    }

    void OnGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.Label("Be sure you have imported the Text Mesh Pro Essential Resources.\nGo to \"Window>TextMeshPro>Import Text Mesh Pro Essential Resources\" to do this.", EditorStyles.miniBoldLabel);
        GUILayout.Label("Text To Add:");
        text = GUILayout.TextArea(text,GUILayout.MinHeight(300));
        GUI.skin = null;
        textColor = EditorGUILayout.ColorField("Text Color: ", textColor);
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
        if (GUILayout.Button("Create"))
        {
            GameObject newText = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.textPrefabPath);
            TextMeshPro newTMP = newText.GetComponent<TextMeshPro>();
            newTMP.text = text;
            newTMP.color = textColor;
            if (text.Length > 30)
            {
                newText.name = text.Substring(0, 30);
            }
            else
            {
                newText.name = text;
            }
            if (!customParent)
            {
                Area parentArea = AreaManager.Instance.GetCurrentArea();
                if (parentArea != null)
                {
                    parent = parentArea.transform;
                }
            }
            newText.transform.parent = parent;
            newText.transform.SetAsFirstSibling();
            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            newText.transform.position = Vector3.Scale(sceneCam.transform.position, new Vector3(1, 1, 0));
            Vector3 relativeScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
            newText.transform.localScale = Vector3.Scale(Vector3.one * sceneCam.orthographicSize * (1/newTMP.fontSize), relativeScale);
            Selection.activeGameObject = newText;
            this.Close();

        }
        GUILayout.EndHorizontal();
    }
}

public class TriggerCreation : EditorWindow
{

    public enum ColliderType
    {
        Circle,
        Square
    }

    bool customParent = false;
    Transform parent;
    ColliderType colliderType;
    private TriggerType triggerType;
    private Area moveToArea;
    private GameObject reveal;
    private GameObject hide;
    private bool toggleOnEnter;
    private bool oneshot;

    public void SetCustomParent(Transform newParent)
    {
        parent = newParent;
        customParent = true;
    }
     
    void OnGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        colliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType);
        triggerType = (TriggerType)EditorGUILayout.EnumPopup("Trigger Type",triggerType);
        EditorGUILayout.LabelField("Trigger Settings");
        switch (triggerType)
        {
            case TriggerType.GoToArea:
                moveToArea = (Area)EditorGUILayout.ObjectField("Area to move to: ", moveToArea, typeof(Area), true);
                break;
            case TriggerType.RevealAndHideObjects:
                reveal = (GameObject)EditorGUILayout.ObjectField("Object to reveal: ", reveal, typeof(GameObject), true);
                hide = (GameObject)EditorGUILayout.ObjectField("Object to hide: ", hide, typeof(GameObject), true);
                RenderToggleOptions();
                break;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
        if (GUILayout.Button("Create"))
        {
            GameObject newTrig = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.triggerPrefabPath);
            switch (colliderType)
            {
                case ColliderType.Circle:
                    newTrig.AddComponent<CircleCollider2D>();
                    break;
                case ColliderType.Square:
                    newTrig.AddComponent<BoxCollider2D>();
                    break;
            }
            ActionTrigger trigger = newTrig.GetComponent<ActionTrigger>();
            trigger.triggerType = triggerType;
            trigger.moveToArea = moveToArea;
            trigger.reveal = reveal;
            trigger.hide = hide;
            trigger.oneshot = oneshot;
            trigger.toggleAudio = toggleOnEnter;
            if (!customParent)
            {
                Area parentArea = AreaManager.Instance.GetCurrentArea();
                if (parentArea != null)
                {
                    parent = parentArea.transform;
                }
            }
            newTrig.transform.parent = parent;
            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            newTrig.transform.position = Vector3.Scale(sceneCam.transform.position, new Vector3(1, 1, 0));
            Vector3 relativeScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
            newTrig.transform.localScale = Vector3.Scale(Vector3.one * sceneCam.orthographicSize, relativeScale);
            newTrig.transform.SetAsFirstSibling();
            Selection.activeGameObject = newTrig;
            this.Close();

        }
        GUILayout.EndHorizontal();
    }

    void RenderToggleOptions()
    {
        oneshot = EditorGUILayout.Toggle("Execute only the first time you enter the trigger?", oneshot);
        if (!oneshot)
        {
            toggleOnEnter = EditorGUILayout.Toggle("Toggle every time you enter the trigger?", toggleOnEnter);
        }
    }
}

public class AreaCreation : EditorWindow
{

    string newName;
    AreaManager areaManager;
    Vector2 newPosition;
    Vector2 newSize;
    Rect boundsRect;
    Texture2D bgTex;
    GameObject areaPositionIndicator;
    bool addBackground;

    void OnEnable()
    {
        areaManager = AreaManager.Instance;
        newName = areaManager.GetNewAreaName();
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        newPosition = sceneCam.transform.position;
        newSize = new Vector2(40, 40);
        boundsRect = new Rect((Vector2)newPosition - (0.5f*newSize), newSize);
        if (areaManager.OverlapsExisting(boundsRect))
        {
            newPosition = areaManager.GetNewAreaPosition(boundsRect);
            boundsRect = new Rect((Vector2)newPosition - (0.5f * newSize), newSize);
        }
        
        areaPositionIndicator = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.positionIndicatorPath);
        areaPositionIndicator.transform.position = newPosition;
        areaPositionIndicator.transform.GetChild(0).localScale = new Vector3(newSize.x, newSize.y, 1);
        Selection.activeGameObject = areaPositionIndicator.transform.GetChild(0).gameObject;
        EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
        Selection.activeGameObject = areaPositionIndicator;
    }

    void OnGUI()
    {

        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.BeginVertical(GUI.skin.box);
        newPosition = areaPositionIndicator.transform.position;
        if (areaPositionIndicator.transform.localScale != Vector3.one)
        {
            newSize = Vector2.Scale(newSize, areaPositionIndicator.transform.localScale);
            areaPositionIndicator.transform.localScale = Vector3.one;
            boundsRect = new Rect(newPosition - (0.5f * newSize), newSize);
        }
        if (addBackground)
        {
            GUILayout.Label("Custom Background Texture");
            GUILayout.Label("leave empty for grid paper");
            GUI.skin = null;
            bgTex = (Texture2D)EditorGUILayout.ObjectField(bgTex, typeof(Texture2D),false, GUILayout.Height(64), GUILayout.Width(64));
            GUI.skin = GUIUtils.DefaultSkin;
        }
        newPosition = GUIUtils.Vector2Field("Area Position", newPosition, 140);
        newSize = GUIUtils.Vector2Field("Bounds Size", newSize, 140);
        areaPositionIndicator.transform.position = newPosition;
        areaPositionIndicator.transform.GetChild(0).localScale = new Vector3(newSize.x, newSize.y, 1);
        newName = GUILayout.TextField(newName);
        addBackground = GUILayout.Toggle(addBackground, "Add Background");
        if (GUILayout.Button("Get New Position"))
        {
            newPosition = areaManager.GetNewAreaPosition(boundsRect);
            areaPositionIndicator.transform.position = newPosition;
            boundsRect = new Rect(newPosition - (0.5f * newSize), newSize);
            Selection.activeGameObject = areaPositionIndicator.transform.GetChild(0).gameObject;
            EditorApplication.ExecuteMenuItem("Edit/Frame Selected");
            Selection.activeGameObject = areaPositionIndicator;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
        if (GUILayout.Button("Create")){
            boundsRect = new Rect((Vector2)newPosition - (0.5f * newSize), newSize);
            GameObject newAreaObj = areaManager.NewArea(newName, newPosition,boundsRect);
            if (addBackground)
            {
                GameObject newBackground;
                if (bgTex == null)
                {
                    newBackground = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.backgroundPrefabPath);
                }
                else
                {
                    newBackground = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.customBackgroundPrefabPath);
                    MeshRenderer bgMesh = newBackground.GetComponent<MeshRenderer>();
                    if(bgMesh != null)
                    {
                        bgMesh.sharedMaterial.mainTexture = bgTex;
                    }
                }
                newBackground.transform.parent = newAreaObj.transform;
                ArtObject bgArt = newBackground.GetComponent<ArtObject>();
                if(bgArt != null)
                {
                    bgArt.ResizeToBounds(boundsRect);
                }
                newBackground.transform.localPosition += Vector3.forward * 10;
            }
            this.Close();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void OnDestroy()
    {
        DestroyImmediate(areaPositionIndicator);
    }

    void FocusCamera()
    {
        Camera cam = Camera.current;
        if (cam != null)
        {
            cam.transform.position = newPosition;
            cam.orthographicSize = newSize.y;
        }
    }
}
