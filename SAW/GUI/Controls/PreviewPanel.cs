using System.Drawing;
using System.Windows.Forms;


namespace SAW
{
	public class PreviewPanel : Panel
	{
		// Mainly used to display file previews, but can be used to display any image, especially if NoImageString is cleared
		
		private Image m_img;
		
		[System.ComponentModel.DefaultValue("[Preview_Unavailable]")]public string NoImageString = "[Preview_Unavailable]";

		public PreviewPanel()
		{
			this.Paint += pnlPreview_Paint;
		}

		public Image Image
		{
			get{return m_img;}
			set
			{
				m_img = value;
				Invalidate();
			}
		}
		
		private void pnlPreview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Rectangle rctPreview = new Rectangle(0, 0, Width, Height);
			if (this.DesignMode)
				e.Graphics.DrawString("Preview goes here", Font, Brushes.DarkGray, rctPreview, GUIUtilities.StringFormatCentreCentre);
			else
			{
				if (m_img == null)
					e.Graphics.DrawString(Strings.Translate(NoImageString), Font, Brushes.DarkGray, rctPreview, GUIUtilities.StringFormatCentreCentre);
				else
				{
					GUIUtilities.CalcDestRect(m_img.Width, m_img.Height, ref rctPreview);
					e.Graphics.DrawImage(m_img, rctPreview);
				}
			}
		}
		
		
	}
	
}
