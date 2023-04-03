using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MicrophoneImporter : EditorWindow
{
    /*
    [MenuItem("Flatgame Maker/Microphone Importer")]
    static void Init()
    {
        MicrophoneImporter window = (MicrophoneImporter)EditorWindow.GetWindow(typeof(MicrophoneImporter));
        window.Show();
    }
    */

    int mic = 0;
    bool micAvailable;
    bool recording;

    int defaultLength = 240;
    int defaultSampleRate = 44100;
    int lastPos = 0;

    List<AudioClip> clipSections;
    AudioClip currentClip;
    AudioClip finalClip;

    void OnEnable()
    {
        micAvailable = Microphone.devices.Length > 0;
    }

    private void OnGUI()
    {
        GUI.skin = GUIUtils.DefaultSkin;
        GUILayout.BeginVertical(GUI.skin.box);
        if (micAvailable)
        {

            GUILayout.Label("Microphone to use:");
            mic = (int)Mathf.Repeat(mic, Microphone.devices.Length);
            GUILayout.BeginHorizontal();
            GUI.enabled = !recording;
            if (GUILayout.Button("", GUI.skin.customStyles[3]))
            {
                mic = (int)Mathf.Repeat(mic - 1, Microphone.devices.Length);
            }
            GUILayout.Label(Microphone.devices[mic]);
            if (GUILayout.Button("", GUI.skin.customStyles[4]))
            {
                mic = (int)Mathf.Repeat(mic + 1, Microphone.devices.Length);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (!recording)
            {
                if (GUILayout.Button("", GUI.skin.customStyles[4]))
                {
                    currentClip = Microphone.Start(Microphone.devices[mic],false,defaultLength,defaultSampleRate);
                    Debug.Log(Microphone.devices[mic] + " : " + Microphone.IsRecording(Microphone.devices[mic]));
                    clipSections = new List<AudioClip>();
                    lastPos = 0;
                    recording = true;
                    EditorApplication.update += RecordNextChunk;
                }
            }
            else
            {
                lastPos = Microphone.GetPosition(Microphone.devices[mic]);
                if (GUILayout.Button("Stop and save"))
                {
                    Microphone.End(Microphone.devices[mic]);
                    recording = false;
                    clipSections.Add(currentClip);
                    finalClip = ConstructFinalClip(clipSections, lastPos);
                    finalClip = SavWav.TrimSilence(finalClip, 0);
                    EditorApplication.update -= RecordNextChunk;
                    SavWav.Save("RecordedAudio", finalClip);
                }
            }

            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("No microphone found");
        }
    }

    void RecordNextChunk()
    {
        if (recording)
        {
            if (!Microphone.IsRecording(Microphone.devices[mic]))
            {
                clipSections.Add(currentClip);
                currentClip = Microphone.Start(Microphone.devices[mic], false, defaultLength, defaultSampleRate);
            }
        }
    }

    private AudioClip ConstructFinalClip(List<AudioClip> clipSections, int lastPos)
    {
        int channels = clipSections[0].channels;
        int sampleLength = clipSections.Count * clipSections[0].samples;
        //float[] lastSamples = new float[lastPos * channels];
        //clipSections[clipSections.Count - 1].GetData(lastSamples, 0);
        float[] allSamples = new float[sampleLength * channels];
        Debug.Log(allSamples.Length);
        int pos = 0;
        for(int i = 0; i < clipSections.Count-1; i++)
        {
            pos = i * clipSections[0].samples * channels;
            Debug.Log(pos);
            Debug.Log(clipSections[i].samples * channels);
            float[] currentSamples = new float[clipSections[i].samples * channels];
            clipSections[i].GetData(currentSamples, 0);
            currentSamples.CopyTo(allSamples, pos);
            //SavWav.Save("chunk" + i, clipSections[i]);
        }
        //allSamples.CopyTo(lastSamples, pos);
        AudioClip newClip = AudioClip.Create("Recorded Audio", sampleLength, channels, defaultSampleRate, false);
        newClip.SetData(allSamples, 0);
        return newClip;
    }
}
