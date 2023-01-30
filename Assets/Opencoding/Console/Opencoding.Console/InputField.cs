using System.Linq;
using Opencoding.Shared.Utils;
using UnityEngine;

namespace Opencoding.Console
{
	internal abstract class InputField
	{
		private string _lastTypedInput = "";

		protected string[] _lastTypedParameters = new string[0];

		public abstract string Input { get; set; }

		public abstract bool HasFocus { get; }

		public abstract Rect TextFieldRect { get; }

		public Rect HelpButtonRect { get; protected set; }

		public string LastTypedInput => _lastTypedInput;

		public string[] LastTypedParameters => _lastTypedParameters;

		public int CurrentParameterIndex
		{
			get
			{
				int num = LastTypedParameters.Length - 2;
				if (LastTypedInput.EndsWith(" "))
				{
					num++;
				}
				return num;
			}
		}

		public abstract void OnGUI(Rect containingRect);

		public abstract void LoseFocus();

		public abstract void Focus();

		public void ConfirmInput()
		{
			_lastTypedParameters = StringUtils.SplitCommandLine(Input).ToArray();
			_lastTypedInput = Input;
		}

		public abstract void ClearInput();
	}
}
