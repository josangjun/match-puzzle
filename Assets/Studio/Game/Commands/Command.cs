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
        public static readonly ProduceGem Default = new ProduceGem();

        public void Run()
        {
            var infoList = ListPool<GemInfo>.Get();
            var gemFactory = Stage.Instance.gemFactory;
            for (var i = 0; i < Stage.COL; ++i)
            {
                int vacancy = -Stage.Lanes[i].Count;
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    if (Stage.Instance[i, j])
                        continue;
                    ++vacancy;
                }
                int m = 1;
                for (var j = 0; j < vacancy; ++j)
                {
                    infoList.Add(new GemInfo
                    {
                        point = new Point(i, -1 * m++),
                        type = gemFactory.NewRandomGemType()
                    });
                }
            }

            for (var index = 0; index < infoList.Count; ++index)
            {
                var info = infoList[index];
                var point = info.point;
                var type = info.type;
                //for (var i = Stage.ROW - 2; i >= 0; --i)
                //{
                //    var g = Stage.Instance[point.x, i];
                //    if (g == null || g.IsFloor() || g.IsFalling())
                //        continue;
                //    g.Fall();
                //}

                {
                    var g = gemFactory.Produce(type);
                    g.SetPosition(point.x, -1);
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

        public Judge judge => Stage.judge;

        public async void Run()
        {
            if (!lhs || !rhs || !lhs.CanMove() || !rhs.CanMove() || !lhs.IsAdjacency(rhs))
                return;

            if (!Explode(lhs, rhs))
            {
                var time = Stage.config.swapTime;
                var s = lhs.MovePosition(rhs.x, rhs.y, null, time);
                var t = rhs.MovePosition(lhs.x, lhs.y, null, time);
                while (s.IsActive() || t.IsActive())
                    await Task.Yield();
                OnComplete();
            }
        }

        private bool ExplodeSpecial(Gem host, Gem guest)
        {
            host.Join(guest);
            if (host.type < GemType.Max)
                return false;
            
            Stage.Instance.Explode(host);
            return true;
        }

        protected virtual void OnComplete()
        {
            var ret = ExplodeSpecial(lhs, rhs) || ExplodeSpecial(rhs, lhs);

            var judge = Stage.judge;
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
                
                lhs.MovePosition(rhs, () => {
                    if (lhs.Equals(max))
                    {
                        var tmp = (Point)lhs;
                        lhs.SetPosition(rhs, false);
                        rhs.SetPosition(tmp, false);
                    }
                    max.Join(min);
                    Stage.Instance.Explode(max);
                }, Stage.config.swapTime);
                return true;
            }

            var judge = Stage.judge;
            var ret = false;
            if (lhs.type < GemType.Max)
                ret = judge.MatchAndExplode(lhs);

            if (rhs.type < GemType.Max)
                ret = judge.MatchAndExplode(rhs) || ret;

            return ret;
        }
    }
}