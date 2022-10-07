namespace SAW
{
	internal partial class frmFocusWarning
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.lblMessage = new System.Windows.Forms.Label();
			this.pnlCountdown = new System.Windows.Forms.Panel();
			this.btnOK = new System.Windows.Forms.Button();
			this.tmrCountdown = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// lblMessage
			// 
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.Location = new System.Drawing.Point(64, 12);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(293, 198);
			this.lblMessage.TabIndex = 0;
			this.lblMessage.Text = "[SAW_Run_FocusWarning]";
			// 
			// pnlCountdown
			// 
			this.pnlCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.pnlCountdown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlCountdown.Location = new System.Drawing.Point(12, 12);
			this.pnlCountdown.Name = "pnlCountdown";
			this.pnlCountdown.Size = new System.Drawing.Size(37, 233);
			this.pnlCountdown.TabIndex = 1;
			this.pnlCountdown.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCountdown_Paint);
			// 
			// btnOK
			// 
			this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(158, 222);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// tmrCountdown
			// 
			this.tmrCountdown.Enabled = true;
			this.tmrCountdown.Tick += new System.EventHandler(this.tmrCountdown_Tick);
			// 
			// frmFocusWarning
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnOK;
			this.ClientSize = new System.Drawing.Size(365, 253);
			this.ControlBox = false;
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.pnlCountdown);
			this.Controls.Add(this.lblMessage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmFocusWarning";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[SAW_Run_FocusTitle]";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.frmFocusWarning_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.Panel pnlCountdown;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Timer tmrCountdown;
	}
}