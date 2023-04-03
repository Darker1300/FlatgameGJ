using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using TMPro;

public static class FlatgameMakerUtils
{
    public static string guiArtPath = "Assets/FlatgameMaker/Resources/Art/GUI/";
    public static string guiSkinPath = "Assets/FlatgameMaker/Resources/GUISkin/NewTest.guiskin";

    public static GameObject InstantiateAtPath(string path)
    {
        #if UNITY_EDITOR
        GameObject artPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
        return (GameObject)PrefabUtility.InstantiatePrefab(artPrefab);
        #endif
        return null;
    }

    public static Transform[] GetChildren(this Transform parent){
        List<Transform> children = new List<Transform>();
        for(int i = 0; i < parent.childCount; i++){
            children.Add(parent.GetChild(i));
        }
        return children.ToArray();
    }

    public static Transform[] GetChildrenRecursive(this Transform parent)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            children.Add(child);
            children = children.Concat(child.GetChildrenRecursive()).ToList();
        }
        return children.ToArray();
    }

    public static void MakeInstanced(this Renderer renderComponent)
    {
        if (renderComponent == null)
        {
            return;
        }
        {
            if (renderComponent.sharedMaterials.Length > 1)
            {
                Material[] newMats = new Material[renderComponent.sharedMaterials.Length];
                for (int i = 0; i < newMats.Length; ++i)
                {
                    Material matToCopy = renderComponent.sharedMaterials[i] ?? renderComponent.materials[i];
                    Material newMat = new Material(matToCopy);
                    string name = newMat.name;
                    name = name.Replace(" (Instance)", "");
                    name += " (Instance)"; // only allow 1 instance in the name
                    newMat.name = name;
                    newMats[i] = newMat;
                }
                renderComponent.sharedMaterials = newMats;
                return;
            }
            if (renderComponent.sharedMaterial != null)
            {
                Material matToCopy = renderComponent.sharedMaterial;
                Material newMat = new Material(matToCopy);
                string name = newMat.name;
                name = name.Replace(" (Instance)", "");
                name += " (Instance)"; // only allow 1 instance in the name
                newMat.name = name;
                renderComponent.sharedMaterial = newMat;
            }
        }
    }

    public static void SetRenderQueue(this Renderer renderComponent, int queue)
    {
        if (renderComponent == null)
        {
            return;
        }
        {
            if (renderComponent.sharedMaterials.Length > 1)
            {
                for (int i = 0; i < renderComponent.sharedMaterials.Length; ++i)
                {
                    renderComponent.sharedMaterials[i].renderQueue = queue;
                }
                return;
            }
            if (renderComponent.sharedMaterial != null)
            {
                renderComponent.sharedMaterial.renderQueue = queue;
            }
        }
    }

    public static void CalculateName(this Transform transform)
    {
        if(transform.GetComponent<Player>() != null)
        {
            return;
        }
        string name = "";
        ArtObject art = transform.GetComponent<ArtObject>();
        if (art != null)
        {
            TextMeshPro tmp = transform.GetComponent<TextMeshPro>();
            MeshRenderer mr = transform.GetComponent<MeshRenderer>();
            if(tmp != null) {
                name += tmp.text.Substring(0, Mathf.Min(20, tmp.text.Length - 1));
            }
            else if(mr != null)
            {
                name += mr.sharedMaterial.mainTexture.name;
            }
        }

        ActionTrigger trig = transform.GetComponent<ActionTrigger>();
        if (trig != null)
        {
            name = trig.triggerType.ToString() + (name == "" ? "" : "_" + name);
        }

        Animation anim = transform.GetComponent<Animation>();
        if (anim != null)
        {
            MeshRenderer childRend = transform.GetChild(0).GetComponent<MeshRenderer>();
            string childName = "";
            if (childRend != null)
            {
                childName = childRend.sharedMaterial.mainTexture.name;
            }
            name = (name == "" ? childName + "_" : name + "_") + "Animation";
        }

        if(transform.parent != null && transform.parent.GetComponent<Animation>())
        {
            name = "Frame" + transform.GetSiblingIndex() + 1 + (name == "" ? "" : "_" + name);
        }

        if(name != "" && name != transform.name)
        {
            transform.name = name;
        }
    }
}
