using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;


namespace SAW
{
	public partial class frmPageSize
	{
		// Note that this is not necessarily act on the current document any longer
		private bool m_Filling;
		private readonly Page m_Page;
		private SizeF m_szDocumentRequires;
		private RectangleF m_PageBounds;
		private readonly Bitmap m_bmpThumbnail; // thumbnail of the current page, beginner that it can fill the preview panel

		private frmPageSize(Page page, SizeF documentRequires, bool isPaletteWithin)
		{
			// second parameter is the space required inside the margin for everything in the current document to be displayed correctly
			InitializeComponent();

			m_Page = page;
			m_szDocumentRequires = documentRequires;
			m_PageBounds = page.TotalBounds();
			m_bmpThumbnail = page.GenerateThumbnail2(pnlPreview.Size, Globals.Root.CurrentDocument.ApproxUnitScale);
			// this does not use EnsureThumbnail as we don't want to replace the existing one; this one is probably larger than normal
			m_Filling = true;
			try
			{
				if (isPaletteWithin)
				{
					nudWidth.Minimum = 10;
					nudHeight.Minimum = 10;
					if (page.IsSingleAutoSize)
						m_szDocumentRequires = SizeF.Empty; // The size warning doesn't really work if it is a flowing palette; it triggers if either dimension is reduced
				}
				nudWidth.Value = (decimal)Math.Min(Math.Max(page.PhysicalSize.Width, (float)nudWidth.Minimum), (float)nudWidth.Maximum); // min/max to prevent crashes, just in case
				nudHeight.Value = Math.Min(Math.Max((decimal)page.PhysicalSize.Height, nudHeight.Minimum), nudHeight.Maximum);
				UpdateWarning();
			}
			finally
			{
				m_Filling = false;
			}
		}

		public void frmPageSize_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public static DialogResult Display(Document document, Transaction transaction)
		{
			// Just displays the settings from the current page of the current document
			SizeF required = SizeF.Empty; // the size required for all of the shapes on all the pages
			SizeF test = document.Page(0).Size; // Check if all pages have the same size
			bool equal = true;
			for (int index = 0; index <= document.Count - 1; index++)
			{
				Page page = document.Page(index);
				SizeF pageSize = page.TotalBounds().Size;
				required = new SizeF(Math.Max(required.Width, pageSize.Width), Math.Max(required.Height, pageSize.Height));
				if (!page.Size.Equals(test))
					equal = false;
			}
			frmPageSize form = new frmPageSize(document.Page(Globals.Root.CurrentPageIndex), required, document.IsPaletteWithin);
			form.chkEqual.Checked = equal;
			DialogResult result = form.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				transaction.IsPageSizeChange = true;
				SizeF newSize = new SizeF((float)form.nudWidth.Value, (float)form.nudHeight.Value);
				if (document.IsPaletteWithin)
					document.PaletteDesignSize = newSize;
				for (int index = 0; index <= document.Count - 1; index++)
				{
					if (form.chkEqual.Checked || index == Globals.Root.CurrentPageIndex)
					{
						Page page = document.Page(index);
						transaction.Edit(page);
						page.SetSize(newSize, 0);
						if (page.IsSingleAutoSize && document.ActivityID.Equals(Activities.PaletteID) && page.Shapes.First() is IAutoSize)
						{
							// Probably editing a palette with a single flow.  It is better if we resize the flow to match the page
							IAutoSize flow = (IAutoSize)page.Shapes.First();
							transaction.Edit((Datum)flow);
							flow.SetBounds(page.Bounds, null); // Note that objPage.Size is not szNew (it excludes margins)
						}
						else
							page.BringShapesWithinPage(transaction);
						page.DiscardThumbnail();
					}
				}
			}
			return result;
		}

		#region Event responses

		public void nudWidth_ValueChanged(object sender, EventArgs e)
		{
			pnlPreview.Invalidate();
			if (m_Filling)
				return;
			UpdateWarning();
		}

		public void nudHeight_ValueChanged(object sender, EventArgs e)
		{
			pnlPreview.Invalidate();
			if (m_Filling)
				return;
			UpdateWarning();
		}

		#endregion


		private void UpdateWarning()
		{
			if (m_Page == null)
				return; // must be within InitialiseComponent; this will be called later within New
			SizeF current = new SizeF((float)nudWidth.Value, (float)nudHeight.Value);
			if (current.Width < m_szDocumentRequires.Width || current.Height < m_szDocumentRequires.Height)
			{
				lblInsufficient.Visible = true;
				lblInsufficient.Text = Strings.Item("PageSize_Insufficient");
			}
			else
				lblInsufficient.Visible = false;
		}

		public void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			float width = (float)nudWidth.Value;
			float height = (float)nudHeight.Value;
			if (width < 10 || height < 10)
				return;
			SizeF total = new SizeF(Math.Max(m_Page.Size.Width, width), Math.Max(m_Page.Size.Height, height));
			// pick scale so that it fills one of the dimensions
			float scale = Math.Min((pnlPreview.ClientSize.Width - 4) / total.Width, (pnlPreview.ClientSize.Height - 4) / total.Height);
			// Assuming that there is a 2 pixel border around each edge of the preview (And/or allowing a bit for rounding errors)
			// the original page can always be drawn bottom left, as this is the origin
			// however it looks nicer to centre it if the entire diagram has spare space horizontally or vertically
			PointF origin = new PointF((pnlPreview.ClientSize.Width - total.Width * scale) / 2, pnlPreview.ClientSize.Height - (pnlPreview.ClientSize.Height - total.Height * scale) / 2);
			float elementHeight = m_Page.Size.Height * scale; // height of the element being drawn
			Rectangle rctImage = new RectangleF(origin.X, origin.Y - elementHeight, m_Page.Size.Width * scale, elementHeight).ToRectangle();
			e.Graphics.DrawImage(m_bmpThumbnail, rctImage, 0, 0, m_bmpThumbnail.Width, m_bmpThumbnail.Height, GraphicsUnit.Pixel);

			// check if the new page needs to be offset (i.e. if narrower it might need to not be at the left of the old page)
			float X = 0;
			float Y = 0; // Y will be negative if offset (X positive)
			if (width < Math.Min(m_PageBounds.Right, m_Page.Size.Width))
				X = m_PageBounds.Left * scale;
			// this checks both the required bounds.Right and the actual page size.Width because sometimes there will be a shape sticking slightly outside the page
			// and it looks odd if the new page is shifted across to accommodate this, because it won't actually move if the settings are accepted
			if (height < Math.Min(-m_PageBounds.Top, m_Page.Size.Height))
				Y = Math.Max(m_PageBounds.Bottom * scale, -(m_Page.Size.Height - height) * scale); //Top + sngHeight
			elementHeight = height * scale;
			Rectangle newRect = new Rectangle((int)(origin.X + X), (int)(origin.Y - elementHeight + Y), (int)(width * scale), (int)elementHeight);
			using (Pen pn = new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
			{
				e.Graphics.DrawRectangle(pn, newRect);
			}

		}

	}
}
