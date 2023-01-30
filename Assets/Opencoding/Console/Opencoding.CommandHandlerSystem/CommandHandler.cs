using System;
using System.Reflection;
using System.Text;

namespace Opencoding.CommandHandlerSystem
{
	public abstract class CommandHandler
	{
		private string _description;

		private WeakReference _objectReference;

		private object _strongObjectReference;

		protected Type _type;

		protected string _methodOrPropertyName;

		protected string _commandName;

		private bool _isStatic;

		public string Description => _description;

		public string CommandName => _commandName;

		public string MethodOrPropertyName => _methodOrPropertyName;

		public WeakReference ObjectReference => _objectReference;

		public Type Type => _type;

		public MemberInfo MemberInfo { get; private set; }

		public abstract ParamInfo[] Parameters { get; }

		protected object GetObjectReference()
		{
			if (_objectReference == null)
			{
				return null;
			}
			if (_objectReference != null && !_objectReference.IsAlive)
			{
				CommandHandlers.UnregisterCommandHandler(CommandName);
				throw new InvalidOperationException("Command handler " + CommandName + " referenced an object that has been garbage collected. Make sure you hold on to a reference to the object somewhere, or unregister the command handler if you no longer want it registered. The command handler has been automatically removed.");
			}
			return _objectReference.Target;
		}

		protected CommandHandler(string commandName, string description, Type type, object obj, MemberInfo memberInfo, bool strongReference)
		{
			_commandName = commandName;
			_description = description;
			_type = type;
			MemberInfo = memberInfo;
			if (obj != null)
			{
				_objectReference = new WeakReference(obj);
				if (strongReference)
				{
					_strongObjectReference = obj;
				}
			}
			else
			{
				_isStatic = true;
			}
		}

		public override bool Equals(object otherObject)
		{
			if (!(otherObject is CommandHandler commandHandler))
			{
				return false;
			}
			if (_objectReference == null && commandHandler._objectReference == null)
			{
				return true;
			}
			if ((_objectReference == null && commandHandler._objectReference != null) || (_objectReference != null && commandHandler._objectReference == null))
			{
				return false;
			}
			if (_objectReference.Target != commandHandler._objectReference.Target)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return (_objectReference.Target ?? throw new InvalidOperationException($"Command handler target of type {_methodOrPropertyName} was found with a null target object. This is caused when an object is destroyed without unregistering its CommandHandlers"))!.GetHashCode();
		}

		public abstract void Invoke(params string[] arguments);

		protected static MethodInfo GetAutoCompleteMethod(ParameterInfo paramInfo)
		{
			object[] customAttributes = paramInfo.GetCustomAttributes(typeof(AutocompleteAttribute), inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			return ((AutocompleteAttribute)customAttributes[0])?.MethodInfo;
		}

		public bool IsValid()
		{
			if (_isStatic)
			{
				return true;
			}
			if (_objectReference != null)
			{
				return _objectReference.IsAlive;
			}
			return false;
		}

		protected bool GetArgumentList(ParamInfo[] paramInfos, ParamInfo[] paramsParamInfos, string[] commandArguments, out object[] argumentValues)
		{
			argumentValues = new object[paramInfos.Length];
			bool flag = false;
			for (int i = 0; i < paramInfos.Length; i++)
			{
				ParamInfo paramInfo = paramInfos[i];
				if (commandArguments.Length <= i)
				{
					if (!paramInfo.IsOptional)
					{
						if (paramInfo.IsParamArray)
						{
							if (paramsParamInfos == null)
							{
								argumentValues[i] = Array.CreateInstance(paramInfo.Type.GetElementType(), 0);
								break;
							}
							Array array = Array.CreateInstance(paramInfo.Type.GetElementType(), paramsParamInfos.Length);
							argumentValues[i] = array;
							flag = !FillArrayWithDefaults(paramsParamInfos, 0, array);
						}
						else
						{
							flag = true;
						}
						break;
					}
					argumentValues[i] = paramInfo.DefaultValue;
				}
				else
				{
					if (paramInfo.IsParamArray)
					{
						Type type = paramInfo.Type.GetElementType();
						if (paramsParamInfos != null && (object)type != typeof(object))
						{
							throw new InvalidOperationException("Somehow you've got a params parameter for a command handler with custom ParamInfos but the parameter isn't of type object[], failing.");
						}
						Array array2 = ((paramsParamInfos != null) ? Array.CreateInstance(type, paramsParamInfos.Length) : Array.CreateInstance(type, commandArguments.Length - i));
						argumentValues[i] = array2;
						try
						{
							int j;
							for (j = i; j < commandArguments.Length; j++)
							{
								if (paramsParamInfos != null)
								{
									type = paramsParamInfos[j - i].Type;
								}
								string argument = commandArguments[j];
								array2.SetValue(Utils.GetArgumentValueFromString(argument, type), j - i);
							}
							flag = !FillArrayWithDefaults(paramsParamInfos, j - i, array2);
							if (!flag)
							{
								continue;
							}
							return flag;
						}
						catch (Exception)
						{
							return true;
						}
					}
					string argument2 = commandArguments[i];
					try
					{
						argumentValues[i] = Utils.GetArgumentValueFromString(argument2, paramInfo.Type);
					}
					catch (Exception)
					{
						return true;
					}
				}
			}
			return flag;
		}

		private static bool FillArrayWithDefaults(ParamInfo[] paramsParamInfos, int startingIndex, Array array)
		{
			if (paramsParamInfos == null)
			{
				return true;
			}
			for (int i = startingIndex; i < paramsParamInfos.Length; i++)
			{
				if (paramsParamInfos[i].IsOptional)
				{
					array.SetValue(paramsParamInfos[i].DefaultValue, i);
					continue;
				}
				return false;
			}
			return true;
		}

		protected string GetMethodParametersAsString(ParamInfo[] paramInfos, ParamInfo[] paramsParamInfos, bool styled = false)
		{
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ParamInfo paramInfo in paramInfos)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append("  ");
				}
				if (paramInfo.IsParamArray && paramsParamInfos != null)
				{
					stringBuilder.Append(GetMethodParametersAsString(paramsParamInfos, null, styled));
					break;
				}
				string arg = "";
				if (paramInfo.IsOptional && !flag)
				{
					flag = true;
					stringBuilder.Append("[");
				}
				if (paramInfo.IsOptional)
				{
					arg = (((object)paramInfo.Type == typeof(string)) ? " = \"\"" : (" = " + paramInfo.DefaultValue));
				}
				stringBuilder.AppendFormat(styled ? "{0} <b>{1}</b>{2}" : "{0} {1}{2}", Utils.GetFriendlyTypeName(paramInfo.Type), paramInfo.Name, arg);
			}
			if (flag)
			{
				stringBuilder.Append("]");
			}
			return stringBuilder.ToString();
		}
	}
}
