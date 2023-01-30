using UnityEngine;

namespace Opencoding.Console
{
	internal class FilterBar
	{
		private bool _isVisible;

		private bool _justMadeVisible;

		private readonly LogHistoryView _logHistoryView;

		private readonly FilteredLogHistoryViewModel _filteredLogHistoryViewModel;

		private readonly TouchScreenKeyboardInput _filterInputField = new TouchScreenKeyboardInput();

		private readonly TouchScreenKeyboardInput _searchInputField = new TouchScreenKeyboardInput();

		private readonly LogSearch _logSearch;

		private bool _hideNextFrame;

		private int MINIMUM_WIDTH_FOR_SINGLE_LINE = 1800;

		private DebugConsole _debugConsole;

		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				if (value != _isVisible)
				{
					_isVisible = value;
					if (_isVisible)
					{
						_logSearch.Enabled = true;
						_justMadeVisible = true;
						_filterInputField.IsDone = false;
						_filterInputField.WasCancelled = false;
						_searchInputField.IsDone = false;
						_searchInputField.WasCancelled = false;
					}
					else
					{
						_logSearch.Enabled = false;
						TouchScreenKeyboardManager.Deactivate(_filterInputField);
						TouchScreenKeyboardManager.Deactivate(_searchInputField);
					}
				}
			}
		}

		public float Height
		{
			get
			{
				float num = (float)MINIMUM_WIDTH_FOR_SINGLE_LINE * GUIStyles.ScaleFactor;
				if (!(_debugConsole.ActualConsoleWidth >= num))
				{
					return GUIStyles.HeaderHeight * 2f + 1f;
				}
				return GUIStyles.HeaderHeight;
			}
		}

		public FilterBar(DebugConsole debugConsole, FilteredLogHistoryViewModel filteredLogHistoryViewModel, LogSearch logSearch, LogHistoryView logHistoryView)
		{
			_debugConsole = debugConsole;
			_filteredLogHistoryViewModel = filteredLogHistoryViewModel;
			_logSearch = logSearch;
			_logHistoryView = logHistoryView;
		}

		public void OnGUI()
		{
			if (!IsVisible)
			{
				return;
			}
			float num = (float)MINIMUM_WIDTH_FOR_SINGLE_LINE * GUIStyles.ScaleFactor;
			if (_debugConsole.ActualConsoleWidth >= num)
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.HeaderHeight) });
			}
			GUILayout.BeginHorizontal(GUIStyles.SearchFilterToolBarStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.HeaderHeight) });
			DrawFilterLine();
			GUILayout.EndHorizontal();
			if (_debugConsole.ActualConsoleWidth >= num)
			{
				Widgets.VerticalSeparator();
			}
			GUILayout.BeginHorizontal(GUIStyles.SearchFilterToolBarStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(GUIStyles.HeaderHeight) });
			DrawSearchLine();
			GUILayout.EndHorizontal();
			if (_debugConsole.ActualConsoleWidth >= num)
			{
				GUILayout.EndHorizontal();
			}
			Widgets.HorizontalSeparator();
			if (_justMadeVisible)
			{
				_justMadeVisible = false;
				if ((int)Application.platform != 11)
				{
					GUI.FocusControl("FilterTextField");
					TouchScreenKeyboardManager.Activate(_filterInputField);
				}
			}
		}

		public void Update()
		{
			if (_hideNextFrame)
			{
				_hideNextFrame = false;
				IsVisible = false;
			}
		}

		private void DrawFilterLine()
		{
			GUILayout.Label("Filter:", GUIStyles.HeaderButtonLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f * GUIStyles.ScaleFactor) });
			GUI.SetNextControlName("FilterTextField");
			TouchFriendlyTextField(_filterInputField, GUIStyles.FilterTextFieldStyle);
			bool flag = Widgets.Checkbox(_filteredLogHistoryViewModel.OnlyMatchStringsThatStartWithFilterString, "At start");
			if (_filteredLogHistoryViewModel.FilterString != _filterInputField.Text || flag != _filteredLogHistoryViewModel.OnlyMatchStringsThatStartWithFilterString)
			{
				_filteredLogHistoryViewModel.OnlyMatchStringsThatStartWithFilterString = flag;
				_filteredLogHistoryViewModel.FilterString = _filterInputField.Text;
				if (_logSearch.IsSearchActive)
				{
					_logSearch.RunSearchFromScratch();
				}
				SelectCurrentSearchResultBasedOnLogScroll();
			}
		}

		private void DrawSearchLine()
		{
			GUILayout.Label("Search:", GUIStyles.HeaderButtonLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f * GUIStyles.ScaleFactor) });
			int num = ((_logSearch.Results.Count != 0) ? (_logSearch.CurrentResultIndex + 1) : 0);
			GUIContent val = new GUIContent($"{num} of {_logSearch.Results.Count}");
			Vector2 val2 = GUIStyles.SearchTextFieldCountStyle.CalcSize(val);
			GUIStyles.SearchTextFieldStyle.padding=(new RectOffset((int)(10f * GUIStyles.ScaleFactor), (int)val2.x + (int)(30f * GUIStyles.ScaleFactor), 2, 2));
			TouchFriendlyTextField(_searchInputField, GUIStyles.SearchTextFieldStyle);
			if (_logSearch.SearchString != _searchInputField.Text)
			{
				_logSearch.SearchString = _searchInputField.Text;
				if (_logSearch.SearchString != "")
				{
					SelectCurrentSearchResultBasedOnLogScroll();
					_logHistoryView.JumpToCurrentSearchResult();
				}
			}
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (_logSearch.IsSearchActive)
			{
				GUI.Label(new Rect((lastRect).x, (lastRect).y, (lastRect).width - 20f * GUIStyles.ScaleFactor, (lastRect).height), val, GUIStyles.SearchTextFieldCountStyle);
			}
			GUILayout.Space(4f * GUIStyles.ScaleFactor);
			GUI.enabled=_logSearch.IsSearchActive;
			if (Widgets.Button(DebugConsole.Instance.ImageFiles._PreviousHistoryItemIcon, null, "", (int)(GUIStyles.HeaderHeight * 0.75f)))
			{
				_logSearch.PreviousSearchResult();
				_logHistoryView.JumpToCurrentSearchResult();
			}
			GUILayout.Space(4f * GUIStyles.ScaleFactor);
			if (Widgets.Button(DebugConsole.Instance.ImageFiles._NextHistoryItemIcon, null, "", (int)(GUIStyles.HeaderHeight * 0.75f)))
			{
				_logSearch.NextSearchResult();
				_logHistoryView.JumpToCurrentSearchResult();
			}
			GUI.enabled=true;
		}

		private void SelectCurrentSearchResultBasedOnLogScroll()
		{
			if (_logSearch.Results.Count != 0 && _logHistoryView.ItemInMiddleOfScreen != null)
			{
				int num = _logSearch.Results.BinarySearch(_logHistoryView.ItemInMiddleOfScreen, new NearestMatchFinder());
				if (num >= 0)
				{
					_logSearch.CurrentResultIndex = num;
				}
				else if (num == ~_logSearch.Results.Count)
				{
					_logSearch.CurrentResultIndex = _logSearch.Results.Count - 1;
				}
				else
				{
					_logSearch.CurrentResultIndex = ~num;
				}
			}
			else
			{
				_logSearch.CurrentResultIndex = -1;
			}
		}

		private void TouchFriendlyTextField(TouchScreenKeyboardInput touchScreenKeyboardInput, GUIStyle style)
		{
			if (_isVisible)
			{
				if (touchScreenKeyboardInput.IsDone)
				{
					if ((int)Application.platform != 11)
					{
						_hideNextFrame = true;
					}
				}
				else if (touchScreenKeyboardInput.WasCancelled && (int)Application.platform != 11)
				{
					_hideNextFrame = true;
				}
			}
			Widgets.TouchFriendlyTextField(touchScreenKeyboardInput, style, GUILayout.ExpandHeight(true));
		}
	}
}
