using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class GridScanEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public GridScanEditor()
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
				txtColumns.Text = m_Command.GetParamAsInt(0, true).ToString();
				txtRows.Text = m_Command.GetParamAsInt(1, true).ToString();
				lblNeedAutoRepeat.Visible = !(Parent as CommandEditor)?.ContainerScriptable?.AutoRepeat ?? false;
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

		public string GetValidationError()
		{
			if (!txtColumns.IsValid || !txtRows.IsValid)
				return Strings.Item("SAW_CommandEdit_NotNumber");
			if (txtColumns.Value <= 0 || txtRows.Value <= 0)
				return Strings.Item("SAW_CommandEdit_BadGrid");
			return null;
		}

		#endregion

		#region Events

		private void txtRows_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!txtRows.IsValid || txtRows.Value <= 0)
				return;
			m_Command.m_ParamList[1] = new IntegerParam(txtRows.Value);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtColumns_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!txtColumns.IsValid || txtColumns.Value <= 0)
				return;
			m_Command.m_ParamList[0] = new IntegerParam(txtColumns.Value);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion


	}
}
