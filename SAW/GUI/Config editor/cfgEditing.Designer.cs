namespace SAW
{
	partial class cfgEditing
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
			System.Windows.Forms.Label Label31;
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.chkResizeDocToWindow = new SAW.WrappingCheckBox();
			this.chkToolbox = new System.Windows.Forms.CheckBox();
			this.chkOutline = new System.Windows.Forms.CheckBox();
			this.chkTools = new System.Windows.Forms.CheckBox();
			this.chkToolbar = new System.Windows.Forms.CheckBox();
			this.chkPages = new System.Windows.Forms.CheckBox();
			this.chkInfoBar = new System.Windows.Forms.CheckBox();
			this.chkShowHidden = new System.Windows.Forms.CheckBox();
			this.chkMultipleDocuments = new System.Windows.Forms.CheckBox();
			Label31 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label31
			// 
			Label31.AutoSize = true;
			Label31.BackColor = System.Drawing.Color.PowderBlue;
			this.tableLayoutPanel1.SetColumnSpan(Label31, 2);
			Label31.Dock = System.Windows.Forms.DockStyle.Top;
			Label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label31.Location = new System.Drawing.Point(3, 0);
			Label31.Name = "Label31";
			Label31.Size = new System.Drawing.Size(368, 20);
			Label31.TabIndex = 3;
			Label31.Text = "[SAW_Settings_EditorView]";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.chkResizeDocToWindow, 0, 9);
			this.tableLayoutPanel1.Controls.Add(Label31, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.chkToolbox, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.chkOutline, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkTools, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.chkToolbar, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.chkPages, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.chkInfoBar, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.chkShowHidden, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.chkMultipleDocuments, 0, 8);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 11;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(374, 364);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// chkResizeDocToWindow
			// 
			this.chkResizeDocToWindow.AutoSize = true;
			this.chkResizeDocToWindow.Location = new System.Drawing.Point(3, 207);
			this.chkResizeDocToWindow.Name = "chkResizeDocToWindow";
			this.chkResizeDocToWindow.Size = new System.Drawing.Size(172, 17);
			this.chkResizeDocToWindow.TabIndex = 12;
			this.chkResizeDocToWindow.Text = "[Config_ResizeDocToWindow]";
			this.chkResizeDocToWindow.UseVisualStyleBackColor = true;
			this.chkResizeDocToWindow.CheckedChanged += new System.EventHandler(this.chkResizeDocToWindow_CheckedChanged);
			// 
			// chkToolbox
			// 
			this.chkToolbox.AutoSize = true;
			this.chkToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkToolbox.Location = new System.Drawing.Point(3, 23);
			this.chkToolbox.Name = "chkToolbox";
			this.chkToolbox.Size = new System.Drawing.Size(368, 17);
			this.chkToolbox.TabIndex = 4;
			this.chkToolbox.Text = "[SAW_Settings_Show_Toolbox]";
			this.chkToolbox.UseVisualStyleBackColor = true;
			this.chkToolbox.CheckedChanged += new System.EventHandler(this.chkToolbox_CheckedChanged);
			// 
			// chkOutline
			// 
			this.chkOutline.AutoSize = true;
			this.chkOutline.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkOutline.Location = new System.Drawing.Point(3, 46);
			this.chkOutline.Name = "chkOutline";
			this.chkOutline.Size = new System.Drawing.Size(368, 17);
			this.chkOutline.TabIndex = 5;
			this.chkOutline.Text = "[SAW_Settings_Show_Outline]";
			this.chkOutline.UseVisualStyleBackColor = true;
			this.chkOutline.CheckedChanged += new System.EventHandler(this.chkOutline_CheckedChanged);
			// 
			// chkTools
			// 
			this.chkTools.AutoSize = true;
			this.chkTools.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkTools.Location = new System.Drawing.Point(3, 69);
			this.chkTools.Name = "chkTools";
			this.chkTools.Size = new System.Drawing.Size(368, 17);
			this.chkTools.TabIndex = 6;
			this.chkTools.Text = "[Config_DisplayTools]";
			this.chkTools.UseVisualStyleBackColor = true;
			this.chkTools.CheckedChanged += new System.EventHandler(this.chkTools_CheckedChanged);
			// 
			// chkToolbar
			// 
			this.chkToolbar.AutoSize = true;
			this.chkToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkToolbar.Location = new System.Drawing.Point(3, 92);
			this.chkToolbar.Name = "chkToolbar";
			this.chkToolbar.Size = new System.Drawing.Size(368, 17);
			this.chkToolbar.TabIndex = 7;
			this.chkToolbar.Text = "[Config_DisplayToolbar]";
			this.chkToolbar.UseVisualStyleBackColor = true;
			this.chkToolbar.CheckedChanged += new System.EventHandler(this.chkToolbar_CheckedChanged);
			// 
			// chkPages
			// 
			this.chkPages.AutoSize = true;
			this.chkPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkPages.Location = new System.Drawing.Point(3, 115);
			this.chkPages.Name = "chkPages";
			this.chkPages.Size = new System.Drawing.Size(368, 17);
			this.chkPages.TabIndex = 8;
			this.chkPages.Text = "[Config_DisplayPageList]";
			this.chkPages.UseVisualStyleBackColor = true;
			this.chkPages.CheckedChanged += new System.EventHandler(this.chkPages_CheckedChanged);
			// 
			// chkInfoBar
			// 
			this.chkInfoBar.AutoSize = true;
			this.chkInfoBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkInfoBar.Location = new System.Drawing.Point(3, 138);
			this.chkInfoBar.Name = "chkInfoBar";
			this.chkInfoBar.Size = new System.Drawing.Size(368, 17);
			this.chkInfoBar.TabIndex = 9;
			this.chkInfoBar.Text = "[SAW_Settings_Show_InfoBar]";
			this.chkInfoBar.UseVisualStyleBackColor = true;
			this.chkInfoBar.CheckedChanged += new System.EventHandler(this.chkInfoBar_CheckedChanged);
			// 
			// chkShowHidden
			// 
			this.chkShowHidden.AutoSize = true;
			this.chkShowHidden.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkShowHidden.Location = new System.Drawing.Point(3, 161);
			this.chkShowHidden.Name = "chkShowHidden";
			this.chkShowHidden.Size = new System.Drawing.Size(368, 17);
			this.chkShowHidden.TabIndex = 10;
			this.chkShowHidden.Text = "[SAW_Settings_Show_Hidden]";
			this.chkShowHidden.UseVisualStyleBackColor = true;
			this.chkShowHidden.CheckedChanged += new System.EventHandler(this.chkShowHidden_CheckedChanged);
			// 
			// chkMultipleDocuments
			// 
			this.chkMultipleDocuments.AutoSize = true;
			this.chkMultipleDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkMultipleDocuments.Location = new System.Drawing.Point(3, 184);
			this.chkMultipleDocuments.Name = "chkMultipleDocuments";
			this.chkMultipleDocuments.Size = new System.Drawing.Size(368, 17);
			this.chkMultipleDocuments.TabIndex = 11;
			this.chkMultipleDocuments.Text = "[Config_DisplayMultipleDocumentTabs]";
			this.chkMultipleDocuments.UseVisualStyleBackColor = true;
			this.chkMultipleDocuments.Visible = false;
			this.chkMultipleDocuments.CheckedChanged += new System.EventHandler(this.chkMultipleDocuments_CheckedChanged);
			// 
			// cfgEditing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "cfgEditing";
			this.Size = new System.Drawing.Size(374, 364);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.CheckBox chkToolbox;
		private System.Windows.Forms.CheckBox chkOutline;
		private System.Windows.Forms.CheckBox chkTools;
		private System.Windows.Forms.CheckBox chkToolbar;
		private System.Windows.Forms.CheckBox chkPages;
		private System.Windows.Forms.CheckBox chkInfoBar;
		private System.Windows.Forms.CheckBox chkShowHidden;
		private System.Windows.Forms.CheckBox chkMultipleDocuments;
		private SAW.WrappingCheckBox chkResizeDocToWindow;
	}
}
