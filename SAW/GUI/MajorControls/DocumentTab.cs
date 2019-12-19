using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	public class DocumentTab : Control
	{

		// tab width is calculated whenever the number of tabs changes or the control is resized
		private const int MAXIMUMWIDTH = 175; // starts at this size, but will squash up if there are too many
											  // There is no fixed minimum width as such, but some limitations are applied in SetSize to keep the graphics sane
		private int m_TabWidth = 175;
		private const int OVERLAP = 14; // the tabs are overlapped by this amount
		private static readonly StringFormat CustomStringFormat;

		static DocumentTab()
		{
			CustomStringFormat = new StringFormat
			{
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center,
				FormatFlags = StringFormatFlags.NoWrap,
				Trimming = StringTrimming.EllipsisCharacter
			};
		}

		public DocumentTab()
		{
			Paint += DocumentTab_Paint;
			MouseClick += DocumentTab_MouseClick;
			MouseLeave += DocumentTab_MouseLeave;
			MouseMove += DocumentTab_MouseMove;
			Resize += DocumentTab_Resize;
			if (!DesignMode && Globals.Root != null) // not sure DesignMode is set yet?
				Globals.Root.CurrentDocumentChanged += Root_CurrentDocumentChanged;
		}

		~DocumentTab()
		{
			if (!DesignMode)
				Globals.Root.CurrentDocumentChanged -= Root_CurrentDocumentChanged;
			if (m_EventDocument != null)
				m_EventDocument.StateChanged -= m_EventDocument_StateChanged;
		}

		private void DocumentTab_Paint(object sender, PaintEventArgs e)
		{
			if (this.DesignMode)
			{
				e.Graphics.Clear(Color.LightBlue);
				e.Graphics.DrawString("Document tab", Font, Brushes.Black, 0, 5);
				return;
			}
			// Because there is some shadow, they are drawn from right to left, except the selected one is drawn last
			for (int index = Count - 1; index >= 0; index--)
			{
				if (index != Selected)
				{
					if (!IsPalette(index))
						DrawTab(e.Graphics, index, Resources.AM.Tabs_Left, Resources.AM.Tabs_Centre, Resources.AM.Tabs_Right);
					else
						DrawTab(e.Graphics, index, Resources.AM.Tabs_PaletteLeft, Resources.AM.Tabs_PaletteCentre, Resources.AM.Tabs_PaletteRight);
				}
			}
			if (Selected >= 0)
			{
				if (!IsPalette(Selected))
					DrawTab(e.Graphics, Selected, Resources.AM.Tabs_LeftSelected, Resources.AM.Tabs_CentreSelected, Resources.AM.Tabs_RightSelected);
				else
					DrawTab(e.Graphics, Selected, Resources.AM.Tabs_PaletteLeftSelected, Resources.AM.Tabs_PaletteCentreSelected, Resources.AM.Tabs_PaletteRightSelected);
			}
			e.Graphics.DrawLine(Pens.Black, 0, Height - 1, Width, Height - 1);
		}

		private void DrawTab(Graphics gr, int index, Image imgLeft, Image imgCentre, Image imgRight)
		{
			Rectangle tabBounds = TabBounds(index);
			Rectangle sectionBounds = tabBounds;
			sectionBounds.Width -= tabBounds.Height * 2; // the left and right end caps have the same width as their height
			sectionBounds.X += tabBounds.Height;
			gr.DrawImage(imgCentre, sectionBounds);
			sectionBounds.X = tabBounds.X;
			sectionBounds.Width = tabBounds.Height;
			gr.DrawImage(imgLeft, sectionBounds);
			sectionBounds.X = tabBounds.Right - tabBounds.Height;
			gr.DrawImage(imgRight, sectionBounds);
			// we allow the text to float somewhat inside the end caps
			sectionBounds = tabBounds;
			sectionBounds.Inflate(-tabBounds.Height * 3 / 4, 0);
			sectionBounds.Width -= tabBounds.Height / 2; // but must move left to avoid the x
			gr.DrawString(TabText(index), Font, Brushes.Black, sectionBounds, CustomStringFormat);
			// and the close box...
			sectionBounds = CloseBounds(index);
			// close only drawn if >1 doc
			if (Globals.Root.DocumentsCount > 1)
			{
				if (m_Hover == index && m_CloseHover)
					gr.DrawImage(Resources.AM.Tabs_CloseHover, sectionBounds);
				else
					gr.DrawImage(Resources.AM.Tabs_Close, sectionBounds);
			}
		}

		public void DocumentsChanged()
		{
			m_Hover = -1;
			SetSize();
			Invalidate();
		}

		#region Event from current document

		private Document m_EventDocument;
		private void Root_CurrentDocumentChanged()
		{
			if (m_EventDocument != null)
				m_EventDocument.StateChanged -= m_EventDocument_StateChanged;
			m_EventDocument = Globals.Root.CurrentDocument;
			if (m_EventDocument != null)
				m_EventDocument.StateChanged += m_EventDocument_StateChanged;
		}

		private void m_EventDocument_StateChanged(object sender, EventArgs e)
		{
			// SAW6Doc Changed modified.  This should get hit every time name is changed, so no need for separate event for that
			if (Selected >= 0)
				Invalidate(TabBounds(Selected));
		}

		#endregion

		#region List of tabs/documents
		private int m_Hover = -1; // doesn't change the appearance at the moment, except for the close box
		private bool m_CloseHover = false; // true if the mouse is over the close box of the tab indicated by m_Hover

		public int Count => this.DesignMode ? 0 : Globals.Root.DocumentsCount;

		public string TabText(int index)
		{
			try
			{
				if (this.DesignMode || index >= Globals.Root.DocumentsCount)
					return "?";
				Document document = Globals.Root.Documents(index);
				string text = Strings.Translate(document.FileTitle());
				// translate is for palettes, which have a dummy filename
				if (document.Changed && !document.IsPaletteWithin)
					text = "* " + text;
				return text;
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
				return "?";
			}
		}

		public bool IsPalette(int index)
		{
			if (this.DesignMode || index >= Globals.Root.DocumentsCount)
				return false;
			return Globals.Root.Documents(index).PaletteWithin != null;
		}

		public int Selected
		{
			get
			{
				if (this.DesignMode)
					return -1;
				return Globals.Root.DocumentIndex;
			}
		}

		#endregion

		#region Mouse

		private void DocumentTab_MouseLeave(object sender, EventArgs e)
		{
			if (m_CloseHover)
				Invalidate(CloseBounds(m_Hover));
			m_Hover = -1;
			m_CloseHover = false;
		}

		private void DocumentTab_MouseMove(object sender, MouseEventArgs e)
		{
			int oldHover = m_Hover;
			bool oldCloseHover = m_CloseHover;
			m_Hover = HitTest(e.Location);
			if (m_Hover >= 0)
				m_CloseHover = Globals.Root.DocumentsCount >= 1 && CloseBounds(m_Hover).Contains(e.Location);
			else
				m_CloseHover = false;
			if (oldCloseHover != m_CloseHover)
			{
				if (oldHover >= 0)
					Invalidate(CloseBounds(oldHover));
				if (m_Hover >= 0)
					Invalidate(CloseBounds(m_Hover));
			}
		}

		private void DocumentTab_MouseClick(object sender, MouseEventArgs e)
		{
			DocumentTab_MouseMove(sender, e);
			if (m_Hover < 0)
				return;
			if (m_CloseHover && Globals.Root.DocumentsCount > 1)
			{
				int close = m_Hover; // in case external calls below trigger an internal update clearing m_Hover
				if (Globals.Root.Documents(close).Changed)
				{
					// if we need to ask about saving, then select the tab (which lets frmMain do its usual stuff)
					if (close != Selected)
						Globals.Root.SelectDocument(m_Hover);
					// otherwise we can just dump this document
					if (!((frmMain)this.FindForm()).CheckDiscard(Globals.Root.CurrentDocument))
						return;
				}
				Globals.Root.RemoveDocument(close); // Will select a different document if needed
				DocumentTab_MouseMove(sender, e); // cos a tab has disappeared - make sure all displayed OK
				Invalidate();
			}
			else // just clicked on main part of tab
			{
				if (m_Hover != Selected)
					Globals.Root.SelectDocument(m_Hover);
			}
		}

		#endregion

		#region Coordinates
		private Rectangle TabBounds(int index)
		{
			int X = index * (m_TabWidth - OVERLAP);
			return new Rectangle(X, 1, m_TabWidth, Height - 1);
		}

		private Rectangle CloseBounds(int index)
		{
			// the bounds of the close box of the given tab
			int size = Height / 3 + 2;
			Rectangle tab = TabBounds(index);
			return new Rectangle(tab.Right - tab.Height - size / 2, Height / 3 + 1, size, size);
		}

		private void SetSize()
		{
			if (Count == 0)
				m_TabWidth = MAXIMUMWIDTH;
			else
			{
				m_TabWidth = Math.Min(MAXIMUMWIDTH, Width / Count + OVERLAP);
				m_TabWidth = Math.Max(Math.Max(m_TabWidth, OVERLAP * 2), Height * 2 + 4);
			}
		}

		private void DocumentTab_Resize(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			int old = m_TabWidth;
			SetSize();
			if (old != m_TabWidth)
				Invalidate();
		}

		private int HitTest(Point pt)
		{
			// returns the index of the tab which has been hit
			int hit = -1;
			if (Selected >= 0 && TabBounds(Selected).Contains(pt))
				// the selected one gets priority where there is overlap
				hit = Selected;
			else if (pt.X < (m_TabWidth - OVERLAP) * Count + OVERLAP)
			{
				hit = pt.X / (m_TabWidth - OVERLAP);
				// The above can return Count if it is within the overlap region of the last one
				hit = Math.Min(Count - 1, hit);
			}
			return hit;
		}


		#endregion

	}

}
