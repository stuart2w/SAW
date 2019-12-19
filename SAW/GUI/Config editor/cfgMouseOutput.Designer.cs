namespace SAW
{
	partial class cfgMouseOutput
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
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoStep = new System.Windows.Forms.RadioButton();
			this.rdoContinuous = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.nudSmall = new System.Windows.Forms.NumericUpDown();
			this.nudMedium = new System.Windows.Forms.NumericUpDown();
			this.nudLarge = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.btnTestSmall = new System.Windows.Forms.Button();
			this.btnTestMedium = new System.Windows.Forms.Button();
			this.btnTestLarge = new System.Windows.Forms.Button();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.label9 = new System.Windows.Forms.Label();
			this.nudWaitMultiplier = new System.Windows.Forms.NumericUpDown();
			Label44 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSmall)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMedium)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudLarge)).BeginInit();
			this.flowLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudWaitMultiplier)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(Label44, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(583, 475);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(164, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Settings_MouseAndCaret]";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.rdoStep);
			this.flowLayoutPanel1.Controls.Add(this.rdoContinuous);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 33);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(583, 23);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// rdoStep
			// 
			this.rdoStep.AutoSize = true;
			this.rdoStep.Location = new System.Drawing.Point(3, 3);
			this.rdoStep.Name = "rdoStep";
			this.rdoStep.Size = new System.Drawing.Size(171, 17);
			this.rdoStep.TabIndex = 0;
			this.rdoStep.TabStop = true;
			this.rdoStep.Text = "[Script_Command_MouseStep]";
			this.rdoStep.UseVisualStyleBackColor = true;
			this.rdoStep.CheckedChanged += new System.EventHandler(this.rdoStep_CheckedChanged);
			// 
			// rdoContinuous
			// 
			this.rdoContinuous.AutoSize = true;
			this.rdoContinuous.Location = new System.Drawing.Point(180, 3);
			this.rdoContinuous.Name = "rdoContinuous";
			this.rdoContinuous.Size = new System.Drawing.Size(202, 17);
			this.rdoContinuous.TabIndex = 1;
			this.rdoContinuous.TabStop = true;
			this.rdoContinuous.Text = "[Script_Command_MouseContinuous]";
			this.rdoContinuous.UseVisualStyleBackColor = true;
			this.rdoContinuous.CheckedChanged += new System.EventHandler(this.rdoContinuous_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 82);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(115, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "[SAW_Settings_Steps]";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 5;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 67F));
			this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label4, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.label5, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.nudSmall, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.nudMedium, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.nudLarge, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.label6, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.label7, 2, 1);
			this.tableLayoutPanel2.Controls.Add(this.label8, 2, 2);
			this.tableLayoutPanel2.Controls.Add(this.btnTestSmall, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.btnTestMedium, 3, 1);
			this.tableLayoutPanel2.Controls.Add(this.btnTestLarge, 3, 2);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 95);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(583, 88);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(168, 29);
			this.label3.TabIndex = 0;
			this.label3.Text = "[Script_Command_MouseSmall]";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 29);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(168, 29);
			this.label4.TabIndex = 1;
			this.label4.Text = "[Script_Command_MouseMedium]";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 58);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(168, 29);
			this.label5.TabIndex = 2;
			this.label5.Text = "[Script_Command_MouseLarge]";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// nudSmall
			// 
			this.nudSmall.Location = new System.Drawing.Point(177, 3);
			this.nudSmall.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
			this.nudSmall.Name = "nudSmall";
			this.nudSmall.Size = new System.Drawing.Size(57, 20);
			this.nudSmall.TabIndex = 3;
			this.nudSmall.ValueChanged += new System.EventHandler(this.nudSmall_ValueChanged);
			// 
			// nudMedium
			// 
			this.nudMedium.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.nudMedium.Location = new System.Drawing.Point(177, 32);
			this.nudMedium.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudMedium.Name = "nudMedium";
			this.nudMedium.Size = new System.Drawing.Size(57, 20);
			this.nudMedium.TabIndex = 4;
			this.nudMedium.ValueChanged += new System.EventHandler(this.nudMedium_ValueChanged);
			// 
			// nudLarge
			// 
			this.nudLarge.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudLarge.Location = new System.Drawing.Point(177, 61);
			this.nudLarge.Name = "nudLarge";
			this.nudLarge.Size = new System.Drawing.Size(57, 20);
			this.nudLarge.TabIndex = 5;
			this.nudLarge.ValueChanged += new System.EventHandler(this.nudLarge_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(240, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(115, 29);
			this.label6.TabIndex = 6;
			this.label6.Text = "[SAW_Settings_Pixels]";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Location = new System.Drawing.Point(240, 29);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(115, 29);
			this.label7.TabIndex = 7;
			this.label7.Text = "[SAW_Settings_Pixels]";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label8.Location = new System.Drawing.Point(240, 58);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(115, 29);
			this.label8.TabIndex = 8;
			this.label8.Text = "[SAW_Settings_Pixels]";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnTestSmall
			// 
			this.btnTestSmall.AutoSize = true;
			this.btnTestSmall.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnTestSmall.Location = new System.Drawing.Point(361, 3);
			this.btnTestSmall.Name = "btnTestSmall";
			this.btnTestSmall.Size = new System.Drawing.Size(139, 23);
			this.btnTestSmall.TabIndex = 9;
			this.btnTestSmall.Text = "[SAW_Settings_TestFine]";
			this.btnTestSmall.UseVisualStyleBackColor = true;
			this.btnTestSmall.Click += new System.EventHandler(this.btnTestSmall_Click);
			// 
			// btnTestMedium
			// 
			this.btnTestMedium.AutoSize = true;
			this.btnTestMedium.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnTestMedium.Location = new System.Drawing.Point(361, 32);
			this.btnTestMedium.Name = "btnTestMedium";
			this.btnTestMedium.Size = new System.Drawing.Size(152, 23);
			this.btnTestMedium.TabIndex = 10;
			this.btnTestMedium.Text = "[SAW_Settings_TestNormal]";
			this.btnTestMedium.UseVisualStyleBackColor = true;
			this.btnTestMedium.Click += new System.EventHandler(this.btnTestMedium_Click);
			// 
			// btnTestLarge
			// 
			this.btnTestLarge.AutoSize = true;
			this.btnTestLarge.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnTestLarge.Location = new System.Drawing.Point(361, 61);
			this.btnTestLarge.Name = "btnTestLarge";
			this.btnTestLarge.Size = new System.Drawing.Size(152, 23);
			this.btnTestLarge.TabIndex = 11;
			this.btnTestLarge.Text = "[SAW_Settings_TestCoarse]";
			this.btnTestLarge.UseVisualStyleBackColor = true;
			this.btnTestLarge.Click += new System.EventHandler(this.btnTestLarge_Click);
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel2.Controls.Add(this.label9);
			this.flowLayoutPanel2.Controls.Add(this.nudWaitMultiplier);
			this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 56);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(249, 26);
			this.flowLayoutPanel2.TabIndex = 4;
			// 
			// label9
			// 
			this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 6);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(151, 13);
			this.label9.TabIndex = 0;
			this.label9.Text = "[SAW_Settings_WaitMultiplier]";
			// 
			// nudWaitMultiplier
			// 
			this.nudWaitMultiplier.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudWaitMultiplier.Location = new System.Drawing.Point(160, 3);
			this.nudWaitMultiplier.Maximum = new decimal(new int[] {
            250,
            0,
            0,
            0});
			this.nudWaitMultiplier.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
			this.nudWaitMultiplier.Name = "nudWaitMultiplier";
			this.nudWaitMultiplier.Size = new System.Drawing.Size(86, 20);
			this.nudWaitMultiplier.TabIndex = 1;
			this.nudWaitMultiplier.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
			this.nudWaitMultiplier.ValueChanged += new System.EventHandler(this.nudWaitMultiplier_ValueChanged);
			// 
			// Label44
			// 
			Label44.AutoSize = true;
			Label44.BackColor = System.Drawing.Color.PowderBlue;
			Label44.Dock = System.Windows.Forms.DockStyle.Top;
			Label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label44.Location = new System.Drawing.Point(3, 0);
			Label44.Name = "Label44";
			Label44.Size = new System.Drawing.Size(577, 20);
			Label44.TabIndex = 6;
			Label44.Text = "[SAW_Settings_MouseMovement]";
			// 
			// cfgMouseOutput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "cfgMouseOutput";
			this.Size = new System.Drawing.Size(583, 475);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSmall)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudMedium)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudLarge)).EndInit();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudWaitMultiplier)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton rdoStep;
		private System.Windows.Forms.RadioButton rdoContinuous;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown nudSmall;
		private System.Windows.Forms.NumericUpDown nudMedium;
		private System.Windows.Forms.NumericUpDown nudLarge;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnTestSmall;
		private System.Windows.Forms.Button btnTestMedium;
		private System.Windows.Forms.Button btnTestLarge;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown nudWaitMultiplier;
	}
}
