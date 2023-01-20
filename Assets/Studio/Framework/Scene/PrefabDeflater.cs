using Studio.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Studio.Framework.SceneBuilding
{
    public interface IValidator : IEventSystemHandler
    {
        void OnValidate();
    }
    
    [ExecuteAlways]
    public class PrefabDeflater : MonoBehaviour
    #if UNITY_EDITOR
    , ISerializationCallbackReceiver {

        void ISerializationCallbackReceiver.OnAfterDeserialize() {}
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (Application.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer || !this)
                return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                ValidateInternal();
            };
        }

#else
    {
#endif
        public LoadingKey key = LoadingKey.Guid;
        
        [SerializeField]
        private List<PrefabRecord> _records = new List<PrefabRecord>();

        public List<PrefabRecord> records => _records;

        [SerializeField]
        private bool _preserve;

        public bool preserve => _preserve;
        
        [SerializeField]
        private bool _preserveTag;

        private void Start()
        {
            #if UNITY_EDITOR
            if (preserve == false)
            {
                IsDone = true;
            }
            if (Application.isPlaying == false)
                return;
            #endif
            Load();
        }

        public CountingLock inprogress = new CountingLock();
        
        public static PrefabDeflater[] FindInScene(Scene scene)
        {
            return GameObject.FindGameObjectsWithTag(TypeName)
                .Select(arg => arg.GetComponent<PrefabDeflater>())
                .Where(arg => arg != null && arg.gameObject.scene.Equals(scene)).ToArray();
        }

        public static IEnumerator Wait(Scene scene = default)
        {
            if (!scene.IsValid())
                yield break;

            foreach (var go in GameObject.FindGameObjectsWithTag(TypeName))
            {
                if (scene.IsValid() && !go.scene.Equals(scene))
                    continue;
                var deflater = go.GetComponent<PrefabDeflater>();
                while (deflater && !deflater.IsDone)
                    yield return null;
            }
        }

        public AssetReference FirstOrDefault(System.Func<string, bool> func)
        {
            var record = _records.FirstOrDefault(arg => func(arg.name));
            if (record != null)
                return record.prefab;
            return null;
        }


        [ContextMenu("Load")]
        private void Load()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                foreach (var record in _records)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(record.prefab.AssetGUID);
                    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!prefab)
                        continue;
                    var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
                    instance.name = record.name;
                    record.CopyTo(instance.transform);
                }
                return;
            }
#endif
            StartCoroutine(LoadInternalAsync());
        }

        private IEnumerator LoadInternalAsync()
        {
            IsDone = false;
            using var h = new ScopeExit(() => IsDone = true);
            var opList = new List<AsyncOperationHandle<GameObject>>();
            
            foreach (var record in _records)
            {
                var asyncOp = record.LoadAssetAsync(key);
                opList.Add(asyncOp);
                StartCoroutine(TrackHandle(record, asyncOp));
            }
            
            foreach (var op in opList)
            {
                while (!op.IsDone)
                    yield return op;
            }
        }

        private IEnumerator TrackHandle(PrefabRecord record, AsyncOperationHandle<GameObject> handle)
        {
            using var h = inprogress.Lock();
            yield return handle;
            if (!this)
            {
                Addressables.Release(handle);
                yield break;
            }
            var prefab = handle.Result;
            if (prefab == null)
            {
                Debug.LogError($"missing prefab : {record.name}", gameObject);
                yield break;
            }
            var existed = record.transform ? record.transform : transform.Find(prefab.name);
            if (existed != null)
            {
                existed.gameObject.tag = prefab.tag;
                yield break;
            }
            var go = Instantiate(prefab, transform);
            go.name = record.name;
            record.CopyTo(go.transform);
        }

        private bool _isDone;
        public bool IsDone {
            get => _isDone && inprogress.count == 0;
            private set => _isDone = value;
        }

        static string TypeName => nameof(PrefabDeflater);

        #if UNITY_EDITOR
        private void Update()
        {
            if (dirty > 0)
            {
                if (!Application.isPlaying)
                    ValidateInternal();
                --dirty;
            }
        }

        private bool ValidateInternal()
        {
            if (!this)
                return false;

            if (!_preserveTag && !CompareTag(TypeName))
                gameObject.tag = TypeName;
            
            var changed = false;
            if (preserve == false)
            {
                for (var i = _records.Count-1; i >= 0; --i)
                {
                    var rec = _records[i];
                    if (rec != null && rec.transform)
                        continue;
                    _records.RemoveAt(i);
                    Debug.Log($"Deflater.Remove:{rec.name}");
                    changed = true;
                }
            }

            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                ExecuteEvents.Execute<IValidator>(child.gameObject, null, (x, y) => x.OnValidate());
            }

            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                
                var status = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(child);
                if (status != UnityEditor.PrefabInstanceStatus.Connected)
                    continue;
                var assetType = UnityEditor.PrefabUtility.GetPrefabAssetType(child);
                if (assetType == UnityEditor.PrefabAssetType.NotAPrefab ||
                    assetType == UnityEditor.PrefabAssetType.MissingAsset ||
                    assetType == UnityEditor.PrefabAssetType.Model)
                    continue;
                var path = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                    continue;
                if (_records.Any(arg => arg.prefab.AssetGUID == guid))
                    continue;
                AddressTool.MakeAddressFromGUID(guid);
                var prefabRec = new PrefabRecord {
                    prefab = new AssetReferenceGameObject(guid)
                }.CopyFrom(child);
                
                _records.Add(prefabRec);
                changed = true;
            }
            
            foreach (var record in _records)
            {
                changed = record.CheckDirty(preserve) || changed;
            }
            if (!preserve)
                _records.Sort((x, y) => x.siblingIndex.CompareTo(y.siblingIndex));
            if (changed && !Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            return changed;
        }

        private int dirty;

        private void OnValidate()
        {
            dirty = 100;
        }

        #endif

        public void SetTransform(Transform target, string name)
        {
            var record = _records.FirstOrDefault(arg => arg.name == name);
            if (record == null) return;
            record.CopyTo(target);
        }

        private void OnDestroy()
        {
            ReleaseAsset();
        }

        public void ReleaseAsset()
        {
            foreach (var record in _records)
            {
                record.ReleaseAsset();
            }
        }
    }
}