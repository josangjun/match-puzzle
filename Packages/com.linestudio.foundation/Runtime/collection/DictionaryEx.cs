using System.Linq;

namespace System.Collections.Generic
{
    public static partial class DictionaryEx
    {
        public static V Find<K, V>(this IDictionary<K, V> dict, K key)
        {
            if (dict != null && key != null)
            {
                if (dict.TryGetValue(key, out var val))
                {
                    return val;
                }
            }
            return default(V);
        }

        public static V Find<K, V>(this IDictionary<K, V> dict, K key, V defaultVal)
        {
            V val = defaultVal;
            if (dict != null&&key != null && dict.TryGetValue(key, out val))
            {
                return val;
            }
            return defaultVal;
        }

        public static Dictionary<V, K> Reverse<K, V>(this IDictionary<K,V> dict)
        {
            return dict.ToDictionary(pair => pair.Value, pair => pair.Key);
        }
    }
}

