using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UnityEngine.AddressableAssets
{

    [System.Serializable]
    public class AssetReferenceScene : AssetReference
    {
        public AssetReferenceScene(string guid) : base(guid) {}

        public override bool ValidateAsset(string path)
        {
            return path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase);
        }
    }

    [System.Serializable]
    public class AssetReferenceAtlas : AssetReferenceT<SpriteAtlas>
    {
        public AssetReferenceAtlas(string guid) : base(guid) {}
    }
}