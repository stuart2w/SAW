using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	internal partial class frmChooseImportPage
	{
		public frmChooseImportPage()
		{
			InitializeComponent();
		}

		private int m_Page;
		private List<string> m_Images = new List<string>();
		private Image m_Current;

		public void frmChooseImportPage_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public void frmChooseImportPage_Disposed(object sender, EventArgs e)
		{
			m_Current?.Dispose();
		}

		/// <summary>Selects from the given list of image filenames; returning the one to use, or "" if cancelled</summary>
		public static string Ask(List<string> colImages)
		{
			frmChooseImportPage frm = new frmChooseImportPage();
			frm.m_Images = colImages;
			frm.SetPage(0);
			if (frm.ShowDialog() != DialogResult.OK)
				return "";
			return colImages[frm.m_Page];
		}

		private void SetPage(int index)
		{
			m_Page = index;
			btnRight.Enabled = index < m_Images.Count - 1;
			btnLeft.Enabled = index > 0;
			lblPage.Text = Strings.Item("ChoosePage_Page").Replace("%0", (m_Page + 1).ToString()).Replace("%1", m_Images.Count.ToString());
			pnlPreview.Invalidate();
			m_Current?.Dispose();
			try
			{
				m_Current = Image.FromFile(m_Images[index]);
			}
			catch (Exception)
			{
				m_Current = null;
			}
			btnOK.Enabled = m_Current != null; // Cannot accept if the image is failing
		}

		public void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			Rectangle rct = new Rectangle(0, 0, pnlPreview.Width, pnlPreview.Height);
			if (m_Current != null)
			{
				GUIUtilities.CalcDestRect(m_Current.Size, ref rct);
				e.Graphics.DrawImage(m_Current, rct);
			}
			else
				e.Graphics.DrawString(Strings.Item("ChoosePage_Failed"), Font, Brushes.Red, rct, GUIUtilities.StringFormatCentreCentre);
		}

		public void btnLeft_Click(object sender, EventArgs e)
		{
			SetPage(m_Page - 1);
		}

		public void btnRight_Click(object sender, EventArgs e)
		{
			SetPage(m_Page + 1);
		}

	}
}
