namespace SAW
{
	internal partial class frmChooseImportPage : System.Windows.Forms.Form
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
			this.btnCancel = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmChooseImportPage_Load);
			this.Disposed += new System.EventHandler(frmChooseImportPage_Disposed);
			this.btnOK = new System.Windows.Forms.Button();
			this.Label1 = new System.Windows.Forms.Label();
			this.pnlPreview = new System.Windows.Forms.Panel();
			this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
			this.lblPage = new System.Windows.Forms.Label();
			this.btnLeft = new System.Windows.Forms.Button();
			this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
			this.btnRight = new System.Windows.Forms.Button();
			this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
			this.SuspendLayout();
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.Location = new System.Drawing.Point(360, 384);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(118, 39);
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//btnOK
			//
			this.btnOK.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(240, 384);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(115, 39);
			this.btnOK.TabIndex = 10;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//Label1
			//
			this.Label1.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.Label1.Location = new System.Drawing.Point(8, 8);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(478, 40);
			this.Label1.TabIndex = 12;
			this.Label1.Text = "[ChoosePage_Prompt]";
			//
			//pnlPreview
			//
			this.pnlPreview.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.pnlPreview.Location = new System.Drawing.Point(56, 56);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlPreview.Size = new System.Drawing.Size(366, 320);
			this.pnlPreview.TabIndex = 13;
			//
			//lblPage
			//
			this.lblPage.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.lblPage.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.lblPage.Location = new System.Drawing.Point(8, 392);
			this.lblPage.Name = "lblPage";
			this.lblPage.Size = new System.Drawing.Size(224, 24);
			this.lblPage.TabIndex = 14;
			this.lblPage.Text = "Page 1 of 3";
			this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			//btnLeft
			//
			this.btnLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnLeft.Image = Resources.AM.Previous_32_64;
			this.btnLeft.Location = new System.Drawing.Point(8, 177);
			this.btnLeft.Name = "btnLeft";
			this.btnLeft.Size = new System.Drawing.Size(40, 72);
			this.btnLeft.TabIndex = 15;
			this.btnLeft.UseVisualStyleBackColor = true;
			//
			//btnRight
			//
			this.btnRight.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnRight.Image = Resources.AM.Next_32_64;
			this.btnRight.Location = new System.Drawing.Point(430, 177);
			this.btnRight.Name = "btnRight";
			this.btnRight.Size = new System.Drawing.Size(48, 72);
			this.btnRight.TabIndex = 16;
			this.btnRight.UseVisualStyleBackColor = true;
			//
			//frmChooseImportPage
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(489, 435);
			this.ControlBox = false;
			this.Controls.Add(this.btnRight);
			this.Controls.Add(this.btnLeft);
			this.Controls.Add(this.lblPage);
			this.Controls.Add(this.pnlPreview);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 300);
			this.Name = "frmChooseImportPage";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[ChoosePage_Title]";
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.Panel pnlPreview;
		private System.Windows.Forms.Label lblPage;
		private System.Windows.Forms.Button btnLeft;
		private System.Windows.Forms.Button btnRight;
	}
	
}
