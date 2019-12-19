using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;


namespace SAW
{
	/// <summary>Edits page background and grid</summary>
	public partial class frmPaper
	{

		private readonly Page m_Page; // we need to remember the page in order to draw the sample, and for changing the colour
		private bool m_Filling;
		private Paper m_Paper;
		private Transaction m_Transaction;
		/// <summary>is changed to true if the page size has been adjusted to fit a background image </summary>
		public bool ChangedSize;
		/// <summary>true if we are editing the default paper in an activity; it just disallows background pictures at the moment </summary>
		private readonly bool m_Default;
		private readonly RadioButton[] m_ImageModes;

		private frmPaper(Page page, Form owner, Transaction transaction, bool isDefault)
		{
			m_Transaction = transaction;
			m_Page = page;
			m_Paper = page.Paper;
			m_Default = isDefault;
			m_Filling = true;
			InitializeComponent();
			m_ImageModes = new[] { rdoImage0, rdoImage1, rdoImage2, rdoImage3, rdoImage4, rdoImage5 };
			for (int index = 0; index < m_ImageModes.Length; index++)
			{
				RadioButton rdo = m_ImageModes[index];
				if (index == (int)m_Page.BackgroundImageMode)
					rdo.Checked = true;
				rdo.CheckedChanged += ImageMode_CheckedChanged;
			}
			try
			{
				m_ImageModes[(int)m_Page.BackgroundImageMode].Checked = true;
			}
			catch { m_ImageModes[0].Checked = true; }
			m_Filling = false;
			Owner = owner;
			ReflectPaper();
		}

		public void frmPaper_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public static DialogResult EditPaper(Page page, Transaction transaction, Form owner, out bool sizeChanged, bool isActivityDefault = false)
		{
			sizeChanged = false;
			Paper objOriginalPaper = page.Paper;
			transaction.Edit(page);
			transaction.Edit(page.Paper);
			frmPaper frm = new frmPaper(page, owner, transaction, isActivityDefault);
			DialogResult result = frm.ShowDialog();
			if (result == DialogResult.OK)
			{
				if (frm.m_Page.LockARToBackgroundImage)
					frm.SetSizeToMatchImage();
				if (frm.m_Paper != objOriginalPaper)
				{
					// The paper object has needed to be changed due to change of type
					// objTransaction.Delete(objOriginalPaper) ' can't do this as it prevents it storing the pre-edit state - just storing the changes to the
					// page list will be enough to restore this if needed
					page.Paper = frm.m_Paper;
					if (page.Paper != objOriginalPaper)
						transaction.Create(page.Paper);
				}
				if (frm.ChangedSize)
				{
					page.BringShapesWithinPage(transaction);
					page.DiscardThumbnail();
					sizeChanged = true;
				}
			}
			else
				transaction.Cancel();
			return result;
		}

		private void ReflectPaper()
		{
			if (m_Filling)
				return;
			m_Filling = true;
			try
			{
				rdoGraph.Checked = m_Paper.PaperType == Paper.Papers.Graph;
				rdoPlain.Checked = m_Paper.PaperType == Paper.Papers.Plain;
				rdoSquare.Checked = m_Paper.PaperType == Paper.Papers.Square;
				nudSize.Value = Math.Min(nudSize.Maximum, (decimal)(m_Paper.PaperType == Paper.Papers.Ruled ? m_Paper.YInterval : m_Paper.XInterval));
				nudSize.Enabled = m_Paper.PaperType != Paper.Papers.Plain;
				bool secondary = true;
				switch (m_Paper.PaperType)
				{
					case Paper.Papers.Graph:
						nudSize.Increment = 10;
						nudSize.Value = (decimal)Math.Min((float)nudSize.Maximum, m_Paper.XInterval * m_Paper.GraphMultiple); // displays size of large box
						nudSecondarySize.Increment = 1;
						nudSecondarySize.Value = m_Paper.GraphMultiple;
						lblSize.Text = Strings.Item("Paper_Size");
						lblSecondarySize.Text = Strings.Item("Paper_GraphInterval");
						break;
					case Paper.Papers.Square:
						nudSize.Increment = 5;
						nudSecondarySize.Increment = 5;
						nudSecondarySize.Value = (decimal)m_Paper.YInterval;
						lblSize.Text = Strings.Item("Paper_XSize");
						lblSecondarySize.Text = Strings.Item("Paper_YSize");
						break;
					default:
						secondary = false;
						lblSize.Text = Strings.Item("Paper_Size");
						break;
				}
				nudSecondarySize.Visible = secondary;
				lblSecondarySize.Visible = secondary;
				chkDraw.Checked = m_Paper.GridVisible;
				chkDotted.Checked = m_Paper.Dotted;
				chkBigDots.Checked = m_Paper.DotSize > Paper.STANDARDDOTSIZE;
				chkDraw.Enabled = m_Paper.PaperType != Paper.Papers.Plain;
				chkDotted.Enabled = chkDraw.Checked && chkDraw.Enabled && chkDraw.Enabled;
				chkBigDots.Enabled = chkDotted.Checked;
				chkPrintBackground.Checked = m_Paper.PrintBackground;
				if (m_Paper.PaperType != Paper.Papers.Plain && nudSize.Value > 0)
					Paper.DefaultSpacing = (float)nudSize.Value;
				ctrColour.CurrentColour = m_Page.Colour;
				ctrGridColour.CurrentColour = m_Paper.GridColour;
				ctrGridColour.Visible = m_Paper.GridVisible;
				lblGridColour.Visible = m_Paper.GridVisible;
				pnlSample.Invalidate();
				lnkBackgroundImage.Text = Strings.Item(m_Page.BackgroundImage == null ? "Paper_BackgroundImage" : "Paper_ChangeBackgroundImage");
				lnkRemoveBackground.Visible = m_Page.BackgroundImage != null;
				lnkBackgroundImage.Visible = !m_Default;
				foreach (RadioButton rdo in m_ImageModes)
				{
					rdo.Enabled = m_Page.BackgroundImage != null && !m_Default;
				}
				lblImageNoLockHeader.Enabled = lblImageModeHeader.Enabled = m_Page.BackgroundImage != null && !m_Default;
			}
			finally
			{
				m_Filling = false;
			}
		}

		#region Control event handlers
		public void rdoPlain_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			Paper.Papers type = Paper.Papers.Square;
			if (rdoGraph.Checked)
				type = Paper.Papers.Graph;
			if (rdoPlain.Checked)
				type = Paper.Papers.Plain;
			if (rdoSquare.Checked)
				type = Paper.Papers.Square;
			if (type == m_Paper.PaperType)
				return;
			m_Paper = Paper.CreateNew(type, m_Paper);
			ReflectPaper();
		}

		public void chkDraw_CheckedChanged(object sender, EventArgs e)
		{
			chkDotted.Enabled = chkDraw.Checked && chkDraw.Enabled;
			if (m_Filling)
				return;
			m_Paper.GridVisible = chkDraw.Checked;
			pnlSample.Invalidate();
			ctrGridColour.Visible = m_Paper.GridVisible;
			lblGridColour.Visible = m_Paper.GridVisible;
		}

		public void chkDotted_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Paper.Dotted = chkDotted.Checked;
			chkBigDots.Enabled = chkDotted.Checked;
			pnlSample.Invalidate();
		}

		public void chkBigDots_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Paper.DotSize = chkBigDots.Checked ? 4 : Paper.STANDARDDOTSIZE;
			pnlSample.Invalidate();
		}

		public void nudSize_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			switch (m_Paper.PaperType)
			{
				case Paper.Papers.Ruled:
					m_Paper.SetIntervals(0, (float)nudSize.Value);
					break;
				case Paper.Papers.Graph:
					if (nudSecondarySize.Value <= 0)
						return;
					float size = (float)(nudSize.Value / nudSecondarySize.Value);
					m_Paper.SetIntervals(size, size, (int)nudSecondarySize.Value);
					break;
				default:
					m_Paper.SetIntervals((float)nudSize.Value);
					break;
			}
			if (nudSize.Value > 0)
				Paper.DefaultSpacing = (float)nudSize.Value;
			pnlSample.Invalidate();
		}

		public void nudSecondarySize_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			switch (m_Paper.PaperType)
			{
				case Paper.Papers.Graph:
					float smallSize = m_Paper.XInterval; // main interval hasn't changed, but can't simply be read from nudSize
					float largeSize = smallSize * m_Paper.GraphMultiple;
					smallSize = largeSize / (float)nudSecondarySize.Value;
					m_Paper.SetIntervals(smallSize, smallSize, (int)nudSecondarySize.Value);
					break;
				case Paper.Papers.Square:
					m_Paper.SetIntervals((float)nudSize.Value, (float)nudSecondarySize.Value);
					break;
				default:
					Debug.Fail("Not expecting changes to secondary size for paper type: " + m_Paper.PaperType);
					break;
			}
			pnlSample.Invalidate();
		}

		public void ctrColour_UserChangedColour(object sender, EventArgs e)
		{
			m_Page.Colour = ctrColour.CurrentColour;
			pnlSample.Invalidate();
		}

		public void ctrGridColour_UserChangedColour(object sender, EventArgs e)
		{
			m_Paper.GridColour = ctrGridColour.CurrentColour;
			pnlSample.Invalidate();
		}

		public void chkPrintBackground_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Paper.PrintBackground = chkPrintBackground.Checked;
		}

		#endregion

		#region Image

		public void lnkBackgroundImage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Alternate filter used; non-bitmaps may not work (especially if tiling)
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image, "[Filter_Image1]");
			if (string.IsNullOrEmpty(filename))
				return;
			m_Page.BackgroundImage = (SharedImage)Globals.Root.CurrentDocument.AddSharedResourceFromFile(filename, m_Transaction);
			if (m_Page.BackgroundImage != null) // just in case
			{
				SizeF imageSize = m_Page.BackgroundImage.GetSize(64); // small size provided here means a resource (!?!) doesn't trigger
				if (imageSize.Width > 100 && imageSize.Height > 100 && Globals.Root.CurrentDocument.Count == 1)
				{
					// This is only done if the document has a single page.  
					if (GUIUtilities.QuestionBox(Strings.Item("Paper_MatchImageSize"), MessageBoxButtons.YesNo) == DialogResult.Yes)
						SetSizeToMatchImage();
				}
			}
			ReflectPaper();
		}

		private void SetSizeToMatchImage()
		{
			SizeF imageSize = m_Page.BackgroundImage.GetSize(64);
			imageSize = imageSize.MultiplyBy((Geometry.INCH / 96) * GUIUtilities.MillimetreSize); // nominal size in system units
			imageSize.Width += m_Page.Margin * 2; // Add-on some extra for the margins, which are not drawn
			imageSize.Height += m_Page.Margin * 2; // The specified size includes these however
			m_Page.SetSize(imageSize, m_Page.Margin);
			ChangedSize = true; // on exit various things need to be updated, including bringing the shapes within the page
			m_Transaction.Edit(Globals.Root.CurrentDocument);
			var header = Globals.Root.CurrentDocument.SAWHeader;
			header.SetWindowBounds(new Rectangle(header.MainWindowBounds.Location, imageSize.ToSize()));
		}

		private void lnkRemoveBackground_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_Page.BackgroundImage = null;
			ReflectPaper();
		}

		private void ImageMode_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RadioButton rdo = (RadioButton)sender;
			if (!rdo.Checked)
				return;
			m_Page.BackgroundImageMode = (Page.BackgroundImageModes)int.Parse(rdo.Tag.ToString());
			pnlSample.Invalidate();
		}


		#endregion

		public void pnlSample_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(m_Page.Colour);
			using (var canvas = new NetCanvas(e.Graphics))
			{
				// if locking page to image AR, then show the page colour in the correct proportions, with a dead grey area outside
				if (m_Page.BackgroundImage != null && m_Page.BackgroundImageMode == Page.BackgroundImageModes.LockAR)
				{
					e.Graphics.Clear(Color.Gray);
					Size imageSize = m_Page.BackgroundImage.GetSize();
					imageSize.Width *= 100; imageSize.Height *= 100;  // ensures image is too large, and therefore ScaleDestRect will fill the rectangle in one direction
					Rectangle bounds = new Rectangle(0, 0, pnlSample.Width, pnlSample.Height);
					GUIUtilities.CalcDestRect(imageSize, ref bounds);
					e.Graphics.FillRectangle(new SolidBrush(m_Page.Colour), bounds);
				}

				if (m_Page.BackgroundImage != null)
					Page.DrawBackgroundImage(canvas, m_Page.BackgroundImage, m_Page.BackgroundImageMode, new RectangleF(0, 0, pnlSample.Width, pnlSample.Height), 1);
				e.Graphics.TranslateTransform(0, pnlSample.Height);
				//e.Graphics.PageUnit = GraphicsUnit.Millimeter
				e.Graphics.ScaleTransform(e.Graphics.DpiX / Geometry.INCH, e.Graphics.DpiY / Geometry.INCH);
				// if the grid is not drawn, just draw it very faintly
				Color colReal = m_Paper.GridColour;
				if (!m_Paper.GridVisible)
					m_Paper.GridColour = Color.FromArgb(50, m_Paper.GridColour);
				m_Paper.GridVisible = true;
				m_Paper.Dotted = chkDotted.Checked && chkDraw.Checked; // dotted is not compatible with transparent
				m_Paper.Draw(canvas, m_Page, 1);

				m_Paper.GridVisible = chkDraw.Checked;
				m_Paper.Dotted = chkDotted.Checked;
				m_Paper.GridColour = colReal;
			}
		}

	}
}
