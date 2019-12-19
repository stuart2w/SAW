using System.Windows.Forms;

namespace SAW
{
	#region Modified text box to intercept all keys
	public class TextBoxAllKeys : TextBox
	{
		protected override bool IsInputChar(char charCode)
		{
			return true;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			return true;
		}
	}
	#endregion

}
