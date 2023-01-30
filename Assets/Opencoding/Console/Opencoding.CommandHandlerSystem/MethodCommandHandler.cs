using System;
using System.Linq;
using System.Reflection;
using Opencoding.LogHistory;
using Opencoding.Shared.Utils;

namespace Opencoding.CommandHandlerSystem
{
	public class MethodCommandHandler : CommandHandler
	{
		private readonly MethodInfo _methodInfo;

		private readonly ParamInfo[] _apparentParameters;

		private readonly ParamInfo[] _realParameters;

		private readonly ParamInfo[] _paramParameters;

		public override ParamInfo[] Parameters => _apparentParameters;

		public MethodCommandHandler(string commandName, string description, Type type, object obj, MethodInfo methodInfo, bool strongReference)
			: base(commandName, description, type, obj, methodInfo, strongReference)
		{
			_realParameters = GenerateParameterList(methodInfo);
			_apparentParameters = _realParameters;
			_methodInfo = methodInfo;
			if ((object)methodInfo.DeclaringType != null)
			{
				_methodOrPropertyName = methodInfo.DeclaringType!.FullName + "." + methodInfo.Name;
			}
			else
			{
				_methodOrPropertyName = methodInfo.Name;
			}
		}

		public MethodCommandHandler(string commandName, string description, Type type, object obj, MethodInfo methodInfo, Type[] paramsParameterTypes, bool strongReference)
			: base(commandName, description, type, obj, methodInfo, strongReference)
		{
			_realParameters = GenerateParameterList(methodInfo);
			ParamInfo paramInfo = _realParameters[_realParameters.Length - 1];
			if (!paramInfo.IsParamArray || (object)paramInfo.Type != typeof(object[]))
			{
				throw new InvalidOperationException("Cannot specify paramsParameterTypes for a method command handler unless the last parameter is a 'params object[]'");
			}
			_paramParameters = paramsParameterTypes.Select((Type x) => new ParamInfo
			{
				IsOptional = false,
				Name = x.Name,
				Type = x,
				DefaultValue = null,
				AutoCompleteMethod = null,
				IsParamArray = false
			}).ToArray();
			_apparentParameters = _realParameters.SubArray(0, _realParameters.Length - 1).Concat(_paramParameters).ToArray();
			_methodInfo = methodInfo;
			if ((object)methodInfo.DeclaringType != null)
			{
				_methodOrPropertyName = methodInfo.DeclaringType!.FullName + "." + methodInfo.Name;
			}
			else
			{
				_methodOrPropertyName = methodInfo.Name;
			}
		}

		public MethodCommandHandler(string commandName, string description, Type type, object obj, MethodInfo methodInfo, ParamInfo[] paramsParameterInfos, bool strongReference)
			: base(commandName, description, type, obj, methodInfo, strongReference)
		{
			_realParameters = GenerateParameterList(methodInfo);
			ParamInfo paramInfo = _realParameters[_realParameters.Length - 1];
			if (!paramInfo.IsParamArray || (object)paramInfo.Type != typeof(object[]))
			{
				throw new InvalidOperationException("Cannot specify paramsParameterInfos for a method command handler unless the last parameter is a 'params object[]'");
			}
			bool flag = false;
			foreach (ParamInfo paramInfo2 in paramsParameterInfos)
			{
				if (flag && !paramInfo2.IsOptional)
				{
					throw new InvalidOperationException($"There was one optional parameter specified, but it was followed by the non-optional parameter {paramInfo2.Name}, which isn't valid.");
				}
				if (paramInfo2.IsOptional)
				{
					flag = true;
					if (paramInfo2.Type.IsPrimitive && paramInfo2.DefaultValue == null)
					{
						throw new InvalidOperationException($"Parameter {paramInfo2.Name} is an optional primitive type, but has a default value of null, which isn't valid.");
					}
				}
			}
			_paramParameters = paramsParameterInfos;
			_apparentParameters = _realParameters.SubArray(0, _realParameters.Length - 1).Concat(_paramParameters).ToArray();
			_methodInfo = methodInfo;
			if ((object)methodInfo.DeclaringType != null)
			{
				_methodOrPropertyName = methodInfo.DeclaringType!.FullName + "." + methodInfo.Name;
			}
			else
			{
				_methodOrPropertyName = methodInfo.Name;
			}
		}

		private static ParamInfo[] GenerateParameterList(MethodInfo methodInfo)
		{
			return (from x in methodInfo.GetParameters()
				select new ParamInfo
				{
					IsOptional = x.IsOptional,
					Name = x.Name,
					Type = x.ParameterType,
					DefaultValue = x.DefaultValue,
					AutoCompleteMethod = CommandHandler.GetAutoCompleteMethod(x),
					IsParamArray = (x.GetCustomAttributes(typeof(ParamArrayAttribute), inherit: false).Length != 0)
				}).ToArray();
		}

		public override bool Equals(object otherObject)
		{
			if (!(otherObject is MethodCommandHandler methodCommandHandler))
			{
				return false;
			}
			if (!base.Equals(otherObject))
			{
				return false;
			}
			return (object)_methodInfo == methodCommandHandler._methodInfo;
		}

		public override int GetHashCode()
		{
			return _methodInfo.GetHashCode() ^ base.GetHashCode();
		}

		public override void Invoke(params string[] arguments)
		{
			if (GetArgumentList(_realParameters, _paramParameters, arguments, out var argumentValues))
			{
				Opencoding.LogHistory.LogHistory.Instance.LogMessage("The command " + _commandName + " expects the arguments " + GetMethodParametersAsString(_realParameters, _paramParameters), LogHistoryLogType.Warning);
			}
			else
			{
				_methodInfo.Invoke(GetObjectReference(), argumentValues);
			}
		}
	}
}
