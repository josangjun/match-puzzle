using System;
using Opencoding.LogHistory;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal class LogItemPopupMenu
	{
		private Vector2 _position;

		private readonly DebugConsole _debugConsole;

		public bool IsVisible { get; set; }

		public Rect Rect { get; private set; }

		public LogHistoryItem TargetLogHistoryItem { get; private set; }

		private static bool IsHorizontal
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Invalid comparison between Unknown and I4
				if ((int)Application.platform != 11)
				{
					return (int)Application.platform == 8;
				}
				return true;
			}
		}

		public LogItemPopupMenu(DebugConsole debugConsole)
		{
			_debugConsole = debugConsole;
		}

		public void Show(Vector2 position, LogHistoryItem logHistoryItem)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (logHistoryItem == null)
			{
				throw new InvalidOperationException("Log history item can't be null");
			}
			_position = position;
			TargetLogHistoryItem = logHistoryItem;
			IsVisible = true;
		}

		public void OnGUI()
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Expected O, but got Unknown
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			int num = (NativeMethodsInterface.HasEmailSupport ? 3 : 2);
			int num2 = 0;
			int num3 = 0;
			Vector2 val = default(Vector2);
			if (IsHorizontal)
			{
				num2 = (int)(GUIStyles.ScaleFactor * 300f * (float)num);
				num3 = (int)GUIStyles.HeaderHeight;
				val=new Vector2(_position.x - (float)num2 / 2f, _position.y - (float)num3);
				val=new Vector2(Mathf.Clamp(val.x, 0f, _debugConsole.ActualConsoleWidth - (float)num2), Mathf.Clamp(val.y, 0f, (float)(Screen.height - num3)));
			}
			else
			{
				num2 = (int)(GUIStyles.ScaleFactor * 300f);
				num3 = (int)(GUIStyles.HeaderHeight * (float)num);
				val=new Vector2(Mathf.Clamp(_position.x - 4f, 0f, _debugConsole.ActualConsoleWidth - (float)num2), Mathf.Clamp(_position.y - 2f, 0f, (float)(Screen.height - num3)));
			}
			Rect = new Rect(val.x, val.y, (float)num2, (float)num3);
			GUI.Window(101, Rect, new GUI.WindowFunction(WindowFunc), "", GUIStyles.PopupMenuWindowStyle);
		}

		private void WindowFunc(int id)
		{
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			if (!IsVisible)
			{
				return;
			}
			if (IsHorizontal)
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (Button("Copy"))
			{
				NativeMethodsInterface.CopyTextToClipboard(TargetLogHistoryItem._LogMessage + "\n\n" + TargetLogHistoryItem._StackTrace);
			}
			if (NativeMethodsInterface.HasEmailSupport)
			{
				Separator();
				if (Button("Email..."))
				{
					string gameVersion = _debugConsole.Settings.GameVersion;
					Email email = new Email();
					email.Subject = TargetLogHistoryItem._LogMessage.Split(new char[1] { '\n' }, 2)[0];
					email.Message = string.Concat("Version: ", string.IsNullOrEmpty(gameVersion) ? "<not set>" : gameVersion, "\nPlatform: ", Application.platform, "\n\n", TargetLogHistoryItem._LogMessage, "\n", TargetLogHistoryItem._StackTrace);
					email.ToAddress = _debugConsole.Settings.DefaultToEmailAddress;
					NativeMethodsInterface.SendEmail(email);
				}
			}
			Separator();
			if (Button("Clear all"))
			{
				_debugConsole.Clear();
			}
			if (IsHorizontal)
			{
				GUILayout.EndHorizontal();
			}
			GUI.FocusWindow(id);
			GUI.BringWindowToFront(id);
		}

		private void Separator()
		{
			if (IsHorizontal)
			{
				Widgets.VerticalSeparator();
			}
			else
			{
				Widgets.HorizontalSeparator();
			}
		}

		private bool Button(string text)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Invalid comparison between Unknown and I4
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			GUIContent val = new GUIContent(text);
			Vector2 val2 = GUIStyles.HeaderButtonLabelStyle.CalcSize(val);
			GUIStyle val3 = (IsHorizontal ? GUIStyles.HorizontalPopupMenuButtonLabelStyle : GUIStyles.PopupMenuButtonLabelStyle);
			Rect rect = GUILayoutUtility.GetRect(val2.x, val2.y, val3, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
			if ((int)Event.current.type == 7)
			{
				val3.Draw(new Rect((rect).x, (rect).y, (rect).width, (rect).height), text, (rect).Contains(Event.current.mousePosition), true, false, false);
			}
			else if ((int)Event.current.type == 1 && GUI.enabled && (rect).Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				IsVisible = false;
				return true;
			}
			return false;
		}
	}
}
