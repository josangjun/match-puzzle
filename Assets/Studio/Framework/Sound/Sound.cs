using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Studio.Framework
{
    using Audio;
    using Common;

    [System.Serializable]
    public class AudioClipInfo
    {
        public AudioClip clip;
        public float delay;
    }

    [System.Serializable]
    public class AudioClipLink : SoftLink<AudioClip>
    {
        public void Play(Transform transform)
        {
            Service.Sound.Play(name, transform);
        }

        public GoEffect PlayImmediate(Transform parent)
        {
            return Service.Sound.PlayImmediate(name, parent);
        }

        public void Stop(Transform transform)
        {
            Service.Sound.Stop(name, transform);
        }

        public async Task<AudioClip> Preload()
        {
            return await Service.Sound.Load(name);
        }
    }

    public class Sound : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _effect;

        [SerializeField]
        private string _path;

        private SoundConfig _config;

        public SoundConfig config => _config;

        [SerializeField]
        private GoEffect _prefab;

        [SerializeField][Range(0, 1f)]
        private float _fxVolume = 1f;
        [SerializeField][Range(0, 1f)]
        private float _bgmVolume = 0.7f;

        private void Awake()
        {
            LoadConfig(_path);

            SetFxVolume(_fxVolume);
            SetBgmVolume(_bgmVolume);

            RegisterAudioConfigCallback();
        }

        private AsyncOperationHandle<SoundConfig> configHandle;

        private void LoadConfig(string path)
        {
            if (configHandle.IsValid())
                Addressables.Release(configHandle);
            configHandle = Addressables.LoadAssetAsync<SoundConfig>(path);
            _config = configHandle.WaitForCompletion();
            if (_config)
            {
                _config = configHandle.Result;
                _config.FreshDictionary();
            }
        }

        public float GetFxVolume() => _fxVolume;

        public void SetFxVolume(float volume)
        {
            _fxVolume = volume;
            var db = ToDecibel(_fxVolume);
            _config.audioMixer.SetFloat("FxVolume", db);
        }

        public float GetBgmVolume() => _bgmVolume;

        public void SetBgmVolume(float volume)
        {
            _bgmVolume = volume;
            var db = ToDecibel(_bgmVolume);
            _config.audioMixer.SetFloat("BgmVolume", db);
        }

        public async Task<AudioClip> Load(string clip)
        {
            var preset = _config.GetPreset(clip);

            if (preset.file.Asset == null)
            {
                return await preset.file.GetOrLoadAsync();
            }
            return preset.file.Asset;
        }

        public void Unload(string clip)
        {
            var preset = _config.GetPreset(clip);
            
            preset.file.ReleaseAsset();
        }

        public async void Play(string clip, Transform parent, float time)
        {
            Play(clip, parent);
            await Task.Delay(System.TimeSpan.FromSeconds(time));
            Stop(clip, parent);
        }

        public async void PlayNoWarn(string clip, Transform parent = null)
        {
            var preset = _config.GetPreset(clip);
            if (preset == null)
            {
                return;
            }

            var file = preset.file.Asset;
            if (file == null)
                file = await preset.file.GetOrLoadAsync();
            if (file)
            {
                PlayInternal(preset, file, parent);
                return;
            }
        }

        public async void Play(string clip, Transform parent = null)
        {
            var preset = _config.GetPreset(clip);
            if (preset == null)
            {
                Debug.LogWarning($"audioClip does not exist.:{clip}");
                return;
            }

            var file = preset.file.Asset;
            if (file == null)
                file = await preset.file.GetOrLoadAsync();
            if (file)
            {
                PlayInternal(preset, file, parent);
                return;
            }
        }

        public bool IsPlaying(string clip, Transform parent)
        {
            return FindEffect(clip, parent) != null;
        }

        internal void Play(AudioClip clip)
        {
            _effect.PlayOneShot(clip);
        }

        public GoEffect Play(AudioClip clip, Transform parent)
        {
            if (clip == null)
                return null;
            var preset = _config.GetPreset(clip.name);
            return PlayInternal(preset, clip, parent);
        }

        public GoEffect PlayImmediate(string file, Transform parent)
        {
            var preset = _config.GetPreset(file);
            if (preset.cache == null)
            {
                var asyncOp = preset.file.LoadAssetAsync();
                asyncOp.WaitForCompletion();
            }
            return PlayInternal(preset, preset.cache, parent);
        }

        private async Task<GoEffect> PlayInternal(SoundPreset preset, Transform parent)
        {
            if (preset.cache == null)
            {
                var asyncOp = preset.file.LoadAssetAsync();
                await asyncOp;
            }
            return PlayInternal(preset, preset.cache, parent);
        }

        private GoEffect PlayInternal(SoundPreset preset, AudioClip clip, Transform parent)
        {
            if (!Service.ready)
                return null;
            if (parent == null)
                parent = transform;
            var lifeTime = clip.length;
            if (preset != null)
            {
                if (preset.IsProcessed())
                    return null;

                if (preset.IsExceeded())
                {
                    if (!preset.overriding)
                        return null;
                    Stop(clip.name, parent);
                }
#if UNITY_EDITOR
                if (config.logEnabled)
                    SLogger.Log($"Sound.Play:{clip.name}", parent);
#endif
                preset.OnPlayed();
                lifeTime = preset.loop ? 0f : (clip.length * preset.pitch + preset.delay);
            }
        
            var effect = Service.GoPool.Rent(_prefab, parent);
            if (effect == null)
                return default;
            var source = (SESoundSource)effect[0];
            
            source.Apply(preset);
            source.SetClip(clip);

            effect.lifeTime = lifeTime;
            effect.Play(parent.gameObject);
            return effect;
        }

        public bool Stop(string file, Transform parent)
        {
            var effect = FindEffect(file, parent);
            if (effect)
            {
                effect.Return();
                return true;
            }
            return false;
        }

        public void PauseMusic(bool toggle)
        {
            var source = Service.Sound.GetMusicSource();
            if (!source)
                return;
            if (toggle)
                source.Pause();
            else
                source.UnPause();
        }

        public void StopMusic()
        {
            ISoundEffect sfx = null;
            ForEachEffect(arg =>
            {
                sfx = arg.GetSubEffect<ISoundEffect>();
                if (sfx.IsMusic())
                    arg.Return();
                return false;
            });
            if (sfx == null)
                return;
            var effect = sfx.GetComponent<GoEffect>();
            if (effect)
                effect.Return();
        }

        public bool PlayMusic(string clip, Transform parent = null)
        {
            bool alreadyPlaying = false;
            if (parent == null)
                parent = transform;

            ForEachEffect(parent, fx =>
            {
                var soundFx = fx.GetSubEffect<ISoundEffect>();
                if (soundFx == null || soundFx.IsMusic() == false)
                    return false;
                if (soundFx.HasFile(clip))
                {
                    alreadyPlaying = true;
                }
                else
                {
                    fx.Return();
                }
                return false;
            });

            if (alreadyPlaying == false)
            {
                var preset = _config.GetPreset(clip);
                if (preset == null)
                {
                    Debug.LogWarning($"audioClip does not exist.:{clip}");
                    return false;
                }

                _ = PlayInternal(preset, parent);
            }
            return true;
        }

        public AudioSource GetMusicSource()
        {
            AudioSource source = null;
            ForEachEffect(arg =>
            {
                var fx = arg.GetSubEffect<ISoundEffect>();
                if (!fx.IsMusic()) return false;
                source = fx.GetSource();
                return true;
            });
            return source;
        }

        public void ForEachEffect(System.Func<GoEffect, bool> predicate)
        {
            ForEachEffect(transform, predicate);
        }

        private const string kAudioSource = "AudioSource";

        public void ForEachEffect(Transform parent, System.Func<GoEffect, bool> predicate)
        {
            for (var i = parent.childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i);
                if (!child.gameObject.activeSelf)
                    continue;
                if (child.name != kAudioSource)
                    continue;
                var effect = child.GetComponent<GoEffect>();
                if (effect == null)
                    continue;
                if (predicate(effect))
                    return;
            }
        }

        public GoEffect FindEffect(string file, Transform parent) 
        {
            if (parent == null)
                parent = transform;
            GoEffect hit = null;
            ForEachEffect(parent, effect =>
            {
                var soundEffect = effect.GetSubEffect<ISoundEffect>();
                if (soundEffect == null || !soundEffect.HasFile(file))
                    return false;
                hit = effect;
                return true;
            });
            return hit;
        }

        private void OnValidate()
        {
            if (_config)
            {
                SetFxVolume(_fxVolume);
                SetBgmVolume(_bgmVolume);
            }
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        private void RegisterAudioConfigCallback()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigChanged;
        }

        private void UnregisterAudioConfigCallback()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigChanged;
        }

        private void OnAudioConfigChanged(bool deviceWasChanged)
        {
            SLogger.Log($"OnAudioConfigChanged:{deviceWasChanged}");

            if (deviceWasChanged)
            {
                var config = AudioSettings.GetConfiguration();
                MEC.Timing.CallDelayed(1 / 60f, () =>
                  AudioSettings.Reset(config));
            }

            SetFxVolume(_fxVolume);
            SetBgmVolume(_bgmVolume);
            var source = GetMusicSource();
            if (source)
            {
                var t = source.time;
                source.Play();
                source.time = t;
            }

            var videoGo = GameObject.FindGameObjectWithTag("Video");
            var video = videoGo ? videoGo.GetComponent<UnityEngine.Video.VideoPlayer>() : default;
            if (video)
            {
                var t = video.time;
                video.Stop();
                video.Play();
                video.time = t;
            }
        }

        public static float ToDecibel(float val)
        {
            val = Mathf.Max(0.0001f, val);
            val = Mathf.Log10(val) * 20f;
            return val;
        }

        private void OnDestroy()
        {
            if (_config)
                _config.Release();
            if (configHandle.IsValid())
                Addressables.Release(configHandle);

            UnregisterAudioConfigCallback();
        }
    }
}