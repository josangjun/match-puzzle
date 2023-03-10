using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Studio.Framework;
using DG.Tweening;
using UniRx;

namespace Studio.Game
{
    public struct GemInfo
    {
        public Point point;
        public GemType type;

        public static implicit operator GemInfo(Gem gem)
        {
            return new GemInfo
            {
                point = gem,
                type = gem.type
            };
        }

        public void SetTypeSilently(GemType type)
        {
            var g = (Gem)point;
            if (g)
            {
                g.SetTypeSilently(type);
            }
        }
    }

    public class ExplosionFlow : ICommand
    {
        private Gem owner;

        private GemInfo guest;

        public void SetOwner(Gem owner)
        {
            this.owner = owner;
            owner.onExplosion.Subscribe(_ =>
            {
                guest = default;
            });
        }

        public void Join(GemInfo guest)
        {
            this.guest = guest;
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

                case GemType.TNT:
                    {
                        handle = ExplodeTNT().RunCoroutine();
                    }
                    break;

                case GemType.Propeller:
                    {
                        handle = ExplodePropeller().RunCoroutine();
                    }
                    break;

                case GemType.LightBall:
                    {
                        handle = ExplodeLightBall().RunCoroutine();
                    }
                    break;

                default:
                    {
                        var g = (Gem)guest.point;
                        Stage.Instance.Explode(g);
                    }
                    break;
            }
        }

        private  Point GetAvailableGem()
        {
            var list = ListPool<Gem>.Get();
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = Stage.Instance[i, j];
                    if (g && !g.IsLocked())
                        list.Add(g);
                }
            }
            if (list.Count == 0)
            {
                return Point.Random();
            }
            var destGem = list[Random.Range(0, list.Count)];
            ListPool<Gem>.Release(list);
            return destGem;
        }

        private IEnumerator<float> ExplodePropeller()
        {

            MEC.CoroutineHandle h1, h2 = default;

            h1 = ExplodePropeller_().RunCoroutine();
            if (guest.type == GemType.Propeller)
            {
                guest.type = GemType.None;
                h2 = ExplodePropeller_().RunCoroutine();
            }
            else
            {
                guest.SetTypeSilently(GemType.None);
            }

            for (var i = Direction.None + 1; i < Direction.Max; ++i)
            {
                var g = owner.GetNext(i);
                if (g) Stage.Instance.Explode(g);
            }

            while (h1.IsRunning || h2.IsRunning)
                yield return 0f;
        }

        private IEnumerator<float> ExplodePropeller_()
        {
            var parent = Stage.Instance.transform;
            var image = Object.Instantiate(owner.image, parent, true);
            image.SetAlpha(1f);
            for (var i = 0; i < 5; ++i)
            {
                Point pt1;
                while (true)
                {
                    pt1 = GetAvailableGem();
                    var diff = ((Vector2)pt1 - (Vector2)(Point)owner).magnitude;
                    if (diff > Gem.Size.x * 4)
                        break;
                    yield return 0f;
                }
                var pt2 = Point.Random(pt1, owner);
                var midPoint = parent.TransformPoint(pt2);
                var destPoint = parent.TransformPoint(pt1);
                var dist = 
                    (midPoint - image.transform.position).magnitude +
                    (destPoint - midPoint).magnitude;
                SLogger.LogEditor($"propeller, {(Point)owner}-{pt2}-{pt1}, dist:{dist}");
                var tween = image.rectTransform.DOPath(new Vector3[]
                {
                    midPoint, destPoint
                }, dist * 0.1f, PathType.CatmullRom, PathMode.Sidescroller2D);
                tween.SetEase(Ease.InOutQuad);
                while (tween.IsActive())
                    yield return 0f;
                var g = (Gem)pt1;
                if (g && !g.IsLocked())
                {
                    if (guest.type != GemType.None)
                        g.SetTypeSilently(guest.type);
                    Stage.Instance.Explode(g);
                    break;
                }
            }
            Object.Destroy(image.gameObject);
        }

        private IEnumerator<float> ExplodeLightBall()
        {
            var targetType = (guest.type > GemType.Max || guest.type == GemType.None) ?
                (GemType)(Random.Range(0, Stage.config.gemTypeCount) + 1) : guest.type;
            var transformType = (guest.type < GemType.Max) ? targetType : guest.type;
            var type = targetType;
            var list = ListPool<Gem>.Get();
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = Stage.Instance[i, j];
                    if (!g || g.type != type)
                        continue;
                    list.Add(g);
                    g.type = transformType;
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
            if (guest.type == GemType.TNT)
            {
                var list = ListPool<Gem>.Get();
                for (var i = 0; i < Stage.COL; ++i)
                {
                    for (var j = 0; j < Stage.ROW; ++j)
                    {
                        var g = Stage.Instance[i, j];
                        if (!g || g.IsLocked())
                            continue;
                        g.SetTypeSilently(GemType.None);
                        list.Add(g);
                    }
                }
                Stage.Instance.Explode(list);
                ListPool<Gem>.Release(list);
                yield break;
            }

            var offset = (Point)owner;
            var dist = 2;
            for (var i = -dist; i <= dist; ++i)
            {
                for (var j = -dist; j <= dist; ++j)
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
            var lanes = 0;
            var offset = (Point)owner;
            if (guest.type == GemType.RocketH ||
                guest.type == GemType.RocketV)
                Stage.Instance.Explode(guest.point);
            else if (guest.type == GemType.TNT)
            {
                lanes = 1;
                guest.SetTypeSilently(GemType.None);
                Stage.Instance.Explode(guest.point);
            }
                
            for (var j = -lanes; j <= lanes; ++j)
            {
                if (j == 0) continue;
                var g = (Gem)offset.Move(j, 0);
                Stage.Instance.Explode(g);
            }
            for (var i = 0; i < Stage.ROW; ++i)
            {
                for (var j = -lanes; j <= lanes; ++j)
                {
                    var up = Stage.Instance[offset.x + j, offset.y - i];
                    var down = Stage.Instance[offset.x + j, offset.y + i];
                    if (up == null && down == null)
                        continue;
                    Stage.Instance.Explode(up);
                    Stage.Instance.Explode(down);
                }
                yield return Timing.WaitForSeconds(0.025f);
            }
        }

        private IEnumerator<float> ExplodeRocketH()
        {
            var lanes = 0;
            if (guest.type == GemType.RocketV ||
                guest.type == GemType.RocketH)
                Stage.Instance.Explode(guest.point);
            else if (guest.type == GemType.TNT)
            {
                lanes = 1;
                guest.SetTypeSilently(GemType.None);
                Stage.Instance.Explode(guest.point);
            }
            var offset = (Point)owner;
            for (var j = -lanes; j <= lanes; ++j)
            {
                if (j == 0) continue;
                var g = (Gem)offset.Move(0, j);
                Stage.Instance.Explode(g);
            }
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = -lanes; j <= lanes; ++j)
                {
                    var left = Stage.Instance[offset.x - i, offset.y + j];
                    var right = Stage.Instance[offset.x + i, offset.y + j];
                    if (left == null && right == null)
                        continue;
                    Stage.Instance.Explode(left);
                    Stage.Instance.Explode(right);
                }
                yield return Timing.WaitForSeconds(0.025f);
            }
        }

        public void Run()
        {
            
        }
    }
}