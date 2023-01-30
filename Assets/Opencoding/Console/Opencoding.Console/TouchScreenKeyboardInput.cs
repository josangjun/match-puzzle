namespace Opencoding.Console
{
	internal class TouchScreenKeyboardInput
	{
		private string _text = "";

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				TouchScreenKeyboardManager.InputChanged(this);
			}
		}

		public bool IsDone { get; set; }

		public bool WasCancelled { get; set; }

		public void SetTextFromKeyboard(string text)
		{
			_text = text ?? "";
		}
	}
}
