using System;
using System.Drawing;
using SAW.Functions;


namespace SAW
{
	internal class CustomButton : SharedButton
	{
		// Not using button to stop it auto-focusing the form (?)
		private SAW.Functions.Codes m_eVerb = Codes.None;

		public CustomButton()
		{
			this.MouseDown += CustomButton_MouseDown;
			this.MouseMove += CustomButton_MouseMove;
			this.MouseUp += CustomButton_MouseUp;
			this.Click += CustomButton_Click;
		}

		[System.ComponentModel.DefaultValue(Codes.None)]
		public Codes Code
		{
			get { return m_eVerb; }
			set
			{
				if (value == m_eVerb)
					return;
				m_eVerb = value;
				if (m_eVerb == Codes.None)
					Tag = "";
				else
					Tag = "Verb/" + m_eVerb;
			}
		}

		#region Mouse

		private void CustomButton_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!Enabled || !m_Applicable)
				return;
			SetSelectionGUI(true);
			Capture = true;
		}

		private void CustomButton_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// MouseEnter and MouseLeave are not sent while capture is active, so we need to do a hit test
			if (!Capture)
				return;
			bool within = e.X >= 0 && e.Y >= 0 && e.X < Width && e.Y < Height;
			if (within != m_Selected)
				SetSelectionGUI(within);
		}

		private void CustomButton_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetSelectionGUI(false);
			Capture = false;
		}

		private void CustomButton_Click(object sender, EventArgs e)
		{
			// is also called indirectly if the button is triggered by other means
			GUIUtilities.FocusIfNotInPalette(this.Parent);
		}

		public override void Trigger()
		{
			CustomButton_Click(this, System.EventArgs.Empty);
		}

		#endregion

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			DrawInternalBackground(e, true);

			Image image = null;
			if (!string.IsNullOrEmpty(m_ImageName ))
				image = (Bitmap)GUIUtilities.RM.GetObject(m_ImageName);
			else if (m_eVerb != Codes.None)
				image = GUIUtilities.VariableSizeImage("Verb", "_" + m_eVerb, this.Width - EXCESSSIZE);
			if (m_Image != null)
				image = m_Image;

			if (image != null)
			{
				DrawStandardImage(e.Graphics, image);
				if (image != m_Image)
					image.Dispose();
			}

			DrawHighlightAndKey(e);
		}

		protected override string BackgroundImagePrefix => "CustomButton";

		protected override void SetSelectionGUI(bool selected)
		{
			// this version does not animate, because we are responding to the mouse held down only.  It is not a state change
			if (selected == m_Selected)
				return;
			m_Selected = selected;
			m_SelectionDisplayFraction = m_Selected ? 255 : 0;
			Invalidate();
		}

	}

}
