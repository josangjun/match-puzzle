using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Studio.Common
{
    public class AnimationEventDispatcher : MonoBehaviour
    {
        [System.Serializable]
        public struct EventMap
        {
            public string id;
            public UnityEvent evt;

            public override string ToString()
            {
                return id;
            }
        }

        [SerializeField] private EventMap[] voidEvents;
        [SerializeField] private UnityEvent<string> stringEvent;
        [SerializeField] private UnityEvent<Object> objEvent;
        [SerializeField] private UnityEvent<int> intEvent;
        [SerializeField] private UnityEvent<float> floatEvent;

        private Dictionary<string, UnityEvent> voidMap;

        public void Event(string id)
        {
            if (voidMap == null)
            {
                voidMap = new Dictionary<string, UnityEvent>();
                foreach (var e in voidEvents)
                {
                    voidMap[e.id] = e.evt;
                }
            }
            if (voidMap.TryGetValue(id, out var evt))
            {
                evt.Invoke();
            }
        }

        public void StringEvent(string id)
        {
            stringEvent.Invoke(id);
        }

        public void FloatEvent(float f)
        {
            floatEvent.Invoke(f);
        }

        public void IntEvent(int i)
        {
            intEvent.Invoke(i);
        }

        public void ObjEvent(Object o)
        {
            objEvent.Invoke(o);
        }
    }
}
