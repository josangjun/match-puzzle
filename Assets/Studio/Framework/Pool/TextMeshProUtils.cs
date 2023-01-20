using UnityEngine;
using TMPro;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Studio.Framework
{
    // refer to : https://forum.unity.com/threads/bug-1-2-4-duplicate-textmeshpro-assets-in-bundles.754211/
    public static class TextMeshProUtils
    {
        static readonly FieldInfo settingsFieldInfo;

        public static TMP_Settings settings
        {
            get => (TMP_Settings)settingsFieldInfo.GetValue(null);
            set => settingsFieldInfo.SetValue(null, value);
        }

        static TextMeshProUtils()
        {
            settingsFieldInfo = typeof(TMP_Settings).GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(settingsFieldInfo != null, nameof(settingsFieldInfo) + " != null");
        }

        private static TMP_FontAsset OnFontAssetRequested(int hash, string path)
        {
            return OnAssetRequested<TMP_FontAsset>(hash, path+".asset");
        }

        private static Material OnMaterialAssetRequested(int hash, string path)
        {
            return OnAssetRequested<Material>(hash, path+".mat");
        }

        private static T OnAssetRequested<T>(int hash, string path) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/TextMesh Pro/Fonts/{path}");
            }
#endif
            var asyncOp = Addressables.LoadAssetAsync<T>(path);
            asyncOp.WaitForCompletion();
            var asset = asyncOp.Result;
            return asset;
        }

        public static void RegisterEvent()
        {
            TMP_Text.OnFontAssetRequest -= OnFontAssetRequested;
            TMP_Text.OnFontAssetRequest += OnFontAssetRequested;
            TMP_Text.OnMaterialAssetRequest -= OnMaterialAssetRequested;
            TMP_Text.OnMaterialAssetRequest += OnMaterialAssetRequested;
        }

        public static void LoadTMPSettings()
        {
            var asyncOp = Addressables.LoadAssetAsync<TMP_Settings>($"TMP Settings.asset");
            asyncOp.WaitForCompletion();
            TextMeshProUtils.settings = asyncOp.Result;

            RegisterEvent();
        }
    }
}

#if UNITY_EDITOR
namespace Studio.Framework.EditorInternal
{
    using UnityEditor;
    using System.Linq;

    [InitializeOnLoad]
    public static class InitTextMeshPro
    {
        static InitTextMeshPro()
        {
            if (!TextMeshProUtils.settings)
            {
                // Can't use our logger here because it's not initialized here.
                Debug.Log("Loading text mesh pro settings in editor.");
                var data = AssetDatabase.FindAssets("t:TMP_Settings").Select(guid =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<TMP_Settings>(path);
                    return asset;
                }).FirstOrDefault();
                TextMeshProUtils.settings = data;
            }
            TextMeshProUtils.RegisterEvent();
        }
    }
}
#endif