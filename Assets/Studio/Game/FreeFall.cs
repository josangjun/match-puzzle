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
        private const float Gravity = -9.81f;
        public Vector2 start;
        
        public float elapsed = 0f;

        public Gem target;
        
        const int BottomY = Stage.ROW - 1;

        private Gem bottom;
        private Point endPoint;

        public Point EndPoint()
        {
            endPoint = (Point)target.rectTransform.anchoredPosition;
            bottom = null;
            
            while (true)
            {
                var next = endPoint.Move(Direction.Down);
                bottom = (Gem)next;
                if (bottom || next.y > BottomY)
                    break;
                endPoint = next;
            }

            target.SetPosition(endPoint, false);
            
            return endPoint;
        }

        public bool keepWaiting => handle.IsRunning;

        private MEC.CoroutineHandle handle;

        public void Start(System.Action action)
        {
            handle = Tick_().RunCoroutineSingleton(handle, SingletonBehavior.Abort);
            if (action != null)
                handle.OnDestroy(action);
        }

        public void Stop()
        {
            elapsed = 0f;
            handle.IsRunning = false;
        }

        private IEnumerator<float> Tick_()
        {
            start = target.rectTransform.anchoredPosition;
            elapsed = 0f;
            EndPoint();

            while (Tick())
                yield return 0f;
            elapsed = 0f;
            target.touchTime = Time.time;
            if (target.y < 0)
            {
                SLogger.LogWarning($"invalid position:{target}");
                target.Return();
            }
        }

        public bool Tick()
        {
            if (!target || !target.isActiveAndEnabled)
            {
                return false;
            }
            
            elapsed += Time.deltaTime;

            var p = target.rectTransform.anchoredPosition;
            var dy = Stage.config.speedScale * Gravity * elapsed * elapsed + start.y;
            p.y = Mathf.Max(dy, p.y - Stage.config.speedLimit * Time.deltaTime);

            var endPos = (Vector2)endPoint;

            if (endPos.y >= p.y)
            {
                target.rectTransform.anchoredPosition = endPos;
                
                if (IsFloor())
                {
                    elapsed = 0f;
                    target.SetPosition(endPoint);
                    return false;
                }
                else
                {
                    EndPoint();
                    return true;
                }
            }
            else
            {
                bottom = target.GetNext(Direction.Down);
                if (bottom)
                {
                    var limit = bottom.rectTransform.anchoredPosition.y + Gem.Size.y;
                    if (limit > p.y)
                    {
                        target.rectTransform.SetAnchordPositionY(limit);
                        elapsed -= Time.deltaTime;
                        return true;
                    }
                }
            }
            target.rectTransform.anchoredPosition = p;
            return true;
        }

        public bool IsFloor()
        {
            var pt = (Point)target;
            var ret = true;
            for (var i = Stage.ROW-1; i > target.y; --i)
            {
                pt.y = i;
                var g = (Gem)pt;
                if (g == null)
                {
                    for (var j = i - 1; j > target.y; --j)
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