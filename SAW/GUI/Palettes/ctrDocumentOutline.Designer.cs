namespace SAW
{
	partial class ctrDocumentOutline
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.lblTop = new System.Windows.Forms.Label();
			this.txtSearch = new SAW.TextBoxAllKeys();
			this.tvOutline = new SAW.TreeViewWithMove();
			this.imlItems = new System.Windows.Forms.ImageList(this.components);
			this.tmrSearch = new System.Windows.Forms.Timer(this.components);
			this.ctxEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.verbCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbCutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuFrontBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbSendBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbBringFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbSendBackOneStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbBringFrontOneStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.verbShowPopupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbHidePopupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbShowAllPopupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verbHideAllPopupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.verbDoubleClickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtInfo = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.picSizeGrip = new System.Windows.Forms.PictureBox();
			this.verbPasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ctxEdit.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picSizeGrip)).BeginInit();
			this.SuspendLayout();
			// 
			// lblTop
			// 
			this.lblTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblTop.AutoSize = true;
			this.lblTop.Location = new System.Drawing.Point(3, 0);
			this.lblTop.Name = "lblTop";
			this.lblTop.Size = new System.Drawing.Size(86, 26);
			this.lblTop.TabIndex = 0;
			this.lblTop.Text = "[Outline_Search]";
			this.lblTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtSearch
			// 
			this.txtSearch.AcceptsReturn = true;
			this.tableLayoutPanel1.SetColumnSpan(this.txtSearch, 2);
			this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSearch.Location = new System.Drawing.Point(95, 3);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(179, 20);
			this.txtSearch.TabIndex = 1;
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
			// 
			// tvOutline
			// 
			this.tvOutline.AllowDrop = true;
			this.tableLayoutPanel1.SetColumnSpan(this.tvOutline, 3);
			this.tvOutline.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvOutline.ImageIndex = 0;
			this.tvOutline.ImageList = this.imlItems;
			this.tvOutline.Location = new System.Drawing.Point(3, 26);
			this.tvOutline.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.tvOutline.Name = "tvOutline";
			this.tvOutline.SelectedImageIndex = 0;
			this.tvOutline.Size = new System.Drawing.Size(271, 248);
			this.tvOutline.TabIndex = 1;
			this.tvOutline.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvOutline_AfterSelect);
			this.tvOutline.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvOutline_MouseDown);
			this.tvOutline.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tvOutline_MouseMove);
			this.tvOutline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tvOutline_MouseUp);
			// 
			// imlItems
			// 
			this.imlItems.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imlItems.ImageSize = new System.Drawing.Size(16, 16);
			this.imlItems.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// tmrSearch
			// 
			this.tmrSearch.Interval = 500;
			this.tmrSearch.Tick += new System.EventHandler(this.tmrSearch_Tick);
			// 
			// ctxEdit
			// 
			this.ctxEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbCopyToolStripMenuItem,
            this.verbCutToolStripMenuItem,
            this.verbPasteToolStripMenuItem,
            this.verbDeleteToolStripMenuItem,
            this.toolStripMenuItem2,
            this.menuFrontBackToolStripMenuItem,
            this.toolStripMenuItem1,
            this.verbShowPopupToolStripMenuItem,
            this.verbHidePopupToolStripMenuItem,
            this.verbShowAllPopupsToolStripMenuItem,
            this.verbHideAllPopupsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.verbDoubleClickToolStripMenuItem});
			this.ctxEdit.Name = "ctxEdit";
			this.ctxEdit.Size = new System.Drawing.Size(194, 264);
			// 
			// verbCopyToolStripMenuItem
			// 
			this.verbCopyToolStripMenuItem.Name = "verbCopyToolStripMenuItem";
			this.verbCopyToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbCopyToolStripMenuItem.Text = "[Verb_Copy]";
			// 
			// verbCutToolStripMenuItem
			// 
			this.verbCutToolStripMenuItem.Name = "verbCutToolStripMenuItem";
			this.verbCutToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbCutToolStripMenuItem.Text = "[Verb_Cut]";
			// 
			// verbDeleteToolStripMenuItem
			// 
			this.verbDeleteToolStripMenuItem.Name = "verbDeleteToolStripMenuItem";
			this.verbDeleteToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbDeleteToolStripMenuItem.Text = "[Verb_Delete]";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(190, 6);
			// 
			// menuFrontBackToolStripMenuItem
			// 
			this.menuFrontBackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verbSendBackToolStripMenuItem,
            this.verbBringFrontToolStripMenuItem,
            this.verbSendBackOneStepToolStripMenuItem,
            this.verbBringFrontOneStepToolStripMenuItem});
			this.menuFrontBackToolStripMenuItem.Name = "menuFrontBackToolStripMenuItem";
			this.menuFrontBackToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.menuFrontBackToolStripMenuItem.Text = "[Menu_FrontBack]";
			// 
			// verbSendBackToolStripMenuItem
			// 
			this.verbSendBackToolStripMenuItem.Name = "verbSendBackToolStripMenuItem";
			this.verbSendBackToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.verbSendBackToolStripMenuItem.Text = "[Verb_SendBack]";
			// 
			// verbBringFrontToolStripMenuItem
			// 
			this.verbBringFrontToolStripMenuItem.Name = "verbBringFrontToolStripMenuItem";
			this.verbBringFrontToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.verbBringFrontToolStripMenuItem.Text = "[Verb_BringFront]";
			// 
			// verbSendBackOneStepToolStripMenuItem
			// 
			this.verbSendBackOneStepToolStripMenuItem.Name = "verbSendBackOneStepToolStripMenuItem";
			this.verbSendBackOneStepToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.verbSendBackOneStepToolStripMenuItem.Text = "[Verb_SendBackOneStep]";
			// 
			// verbBringFrontOneStepToolStripMenuItem
			// 
			this.verbBringFrontOneStepToolStripMenuItem.Name = "verbBringFrontOneStepToolStripMenuItem";
			this.verbBringFrontOneStepToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
			this.verbBringFrontOneStepToolStripMenuItem.Text = "[Verb_BringFrontOneStep]";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(190, 6);
			// 
			// verbShowPopupToolStripMenuItem
			// 
			this.verbShowPopupToolStripMenuItem.Name = "verbShowPopupToolStripMenuItem";
			this.verbShowPopupToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbShowPopupToolStripMenuItem.Text = "[Verb_ShowPopup]";
			// 
			// verbHidePopupToolStripMenuItem
			// 
			this.verbHidePopupToolStripMenuItem.Name = "verbHidePopupToolStripMenuItem";
			this.verbHidePopupToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbHidePopupToolStripMenuItem.Text = "[Verb_HidePopup]";
			// 
			// verbShowAllPopupsToolStripMenuItem
			// 
			this.verbShowAllPopupsToolStripMenuItem.Name = "verbShowAllPopupsToolStripMenuItem";
			this.verbShowAllPopupsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbShowAllPopupsToolStripMenuItem.Text = "[Verb_ShowAllPopups]";
			// 
			// verbHideAllPopupsToolStripMenuItem
			// 
			this.verbHideAllPopupsToolStripMenuItem.Name = "verbHideAllPopupsToolStripMenuItem";
			this.verbHideAllPopupsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbHideAllPopupsToolStripMenuItem.Text = "[Verb_HideAllPopups]";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(190, 6);
			// 
			// verbDoubleClickToolStripMenuItem
			// 
			this.verbDoubleClickToolStripMenuItem.Name = "verbDoubleClickToolStripMenuItem";
			this.verbDoubleClickToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbDoubleClickToolStripMenuItem.Text = "[Verb_DoubleClick]";
			// 
			// txtInfo
			// 
			this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel1.SetColumnSpan(this.txtInfo, 2);
			this.txtInfo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.txtInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.txtInfo.Location = new System.Drawing.Point(2, 285);
			this.txtInfo.Margin = new System.Windows.Forms.Padding(2);
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ReadOnly = true;
			this.txtInfo.Size = new System.Drawing.Size(253, 13);
			this.txtInfo.TabIndex = 2;
			this.txtInfo.Click += new System.EventHandler(this.txtInfo_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Controls.Add(this.txtSearch, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblTop, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tvOutline, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtInfo, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.picSizeGrip, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(277, 300);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// picSizeGrip
			// 
			this.picSizeGrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picSizeGrip.Image = global::Resources.AM.SizeGrip_24;
			this.picSizeGrip.Location = new System.Drawing.Point(261, 278);
			this.picSizeGrip.Margin = new System.Windows.Forms.Padding(4, 4, 0, 0);
			this.picSizeGrip.Name = "picSizeGrip";
			this.picSizeGrip.Size = new System.Drawing.Size(16, 22);
			this.picSizeGrip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picSizeGrip.TabIndex = 3;
			this.picSizeGrip.TabStop = false;
			this.picSizeGrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picSizeGrip_MouseDown);
			this.picSizeGrip.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picSizeGrip_MouseMove);
			this.picSizeGrip.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picSizeGrip_MouseUp);
			// 
			// verbPasteToolStripMenuItem
			// 
			this.verbPasteToolStripMenuItem.Name = "verbPasteToolStripMenuItem";
			this.verbPasteToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.verbPasteToolStripMenuItem.Text = "[Verb_Paste]";
			// 
			// ctrDocumentOutline
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(100, 300);
			this.Name = "ctrDocumentOutline";
			this.Size = new System.Drawing.Size(277, 300);
			this.ctxEdit.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picSizeGrip)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblTop;
		private TreeViewWithMove tvOutline;
		private TextBoxAllKeys txtSearch;
		private System.Windows.Forms.Timer tmrSearch;
		private System.Windows.Forms.ImageList imlItems;
		private System.Windows.Forms.ContextMenuStrip ctxEdit;
		private System.Windows.Forms.ToolStripMenuItem verbCopyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbCutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbDeleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem verbShowPopupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbHidePopupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbDoubleClickToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbShowAllPopupsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbHideAllPopupsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.PictureBox picSizeGrip;
		private System.Windows.Forms.ToolStripMenuItem menuFrontBackToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbSendBackToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbBringFrontToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbSendBackOneStepToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verbBringFrontOneStepToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem verbPasteToolStripMenuItem;
	}
}
