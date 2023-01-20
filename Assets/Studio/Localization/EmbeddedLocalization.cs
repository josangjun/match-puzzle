using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using DataAccess;
#endif

namespace Studio.Common
{
    [CreateAssetMenu(fileName = "EmbeddedLocalization", menuName = "Game/Embedded MessageSet")]
    public class EmbeddedLocalization : ScriptableObject
    {
        [SerializeField]
        private List<Localization.Entry> _entries;
        [SerializeField]
        private string[] languageOrder;

        private Dictionary<string, string[]> _dict;

        public string[] GetLanguages() => languageOrder;

        public int order { get; private set; } = -1;

        public void Init()
        {
            var lang = Localization.systemLanguage;
            order = SelectOrder(lang);
            if (order < 0)
            {
                order = SelectOrder(LocalCode.DEFAULT_LANG);
            }
        }
#if UNITY_EDITOR
        [ContextMenu("LoadFromFile")]
        public void LoadFromFile()
        {
            order = -1;
            var table = DataTable.New.ReadCsv("Assets/embedded_text.csv");
            var colNames = new List<string>(table.ColumnNames);
            colNames.RemoveAt(0);
            languageOrder = colNames.ToArray();
            _entries = new List<Localization.Entry>();
            for (int i = 0; i < table.Columns[0].Values.Length; ++i)
            {
                var values = new string[table.Columns.Length - 1];
                for (int j=0; j<values.Length; ++j)
                {
                    values[j] = table.Columns[j+1].Values[i];
                }
                var entry = new Localization.Entry { key = table.Columns[0].Values[i], values = values };
                _entries.Add(entry);
            }
        }
#endif

        public string this[string key]
        {
            get
            {
                if (_dict == null || _dict.Count == 0)
                {
                    _dict = new Dictionary<string, string[]>();
                    foreach (var e in _entries)
                    {
                        _dict[e.key] = e.values;
                    }
                }
                string[] values;
                if (_dict.TryGetValue(key, out values) == false)
                {
                    var e = _entries.BinarySearchFirst(arg => arg.key.CompareTo(key));
                    if (e == null)
                        return key;
                    values = e.values;
                }
                return values[order];
            }
        }

        private int SelectOrder(SystemLanguage l)
        {
            for (var i = 0; i < languageOrder.Length; ++i)
            {
                var arg = LocalCode.ToSystemLanguage(languageOrder[i]);
                if (l == arg)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}