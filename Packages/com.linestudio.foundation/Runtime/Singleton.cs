using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Common
{
    [System.Diagnostics.DebuggerStepThrough]
    public class Singleton<T> : StaticInstance where T : MonoBehaviour
    {
        public static T Instance => (T)GetInstance(TypeID);

        public readonly static int TypeID = typeof(T).GetHashCode();
        
        protected virtual void Awake()
        {
            if (Register(this, TypeID) == false)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            Remove(this, TypeID);
        }
    }
    
    public class StaticInstance : MonoBehaviour
    {
        private static readonly IDictionary<int, Component> instances = new Dictionary<int, Component>();

        public static void Purge()
        {
            foreach (Component instance in instances.Values)
            {
                if (instance)
                    Destroy(instance.gameObject);
            }
            instances.Clear();
        }

        protected static bool Register(Component com, int id)
        {
            if (instances.TryGetValue(id, out var existing))
            {
                Debug.LogWarning($"already existing:{existing.name}");
                return false;
            }
            instances[id] = com;
            return true;
        }

        protected static bool Register<T>(T instance) where T : Component
        {
            return Register(instance, typeof(T).GetHashCode());
        }

        protected static bool Remove<T>(T instance) where T : Component
        {
            return Remove(instance, typeof(T).GetHashCode());
        }

        protected static bool Remove(Component instance, int id)
        {
            if (instances.TryGetValue(id, out var existing))
            {
                if (ReferenceEquals(existing, instance))
                    return instances.Remove(id);
            }
            return false;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected static Component GetInstance(int typeID)
        {
            instances.TryGetValue(typeID, out var instance);
            return instance;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected static T GetInstance<T>() where T : Component
        {
            return (T)GetInstance(typeof(T).GetHashCode());
        }
    }
}