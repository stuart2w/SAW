
namespace SAW
{
	public partial class frmErrorReport : System.Windows.Forms.Form
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmErrorReport));
			this.Label1 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.Label3 = new System.Windows.Forms.Label();
			this.btnSend = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.Label4 = new System.Windows.Forms.Label();
			this.txtInfo = new System.Windows.Forms.TextBox();
			this.btnQuit = new System.Windows.Forms.Button();
			this.lblErrorMessage = new System.Windows.Forms.Label();
			this.lblUpgrade = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Label1
			// 
			this.Label1.Image = ((System.Drawing.Image)(resources.GetObject("Label1.Image")));
			this.Label1.Location = new System.Drawing.Point(16, 16);
			this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(64, 52);
			this.Label1.TabIndex = 0;
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label2.Location = new System.Drawing.Point(88, 8);
			this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(130, 24);
			this.Label2.TabIndex = 1;
			this.Label2.Text = "[Error_Oops]";
			// 
			// Label3
			// 
			this.Label3.Location = new System.Drawing.Point(88, 40);
			this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(528, 56);
			this.Label3.TabIndex = 2;
			this.Label3.Text = "[Error_Label]";
			// 
			// btnSend
			// 
			this.btnSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSend.Location = new System.Drawing.Point(318, 352);
			this.btnSend.Margin = new System.Windows.Forms.Padding(4);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(141, 30);
			this.btnSend.TabIndex = 3;
			this.btnSend.Text = "[Error_Send]";
			this.btnSend.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(488, 352);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(128, 30);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// Label4
			// 
			this.Label4.Location = new System.Drawing.Point(16, 128);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(608, 78);
			this.Label4.TabIndex = 0;
			this.Label4.Text = "[Error_Question]";
			// 
			// txtInfo
			// 
			this.txtInfo.AcceptsReturn = true;
			this.txtInfo.AcceptsTab = true;
			this.txtInfo.Location = new System.Drawing.Point(16, 200);
			this.txtInfo.Multiline = true;
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtInfo.Size = new System.Drawing.Size(600, 136);
			this.txtInfo.TabIndex = 1;
			// 
			// btnQuit
			// 
			this.btnQuit.Location = new System.Drawing.Point(16, 352);
			this.btnQuit.Name = "btnQuit";
			this.btnQuit.Size = new System.Drawing.Size(192, 32);
			this.btnQuit.TabIndex = 2;
			this.btnQuit.Text = "[Error_Quit]";
			this.btnQuit.UseVisualStyleBackColor = true;
			this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
			// 
			// lblErrorMessage
			// 
			this.lblErrorMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblErrorMessage.Location = new System.Drawing.Point(16, 104);
			this.lblErrorMessage.Name = "lblErrorMessage";
			this.lblErrorMessage.Size = new System.Drawing.Size(607, 24);
			this.lblErrorMessage.TabIndex = 5;
			this.lblErrorMessage.Text = "[Error_Message]";
			// 
			// lblUpgrade
			// 
			this.lblUpgrade.Location = new System.Drawing.Point(16, 391);
			this.lblUpgrade.Name = "lblUpgrade";
			this.lblUpgrade.Size = new System.Drawing.Size(600, 79);
			this.lblUpgrade.TabIndex = 6;
			this.lblUpgrade.Text = "[Error_Upgrade]";
			this.lblUpgrade.Visible = false;
			// 
			// frmErrorReport
			// 
			this.AcceptButton = this.btnSend;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(635, 472);
			this.Controls.Add(this.lblUpgrade);
			this.Controls.Add(this.lblErrorMessage);
			this.Controls.Add(this.btnQuit);
			this.Controls.Add(this.txtInfo);
			this.Controls.Add(this.Label4);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmErrorReport";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Error_Title]";
			this.Load += new System.EventHandler(this.frmErrorReport_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.Label Label3;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label Label4;
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.Button btnQuit;
		private System.Windows.Forms.Label lblErrorMessage;
		private System.Windows.Forms.Label lblUpgrade;
	}
	
}
