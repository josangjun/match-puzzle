using System;

namespace Opencoding.CommandHandlerSystem
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class ConsoleCommandAttribute : CommandHandlerAttribute
	{
	}
}
