using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmFont
	{
		public frmFont()
		{
			InitializeComponent();
		}

		// We keep the list of names for each item listed in the box
		private bool m_Filling ;
		public Font SelectedFont; // cannot change the actual form font, as this updates all of the controls as well !

		#region Font list
		private static readonly List<string> g_Names = new List<string>();
		private static readonly Dictionary<string, int> g_NameLookup = new Dictionary<string, int>(); // Returns the index within g_Names based on the font name
		private static readonly List<FontFamily> g_Families = new List<FontFamily>();

		private static void EnsureFontList()
		{
			if (g_Names.Count > 0)
				return;
			InstalledFontCollection collection = new InstalledFontCollection();
			foreach (FontFamily family in collection.Families)
			{
				// we will only list fonts which support style "Regular", and can successfully be created
				// (.net has trouble with fonts which are not TrueType)
				Font test;
				try
				{
					test = new Font(family.Name, 10, FontStyle.Regular);
				}
				catch (Exception)
				{
					test = null;
				}
				if (test != null)
				{
					g_NameLookup.Add(family.Name, g_Names.Count);
					g_Names.Add(family.Name);
					g_Families.Add(family);
					test.Dispose();
				}
			}
		}

		/// <summary>Returns the index number of the given font face, or -1 if not found</summary>
		public static int IndexOfFontFace(string font)
		{
			EnsureFontList();
			if (g_NameLookup.ContainsKey(font))
				return g_NameLookup[font];
			Debug.Assert(string.IsNullOrEmpty(font), "Font face not in list: " + font);
			return -1;
		}

		/// <summary>Returns the name of the font at the given position in the list, or "" if index is invalid (-1 in particular is intended for unknown/default)</summary>
		public static string FontFaceFromIndex(int index)
		{
			EnsureFontList();
			if (index < 0 || index >= g_Names.Count)
				return "";
			return g_Names[index];
		}

		#endregion

		public void frmFont_Load(object sender, EventArgs e)
		{
			// this assumes the form will only be used once before being disposed
			m_Filling = true;
			try
			{
				if (SelectedFont == null)
					SelectedFont = new Font(FontFamily.GenericSansSerif, 10);
				chkBold.Checked = (SelectedFont.Style & FontStyle.Bold) > 0;
				chkItalic.Checked = (SelectedFont.Style & FontStyle.Italic) > 0;
				chkStrikeout.Checked = (SelectedFont.Style & FontStyle.Strikeout) > 0;
				chkUnderline.Checked = (SelectedFont.Style & FontStyle.Underline) > 0;

				// size box... first fill in the values
				float size = 6f;
				while (size < MAXIMUMFONTSIZE)
				{
					cmbSize.Items.Add(size.ToString("0"));
					size = AdjustSize(size, +1);
				}
				cmbSize.Items.Add(size.ToString("0")); // add the final one, MAXIMUMFONTSIZE
				cmbSize.Text = SelectedFont.Size.ToString("0.#");

				Strings.Translate(this);
				GUIUtilities.ScaleDPI(this);
				int selected = IndexOfFontFace(SelectedFont.Name);
				for (int index = 0; index <= g_Names.Count - 1; index++)
				{
					lstFont.Items.Add(g_Names[index]);
				}
				if (selected >= 0)
					lstFont.SelectedIndex = selected;
			}
			finally
			{
				m_Filling = false;
			}
		}

		public void frmFont_Disposed(object sender, EventArgs e)
		{
			GC.Collect(); // will hopefully dispose all of the fonts we have been leaving lying around
		}

		public void lstFont_DrawItem(object sender, DrawItemEventArgs e)
		{
			string name = g_Names[e.Index];
			if ((e.State & DrawItemState.Selected) > 0)
				e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
			using (Font objFont = new Font(name, 12, FontStyle.Regular))
			{
				e.Graphics.DrawString(name, objFont, Brushes.Black, e.Bounds);
			}

		}

		public void Control_Change(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			SelectedFont = GetFont();
			lblSample.Font = SelectedFont;
		}

		private FontStyle GetFontStyle()
		{
			FontStyle eStyle = FontStyle.Regular;
			if (chkItalic.Checked)
				eStyle |= FontStyle.Italic;
			if (chkBold.Checked)
				eStyle |= FontStyle.Bold;
			if (chkUnderline.Checked)
				eStyle |= FontStyle.Underline;
			if (chkStrikeout.Checked)
				eStyle |= FontStyle.Strikeout;
			return eStyle;
		}

		private float GetFontSize()
		{
			float size;
			if (!float.TryParse(cmbSize.Text, out size))
				return SelectedFont.Size;
			if (size < MINIMUMFONTSIZE || size > MAXIMUMFONTSIZE)
				return SelectedFont.Size;
			return size;
		}

		private Font GetFont()
		{
			if (lstFont.SelectedIndex < 0)
				return new Font(FontFamily.GenericSansSerif, GetFontSize(), GetFontStyle());
			return new Font(g_Families[lstFont.SelectedIndex], GetFontSize(), GetFontStyle());
		}

		public void lstFont_SelectedIndexChanged(object sender, EventArgs e)
		{
			Control_Change(sender, e);
			lstFont.Invalidate(); // cos its not invalidating the previous selection (!?)
		}

		public const float MINIMUMFONTSIZE = 6;
		public const float MAXIMUMFONTSIZE = 96; // care should be taken that this size will be reached by AdjustSize
		public static float AdjustSize(float currentSize, int delta)
		{
			// increments or decrements a font size in meaningful intervals
			delta = Math.Sign(delta);
			if (delta == 0)
				return currentSize;
			float baseSize = currentSize + delta / 100f;
			float step;
			if (baseSize <= 12)
				step = 1;
			else if (baseSize <= 24)
				step = 2;
			else if (baseSize <= 48)
				step = 4;
			else
				step = 8;
			currentSize += delta * step;
			if (currentSize < MINIMUMFONTSIZE)
				currentSize = MINIMUMFONTSIZE;
			if (currentSize > MAXIMUMFONTSIZE)
				currentSize = MAXIMUMFONTSIZE;
			return currentSize;
		}

	}
}
