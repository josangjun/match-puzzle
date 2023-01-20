using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Studio.Framework
{
    [CreateAssetMenu(fileName = "ContentConfig", menuName = "Game/ContentConfig")]
    public class ContentConfig : ScriptableObject, IEnumerable<ScriptableObject>
    {
        public List<ScriptableObject> list;

        public IEnumerator<ScriptableObject> GetEnumerator()
        {
            return ((IEnumerable<ScriptableObject>)list).GetEnumerator();
        }


#if UNITY_EDITOR
        readonly IDictionary<int, ScriptableObject> dict = new SortedList<int, ScriptableObject>();
        public void OnValidate()
        {
            dict.Clear();

            for (var i = list.Count-1; i >= 0; --i)
            {
                var config = list[i];
                if (dict.ContainsKey(config.GetInstanceID()))
                {
                    list.RemoveAt(i);
                    continue;
                }

                dict[config.GetInstanceID()] = config;

                if (ReferenceEquals(config, this))
                    list.RemoveAt(i);
            }
        }
#endif
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}