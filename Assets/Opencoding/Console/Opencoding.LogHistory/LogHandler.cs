using UnityEngine;
using static UnityEngine.Application;

namespace Opencoding.LogHistory
{
	public static class LogHandler
	{
		private static LogCallback callbacks = (LogCallback)delegate
		{
		};

		public static void RegisterLogCallback(LogCallback callback)
		{
			if (Application.unityVersion.StartsWith("4"))
			{
				Debug.LogError((object)"This version of TouchConsole Pro is designed to work with Unity 5 and newer only. Reimport the latest version from the asset store to get the Unity 4 version.");
			}
			Application.logMessageReceivedThreaded +=callback;
		}

		public static void UnRegisterLogCallback(LogCallback callback)
		{
			Application.logMessageReceivedThreaded-=callback;
		}
	}
}
