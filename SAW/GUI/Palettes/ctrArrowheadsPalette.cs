
namespace SAW
{
	public partial class ctrArrowheadsPalette : PalettePanel, IKeyControl
	{
		public ctrArrowheadsPalette()
		{
			InitializeComponent();
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			switch (e.KeyCode)
			{
				case System.Windows.Forms.Keys.S: //Or Keys.Alt
					pnlArrowheads.SelectedIndex = 0;
					break;
				case System.Windows.Forms.Keys.E: //Or Keys.Alt
					pnlArrowheads.SelectedIndex = 1;
					break;
			}
		}
		
		public void CombinedKeyUp(CombinedKeyEvent e)
		{ }
	}
	
}
