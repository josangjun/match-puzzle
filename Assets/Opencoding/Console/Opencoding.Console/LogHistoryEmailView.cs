using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Opencoding.LogHistory;
using UnityEngine;

namespace Opencoding.Console
{
	internal class LogHistoryEmailView
	{
		private readonly string _logEmailHeader;

		private readonly string _logEmailFooter;

		private readonly DebugConsole _debugConsole;

		public LogHistoryEmailView(string logEmailHeader, string logEmailFooter, DebugConsole debugConsole)
		{
			_logEmailHeader = logEmailHeader;
			_logEmailFooter = logEmailFooter;
			_debugConsole = debugConsole;
		}

		public void WriteToFile(Stream stream, byte[] screenshotData, IEnumerable<SaveFileData> saveFiles, IEnumerable<KeyValuePair<string, string>> gameInfo)
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.WriteLine(_logEmailHeader);
			Opencoding.LogHistory.LogHistory instance = Opencoding.LogHistory.LogHistory.Instance;
			DateTime gameStartTime = instance.GameStartTime;
			int num = 0;
			foreach (LogHistoryItem logItem in instance.LogItems)
			{
				num++;
				string text = "";
				if (logItem._Type == LogHistoryLogType.Warning)
				{
					text = "header-warning";
				}
				else if (logItem._Type == LogHistoryLogType.Error)
				{
					text = "header-error";
				}
				else if (logItem._Type == LogHistoryLogType.Exception)
				{
					text = "header-exception";
				}
				else if (logItem._Type == LogHistoryLogType.Assert)
				{
					text = "header-assert";
				}
				else if (logItem._Type == LogHistoryLogType.Log)
				{
					text = "header-info";
				}
				else if (logItem._Type == LogHistoryLogType.ConsoleInput)
				{
					text = "header-input";
				}
				string text2 = gameStartTime.AddSeconds(logItem._Time).ToString("G");
				streamWriter.WriteLine("<div class='log-row'><div class='header " + text + "'><span class='selected-marker'></span><div class='time' title='" + text2 + "'>" + logItem._Time + "</div><div class='header-text'>" + logItem._LogMessage.Trim() + "</div></div>");
				streamWriter.WriteLine("<div class='stacktrace'>" + logItem._StackTrace.Replace("\n", "<br>") + "</div></div>");
			}
			string newValue = "";
			if (gameInfo != null && gameInfo.Any())
			{
				newValue = ToHTMLTable(gameInfo);
			}
			StringBuilder stringBuilder = GenerateSaveFileBody(saveFiles);
			string value = _logEmailFooter.Replace("<!-- GENERAL INFORMATION -->", GenerateGeneralInformationSection()).Replace("<!-- HARDWARE INFORMATION -->", GenerateHardwareInformationSection()).Replace("<!-- SCREENSHOT -->", GenerateScreenshot(screenshotData))
				.Replace("<!-- SAVE FILE -->", stringBuilder.ToString())
				.Replace("<!-- GAME INFORMATION -->", newValue);
			streamWriter.WriteLine(value);
			streamWriter.Close();
		}

		private static StringBuilder GenerateSaveFileBody(IEnumerable<SaveFileData> saveFiles)
		{
			string text = "<div class='panel panel-default' id='panel<!-- ID -->'>\r\n  <div class='panel-heading'>\r\n    <h4 class='panel-title'>\r\n      <a data-toggle='collapse' data-target='#collapse<!-- ID -->' href='#collapse<!-- ID -->'><!-- TITLE --></a> <div style='float:right'><!-- DOWNLOAD_BUTTON --></div>\r\n    </h4>\r\n  </div>\r\n  <div id='collapse<!-- ID -->' class='panel-collapse collapse in'>\r\n      <div class='panel-body'><!-- BODY --></div>\r\n  </div>\r\n</div>";
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SaveFileData saveFile in saveFiles)
			{
				string mimeType = saveFile.GetMimeType();
				string text2 = "";
				text2 = "<button class='btn btn-secondary btn-sm' onclick=\"downloadURI('data: " + mimeType + ";base64," + Convert.ToBase64String(saveFile.Data) + "', '" + saveFile.Name + "');\">Download</button>";
				string newValue = "";
				switch (saveFile.DataType)
				{
				case SaveFileDataType.JSON:
					newValue = "<pre class='json' id='data" + num + "'>" + Encoding.UTF8.GetString(saveFile.Data) + "</pre>";
					break;
				case SaveFileDataType.TEXT:
					newValue = "<pre id='data" + num + "'>" + Encoding.UTF8.GetString(saveFile.Data) + "</pre>";
					break;
				default:
					throw new InvalidOperationException("Unrecognised save file data type " + saveFile.DataType);
				case SaveFileDataType.BINARY:
					break;
				}
				stringBuilder.Append(text.Replace("<!-- ID -->", num.ToString()).Replace("<!-- TITLE -->", saveFile.Name).Replace("<!-- BODY -->", newValue)).Replace("<!-- DOWNLOAD_BUTTON -->", text2);
				num++;
			}
			return stringBuilder;
		}

		private string GenerateScreenshot(byte[] screenshotData)
		{
			if (screenshotData == null)
			{
				return "";
			}
			return "<img src='data:image/png;base64," + Convert.ToBase64String(screenshotData) + "'/>";
		}

		private string GenerateHardwareInformationSection()
		{
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			List<KeyValuePair<string, string>> obj = new List<KeyValuePair<string, string>>
			{
                new KeyValuePair<string, string>("Operating System", SystemInfo.operatingSystem.ToString()),
                new KeyValuePair<string, string>("Processor Type", SystemInfo.processorType.ToString()),
                new KeyValuePair<string, string>("Processor Count", SystemInfo.processorCount.ToString()),
                new KeyValuePair<string, string>("System Memory Size", SystemInfo.systemMemorySize.ToString()),
                new KeyValuePair<string, string>("Graphics Memory Size", SystemInfo.graphicsMemorySize.ToString()),
                new KeyValuePair<string, string>("Graphics Device Name", SystemInfo.graphicsDeviceName.ToString()),
                new KeyValuePair<string, string>("Graphics Device Vendor", SystemInfo.graphicsDeviceVendor.ToString()),
                new KeyValuePair<string, string>("Graphics Device ID", SystemInfo.graphicsDeviceID.ToString()),
                new KeyValuePair<string, string>("Graphics Device Vendor ID", SystemInfo.graphicsDeviceVendorID.ToString()),
                new KeyValuePair<string, string>("Graphics Device Version", SystemInfo.graphicsDeviceVersion.ToString()),
                new KeyValuePair<string, string>("Graphics Shader Level", SystemInfo.graphicsShaderLevel.ToString()),
                new KeyValuePair<string, string>("Supports Shadows", SystemInfo.supportsShadows.ToString()),
                //new KeyValuePair<string, string>("Supports Render To Cubemap", SystemInfo.supportsRenderToCubemap.ToString()),
                //new KeyValuePair<string, string>("Supports Image Effects", SystemInfo.supportsImageEffects.ToString()),
                new KeyValuePair<string, string>("Supports 3D Textures", SystemInfo.supports3DTextures.ToString()),
                new KeyValuePair<string, string>("Supports Compute Shaders", SystemInfo.supportsComputeShaders.ToString()),
                new KeyValuePair<string, string>("Supports Instancing", SystemInfo.supportsInstancing.ToString()),
                new KeyValuePair<string, string>("Supports Sparse Textures", SystemInfo.supportsSparseTextures.ToString()),
                new KeyValuePair<string, string>("Supported Render Target Count", SystemInfo.supportedRenderTargetCount.ToString())
            };
            NPOTSupport npotSupport = SystemInfo.npotSupport;
            obj.Add(new KeyValuePair<string, string>("Npot Support", npotSupport.ToString()));
            obj.Add(new KeyValuePair<string, string>("Device Name", SystemInfo.deviceName.ToString()));
            obj.Add(new KeyValuePair<string, string>("Device Model", SystemInfo.deviceModel.ToString()));
            obj.Add(new KeyValuePair<string, string>("Supports Accelerometer", SystemInfo.supportsAccelerometer.ToString()));
            obj.Add(new KeyValuePair<string, string>("Supports Gyroscope", SystemInfo.supportsGyroscope.ToString()));
            obj.Add(new KeyValuePair<string, string>("Supports Location Service", SystemInfo.supportsLocationService.ToString()));
            obj.Add(new KeyValuePair<string, string>("Supports Vibration", SystemInfo.supportsVibration.ToString()));
            DeviceType deviceType = SystemInfo.deviceType;
            obj.Add(new KeyValuePair<string, string>("Device Type", deviceType.ToString()));
            obj.Add(new KeyValuePair<string, string>("Max Texture Size", SystemInfo.maxTextureSize.ToString()));
            List<KeyValuePair<string, string>> data = obj;
            return ToHTMLTable(data);
        }

		private string ToFriendlyName(string unfriendlyName)
		{
			string text = "";
			for (int i = 0; i < unfriendlyName.Length; i++)
			{
				if (char.IsUpper(unfriendlyName[i]))
				{
					text += " ";
				}
				text = ((!(text == "")) ? (text + char.ToLower(unfriendlyName[i])) : (text + char.ToUpper(unfriendlyName[i])));
			}
			return text.Replace("i d", "ID").Replace("3 d", " 3D");
		}

		private string GenerateGeneralInformationSection()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			list.Add(new KeyValuePair<string, string>("Game version", string.IsNullOrEmpty(_debugConsole.Settings.GameVersion) ? "&lt;not set&gt;" : _debugConsole.Settings.GameVersion));
			RuntimePlatform platform = Application.platform;
			list.Add(new KeyValuePair<string, string>("Platform", platform.ToString()));
			if (!string.IsNullOrEmpty(Environment.UserName))
			{
				list.Add(new KeyValuePair<string, string>("Username", Environment.UserName));
			}
			return ToHTMLTable(list);
		}

		private string ToHTMLTable(IEnumerable<KeyValuePair<string, string>> data)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("<table class='table'><tbody>");
			foreach (KeyValuePair<string, string> datum in data)
			{
				stringBuilder.AppendLine("<tr><td>" + datum.Key + "</td><td>" + datum.Value + "</td></tr>");
			}
			stringBuilder.AppendLine("</tbody></table>");
			return stringBuilder.ToString();
		}
	}
}
