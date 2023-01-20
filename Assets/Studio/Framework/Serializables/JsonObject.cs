using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class JsonArray<T> : IEnumerable<T>
{
    public JsonArray(T[] array)
    {
        this.array = array;
    }
    
    public void Copy(T[] array)
    {
        this.array = new T[array.Length];
        System.Array.Copy(array, this.array, array.Length);
    }

    public T[] array;
    public T this[int index] { get => array[index]; set => array[index] = value; }
    public static implicit operator T[] (JsonArray<T> jsonArray)
    {
        return jsonArray.array;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return array.GetEnumerator();
    }
}

[System.Serializable]
public class JsonDictionary<K, V>
{
    public K[] keys;
    public V[] values;
    
    public JsonDictionary(IDictionary<K, V> dict)
    {
        keys = dict.Keys.ToArray();
        values = dict.Values.ToArray();
    }

    public Dictionary<K, V> ToDict()
    {
        var dict = new Dictionary<K, V>();
        for (var i = 0; i < keys.Length; ++i)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }

    public static implicit operator Dictionary<K, V> (JsonDictionary<K, V> obj)
    {
        return obj.ToDict();
    }
}
