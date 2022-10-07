using System;


namespace SAW
{
	internal partial class frmActivityRename
	{

		private Document m_Activity;

		public frmActivityRename(Document activity)
		{
			InitializeComponent();
			m_Activity = activity;
			Strings.Translate(this); // must be before the text boxes are assigned
			txtDisplayName.Text = activity.DisplayName;
			txtName.Text = activity.Name;
		}

		public void frmActivityRename_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
		}

		public void btnOK_Click(object sender, EventArgs e)
		{
			m_Activity.DisplayName = txtDisplayName.Text;
			m_Activity.Name = txtName.Text;
		}

		public void txtName_TextChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = !string.IsNullOrEmpty(txtName.Text ) && !string.IsNullOrEmpty(txtDisplayName.Text );
		}
	}
}
