
namespace SAW
{
	public partial class frmMoreColours : KeyForm
	{
		
		//Form overrides dispose to clean up the component list.
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
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.lblTop = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.pnlColours = new ColourPanel();
			this.btnWindows = new System.Windows.Forms.Button();
			this.dlgColour = new System.Windows.Forms.ColorDialog();
			this.SuspendLayout();
			// 
			// lblTop
			// 
			this.lblTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTop.Location = new System.Drawing.Point(8, 8);
			this.lblTop.Name = "lblTop";
			this.lblTop.Size = new System.Drawing.Size(280, 40);
			this.lblTop.TabIndex = 0;
			this.lblTop.Text = "?";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(184, 200);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 32);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOK.Location = new System.Drawing.Point(88, 200);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(91, 32);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Visible = false;
			// 
			// pnlColours
			// 
			this.pnlColours.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlColours.DisplayAdvanced = false;
			this.pnlColours.Location = new System.Drawing.Point(8, 36);
			this.pnlColours.Name = "pnlColours";
			this.pnlColours.Size = new System.Drawing.Size(280, 156);
			this.pnlColours.TabIndex = 1;
			this.pnlColours.Text = "ColourPanel1";
			this.pnlColours.UseSettings = true;
			this.pnlColours.VariableValue = 100;
			this.pnlColours.UserSelectedColour += new NullEventHandler(this.pnlColours_UserSelectedColour);
			// 
			// btnWindows
			// 
			this.btnWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnWindows.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnWindows.Location = new System.Drawing.Point(8, 200);
			this.btnWindows.Name = "btnWindows";
			this.btnWindows.Size = new System.Drawing.Size(48, 32);
			this.btnWindows.TabIndex = 4;
			this.btnWindows.Text = "...";
			this.btnWindows.UseVisualStyleBackColor = true;
			this.btnWindows.Visible = false;
			this.btnWindows.Click += new System.EventHandler(this.btnWindows_Click);
			// 
			// dlgColour
			// 
			this.dlgColour.FullOpen = true;
			// 
			// frmMoreColours
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(302, 240);
			this.Controls.Add(this.btnWindows);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.pnlColours);
			this.Controls.Add(this.lblTop);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMoreColours";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Colours_Title]";
			this.Load += new System.EventHandler(this.frmMoreColours_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMoreColours_KeyDown);
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblTop;
		private ColourPanel pnlColours;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnWindows;
		private System.Windows.Forms.ColorDialog dlgColour;
	}
	
}
