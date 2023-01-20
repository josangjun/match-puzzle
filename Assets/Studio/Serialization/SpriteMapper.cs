using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace Studio.Common
{
    public class SpriteMapper : MonoBehaviour
    {
        public AssetLink<SpriteAtlasPack> packRef;

        private SortedList<string, SpriteAtlas> _atlasDict = new SortedList<string, SpriteAtlas>();

        public SpriteAtlasPack atlasPack { get; private set; }

        protected void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        protected void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= RequestAtlas;

            ReleaseAtlas();
        }

        string[] langCodes = new[] { "tw", "ja", "ko", "en" };

        private bool IsLangCode(string tag)
        {
            if (string.IsNullOrEmpty(tag) || tag.Length != 2)
                return false;
            return langCodes.Contains(tag);
        }

        public async void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            if (IsLangCode(tag))
                return;

            SpriteAtlas atlas = FindAtlas(tag);
            if (atlas != null)
            {
                callback.Invoke(atlas);
                return;
            }

            if (atlasPack == null)
            {
                atlasPack = await packRef.GetOrLoadAsync();
                if (atlasPack == null)
                    return;
            }

            var atlasRef = atlasPack.Find(tag);

            if (atlasRef != null)
            {
                if (!atlasRef.Asset)
                {
                    var task = atlasRef.GetOrLoadAsync();
                    var status = atlasRef.OperationHandle.GetDownloadStatus();
                    if (status.TotalBytes == 0)
                    {
                        atlasRef.OperationHandle.WaitForCompletion();
                    }
                    else
                    {
                        await task;
                    }
                }
                atlas = atlasRef.Asset;
                callback.Invoke(atlas);
                return;
            }
            
            var asyncOp = Addressables.LoadAssetAsync<SpriteAtlas>($"Assets/Scenes/Spin/Atlas/{tag}.spriteatlas");
            asyncOp.Completed += (handle) =>
            {
                var atlas = handle.Result;
                callback.Invoke(atlas);
            };
        }

        public SpriteAtlas FindAtlas(string tag)
        {
            SpriteAtlas atlas;
            if (_atlasDict.TryGetValue(tag, out atlas))
            {
                if (atlas == null)
                    _atlasDict.Remove(tag);
            }
            return atlas;
        }

        public void AddAtlas(string tag, SpriteAtlas atlas)
        {
            _atlasDict[tag] = atlas;
        }

        public bool RemoveAtlas(string tag)
        {
            return _atlasDict.Remove(tag);
        }


        [ContextMenu("ReleaseAtlas")]
        public void ReleaseAtlas()
        {
            if (packRef.IsValid())
                packRef.ReleaseAsset();
            _atlasDict.Clear();
            if (atlasPack)
            {
                atlasPack.Release();
                atlasPack = null;
            }
        }
    }
}