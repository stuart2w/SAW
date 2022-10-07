using System;
using SAW.Shapes;

namespace SAW
{
	internal partial class frmUserButtonStyle
	{
		public frmUserButtonStyle()
		{
			InitializeComponent();
		}

		// This always uses a separate transaction from the outer screen

		public void frmUserButtonStyle_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
			Strings.Translate(this);
		}

		public static void Display(Config config, int index)
		{
			using (frmUserButtonStyle frm = new frmUserButtonStyle())
			{
				ButtonStyle.EnsureConfigUserDefaultsSet(config); // (theoretically reachable without having created the default if they started in teacher mode?)
				frm.ctrStyle.DisplayStyle(config.ButtonStyle[index]);
				Transaction objTransaction = new Transaction();
				objTransaction.Edit(config.ButtonStyle[index]);

				string typeName;
				switch (index)
				{
					case 0:
						typeName = Strings.Item("Config_ButtonStyle_Action");
						break;
					case 1:
						typeName = Strings.Item("Config_ButtonStyle_Selection");
						break;
					default:
						Item.ItemDisplayTypes displayType = (Item.ItemDisplayTypes)(index - 2);
						typeName = Strings.Item("SAW_Style_" + displayType.ToString().Replace("IDT_", ""));
						break;
				}
				frm.Text = Strings.Item("Button_UserStyle_Title") + ": " + typeName;

				frm.lblHeader.Text = Strings.Item("Button_UserStyle_Header").Replace("%0", typeName);

				if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					ButtonStyle.SetDefaultOnUserChange(); // This is often redundant as we are in teacher mode and editing the user mode, but it is harmless and IS occasionally needed
					Globals.Root.StoreNewTransaction(objTransaction);
				}
				else
					objTransaction.Cancel();
			}
			//frm.Dispose(); // Must be disposed immediately because the style panel registers itself with Engine
		}

		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					components?.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
