using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public enum AreaBoundsCollision
{
    Hard,
    Soft,
    //Elastic,
    Loop,
    None
}

public class Area : MonoBehaviour
{

    static string playerPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/PlayerPrefab/PlayerPrefab.prefab";
    public string areaName;
    Transform[] areaArt;
    int currentArt = 0;
    Transform currentTransform;
    public Rect bounds;
    public AreaBoundsCollision boundsCollision;
    public float boundsCollisionOffset;
    public bool playAudioFromStart;
    private Player areaPlayer;
    public Player AreaPlayer
    {
        get
        {
            if(areaPlayer == null)
            {
                RecalculatePlayer();
            }
            return areaPlayer;
        }
    }

    [FormerlySerializedAs("startSpace")]
    public bool startArea;

    public void Recalculate(ref int queue)
    {
        if(gameObject.name != areaName)
        {
            gameObject.name = areaName;
        }
        RecalculatePlayer();
        areaArt = transform.GetChildrenRecursive();
        if (currentTransform != null)
        {
            currentArt = Array.IndexOf(areaArt, currentTransform);
        }
        if(currentArt >= areaArt.Length){
            currentArt = 0;
            currentTransform = GetCurrentArt();
        }
        MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>(true);
        foreach(MeshRenderer r in renderers)
        {
            r.MakeInstanced();
            r.SetRenderQueue(queue);
            queue--;
        }
        if (AreaManager.Instance.autoNameing)
        {
            foreach (Transform t in areaArt)
            {
                t.CalculateName();
            }
        }
    }

    public void RecalculatePlayer()
    {
        Player[] existingPlayers = transform.GetComponentsInChildren<Player>();
        if (areaPlayer == null || !areaPlayer.transform.IsChildOf(transform))
        {
            if (existingPlayers.Length > 0)
            {
                areaPlayer = existingPlayers[0];
                for (int i = 1; i < existingPlayers.Length; i++)
                {
                    DestroyImmediate(existingPlayers[i]);
                }
            }
            else
            {
                AddPlayer();
            }
        }
        else if (existingPlayers.Length > 0)
        {
            for (int i = 1; i < existingPlayers.Length; i++)
            {
                if (existingPlayers[i] != areaPlayer)
                {
                    DestroyImmediate(existingPlayers[i]);
                }
            }
        }
    }

    public void AddMusic(AudioClip clip)
    {
        AudioSource src = gameObject.GetComponent<AudioSource>();
        if (src == null)
        {
            src = gameObject.AddComponent<AudioSource>();
        }
        src.clip = clip;
        src.loop = true;
        src.playOnAwake = true;
    }

    public Transform GetCurrentArt(){
        if(areaArt.Length <= currentArt) {
            currentArt = 0;
        }
        return areaArt[currentArt];
    }

    public bool HasArt(){
        if(areaArt == null)
        {
            return false;
        }
        return areaArt.Length > 0;
    }

    public void SelectCurrentArt(){
        #if UNITY_EDITOR
        Selection.activeTransform = areaArt[currentArt];
        #endif
    }

    public bool MoveTo(Transform select)
    {
        int index = Array.IndexOf(areaArt, select);
        if (index >= 0)
        {
            currentArt = index;
            currentTransform = select;
            #if UNITY_EDITOR
            Selection.activeTransform = select;
            #endif
            return true;
        }
        return false;
    }

    public bool Contains(Transform select)
    {
        if (areaArt == null)
        {
            areaArt = transform.GetChildrenRecursive();
        }
        int index = Array.IndexOf(areaArt, select);
        if (index >= 0)
        {
            return true;
        }
        return false;
    }

    public void NextArt()
    {
        Transform startTrans = currentTransform;
        do
        {
            currentArt ++;
            if(currentArt >= areaArt.Length){
                currentArt = 0;
            }
            currentTransform = GetCurrentArt();
            if (currentTransform == startTrans)
            {
                break;
            }
        } while (!currentTransform.gameObject.activeSelf);
        SelectCurrentArt();
    }

    public void PreviousArt(){
        Transform startTrans = currentTransform;
        int startpoint = currentTransform.GetSiblingIndex();
        do
        {
            currentArt--;
            if (currentArt < 0)
            {
                currentArt = areaArt.Length - 1;
            }
            currentTransform = GetCurrentArt();
            if (currentTransform == startTrans)
            {
                break;
            }
        } while (!currentTransform.gameObject.activeSelf);
        SelectCurrentArt();
    }

    public void AddPlayer()
    {
        GameObject newPlayer = FlatgameMakerUtils.InstantiateAtPath(playerPrefabPath);
        newPlayer.name = gameObject.name + " Player";
        newPlayer.transform.parent = transform;
        newPlayer.transform.SetAsFirstSibling();
        newPlayer.transform.localPosition = Vector3.zero;
        areaPlayer = newPlayer.GetComponent<Player>();
    }

    public void RecalculateBoundsFromContents()
    {
        Transform[] areaObjects = transform.GetChildrenRecursive();
        float minX = transform.position.x;
        float maxX = transform.position.x;
        float minY = transform.position.y; 
        float maxY = transform.position.y;
        Rect newBounds = new Rect(transform.position.x, transform.position.y, 0, 0);
        foreach (Transform t in areaObjects)
        {
            MeshRenderer r = t.GetComponent<MeshRenderer>();
            if(r != null)
            {
                Bounds b = r.bounds;
                newBounds = RectFromBounds(newBounds, b);
            }
            Collider2D c = t.GetComponent<Collider2D>();
            if(c != null)
            {
                Bounds b = c.bounds;
                newBounds = RectFromBounds(newBounds, b);
            }
        }
        bounds = newBounds;
    }

    Rect RectFromBounds(Rect r, Bounds b)
    {
        r.xMin = b.min.x < r.xMin ? b.min.x : r.xMin;
        r.xMax = b.max.x > r.xMax ? b.max.x : r.xMax;
        r.yMin = b.min.y < r.yMin ? b.min.y : r.yMin;
        r.yMax = b.max.y > r.yMax ? b.max.y : r.yMax;
        return r;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bounds.center, bounds.size - (Vector2.one * 2 * boundsCollisionOffset) );
    }

}
