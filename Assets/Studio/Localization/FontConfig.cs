using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Common
{
    [CreateAssetMenu(menuName = "Game/FontConfig")]
    public class FontConfig : ScriptableObject
    {
        private static FontConfig _asset;

        [SerializeField]
        private bool _preload;

        [System.Serializable]
        public class MatProp
        {
            public MatProp(Material material, AsyncOperationHandle handle)
            {
                this.material = material;
                this.handle = handle;
            }
            public MatProp(Material material, bool instance)
            {
                this.material = material;
                this.instance = instance;
            }
            private bool instance;
            private AsyncOperationHandle handle;
            private Material material;
            public void Release()
            {
                if (instance)
                {
                    Destroy(material);
                    instance = false;
                }
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    handle = default;
                }
            }
            public static implicit operator Material(MatProp prop) =>
                prop?.material;
        }

        private readonly IDictionary<string, MatProp> presets = new SortedList<string, MatProp>();

        private AsyncOperationHandle hAsset;

        private AsyncOperationHandle hPreload;

        public Material GetMaterial(string name)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return default;
#endif
            if (presets.TryGetValue(name, out var mat))
                return mat;
            
            var path = $"Fonts & Materials/{name}.mat";
            //var hLoc = Addressables.LoadResourceLocationsAsync(path, typeof(Material));
            //hLoc.WaitForCompletion();
            //if (hLoc.Status != AsyncOperationStatus.Succeeded || hLoc.Result.Count == 0)
            //{
            //    SLogger.LogWarning($"missing material:{name}");
            //    return default;
            //}
            var hMat = Addressables.LoadAssetAsync<Material>(path);
            hMat.WaitForCompletion();
            var material = hMat.Result;
            if (hMat.Status == AsyncOperationStatus.Succeeded)
                presets[name] = new MatProp(material, hMat);
            return material;
        }

        public void SetMaterial(string name, Material mat)
        {
            UnityEngine.Assertions.Assert.IsNotNull(mat);
            presets[name] = new MatProp(mat, true);
        }

        public static void Init(string path)
        {
            if (_asset)
                return;

            var asyncOp = Addressables.LoadAssetAsync<FontConfig>(path);
            asyncOp.WaitForCompletion();
            _asset = asyncOp.Result;
            if (_asset)
            {
                _asset.hAsset = asyncOp;
                _asset.Preload();
            }
        }

        public static FontConfig GetAsset()
        {
            return _asset;
        }

        private void Preload()
        {
            if (!_preload)
                return;
            
            hPreload = Addressables.LoadAssetsAsync<Material>("font", mat =>
            {
                presets[mat.name] = new MatProp(mat, false);
            });
            hPreload.WaitForCompletion();
        }

        public static void ReleaseAsset()
        {
            if (_asset)
                _asset.ReleaseAsset_();
        }

        private void ReleaseAsset_()
        {
            foreach (var kv in presets)
                kv.Value.Release();
            presets.Clear();

            if (hPreload.IsValid())
            {
                Addressables.Release(hPreload);
                hPreload = default;
            }

            if (hAsset.IsValid())
            {
                Addressables.Release(hAsset);
                _asset = null;
            }
        }

        [Serializable]
        public class LanguageFont
        {
            public SystemLanguage lang;
            public Font font;
        }

        [Serializable]
        public class FontItem
        {
            public Font pivotFont;
            public LanguageFont[] fonts;
        }

        [Serializable]
        public class LanguageTMFont
        {
            public SystemLanguage lang;
            public TMPro.TMP_FontAsset font;
        }

        [Serializable]
        public class TMFontItem
        {
            public TMPro.TMP_FontAsset pivotFont;
            public LanguageTMFont[] fonts;
        }

        [Serializable]
        public class FontSizeRatio
        {
            public SystemLanguage lang;
            public float ratio;
        }
        
        [Serializable]
        public class LineSpacingRatio
        {
            public SystemLanguage lang;
            public float ratio;
        }

        [SerializeField] public FontItem[] fonts;
        [SerializeField] public LanguageFont[] fallbackFonts;

        [SerializeField] public TMFontItem[] tmFonts;
        [SerializeField] public LanguageTMFont[] fallbackTMFonts;

        [SerializeField] public FontSizeRatio[] fontSizeRatio;
        [SerializeField] public LineSpacingRatio[] lineSpacingRatio;

        public Font GetFontByLanguage(Font font, SystemLanguage language)
        {
            FontItem findPivotFont = fonts.Find(item => item.pivotFont == font);
            if (findPivotFont != null)
            {
                LanguageFont languageFont = findPivotFont.fonts.Find(item => item.lang == language);
                if (languageFont != null) return languageFont.font;
            }

            LanguageFont fallbackFont = fallbackFonts.Find(item => item.lang == language);
            if (fallbackFont == null) return font;

            return fallbackFont.font;
        }
        
        public int GetFontSizeByLanguage(int originalSize, SystemLanguage lang)
        {
            var fontSize = fontSizeRatio.Find(arg => arg.lang == lang);
            if (fontSize == null)
            {
                fontSize = fontSizeRatio.Find(arg => arg.lang == LocalCode.DEFAULT_LANG);
            }
            return (int)((float)originalSize * fontSize.ratio);
        }

        public float GetLineSpacingByLanguage(float originalSpacing, SystemLanguage lang)
        {
            var lineSpacing = lineSpacingRatio.Find(arg => arg.lang == lang);
            if (lineSpacing == null)
            {
                lineSpacing = lineSpacingRatio.Find(arg => arg.lang == LocalCode.DEFAULT_LANG);
            }
            return (float)originalSpacing * lineSpacing.ratio;
        }

        public TMPro.TMP_FontAsset GetFontByLanguage(TMPro.TMP_FontAsset font, SystemLanguage lang)
        {
            TMFontItem findPivotTMFont = tmFonts.Find(item => item.pivotFont == font);
            if (findPivotTMFont != null)
            {
                LanguageTMFont languageTMFont = findPivotTMFont.fonts.Find(item => item.lang == lang);
                if (languageTMFont != null) return languageTMFont.font;
            }

            LanguageTMFont fallbackFont = fallbackTMFonts.Find(item => item.lang == lang);
            if (fallbackFont == null) return font;

            return fallbackFont.font;
        }
    }
}