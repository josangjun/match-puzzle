using System;
using System.Reflection;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal static class NativeMethodsInterface
	{
		private static MethodInfo _copyTextToClipboardMethod;

		private static MethodInfo _emailMethod;

		private static MethodInfo _getNativeScreenScaleFactorMethod;

		public static bool HasEmailSupport
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Invalid comparison between Unknown and I4
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Invalid comparison between Unknown and I4
				if ((int)Application.platform != 8)
				{
					return (int)Application.platform == 11;
				}
				return true;
			}
		}

		static NativeMethodsInterface()
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType("Opencoding.Console.NativeMethods");
				if ((object)type != null)
				{
					break;
				}
			}
			if ((object)type == null)
			{
				throw new InvalidOperationException("Couldn't find the NativeMethods class. Make sure it's in your project somewhere.");
			}
			_copyTextToClipboardMethod = type.GetMethod("CopyTextToClipboard", BindingFlags.Static | BindingFlags.Public);
			_emailMethod = type.GetMethod("SendEmail", BindingFlags.Static | BindingFlags.Public);
			_getNativeScreenScaleFactorMethod = type.GetMethod("GetNativeScreenScaleFactor", BindingFlags.Static | BindingFlags.Public);
			if ((object)_copyTextToClipboardMethod == null)
			{
				throw new InvalidOperationException("Couldn't find method 'CopyTextToClipboard' in NativeMethods class");
			}
			if ((object)_emailMethod == null)
			{
				throw new InvalidOperationException("Couldn't find method 'SendEmail' in NativeMethods class");
			}
			if ((object)_getNativeScreenScaleFactorMethod == null)
			{
				throw new InvalidOperationException("Couldn't find method 'GetNativeScreenScaleFactor' NativeMethods class");
			}
		}

		public static void CopyTextToClipboard(string text)
		{
			_copyTextToClipboardMethod.Invoke(null, new object[1] { text });
		}

		public static void SendEmail(Email email)
		{
			_emailMethod.Invoke(null, new object[1] { email });
		}

		public static float GetNativeScreenScaleFactor()
		{
			return (float)_getNativeScreenScaleFactorMethod.Invoke(null, null);
		}
	}
}
