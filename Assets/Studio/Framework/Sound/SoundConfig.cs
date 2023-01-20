using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Audio;
using System.Linq;

namespace Studio.Framework.Audio
{
    [System.Serializable][Preserve]
    public class SoundPreset
    {
        public string name => file.name;
        public AudioClipLink file;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0f, 2f)]
        public float pitch = 1f;
        public float delay = 0f;
        public int limit = 0;
        public bool overriding;
        public bool loop;
        public bool mute;

        [System.NonSerialized]
        public AudioMixerGroup mixerGroup;

        [System.NonSerialized]
        public int count;
        [System.NonSerialized]
        public int frame = -1;
        
        public AudioClip cache => file.Asset;

        public bool IsExceeded()
        {
            return limit > 0 && count >= limit;
        }

        public bool IsProcessed()
        {
            return frame == Time.frameCount;
        }

        public void OnPlayed()
        {
            frame = Time.frameCount;
            if (limit == 0) return;
            ++count;
        }

        public void OnStopped()
        {
            if (limit == 0) return;
            --count;
        }

        public void OnValidate()
        {
            #if false
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:AudioClip {name}");
            if (guids.Length > 0) {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                file.name = name;
                file.guid = guids[0];
                file.path = path;
            }
            #endif
        }

        public void Release()
        {
            file.ReleaseAsset();
            frame = -1;
        }
    }
    
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Game/Audio/SoundConfig")]
    public class SoundConfig : ScriptableObject
    {
        private IDictionary<string, SoundPreset> _dict = new SortedList<string, SoundPreset>();
        
        [SerializeField]
        [NaughtyAttributes.Expandable]
        private SoundPresetGroup[] _presetGroups;

        public AudioMixer audioMixer;

        public bool logEnabled;
        
        public void FreshDictionary()
        {
            _dict.Clear();

            foreach (var presetGroup in _presetGroups)
            {
                presetGroup.Load(_dict);
            }
        }

        public ICollection<SoundPreset> presets => _dict.Values;

        public SoundPreset GetPreset(string presetName)
        {
            _dict.TryGetValue(presetName, out var preset);
            
            return preset;
        }

        [ContextMenu("Sort")]
        public void Sort()
        {
            foreach (var group in _presetGroups)
                group.Sort();
        }

        [ContextMenu("Check Redundancy")]
        public void CheckRedundancy()
        {
            var bag = new HashSet<string>();
            
            var sb = new System.Text.StringBuilder();

            void Push(IEnumerable<SoundPreset> presets)
            {
                foreach (var preset in presets)
                {
                    try
                    {
                        bag.Add(preset.name);
                    }
                    catch (System.Exception)
                    {
                        sb.Append(preset.file.path).AppendLine();
                    }
                }
            }

            Push(presets);
            
            foreach (var group in _presetGroups)
                Push(group.presets);

            if (sb.Length == 0)
                sb.Append("redundancy does not exist.");
            Debug.Log(sb);
        }

        public void Release()
        {
            foreach (var group in _presetGroups)
            {
                foreach (var preset in group.presets)
                {
                    preset.Release();
                }
            }
        }

        private void OnValidate()
        {
            if (_presetGroups == null)
                return;
                
            foreach (var group in _presetGroups)
            {
                if (group != null)
                    group.OnValidate();
            }
        }
    }
}
/*
#if UNITY_EDITOR
namespace Studio.Framework.Audio.InternalEditor
{
    using UnityEditor;
    
    [CustomEditor(typeof(SoundConfig))]
    public class SoundConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var it = serializedObject.GetIterator();
            it.NextVisible(true);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(it);
            EditorGUI.EndDisabledGroup();
            while (it.NextVisible(false))
            {
                EditorGUILayout.PropertyField(it);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
*/