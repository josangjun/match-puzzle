using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Studio.Common
{
    public static class LocalCode
    {
        public static string JA = "ja";
        public static string EN = "en";
        public static string KO = "ko";
        public static string TH = "th";
        public static string TW = "tw";
        //public static string ID = "id";

        public delegate string CodeGetter(SystemLanguage l);
        public delegate int IndexGetter(SystemLanguage l);
        public delegate SystemLanguage SysLanguageConverter(string code);

        public static SystemLanguage DEFAULT_LANG = SystemLanguage.English;

        /// <summary>
        /// ISO 639-1 Language Code
        /// </summary>
        public static CodeGetter GetLangCode;
        /// <summary>
        /// ISO 3166-1 Alpha2 Country Code
        /// </summary>
        public static CodeGetter GetCountryCode;
        /// <summary>
        /// ISO 3166-1 Alpha3 Country Code
        /// </summary>
        public static CodeGetter GetCountryCode3Digit;
        /// <summary>
        /// ISO 4217 Currency Code
        /// </summary>
        public static CodeGetter GetCurrencyCode;

        public static IndexGetter GetIndex;

        private static SysLanguageConverter _toSystemLanguage;

        public static SysLanguageConverter ToSystemLanguage
        {
            get
            {
                if (_toSystemLanguage == null)
                    Init();
                return _toSystemLanguage;
            }
            private set
            {
                _toSystemLanguage = value;
            }
        }

        private static ILogger log => Debug.unityLogger;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Init()
        {
            var dicts = new[] { lang2digit, country2digit, country3digit, currency, additional };
            code2syslang = new Dictionary<string, SystemLanguage>();
            foreach (var d in dicts)
            {
                foreach (var pair in d)
                {
                    code2syslang[pair.Key.ToLowerInvariant()] = pair.Value;
                }
            }

            ToSystemLanguage = CodeToLang;
            sysToLang2 = lang2digit.Reverse();
            sysToCountry2 = country2digit.Reverse();
            sysToCountry3 = country3digit.Reverse();
            sysToCurrency = currency.Reverse();

            GetLangCode = l => LangToCode(sysToLang2, l);
            GetCountryCode = l => LangToCode(sysToCountry2, l);
            GetCountryCode3Digit = l => LangToCode(sysToCountry3, l);
            GetCurrencyCode = l => LangToCode(sysToCurrency, l);
            GetIndex = LangToIndex;

            InitCalander();

            SystemLanguage CodeToLang(string code)
            {
                if (string.IsNullOrEmpty(code))
                {
                    log.Log(LogType.Warning, "Null code");
                    return DEFAULT_LANG;
                }
                else if (code2syslang.TryGetValue(code.ToLowerInvariant(), out var l))
                {
                    return l;
                }
                else
                {
                    log.LogFormat(LogType.Warning, "Local code missing for '{0}'", code);
                    return DEFAULT_LANG;
                }
            }

            string LangToCode(Dictionary<SystemLanguage, string> dict, SystemLanguage lang)
            {
                if (dict.TryGetValue(lang, out var l))
                {
                    return l;
                }
                else
                {
                    if (dict.TryGetValue(DEFAULT_LANG, out l))
                    {
                        log.LogFormat(LogType.Warning, "Language match missing for '{0}'. Default Language {1}", lang.ToString(), l.ToString());
                        return l;
                    } else
                    {
                        log.LogFormat(LogType.Warning, "Default Language {0} is not defined", DEFAULT_LANG);
                        return lang.ToString();
                    }
                }
            }

            int LangToIndex(SystemLanguage l)
            {
                if (indexer.TryGetValue(l, out var cur))
                {
                    return cur;
                }
                else
                {
                    log.LogFormat(LogType.Warning, "Language match missing for '{0}'", l.ToString());
                    if (indexer.TryGetValue(DEFAULT_LANG, out cur))
                    {
                        return cur;
                    }
                    return -1;
                }
            }

            void InitCalander()
            {
                string digit2 = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                switch (digit2)
                {
                    case "ar":
                        Debug.LogFormat("Created UmAlQuraCalendar");
                        new System.Globalization.UmAlQuraCalendar();
                        break;
                    case "th":
                        Debug.LogFormat("Created ThaiBuddhistCalendar");
                        new System.Globalization.ThaiBuddhistCalendar();
                        break;
                }
            }
        }


        public static Dictionary<string, SystemLanguage> code2syslang;

        private static Dictionary<SystemLanguage, string> sysToLang2;
        public static Dictionary<string, SystemLanguage> lang2digit = new Dictionary<string, SystemLanguage>
        {
            { JA, SystemLanguage.Japanese },
            { EN, SystemLanguage.English },
            { KO, SystemLanguage.Korean },
            //{ TH, SystemLanguage.Thai },
            { TW, SystemLanguage.ChineseTraditional },
            //{ ID, SystemLanguage.Indonesian },
        };

        private static Dictionary<SystemLanguage, string> sysToCountry2;
        public static Dictionary<string, SystemLanguage> country2digit = new Dictionary<string, SystemLanguage>  // ISO 3166-1 Alpha2
        {
            { "JP", SystemLanguage.Japanese },
            { "EN", SystemLanguage.English },
            { "KO", SystemLanguage.Korean },
            //{ "TH", SystemLanguage.Thai },
            { "TW", SystemLanguage.ChineseTraditional },
            //{ "ID", SystemLanguage.Indonesian },
        };

        private static Dictionary<SystemLanguage, string> sysToCountry3;
        public static Dictionary<string, SystemLanguage> country3digit = new Dictionary<string, SystemLanguage>  // ISO 3166-1 Alpha3
        {
            { "JPN", SystemLanguage.Japanese },
            { "USA", SystemLanguage.English },
            { "KOR", SystemLanguage.Korean },
            //{ "THA", SystemLanguage.Thai },
            { "TWN", SystemLanguage.ChineseTraditional },
            //{ "IDN", SystemLanguage.Indonesian },
        };

        public static Dictionary<string, SystemLanguage> additional = new Dictionary<string, SystemLanguage>
        {
            { "zh-Hant", SystemLanguage.ChineseTraditional },
            { "zh_TW", SystemLanguage.ChineseTraditional },
            { "zhHant", SystemLanguage.ChineseTraditional },
            { "zhTW", SystemLanguage.ChineseTraditional },
            { "ZH-TW", SystemLanguage.ChineseTraditional },
            { "ZH_HANT", SystemLanguage.ChineseTraditional },
        };

        private static Dictionary<SystemLanguage, string> sysToCurrency;
        public static Dictionary<string, SystemLanguage> currency = new Dictionary<string, SystemLanguage> // ISO 4217
        {
            { "JPY", SystemLanguage.Japanese },
            { "USD", SystemLanguage.English },
            { "KRW", SystemLanguage.Korean },
            { "THB", SystemLanguage.Thai },
            { "TZS", SystemLanguage.ChineseTraditional },
            //{ "IDR", SystemLanguage.Indonesian },
        };

        public static SortedDictionary<SystemLanguage, int> indexer = new SortedDictionary<SystemLanguage, int>
        {
            { SystemLanguage.Japanese, 0 },
            { SystemLanguage.English, 1 },
            { SystemLanguage.Korean, 2 },
            { SystemLanguage.Thai, 3 },
            { SystemLanguage.ChineseTraditional, 4 },
            { SystemLanguage.Indonesian, 5 },
        };
    }
}