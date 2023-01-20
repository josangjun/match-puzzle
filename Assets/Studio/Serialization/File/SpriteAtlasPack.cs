using UnityEngine;
using UnityEngine.U2D;

namespace Studio.Common
{
    [CreateAssetMenu(fileName = "SpriteAtlasPack", menuName = "Game/SpriteAtlasPack")]
    public class SpriteAtlasPack : ScriptableObject
    {
        [System.Serializable]
        public class SpriteAtlasInfo {
            [HideInInspector]
            public string name;
            public AssetLink<SpriteAtlas> atlasRef;
            public void OnValidate()
            {
                #if UNITY_EDITOR
                if (string.IsNullOrEmpty(atlasRef.AssetGUID))
                {
                    name = string.Empty;
                    return;
                }
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(atlasRef.AssetGUID);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                if (asset)
                    name = asset.name;
                #endif
            }
        }

        [ArrayElementTitle("atlasRef")]
        [SerializeField]
        private SpriteAtlasInfo[] atlasInfos;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var info in atlasInfos)
                info.OnValidate();
        }

        [ContextMenu("Sort")]
        private void Sort()
        {
            OnValidate();
            System.Array.Sort(atlasInfos, (lhs, rhs) => lhs.name.CompareTo(rhs.name));
        }
        #endif

        public void Release()
        {
            foreach (var info in atlasInfos)
            {
                if (info.atlasRef.OperationHandle.IsValid())
                {
                    if (info.atlasRef.Asset)
                        info.atlasRef.ReleaseAsset();
                }
            }
        }

        public IAddressLink<SpriteAtlas> Find(string tag)
        {
            for (var i = 0; i < atlasInfos.Length; ++i) {
                if (atlasInfos[i].name == tag)
                    return atlasInfos[i].atlasRef;
            }
            return null;
        }
    }
}