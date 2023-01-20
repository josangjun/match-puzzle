using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

namespace Studio.Common
{
    public class SilentEnumConverter : StringEnumConverter
    {
        public SilentEnumConverter()
        {
            base.AllowIntegerValues = true;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                return existingValue;
            }
        }
    }
}