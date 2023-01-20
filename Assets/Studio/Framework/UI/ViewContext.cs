using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Studio.Framework
{
    [System.Serializable]
    public class ViewContext : SortedList<string, object>
    {
        public ViewContext() {}

        public static readonly ViewContext Default = new ViewContext();

        public ViewContext(string json)
        {
            FromJson(json);
        }

        public ViewContext(IDictionary<string, object> other) : base(other) {}

        public ViewContext(params object[] args)
        {
            for (var i = 0; i < args.Length - 1; i += 2)
            {
                UnityEngine.Assertions.Assert.IsTrue(args[i] is string);
                this[args[i].ToString()] = args[i+1];
            }
        }

        public void CopyFrom(ViewContext arg)
        {
            if (this == arg)
                return;

            base.Clear();
            for (var i = 0; i < arg.Count; ++i)
            {
                var key = arg.Keys[i];
                var val = arg.Values[i];
                this[key] = val;
            }
        }

        public T Get<T>(string key)
        {
            if (TryGetValue(key, out var prop))
            {
                try
                {
                    return (T)prop;
                }
                catch (System.InvalidCastException e)
                {
                    if (prop is System.IConvertible)
                        return (T)System.Convert.ChangeType(prop, typeof(T));
                    Debug.LogWarning($"{prop.GetType()} -> {typeof(T)}");
                    Debug.LogError(e.Message);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return default;
        }

        public T GetByCast<T>(string key)
        {
            if (TryGetValue(key, out var prop))
            {
                return (T)prop;
            }
            return default;
        }

        public ViewContext FromJson(string json)
        {
            base.Clear();
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var pair in dict)
                this[pair.Key] = pair.Value;
            return this;
        }
    }
}