using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Game.Commands
{
    [System.Serializable]
    public class ProduceGem : ICommand
    {
        public GemType type;
        public int x, y;
        
        public void Run()
        {
            var g = Stage.Instance.gemFactory.Produce(type);
            g.SetPosition(x, y);
        }
    }

    [System.Serializable]
    public class SwapGem : ICommand
    {
        public Point lhs, rhs;

        private Judge judge => Stage.Instance.judge;

        public async void Run()
        {
            var gx = (Gem)lhs;
            var gy = (Gem)rhs;
            if (gx.IsLocked() || gy.IsLocked())
                return;

            using var h1 = gx.Lock();
            using var h2 = gy.Lock();

            var s = gx.MovePosition(rhs.x, rhs.y);
            var t = gy.MovePosition(lhs.x, lhs.y);
            while (s.IsActive() || t.IsActive())
                await Task.Yield();
            
            var matched = Match(gx) || Match(gy);
            
            if (!matched)
            {
                var u = gx.MovePosition(lhs.x, lhs.y);
                var v = gy.MovePosition(rhs.x, rhs.y);
                while (u.IsActive() || v.IsActive())
                    await Task.Yield();
            }
        }

        public bool Match(Point p)
        {
            if (!judge.Match(p))
                return false;

            var explosion = new Explosion();

            if (judge.horizontalList.Count >= Stage.MATCH)
            {
                foreach (var g in judge.horizontalList)
                    explosion.points.Add(g);
            }
            if (judge.verticalList.Count >= Stage.MATCH)
            {
                foreach (var g in judge.verticalList)
                    explosion.points.Add(g);
            }
            Stage.Instance.commands.AddFirst(explosion);
            return true;
        }
    }

    [System.Serializable]
    public class Match : ICommand
    {
        public Point p;
        private Judge judge => Stage.Instance.judge;

        public void Run()
        {
            if (!judge.Match(p))
                return;

            Explosion explosion = new Explosion();

            if (judge.horizontalList.Count >= Stage.MATCH)
            {
                foreach (var g in judge.horizontalList)
                    explosion.points.Add(g);
            }
            if (judge.verticalList.Count >= Stage.MATCH)
            {
                foreach (var g in judge.verticalList)
                    explosion.points.Add(g);
            }
        }
    }

    [System.Serializable]
    public class Explosion : ICommand
    {
        public List<Point> points = new List<Point>();

        public void Run()
        {
            foreach (var p in points)
            {
                var g = (Gem)p;
                g.Explode();
            }
        }
    }

    [System.Serializable]
    public class PrepareBoard : ICommand
    {
        public void Run()
        {

        }
    }
}