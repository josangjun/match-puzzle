using Studio.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace Studio.Framework
{
    [System.Serializable]
    public class AssetLabel<T> where T : Object
    {
        public AssetLabel(string name, string guid, string path)
        {
            m_Asset = new SoftLink<T>(guid, path);
            this.name = string.IsNullOrEmpty(name) ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
        }

        public string name;

        [SerializeField]
        [HideInInspector]
        private int m_Hash;
        public int hash => m_Hash;
        [SerializeField]
        private SoftLink<T> m_Asset;

        public T Asset => m_Asset.Asset;

        public string guid
        {
            get => m_Asset.guid;
            set => m_Asset.guid = value;
        }

        public string path => m_Asset.path;

        public void Release()
        {
            if (m_Asset.OperationHandle.IsValid())
                m_Asset.ReleaseAsset();
        }

        public T GetAsset()
        {
            if (m_Asset.Asset)
            {
                return m_Asset.Asset;
            }
            var asyncOp = m_Asset.LoadAssetAsync();
            asyncOp.WaitForCompletion();
            return asyncOp.Result;
        }

        public async Task<T> GetOrLoadAsync()
        {
            return await m_Asset.GetOrLoadAsync();
        }

        public static implicit operator T(AssetLabel<T> label)
        {
            return label != null ? label.Asset : null;
        }
    }

    [System.Serializable]
    public class AssetCategory<T> where T : Object
    {
        public string name;
        public string path;

        [SerializeField]
        public List<AssetLabel<T>> m_Labels;

        public List<AssetLabel<T>> labels => m_Labels;

        public T GetAsset(string name)
        {
            var result = m_Labels.BinOrSeqSearch(arg => arg.name.CompareTo(name));
            if (result != null)
            {
                return result.GetAsset();
            }
            return null;
        }

        public T GetAsset(int index)
        {
            return labels[index].GetAsset();
        }

        public string GetName(int index)
        {
            return labels[index].name;
        }

        public void SetLabels(AssetLabel<T>[] labels)
        {
            m_Labels.Clear();
            m_Labels.AddRange(labels);
        }

        public int Count => labels.Count;

        public void Sort()
        {
            m_Labels.Sort((x, y) =>
            {
                if (x.name == null || y.name == null)
                {
                    return 0;
                }
                return x.name.CompareTo(y.name);
            });
        }

        public async Task<T> GetAssetAsync(string name)
        {
            foreach (var label in m_Labels)
            {
                if (label.name != name)
                    continue;
                if (label.Asset)
                    return label.Asset;
                var asset = await label.GetOrLoadAsync();
                if (asset != null)
                    return asset;
            }

            {
                await LoadAssetAsync(name);
                var label = GetLabel(name);
                return label != null ? label.Asset : null;
            }
        }

        public AssetLabel<T> GetLabel(string filename)
        {
            return m_Labels.BinOrSeqSearch(arg => arg.name.CompareTo(filename));
        }

        public async Task LoadAssetAsync(string filename)
        {
            var label = GetLabel(filename);
            if (label == null)
            {
                var path = string.Format(this.path, filename);
                label = new AssetLabel<T>(filename, string.Empty, path);
                m_Labels.Add(label);
                Sort();
            }
            var result = await label.GetOrLoadAsync();

            if (result == default)
            {
                Debug.LogWarning(filename);
            }
        }

        public void Clear()
        {
            Release();
            labels.Clear();
        }

        public void Release()
        {
            foreach (var label in labels)
            {
                label.Release();
            }
        }
    }


    public class AssetLib<T> : ScriptableObject where T : Object
    {
        [SerializeField]
        private List<AssetCategory<T>> m_Categories;

        public T GetAsset(string category, string name)
        {
            var cat = m_Categories.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return default;
            return cat.GetAsset(name);
        }

        public async Task LoadAssetAsync(string category, string name)
        {
            var cat = m_Categories.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return;
            await cat.LoadAssetAsync(name);
        }

        public async Task LoadAssetAsync(string name)
        {
            foreach (var cat in m_Categories)
            {
                await cat.LoadAssetAsync(name);
                break;
            }
        }

        public async Task<T> GetAssetAsync(string name)
        {
            foreach (var cat in m_Categories)
            {
                if (cat == null) return default;
                var asset = await cat.GetAssetAsync(name);
                if (asset)
                    return asset;
            }
            return default;
        }

        public async Task<T> GetAssetAsync(string category, string name)
        {
            var cat = m_Categories.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return default;
            return await cat.GetAssetAsync(name);
        }

        public bool Contains(string category, string name)
        {
            var cat = m_Categories.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return false;
            return cat.GetLabel(name) != null;
        }

        public void Clear()
        {
            foreach (var cat in m_Categories)
                cat.Clear();
        }

        public void Release()
        {
            foreach (var cat in m_Categories)
                cat.Release();
        }
    }
}