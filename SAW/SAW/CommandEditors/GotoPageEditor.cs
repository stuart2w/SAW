using System;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	public partial class GotoPageEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public GotoPageEditor()
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
				nudPage.Maximum = Globals.Root.CurrentDocument.Pages.Count();
				nudPage.Value = m_Command.GetParamAsInt(0, true);
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex); // ignored since it is likely to be an out-of-range issue
			}
			finally { m_Filling = false; }
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

		public string GetValidationError() => null;

		#endregion

		#region Events

		private void nudPage_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList[0] = new IntegerParam((int)(nudPage.Value));
			UserChanged?.Invoke(sender, e);
		}

		#endregion

	}
}
