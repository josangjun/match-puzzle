using Opencoding.LogHistory;
using UnityEngine;

namespace Opencoding.Console
{
	internal class SettingsOverlay
	{
		private Rect _settingsButtonRect;

		private DebugConsole _debugConsole;

		public bool IsVisible { get; set; }

		public Rect SettingsButtonRect
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _settingsButtonRect;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				_settingsButtonRect = value;
				Rect = new Rect(_settingsButtonRect.xMax - (float)Width + 6f, _settingsButtonRect.yMax, (float)Width, (float)Height);
			}
		}

		public Rect Rect { get; private set; }

		private int Height
		{
			get
			{
				int num = (int)(GUIStyles.ScaleFactor * 250f + 8f);
				if (!string.IsNullOrEmpty(_debugConsole.Settings.GameVersion))
				{
					num += GUIStyles.HeaderButtonLabelStyle.fontSize + 8;
				}
				return num;
			}
		}

		private int Width => (int)(GUIStyles.ScaleFactor * 900f);

		public SettingsOverlay(DebugConsole debugConsole)
		{
			IsVisible = false;
			_debugConsole = debugConsole;
		}

		public void OnGUI()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Invalid comparison between Unknown and I4
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Invalid comparison between Unknown and I4
			if (!IsVisible)
			{
				return;
			}
			if ((int)Event.current.type == 4 && ((int)Event.current.keyCode == 27 || (int)Event.current.keyCode == 13))
			{
				IsVisible = false;
				return;
			}
			GUILayout.BeginArea(Rect, GUIStyles.OverlayWindowStyle);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(16f * GUIStyles.ScaleFactor);
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(5f * GUIStyles.ScaleFactor);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			_debugConsole.ShowOnException = Widgets.Checkbox(_debugConsole.ShowOnException, "Show automatically on exception");
			GUILayout.Space(GUIStyles.ScaleFactor * 20f);
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			bool flag = (int)Application.platform != 17;
			bool hasEmailSupport = NativeMethodsInterface.HasEmailSupport;
			if ((hasEmailSupport || flag) && GUILayout.Button(hasEmailSupport ? "Email log..." : "Export log...", GUIStyles.SettingsPanelButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.ScaleFactor * 100f) }))
			{
				EmailLog();
			}
			GUI.enabled=Opencoding.LogHistory.LogHistory.Instance.LogItems.Count != 0;
			if (GUILayout.Button("Clear history", GUIStyles.SettingsPanelButtonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.ScaleFactor * 100f) }))
			{
				_debugConsole.Clear();
			}
			GUI.enabled=true;
			GUILayout.EndHorizontal();
			if (!string.IsNullOrEmpty(_debugConsole.Settings.GameVersion))
			{
				GUILayout.Label("Game version: " + _debugConsole.Settings.GameVersion, GUIStyles.HeaderButtonLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void EmailLog()
		{
			_debugConsole.EmailLog();
		}
	}
}
