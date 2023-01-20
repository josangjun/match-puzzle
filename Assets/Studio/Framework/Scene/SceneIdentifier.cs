using UnityEngine;

namespace Studio.Framework
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class SceneIdentifier : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private GUID _guid;
        public GUID guid { get { return _guid; } }

        public static implicit operator GUID(SceneIdentifier identifier)
        {
            return identifier.guid;
        }

        public bool registered { get; set; }

        private void Awake()
        {
            SceneIdDict.Add(this);
        }

        public void OnValidate()
        {
            if (!Application.isPlaying)
                SceneIdDict.Add(this);
        }

        private void OnDestroy()
        {
            var existedID = SceneIdDict.Find(this);
            if (existedID && existedID.GetInstanceID() == GetInstanceID())
                SceneIdDict.Remove(this);
        }

        public void OnBeforeSerialize()
        {
            if (_guid.IsEmpty)
            {
                _guid = GUID.New();
            }
        }

        public void OnAfterDeserialize()
        {
            
        }
    }

    [System.Serializable]
    public class SceneReference
    {
        public string name;
        
        [SerializeField]
        public GUID guid;

        public SceneReference() { }
        public SceneReference(string name, GUID guid)
        {
            this.name = name;
            this.guid = guid;
        }

        private System.WeakReference _sceneObj = null;

        public T GetComponent<T>() where T : class
        {
            T obj;
            if (_sceneObj != null && (UnityEngine.Object)_sceneObj.Target)
            {
                obj = _sceneObj.Target as T;
                if (obj != null)
                    return obj;
            }

            var id = SceneIdDict.Find(guid);
            obj = id ? id.GetComponent<T>() : null;
            if (obj != null)
            {
                _sceneObj = new System.WeakReference(obj);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"missing reference:{name}, GUID:{guid}");
            }
#endif
            return obj;
        }

        public GameObject gameObject
        {
            get
            {
                var id = SceneIdDict.Find(guid);
                if (id) return id.gameObject;
                return null;
            }
        }

        public static implicit operator bool (SceneReference sceneRef)
        {
            return sceneRef != null ? sceneRef.gameObject : false;
        }

        public void SetInstance(SceneIdentifier instance)
        {
            this._sceneObj = new System.WeakReference(instance);
            SceneIdDict.Add(instance);
        }
    }

    [System.Serializable]
    public struct GUID : System.IEquatable<GUID>, System.IComparable<GUID>
    {
        public int x, y, z, w;

        public bool IsEmpty
        {
            get { return x == 0 && y == 0 && z == 0 && w == 0; }
        }

        public static GUID New()
        {
            var bytes = System.Guid.NewGuid().ToByteArray();
            var x = System.BitConverter.ToInt32(bytes, 0);
            var y = System.BitConverter.ToInt32(bytes, 4);
            var z = System.BitConverter.ToInt32(bytes, 8);
            var w = System.BitConverter.ToInt32(bytes, 12);
            return new GUID(x, y, z, w);
        }

        public GUID(string arg)
        {
            if (arg.Length != 32)
                throw new System.InvalidOperationException();
            for (var i = 0; i < arg.Length; ++i)
            {
                if (!IsValid(arg[i]))
                {
                    Debug.LogError(arg[i]);
                    throw new System.InvalidOperationException();
                }
            }
            SetValue(out x, 0, arg);
            SetValue(out y, 8, arg);
            SetValue(out z, 16, arg);
            SetValue(out w, 24, arg);
        }

        public GUID(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static bool IsValid(char c)
        {
            if (char.IsDigit(c))
                return true;
            c = char.ToLower(c);
            return (c >= 'a' && c <= 'f');
        }

        public static GUID From(string arg)
        {
            return new GUID(arg);
        }

        private static void SetValue(out int d, int offset, string arg)
        {
            d = 0;
            var end = offset + 8;
            for (int i = offset, j = 0; i < end; i += 2, ++j)
            {
                var h = ToInt(arg[i]);
                var l = ToInt(arg[i + 1]);
                var b = (h << 4) | l;
                if (System.BitConverter.IsLittleEndian)
                    d |= (b << 24 - j * 8);
                else
                    d |= (b << j * 8);
            }
        }

        private static int ToInt(char c)
        {
            c = char.ToLower(c);
            if (char.IsDigit(c))
                return c - '0';
            return c - 'a' + 10;
        }

        public bool Equals(GUID other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }

        public int CompareTo(GUID other)
        {
            var c = x.CompareTo(other.x);
            if (c != 0) return c;
            c = y.CompareTo(other.y);
            if (c != 0) return c;
            c = z.CompareTo(other.z);
            if (c != 0) return c;
            return w.CompareTo(other.w);
        }

        public override string ToString()
        {
            return string.Format("{0:x8}{1:x8}{2:x8}{3:x8}", x, y, z, w);
        }
    }
}
