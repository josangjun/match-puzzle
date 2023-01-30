using System;
using System.Collections.Generic;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console
{
	internal class HelpOverlay : IDisposable
	{
		private Vector2 _scrollPosition = Vector2.zero;

		private List<CommandHandler> _sortedCommandList = new List<CommandHandler>();

		private DebugConsole _debugConsole;

		public bool IsVisible { get; set; }

		public HelpOverlay(DebugConsole debugConsole)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_debugConsole = debugConsole;
			CommandHandlers.CommandHandlerAdded += OnCommandHandlerAdded;
			CommandHandlers.CommandHandlerRemoved += OnCommandHandlerRemoved;
			_sortedCommandList.AddRange(CommandHandlers.Handlers);
		}

		public void OnGUI(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginArea(rect);
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyles.HelpWindowBackgroundStyle);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Label("This is a complete list of commands you can use:", GUIStyles.HelpWindowHelpStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			float num = 0f;
			float num2 = (rect).width - 20f - (float)GUIStyles.HelpWindowBackgroundStyle.padding.left - (float)GUIStyles.HelpWindowBackgroundStyle.padding.right;
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			foreach (CommandHandler sortedCommand in _sortedCommandList)
			{
				string text = sortedCommand.CommandName + ((sortedCommand.Parameters.Length != 0) ? "..." : "");
				float x = GUIStyles.SuggestionButtonBackgroundStyle.CalcSize(new GUIContent(text)).x;
				if (num + x > num2 && num > 0f)
				{
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
					num = 0f;
				}
				num += x + (float)GUIStyles.SuggestionButtonBackgroundStyle.margin.left + (float)GUIStyles.SuggestionButtonBackgroundStyle.margin.right;
				GUILayout.Label(text, GUIStyles.SuggestionButtonBackgroundStyle, (GUILayoutOption[])(object)new GUILayoutOption[2]
				{
					GUILayout.ExpandWidth(false),
					GUILayout.Height(GUIStyles.SuggestionButtonHeight)
				});
				if ((int)Event.current.type == 0)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					if ((lastRect).Contains(Event.current.mousePosition))
					{
						HandleButtonPress(sortedCommand);
					}
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void HandleButtonPress(CommandHandler commandHandler)
		{
			if (commandHandler.Parameters.Length == 0)
			{
				CommandHandlers.HandleCommand(commandHandler.CommandName);
			}
			else
			{
				_debugConsole.SetCommandInputTextAsIfTyped(commandHandler.CommandName + ((commandHandler.Parameters.Length != 0) ? " " : ""));
			}
			Event.current.Use();
			IsVisible = false;
		}

		private void ResortRecentCommandList()
		{
			_sortedCommandList.Sort((CommandHandler a, CommandHandler b) => a.CommandName.ToLower().CompareTo(b.CommandName.ToLower()));
		}

		private void OnCommandHandlerAdded(CommandHandler commandHandler)
		{
			_sortedCommandList.Add(commandHandler);
			ResortRecentCommandList();
		}

		private void OnCommandHandlerRemoved(CommandHandler commandHandler)
		{
			_sortedCommandList.Remove(commandHandler);
			ResortRecentCommandList();
		}

		public void Dispose()
		{
			CommandHandlers.CommandHandlerAdded -= OnCommandHandlerAdded;
			CommandHandlers.CommandHandlerRemoved -= OnCommandHandlerRemoved;
		}
	}
}
