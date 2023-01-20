using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public partial class TypeEx
    {
        private static int assemblyCounts;
        private static Dictionary<string, Type> types;

        public static Type GetType(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblyCounts != assemblies.Length)
            {
                assemblyCounts = assemblies.Length;
                types = new Dictionary<string, Type>();
                foreach (Assembly assembly in assemblies)
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        types[type.FullName] = type;
                    }
                }
            }
            if (types.TryGetValue(fullName, out var t))
            {
                return t;
            } else
            {
                return null;
            }
        }
    }
}


