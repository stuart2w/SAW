namespace SAW
{
	partial class frmEditDesktop
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
			this.btnRemove = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lstPrograms = new System.Windows.Forms.ListView();
			this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRemove.AutoSize = true;
			this.btnRemove.Enabled = false;
			this.btnRemove.Location = new System.Drawing.Point(12, 412);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(140, 28);
			this.btnRemove.TabIndex = 1;
			this.btnRemove.Text = "[SAW_Desktop_Remove]";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(468, 412);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(97, 28);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(580, 412);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(97, 28);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// lstPrograms
			// 
			this.lstPrograms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lstPrograms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.colName,
			this.colPath});
			this.lstPrograms.FullRowSelect = true;
			this.lstPrograms.GridLines = true;
			this.lstPrograms.HideSelection = false;
			this.lstPrograms.Location = new System.Drawing.Point(12, 13);
			this.lstPrograms.Name = "lstPrograms";
			this.lstPrograms.ShowGroups = false;
			this.lstPrograms.Size = new System.Drawing.Size(665, 393);
			this.lstPrograms.TabIndex = 4;
			this.lstPrograms.UseCompatibleStateImageBehavior = false;
			this.lstPrograms.View = System.Windows.Forms.View.Details;
			this.lstPrograms.SelectedIndexChanged += new System.EventHandler(this.lstPrograms_SelectedIndexChanged);
			this.lstPrograms.Resize += new System.EventHandler(this.lstPrograms_Resize);
			// 
			// colName
			// 
			this.colName.Text = "[SAW_Desktop_ProgramHeader]";
			this.colName.Width = 120;
			// 
			// colPath
			// 
			this.colPath.Text = "[SAW_Desktop_PathHeader]";
			this.colPath.Width = 300;
			// 
			// frmEditDesktop
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(689, 449);
			this.ControlBox = false;
			this.Controls.Add(this.lstPrograms);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnRemove);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmEditDesktop";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[SAW_Desktop_EditTitle]";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ListView lstPrograms;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colPath;
	}
}