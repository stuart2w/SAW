
namespace SAW.CommandEditors
{
	partial class RedRatSendEditor
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
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			this.tblMain = new System.Windows.Forms.TableLayoutPanel();
			this.cmbDevice = new System.Windows.Forms.ComboBox();
			this.cmbSignal = new System.Windows.Forms.ComboBox();
			this.btnTest = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			this.tblMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Dock = System.Windows.Forms.DockStyle.Fill;
			label1.Location = new System.Drawing.Point(3, 0);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(142, 27);
			label1.TabIndex = 0;
			label1.Text = "[SAW_CommandEdit_RedRat_Device]";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Dock = System.Windows.Forms.DockStyle.Fill;
			label2.Location = new System.Drawing.Point(3, 27);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(142, 27);
			label2.TabIndex = 1;
			label2.Text = "[SAW_CommandEdit_RedRat_Signal]";
			// 
			// tblMain
			// 
			this.tblMain.ColumnCount = 2;
			this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblMain.Controls.Add(this.btnTest, 0, 2);
			this.tblMain.Controls.Add(label1, 0, 0);
			this.tblMain.Controls.Add(label2, 0, 1);
			this.tblMain.Controls.Add(this.cmbDevice, 1, 0);
			this.tblMain.Controls.Add(this.cmbSignal, 1, 1);
			this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblMain.Location = new System.Drawing.Point(0, 0);
			this.tblMain.Margin = new System.Windows.Forms.Padding(0);
			this.tblMain.Name = "tblMain";
			this.tblMain.RowCount = 4;
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblMain.Size = new System.Drawing.Size(297, 281);
			this.tblMain.TabIndex = 0;
			// 
			// cmbDevice
			// 
			this.cmbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbDevice.FormattingEnabled = true;
			this.cmbDevice.Location = new System.Drawing.Point(151, 3);
			this.cmbDevice.Name = "cmbDevice";
			this.cmbDevice.Size = new System.Drawing.Size(121, 21);
			this.cmbDevice.TabIndex = 2;
			this.cmbDevice.SelectedIndexChanged += new System.EventHandler(this.cmbDevice_SelectedIndexChanged);
			// 
			// cmbSignal
			// 
			this.cmbSignal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSignal.FormattingEnabled = true;
			this.cmbSignal.Location = new System.Drawing.Point(151, 30);
			this.cmbSignal.Name = "cmbSignal";
			this.cmbSignal.Size = new System.Drawing.Size(121, 21);
			this.cmbSignal.TabIndex = 3;
			this.cmbSignal.SelectedIndexChanged += new System.EventHandler(this.cmbSignal_SelectedIndexChanged);
			// 
			// btnTest
			// 
			this.btnTest.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTest.AutoSize = true;
			this.btnTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tblMain.SetColumnSpan(this.btnTest, 2);
			this.btnTest.Location = new System.Drawing.Point(54, 69);
			this.btnTest.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(189, 23);
			this.btnTest.TabIndex = 4;
			this.btnTest.Text = "[SAW_CommandEdit_RedRat_Test]";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// RedRatSendEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tblMain);
			this.Name = "RedRatSendEditor";
			this.Size = new System.Drawing.Size(297, 281);
			this.tblMain.ResumeLayout(false);
			this.tblMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tblMain;
		private System.Windows.Forms.ComboBox cmbDevice;
		private System.Windows.Forms.ComboBox cmbSignal;
		private System.Windows.Forms.Button btnTest;
	}
}
