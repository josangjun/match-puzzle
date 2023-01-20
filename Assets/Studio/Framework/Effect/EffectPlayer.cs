using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Studio.Framework
{
    [DisallowMultipleComponent]
    public class EffectPlayer : MonoBehaviour, IEffectPlayer
    {
        public GameObject target { get; set; }
        
        private SortedList<int, GoEffect> instances;

        public bool ignoreEvent;

        public void Play(AnimationEvent evt)
        {
            PlayEvent(evt);
        }

        public void PlayEvent(AnimationEvent evt)
        {
            if (ignoreEvent)
            {
                return;
            }
            var prefab = evt.objectReferenceParameter as GameObject;
            if (!prefab) return;
            var asset = prefab.GetComponent<GoEffect>();
            PlayEffect(asset);
        }

        public void ReleaseChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; --i)
            {
                var child = transform.GetChild(i);
                var item = child.GetComponent<GoItem>();
                if (item == null)
                    continue;
                item.Return();
            }
        }

        public void PlayEffect(GoEffect asset)
        {
            var effect = (GoEffect)Service.GoPool.Rent(asset);
            if (!effect) return;
            
            if (!effect.Play(gameObject))
                return;
            
            if (effect.canStop)
            {
                if (instances == null)
                    instances = new SortedList<int, GoEffect>();
                
                var id = asset.GetInstanceID();
                instances[id] = effect;
                effect.OnDisableAsObservable()
                    .FirstOrDefault()
                    .Subscribe(_ => OnEffectDisable(effect, id))
                    .AddTo(this);
            }
        }

        private void OnEffectDisable(Object effect, int id)
        {
            instances.Remove(id);
        }

        public bool HasEffectInstance()
        {
            return instances != null && instances.Count > 0;
        }

        public void PlaySound(AnimationEvent evt)
        {
            if (string.IsNullOrEmpty(evt.stringParameter) == false)
            {
                Service.Sound.Play(evt.stringParameter, transform);
                return;
            }

            var clip = evt.objectReferenceParameter as AudioClip;
            if (clip)
                Service.Sound.Play(clip, transform);
        }

        public void StopSound(AnimationEvent evt)
        {
            Service.Sound.Stop(evt.stringParameter, transform);
        }

        public void Stop(AnimationEvent evt)
        {
            if (instances == null || instances.Count == 0)
                return;

            var prefab = evt.objectReferenceParameter as GameObject;
            if (prefab)
            {
                var asset = prefab.GetComponent<GoEffect>();
                if (asset != null)
                {
                    var id = asset.GetInstanceID();
                    if (instances.TryGetValue(id, out var inst))
                        inst.Return();
                }
            }
            if (string.IsNullOrEmpty(evt.stringParameter) == false)
            {
                StopByName(evt.stringParameter);
            }
        }

        public void StopAll()
        {
            if (instances == null || instances.Count == 0)
                return;

            for (var i = instances.Count - 1; i >= 0; --i)
            {
                var inst = instances.Values[i];
                if (inst != null)
                {
                    inst.Return();
                }
            }
        }

        public void StopByName(string targetName)
        {
            if (instances == null || instances.Count == 0)
                return;
                
            for (var i = instances.Count-1; i >= 0; --i)
            {
                var inst = instances.Values[i];
                if (inst != null && inst.name == targetName)
                {
                    inst.Return();
                }
            }
        }

        public void OnAccepted()
        {
        }
    }
}