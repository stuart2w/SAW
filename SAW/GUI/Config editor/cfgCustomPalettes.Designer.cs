namespace SAW
{
	public partial class cfgCustomPalettes : ConfigPage
	{
		
		//UserControl overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.Windows.Forms.Label Label31;
			System.Windows.Forms.Label Label32;
			this.pnlPalettes = new System.Windows.Forms.TableLayoutPanel();
			this.btnRename = new System.Windows.Forms.Button();
			this.lblPalettesTabsRequired = new System.Windows.Forms.Label();
			this.lnkPalettesTabsChange = new System.Windows.Forms.LinkLabel();
			this.lblPalettesHeading = new System.Windows.Forms.Label();
			this.lstPalettes = new System.Windows.Forms.ListBox();
			this.btnCreate = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnEdit = new System.Windows.Forms.Button();
			this.chkShowSystem = new System.Windows.Forms.CheckBox();
			this.btnDuplicate = new System.Windows.Forms.Button();
			Label31 = new System.Windows.Forms.Label();
			Label32 = new System.Windows.Forms.Label();
			this.pnlPalettes.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label31
			// 
			Label31.AutoSize = true;
			Label31.BackColor = System.Drawing.Color.PowderBlue;
			this.pnlPalettes.SetColumnSpan(Label31, 2);
			Label31.Dock = System.Windows.Forms.DockStyle.Top;
			Label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label31.Location = new System.Drawing.Point(3, 0);
			Label31.Name = "Label31";
			Label31.Size = new System.Drawing.Size(544, 20);
			Label31.TabIndex = 2;
			Label31.Text = "[Config_Palettes]";
			// 
			// Label32
			// 
			Label32.AutoSize = true;
			this.pnlPalettes.SetColumnSpan(Label32, 2);
			Label32.Location = new System.Drawing.Point(3, 20);
			Label32.Name = "Label32";
			Label32.Size = new System.Drawing.Size(108, 13);
			Label32.TabIndex = 3;
			Label32.Text = "[Config_PalettesIntro]";
			// 
			// pnlPalettes
			// 
			this.pnlPalettes.ColumnCount = 2;
			this.pnlPalettes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlPalettes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlPalettes.Controls.Add(this.btnRename, 1, 9);
			this.pnlPalettes.Controls.Add(Label31, 0, 0);
			this.pnlPalettes.Controls.Add(Label32, 0, 1);
			this.pnlPalettes.Controls.Add(this.lblPalettesTabsRequired, 0, 2);
			this.pnlPalettes.Controls.Add(this.lnkPalettesTabsChange, 0, 3);
			this.pnlPalettes.Controls.Add(this.lblPalettesHeading, 0, 4);
			this.pnlPalettes.Controls.Add(this.lstPalettes, 0, 6);
			this.pnlPalettes.Controls.Add(this.btnCreate, 1, 6);
			this.pnlPalettes.Controls.Add(this.btnDelete, 1, 10);
			this.pnlPalettes.Controls.Add(this.btnEdit, 1, 7);
			this.pnlPalettes.Controls.Add(this.chkShowSystem, 0, 12);
			this.pnlPalettes.Controls.Add(this.btnDuplicate, 1, 8);
			this.pnlPalettes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlPalettes.Location = new System.Drawing.Point(0, 0);
			this.pnlPalettes.Margin = new System.Windows.Forms.Padding(0);
			this.pnlPalettes.Name = "pnlPalettes";
			this.pnlPalettes.RowCount = 13;
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.pnlPalettes.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlPalettes.Size = new System.Drawing.Size(550, 509);
			this.pnlPalettes.TabIndex = 66;
			// 
			// btnRename
			// 
			this.btnRename.AutoSize = true;
			this.btnRename.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnRename.Location = new System.Drawing.Point(278, 174);
			this.btnRename.Name = "btnRename";
			this.btnRename.Size = new System.Drawing.Size(63, 23);
			this.btnRename.TabIndex = 11;
			this.btnRename.Text = "[Rename]";
			this.btnRename.UseVisualStyleBackColor = true;
			this.btnRename.Click += new System.EventHandler(this.btnPaletteRename_Click);
			// 
			// lblPalettesTabsRequired
			// 
			this.lblPalettesTabsRequired.AutoSize = true;
			this.pnlPalettes.SetColumnSpan(this.lblPalettesTabsRequired, 2);
			this.lblPalettesTabsRequired.Location = new System.Drawing.Point(3, 33);
			this.lblPalettesTabsRequired.Name = "lblPalettesTabsRequired";
			this.lblPalettesTabsRequired.Size = new System.Drawing.Size(158, 13);
			this.lblPalettesTabsRequired.TabIndex = 4;
			this.lblPalettesTabsRequired.Text = "[Config_PalettesMultiDocument]";
			// 
			// lnkPalettesTabsChange
			// 
			this.lnkPalettesTabsChange.AutoSize = true;
			this.pnlPalettes.SetColumnSpan(this.lnkPalettesTabsChange, 2);
			this.lnkPalettesTabsChange.Location = new System.Drawing.Point(3, 46);
			this.lnkPalettesTabsChange.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this.lnkPalettesTabsChange.Name = "lnkPalettesTabsChange";
			this.lnkPalettesTabsChange.Size = new System.Drawing.Size(195, 13);
			this.lnkPalettesTabsChange.TabIndex = 5;
			this.lnkPalettesTabsChange.TabStop = true;
			this.lnkPalettesTabsChange.Text = "[Config_PalettesMultiDocumentChange]";
			this.lnkPalettesTabsChange.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkPalettesTabsChange_LinkClicked);
			// 
			// lblPalettesHeading
			// 
			this.lblPalettesHeading.AutoSize = true;
			this.pnlPalettes.SetColumnSpan(this.lblPalettesHeading, 2);
			this.lblPalettesHeading.Location = new System.Drawing.Point(3, 71);
			this.lblPalettesHeading.Name = "lblPalettesHeading";
			this.lblPalettesHeading.Size = new System.Drawing.Size(123, 13);
			this.lblPalettesHeading.TabIndex = 6;
			this.lblPalettesHeading.Text = "[Config_PalettesExisting]";
			// 
			// lstPalettes
			// 
			this.lstPalettes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstPalettes.FormattingEnabled = true;
			this.lstPalettes.IntegralHeight = false;
			this.lstPalettes.Location = new System.Drawing.Point(3, 87);
			this.lstPalettes.Name = "lstPalettes";
			this.pnlPalettes.SetRowSpan(this.lstPalettes, 6);
			this.lstPalettes.Size = new System.Drawing.Size(269, 396);
			this.lstPalettes.TabIndex = 7;
			this.lstPalettes.SelectedIndexChanged += new System.EventHandler(this.lstPalettes_SelectedIndexChanged);
			// 
			// btnCreate
			// 
			this.btnCreate.AutoSize = true;
			this.btnCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnCreate.Location = new System.Drawing.Point(278, 87);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new System.Drawing.Size(128, 23);
			this.btnCreate.TabIndex = 8;
			this.btnCreate.Text = "[Config_PalettesCreate]";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new System.EventHandler(this.btnPaletteCreate_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.AutoSize = true;
			this.btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnDelete.Location = new System.Drawing.Point(278, 203);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(128, 23);
			this.btnDelete.TabIndex = 9;
			this.btnDelete.Text = "[Config_PalettesDelete]";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnPaletteDelete_Click);
			// 
			// btnEdit
			// 
			this.btnEdit.AutoSize = true;
			this.btnEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnEdit.Location = new System.Drawing.Point(278, 116);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(80, 23);
			this.btnEdit.TabIndex = 10;
			this.btnEdit.Text = "[Palette_Edit]";
			this.btnEdit.UseVisualStyleBackColor = true;
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// chkShowSystem
			// 
			this.chkShowSystem.AutoSize = true;
			this.chkShowSystem.Location = new System.Drawing.Point(3, 489);
			this.chkShowSystem.Name = "chkShowSystem";
			this.chkShowSystem.Size = new System.Drawing.Size(167, 17);
			this.chkShowSystem.TabIndex = 13;
			this.chkShowSystem.Text = "[Config_PalettesShowSystem]";
			this.chkShowSystem.UseVisualStyleBackColor = true;
			this.chkShowSystem.CheckedChanged += new System.EventHandler(this.chkShowSystem_CheckedChanged);
			// 
			// btnDuplicate
			// 
			this.btnDuplicate.AutoSize = true;
			this.btnDuplicate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnDuplicate.Location = new System.Drawing.Point(278, 145);
			this.btnDuplicate.Name = "btnDuplicate";
			this.btnDuplicate.Size = new System.Drawing.Size(139, 23);
			this.btnDuplicate.TabIndex = 14;
			this.btnDuplicate.Text = "[Config_PalettesEditCopy]";
			this.btnDuplicate.UseVisualStyleBackColor = true;
			this.btnDuplicate.Click += new System.EventHandler(this.btnDuplicate_Click);
			// 
			// cfgCustomPalettes
			// 
			this.Controls.Add(this.pnlPalettes);
			this.Name = "cfgCustomPalettes";
			this.Size = new System.Drawing.Size(550, 509);
			this.pnlPalettes.ResumeLayout(false);
			this.pnlPalettes.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.TableLayoutPanel pnlPalettes;
		private System.Windows.Forms.Button btnRename;
		private System.Windows.Forms.Label lblPalettesTabsRequired;
		private System.Windows.Forms.LinkLabel lnkPalettesTabsChange;
		private System.Windows.Forms.Label lblPalettesHeading;
		private System.Windows.Forms.ListBox lstPalettes;
		private System.Windows.Forms.Button btnCreate;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnDuplicate;
		private System.Windows.Forms.CheckBox chkShowSystem;
		
	}
	
}
