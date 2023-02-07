using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Studio.Framework;

namespace Studio.Game
{
    public struct ExpInfo
    {
        public Point point;
        public GemType type;

        public static implicit operator ExpInfo(Gem gem)
        {
            return new ExpInfo
            {
                point = gem,
                type = gem.type
            };
        }
    }

    public class ExplosionFlow : ICommand
    {
        private Gem owner;

        private ExpInfo info;

        public void SetOwner(Gem owner)
        {
            this.owner = owner;
        }

        public void Join(ExpInfo info)
        {
            this.info = info;
        }

        MEC.CoroutineHandle handle;

        public bool IsRunning() => handle.IsRunning;

        public void Explode()
        {
            if (owner.type <= GemType.Max)
                return;

            Stage.Instance.commands.AddLast(this);

            switch (owner.type)
            {
                case GemType.RocketH:
                    {
                        handle = ExplodeRocketH().RunCoroutine();
                    }
                    break;

                case GemType.RocketV:
                    {
                        handle = ExplodeRocketV().RunCoroutine();
                    }
                    break;

                case GemType.Propeller:
                    {
                        handle = ExplodePropeller().RunCoroutine();
                    }
                    break;

                case GemType.TNT:
                    {
                        handle = ExplodeTNT().RunCoroutine();
                    }
                    break;

                case GemType.LightBall:
                    {
                        handle = ExplodeLightBall().RunCoroutine();
                    }
                    break;
            }
        }

        private IEnumerator<float> ExplodePropeller()
        {
            var offset = (Point)owner;
            
            for (var i = Direction.None + 1; i < Direction.Max; ++i)
            {
                var g = owner.GetNext((Direction)i);
                if (g) Stage.Instance.Explode(g);
            }

            yield break;
        }

        private IEnumerator<float> ExplodeLightBall()
        {
            var type = info.type == GemType.None ? (GemType)(Random.Range(0, Stage.config.gemTypeCount) + 1) : info.type;
            var list = ListPool<Gem>.Get();
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = Stage.Instance[i, j];
                    if (!g || g.type != type)
                        continue;
                    list.Add(g);
                }
            }
            list.Shuffle();
            for (var i = 0; i < list.Count; ++i)
            {
                var g = list[i];
                if (g)
                    Stage.Instance.Explode(g);
                yield return Timing.WaitForSeconds(0.025f);
            }
            ListPool<Gem>.Release(list);
        }

        private IEnumerator<float> ExplodeTNT()
        {
            var offset = (Point)owner;
            for (var i = -2; i <=2; ++i)
            {
                for (var j = -2; j <= 2; ++j)
                {
                    if (i == 0 && j == 0)
                        continue;

                    var dest = offset.Move(i, j);
                    var g = (Gem)dest;

                    if (g)
                        Stage.Instance.Explode(g);
                }
            }

            yield break;
        }

        private IEnumerator<float> ExplodeRocketV()
        {
            var offset = (Point)owner;
            for (var i = 0; i < Stage.ROW; ++i)
            {
                var up = Stage.Instance[offset.x, offset.y - i];
                var down = Stage.Instance[offset.x, offset.y + i];
                if (up == null && down == null)
                    break;
                Stage.Instance.Explode(up);
                Stage.Instance.Explode(down);
                yield return Timing.WaitForSeconds(0.025f);
            }
        }

        private IEnumerator<float> ExplodeRocketH()
        {
            var offset = (Point)owner;
            for (var i = 0; i < Stage.COL; ++i)
            {
                var left = Stage.Instance[offset.x - i, offset.y];
                var right = Stage.Instance[offset.x + i, offset.y];
                if (left == null && right == null)
                    break;
                Stage.Instance.Explode(left);
                Stage.Instance.Explode(right);
                yield return Timing.WaitForSeconds(0.025f);
            }
        }

        public void Run()
        {
            
        }
    }
}