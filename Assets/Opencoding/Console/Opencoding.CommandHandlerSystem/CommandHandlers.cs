using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Opencoding.LogHistory;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.CommandHandlerSystem
{
	public static class CommandHandlers
	{
		private static readonly Dictionary<string, CommandHandler> _commandHandlers = new Dictionary<string, CommandHandler>();

		private static readonly HashSet<Type> _registeredCommandHandlers = new HashSet<Type>();

		private static Action<CommandHandler> _commandHandlerAdded = delegate
		{
		};

		private static Action<CommandHandler> _commandHandlerRemoved = delegate
		{
		};

		private static Action<CommandHandler> _commandExecuted = delegate
		{
		};

		public static Func<string, bool> BeforeCommandParsedHook;

		public static Func<CommandHandler, string[], bool> BeforeCommandExecutedHook;

		public static Func<string, string[], bool> DefaultCommandHandler;

		public static string CurrentExecutingCommand;

		public static IEnumerable<CommandHandler> Handlers => _commandHandlers.Values;

		public static event Action<CommandHandler> CommandHandlerAdded
		{
			add
			{
				_commandHandlerAdded = (Action<CommandHandler>)Delegate.Combine(_commandHandlerAdded, value);
			}
			remove
			{
				_commandHandlerAdded = (Action<CommandHandler>)Delegate.Remove(_commandHandlerAdded, value);
			}
		}

		public static event Action<CommandHandler> CommandHandlerRemoved
		{
			add
			{
				_commandHandlerRemoved = (Action<CommandHandler>)Delegate.Combine(_commandHandlerRemoved, value);
			}
			remove
			{
				_commandHandlerRemoved = (Action<CommandHandler>)Delegate.Remove(_commandHandlerRemoved, value);
			}
		}

		public static event Action<CommandHandler> CommandExecuted
		{
			add
			{
				_commandExecuted = (Action<CommandHandler>)Delegate.Combine(_commandExecuted, value);
			}
			remove
			{
				_commandExecuted = (Action<CommandHandler>)Delegate.Remove(_commandExecuted, value);
			}
		}

		public static void HandleCommand(string commandLine)
		{
			if (BeforeCommandParsedHook != null && BeforeCommandParsedHook(commandLine))
			{
				return;
			}
			string[] array = StringUtils.SplitCommandLine(commandLine).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			Opencoding.LogHistory.LogHistory.Instance.LogMessage(commandLine, LogHistoryLogType.ConsoleInput);
			string text = array[0];
			string key = text.ToLower();
			string[] array2 = array.SubArray(1, array.Length - 1);
			if (!_commandHandlers.TryGetValue(key, out var value))
			{
				if (DefaultCommandHandler == null || !DefaultCommandHandler(text, array2))
				{
					Debug.LogWarning((object)$"The command {text} could not be found.");
				}
				return;
			}
			if (!value.IsValid())
			{
				throw new InvalidOperationException($"The command handler {value.MethodOrPropertyName} has been destroyed without being unregistered.");
			}
			ParamInfo[] parameters = value.Parameters;
			bool flag = parameters.Length != 0 && parameters[^1].IsParamArray;
			if (array2.Length > parameters.Length && !flag)
			{
				Debug.LogWarning((object)$"The command {text} expects at most {parameters.Length} arguments");
			}
			else if (BeforeCommandExecutedHook == null || BeforeCommandExecutedHook(value, array2))
			{
				CurrentExecutingCommand = value.CommandName;
				value.Invoke(array2);
				CurrentExecutingCommand = null;
				_commandExecuted(value);
			}
		}

		public static CommandHandler FindClosestMatchingCommand(string partialCommand, int index)
		{
			CommandHandler[] array = FindMatchingCommands(partialCommand).ToArray();
			if (array.Length == 0)
			{
				return null;
			}
			return array[index % array.Length];
		}

		public static IEnumerable<CommandHandler> FindMatchingCommands(string partialCommand)
		{
			return (from x in _commandHandlers.Values
				where x.CommandName.StartsWith(partialCommand, StringComparison.CurrentCultureIgnoreCase)
				orderby x.CommandName.Length descending
				select x).ToArray();
		}

		public static CommandHandler GetCommandHandler(string commandName)
		{
			string[] array = StringUtils.SplitCommandLine(commandName).ToArray();
			if (array.Length == 0)
			{
				return null;
			}
			_commandHandlers.TryGetValue(array[0].ToLower(), out var value);
			return value;
		}

		public static void RegisterCommandHandlers(Type type)
		{
			RegisterCommandHandlers(type, null);
		}

		public static void RegisterCommandHandlers(object obj)
		{
			RegisterCommandHandlers(obj.GetType(), obj);
		}

		public static void RegisterCommandHandler(MethodInfo methodInfo, object obj, string commandName = null, string description = null, bool strongReference = false)
		{
			if (!methodInfo.IsStatic && obj == null)
			{
				throw new InvalidOperationException("Attempting to register non static method as command handler without specifying an instance object.");
			}
			if (methodInfo.IsStatic && obj != null)
			{
				throw new InvalidOperationException("Attempting to a static method as command handler while specifying an instance object.");
			}
			RegisterMethodCommandHandler(methodInfo.DeclaringType, obj, methodInfo, description, commandName, strongReference);
		}

		public static void RegisterCommandHandler(PropertyInfo propertyInfo, object obj, string commandName = null, string description = null, bool strongReference = false)
		{
			if (!propertyInfo.GetGetMethod()!.IsStatic && obj == null)
			{
				throw new InvalidOperationException("Attempting to register non static property as command handler without specifying an instance object.");
			}
			if (propertyInfo.GetGetMethod()!.IsStatic && obj != null)
			{
				throw new InvalidOperationException("Attempting to a static property as command handler while specifying an instance object.");
			}
			RegisterPropertyCommandHandler(propertyInfo.DeclaringType, obj, propertyInfo, description, commandName, strongReference);
		}

		public static void RegisterCommandHandler(MethodInfo getMethodInfo, MethodInfo setMethodInfo, object obj, string commandName, string description = null, bool strongReference = false)
		{
			RegisterPropertyCommandHandler(getMethodInfo.DeclaringType, obj, getMethodInfo, setMethodInfo, description, commandName, strongReference);
		}

		public static void RegisterCommandHandler(MethodInfo methodInfo, string commandName = null, string description = null, bool strongReference = false)
		{
			if (!methodInfo.IsStatic)
			{
				throw new InvalidOperationException("Attempting to register non static method as command handler without specifying an instance object. Use the variant of RegisterCommandHandler that takes an object instead.");
			}
			RegisterMethodCommandHandler(methodInfo.DeclaringType, null, methodInfo, description, commandName, strongReference);
		}

		public static void RegisterCommandHandler(PropertyInfo propertyInfo, string commandName = null, string description = null, bool strongReference = false)
		{
			if (!propertyInfo.GetGetMethod()!.IsStatic)
			{
				throw new InvalidOperationException("Attempting to register non static property as command handler without specifying an instance object. Use the variant of RegisterCommandHandler that takes an object instead.");
			}
			RegisterPropertyCommandHandler(propertyInfo.DeclaringType, null, propertyInfo, description, commandName, strongReference);
		}

		public static void RegisterCommandHandler(MethodInfo getMethodInfo, MethodInfo setMethodInfo, string commandName, string description = null, bool strongReference = false)
		{
			if (!getMethodInfo.IsStatic || !setMethodInfo.IsStatic)
			{
				throw new InvalidOperationException("Attempting to register non static property as command handler without specifying an instance object. Use the variant of RegisterCommandHandler that takes an object instead.");
			}
			RegisterPropertyCommandHandler(getMethodInfo.DeclaringType, null, getMethodInfo, setMethodInfo, description, commandName, strongReference);
		}

		private static void RegisterCommandHandler_internal(Delegate del, string commandName = null, string description = null, Type[] types = null, bool strongReference = false)
		{
			MethodInfo method = del.Method;
			if (types != null)
			{
				RegisterMethodCommandHandler(method.DeclaringType, del.Target, method, description, commandName, types, strongReference);
			}
			else
			{
				RegisterMethodCommandHandler(method.DeclaringType, del.Target, method, description, commandName, strongReference);
			}
		}

		private static void RegisterCommandHandler_internal(Delegate del, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = false)
		{
			MethodInfo method = del.Method;
			RegisterMethodCommandHandler(method.DeclaringType, del.Target, method, description, commandName, paramInfos, strongReference);
		}

		private static void RegisterCommandHandler_internal(Delegate getDelegate, Delegate setDelegate, string commandName, string description = null, bool strongReference = false)
		{
			MethodInfo method = getDelegate.Method;
			MethodInfo method2 = setDelegate.Method;
			if (getDelegate.Target != null || setDelegate.Target != null)
			{
				throw new InvalidOperationException("Command handlers can't be created from get/set delegates when either of these are instances. If this is an important feature to you, please contact support@opencoding.net.");
			}
			RegisterPropertyCommandHandler(method.DeclaringType, null, method, method2, description, commandName, strongReference);
		}

		private static void RegisterCommandHandlers(Type type, object obj)
		{
			if (!_registeredCommandHandlers.Contains(type))
			{
				_registeredCommandHandlers.Add(type);
				RegisterCommandHandlerMethods(type, obj);
				RegisterCommandHandlerProperties(type, obj);
			}
		}

		private static void RegisterCommandHandlerMethods(Type type, object obj, bool strongReference = false)
		{
			MethodInfo[] array = ((obj != null) ? type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
			MethodInfo[] array2 = array;
			foreach (MethodInfo methodInfo in array2)
			{
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(CommandHandlerAttribute), inherit: true);
				if (customAttributes.Length != 0)
				{
					CommandHandlerAttribute commandHandlerAttribute = (CommandHandlerAttribute)customAttributes[0];
					string name = commandHandlerAttribute.Name;
					RegisterMethodCommandHandler(type, obj, methodInfo, commandHandlerAttribute.Description, name, strongReference);
				}
			}
		}

		private static void RegisterCommandHandlerProperties(Type type, object obj, bool strongReference = false)
		{
			PropertyInfo[] array = ((obj != null) ? type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
			PropertyInfo[] array2 = array;
			foreach (PropertyInfo propertyInfo in array2)
			{
				object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(CommandHandlerAttribute), inherit: true);
				if (customAttributes.Length != 0)
				{
					CommandHandlerAttribute commandHandlerAttribute = (CommandHandlerAttribute)customAttributes[0];
					string name = commandHandlerAttribute.Name;
					if (string.IsNullOrEmpty(name))
					{
						name = propertyInfo.Name;
					}
					RegisterPropertyCommandHandler(type, obj, propertyInfo, commandHandlerAttribute.Description, name, strongReference);
				}
			}
		}

		private static void RegisterMethodCommandHandler(Type type, object obj, MethodInfo method, string description, string commandName, ParamInfo[] paramsParameterInfos, bool strongReference)
		{
			if (string.IsNullOrEmpty(commandName))
			{
				commandName = method.Name;
			}
			string methodOrPropertyName = $"{method.DeclaringType!.FullName}.{method.Name}";
			if (ValidateCommandHandlerName(commandName, methodOrPropertyName))
			{
				MethodCommandHandler methodCommandHandler = new MethodCommandHandler(commandName, description, type, obj, method, paramsParameterInfos, strongReference);
				_commandHandlers.Add(commandName.ToLower(), methodCommandHandler);
				_commandHandlerAdded(methodCommandHandler);
			}
		}

		private static void RegisterMethodCommandHandler(Type type, object obj, MethodInfo method, string description, string commandName, Type[] paramsParameterTypes, bool strongReference)
		{
			if (string.IsNullOrEmpty(commandName))
			{
				commandName = method.Name;
			}
			string methodOrPropertyName = $"{method.DeclaringType!.FullName}.{method.Name}";
			if (ValidateCommandHandlerName(commandName, methodOrPropertyName))
			{
				MethodCommandHandler methodCommandHandler = new MethodCommandHandler(commandName, description, type, obj, method, paramsParameterTypes, strongReference);
				_commandHandlers.Add(commandName.ToLower(), methodCommandHandler);
				_commandHandlerAdded(methodCommandHandler);
			}
		}

		private static void RegisterMethodCommandHandler(Type type, object obj, MethodInfo method, string description, string commandName, bool strongReference)
		{
			if (string.IsNullOrEmpty(commandName))
			{
				commandName = method.Name;
			}
			string methodOrPropertyName = $"{method.DeclaringType!.FullName}.{method.Name}";
			if (ValidateCommandHandlerName(commandName, methodOrPropertyName))
			{
				MethodCommandHandler methodCommandHandler = new MethodCommandHandler(commandName, description, type, obj, method, strongReference);
				_commandHandlers.Add(commandName.ToLower(), methodCommandHandler);
				_commandHandlerAdded(methodCommandHandler);
			}
		}

		private static void RegisterPropertyCommandHandler(Type type, object obj, PropertyInfo property, string description, string commandName, bool strongReference)
		{
			string methodOrPropertyName = $"{property.DeclaringType!.FullName}.{property.Name}";
			if (ValidateCommandHandlerName(commandName, methodOrPropertyName))
			{
				PropertyCommandHandler propertyCommandHandler = new PropertyCommandHandler(commandName, description, type, obj, property, strongReference);
				_commandHandlers.Add(commandName.ToLower(), propertyCommandHandler);
				_commandHandlerAdded(propertyCommandHandler);
			}
		}

		private static void RegisterPropertyCommandHandler(Type type, object obj, MethodInfo getMethodInfo, MethodInfo setMethodInfo, string description, string commandName, bool strongReference)
		{
			string methodOrPropertyName = $"{getMethodInfo.DeclaringType!.FullName}.{getMethodInfo.Name}";
			if (ValidateCommandHandlerName(commandName, methodOrPropertyName))
			{
				PropertyCommandHandler propertyCommandHandler = new PropertyCommandHandler(commandName, description, type, obj, getMethodInfo, setMethodInfo, strongReference);
				_commandHandlers.Add(commandName.ToLower(), propertyCommandHandler);
				_commandHandlerAdded(propertyCommandHandler);
			}
		}

		private static bool ValidateCommandHandlerName(string commandName, string methodOrPropertyName)
		{
			string text = commandName.ToLower();
			if (text.Contains(" "))
			{
				Debug.LogWarning((object)$"Invalid command name {text}. Command names cannot contain spaces. Ignoring.");
				return false;
			}
			if (_commandHandlers.ContainsKey(text))
			{
				Debug.LogWarning((object)$"A command handler has already been registered for the command {commandName}. Ignoring {methodOrPropertyName}.");
				return false;
			}
			return true;
		}

		public static void UnregisterCommandHandlers(object obj)
		{
			if (!_registeredCommandHandlers.Contains(obj.GetType()))
			{
				return;
			}
			foreach (string item in (from i in _commandHandlers
				where i.Value.ObjectReference != null && i.Value.ObjectReference.Target == obj
				select i.Key).ToList())
			{
				_commandHandlerRemoved(_commandHandlers[item]);
				_commandHandlers.Remove(item);
			}
			_registeredCommandHandlers.Remove(obj.GetType());
		}

		public static void UnregisterCommandHandlers(Type type)
		{
			if (!_registeredCommandHandlers.Contains(type))
			{
				return;
			}
			foreach (string item in (from i in _commandHandlers
				where (object)i.Value.Type == type
				select i.Key).ToList())
			{
				_commandHandlerRemoved(_commandHandlers[item]);
				_commandHandlers.Remove(item);
			}
			_registeredCommandHandlers.Remove(type);
		}

		public static bool UnregisterCommandHandler(string commandName)
		{
			commandName = commandName.ToLower();
			_commandHandlers.TryGetValue(commandName, out var value);
			if (value != null && _commandHandlers.Remove(commandName))
			{
				_commandHandlerRemoved(value);
				return true;
			}
			return false;
		}

		public static void RegisterCommandHandler<T>(Func<T> getDelegate, Action<T> setDelegate, string commandName, string description = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(getDelegate, setDelegate, commandName, description, strongReference);
		}

		public static void RegisterCommandHandler(Action action, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler(Delegate del, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(del, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler<T1>(Action<T1> action, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2>(Action<T1, T2> action, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2, T3>(Action<T1, T2, T3> action, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, string commandName = null, string description = null, Type[] types = null, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, types, strongReference);
		}

		public static void RegisterCommandHandler(Delegate del, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(del, commandName, description, paramInfos, strongReference);
		}

		public static void RegisterCommandHandler(Action action, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, paramInfos, strongReference);
		}

		public static void RegisterCommandHandler<T1>(Action<T1> action, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, paramInfos, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2>(Action<T1, T2> action, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, paramInfos, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2, T3>(Action<T1, T2, T3> action, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, paramInfos, strongReference);
		}

		public static void RegisterCommandHandler<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, string commandName, string description, ParamInfo[] paramInfos, bool strongReference = true)
		{
			RegisterCommandHandler_internal(action, commandName, description, paramInfos, strongReference);
		}
	}
}
