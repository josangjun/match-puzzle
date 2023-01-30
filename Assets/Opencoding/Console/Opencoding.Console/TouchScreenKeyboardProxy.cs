using System;
using System.Reflection;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	public class TouchScreenKeyboardProxy
	{
		public enum TouchScreenKeyboardTypeProxy
		{
			Default,
			ASCIICapable,
			NumbersAndPunctuation,
			URL,
			NumberPad,
			PhonePad,
			NamePhonePad,
			EmailAddress
		}

		public enum Status
		{
			Visible,
			Done,
			Canceled,
			LostFocus
		}

		private object _keyboardObject;

		private static readonly Type _touchScreenKeyboardTypeType;

		private static readonly MethodInfo _destroyMethod;

		private static readonly MethodInfo _openMethod;

		private static MethodInfo _textPropertyGet;

		private static MethodInfo _textPropertySet;

		private static MethodInfo _hideInputPropertyGet;

		private static MethodInfo _hideInputPropertySet;

		private static MethodInfo _activePropertyGet;

		private static MethodInfo _activePropertySet;

		private static MethodInfo _donePropertyGet;

		private static MethodInfo _wasCanceledPropertyGet;

		private static MethodInfo _statusPropertyGet;

		private static MethodInfo _areaPropertyGet;

		private static MethodInfo _visiblePropertyGet;

		public string text
		{
			get
			{
				return (string)_textPropertyGet.Invoke(_keyboardObject, null);
			}
			set
			{
				_textPropertySet.Invoke(_keyboardObject, new object[1] { value });
			}
		}

		public static bool hideInput
		{
			get
			{
				if (PlatformUtils.IsMobile)
				{
					return (bool)_hideInputPropertyGet.Invoke(null, null);
				}
				return false;
			}
			set
			{
				if (PlatformUtils.IsMobile)
				{
					_hideInputPropertySet.Invoke(null, new object[1] { value });
				}
			}
		}

		public bool active
		{
			get
			{
				return (bool)_activePropertyGet.Invoke(_keyboardObject, null);
			}
			set
			{
				_activePropertySet.Invoke(_keyboardObject, new object[1] { value });
			}
		}

		public Status status => (Status)_statusPropertyGet.Invoke(_keyboardObject, null);

		public bool done => (bool)_donePropertyGet.Invoke(_keyboardObject, null);

		public bool wasCanceled => (bool)_wasCanceledPropertyGet.Invoke(_keyboardObject, null);

		public static Rect area
		{
			get
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				if (PlatformUtils.IsMobile)
				{
					return (Rect)_areaPropertyGet.Invoke(null, null);
				}
				return default(Rect);
			}
		}

		public static bool visible
		{
			get
			{
				if (PlatformUtils.IsMobile)
				{
					return (bool)_visiblePropertyGet.Invoke(null, null);
				}
				return false;
			}
		}

		static TouchScreenKeyboardProxy()
		{
			if (PlatformUtils.IsMobile)
			{
				Type obj = Assembly.GetAssembly(typeof(Vector3))!.GetType("UnityEngine.TouchScreenKeyboard") ?? throw new InvalidOperationException("Can't find the TouchScreenKeyboard class. Most likely you have stripping turned on, if so see here: http://opencoding.net/TouchConsolePro/technical_documentation.php#stripping - alternatively you may be using an unsupported Unity version.");
				_touchScreenKeyboardTypeType = obj!.Assembly.GetType("UnityEngine.TouchScreenKeyboardType");
				if ((object)_touchScreenKeyboardTypeType == null)
				{
					throw new InvalidOperationException("Can't find the TouchScreenKeyboardType class. Most likely you have stripping turned on, if so see here: http://opencoding.net/TouchConsolePro/technical_documentation.php#stripping - alternatively you may be using an unsupported Unity version.");
				}
				PropertyInfo property = obj!.GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
				_textPropertyGet = property.GetGetMethod();
				_textPropertySet = property.GetSetMethod();
				PropertyInfo property2 = obj!.GetProperty("hideInput", BindingFlags.Static | BindingFlags.Public);
				_hideInputPropertyGet = property2.GetGetMethod();
				_hideInputPropertySet = property2.GetSetMethod();
				PropertyInfo property3 = obj!.GetProperty("active", BindingFlags.Instance | BindingFlags.Public);
				_activePropertyGet = property3!.GetGetMethod();
				_activePropertySet = property3!.GetSetMethod();
				_donePropertyGet = obj!.GetProperty("done", BindingFlags.Instance | BindingFlags.Public)!.GetGetMethod();
				_wasCanceledPropertyGet = obj!.GetProperty("wasCanceled", BindingFlags.Instance | BindingFlags.Public)!.GetGetMethod();
				_areaPropertyGet = obj!.GetProperty("area", BindingFlags.Static | BindingFlags.Public)!.GetGetMethod();
				_visiblePropertyGet = obj!.GetProperty("visible", BindingFlags.Static | BindingFlags.Public)!.GetGetMethod();
				_statusPropertyGet = obj!.GetProperty("status", BindingFlags.Instance | BindingFlags.Public)!.GetGetMethod();
				_destroyMethod = obj!.GetMethod("Destroy", BindingFlags.Instance | BindingFlags.NonPublic);
				_openMethod = obj!.GetMethod("Open", BindingFlags.Static | BindingFlags.Public, null, new Type[7]
				{
					typeof(string),
					_touchScreenKeyboardTypeType,
					typeof(bool),
					typeof(bool),
					typeof(bool),
					typeof(bool),
					typeof(string)
				}, null);
				if ((object)property == null || (object)_areaPropertyGet == null || (object)property2 == null || (object)_donePropertyGet == null || (object)_wasCanceledPropertyGet == null || (object)_visiblePropertyGet == null || (object)_destroyMethod == null || (object)_openMethod == null)
				{
					throw new InvalidOperationException("Can't find a required property in the TouchScreenKeyboard class. Most likely you have stripping turned on, if so see here: http://opencoding.net/TouchConsolePro/technical_documentation.php#stripping - alternatively you may be using an unsupported Unity version.");
				}
			}
		}

		~TouchScreenKeyboardProxy()
		{
			_destroyMethod.Invoke(_keyboardObject, null);
		}

		public static TouchScreenKeyboardProxy Open(string text, TouchScreenKeyboardTypeProxy keyboardType, bool autocorrection, bool multiline, bool secure, bool alert)
		{
			if (PlatformUtils.IsMobile)
			{
				TouchScreenKeyboardProxy touchScreenKeyboardProxy = new TouchScreenKeyboardProxy();
				touchScreenKeyboardProxy._keyboardObject = _openMethod.Invoke(null, new object[7]
				{
					text,
					Enum.ToObject(_touchScreenKeyboardTypeType, keyboardType),
					autocorrection,
					multiline,
					secure,
					alert,
					""
				});
				return touchScreenKeyboardProxy;
			}
			return null;
		}
	}
}
