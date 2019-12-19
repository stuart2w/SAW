namespace SAW.CCF
{
	partial class frmMultiUpdateProgress
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
			System.Windows.Forms.Label label1;
			this.label2 = new System.Windows.Forms.Label();
			this.lblCurrent = new System.Windows.Forms.Label();
			this.lblDone = new System.Windows.Forms.Label();
			this.txtLog = new System.Windows.Forms.TextBox();
			label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.Location = new System.Drawing.Point(8, 8);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(96, 24);
			label1.TabIndex = 0;
			label1.Text = "[CCF_Update_Updating]";
			label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 24);
			this.label2.TabIndex = 1;
			this.label2.Text = "[CCF_Update_Done]";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblCurrent
			// 
			this.lblCurrent.Location = new System.Drawing.Point(104, 8);
			this.lblCurrent.Name = "lblCurrent";
			this.lblCurrent.Size = new System.Drawing.Size(208, 24);
			this.lblCurrent.TabIndex = 2;
			// 
			// lblDone
			// 
			this.lblDone.Location = new System.Drawing.Point(104, 40);
			this.lblDone.Name = "lblDone";
			this.lblDone.Size = new System.Drawing.Size(208, 24);
			this.lblDone.TabIndex = 3;
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(8, 72);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLog.Size = new System.Drawing.Size(296, 168);
			this.txtLog.TabIndex = 4;
			// 
			// frmMultiUpdateProgress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(309, 247);
			this.ControlBox = false;
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.lblDone);
			this.Controls.Add(this.lblCurrent);
			this.Controls.Add(this.label2);
			this.Controls.Add(label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMultiUpdateProgress";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[CCF_Update_Progress]";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblCurrent;
		private System.Windows.Forms.Label lblDone;
		private System.Windows.Forms.TextBox txtLog;
	}
}