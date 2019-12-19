using System;
using System.Drawing;


namespace SAW
{
	public partial class frmCreateActivity
	{

		private MemoryImage m_Image;
		/// <summary>we keep the configurations as they applied to both user and teacher </summary>
		private AppliedConfig m_AppliedUser;
		private AppliedConfig m_AppliedTeacher;
		/// <summary>the transaction used by frmEditConfig </summary>
		private Transaction m_OuterTransaction;

		public frmCreateActivity(Transaction transaction)
		{
			m_OuterTransaction = transaction;
			InitializeComponent();
			pnlPreviewActivityIcon.NoImageString = Strings.Item("Intro_NoImage");
		}

		public void frmCreateActivity_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this); // no need to scale since the entire form is auto size
			m_AppliedUser = Globals.Root.GenerateAppliedConfig(Users.User, Globals.Root.CurrentDocument);
			m_AppliedTeacher = Globals.Root.GenerateAppliedConfig(Users.Editor, Globals.Root.CurrentDocument);
		}

		public void txtName_TextChanged(object sender, EventArgs e)
		{
			EnableOK();
		}

		public void btnChangeActivityIcon_Click(object sender, EventArgs e)
		{
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image);
			if (string.IsNullOrEmpty(filename))
				return;
			m_Image = new MemoryImage(filename);
			m_Image.ChangeToBitmap(new Size(144, 144));
			pnlPreviewActivityIcon.Image = m_Image.GetNetImage();
			EnableOK();
			Globals.OnAvailableActivitiesChanged();
			// We should also raise this if the text changes; but that's more complicated if we don't want to raise numerous events, and I'm not sure it's worth the effort
		}

		private void EnableOK()
		{
			btnOK.Enabled = m_Image != null && !string.IsNullOrEmpty(txtName.Text);
		}

		public void btnOK_Click(object sender, EventArgs e)
		{
			Document document = new Document(true);
			Transaction transaction = new Transaction(); // this is not stored, some of the methods below require it)
			document.GetCreateUserSettings(transaction);
			document.GetCreateBothSettings(transaction);
			Document existing = Activities.GetActivitySettings(); // might be nothing
			if (existing != null)
			{
				MergeSettings(document.UserSettings, existing.UserSettings, m_AppliedUser);
				MergeSettings(document.BothSettings, existing.BothSettings, m_AppliedTeacher);
			}
			MergeSettings(document.UserSettings, Globals.Root.CurrentDocument.UserSettings, m_AppliedUser);
			MergeSettings(document.BothSettings, Globals.Root.CurrentDocument.BothSettings, m_AppliedTeacher);
			document.Name = txtName.Text;
			document.DisplayName = txtName.Text;
			document.ActivityIcon = m_Image;
			document.DisplayName = txtDisplayName.Text;
			Globals.Root.AddNewActivity(document);
			m_OuterTransaction.Edit(Globals.Root.CurrentDocument);
			Globals.Root.CurrentDocument.ActivityID = document.ID;
			Globals.Root.CurrentDocument.Name = document.Name;
			DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void MergeSettings(Config newSettings, Config previous, AppliedConfig applied)
		{
			if (previous == null)
				return;
			if (previous.ReadBooleanEx(Config.Shapes_DefaultOff))
			{
				// this is not supported at the activity level, so we'll need to hide every shape
				foreach (Shape.Shapes shape in Shape.UserShapes)
				{
					newSettings.Write(Config.ShapeEnableKey(shape), false.ToString());
				}
			}
			foreach (string key in previous.Values.Keys)
			{
				newSettings.Write(key, previous.Values[key]);
			}
		}

	}
}
