using SAW.CommandEditors;

namespace SAW
{
	internal partial class ctrCoordEdit
	{
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableMain = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txt1X = new SAW.CommandEditors.IntegerTextBox();
			this.txt1Y = new SAW.CommandEditors.IntegerTextBox();
			this.txt2Y = new SAW.CommandEditors.IntegerTextBox();
			this.txt2X = new SAW.CommandEditors.IntegerTextBox();
			this.txt3X = new SAW.CommandEditors.IntegerTextBox();
			this.txt3Y = new SAW.CommandEditors.IntegerTextBox();
			this.txt4X = new SAW.CommandEditors.IntegerTextBox();
			this.txt4Y = new SAW.CommandEditors.IntegerTextBox();
			this.lblSelect = new System.Windows.Forms.Label();
			this.tmrTyping = new System.Windows.Forms.Timer(this.components);
			this.tmrRefill = new System.Windows.Forms.Timer(this.components);
			this.tableMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableMain
			// 
			this.tableMain.ColumnCount = 3;
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableMain.Controls.Add(this.label1, 0, 0);
			this.tableMain.Controls.Add(this.label2, 0, 1);
			this.tableMain.Controls.Add(this.label3, 0, 2);
			this.tableMain.Controls.Add(this.label4, 0, 3);
			this.tableMain.Controls.Add(this.txt1X, 1, 0);
			this.tableMain.Controls.Add(this.txt1Y, 2, 0);
			this.tableMain.Controls.Add(this.txt2Y, 2, 1);
			this.tableMain.Controls.Add(this.txt2X, 1, 1);
			this.tableMain.Controls.Add(this.txt3X, 1, 2);
			this.tableMain.Controls.Add(this.txt3Y, 2, 2);
			this.tableMain.Controls.Add(this.txt4X, 1, 3);
			this.tableMain.Controls.Add(this.txt4Y, 2, 3);
			this.tableMain.Controls.Add(this.lblSelect, 0, 4);
			this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableMain.Location = new System.Drawing.Point(0, 0);
			this.tableMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableMain.Name = "tableMain";
			this.tableMain.RowCount = 6;
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableMain.Size = new System.Drawing.Size(226, 186);
			this.tableMain.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(116, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(116, 26);
			this.label2.TabIndex = 1;
			this.label2.Text = "label2";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Visible = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 52);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 26);
			this.label3.TabIndex = 2;
			this.label3.Text = "label3";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label3.Visible = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 78);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(116, 26);
			this.label4.TabIndex = 3;
			this.label4.Text = "label4";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label4.Visible = false;
			// 
			// txt1X
			// 
			this.txt1X.Location = new System.Drawing.Point(125, 3);
			this.txt1X.Name = "txt1X";
			this.txt1X.Size = new System.Drawing.Size(46, 20);
			this.txt1X.TabIndex = 4;
			this.txt1X.Text = "0";
			this.txt1X.Value = 0;
			this.txt1X.Visible = false;
			// 
			// txt1Y
			// 
			this.txt1Y.Location = new System.Drawing.Point(177, 3);
			this.txt1Y.Name = "txt1Y";
			this.txt1Y.Size = new System.Drawing.Size(46, 20);
			this.txt1Y.TabIndex = 5;
			this.txt1Y.Text = "0";
			this.txt1Y.Value = 0;
			this.txt1Y.Visible = false;
			// 
			// txt2Y
			// 
			this.txt2Y.Location = new System.Drawing.Point(177, 29);
			this.txt2Y.Name = "txt2Y";
			this.txt2Y.Size = new System.Drawing.Size(46, 20);
			this.txt2Y.TabIndex = 7;
			this.txt2Y.Text = "0";
			this.txt2Y.Value = 0;
			this.txt2Y.Visible = false;
			// 
			// txt2X
			// 
			this.txt2X.Location = new System.Drawing.Point(125, 29);
			this.txt2X.Name = "txt2X";
			this.txt2X.Size = new System.Drawing.Size(46, 20);
			this.txt2X.TabIndex = 6;
			this.txt2X.Text = "0";
			this.txt2X.Value = 0;
			this.txt2X.Visible = false;
			// 
			// txt3X
			// 
			this.txt3X.Location = new System.Drawing.Point(125, 55);
			this.txt3X.Name = "txt3X";
			this.txt3X.Size = new System.Drawing.Size(46, 20);
			this.txt3X.TabIndex = 8;
			this.txt3X.Text = "0";
			this.txt3X.Value = 0;
			this.txt3X.Visible = false;
			// 
			// txt3Y
			// 
			this.txt3Y.Location = new System.Drawing.Point(177, 55);
			this.txt3Y.Name = "txt3Y";
			this.txt3Y.Size = new System.Drawing.Size(46, 20);
			this.txt3Y.TabIndex = 9;
			this.txt3Y.Text = "0";
			this.txt3Y.Value = 0;
			this.txt3Y.Visible = false;
			// 
			// txt4X
			// 
			this.txt4X.Location = new System.Drawing.Point(125, 81);
			this.txt4X.Name = "txt4X";
			this.txt4X.Size = new System.Drawing.Size(46, 20);
			this.txt4X.TabIndex = 10;
			this.txt4X.Text = "0";
			this.txt4X.Value = 0;
			this.txt4X.Visible = false;
			// 
			// txt4Y
			// 
			this.txt4Y.Location = new System.Drawing.Point(177, 81);
			this.txt4Y.Name = "txt4Y";
			this.txt4Y.Size = new System.Drawing.Size(46, 20);
			this.txt4Y.TabIndex = 11;
			this.txt4Y.Text = "0";
			this.txt4Y.Value = 0;
			this.txt4Y.Visible = false;
			// 
			// lblSelect
			// 
			this.lblSelect.AutoSize = true;
			this.tableMain.SetColumnSpan(this.lblSelect, 3);
			this.lblSelect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSelect.Location = new System.Drawing.Point(3, 104);
			this.lblSelect.Name = "lblSelect";
			this.lblSelect.Size = new System.Drawing.Size(220, 13);
			this.lblSelect.TabIndex = 12;
			this.lblSelect.Text = "[SAW_Coord_Select]";
			this.lblSelect.Visible = false;
			// 
			// tmrTyping
			// 
			this.tmrTyping.Interval = 400;
			this.tmrTyping.Tick += new System.EventHandler(this.tmrTyping_Tick);
			// 
			// tmrRefill
			// 
			this.tmrRefill.Interval = 400;
			this.tmrRefill.Tick += new System.EventHandler(this.tmrRefill_Tick);
			// 
			// ctrCoordEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableMain);
			this.MinimumSize = new System.Drawing.Size(200, 145);
			this.Name = "ctrCoordEdit";
			this.Size = new System.Drawing.Size(226, 186);
			this.tableMain.ResumeLayout(false);
			this.tableMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableMain;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private IntegerTextBox txt1X;
		private IntegerTextBox txt1Y;
		private IntegerTextBox txt2Y;
		private IntegerTextBox txt2X;
		private IntegerTextBox txt3X;
		private IntegerTextBox txt3Y;
		private IntegerTextBox txt4X;
		private IntegerTextBox txt4Y;
		private System.Windows.Forms.Label lblSelect;
		private System.Windows.Forms.Timer tmrTyping;
		private System.Windows.Forms.Timer tmrRefill;
	}
}
