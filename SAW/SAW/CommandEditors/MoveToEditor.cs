using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class MoveToEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;
		/// <summary>Whether trying to capture mouse location.  Can't rely on Control.Capture as that seems to become true automatically in some places
		/// 
		/// </summary>
		private bool m_Capture;

		public MoveToEditor()
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
				// by using create=true param here we can just assume everywhere else that the params exist
				txtX.Value = m_Command.GetParamAsInt(0, true);
				txtY.Value = m_Command.GetParamAsInt(1, true);
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
			if (!txtX.IsValid || !txtY.IsValid)
				return Strings.Item("SAW_CommandEdit_NotNumber");
			return null;
		}

		#endregion

		#region Events

		private void btnTest_Click(object sender, EventArgs e)
		{
			if (GetValidationError() != null)
				return;
			Cursor.Position = new Point(m_Command.GetParamAsInt(0), m_Command.GetParamAsInt(1));
		}

		private void btnSelect_Click(object sender, EventArgs e)
		{
			Capture = true;
			lblMessage.Visible = true;
			m_Capture = true;
		}

		private void txtX_TextChanged(object sender, EventArgs e)
		{
			btnTest.Enabled = GetValidationError() == null;
			if (m_Filling)
				return;
			if (txtX.IsValid)
			{
				m_Command.ParamList[0] = new IntegerParam(txtX.Value);
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void txtY_TextChanged(object sender, EventArgs e)
		{
			btnTest.Enabled = GetValidationError() == null;
			if (m_Filling)
				return;
			if (txtY.IsValid)
			{
				m_Command.ParamList[1] = new IntegerParam(txtY.Value);
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void MoveToEditor_MouseDown(object sender, MouseEventArgs e)
		{
			if (!m_Capture)
				return;
			Capture = false;
			m_Capture = false;
			lblMessage.Visible = false;
			if (e.Button == MouseButtons.Left) // prompt says right to cancel, but anything except left will do so
			{
				m_Filling = true;
				Point pt = this.PointToScreen(e.Location);
				m_Command.ParamList[0] = new IntegerParam(pt.X);
				m_Command.ParamList[1] = new IntegerParam(pt.Y);
				m_Filling = false;
				FillUI();
				UserChanged?.Invoke(this, EventArgs.Empty);
			}

		}

		#endregion

	}
}
