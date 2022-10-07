namespace SAW
{
	internal partial class frmReportOpErrors
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
			System.Windows.Forms.PictureBox pictureBox1;
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblPrompt = new System.Windows.Forms.Label();
			this.txtErrors = new System.Windows.Forms.TextBox();
			this.btnContinue = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblTitle = new System.Windows.Forms.Label();
			pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			pictureBox1.Image = global::Resources.AM.Warning;
			pictureBox1.Location = new System.Drawing.Point(3, 3);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(34, 34);
			pictureBox1.TabIndex = 0;
			pictureBox1.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.lblPrompt, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtErrors, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnContinue, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 3, 3);
			this.tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblTitle, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(422, 303);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tableLayoutPanel1.Resize += new System.EventHandler(this.tableLayoutPanel1_Resize);
			// 
			// lblPrompt
			// 
			this.lblPrompt.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblPrompt, 4);
			this.lblPrompt.Location = new System.Drawing.Point(3, 48);
			this.lblPrompt.Name = "lblPrompt";
			this.lblPrompt.Size = new System.Drawing.Size(88, 13);
			this.lblPrompt.TabIndex = 0;
			this.lblPrompt.Text = "[OpError_Prompt]";
			// 
			// txtErrors
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtErrors, 4);
			this.txtErrors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtErrors.Location = new System.Drawing.Point(3, 64);
			this.txtErrors.Multiline = true;
			this.txtErrors.Name = "txtErrors";
			this.txtErrors.ReadOnly = true;
			this.txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtErrors.Size = new System.Drawing.Size(416, 207);
			this.txtErrors.TabIndex = 1;
			// 
			// btnContinue
			// 
			this.btnContinue.AutoSize = true;
			this.btnContinue.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnContinue.Location = new System.Drawing.Point(271, 277);
			this.btnContinue.Name = "btnContinue";
			this.btnContinue.Size = new System.Drawing.Size(86, 23);
			this.btnContinue.TabIndex = 2;
			this.btnContinue.Text = "[Continue]";
			this.btnContinue.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.AutoSize = true;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(363, 277);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(56, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// lblTitle
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.lblTitle, 3);
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(45, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(374, 48);
			this.lblTitle.TabIndex = 1;
			this.lblTitle.Text = "label2";
			// 
			// frmReportOpErrors
			// 
			this.AcceptButton = this.btnContinue;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(422, 303);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmReportOpErrors";
			this.ShowIcon = false;
			this.Text = "[OpError_Title]";
			((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblPrompt;
		private System.Windows.Forms.TextBox txtErrors;
		private System.Windows.Forms.Button btnContinue;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblTitle;
	}
}