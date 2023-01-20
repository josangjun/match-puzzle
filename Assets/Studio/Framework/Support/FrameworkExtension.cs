using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Studio.Framework
{
    public static class FrameworkExtension
    {
        public static Transform FindDescendant(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var searchResult = child.FindDescendant(name);
                if (searchResult != null)
                    return searchResult;
            }

            return null;
        }

        // for debug.
        public static string GetPathName(this Transform trans)
        {
            string result = trans.name;
            
            if(trans.parent != null)
            {
                result = $"{trans.parent.GetPathName()}/{result}";
            }
            return result;
        }

        public static T GetOrNewComponent<T>(this Component component) where T : UnityEngine.Component
        {
            return component.gameObject.GetOrNewComponent<T>();
        }

        public static bool DestroyComponent<T>(this Component component, bool immediate = false) where T : UnityEngine.Component
        {
            if (component == null)
                return false;
            var t = component.GetComponent<T>();
            if (t == null)
                return false;
            if (immediate)
                Object.DestroyImmediate(t);
            else
                Object.Destroy(t);
            return true;
        }

        public static T GetOrNewComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            var component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();
            return component;
        }

        public static void RebuildLayout(this RectTransform rect)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rect);
        }

        public static string ToJson<T>(this IList<T> list)
        {
            var sb = new System.Text.StringBuilder();
            sb.Length = 0;
            sb.Append('[');
            if (list != null)
            {
                if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                {
                    foreach (var arg in list)
                        sb.Append('"').Append(arg).Append('"').Append(',');
                }
                else
                {
                    foreach (var arg in list)
                    {
                        var str = Newtonsoft.Json.JsonConvert.SerializeObject(arg);
                        sb.Append(str).Append(','); 
                    }
                }
                if (sb[sb.Length - 1] == ',')
                    sb.Remove(sb.Length - 1, 1);
            }
            sb.Append(']');
            var json = sb.ToString();
            sb.Length = 0;
            return json;
        }

        public static T BinOrSeqSearch<T>(this IList<T> list, System.Func<T, int> predicate)
        {
            var hit = list.BinarySearchFirst(predicate);
            if (hit == null && list != null)
            {
                foreach (var item in list)
                {
                    if (predicate.Invoke(item) == 0)
                        return item;
                }
            }
            return hit;
        }

        #if FRAMEWORK_BINSEARCH
        public static T BinarySearchFirst<T>(this IList<T> list, System.Func<T, int> predicate)
        {
            if (list == null)
                return default;
            int min = 0;
            int max = list.Count;
            while (min < max)
            {
                int mid = (max + min) / 2;
                T midItem = list[mid];
                int comp = predicate(midItem);
                if (comp < 0)
                {
                    min = mid + 1;
                }
                else if (comp > 0)
                {
                    max = mid - 1;
                }
                else
                {
                    return midItem;
                }
            }
            if (min == max && 
                min < list.Count &&
                predicate(list[min]) == 0)
            {
                return list[min];
            }
            return default;
        }
        #endif

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = 0; i < list.Count; ++i)
            {
                var j = Random.Range(0, list.Count);
                var t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
        }

        public static bool IsNullOrEmpty<T>(this IList<T> This)
        {
            if(This == null) 
                return true;

            return This.Count == 0;
        }

        public static GoEffect Play(this IEffectPlayer player, GoEffect asset)
        {
            var effect = Service.GoPool.Rent(asset);
            if (effect)
                effect.Play(player.gameObject);
            return effect;
        }

        public static CoroutineHandle Count(this Text text, long to, float time, string fmt)
        {
            long from;
            long.TryParse(text.text,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out from);
            return Timing.RunCoroutine(CountAsync(str => text.text = str, from, to, time, fmt).CancelWith(text.gameObject));
        }

        public static CoroutineHandle Count(this Text text, long from, long to, float time, string fmt, float delay)
        {
            text.text = from.ToString(fmt);
            return Timing.RunCoroutineSingleton(CountAsync((str) => text.text = str, from, to, time, fmt, delay).CancelWith(text.gameObject), text.gameObject, SingletonBehavior.Overwrite);
        }

        public static CoroutineHandle Count(this Text text, long from, long to, float time, string fmt)
        {
            text.text = from.ToString(fmt);
            return Timing.RunCoroutineSingleton(CountAsync((str) => text.text = str, from, to, time, fmt).CancelWith(text.gameObject), text.gameObject, SingletonBehavior.Overwrite);
        }

        public static CoroutineHandle Count(this TMPro.TMP_Text text, long from, long to, float time, string fmt)
        {
            text.text = from.ToString(fmt);
            return Timing.RunCoroutineSingleton(CountAsync((str) => text.text = str, from, to, time, fmt).CancelWith(text.gameObject), text.gameObject, SingletonBehavior.Overwrite);
        }

        public static CoroutineHandle Count(this TMPro.TMP_Text text, long to, float time, string fmt)
        {
            long from;
            long.TryParse(text.text,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out from);
            return Timing.RunCoroutine(CountAsync(str => text.text = str, from, to, time, fmt).CancelWith(text.gameObject));
        }

        public static IEnumerator<float> CountAsync(System.Action<string> onUpdate, long from, long to, float time, string fmt, float delay = 0f)
        {
            if (onUpdate == null)
                yield break;

            if(delay != 0f)
            {
                yield return Timing.WaitForSeconds(delay);
            }

            if (from != to)
            {
                time = Mathf.Max(time, 0.01f);
                long last = 0;
                for (var elapsed = 0f; elapsed < time; elapsed += Time.deltaTime)
                {
                    var t = (double)elapsed / time;
                    var v = MathL.Lerp(from, to, t);
                    // var v = (long)((to - from) * (double)t + from);
                    // var v = Mathf.FloorToInt(Mathf.Lerp(from, to, t));
                    if (v != last)
                    {
                        onUpdate(v.ToString(fmt));
                        last = v;
                    }
                    yield return Timing.WaitForOneFrame;
                }
            }
            onUpdate(to.ToString(fmt));
        }

        public static void SetLayer(this GameObject gameObject, string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            gameObject.SetLayer(layer);
        }

        public static void SetLayer(this GameObject gameObject, int layer)
        {
            var tm = gameObject.transform;
            for (var i = 0; i < tm.childCount; ++i)
            {
                var child = tm.GetChild(i);
                child.gameObject.SetLayer(layer);
            }
            gameObject.layer = layer;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void MoveToComponent(this GameObject source, GameObject dest, System.Type type)
        {
#if UNITY_EDITOR
            var t = source.GetComponent(type);
            if (!t) return;
            UnityEditorInternal.ComponentUtility.CopyComponent(t);
            var d = dest.GetComponent(type);
            d = dest.AddComponent(type);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(d);
            Object.DestroyImmediate(t);
#endif
        }

        public static float GetAlpha(this Graphic graphic)
        {
            return graphic.color.a;
        }

        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            var c = graphic.color;
            c.a = alpha;
            graphic.color = c;
        }

        public static bool ToggleHandler(this UnityEventBase evt, Object obj, string method, bool toggle)
        {
            var count = evt.GetPersistentEventCount();
            for (var i = 0; i < count; ++i)
            {
                var target = evt.GetPersistentTarget(i);
                if (target != obj)
                    continue;
                if (evt.GetPersistentMethodName(i) != method)
                    continue;
                evt.SetPersistentListenerState(i, toggle ? UnityEventCallState.RuntimeOnly : UnityEventCallState.Off);
                return true;
            }
            return false;
        }
    }

    public struct MathL
    {
        public static long Lerp(long start, long end, double t)
        {
            return (long)((end - start) * t) + start;
        }
    }
}