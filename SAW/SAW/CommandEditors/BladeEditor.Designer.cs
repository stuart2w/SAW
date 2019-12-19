namespace SAW.CommandEditors
{
	partial class BladeEditor
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
			this.pnlItem = new System.Windows.Forms.FlowLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtItemID = new ItemIDTextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.chkSimpleSingleLetter = new System.Windows.Forms.CheckBox();
			this.chkAlphabeticalResults = new System.Windows.Forms.CheckBox();
			this.chkOmitPrevious = new System.Windows.Forms.CheckBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.chkMinimumLength = new System.Windows.Forms.CheckBox();
			this.nudMinimumLength = new System.Windows.Forms.NumericUpDown();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.chkMinimumGain = new System.Windows.Forms.CheckBox();
			this.nudMinimumGain = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.chkCase = new System.Windows.Forms.CheckBox();
			this.cmbCase = new System.Windows.Forms.ComboBox();
			this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
			this.chkPunctSpace = new System.Windows.Forms.CheckBox();
			this.cmbPunctSpace = new System.Windows.Forms.ComboBox();
			this.pnlItem.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMinimumLength)).BeginInit();
			this.flowLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMinimumGain)).BeginInit();
			this.flowLayoutPanel3.SuspendLayout();
			this.flowLayoutPanel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlItem
			// 
			this.pnlItem.AutoSize = true;
			this.pnlItem.Controls.Add(this.label1);
			this.pnlItem.Controls.Add(this.txtItemID);
			this.pnlItem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlItem.Location = new System.Drawing.Point(0, 0);
			this.pnlItem.Margin = new System.Windows.Forms.Padding(0);
			this.pnlItem.Name = "pnlItem";
			this.pnlItem.Size = new System.Drawing.Size(381, 26);
			this.pnlItem.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(164, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_CommandEdit_WordlistID]";
			// 
			// txtItemID
			// 
			this.txtItemID.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtItemID.InvalidMessage = null;
			this.txtItemID.Location = new System.Drawing.Point(173, 3);
			this.txtItemID.Name = "txtItemID";
			this.txtItemID.Size = new System.Drawing.Size(100, 20);
			this.txtItemID.TabIndex = 1;
			this.txtItemID.Value = 0;
			this.txtItemID.TextChanged += new System.EventHandler(this.txtItemID_TextChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.chkSimpleSingleLetter, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.pnlItem, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.chkAlphabeticalResults, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkOmitPrevious, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel4, 0, 9);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 12;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
			this.tableLayoutPanel1.Size = new System.Drawing.Size(381, 331);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// chkSimpleSingleLetter
			// 
			this.chkSimpleSingleLetter.AutoSize = true;
			this.chkSimpleSingleLetter.Location = new System.Drawing.Point(3, 88);
			this.chkSimpleSingleLetter.Name = "chkSimpleSingleLetter";
			this.chkSimpleSingleLetter.Size = new System.Drawing.Size(225, 17);
			this.chkSimpleSingleLetter.TabIndex = 3;
			this.chkSimpleSingleLetter.Tag = "SimpleSingleLetter";
			this.chkSimpleSingleLetter.Text = "[SAW_CommandEdit_BladeSimple1Letter]";
			this.chkSimpleSingleLetter.ThreeState = true;
			this.chkSimpleSingleLetter.UseVisualStyleBackColor = true;
			this.chkSimpleSingleLetter.CheckStateChanged += new System.EventHandler(this.chkSimpleSingleLetter_CheckStateChanged);
			// 
			// chkAlphabeticalResults
			// 
			this.chkAlphabeticalResults.AutoSize = true;
			this.chkAlphabeticalResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkAlphabeticalResults.Location = new System.Drawing.Point(3, 42);
			this.chkAlphabeticalResults.Name = "chkAlphabeticalResults";
			this.chkAlphabeticalResults.Size = new System.Drawing.Size(375, 17);
			this.chkAlphabeticalResults.TabIndex = 1;
			this.chkAlphabeticalResults.Tag = "AlphabeticalResults";
			this.chkAlphabeticalResults.Text = "[SAW_CommandEdit_BladeAlphabetical]";
			this.chkAlphabeticalResults.ThreeState = true;
			this.chkAlphabeticalResults.UseVisualStyleBackColor = true;
			this.chkAlphabeticalResults.CheckStateChanged += new System.EventHandler(this.chkAlphabeticalResults_CheckStateChanged);
			// 
			// chkOmitPrevious
			// 
			this.chkOmitPrevious.AutoSize = true;
			this.chkOmitPrevious.Location = new System.Drawing.Point(3, 65);
			this.chkOmitPrevious.Name = "chkOmitPrevious";
			this.chkOmitPrevious.Size = new System.Drawing.Size(223, 17);
			this.chkOmitPrevious.TabIndex = 2;
			this.chkOmitPrevious.Tag = "OmitPreviousSuggestions";
			this.chkOmitPrevious.Text = "[SAW_CommandEdit_BladeOmitPrevious]";
			this.chkOmitPrevious.ThreeState = true;
			this.chkOmitPrevious.UseVisualStyleBackColor = true;
			this.chkOmitPrevious.CheckStateChanged += new System.EventHandler(this.chkOmitPrevious_CheckStateChanged);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.chkMinimumLength);
			this.flowLayoutPanel1.Controls.Add(this.nudMinimumLength);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 121);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(381, 26);
			this.flowLayoutPanel1.TabIndex = 4;
			// 
			// chkMinimumLength
			// 
			this.chkMinimumLength.AutoSize = true;
			this.chkMinimumLength.Location = new System.Drawing.Point(3, 3);
			this.chkMinimumLength.Name = "chkMinimumLength";
			this.chkMinimumLength.Size = new System.Drawing.Size(202, 17);
			this.chkMinimumLength.TabIndex = 2;
			this.chkMinimumLength.Text = "[SAW_CommandEdit_BladeMinimum]";
			this.chkMinimumLength.UseVisualStyleBackColor = true;
			this.chkMinimumLength.CheckedChanged += new System.EventHandler(this.chkMinimumLength_CheckedChanged);
			// 
			// nudMinimumLength
			// 
			this.nudMinimumLength.Enabled = false;
			this.nudMinimumLength.Location = new System.Drawing.Point(211, 3);
			this.nudMinimumLength.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudMinimumLength.Name = "nudMinimumLength";
			this.nudMinimumLength.Size = new System.Drawing.Size(52, 20);
			this.nudMinimumLength.TabIndex = 1;
			this.nudMinimumLength.ValueChanged += new System.EventHandler(this.nudMinimumLength_ValueChanged);
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.Controls.Add(this.chkMinimumGain);
			this.flowLayoutPanel2.Controls.Add(this.nudMinimumGain);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 147);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(381, 26);
			this.flowLayoutPanel2.TabIndex = 5;
			// 
			// chkMinimumGain
			// 
			this.chkMinimumGain.AutoSize = true;
			this.chkMinimumGain.Location = new System.Drawing.Point(3, 3);
			this.chkMinimumGain.Name = "chkMinimumGain";
			this.chkMinimumGain.Size = new System.Drawing.Size(202, 17);
			this.chkMinimumGain.TabIndex = 0;
			this.chkMinimumGain.Text = "[SAW_CommandEdit_BladeMinimum]";
			this.chkMinimumGain.UseVisualStyleBackColor = true;
			this.chkMinimumGain.CheckedChanged += new System.EventHandler(this.chkMinimumGain_CheckedChanged);
			// 
			// nudMinimumGain
			// 
			this.nudMinimumGain.Enabled = false;
			this.nudMinimumGain.Location = new System.Drawing.Point(211, 3);
			this.nudMinimumGain.Name = "nudMinimumGain";
			this.nudMinimumGain.Size = new System.Drawing.Size(120, 20);
			this.nudMinimumGain.TabIndex = 1;
			this.nudMinimumGain.ValueChanged += new System.EventHandler(this.nudMinimumGain_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.ControlDark;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 108);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(375, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "[SAW_CommandEdit_BladeOther]";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.ControlDark;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 26);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(375, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "[SAW_CommandEdit_BladeBoolean]";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.AutoSize = true;
			this.flowLayoutPanel3.Controls.Add(this.chkCase);
			this.flowLayoutPanel3.Controls.Add(this.cmbCase);
			this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 173);
			this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(381, 27);
			this.flowLayoutPanel3.TabIndex = 8;
			// 
			// chkCase
			// 
			this.chkCase.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkCase.AutoSize = true;
			this.chkCase.Location = new System.Drawing.Point(3, 5);
			this.chkCase.Name = "chkCase";
			this.chkCase.Size = new System.Drawing.Size(185, 17);
			this.chkCase.TabIndex = 0;
			this.chkCase.Text = "[SAW_CommandEdit_BladeCase]";
			this.chkCase.UseVisualStyleBackColor = true;
			this.chkCase.CheckedChanged += new System.EventHandler(this.chkCase_CheckedChanged);
			// 
			// cmbCase
			// 
			this.cmbCase.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cmbCase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbCase.Enabled = false;
			this.cmbCase.FormattingEnabled = true;
			this.cmbCase.Items.AddRange(new object[] {
            "[SAW_CommandEdit_BladeLower]",
            "[SAW_CommandEdit_BladeNormal]",
            "[SAW_CommandEdit_BladeInitialUpper]",
            "[SAW_CommandEdit_BladeUpper]"});
			this.cmbCase.Location = new System.Drawing.Point(194, 3);
			this.cmbCase.Name = "cmbCase";
			this.cmbCase.Size = new System.Drawing.Size(121, 21);
			this.cmbCase.TabIndex = 1;
			this.cmbCase.SelectedIndexChanged += new System.EventHandler(this.cmbCase_SelectedIndexChanged);
			// 
			// flowLayoutPanel4
			// 
			this.flowLayoutPanel4.AutoSize = true;
			this.flowLayoutPanel4.Controls.Add(this.chkPunctSpace);
			this.flowLayoutPanel4.Controls.Add(this.cmbPunctSpace);
			this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel4.Location = new System.Drawing.Point(0, 200);
			this.flowLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel4.Name = "flowLayoutPanel4";
			this.flowLayoutPanel4.Size = new System.Drawing.Size(381, 27);
			this.flowLayoutPanel4.TabIndex = 9;
			// 
			// chkPunctSpace
			// 
			this.chkPunctSpace.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkPunctSpace.AutoSize = true;
			this.chkPunctSpace.Location = new System.Drawing.Point(3, 5);
			this.chkPunctSpace.Name = "chkPunctSpace";
			this.chkPunctSpace.Size = new System.Drawing.Size(220, 17);
			this.chkPunctSpace.TabIndex = 0;
			this.chkPunctSpace.Text = "[SAW_CommandEdit_BladePunctSpace]";
			this.chkPunctSpace.UseVisualStyleBackColor = true;
			this.chkPunctSpace.CheckedChanged += new System.EventHandler(this.chkPunctSpace_CheckedChanged);
			// 
			// cmbPunctSpace
			// 
			this.cmbPunctSpace.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cmbPunctSpace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPunctSpace.Enabled = false;
			this.cmbPunctSpace.FormattingEnabled = true;
			this.cmbPunctSpace.Items.AddRange(new object[] {
            "0",
            "1",
            "2"});
			this.cmbPunctSpace.Location = new System.Drawing.Point(229, 3);
			this.cmbPunctSpace.Name = "cmbPunctSpace";
			this.cmbPunctSpace.Size = new System.Drawing.Size(61, 21);
			this.cmbPunctSpace.TabIndex = 1;
			this.cmbPunctSpace.SelectedIndexChanged += new System.EventHandler(this.cmbPunctSpace_SelectedIndexChanged);
			// 
			// BladeEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "BladeEditor";
			this.Size = new System.Drawing.Size(381, 331);
			this.pnlItem.ResumeLayout(false);
			this.pnlItem.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMinimumLength)).EndInit();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudMinimumGain)).EndInit();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.flowLayoutPanel3.PerformLayout();
			this.flowLayoutPanel4.ResumeLayout(false);
			this.flowLayoutPanel4.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel pnlItem;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private ItemIDTextBox txtItemID;
		private System.Windows.Forms.CheckBox chkSimpleSingleLetter;
		private System.Windows.Forms.CheckBox chkAlphabeticalResults;
		private System.Windows.Forms.CheckBox chkOmitPrevious;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.NumericUpDown nudMinimumLength;
		private System.Windows.Forms.CheckBox chkMinimumLength;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.CheckBox chkMinimumGain;
		private System.Windows.Forms.NumericUpDown nudMinimumGain;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.CheckBox chkCase;
		private System.Windows.Forms.ComboBox cmbCase;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
		private System.Windows.Forms.CheckBox chkPunctSpace;
		private System.Windows.Forms.ComboBox cmbPunctSpace;
	}
}
