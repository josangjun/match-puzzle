
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.Framework
{
    public class AudioClipPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioClipLink[] _clips;

        public int clipCount => _clips.Length;

        public enum Event {
            None, Enable = 1, Disable = 2, Start = 3
        }

        [SerializeField]
        private Event _event;

        [SerializeField]
        private bool _preload;

        private IEnumerator Start()
        {
            while (Service.Instance == null)
                yield return null;
            if (_event == Event.Start)
                PlaySound();
            if (_preload)
            {
                foreach (var clip in _clips)
                {
                    _ = clip.Preload();
                }
            }
        }

        public void PlaySound()
        {
            if (_clips.Length == 0)
                return;
            PlaySoundAt(0);
        }

        public void PlaySoundEvent(AnimationEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.stringParameter))
            {
                PlaySound(evt.stringParameter);
            }
            else
            {
                PlaySoundAt(evt.intParameter);
            }
        }

        public void PlaySound(string file)
        {
            if (Service.ready)
                Service.Sound.Play(file, transform);
        }

        public void PlayOneShot(string file)
        {
            if (Service.ready)
                Service.Sound.Play(file);
        }

        public void PlaySoundAt(int index)
        {
            if (index >= _clips.Length)
            {
                Debug.LogError($"IndexOutOfRange:{gameObject.GetHierarchy()}, {index}/{_clips.Length}");
                return;
            }
            var file = _clips[index];
            file.Play(transform);
        }

        public void Stop(string file)
        {
            if (Service.ready)
                Service.Sound.Stop(file, transform);
        }

        public void StopSound(string file)
        {
            if (Service.ready)
                Service.Sound.Stop(file, transform);
        }

        public void StopSoundAt(int index)
        {
            if (index >= _clips.Length)
            {
                Debug.LogError($"IndexOutOfRange:{gameObject.GetHierarchy()}, {index}/{_clips.Length}");
                return;
            }
            var file = _clips[index];
            file.Stop(transform);
        }

        private void OnEnable()
        {
            if (_event == Event.Enable)
            {
                this.CallDelayed(1/60f, PlaySound);
            }
        }

        private void OnDisable()
        {
            if (_event == Event.Disable)
                PlaySound();
        }

        #if UNITY_EDITOR
        [ContextMenu("Generate SoundEvent")]
        private void GenerateSoundEvent()
        {
            var sb = new System.Text.StringBuilder();
            var playMethods = new [] { "PlaySound", "PlayOneShot" };
            var cancelCandiates = new [] { "cancel", "close" };
            var okay = "ui_click_global";
            var cancel = "ui_click_cancel";
            var count = 0;
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                var clickEvent = button.onClick;
                var alreadyExist = false;
                for (var i = 0; i < clickEvent.GetPersistentEventCount(); ++i)
                {
                    var target = clickEvent.GetPersistentTarget(i);
                    var method = clickEvent.GetPersistentMethodName(i);
                    if (ReferenceEquals(target, this) && playMethods.Contains(method))
                    {
                        alreadyExist = true;
                        break;
                    }
                    else if (target == null || ((target is View) && playMethods.Contains(method)))
                    {
                        UnityEditor.Events.UnityEventTools.RemovePersistentListener(clickEvent, i);
                    }
                }
                if (alreadyExist) 
                    continue;
                var name = button.name.ToLower();
                var clip = cancelCandiates.Any(arg => name.Contains(arg)) ? cancel : okay;
                sb.Append($"{button.gameObject.GetHierarchy()} : {clip}").AppendLine();
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(clickEvent, this.PlaySound, clip);
                ++count;
            }
            if (count > 0)
                UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log(sb);
        }
        #endif
    }
}