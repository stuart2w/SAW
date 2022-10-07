using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;
using SAW.Shapes;

namespace SAW.CommandEditors
{
	/// <summary>Editor for Popup Item commands. </summary>
	[ToolboxItem("false")]
	public partial class PopupItemEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public PopupItemEditor()
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
					foreach (Scriptable s in list)
						comboEntries.Add(new { Text = s.SAWID + ": '" + s.Element.LabelText + "'", ID = s.SAWID });
					cmbPopups.ValueMember = "ID";
					cmbPopups.DisplayMember = "Text";
					cmbPopups.DataSource = comboEntries;
				}
				cmbPopups.SelectedValue = m_Command.GetParamAsInt(0, true);
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
			if (cmbPopups.SelectedIndex < 0)
				return Strings.Item("Script_Error_ItemNotSelected");
			return null;
		}

		#endregion

		#region Events

		private void cmbPopups_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList.Clear();
			m_Command.ParamList.Add(new IntegerParam((int)cmbPopups.SelectedValue));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

	}
}
