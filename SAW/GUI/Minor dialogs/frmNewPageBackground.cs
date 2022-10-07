using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class frmNewPageBackground
	{
		/// <summary>this is used when clicking a new page where the original has a large background image.  The user can choose whether to use the same image or not </summary>
		public frmNewPageBackground()
		{
			InitializeComponent();
		}

		private MemoryImage m_OriginalImage;
		/// <summary>set by the form if the user selects an alternate image</summary>
		private string m_OtherFile;

		public void frmNewPageBackground_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		/// <summary>Returns false if cancelled.  Updates the background image in newPage</summary>
		public static bool Ask(Page originalPage, Page newPage, Transaction transaction)
		{
			Debug.Assert(originalPage.BackgroundImage != null);
			if (originalPage.BackgroundImageMode == Page.BackgroundImageModes.Tile) // Would imply a small image.  This form is not used and the image should automatically be reused
			{
				newPage.BackgroundImage = originalPage.BackgroundImage;
				return true;
			}
			frmNewPageBackground frm = new frmNewPageBackground();
			frm.m_OriginalImage = originalPage.BackgroundImage.MemoryImage;
			if (frm.ShowDialog() != DialogResult.OK)
				return false;
			if (frm.rdoBlank.Checked)
				newPage.BackgroundImage = null;
			else if (frm.rdoSame.Checked)
				newPage.BackgroundImage = originalPage.BackgroundImage;
			else if (frm.rdoOther.Checked)
				newPage.BackgroundImage = (SharedImage)Globals.Root.CurrentDocument.AddSharedResourceFromFile(frm.m_OtherFile, transaction);
			else
				Debug.Fail("No selection found in frmNewPageBackground");
			return true;
		}

		public void rdoSame_CheckedChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = rdoSame.Checked || rdoBlank.Checked || rdoOther.Checked;
		}

		public void btnOK_Click(object sender, EventArgs e)
		{
			if (rdoOther.Checked)
			{
				string file = FileDialog.ShowOpen(FileDialog.Context.BackgroundDocument);
				if (string.IsNullOrEmpty(file))
					return;
				m_OtherFile = file;
				DialogResult = DialogResult.OK;
			}
			else
				DialogResult = DialogResult.OK;
		}

		public void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			Rectangle rect = new Rectangle(0, 0, pnlPreview.Width, pnlPreview.Height);
			GUIUtilities.CalcDestRect(m_OriginalImage.Size, ref rect);
			m_OriginalImage.Draw(e.Graphics, rect);
		}


	}
}
