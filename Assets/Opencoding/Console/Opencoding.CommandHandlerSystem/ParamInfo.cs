using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Opencoding.LogHistory;

namespace Opencoding.CommandHandlerSystem
{
	public class ParamInfo
	{
		public string Name;

		public bool IsOptional;

		public Type Type;

		public object DefaultValue;

		public MethodInfo AutoCompleteMethod;

		public string[] AutoCompleteOptions;

		public bool IsParamArray;

		public List<string> GetParameterPossibleValues(string parameterValue)
		{
			IEnumerable<string> source;
			if ((object)AutoCompleteMethod == null)
			{
				source = ((AutoCompleteOptions != null) ? AutoCompleteOptions : ((!IsParamArray) ? Utils.GetDefaultParameterPossibleOptions(Type) : Utils.GetDefaultParameterPossibleOptions(Type.GetElementType())));
			}
			else
			{
				object obj = AutoCompleteMethod.Invoke(null, null);
				if (!(obj is IEnumerable<string>))
				{
					Opencoding.LogHistory.LogHistory.Instance.LogMessage("Invalid return type from auto complete method", LogHistoryLogType.Error);
					source = Utils.GetDefaultParameterPossibleOptions(Type);
				}
				else
				{
					source = (IEnumerable<string>)obj;
				}
			}
			List<string> source2 = source.ToList();
			List<string> options = source2.Where((string x) => x.StartsWith(parameterValue, StringComparison.CurrentCultureIgnoreCase)).ToList();
			IEnumerable<string> second = source2.Where((string x) => x.IndexOf(parameterValue, StringComparison.CurrentCultureIgnoreCase) >= 0);
			options.AddRange(from x in source2
				where !options.Contains(x)
				where x.IndexOf(parameterValue, StringComparison.CurrentCultureIgnoreCase) >= 0
				select x);
			return options.Union(second).ToList();
		}
	}
}
