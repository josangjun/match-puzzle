using Studio.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework
{
    [System.Serializable]
    public class NamedAssetReference<T> : AssetReferenceT<T>, IAddressLink<T> where T : UnityEngine.Object
    {
        public NamedAssetReference(string guid) : base(guid)
        {
        }

        public string name;
        public string path;

        T IAddressLink<T>.Asset => (T)base.Asset;

        public void ValidateAsset()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(AssetGUID);
            ValidateAsset(path);
#endif
        }
        public override bool ValidateAsset(string mainAssetPath)
        {
            name = System.IO.Path.GetFileNameWithoutExtension(mainAssetPath);
            path = mainAssetPath;

            return base.ValidateAsset(mainAssetPath);
        }
    }
}