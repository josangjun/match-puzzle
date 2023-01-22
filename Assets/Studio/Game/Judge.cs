using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.Game
{
    [System.Serializable]
    public class Judge
    {
        public readonly List<Gem> horizontalList = new List<Gem>();
        public readonly List<Gem> verticalList = new List<Gem>();

        public bool Match(Point p)
        {
            Match(p, Axis.Horizontal, horizontalList);
            Match(p, Axis.Vetical, verticalList);
            return
                horizontalList.Count >= Stage.MATCH ||
                verticalList.Count >= Stage.MATCH;
        }

        public bool Match(Point p, Axis axis, List<Gem> buffer)
        {
            var g = (Gem)p;
            buffer.Clear();
            buffer.Add(g);
            if (axis == Axis.Horizontal)
            {
                Match_(p, buffer, Direction.Left);
                Match_(p, buffer, Direction.Right);
            }
            else if (axis == Axis.Vetical)
            {
                Match_(p, buffer, Direction.Up);
                Match_(p, buffer, Direction.Down);
            }
            return buffer.Count > Stage.MATCH;
        }

        private void Match_(Gem g, List<Gem> list, Direction dir)
        {
            var t = g.type;

            for (g = g.GetNext(dir); g != null; g = g.GetNext(dir))
            {
                if (g.type == t)
                    list.Add(g);
                else break;
            }
        }
    }
}