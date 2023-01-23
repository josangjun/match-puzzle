using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace Studio.Game
{
    [System.Serializable]
    public class FreeFall : CustomYieldInstruction
    {
        public const float Gravity = -9.81f * 5;
        public Vector2 start;
        public Vector2 dest;
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

        private bool _keepWaiting;

        public override bool keepWaiting => _keepWaiting;

        public void Tick()
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
                _keepWaiting = false;
                return;
            }
            target.rectTransform.anchoredPosition = p;
            _keepWaiting = true;
        }

        public override void Reset()
        {
            base.Reset();
            elapsed = 0f;
        }
    }
}