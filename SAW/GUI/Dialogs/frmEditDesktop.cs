using System;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Edits the list of applications in a Desktop, allowing the user to remove unwanted ones </summary>
	internal partial class frmEditDesktop : Form
	{
		private Desktop m_Desktop;

		private frmEditDesktop(Desktop desktopToEdit)
		{
			InitializeComponent();
			Strings.Translate(this);

			m_Desktop = desktopToEdit;
			foreach (Desktop.Program p in m_Desktop.Programs)
			{
				var create = lstPrograms.Items.Add(p.Name);
				create.SubItems.Add(p.Path);
				create.Tag = p;
			}
		}

		public static bool Display(Desktop desktopToEdit)
		{
			using (var frm = new frmEditDesktop(desktopToEdit))
			{
				return frm.ShowDialog() == DialogResult.OK;
			}
		}

		private void lstPrograms_Resize(object sender, EventArgs e)
		{
			colName.Width = (lstPrograms.Width * 3) / 10 - 16;
			colPath.Width = (lstPrograms.Width * 7) / 10 - 16;
		}

		private void lstPrograms_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnRemove.Enabled = lstPrograms.SelectedIndices.Count > 0;
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			var list = lstPrograms.SelectedItems.OfType<ListViewItem>().ToArray(); // ensures we have a separate list, in case iterating the old one blows up when changing it
			foreach (var item in list)
			{
				m_Desktop.Programs.Remove((Desktop.Program) item.Tag);
				lstPrograms.Items.Remove(item);
			}
		}
	}
}
