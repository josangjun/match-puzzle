using System;
using System.Collections.Generic;
using UnityEngine;

namespace Opencoding.CommandHandlerSystem
{
	public class Utils
	{
		private static string[] _colorNames = new string[10] { "red", "green", "blue", "white", "black", "yellow", "cyan", "magenta", "gray", "clear" };

		public static object GetArgumentValueFromString(string argument, Type type)
		{
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			if ((object)type == typeof(string))
			{
				return argument;
			}
			if ((object)type == typeof(int))
			{
				return int.Parse(argument);
			}
			if ((object)type == typeof(uint))
			{
				return uint.Parse(argument);
			}
			if ((object)type == typeof(short))
			{
				return short.Parse(argument);
			}
			if ((object)type == typeof(ushort))
			{
				return ushort.Parse(argument);
			}
			if ((object)type == typeof(ulong))
			{
				return ulong.Parse(argument);
			}
			if ((object)type == typeof(long))
			{
				return long.Parse(argument);
			}
			if ((object)type == typeof(byte))
			{
				return byte.Parse(argument);
			}
			if ((object)type == typeof(sbyte))
			{
				return sbyte.Parse(argument);
			}
			if ((object)type == typeof(char))
			{
				return char.Parse(argument);
			}
			if ((object)type == typeof(float))
			{
				return float.Parse(argument);
			}
			if ((object)type == typeof(bool))
			{
				if (argument == "1")
				{
					return true;
				}
				if (argument == "0")
				{
					return false;
				}
				return bool.Parse(argument);
			}
			if (type.IsEnum)
			{
				return Enum.Parse(type, argument, ignoreCase: true);
			}
			if ((object)type == typeof(Vector3))
			{
				string[] array = argument.Split(new char[1] { ',' });
				if (array.Length == 1)
				{
					return (object)new Vector3(float.Parse(array[0]), float.Parse(array[0]), float.Parse(array[0]));
				}
				if (array.Length == 3)
				{
					return (object)new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
				}
				throw new InvalidOperationException("Expected either 1 or 3 arguments.");
			}
			if ((object)type == typeof(Vector2))
			{
				string[] array2 = argument.Split(new char[1] { ',' });
				if (array2.Length == 1)
				{
					return (object)new Vector2(float.Parse(array2[0]), float.Parse(array2[0]));
				}
				if (array2.Length == 2)
				{
					return (object)new Vector2(float.Parse(array2[0]), float.Parse(array2[1]));
				}
				throw new InvalidOperationException("Expected either 1 or 2 arguments.");
			}
			if ((object)type == typeof(Color))
			{
				return GetColorFromArgument(argument);
			}
			if ((object)type == typeof(Color32))
			{
				Color colorFromArgument = GetColorFromArgument(argument);
				return (object)new Color32((byte)(colorFromArgument.r * 255f), (byte)(colorFromArgument.g * 255f), (byte)(colorFromArgument.b * 255f), (byte)(colorFromArgument.a * 255f));
			}
			throw new InvalidOperationException(string.Concat("Type ", type, " is not supported as a command handler argument."));
		}

		private static Color GetColorFromArgument(string argument)
		{
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			switch (argument.ToLower())
			{
			case "red":
				return Color.red;
			case "green":
				return Color.green;
                case "blue":
				return Color.blue;
                case "white":
				return Color.white;
			case "black":
				return Color.black;
                case "yellow":
				return Color.yellow;
                case "cyan":
				return Color.cyan;
                case "magenta":
				return Color.magenta;
                case "gray":
				return Color.gray;
                case "grey":
				return Color.grey;
                case "clear":
				return Color.clear;
                default:
			{
				string[] array = argument.Split(new char[1] { ',' });
				if (array.Length == 1)
				{
					return new Color(float.Parse(array[0]), float.Parse(array[0]), float.Parse(array[0]));
				}
				if (array.Length == 3)
				{
					return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
				}
				if (array.Length == 4)
				{
					return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
				}
				throw new InvalidOperationException("Expected either a color name or 1 or 3 or 4 numerical arguments.");
			}
			}
		}

		public static IEnumerable<string> GetDefaultParameterPossibleOptions(Type type)
		{
			List<string> list = new List<string>();
			if (type.IsEnum)
			{
				list.AddRange(Enum.GetNames(type));
			}
			else if ((object)type == typeof(bool))
			{
				list.Add("true");
				list.Add("false");
			}
			else if ((object)type == typeof(Color) || (object)type == typeof(Color32))
			{
				return _colorNames;
			}
			return list;
		}

		public static string GetFriendlyTypeName(Type type)
		{
			if ((object)type == typeof(float))
			{
				return "float";
			}
			if ((object)type == typeof(double))
			{
				return "double";
			}
			if ((object)type == typeof(int) || (object)type == typeof(short) || (object)type == typeof(long))
			{
				return "integer";
			}
			if ((object)type == typeof(Color) || (object)type == typeof(Color32))
			{
				return "color";
			}
			return type.Name;
		}

		public static string WrapInQuotesIfNecessary(string s)
		{
			if (s.Contains(" "))
			{
				return $"\"{s}\"";
			}
			return s;
		}
	}
}
