using System.Windows.Forms;

namespace SAW
{
	public partial class frmShowErrorReport : System.Windows.Forms.Form
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
			this.txtError = new System.Windows.Forms.TextBox();
			this.txtMessage = new System.Windows.Forms.TextBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.Label1 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.txtErrorLog = new System.Windows.Forms.TextBox();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblWarning = new System.Windows.Forms.Label();
			this.btnUserConfigs = new System.Windows.Forms.Button();
			this.rdoSubErrors = new System.Windows.Forms.RadioButton();
			this.rdoLog = new System.Windows.Forms.RadioButton();
			this.rdoPermanentLog = new System.Windows.Forms.RadioButton();
			this.lblActivity = new System.Windows.Forms.Label();
			this.rdoEvents = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// txtError
			// 
			this.txtError.Location = new System.Drawing.Point(8, 24);
			this.txtError.Multiline = true;
			this.txtError.Name = "txtError";
			this.txtError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtError.Size = new System.Drawing.Size(528, 104);
			this.txtError.TabIndex = 0;
			// 
			// txtMessage
			// 
			this.txtMessage.Location = new System.Drawing.Point(8, 152);
			this.txtMessage.Multiline = true;
			this.txtMessage.Name = "txtMessage";
			this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtMessage.Size = new System.Drawing.Size(528, 88);
			this.txtMessage.TabIndex = 1;
			// 
			// btnClose
			// 
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(448, 392);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(88, 32);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(8, 8);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(32, 13);
			this.Label1.TabIndex = 3;
			this.Label1.Text = "Error:";
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(8, 136);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(77, 13);
			this.Label2.TabIndex = 4;
			this.Label2.Text = "User message:";
			// 
			// txtErrorLog
			// 
			this.txtErrorLog.Location = new System.Drawing.Point(8, 288);
			this.txtErrorLog.Multiline = true;
			this.txtErrorLog.Name = "txtErrorLog";
			this.txtErrorLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtErrorLog.Size = new System.Drawing.Size(528, 88);
			this.txtErrorLog.TabIndex = 5;
			// 
			// lblVersion
			// 
			this.lblVersion.AutoSize = true;
			this.lblVersion.Location = new System.Drawing.Point(16, 384);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(48, 13);
			this.lblVersion.TabIndex = 7;
			this.lblVersion.Text = "Version: ";
			// 
			// lblWarning
			// 
			this.lblWarning.Location = new System.Drawing.Point(16, 408);
			this.lblWarning.Name = "lblWarning";
			this.lblWarning.Size = new System.Drawing.Size(328, 32);
			this.lblWarning.TabIndex = 8;
			// 
			// btnUserConfigs
			// 
			this.btnUserConfigs.Location = new System.Drawing.Point(352, 392);
			this.btnUserConfigs.Name = "btnUserConfigs";
			this.btnUserConfigs.Size = new System.Drawing.Size(88, 40);
			this.btnUserConfigs.TabIndex = 9;
			this.btnUserConfigs.Text = "Apply user configs";
			this.btnUserConfigs.UseVisualStyleBackColor = true;
			this.btnUserConfigs.Click += new System.EventHandler(this.btnUserConfigs_Click);
			// 
			// rdoSubErrors
			// 
			this.rdoSubErrors.AutoSize = true;
			this.rdoSubErrors.Checked = true;
			this.rdoSubErrors.Location = new System.Drawing.Point(8, 272);
			this.rdoSubErrors.Name = "rdoSubErrors";
			this.rdoSubErrors.Size = new System.Drawing.Size(130, 17);
			this.rdoSubErrors.TabIndex = 11;
			this.rdoSubErrors.TabStop = true;
			this.rdoSubErrors.Text = "SubErrors (oldest first):";
			this.rdoSubErrors.UseVisualStyleBackColor = true;
			this.rdoSubErrors.CheckedChanged += new System.EventHandler(this.rdoSubErrors_CheckedChanged);
			// 
			// rdoLog
			// 
			this.rdoLog.AutoSize = true;
			this.rdoLog.Location = new System.Drawing.Point(144, 272);
			this.rdoLog.Name = "rdoLog";
			this.rdoLog.Size = new System.Drawing.Size(43, 17);
			this.rdoLog.TabIndex = 12;
			this.rdoLog.Text = "Log";
			this.rdoLog.UseVisualStyleBackColor = true;
			this.rdoLog.CheckedChanged += new System.EventHandler(this.rdoSubErrors_CheckedChanged);
			// 
			// rdoPermanentLog
			// 
			this.rdoPermanentLog.AutoSize = true;
			this.rdoPermanentLog.Location = new System.Drawing.Point(192, 272);
			this.rdoPermanentLog.Name = "rdoPermanentLog";
			this.rdoPermanentLog.Size = new System.Drawing.Size(93, 17);
			this.rdoPermanentLog.TabIndex = 13;
			this.rdoPermanentLog.Text = "Permanent log";
			this.rdoPermanentLog.UseVisualStyleBackColor = true;
			this.rdoPermanentLog.CheckedChanged += new System.EventHandler(this.rdoSubErrors_CheckedChanged);
			// 
			// lblActivity
			// 
			this.lblActivity.AutoSize = true;
			this.lblActivity.Location = new System.Drawing.Point(16, 440);
			this.lblActivity.Name = "lblActivity";
			this.lblActivity.Size = new System.Drawing.Size(44, 13);
			this.lblActivity.TabIndex = 14;
			this.lblActivity.Text = "Activity:";
			// 
			// rdoEvents
			// 
			this.rdoEvents.AutoSize = true;
			this.rdoEvents.Location = new System.Drawing.Point(291, 272);
			this.rdoEvents.Name = "rdoEvents";
			this.rdoEvents.Size = new System.Drawing.Size(68, 17);
			this.rdoEvents.TabIndex = 16;
			this.rdoEvents.TabStop = true;
			this.rdoEvents.Text = "Event list";
			this.rdoEvents.UseVisualStyleBackColor = true;
			this.rdoEvents.CheckedChanged += new System.EventHandler(this.rdoSubErrors_CheckedChanged);
			// 
			// frmShowErrorReport
			// 
			this.AcceptButton = this.btnClose;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(541, 466);
			this.Controls.Add(this.rdoEvents);
			this.Controls.Add(this.lblActivity);
			this.Controls.Add(this.txtErrorLog);
			this.Controls.Add(this.rdoPermanentLog);
			this.Controls.Add(this.rdoLog);
			this.Controls.Add(this.rdoSubErrors);
			this.Controls.Add(this.btnUserConfigs);
			this.Controls.Add(this.lblWarning);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.txtMessage);
			this.Controls.Add(this.txtError);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmShowErrorReport";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Error report";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.TextBox txtError;
		private System.Windows.Forms.TextBox txtMessage;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.TextBox txtErrorLog;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.Label lblWarning;
		private System.Windows.Forms.Button btnUserConfigs;
		private System.Windows.Forms.RadioButton rdoSubErrors;
		private System.Windows.Forms.RadioButton rdoLog;
		private System.Windows.Forms.RadioButton rdoPermanentLog;
		private System.Windows.Forms.Label lblActivity;
		private RadioButton rdoEvents;
	}
	
}
