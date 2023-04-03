using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;
using UnityEditor;

/// <summary>
/// animate by treating all children of this object as a frame of an animation,
/// turning them off and on in sequence, ensuring only one is ever on at a time
/// </summary>
[DisallowMultipleComponent]
public class Animation : MonoBehaviour 
{
    [HideInInspector]
    public int currentFrame;

    [Header("Timing (seconds)")]
    [Tooltip("How many frames play over the course of a second")]
    public float framesPerSecond = 12;

    float eachFrameDuration;

    [Tooltip("Should the animation start part way through a random frame?")]
    public bool startAtRandomTime;

    [HideInInspector]
    public bool animateInEditMode = false;

    private float timeSinceCurrentFrame;


    /// <summary>
    /// update timings so that loop duration, frame duration, and frame count
    /// are consistent with each other, based on which have just changed
    /// </summary>
    private void UpdateTiming()
    {
        eachFrameDuration = 1.0f / framesPerSecond;
    }

    private void Start()
    {
        if (startAtRandomTime)
        {
            currentFrame = Random.Range(0, transform.childCount);
            timeSinceCurrentFrame = Random.value * eachFrameDuration;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying && !animateInEditMode)
        {
            return;
        }
		
        UpdateTiming();
		AdvanceTime(Time.deltaTime);
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.QueuePlayerLoopUpdate();
        }
#endif
    }
    public void EditorPlay()
    {
#if UNITY_EDITOR
        if (animateInEditMode)
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        else
        {
            EditorApplication.update -= Update;
        }
#endif
    }

    /// <summary>
    /// advance the animation by a duration in seconds, advancing as many
    /// frames as necessary
    /// </summary>
    private void AdvanceTime(float duration)
    {
        timeSinceCurrentFrame += duration;
        
        while (timeSinceCurrentFrame > eachFrameDuration && eachFrameDuration > 0)
        {
            timeSinceCurrentFrame -= eachFrameDuration;

            AdvanceFrame();
        }
    }

    /// <summary>
    /// advance the animation by one frame, looping if necessary
    /// </summary>
    public void AdvanceFrame(bool forward = true)
    {
        currentFrame += forward ? 1 : -1;

        Loop();

        RefreshFrame();
    }

    /// <summary>
    /// loops the frame within the available number of frames
    /// </summary>
    public void Loop()
    {
        if (currentFrame >= transform.childCount)
        {
            currentFrame = 0;
        }
        if (currentFrame < 0)
        {
            currentFrame = transform.childCount - 1;
        }
    }

    /// <summary>
    /// turn off all frames except the current frame, which is turned on
    /// </summary>
    public void RefreshFrame()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform frame = transform.GetChild(i);
            MeshRenderer[] frameRenderers = frame.GetComponentsInChildren<MeshRenderer>(false);
            bool active = i == currentFrame;
            foreach (MeshRenderer r in frameRenderers)
            {
                r.enabled = active;
            }
        }
    }
}
