using DG.Tweening;
using MEC;
using Studio.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.Pool;
using System.Threading.Tasks;

namespace Studio.Game
{
    [System.Serializable]
    public class FreeFall
    {
        private const float Gravity = -9.81f;
        public Vector2 start;
        
        public float elapsed = 0f;

        private Gem owner;

        public void SetOwner(Gem owner)
        {
            this.owner = owner;
        }
        
        const int LAST_ROW = Stage.ROW - 1;

        public Point endPoint;

        public Point FindEndPoint()
        {
            endPoint = (Point)owner.rectTransform.anchoredPosition;
            
            var next = endPoint.Move(Direction.Down);
            var bottom = (Gem)next;
            if (!bottom && next.y <= LAST_ROW)
                endPoint = next;
            
            owner.SetPosition(endPoint, false);
            
            return endPoint;
        }

        public bool keepWaiting { get; private set; }

        private System.Action onComplete;

        public void Start(System.Action action)
        {
            if (keepWaiting)
                return;

            Enter();
            onComplete = action;
        }

        private readonly static List<FreeFall> falls = new();

        public static void Update()
        {
            var indices = ListPool<int>.Get();
            for (var i = 0; i < falls.Count; ++i)
            {
                var f = falls[i];
                if (!f.Tick())
                {
                    f.Exit();
                    indices.Add(i);
                }
            }
            for (var i = indices.Count - 1; i >= 0; --i)
            {
                var index = indices[i];
                falls.RemoveAt(index);
            }
            ListPool<int>.Release(indices);
        }

        public void Stop()
        {
            if (Stage.Instance)
                Exit();
            onComplete = null;
        }

        private void Enter()
        {
            ++Stage.Instance.moveCount;
            start = owner.rectTransform.anchoredPosition;
            elapsed = 0f;
            FindEndPoint();
            falls.Add(this);
            keepWaiting = true;
        }

        private void Exit()
        {
            if (!keepWaiting)
                return;
            keepWaiting = false;
            
            owner.touchTime = Time.time;
            
            if (onComplete != null)
            {
                onComplete();
                onComplete = null;
            }

            if (owner.y < 0 && !paused)
            {
                SLogger.LogWarning($"invalid position:{owner}", owner);
                owner.Return();
            }
            --Stage.Instance.moveCount;
        }

        private static bool paused;

        public static void Pause(bool tf)
        {
            paused = tf;
            if (!paused)
            {
                for (var i = 0; i < Stage.COL; ++i)
                {
                    for (var j = 0; j < Stage.ROW; ++j)
                    {
                        var g = Stage.Instance[i, j];
                        if (g && !g.IsFloor())
                            g.Fall();
                    }
                }
            }
        }

        /*
        private IEnumerator<float> WatchVacancy()
        {
            do
                yield return 0f;
            while (Stage.moveCount > 0);
            
            while (owner.isActiveAndEnabled)
            {
                for (var i = 0; i < Stage.COL; ++i)
                {
                    if (Stage.Instance[i, 0] == null)
                    {
                        owner.SetPosition(i, -1);
                        if (owner.Fall())
                        {
                            SLogger.Log($"Vacancy fixed:{owner}");
                            yield break;
                        }
                    }
                }
                if (Stage.moveCount == 0 && owner.IsFloor())
                    break;
                yield return 0f;
            }
            owner.Return();
        }
        */
        public bool Tick()
        {
            if (!owner || !owner.isActiveAndEnabled || !keepWaiting)
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

                if (paused || IsFloor())
                {
                    elapsed = 0f;
                    owner.SetPosition(endPoint);
                    return false;
                }
                else
                {
                    elapsed -= Time.deltaTime;
                    FindEndPoint();
                    return true;
                }
            }
            else
            {
                var bottom = owner.GetNext(Direction.Down);
                if (bottom)
                {
                    var limit = bottom.rectTransform.anchoredPosition.y + Gem.Size.y;
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

        private bool IsFloor()
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