using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public struct Point
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

        public static implicit operator Gem(Point p)
        {
            return Stage.Instance[p.x, p.y];
        }
    }
}