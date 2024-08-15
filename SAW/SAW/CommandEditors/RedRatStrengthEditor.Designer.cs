
namespace SAW.CommandEditors
{
	partial class RedRatStrengthEditor
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
			System.Windows.Forms.TableLayoutPanel tblMain;
			this.nud1 = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.nud2 = new System.Windows.Forms.NumericUpDown();
			this.btnTest = new System.Windows.Forms.Button();
			tblMain = new System.Windows.Forms.TableLayoutPanel();
			tblMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nud1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nud2)).BeginInit();
			this.SuspendLayout();
			// 
			// tblMain
			// 
			tblMain.ColumnCount = 2;
			tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			tblMain.Controls.Add(this.nud1, 1, 0);
			tblMain.Controls.Add(this.label2, 0, 0);
			tblMain.Controls.Add(this.label1, 0, 1);
			tblMain.Controls.Add(this.nud2, 1, 1);
			tblMain.Controls.Add(this.btnTest, 1, 2);
			tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
			tblMain.Location = new System.Drawing.Point(0, 0);
			tblMain.Margin = new System.Windows.Forms.Padding(0);
			tblMain.Name = "tblMain";
			tblMain.RowCount = 4;
			tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tblMain.Size = new System.Drawing.Size(373, 316);
			tblMain.TabIndex = 0;
			// 
			// nud1
			// 
			this.nud1.Location = new System.Drawing.Point(189, 3);
			this.nud1.Name = "nud1";
			this.nud1.Size = new System.Drawing.Size(111, 20);
			this.nud1.TabIndex = 4;
			this.nud1.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(180, 26);
			this.label2.TabIndex = 3;
			this.label2.Text = "[SAW_CommandEdit_RedRat_Strength1]";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(180, 26);
			this.label1.TabIndex = 1;
			this.label1.Text = "[SAW_CommandEdit_RedRat_Strength2]";
			// 
			// nud2
			// 
			this.nud2.Location = new System.Drawing.Point(189, 29);
			this.nud2.Name = "nud2";
			this.nud2.Size = new System.Drawing.Size(111, 20);
			this.nud2.TabIndex = 2;
			this.nud2.ValueChanged += new System.EventHandler(this.nud_ValueChanged);
			// 
			// btnTest
			// 
			this.btnTest.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTest.AutoSize = true;
			this.btnTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			tblMain.SetColumnSpan(this.btnTest, 2);
			this.btnTest.Location = new System.Drawing.Point(92, 67);
			this.btnTest.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(189, 23);
			this.btnTest.TabIndex = 5;
			this.btnTest.Text = "[SAW_CommandEdit_RedRat_Test]";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// RedRatStrengthEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(tblMain);
			this.Name = "RedRatStrengthEditor";
			this.Size = new System.Drawing.Size(373, 316);
			tblMain.ResumeLayout(false);
			tblMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nud1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nud2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown nud2;
		private System.Windows.Forms.NumericUpDown nud1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnTest;
	}
}
