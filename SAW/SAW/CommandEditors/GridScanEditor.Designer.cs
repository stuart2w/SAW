namespace SAW.CommandEditors
{
	partial class GridScanEditor
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtRows = new SAW.CommandEditors.IntegerTextBox();
			this.txtColumns = new SAW.CommandEditors.IntegerTextBox();
			this.lblNeedAutoRepeat = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtRows, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtColumns, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblNeedAutoRepeat, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(301, 304);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(142, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_CommandEdit_Rows]";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(141, 26);
			this.label2.TabIndex = 1;
			this.label2.Text = "[SAW_CommandEdit_Columns]";
			// 
			// txtRows
			// 
			this.txtRows.Location = new System.Drawing.Point(153, 3);
			this.txtRows.Name = "txtRows";
			this.txtRows.Size = new System.Drawing.Size(100, 20);
			this.txtRows.TabIndex = 2;
			this.txtRows.Value = 0;
			this.txtRows.TextChanged += new System.EventHandler(this.txtRows_TextChanged);
			// 
			// txtColumns
			// 
			this.txtColumns.Location = new System.Drawing.Point(153, 29);
			this.txtColumns.Name = "txtColumns";
			this.txtColumns.Size = new System.Drawing.Size(100, 20);
			this.txtColumns.TabIndex = 3;
			this.txtColumns.Value = 0;
			this.txtColumns.TextChanged += new System.EventHandler(this.txtColumns_TextChanged);
			// 
			// lblNeedAutoRepeat
			// 
			this.lblNeedAutoRepeat.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblNeedAutoRepeat, 2);
			this.lblNeedAutoRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblNeedAutoRepeat.ForeColor = System.Drawing.Color.Red;
			this.lblNeedAutoRepeat.Location = new System.Drawing.Point(3, 52);
			this.lblNeedAutoRepeat.Name = "lblNeedAutoRepeat";
			this.lblNeedAutoRepeat.Size = new System.Drawing.Size(295, 13);
			this.lblNeedAutoRepeat.TabIndex = 4;
			this.lblNeedAutoRepeat.Text = "[SAW_CommandEdit_NeedsAutoRepeat]";
			this.lblNeedAutoRepeat.Visible = false;
			// 
			// GridScanEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "GridScanEditor";
			this.Size = new System.Drawing.Size(301, 304);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private IntegerTextBox txtRows;
		private IntegerTextBox txtColumns;
		private System.Windows.Forms.Label lblNeedAutoRepeat;
	}
}
