using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    public enum SetType { None, Sequence3, Sequence4, Square2x2, Cross }

    [System.Serializable]
    public class GemSet
    {
        public SetType type;
        public List<Gem> gems = new List<Gem>(5);

        public bool IsSequence()
        {
            return type is SetType.Sequence3 or SetType.Sequence4;
        }

        public bool IsCross(GemSet other)
        {
            if (!IsSequence() || !other.IsSequence())
                return false;
            
            for (var i = 0; i < gems.Count; ++i)
            {
                for (var j = 0; j < other.gems.Count; ++j)
                {
                    var lhs = gems[i];
                    var rhs = other.gems[j];
                    if (lhs.x == rhs.x && lhs.y == rhs.y)
                        return true;
                }
            }
            return false;
        }

        public void Transform(List<Gem> alive)
        {
            if (alive.Count > 0)
            {
                foreach (var g in alive)
                    gems.Remove(g);
            }

            if (type == SetType.Sequence3)
                return;

            if (type == SetType.Square2x2)
            {
                var g = TransformGem(gems, GemType.TNT);
                alive.Add(g);
            }
            else if (type == SetType.Sequence4)
            {
                var type = gems[0].x == gems[1].x ? GemType.RocketH : GemType.RocketV;
                var g = TransformGem(gems, type);
                alive.Add(g);
            }
            else if (type == SetType.Cross)
            {
                var g = TransformGem(gems, GemType.LightBall);
                alive.Add(g);
            }
        }

        private Gem TransformGem(List<Gem> gems, GemType newType)
        {
            if (gems.Count == 0)
                return null;
            var cursor = 0;
            for (var i = 1; i < gems.Count; ++i)
            {
                var gem = gems[i];
                if (gem.touchTime > gems[cursor].touchTime)
                    cursor = i;
            }

            var ret = gems[cursor];
            if (ret != null)
            {
                ret.type = newType;
                gems.RemoveAt(cursor);
            }
            return ret;
        }
    }
}