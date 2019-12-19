using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;


namespace SAW
{
	public partial class ctrButtonStyleEdit : IParameterConsumer
	{
		// the part of the button editor which deals with the content of a style - can also be used to edit
		// document styles directly, not via a button

		private ButtonStyle m_Style;
		private bool m_Filling = true; // will also stop DesignMode doing owt

		private bool m_Attached;// record whether controls attached to me as ParameterGUI
								// (only possible once Style is set)

		public ctrButtonStyleEdit()
		{
			InitializeComponent();
			Globals.UseParameterScreen(this);
			rdoSelected.Visible = false;
			pnlBorderStyle.AutoScaleMode = AutoScaleMode.None;

			cmbBackground.Items.Add(Strings.Item("Button_Simple"));
			cmbBackground.Items.Add(Strings.Item("Button_Picture"));
			cmbBackground.Items.Add(Strings.Item("Button_Tool"));
			cmbBackground.Items.Add(Strings.Item("Button_Glass"));
			cmbBackground.Items.Add(Strings.Item("Button_Windows"));

			pnlSimple.Dock = DockStyle.Fill;
			pnlCustomImage.Dock = DockStyle.Fill;
			pnlSoftwareImages.Dock = DockStyle.Fill;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					components?.Dispose();
				if (!IsDisposed)
					Globals.RemoveParameterScreen(this);
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public void DisplayStyle(ButtonStyle style)
		{
			// Most of this is actually initialisation, some of which can only be done once there is a style
			// the caller the end to the standard DisplayStyle does the GUI update
			m_Style = style;
			if (!m_Attached)
			{
				pnlBorderStyle.ConstructLinePalette(ParameterSupport.LineWidthsPlusVeryThin);
				pnlBorderStyle.ButtonSize = (int)(38 * GUIUtilities.SystemDPIRelative);
				ctrBorderColour.SetStandard18Colours();
				ctrBorderColour.AttachParameter(Parameters.LineColour);
				pnlFillStyle.ButtonSize = (int)(38 * GUIUtilities.SystemDPIRelative);
				//pnlFillStyle.ButtonSize = pnlFillStyle.ButtonSize * SystemDPIRelative
				pnlFillStyle.ConstructFillStylePalette();
				ctrFillColour.SetStandard18Colours();
				ctrFillColour.AttachParameter(Parameters.FillColour);
				ctrTextColour.SetStandard18Colours();
				ctrTextColour.AttachParameter(Parameters.TextColour);
				m_Attached = true;
			}
			m_Filling = false;
			DisplayStyle();
		}

		private void DisplayStyle()
		{
			bool oldFilling = m_Filling;
			m_Filling = true;
			cmbBackground.SelectedIndex = (int)m_Style.ImageType;
			pnlSimple.Visible = m_Style.ImageType == ButtonStyle.ImageTypes.None;
			pnlCustomImage.Visible = m_Style.ImageType == ButtonStyle.ImageTypes.Custom;
			pnlSoftwareImages.Visible = m_Style.ImageType != ButtonStyle.ImageTypes.None && m_Style.ImageType != ButtonStyle.ImageTypes.Custom;
			switch (m_Style.BorderShape)
			{
				case ButtonStyle.BorderShapes.Rectangle:
					rdoRectangular.Checked = true;
					break;
				case ButtonStyle.BorderShapes.Ellipse:
					rdoEllipse.Checked = true;
					break;
				case ButtonStyle.BorderShapes.Rounded:
					rdoRounded.Checked = true;
					break;
				default:
					Utilities.LogSubError("Unexpected BorderStyle.BorderShape: " + m_Style.BorderShape);
					break;
			}

			if (m_Style.ImageType == ButtonStyle.ImageTypes.None)
			{
				// raising the events will call the GUI items to update as needed
				ctrFillColour.SelectedColour = Color.FromArgb(m_Style.FillStyle[(int)State].ParameterValue(Parameters.FillColour));
				ctrBorderColour.SelectedColour = Color.FromArgb(m_Style.LineStyle[(int)State].ParameterValue(Parameters.LineColour));
				ctrTextColour.SelectedColour = m_Style.TextColour[(int)State];
				Globals.OnParameterChanged(Parameters.LineWidth);
				Globals.OnParameterChanged(Parameters.LinePattern);
				Globals.OnParameterChanged(Parameters.FillPattern);
			}

			if (m_Style.ImageType == ButtonStyle.ImageTypes.Custom)
				DisplayImageIndex();

			m_Filling = oldFilling;
		}

		private ButtonShape.States State
		{
			get
			{
				if (rdoSelected.Checked)
					return ButtonShape.States.Selected;
				if (rdoHighlight.Checked)
					return ButtonShape.States.Highlight;
				return ButtonShape.States.Normal;
			}
		}

		public void rdoNormal_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			DisplayStyle();
			pnlSoftwarePreview.Invalidate();
		}

		public void cmbBackground_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Style.ImageType = (ButtonStyle.ImageTypes)cmbBackground.SelectedIndex;
			DisplayStyle();
			pnlSoftwarePreview.Refresh();
		}

		public void rdoRectangular_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (rdoRectangular.Checked)
				m_Style.BorderShape = ButtonStyle.BorderShapes.Rectangle;
			else if (rdoRounded.Checked)
				m_Style.BorderShape = ButtonStyle.BorderShapes.Rounded;
			else if (rdoEllipse.Checked)
				m_Style.BorderShape = ButtonStyle.BorderShapes.Ellipse;
		}

		#region IParameterGUI

		public int ParameterValue(Parameters parameter)
		{
			switch (parameter)
			{
				case Parameters.LinePattern:
				case Parameters.LineColour:
				case Parameters.LineWidth:
					return m_Style.LineStyle[(int)State].ParameterValue(parameter);
				case Parameters.FillColour:
				case Parameters.FillPattern:
					return m_Style.FillStyle[(int)State].ParameterValue(parameter);
				case Parameters.TextColour:
					return m_Style.TextColour[(int)State].ToArgb();
				default:
					return 0; // other params will call here as engine redirects everything here
			}
		}
		public void SetParameterValue(int value, Parameters parameter)
		{
			switch (parameter)
			{
				case Parameters.LineColour:
				case Parameters.LinePattern:
				case Parameters.LineWidth:
					m_Style.LineStyle[(int)State].SetParameterValue(value, parameter);
					break;
				case Parameters.FillColour:
				case Parameters.FillPattern:
					m_Style.FillStyle[(int)State].SetParameterValue(value, parameter);
					break;
				case Parameters.TextColour:
					m_Style.TextColour[(int)State] = Color.FromArgb(value);
					break;
			}
			Globals.OnParameterChanged(parameter);
		}

		#endregion

		public void pnlSoftwarePreview_Paint(object sender, PaintEventArgs e)
		{
			if (Globals.Root == null)
				return;
			RectangleF destination = new RectangleF(0, 0, pnlSoftwarePreview.Width, pnlSoftwarePreview.Height);
			using (NetCanvas canvas = new NetCanvas(e.Graphics))
			{
				switch (m_Style.ImageType)
				{
					case ButtonStyle.ImageTypes.RoundButton:
						ScalableImage.RoundButton((int)State).Draw(canvas, destination);
						break;
					case ButtonStyle.ImageTypes.GlassButton:
						ScalableImage.PaletteButton((int)State).Draw(canvas, destination);
						break;
					case ButtonStyle.ImageTypes.Windows:
						ButtonRenderer.DrawButton(e.Graphics, destination.ToRectangle(), false, ButtonStyle.WindowsPushButtonState(State));
						break;
				}
			}

		}

		#region Custom images
		private int m_ImageIndex;
		private bool m_SliceEdit; // true when in slice editing mode
		private Size m_PreviewSize; // the size to draw the preview button; initial value is set by pnlPreview_Resize
		private ScalableImage Img
		{
			get
			{
				if (this.DesignMode || m_Style == null)
					return null;
				return m_Style.CustomImage[(int)State];
			}
		}

		private bool Valid
		{
			get
			{
				if (Img == null)
					return false;
				if (Img.Images.Count == 0)
					return false;
				return true;
			}
		}

		private void DisplayImageIndex()
		{
			// also checks index is within bounds
			btnLeft.Visible = Valid;
			btnRight.Visible = Valid;
			bool oldFilling = m_Filling;
			m_Filling = true;
			if (!Valid)
				lblImageIndex.Text = Strings.Item("Button_ImageNone");
			else
			{
				m_ImageIndex = Math.Min(Math.Max(m_ImageIndex, 0), Img.Images.Count - 1);
				lblImageIndex.Text = Strings.Item("Button_ImageIndex").Replace("%0", (m_ImageIndex + 1).ToString()).Replace("%1", Img.Images.Count.ToString());
				btnLeft.Enabled = m_ImageIndex > 0;
				btnRight.Enabled = m_ImageIndex < Img.Images.Count - 1;
				chkSlice.Checked = !Img.Slices[m_ImageIndex].IsEmpty;
			}
			pnlPreview.Visible = Valid;
			btnRemoveImage.Visible = Valid;
			chkSlice.Visible = Valid;
			lblSlicePrompt.Visible = Valid;
			Reflect9Slice();
			m_Filling = oldFilling;
		}

		public void btnLeft_Click(object sender, EventArgs e)
		{
			m_ImageIndex -= 1;
			DisplayImageIndex();
		}

		public void btnRight_Click(object sender, EventArgs e)
		{
			m_ImageIndex += 1;
			DisplayImageIndex();
		}

		public void btnAddImage_Click(object sender, EventArgs e)
		{
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image);
			if (string.IsNullOrEmpty(filename))
				return;
			try
			{
				if (m_Style.CustomImage[(int)State] == null)
					m_Style.CustomImage[(int)State] = new ScalableImage();
				Img.AddImageFromDisc(filename);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
			m_ImageIndex = Img.Images.Count - 1;
			DisplayImageIndex();
		}

		public void btnRemoveImage_Click(object sender, EventArgs e)
		{
			Img.Images.RemoveAt(m_ImageIndex);
			// index will be updated in DisplayImageIndex if out of bounds
			if (Img.Images.Count == 0)
				m_Style.CustomImage[(int)State] = null;
			// this is needed because otherwise the shape in the page may try and call this; which causes masses of LogSubError
			DisplayImageIndex();
		}

		public void chkSlice_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (Valid)
			{
				if (chkSlice.Checked)
				{
					if (Img.Slices[m_ImageIndex].IsEmpty)
					{
						Size sz = Img.Sizes[m_ImageIndex];
						Img.Slices[m_ImageIndex] = new Rectangle(sz.Width / 3, sz.Height / 3, sz.Width - 2 * sz.Width / 3, sz.Height - 2 * sz.Height / 3);
						// odd width and height intended to deal with rounding - adds any bits lost in rounding down to middle
					}
				}
				else
					Img.Slices[m_ImageIndex] = Rectangle.Empty;
			}
			Reflect9Slice();
		}

		private void Reflect9Slice()
		{
			if (!Valid || Img.Slices[m_ImageIndex].Equals(Rectangle.Empty))
				m_SliceEdit = false;
			else
				m_SliceEdit = rdoEditSlice.Checked;
			if (m_SliceEdit)
				lblSlicePrompt.Text = Strings.Item("Button_9SliceEditPrompt");
			else
			{
				lblSlicePrompt.Text = Strings.Item("Button_PreviewPrompt");
				pnlPreview.Cursor = Cursors.Default;
			}
			lnkCopy9.Visible = chkSlice.Checked;
			pnlSliceEdit.Visible = Valid && chkSlice.Checked;
			pnlPreview.Refresh();
		}

		public void pnlCustomTable_Resize(object sender, EventArgs e)
		{
			pnlPreview.Width = Math.Max(Math.Min(pnlPreview.Width, pnlCustomTable.Width / 2 - 1), 20);
		}

		public void pnlPreview_Paint(object sender, PaintEventArgs e)
		{
			if (!Valid)
				return;
			Rectangle slice = Img.Slices[m_ImageIndex];
			if (slice.IsEmpty)
			{
				// no nine slice scaling
				Img.Images[m_ImageIndex].Draw(e.Graphics, new RectangleF(0, 0, m_PreviewSize.Width, m_PreviewSize.Height));
			}
			else if (m_SliceEdit == false)
			{
				// nine slice scaling in preview mode
				ScalableImage.Draw9Slice(new NetCanvas(e.Graphics), new RectangleF(0, 0, m_PreviewSize.Width, m_PreviewSize.Height), new SharedImage(Img.Images[m_ImageIndex]), slice);
			}
			else
			{
				Img.Images[m_ImageIndex].Draw(e.Graphics, new RectangleF(0, 0, pnlPreview.Width, pnlPreview.Height));
				// editing mode - draw preview lines.  The image always draws using the entire panel for this
				Size size = Img.Sizes[m_ImageIndex];
				float X = (float)pnlPreview.Width / size.Width;
				float Y = (float)pnlPreview.Height / size.Height;
				e.Graphics.DrawLine(Pens.Red, slice.X * X, 0, slice.X * X, size.Height * Y);
				e.Graphics.DrawLine(Pens.Red, slice.Right * X, 0, slice.Right * X, size.Height * Y);
				e.Graphics.DrawLine(Pens.Red, 0, slice.Y * Y, size.Width * X, slice.Y * Y);
				e.Graphics.DrawLine(Pens.Red, 0, slice.Bottom * Y, size.Width * X, slice.Bottom * Y);
			}
		}

		public void rdoEditSlice_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			Reflect9Slice();
		}

		public void pnlPreview_Resize(object sender, EventArgs e)
		{
			// Note that this MUST execute even if m_bolFilling - the assets the initial value of m_szPreview
			// any resize of perform resets the preview to fill the panel
			m_PreviewSize = pnlPreview.Size;
		}

		#region Dragging within preview to change preview size or edit
		private Rectangle m_DragTrigger = Rectangle.Empty;
		private bool m_Dragging = false;
		private int m_DragLine = 0; // switch of the slice lines we are dragging (if in slice editing mode)
		public void pnlPreview_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (m_SliceEdit)
				{
					int line = FindSliceLine(new Point(e.X, e.Y));
					if (line <= 0)
						return; // can only start dragging if over a line in editing mode
					m_DragLine = line;
				}
				Size szDragSize = SystemInformation.DragSize;
				m_DragTrigger = new Rectangle(e.X - szDragSize.Width / 2, e.Y - szDragSize.Height / 2, szDragSize.Width, szDragSize.Height);
			}
		}

		public void pnlPreview_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_Dragging)
			{
				if (m_SliceEdit)
				{
					// easiest to calculate both X and Y now, rather than in various places below; although only one will actually be needed
					Size size = Img.Sizes[m_ImageIndex];
					float X = e.X * size.Width / (float)pnlPreview.Width;
					float Y = e.Y * size.Height / (float)pnlPreview.Height;
					Rectangle rct = Img.Slices[m_ImageIndex];
					switch (m_DragLine)
					{
						case 1:
							int right = rct.Right;
							if (X >= right)
								return;
							rct.X = (int)X;
							rct.Width = (int)(right - X);
							break;
						case 2:
							if (X <= rct.X)
								return;
							rct.Width = (int)(X - rct.X);
							break;
						case 3:
							int bottom = rct.Bottom;
							if (Y >= bottom)
								return;
							rct.Y = (int)Y;
							rct.Height = (int)(bottom - Y);
							break;
						case 4:
							if (Y <= rct.Y)
								return;
							rct.Height = (int)(Y - rct.Y);
							break;
						default:
							Debug.Fail("Unexpected dragline in ctrButtonStyleEdit.pnlPreview_MouseMove");
							break;
					}
					Img.Slices[m_ImageIndex] = rct;
					pnlPreview.Invalidate();
				}
				else
				{
					int X = Math.Min(pnlPreview.Width, Math.Max(e.X, 0));
					int Y = Math.Min(pnlPreview.Height, Math.Max(e.Y, 0));
					m_PreviewSize = new Size(X, Y);
					pnlPreview.Invalidate();
				}
			}
			else if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y))
			{
				// has moved far enough to count as dragging.  This works both for preview and editing modes
				m_Dragging = true;
				pnlPreview.Capture = true;
				m_DragTrigger = Rectangle.Empty;
			}
			else if (m_SliceEdit)
			{
				// set cursor depending on whether we are many of the slice lines
				int line = FindSliceLine(new Point(e.X, e.Y));
				switch (line)
				{
					case 1:
					case 2:
						pnlPreview.Cursor = Cursors.SizeWE;
						break;
					case 3:
					case 4:
						pnlPreview.Cursor = Cursors.SizeNS;
						break;
					default:
						pnlPreview.Cursor = Cursors.Default;
						break;
				}
			}
		}

		public void pnlPreview_MouseUp(object sender, MouseEventArgs e)
		{
			// note this acts on any button
			m_DragTrigger = Rectangle.Empty;
			m_Dragging = false;
			pnlPreview.Capture = false;
		}

		public void pnlPreview_DoubleClick(object sender, EventArgs e)
		{
			m_PreviewSize = pnlPreview.Size;
			pnlPreview.Invalidate();
		}

		private int FindSliceLine(Point pt)
		{
			// Returns 1-4 for the left, right, top, bottom lines.  Returns 0 if none
			Debug.Assert(m_SliceEdit);
			Size sz = Img.Sizes[m_ImageIndex];
			RectangleF sliceRect = Img.Slices[m_ImageIndex];
			PointF fractionalPoint = new PointF((float)pt.X / pnlPreview.Width, (float)pt.Y / pnlPreview.Height);
			if (Math.Abs(fractionalPoint.X - sliceRect.Left / sz.Width) < 0.05)
				return 1;
			if (Math.Abs(fractionalPoint.X - sliceRect.Right / sz.Width) < 0.05)
				return 2;
			if (Math.Abs(fractionalPoint.Y - sliceRect.Top / sz.Height) < 0.05)
				return 3;
			if (Math.Abs(fractionalPoint.Y - sliceRect.Bottom / sz.Height) < 0.05)
				return 4;
			return 0;
		}
		#endregion

		public void lnkCopy9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// updates the other button states, where there are images of exactly the same size
			ButtonShape.States current = State;
			Size sz = Img.Sizes[m_ImageIndex]; // size of the currently selected image
			for (int change = 0; change <= 2; change++)
			{
				if (change != (int)current)
				{
					ScalableImage scalable = m_Style.CustomImage[change];
					for (int index = 0; index <= scalable.Sizes.Count - 1; index++)
					{
						if (scalable.Sizes[index].Equals(sz))
							scalable.Slices[index] = Img.Slices[m_ImageIndex];
					}
				}
			}

		}

		#endregion

	}

}
