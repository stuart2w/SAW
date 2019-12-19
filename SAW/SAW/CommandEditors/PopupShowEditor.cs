using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem("false")]
	public partial class PopupShowEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public PopupShowEditor()
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
				if (cmbPopups.Items.Count == 0)
				{ // hasn't yet been populated - fill it
					Document doc = Globals.Root.CurrentDocument;
					List<Scriptable> list = (from page in doc.Pages
											 from shape in page.Shapes.OfType<Scriptable>()
											 where shape.Popup && shape.Element is Item
											 select shape).ToList();
					// only scriptable containing SAW items are allowed as only they have IDs
					List<object> comboEntries = new List<object>();
					comboEntries.Add(new { Text = Strings.Item("SAW_CommandEdit_PopupThis"), ID = -1 });
					foreach (Scriptable s in list)
						comboEntries.Add(new { Text = s.SAWID + ": '" + s.Element.LabelText + "'", ID = s.SAWID });
					cmbPopups.ValueMember = "ID";
					cmbPopups.DisplayMember = "Text";
					cmbPopups.DataSource = comboEntries;
				}
				cmbPopups.SelectedValue = m_Command.m_ParamList.Count == 0 ? -1 : m_Command.GetParamAsInt(0);
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
			return null;
		}
		#endregion

		#region Events

		private void cmbPopups_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList.Clear(); // we need to leave it empty for "this" and fill in the param otherwise
			if ((int) cmbPopups.SelectedValue >= 0)
				m_Command.m_ParamList.Add(new IntegerParam((int) cmbPopups.SelectedValue));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion
	}
}
