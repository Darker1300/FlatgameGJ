using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TriggerType
    {
        GoToArea,
        RevealAndHideObjects,
        PlaySound,
        LoadUnityScene,
        QuitGame
    }
public class ActionTrigger : MonoBehaviour
{

    public TriggerType triggerType;
    public bool oneshot;
    public bool toggleAudio;
    public Area moveToArea;
    public GameObject reveal;
    public GameObject hide;
    public AudioClip audioClip;
    public string scene;
    bool oneshotDone = false;
    Renderer[] revealRend;
    Renderer[] hideRend;
    AudioSource src;

    void Start()
    {
        revealRend = new Renderer[0];
        hideRend = new Renderer[0];
        src = GetComponent<AudioSource>();
        if(src == null)
        {
            src = gameObject.AddComponent<AudioSource>();
        }
        if (reveal != null)
        {
            revealRend = reveal.GetComponentsInChildren<Renderer>(true);
        }
        if (hide != null)
        {
            hideRend = hide.GetComponentsInChildren<Renderer>(true);
        }
        switch (triggerType)
        {
            case TriggerType.RevealAndHideObjects:
                SetRenderersVisability(revealRend, false);
                SetRenderersVisability(hideRend, true);
                break;
        }
    }

    void Reset()
    {
        if(GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
        if (!gameObject.name.Contains("Trigger_"))
        {
            gameObject.name = "Trigger_" + triggerType.ToString() + "_" + gameObject.name;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (triggerType)
        {
            case TriggerType.GoToArea:
                AreaManager.Instance.MoveToArea(moveToArea);
                break;
            case TriggerType.RevealAndHideObjects:
                if (!oneshot || (oneshot && !oneshotDone))
                {
                    SetRenderersVisability(revealRend, true);
                    SetRenderersVisability(hideRend, false);
                    oneshotDone = true;
                }
                break;
            case TriggerType.PlaySound:
                if (toggleAudio)
                {
                    src.clip = audioClip;
                    src.loop = true;
                    src.Play();
                }
                else if(!oneshot || (oneshot && !oneshotDone))
                {
                    src.PlayOneShot(audioClip);
                    oneshotDone = true;
                }
                break;
            case TriggerType.LoadUnityScene:
                SceneManager.LoadScene(scene);
                break;
            case TriggerType.QuitGame:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        switch (triggerType)
        {
            case TriggerType.RevealAndHideObjects:
                if (!oneshot)
                {
                    SetRenderersVisability(revealRend, false);
                    SetRenderersVisability(hideRend, true);
                }
                break;
            case TriggerType.PlaySound:
                if(toggleAudio)
                {
                    src.Stop();
                }
                break;
        }
    }

    void SetRenderersVisability(Renderer[] renderers, bool visible)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }

    void ToggleRenderersVisability(Renderer[] renderers)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = !r.enabled;
        }
    }
}