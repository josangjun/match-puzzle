using System.Collections.Generic;
using System.Linq;
using Opencoding.CommandHandlerSystem;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal class Suggestions
	{
		private class SuggestionButton
		{
			private string _input;

			private bool _automaticallyExecute;

			private readonly InputField _inputField;

			private GUIContent _guiContent;

			private float _width;

			public float Width
			{
				get
				{
					//IL_0019: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Invalid comparison between Unknown and I4
					//IL_002d: Unknown result type (might be due to invalid IL or missing references)
					if (_width != 0f)
					{
						return _width;
					}
					if ((int)Event.current.type == 8)
					{
						_width = GUIStyles.SuggestionButtonBackgroundStyle.CalcSize(_guiContent).x;
					}
					return _width;
				}
			}

			public GUIContent Content => _guiContent;

			public string Input => _input;

			public SuggestionButton(string label, string input, bool automaticallyExecute, InputField inputField)
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Expected O, but got Unknown
				_input = input + (automaticallyExecute ? "" : " ");
				_automaticallyExecute = automaticallyExecute;
				_inputField = inputField;
				_guiContent = new GUIContent(label + (automaticallyExecute ? "" : "..."));
			}

			public void Pressed()
			{
				if (_automaticallyExecute)
				{
					CommandHandlers.HandleCommand(Input);
					_inputField.ClearInput();
					_inputField.ConfirmInput();
				}
				else
				{
					_inputField.Input = Input;
					_inputField.ConfirmInput();
				}
				Event.current.Use();
			}
		}

		private List<SuggestionButton> _suggestionButtons = new List<SuggestionButton>();

		private InputField _inputField;

		private string _lastInput = "";

		private bool _firstRun = true;

		public Suggestions(InputField inputField)
		{
			_inputField = inputField;
		}

		public void Update(bool forceReLayout = false)
		{
			if (_firstRun || _inputField.LastTypedInput != _lastInput)
			{
				_firstRun = false;
				_lastInput = _inputField.LastTypedInput;
				GenerateSuggestionButtons();
			}
		}

		public void ForceRelayoutNextUpdate()
		{
			_firstRun = true;
		}

		private void GenerateSuggestionButtons()
		{
			_suggestionButtons.Clear();
			if (_lastInput.Contains(" ") && _lastInput.Trim().Length != 0)
			{
				GenerateParameterSuggestionButtons();
			}
			else if (_lastInput == "")
			{
				GenerateRecentCommandButtons();
			}
			else if (_inputField.LastTypedParameters.Length != 0)
			{
				GeneratePossibleCommandButtons();
			}
		}

		private void GeneratePossibleCommandButtons()
		{
			CommandHandler[] array = CommandHandlers.FindMatchingCommands(_inputField.LastTypedParameters[0]).ToArray();
			foreach (CommandHandler commandHandler in array)
			{
				bool flag = commandHandler.Parameters.Length != 0;
				_suggestionButtons.Add(new SuggestionButton(commandHandler.CommandName, commandHandler.CommandName, !flag, _inputField));
			}
		}

		private void GenerateParameterSuggestionButtons()
		{
			string[] lastTypedParameters = _inputField.LastTypedParameters;
			CommandHandler commandHandler = CommandHandlers.GetCommandHandler(lastTypedParameters.First());
			if (commandHandler == null)
			{
				return;
			}
			ParamInfo[] array = commandHandler.Parameters.ToArray();
			int num = _inputField.CurrentParameterIndex;
			bool lastParameterIsParamsArray = array.Length != 0 && array[^1].IsParamArray;
			if (num >= array.Length)
			{
				if (!lastParameterIsParamsArray)
				{
					return;
				}
				num = array.Length - 1;
			}
			string parameterValue = ((num + 1 < lastTypedParameters.Length) ? lastTypedParameters[num + 1] : "");
			List<string> parameterPossibleValues = commandHandler.Parameters[num].GetParameterPossibleValues(parameterValue);
			bool isLastParameter = num == array.Length - 1;
			string commandUpToLastParameter = string.Join(" ", lastTypedParameters.SubArray(0, num + 1));
			_suggestionButtons.AddRange(parameterPossibleValues.Select((string x) => new SuggestionButton(x, commandUpToLastParameter + " " + Opencoding.CommandHandlerSystem.Utils.WrapInQuotesIfNecessary(x), isLastParameter && !lastParameterIsParamsArray, _inputField)));
		}

		private void GenerateRecentCommandButtons()
		{
			if (!DebugConsole.Instance.Settings.ShowRecentCommands)
			{
				return;
			}
			foreach (string recentCommand in DebugConsole.Instance.RecentCommands)
			{
				CommandHandler commandHandler = CommandHandlers.GetCommandHandler(recentCommand);
				if (commandHandler != null)
				{
					bool flag = commandHandler.Parameters.Length != 0;
					_suggestionButtons.Add(new SuggestionButton(commandHandler.CommandName, commandHandler.CommandName, !flag, _inputField));
				}
			}
		}

		public void Draw(Rect rect)
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			float num = GUIStyles.SuggestionButtonBackgroundStyle.margin.left;
			float num2 = (rect).x;
			float num3 = (rect).y;
			int num4 = Mathf.FloorToInt((rect).height / (GUIStyles.SuggestionButtonHeight + num)) - 1;
			if (num4 < 0)
			{
				return;
			}
			int num5 = _suggestionButtons.Count;
			Rect val2 = default(Rect);
			Rect val3 = default(Rect);
			foreach (SuggestionButton suggestionButton in _suggestionButtons)
			{
				if (num4 == 0)
				{
					GUIContent val = UIUtilities.TempGUIContent(num5 + " more...");
					float x = GUIStyles.SuggestionButtonMoreBackgroundStyle.CalcSize(val).x;
					if (num2 + suggestionButton.Width + x > (rect).width - 20f)
					{
						val2= new Rect(num2, num3, x, GUIStyles.SuggestionButtonHeight);
						GUI.Label(val2, val, GUIStyles.SuggestionButtonMoreBackgroundStyle);
						if ((int)Event.current.type == 0 && val2.Contains(Event.current.mousePosition))
						{
							TouchScreenKeyboardManager.Deactivate();
						}
						break;
					}
				}
				if (num2 + suggestionButton.Width > (rect).width - 20f)
				{
					num4--;
					num3 += GUIStyles.SuggestionButtonHeight + num;
					num2 = (rect).x;
				}
				val3 = new Rect(num2, num3, suggestionButton.Width, GUIStyles.SuggestionButtonHeight);
				num2 += suggestionButton.Width + num;
				GUI.Label(val3, suggestionButton.Content, GUIStyles.SuggestionButtonBackgroundStyle);
				if ((int)Event.current.type == 0 && val3.Contains(Event.current.mousePosition))
				{
					suggestionButton.Pressed();
				}
				num5--;
			}
		}
	}
}
