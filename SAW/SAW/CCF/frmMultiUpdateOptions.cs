using System;
using System.Windows.Forms;

namespace SAW.CCF
{
	public partial class frmMultiUpdateOptions : Form
	{
		public frmMultiUpdateOptions()
		{
			InitializeComponent();
			Strings.Translate(this);
			cmbPreferredSymbols.SelectedIndex = 0;
			cmbSecondarySymbols.SelectedIndex = 1;
			InitialiseLanguage();
			cmbTextLanguage_SelectedIndexChanged(this, EventArgs.Empty);
		}

		public static bool Display(bool hasSelection, MultiUpdate update)
		{
			frmMultiUpdateOptions frmNew = new frmMultiUpdateOptions();
			frmNew.chkUpdateSelected.Enabled = hasSelection;
			frmNew.chkUpdateSelected.Checked = hasSelection; // if the user has selected multiple items, default to updating only them - this is probably what the user would expect
			if (frmNew.ShowDialog() != DialogResult.OK)
				return false;
			update.OnlyCoded = frmNew.chkOnlyCoded.Checked;
			update.OnlySelected = frmNew.chkUpdateSelected.Checked;
			if (frmNew.rdoRemoveCCF.Checked)
			{
				update.PerformFunction = MultiUpdate.Functions.RemoveInfo;
				update.OnlyCoded = true; // can't remove any CCF information from items without it!
				update.PreferredOutput = "";
			}
			else if (frmNew.rdoText.Checked)
			{
				update.PerformFunction = MultiUpdate.Functions.ChangeText;
				update.PreferredOutput = frmNew.SelectedOutLanguage;
			}
			else
			{
				update.PerformFunction = MultiUpdate.Functions.ChangeSymbols;
				update.PreferredOutput = frmNew.cmbPreferredSymbols.Text;
			}
			update.SourceLanguage = frmNew.SelectedInLanguage;
			update.SecondarySymbols = frmNew.cmbSecondarySymbols.Text;
			if (update.SecondarySymbols.StartsWith("(")) // is the (None) option
				update.SecondarySymbols = "";

			return true;
		}

		#region Language - copied and modified from frmCCFLookup
		private void InitialiseLanguage()
		{
			SelectedInLanguage = Server.Language2;
			SelectedOutLanguage = SelectedInLanguage;
		}

		private string SelectedInLanguage
		{  // first 2 characters are language code.  List prefilled with the ones used in CCF, in form "code - name".  Only first 2 chars used
			get
			{
				if (cmbTextLanguage.Text.Length < 2) return "en";
				return cmbTextLanguage.Text.Substring(0, 2);
			}
			set
			{
				if (value.Length < 2) return;
				for (int i = 0; i < cmbTextLanguage.Items.Count - 1; i++)
					if (cmbTextLanguage.Items[i].ToString().Substring(0, 2) == value)
					{
						cmbTextLanguage.SelectedIndex = i;
						return;
					}
				cmbTextLanguage.Text = value;
			}
		}

		private string SelectedOutLanguage
		{  // first 2 characters are language code.  List prefilled with the ones used in CCF, in form "code - name".  Only first 2 chars used
			get
			{
				if (cmbLanguageOut.Text.Length < 2) return "en";
				return cmbLanguageOut.Text.Substring(0, 2);
			}
			set
			{
				if (value.Length < 2) return;
				for (int i = 0; i < cmbLanguageOut.Items.Count - 1; i++)
					if (cmbLanguageOut.Items[i].ToString().Substring(0, 2) == value)
					{
						cmbLanguageOut.SelectedIndex = i;
						return;
					}
				cmbLanguageOut.Text = value;
			}
		}
		#endregion

		private void rdoFunction_CheckedChanged(object sender, EventArgs e)
		{ // Handles the checked changed for all three radio buttons
			chkOnlyCoded.Enabled = !rdoRemoveCCF.Checked;
			pnlSymbolOut.Visible = rdoSymbols.Checked;
			pnlLanguageOut.Visible = rdoText.Checked;
			cmbTextLanguage.Enabled = !rdoRemoveCCF.Checked;
		}

		private void cmbTextLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = cmbTextLanguage.Enabled == false || cmbTextLanguage.Text != "";
		}

		private void chkOnlyCoded_CheckedChanged(object sender, EventArgs e)
		{ // if only using concept coding is the text on the buttons at the moment doesn't matter
			cmbTextLanguage.Enabled = !chkOnlyCoded.Checked;
		}

	}
}