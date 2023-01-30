using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal static class Widgets
	{
		private static float _timeOfMouseDown;

		public static void VerticalSeparator()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			GUI.DrawTexture(GUILayoutUtility.GetRect(1f, 1f, 1f, 5000f, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandHeight(true),
				GUILayout.Width(1f)
			}), (Texture)(object)GUIStyles.BlackTexture);
		}

		public static void HorizontalSeparator()
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			GUI.DrawTexture(GUILayoutUtility.GetRect(1f, 5000f, 1f, 1f, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandWidth(true),
				GUILayout.Height(1f)
			}), (Texture)(object)GUIStyles.BlackTexture);
		}

		public static bool Button(string text)
		{
			bool wasButtonHeldDown = false;
			return Button(null, null, text, 0, out wasButtonHeldDown);
		}

		public static bool Button(Texture2D icon, Texture2D backgroundImage, string text, int iconSize)
		{
			bool wasButtonHeldDown;
			return Button(icon, backgroundImage, text, iconSize, out wasButtonHeldDown);
		}

		public static bool Button(Texture2D icon, Texture2D backgroundImage, string text, int iconSize, out bool wasButtonHeldDown)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			int num = (int)(15f * GUIStyles.ScaleFactor);
			if (icon == null)
			{
				iconSize = 0;
			}
			Vector2 val = new Vector2((float)(iconSize + num * 2), (float)iconSize);
			if (text != "")
			{
				GUIContent val2 = new GUIContent(text);
				val = GUIStyles.HeaderButtonLabelStyle.CalcSize(val2);
				val.x += num + iconSize + num;
				if (val.y < (float)iconSize)
				{
					val.y = iconSize;
				}
			}
			return Button(GUILayoutUtility.GetRect(val.x, val.y, GUIStyles.HeaderButtonLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(val.x) }), icon, backgroundImage, text, iconSize, num, out wasButtonHeldDown);
		}

		public static bool Button(Rect rect, Texture2D icon, Texture2D backgroundImage, string text, int iconSize, int padding)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			bool wasButtonHeldDown;
			return Button(rect, icon, backgroundImage, text, iconSize, padding, out wasButtonHeldDown);
		}

		public static bool Button(Rect rect, Texture2D icon, Texture2D backgroundImage, string text, int iconSize, int padding, out bool wasButtonHeldDown)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Invalid comparison between Unknown and I4
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			wasButtonHeldDown = false;
			int controlID = GUIUtility.GetControlID((FocusType)2, rect);
			EventType type = Event.current.type;
			if ((int)type != 0)
			{
				if ((int)type != 1)
				{
					if ((int)type == 7)
					{
						if (backgroundImage != null)
						{
							GUI.DrawTexture(rect, (Texture)(object)backgroundImage);
						}
						if (GUIUtility.hotControl == controlID && _timeOfMouseDown > 0f && Time.realtimeSinceStartup - _timeOfMouseDown >= 0.3f)
						{
							wasButtonHeldDown = true;
							GUIUtility.hotControl=0;
						}
						Color color = GUI.color;
						if (!GUI.enabled)
						{
							GUI.color=new Color(0.5f, 0.5f, 0.5f, 0.5f);
						}
						if (icon != null)
						{
							GUI.DrawTexture(new Rect((rect).x + (float)padding, (rect).y + ((rect).height - (float)iconSize) / 2f, (float)iconSize, (float)iconSize), (Texture)(object)icon);
						}
						GUI.color=color;
						if (text != "")
						{
							GUI.Label(new Rect((rect).x + (float)iconSize + (float)padding, (rect).y, (rect).width, (rect).height), text, GUIStyles.HeaderButtonLabelStyle);
						}
					}
				}
				else if (GUI.enabled && (rect).Contains(Event.current.mousePosition) && GUIUtility.hotControl == controlID)
				{
					_timeOfMouseDown = -1f;
					GUIUtility.hotControl=0;
					Event.current.Use();
					return true;
				}
			}
			else if (GUI.enabled && (rect).Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl=controlID;
				_timeOfMouseDown = Time.realtimeSinceStartup;
				Event.current.Use();
			}
			return false;
		}

		public static bool SimpleImageButton(Rect rect, Texture2D icon)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			int controlID = GUIUtility.GetControlID((FocusType)2, rect);
			EventType type = Event.current.type;
			if ((int)type != 0)
			{
				if ((int)type != 1)
				{
					if ((int)type == 7)
					{
						Color color = GUI.color;
						if (!GUI.enabled)
						{
							GUI.color=new Color(0.5f, 0.5f, 0.5f, 0.5f);
						}
						if (icon != null)
						{
							GUI.DrawTexture(rect, (Texture)(object)icon, (ScaleMode)0);
						}
						GUI.color=color;
					}
				}
				else if (GUI.enabled && (rect).Contains(Event.current.mousePosition) && GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl=0;
					Event.current.Use();
					return true;
				}
			}
			else if (GUI.enabled && (rect).Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl=controlID;
				Event.current.Use();
			}
			return false;
		}

		public static bool Checkbox(bool isChecked, string text)
		{
			bool wasButtonHeldDown = false;
			if (Button(isChecked ? DebugConsole.Instance.ImageFiles._CheckboxChecked : DebugConsole.Instance.ImageFiles._CheckboxUnchecked, null, text, (int)(GUIStyles.ScaleFactor * 80f), out wasButtonHeldDown))
			{
				return !isChecked;
			}
			return isChecked;
		}

		public static bool HighlightHeaderButton(Texture2D icon, string text, bool isHighlighted, bool isActive, out bool wasButtonHeldDown)
		{
			Texture2D backgroundImage = (isHighlighted ? DebugConsole.Instance.ImageFiles._HighlightGradient : DebugConsole.Instance.ImageFiles._BackgroundGradient);
			if (isActive)
			{
				backgroundImage = DebugConsole.Instance.ImageFiles._ActiveGradient;
			}
			return HeaderButton(icon, backgroundImage, text, out wasButtonHeldDown);
		}

		public static bool ToggleHeaderButton(Texture2D icon, string text, bool value)
		{
			bool wasButtonHeldDown;
			return ToggleHeaderButton(icon, text, value, out wasButtonHeldDown);
		}

		public static bool ToggleHeaderButton(Texture2D icon, string text, bool value, out bool wasButtonHeldDown)
		{
			if (!HighlightHeaderButton(icon, text, value, isActive: false, out wasButtonHeldDown))
			{
				return value;
			}
			return !value;
		}

		public static bool HeaderButton(Texture2D icon)
		{
			bool wasButtonHeldDown;
			return HeaderButton(icon, DebugConsole.Instance.ImageFiles._BackgroundGradient, "", out wasButtonHeldDown);
		}

		public static bool OverlayHeaderButton(Texture2D icon, string text, bool value)
		{
			if (!HighlightHeaderButton(icon, text, isHighlighted: false, value, out var _))
			{
				return value;
			}
			return !value;
		}

		public static bool HeaderButton(Texture2D icon, Texture2D backgroundImage, string text)
		{
			bool wasButtonHeldDown;
			return HeaderButton(icon, backgroundImage, text, out wasButtonHeldDown);
		}

		public static bool HeaderButton(Texture2D icon, Texture2D backgroundImage, string text, out bool wasButtonHeldDown)
		{
			int iconSize = 0;
			if (icon != null)
			{
				iconSize = (int)(GUIStyles.HeaderHeight * 0.75f);
			}
			return Button(icon, backgroundImage, text, iconSize, out wasButtonHeldDown);
		}

		public static void TouchFriendlyTextField(TouchScreenKeyboardInput touchScreenKeyboardInput, GUIStyle style, params GUILayoutOption[] options)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Invalid comparison between Unknown and I4
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			Color color = GUI.color;
			if (PlatformUtils.IsMobile)
			{
				GUI.enabled=false;
				GUI.color=new Color(1f, 1f, 1f, 2f);
			}
			touchScreenKeyboardInput.Text = DebugConsole.Instance.DetectCloseKeyInInputText(FixedSizeTextField(touchScreenKeyboardInput.Text, style));
			GUI.enabled=true;
			GUI.color=color;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if ((int)Event.current.type == 7)
			{
				if (TouchScreenKeyboardManager.IsActive(touchScreenKeyboardInput))
				{
					GUIStyles.InputTextFieldStyle.DrawCursor(new Rect((lastRect).x, (lastRect).y, (lastRect).width, (lastRect).height), new GUIContent(touchScreenKeyboardInput.Text), GUIUtility.hotControl, touchScreenKeyboardInput.Text.Length);
				}
			}
			else if ((int)Event.current.type == 0 && (lastRect).Contains(Event.current.mousePosition))
			{
				TouchScreenKeyboardManager.Activate(touchScreenKeyboardInput);
			}
		}

		public static string FixedSizeTextField(string text, GUIStyle style, params GUILayoutOption[] options)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return GUI.TextField(GUILayoutUtility.GetRect(100f, 10000f, 0f, 1000f, style, options), text, style);
		}
	}
}
