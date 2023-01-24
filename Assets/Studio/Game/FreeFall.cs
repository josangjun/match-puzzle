using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public class FreeFall
    {
        public const float Gravity = -9.81f * 10;
        public Vector2 start;
        
        public float elapsed = 0f;

        public Gem target;
        
        const int Bottom = Stage.ROW - 1;

        private Point EndPoint()
        {
            var pt = (Point)target;
            Gem bottom = null;
            for (; bottom == null; pt = pt.Move(Direction.Down))
            {
                bottom = pt;
                if (pt.y >= Bottom)
                {
                    pt.y = bottom.y;
                    break;
                }
            }
            return pt;
        }

        public bool Tick()
        {
            elapsed += Time.deltaTime;

            var p = target.rectTransform.anchoredPosition;

            p.y = Gravity * elapsed * elapsed + start.y;

            var endPoint = EndPoint();
            var endPos = (Vector2)endPoint;

            if (endPos.y >= p.y)
            {
                target.rectTransform.anchoredPosition = p;
                target.SetPosition(endPoint);
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