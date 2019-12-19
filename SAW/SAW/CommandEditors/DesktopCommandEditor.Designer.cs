namespace SAW.CommandEditors
{
	partial class DesktopCommandEditor
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
			this.txtFile = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
			this.lblMissing = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtFile, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnBrowse, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblMissing, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(445, 246);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(439, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_CommandEdit_DesktopFile]";
			// 
			// txtFile
			// 
			this.txtFile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtFile.Location = new System.Drawing.Point(3, 16);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(320, 20);
			this.txtFile.TabIndex = 1;
			this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.AutoSize = true;
			this.btnBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnBrowse.Location = new System.Drawing.Point(329, 16);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(113, 23);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "[SAW_Edit_Browse]";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// dlgOpen
			// 
			this.dlgOpen.FileName = "openFileDialog1";
			// 
			// lblMissing
			// 
			this.lblMissing.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblMissing, 2);
			this.lblMissing.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblMissing.ForeColor = System.Drawing.Color.Red;
			this.lblMissing.Location = new System.Drawing.Point(3, 42);
			this.lblMissing.Name = "lblMissing";
			this.lblMissing.Size = new System.Drawing.Size(439, 13);
			this.lblMissing.TabIndex = 3;
			this.lblMissing.Text = "[SAW_CommandEdit_DesktopMissingFile]";
			this.lblMissing.Visible = false;
			// 
			// DesktopCommandEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "DesktopCommandEditor";
			this.Size = new System.Drawing.Size(445, 246);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.OpenFileDialog dlgOpen;
		private System.Windows.Forms.Label lblMissing;
	}
}
