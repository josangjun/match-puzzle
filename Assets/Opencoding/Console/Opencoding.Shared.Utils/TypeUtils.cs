using System;
using System.Reflection;

namespace Opencoding.Shared.Utils
{
	public static class TypeUtils
	{
		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while ((object)toCheck != typeof(object) && (object)toCheck != null)
			{
				Type type = (toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck);
				if ((object)generic == type)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static PropertyInfo GetStaticPrivatePropertyInType(Type type, string name)
		{
			PropertyInfo property = type.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic);
			if ((object)property != null)
			{
				return property;
			}
			if ((object)type.BaseType != null)
			{
				return GetStaticPrivatePropertyInType(type.BaseType, name);
			}
			return null;
		}

		public static FieldInfo GetStaticPrivateFieldInType(Type type, string name)
		{
			FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
			if ((object)field != null)
			{
				return field;
			}
			if ((object)type.BaseType != null)
			{
				return GetStaticPrivateFieldInType(type.BaseType, name);
			}
			return null;
		}
	}
}
