using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console
{
	internal class TouchScreenInputField : InputField
	{
		private TouchScreenKeyboardInput _touchScreenKeyboardInput = new TouchScreenKeyboardInput();

		private Rect _commandInputRect;

		private readonly InputHistory _inputHistory;

		private readonly LogHistoryView _logHistoryView;

		private readonly HelpOverlay _helpOverlay;

		public override string Input
		{
			get
			{
				return _touchScreenKeyboardInput.Text;
			}
			set
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				_touchScreenKeyboardInput.Text = value;
				if ((int)Application.platform != 11)
				{
					ShowInputForConsole();
				}
			}
		}

		public override bool HasFocus => TouchScreenKeyboardManager.IsActive(_touchScreenKeyboardInput);

		public override Rect TextFieldRect => _commandInputRect;

		public TouchScreenInputField(InputHistory inputHistory, LogHistoryView logHistoryView, HelpOverlay helpOverlay)
		{
			_inputHistory = inputHistory;
			_logHistoryView = logHistoryView;
			_helpOverlay = helpOverlay;
		}

		public override void OnGUI(Rect containingRect)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Invalid comparison between Unknown and I4
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Invalid comparison between Unknown and I4
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginHorizontal(GUIStyles.HeaderStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandWidth(true),
				GUILayout.Height(GUIStyles.HeaderHeight)
			});
			GUI.SetNextControlName("Input");
			Widgets.TouchFriendlyTextField(_touchScreenKeyboardInput, GUIStyles.InputTextFieldStyle, GUILayout.ExpandHeight(true));
			_touchScreenKeyboardInput.Text = _touchScreenKeyboardInput.Text.TrimStart(new char[1] { ' ' });
			if ((int)Event.current.type == 7)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				_commandInputRect = new Rect((containingRect).x + (lastRect).x, (containingRect).y + (lastRect).y, (lastRect).width, (lastRect).height);
			}
			if (!TouchScreenKeyboardManager.IsActive(_touchScreenKeyboardInput))
			{
				if ((int)Event.current.type == 0 && (_commandInputRect).Contains(Event.current.mousePosition))
				{
					ShowInputForConsole();
				}
			}
			else if (_touchScreenKeyboardInput.IsDone)
			{
				ExecuteInput();
			}
			else if (_touchScreenKeyboardInput.WasCancelled)
			{
				LoseFocus();
			}
			if (Widgets.HeaderButton(DebugConsole.Instance.ImageFiles._RunIcon, DebugConsole.Instance.ImageFiles._BackgroundGradient, ""))
			{
				ExecuteInput();
			}
			GUI.enabled=Input != "";
			GUILayout.Space(4f * GUIStyles.ScaleFactor);
			if (Widgets.HeaderButton(DebugConsole.Instance.ImageFiles._ClearIcon, DebugConsole.Instance.ImageFiles._BackgroundGradient, ""))
			{
				Input = "";
			}
			GUI.enabled=true;
			bool flag = Widgets.ToggleHeaderButton(DebugConsole.Instance.ImageFiles._HelpIcon, "", _helpOverlay.IsVisible);
			if (flag && !_helpOverlay.IsVisible)
			{
				TouchScreenKeyboardManager.Deactivate();
				DebugConsole.Instance.Minimize();
			}
			_helpOverlay.IsVisible = flag;
			if ((int)Event.current.type == 7)
			{
				base.HelpButtonRect = GUILayoutUtility.GetLastRect();
			}
			GUILayout.EndHorizontal();
			ConfirmInput();
		}

		private void ExecuteInput()
		{
			LoseFocus();
			_inputHistory.RecordInput(_touchScreenKeyboardInput.Text);
			CommandHandlers.HandleCommand(_touchScreenKeyboardInput.Text);
			_touchScreenKeyboardInput.Text = "";
			_logHistoryView.AutoScrolling = true;
		}

		public override void LoseFocus()
		{
			TouchScreenKeyboardManager.Deactivate(_touchScreenKeyboardInput);
		}

		public override void ClearInput()
		{
			_touchScreenKeyboardInput.Text = "";
		}

		public override void Focus()
		{
			ShowInputForConsole();
		}

		private void ShowInputForConsole()
		{
			TouchScreenKeyboardManager.Activate(_touchScreenKeyboardInput);
			DebugConsole.Instance.HidePopups();
		}
	}
}
