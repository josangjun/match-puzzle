using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Studio.Common
{
    public class DateTimeParser : DateTimeConverterBase
    {
        public System.DateTime Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return System.DateTime.FromBinary(0);
            const int zoneOffset = 20;
            if (zoneOffset >= str.Length)
                return System.DateTime.Parse(str);
            
            var zoneLength = 0;
            for (var i = zoneOffset; i < str.Length; ++i, ++zoneLength)
                if (char.IsWhiteSpace(str[i]))
                    break;
            var zone = str.Substring(20, zoneLength);
            if (!timeZoneOffsets.TryGetValue(zone, out var zzz)) 
                return System.DateTime.Parse(str);
            
            str = str.Replace(zone, zzz);
            var dateTime = System.DateTime.ParseExact(str, format, System.Globalization.CultureInfo.InvariantCulture);
            return dateTime;
        }

        public override bool CanConvert(System.Type objectType)
        {
            return base.CanConvert(objectType);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = reader.Value.ToString();
            var dateTime = Parse(val);
            return dateTime.ToBinary();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dateTime = System.DateTime.FromBinary((long)value);
            writer.WriteValue(dateTime.ToString());
        }

        private const string format = "ddd MMM dd HH:mm:ss zzz yyyy";
        private static System.Collections.Generic.Dictionary<string, string> timeZoneOffsets =
            new System.Collections.Generic.Dictionary<string, string>() {
                {"ACDT", "+10:30"},
                {"ACST", "+09:30"},
                {"ADT", "-03:00"},
                {"AEDT", "+11:00"},
                {"AEST", "+10:00"},
                {"AHDT", "-09:00"},
                {"AHST", "-10:00"},
                {"AST", "-04:00"},
                {"AT", "-02:00"},
                {"AWDT", "+09:00"},
                {"AWST", "+08:00"},
                {"BAT", "+03:00"},
                {"BDST", "+02:00"},
                {"BET", "-11:00"},
                {"BST", "-03:00"},
                {"BT", "+03:00"},
                {"BZT2", "-03:00"},
                {"CADT", "+10:30"},
                {"CAST", "+09:30"},
                {"CAT", "-10:00"},
                {"CCT", "+08:00"},
                {"CDT", "-05:00"},
                {"CED", "+02:00"},
                {"CET", "+01:00"},
                {"CEST", "+02:00"},
                {"CST", "-06:00"},
                {"EAST", "+10:00"},
                {"EDT", "-04:00"},
                {"EED", "+03:00"},
                {"EET", "+02:00"},
                {"EEST", "+03:00"},
                {"EST", "-05:00"},
                {"FST", "+02:00"},
                {"FWT", "+01:00"},
                {"GMT", "+00:00"},
                {"GST", "+10:00"},
                {"HDT", "-09:00"},
                {"HST", "-10:00"},
                {"IDLE", "+12:00"},
                {"IDLW", "-12:00"},
                {"IST", "+05:30"},
                {"IT", "+03:30"},
                {"JST", "+09:00"},
                {"JT", "+07:00"},
                {"MDT", "-06:00"},
                {"MED", "+02:00"},
                {"MET", "+01:00"},
                {"MEST", "+02:00"},
                {"MEWT", "+01:00"},
                {"MST", "-07:00"},
                {"MT", "+08:00"},
                {"NDT", "-02:30"},
                {"NFT", "-03:30"},
                {"NT", "-11:00"},
                {"NST", "+06:30"},
                {"NZ", "+11:00"},
                {"NZST", "+12:00"},
                {"NZDT", "+13:00"},
                {"NZT", "+12:00"},
                {"PDT", "-07:00"},
                {"PST", "-08:00"},
                {"ROK", "+09:00"},
                {"SAD", "+10:00"},
                {"SAST", "+09:00"},
                {"SAT", "+09:00"},
                {"SDT", "+10:00"},
                {"SST", "+02:00"},
                {"SWT", "+01:00"},
                {"USZ3", "+04:00"},
                {"USZ4", "+05:00"},
                {"USZ5", "+06:00"},
                {"USZ6", "+07:00"},
                {"UT", "-00:00"},
                {"UTC", "-00:00"},
                {"UZ10", "+11:00"},
                {"WAT", "-01:00"},
                {"WET", "-00:00"},
                {"WST", "+08:00"},
                {"YDT", "-08:00"},
                {"YST", "-09:00"},
                {"ZP4", "+04:00"},
                {"ZP5", "+05:00"},
                {"ZP6", "+06:00"}
            };
    }

    public class BinaryDateTimeAttribute : PropertyAttribute {}

    public class BinaryUnixTimeAttribute : PropertyAttribute {}

#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(BinaryDateTimeAttribute))]
    public class BinaryDateTimeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var val = property.longValue;
            var dt = System.DateTime.FromBinary(val);
            UnityEditor.EditorGUI.TextField(position, label, dt.ToString());
        }
    }

    [UnityEditor.CustomPropertyDrawer(typeof(BinaryUnixTimeAttribute))]
    public class BinaryUnixTimeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var val = property.longValue;
            try
            {
                var origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                var time = origin.AddMilliseconds(val).ToLocalTime();
                UnityEditor.EditorGUI.TextField(position, label, time.ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                base.OnGUI(position, property, label);
            }
        }
    }



#endif
}