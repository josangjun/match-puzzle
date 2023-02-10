using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

namespace Studio.Game.Commands
{
    [System.Serializable]
    public class ProduceGem : ICommand
    {
        public List<GemInfo> infoList;

        public ProduceGem()
        {
            infoList = ListPool<GemInfo>.Get();
        }
        
        public void Run()
        {
            for (var index = 0; index < infoList.Count; ++index)
            {
                var info = infoList[index];
                var point = info.point;
                var type = info.type;
                for (var i = point.y - 1; i >= 0; --i)
                {
                    var g = Stage.Instance[point.x, i];
                    if (g == null)
                        continue;
                    g.Fall();
                }

                {
                    var g = Stage.Instance.gemFactory.Produce(type);
                    g.SetPosition(point);
                    if (point.y < 0)
                        g.Fall();
                }
            }
            ListPool<GemInfo>.Release(infoList);
        }
    }

    [System.Serializable]
    public class SwapGem : ICommand
    {
        public Point lhs, rhs;

        public Subject<Unit> onComplete = new Subject<Unit>();

        public async void Run()
        {
            var g = (Gem)lhs;
            var q = (Gem)rhs;
            if (!g || !q || !g.CanMove() || !q.CanMove() || !g.IsAdjacency(q))
                return;

            {
                using var h1 = g.Lock();
                using var h2 = q.Lock();
                var time = Stage.config.swapTime;
                var s = g.MovePosition(rhs.x, rhs.y, time);
                var t = q.MovePosition(lhs.x, lhs.y, time);
                while (s.IsActive() || t.IsActive())
                    await Task.Yield();
            }
            onComplete.OnNext(Unit.Default);
        }
    }

    [System.Serializable]
    public class Explosion : ICommand
    {
        public List<Point> points = new List<Point>();

        public Subject<List<Point>> onComplete = new Subject<List<Point>>();

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