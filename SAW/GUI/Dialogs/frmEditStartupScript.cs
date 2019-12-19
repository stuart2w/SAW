using System;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmEditStartupScript : Form
	{
		public frmEditStartupScript(Script script, Page page)
		{
			InitializeComponent();
			Strings.Translate(this);
			ctrEditor.Edit(script, page);
		}

		/// <summary>Displays this to edit the start-up script for the current document</summary>
		public static void Display(Form owner)
		{
			// Cannot use standard transaction logic since the scripts are not Datum
			Document document = Globals.Root.CurrentDocument;
			if (document.StartupScript == null)
				document.StartupScript = new Script(); // this isn't transacted, as there is no need to reverse it
			Script copy = document.StartupScript.Clone();
			using (var frm = new frmEditStartupScript(document.StartupScript, document.Pages.First()))
			{
				frm.Owner = owner;
				if (frm.ShowDialog() != DialogResult.OK)
					document.StartupScript = copy; // reverse the changes
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			string error = ctrEditor.GetValidationError();
			if (error != null)
				MessageBox.Show(error);
			else
				DialogResult = DialogResult.OK;
		}
	}
}
