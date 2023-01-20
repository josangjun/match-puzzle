using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Text;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework
{
    [CreateAssetMenu(menuName = "Game/EffectConfig", fileName = "EffectConfig.asset")]
    public class EffectConfig : ScriptableObject
    {
        [System.Serializable]
        public class EffectAsset {
            public string name;

            [SerializeField]
            private AssetReferenceEffect assetRef;

            public string guid => assetRef.AssetGUID;

            public Object asset => assetRef.Asset;

            public AsyncOperationHandle LoadAsync()
            {
                if (asset)
                    return assetRef.OperationHandle;

                ReleaseAsset();

                var handle = assetRef.LoadAssetAsync<Object>();
                handle.Completed += OnFinsihed;
                return handle;
            }

            public void ReleaseAsset()
            {
                if (assetRef.OperationHandle.IsValid())
                    assetRef.ReleaseAsset();
            }

            private void OnFinsihed(AsyncOperationHandle<Object> handle)
            {
                if (handle.Result == null)
                    Debug.LogError($"missing object: {name}, {guid}");
            }

            public GoItem Rent()
            {
                var go = assetRef.Asset as GameObject;
                if (go == null)
                    return null;
                var item = go.GetComponent<GoItem>();
                return Service.GoPool.Rent(item);
            }
        }

        [SerializeField]
        protected EffectAsset[] _preloadAssets = new EffectAsset[0];

        [SerializeField]
        protected EffectAsset[] _lazyAssets = new EffectAsset[0];

        protected IDictionary<string, EffectAsset> _dict = new SortedList<string, EffectAsset>();

        [ContextMenu("Sort")]
        public void Sort()
        {
            System.Comparison<EffectAsset> cmp = (x, y) => x.name.CompareTo(y.name);
            System.Array.Sort(_preloadAssets, cmp);
            System.Array.Sort(_lazyAssets, cmp);
        }

        public void OnEnable()
        {
            foreach (var slot in _preloadAssets)
            {
                _dict[slot.name] = slot;
            }
            foreach (var slot in _lazyAssets)
            {
                _dict[slot.name] = slot;
            }
        }

        public void OnDestroy()
        {
            foreach (var arg in _preloadAssets)
                arg.ReleaseAsset();
            foreach (var arg in _lazyAssets)
                arg.ReleaseAsset();
        }

        public IEnumerator Start()
        {
            var handles = new List<AsyncOperationHandle>();
            foreach (var slot in _preloadAssets)
            {
                var h = slot.LoadAsync();
                handles.Add(h);
            }
            foreach (var h in handles)
                yield return h;
        }

        public GoEffect Play(string effectName, GameObject owner)
        {
            var effect = Rent(effectName);
            if (effect) effect.Play(owner);
            return effect;
        }

        public GoEffect Play(string effectName, Vector3 point)
        {
            var effect = Rent(effectName);
            effect.Play(null);
            effect.transform.position = point;
            return effect;
        }

        public static string BuildEffectName(string starType = "")
        {
            return starType.IsEmpty() ? "build_yellow" : "build_" + starType.ToLower();
        }

        public GoEffect Rent(string effectName)
        {
            EffectAsset effectRef;
            _dict.TryGetValue(effectName, out effectRef);
            if (effectRef == null)
            {
                Debug.LogError($"missing : {effectName}");
                return null;
            }
            return effectRef.Rent() as GoEffect;
        }

        public Coroutine PlayAsync(string effectName, GameObject owner, System.Action<GoEffect> onRent)
        {
            return Service.Instance.StartCoroutine(PlayAsyncImpl(effectName, owner, onRent));
        }

        private IEnumerator PlayAsyncImpl(string effectName, GameObject owner, System.Action<GoEffect> onRent)
        {
            EffectAsset effectRef;
            if (!_dict.TryGetValue(effectName, out effectRef))
            {
                onRent?.Invoke(null);
                yield break;
            }
            if (!effectRef.asset)
                yield return effectRef.LoadAsync();
            var item = effectRef.Rent();
            var effect = (GoEffect)item;
            effect.Play(owner);
            onRent?.Invoke(effect);
        }

        private void OnDisable() {
            foreach (var slot in _preloadAssets)
                if (slot.asset)
                    slot.ReleaseAsset();
        }

        #if UNITY_EDITOR
        [System.NonSerialized]
        private IDictionary<string, EffectAsset> _guidDict = new SortedList<string, EffectAsset>();

        public void OnValidate()
        {
            _guidDict.Clear();
            Validate(_preloadAssets);
            Validate(_lazyAssets);
        }

        private void Validate(EffectAsset[] slots)
        {
            EffectAsset conflict = null;
            foreach (var slot in slots)
            {
                var guid = slot.guid;
                if (string.IsNullOrEmpty(guid))
                    continue;
                if (_guidDict.TryGetValue(guid, out conflict))
                {
                    Debug.LogError($"conflict: {guid}, {slot.name} = {conflict.name}");
                    continue;
                }
                _guidDict[slot.guid] = slot;
            }
        }
        #endif
    }
}