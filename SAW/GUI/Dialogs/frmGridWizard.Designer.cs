namespace SAW
{
	partial class frmGridWizard
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGridWizard));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tblLayout = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.nudRows = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.nudColumns = new System.Windows.Forms.NumericUpDown();
			this.chkUseDefaults = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tblOrder = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.rdoIndividual = new System.Windows.Forms.RadioButton();
			this.rdoRows = new System.Windows.Forms.RadioButton();
			this.rdoColumns = new System.Windows.Forms.RadioButton();
			this.tblEscape = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.nudSpacing = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.cmbEscapes = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.cmbEscapeRatio = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.btnPrevious = new System.Windows.Forms.Button();
			this.btnNext = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.pnlPreview = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.tblLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudRows)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudColumns)).BeginInit();
			this.tblOrder.SuspendLayout();
			this.tblEscape.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSpacing)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(6, 4);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(156, 349);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// tblLayout
			// 
			this.tblLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tblLayout.ColumnCount = 2;
			this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tblLayout.Controls.Add(this.label1, 0, 0);
			this.tblLayout.Controls.Add(this.nudRows, 1, 1);
			this.tblLayout.Controls.Add(this.label2, 0, 1);
			this.tblLayout.Controls.Add(this.nudColumns, 1, 0);
			this.tblLayout.Controls.Add(this.chkUseDefaults, 0, 2);
			this.tblLayout.Controls.Add(this.label3, 0, 3);
			this.tblLayout.Location = new System.Drawing.Point(169, 4);
			this.tblLayout.Name = "tblLayout";
			this.tblLayout.RowCount = 6;
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblLayout.Size = new System.Drawing.Size(380, 211);
			this.tblLayout.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(293, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Edit_GridColumns]";
			// 
			// nudRows
			// 
			this.nudRows.Location = new System.Drawing.Point(302, 29);
			this.nudRows.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudRows.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudRows.Name = "nudRows";
			this.nudRows.Size = new System.Drawing.Size(75, 20);
			this.nudRows.TabIndex = 2;
			this.nudRows.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.nudRows.ValueChanged += new System.EventHandler(this.nudRows_ValueChanged);
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(293, 26);
			this.label2.TabIndex = 3;
			this.label2.Text = "[SAW_Edit_GridRows]";
			// 
			// nudColumns
			// 
			this.nudColumns.Location = new System.Drawing.Point(302, 3);
			this.nudColumns.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudColumns.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudColumns.Name = "nudColumns";
			this.nudColumns.Size = new System.Drawing.Size(75, 20);
			this.nudColumns.TabIndex = 1;
			this.nudColumns.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.nudColumns.ValueChanged += new System.EventHandler(this.nudColumns_ValueChanged);
			// 
			// chkUseDefaults
			// 
			this.chkUseDefaults.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.chkUseDefaults.AutoSize = true;
			this.tblLayout.SetColumnSpan(this.chkUseDefaults, 2);
			this.chkUseDefaults.Location = new System.Drawing.Point(120, 55);
			this.chkUseDefaults.Name = "chkUseDefaults";
			this.chkUseDefaults.Size = new System.Drawing.Size(140, 17);
			this.chkUseDefaults.TabIndex = 4;
			this.chkUseDefaults.Text = "[SAW_Edit_GridDefault]";
			this.chkUseDefaults.UseVisualStyleBackColor = true;
			this.chkUseDefaults.CheckedChanged += new System.EventHandler(this.chkUseDefaults_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.tblLayout.SetColumnSpan(this.label3, 2);
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(374, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "[SAW_Edit_GridDefault_Explain]";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// tblOrder
			// 
			this.tblOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tblOrder.ColumnCount = 1;
			this.tblOrder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblOrder.Controls.Add(this.label4, 0, 0);
			this.tblOrder.Controls.Add(this.rdoIndividual, 0, 1);
			this.tblOrder.Controls.Add(this.rdoRows, 0, 2);
			this.tblOrder.Controls.Add(this.rdoColumns, 0, 3);
			this.tblOrder.Location = new System.Drawing.Point(185, 234);
			this.tblOrder.Name = "tblOrder";
			this.tblOrder.RowCount = 5;
			this.tblOrder.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblOrder.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblOrder.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblOrder.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblOrder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblOrder.Size = new System.Drawing.Size(380, 211);
			this.tblOrder.TabIndex = 2;
			this.tblOrder.Visible = false;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(152, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "[SAW_Edit_GridOrder_Prompt]";
			// 
			// rdoIndividual
			// 
			this.rdoIndividual.AutoSize = true;
			this.rdoIndividual.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rdoIndividual.Location = new System.Drawing.Point(3, 16);
			this.rdoIndividual.Name = "rdoIndividual";
			this.rdoIndividual.Size = new System.Drawing.Size(374, 17);
			this.rdoIndividual.TabIndex = 1;
			this.rdoIndividual.Text = "[SAW_Edit_GridOrder_Individual]";
			this.rdoIndividual.UseVisualStyleBackColor = true;
			this.rdoIndividual.CheckedChanged += new System.EventHandler(this.rdoIndividual_CheckedChanged);
			// 
			// rdoRows
			// 
			this.rdoRows.AutoSize = true;
			this.rdoRows.Checked = true;
			this.rdoRows.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rdoRows.Location = new System.Drawing.Point(3, 39);
			this.rdoRows.Name = "rdoRows";
			this.rdoRows.Size = new System.Drawing.Size(374, 17);
			this.rdoRows.TabIndex = 2;
			this.rdoRows.TabStop = true;
			this.rdoRows.Text = "[SAW_Edit_GridOrder_Rows]";
			this.rdoRows.UseVisualStyleBackColor = true;
			this.rdoRows.CheckedChanged += new System.EventHandler(this.rdoRows_CheckedChanged);
			// 
			// rdoColumns
			// 
			this.rdoColumns.AutoSize = true;
			this.rdoColumns.Location = new System.Drawing.Point(3, 62);
			this.rdoColumns.Name = "rdoColumns";
			this.rdoColumns.Size = new System.Drawing.Size(177, 17);
			this.rdoColumns.TabIndex = 3;
			this.rdoColumns.Text = "[SAW_Edit_GridOrder_Columns]";
			this.rdoColumns.UseVisualStyleBackColor = true;
			this.rdoColumns.CheckedChanged += new System.EventHandler(this.rdoColumns_CheckedChanged);
			// 
			// tblEscape
			// 
			this.tblEscape.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tblEscape.ColumnCount = 2;
			this.tblEscape.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
			this.tblEscape.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tblEscape.Controls.Add(this.label5, 0, 0);
			this.tblEscape.Controls.Add(this.nudSpacing, 1, 4);
			this.tblEscape.Controls.Add(this.label6, 0, 1);
			this.tblEscape.Controls.Add(this.cmbEscapes, 1, 1);
			this.tblEscape.Controls.Add(this.label7, 0, 2);
			this.tblEscape.Controls.Add(this.cmbEscapeRatio, 1, 2);
			this.tblEscape.Controls.Add(this.label8, 0, 3);
			this.tblEscape.Controls.Add(this.label9, 0, 4);
			this.tblEscape.Location = new System.Drawing.Point(131, 369);
			this.tblEscape.Name = "tblEscape";
			this.tblEscape.RowCount = 8;
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblEscape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblEscape.Size = new System.Drawing.Size(380, 211);
			this.tblEscape.TabIndex = 3;
			this.tblEscape.Visible = false;
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(279, 23);
			this.label5.TabIndex = 1;
			this.label5.Text = "[SAW_Edit_GridEscape1]";
			// 
			// nudSpacing
			// 
			this.nudSpacing.Location = new System.Drawing.Point(288, 100);
			this.nudSpacing.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.nudSpacing.Name = "nudSpacing";
			this.nudSpacing.Size = new System.Drawing.Size(89, 20);
			this.nudSpacing.TabIndex = 0;
			this.nudSpacing.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.nudSpacing.ValueChanged += new System.EventHandler(this.nudSpacing_ValueChanged);
			// 
			// label6
			// 
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(3, 23);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(279, 27);
			this.label6.TabIndex = 2;
			this.label6.Text = "[SAW_Edit_GridEscape_Where]";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmbEscapes
			// 
			this.cmbEscapes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbEscapes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbEscapes.FormattingEnabled = true;
			this.cmbEscapes.Items.AddRange(new object[] {
            "[SAW_Edit_GridEscape_None]",
            "[SAW_Edit_GridEscape_Start]",
            "[SAW_Edit_GridEscape_End]",
            "[SAW_Edit_GridEscape_Both]"});
			this.cmbEscapes.Location = new System.Drawing.Point(288, 26);
			this.cmbEscapes.Name = "cmbEscapes";
			this.cmbEscapes.Size = new System.Drawing.Size(89, 21);
			this.cmbEscapes.TabIndex = 3;
			this.cmbEscapes.SelectedIndexChanged += new System.EventHandler(this.cmbEscapes_SelectedIndexChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label7.Location = new System.Drawing.Point(3, 50);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(279, 27);
			this.label7.TabIndex = 4;
			this.label7.Text = "[SAW_Edit_GridEscape_Size]";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmbEscapeRatio
			// 
			this.cmbEscapeRatio.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbEscapeRatio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbEscapeRatio.FormattingEnabled = true;
			this.cmbEscapeRatio.Items.AddRange(new object[] {
            "10%",
            "20%",
            "25%",
            "30%",
            "40%",
            "50%",
            "75%",
            "100%"});
			this.cmbEscapeRatio.Location = new System.Drawing.Point(288, 53);
			this.cmbEscapeRatio.Name = "cmbEscapeRatio";
			this.cmbEscapeRatio.Size = new System.Drawing.Size(89, 21);
			this.cmbEscapeRatio.TabIndex = 5;
			this.cmbEscapeRatio.SelectedIndexChanged += new System.EventHandler(this.cmbEscapeRatio_SelectedIndexChanged);
			// 
			// label8
			// 
			this.tblEscape.SetColumnSpan(this.label8, 2);
			this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label8.Location = new System.Drawing.Point(3, 77);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(374, 20);
			this.label8.TabIndex = 6;
			this.label8.Text = "[SAW_Edit_GridSpace_Header]";
			// 
			// label9
			// 
			this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label9.Location = new System.Drawing.Point(3, 97);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(279, 20);
			this.label9.TabIndex = 7;
			this.label9.Text = "[SAW_Edit_GridSpace_Label]";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnPrevious
			// 
			this.btnPrevious.Enabled = false;
			this.btnPrevious.Location = new System.Drawing.Point(248, 369);
			this.btnPrevious.Name = "btnPrevious";
			this.btnPrevious.Size = new System.Drawing.Size(96, 28);
			this.btnPrevious.TabIndex = 4;
			this.btnPrevious.Text = "[SAW_Edit_Previous]";
			this.btnPrevious.UseVisualStyleBackColor = true;
			this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
			// 
			// btnNext
			// 
			this.btnNext.Location = new System.Drawing.Point(351, 369);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(96, 28);
			this.btnNext.TabIndex = 5;
			this.btnNext.Text = "[SAW_Edit_Next]";
			this.btnNext.UseVisualStyleBackColor = true;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(453, 369);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(96, 28);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// pnlPreview
			// 
			this.pnlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlPreview.Location = new System.Drawing.Point(198, 221);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlPreview.Size = new System.Drawing.Size(300, 132);
			this.pnlPreview.TabIndex = 6;
			this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
			// 
			// frmGridWizard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(557, 409);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnNext);
			this.Controls.Add(this.btnPrevious);
			this.Controls.Add(this.tblEscape);
			this.Controls.Add(this.tblOrder);
			this.Controls.Add(this.tblLayout);
			this.Controls.Add(this.pnlPreview);
			this.Controls.Add(this.pictureBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmGridWizard";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[SAW_Edit_GridWizard]";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.tblLayout.ResumeLayout(false);
			this.tblLayout.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudRows)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudColumns)).EndInit();
			this.tblOrder.ResumeLayout(false);
			this.tblOrder.PerformLayout();
			this.tblEscape.ResumeLayout(false);
			this.tblEscape.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSpacing)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TableLayoutPanel tblLayout;
		private System.Windows.Forms.TableLayoutPanel tblOrder;
		private System.Windows.Forms.TableLayoutPanel tblEscape;
		private System.Windows.Forms.Button btnPrevious;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown nudColumns;
		private System.Windows.Forms.NumericUpDown nudRows;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox chkUseDefaults;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel pnlPreview;
		private System.Windows.Forms.NumericUpDown nudSpacing;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.RadioButton rdoIndividual;
		private System.Windows.Forms.RadioButton rdoRows;
		private System.Windows.Forms.RadioButton rdoColumns;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cmbEscapes;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox cmbEscapeRatio;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
	}
}