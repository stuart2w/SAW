
namespace SAW.CommandEditors
{
	partial class RedRatColourEditor
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
			this.tblMain = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.pnlColour = new System.Windows.Forms.Panel();
			this.btnTest = new System.Windows.Forms.Button();
			this.dlgColour = new System.Windows.Forms.ColorDialog();
			this.tblMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tblMain
			// 
			this.tblMain.ColumnCount = 3;
			this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblMain.Controls.Add(this.label1, 0, 0);
			this.tblMain.Controls.Add(this.pnlColour, 1, 1);
			this.tblMain.Controls.Add(this.btnTest, 1, 2);
			this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblMain.Location = new System.Drawing.Point(0, 0);
			this.tblMain.Margin = new System.Windows.Forms.Padding(0);
			this.tblMain.Name = "tblMain";
			this.tblMain.RowCount = 4;
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblMain.Size = new System.Drawing.Size(298, 297);
			this.tblMain.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tblMain.SetColumnSpan(this.label1, 3);
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(188, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_CommandEdit_RedRat_Colour]";
			// 
			// pnlColour
			// 
			this.pnlColour.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pnlColour.Location = new System.Drawing.Point(49, 16);
			this.pnlColour.Name = "pnlColour";
			this.pnlColour.Size = new System.Drawing.Size(200, 95);
			this.pnlColour.TabIndex = 1;
			this.pnlColour.Click += new System.EventHandler(this.pnlColour_Click);
			// 
			// btnTest
			// 
			this.btnTest.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTest.AutoSize = true;
			this.btnTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnTest.Location = new System.Drawing.Point(54, 129);
			this.btnTest.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(189, 23);
			this.btnTest.TabIndex = 2;
			this.btnTest.Text = "[SAW_CommandEdit_RedRat_Test]";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// RedRatColourEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tblMain);
			this.Name = "RedRatColourEditor";
			this.Size = new System.Drawing.Size(298, 297);
			this.tblMain.ResumeLayout(false);
			this.tblMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tblMain;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel pnlColour;
		private System.Windows.Forms.ColorDialog dlgColour;
		private System.Windows.Forms.Button btnTest;
	}
}
