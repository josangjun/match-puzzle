using Studio.Game.Commands;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.Game
{
    [System.Serializable]
    public class Judge
    {
        public readonly List<Gem> buffer = new List<Gem>();
        public readonly List<Gem> result = new List<Gem>();

        public bool Match()
        {
            result.Clear();
            return Match(buffer);
        }

        public bool Match(Point p)
        {
            result.Clear();
            Match(p, Axis.Horizontal, buffer);
            Match(p, Axis.Vetical, buffer);
            return result.Count >= Stage.MATCH;
        }

        public bool MatchAndExplode()
        {
            if (Match())
            {
                if (result.Count >= Stage.MATCH)
                {
                    Stage.Instance.Explode(result);
                    return true;
                }
            }
            return false;
        }

        public bool Match(List<Gem> buffer)
        {
            result.Clear();
            buffer.Clear();
            for (var i = 0; i < Stage.COL; ++i)
            {
                var type = GemType.None;
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    Match_(i, j, ref type, buffer);
                }
                FlushBuffer(buffer);
            }
            for (var i = 0; i < Stage.ROW; ++i)
            {
                var type = GemType.None;
                for (var j = 0; j < Stage.COL; ++j)
                {
                    Match_(j, i, ref type, buffer);
                }
                FlushBuffer(buffer);
            }
            return result.Count > 0;
        }

        private void FlushBuffer(List<Gem> buffer)
        {
            if (buffer.Count >= Stage.MATCH)
                result.AddRange(buffer);
            buffer.Clear();
        }

        private void Match_(int x, int y, ref GemType type, List<Gem> buffer)
        {
            var g = Stage.Instance[x, y];
            if (g == null || g.IsLocked())
            {
                FlushBuffer(buffer);
                type = GemType.None;
                return;
            }
            if (g.type != type)
            {
                FlushBuffer(buffer);
                buffer.Add(g);
                type = g.type;
            }
            else if (g.type == type)
                buffer.Add(g);
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
            if (buffer.Count >= Stage.MATCH)
                result.AddRange(buffer);
            return buffer.Count >= Stage.MATCH;
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