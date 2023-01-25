using DG.Tweening;
using Studio.Framework;
using System.Collections;
using UnityEngine;

namespace Studio.Game
{
    public interface ITickable
    {
        bool Tick();
    }

    [System.Serializable]
    public class FreeFall : ITickable
    {
        private const float Gravity = -9.81f * 100;
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
            _keepWaiting = true;
            return endPoint;
        }

        public bool _keepWaiting = false;
        public bool keepWaiting => _keepWaiting;

        public bool Tick()
        {
            elapsed += Time.deltaTime;

            var p = target.rectTransform.anchoredPosition;

            p.y = Gravity * elapsed * elapsed + start.y;

            var endPos = (Vector2)endPoint;
            
            if (endPos.y >= p.y)
            {
                target.rectTransform.anchoredPosition = endPos;
                target.SetPosition(endPoint);
                _keepWaiting = false;
                return false;
            }
            else if (bottom)
            {
                var limit = bottom.rectTransform.anchoredPosition.y + Gem.Size.y;
                if (limit > p.y)
                {
                    target.rectTransform.SetAnchordPositionY(limit);
                    elapsed -= Time.deltaTime;
                    _keepWaiting = true;
                    return true;
                }
            }
            target.rectTransform.anchoredPosition = p;
            return true;
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}