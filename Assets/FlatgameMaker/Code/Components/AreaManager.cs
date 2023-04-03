using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AreaManager : MonoBehaviour
{

    private static string areaPrefabPath = "Assets/FlatgameMaker/Resources/Prefabs/SpacePrefab/SpacePrefab.prefab";

    private static AreaManager instance;
    public static AreaManager Instance
    {
        get
        {
            if(instance == null)
            {
                AreaManager existing = FindObjectOfType<AreaManager>();
                if (existing != null)
                {
                    instance = existing;
                }
            }
            return instance;
        }
    }

    [SerializeField]
    List<Area> areas;
    [FormerlySerializedAs("currentSpace")]
    public int currentArea;
    [FormerlySerializedAs("startSpace")]
    public int startArea = -1;
    [HideInInspector]
    public bool startAtEditorCurrent;
    public float musicCrossFadeTime = 5;
    public bool playMainSongFromStart;
    public bool autoNameing = false;
    AudioSource current;
    AudioSource last;

    private void Start()
    {
        if (!Application.isEditor || !startAtEditorCurrent)
        {
            MoveToArea(startArea);
        }
        else
        {
            MoveToArea(currentArea);
        }
        for (int i = 0; i < areas.Count; i++)
        {
            AudioSource areaSrc = areas[i].GetComponent<AudioSource>();
            if (areaSrc != null && areaSrc.clip != null)
            {
                if (i == currentArea)
                {
                    areaSrc.volume = 1;
                    current = areaSrc;
                    AudioSource mainSrc = gameObject.GetComponent<AudioSource>();
                    if (mainSrc != null && mainSrc.clip != null && mainSrc != current)
                    {
                        mainSrc.volume = 0;
                    }
                }
                else
                {
                    areaSrc.volume = 0;
                }
            }
            else if (i == currentArea)
            {
                AudioSource mainSrc = gameObject.GetComponent<AudioSource>();
                if (mainSrc != null && mainSrc.clip != null && mainSrc != current)
                {
                    mainSrc.volume = 1;
                    current = areaSrc;
                }
            }
        }
        last = null;
    }

    private void Update()
    {
        if (current != null)
        {
            if (last == current)
            {
                last = null;
            }
            if (musicCrossFadeTime <= 0)
            {
                if (last != null)
                {
                    last.volume = 0;
                }
                current.volume = 1;
            }
            else
            {
                float crossfade = 1 / musicCrossFadeTime;
                if (last != null)
                {
                    last.volume = Mathf.MoveTowards(last.volume, 0, Time.deltaTime * crossfade);
                }
                current.volume = Mathf.MoveTowards(current.volume, 1, Time.deltaTime * crossfade);
            }
        }
    }

    void Reset()
    {
        if (areas == null)
        {
            areas = new List<Area>();
        }
    }

    public void Recalculate()
    {
        if(Camera.main.GetComponent<CameraFollow>() == null)
        {
            Camera.main.gameObject.AddComponent<CameraFollow>();
        }
        int newCurrent = currentArea;
        for(int i = 0; i < areas.Count; i++)
        {
            if(areas[i] == null)
            {
                areas.RemoveAt(i);
                if(currentArea >= i)
                {
                    newCurrent -= 1;
                }
                i--;
            }
        }
        if (areas.Count > 0)
        {
            MoveToArea(newCurrent);
        }
        Area[] heirarchyAreas = GetComponentsInChildren<Area>();
        int queue = 2000;
        foreach(Area area in heirarchyAreas)
        {
            area.Recalculate(ref queue);
        }
    }

    public void UpdateStartArea(Area gameArea)
    {
        if (gameArea.startArea)
        {
            areas[startArea].startArea = false;
            startArea = areas.IndexOf(gameArea);
        }
        else
        {
            areas[0].startArea = true;
            startArea = 0;
        }
    }

    public GameObject NewArea(string name, Vector3 pos, Rect bounds)
    {
        GameObject newObject = FlatgameMakerUtils.InstantiateAtPath(areaPrefabPath);
        newObject.name = name;
        newObject.transform.position = pos;
        newObject.transform.parent = transform;
        Area newArea = newObject.GetComponent<Area>();
        newArea.areaName = name;
        newArea.bounds = bounds;
        areas.Add(newArea);
        if (startArea < 0 || startArea >= areas.Count)
        {
            startArea = areas.Count - 1;
            newArea.startArea = true;
        }
        MoveToArea(areas.Count - 1);
        #if UNITY_EDITOR
        Selection.activeGameObject = newObject;
        #endif
        return newObject;
    }

    public Area GetCurrentArea()
    {
        if(areas.Count == 0)
        {
            return null;
        }
        if(currentArea >= areas.Count)
        {
            MoveToArea(areas.Count - 1);
        }
        return areas[currentArea];
    }

    public string GetNewAreaName()
    {
        return "Area " + (areas.Count + 1);
    }

    public Vector3 GetNewAreaPosition(Rect newRect)
    {
        Rect occupiedArea= new Rect(0,0,0,0);
        foreach(Area s in areas)
        {
            occupiedArea.xMin = Mathf.Min(s.bounds.xMin, occupiedArea.xMin);
            occupiedArea.xMax = Mathf.Max(s.bounds.xMax, occupiedArea.xMax);
            occupiedArea.yMin = Mathf.Min(s.bounds.yMin, occupiedArea.yMin);
            occupiedArea.yMax = Mathf.Max(s.bounds.yMax, occupiedArea.yMax);
        }
        int dir = Random.Range(0,4);
        Vector2 pos = Vector2.zero;
        switch (dir)
        {
            case 0:
                pos.x = occupiedArea.xMax + (newRect.width / 2);
                break;
            case 1:
                pos.x = occupiedArea.xMin - (newRect.width / 2);
                break;
            case 2:
                pos.y = occupiedArea.yMax + (newRect.height / 2);
                break;
            default:
                pos.y = occupiedArea.yMin - (newRect.height / 2);
                break;
        }
        return pos;
    }

    public bool OverlapsExisting(Rect r)
    {
        foreach(Area s in areas)
        {
            if (r.Overlaps(s.bounds))
            {
                return true;
            }
        }
        return false;
    }

    public void SelectCurrentArea()
    {
        #if UNITY_EDITOR
        if (areas.Count == 0)
        {
            return;
        }
        Selection.activeGameObject = GetCurrentArea().gameObject;
        #endif
    }

    public void MoveTo(Transform selected)
    {
        if(selected == null)
        {
            return;
        }
        Area selectedArea = null;
        #if UNITY_EDITOR
        selectedArea = selected.GetComponent<Area>();
        #endif
        if (selectedArea != null)
        {
            MoveToArea(selectedArea);
            #if UNITY_EDITOR
            Selection.activeTransform = selected;
            #endif
            return;
        }
        else
        {
            foreach(Area s in areas)
            {
                if (s.MoveTo(selected))
                {
                    MoveToArea(s);
                    return;
                }
            }
        }
        //Debug.LogWarning("Attempted to move to object that is not part of the flatgame heirarchy");
    }

    public void MoveToArea(Area selected)
    {
        int index = areas.IndexOf(selected);
        MoveToArea(index);
    }

    public void MoveToArea(int selected)
    {
        if (selected >= 0)
        {
            if (currentArea >= 0 && currentArea < areas.Count){
                areas[currentArea].AreaPlayer.enabled = false;
            }
            currentArea = selected;
            areas[currentArea].AreaPlayer.enabled = true;
            if (Application.isPlaying)
            {
                AudioSource newSrc = areas[currentArea].GetComponent<AudioSource>();
                if (newSrc != null && newSrc.clip != null)
                {
                    if (last != null)
                    {
                        last.volume = 0;
                    }
                    last = current;
                    current = newSrc;
                    if (areas[currentArea].playAudioFromStart)
                    {
                        current.Play();
                        current.volume = 1;
                    }
                }
                else
                {
                    newSrc = gameObject.GetComponent<AudioSource>();
                    if (newSrc != null && newSrc.clip != null && newSrc != current)
                    {
                        if (last != null)
                        {
                            last.volume = 0;
                        }
                        last = current;
                        current = newSrc;
                        if (playMainSongFromStart)
                        {
                            current.Play();
                            current.volume = 1;
                        }
                    }
                }
            }
        }
    }

    public Area FindAreaOf(Transform selection)
    {
        foreach (Area s in areas)
        {
            if (s.Contains(selection))
            {
                return s;
            }
        }
        return null;
    }

    public int IndexOfArea(Area area)
    {
        return areas.IndexOf(area);
    }

    public void PreviousArea()
    {
        if(currentArea <= 0){
            MoveToArea(areas.Count - 1);
        }
        else
        {
            MoveToArea(currentArea - 1);
        }
        SelectCurrentArea();
    }

    public void NextArea()
    {
        if(currentArea >= areas.Count-1)
        {
            MoveToArea(0);
        }
        else
        {
            MoveToArea(currentArea + 1);
        }
    }

}
