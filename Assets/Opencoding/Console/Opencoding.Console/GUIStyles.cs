using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
    public static class GUIStyles
    {
        private static GUISkin _customGUISkin;

        private static GUISkin _defaultGUISkin;

        private static float _nativeScreenScaleFactor = -1f;

        public static GUIStyle ExceptionLabelStyle { get; private set; }

        public static GUIStyle AssertLabelStyle { get; private set; }

        public static GUIStyle ErrorLabelStyle { get; private set; }

        public static GUIStyle InfoLabelStyle { get; private set; }

        public static GUIStyle WarningLabelStyle { get; private set; }

        public static GUIStyle CommandInformationParameterNormalStyle { get; private set; }

        public static GUIStyle CommandInformationParameterHighlightedStyle { get; private set; }

        public static GUIStyle SuggestionButtonBackgroundStyle { get; private set; }

        public static GUIStyle SuggestionButtonMoreBackgroundStyle { get; private set; }

        public static GUIStyle ConsoleWindowStyle { get; private set; }

        public static GUIStyle ConsoleWindowBackgroundStyle { get; private set; }

        public static GUIStyle CommandDescriptionBackgroundStyle { get; private set; }

        public static GUIStyle HeaderStyle { get; private set; }

        public static GUIStyle HeaderButtonLabelStyle { get; private set; }

        public static GUIStyle ItemAlternateBackgroundStyle { get; private set; }

        public static GUIStyle HighlightedItemBackgroundStyle { get; private set; }

        public static GUIStyle SearchResultItemBackgroundStyle { get; private set; }

        public static GUIStyle OverlayWindowStyle { get; private set; }

        public static GUIStyle FilterTextFieldStyle { get; private set; }

        public static GUIStyle SettingsPanelButtonStyle { get; private set; }

        public static GUIStyle FilterTextFieldHelpOverlayStyle { get; private set; }

        public static GUIStyle InputTextFieldStyle { get; private set; }

        public static GUIStyle LogHistoryItemTextAreaStyle { get; private set; }

        public static GUIStyle SearchTextFieldStyle { get; private set; }

        public static GUIStyle SearchTextFieldCountStyle { get; private set; }

        public static GUIStyle SearchFilterToolBarStyle { get; private set; }

        public static GUIStyle SelectedSearchItemBackgroundStyle { get; private set; }

        public static GUIStyle PopupMenuButtonLabelStyle { get; private set; }

        public static GUIStyle HorizontalPopupMenuButtonLabelStyle { get; private set; }

        public static GUIStyle PopupMenuWindowStyle { get; private set; }

        public static GUIStyle HelpWindowParameterListStyle { get; private set; }

        public static GUIStyle HelpWindowHelpStyle { get; private set; }

        public static GUIStyle HelpItemBackgroundStyle { get; private set; }

        public static GUIStyle HelpWindowDescriptionStyle { get; private set; }

        public static GUIStyle HelpWindowBackgroundStyle { get; private set; }

        public static float ScaleFactor { get; private set; }

        public static int ConsoleRowHeight { get; private set; }

        public static int ConsoleRowTextLeftMargin { get; private set; }

        public static float HeaderHeight { get; private set; }

        public static float SuggestionButtonHeight { get; set; }

        public static Texture2D BlackTexture { get; private set; }

        public static void SetUpStyles(ImageFilesContainer imageFiles, Settings settings, Rect safeArea, bool force = false)
        {
            //IL_0193: Unknown result type (might be due to invalid IL or missing references)
            //IL_019d: Expected O, but got Unknown
            //IL_01ac: Unknown result type (might be due to invalid IL or missing references)
            //IL_01b1: Unknown result type (might be due to invalid IL or missing references)
            //IL_01ba: Unknown result type (might be due to invalid IL or missing references)
            //IL_01c4: Expected O, but got Unknown
            //IL_01c4: Unknown result type (might be due to invalid IL or missing references)
            //IL_01d0: Expected O, but got Unknown
            //IL_01f3: Unknown result type (might be due to invalid IL or missing references)
            //IL_01fd: Expected O, but got Unknown
            //IL_020c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0216: Expected O, but got Unknown
            //IL_0225: Unknown result type (might be due to invalid IL or missing references)
            //IL_022f: Expected O, but got Unknown
            //IL_023e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0248: Expected O, but got Unknown
            //IL_0257: Unknown result type (might be due to invalid IL or missing references)
            //IL_0261: Expected O, but got Unknown
            //IL_0270: Unknown result type (might be due to invalid IL or missing references)
            //IL_027a: Expected O, but got Unknown
            //IL_0289: Unknown result type (might be due to invalid IL or missing references)
            //IL_028e: Unknown result type (might be due to invalid IL or missing references)
            //IL_029a: Expected O, but got Unknown
            //IL_02a9: Unknown result type (might be due to invalid IL or missing references)
            //IL_02b3: Expected O, but got Unknown
            //IL_02c2: Unknown result type (might be due to invalid IL or missing references)
            //IL_02c7: Unknown result type (might be due to invalid IL or missing references)
            //IL_02d3: Expected O, but got Unknown
            //IL_02e2: Unknown result type (might be due to invalid IL or missing references)
            //IL_02e7: Unknown result type (might be due to invalid IL or missing references)
            //IL_02f3: Expected O, but got Unknown
            //IL_0302: Unknown result type (might be due to invalid IL or missing references)
            //IL_0307: Unknown result type (might be due to invalid IL or missing references)
            //IL_0313: Expected O, but got Unknown
            //IL_0322: Unknown result type (might be due to invalid IL or missing references)
            //IL_0327: Unknown result type (might be due to invalid IL or missing references)
            //IL_0334: Expected O, but got Unknown
            //IL_0343: Unknown result type (might be due to invalid IL or missing references)
            //IL_034d: Expected O, but got Unknown
            //IL_035c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0366: Expected O, but got Unknown
            //IL_0375: Unknown result type (might be due to invalid IL or missing references)
            //IL_037f: Expected O, but got Unknown
            //IL_039f: Unknown result type (might be due to invalid IL or missing references)
            //IL_03a9: Expected O, but got Unknown
            //IL_03b8: Unknown result type (might be due to invalid IL or missing references)
            //IL_03c2: Expected O, but got Unknown
            //IL_03d1: Unknown result type (might be due to invalid IL or missing references)
            //IL_03db: Expected O, but got Unknown
            //IL_03fa: Unknown result type (might be due to invalid IL or missing references)
            //IL_03ff: Unknown result type (might be due to invalid IL or missing references)
            //IL_0404: Unknown result type (might be due to invalid IL or missing references)
            //IL_040e: Expected O, but got Unknown
            //IL_040e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0413: Unknown result type (might be due to invalid IL or missing references)
            //IL_041d: Expected O, but got Unknown
            //IL_041d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0424: Unknown result type (might be due to invalid IL or missing references)
            //IL_042b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0432: Unknown result type (might be due to invalid IL or missing references)
            //IL_0438: Expected O, but got Unknown
            //IL_0433: Unknown result type (might be due to invalid IL or missing references)
            //IL_0438: Unknown result type (might be due to invalid IL or missing references)
            //IL_044b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0450: Unknown result type (might be due to invalid IL or missing references)
            //IL_045f: Expected O, but got Unknown
            //IL_045f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0465: Expected O, but got Unknown
            //IL_0460: Unknown result type (might be due to invalid IL or missing references)
            //IL_0465: Unknown result type (might be due to invalid IL or missing references)
            //IL_047c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0481: Unknown result type (might be due to invalid IL or missing references)
            //IL_0490: Expected O, but got Unknown
            //IL_0490: Unknown result type (might be due to invalid IL or missing references)
            //IL_0496: Expected O, but got Unknown
            //IL_0491: Unknown result type (might be due to invalid IL or missing references)
            //IL_0496: Unknown result type (might be due to invalid IL or missing references)
            //IL_049c: Unknown result type (might be due to invalid IL or missing references)
            //IL_04ab: Expected O, but got Unknown
            //IL_04ab: Unknown result type (might be due to invalid IL or missing references)
            //IL_04b1: Expected O, but got Unknown
            //IL_04ac: Unknown result type (might be due to invalid IL or missing references)
            //IL_04b1: Unknown result type (might be due to invalid IL or missing references)
            //IL_04b7: Unknown result type (might be due to invalid IL or missing references)
            //IL_04c6: Expected O, but got Unknown
            //IL_04c6: Unknown result type (might be due to invalid IL or missing references)
            //IL_04cc: Expected O, but got Unknown
            //IL_04c7: Unknown result type (might be due to invalid IL or missing references)
            //IL_04cc: Unknown result type (might be due to invalid IL or missing references)
            //IL_04d2: Unknown result type (might be due to invalid IL or missing references)
            //IL_04e1: Expected O, but got Unknown
            //IL_04e1: Unknown result type (might be due to invalid IL or missing references)
            //IL_04e7: Expected O, but got Unknown
            //IL_04e2: Unknown result type (might be due to invalid IL or missing references)
            //IL_04e7: Unknown result type (might be due to invalid IL or missing references)
            //IL_04ee: Unknown result type (might be due to invalid IL or missing references)
            //IL_04f3: Unknown result type (might be due to invalid IL or missing references)
            //IL_04fd: Expected O, but got Unknown
            //IL_04fd: Unknown result type (might be due to invalid IL or missing references)
            //IL_0514: Expected O, but got Unknown
            //IL_0514: Unknown result type (might be due to invalid IL or missing references)
            //IL_051a: Expected O, but got Unknown
            //IL_0515: Unknown result type (might be due to invalid IL or missing references)
            //IL_051a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0521: Unknown result type (might be due to invalid IL or missing references)
            //IL_0552: Unknown result type (might be due to invalid IL or missing references)
            //IL_055c: Expected O, but got Unknown
            //IL_055c: Unknown result type (might be due to invalid IL or missing references)
            //IL_056e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0588: Unknown result type (might be due to invalid IL or missing references)
            //IL_0597: Unknown result type (might be due to invalid IL or missing references)
            //IL_059d: Unknown result type (might be due to invalid IL or missing references)
            //IL_05ac: Expected O, but got Unknown
            //IL_05b1: Unknown result type (might be due to invalid IL or missing references)
            //IL_05b6: Unknown result type (might be due to invalid IL or missing references)
            //IL_05bd: Unknown result type (might be due to invalid IL or missing references)
            //IL_05ee: Unknown result type (might be due to invalid IL or missing references)
            //IL_05f8: Expected O, but got Unknown
            //IL_05fd: Expected O, but got Unknown
            //IL_05fd: Unknown result type (might be due to invalid IL or missing references)
            //IL_0603: Expected O, but got Unknown
            //IL_05fe: Unknown result type (might be due to invalid IL or missing references)
            //IL_0603: Unknown result type (might be due to invalid IL or missing references)
            //IL_060a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0610: Unknown result type (might be due to invalid IL or missing references)
            //IL_061a: Expected O, but got Unknown
            //IL_061a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0621: Unknown result type (might be due to invalid IL or missing references)
            //IL_0628: Unknown result type (might be due to invalid IL or missing references)
            //IL_0634: Expected O, but got Unknown
            //IL_0634: Unknown result type (might be due to invalid IL or missing references)
            //IL_063a: Expected O, but got Unknown
            //IL_0635: Unknown result type (might be due to invalid IL or missing references)
            //IL_063a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0641: Unknown result type (might be due to invalid IL or missing references)
            //IL_0646: Unknown result type (might be due to invalid IL or missing references)
            //IL_0650: Expected O, but got Unknown
            //IL_0650: Unknown result type (might be due to invalid IL or missing references)
            //IL_0657: Unknown result type (might be due to invalid IL or missing references)
            //IL_0663: Expected O, but got Unknown
            //IL_066d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0672: Unknown result type (might be due to invalid IL or missing references)
            //IL_0679: Unknown result type (might be due to invalid IL or missing references)
            //IL_067f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0689: Expected O, but got Unknown
            //IL_068e: Expected O, but got Unknown
            //IL_0698: Unknown result type (might be due to invalid IL or missing references)
            //IL_069d: Unknown result type (might be due to invalid IL or missing references)
            //IL_06b7: Unknown result type (might be due to invalid IL or missing references)
            //IL_06c1: Unknown result type (might be due to invalid IL or missing references)
            //IL_06d2: Unknown result type (might be due to invalid IL or missing references)
            //IL_06d7: Unknown result type (might be due to invalid IL or missing references)
            //IL_06e1: Expected O, but got Unknown
            //IL_06e1: Unknown result type (might be due to invalid IL or missing references)
            //IL_06e8: Unknown result type (might be due to invalid IL or missing references)
            //IL_06ed: Unknown result type (might be due to invalid IL or missing references)
            //IL_06f7: Expected O, but got Unknown
            //IL_06f7: Unknown result type (might be due to invalid IL or missing references)
            //IL_06fe: Unknown result type (might be due to invalid IL or missing references)
            //IL_070a: Expected O, but got Unknown
            //IL_070f: Expected O, but got Unknown
            //IL_070a: Unknown result type (might be due to invalid IL or missing references)
            //IL_070f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0716: Unknown result type (might be due to invalid IL or missing references)
            //IL_071b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0725: Expected O, but got Unknown
            //IL_0725: Unknown result type (might be due to invalid IL or missing references)
            //IL_073f: Unknown result type (might be due to invalid IL or missing references)
            //IL_074e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0755: Unknown result type (might be due to invalid IL or missing references)
            //IL_075c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0768: Expected O, but got Unknown
            //IL_076d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0772: Unknown result type (might be due to invalid IL or missing references)
            //IL_078c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0796: Unknown result type (might be due to invalid IL or missing references)
            //IL_07b0: Unknown result type (might be due to invalid IL or missing references)
            //IL_07bf: Unknown result type (might be due to invalid IL or missing references)
            //IL_07c4: Unknown result type (might be due to invalid IL or missing references)
            //IL_07ce: Expected O, but got Unknown
            //IL_07ce: Unknown result type (might be due to invalid IL or missing references)
            //IL_07d5: Unknown result type (might be due to invalid IL or missing references)
            //IL_07dc: Unknown result type (might be due to invalid IL or missing references)
            //IL_07e8: Expected O, but got Unknown
            //IL_07f2: Unknown result type (might be due to invalid IL or missing references)
            //IL_07f7: Unknown result type (might be due to invalid IL or missing references)
            //IL_07fc: Unknown result type (might be due to invalid IL or missing references)
            //IL_0806: Expected O, but got Unknown
            //IL_0806: Unknown result type (might be due to invalid IL or missing references)
            //IL_0820: Unknown result type (might be due to invalid IL or missing references)
            //IL_082f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0836: Unknown result type (might be due to invalid IL or missing references)
            //IL_083d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0849: Expected O, but got Unknown
            //IL_0853: Unknown result type (might be due to invalid IL or missing references)
            //IL_0858: Unknown result type (might be due to invalid IL or missing references)
            //IL_085d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0867: Expected O, but got Unknown
            //IL_0867: Unknown result type (might be due to invalid IL or missing references)
            //IL_0870: Unknown result type (might be due to invalid IL or missing references)
            //IL_087a: Expected O, but got Unknown
            //IL_087a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0894: Unknown result type (might be due to invalid IL or missing references)
            //IL_089e: Unknown result type (might be due to invalid IL or missing references)
            //IL_08af: Unknown result type (might be due to invalid IL or missing references)
            //IL_08b6: Unknown result type (might be due to invalid IL or missing references)
            //IL_08bb: Unknown result type (might be due to invalid IL or missing references)
            //IL_08c5: Expected O, but got Unknown
            //IL_08c5: Unknown result type (might be due to invalid IL or missing references)
            //IL_08cc: Unknown result type (might be due to invalid IL or missing references)
            //IL_08d8: Expected O, but got Unknown
            //IL_08e2: Unknown result type (might be due to invalid IL or missing references)
            //IL_08e7: Unknown result type (might be due to invalid IL or missing references)
            //IL_08ec: Unknown result type (might be due to invalid IL or missing references)
            //IL_08f6: Expected O, but got Unknown
            //IL_08f6: Unknown result type (might be due to invalid IL or missing references)
            //IL_0910: Unknown result type (might be due to invalid IL or missing references)
            //IL_091a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0934: Unknown result type (might be due to invalid IL or missing references)
            //IL_0943: Unknown result type (might be due to invalid IL or missing references)
            //IL_094a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0956: Expected O, but got Unknown
            //IL_0960: Unknown result type (might be due to invalid IL or missing references)
            //IL_0965: Unknown result type (might be due to invalid IL or missing references)
            //IL_096a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0974: Expected O, but got Unknown
            //IL_0974: Unknown result type (might be due to invalid IL or missing references)
            //IL_099c: Unknown result type (might be due to invalid IL or missing references)
            //IL_09a6: Expected O, but got Unknown
            //IL_09a6: Unknown result type (might be due to invalid IL or missing references)
            //IL_09c0: Unknown result type (might be due to invalid IL or missing references)
            //IL_09cf: Unknown result type (might be due to invalid IL or missing references)
            //IL_09db: Expected O, but got Unknown
            //IL_09e5: Unknown result type (might be due to invalid IL or missing references)
            //IL_09ea: Unknown result type (might be due to invalid IL or missing references)
            //IL_09ef: Unknown result type (might be due to invalid IL or missing references)
            //IL_09f9: Expected O, but got Unknown
            //IL_09f9: Unknown result type (might be due to invalid IL or missing references)
            //IL_09fe: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a08: Expected O, but got Unknown
            //IL_0a08: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a22: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a31: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a3d: Expected O, but got Unknown
            //IL_0a42: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a47: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a62: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a6c: Expected O, but got Unknown
            //IL_0a6c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a86: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a9a: Expected O, but got Unknown
            //IL_0a9a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0a9f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ab9: Unknown result type (might be due to invalid IL or missing references)
            //IL_0acd: Expected O, but got Unknown
            //IL_0acd: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ad2: Unknown result type (might be due to invalid IL or missing references)
            //IL_0aec: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b00: Expected O, but got Unknown
            //IL_0b00: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b05: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b1f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b33: Expected O, but got Unknown
            //IL_0b33: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b38: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b52: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b66: Expected O, but got Unknown
            //IL_0b66: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b6b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b81: Expected O, but got Unknown
            //IL_0b8b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b90: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b95: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b9f: Expected O, but got Unknown
            //IL_0b9f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bb1: Expected O, but got Unknown
            //IL_0bb6: Expected O, but got Unknown
            //IL_0bcb: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bd0: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bd5: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bdf: Expected O, but got Unknown
            //IL_0bdf: Unknown result type (might be due to invalid IL or missing references)
            //IL_0be4: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bee: Expected O, but got Unknown
            //IL_0bf3: Expected O, but got Unknown
            //IL_0bfd: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c02: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c09: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c24: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c2e: Expected O, but got Unknown
            //IL_0c2e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c45: Expected O, but got Unknown
            //IL_0c4f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c54: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c5a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c64: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c74: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c84: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c8b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c91: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ca0: Expected O, but got Unknown
            //IL_0caa: Unknown result type (might be due to invalid IL or missing references)
            //IL_0caf: Unknown result type (might be due to invalid IL or missing references)
            //IL_0cb6: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ccd: Expected O, but got Unknown
            //IL_0cd7: Unknown result type (might be due to invalid IL or missing references)
            //IL_0cdc: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ce3: Unknown result type (might be due to invalid IL or missing references)
            //IL_0cf3: Unknown result type (might be due to invalid IL or missing references)
            //IL_0cf9: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d08: Expected O, but got Unknown
            //IL_0d12: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d17: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d1e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d39: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d43: Expected O, but got Unknown
            //IL_0d43: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d55: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d5a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d64: Expected O, but got Unknown
            //IL_0d69: Expected O, but got Unknown
            //IL_0d73: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d78: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d88: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d8d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0d97: Expected O, but got Unknown
            //IL_0d9c: Expected O, but got Unknown
            //IL_0da6: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dab: Unknown result type (might be due to invalid IL or missing references)
            //IL_0db2: Unknown result type (might be due to invalid IL or missing references)
            //IL_0db3: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dbd: Expected O, but got Unknown
            //IL_0dbd: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dbe: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dc8: Expected O, but got Unknown
            //IL_0dc8: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dd4: Expected O, but got Unknown
            //IL_0dd4: Unknown result type (might be due to invalid IL or missing references)
            //IL_0dd9: Unknown result type (might be due to invalid IL or missing references)
            //IL_0e0a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0e14: Expected O, but got Unknown
            //IL_0e19: Expected O, but got Unknown
            //IL_0e1e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0e2d: Unknown result type (might be due to invalid IL or missing references)
            if (_customGUISkin != null && !force)
            {
                return;
            }
            if (_nativeScreenScaleFactor < 0f)
            {
                _nativeScreenScaleFactor = NativeMethodsInterface.GetNativeScreenScaleFactor();
            }
            int num = (int)Screen.dpi;
            if (Application.isEditor || num == 0)
            {
                num = 120;
            }
            ScaleFactor = (float)num / 440f;
            if (!Application.isEditor && ((double)((float)Screen.width / Screen.dpi) < 2.5 || (double)((float)Screen.height / Screen.dpi) < 2.5))
            {
                ScaleFactor *= 0.75f;
            }
            if (Application.isEditor)
            {
                ScaleFactor *= settings.EditorScaleFactor;
            }
            else if (PlatformUtils.IsMobile)
            {
                if (Screen.width > Screen.height)
                {
                    ScaleFactor *= settings.MobileScaleFactorLandscape;
                }
                else
                {
                    ScaleFactor *= settings.MobileScaleFactorPortrait;
                }
            }
            else
            {
                ScaleFactor *= settings.StandaloneScaleFactor;
            }
            ScaleFactor *= _nativeScreenScaleFactor;
            int num2 = (int)(50f * ScaleFactor);
            int num3 = (int)(50f * ScaleFactor);
            ConsoleRowHeight = (int)((float)num2 + 30f * ScaleFactor);
            ConsoleRowTextLeftMargin = (int)(10f * ScaleFactor);
            HeaderHeight = (float)ConsoleRowHeight * 1.3f;
            SuggestionButtonHeight = ScaleFactor * 100f;
            if (Application.isEditor)
            {
                SuggestionButtonHeight *= 1.05f;
            }
            _customGUISkin = ScriptableObject.CreateInstance<GUISkin>();
            _customGUISkin.box = new GUIStyle(GUI.skin.box);
            GUISkin customGUISkin = _customGUISkin;
            GUIStyle val = new GUIStyle(GUI.skin.button);
            val.padding = new RectOffset(10, 10, 10, 10);
            val.fontSize = num2;
            customGUISkin.button = val;
            _customGUISkin.font = GUI.skin.font;
            _customGUISkin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
            _customGUISkin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
            _customGUISkin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
            _customGUISkin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
            _customGUISkin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
            _customGUISkin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            GUISkin customGUISkin2 = _customGUISkin;
            GUIStyle val2 = new GUIStyle(GUI.skin.label);
            val2.fontSize = num2;
            customGUISkin2.label = val2;
            _customGUISkin.scrollView = new GUIStyle(GUI.skin.scrollView);
            GUISkin customGUISkin3 = _customGUISkin;
            GUIStyle val3 = new GUIStyle(GUI.skin.textArea);
            val3.fontSize = num2;
            customGUISkin3.textArea = val3;
            GUISkin customGUISkin4 = _customGUISkin;
            GUIStyle val4 = new GUIStyle(GUI.skin.textField);
            val4.fontSize = num2;
            customGUISkin4.textField = val4;
            GUISkin customGUISkin5 = _customGUISkin;
            GUIStyle val5 = new GUIStyle(GUI.skin.toggle);
            val5.fontSize = num2;
            customGUISkin5.toggle = val5;
            GUISkin customGUISkin6 = _customGUISkin;
            GUIStyle val6 = new GUIStyle(GUI.skin.verticalScrollbar);
            val6.fixedWidth = (float)num3;
            customGUISkin6.verticalScrollbar = val6;
            _customGUISkin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            _customGUISkin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            _customGUISkin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            _customGUISkin.verticalScrollbarThumb.fixedWidth = (float)num3;
            _customGUISkin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
            _customGUISkin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
            _customGUISkin.window = new GUIStyle(GUI.skin.window);
            SetBackgroundForAllStyleStates(_customGUISkin.verticalScrollbarThumb, imageFiles._ScrollbarThumb);
            GUIStyle val7 = new GUIStyle(GUI.skin.label);
            val7.padding = new RectOffset(0, 0, 0, 0);
            val7.margin = new RectOffset(0, 0, 0, 0);
            val7.fontSize = num2;
            val7.wordWrap = false;
            val7.alignment = (TextAnchor)3;
            GUIStyle val8 = new GUIStyle(val7);
            val8.normal.textColor = new Color32(byte.MaxValue, (byte)0, (byte)108, byte.MaxValue);
            ExceptionLabelStyle = val8;
            GUIStyle val9 = new GUIStyle(val7);
            val9.normal.textColor = new Color32((byte)48, (byte)163, byte.MaxValue, byte.MaxValue);
            AssertLabelStyle = val9;
            GUIStyle val10 = new GUIStyle(val7);
            val10.normal.textColor = Color.red;
            ErrorLabelStyle = val10;
            GUIStyle val11 = new GUIStyle(val7);
            val11.normal.textColor = Color.white;
            InfoLabelStyle = val11;
            GUIStyle val12 = new GUIStyle(val7);
            val12.normal.textColor = Color.yellow;
            WarningLabelStyle = val12;
            GUIStyle val13 = new GUIStyle(val7);
            val13.stretchHeight = true;
            val13.padding = new RectOffset(5, 5, 3, 3);
            val13.fontSize = (int)(HeaderHeight * 0.5f);
            HeaderButtonLabelStyle = val13;
            GUIStyle val14 = new GUIStyle(val7);
            val14.stretchHeight = true;
            val14.padding = new RectOffset((int)(50f * ScaleFactor), (int)(25f * ScaleFactor), (int)(15f * ScaleFactor), (int)(15f * ScaleFactor));
            val14.fontSize = (int)(HeaderHeight * 0.5f);
            val14.hover.background = UIUtilities.CreateTexture(new Color(0f, 0f, 0.6f, 0.4f));
            val14.hover.textColor = Color.white;
            PopupMenuButtonLabelStyle = val14;
            GUIStyle val15 = new GUIStyle(PopupMenuButtonLabelStyle);
            val15.alignment = (TextAnchor)4;
            val15.padding = new RectOffset((int)(25f * ScaleFactor), (int)(25f * ScaleFactor), (int)(15f * ScaleFactor), (int)(15f * ScaleFactor));
            HorizontalPopupMenuButtonLabelStyle = val15;
            GUIStyle val16 = new GUIStyle(val7);
            val16.fontSize = num2;
            val16.padding = new RectOffset(8, 8, 12, 8);
            val16.richText = true;
            val16.alignment = (TextAnchor)3;
            val16.wordWrap = true;
            HelpWindowParameterListStyle = val16;
            GUIStyle val17 = new GUIStyle(val7);
            val17.fontSize = num2;
            val17.padding = new RectOffset(8, 8, 8, 8);
            val17.fontStyle = (FontStyle)1;
            val17.wordWrap = true;
            HelpWindowHelpStyle = val17;
            GUIStyle val18 = new GUIStyle(GUI.skin.label);
            val18.fontSize = num2;
            val18.padding = new RectOffset(30, 2, 2, 2);
            HelpWindowDescriptionStyle = val18;
            GUIStyle val19 = new GUIStyle(GUI.skin.label);
            val19.normal.textColor = new Color(1f, 1f, 1f, 1f);
            val19.normal.background = imageFiles._SuggestionButtonBackground;
            val19.padding = new RectOffset(0, 0, 0, 0);
            val19.alignment = (TextAnchor)3;
            val19.border = new RectOffset(7, 7, 7, 7);
            val19.fontSize = num2;
            val19.wordWrap = false;
            HelpItemBackgroundStyle = val19;
            GUIStyle val20 = new GUIStyle(val7);
            val20.fontSize = num2;
            val20.padding = new RectOffset(3, 3, 3, 3);
            val20.normal.background = UIUtilities.CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.9f));
            val20.richText = true;
            val20.alignment = (TextAnchor)3;
            val20.wordWrap = true;
            CommandInformationParameterNormalStyle = val20;
            GUIStyle val21 = new GUIStyle(CommandInformationParameterNormalStyle);
            val21.normal.textColor = new Color(0f, 1f, 0f, 1f);
            val21.normal.background = UIUtilities.CreateTexture(new Color(0f, 0.3f, 0f, 0.8f));
            val21.padding = new RectOffset(3, 3, 3, 3);
            val21.fontSize = num2;
            val21.wordWrap = false;
            val21.richText = true;
            CommandInformationParameterHighlightedStyle = val21;
            GUIStyle val22 = new GUIStyle(GUI.skin.label);
            val22.padding = new RectOffset(4, 4, 4, 4);
            val22.normal.background = UIUtilities.CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.9f));
            val22.fontSize = num2;
            val22.alignment = (TextAnchor)3;
            val22.wordWrap = true;
            CommandDescriptionBackgroundStyle = val22;
            GUIStyle val23 = new GUIStyle(GUI.skin.label);
            val23.padding = new RectOffset(8, 8, 4, 4);
            val23.margin = new RectOffset(10, 10, 10, 10);
            val23.normal.textColor = new Color(1f, 1f, 1f, 1f);
            val23.normal.background = imageFiles._SuggestionButtonBackground;
            val23.alignment = (TextAnchor)3;
            val23.border = new RectOffset(7, 7, 7, 7);
            val23.fontSize = num2;
            val23.wordWrap = false;
            SuggestionButtonBackgroundStyle = val23;
            GUIStyle val24 = new GUIStyle(GUI.skin.label);
            val24.padding = new RectOffset(8, 8, 8, 8);
            val24.normal.textColor = new Color(1f, 1f, 1f, 1f);
            val24.normal.background = UIUtilities.CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.4f));
            val24.alignment = (TextAnchor)3;
            val24.fontSize = num2;
            SuggestionButtonMoreBackgroundStyle = val24;
            GUIStyle val25 = new GUIStyle("box");
            val25.margin = new RectOffset(0, 0, 0, 0);
            val25.padding = new RectOffset((int)safeArea.xMin, (int)((float)Screen.width - safeArea.xMax), (int)((float)Screen.height - safeArea.yMax), 0);
            val25.normal.background = UIUtilities.CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.9f));
            val25.fontSize = num2;
            ConsoleWindowBackgroundStyle = val25;
            GUIStyle val26 = new GUIStyle("box");
            val26.margin = new RectOffset(0, 0, 0, 0);
            val26.padding = new RectOffset(0, 0, 0, 0);
            val26.normal.background = UIUtilities.CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.9f));
            val26.fontSize = num2;
            ConsoleWindowStyle = val26;
            GUIStyle val27 = new GUIStyle(ConsoleWindowBackgroundStyle);
            val27.padding = new RectOffset((int)(ScaleFactor * 50f), (int)(ScaleFactor * 50f), 0, 0);
            val27.normal.background = UIUtilities.CreateTexture(new Color(0.278f, 0.278f, 0.278f, 1f));
            HelpWindowBackgroundStyle = val27;
            GUIStyle val28 = new GUIStyle();
            val28.normal.background = UIUtilities.CreateTexture(new Color(0f, 0f, 0f, 0.1f));
            ItemAlternateBackgroundStyle = val28;
            GUIStyle val29 = new GUIStyle();
            val29.normal.background = UIUtilities.CreateTexture(new Color(1f, 1f, 1f, 0.1f));
            HighlightedItemBackgroundStyle = val29;
            GUIStyle val30 = new GUIStyle();
            val30.normal.background = UIUtilities.CreateTexture(new Color(1f, 1f, 0f, 0.05f));
            SearchResultItemBackgroundStyle = val30;
            GUIStyle val31 = new GUIStyle();
            val31.normal.background = UIUtilities.CreateTexture(new Color(1f, 1f, 0f, 0.2f));
            SelectedSearchItemBackgroundStyle = val31;
            GUIStyle val32 = new GUIStyle();
            val32.normal.background = imageFiles._BackgroundGradient;
            HeaderStyle = val32;
            GUIStyle val33 = new GUIStyle(GUI.skin.window);
            val33.border = new RectOffset(8, 8, 2, 8);
            val33.padding = new RectOffset(9, 9, 1, 9);
            OverlayWindowStyle = val33;
            SetBackgroundForAllStyleStates(OverlayWindowStyle, imageFiles._SettingsPopupBackground);
            GUIStyle val34 = new GUIStyle(OverlayWindowStyle);
            val34.border = new RectOffset(8, 8, 2, 8);
            val34.padding = new RectOffset(6, 6, 0, 6);
            PopupMenuWindowStyle = val34;
            GUIStyle val35 = new GUIStyle(GUI.skin.textField);
            val35.alignment = (TextAnchor)3;
            val35.padding = new RectOffset((int)(10f * ScaleFactor), (int)(10f * ScaleFactor), 2, 2);
            val35.fontSize = (int)(HeaderHeight * 0.5f);
            FilterTextFieldStyle = val35;
            GUIStyle val36 = new GUIStyle(GUI.skin.label);
            val36.alignment = FilterTextFieldStyle.alignment;
            val36.padding = FilterTextFieldStyle.padding;
            val36.fontSize = FilterTextFieldStyle.fontSize;
            val36.fontStyle = (FontStyle)2;
            val36.normal.textColor = Color.gray;
            FilterTextFieldHelpOverlayStyle = val36;
            GUIStyle val37 = new GUIStyle(GUI.skin.textField);
            val37.alignment = (TextAnchor)3;
            val37.fontSize = (int)(HeaderHeight * 0.5f);
            SearchTextFieldStyle = val37;
            GUIStyle val38 = new GUIStyle(GUI.skin.label);
            val38.alignment = (TextAnchor)5;
            val38.fontSize = SearchTextFieldStyle.fontSize;
            val38.normal.textColor = Color.gray;
            SearchTextFieldCountStyle = val38;
            GUIStyle val39 = new GUIStyle(GUI.skin.textField);
            val39.alignment = (TextAnchor)3;
            val39.padding = new RectOffset((int)(10f * ScaleFactor), (int)(10f * ScaleFactor), 2, 2);
            val39.fontSize = (int)(HeaderHeight * 0.5f);
            val39.margin = new RectOffset(5, 3, 3, 4);
            InputTextFieldStyle = val39;
            GUIStyle val40 = new GUIStyle(GUI.skin.button);
            val40.fontSize = HeaderButtonLabelStyle.fontSize;
            val40.padding = new RectOffset(5, 5, 5, 5);
            SettingsPanelButtonStyle = val40;
            GUIStyle val41 = new GUIStyle(GUI.skin.textArea);
            val41.fontSize = num2;
            val41.padding = new RectOffset();
            val41.margin = new RectOffset();
            val41.richText = true;
            LogHistoryItemTextAreaStyle = val41;
            GUIStyle val42 = new GUIStyle();
            val42.padding = new RectOffset((int)(10f * ScaleFactor), (int)(10f * ScaleFactor), (int)(10f * ScaleFactor), (int)(10f * ScaleFactor));
            SearchFilterToolBarStyle = val42;
            SetBackgroundForAllStyleStates(LogHistoryItemTextAreaStyle, UIUtilities.CreateTexture(Color.clear));
            BlackTexture = UIUtilities.CreateTexture(Color.black);
        }

        private static void SetBackgroundForAllStyleStates(GUIStyle style, Texture2D texture)
        {
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onActive.background = texture;
            style.focused.background = texture;
            style.onFocused.background = texture;
        }

        public static void BeginCustomSkin(ImageFilesContainer imageFiles, Settings settings, Rect safeArea)
        {
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            SetUpStyles(imageFiles, settings, safeArea);
            _defaultGUISkin = GUI.skin;
            GUI.skin = _customGUISkin;
        }

        public static void EndCustomSkin()
        {
            GUI.skin = _defaultGUISkin;
        }

        public static void InvalidateSkin()
        {
            _customGUISkin = null;
        }
    }
}

