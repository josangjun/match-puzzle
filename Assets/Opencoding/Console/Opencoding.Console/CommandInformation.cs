using System;
using System.Collections.Generic;
using System.Linq;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console
{
	internal class CommandInformation
	{
		private class ParameterEntry
		{
			public List<KeyValuePair<GUIContent, GUIStyle>> _Items = new List<KeyValuePair<GUIContent, GUIStyle>>();

			public float _Width;

			public void Add(string text, GUIStyle style)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				GUIContent val = new GUIContent(text);
				_Items.Add(new KeyValuePair<GUIContent, GUIStyle>(val, style));
				_Width += style.CalcSize(val).x;
			}
		}

		private readonly InputField _inputField;

		private readonly List<ParameterEntry> _parameterEntries = new List<ParameterEntry>();

		private CommandHandler _lastCommand;

		private int _lastCurrentParameterIndex;

		private DebugConsole _debugConsole;

		public CommandInformation(DebugConsole debugConsole, InputField inputField)
		{
			_debugConsole = debugConsole;
			_inputField = inputField;
		}

		public Vector2 OnGUI()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			Rect textFieldRect = _inputField.TextFieldRect;
			float xMin = textFieldRect.xMin;
			float consoleTop = _debugConsole.ConsoleTop;
			textFieldRect = _inputField.TextFieldRect;
			Vector2 topLeft = DrawDescription(new Vector2(xMin, consoleTop + textFieldRect.yMax + 10f));
			Vector2 val = DrawParameterList(topLeft);
			textFieldRect = _inputField.TextFieldRect;
			return new Vector2(textFieldRect.xMin, val.y);
		}

		private Vector2 DrawDescription(Vector2 topLeft)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(_inputField.Input.Trim()))
			{
				return topLeft;
			}
			CommandHandler commandHandler = CommandHandlers.GetCommandHandler(_inputField.Input);
			if (commandHandler == null || string.IsNullOrEmpty(commandHandler.Description))
			{
				return topLeft;
			}
			GUIContent val = new GUIContent(commandHandler.Description);
			float num = GUIStyles.CommandDescriptionBackgroundStyle.CalcHeight(val, _debugConsole.ActualConsoleWidth - topLeft.x * 2f);
			Rect val2 = new Rect(topLeft.x, topLeft.y, _debugConsole.ActualConsoleWidth - topLeft.x * 2f, num + 5f);
			GUI.Label(val2, val, GUIStyles.CommandDescriptionBackgroundStyle);
			return new Vector2(topLeft.x, val2.yMax + 5f);
		}

		private Vector2 DrawParameterList(Vector2 topLeft)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			if (_inputField.Input == "")
			{
				return topLeft;
			}
			CommandHandler commandHandler = CommandHandlers.GetCommandHandler(_inputField.Input);
			if (commandHandler == null || commandHandler.Parameters.Length == 0)
			{
				return topLeft;
			}
			if (commandHandler is MethodCommandHandler)
			{
				float num = DrawMethodCommandParameterList(topLeft, commandHandler);
				return new Vector2(topLeft.x, num + 16f * GUIStyles.ScaleFactor);
			}
			if (commandHandler is PropertyCommandHandler)
			{
				float num2 = DrawPropertyCommandParameterList(topLeft, commandHandler);
				return new Vector2(topLeft.x, num2 + 16f * GUIStyles.ScaleFactor);
			}
			throw new InvalidOperationException("Unrecoginised command type");
		}

		private float DrawPropertyCommandParameterList(Vector2 topLeft, CommandHandler exactlyMatchingCommand)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			float num = GUIStyles.CommandInformationParameterNormalStyle.CalcSize(new GUIContent("]")).y * 1.2f;
			string text = $"<b>{Opencoding.CommandHandlerSystem.Utils.GetFriendlyTypeName(exactlyMatchingCommand.Parameters[0].Type)}</b>  Current value: {exactlyMatchingCommand.Parameters[0].DefaultValue}";
			Vector2 val = GUIStyles.CommandInformationParameterNormalStyle.CalcSize(new GUIContent(text));
			GUI.Label(new Rect(topLeft.x, topLeft.y, val.x, num), text, GUIStyles.CommandInformationParameterNormalStyle);
			return topLeft.y + num;
		}

		private float DrawMethodCommandParameterList(Vector2 topLeft, CommandHandler exactlyMatchingCommand)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			RegenerateMethodCommandParameterList(exactlyMatchingCommand);
			float num = GUIStyles.CommandInformationParameterNormalStyle.CalcSize(new GUIContent("]")).y * 1.2f;
			float x = GUIStyles.CommandInformationParameterNormalStyle.CalcSize(new GUIContent("_")).x;
			float num2 = _debugConsole.ActualConsoleWidth - 80f * GUIStyles.ScaleFactor;
			Rect? keyboardArea = TouchScreenKeyboardManager.KeyboardArea;
			if (keyboardArea.HasValue)
			{
				Rect val = keyboardArea.Value;
				if (val.height > 0f)
				{
					float num3 = num2;
					val = TouchScreenKeyboardManager.GetTouchScreenKeyboardCloseButtonRect();
					num2 = num3 - val.width;
				}
			}
			float num4 = 0f;
			int num5 = 0;
			float num6 = topLeft.y;
			foreach (ParameterEntry parameterEntry in _parameterEntries)
			{
				if (topLeft.x + num4 + parameterEntry._Width + x > num2 && num6 != topLeft.y)
				{
					num4 = 0f;
					num6 += num;
				}
				else if (num5 != 0)
				{
					GUI.Label(new Rect(topLeft.x + num4, num6, x, num), " ", GUIStyles.CommandInformationParameterNormalStyle);
					num4 += x;
				}
				foreach (KeyValuePair<GUIContent, GUIStyle> item in parameterEntry._Items)
				{
					float x2 = item.Value.CalcSize(item.Key).x;
					GUI.Label(new Rect(topLeft.x + num4, num6, x2, num), item.Key, item.Value);
					num4 += x2;
				}
				num5++;
			}
			return num6 + num;
		}

		private void RegenerateMethodCommandParameterList(CommandHandler commandHandler)
		{
			if (_lastCommand == commandHandler && _lastCurrentParameterIndex == _inputField.CurrentParameterIndex)
			{
				return;
			}
			_lastCommand = commandHandler;
			_lastCurrentParameterIndex = _inputField.CurrentParameterIndex;
			_parameterEntries.Clear();
			ParamInfo[] array = commandHandler.Parameters.ToArray();
			bool flag = false;
			int num = 0;
			ParamInfo[] array2 = array;
			foreach (ParamInfo paramInfo in array2)
			{
				ParameterEntry parameterEntry = new ParameterEntry();
				_parameterEntries.Add(parameterEntry);
				string arg = "";
				if (paramInfo.IsOptional && !flag)
				{
					flag = true;
					parameterEntry.Add("[", GUIStyles.CommandInformationParameterNormalStyle);
				}
				if (paramInfo.IsOptional)
				{
					arg = ((paramInfo.DefaultValue == null && (object)paramInfo.Type == typeof(string)) ? " = \"\"" : (" = " + paramInfo.DefaultValue));
				}
				GUIStyle style = ((num == _inputField.CurrentParameterIndex || (_inputField.CurrentParameterIndex >= array.Length && paramInfo.IsParamArray)) ? GUIStyles.CommandInformationParameterHighlightedStyle : GUIStyles.CommandInformationParameterNormalStyle);
				string text = $"{Opencoding.CommandHandlerSystem.Utils.GetFriendlyTypeName(paramInfo.Type)} <b>{paramInfo.Name}</b>{arg}";
				parameterEntry.Add(text, style);
				num++;
			}
			if (flag)
			{
				_parameterEntries.Last().Add("]", GUIStyles.CommandInformationParameterNormalStyle);
			}
		}
	}
}
