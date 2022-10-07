
namespace SAW
{
	internal partial class frmUserButtonStyle : System.Windows.Forms.Form
	{
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.btnOK = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmUserButtonStyle_Load);
			this.btnCancel = new System.Windows.Forms.Button();
			this.ctrStyle = new SAW.ctrButtonStyleEdit();
			this.lblHeader = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			//btnOK
			//
			this.btnOK.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(349, 492);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(80, 32);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(435, 492);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 32);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//ctrStyle
			//
			this.ctrStyle.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.ctrStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.ctrStyle.Location = new System.Drawing.Point(8, 40);
			this.ctrStyle.Name = "ctrStyle";
			this.ctrStyle.Size = new System.Drawing.Size(504, 444);
			this.ctrStyle.TabIndex = 4;
			//
			//lblHeader
			//
			this.lblHeader.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.lblHeader.Location = new System.Drawing.Point(8, 0);
			this.lblHeader.Name = "lblHeader";
			this.lblHeader.Size = new System.Drawing.Size(512, 32);
			this.lblHeader.TabIndex = 5;
			this.lblHeader.Text = "Label1";
			//
			//frmUserButtonStyle
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(523, 536);
			this.Controls.Add(this.lblHeader);
			this.Controls.Add(this.ctrStyle);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmUserButtonStyle";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Button_UserStyle_Title]";
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private SAW.ctrButtonStyleEdit ctrStyle;
		private System.Windows.Forms.Label lblHeader;
	}
	
}
