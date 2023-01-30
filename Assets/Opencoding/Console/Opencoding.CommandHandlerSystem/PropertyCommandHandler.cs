using System;
using System.Reflection;
using Opencoding.LogHistory;

namespace Opencoding.CommandHandlerSystem
{
	public class PropertyCommandHandler : CommandHandler
	{
		private readonly MethodInfo _getMethodInfo;

		private readonly MethodInfo _setMethodInfo;

		public override ParamInfo[] Parameters
		{
			get
			{
				object defaultValue = null;
				if ((object)_getMethodInfo != null)
				{
					defaultValue = _getMethodInfo.Invoke(GetObjectReference(), null);
				}
				return new ParamInfo[1]
				{
					new ParamInfo
					{
						IsOptional = true,
						Name = "value",
						Type = _getMethodInfo.ReturnType,
						DefaultValue = defaultValue
					}
				};
			}
		}

		public PropertyCommandHandler(string commandName, string description, Type type, object obj, PropertyInfo propertyInfo, bool strongReference)
			: base(commandName, description, type, obj, propertyInfo, strongReference)
		{
			_getMethodInfo = propertyInfo.GetGetMethod(nonPublic: true);
			_setMethodInfo = propertyInfo.GetSetMethod(nonPublic: true);
			if ((object)propertyInfo.DeclaringType != null)
			{
				_methodOrPropertyName = propertyInfo.DeclaringType!.FullName + "." + propertyInfo.Name;
			}
			else
			{
				_methodOrPropertyName = propertyInfo.Name;
			}
		}

		public PropertyCommandHandler(string commandName, string description, Type type, object obj, MethodInfo getMethodInfo, MethodInfo setMethodInfo, bool strongReference)
			: base(commandName, description, type, obj, null, strongReference)
		{
			_getMethodInfo = getMethodInfo;
			_setMethodInfo = setMethodInfo;
			if ((object)getMethodInfo.DeclaringType != null)
			{
				_methodOrPropertyName = getMethodInfo.DeclaringType!.FullName + "." + getMethodInfo.Name;
			}
			else
			{
				_methodOrPropertyName = getMethodInfo.Name;
			}
		}

		public override bool Equals(object otherObject)
		{
			if (!(otherObject is PropertyCommandHandler propertyCommandHandler))
			{
				return false;
			}
			if (!base.Equals(otherObject))
			{
				return false;
			}
			if ((object)_getMethodInfo == propertyCommandHandler._getMethodInfo)
			{
				return (object)_setMethodInfo == propertyCommandHandler._setMethodInfo;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _setMethodInfo.GetHashCode() ^ _getMethodInfo.GetHashCode() ^ base.GetHashCode();
		}

		public override void Invoke(params string[] arguments)
		{
			if (arguments.Length == 0)
			{
				object obj = _getMethodInfo.Invoke(GetObjectReference(), null);
				Opencoding.LogHistory.LogHistory.Instance.LogMessage(_commandName + " = " + obj);
			}
			else if (arguments.Length == 1)
			{
				object argumentValueFromString;
				try
				{
					argumentValueFromString = Utils.GetArgumentValueFromString(arguments[0], _getMethodInfo.ReturnType);
				}
				catch (Exception)
				{
					Opencoding.LogHistory.LogHistory.Instance.LogMessage("Invalid argument to property command " + _commandName + " - expected " + _getMethodInfo.ReturnType);
					return;
				}
				_setMethodInfo.Invoke(GetObjectReference(), new object[1] { argumentValueFromString });
				Opencoding.LogHistory.LogHistory.Instance.LogMessage(_commandName + " set to " + argumentValueFromString);
			}
			else
			{
				Opencoding.LogHistory.LogHistory.Instance.LogMessage("Properties take either zero or one arguments");
			}
		}
	}
}
