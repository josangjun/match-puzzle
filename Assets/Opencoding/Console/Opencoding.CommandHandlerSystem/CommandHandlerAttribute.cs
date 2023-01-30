using System;

namespace Opencoding.CommandHandlerSystem
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class CommandHandlerAttribute : Attribute
	{
		public string Name { get; set; }

		public string Description { get; set; }
	}
}
