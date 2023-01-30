using System;
using System.Collections;
using UnityEngine;

namespace Opencoding.Console.Scripts.UI.Utilities
{
	internal static class ScreenshotCapture
	{
		public static void CaptureScreenshot(Settings.SelectedScreenshotCaptureMode captureMode, Action<byte[]> screenshotTakenAction)
		{
			switch (captureMode)
			{
			case Settings.SelectedScreenshotCaptureMode.DISABLED:
				screenshotTakenAction(null);
				break;
			case Settings.SelectedScreenshotCaptureMode.CAPTURE_MAIN_CAMERA:
				CaptureMainCamera(screenshotTakenAction);
				break;
			case Settings.SelectedScreenshotCaptureMode.CAPTURE_FULL_SCREEN:
				CaptureFullScreen(screenshotTakenAction);
				break;
			default:
				throw new InvalidOperationException("Unsupported screenshot capture mode");
			}
		}

		private static void CaptureMainCamera(Action<byte[]> screenshotTakenAction)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			Camera main = Camera.main;
			Texture2D val = new Texture2D(main.pixelWidth, main.pixelHeight, (TextureFormat)3, false);
			RenderTexture targetTexture = Camera.main.targetTexture;
			RenderTexture temporary = RenderTexture.GetTemporary(main.pixelWidth, main.pixelHeight, 8, (RenderTextureFormat)0);
			Camera.main.targetTexture=(temporary);
			Camera.main.Render();
			Camera.main.targetTexture=(targetTexture);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active=(temporary);
			val.ReadPixels(new Rect(0f, 0f, (float)main.pixelWidth, (float)main.pixelHeight), 0, 0);
			val.Apply();
			RenderTexture.active=(active);
			temporary.Release();
			screenshotTakenAction(ImageConversion.EncodeToPNG(val));
		}

		private static void CaptureFullScreen(Action<byte[]> screenshotTakenAction)
		{
			if (DebugConsole.IsVisible)
			{
				DebugConsole.IsVisible = false;
				((MonoBehaviour)DebugConsole.Instance).StartCoroutine(WaitForCapture(screenshotTakenAction, showConsoleAfter: true));
			}
			else
			{
				((MonoBehaviour)DebugConsole.Instance).StartCoroutine(WaitForCapture(screenshotTakenAction, showConsoleAfter: false));
			}
		}

		private static IEnumerator WaitForCapture(Action<byte[]> screenshotTakenAction, bool showConsoleAfter)
		{
			yield return null;
			yield return (object)new WaitForEndOfFrame();
			Texture2D val = CaptureFullScreenTexture();
			if (showConsoleAfter)
			{
				DebugConsole.Instance.ShowConsoleInstantly();
			}
			screenshotTakenAction(ImageConversion.EncodeToPNG(val));
		}

		private static Texture2D CaptureFullScreenTexture()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			Texture2D val = new Texture2D(Screen.width, Screen.height, (TextureFormat)3, false);
			val.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0);
			val.Apply();
			return val;
		}
	}
}
