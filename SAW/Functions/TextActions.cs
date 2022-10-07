using System.Drawing;
using System.Windows.Forms;

namespace SAW.Functions
{
	abstract class TextActionBase : Action
	{
		public readonly string Text;
		protected TextActionBase(Parameters param, string text) : base(param)
		{
			Text = text;
		}

		public override string PersistValueText => Text;
	}

	class TextAction : TextActionBase
	{
		public TextAction(string text) : base(Parameters.Action_Text, text)
		{
		}

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			// sends them as separate chars - this allows equation editor to do funky stuff like on "("
			foreach (var c in Strings.Translate(Text))
			{
				Editor.SimulateKey(c);
			}
		}

		#region Info
		public override bool IsApplicable(EditableView pnlView) => true;

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			if (string.IsNullOrEmpty(Text))
				return Strings.Item("Action_TypeText");
			if (forSpeech)
				return Text;
			return Strings.Item("Action_TypeText") + " (" + Text + ")";

		}
		#endregion

	}

	class KeyAction : TextActionBase
	{
		public readonly Keys Key;
		public KeyAction(Keys key) : base(Parameters.Action_Key, ((int)key).ToString())
		{
			Key = key;
		}

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.SimulateKey(Key);
		}

		#region Info
		public override bool IsApplicable(EditableView pnlView) => true;

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			// If an actual key is defined then we show the key, otherwise it just uses the default text
			if (Key == Keys.None)
				return Strings.Item("Action_SimulateKey");
			if (forSpeech)
				return GUIUtilities.KeyDescription(Key); // When this is used for speech description, omit the "Press key combination", and just name the key
			return Strings.Item("Action_SimulateKey") + " (" + GUIUtilities.KeyDescription(Key) + ")";
		}
		#endregion

	}

	/// <summary>Value of this can be a char, or a translation code (if length >1) </summary>
	class CharAction : TextActionBase
	{

		#region Values construct and persist
		public CharAction(char text) : base(Parameters.Action_Character, text.ToString())
		{
		}

		/// <summary>Constructor to use where it is translateable </summary>
		/// <param name="text"></param>
		public CharAction(string text) : base(Parameters.Action_Character, text)
		{
		}

		public char Character
		{
			get
			{
				if (Text.Length == 1)
					return Text[0];
				return Strings.Translate(Text)[0];
			}
		}
		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.SimulateKey(Character);
		}

		#region Info
		public override bool IsApplicable(EditableView pnlView) => true;

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			// If an actual key is defined then we show the key, otherwise it just uses the default text
			if (Character == 0)
				return Strings.Item("Action_TypeCharacter");
			if (forSpeech)
				return Character.ToString();
			return Strings.Item("Action_TypeCharacter") + " (" + Character + ")";
		}

		protected override Bitmap CreateSampleImage2(int size)
		{
			//return ParameterSupport.CreateParameterSampleImage(Change,Character);
			Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics gr = Graphics.FromImage(bitmap))
			{
				gr.Clear(Color.Transparent);
				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				using (Font font = new Font(FontFamily.GenericSansSerif, 14))
				{
					gr.DrawString(Character.ToString(), font, Brushes.Black, 2, 10);
				}
				return bitmap;
			}
		}
		#endregion
	}
}
