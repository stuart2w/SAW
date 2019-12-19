using System.Collections.Generic;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace SAW
{
	public partial class frmMoreColours : KeyForm
	{
		public frmMoreColours()
		{
			InitializeComponent();
		}
		// This displays 2 slightly different versions.  One offers a range of colours similar to the base colour
		// the other offers transparent colours.  The actual colours always provided, but the different versions have different text etc

		// usually the OK button is not displayed and the form closes automatically when a selection is made
		// unless it was opened with DisplayPickColour, in which case it waits for OK (because otherwise a subform selection closes the initial form which doesn't work so well)

		private Color m_Result = Color.Empty;
		private Color m_Basis; // The original colour from which we are editing

		public static Color Display(Color basis, List<Color> colours, bool transparent, Form parent)
		{
			// displays the form to refine a colour within ColoursPanel
			// returns Empty if cancelled
			// blackWhite forces the popup empty to display is black and white box
			frmMoreColours frmNew = new frmMoreColours();
			frmNew.Owner = parent;
			frmNew.m_Basis = basis;
			if (transparent)
			{
				frmNew.lblTop.Text = Strings.Item("Colours_Transparent");
				frmNew.pnlColours.DisplayColours(colours, 8);
				frmNew.pnlColours.BackgroundHatching = true;
			}
			else
			{
				frmNew.lblTop.Text = Strings.Item("Colours_More");
				frmNew.pnlColours.DisplayColours(colours, 9);
			}
			if (frmNew.ShowDialog() != DialogResult.OK)
				return Color.Empty;
			return frmNew.m_Result;
		}

		/// <summary>Displays the form to pick a colour from scratch.  Returns current if cancelled</summary>
		public static Color DisplayPickColour(Form parent, Color current, bool withEmpty = false, bool blackWhite = false,
			bool transparent = true, bool useSettings = true, List<Color> colours = null)
		{
			frmMoreColours frmNew = new frmMoreColours();
			frmNew.Owner = parent;
			frmNew.lblTop.Text = Strings.Item("Colours_Title");
			if (colours == null) 
				colours = SAW.SAW6.StandardColours.ToList();
			if (withEmpty)
				colours.Add(Color.Empty);
			frmNew.pnlColours.DisplayColours(colours, 6);
			frmNew.pnlColours.CurrentColour = current;
			frmNew.pnlColours.DisplayAdvanced = true; // allowing it to open another form to refine
			frmNew.pnlColours.BlackWhite = blackWhite;
			frmNew.pnlColours.AllowTransparent = transparent;
			frmNew.pnlColours.UseSettings = useSettings;
			frmNew.pnlColours.Initialise();
			frmNew.m_Result = current;
			if (frmNew.ShowDialog() != DialogResult.OK)
				return current;
			return frmNew.m_Result;
		}

		public void frmMoreColours_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Space:
					if (pnlColours.SelectionIndex >= 0)
						this.DialogResult = DialogResult.OK; // this also does OK, if something in display is selected
					break;
			}
		}

		public void frmMoreColours_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
			btnOK.Visible = pnlColours.DisplayAdvanced ;
			btnWindows.Visible = Globals.Root.User == Users.Editor;
		}

		public void pnlColours_UserSelectedColour()
		{
			m_Result = pnlColours.CurrentColour;
			if (pnlColours.DisplayAdvanced )
				return; // this version will wait for the OK button
			this.DialogResult = DialogResult.OK;
		}

		public void btnWindows_Click(object sender, EventArgs e)
		{
			dlgColour.AnyColor = true;
			dlgColour.Color = m_Basis;
			if (dlgColour.ShowDialog() == DialogResult.OK)
			{
				m_Result = dlgColour.Color;
				DialogResult = DialogResult.OK;
			}
		}

	}
}
