using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Control which can stack several collapsible Accordion controls vertically - collapsing them automatically when needed for space</summary>
	[System.ComponentModel.ToolboxItem(false)]
	public class AccordionContainer : ContainerControl, IComparable<AccordionContainer>
	{

		// these are now added at run time, and none are included at design time
		// adding is done initially as a single operation, although single palettes can be dragged in later

		public void AttachEvents()
		{
			ControlAdded += AccordionContainer_ControlAdded;
			ControlRemoved += AccordionContainer_ControlRemoved;
			Layout += AccordionContainer_Layout;
			Resize += AccordionContainer_Resize;
			DockChanged += AccordionContainer_DockChanged;
			MouseDown += AccordionContainer_MouseDown;
			MouseMove += AccordionContainer_MouseMove;
			MouseLeave += AccordionContainer_MouseLeave;
			MouseUp += AccordionContainer_MouseUp;
			Paint += AccordionContainer_Paint;
		}

		#region Adding controls
		private bool m_Building;
		private List<Accordion> m_Build; // when building they are only added to controls at the end - allowing them to be sorted first

		public void StartBuild()
		{
			SuspendLayout();
			m_Building = true;
			m_Build = new List<Accordion>();
			Controls.Clear();
		}

		public void EndBuild()
		{
			if (m_Build == null)
			{
				m_Building = false;
				Utilities.LogSubError(new Exception("AccordionContainer EndBuild with m_Build = Nothing"));
				ResumeLayout();
				return;
			}
			m_Build.Sort(); // sorts by Palette.DockIndex
			foreach (Accordion accordion in m_Build)
			{
				this.Controls.Add(accordion);
			}
			m_Build = null;
			m_Building = false;
			// We can't really try and sort out all the heights as the controls are added, because many of the button panels might be added
			// during the form construction; but the buttons will only be added afterwards, so we don't know how high they need to be until somewhat later
			for (int index = 0; index <= this.Controls.Count - 1; index++)
			{
				Accordion accordion = (Accordion)Controls[index];
				accordion.Width = this.Width - 1 - RESIZEWIDTH; // would be done by Me.ResumeLayout(),
				accordion.InitialisePosition();
				// make ones at bottom 'oldest' so they collapse first
				accordion.LastAccessed = accordion.LastAccessed.AddMinutes(-index);
			}
			CheckSizes(-1);
			SetVerticalPositions();
			ResumeLayout();
		}

		public void AddPalette(Palette palette)
		{
			if (palette.Accordion == null)
				palette.Accordion = new Accordion(palette);
			if (m_Building)
				m_Build.Add(palette.Accordion);
			else
				this.Controls.Add(palette.Accordion);

			// check some things which can change with config:
			palette.Accordion.Invalidate(); // (in case Keyboard_User has changed)
		}

		public void AccordionContainer_ControlAdded(object sender, ControlEventArgs e)
		{
			if (!(this.Controls[this.Controls.Count - 1] is Accordion))
				throw new InvalidOperationException("AccordionContainer can only contain accordion objects");
			Accordion control = (Accordion)e.Control;
			if (!this.DesignMode)
				control.Opening += Accordion_Opening;
		}

		public void AccordionContainer_ControlRemoved(object sender, ControlEventArgs e)
		{
			Accordion control = (Accordion)e.Control;
			if (!this.DesignMode)
				control.Opening -= Accordion_Opening;
		}

		#endregion

		#region Positioning
		private bool m_Repositioning ; // True when changing the vertical positions of all the controls; also used when automatically increasing the width during Resize
		private const int SPACING = 2;
		private int m_LeftResize = 2; // size of the left bar which allows resizing
		private int m_RightResize = 0; // likewise right.  One of them will always be 0.  Which is made available depends where this is docked.
		public const int RESIZEWIDTH = 2; // it is required that one of the above = RESIZEWIDTH and the other = 0; therefore the sum = RESIZEWIDTH

		private void SetVerticalPositions()
		{
			if (m_Repositioning)
				return;
			Debug.Assert(m_LeftResize == RESIZEWIDTH || m_RightResize == RESIZEWIDTH && (m_LeftResize == 0 || m_RightResize == 0));
			int Y = 0;
			m_Repositioning = true;
			try
			{
				for (int index = 0; index <= this.Controls.Count - 1; index++)
				{
					Accordion control = (Accordion)this.Controls[index];
					if (control.Visible)
					{
						control.Bounds = new Rectangle(m_LeftResize, Y, Width - 1 - RESIZEWIDTH, control.Height);
						Y += control.Height + SPACING;
					}
				}
			}
			finally
			{m_Repositioning = false;}
		}

		public void AccordionContainer_Layout(object sender, LayoutEventArgs e)
		{
			SetVerticalPositions();
		}

		public void AccordionContainer_Resize(object sender, EventArgs e)
		{
			bool widthChanged = Width != m_CurrentWidth;
			m_CurrentWidth = Width;
			if (Controls.Count == 0 || DesignMode || m_Building)
				return;
			Form form = FindForm(); // also don't want to do this if the form is minimised
			if (form == null || form.WindowState == FormWindowState.Minimized)
				return;
			if (widthChanged)
			{
				SuspendLayout();
				int minimumWidth = Math.Max(MINIMUMWIDTH, Width); // Width may need to be increased if any controls try and set themselves wider than this
				foreach (Accordion accordion in Controls)
				{
					int controlRequested = accordion.SetWidth(Width - RESIZEWIDTH);
					if (controlRequested > minimumWidth)
					{
						Debug.WriteLine("Accordion: " + accordion + " requested min width = " + controlRequested);
						minimumWidth = controlRequested;
					}
				}
				//Debug.WriteLine("AccordionContainer_Resize, Width=" + Width.ToString + "; intMinimumWidth=" + intMinimumWidth.ToString)
				if (minimumWidth > Width && !m_Repositioning)
				{
					m_Repositioning = true; // Will stop the Resize event triggered by the next line increasing the size (which might happen however if some control kept rounding up the width)
					Width = minimumWidth;
					m_Repositioning = false;
				}
				ResumeLayout();
			}
			CheckSizes(-1); // Me.Controls(Me.Controls.Count - 1).Bottom,
		}

		public void AccordionContainer_DockChanged(object sender, EventArgs e)
		{
			if (Dock == DockStyle.Left)
			{
				m_LeftResize = 0;
				m_RightResize = RESIZEWIDTH;
			}
			else // should be right
			{
				m_LeftResize = RESIZEWIDTH;
				m_RightResize = 0;
			}
		}

		#endregion

		#region Aging and auto-collapse

		private int m_CurrentWidth = 0;

		private int OldestOpen(int excluding = -1)
		{
			DateTime oldestTime = DateTime.MaxValue; // Now.AddDays(-1) ' DateTime.MinValue doesn't seem to work (returns time only?)
			int oldestIndex = -1;
			for (int index = 0; index <= this.Controls.Count - 1; index++)
			{
				Accordion accordion = (Accordion)this.Controls[index];
				if (accordion.Open && accordion.LastAccessed < oldestTime && index != excluding)
				{
					oldestIndex = index;
					oldestTime = accordion.LastAccessed;
				}
			}
			return oldestIndex;
		}

		private void Accordion_Opening(object sender, EventArgs e)
		{
			CheckSizes(this.Controls.IndexOf((Control)sender)); // intHeight,
		}

		private void CheckSizes(int dontCollapse)
		{
			// used both when one opens and when container is resized
			int height = TotalHeight();
			while (height > this.Height)
			{
				// need to collapse something
				int oldest = OldestOpen(dontCollapse);
				if (oldest >= 0)
				{
					height -= ((Accordion)Controls[oldest]).Height - ((Accordion)this.Controls[oldest]).ClosedHeight;
					((Accordion)Controls[oldest]).Open = false;
				}
				else
					break; // cannot find anything to close - stop trying
			}
		}

		private int TotalHeight()
		{
			// can't check Bottom of last control as we want to know the height AFTER all the animation is complete, not height NOW
			int height = 0;
			foreach (Accordion objAccordion in Controls)
			{
				height += objAccordion.Open ? objAccordion.OpenHeight : objAccordion.ClosedHeight;
				height += 2;
			}
			return height;
		}

		#endregion

		#region Information for organising layout

		public int DockLevel;
		/// <summary>unique key showing position </summary>
		public int Key
		{get {return PalettePosition.CalculateAccordionKey(Dock, DockLevel);}}

		public int CompareTo(AccordionContainer other)
		{
			return DockLevel.CompareTo(other.DockLevel);
		}

		/// <summary>number of controls - but works while building </summary>
		public int Count
		{get { return m_Building ? m_Build.Count : Controls.Count; }}

		public int FindInsertIndex(int Y)
		{
			// returns the control index to insert BEFORE if dropping palette here
			for (int index = 0; index <= Controls.Count - 1; index++)
			{
				if (Y < (Controls[index].Top + Controls[index].Bottom) / 2)
					return index;
			}
			return Controls.Count;
		}

		#endregion

		#region Resize support
		private int m_DragStart = int.MaxValue; // We only record the X value to test for dragging; Vertical movement is ignored
		private bool m_Resizing ;
		private int m_DragStartWidth; // the Width when we started resizing
		private const int MINIMUMWIDTH = 100;

		public event EventHandler ResizeWidthFinished;// triggered so form can update stored positions if this has resized

		public void AccordionContainer_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.X >= 0 && e.X < m_LeftResize || e.X >= Width - m_RightResize && e.X < Width)
				m_DragStart = e.X;
			else
				m_DragStart = int.MaxValue;
		}

		public void AccordionContainer_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_Resizing)
			{
				int X = PointToScreen(new Point(e.X, 0)).X;
				int width;
				if (m_LeftResize > 0)
					width = m_DragStartWidth + m_DragStart - X;
				else
					width = m_DragStartWidth + X - m_DragStart;
				if (width < MINIMUMWIDTH)
					width = MINIMUMWIDTH;
				Width = width;
			}
			else
			{
				if (m_DragStart < int.MaxValue)
				{
					Size dragSize = SystemInformation.DragSize;
					if (e.X < m_DragStart - dragSize.Width / 2 || e.X > m_DragStart + dragSize.Width / 2)
					{
						m_Resizing = true;
						// Convert the start position to screen coordinates (control coordinates might not be much use what result moving this control
						m_DragStart = PointToScreen(new Point(m_DragStart, 0)).X;
						m_DragStartWidth = Width;
					}
				}
				else if (e.Button == MouseButtons.None)
				{
					// change cursor if not dragging (or starting to) and in fact if mouse not already pressed
					if (e.X >= 0 && e.X < m_LeftResize || e.X >= Width - m_RightResize && e.X < Width)
						Cursor = Cursors.SizeWE;
					else
						Cursor = Cursors.Default;
				}
			}
		}

		public void AccordionContainer_MouseLeave(object sender, EventArgs e)
		{
			Cursor = Cursors.Default;
		}

		public void AccordionContainer_MouseUp(object sender, MouseEventArgs e)
		{
			if (m_Resizing)
				ResizeWidthFinished?.Invoke(this, EventArgs.Empty);
			m_DragStart = int.MaxValue;
			m_Resizing = false;
			Cursor = Cursors.Default;
		}

		public void AccordionContainer_Paint(object sender, PaintEventArgs e)
		{
			if (m_LeftResize > 0)
				e.Graphics.FillRectangle(Brushes.DarkGray, 0, 0, m_LeftResize, Height);
			else
				e.Graphics.FillRectangle(Brushes.DarkGray, Width - m_RightResize, 0, m_RightResize, Height);
		}

		#endregion

	}

}
