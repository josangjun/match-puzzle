using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Studio.Framework.Audio
{
    [CreateAssetMenu(fileName = "SondPresetGroup", menuName = "Game/Audio/SondPresetGroup")]
    public class SoundPresetGroup : ScriptableObject
    {
        public AudioMixerGroup mixerGroup;

        [ArrayElementTitle("file.name")]
        public SoundPreset[] presets;

        public void OnValidate()
        {
            foreach (var pr in presets)
            {
                pr.count = 0;
                pr.mixerGroup = mixerGroup;
                pr.OnValidate();
            }
        }

        public void Load(IDictionary<string, SoundPreset> dict)
        {
            foreach (var pr in presets)
            {
                pr.mixerGroup = mixerGroup;
                pr.count = 0;
                try
                {
                    dict.Add(pr.name, pr);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"name:{pr.name}\n {e.Message}");
                    Debug.LogWarning(e.StackTrace);
                }
            }
        }

        [ContextMenu("Sort")]
        public void Sort()
        {
            System.Array.Sort(presets, (lhs, rhs) => lhs.name.CompareTo(rhs.name));
            foreach (var arg in presets)
            {
                arg.file.SyncPath();
            }
        }
    }
}