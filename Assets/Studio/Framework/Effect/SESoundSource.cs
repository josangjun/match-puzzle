using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Studio.Framework.Audio
{
    public interface ISoundEffect {
        GameObject gameObject { get; }
        T GetComponent<T>();
        bool HasFile(string fileName);
        bool IsMusic();
        AudioSource GetSource();
    }

    [RequireComponent(typeof(AudioSource))]
    public class SESoundSource : SubEffect, ISoundEffect, System.IEquatable<SoundPreset>
    {
        [SerializeField]
        private AudioClipInfo _info;
        
        [SerializeField]
        private AudioSource _source;

        public AudioSource GetSource() => _source;

        public AudioMixerGroup GetAudioMixerGroup() => _source.outputAudioMixerGroup;

        public AudioMixer GetAudioMixer() => _source.outputAudioMixerGroup.audioMixer;

        public bool IsMusic()
        {
            return "Music" == _preset?.mixerGroup?.name;
        }

        private SoundPreset _preset;
        
        public bool HasFile(string file)
        {
            if (_source == null || _source.clip == null)
                return false;
            return _source.clip.name == file;
        }

        public void SetClip(AudioClip clip)
        {
            _info.clip = clip;
            _source.clip = clip;
        }

        public override void Play(GameObject owner)
        {
            _source.clip = _info.clip;
            if (_info.delay > 0f)
                _source.PlayDelayed(_info.delay);
            else
                _source.Play();
            
            CheckPreset();
        }

        private void OnDisable()
        {
            if (_preset != null)
            {
                _preset.OnStopped();
                _preset = null;
            }
            _info.clip = null;
            _source.clip = null;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void CheckPreset()
        {
            #if UNITY_EDITOR
            IEnumerator CheckPresetAsync() {
                while (this && _preset != null)
                {
                    if (!Equals(_preset))
                        Apply(_preset);
                    yield return null;
                }
            }
            if (_preset != null)
                StartCoroutine(CheckPresetAsync());
            #endif
        }

        public void Apply(SoundPreset preset)
        {
            _preset = preset;
            if (preset == null)
                return;
            _info.delay = preset.delay;
            _source.pitch = preset.pitch;
            _source.volume = preset.volume;
            _source.loop = preset.loop;
            _source.mute = preset.mute;
            _source.outputAudioMixerGroup = preset.mixerGroup;
        }

        public void OnValidate()
        {
            if (_source == null)
                _source = GetComponent<AudioSource>();
        }

        public bool Equals(SoundPreset other)
        {
            return _source.pitch == other.pitch &&
                _source.volume == other.volume &&
                _source.loop == other.loop &&
                _source.mute == other.mute &&
                _source.outputAudioMixerGroup == other.mixerGroup;
        }
    }
}