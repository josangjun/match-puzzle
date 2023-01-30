using System;
using System.Collections.Generic;
using System.Linq;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.LogHistory
{
	public class LogHistory : IDisposable
	{
		private int _itemLimit = 3000;

		private readonly List<LogHistoryItem> _deferredLogItems = new List<LogHistoryItem>();

		private readonly List<LogHistoryItem> _logItems = new List<LogHistoryItem>();

		private DateTime _gameStartTime;

		private object _deferredLogItemListMutex = new object();

		private Action<LogHistoryItem> _logHistoryItemAdded = delegate
		{
		};

		private Action<LogHistoryItem> _logHistoryItemRemoved = delegate
		{
		};

		private Action _logHistoryCleared = delegate
		{
		};

		private static LogHistory _instance;

		public int InfoCount { get; private set; }

		public int WarningCount { get; private set; }

		public int ErrorCount { get; private set; }

		public int ExceptionCount { get; private set; }

		public int AssertCount { get; private set; }

		public DateTime GameStartTime => _gameStartTime;

		public IList<LogHistoryItem> LogItems => _logItems.AsReadOnly();

		public static LogHistory Instance => _instance;

		public int ItemLimit
		{
			get
			{
				return _itemLimit;
			}
			set
			{
				lock (_deferredLogItemListMutex)
				{
					_itemLimit = value;
				}
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

		public event Action LogHistoryCleared
		{
			add
			{
				_logHistoryCleared = (Action)Delegate.Combine(_logHistoryCleared, value);
			}
			remove
			{
				_logHistoryCleared = (Action)Delegate.Remove(_logHistoryCleared, value);
			}
		}

		public LogHistory()
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			if (_instance != null)
			{
				throw new InvalidOperationException("DebugLogHistory is a singleton and has already been instantiated");
			}
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
			_gameStartTime = DateTime.Now;
			_instance = this;
			LogHandler.RegisterLogCallback(HandleLoggingEvent);
		}

		public void Dispose()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (_instance == this)
			{
				_instance = null;
			}
			LogHandler.UnRegisterLogCallback(HandleLoggingEvent);
		}

		public void Update()
		{
			if (_deferredLogItems.Count == 0)
			{
				return;
			}
			LogHistoryItem[] array = _deferredLogItems.ToArray();
			lock (_deferredLogItemListMutex)
			{
				_logItems.AddRange(_deferredLogItems);
				foreach (LogHistoryItem deferredLogItem in _deferredLogItems)
				{
					UpdateTypeCounter(deferredLogItem._Type, 1);
				}
				while (_logItems.Count > _itemLimit)
				{
					LogHistoryItem logHistoryItem = _logItems.First();
					_logHistoryItemRemoved(logHistoryItem);
					UpdateTypeCounter(logHistoryItem._Type, -1);
					_logItems.RemoveAt(0);
				}
				_deferredLogItems.Clear();
			}
			LogHistoryItem[] array2 = array;
			foreach (LogHistoryItem obj in array2)
			{
				_logHistoryItemAdded(obj);
			}
		}

		private void UpdateTypeCounter(LogHistoryLogType type, int change)
		{
			switch (type)
			{
			case LogHistoryLogType.Log:
				InfoCount += change;
				break;
			case LogHistoryLogType.Warning:
				WarningCount += change;
				break;
			case LogHistoryLogType.Error:
				ErrorCount += change;
				break;
			case LogHistoryLogType.Exception:
				ExceptionCount += change;
				break;
			case LogHistoryLogType.Assert:
				AssertCount += change;
				break;
			}
		}

		public void Clear()
		{
			InfoCount = 0;
			ErrorCount = 0;
			WarningCount = 0;
			AssertCount = 0;
			ExceptionCount = 0;
			lock (_deferredLogItemListMutex)
			{
				_deferredLogItems.Clear();
			}
			_logItems.Clear();
			_logHistoryCleared();
		}

		private string FilterStackTrace(string stackTrace)
		{
			string[] array = stackTrace.Split(new char[1] { '\n' });
			bool flag = array[0].StartsWith("Debug:Log");
			if (flag && array[1].StartsWith("UnityAppender:Append"))
			{
				int i;
				for (i = 2; i < array.Length && !array[i].StartsWith("log4net.Core."); i++)
				{
				}
				i++;
				return string.Join("\n", array.SubArray(i, array.Length - i));
			}
			if (flag)
			{
				return string.Join("\n", array.SubArray(1, array.Length - 1));
			}
			return stackTrace.Trim();
		}

		public LogHistoryLogType ConvertFromUnityLogType(LogType logType)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected I4, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			return (int)logType switch
			{
				3 => LogHistoryLogType.Log, 
				2 => LogHistoryLogType.Warning, 
				4 => LogHistoryLogType.Exception, 
				1 => LogHistoryLogType.Assert, 
				0 => LogHistoryLogType.Error, 
				_ => throw new InvalidOperationException("Unrecognised log type " + logType), 
			};
		}

		private void HandleLoggingEvent(string logString, string stackTrace, LogType type)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			InternalHandleLoggingEvent(logString, stackTrace, ConvertFromUnityLogType(type));
		}

		private void InternalHandleLoggingEvent(string logString, string stackTrace, LogHistoryLogType type)
		{
			float time = (float)DateTime.Now.Subtract(_gameStartTime).Ticks / 10000000f;
			LogHistoryItem item = new LogHistoryItem(type, logString, time, FilterStackTrace(stackTrace));
			lock (_deferredLogItemListMutex)
			{
				_deferredLogItems.Add(item);
			}
		}

		public void LogException(string message)
		{
			string stackTrace = Environment.StackTrace;
			InternalHandleLoggingEvent(message, stackTrace, LogHistoryLogType.Exception);
		}

		public void LogMessage(string message, LogHistoryLogType type = LogHistoryLogType.Log)
		{
			InternalHandleLoggingEvent(message, "", type);
		}

		public void LogMessage(string message, string stacktrace, LogHistoryLogType type = LogHistoryLogType.Log)
		{
			InternalHandleLoggingEvent(message, stacktrace, type);
		}

		private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Debug.LogException((Exception)e.ExceptionObject);
		}
	}
}
