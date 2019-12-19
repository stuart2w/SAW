namespace SAW.CommandEditors
{
	partial class PopupItemEditor
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
			this.lblTop = new System.Windows.Forms.Label();
			this.cmbPopups = new System.Windows.Forms.ComboBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTop
			// 
			this.lblTop.AutoSize = true;
			this.lblTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTop.Location = new System.Drawing.Point(3, 0);
			this.lblTop.Name = "lblTop";
			this.lblTop.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.lblTop.Size = new System.Drawing.Size(274, 16);
			this.lblTop.TabIndex = 0;
			this.lblTop.Text = "[SAW_CommandEdit_PopupItem]";
			// 
			// cmbPopups
			// 
			this.cmbPopups.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbPopups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPopups.FormattingEnabled = true;
			this.cmbPopups.Location = new System.Drawing.Point(3, 19);
			this.cmbPopups.Name = "cmbPopups";
			this.cmbPopups.Size = new System.Drawing.Size(274, 21);
			this.cmbPopups.TabIndex = 1;
			this.cmbPopups.SelectedIndexChanged += new System.EventHandler(this.cmbPopups_SelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.lblTop, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.cmbPopups, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(280, 294);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// PopupItemEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "PopupItemEditor";
			this.Size = new System.Drawing.Size(280, 294);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblTop;
		private System.Windows.Forms.ComboBox cmbPopups;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
