using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace Studio.Common
{
    public static class FileTool
    {
        public static string GetResourceDir() =>
#if UNITY_EDITOR
        $"{Application.dataPath}/.."
#else
        Application.persistentDataPath
#endif
         + "/Resource";

        private static JsonSerializer jsonSerializer;

        public static JsonSerializer GetSerializer()
        {
            if (jsonSerializer == null)
            {
                var serializer = JsonSerializer.Create(
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Converters = {
                            new DateTimeParser(),
                            new SilentEnumConverter()
                        }
                    });
                jsonSerializer = serializer;
            }
            return jsonSerializer;
        }

        [RuntimeInitializeOnLoadMethod]
        public static void InitOnLoad()
        {
            var settings = JsonConvert.DefaultSettings?.Invoke();
            if (settings != null)
            {
                settings.Converters = new List<JsonConverter> { new SilentEnumConverter(), new DateTimeParser() };
            }
            if (!System.IO.Directory.Exists(GetResourceDir()))
                System.IO.Directory.CreateDirectory(GetResourceDir());
        }

        public static void Load<T>(string file, out T[] field)
        {
            Load(file, out field, () => new T[0]);
        }

        public static void Load<T>(string file, out List<T> field)
        {
            Load(file, out field, () => new List<T>());
        }

        public static void Load<T>(string file, out T field, System.Func<T> defaultCreator = null, bool silently = false)
        {
            UnityEngine.Assertions.Assert.IsTrue(
                !typeof(T).IsAbstract,
                $"Load<T>: T({typeof(T)}) is invalid.");

            var path = $"{GetResourceDir()}/{file}";
            if (System.IO.File.Exists(path) == false)
            {
                if (silently == false)
                    Debug.LogWarning($"File does not exist. : {path}");
                if (defaultCreator == null)
                    defaultCreator = () => default;
                field = defaultCreator.Invoke();
                return;
            }

            var jsonSerializer = GetSerializer();

            using var inStream = new System.IO.FileStream(path, System.IO.FileMode.Open);
            using var reader = new System.IO.StreamReader(inStream);
            using var jsonReader = new Newtonsoft.Json.JsonTextReader(reader);
            var obj = jsonSerializer.Deserialize<T>(jsonReader);
            if (obj != null)
            {
                field = obj;
                return;
            }
            else
            {
                throw new System.InvalidOperationException($"Deseriaization failed:{path}");
            }
        }

        public static void DeleteAll()
        {
            var path = GetResourceDir();
            if (System.IO.Directory.Exists(path))
                System.IO.Directory.Delete(path, true);
        }

        public static bool Delete(string path)
        {
            var dir = GetResourceDir();
            var fullPath = $"{dir}/{path}";
            
            if (!System.IO.File.Exists(fullPath))
                return false;

            System.IO.File.Delete(fullPath);
            return true;
        }

        public static string Read(string path)
        {
            var dir = GetResourceDir();
            var fullPath = $"{dir}/{path}";
            if (!System.IO.File.Exists(fullPath))
                return string.Empty;
            return System.IO.File.ReadAllText(fullPath);
        }

        public static void Write(string path, string text)
        {
            var dir = GetResourceDir();
            var fullPath = $"{dir}/{path}";

            using (var file = new System.IO.StreamWriter(fullPath))
            {
                file.Write(text);
            }
        }

        public static async Task WriteAsync(string path, string text)
        {
            var dir = GetResourceDir();
            var fullPath = $"{dir}/{path}";

            using (var file = new System.IO.StreamWriter(fullPath))
            {
                await file.WriteAsync(text);
            }
        }
    }
}