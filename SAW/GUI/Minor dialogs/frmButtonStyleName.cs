using System;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class frmButtonStyleName
	{
		public frmButtonStyleName()
		{
			InitializeComponent();
		}

		private ButtonStyle m_Style;

		public void frmButtonStyleName_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public void txtName_TextChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = !string.IsNullOrEmpty(txtName.Text);
		}

		public static string Display(ButtonStyle objStyle)
		{
			using (frmButtonStyleName frm = new frmButtonStyleName())
			{
				frm.txtName.Text = objStyle.Name;
				frm.m_Style = objStyle;
				if (frm.ShowDialog() != DialogResult.OK)
					return "";
				return frm.txtName.Text;
			}

		}

		public void btnOK_Click(object sender, EventArgs e)
		{
			ButtonStyle existing = Globals.Root.CurrentDocument.FindSharedButtonStyleByName(txtName.Text);
			if (existing != null && existing != m_Style)
			{
				MessageBox.Show(Strings.Item("[Button_StyleNameUsed]"));
				return;
			}
			DialogResult = DialogResult.OK;
		}

	}
}
