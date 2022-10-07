using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	/// <summary>Editor for any command which has 1 param which is a time in decimal seconds </summary>
	[ToolboxItem("false")]
	public partial class FloatingTimeEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public FloatingTimeEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			m_Filling = false;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				nudTime.Value = (decimal) m_Command.GetParamAsFloat(0, true);
			}
			catch // ignore errors (would be out of bounds for min/max most likely)
			{ }
			finally
			{m_Filling = false;}
		}

		#region ICommandEditor
		private Command m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			return null;
		}

		#endregion

		#region Events

		#endregion

		private void nudTime_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList.Clear();
			m_Command.ParamList.Add(new FloatParam((float) nudTime.Value));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
