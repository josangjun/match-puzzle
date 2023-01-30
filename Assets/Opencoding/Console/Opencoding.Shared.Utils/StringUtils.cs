using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Opencoding.Shared.Utils
{
	public static class StringUtils
	{
		public static IEnumerable<string> SplitCommandLine(string commandLine)
		{
			bool inQuotes = false;
			return from arg in commandLine.Split(delegate(char c)
				{
					if (c == '"')
					{
						inQuotes = !inQuotes;
					}
					return !inQuotes && c == ' ';
				})
				select arg.Trim().TrimMatchingQuotes('"') into arg
				where !string.IsNullOrEmpty(arg)
				select arg;
		}

		public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
		{
			int num = 0;
			for (int c = 0; c < str.Length; c++)
			{
				if (controller(str[c]))
				{
					yield return str.Substring(num, c - num);
					num = c + 1;
				}
			}
			yield return str.Substring(num);
		}

		public static string TrimMatchingQuotes(this string input, char quote)
		{
			if (input.Length >= 2 && input[0] == quote && input[input.Length - 1] == quote)
			{
				return input.Substring(1, input.Length - 2);
			}
			return input;
		}

		public static string Indent(this string input, int tabCount)
		{
			string text = new string('\t', tabCount);
			return text + input.Replace("\n", "\n" + text);
		}

		public static Color GetColorForString(this string input)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			byte[] bytes = BitConverter.GetBytes(input.GetHashCode());
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] < 100)
				{
					bytes[i] = 100;
				}
			}
			return new Color32(bytes[0], bytes[1], bytes[2], byte.MaxValue);
		}

		public static string BetterToString(this Vector3 input)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return $"({input.x}, {input.y}, {input.z})";
		}

		public static string BetterToString(this Vector2 input)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return $"({input.x}, {input.y})";
		}
	}
}
