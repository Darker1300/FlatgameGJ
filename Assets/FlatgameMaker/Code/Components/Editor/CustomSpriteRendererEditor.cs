using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(SpriteRenderer))]
    public class SpriteRendererEditor : DecoratorEditor
    {

        public SpriteRendererEditor() : base("SpriteRendererEditor") { }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SpriteRenderer rend = (SpriteRenderer)target;
            if (AreaManager.Instance != null)
            {
                if (GUILayout.Button("Make Flatgame Maker Art"))
                {
                    Texture2D spriteTex = rend.sprite.texture;
                    GameObject artObject = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.artPrefabPath);
                    artObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = spriteTex;
                    artObject.transform.localScale = rend.bounds.size + Vector3.forward;
                    artObject.transform.position = rend.transform.position;
                    Area area = AreaManager.Instance.FindAreaOf(rend.transform);
                    if (area == null)
                    {
                        area = AreaManager.Instance.GetCurrentArea();
                    }
                    artObject.transform.parent = area.transform;
                    artObject.transform.SetAsFirstSibling();
                    Selection.activeGameObject = artObject;
                    DestroyImmediate(rend.gameObject);
                }
            }
        }

    }
}
