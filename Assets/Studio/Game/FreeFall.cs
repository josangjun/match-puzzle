using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public class FreeFall
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
            return endPoint;
            //while (endPoint.y < BottomY)
            //{
            //    next = endPoint.Move(Direction.Down);
            //    bottom = next;
            //    if (bottom)
            //        break;
            //    endPoint = next;
            //}
            //return endPoint;
        }

        public bool Tick()
        {
            elapsed += Time.deltaTime;

            var p = target.rectTransform.anchoredPosition;

            p.y = Gravity * elapsed * elapsed + start.y;

            EndPoint();
            var endPos = bottom ? bottom.rectTransform.anchoredPosition + new Vector2(0, Gem.Size.y*0.5f) : (Vector2)endPoint;
            //target.SetPosition((Point)p, false);

            if (endPos.y >= p.y)
            {
                target.rectTransform.anchoredPosition = endPos;
                target.SetPosition(endPoint);
                return !CanFinish(endPoint);
            }
            target.rectTransform.anchoredPosition = p;
            return true;
        }

        private bool CanFinish(Point pt)
        {
            for (var i = pt.y+1; i <= BottomY; ++i)
            {
                if (Stage.Instance[pt.x, i] == null)
                    return false;
            }
            return true;
        }

        public void Reset()
        {
            elapsed = 0f;
        }
    }
}