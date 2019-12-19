using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	//also radio available: http://blogs.msdn.com/b/jfoscoding/archive/2005/11/14/492559.aspx
	internal sealed class WrappingCheckBox : CheckBox
	{

		Size cachedSizeOfOneLineOfText = Size.Empty;
		Dictionary<Size, Size> preferredSizeHash = new Dictionary<Size, Size>(3); // testing this out - typically we've got three different constraints.

		public WrappingCheckBox()
		{
			AutoSize = true;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			CacheTextSize();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			CacheTextSize();
		}

		private void CacheTextSize()
		{
			//When the text has changed, the preferredSizeHash is invalid...
			preferredSizeHash.Clear();
			if (string.IsNullOrEmpty(Text)) // || Font == null)
				cachedSizeOfOneLineOfText = Size.Empty;
			else
				cachedSizeOfOneLineOfText = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
		}

		public override Size GetPreferredSize(Size proposedsize)
		{
			Size prefSize = base.GetPreferredSize(proposedsize);
			if (proposedsize.Width > 1 && prefSize.Width > proposedsize.Width && !string.IsNullOrEmpty(this.Text) && (proposedsize.Width < int.MaxValue || !(proposedsize.Height < int.MaxValue)))
			{
				// we have the possiblility of wrapping... back out the single line of text
				Size bordersAndPadding = prefSize - cachedSizeOfOneLineOfText;
				// add back in the text size, subtract baseprefsize.width and 3 from proposed size width so they wrap properly
				Size newConstraints = proposedsize - bordersAndPadding - new Size(3, 0);
				if (newConstraints.Width < 0)
					newConstraints.Width = 0;
				if (newConstraints.Height < 0)
					newConstraints.Height = 0;

				if (!preferredSizeHash.ContainsKey(newConstraints))
				{
					prefSize = bordersAndPadding + TextRenderer.MeasureText(Text, Font, newConstraints, TextFormatFlags.WordBreak);
					preferredSizeHash[newConstraints] = prefSize;
				}
				else
					prefSize = preferredSizeHash[newConstraints];
			}
			return prefSize;
		}
	}
}