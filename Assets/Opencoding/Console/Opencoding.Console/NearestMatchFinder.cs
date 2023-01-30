using System.Collections.Generic;
using Opencoding.LogHistory;

namespace Opencoding.Console
{
	internal class NearestMatchFinder : IComparer<LogHistoryItem>
	{
		public int Compare(LogHistoryItem x, LogHistoryItem y)
		{
			return x._Id.CompareTo(y._Id);
		}
	}
}
