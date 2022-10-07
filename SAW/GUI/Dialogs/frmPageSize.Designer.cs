
namespace SAW
{
	internal partial class frmPageSize : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label2;
			System.Windows.Forms.Label Label3;
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.nudWidth = new System.Windows.Forms.NumericUpDown();
			this.nudHeight = new System.Windows.Forms.NumericUpDown();
			this.lblInsufficient = new System.Windows.Forms.Label();
			this.pnlPreview = new System.Windows.Forms.Panel();
			this.chkEqual = new System.Windows.Forms.CheckBox();
			Label2 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.nudWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
			this.SuspendLayout();
			// 
			// Label2
			// 
			Label2.Location = new System.Drawing.Point(312, 48);
			Label2.Name = "Label2";
			Label2.Size = new System.Drawing.Size(152, 32);
			Label2.TabIndex = 7;
			Label2.Text = "[PageSize_Width]";
			Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label3
			// 
			Label3.Location = new System.Drawing.Point(312, 80);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(152, 32);
			Label3.TabIndex = 9;
			Label3.Text = "[PageSize_Height]";
			Label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(456, 280);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(5);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(152, 32);
			this.btnCancel.TabIndex = 12;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(296, 280);
			this.btnOK.Margin = new System.Windows.Forms.Padding(5);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(144, 32);
			this.btnOK.TabIndex = 11;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// nudWidth
			// 
			this.nudWidth.Location = new System.Drawing.Point(472, 48);
			this.nudWidth.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.nudWidth.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudWidth.Name = "nudWidth";
			this.nudWidth.Size = new System.Drawing.Size(80, 23);
			this.nudWidth.TabIndex = 8;
			this.nudWidth.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudWidth.ValueChanged += new System.EventHandler(this.nudWidth_ValueChanged);
			// 
			// nudHeight
			// 
			this.nudHeight.Location = new System.Drawing.Point(472, 80);
			this.nudHeight.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.nudHeight.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudHeight.Name = "nudHeight";
			this.nudHeight.Size = new System.Drawing.Size(80, 23);
			this.nudHeight.TabIndex = 10;
			this.nudHeight.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudHeight.ValueChanged += new System.EventHandler(this.nudHeight_ValueChanged);
			// 
			// lblInsufficient
			// 
			this.lblInsufficient.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblInsufficient.ForeColor = System.Drawing.Color.Red;
			this.lblInsufficient.Location = new System.Drawing.Point(8, 208);
			this.lblInsufficient.Name = "lblInsufficient";
			this.lblInsufficient.Size = new System.Drawing.Size(600, 64);
			this.lblInsufficient.TabIndex = 21;
			this.lblInsufficient.Text = "[PageSize_Insufficient]";
			this.lblInsufficient.Visible = false;
			// 
			// pnlPreview
			// 
			this.pnlPreview.BackColor = System.Drawing.Color.LightGray;
			this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlPreview.Location = new System.Drawing.Point(608, 16);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlPreview.Size = new System.Drawing.Size(272, 256);
			this.pnlPreview.TabIndex = 22;
			this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
			// 
			// chkEqual
			// 
			this.chkEqual.Checked = true;
			this.chkEqual.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEqual.Location = new System.Drawing.Point(16, 0);
			this.chkEqual.Name = "chkEqual";
			this.chkEqual.Size = new System.Drawing.Size(576, 40);
			this.chkEqual.TabIndex = 0;
			this.chkEqual.Text = "[PageSize_Equal]";
			this.chkEqual.UseVisualStyleBackColor = true;
			// 
			// frmPageSize
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(886, 324);
			this.Controls.Add(this.chkEqual);
			this.Controls.Add(this.pnlPreview);
			this.Controls.Add(this.lblInsufficient);
			this.Controls.Add(this.nudHeight);
			this.Controls.Add(this.nudWidth);
			this.Controls.Add(Label3);
			this.Controls.Add(Label2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPageSize";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[PageSize_Title]";
			this.Load += new System.EventHandler(this.frmPageSize_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.NumericUpDown nudWidth;
		private System.Windows.Forms.NumericUpDown nudHeight;
		private System.Windows.Forms.Label lblInsufficient;
		private System.Windows.Forms.Panel pnlPreview;
		private System.Windows.Forms.CheckBox chkEqual;
	}
	
}
