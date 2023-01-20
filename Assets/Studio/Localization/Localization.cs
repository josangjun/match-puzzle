using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.AddressableAssets;

namespace Studio.Common
{
    [CreateAssetMenu(fileName = "MessageSet", menuName = "Game/MessageSet")]
    public class Localization : ScriptableObject
    {
        private static Localization _data;

        public static Localization data
        {
            get
            {
#if UNITY_EDITOR
                if (_data == null)
                {
                    var path = UnityEditor.AssetDatabase.FindAssets("t:Localization Localization")
                        .Select(UnityEditor.AssetDatabase.GUIDToAssetPath).FirstOrDefault();
                    _data = UnityEditor.AssetDatabase.LoadAssetAtPath<Localization>(path);
                }
#endif
                return _data;
            }
            private set
            {
                _data = value;
            }
        }
        
        public static readonly PersistentData Lang = new PersistentData("Lang", PersistentData.Group.PERSISTENT);

        public static SystemLanguage systemLanguage
        {
            get
            {
                return LocalCode.ToSystemLanguage(GetSimpleId());
            }
            set
            {
                Lang.Set(LocalCode.GetLangCode(value));
                SystemInit();
            }
        }

        public static string GetLanguageCode()
        {
            var code = GetSimpleId();
            if (code == "tw")
            {
                return "zh-Hant";
            }
            else
            {
                return code;
            }
        }

        /// <summary>
		/// return tw instead of zh-Hant for Taiwan
		/// </summary>
		/// <returns></returns>
        //[System.Obsolete]
        public static string GetSimpleId()
        {
            var code = Lang.GetString();
            if (string.IsNullOrWhiteSpace(code))
            {
                code = LocalCode.GetLangCode(Application.systemLanguage);
                Lang.Set(code);
                Debug.LogWarningFormat("Initial Language: {0}", code);
            }
            //Debug.LogFormat("Language: {0}", code);
            return code;
        }
        
        [System.Serializable]
        public class Content {
            public string version;
            public int reivision;
            public string[] languageOrder;
            [HideInInspector]
            public string messagesetJson;
        }

        [System.Serializable]
        public class Entry : System.IComparable<Entry>
        {
            public string key;
            public string[] values;

            int System.IComparable<Entry>.CompareTo(Entry other)
            {
                return key.CompareTo(other.key);
            }
        }

        [SerializeField]
        private List<Entry> _entries;

        private static IDictionary<string, string[]> _dict = new SortedList<string, string[]>();

        [SerializeField]
        private Content _content;
        
        private static EmbeddedLocalization _embedded;
        
        public string[] GetLanguages() => _content.languageOrder;

        private static int _order { get; set; } = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void SystemInit()
        { 
            _embedded = Resources.Load<EmbeddedLocalization>("EmbeddedLocalization");
            if (_embedded)
                _embedded.Init();
            _order = -1;
        }

        public static IEnumerator InitializeAsync(string dataPath)
        {
            if (data == null)
            {
                var asyncOp = Addressables.LoadAssetAsync<Localization>(dataPath);
                yield return asyncOp;
                data = asyncOp.Result;
            }
            try
            {
                data.LoadFromFile();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                FileTool.Delete("xlt/messageset.json");
            }
        }

        [ContextMenu("LoadFromFile")]
        public void LoadFromFile()
        {
            FileTool.Load("xlt/messageset.json", out _content, null, true);
            if (_content == null)
                return;
            var list = JsonConvert.DeserializeObject<JArray>(_content.messagesetJson);
            _entries.Clear();
            _dict.Clear();
            foreach (JObject jobj in list)
            {
                foreach (var kv in jobj)
                {
                    var entry = new Entry {
                        key = kv.Key,
                        values = kv.Value.Values<string>().ToArray()
                    };
                    _dict[entry.key] = entry.values;
                    _entries.Add(entry);
                }
            }
            _entries.Sort();
            _content.messagesetJson = string.Empty;

            var lang = Localization.systemLanguage;
            for (var i = 0; i < _content.languageOrder.Length; ++i)
            {
                var arg = LocalCode.ToSystemLanguage(_content.languageOrder[i]);
                if (lang == arg)
                {
                    _order = i;
                    break;
                }
            }
            if (_order < 0)
            {
                _order = _embedded.order;
            }
        }

        private static string Get_(string key)
        {
            if (_order < 0)
            {
                if (_embedded == null)
                    return key;
                _order = _embedded.order;
                if (_order < 0)
                    return key;
            }

            string[] values;

            if (_dict.TryGetValue(key, out values) == false)
            {
                var e = data ? data._entries.BinarySearchFirst(arg => arg.key.CompareTo(key)) : null;
                if (e == null)
                {
                    if (_embedded == null)
                        return key;
                    return _embedded[key];
                }
                values = e.values;
            }
            return values[_order];
        }

        public static string Get(string key, params object[] args)
        {
            var val = Get_(key);
            if (string.IsNullOrEmpty(val))
                return key;
            if (args == null || args.Length == 0)
            {
                return val;
            }
            else
            {
                try
                {
                    return string.Format(val, args);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"{key} - {e.Message}\n{e.StackTrace}");
                }
            }
            return key;
        }

        public static bool ready 
        {
            get
            {
                return _order != -1;
            }
        }
    }
}