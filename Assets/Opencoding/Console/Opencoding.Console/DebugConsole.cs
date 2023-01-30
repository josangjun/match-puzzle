using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Opencoding.CommandHandlerSystem;
using Opencoding.Console.Scripts.UI.Utilities;
using Opencoding.Console.TouchDetectors;
using Opencoding.LogHistory;
using Opencoding.Shared.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Opencoding.Console
{
	public class DebugConsole : MonoBehaviour
	{
		private static readonly Vector2 OFF_SCREEN_MOUSE_POSITION = new Vector2(-1f, -1f);

		[SerializeField]
		private ImageFilesContainer _imageFiles;

		[SerializeField]
		private EmailTemplateFilesContainer _emailTemplateFiles;

		[SerializeField]
		private Settings _settings;

		private Opencoding.LogHistory.LogHistory _logHistory;

		private FilteredLogHistoryViewModel _logHistoryViewModel;

		private InputHistory _inputHistory;

		private HeaderBar _headerBar;

		private FilterBar _filterBar;

		private SettingsOverlay _settingsOverlay;

		private LogHistoryView _logHistoryView;

		private LogItemPopupMenu _logItemPopupMenu;

		private InputField _inputField;

		private CommandInformation _commandInformation;

		private Suggestions _suggestions;

		private HelpOverlay _helpOverlay;

		private TouchDetector _touchDetector;

		private List<string> _recentCommands = new List<string>();

		private int _previousScreenHeight;

		private int _previousScreenWidth;

		private bool _visible;

		private float _timescaleWhenConsoleOpened;

		private LogSearch _logSearch;

		public static Func<Email, bool> CompleteLogEmailPreprocessor;

		public static Func<IEnumerable<SaveFileData>> SaveFileProvider;

		public static Func<IEnumerable<KeyValuePair<string, string>>> GameInfoProvider;

		public static Func<bool> ConsoleAboutToOpen;

		public static Func<bool> ConsoleAboutToClose;

		private float _consoleTop;

		[NonSerialized]
		private bool _hasInitialized;

		private static MonoBehaviour _eventSystemWhenConsoleOpened = null;

		private static PropertyInfo _currentUGUIEventSystemProperty = null;

		private Rect _safeArea;

		private int _screenWidth;

		private int _screenHeight;

		private bool _showLogHistory;

		public static DebugConsole Instance { get; private set; }

		public bool ShowOnException { get; set; }

		public Settings Settings => _settings;

		public EmailTemplateFilesContainer EmailTemplateFiles => _emailTemplateFiles;

		public ImageFilesContainer ImageFiles => _imageFiles;

		public bool JustMadeVisible { get; set; }

		public static bool IsVisible
		{
			get
			{
				if (Instance != null)
				{
					return Instance._IsVisible;
				}
				return false;
			}
			set
			{
				if (Instance == null)
				{
					throw new InvalidOperationException("DebugConsole has not been loaded yet");
				}
				Instance._IsVisible = value;
			}
		}

		private bool _IsVisible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (!_visible && value)
				{
					if (ConsoleAboutToOpen == null || ConsoleAboutToOpen())
					{
						if (Settings.PauseGameWhenOpen)
						{
							_timescaleWhenConsoleOpened = Time.timeScale;
							Time.timeScale=0f;
						}
						DisableUGUIEventSystem();
						Instance.JustMadeVisible = true;
						Instance._visible = true;
						((MonoBehaviour)this).StartCoroutine(AnimateConsoleIn());
					}
				}
				else if (Instance._visible && !value && (ConsoleAboutToClose == null || ConsoleAboutToClose()))
				{
					if (Settings.PauseGameWhenOpen)
					{
						Time.timeScale=_timescaleWhenConsoleOpened;
					}
					Rect = default(Rect);
					EnableUGUIEventSystem();
					Instance.OnConsoleHidden();
					Instance._visible = false;
				}
			}
		}

		public bool IsMaximized { get; set; }

		public Rect Rect { get; private set; }

		public IEnumerable<string> RecentCommands => _recentCommands;

		public bool IsPopupMenuOpen => _logItemPopupMenu.IsVisible;

		public float ConsoleTop
		{
			get
			{
				return _consoleTop;
			}
			set
			{
				_consoleTop = value;
			}
		}

		public TouchDetector CustomTouchDetector
		{
			get
			{
				return _touchDetector;
			}
			set
			{
				if (PlatformUtils.IsMobile)
				{
					_touchDetector = value;
				}
			}
		}

		public int ConsoleWidth { get; set; }

		public float ActualConsoleWidth
		{
			get
			{
				if (ConsoleWidth != 0)
				{
					return ConsoleWidth;
				}
				return _safeArea.width;
			}
		}

		public float ConsoleLeft => _safeArea.xMin;

		public void ShowConsoleInstantly()
		{
			_IsVisible = true;
			_consoleTop = 0f;
		}

		private IEnumerator AnimateConsoleIn()
		{
			_consoleTop = 0f - GetConsoleHeight();
			float lastTime = Time.realtimeSinceStartup;
			while (true)
			{
				float num = Time.realtimeSinceStartup - lastTime;
				lastTime = Time.realtimeSinceStartup;
				_consoleTop = Mathf.Lerp(_consoleTop, 0f, num * 12f);
				if (_consoleTop >= -0.01f)
				{
					break;
				}
				yield return null;
			}
		}

		private static void CacheCurrentUGUIEventSystemProperty()
		{
			if ((object)_currentUGUIEventSystemProperty == null)
			{
				Type type = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
				if ((object)type != null)
				{
					_currentUGUIEventSystemProperty = type.GetProperty("current");
				}
			}
		}

		private static void DisableUGUIEventSystem()
		{
			if (!Instance.Settings.DisableUGUIWhenOpen)
			{
				return;
			}
			CacheCurrentUGUIEventSystemProperty();
			if ((object)_currentUGUIEventSystemProperty != null)
			{
				_eventSystemWhenConsoleOpened = (MonoBehaviour)_currentUGUIEventSystemProperty.GetGetMethod(nonPublic: true)!.Invoke(null, null);
				if (_eventSystemWhenConsoleOpened != null)
				{
					((Behaviour)_eventSystemWhenConsoleOpened).enabled=false;
				}
			}
		}

		private static void EnableUGUIEventSystem()
		{
			if (Instance.Settings.DisableUGUIWhenOpen)
			{
				CacheCurrentUGUIEventSystemProperty();
				if ((object)_currentUGUIEventSystemProperty != null && _eventSystemWhenConsoleOpened != null)
				{
					((Behaviour)_eventSystemWhenConsoleOpened).enabled=true;
				}
			}
		}

		public void SelectBuiltInTouchDetector(Settings.SelectedTouchDetector touchDetector)
		{
            if (PlatformUtils.IsMobile)
			{
				switch (touchDetector)
				{
				case Settings.SelectedTouchDetector.TWO_FINGER_SWIPE_DOWN:
					_touchDetector = new TwoFingerSwipeTouchDetector();
					break;
				case Settings.SelectedTouchDetector.THREE_FINGERS_HELD:
					_touchDetector = new ThreeFingersHeldTouchDetector();
					break;
				case Settings.SelectedTouchDetector.NONE:
					_touchDetector = null;
					break;
				}
			}
		}

		protected void Awake()
		{
            if (Instance != null)
			{
                Destroy(((Component)this).gameObject);
			}
			else
			{
                Initialize();
			}
		}

		private void Start()
		{
            UpdateSafeArea();
		}

		private void UpdateSafeArea()
		{
			if (_screenWidth != Screen.width || _screenHeight != Screen.height)
			{
				_screenWidth = Screen.width;
				_screenHeight = Screen.height;
				PropertyInfo property = typeof(Screen).GetProperty("safeArea", BindingFlags.Static | BindingFlags.Public);
				if ((object)property != null)
				{
					_safeArea = (Rect)property.GetValue(null, null);
				}
				else
				{
					_safeArea = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
				}
			}
		}

		private void Initialize()
		{
			if (_settings._DontDestroyOnLoad)
			{
				DontDestroyOnLoad(((Component)this).gameObject);
			}
			Instance = this;
			ShowOnException = _settings.ShowOnException;
			_logHistory = new Opencoding.LogHistory.LogHistory();
			_logHistoryViewModel = new FilteredLogHistoryViewModel(_logHistory);
			_logSearch = new LogSearch(_logHistoryViewModel);
			InitUIElements();
			SelectBuiltInTouchDetector(Settings.TouchDetector);
			_recentCommands = (from x in PlayerPrefs.GetString("Opencoding.Console.RecentConsoleCommands").Split(new char[1] { ',' })
				where x != ""
				select x).ToList();
			CommandHandlers.CommandExecuted += UpdateRecentCommandsListAfterCommandExecuted;
			_logHistory.LogHistoryItemAdded += OnLogHistoryItemAdded;
			_hasInitialized = true;

            InputSystem.onAnyButtonPress.Call(OnKeyPress);
        }

        private void OnKeyPress(InputControl ctrl)
        {
            string inputString = ctrl.displayName;
			if (inputString.Length == 1)
			{
				for (int i = 0; i < _settings.OpenAndCloseKeys.Length; i++)
				{
					char c = _settings.OpenAndCloseKeys[i];
					if (inputString[0] == c)
					{
						IsVisible = !IsVisible;
					}
				}
			}
        }

        private void InitUIElements()
		{
			_logItemPopupMenu = new LogItemPopupMenu(this);
			_logHistoryView = new LogHistoryView(_logHistoryViewModel, _logItemPopupMenu, this, _logSearch);
			_inputHistory = new InputHistory();
			_helpOverlay = new HelpOverlay(this);
			if (PlatformUtils.IsDesktop)
			{
				_inputField = new KeyboardInputField(_imageFiles, _inputHistory, _logHistoryView, _helpOverlay);
			}
			else
			{
				_inputField = new TouchScreenInputField(_inputHistory, _logHistoryView, _helpOverlay);
			}
			_commandInformation = new CommandInformation(this, _inputField);
			_suggestions = new Suggestions(_inputField);
			_filterBar = new FilterBar(this, _logHistoryViewModel, _logSearch, _logHistoryView);
			_settingsOverlay = new SettingsOverlay(this);
			_headerBar = new HeaderBar(this, _logHistoryViewModel, _inputField, _filterBar, _settingsOverlay, _logHistory, _settings);
		}

		protected void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
			if (_logHistory != null)
			{
				_logHistory.Dispose();
			}
			if (_helpOverlay != null)
			{
				_helpOverlay.Dispose();
			}
			if (_logHistoryView != null)
			{
				_logHistoryView.Dispose();
			}
			if (_logHistoryViewModel != null)
			{
				_logHistoryViewModel.Dispose();
			}
		}

		protected void Update()
		{
			if (!_hasInitialized)
			{
				Initialize();
			}
			if (Application.isEditor && Settings.HasChangedSinceLastCall())
			{
				GUIStyles.InvalidateSkin();
			}
			_filterBar.Update();
			_logHistory.Update();
			_logHistoryView.Update();
			if (_touchDetector != null && _touchDetector.Update())
			{
				IsVisible = true;
			}
			_suggestions.Update();
		}

		public static bool IsOnUnityVersionWithBrokenKeyboardInput()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			string[] array = Application.unityVersion.Split(new char[1] { 'p' });
			int result = 0;
			if (array.Length == 2)
			{
				int.TryParse(array[1], out result);
			}
			if ((int)Application.platform == 0 || (int)Application.platform == 1)
			{
				if (!Application.unityVersion.StartsWith("5.2.1") || result >= 3)
				{
					return Application.unityVersion.StartsWith("5.2.0");
				}
				return true;
			}
			return false;
		}

		private void UpdateRecentCommandsListAfterCommandExecuted(CommandHandler handler)
		{
			if (_settings.ShowRecentCommands)
			{
				string item = handler.CommandName.ToLower();
				_recentCommands.Remove(item);
				_recentCommands.Insert(0, item);
				if (_recentCommands.Count > 12)
				{
					_recentCommands.RemoveAt(_recentCommands.Count - 1);
				}
				PlayerPrefs.SetString("Opencoding.Console.RecentConsoleCommands", string.Join(",", _recentCommands.ToArray()));
				PlayerPrefs.Save();
			}
		}

		private void OnConsoleHidden()
		{
			_logHistoryView.AutoScrolling = true;
			TouchScreenKeyboardManager.Deactivate();
			HidePopups();
		}

		public void HidePopups()
		{
			_filterBar.IsVisible = false;
			_settingsOverlay.IsVisible = false;
			_helpOverlay.IsVisible = false;
			_logItemPopupMenu.IsVisible = false;
		}

		public void SetCommandInputTextAsIfTyped(string text)
		{
			_inputField.Input = text;
			_inputField.ConfirmInput();
		}

		private void OnGUI()
		{
			if (!IsVisible)
			{
				return;
			}
			if ((int)Event.current.type == 8)
			{
				TouchScreenKeyboardManager.Update();
			}
			if (_previousScreenWidth != Screen.width || _previousScreenHeight != Screen.height)
			{
				OnScreenRotated();
			}
			bool isVisible = _logItemPopupMenu.IsVisible;
			GUIStyles.BeginCustomSkin(_imageFiles, _settings, _safeArea);
			float actualConsoleWidth = ActualConsoleWidth;
			float consoleHeight = GetConsoleHeight();
			if (_helpOverlay.IsVisible)
			{
				_helpOverlay.OnGUI(new Rect(ConsoleLeft, _consoleTop + consoleHeight, actualConsoleWidth, (float)Screen.height - consoleHeight));
			}
			else
			{
				DrawSuggestionsUI();
			}
			Rect = new Rect(0f, _consoleTop, (float)Screen.width, consoleHeight);
			Rect rect;
			if ((int)Event.current.type == 8)
			{
				rect = Rect;
				_showLogHistory = (rect).height > GUIStyles.HeaderHeight * 2.9f;
			}
			GUI.Window(99, Rect, new GUI.WindowFunction(WindowFunc), "", GUIStyles.ConsoleWindowStyle);
			if (_logItemPopupMenu.IsVisible && _inputField.HasFocus)
			{
				_inputField.LoseFocus();
			}
			if (isVisible)
			{
				_logItemPopupMenu.OnGUI();
				if ((int)Event.current.type == 1)
				{
					rect = _logItemPopupMenu.Rect;
					if (!(rect).Contains(Event.current.mousePosition))
					{
						_logItemPopupMenu.IsVisible = false;
						Event.current.Use();
					}
				}
			}
			TouchScreenKeyboardManager.OnGUI();
			GUIStyles.EndCustomSkin();
		}

		private void DrawSuggestionsUI()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = _commandInformation.OnGUI();
			float num = ActualConsoleWidth - 80f * GUIStyles.ScaleFactor;
			float num2 = (float)Screen.height - val.y - 10f;
			if (PlatformUtils.IsMobile)
			{
				Rect? keyboardArea = TouchScreenKeyboardManager.KeyboardArea;
				if (keyboardArea.HasValue)
				{
					Rect val2 = keyboardArea.Value;
					if (val2.height > 0f)
					{
						float num3 = num;
						val2 = TouchScreenKeyboardManager.GetTouchScreenKeyboardCloseButtonRect();
						num = num3 - val2.width;
						float num4 = num2;
						val2 = keyboardArea.Value;
						num2 = num4 - val2.height;
					}
				}
			}
			_suggestions.Draw(new Rect(val.x, _consoleTop + val.y, num, num2));
		}

		public float GetConsoleHeight()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			float num = Screen.height;
			float num2 = (float)Screen.height / 2.5f;
			float num3 = GUIStyles.HeaderHeight * 3f;
			float num4 = (IsMaximized ? num : num2);
			if (!IsMaximized && _filterBar.IsVisible)
			{
				num4 += _filterBar.Height;
			}
			if (TouchScreenKeyboardManager.IsAnyInputActive && PlatformUtils.IsMobile)
			{
				Rect? keyboardArea = TouchScreenKeyboardManager.KeyboardArea;
				if (keyboardArea.HasValue)
				{
					Rect value = keyboardArea.Value;
					float num5 = (float)Screen.height - value.height - GUIStyles.SuggestionButtonHeight * 4f;
					if (num5 < num4)
					{
						num4 = ((!_filterBar.IsVisible || !(_filterBar.Height > num5 / 4f)) ? num5 : ((float)Screen.height - value.height));
					}
				}
			}
			if (num4 < num3)
			{
				num4 = GUIStyles.HeaderHeight * 2f;
			}
			return num4;
		}

		private void OnScreenRotated()
		{
			bool hasFocus = _inputField.HasFocus;
			_settingsOverlay.IsVisible = false;
			_logItemPopupMenu.IsVisible = false;
			_inputField.LoseFocus();
			_previousScreenWidth = Screen.width;
			_previousScreenHeight = Screen.height;
			UpdateSafeArea();
			GUIStyles.InvalidateSkin();
			_suggestions.ForceRelayoutNextUpdate();
			if (hasFocus)
			{
				_inputField.Focus();
			}
		}

		private void OnLogHistoryItemAdded(LogHistoryItem logHistoryItem)
		{
			if (logHistoryItem._Type == LogHistoryLogType.Exception && ShowOnException)
			{
				IsVisible = true;
			}
		}

		private void WindowFunc(int windowId)
		{
			bool isVisible = _settingsOverlay.IsVisible;
			Rect val;
			int num;
			if (_settingsOverlay.IsVisible)
			{
				val = _settingsOverlay.Rect;
				num = (val.Contains(Event.current.mousePosition) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag = (byte)num != 0;
			bool flag2 = !_logItemPopupMenu.IsVisible && !flag;
			if (!flag2)
			{
				if (Event.current.isKey || Event.current.isMouse || (int)Event.current.type == 6)
				{
					if (isVisible)
					{
						_settingsOverlay.OnGUI();
					}
					return;
				}
				Event.current.mousePosition=OFF_SCREEN_MOUSE_POSITION;
			}
			if (_settingsOverlay.IsVisible && (int)Event.current.type == 1)
			{
				val = _settingsOverlay.Rect;
				if (!val.Contains(Event.current.mousePosition))
				{
					val = _settingsOverlay.SettingsButtonRect;
					if (!val.Contains(Event.current.mousePosition))
					{
						_settingsOverlay.IsVisible = false;
						Event.current.Use();
					}
				}
			}
			if (_helpOverlay.IsVisible && (int)Event.current.type == 1)
			{
				val = _inputField.HelpButtonRect;
				if (!val.Contains(Event.current.mousePosition))
				{
					_helpOverlay.IsVisible = false;
				}
			}
			DrawMainWindow(windowId, flag2);
			if (isVisible)
			{
				_settingsOverlay.OnGUI();
			}
		}

		private void DrawMainWindow(int windowId, bool inFocus)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.BeginVertical(GUIStyles.ConsoleWindowBackgroundStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(ActualConsoleWidth) });
			_headerBar.OnGUI();
			_filterBar.OnGUI();
			if (_showLogHistory)
			{
				_logHistoryView.OnGUI(inFocus);
			}
			Widgets.HorizontalSeparator();
			_inputField.OnGUI(Rect);
			GUILayout.EndVertical();
			if (inFocus)
			{
				GUI.FocusWindow(windowId);
				GUI.BringWindowToFront(windowId);
			}
		}

		public void Minimize()
		{
			IsMaximized = false;
		}

		public string DetectCloseKeyInInputText(string input)
		{
			if (_settings.OpenAndCloseKeys == "")
			{
				return input;
			}
			string openAndCloseKeys = _settings.OpenAndCloseKeys;
			for (int i = 0; i < openAndCloseKeys.Length; i++)
			{
				input = input.Replace(openAndCloseKeys[i].ToString(CultureInfo.InvariantCulture), "");
			}
			return input;
		}

		public void Clear()
		{
			_logHistory.Clear();
			_logHistoryView.AutoScrolling = true;
		}

		public void EmailLog(Action<string> callback = null)
		{
			string filename = Application.temporaryCachePath + "/log_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".htm";
			_logHistory.Update();
			ExportLog(filename, captureScreenshot: true, delegate
			{
				EmailOrOpenLog(filename);
				if (callback != null)
				{
					callback(filename);
				}
			});
		}

		public void ExportLog(Stream stream, bool captureScreenshot, Action callback = null)
		{
			if (captureScreenshot)
			{
				ScreenshotCapture.CaptureScreenshot(Settings.LogScreenshotCaptureMode, delegate(byte[] data)
				{
					ExportLog(data, stream);
					if (callback != null)
					{
						callback();
					}
				});
			}
			else
			{
				ExportLog(null, stream);
			}
		}

		public void ExportLog(string filename, bool captureScreenshot, Action callback = null)
		{
			FileStream fs = new FileStream(filename, FileMode.Create);
			try
			{
				ExportLog(fs, captureScreenshot, delegate
				{
					fs.Close();
					if (callback != null)
					{
						callback();
					}
				});
			}
			catch (Exception)
			{
				fs.Close();
				throw;
			}
		}

		private void ExportLog(byte[] screenshotData, Stream stream)
		{
			LogHistoryEmailView logHistoryEmailView = new LogHistoryEmailView(EmailTemplateFiles._LogEmailHeaderTextAsset.text, EmailTemplateFiles._LogEmailFooterTextAsset.text, this);
			IEnumerable<SaveFileData> saveFiles = new SaveFileData[0];
			if (SaveFileProvider != null)
			{
				saveFiles = SaveFileProvider();
			}
			IEnumerable<KeyValuePair<string, string>> gameInfo = new KeyValuePair<string, string>[0];
			if (GameInfoProvider != null)
			{
				gameInfo = GameInfoProvider();
			}
			logHistoryEmailView.WriteToFile(stream, screenshotData, saveFiles, gameInfo);
		}

		private void EmailOrOpenLog(string filename)
		{
			if (Application.isEditor || (int)Application.platform == 2 || (int)Application.platform == 1 || (int)Application.platform == 13)
			{
				if ((int)Application.platform == 2 || (int)Application.platform == 7)
				{
					Application.OpenURL("file:///" + filename.Replace(" ", "%20"));
				}
				else
				{
					Application.OpenURL("file://" + filename.Replace(" ", "%20"));
				}
				return;
			}
			Email email = new Email
			{
				Subject = "Console Log",
				Message = "Log file is attached. If you're viewing this in Gmail, select the 'download' option - it will be unreadable with the 'view' option.",
				Attachments = new List<Email.Attachment>
				{
					new Email.Attachment(filename, "text/html")
				},
				ToAddress = Settings.DefaultToEmailAddress
			};
			if (CompleteLogEmailPreprocessor == null || CompleteLogEmailPreprocessor(email))
			{
				NativeMethodsInterface.SendEmail(email);
			}
		}
	}
}
