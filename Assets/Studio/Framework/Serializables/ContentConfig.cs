using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}