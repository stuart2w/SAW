namespace SAW
{
	partial class cfgOutput
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label Label44;
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.chkSpeech = new System.Windows.Forms.CheckBox();
			this.chkSpeechVisit = new System.Windows.Forms.CheckBox();
			this.chkWAV = new System.Windows.Forms.CheckBox();
			this.chkApplication = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.chkHideTitle = new System.Windows.Forms.CheckBox();
			this.nudHideDelay = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			Label44 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudHideDelay)).BeginInit();
			this.SuspendLayout();
			// 
			// Label44
			// 
			Label44.AutoSize = true;
			Label44.BackColor = System.Drawing.Color.PowderBlue;
			this.tableLayoutPanel1.SetColumnSpan(Label44, 2);
			Label44.Dock = System.Windows.Forms.DockStyle.Top;
			Label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label44.Location = new System.Drawing.Point(3, 0);
			Label44.Name = "Label44";
			Label44.Size = new System.Drawing.Size(386, 20);
			Label44.TabIndex = 9;
			Label44.Text = "[SAW_Settings_Output]";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(Label44, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.chkSpeech, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkSpeechVisit, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.chkWAV, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.chkApplication, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.chkHideTitle, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.nudHideDelay, 0, 9);
			this.tableLayoutPanel1.Controls.Add(this.label3, 1, 9);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 11;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(392, 320);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(386, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Settings_SendOutput]";
			// 
			// chkSpeech
			// 
			this.chkSpeech.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkSpeech, 2);
			this.chkSpeech.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkSpeech.Location = new System.Drawing.Point(3, 36);
			this.chkSpeech.Name = "chkSpeech";
			this.chkSpeech.Size = new System.Drawing.Size(386, 17);
			this.chkSpeech.TabIndex = 1;
			this.chkSpeech.Text = "[SAW_Settings_Speech]";
			this.chkSpeech.UseVisualStyleBackColor = true;
			this.chkSpeech.CheckedChanged += new System.EventHandler(this.chkSpeech_CheckedChanged);
			// 
			// chkSpeechVisit
			// 
			this.chkSpeechVisit.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkSpeechVisit, 2);
			this.chkSpeechVisit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkSpeechVisit.Location = new System.Drawing.Point(3, 59);
			this.chkSpeechVisit.Name = "chkSpeechVisit";
			this.chkSpeechVisit.Size = new System.Drawing.Size(386, 17);
			this.chkSpeechVisit.TabIndex = 2;
			this.chkSpeechVisit.Text = "[SAW_Settings_SpeechVisit]";
			this.chkSpeechVisit.UseVisualStyleBackColor = true;
			this.chkSpeechVisit.CheckedChanged += new System.EventHandler(this.chkSpeechVisit_CheckedChanged);
			// 
			// chkWAV
			// 
			this.chkWAV.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkWAV, 2);
			this.chkWAV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkWAV.Location = new System.Drawing.Point(3, 82);
			this.chkWAV.Name = "chkWAV";
			this.chkWAV.Size = new System.Drawing.Size(386, 17);
			this.chkWAV.TabIndex = 3;
			this.chkWAV.Text = "[SAW_Settings_WAV]";
			this.chkWAV.UseVisualStyleBackColor = true;
			this.chkWAV.CheckedChanged += new System.EventHandler(this.chkWAV_CheckedChanged);
			// 
			// chkApplication
			// 
			this.chkApplication.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkApplication, 2);
			this.chkApplication.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkApplication.Location = new System.Drawing.Point(3, 105);
			this.chkApplication.Name = "chkApplication";
			this.chkApplication.Size = new System.Drawing.Size(386, 17);
			this.chkApplication.TabIndex = 4;
			this.chkApplication.Text = "[SAW_Settings_SendApplication]";
			this.chkApplication.UseVisualStyleBackColor = true;
			this.chkApplication.CheckedChanged += new System.EventHandler(this.chkApplication_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label2, 2);
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 125);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(386, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "[SAW_Settings_OtherOutput]";
			// 
			// chkHideTitle
			// 
			this.chkHideTitle.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkHideTitle, 2);
			this.chkHideTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkHideTitle.Location = new System.Drawing.Point(3, 141);
			this.chkHideTitle.Name = "chkHideTitle";
			this.chkHideTitle.Size = new System.Drawing.Size(386, 17);
			this.chkHideTitle.TabIndex = 6;
			this.chkHideTitle.Text = "[SAW_Settings_HideTitle]";
			this.chkHideTitle.UseVisualStyleBackColor = true;
			this.chkHideTitle.CheckedChanged += new System.EventHandler(this.chkHideTitle_CheckedChanged);
			// 
			// nudHideDelay
			// 
			this.nudHideDelay.DecimalPlaces = 1;
			this.nudHideDelay.Location = new System.Drawing.Point(3, 164);
			this.nudHideDelay.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudHideDelay.Name = "nudHideDelay";
			this.nudHideDelay.Size = new System.Drawing.Size(62, 20);
			this.nudHideDelay.TabIndex = 7;
			this.nudHideDelay.ValueChanged += new System.EventHandler(this.nudHideDelay_ValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(71, 161);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(318, 20);
			this.label3.TabIndex = 8;
			this.label3.Text = "[SAW_Settings_HideTime]";
			// 
			// cfgOutput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "cfgOutput";
			this.Size = new System.Drawing.Size(392, 320);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudHideDelay)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox chkSpeech;
		private System.Windows.Forms.CheckBox chkSpeechVisit;
		private System.Windows.Forms.CheckBox chkWAV;
		private System.Windows.Forms.CheckBox chkApplication;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox chkHideTitle;
		private System.Windows.Forms.NumericUpDown nudHideDelay;
		private System.Windows.Forms.Label label3;
	}
}
