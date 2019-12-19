
namespace SAW
{
	public partial class frmUpdate : System.Windows.Forms.Form
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
			this.components = new System.ComponentModel.Container();
			base.Load += new System.EventHandler(frmUpdate_Load);
			this.btnClose = new System.Windows.Forms.Button();
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			this.Label1 = new System.Windows.Forms.Label();
			this.txtCurrent = new System.Windows.Forms.TextBox();
			this.txtLatest = new System.Windows.Forms.TextBox();
			this.Label2 = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.tmrStart = new System.Windows.Forms.Timer(this.components);
			this.tmrStart.Tick += new System.EventHandler(this.tmrStart_Tick);
			this.btnDownload = new System.Windows.Forms.Button();
			this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
			this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
			this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
			this.proDownload = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			//
			//btnClose
			//
			this.btnClose.Location = new System.Drawing.Point(416, 184);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(96, 32);
			this.btnClose.TabIndex = 0;
			this.btnClose.Text = "[Close]";
			this.btnClose.UseVisualStyleBackColor = true;
			//
			//Label1
			//
			this.Label1.Location = new System.Drawing.Point(88, 8);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(240, 24);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "[Update_Current]";
			this.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//txtCurrent
			//
			this.txtCurrent.Location = new System.Drawing.Point(336, 8);
			this.txtCurrent.Name = "txtCurrent";
			this.txtCurrent.ReadOnly = true;
			this.txtCurrent.Size = new System.Drawing.Size(128, 23);
			this.txtCurrent.TabIndex = 3;
			//
			//txtLatest
			//
			this.txtLatest.Location = new System.Drawing.Point(336, 40);
			this.txtLatest.Name = "txtLatest";
			this.txtLatest.ReadOnly = true;
			this.txtLatest.Size = new System.Drawing.Size(128, 23);
			this.txtLatest.TabIndex = 5;
			this.txtLatest.Text = "?";
			//
			//Label2
			//
			this.Label2.Location = new System.Drawing.Point(88, 40);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(240, 24);
			this.Label2.TabIndex = 4;
			this.Label2.Text = "[Update_Latest]";
			this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//lblStatus
			//
			this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (12.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.lblStatus.Location = new System.Drawing.Point(8, 72);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(504, 72);
			this.lblStatus.TabIndex = 6;
			this.lblStatus.Text = "[Update_Contacting]";
			this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			//tmrStart
			//
			this.tmrStart.Enabled = true;
			this.tmrStart.Interval = 200;
			//
			//btnDownload
			//
			this.btnDownload.Location = new System.Drawing.Point(8, 184);
			this.btnDownload.Name = "btnDownload";
			this.btnDownload.Size = new System.Drawing.Size(392, 32);
			this.btnDownload.TabIndex = 7;
			this.btnDownload.Text = "Button1";
			this.btnDownload.UseVisualStyleBackColor = true;
			this.btnDownload.Visible = false;
			//
			//tmrRefresh
			//
			this.tmrRefresh.Interval = 500;
			//
			//proDownload
			//
			this.proDownload.Location = new System.Drawing.Point(8, 152);
			this.proDownload.Name = "proDownload";
			this.proDownload.Size = new System.Drawing.Size(504, 23);
			this.proDownload.TabIndex = 9;
			this.proDownload.Visible = false;
			//
			//frmUpdate
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(525, 229);
			this.ControlBox = false;
			this.Controls.Add(this.proDownload);
			this.Controls.Add(this.btnDownload);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.txtLatest);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.txtCurrent);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.btnClose);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmUpdate";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[Update_Title]";
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.TextBox txtCurrent;
		private System.Windows.Forms.TextBox txtLatest;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Timer tmrStart;
		private System.Windows.Forms.Button btnDownload;
		private System.Windows.Forms.Timer tmrRefresh;
		private System.Windows.Forms.ProgressBar proDownload;
	}
	
}
