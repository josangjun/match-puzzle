using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
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
            if (y < 0)
                g.Fall();
        }
    }

    [System.Serializable]
    public class SwapGem : ICommand
    {
        public Point lhs, rhs;

        public UniRx.Subject<UniRx.Unit> onComplete = new UniRx.Subject<UniRx.Unit>();

        public async void Run()
        {
            var g = (Gem)lhs;
            var q = (Gem)rhs;
            if (!g || !q || !g.CanMove() || !q.CanMove())
                return;
            {
                using var h1 = g.Lock();
                using var h2 = q.Lock();

                var s = g.MovePosition(rhs.x, rhs.y);
                var t = q.MovePosition(lhs.x, lhs.y);
                while (s.IsActive() || t.IsActive())
                    await Task.Yield();
            }
            onComplete.OnNext(UniRx.Unit.Default);
        }
    }

    [System.Serializable]
    public class Explosion : ICommand
    {
        public List<Point> points = new List<Point>();

        public UniRx.Subject<List<Point>> onComplete = new UniRx.Subject<List<Point>>();

        public int count;

        public void Run()
        {
            //SLogger.LogEditor("explode:"+points.Select(arg => (Gem)arg).Where(arg => arg).Select(arg => arg.ToString()).ToArray().ToJson());

            foreach (var p in points)
            {
                var g = (Gem)p;
                if (g == null)
                    continue;
                ++count;
                g.Explode();
                g.onExplosion.Take(1).
                    Subscribe(_ =>
                    {
                        if (0 == --count)
                            onComplete.OnNext(points);
                    });
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