using UnityEngine;

namespace Opencoding.Shared.Utils
{
	public static class UIUtilities
	{
		private static GUIContent _guiContent = new GUIContent();

		public static Texture2D CreateTexture(int width, int height, Color col)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			Color[] array = (Color[])(object)new Color[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = col;
			}
			Texture2D val = new Texture2D(width, height);
			((Texture)val).wrapMode=((TextureWrapMode)0);
			val.SetPixels(array);
			val.Apply();
			((Object)val).hideFlags=((HideFlags)61);
			return val;
		}

		public static Texture2D CreateTexture(Color col)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return CreateTexture(1, 1, col);
		}

		public static GUIContent TempGUIContent(string text)
		{
			_guiContent.text=(text);
			_guiContent.image=((Texture)null);
			return _guiContent;
		}

		public static GUIContent TempGUIContent(string text, Texture2D image)
		{
			_guiContent.text=(text);
			_guiContent.image=((Texture)(object)image);
			return _guiContent;
		}

		public static GUIContent TempGUIContent(Texture2D image)
		{
			_guiContent.text=("");
			_guiContent.image=((Texture)(object)image);
			return _guiContent;
		}
	}
}
