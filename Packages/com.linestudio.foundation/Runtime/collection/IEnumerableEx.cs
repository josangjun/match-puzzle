using System.Text;

namespace System.Collections.Generic
{
    public static partial class IEnumerableEx
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null)
            {
                return;
            }
            foreach (T t in list)
            {
    #pragma warning disable RECS0017 // Possible compare of value type with 'null'
                if (t != null)
    #pragma warning restore RECS0017 // Possible compare of value type with 'null'
                {
                    action(t);
                }
            }
        }

        public static List<TOutput> ToList<T, TOutput>(this IEnumerable<T> src, Converter<T, TOutput> converter)
        {
            if (src == null)
            {
                return null;
            }
            List<TOutput> dst = new List<TOutput>();
            foreach (T t in src)
            {
                if (t != null)
                {
                    dst.Add(converter(t));
                }
            }
            return dst;
        }

        public static string ToJson<T>(this IEnumerable<T> list)
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                var sb = new StringBuilder();
                sb.Length = 0;
                sb.Append('[');
                if (list != null)
                {
                    foreach (var arg in list)
                    {
                        sb.Append(arg).Append(',');
                    }
                    if (sb[sb.Length - 1] == ',')
                        sb.Remove(sb.Length - 1, 1);
                }
                sb.Append(']');
                var json = sb.ToString();
                sb.Length = 0;
                return json;
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }

        public static string Join(this IEnumerable list, string separator)
        {
            int count = 0;
            StringBuilder str = new StringBuilder(256);
            foreach (object o in list)
            {
                if (o != null)
                {
                    if (count != 0)
                    {
                        str.Append(separator);
                    }
                    string t = o.ToString();
                    if (!t.IsEmpty())
                    {
                        str.Append(t);
                        count++;
                    }
                }
            }
            return str.ToString();
        }
    }
}

