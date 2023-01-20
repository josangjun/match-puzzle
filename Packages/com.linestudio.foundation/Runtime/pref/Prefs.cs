using System.Diagnostics;
using UnityEngine;

namespace Studio.Common
{
    [DebuggerStepThrough]
    public class Prefs : PreferenceImpl
    {
        public static readonly Prefs global = new Prefs(null);
        public static Prefs _prj;
        public static Prefs prj
        {
            get
            {
                if (_prj == null)
                {
    #if UNITY_EDITOR
                    string[] s = Application.dataPath.Split('/');
                    var prefix = s[s.Length - 2] + "/";
                    _prj = new Prefs(prefix);
    #else
                    _prj = global;
    #endif
                }
                return _prj;
            }
        }

        public Prefs(string prefix) : base(new RuntimePrefImpl(prefix)) { }
    }
}

