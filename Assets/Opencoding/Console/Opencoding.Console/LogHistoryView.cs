using System;
using System.Text.RegularExpressions;
using Opencoding.LogHistory;
using Opencoding.Shared.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Opencoding.Console
{
	internal class LogHistoryView : IDisposable
	{
		private bool _autoScrolling = true;

		private readonly DebugConsole _debugConsole;

		private readonly FilteredLogHistoryViewModel _viewModel;

		private readonly LogSearch _logSearch;

		private readonly LogItemPopupMenu _logItemPopupMenu;

		private int _expandedLogHistoryItemIndex = -1;

		private int _expandedItemHeight;

		private string _expandedItemStacktrace;

		private bool _isDragging;

		private float _totalDragDelta;

		private Vector2 _scrollPosition = Vector2.zero;

		private float _listHeight;

		private float _scollingViewTopInWindowCoords;

		private Vector2 _clickDownPosition;

		private float _clickDownTime = float.PositiveInfinity;

		private int PADDING_BETWEEN_MESSAGE_AND_STACKTRACE = 12;

		private static Regex _stackTraceFormattingRegex;

		public LogHistoryItem ItemInMiddleOfScreen { get; set; }

		public bool AutoScrolling
		{
			get
			{
				if (_debugConsole.IsPopupMenuOpen || _isDragging)
				{
					return false;
				}
				return _autoScrolling;
			}
			set
			{
				_autoScrolling = value;
			}
		}

		public LogHistoryView(FilteredLogHistoryViewModel viewModel, LogItemPopupMenu logItemPopupMenu, DebugConsole debugConsole, LogSearch logSearch)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			_logItemPopupMenu = logItemPopupMenu;
			_viewModel = viewModel;
			_debugConsole = debugConsole;
			_logSearch = logSearch;
			_viewModel.LogHistoryListReset += CollapseExpandedItem;
			_viewModel.LogHistoryItemRemoved += DecrementExpandedItemIndex;
		}

		private void DecrementExpandedItemIndex(LogHistoryItem obj)
		{
			if (_expandedLogHistoryItemIndex >= 0)
			{
				_expandedLogHistoryItemIndex--;
			}
		}

		private void CollapseExpandedItem()
		{
			_expandedLogHistoryItemIndex = -1;
		}

		private bool isPressed;
		public void Update()
		{
			if (Mouse.current != null)
			{
				if (isPressed)
				{
					if (!Mouse.current.leftButton.isPressed)
					{
						isPressed = false;
						_clickDownTime = float.PositiveInfinity;
					}
				} else
				{
					if (Mouse.current.leftButton.isPressed)
					{
						isPressed = true;
					}
				}
			}
		}

		public void OnGUI(bool inFocus)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Invalid comparison between Unknown and I4
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Invalid comparison between Unknown and I4
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			if (AutoScrolling && (int)Event.current.type != 6)
			{
				_scrollPosition = new Vector2(0f, float.PositiveInfinity);
			}
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[2]
			{
				GUILayout.ExpandHeight(true),
				GUILayout.Width(_debugConsole.ActualConsoleWidth)
			});
			DrawConsoleItemList(inFocus);
			GUILayout.EndScrollView();
			Rect lastRect = GUILayoutUtility.GetLastRect();
			float height = (lastRect).height;
			if ((int)Event.current.type == 7)
			{
				_listHeight = height;
				_scollingViewTopInWindowCoords = (lastRect).y;
			}
			if ((int)Application.platform == 11 || (int)Application.platform == 8)
			{
				HandleTouchDrag(lastRect);
			}
			if (!_isDragging)
			{
				_autoScrolling = _scrollPosition.y >= (float)CalculateListHeight() - height;
			}
		}

		private void HandleTouchDrag(Rect listRect)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Invalid comparison between Unknown and I4
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Invalid comparison between Unknown and I4
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			if (!Event.current.isMouse || (!listRect.Contains(Event.current.mousePosition) && !_isDragging))
			{
				return;
			}
			EventType type = Event.current.type;
			if ((int)type != 1)
			{
				if ((int)type != 3)
				{
					return;
				}
				_totalDragDelta += Event.current.delta.y;
				if (!_isDragging && Mathf.Abs(_totalDragDelta) > 25f * GUIStyles.ScaleFactor)
				{
					_isDragging = true;
					AutoScrolling = false;
				}
				if (_isDragging)
				{
					_scrollPosition += new Vector2(0f, Event.current.delta.y);
					if (_scrollPosition.y < 0f)
					{
						_scrollPosition.y = 0f;
					}
				}
			}
			else
			{
				_totalDragDelta = 0f;
				_isDragging = false;
			}
		}

		public void JumpToCurrentSearchResult()
		{
			ScrollToShowItem(_logSearch.CurrentResult);
		}

		private void ExpandItem(int index, Rect itemRect)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			_expandedLogHistoryItemIndex = index;
			LogHistoryItem logHistoryItem = _viewModel.LogHistoryItems[index];
			Utils.GetImageAndStyleForHistoryItem(logHistoryItem, _debugConsole.ImageFiles, out var style, out var _);
			style.wordWrap = true;
			int num = CalculateRowTextSpaceAvailable(itemRect);
			float num2 = Mathf.Max(style.CalcHeight(new GUIContent(TrimLogMessageIfNecessary(logHistoryItem._LogMessage)), (float)num), (float)GUIStyles.ConsoleRowHeight);
			_expandedItemHeight = (int)num2;
			if (!string.IsNullOrEmpty(logHistoryItem._StackTrace))
			{
				_expandedItemStacktrace = FormatStackTrace(logHistoryItem._StackTrace);
				_expandedItemHeight += PADDING_BETWEEN_MESSAGE_AND_STACKTRACE + (int)GUIStyles.LogHistoryItemTextAreaStyle.CalcHeight(new GUIContent(_expandedItemStacktrace), (float)num) + 8;
			}
			else
			{
				_expandedItemStacktrace = "";
			}
			style.wordWrap = false;
			ScrollToShowItem(logHistoryItem);
		}

		private static string TrimLogMessageIfNecessary(string message)
		{
			if (message.Length < 10000)
			{
				return message;
			}
			return message.Substring(0, 10000) + "\n<trimmed>";
		}

		private string FormatStackTrace(string stackTrace)
		{
			_stackTraceFormattingRegex = new Regex("((?:[a-z][a-z\\.\\d\\-]+):\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return _stackTraceFormattingRegex.Replace(stackTrace, "<b>$1</b>");
		}

		private void ScrollToShowItem(LogHistoryItem item)
		{
			int index = _viewModel.LogHistoryItems.IndexOf(item);
			int itemTop = GetItemTop(index);
			int num = itemTop + GetItemHeight(index);
			float y = _scrollPosition.y;
			float num2 = _scrollPosition.y + _listHeight;
			if ((float)itemTop > y && (float)num < num2)
			{
				return;
			}
			if ((float)itemTop < y)
			{
				_scrollPosition.y = Mathf.Clamp((float)itemTop, 0f, float.PositiveInfinity);
			}
			else
			{
				if (!((float)num > num2))
				{
					return;
				}
				_scrollPosition.y = Mathf.Min(Mathf.Clamp((float)itemTop, 0f, float.PositiveInfinity), Mathf.Clamp((float)num - _listHeight, 0f, float.PositiveInfinity));
			}
			AutoScrolling = false;
		}

		private void DrawConsoleItemList(bool inFocus)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Invalid comparison between Unknown and I4
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			int count = _viewModel.LogHistoryItems.Count;
			int num = CalculateListHeight();
			if (float.IsInfinity(_scrollPosition.y))
			{
				_scrollPosition = new Vector2(0f, (float)num);
			}
			Rect rect = GUILayoutUtility.GetRect(0f, _debugConsole.ActualConsoleWidth, (float)num, (float)num);
			int num2 = (int)(_scrollPosition.y / (float)GUIStyles.ConsoleRowHeight);
			if (_expandedLogHistoryItemIndex != -1 && num2 > _expandedLogHistoryItemIndex)
			{
				num2 = ((!(_scrollPosition.y < (float)(GUIStyles.ConsoleRowHeight * _expandedLogHistoryItemIndex - 1 + _expandedItemHeight))) ? ((int)((_scrollPosition.y - (float)_expandedItemHeight) / (float)GUIStyles.ConsoleRowHeight) + 1) : _expandedLogHistoryItemIndex);
			}
			int num3 = Mathf.Clamp(num2 + Screen.height / GUIStyles.ConsoleRowHeight, 0, count - 1);
			if ((int)Event.current.type == 7)
			{
				if (_viewModel.LogHistoryItems.Count != 0 && num3 > num2)
				{
					ItemInMiddleOfScreen = _viewModel.LogHistoryItems[num2 + (num3 - num2) / 2];
				}
				else
				{
					ItemInMiddleOfScreen = null;
				}
			}
			Rect itemRect = default(Rect);
			for (int i = num2; i <= num3; i++)
			{
				LogHistoryItem historyItem = _viewModel.LogHistoryItems[i];
				itemRect = new Rect(0f, (float)GetItemTop(i), (rect).width, (float)GetItemHeight(i));
				DrawConsoleItem(itemRect, historyItem, i, inFocus);
			}
			GUI.color=Color.white;
		}

		private int CalculateListHeight()
		{
			int num = _viewModel.LogHistoryItems.Count * GUIStyles.ConsoleRowHeight;
			if (_expandedLogHistoryItemIndex != -1)
			{
				num += -GUIStyles.ConsoleRowHeight + _expandedItemHeight;
			}
			return num;
		}

		private int GetItemTop(int index)
		{
			if (_expandedLogHistoryItemIndex == -1)
			{
				return index * GUIStyles.ConsoleRowHeight;
			}
			if (index <= _expandedLogHistoryItemIndex)
			{
				return index * GUIStyles.ConsoleRowHeight;
			}
			return (index - 1) * GUIStyles.ConsoleRowHeight + _expandedItemHeight;
		}

		private int GetItemHeight(int index)
		{
			if (index != _expandedLogHistoryItemIndex)
			{
				return GUIStyles.ConsoleRowHeight;
			}
			return _expandedItemHeight;
		}

		private int CalculateRowTextSpaceAvailable(Rect itemRect)
		{
			return (int)(itemRect.width - (float)(3 * GUIStyles.ConsoleRowTextLeftMargin) - (float)GUIStyles.ConsoleRowHeight);
		}

		private void DrawConsoleItem(Rect itemRect, LogHistoryItem historyItem, int index, bool inFocus)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Invalid comparison between Unknown and I4
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			int controlId = 100000 + index;
			if (itemRect.Contains(Event.current.mousePosition) && !_debugConsole.IsPopupMenuOpen && inFocus)
			{
				EventType type = Event.current.type;
				if ((int)type != 0)
				{
					if ((int)type == 1)
					{
						HandleMouseUpOnItem(itemRect, historyItem, index, controlId);
					}
				}
				else
				{
					HandleMouseDownOnItem(controlId);
				}
			}
			if ((int)Event.current.type != 7)
			{
				return;
			}
			if (_logSearch.IsSearchActive)
			{
				if (_logSearch.CurrentResult == historyItem)
				{
					GUIStyles.SelectedSearchItemBackgroundStyle.Draw(itemRect, false, false, false, false);
				}
				else if (_logSearch.IsLogHistoryItemASearchResult(historyItem))
				{
					GUIStyles.SearchResultItemBackgroundStyle.Draw(itemRect, false, false, false, false);
				}
			}
			if (_logItemPopupMenu.IsVisible && _logItemPopupMenu.TargetLogHistoryItem == historyItem)
			{
				GUIStyles.HighlightedItemBackgroundStyle.Draw(itemRect, false, false, false, false);
			}
			else if (index % 2 == 0)
			{
				GUIStyles.ItemAlternateBackgroundStyle.Draw(itemRect, false, false, false, false);
			}
			if (_expandedLogHistoryItemIndex == index)
			{
				DrawExpandedItemContent(itemRect, historyItem);
			}
			else
			{
				DrawCollapsedItemContent(itemRect, historyItem);
			}
		}

		private void HandleMouseDownOnItem(int controlId)
		{
			GUIUtility.hotControl=controlId;
			HandleMobileTouchBeginOnItem();
		}

		private void HandleMobileTouchBeginOnItem()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (!PlatformUtils.IsDesktop)
			{
				_clickDownTime = Time.realtimeSinceStartup;
				_clickDownPosition = Event.current.mousePosition;
			}
		}

		private void HandleMouseUpOnItem(Rect itemRect, LogHistoryItem historyItem, int index, int controlId)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			if (_isDragging || GUIUtility.hotControl != controlId)
			{
				return;
			}
			if (Event.current.button == 0)
			{
				if (historyItem._Type == LogHistoryLogType.ConsoleInput)
				{
					_debugConsole.SetCommandInputTextAsIfTyped(historyItem._LogMessage);
					CollapseExpandedItem();
					_clickDownTime = float.PositiveInfinity;
				}
				else if (Time.realtimeSinceStartup - _clickDownTime > 0.3f && Vector2.Distance(_clickDownPosition, Event.current.mousePosition) < 40f * GUIStyles.ScaleFactor)
				{
					ShowItemPopup(historyItem);
					_clickDownTime = float.PositiveInfinity;
				}
				else if (_expandedLogHistoryItemIndex == index)
				{
					CollapseExpandedItem();
				}
				else
				{
					ExpandItem(index, itemRect);
				}
			}
			else if (Event.current.button == 1)
			{
				ShowItemPopup(historyItem);
			}
			Event.current.Use();
		}

		private void ShowItemPopup(LogHistoryItem historyItem)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Vector2 position = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y - _scrollPosition.y + _scollingViewTopInWindowCoords);
			_logItemPopupMenu.Show(position, historyItem);
		}

		private void DrawCollapsedItemContent(Rect itemRect, LogHistoryItem historyItem)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			Utils.GetImageAndStyleForHistoryItem(historyItem, _debugConsole.ImageFiles, out var style, out var image);
			int num = CalculateRowTextSpaceAvailable(itemRect);
			GUI.Label(new Rect(itemRect.x + (float)(2 * GUIStyles.ConsoleRowTextLeftMargin) + (float)GUIStyles.ConsoleRowHeight, itemRect.y, (float)num, (float)GUIStyles.ConsoleRowHeight), TrimLogMessageIfNecessary(historyItem._FirstLineOfLogMessage), style);
			float num2 = (float)GUIStyles.ConsoleRowHeight * 0.8f;
			float num3 = ((float)GUIStyles.ConsoleRowHeight - num2) / 2f;
			GUI.DrawTexture(new Rect(itemRect.x + (float)GUIStyles.ConsoleRowTextLeftMargin, itemRect.y + num3, num2, num2), (Texture)(object)image, (ScaleMode)2);
		}

		private void DrawExpandedItemContent(Rect itemRect, LogHistoryItem historyItem)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			Utils.GetImageAndStyleForHistoryItem(historyItem, _debugConsole.ImageFiles, out var style, out var image);
			GUIContent val = new GUIContent(TrimLogMessageIfNecessary(historyItem._LogMessage));
			int num = CalculateRowTextSpaceAvailable(itemRect);
			style.wordWrap=true;
			float num2 = Mathf.Max(style.CalcHeight(val, (float)num), (float)GUIStyles.ConsoleRowHeight);
			GUI.Label(new Rect(itemRect.x + (float)(2 * GUIStyles.ConsoleRowTextLeftMargin) + (float)GUIStyles.ConsoleRowHeight, itemRect.y, (float)num, num2), val, style);
			style.wordWrap=false;
			float num3 = (float)GUIStyles.ConsoleRowHeight * 0.8f;
			float num4 = ((float)GUIStyles.ConsoleRowHeight - num3) / 2f;
			GUI.DrawTexture(new Rect(itemRect.x + (float)GUIStyles.ConsoleRowTextLeftMargin, itemRect.y + num4, num3, num3), (Texture)(object)image, (ScaleMode)2);
			if (!string.IsNullOrEmpty(_expandedItemStacktrace))
			{
				GUI.Label(new Rect(itemRect.x + (float)(2 * GUIStyles.ConsoleRowTextLeftMargin) + (float)GUIStyles.ConsoleRowHeight, itemRect.y + num2 + (float)PADDING_BETWEEN_MESSAGE_AND_STACKTRACE, (float)num, itemRect.height - num2 - 4f), _expandedItemStacktrace, GUIStyles.LogHistoryItemTextAreaStyle);
			}
		}

		public void Dispose()
		{
			_viewModel.LogHistoryListReset -= CollapseExpandedItem;
			_viewModel.LogHistoryItemRemoved -= DecrementExpandedItemIndex;
		}
	}
}
