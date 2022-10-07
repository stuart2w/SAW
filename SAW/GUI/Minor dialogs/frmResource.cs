using System.Collections.Generic;
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Resources;

namespace SAW
{
	internal partial class frmResource
	{
		// This has an image list control just to set the size of the items; Images are not stored in the image list

		private List<string> m_IDs = new List<string>(); // the resource IDs of the items displayed in the list
		private List<Image> m_Images = new List<Image>(); // images currently displayed in list
		private bool m_Filling; // used to protect from DrawItem - can sometime happen DURING filling the list (when m_Images may not be complete)
		private string m_SelectedID = null;

		public frmResource()
		{
			InitializeComponent();
			ctrPreview.NoImageString = "";
			// Fill list of images
			FillImages(0, 256);
			foreach (RadioButton radio in pnlSizes.Controls)
			{
				radio.CheckedChanged += rdoSize_CheckedChanged;
			}
		}

		private int m_DisplayedMinimum;
		private int m_DisplayedMaximum; // used to avoid redundant updates
		private void FillImages(int minimum, int maximum)
		{
			// The parameters specify the range of image sizes permitted (height is used)
			if (minimum == m_DisplayedMinimum && maximum == m_DisplayedMaximum)
				return;
			m_DisplayedMinimum = minimum;
			m_DisplayedMaximum = maximum;
			ctrPreview.Image = null;
			lstImages.Items.Clear();
			m_IDs.Clear();
			m_Images.Clear();
			m_Filling = true;
			try
			{
				ResourceSet resources = GUIUtilities.RM.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, false, true);
				foreach (DictionaryEntry item in resources)
				{
					if (item.Value is Bitmap image)
					{
						if (image.Height >= minimum && image.Height <= maximum)
							m_IDs.Add((string)item.Key);
					}
				}
				m_IDs.Sort(); // although the IDs are not visible to the user; this does put them in some sort of order, and tend to group related ones
				foreach (string ID in m_IDs)
				{
					Bitmap image = (Bitmap)GUIUtilities.RM.GetObject(ID);
					ListViewItem newItem = lstImages.Items.Add("");
					m_Images.Add(image);
					newItem.Tag = ID;
				}
			}
			finally
			{
				m_Filling = false;
				lstImages.Invalidate();
			}
		}

		public static string ChooseImage(Form frmParent)
		{
			// picks an image; returns the resource ID
			frmResource form = new frmResource();
			form.Owner = frmParent;
			if (form.ShowDialog() == DialogResult.OK && form.m_SelectedID != null)
				return form.m_SelectedID;
			return "";
		}

		public void frmResource_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			pnlImage.Focus();
		}

		public void lstImages_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			if (m_Filling)
				return;
			Rectangle rct = e.Bounds;
			rct.Inflate(-1, -1);
			GUIUtilities.CalcDestRect(m_Images[e.ItemIndex].Size, ref rct);
			e.Graphics.DrawImage(m_Images[e.ItemIndex], rct);
			if (e.Item.Selected)
			{
				e.DrawFocusRectangle();
			}
		}

		public void lstStamps_DoubleClick(object sender, EventArgs e)
		{
			if (btnOK.Enabled)
				this.DialogResult = DialogResult.OK;
		}

		public void lstImages_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = lstImages.SelectedIndices.Count > 0;
			ctrPreview.Image = null;
			m_SelectedID = null;
			if (lstImages.SelectedIndices.Count > 0)
			{
				ListViewItem objItem = lstImages.SelectedItems[0];
				m_SelectedID = (string)objItem.Tag;
				ctrPreview.Image = m_Images[objItem.Index];
			}
		}

		private void rdoSize_CheckedChanged(object sender, EventArgs e)
		{
			// first find the selected option
			string selectedRange = "0-256";
			foreach (RadioButton radio in pnlSizes.Controls)
			{
				if (radio.Checked)
					selectedRange = (string)radio.Tag;
			}
			// Then decode it and display
			int minimum = Convert.ToInt32(selectedRange.Split('-')[0]);
			int maximum = Convert.ToInt32(selectedRange.Split('-')[1]);
			FillImages(minimum, maximum);
			// it doesn't matter that this function will usually fire twice.  FillImages will ignore extra calls
		}

	}
}
