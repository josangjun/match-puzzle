using System;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal static class TouchScreenKeyboardManager
	{
		private static TouchScreenKeyboardProxy _touchScreenKeyboard;

		private static TouchScreenKeyboardInput _currentInput;

		public static bool IsAnyInputActive => _currentInput != null;

		public static Rect? KeyboardArea
		{
			get
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Invalid comparison between Unknown and I4
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				if (_touchScreenKeyboard == null)
				{
					return null;
				}
				if ((int)Application.platform == 11)
				{
					float num = (float)GetAndroidKeyboardSize() * 1.1f;
					return new Rect(0f, (float)Screen.height - num, (float)Screen.width, num);
				}
				Rect area = TouchScreenKeyboardProxy.area;
				if (area.height > 0f)
				{
					return area;
				}
				return null;
			}
		}

		public static void Update()
		{
			if (!PlatformUtils.IsMobile)
			{
				return;
			}
			if (_currentInput != null && _touchScreenKeyboard != null)
			{
				_currentInput.SetTextFromKeyboard(_touchScreenKeyboard.text);
			}
			if (_touchScreenKeyboard != null)
			{
				if (_touchScreenKeyboard.status == TouchScreenKeyboardProxy.Status.Done)
				{
					_touchScreenKeyboard = null;
					if (_currentInput != null)
					{
						_currentInput.IsDone = true;
					}
				}
				else if (_touchScreenKeyboard.status == TouchScreenKeyboardProxy.Status.Canceled || _touchScreenKeyboard.status == TouchScreenKeyboardProxy.Status.LostFocus || !_touchScreenKeyboard.active)
				{
					_touchScreenKeyboard = null;
					if (_currentInput != null)
					{
						_currentInput.WasCancelled = true;
					}
				}
			}
			if (_currentInput == null && _touchScreenKeyboard != null)
			{
				_touchScreenKeyboard.active = false;
				_touchScreenKeyboard = null;
			}
		}

		public static void OnGUI()
		{
			if (PlatformUtils.IsMobile && _touchScreenKeyboard != null && Widgets.SimpleImageButton(GetTouchScreenKeyboardCloseButtonRect(), DebugConsole.Instance.ImageFiles._CloseKeyboardIcon))
			{
				Deactivate();
			}
		}

		public static Rect GetTouchScreenKeyboardCloseButtonRect()
		{
			if (!PlatformUtils.IsMobile)
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
			Rect area = TouchScreenKeyboardProxy.area;
			int num = (int)(GUIStyles.ScaleFactor * 100f);
			return new Rect(area.xMax - (float)num, area.yMin - (float)num, (float)num, (float)num);
		}

		public static void InputChanged(TouchScreenKeyboardInput input)
		{
			if (PlatformUtils.IsMobile && input == _currentInput && _touchScreenKeyboard != null)
			{
				_touchScreenKeyboard.text = input.Text;
			}
		}

		public static void Activate(TouchScreenKeyboardInput input)
		{
			if (PlatformUtils.IsMobile)
			{
				if (_touchScreenKeyboard == null)
				{
					TouchScreenKeyboardProxy.hideInput = true;
					_touchScreenKeyboard = TouchScreenKeyboardProxy.Open(input.Text, TouchScreenKeyboardProxy.TouchScreenKeyboardTypeProxy.Default, autocorrection: false, multiline: false, secure: false, alert: true);
					input.IsDone = false;
					input.WasCancelled = false;
				}
				else
				{
					_touchScreenKeyboard.text = input.Text;
				}
				_currentInput = input;
			}
		}

		public static bool IsActive(TouchScreenKeyboardInput input)
		{
			return _currentInput == input;
		}

		private static int GetAndroidKeyboardSize()
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				AndroidJavaObject val2 = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView", new object[0]);
				AndroidJavaObject val3 = new AndroidJavaObject("android.graphics.Rect", new object[0]);
				try
				{
					val2.Call("getWindowVisibleDisplayFrame", new object[1] { val3 });
					return Screen.height - val3.Call<int>("height", new object[0]);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		public static void Deactivate(TouchScreenKeyboardInput input)
		{
			if (input == _currentInput)
			{
				_currentInput = null;
			}
		}

		public static void Deactivate()
		{
			_currentInput = null;
		}
	}
}
