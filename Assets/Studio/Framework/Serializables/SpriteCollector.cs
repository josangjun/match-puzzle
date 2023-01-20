using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace Studio.Framework
{
    //[CreateAssetMenu(fileName = "SpriteCollector", menuName = "Game/Sprite/Collector")]
    public class SpriteCollector : ScriptableObject
    {
        public enum TagCase { Preserve, Lower, Upper };

        public SpriteLib target;

        [System.Serializable]
        public class CategoryDefinition
        {
            public string name;
            public string[] inputDir;
            public string tagPattern;
            public TagCase tagCase;
            public string pattern;
            public string suffix;

            public List<SpriteLabel> Load()
            {
                var list = new List<SpriteLabel>();
#if UNITY_EDITOR
                var pathArray = UnityEditor.AssetDatabase.FindAssets($"t:Sprite", inputDir)
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .ToArray();

                var regex = new Regex(pattern);
                var tagRegex = new Regex(tagPattern);

                foreach (var path in pathArray)
                {
                    var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    var match = regex.Match(sprite.name);
                    if (match.Success == false)
                        continue;
                    var tagMatch = tagRegex.Match(sprite.name);
                    var tag = tagMatch.Groups["tag"].Value;
                    if (tagCase == TagCase.Lower)
                    {
                        tag = tag.ToLower();
                    }
                    else if (tagCase == TagCase.Upper)
                        tag = tag.ToUpper();
                    if (!string.IsNullOrEmpty(suffix) && sprite.name.EndsWith(suffix))
                        tag += suffix;
                    list.Add(new SpriteLabel(tag, sprite));
                }
#endif
                return list;
            }
        }

        [SerializeField]
        private CategoryDefinition[] categoryDefinitions;

        [SerializeField]
        private bool overwrite = true;

        [ContextMenu("Load")]
        public void Load()
        {
#if UNITY_EDITOR
            var catList = target.ToList();
            if (overwrite)
                catList.Clear();

            foreach (var cat in categoryDefinitions)
            {
                var list = cat.Load();
                var find = catList.FirstOrDefault(arg => arg.name == cat.name);
                if (find == null)
                {
                    find = new SpriteCategory(cat.name, list);
                    catList.Add(find);
                    target.SetCategory(find);
                }
                else
                {
                    var dict = find.labels.ToDictionary(arg => arg.name, arg => arg.sprite);
                    foreach (var arg in list)
                        dict[arg.name] = arg.sprite;
                    find.m_CategoryList = dict.Select(arg => new SpriteLabel(arg.Key, arg.Value)).ToList();
                }
            }
            target.OnValidate();
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }
    }
}

#if UNITY_EDITOR
namespace Studio.Framework
{
    using UnityEditor;

    [CustomEditor(typeof(SpriteCollector))]
    public class SpriteCollectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Load"))
            {
                var collector = target as SpriteCollector;
                collector.Load();
            }

            base.OnInspectorGUI();
        }
    }
}
#endif