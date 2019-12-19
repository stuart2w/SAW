
namespace SAW
{
	public partial class frmReportProblem : System.Windows.Forms.Form
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
			this.txtInfo = new System.Windows.Forms.TextBox();
			base.Load += new System.EventHandler(frmReportProblem_Load);
			this.txtInfo.TextChanged += new System.EventHandler(this.txtInfo_TextChanged);
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnSend = new System.Windows.Forms.Button();
			Label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			//Label1
			//
			Label1.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			Label1.Location = new System.Drawing.Point(8, 8);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(536, 72);
			Label1.TabIndex = 8;
			Label1.Text = "[Error_ProblemLabel]";
			//
			//txtInfo
			//
			this.txtInfo.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtInfo.Location = new System.Drawing.Point(8, 80);
			this.txtInfo.Multiline = true;
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtInfo.Size = new System.Drawing.Size(538, 154);
			this.txtInfo.TabIndex = 5;
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(417, 238);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(128, 30);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//btnSend
			//
			this.btnSend.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSend.Enabled = false;
			this.btnSend.Location = new System.Drawing.Point(247, 238);
			this.btnSend.Margin = new System.Windows.Forms.Padding(4);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(141, 30);
			this.btnSend.TabIndex = 6;
			this.btnSend.Text = "[Error_Send]";
			this.btnSend.UseVisualStyleBackColor = true;
			//
			//frmReportProblem
			//
			this.AcceptButton = this.btnSend;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(557, 278);
			this.ControlBox = false;
			this.Controls.Add(Label1);
			this.Controls.Add(this.txtInfo);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSend);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmReportProblem";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[Error_Title]";
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnSend;
	}
	
}
