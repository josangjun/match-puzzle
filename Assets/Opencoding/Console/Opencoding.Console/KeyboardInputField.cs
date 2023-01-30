using System;
using System.Reflection;
using Opencoding.CommandHandlerSystem;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal class KeyboardInputField : InputField
	{
		private string _commandInput = "";

		private Rect _commandInputRect;

		private TextEditor _commandInputBoxTextEditor;

		private readonly ImageFilesContainer _imageFiles;

		private readonly InputHistory _inputHistory;

		private int _tabIndex;

		private bool _moveCursorToEndNextRepaint;

		private string _inputFieldName;

		private FieldInfo _unityBefore53ContentField;

		private PropertyInfo _unityAfter53TextProperty;

		private bool _isOnUnityBefore53;

		private readonly LogHistoryView _logHistoryView;

		private readonly HelpOverlay _helpOverlay;

		private readonly PropertyInfo _cursorIndexProperty;

		private readonly PropertyInfo _selectIndexProperty;

		private readonly FieldInfo _cursorIndexField;

		private readonly FieldInfo _selectIndexField;

		public override string Input
		{
			get
			{
				return _commandInput;
			}
			set
			{
				_commandInput = value;
				SetTextEditorTextWithReflection();
				GUI.FocusControl(_inputFieldName);
				_moveCursorToEndNextRepaint = true;
			}
		}

		public override bool HasFocus => GUI.GetNameOfFocusedControl() == _inputFieldName;

		public override Rect TextFieldRect => _commandInputRect;

		public KeyboardInputField(ImageFilesContainer imageFiles, InputHistory inputHistory, LogHistoryView logHistoryView, HelpOverlay helpOverlay)
		{
			_imageFiles = imageFiles;
			_inputHistory = inputHistory;
			_logHistoryView = logHistoryView;
			_helpOverlay = helpOverlay;
			string unityVersion = Application.unityVersion;
			_isOnUnityBefore53 = unityVersion.StartsWith("4") || unityVersion.StartsWith("5.0") || unityVersion.StartsWith("5.1") || unityVersion.StartsWith("5.2");
			_cursorIndexProperty = typeof(TextEditor).GetProperty("cursorIndex", BindingFlags.Instance | BindingFlags.Public);
			_selectIndexProperty = typeof(TextEditor).GetProperty("selectIndex", BindingFlags.Instance | BindingFlags.Public);
			if ((object)_cursorIndexProperty == null && (object)_selectIndexProperty == null)
			{
				_cursorIndexField = typeof(TextEditor).GetField("pos", BindingFlags.Instance | BindingFlags.Public);
				_selectIndexField = typeof(TextEditor).GetField("selectPos", BindingFlags.Instance | BindingFlags.Public);
				if ((object)_cursorIndexField == null || (object)_selectIndexField == null)
				{
					Debug.LogWarning((object)"Couldn't find TextEditor properties via reflection. This probably means that you're using an unsupported version of Unity. Contact support@opencoding.net and let me know!");
				}
			}
		}

		public override void OnGUI(Rect containingRect)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Invalid comparison between Unknown and I4
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Invalid comparison between Unknown and I4
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Invalid comparison between Unknown and I4
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Expected O, but got Unknown
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Invalid comparison between Unknown and I4
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			if (_moveCursorToEndNextRepaint && (int)Event.current.type == 7)
			{
				if ((object)_cursorIndexProperty != null)
				{
					_cursorIndexProperty.SetValue(_commandInputBoxTextEditor, _commandInput.Length, null);
				}
				if ((object)_selectIndexProperty != null)
				{
					_selectIndexProperty.SetValue(_commandInputBoxTextEditor, _commandInput.Length, null);
				}
				if ((object)_cursorIndexField != null)
				{
					_cursorIndexField.SetValue(_commandInputBoxTextEditor, _commandInput.Length);
				}
				if ((object)_selectIndexField != null)
				{
					_selectIndexField.SetValue(_commandInputBoxTextEditor, _commandInput.Length);
				}
				_moveCursorToEndNextRepaint = false;
			}
			if ((int)Event.current.type == 4)
			{
				HandleKeyDownEvent();
			}
			if (Event.current.isKey && ((int)Event.current.keyCode == 9 || Event.current.character == '\t'))
			{
				Event.current.Use();
			}
			GUILayout.BeginHorizontal(GUIStyles.HeaderStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandWidth(true),
				GUILayout.Height(GUIStyles.HeaderHeight)
			});
			_inputFieldName = "InputField" + GUIUtility.GetControlID((FocusType)1);
			GUI.SetNextControlName(_inputFieldName);
			string commandInput = _commandInput;
			string text = Widgets.FixedSizeTextField(_commandInput, GUIStyles.InputTextFieldStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			_commandInput = DebugConsole.Instance.DetectCloseKeyInInputText(text);
			if (DebugConsole.IsOnUnityVersionWithBrokenKeyboardInput() && _commandInput != text)
			{
				DebugConsole.IsVisible = false;
			}
			_commandInput = _commandInput.TrimStart(new char[1] { ' ' });
			if ((int)Event.current.type == 7)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				_commandInputRect = new Rect(containingRect.x + (lastRect).x, containingRect.y + (lastRect).y, (lastRect).width, (lastRect).height);
			}
			if (HasFocus)
			{
				_commandInputBoxTextEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			}
			if (DebugConsole.Instance.JustMadeVisible)
			{
				DebugConsole.Instance.JustMadeVisible = false;
				GUI.FocusControl(_inputFieldName);
			}
			if (commandInput != _commandInput)
			{
				_tabIndex = 0;
				ConfirmInput();
			}
			bool flag = Widgets.ToggleHeaderButton(_imageFiles._HelpIcon, "", _helpOverlay.IsVisible);
			if (flag && !_helpOverlay.IsVisible)
			{
				DebugConsole.Instance.Minimize();
			}
			_helpOverlay.IsVisible = flag;
			if ((int)Event.current.type == 7)
			{
				base.HelpButtonRect = GUILayoutUtility.GetLastRect();
			}
			GUILayout.EndHorizontal();
		}

		public override void ClearInput()
		{
			_commandInput = "";
		}

		private void HandleKeyDownEvent()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Invalid comparison between Unknown and I4
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			KeyCode keyCode = Event.current.keyCode;
			if ((int)keyCode <= 13)
			{
				if ((int)keyCode != 9)
				{
					if ((int)keyCode == 13)
					{
						HandleReturnKeyPressed();
					}
				}
				else
				{
					HandleTabKeyPressed();
				}
			}
			else if ((int)keyCode != 273)
			{
				if ((int)keyCode == 274)
				{
					HandleDownKeyPressed();
				}
			}
			else
			{
				HandleUpKeyPressed();
			}
		}

		private void HandleReturnKeyPressed()
		{
			if (!(_commandInput.Trim() == ""))
			{
				_inputHistory.RecordInput(_commandInput);
				CommandHandlers.HandleCommand(_commandInput);
				_commandInput = "";
				ConfirmInput();
				_logHistoryView.AutoScrolling = true;
			}
		}

		private void HandleUpKeyPressed()
		{
			string previousInput = _inputHistory.GetPreviousInput();
			if (previousInput != null)
			{
				Input = previousInput;
				ConfirmInput();
				Event.current.Use();
			}
		}

		private void HandleDownKeyPressed()
		{
			string nextInput = _inputHistory.GetNextInput();
			if (nextInput != null)
			{
				Input = nextInput;
				ConfirmInput();
				Event.current.Use();
			}
		}

		private void HandleTabKeyPressed()
		{
			if (_lastTypedParameters != null && _lastTypedParameters.Length != 0)
			{
				int num = _lastTypedParameters.Length - 2;
				if (base.LastTypedInput.EndsWith(" "))
				{
					num++;
				}
				if (num < 0)
				{
					CommandHandler commandHandler = CommandHandlers.FindClosestMatchingCommand(_lastTypedParameters[0], _tabIndex);
					if (commandHandler != null)
					{
						Input = commandHandler.CommandName;
					}
				}
				else
				{
					CommandHandler commandHandler2 = CommandHandlers.GetCommandHandler(_lastTypedParameters[0]);
					if (commandHandler2 != null)
					{
						ParamInfo[] parameters = commandHandler2.Parameters;
						if (num < parameters.Length)
						{
							string parameterValue = ((_lastTypedParameters.Length < num + 2) ? "" : _lastTypedParameters[num + 1]);
							string[] array = parameters[num].GetParameterPossibleValues(parameterValue).ToArray();
							if (array.Length != 0)
							{
								string text = array[_tabIndex % array.Length];
								if (!string.IsNullOrEmpty(text))
								{
									string text2 = string.Join(" ", _lastTypedParameters.SubArray(0, num + 1));
									Input = text2 + " " + Opencoding.CommandHandlerSystem.Utils.WrapInQuotesIfNecessary(text);
								}
							}
						}
					}
				}
			}
			_tabIndex++;
		}

		private void SetTextEditorTextWithReflection()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			if (_isOnUnityBefore53)
			{
				GUIContent value = new GUIContent(_commandInput);
				if ((object)_unityBefore53ContentField == null)
				{
					_unityBefore53ContentField = typeof(TextEditor).GetField("content", BindingFlags.Instance | BindingFlags.Public);
				}
				if ((object)_unityBefore53ContentField == null)
				{
					throw new InvalidOperationException("Couldn't find 'content' field in TextEditor class on Unity version " + Application.unityVersion);
				}
				_unityBefore53ContentField.SetValue(_commandInputBoxTextEditor, value);
			}
			else
			{
				if ((object)_unityAfter53TextProperty == null)
				{
					_unityAfter53TextProperty = typeof(TextEditor).GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
				}
				if ((object)_unityAfter53TextProperty == null)
				{
					throw new InvalidOperationException("Couldn't find 'text' property in TextEditor class on Unity version " + Application.unityVersion);
				}
				_unityAfter53TextProperty.SetValue(_commandInputBoxTextEditor, _commandInput, null);
			}
		}

		public override void LoseFocus()
		{
			_tabIndex = 0;
		}

		public override void Focus()
		{
		}
	}
}
