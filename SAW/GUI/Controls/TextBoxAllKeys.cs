using System.Windows.Forms;

namespace SAW
{
	/// <summary>Modified text box to intercept all keys so that they are reported to the normal key events on the control </summary>
	public class TextBoxAllKeys : TextBox, IKeyControl
	{
		protected override bool IsInputChar(char charCode) => true;

		protected override bool IsInputKey(Keys keyData) => true;

		// also prevents Splash's own handlers from interfering
		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			e.DoNotHandle = true;
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{
			e.DoNotHandle = true;
		}
	}


}
