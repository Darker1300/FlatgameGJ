using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ArtObject : MonoBehaviour
{

    void Start()
    {
        
    }


    public void ResizeFromTexture()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Transform transform = renderer.transform;

        #if UNITY_EDITOR
        if (renderer.sharedMaterial.mainTexture != null)
        {            
            float _height = transform.lossyScale.y;

            Texture2D _tmpTexture = new Texture2D(1,1);
            byte[] tmpBytes = System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture));
            _tmpTexture.LoadImage(tmpBytes);

            float _newWidth = _height * _tmpTexture.width / _tmpTexture.height;

            Vector3 _currentScale = transform.localScale;

            float parentScale = 1f;
            if (transform.parent != null) {
                parentScale = transform.parent.lossyScale.x;
            }
            _currentScale.x = _newWidth / parentScale; 

            _currentScale.x *= renderer.sharedMaterial.mainTextureScale.x;
            _currentScale.y *= renderer.sharedMaterial.mainTextureScale.y;

            transform.localScale = _currentScale;
        }
        #endif
    }

    public void ResizeToBounds()
    {
        Area area = AreaManager.Instance.FindAreaOf(transform);
        ResizeToBounds(area.bounds);
    }
    public void ResizeToBounds(Rect bounds)
    {
        Area area = AreaManager.Instance.FindAreaOf(transform);
        if (area != null)
        {
            transform.position = bounds.center;
            transform.localScale = transform.parent.InverseTransformVector(new Vector3(bounds.width, bounds.height, 1.0f));
        }
    }
}
