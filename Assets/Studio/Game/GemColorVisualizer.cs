using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    public class GemColorVisualizer : GemVisualizer
    {
        [System.Serializable]
        public class TypeInfo
        {
            public GemType type;
            public Color color;
        }

        public List<TypeInfo> typeInfos;

        public override void SetAttribute(Gem gem, GemType type)
        {
            gem.image.color = typeInfos[(int)type].color;
        }
    }
}