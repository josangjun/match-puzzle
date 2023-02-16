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
                for (var i = Stage.ROW - 2; i >= 0; --i)
                {
                    var g = Stage.Instance[point.x, i];
                    if (g == null || g.IsFloor())
                        continue;
                    g.Fall();
                }

                {
                    var g = Stage.Instance.gemFactory.Produce(type);
                    g.SetPosition(point.x, - 1);
                    Stage.Instance.AddLane(point.x, g);
                }
            }
            ListPool<GemInfo>.Release(infoList);
        }
    }

    [System.Serializable]
    public class SwapGemRestore : SwapGem
    {
        protected override void OnComplete()
        {
            judge.MatchAndExplode();
            if (!lhs.IsFloor())
                lhs.Fall();
            if (!rhs.IsFloor())
                rhs.Fall();
        }
    }

    [System.Serializable]
    public class SwapGem : ICommand
    {
        public Gem lhs, rhs;

        public Judge judge => Stage.Instance.judge;

        public async void Run()
        {
            if (!lhs || !rhs || !lhs.CanMove() || !rhs.CanMove() || !lhs.IsAdjacency(rhs))
                return;

            if (!Explode(lhs, rhs))
            {
                var time = Stage.config.swapTime;
                var s = lhs.MovePosition(rhs.x, rhs.y, time);
                var t = rhs.MovePosition(lhs.x, lhs.y, time);
                while (s.IsActive() || t.IsActive())
                    await Task.Yield();
            }
            OnComplete();
        }

        protected virtual void OnComplete()
        {
            var ret = false;
            if (lhs.type > GemType.Max)
                ret = Stage.Instance.Explode(lhs);
            if (rhs.type > GemType.Max)
                ret = Stage.Instance.Explode(rhs) || ret;

            var judge = Stage.Instance.judge;
            if (!judge.MatchAndExplode() && !ret)
            {
                var cmd = new SwapGemRestore
                {
                    lhs = rhs,
                    rhs = lhs
                };
                Stage.Instance.commands.AddLast(cmd);
            }
        }

        public bool Explode(Gem lhs, Gem rhs)
        {
            if (lhs.type > GemType.Max && rhs.type > GemType.Max)
            {
                var max = lhs.type > rhs.type ? lhs : rhs;
                var min = lhs.type <= rhs.type ? lhs : rhs;
                max.Join(min);
                min.MovePosition(max).onComplete += () => {
                    Stage.Instance.Explode(max);
                };
                return true;
            }

            var judge = Stage.Instance.judge;
            var ret = false;
            if (lhs.type < GemType.Max)
                ret = judge.MatchAndExplode(lhs);

            if (rhs.type < GemType.Max)
                ret = judge.MatchAndExplode(rhs) || ret;

            return ret;
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