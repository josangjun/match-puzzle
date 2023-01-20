using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using NaughtyAttributes;
using System.Collections;
using Studio.Framework;
using TMPro;
using UniRx;

namespace Studio.Common
{
    [DisallowMultipleComponent]
    public class UILocalize : MonoBehaviour
    {
        [System.Serializable]
        private class TextureTag
        {
            public SystemLanguage tag;
            public Texture2D texture;
        }

        public enum UIType { Nothing, Text, TMP_Text, Image, RawImage }

        [System.Serializable]
        private class Target
        {
            [HideInInspector]
            public Graphic graphic;

            public UIType type;

            SpriteAtlas currentAtlas;

            AsyncOperationHandle hMaterial;

            private bool _HasImage() => type == UIType.Image;
            private bool _HasRawImage() => type == UIType.RawImage;
            private bool _HasText() => type == UIType.Text;

            [SerializeField, ShowIf(nameof(_HasImage))]
            private SoftLink<SpriteAtlas>[] atlases;

            [ArrayElementTitle("tag")]
            [SerializeField, ShowIf(nameof(_HasRawImage))]
            public TextureTag[] textures;

            public void Collect(GameObject go)
            {
                if (graphic == null)
                {
                    graphic = go.GetComponent<Graphic>();
                }

                VerifyType();
            }

            public void VerifyType()
            {
                if (graphic == null)
                {
                    type = UIType.Nothing;
                }
                else if (graphic is Image)
                {
                    type = UIType.Image;
                    VerifyAtlases(((Image)graphic).sprite);
                }
                else if (graphic is TMP_Text)
                {
                    type = UIType.TMP_Text;
                }
                else if (graphic is Text)
                {
                    type = UIType.Text;
                }
                else if (!System.Enum.TryParse(graphic.GetType().Name, out type))
                    type = UIType.Nothing;
            }

            [ShowIf(nameof(_HasImage))]
            public AssetReference atlasFolder;

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            void VerifyAtlases(Sprite sprite)
            {
#if UNITY_EDITOR
                if (sprite == null)
                    return;
                if (atlases != null && atlases.Length > 0 && atlases.All(arg => arg != null))
                    return;

                if (atlasFolder == null || string.IsNullOrEmpty(atlasFolder.AssetGUID))
                {
                    var atlasArray = UnityEditor.AssetDatabase.FindAssets("t:SpriteAtlas")
                        .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                        .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>).ToArray();

                    SpriteAtlas atlas = null;
                    foreach (var element in atlasArray)
                    {
                        if (element.CanBindTo(sprite))
                        {
                            atlas = element;
                            break;
                        }
                    }
                    if (atlas == null)
                        return;
                    var atlasPath = UnityEditor.AssetDatabase.GetAssetPath(atlas);
                    var atlasDir = System.IO.Path.GetDirectoryName(atlasPath);
                    var dirGuid = UnityEditor.AssetDatabase.AssetPathToGUID(atlasDir);
                    atlasFolder = new AssetReference(dirGuid);
                }

                string[] path = new string[1];
                path[0] = UnityEditor.AssetDatabase.GUIDToAssetPath(atlasFolder.AssetGUID);
                if (path == null || path.Length == 0 || string.IsNullOrEmpty(path[0]) == true) return;

                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpriteAtlas", path);
                atlases = new SoftLink<SpriteAtlas>[guids.Length];

                for (int i = 0; i < guids.Length; i++)
                {
                    string guid = guids[i];
                    atlases[i] = new SoftLink<SpriteAtlas>(guid);
                }
#endif
            }

            private void ApplyFont(Text text)
            {
                var fontConfig = FontConfig.GetAsset();
                if (fontConfig == null)
                    return;
                var lang = Localization.systemLanguage;

                if (text.font == null || text.font.dynamic == false)
                    return;

                Font newFont = fontConfig.GetFontByLanguage(text.font, lang);
                text.font = newFont;
            }

            private void ApplyFont(TMP_Text tmpText)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return;
#endif
                var fontConfig = FontConfig.GetAsset();
                if (fontConfig == null)
                    return;
                var lang = Localization.systemLanguage;
                
                var newFont = fontConfig.GetFontByLanguage(tmpText.font, lang);
                var oldFont = tmpText.font;

                if (oldFont.GetInstanceID() == newFont.GetInstanceID())
                    return;

                string matName = string.Empty;
                Material newMat = null;
                var mat = tmpText.fontSharedMaterial;
                if (mat.GetInstanceID() == oldFont.material.GetInstanceID())
                {
                    newMat = newFont.material;
                    matName = newMat.name;
                }
                else
                {
                    matName = mat.name.Replace(oldFont.name, newFont.name);
                    newMat = fontConfig.GetMaterial(matName);
                }

                if (newMat)
                {
                    tmpText.font = newFont;
                    tmpText.fontSharedMaterial = newMat;
                }
                else
                {
                    SLogger.LogEditor($"UILocalize.CopyMaterial:{tmpText.name}", tmpText);
                    var sharedMaterial = tmpText.fontSharedMaterial;
                    var keywords = sharedMaterial.shaderKeywords;
                    newMat = new Material(sharedMaterial);
                    tmpText.font = newFont;
                    newMat.name = matName;
                    newMat.SetTexture(TMPro.ShaderUtilities.ID_MainTex, newFont.atlasTexture);
                    tmpText.fontSharedMaterial = newMat;
                    fontConfig.SetMaterial(matName, newMat);
                }
            }

            public void ApplyText(string val)
            {
                if (type == UIType.Text)
                {
                    var text = graphic as Text;
                    ApplyFont(text);

                    if (!string.IsNullOrEmpty(val))
                        text.text = val;
                }
                else if (type == UIType.TMP_Text)
                {
                    var text = graphic as TMP_Text;
                    ApplyFont(text);

                    if (!string.IsNullOrEmpty(val))
                        text.text = val;
                }
            }

            public void ApplyImage(Image image)
            {
                string code = Localization.GetSimpleId();

                if (image.sprite == null)
                {
                    Debug.LogWarning("image sprite is null.");
                    return;
                }

                if (code == "zh-Hant") code = "tw";

                SoftLink<SpriteAtlas> link = default;
                foreach (var arg in atlases)
                {
                    if (!arg.name.EndsWith(code))
                        continue;
                    link = arg;
                    break;
                }

                if (link == null)
                {
                    Debug.LogWarning($"atlas does not exist. : {image.gameObject.GetHierarchy()} code : {code}");
                    return;
                }

                if (link.Asset)
                {
                    SetAtlas(image, link.Asset, code);
                    return;
                }

                var asyncOp = link.LoadAssetAsync();
                asyncOp.Completed += h =>
                {
                    var atlas = h.Result;
                    if (atlas == null)
                    {
                        Debug.LogFormat($"UILocalize::ApplyImage() atlas load fail. {link.path} is null");
                        return;
                    }
                    SetAtlas(image, atlas, code);
                };
            }

            public void ApplyTexture(RawImage rawImage)
            {
                var lang = Localization.systemLanguage;
                var textureTag = textures.FirstOrDefault(arg => arg.tag == lang);

                if (textureTag == null)
                {
                    Debug.LogWarning($"texture not exist. name :{rawImage.name}, {lang}");
                    return;
                }

                rawImage.texture = textureTag.texture;
            }

            void SetAtlas(Image image, SpriteAtlas atlas, string code)
            {
                if (currentAtlas == atlas || !image) return;

                Sprite newSprite = atlas.GetSprite(image.sprite.name);
                if (newSprite == null)
                {
                    if (atlas.CanBindTo(image.sprite))
                    {
                        var name = image.sprite.name.Replace("(Clone)", string.Empty);
                        newSprite = atlas.GetSprite(name);
                    }
                    if (newSprite == null)
                    {
                        Debug.Log($"image text not exist. lang : {code}, spritename : {image.sprite.name}, {image.gameObject.GetHierarchy()}");
                        return;
                    }
                }
                currentAtlas = atlas;
                
                if (CanvasUpdateRegistry.IsRebuildingGraphics())
                {
                    MainThreadDispatcher.Post(_ =>
                    {
                        image.sprite = newSprite;
                        image.SetNativeSize();
                    }, null);
                }
                else
                {
                    image.sprite = newSprite;
                    image.SetNativeSize();
                }
            }

            public void ReleaseAsset()
            {
                if (hReserve != null)
                    graphic.StopCoroutine(hReserve);
                hReserve = null;

                foreach (var atlas in atlases)
                    atlas.ReleaseAsset();

                if (hMaterial.IsValid())
                {
                    Addressables.Release(hMaterial);
                    hMaterial = default;
                }
            }

            public void Apply(string key)
            {
                if (type == UIType.Image)
                {
                    ApplyImage(graphic as Image);
                }
                else if (type == UIType.RawImage)
                {
                    ApplyTexture(graphic as RawImage);
                }
                else
                {
                    hReserve = null;

                    var val = Localization.Get(key);
                    if (!string.IsNullOrEmpty(key) && key == val)
                    {
                        ReserveApplying(key);
                        return;
                    }

                    ApplyText(val);
                }
            }

            private void ReserveApplying(string key)
            {
                if (hReserve != null)
                    return;

                graphic.StartCoroutine(hReserve = ReserveApplyingAsync());

                IEnumerator ReserveApplyingAsync()
                {
                    yield return null;
                    for (var e = 0f; e < 10f && Localization.data == null; e += Time.deltaTime)
                        yield return null;
                    Apply(key);
                }
            }

            private IEnumerator hReserve;
        }

        [NestedArray]
        [SerializeField]
        private Target _target = new Target();
        [SerializeField]
        [ShowIf("HasKey")]
        private string _key;

        public bool HasKey => _target.type == UIType.Text || _target.type == UIType.TMP_Text;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
            if (gameObject.scene.IsValid() == false)
                return;

            UnityEngine.Assertions.Assert.IsNotNull(_target.graphic);
#endif
            Apply();
        }

        [ContextMenu("Apply")]
        public void Apply()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                Localization.systemLanguage = SystemLanguage.English;
            }
#endif
            _target.Apply(_key);
        }

        [ContextMenu("Collect")]
        public void Collect()
        {
            _target.Collect(gameObject);
        }

        private void OnDisable()
        {
            _target.ReleaseAsset();
        }

        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(_key))
                _key = _key.Trim();
            _target.Collect(gameObject);
        }
    }

    public static class TMPTextEx
    {
        public static bool SetMaterial(this TMP_Text text, string name)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return false;
#endif
            var config = FontConfig.GetAsset();
            var material = config ? config.GetMaterial(name) : default;
            if (material)
                text.fontSharedMaterial = material;
            return true;
        }
    }
}


#if UNITY_EDITOR
namespace Studio.Common.EditorInternal
{
    using UnityEditor;

    [CustomEditor(typeof(UILocalize))]
    public class UILocalizeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = target as UILocalize;

            if (script)
            {
                script.Collect();
            }

            base.OnInspectorGUI();
        }
    }
}    
#endif
