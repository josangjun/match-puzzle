using Opencoding.LogHistory;
using UnityEngine;

namespace Opencoding.Console
{
	internal class HeaderBar
	{
		private readonly DebugConsole _debugConsole;

		private readonly FilteredLogHistoryViewModel _logHistoryViewModel;

		private readonly InputField _inputField;

		private readonly FilterBar _filterBar;

		private readonly SettingsOverlay _settingsOverlay;

		private readonly Opencoding.LogHistory.LogHistory _logHistory;

		private readonly ImageFilesContainer _imageFiles;

		private readonly Settings _settings;

		public HeaderBar(DebugConsole debugConsole, FilteredLogHistoryViewModel filteredLogHistoryViewModel, InputField inputField, FilterBar filterBar, SettingsOverlay settingsOverlay, Opencoding.LogHistory.LogHistory logHistory, Settings settings)
		{
			_debugConsole = debugConsole;
			_logHistoryViewModel = filteredLogHistoryViewModel;
			_inputField = inputField;
			_filterBar = filterBar;
			_settingsOverlay = settingsOverlay;
			_logHistory = logHistory;
			_settings = settings;
			_imageFiles = _debugConsole.ImageFiles;
		}

		public void OnGUI()
		{
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Invalid comparison between Unknown and I4
			//IL_0393: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginHorizontal(GUIStyles.HeaderStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.HeaderHeight) });
			if (Widgets.HeaderButton(_debugConsole.ImageFiles._CloseButton, _debugConsole.ImageFiles._RedBackgroundGradient, ""))
			{
				_inputField.LoseFocus();
				DebugConsole.IsVisible = false;
			}
			Widgets.VerticalSeparator();
			if (Widgets.HeaderButton(_debugConsole.IsMaximized ? _imageFiles._MinimizeTopIcon : _imageFiles._MaximizeIcon))
			{
				_debugConsole.IsMaximized = !_debugConsole.IsMaximized;
				if (_debugConsole.IsMaximized && TouchScreenKeyboardManager.IsAnyInputActive)
				{
					TouchScreenKeyboardManager.Deactivate();
				}
			}
			Widgets.VerticalSeparator();
			GUI.enabled=true;
			GUILayout.FlexibleSpace();
			Widgets.VerticalSeparator();
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandHeight(true) });
			bool wasButtonHeldDown = false;
			_filterBar.IsVisible = Widgets.ToggleHeaderButton((_logHistoryViewModel.FilterString != "") ? _imageFiles._FilterActiveIcon : _imageFiles._SearchIcon, "", _filterBar.IsVisible, out wasButtonHeldDown);
			Widgets.VerticalSeparator();
			_logHistoryViewModel.ShowInfos = Widgets.ToggleHeaderButton(_imageFiles._InfoIcon, _logHistory.InfoCount.ToString(), _logHistoryViewModel.ShowInfos, out wasButtonHeldDown);
			if (wasButtonHeldDown)
			{
				HandleButtonHeld((LogType)3);
			}
			Widgets.VerticalSeparator();
			_logHistoryViewModel.ShowWarnings = Widgets.ToggleHeaderButton(_imageFiles._WarningIcon, _logHistory.WarningCount.ToString(), _logHistoryViewModel.ShowWarnings, out wasButtonHeldDown);
			if (wasButtonHeldDown)
			{
				HandleButtonHeld((LogType)2);
			}
			Widgets.VerticalSeparator();
			int num = _logHistory.ErrorCount;
			if (!_settings.ShowSeparateExceptionButton)
			{
				num += _logHistory.ExceptionCount;
			}
			if (!_settings.ShowSeparateAssertButton)
			{
				num += _logHistory.AssertCount;
			}
			_logHistoryViewModel.ShowErrors = Widgets.ToggleHeaderButton(_imageFiles._ErrorIcon, num.ToString(), _logHistoryViewModel.ShowErrors, out wasButtonHeldDown);
			if (wasButtonHeldDown)
			{
				HandleButtonHeld((LogType)0);
			}
			if (!_settings.ShowSeparateExceptionButton)
			{
				_logHistoryViewModel.ShowExceptions = _logHistoryViewModel.ShowErrors;
			}
			if (!_settings.ShowSeparateAssertButton)
			{
				_logHistoryViewModel.ShowAsserts = _logHistoryViewModel.ShowErrors;
			}
			Widgets.VerticalSeparator();
			if (_settings.ShowSeparateExceptionButton)
			{
				_logHistoryViewModel.ShowExceptions = Widgets.ToggleHeaderButton(_imageFiles._ExceptionIcon, _logHistory.ExceptionCount.ToString(), _logHistoryViewModel.ShowExceptions, out wasButtonHeldDown);
				if (wasButtonHeldDown)
				{
					HandleButtonHeld((LogType)4);
				}
				Widgets.VerticalSeparator();
			}
			if (_settings.ShowSeparateAssertButton)
			{
				_logHistoryViewModel.ShowAsserts = Widgets.ToggleHeaderButton(_imageFiles._AssertIcon, _logHistory.AssertCount.ToString(), _logHistoryViewModel.ShowAsserts, out wasButtonHeldDown);
				if (wasButtonHeldDown)
				{
					HandleButtonHeld((LogType)1);
				}
				Widgets.VerticalSeparator();
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			Widgets.VerticalSeparator();
			_settingsOverlay.IsVisible = Widgets.OverlayHeaderButton(_imageFiles._SettingsIcon, "", _settingsOverlay.IsVisible);
			if ((int)Event.current.type == 7)
			{
				_settingsOverlay.SettingsButtonRect = GUILayoutUtility.GetLastRect();
			}
			GUILayout.EndHorizontal();
			GUI.DrawTexture(GUILayoutUtility.GetRect(1f, 5000f, 1f, 1f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) }), (Texture)(object)GUIStyles.BlackTexture);
		}

		private void HandleButtonHeld(LogType logType)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Invalid comparison between Unknown and I4
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Invalid comparison between Unknown and I4
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Invalid comparison between Unknown and I4
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Invalid comparison between Unknown and I4
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Invalid comparison between Unknown and I4
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Invalid comparison between Unknown and I4
			if (_logHistoryViewModel.ShowWarnings == ((int)logType == 2) && _logHistoryViewModel.ShowErrors == ((int)logType == 0) && _logHistoryViewModel.ShowExceptions == ((int)logType == 4) && _logHistoryViewModel.ShowAsserts == ((int)logType == 1) && _logHistoryViewModel.ShowInfos == ((int)logType == 3))
			{
				_logHistoryViewModel.ShowWarnings = true;
				_logHistoryViewModel.ShowErrors = true;
				_logHistoryViewModel.ShowExceptions = true;
				_logHistoryViewModel.ShowAsserts = true;
				_logHistoryViewModel.ShowInfos = true;
			}
			else
			{
				_logHistoryViewModel.ShowWarnings = (int)logType == 2;
				_logHistoryViewModel.ShowErrors = (int)logType == 0;
				_logHistoryViewModel.ShowExceptions = (int)logType == 4;
				_logHistoryViewModel.ShowAsserts = (int)logType == 1;
				_logHistoryViewModel.ShowInfos = (int)logType == 3;
			}
		}
	}
}
