using DG.Tweening;
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
        private const float Gravity = -9.81f * 10;
        public Vector2 start;
        
        public float elapsed = 0f;

        public Gem target;
        
        const int BottomY = Stage.ROW - 1;

        private Gem bottom;
        private Point endPoint;

        public Point EndPoint()
        {
            endPoint = (Point)target.rectTransform.anchoredPosition;
            var next = endPoint.Move(Direction.Down);
            bottom = (Gem)next;
            if (next.y <= BottomY && bottom == null)
                endPoint = next;
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
            target.rectTransform.anchoredPosition = p;
            return true;
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}