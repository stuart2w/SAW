namespace SAW
{
	internal partial class ColourPicker : System.Windows.Forms.Control
	{
		
		//Control overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Control Designer
		private System.ComponentModel.Container components = null;
		
		// NOTE: The following procedure is required by the Component Designer
		// It can be modified using the Component Designer.  Do not modify it
		// using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.Click += new System.EventHandler(ColourPicker_Click);
			this.GotFocus += new System.EventHandler(ColourPicker_GotFocus);
			this.LostFocus += new System.EventHandler(ColourPicker_GotFocus);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(ColourPicker_KeyDown);
		}
		
	}
	
	
}
