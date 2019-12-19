using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmShowErrorReport
	{
		public frmShowErrorReport()
		{
			InitializeComponent();
		}

		private Config m_UserConfig;
		private string m_SubErrors = "";
		private string m_Log = "Not included";
		private string m_LogPermanent = "Not included";
		private string m_Events = "Not included";

		public static Document Display(string file)
		{
			frmShowErrorReport frmNew = new frmShowErrorReport();
			frmNew.Text += " " + System.IO.Path.GetFileName(file);
			DataReader reader = new DataReader(file, FileMarkers.ErrorReport);
			//reader.ReadByte() ' Shouldn't be needed.  In versions 2.03.n if it failed to save the document it added an extra byte here (followed by a blank document)
			Document document = (Document)reader.ReadData(FileMarkers.Document);
			frmNew.txtError.Text = reader.ReadString();
			frmNew.txtMessage.Text = reader.ReadString();
			List<string> colSubErrors = reader.ReadListString();
			System.Text.StringBuilder sub = new System.Text.StringBuilder();
			for (int index = 0; index <= colSubErrors.Count - 1; index++)
			{
				sub.Append("(").Append(index.ToString()).Append(") ").AppendLine(colSubErrors[index]);
			}
			frmNew.m_SubErrors = sub.ToString();
			frmNew.txtErrorLog.Text = frmNew.m_SubErrors;
			// some of this extra information is not displayed yet, but is included in later reports
			reader.ReadByte(); //Dim eUser As Users =
			Guid idActivity = reader.ReadGuid();
			frmNew.m_UserConfig = (Config)reader.ReadData();
			reader.ReadData(); // will be null - UserEditor in Splash
	reader.ReadString();
			frmNew.m_Log = reader.ReadString();
			frmNew.m_LogPermanent = reader.ReadString();
			if (reader.Version >= 108)
				frmNew.m_Events = Globals.ReadDiagnosticEvents(reader);
			reader.ReadOptionalPNG(); // never used
			reader.Close();
			document.UpdateReferencesObjectsCreated(document, reader);

			frmNew.lblVersion.Text = "Binary data version: " + reader.Version + "; Software version: " + SoftwareVersion.VersionStringFromNumber(reader.SoftwareVersion);
			if (document.Count == 0)
			{
				frmNew.lblWarning.Text = "SAW6Doc contained no pages - blank page created";
				document.AddPage(new Page(), new Transaction(), false);
			}
			frmNew.lblActivity.Text = "Activity: " + idActivity;
			try // Error handler in case the activity is not present on this machine
			{
				frmNew.lblActivity.Text += " (" + Activities.GetActivityText(idActivity, false, false) + ")";
			}
			catch
			{
			}
			frmNew.TopMost = true;
			frmNew.Show();
			return document;
		}

		public void btnClose_Click(object sender, EventArgs e)
		{
			this.Dispose();
		}

		public void btnUserConfigs_Click(object sender, EventArgs e)
		{
			Config.UserUser = m_UserConfig;
			m_UserConfig.AutoCreated = true;
			// The document is not saved with the actual settings objects
			Document newDocument = new Document(true) {UserSettings = m_UserConfig};
			m_UserConfig.Document = newDocument;
			btnUserConfigs.Enabled = false;
			MessageBox.Show("Settings from file loaded as current. Change between user and teacher to force screen to reflect these settings");
		}

		public void rdoSubErrors_CheckedChanged(object sender, EventArgs e)
		{
			if (this.Visible)
			{
				if (rdoLog.Checked)
					txtErrorLog.Text = m_Log;
				else if (rdoPermanentLog.Checked)
					txtErrorLog.Text = m_LogPermanent;
				else if (rdoEvents.Checked)
					txtErrorLog.Text = m_Events;
				else
					txtErrorLog.Text = m_SubErrors;
			}
		}


	}
}
