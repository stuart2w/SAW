using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedRat.AvDeviceDb;
using SAW.Commands;

namespace SAW.CommandEditors
{
	public partial class RedRatColourEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public RedRatColourEditor()
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
				pnlColour.BackColor = m_Command.Colour;
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor

		private CmdRedRatColour m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (CmdRedRatColour)value;
				FillUI();
			}
		}

		public string GetValidationError() => null;

		#endregion

		private void pnlColour_Click(object sender, EventArgs e)
		{
			dlgColour.Color = pnlColour.BackColor;
			if (dlgColour.ShowDialog() == DialogResult.OK)
			{
				pnlColour.BackColor = m_Command.Colour = dlgColour.Color;
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			// none of the Redrat commands actually use the context, so it's OK to create a dummy one
			m_Command.Execute(new Command.ExecutionContext(null, null, null, null, null, Shapes.Scriptable.ScriptTypes.Visit));
		}
	}
}
