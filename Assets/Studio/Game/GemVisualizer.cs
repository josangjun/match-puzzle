using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    [CreateAssetMenu(fileName = "GemVisualizer", menuName = "Game/GemVisualizer", order = 0)]
    public class GemVisualizer : ScriptableObject, IGemVisualizer
    {
        [System.Serializable]
        public class TypeInfo
        {
            [HideInInspector]
            public string tag;
            public GemType type;
            public Sprite sprite;
            public Color color;

            public void OnValidate()
            {
                tag = type.ToString();
            }
        }

        public List<TypeInfo> typeInfos;

        public void SetType(Gem gem, GemType type)
        {
            var info = typeInfos.BinarySearchFirst(arg => arg.type.CompareTo(type));
            if (info != null)
            {
                gem.image.color = info.color;
                gem.image.sprite = info.sprite;
            }
        }

        [ContextMenu("Sort")]
        private void Sort()
        {
            typeInfos.Sort((lhs, rhs) => lhs.type.CompareTo(rhs.type));
        }

        public void OnValidate()
        {
            foreach (var info in typeInfos)
                info.OnValidate();
        }
    }
}