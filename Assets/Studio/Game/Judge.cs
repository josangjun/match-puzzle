using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Studio.Game
{
    [System.Serializable]
    public class Judge
    {
        public readonly List<Gem> buffer = new List<Gem>();
        public readonly List<GemSet> sets = new List<GemSet>();

        public ObjectPool<GemSet> setPool = new ObjectPool<GemSet>(
            () => new GemSet(),
            null,
            arg => {
                arg.gems.Clear();
                arg.type = SetType.None;
            });

        public void Dispose()
        {
            setbuffer.Clear();
            buffer.Clear();
            sets.Clear();
            setPool.Dispose();
        }

        public bool Match()
        {
            InitSets();
            Match_(buffer);
            SortSets();
            return sets.Count > 0;
        }

        private void InitSets()
        {
            foreach (var s in sets)
                setPool.Release(s);
            sets.Clear();
        }

        private readonly List<GemSet> setbuffer = new List<GemSet>();
        private void SortSets()
        {
            setbuffer.Clear();
            for (var i = 0; i < sets.Count; ++i)
            {
                var s = sets[i];
                if (!s.IsSequence())
                    continue;
                setbuffer.Add(s);
            }
            Comparison<GemSet> pred = (x, y) => ((int)y.type).CompareTo((int)x.type);
            setbuffer.Sort(pred);

            for (var i = 0; i < setbuffer.Count; ++i)
            {
                var lhs = setbuffer[i];
                for (var j = setbuffer.Count-1; j > i; --j)
                {
                    var rhs = setbuffer[j];
                    if (lhs.IsCross(rhs))
                    {
                        setbuffer.RemoveAt(j);
                        sets.Remove(rhs);
                        foreach (var g in rhs.gems)
                            if (!lhs.gems.Contains(g))
                                lhs.gems.Add(g);
                        lhs.type = SetType.Cross;
                    }
                }
            }
            sets.Sort(pred);
        }

        public void Explode(List<GemSet> sets)
        {
            if (sets.Count == 0)
                return;

            var alive = ListPool<Gem>.Get();

            foreach (var s in sets)
            {
                SLogger.Log($"{s.type}, {s.gems.Select(arg => arg.ToString()).ToJson()}");
                
                s.Transform(alive);

                Stage.Instance.Explode(s.gems);
            }

            ListPool<Gem>.Release(alive);
        }

        public bool Match(Point p)
        {
            InitSets();
            MatchSquare(p, p.Move(1, 1));
            MatchSquare(p, p.Move(1, -1));
            MatchSquare(p, p.Move(-1, 1));
            MatchSquare(p, p.Move(-1, -1));
            Match(p, Axis.Horizontal, buffer);
            Match(p, Axis.Vetical, buffer);
            SortSets();
            return sets.Count > 0;
        }

        public bool MatchAndExplode()
        {
            if (Match())
            {
                if (sets.Count > 0)
                {
                    Explode(sets);
                    return true;
                }
            }
            return false;
        }

        private bool Match_(List<Gem> buffer)
        {
            buffer.Clear();
            var c = sets.Count;
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
                    var p = new Point(i, j);
                    MatchSquare(p, p.Move(1, 1));
                }
                FlushBuffer(buffer);
            }
            return sets.Count > c;
        }

        private void FlushBuffer(List<Gem> buffer)
        {
            if (buffer.Count >= Stage.MATCH)
            {
                var set = setPool.Get();
                set.gems.AddRange(buffer);
                set.type = GetSequenceType(buffer.Count);
                sets.Add(set);
            }
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
            if (p == null || q == null || p.type != q.type)
                return false;
            var type = p.type;
            var xmin = Math.Min(p.x, q.x);
            var ymin = Math.Min(p.y, q.y);
            var xmax = Math.Max(p.x, q.x);
            var ymax = Math.Max(p.y, q.y);

            var set = setPool.Get();
            for (var x = xmin; x <= xmax; ++x)
            {
                for (var y = ymin; y <= ymax; ++y)
                {
                    var c = Stage.Instance[x, y];
                    if (c == null || c.type != type)
                    {
                        setPool.Release(set);
                        return false;
                    }
                    set.gems.Add(c);
                }
            }
            set.type = SetType.Square2x2;
            sets.Add(set);
            return true;
        }

        private bool Match(Point p, Axis axis, List<Gem> buffer)
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
            {
                var set = setPool.Get();
                set.gems.AddRange(buffer);
                set.type = GetSequenceType(buffer.Count);
                sets.Add(set);
            }
            return buffer.Count >= Stage.MATCH;
        }

        private SetType GetSequenceType(int count)
        {
            return count switch
            {
                3 => SetType.Sequence3,
                4 => SetType.Sequence4,
                5 => SetType.Sequence5,
                _ => SetType.Sequence4
            };
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
            InitSets();
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
            var countS = sets.Count;
            Match(dst);
            var countD = sets.Count;
            var count = countS + countD;

            g.SetPosition(src, false);
            h.SetPosition(dst, false);
            return count;
        }
    }
}