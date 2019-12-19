namespace SAW
{
	public partial class frmButtonStyleName : System.Windows.Forms.Form
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
			this.Label1 = new System.Windows.Forms.Label();
			base.Load += new System.EventHandler(frmButtonStyleName_Load);
			this.txtName = new System.Windows.Forms.TextBox();
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.FlowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			this.btnCancel = new System.Windows.Forms.Button();
			this.FlowLayoutPanel1.SuspendLayout();
			this.FlowLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(3, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(178, 17);
			this.Label1.TabIndex = 0;
			this.Label1.Text = "[Button_StyleNamePrompt]";
			//
			//txtName
			//
			this.txtName.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtName.Location = new System.Drawing.Point(10, 20);
			this.txtName.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(164, 23);
			this.txtName.TabIndex = 1;
			//
			//FlowLayoutPanel1
			//
			this.FlowLayoutPanel1.AutoSize = true;
			this.FlowLayoutPanel1.Controls.Add(this.Label1);
			this.FlowLayoutPanel1.Controls.Add(this.txtName);
			this.FlowLayoutPanel1.Controls.Add(this.FlowLayoutPanel2);
			this.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.FlowLayoutPanel1.Location = new System.Drawing.Point(8, 8);
			this.FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(8);
			this.FlowLayoutPanel1.MaximumSize = new System.Drawing.Size(400, 1000);
			this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			this.FlowLayoutPanel1.Size = new System.Drawing.Size(248, 104);
			this.FlowLayoutPanel1.TabIndex = 2;
			//
			//FlowLayoutPanel2
			//
			this.FlowLayoutPanel2.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.FlowLayoutPanel2.Controls.Add(this.btnOK);
			this.FlowLayoutPanel2.Controls.Add(this.btnCancel);
			this.FlowLayoutPanel2.Location = new System.Drawing.Point(8, 49);
			this.FlowLayoutPanel2.Name = "FlowLayoutPanel2";
			this.FlowLayoutPanel2.Size = new System.Drawing.Size(173, 39);
			this.FlowLayoutPanel2.TabIndex = 2;
			//
			//btnOK
			//
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(3, 3);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(80, 32);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(89, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 32);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//frmButtonStyleName
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(259, 118);
			this.ControlBox = false;
			this.Controls.Add(this.FlowLayoutPanel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmButtonStyleName";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Button_StyleNameTitle]";
			this.FlowLayoutPanel1.ResumeLayout(false);
			this.FlowLayoutPanel1.PerformLayout();
			this.FlowLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		internal System.Windows.Forms.Label Label1;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel2;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtName;
	}
	
}
