using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;

namespace Studio.Framework
{
    [ExecuteInEditMode]
    public class KeyMapper : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static bool ignoreAlphaNumeric { get; set; }

        [SerializeField]
        private EventSystem _evtSys;

        [System.Serializable]
        private struct KeyModifiers
        {
            public bool alt;
            public bool control;
            public bool shift;
            public int GetID()
            {
                int x = 0;
                if (alt) x |= 0x4;
                if (control) x |= 0x2;
                if (shift) x |= 0x1;
                return x;
            }
        }

        [System.Serializable]
        private class Binding
        {
            [HideInInspector] public string name;
            public KeyCode key = KeyCode.None;
            public KeyModifiers modifiers;
            public UnityEvent evt = null;
            [HideInInspector] public int ordinal = -1;
            public int GetID()
            {
                return GetID(this);
            }
            public static int GetID(Binding arg)
            {
                var id = GetID(arg.key, arg.modifiers.GetID());
                return id;
            }
            public static int GetID(KeyCode k, int mod)
            {
                return ((int)k << 8) | mod;
            }
            public static int GetID(Event arg)
            {
                var mod = new KeyModifiers();
                mod.alt = arg.alt;
                mod.control = arg.control;
                mod.shift = arg.shift;
                var m = mod.GetID();
                return GetID(arg.keyCode, m);
            }
            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.Append(key);
                if (modifiers.alt) sb.Append("+alt");
                if (modifiers.control) sb.Append("+control");
                if (modifiers.shift) sb.Append("+shift");
                return sb.ToString();
            }
        }
        [SerializeField] private List<Binding> bindings = new List<Binding>();
        private readonly Dictionary<int, UnityEvent> bindingDict = new Dictionary<int, UnityEvent>();

        // Use this for initialization
        void Start()
        {
            gameObject.tag = "EditorOnly";
        }

#if UNITY_EDITOR

        void OnGUI()
        {
            if (_evtSys && !_evtSys.isActiveAndEnabled)
                return;
            if (Opencoding.Console.DebugConsole.IsVisible)
                return;
            var evt = Event.current;
            if (evt != null && evt.type == EventType.KeyUp)
            {
                HandleKeyUp(evt);
            }
        }

        private static bool IsAlpha(KeyCode c)
        {
            return (c >= KeyCode.A && c <= KeyCode.Z);
        }

        private static bool IsNumeric(KeyCode c)
        {
            return (c >= KeyCode.Alpha0 && c <= KeyCode.Alpha9);
        }

        private void HandleKeyUp(Event evt)
        {
            if (ignoreAlphaNumeric)
            {
                if (IsAlpha(evt.keyCode))
                    return;
                if (IsNumeric(evt.keyCode))
                    return;
            }
            // if (evt.keyCode == KeyCode.Escape)
            // {
            //     if (Facade.IsTraining())
            //         Debug.Break();
            //     return;
            // }
            UnityEvent e;
            var id = Binding.GetID(evt);
            if (bindingDict.TryGetValue(id, out e))
                e?.Invoke();
        }
        //
        //	[ContextMenu("Sort")]
        //	private void Sort()
        //	{
        //		bindings.Sort((lhs, rhs) =>
        //			{
        //				return lhs.key.CompareTo(rhs.key);
        //			});
        //	}
        //

        [ContextMenu("OrderByOrdinal")]
        private void OrderByOrdinal()
        {
            bindings = bindings.OrderBy(arg => arg.ordinal).ToList();
            for (var i = 0; i < bindings.Count; ++i)
                bindings[i].ordinal = i;
        }

        private System.Text.StringBuilder sb = new System.Text.StringBuilder();

        private static UnityEditor.SerializedProperty FindArgument(UnityEditor.SerializedProperty prop, System.Type type)
        {
            if (type == typeof(int))
                return prop.FindPropertyRelative("m_IntArgument");
            else if (type == typeof(float) || type == typeof(double))
                return prop.FindPropertyRelative("m_FloatArgument");
            else if (type == typeof(string))
                return prop.FindPropertyRelative("m_StringArgument");
            else if (type == typeof(bool))
                return prop.FindPropertyRelative("m_BoolArgument");
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return prop.FindPropertyRelative("m_ObjectArgument");
            else
                return null;
        }

        private static object GetValue(UnityEditor.SerializedProperty prop)
        {
            if (prop == null) return null;
            switch (prop.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Integer:
                    return prop.intValue;
                case UnityEditor.SerializedPropertyType.Float:
                    return prop.floatValue;
                case UnityEditor.SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case UnityEditor.SerializedPropertyType.String:
                    return prop.stringValue;
                case UnityEditor.SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case UnityEditor.SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case UnityEditor.SerializedPropertyType.ObjectReference:
                    if (prop.objectReferenceValue)
                        return prop.objectReferenceValue.name;
                    return null;
                default:
                    return null;
            }
        }

        public enum CallMode { Void = 1, Object =2, Int = 3, Float, String = 5, Bool }

        private void OnValidate()
        {
            var existings = new Dictionary<string, int>();
            var serializedObject = new UnityEditor.SerializedObject(this);
            for (int i = 0; i < bindings.Count; ++i)
            {
                var binding = bindings[i];
                sb.Length = 0;
                var desc = binding.ToString();
                if (binding.ordinal == -1)
                    binding.ordinal = i;
                var evt = binding.evt;
                if (evt == null)
                    continue;
                var target = evt.GetPersistentTarget(0);
                var method = evt.GetPersistentMethodName(0);
                if (string.IsNullOrEmpty(method))
                    continue;
                sb.Append(i).Append('.').Append(desc).Append(", ")
                    .Append(method).Append('(');
                var methodList = target.GetType()
                    .GetMethods(
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance)
                    .Where(arg => arg.Name == method).ToList();
                if (methodList != null && methodList.Count > 0)
                {
                    var propPath = string.Format("bindings.Array.data[{0}].evt.m_PersistentCalls.m_Calls.Array.data[0].m_Arguments", i);
                    var prop = serializedObject.FindProperty(propPath);
                    var modePath = string.Format("bindings.Array.data[{0}].evt.m_PersistentCalls.m_Calls.Array.data[0].m_Mode", i);
                    var modeProp = serializedObject.FindProperty(modePath);
                    var mode = (CallMode)modeProp.intValue;
                    foreach (var methodInfo in methodList)
                    {
                        var parameters = methodInfo.GetParameters();
                        if (parameters.Length == 0)
                            continue;
                        var parameter = parameters[0].ParameterType;
                        if (mode == CallMode.String)
                            parameter = typeof(string);
                        
                        var first = FindArgument(prop, parameter);
                        if (first != null)
                        {
                            sb.Append(GetValue(first));
                            break;
                        }
                    }
                }
                sb.Append(')');
                binding.name = sb.ToString();
                int v;
                if (existings.TryGetValue(desc, out v))
                {
                    Debug.LogErrorFormat("already defined key: {0} -> {1}", binding.name, v);
                    continue;
                }
                existings.Add(desc, i);
            }
        }
#endif

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            BuildDictionary();
        }

        private void BuildDictionary()
        {
            bindingDict.Clear();
            foreach (var arg in bindings)
            {
                if (arg.key == KeyCode.None || arg.evt == null)
                    continue;

                var id = arg.GetID();
                if (bindingDict.ContainsKey(id))
                {
                    //Debug.LogErrorFormat("key already defined.:{0}", arg);
                    continue;
                }
                bindingDict[id] = arg.evt;
            }
        }
    }
}