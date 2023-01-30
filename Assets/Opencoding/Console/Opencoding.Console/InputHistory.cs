using System.Collections.Generic;

namespace Opencoding.Console
{
	public class InputHistory
	{
		private readonly List<string> _history = new List<string>();

		private int _currentIndex;

		public bool HasPreviousInput => _currentIndex != 0;

		public bool HasNextInput => _currentIndex < _history.Count - 1;

		public void RecordInput(string input)
		{
			if (input.Trim().Length != 0)
			{
				_history.Add(input);
				_currentIndex = _history.Count;
			}
		}

		public string GetPreviousInput()
		{
			if (!HasPreviousInput)
			{
				return null;
			}
			_currentIndex--;
			return _history[_currentIndex];
		}

		public string GetNextInput()
		{
			if (!HasNextInput)
			{
				return null;
			}
			_currentIndex++;
			return _history[_currentIndex];
		}
	}
}
