using UnityEngine;

namespace Opencoding.Shared.Utils
{
	public static class PlatformUtils
	{
		private static readonly RuntimePlatform[] _desktopRuntimePlatforms;

		public static bool IsDesktop { get; private set; }

		public static bool IsMobile { get; private set; }

		static PlatformUtils()
		{
			_desktopRuntimePlatforms = new RuntimePlatform[]
			{
				RuntimePlatform.WindowsPlayer,
				RuntimePlatform.WindowsEditor,
				RuntimePlatform.LinuxPlayer,
				RuntimePlatform.LinuxEditor,
				RuntimePlatform.OSXPlayer,
				RuntimePlatform.OSXEditor
			};
			IsMobile = true;
			IsDesktop = false;
			RuntimePlatform[] desktopRuntimePlatforms = _desktopRuntimePlatforms;
			foreach (RuntimePlatform val in desktopRuntimePlatforms)
			{
				if (Application.platform == val)
				{
					IsDesktop = true;
					IsMobile = false;
					break;
				}
			}
		}
	}
}
