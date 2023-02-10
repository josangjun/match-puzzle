using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    public enum SetType { None, Sequence3, Sequence4, Square2x2, Cross, Sequence5 }

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
            if (!Stage.config.powerUp)
                return;

            if (alive.Count > 0)
            {
                var c = gems.Count;
                foreach (var g in alive)
                    gems.Remove(g);
                if (c > gems.Count)
                    return;
            }

            if (type == SetType.Sequence3)
                return;

            if (type == SetType.Square2x2)
            {
                TransformGem(gems, GemType.Propeller, alive);
            }
            else if (type == SetType.Cross)
            {
                TransformGem(gems, GemType.TNT, alive);
            }
            else if (type == SetType.Sequence4)
            {
                var type = gems[0].x == gems[1].x ? GemType.RocketH : GemType.RocketV;
                TransformGem(gems, type, alive);
            }
            else if (type == SetType.Sequence5)
            {
                TransformGem(gems, GemType.LightBall, alive);
            }
        }

        private Gem TransformGem(List<Gem> gems, GemType newType, List<Gem> alive)
        {
            if (gems.Count == 0 || gems.Count < 4)
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
                alive.Add(ret);
            }
            return ret;
        }
    }
}