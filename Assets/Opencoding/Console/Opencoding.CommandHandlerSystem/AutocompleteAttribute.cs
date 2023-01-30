using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Opencoding.CommandHandlerSystem
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class AutocompleteAttribute : Attribute
	{
		public MethodInfo MethodInfo { get; private set; }

		public AutocompleteAttribute(Type type, string methodName = "Autocomplete")
		{
			MethodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);
			if ((object)MethodInfo == null || (object)MethodInfo.ReturnType != typeof(IEnumerable<string>))
			{
				MethodInfo = null;
				Debug.LogWarning((object)$"Parameter has AutoComplete attribute but the type specified doesn't have a method called '{methodName}' that takes no arguments and returns IEnumerable<string> - ignoring");
			}
		}
	}
}
