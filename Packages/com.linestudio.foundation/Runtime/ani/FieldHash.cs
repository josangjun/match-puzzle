using System.Collections.Generic;

namespace Studio.Common
{
    public static class FieldHash
    {
        public static void FillHash(System.Type type, System.Func<string, int> hashFunc)
        {
            foreach (var f in GetFields(type))
            {
                if (f.IsLiteral)
                    continue;
                if (f.FieldType == typeof(int))
                {
                    var hash = hashFunc(f.Name);
                    f.SetValue(null, hash);
                    _hashToString[hash] = f.Name;
                }
            }
        }

        public static void FillNames(System.Type type)
        {
            foreach (var f in GetFields(type))
            {
                if (f.IsLiteral)
                {
                    continue;
                }
                if (f.FieldType == typeof(string))
                {
                    f.SetValue(null, f.Name);
                }
            }
        }

        private static readonly SortedList<int, string> _hashToString = new SortedList<int, string>();
        private static System.Reflection.FieldInfo[] GetFields(System.Type type)
        {
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.DeclaredOnly);
            return fields;
        }

        public static string ToString(int hash)
        {
            string name;
            if (_hashToString.TryGetValue(hash, out name))
                return name;
            return hash.ToString();
        }
    }
}



