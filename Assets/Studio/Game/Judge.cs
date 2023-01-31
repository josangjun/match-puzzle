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
            return Match_(buffer);
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

        private bool Match_(List<Gem> buffer)
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

        public bool MatchSquare(Gem p, Gem q)
        {
            if (p.type != q.type)
                return false;
            var type = p.type;
            var xmin = Math.Min(p.x, q.x);
            var ymin = Math.Min(p.y, q.y);
            var xmax = Math.Max(p.x, q.x);
            var ymax = Math.Max(p.y, q.y);

            for (var x = xmin; x <= xmax; ++x)
            {
                for (var y = ymin; y <= ymax; ++y)
                {
                    var c = Stage.Instance[x, y];
                    if (c == null || c.type != type)
                        return false;
                }
            }
            return true;
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

        public bool HasMove()
        {
            for (var i = 0; i < Stage.COL-1; ++i)
            {
                for (var j = 0; j < Stage.ROW-1; ++j)
                {
                    var pt = new Point(i, j);

                    if (MatchSwap(pt, Direction.Right) > 0 ||
                        MatchSwap(pt, Direction.Down) > 0)
                        return true;
                }
            }

            return false;
        }

        public List<Move> CollectMove(List<Move> moves = null)
        {
            if (moves == null)
                moves = new List<Move>();
            for (var i = 0; i < Stage.COL - 1; ++i)
            {
                for (var j = 0; j < Stage.ROW - 1; ++j)
                {
                    var pt = new Point(i, j);

                    var c1 = MatchSwap(pt, Direction.Right);
                    if (c1 > 0)
                    {
                        moves.Add(new Move
                        {
                            point = pt,
                            dir = Direction.Right,
                            match = c1
                        });
                    }
                    var c2 = MatchSwap(pt, Direction.Down);
                    if (c2 > 0)
                    {
                        moves.Add(new Move
                        {
                            point = pt,
                            dir = Direction.Down,
                            match = c2
                        });
                    }
                }
            }

            moves.Sort((lhs, rhs) => rhs.match.CompareTo(lhs.match));
            return moves;
        }

        private int MatchSwap(Point src, Direction dir)
        {
            var dst = src.Move(dir);
            var g = (Gem)src;
            var h = (Gem)dst;

            if (g == null || h == null || g.IsLocked() || h.IsLocked() || g.type == h.type)
                return 0;

            g.SetPosition(dst, false);
            h.SetPosition(src, false);

            Match(src);
            var countS = result.Count;
            Match(dst);
            var countD = result.Count;
            var count = countS + countD;

            g.SetPosition(src, false);
            h.SetPosition(dst, false);
            return count;
        }
    }
}