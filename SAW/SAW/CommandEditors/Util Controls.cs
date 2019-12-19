using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

// some controls used in the command editors for convenience.  All simple overrides of windows ones
namespace SAW.CommandEditors
{

	#region Capturing textbox class
	/// <summary>Override of Text which supports capturing every keypress</summary>
	[ToolboxItem(false)]
	public class CapturingTextBox : TextBox
	{
		private bool m_Capture;

		[DefaultValue(false)]
		public bool CaptureAllKeys
		{
			get { return m_Capture; }
			set { m_Capture = value; }
		}

		protected override bool IsInputChar(char charCode)
		{
			if (m_Capture)
				return true;
			return base.IsInputChar(charCode);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (m_Capture)
				return true;
			return base.IsInputKey(keyData);
		}
	}
	#endregion

	#region IntegerTextBox

	/// <summary>Version of textbox that supports reading the contained value safely as an integer.  Empty text is considered as a valid number 0</summary>
	public class IntegerTextBox : TextBox
	{
		public int Value
		{
			get
			{
				if (Text == "")
					return 0;
				int value;
				if (int.TryParse(Text, out value))
					return value;
				Debug.Fail("Reading invalid value from IntegerTextBox - Check IsValid first");
				return 0;
			}
			set { Text = value.ToString(); }
		}

		public bool IsValid
		{
			get
			{
				if (Text == "")
					return true;
				return int.TryParse(Text, out _);
			}
		}
	}

	#endregion

	#region ItemIDTextBox

	/// <summary>Text box with a numeric ID, suitable for selecting an item ID.  Use the Value property and ValueChanged events to maintain compatibility if this is eventually re-implemented </summary>
	[DefaultEvent("ValueChanged")]
	public class ItemIDTextBox : TextBox
	{
		private bool m_Filling;

		/// <summary>Only raised if the user changes it</summary>
		public event EventHandler ValueChanged;

		public ItemIDTextBox()
		{
			if (DesignMode)
				return;
			this.TextChanged += ItemIDTextBox_TextChanged;
		}

		/// <summary>The (translateable) message to return if the text is not valid.  Leaving as empty string uses default [SAW_CommandEdit_InvalidID]</summary>
		[DefaultValue("")]
		public string InvalidMessage { get; set; }

		/// <summary>If true, then an empty text is treated as value zero, without error</summary>
		[DefaultValue(false)]
		public bool AllowEmpty { get; set; }

		public string GetValidationError()
		{
			if (AllowEmpty && string.IsNullOrEmpty(Text))
				return null;
			if (int.TryParse(Text, out _))
				return null;
			return Strings.Translate(string.IsNullOrEmpty(InvalidMessage) ? "[SAW_CommandEdit_InvalidID]" : InvalidMessage);
		}

		/// <summary>Always returns 0 safely if not valid</summary>
		[Browsable(false)]
		public int Value
		{
			get
			{
				if (AllowEmpty && string.IsNullOrEmpty(Text))
					return 0;
				int value;
				return int.TryParse(Text, out value) ? value : 0;
			}
			set
			{
				m_Filling = true;
				Text = value.ToString();
				BackColor = System.Drawing.Color.White;
				m_Filling = false;
			}
		}

		private void ItemIDTextBox_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!string.IsNullOrEmpty(Text) && !int.TryParse(Text, out _))
				BackColor = System.Drawing.Color.Tomato;
			else
				BackColor = System.Drawing.Color.White;
			ValueChanged?.Invoke(this, EventArgs.Empty);

			// following not helpful?  It makes it hard to add command if the user can't remember a valid ID
			//if (m_IDValid)
			//{
			//	var page = (Parent as CommandEditor)?.ContainerScriptable?.FindPage();
			//	if (page != null && page.FindScriptableByID(value) == null)
			//		m_IDValid = false;
			//}

		}


		#endregion

	}
}
