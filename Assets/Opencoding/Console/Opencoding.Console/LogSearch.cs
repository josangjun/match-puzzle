using System;
using System.Collections.Generic;
using Opencoding.LogHistory;

namespace Opencoding.Console
{
	internal class LogSearch
	{
		private FilteredLogHistoryViewModel _filteredLogHistoryViewModel;

		private readonly List<LogHistoryItem> _searchResults = new List<LogHistoryItem>();

		private readonly HashSet<LogHistoryItem> _searchResultsHashSet = new HashSet<LogHistoryItem>();

		private string _searchString = "";

		public Action<LogHistoryItem> CurrentResultChanged = delegate
		{
		};

		private int _currentResultIndex = -1;

		public bool Enabled { get; set; }

		public bool IsSearchActive
		{
			get
			{
				if (Enabled)
				{
					return _searchString != "";
				}
				return false;
			}
		}

		public List<LogHistoryItem> Results => _searchResults;

		public int CurrentResultIndex
		{
			get
			{
				return _currentResultIndex;
			}
			set
			{
				if (value > _searchResults.Count - 1)
				{
					throw new InvalidOperationException($"Invalid search result index: {value} Max: {_searchResults.Count - 1}");
				}
				_currentResultIndex = value;
				CurrentResultChanged(CurrentResult);
			}
		}

		public LogHistoryItem CurrentResult
		{
			get
			{
				if (CurrentResultIndex < 0)
				{
					return null;
				}
				return Results[CurrentResultIndex];
			}
		}

		public string SearchString
		{
			get
			{
				return _searchString;
			}
			set
			{
				if (!(value == _searchString))
				{
					_searchString = value;
					RunSearchFromScratch();
				}
			}
		}

		public LogSearch(FilteredLogHistoryViewModel filteredLogHistoryViewModel)
		{
			_filteredLogHistoryViewModel = filteredLogHistoryViewModel;
			_filteredLogHistoryViewModel.LogHistoryItemAdded += CheckIfItemMatchesSearch;
			_filteredLogHistoryViewModel.LogHistoryItemRemoved += RemoveLogHistoryItem;
			_filteredLogHistoryViewModel.LogHistoryListReset += RunSearchFromScratch;
		}

		private void RemoveLogHistoryItem(LogHistoryItem logHistoryItem)
		{
			_searchResults.Remove(logHistoryItem);
			_searchResultsHashSet.Remove(logHistoryItem);
		}

		private void CheckIfItemMatchesSearch(LogHistoryItem logHistoryItem)
		{
			if (IsSearchActive)
			{
				AddItemIfMatchesSearch(logHistoryItem);
			}
		}

		public void RunSearchFromScratch()
		{
			_searchResults.Clear();
			_searchResultsHashSet.Clear();
			CurrentResultIndex = -1;
			if (!IsSearchActive)
			{
				return;
			}
			foreach (LogHistoryItem logHistoryItem in _filteredLogHistoryViewModel.LogHistoryItems)
			{
				AddItemIfMatchesSearch(logHistoryItem);
			}
		}

		private void AddItemIfMatchesSearch(LogHistoryItem item)
		{
			if (DoesItemMatchSearchString(item))
			{
				_searchResults.Add(item);
				_searchResultsHashSet.Add(item);
			}
		}

		private bool DoesItemMatchSearchString(LogHistoryItem item)
		{
			return item._LogMessage.IndexOf(_searchString, StringComparison.CurrentCultureIgnoreCase) >= 0;
		}

		public bool IsLogHistoryItemASearchResult(LogHistoryItem item)
		{
			return _searchResultsHashSet.Contains(item);
		}

		public void NextSearchResult()
		{
			if (CurrentResultIndex == Results.Count - 1)
			{
				CurrentResultIndex = 0;
			}
			else
			{
				CurrentResultIndex++;
			}
		}

		public void PreviousSearchResult()
		{
			if (CurrentResultIndex == 0)
			{
				CurrentResultIndex = Results.Count - 1;
			}
			else
			{
				CurrentResultIndex--;
			}
		}
	}
}
