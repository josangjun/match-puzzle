using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Studio.Common;
using System.Threading.Tasks;

namespace Studio.Framework
{
    [System.Serializable]
    public class SpriteLabel 
    {
        public SpriteLabel(string name, Sprite sprite)
        {
            m_Name = name;
            m_Sprite = sprite;
            OnValidate();
        }

        [SerializeField]
        private string m_Name;
        public string name { get => m_Name; set => m_Name = value; }

        [SerializeField][HideInInspector]
        private int m_Hash;
        public int hash => m_Hash;
        [SerializeField]
        private Sprite m_Sprite;
        public Sprite sprite
        {
            get => m_Sprite;
            set => m_Sprite = value;
        }

        [System.NonSerialized]
        public bool instance;
        [System.NonSerialized]
        public SoftLink<Sprite> link;

        public static implicit operator Sprite(SpriteLabel spriteLabel)
        {
            return spriteLabel != null ? spriteLabel.sprite : null;
        }
        public void OnValidate()
        {
            if (string.IsNullOrEmpty(m_Name) && m_Sprite)
                m_Name = m_Sprite.name;
        }
    }

    [System.Serializable]
    public class SpriteCategory
    {
        [SerializeField]
        private string m_Name;
        public string name => m_Name;
        [SerializeField][HideInInspector]
        private int m_Hash;
        public int hash => m_Hash;

        [SerializeField]
        private string path;

        public SpriteCategory(string name, List<SpriteLabel> labels)
        {
            m_Name = name;
            m_CategoryList = labels.ToList();
            OnValidate();
        }

        [SerializeField]
        [Column(
            new[] { "Name", "Sprite" }, new[] { "m_Name", "m_Sprite" }, new[] { 0.4f, 0.6f }
        )]
        public List<SpriteLabel> m_CategoryList = new List<SpriteLabel>();

        public List<SpriteLabel> labels => m_CategoryList;
        
        public SpriteLabel GetLabel(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var result = m_CategoryList.BinOrSeqSearch(arg => arg.name.CompareTo(name));
            return result;
        }

        public Sprite GetSprite(string spriteName, string labelName = null, bool addCache = false)
        {
            var label = GetLabel(spriteName);
            if (string.IsNullOrEmpty(labelName))
                labelName = spriteName;

            if (label != null)
            {
                if (label.sprite)
                    return label.sprite;
            }
            if (!addCache)
                return default;
            
            if (label != null)
                m_CategoryList.Remove(label);

            if (string.IsNullOrEmpty(this.path))
                return default;
            label = m_CategoryList.FirstOrDefault(arg => arg.name == labelName);

            if (label == null)
            {
                label = new SpriteLabel(labelName, null);
                m_CategoryList.Add(label);
                m_CategoryList.Sort((x, y) => x.name.CompareTo(y.name));
                var path = string.Format(this.path, spriteName);
                label.link = new SoftLink<Sprite>(string.Empty, path);
                label.link.muteWarning = true;
                label.instance = true;
                var asyncOp = label.link.LoadAssetAsync();
                asyncOp.WaitForCompletion();
                label.sprite = label.link.Asset;
            }
            
            if (label.sprite == null)
            {
                m_CategoryList.Remove(label);
                return default;
            }
            return label.sprite;
        }

        public async Task<Sprite> GetSpriteAsync(string spriteName, string labelName = null)
        {
            if (string.IsNullOrEmpty(spriteName))
                return null;
            if (string.IsNullOrEmpty(labelName))
                labelName = spriteName;

            var label = GetLabel(labelName);
            if (label != null)
            {
                if (label.sprite)
                    return label.sprite;
            }
            else
            {
                label = new SpriteLabel(labelName, null);
                m_CategoryList.Add(label);
                m_CategoryList.Sort((x, y) => x.name.CompareTo(y.name));
                var path = string.Format(this.path, spriteName);
                label.link = new SoftLink<Sprite>(string.Empty, path);
                label.instance = true;
            }
    
            if (label.instance)
                label.sprite = await label.link.GetOrLoadAsync();

            if (label.sprite == null)
            {
                m_CategoryList.Remove(label);
                return default;
            }
            return label.sprite;
        }

        public Sprite GetSprite(int index)
        {
            return labels[index].sprite;
        }

        public string GetName(int index)
        {
            return labels[index].name;
        }

        public void SetLabels(List<SpriteLabel> labels)
        {
            m_CategoryList = labels.ToList();
        }

        public int Count => labels.Count;

        public void Sort()
        {
            labels.Sort((x, y) => x.name.CompareTo(y.name));
        }

        public void OnValidate()
        {
            foreach (var label in labels)
                label.OnValidate();
        }

        public void Clear()
        { 
            for (var i = 0; i < labels.Count; ++i)
            {
                if (labels[i].instance)
                    labels.RemoveAt(i--);
            }
        }
    }

    [CreateAssetMenu(fileName = "SpriteLib.asset", menuName = "Game/Asset/SpriteLib")]
    public class SpriteLib : AbstractSpriteLib, IEnumerable<SpriteCategory>
    {
        [SerializeField]
        protected SpriteCategory[] m_Labels;
        
        public override Sprite GetSprite(string name)
        {
            foreach (var label in m_Labels)
            {
                var sprite = label.GetSprite(name);
                if (sprite != null)
                    return sprite;
            }
            return null;
        }

        protected Sprite GetSpriteInternal(string category, string name, bool addCache = false)
        {
            var cat = m_Labels.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return null;
            return cat.GetSprite(name, null, addCache);
        }

        public bool noWarning;

        public override Sprite GetSprite(string category, string name)
        {
            var sprite = GetSpriteInternal(category, name, true);
            if (sprite == null && !noWarning)
                Debug.Log($"missing sprite:{this.name}:{category}[{name}]");
            return sprite;
        }

        public virtual async Task<Sprite> GetSpriteAsync(string name)
        {
            foreach (var cat in m_Labels)
            {
                var sprite = await cat.GetSpriteAsync(name);
                if (sprite != null)
                    return sprite;
            }
            return default;
        }

        public override async Task<Sprite> GetSpriteAsync(string category, string name)
        {
            var cat = m_Labels.BinOrSeqSearch(arg => arg.name.CompareTo(category));
            if (cat == null) return null;
            var sprite = await cat.GetSpriteAsync(name, null);
            if (sprite == null && !noWarning)
                Debug.Log($"missing sprite:{category}, {name}");
            return sprite;
        }

        public void SetCategory(SpriteCategory category)
        {
            var find = m_Labels.FirstOrDefault(arg => arg.name == category.name);
            if (find != null)
            {
                find.SetLabels(category.labels);
            }
            else
            {
                m_Labels = m_Labels.Concat(new[] { category }).ToArray();
            }
        }

        public SpriteCategory GetCategory(string category)
        {
            return m_Labels.FirstOrDefault(arg => arg.name == category);
        }

        public bool HasCategory(string category)
        {
            return m_Labels.FirstOrDefault(arg => arg.name == category) != null;
        }

        [ContextMenu("Sort")]
        public void Sort()
        {
            System.Array.Sort(m_Labels, (x, y) => x.name.CompareTo(y.name));
            foreach (var cat in m_Labels)
                cat.Sort();
        }

        public virtual void OnValidate()
        {
            foreach (var cat in m_Labels)
            {
                cat.OnValidate();
            }
        }

        public IEnumerator<SpriteCategory> GetEnumerator()
        {
            return ((IEnumerable<SpriteCategory>)m_Labels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Labels.GetEnumerator();
        }

        public void Clear()
        {
            foreach (var cat in m_Labels)
                cat.Clear();
        }
    }

    public class ColumnAttribute : PropertyAttribute {
        public string[] labels;
        public string[] props;
        public float[] sizes;
        public ColumnAttribute(string[] labels, string[] props, float[] sizes) {
            this.labels = labels;
            this.props = props;
            this.sizes = sizes;
        }
    }
}

#if UNITY_EDITOR
namespace UnityEditor
{
    using Studio.Framework;
    [CustomPropertyDrawer(typeof(ColumnAttribute))]
    public class ColumnDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            var col = (ColumnAttribute)attribute;
            var rect = position;
            //property.isExpanded = false;

            for (var i = 0; i < col.labels.Length; ++i)
            {
                var l = col.labels[i];
                var prop = col.props[i];
                var size = col.sizes[i];
                var p = property.FindPropertyRelative(prop);
                rect.xMax = rect.xMin + position.width * size;
                EditorGUI.PropertyField(rect, p, GUIContent.none, true);
                rect.xMin = rect.xMax;
            }
        }
    }
}
#endif
