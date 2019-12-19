using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SAW
{
	public sealed class ctrPages : ScrollableControl
	{

		// DisplayedIndex should be assigned whenever the list of pages in the document changes, even if the same page index is still displayed

		#region Events
		public delegate void DisplayPageEventHandler(int index);
		public event DisplayPageEventHandler DisplayPage;

		public delegate void PagesRearrangedEventHandler(Transaction transaction);
		public event PagesRearrangedEventHandler PagesRearranged;// this control has made the changes using the given transaction
																 // the main GUI need to store the transaction.  DisplayPage will always be called after this event to ensure the display finishes on the correct page

		public delegate void CentreViewOnEventHandler(PointF dataPoint);
		public event CentreViewOnEventHandler CentreViewOn;// requests that the main editing view be centred on the given point

		/// <summary>Tells the container to display the editing context menu.  pt is the location in local graphics coordinates</summary>
		public delegate void DisplayContextMenuEventHandler(Point pt);
		public event DisplayContextMenuEventHandler DisplayContextMenu;
		#endregion

		private int ImageWidth = 80;
		private int ImageHeight = 110; // actual thumbnails may be smaller following page A:R
		private readonly Timer m_tmrUpdateCurrent; // used to defer repaints of current page when edited only enabled when repaint pending

		public ctrPages()
		{
			Width = ImageWidth;
			this.BackColor = Color.FromKnownColor(KnownColor.ControlDark);
			m_tmrUpdateCurrent = new Timer();
			m_tmrUpdateCurrent.Tick += m_tmrUpdateCurrent_Tick;
			m_tmrUpdateCurrent.Interval = 1500;
			m_tmrUpdateCurrent.Enabled = false;
			this.DoubleBuffered = true;
			Globals.CurrentPageSizeChanged += PageSizeChanged;
			Globals.TransactionStored += Engine_TransactionStored;
			this.Resize += ctrPages_Resize;
			this.Paint += ctrPages_Paint;
			this.MouseUp += ctrPages_MouseUp;
			this.MouseMove += ctrPages_MouseMove;
			this.MouseDown += ctrPages_MouseDown;
			this.MouseLeave += ctrPages_MouseLeave;
			this.MouseClick += ctrPages_MouseClick;
		}


		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Globals.CurrentPageSizeChanged -= PageSizeChanged;
			Globals.TransactionStored -= Engine_TransactionStored;
		}

		#region Main properties
		private int m_DisplayedIndex; // page currently displayed by editor
		private int m_PageCountSeen = -1; // the number of pages in the document last time DisplayedIndex was updated

		[System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int DisplayedIndex
		{
			get{return m_DisplayedIndex;}
			set
			{
				if (m_Document == null)
				{
					Debug.Fail("ctrPages.DisplayedIndex should not be assigned before the document");
					return;
				}
				if (m_Document.Count != m_PageCountSeen)
				{
					// will need to update scrollbars etc
					m_PageCountSeen = m_Document.Count;
					Rectangle last = PageBounds(m_PageCountSeen - 1);
					this.AutoScrollMinSize = new Size(ImageWidth + this.Padding.Left + this.Padding.Right, last.Bottom + Padding.Bottom - this.AutoScrollPosition.Y);
					// - Me.AutoScrollPosition.Y is needed because PageBounds has corrected for the scrolling; but we don't want to, we need the location of this page in client coordinates
					UpdateParentWidth();
					Invalidate();
				}
				if (m_DisplayedIndex == value)
					return;
				if (m_tmrUpdateCurrent.Enabled)
				{
					// force repaint of current - otherwise when timer fires it will do wrong page
					m_tmrUpdateCurrent_Tick(this, EventArgs.Empty);
					m_tmrUpdateCurrent.Enabled = false;
				}
				m_DisplayedIndex = value;
				if (this.VScroll)
				{
					// Need to scroll this page into view
					Rectangle page = PageBounds(m_DisplayedIndex);
					// rctPage is in control coordinates after correcting for scrolling
					if (page.Y < 0)
					{
						// page is currently off the top
						this.AutoScrollPosition = new Point(0, PageBounds(m_DisplayedIndex, false).Y - this.Padding.Top);
					}
					else if (page.Bottom > Height)
					{
						// page is currently off the bottom
						this.AutoScrollPosition = new Point(0, PageBounds(m_DisplayedIndex, false).Bottom - Height + this.Padding.Top);
					}
				}
				Invalidate();
			}
		}

		private Document m_Document;
		[System.ComponentModel.Browsable(false), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Document Document
		{
			get{return m_Document;}
			set
			{
				if (m_Document == value)
					return;
				m_Document = value;
				m_DisplayedIndex = -1;
				m_PageCountSeen = -1;
				// m_tmrUpdateCurrent.Enabled = False
				// Invalidate() both done by...
				PageSizeChanged();
			}
		}

		// The part of the current page which is visible in the main view, in data coordinates.  Is RectangleF.Empty if the whole page is visible
		private RectangleF m_DisplayArea = RectangleF.Empty;
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public RectangleF DisplayArea
		{
			get{return m_DisplayArea;}
			set
			{
				m_DisplayArea = value;
				if (m_DisplayedIndex >= 0)
					Invalidate(PageBounds(m_DisplayedIndex));
			}
		}

		#endregion

		private void PageSizeChanged()
		{
			// this applies to all pages.  The thumbnail has already been discarded by the editing GUI
			Invalidate();
			// we use 95 pixels (-ish) for the average of page width and height
			SizeF sz = SizeF.Empty;
			for (int page = 0; page <= m_Document.Count - 1; page++)
			{
				sz.Width = Math.Max(sz.Width, m_Document.Page(page).Size.Width);
				sz.Height = Math.Max(sz.Height, m_Document.Page(page).Size.Height);
			}
			float average = (sz.Width + sz.Height) / 2;
			ImageWidth = (int)(95 * sz.Width / average);
			ImageHeight = (int)(95 * sz.Height / average);
			Width = ImageWidth;
			m_tmrUpdateCurrent.Enabled = false;
		}

		private void Engine_TransactionStored(object sender, Globals.TransactionEventArgs e)
		{
			if (e.Transaction.RequiresDocumentRepaint)
				Invalidate(); // The entire document structure might need updating; e.g. the number of pages has changed

			if (e.Transaction.CurrentPage == null || e.Transaction.CurrentPage == Globals.Root.CurrentPage)
				m_tmrUpdateCurrent.Enabled = true; // doesn't matter if already true
			else
			{
				// if something changes a different page, we do update the graphics immediately rather than using the timer
				int index = m_Document.IndexOf(e.Transaction.CurrentPage);
				if (index < 0)
				{
					Debug.Fail("Page does not appear in document, ignoring ctrPages.NonCurrentPageEdited");
					return;
				}
				m_Document.Page(index).DiscardThumbnail();
				Invalidate(PageBounds(index));
			}
		}


		#region Coordinates
		private const int VERTICALMARGIN = 5; // between pages
											  // Me.padding is used around the edges of the control
		private const int INSERTMARKERHEIGHT = 32;
		private const int INSERTMARKERWIDTH = 32;

		private Rectangle PageBounds(int index, bool adjustForScroll = true)
		{
			// if bolAdjustForScroll then the return value is in screen coordinates within this control
			// if false, then the returned rectangle is relative to the client origin
			// Me.AutoScrollPosition.Y returns negative numbers when the user has scrolled down
			return new Rectangle(Padding.Left, index * (ImageHeight + VERTICALMARGIN) + Padding.Top + (adjustForScroll ? this.AutoScrollPosition.Y : 0), ImageWidth, ImageHeight);
		}

		private int HitTest(Point click)
		{
			// returns index of page hit
			if (click.Y < Padding.Top + this.AutoScrollPosition.Y)
				return -1;
			int Y = click.Y - Padding.Top - this.AutoScrollPosition.Y;
			int index = Y / (ImageHeight + VERTICALMARGIN);
			Y -= index * (ImageHeight + VERTICALMARGIN);
			if (Y >= ImageHeight)
				return -1; // missed off the bottom of this image
			if (index >= m_Document.Count)
				return -1; // page doesn't exist
			return index;
		}

		private int HitTestInsert(Point click)
		{
			// returns index of page before which we would insert if dropping a page at this location
			int Y = click.Y - this.AutoScrollPosition.Y;
			if (Y < Padding.Top)
				return 0;
			Y -= Padding.Top;
			int index = Y / (ImageHeight + VERTICALMARGIN);
			Y -= index * (ImageHeight + VERTICALMARGIN);
			// If intY >= IMAGEHEIGHT Then Return -1 ' does not matter if we are in the gap between pages; ideal really!
			if (index >= m_Document.Count)
				return m_Document.Count; // off the end; will insert after last existing page
			if (Y < ImageHeight / 2)
				return index; // dropping on the top half of this page; insert before
			else
				return index + 1; // dropping on the bottom half of this page; insert after
		}

		private Rectangle InsertMarkerBounds(int index)
		{
			// area to invalidate to update the insert markup for the given page index
			if (index < 0)
				return Rectangle.Empty;
			int Y = index * (ImageHeight + VERTICALMARGIN) + Padding.Top - VERTICALMARGIN / 2 + this.AutoScrollPosition.Y;
			return new Rectangle(Padding.Left + ImageWidth + Padding.Right - INSERTMARKERWIDTH, Y - INSERTMARKERHEIGHT / 2, INSERTMARKERWIDTH, INSERTMARKERHEIGHT);
			// can't use Me.Width because that might leave us underneath the scrollbar
		}

		private void UpdateParentWidth()
		{
			// this control is always docked in a container; setting the size of the container changes the size of this control
			// if the vertical scrollbar appears we don't want to lose any content space, so we make the container (and this control) larger
			if (this.Parent == null || this.DesignMode)
				return; // can be the case during construction
			if (this.Width == 0)
				return;
			if (this.FindForm() != null && this.FindForm().WindowState == FormWindowState.Minimized)
				return;
			int required;
			if (this.VScroll)
				required = ImageWidth + this.Padding.Left + this.Padding.Right + 20; // not sure how to check the scrollbar width at the moment
			else
				required = ImageWidth + this.Padding.Left + this.Padding.Right;
			if (this.Parent.Width != required)
				this.Parent.Width = required; // not sure if the framework would completely ignore a redundant update
												 // so it is best to filter it out (especially as this can be called in response to Me.Resize)
		}

		private void ctrPages_Resize(object sender, EventArgs e)
		{
			// this may have added or removed the vertical scrollbar, so update the parent width
			UpdateParentWidth();
		}

		private Rectangle VisibleAreaLocal()
		{
			// the rectangle where the visible area is drawn, in control coordinates
			Rectangle pageBounds = PageBounds(m_DisplayedIndex);
			Page page = m_Document.Page(m_DisplayedIndex);
			if (page.Thumbnail == null)
				return Rectangle.Empty; // Had one error when starting in debug mode with this; not sure if it was a genuine problem
			pageBounds.Inflate((page.Thumbnail.Width - ImageWidth) / 2, (page.Thumbnail.Height - ImageHeight) / 2);
			float scale = pageBounds.Width / page.Size.Width;
			return new Rectangle((int)(m_DisplayArea.X * scale + pageBounds.X), (int)(m_DisplayArea.Top * scale + pageBounds.Bottom), (int)(m_DisplayArea.Width * scale), (int)(m_DisplayArea.Height * scale));
		}

		#endregion

		#region Graphics

		private void ctrPages_Paint(object sender, PaintEventArgs e)
		{
			if (this.DesignMode || m_Document == null)
				return;
			for (int index = 0; index <= m_Document.Count - 1; index++)
			{
				Page page = m_Document.Page(index);
				Rectangle pageBounds = PageBounds(index);
				page.EnsureThumbnail(new Size(ImageWidth, ImageHeight),m_Document.ApproxUnitScale);
				// need to ensure we don't keep all backgrounds in memory if generating them all at once:
				if (index != Globals.Root.CurrentPageIndex)
					page.BackgroundImage?.Release();
				pageBounds.Inflate((page.Thumbnail.Width - ImageWidth) / 2, (page.Thumbnail.Height - ImageHeight) / 2);
				e.Graphics.DrawImage(page.Thumbnail, pageBounds);
				if (index == m_DragPage && !m_DragVisible)
				{
					// when dragging fade the page
					using (SolidBrush fade = new SolidBrush(Color.FromArgb(224, Color.FromKnownColor(KnownColor.ControlLight))))
					{
						e.Graphics.FillRectangle(fade, pageBounds);
					}

				}
			}
			// hilite selected
			if (m_DisplayedIndex >= 0 && m_DisplayedIndex < m_Document.Count)
			{
				Rectangle pageBounds = PageBounds(m_DisplayedIndex);
				Page page = m_Document.Page(m_DisplayedIndex);
				// get exact bounds of this image
				pageBounds.Inflate((page.Thumbnail.Width - ImageWidth) / 2, (page.Thumbnail.Height - ImageHeight) / 2);
				pageBounds.Inflate(3, 3); // reinflates
				using (Pen pn = new Pen(Color.Red, 2) { DashStyle = DashStyle.Dash })
				{
					e.Graphics.DrawRectangle(pn, pageBounds);
				}

				// also show which part of the page is visible
				if (!m_DisplayArea.IsEmpty)
				{
					// note page Y starts at BL and will be -ve number
					Rectangle visible = VisibleAreaLocal(); //(m_rctDisplayArea.X * sngScale + rctPage.X, m_rctDisplayArea.Top * sngScale + rctPage.Bottom, 				'm_rctDisplayArea.Width * sngScale, m_rctDisplayArea.Height * sngScale)
					using (Pen pn = new Pen(Color.FromArgb(128, Color.Red), 1) { DashStyle = DashStyle.Dot })
					{
						e.Graphics.DrawRectangle(pn, visible);
					}

				}
			}
			if (m_DragPage >= 0 && !m_HasDraggedOutside && !m_DragVisible)
			{
				Page page = m_Document.Page(m_DragPage);
				page.EnsureThumbnail(m_DragPageLocation.Size , m_Document.ApproxUnitScale);
				e.Graphics.DrawImage(page.Thumbnail, m_DragPageLocation, GUIUtilities.DrawImage40Percent);
				if (m_DropBefore >= 0)
				{
					Rectangle insert = InsertMarkerBounds(m_DropBefore);
					if (!insert.IsEmpty)
						e.Graphics.DrawImage(Resources.AM.LeftRedArrrow, insert);
				}
			}
		}

		public void DoPendingRefresh()
		{
			// forces any refresh to be immediate and not wait for timer
			m_tmrUpdateCurrent_Tick(this, EventArgs.Empty);
		}

		private void m_tmrUpdateCurrent_Tick(object sender, EventArgs e)
		{
			try
			{
				if (m_DisplayedIndex < 0 || m_DisplayedIndex >= m_Document.Count) // must have lost a page(!?)
				{
					Debug.WriteLine("m_tmrUpdateCurrent_Tick: invalid page");
					return;
				}
				m_Document.Page(m_DisplayedIndex).DiscardThumbnail();
				Invalidate(PageBounds(m_DisplayedIndex));
			}
			finally
			{
				m_tmrUpdateCurrent.Enabled = false;
			}
		}

		#endregion

		#region Mouse
		private Rectangle m_DragTrigger = Rectangle.Empty; // is defined after a click when the user might be initiating a drag.  Anything outside this rectangle starts dragging
															  // uses screen coordinates, not data coordinates
		private int m_DragPage = -1; // <0 if not dragging
		private int m_HitPage = -1; // before drag starts this is the page that was hit
		private Size m_DragPageOffset; // offset from mouse to TL of page (-ve)
		private Rectangle m_DragPageLocation; // current position of dragged faint version.  undefined unless m_DragPage>=0
		private bool m_HasDraggedOutside; // mouse has left the building when dragging - no longer draw any targets
		private int m_DropBefore = -1; // the page before which the dragged page would be inserted, -1 if invalid
		private Size m_VisibleOffset; // offset from mouse to TL of the visible area
		private bool m_DragVisible; // true if the current drag is moving the visible area.  False means it is rearranging pages
									   // once the user moves far enough to trigger page rearrangement, it never reverts to adjusting the visible area

		private void ctrPages_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				if (Globals.Root.CurrentConfig.ReadBoolean(Config.Context_Menus))
					DisplayContextMenu?.Invoke(e.Location);
				return;
			}
			int pageIndex = HitTest(e.Location);
			if (pageIndex < 0 || pageIndex >= m_Document.Count)
				return;
			if (pageIndex != m_DisplayedIndex)
				DisplayPage?.Invoke(pageIndex);
			// don't also move within page (tried it and OK, but I think can be confusing)
			else if (!m_DisplayArea.IsEmpty)
			{
				// if m_rctDisplayArea is empty it implies that the entire page is displayed at once, so no point asking to scroll the display
				Rectangle pageBounds = PageBounds(pageIndex);
				Page page = m_Document.Page(pageIndex);
				PointF data = new PointF((e.X - pageBounds.Left) * page.Size.Width / pageBounds.Width,
					(pageBounds.Bottom - e.Y) * page.Size.Height / pageBounds.Height); //note that the Y coordinate is inverted between the screen and page
				CentreViewOn?.Invoke(data);
			}
		}

		private void ctrPages_MouseDown(object sender, MouseEventArgs e)
		{
			Size dragSize = SystemInformation.DragSize;
			m_HitPage = HitTest(e.Location);
			if (m_DragPage >= 0)
			{
				m_DragPage = -1; // safety in case left running
				Invalidate();
			}
			if (m_HitPage >= 0)
			{
				m_DragTrigger = new Rectangle(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
				Rectangle pageBounds = PageBounds(m_HitPage);
				m_DragPageOffset = new Size(pageBounds.Left - e.X, pageBounds.Top - e.Y);
				// movement starts by adjusting the visible area, until this becomes unfeasible
				Rectangle visible = VisibleAreaLocal();
				m_VisibleOffset = new Size(visible.Left - e.X, visible.Top - e.Y);
				m_DragVisible = true;
				// unless the entire page is visible, in which case we can only rearrange pages
				if (m_DisplayArea.IsEmpty)
					m_DragVisible = false;
			}
		}

		private void ctrPages_MouseLeave(object sender, EventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
		}

		private void ctrPages_MouseMove(object sender, MouseEventArgs e)
		{
			if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y) && m_HitPage >= 0)
			{
				m_DragPage = m_HitPage;
				m_DragTrigger = Rectangle.Empty;
				// will flow thru into next condition...
				m_DragPageLocation = PageBounds(m_DragPage);
				Capture = true;
				Invalidate();
				if (m_DragPage != m_DisplayedIndex)
					DisplayPage?.Invoke(m_DragPage);
			}
			if (m_DragPage >= 0)
			{
				// already dragging.  If the drag is quite small we will just move the visible area.  If it is larger we will start rearranging pages
				Rectangle visible = VisibleAreaLocal();
				visible.X = e.X + m_VisibleOffset.Width;
				visible.Y = e.Y + m_VisibleOffset.Height; // this is where the visible area would be if we just drag it
															   // if this has gone significantly outside the page that we will treat this as page rearranging
				Rectangle pageBounds = PageBounds(m_DragPage);
				pageBounds.Inflate(12, 20);
				if (!m_HasDraggedOutside && !m_DragVisible)
					Invalidate(m_DragPageLocation);
				if (m_DragVisible && pageBounds.Contains(visible))
				{
					pageBounds.Inflate(-12, -20);
					Page page = m_Document.Page(m_DragPage);
					// we then centre the view on the centre of this rectangle (see MouseClick)
					Point local = new Point(visible.X + visible.Width / 2, visible.Y + visible.Height / 2);
					PointF data = new PointF((local.X - pageBounds.Left) * page.Size.Width / pageBounds.Width, (pageBounds.Bottom - local.Y) * page.Size.Height / pageBounds.Height); //note that the Y coordinate is inverted between the screen and page
					CentreViewOn?.Invoke(data);
				}
				else
				{
					if (m_DragVisible)
						Invalidate(PageBounds(m_DragPage));
					m_DragVisible = false;
					// keep width and height of drag version, but move TL
					m_DragPageLocation.X = e.X + m_DragPageOffset.Width;
					m_DragPageLocation.Y = e.Y + m_DragPageOffset.Height;
					m_HasDraggedOutside = e.X < 0 || e.Y < 0 || e.X > Width || e.Y > Height;
					if (m_HasDraggedOutside)
					{
						if (m_DropBefore >= 0)
							Invalidate(InsertMarkerBounds(m_DropBefore));
						m_DropBefore = -1;
					}
					else
					{
						int newIndex = HitTestInsert(e.Location);
						if (newIndex == m_DragPage || newIndex == m_DragPage + 1)
							newIndex = -1; // moving page before or after itself want actually change anything!
						if (newIndex != m_DropBefore)
						{
							Invalidate(InsertMarkerBounds(m_DropBefore));
							Invalidate(InsertMarkerBounds(newIndex));
						}
						m_DropBefore = newIndex;
						Invalidate(m_DragPageLocation);
					}
				}

			}
		}

		private void ctrPages_MouseUp(object sender, MouseEventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
			if (m_DragPage >= 0)
			{
				Invalidate(PageBounds(m_DragPage));
				Invalidate(InsertMarkerBounds(m_DropBefore)); // doesn't really matter if m_DropBefore < 0
				Capture = false;
				if (!m_HasDraggedOutside && m_DropBefore >= 0) //else just ignored
				{
					Transaction transaction = new Transaction();
					m_Document.MovePage(m_DragPage, m_DropBefore, transaction);
					PagesRearranged?.Invoke(transaction);
					if (m_DropBefore > m_DragPage)
						DisplayPage?.Invoke(m_DropBefore - 1);
					else
						DisplayPage?.Invoke(m_DropBefore);
					Invalidate();
				}
				m_DragPage = -1;
				m_DropBefore = -1;
			}
		}

		#endregion

	}
}
