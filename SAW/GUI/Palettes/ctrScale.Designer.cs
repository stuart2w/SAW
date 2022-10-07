namespace SAW
{
	internal partial class ctrScale
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
			this.nudValue = new System.Windows.Forms.NumericUpDown();
			this.rdoValue = new System.Windows.Forms.RadioButton();
			this.rdoPercent = new System.Windows.Forms.RadioButton();
			this.btnSmaller = new System.Windows.Forms.Button();
			this.btnBigger = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.nudValue)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// nudValue
			// 
			this.nudValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.nudValue, 2);
			this.nudValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nudValue.Location = new System.Drawing.Point(46, 5);
			this.nudValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.nudValue.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.nudValue.Name = "nudValue";
			this.nudValue.Size = new System.Drawing.Size(80, 23);
			this.nudValue.TabIndex = 0;
			this.nudValue.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			// 
			// rdoValue
			// 
			this.rdoValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rdoValue.Appearance = System.Windows.Forms.Appearance.Button;
			this.rdoValue.Location = new System.Drawing.Point(89, 36);
			this.rdoValue.Name = "rdoValue";
			this.rdoValue.Size = new System.Drawing.Size(37, 27);
			this.rdoValue.TabIndex = 1;
			this.rdoValue.Text = "x";
			this.rdoValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.rdoValue.UseVisualStyleBackColor = true;
			// 
			// rdoPercent
			// 
			this.rdoPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rdoPercent.Appearance = System.Windows.Forms.Appearance.Button;
			this.rdoPercent.Checked = true;
			this.rdoPercent.Location = new System.Drawing.Point(46, 36);
			this.rdoPercent.Name = "rdoPercent";
			this.rdoPercent.Size = new System.Drawing.Size(37, 27);
			this.rdoPercent.TabIndex = 2;
			this.rdoPercent.TabStop = true;
			this.rdoPercent.Text = "%";
			this.rdoPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.rdoPercent.UseVisualStyleBackColor = true;
			this.rdoPercent.CheckedChanged += new System.EventHandler(this.rdoPercent_CheckedChanged);
			// 
			// btnSmaller
			// 
			this.btnSmaller.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSmaller.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSmaller.Location = new System.Drawing.Point(3, 12);
			this.btnSmaller.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
			this.btnSmaller.Name = "btnSmaller";
			this.tableLayoutPanel1.SetRowSpan(this.btnSmaller, 2);
			this.btnSmaller.Size = new System.Drawing.Size(37, 42);
			this.btnSmaller.TabIndex = 3;
			this.btnSmaller.Text = "-";
			this.btnSmaller.UseVisualStyleBackColor = true;
			this.btnSmaller.Click += new System.EventHandler(this.btnSmaller_Click);
			// 
			// btnBigger
			// 
			this.btnBigger.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBigger.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBigger.Location = new System.Drawing.Point(132, 12);
			this.btnBigger.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
			this.btnBigger.Name = "btnBigger";
			this.tableLayoutPanel1.SetRowSpan(this.btnBigger, 2);
			this.btnBigger.Size = new System.Drawing.Size(39, 42);
			this.btnBigger.TabIndex = 4;
			this.btnBigger.Text = "+";
			this.btnBigger.UseVisualStyleBackColor = true;
			this.btnBigger.Click += new System.EventHandler(this.btnBigger_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.Controls.Add(this.btnSmaller, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.rdoValue, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.nudValue, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.rdoPercent, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnBigger, 3, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(174, 66);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// ctrScale
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(170, 66);
			this.Name = "ctrScale";
			this.Size = new System.Drawing.Size(174, 66);
			((System.ComponentModel.ISupportInitialize)(this.nudValue)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown nudValue;
		private System.Windows.Forms.RadioButton rdoValue;
		private System.Windows.Forms.RadioButton rdoPercent;
		private System.Windows.Forms.Button btnSmaller;
		private System.Windows.Forms.Button btnBigger;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
