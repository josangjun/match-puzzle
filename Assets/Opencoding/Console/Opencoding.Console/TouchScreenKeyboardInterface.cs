using UnityEngine;

namespace Opencoding.Console
{
	public abstract class TouchScreenKeyboardInterface
	{
		public enum TouchScreenKeyboardTypeProxy
		{
			Default,
			ASCIICapable,
			NumbersAndPunctuation,
			URL,
			NumberPad,
			PhonePad,
			NamePhonePad,
			EmailAddress
		}

		public abstract string text { get; set; }

		public abstract bool hideInput { get; set; }

		public abstract bool active { get; set; }

		public abstract bool done { get; }

		public abstract bool wasCanceled { get; }

		public abstract Rect area { get; }

		public abstract bool visible { get; }

		public abstract TouchScreenKeyboardProxy Open(string text, TouchScreenKeyboardTypeProxy keyboardType, bool autocorrection, bool multiline, bool secure, bool alert);
	}
}
