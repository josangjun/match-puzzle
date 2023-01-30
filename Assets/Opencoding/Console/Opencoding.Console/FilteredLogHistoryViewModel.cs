using System;
using System.Collections.Generic;
using System.Linq;
using Opencoding.LogHistory;
using UnityEngine;

namespace Opencoding.Console
{
	internal class FilteredLogHistoryViewModel : IDisposable
	{
		private readonly List<LogHistoryItem> _filteredLogItems = new List<LogHistoryItem>();

		private string _filterString = "";

		private bool _showInfos = true;

		private bool _showWarnings = true;

		private bool _showErrors = true;

		private bool _showExceptions = true;

		private bool _showAsserts = true;

		private Action<LogHistoryItem> _logHistoryItemAdded = delegate
		{
		};

		private Action<LogHistoryItem> _logHistoryItemRemoved = delegate
		{
		};

		private Action _logHistoryListReset = delegate
		{
		};

		private IList<LogHistoryItem> _currentItemSource;

		private Opencoding.LogHistory.LogHistory _logHistory;

		private bool _onlyMatchStringsThatStartWithFilterString;

		public string FilterString
		{
			get
			{
				return _filterString;
			}
			set
			{
				if (!(_filterString == value))
				{
					_filterString = value;
					FilterLogItems();
				}
			}
		}

		public bool OnlyMatchStringsThatStartWithFilterString
		{
			get
			{
				return _onlyMatchStringsThatStartWithFilterString;
			}
			set
			{
				if (_onlyMatchStringsThatStartWithFilterString != value)
				{
					PlayerPrefs.SetInt("Opencoding.Console.OnlyMatchStringsThatStartWithFilterString", value ? 1 : 0);
					PlayerPrefs.Save();
					_onlyMatchStringsThatStartWithFilterString = value;
					FilterLogItems();
				}
			}
		}

		public bool ShowInfos
		{
			get
			{
				return _showInfos;
			}
			set
			{
				if (_showInfos != value)
				{
					_showInfos = value;
					FilterLogItems();
				}
			}
		}

		public bool ShowWarnings
		{
			get
			{
				return _showWarnings;
			}
			set
			{
				if (_showWarnings != value)
				{
					_showWarnings = value;
					FilterLogItems();
				}
			}
		}

		public bool ShowErrors
		{
			get
			{
				return _showErrors;
			}
			set
			{
				if (_showErrors != value)
				{
					_showErrors = value;
					FilterLogItems();
				}
			}
		}

		public bool ShowExceptions
		{
			get
			{
				return _showExceptions;
			}
			set
			{
				if (_showExceptions != value)
				{
					_showExceptions = value;
					FilterLogItems();
				}
			}
		}

		public bool ShowAsserts
		{
			get
			{
				return _showAsserts;
			}
			set
			{
				if (_showAsserts != value)
				{
					_showAsserts = value;
					FilterLogItems();
				}
			}
		}

		public IList<LogHistoryItem> LogHistoryItems => _currentItemSource;

		public bool IsFiltered
		{
			get
			{
				if (!(_filterString != "") && _showErrors && _showInfos && _showWarnings && _showAsserts)
				{
					return !_showExceptions;
				}
				return true;
			}
		}

		public event Action<LogHistoryItem> LogHistoryItemAdded
		{
			add
			{
				_logHistoryItemAdded = (Action<LogHistoryItem>)Delegate.Combine(_logHistoryItemAdded, value);
			}
			remove
			{
				_logHistoryItemAdded = (Action<LogHistoryItem>)Delegate.Remove(_logHistoryItemAdded, value);
			}
		}

		public event Action<LogHistoryItem> LogHistoryItemRemoved
		{
			add
			{
				_logHistoryItemRemoved = (Action<LogHistoryItem>)Delegate.Combine(_logHistoryItemRemoved, value);
			}
			remove
			{
				_logHistoryItemRemoved = (Action<LogHistoryItem>)Delegate.Remove(_logHistoryItemRemoved, value);
			}
		}

		public event Action LogHistoryListReset
		{
			add
			{
				_logHistoryListReset = (Action)Delegate.Combine(_logHistoryListReset, value);
			}
			remove
			{
				_logHistoryListReset = (Action)Delegate.Remove(_logHistoryListReset, value);
			}
		}

		public FilteredLogHistoryViewModel(Opencoding.LogHistory.LogHistory logHistory)
		{
			logHistory.LogHistoryItemAdded += AddLogItemToFilteredListIfNecessary;
			logHistory.LogHistoryItemRemoved += RemoveLogItemFromFilteredList;
			logHistory.LogHistoryCleared += FilterLogItems;
			_logHistory = logHistory;
			_currentItemSource = logHistory.LogItems;
			_onlyMatchStringsThatStartWithFilterString = PlayerPrefs.GetInt("Opencoding.Console.OnlyMatchStringsThatStartWithFilterString") == 1;
		}

		public void Dispose()
		{
			_logHistory.LogHistoryItemAdded -= AddLogItemToFilteredListIfNecessary;
			_logHistory.LogHistoryItemRemoved -= RemoveLogItemFromFilteredList;
			_logHistory.LogHistoryCleared -= FilterLogItems;
			_logHistory = null;
		}

		private void AddLogItemToFilteredListIfNecessary(LogHistoryItem historyItem)
		{
			if (IsFiltered && PassesFilter(historyItem))
			{
				_filteredLogItems.Add(historyItem);
				_logHistoryItemAdded(historyItem);
			}
			else if (!IsFiltered)
			{
				_logHistoryItemAdded(historyItem);
			}
		}

		private void RemoveLogItemFromFilteredList(LogHistoryItem historyItem)
		{
			if (IsFiltered && _filteredLogItems.Remove(historyItem))
			{
				_logHistoryItemRemoved(historyItem);
			}
			else if (!IsFiltered)
			{
				_logHistoryItemRemoved(historyItem);
			}
		}

		private void FilterLogItems()
		{
			_filteredLogItems.Clear();
			if (!IsFiltered)
			{
				_currentItemSource = _logHistory.LogItems;
				_logHistoryListReset();
				return;
			}
			foreach (LogHistoryItem item in _logHistory.LogItems.Where((LogHistoryItem item) => PassesFilter(item)))
			{
				_filteredLogItems.Add(item);
			}
			_currentItemSource = _filteredLogItems.AsReadOnly();
			_logHistoryListReset();
		}

		private bool PassesFilter(LogHistoryItem historyItem)
		{
			if (!_showInfos && historyItem._Type == LogHistoryLogType.Log)
			{
				return false;
			}
			if (!_showWarnings && historyItem._Type == LogHistoryLogType.Warning)
			{
				return false;
			}
			if (!_showErrors && historyItem._Type == LogHistoryLogType.Error)
			{
				return false;
			}
			if (!_showAsserts && historyItem._Type == LogHistoryLogType.Assert)
			{
				return false;
			}
			if (!_showExceptions && historyItem._Type == LogHistoryLogType.Exception)
			{
				return false;
			}
			int num = historyItem._LogMessage.IndexOf(_filterString, StringComparison.CurrentCultureIgnoreCase);
			if (OnlyMatchStringsThatStartWithFilterString)
			{
				return num == 0;
			}
			return num >= 0;
		}
	}
}
