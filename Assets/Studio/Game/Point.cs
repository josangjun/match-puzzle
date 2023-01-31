using System.Collections.Generic;
using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public struct Point : System.IEquatable<Point>
    {
        public int x, y;

        public Point(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public Point Move(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    return new Point { x = x - 1, y = y };
                case Direction.Right:
                    return new Point { x = x + 1, y = y };
                case Direction.Up:
                    return new Point { x = x, y = y - 1 };
                case Direction.Down:
                    return new Point { x = x, y = y + 1 };
                default:
                    return new Point(-1, -1);
            }
        }

        public Point Move(int x, int y)
        {
            return new Point(this.x + x, this.y + y);
        }

        public static implicit operator Gem(Point p)
        {
            return Stage.Instance[p.x, p.y];
        }

        public static implicit operator Vector2(Point p)
        {
            return Gem.GetAnchoredPosition(p.x, p.y);
        }

        public static implicit operator Point(Vector3 p)
        {
            return Gem.GetPoint((Vector2)p);
        }

        public static implicit operator Point(Vector2 p)
        {
            return Gem.GetPoint(p);
        }

        public override string ToString()
        {
            return $"Pt[{x},{y}]";
        }

        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public bool IsFloor()
        {
            var pt = this;
            for (var i = y + 1; i < Stage.ROW; ++i)
            {
                pt.y = i;
                var g = (Gem)pt;
                if (g == null)
                    return false;
            }
            return true;
        }
    }

    [System.Serializable]
    public class Move
    {
        public Point point;
        public Direction dir;
        public int match;
    }

    public enum SetType { None, Sequence3, Sequence4, Square2x2, Cross3x3 }

    [System.Serializable]
    public class PointSet
    {
        public SetType type;
        public List<Point> points = new List<Point>(5);
    }
}