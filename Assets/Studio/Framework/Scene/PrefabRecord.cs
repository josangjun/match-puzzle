using Studio.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework.SceneBuilding
{
    public enum LoadingKey { Guid, Name }

    [System.Serializable]
    public class PrefabRecord
    {
        [HideInInspector]
        public string name;
        public Transform transform;
        
        public AssetReferenceGameObject prefab;
        public Vector3 localPosition;
        public Vector3 localRotation;
        public Vector3 localScale;
        [HideInInspector]
        public RectTransform rectTransform;
        public Vector3 anchordPositon3D;
        public Vector2 anchorMin, anchorMax, pivot, sizeDelta, offsetMin, offsetMax;
        
        public int siblingIndex;
        
        private AsyncOperationHandle<GameObject> overrideHandle;

        public AsyncOperationHandle<GameObject> LoadAssetAsync(LoadingKey key = LoadingKey.Guid)
        {
            if (key == LoadingKey.Name)
            {
                return overrideHandle = Addressables.LoadAssetAsync<GameObject>(name);
            }
            if (prefab.OperationHandle.IsValid())
            {
                prefab.ReleaseAsset();
            }
            return prefab.LoadAssetAsync<GameObject>();
        }

        public void ReleaseAsset()
        {
            if (prefab.OperationHandle.IsValid())
            {
                prefab.ReleaseAsset();
            }
            if (overrideHandle.IsValid())
            {
                Addressables.Release(overrideHandle);
                overrideHandle = default;
            }
        }

        public PrefabRecord CopyFrom(Transform target, bool preserveSiblings = false)
        {
            #if UNITY_EDITOR
            if (target == null)
                return this;
            
            var rect = target as RectTransform;
            if (rect)
            {
                transform = rectTransform = rect;
                anchordPositon3D = rect.anchoredPosition3D;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                localRotation = rect.localEulerAngles;
                localPosition = rect.localPosition;
                sizeDelta = rect.sizeDelta;
                offsetMin = rect.offsetMin;
                offsetMax = rect.offsetMax;
            }
            else
            {
                transform = target;
                localPosition = target.localPosition;
                rectTransform = null;
                pivot = anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                anchordPositon3D = new Vector3(0, 0, 0);
            }
            name = target.name;
            localRotation = target.localEulerAngles;
            localScale = target.localScale;
            if (preserveSiblings == false)
                siblingIndex = target.GetSiblingIndex();
            const string EditorOnly = "EditorOnly";
            if (target.CompareTag(EditorOnly) == false)
                target.tag = EditorOnly;
            #endif
            return this;
        }
        
        public bool CheckDirty(bool preserveSiblings = false)
        {
            #if UNITY_EDITOR
            if (transform)
                CopyFrom(transform, preserveSiblings);
            #endif
            return false;
        }

        public void CopyTo(Transform target)
        {
            if (target.GetSiblingIndex() != siblingIndex &&
                target.parent.childCount - 1 >= target.GetSiblingIndex())
                target.SetSiblingIndex(siblingIndex);

            var rect = target as RectTransform;

            if (rect)
            {
                rect.offsetMax = offsetMax;
                rect.offsetMin = offsetMin;
                rect.sizeDelta = sizeDelta;
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = pivot;
                rect.localScale = localScale;
                rect.localEulerAngles = localRotation;
                rect.anchoredPosition3D = anchordPositon3D;
            }
            else if (target)
            {
                target.localEulerAngles = localRotation;
                target.localScale = localScale;
                target.localPosition = localPosition;
            }
        }
    }
}