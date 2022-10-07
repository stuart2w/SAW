using System;
using System.Windows.Forms;
using SAW.Functions;
using SAW.Shapes;

namespace SAW
{
	internal partial class frmEditDefaultScripts : Form
	{
		public frmEditDefaultScripts()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public static void Display(Codes verb, Form owner)
		{
			Scriptable edit;
			Document document = Globals.Root.CurrentDocument;
			switch (verb)
			{
				case Codes.DefaultEscapeScript:
					edit = document.DefaultEscapeScripts; break;
				case Codes.DefaultGroupScript:
					edit = document.DefaultGroupScripts; break;
				case Codes.DefaultItemScript:
					edit = document.DefaultItemScripts; break;
				default: throw new ArgumentException("verb");
			}
			using (var frm = new frmEditDefaultScripts())
			{
				frm.Owner = owner;
				frm.Text = Strings.Item("Verb_" + verb);
				Transaction transaction = new Transaction();
				frm.ctrEditor.Edit(edit, true);
				if (frm.ShowDialog() != DialogResult.OK)
					transaction.Cancel();
				else
					Globals.Root.StoreNewTransaction(transaction);
			}
		}
	}
}
