using DG.Tweening;
using MEC;
using Studio.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public class FreeFall
    {
        private static int count
        {
            get => Stage.moveCount;
            set => Stage.moveCount = value;
        }

        private const float Gravity = -9.81f;
        public Vector2 start;
        
        public float elapsed = 0f;

        private Gem owner;

        public void SetOwner(Gem owner)
        {
            this.owner = owner;
        }
        
        const int BottomY = Stage.ROW - 1;

        private Gem bottom;
        private Point endPoint;

        public Point EndPoint()
        {
            endPoint = (Point)owner.rectTransform.anchoredPosition;
            bottom = null;
            
            while (true)
            {
                var next = endPoint.Move(Direction.Down);
                bottom = (Gem)next;
                if (bottom || next.y > BottomY)
                    break;
                endPoint = next;
            }

            owner.SetPosition(endPoint, false);
            
            return endPoint;
        }

        public bool keepWaiting => handle.IsRunning;

        private MEC.CoroutineHandle handle;

        public void Start(System.Action action)
        {
            if (handle.IsRunning)
                return;
            ++count;
            handle = Tick_().RunCoroutine();
            handle.OnDestroy(() =>
            {
                --count;
                if (action != null)
                    action();
            });
        }

        public void Stop()
        {
            handle.IsRunning = false;
        }

        private IEnumerator<float> Tick_()
        {
            start = owner.rectTransform.anchoredPosition;
            elapsed = 0f;
            EndPoint();

            while (Tick())
                yield return 0f;
            owner.touchTime = Time.time;

            if (owner.y < 0)
            {
                SLogger.LogWarning($"invalid position:{owner}", owner);
                WatchVacancy().RunCoroutine();
            }
        }

        private IEnumerator<float> WatchVacancy()
        {
            while (count > 0 && owner.isActiveAndEnabled)
            {
                for (var i = 0; i < Stage.COL; ++i)
                {
                    if (Stage.Instance[i, 0] == null)
                    {
                        owner.SetPosition(i, -1);
                        if (owner.Fall())
                            yield break;
                    }
                }
                if (count == 0 && owner.IsFloor())
                    break;
                yield return 0f;
            }
            owner.Return();
        }

        public bool Tick()
        {
            if (!owner || !owner.isActiveAndEnabled)
            {
                return false;
            }
            
            elapsed += Time.deltaTime;

            var p = owner.rectTransform.anchoredPosition;
            var dy = Stage.config.gravityScale * Gravity * elapsed * elapsed + start.y;
            p.y = Mathf.Max(dy, p.y - Stage.config.terminalVelocity * Time.deltaTime);

            var endPos = (Vector2)endPoint;

            if (endPos.y >= p.y)
            {
                owner.rectTransform.anchoredPosition = endPos;
                
                if (IsFloor())
                {
                    elapsed = 0f;
                    owner.SetPosition(endPoint);
                    return false;
                }
                else
                {
                    elapsed -= Time.deltaTime;
                    EndPoint();
                    return true;
                }
            }
            else
            {
                bottom = owner.GetNext(Direction.Down);
                if (bottom)
                {
                    var limit = bottom.rectTransform.anchoredPosition.y + Gem.Size.y * 0.7f;
                    if (limit > p.y)
                    {
                        owner.rectTransform.SetAnchordPositionY(limit);
                        elapsed -= Time.deltaTime;
                        return true;
                    }
                }
            }
            owner.rectTransform.anchoredPosition = p;
            return true;
        }

        public bool IsFloor()
        {
            var pt = (Point)owner;
            var ret = true;
            for (var i = Stage.ROW-1; i > owner.y; --i)
            {
                pt.y = i;
                var g = (Gem)pt;
                if (g == null)
                {
                    for (var j = i - 1; j > owner.y; --j)
                    {
                        var q = Stage.Instance[pt.x, j];
                        if (q && !q.IsFalling())
                            q.Fall();
                    }
                    return false;
                }
                else if (g.IsFalling())
                    ret = false;
            }
            return ret;
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}