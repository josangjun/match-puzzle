using UnityEngine;
using DG.Tweening;
namespace Studio.Framework
{
    public static class RectTransformExtension
    {
        public static void SetAnchordPositionX(this RectTransform rect, float x)
        {
            var pos = rect.anchoredPosition;
            pos.x = x;
            rect.anchoredPosition = pos;
        }

        public static void SetAnchordPositionY(this RectTransform rect, float y)
        {
            var pos = rect.anchoredPosition;
            pos.y = y;
            rect.anchoredPosition = pos;
        }

        public static void SetAnchordPositionZ(this RectTransform rect, float z)
        {
            var pos = rect.anchoredPosition3D;
            pos.z = z;
            rect.anchoredPosition3D = pos;
        }

        public static void SetSizeDeltaX(this RectTransform rect, float x)
        {
            var y = rect.sizeDelta.y;
            rect.sizeDelta = new Vector2(x, y);
        }

        public static void SetSizeDeltaY(this RectTransform rect, float y)
        {
            var x = rect.sizeDelta.x;
            rect.sizeDelta = new Vector2(x, y);
        }

        public static Tween DOSizeDeltaX(this RectTransform rect, float x, float duration, bool snapping = false)
        {
            var y = rect.sizeDelta.y;
            return rect.DOSizeDelta(new Vector2(x, y), duration, snapping);
        }

        public static Tween DOSizeDeltaY(this RectTransform rect, float y, float duration, bool snapping = false)
        {
            var x = rect.sizeDelta.x;
            return rect.DOSizeDelta(new Vector2(x, y), duration, snapping);
        }

        public static void Stretch(this RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.localScale = Vector3.one;
        }
    }
}