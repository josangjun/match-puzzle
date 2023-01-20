using UnityEngine;

namespace Studio.Framework
{
    public static class TransformExtension
    {
        public static Transform FindAllChild(this Transform transform, string name)
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.name == name)
                    return child;
            }
            return null;
        }

        public static void ForEachChildren(this Transform transform, System.Action<Transform> action)
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                child.ForEachChildren(action);
            }
            action.Invoke(transform);
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            var pt = transform.localPosition;
            pt.y = y;
            transform.localPosition = pt;
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            var pt = transform.localPosition;
            pt.x = x;
            transform.localPosition = pt;
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            var pt = transform.localPosition;
            pt.z = z;
            transform.localPosition = pt;
        }

        public static void SetLocalScaleX(this Transform transform, float x)
        {
            var s = transform.localScale;
            s.x = x;
            transform.localScale = s;
        }

        public static void SetLocalScaleY(this Transform transform, float y)
        {
            var s = transform.localScale;
            s.y = y;
            transform.localScale = s;
        }

        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            var s = transform.localScale;
            s.z = z;
            transform.localScale = s;
        }
    }
}