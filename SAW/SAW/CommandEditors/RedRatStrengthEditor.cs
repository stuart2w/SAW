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
using SAW;

namespace SAW.CommandEditors
{
	public partial class RedRatStrengthEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public RedRatStrengthEditor()
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
				nud1.Value = m_Command.GetParamAsInt(0).LimitTo(0,100);
				nud2.Value = m_Command.GetParamAsInt(1).LimitTo(0,100);
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor

		private CmdRedRatStrength m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (CmdRedRatStrength)value;
				FillUI();
			}
		}

		public string GetValidationError() => null;

		#endregion

		private void nud_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList.Clear();
			m_Command.ParamList.Add(new IntegerParam((short)nud1.Value));
			m_Command.ParamList.Add(new IntegerParam((short)nud2.Value));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			// none of the Redrat commands actually use the context, so it's OK to create a dummy one
			m_Command.Execute(new Command.ExecutionContext(null, null, null, null, null, Shapes.Scriptable.ScriptTypes.Visit));
		}
	}
}
