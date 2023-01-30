using System;

namespace Opencoding.Shared.Utils
{
	public static class ArrayUtils
	{
		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] array = new T[length];
			Array.Copy(data, index, array, 0, length);
			return array;
		}

		public static void Populate<T>(this T[] data, T value)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = value;
			}
		}
	}
}
