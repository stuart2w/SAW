using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;
using SAW.Functions;

namespace SAW
{
	public partial class ctrTextStyle : IPalette
	{

		// Unfortunately the .net font chooser dialogue does not work because it allows selection of non-true type fonts
		// and crashes as soon as the user selects one of these (the exception is thrown during the call to ShowDialog)
		// therefore we will need a custom dialog box

		public ctrTextStyle()
		{
			InitializeComponent();
		}

		public void ctrTextStyle_Load(object sender, EventArgs e)
		{
			Strings.Translate(btnChoose);
			Globals.ApplicabilityChanged += ApplicabilityChanged;
		}

		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					components?.Dispose();
				Globals.ApplicabilityChanged -= ApplicabilityChanged;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public void Initialise()
		{
			AddButton("TextLeft", Parameters.TextAlignment, StringAlignment.Near);
			AddButton("TextCentre", Parameters.TextAlignment, StringAlignment.Center);
			AddButton("TextRight", Parameters.TextAlignment, StringAlignment.Far);
			AddButton("TextTop", Parameters.TextVerticalAlignment, StringAlignment.Near);
			AddButton("TextMiddle", Parameters.TextVerticalAlignment, StringAlignment.Center);
			AddButton("TextBottom", Parameters.TextVerticalAlignment, StringAlignment.Far);
			ctrColour.AttachParameter(Parameters.TextColour, Globals.ParameterValue(Parameters.TextColour));
		}

		private void AddButton(string image, Parameters parameter, int value)
		{
			RoundButton button = pnlAlignment.AddButton();
			button.ImageName = image;
			button.AttachParameter(parameter, value);
		}

		private void AddButton(string image, Parameters parameter, StringAlignment sa)
		{
			AddButton(image, parameter, (int)sa);
		}

		public void ctrTextStyle_Resize(object sender, EventArgs e)
		{
			if (Width > 50)
				pnlAlignment.ButtonSize = Math.Min(pnlAlignment.Height, pnlAlignment.Width / 6);
		}

		public void btnChoose_Click(object sender, EventArgs e)
		{
			List<Shape> shapes = Shape.FlattenList(Globals.SelectedOrCurrentShapes(), Shape.FlatListPurpose.Style);
			// this reads the font out of the first suitable object it can find
			Font font = null;
			foreach (Shape shape in shapes)
			{
				if (shape.StyleObjectForParameter(Parameters.FontSize) is Shape.TextStyleC style)
				{
					// this shape supports font style
					font = style.CreateFont(style.Size);
					break;
				}
			}
			using (frmFont frm = new frmFont())
			{
				if (font != null)
					frm.SelectedFont = font;
				if (frm.ShowDialog() != DialogResult.OK)
				{
					Globals.RestoreFocus();
					return;
				}
				font = frm.SelectedFont;
			}
			// Now apply this fonts to all applicable selected objects
			Transaction transaction = new Transaction();
			foreach (Shape shape in shapes)
			{
				if (shape.StyleObjectForParameter(Parameters.FontSize) is Shape.TextStyleC style)
				{
					transaction.Edit(shape);
					int oldSize = style.ParameterValue(Parameters.FontSize);
					style.ApplyFont(font);
					shape.NotifyStyleChanged(Parameters.FontSize, oldSize, (int)(font.Size * 100));
					// this allows the shape to update its bounding box. Parameter FontSize just lets it know that the font has changed
				}
			}
			Globals.SetFontAsDefault(font);
			Globals.Root.StoreNewTransaction(transaction, true);
			//m_ctrGUI.NotifyExternalChangesToSelected(objTransaction)
			Globals.RestoreFocus();
		}

		private void ApplicabilityChanged()
		{
			EnableButtons();
		}

		public void EnableButtons()
		{
			bool applicable = Globals.ParameterApplicable(Parameters.FontSize);
			float size = (float)Globals.ParameterValue(Parameters.FontSize) / 100;
			btnChoose.Enabled = applicable;
			btnSmaller.Enabled = applicable && size > frmFont.MINIMUMFONTSIZE;
			btnLarger.Enabled = applicable && size < frmFont.MAXIMUMFONTSIZE;
		}

		public void btnSmaller_Click(object sender, EventArgs e)
		{
			Globals.Root.PerformAction(Verb.Find(Codes.TextSmaller));
			RestoreFocus();
		}

		public void btnLarger_Click(object sender, EventArgs e)
		{
			Globals.Root.PerformAction(Verb.Find(Codes.TextLarger));
			RestoreFocus();
		}

		private void RestoreFocus()
		{
			// if the user clicked with the mouse we want to try and put the focus back in the main drawing area
			// there is no way in the actual Click event to tell whether the user clicked with the mouse or keyboard
			// however we can assume that if the mouse is over this control, then they probably clicked using the mouse
			Point mouse = this.PointToClient(Cursor.Position);
			if (!new Rectangle(0, 0, Width, Height).Contains(mouse))
				return;
			Globals.RestoreFocus();
		}

		protected override bool ProcessTabKey(bool forward)
		{
			GUIUtilities.IterateTabStop(this, forward);
			return true; // says that we have processed the key.  We will do any normal moving between controls type navigation
		}

		#region Palette stuff
		//public event EventHandler Accessed;
		public event NullEventHandler FocusChanged;
		public void SubscribeEvents(EventHandler AccessedHandler, NullEventHandler FocusChangedHandler)
		{
			//if (AccessedHandler != null)
			//	Accessed += AccessedHandler;
			if (FocusChangedHandler != null)
				FocusChanged += FocusChangedHandler;
		}

		void IPalette.Initialise()
		{
			btnChoose.GotFocus += OnFocusChanged;
		}

		private void OnFocusChanged(object sender, EventArgs e)
		{
			FocusChanged?.Invoke();
		}

		public void SetToolTips(ToolTip tt)
		{
			// none in this control
		}

		#region Sizing
		private static Size StandardSize = new Size(200, 69);

		public Size SizeFromSize(Size requested)
		{
			return StandardSize.ToSizeF().ChangeLength(requested.Length()).ToSize();
		}

		public Size SizeFromWidth(int requested)
		{
			int height = requested * StandardSize.Height / StandardSize.Width;
			return new Size(requested, height);
		}

		#endregion

		public void BringPointerInsideOnActivation()
		{ }
		#endregion

	}

}
