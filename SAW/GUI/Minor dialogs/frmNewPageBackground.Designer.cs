
namespace SAW
{
	internal partial class frmNewPageBackground : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label1;
			this.btnCancel = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmNewPageBackground_Load);
			this.btnOK = new System.Windows.Forms.Button();
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			this.rdoSame = new System.Windows.Forms.RadioButton();
			this.rdoSame.CheckedChanged += new System.EventHandler(this.rdoSame_CheckedChanged);
			this.rdoBlank = new System.Windows.Forms.RadioButton();
			this.rdoBlank.CheckedChanged += new System.EventHandler(this.rdoSame_CheckedChanged);
			this.rdoOther = new System.Windows.Forms.RadioButton();
			this.rdoOther.CheckedChanged += new System.EventHandler(this.rdoSame_CheckedChanged);
			this.pnlPreview = new System.Windows.Forms.Panel();
			this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
			Label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			//Label1
			//
			Label1.Location = new System.Drawing.Point(8, 0);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(472, 48);
			Label1.TabIndex = 13;
			Label1.Text = "[NewPage_Background_Prompt]";
			//
			//btnCancel
			//
			this.btnCancel.Location = new System.Drawing.Point(352, 192);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(128, 39);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//btnOK
			//
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(213, 192);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(128, 39);
			this.btnOK.TabIndex = 8;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//rdoSame
			//
			this.rdoSame.Location = new System.Drawing.Point(112, 40);
			this.rdoSame.Name = "rdoSame";
			this.rdoSame.Size = new System.Drawing.Size(368, 40);
			this.rdoSame.TabIndex = 10;
			this.rdoSame.TabStop = true;
			this.rdoSame.Text = "[NewPage_Background_Same]";
			this.rdoSame.UseVisualStyleBackColor = true;
			//
			//rdoBlank
			//
			this.rdoBlank.Location = new System.Drawing.Point(112, 80);
			this.rdoBlank.Name = "rdoBlank";
			this.rdoBlank.Size = new System.Drawing.Size(368, 40);
			this.rdoBlank.TabIndex = 11;
			this.rdoBlank.TabStop = true;
			this.rdoBlank.Text = "[NewPage_Background_Blank]";
			this.rdoBlank.UseVisualStyleBackColor = true;
			//
			//rdoOther
			//
			this.rdoOther.Location = new System.Drawing.Point(112, 120);
			this.rdoOther.Name = "rdoOther";
			this.rdoOther.Size = new System.Drawing.Size(368, 64);
			this.rdoOther.TabIndex = 12;
			this.rdoOther.TabStop = true;
			this.rdoOther.Text = "[NewPage_Background_Other]";
			this.rdoOther.UseVisualStyleBackColor = true;
			//
			//pnlPreview
			//
			this.pnlPreview.Location = new System.Drawing.Point(8, 56);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlPreview.Size = new System.Drawing.Size(96, 120);
			this.pnlPreview.TabIndex = 14;
			//
			//frmNewPageBackground
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(487, 237);
			this.ControlBox = false;
			this.Controls.Add(this.pnlPreview);
			this.Controls.Add(Label1);
			this.Controls.Add(this.rdoOther);
			this.Controls.Add(this.rdoBlank);
			this.Controls.Add(this.rdoSame);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmNewPageBackground";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[NewPage_Background_Title]";
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.RadioButton rdoSame;
		private System.Windows.Forms.RadioButton rdoBlank;
		private System.Windows.Forms.RadioButton rdoOther;
		private System.Windows.Forms.Panel pnlPreview;
	}
	
}
