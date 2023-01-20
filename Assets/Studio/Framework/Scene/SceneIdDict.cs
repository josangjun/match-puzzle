using UnityEngine;
using System.Collections.Generic;

#pragma warning disable 649

namespace Studio.Framework
{
    public class SceneIdDict : MonoBehaviour
    {
        [SerializeField]
        private SceneIdentifier[] _identifiers;

        public void Awake()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        private void RefreshFromMenu()
        {
            _dict.Clear();
            Refresh();
        }

        private void Refresh()
        {
            for (var i = 0; i < _identifiers.Length; ++i)
            {
                var id = _identifiers[i];
                if (id && !id.isActiveAndEnabled)
                    Add(id);
            }
        }

        public void OnValidate()
        {
            Refresh();
        }

        public class GUIDComparer : IComparer<GUID>
        {
            public int Compare(GUID x, GUID y)
            {
                return x.CompareTo(y);
            }
        }

        private static SortedList<GUID, System.WeakReference> _dict = new SortedList<GUID, System.WeakReference>(new GUIDComparer());


        public static void Add(SceneIdentifier identifier)
        {
#if UNITY_EDITOR
            if (ResourceEntry.IsPrefab(identifier) || ResourceEntry.IsPrefabStage(identifier))
                return;
#endif
            if (Application.isPlaying && identifier.registered)
                return;
#if UNITY_EDITOR
            if (_dict.TryGetValue(identifier.guid, out var existedRef))
            {
                if (!ReferenceEquals(existedRef.Target, identifier) && !Application.isEditor)
                    Debug.Log($"already registered:{existedRef.Target}, {identifier}");
            }
#endif
            _dict[identifier.guid] = new System.WeakReference(identifier);
            identifier.registered = true;
        }

        public static System.WeakReference FindRef(GUID guid)
        {
            System.WeakReference wref;
            if (_dict.TryGetValue(guid, out wref))
            {
                return wref;
            }
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                foreach (var arg in FindObjectsOfType<SceneIdentifier>())
                {
                    wref = new System.WeakReference(arg);
                    _dict[arg.guid] = wref;
                    if (arg.guid.Equals(guid))
                        return wref;
                }
            }
            #endif
            return null;
        }

        public static SceneIdentifier Find(GUID guid)
        {
            var wref = FindRef(guid);
            var target = wref?.Target as SceneIdentifier;
            if (target) return target;
            Remove(guid);
            return null;
        }

        public static void Remove(GUID guid)
        {
            _dict.Remove(guid);
        }
    }

    public static class ResourceEntry
    {
        private static GameObject AsGameObject(Object goOrCom)
        {
            var go = goOrCom as GameObject;
            if (!go)
            {
                var com = goOrCom as Component;
                go = com != null ? com.gameObject : null;
            }
            return go;
        }
        
#if UNITY_EDITOR
        private static UnityEditor.SceneManagement.PrefabStage GetPrefabStage(Object goOrCom)
        {
            var go = AsGameObject(goOrCom);
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go);
            return stage;
        }
#endif
        public static bool IsPrefabStage(Object goOrCom)
        {
            #if UNITY_EDITOR
            return GetPrefabStage(goOrCom) != null;
            #else
            return false;
            #endif
        }

        public static bool IsPrefab(UnityEngine.Object goOrCom)
        {
#if UNITY_EDITOR
            var isPartOfPrefab = UnityEditor.PrefabUtility.IsPartOfPrefabAsset(goOrCom);
            if (isPartOfPrefab == false)
            {
                return IsPrefabStage(goOrCom);
            }
            return isPartOfPrefab;
#else
            return false;
#endif
        }
    }
}