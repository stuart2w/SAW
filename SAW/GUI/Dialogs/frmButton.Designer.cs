using System;


namespace SAW
{
	public partial class frmButton : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
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
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.Label Label2;
			System.Windows.Forms.Label Label3;
			SAW.Functions.NullAction nullAction1 = new SAW.Functions.NullAction();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.Label1 = new System.Windows.Forms.Label();
			this.cmbStyle = new System.Windows.Forms.ComboBox();
			this.lblExplain = new System.Windows.Forms.Label();
			this.lnkShare = new System.Windows.Forms.LinkLabel();
			this.tcMain = new System.Windows.Forms.TabControl();
			this.tpContent = new System.Windows.Forms.TabPage();
			this.tblContent = new System.Windows.Forms.TableLayoutPanel();
			this.btnResource = new System.Windows.Forms.Button();
			this.Label9 = new System.Windows.Forms.Label();
			this.Label7 = new System.Windows.Forms.Label();
			this.txtText = new System.Windows.Forms.TextBox();
			this.Label8 = new System.Windows.Forms.Label();
			this.btnImageDisc = new System.Windows.Forms.Button();
			this.pnlImagePreview = new SAW.PreviewPanel();
			this.btnImageRemove = new System.Windows.Forms.Button();
			this.lblTextRatio = new System.Windows.Forms.Label();
			this.pnlTextRatio = new System.Windows.Forms.Panel();
			this.sldTextRatio = new System.Windows.Forms.TrackBar();
			this.rdoTextRatioSet = new System.Windows.Forms.RadioButton();
			this.rdoTextRatioDefault = new System.Windows.Forms.RadioButton();
			this.chkSuperimpose = new System.Windows.Forms.CheckBox();
			this.pnlLayout = new SAW.SelectionPanel();
			this.tpStyle = new System.Windows.Forms.TabPage();
			this.ctrStyle = new SAW.ctrButtonStyleEdit();
			this.lnkDefaultStyle = new System.Windows.Forms.LinkLabel();
			this.tpAction = new System.Windows.Forms.TabPage();
			this.tblAction = new System.Windows.Forms.TableLayoutPanel();
			this.ctrActions = new SAW.ctrActionSelect();
			this.chkDisplayFromAction = new System.Windows.Forms.CheckBox();
			this.pnlActionSelected = new System.Windows.Forms.FlowLayoutPanel();
			this.lblActionSelected = new System.Windows.Forms.Label();
			this.ctrActionColour = new SAW.ColourPicker();
			this.txtActionKey = new SAW.TextBoxAllKeys();
			this.chkActionKeyAuto = new System.Windows.Forms.CheckBox();
			this.mnuImage = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mnuExportImage = new System.Windows.Forms.ToolStripMenuItem();
			Label2 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			this.tcMain.SuspendLayout();
			this.tpContent.SuspendLayout();
			this.tblContent.SuspendLayout();
			this.pnlTextRatio.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sldTextRatio)).BeginInit();
			this.tpStyle.SuspendLayout();
			this.tpAction.SuspendLayout();
			this.tblAction.SuspendLayout();
			this.pnlActionSelected.SuspendLayout();
			this.mnuImage.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label2
			// 
			Label2.AutoSize = true;
			Label2.Location = new System.Drawing.Point(3, 0);
			Label2.Name = "Label2";
			Label2.Size = new System.Drawing.Size(149, 17);
			Label2.TabIndex = 1;
			Label2.Text = "[Button_ActionPrompt]";
			// 
			// Label3
			// 
			Label3.AutoSize = true;
			this.tblContent.SetColumnSpan(Label3, 3);
			Label3.Dock = System.Windows.Forms.DockStyle.Fill;
			Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label3.Location = new System.Drawing.Point(3, 19);
			Label3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(493, 15);
			Label3.TabIndex = 16;
			Label3.Text = "[Button_ContentHint]";
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(338, 497);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(83, 31);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(426, 497);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(83, 31);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(8, 8);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(144, 24);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "[Button_Style]";
			this.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmbStyle
			// 
			this.cmbStyle.DropDownHeight = 212;
			this.cmbStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbStyle.FormattingEnabled = true;
			this.cmbStyle.IntegralHeight = false;
			this.cmbStyle.ItemHeight = 16;
			this.cmbStyle.Location = new System.Drawing.Point(156, 7);
			this.cmbStyle.Name = "cmbStyle";
			this.cmbStyle.Size = new System.Drawing.Size(188, 24);
			this.cmbStyle.TabIndex = 3;
			this.cmbStyle.SelectedIndexChanged += new System.EventHandler(this.cmbStyle_SelectedIndexChanged);
			// 
			// lblExplain
			// 
			this.lblExplain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblExplain.Location = new System.Drawing.Point(8, 32);
			this.lblExplain.Name = "lblExplain";
			this.lblExplain.Size = new System.Drawing.Size(340, 29);
			this.lblExplain.TabIndex = 4;
			this.lblExplain.Text = "[Button_IsShared]";
			// 
			// lnkShare
			// 
			this.lnkShare.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkShare.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkShare.Location = new System.Drawing.Point(352, 8);
			this.lnkShare.Name = "lnkShare";
			this.lnkShare.Size = new System.Drawing.Size(108, 56);
			this.lnkShare.TabIndex = 5;
			this.lnkShare.TabStop = true;
			this.lnkShare.Text = "[Button_MakeShared]";
			this.lnkShare.Click += new System.EventHandler(this.lnkShare_Click);
			// 
			// tcMain
			// 
			this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tcMain.Controls.Add(this.tpContent);
			this.tcMain.Controls.Add(this.tpStyle);
			this.tcMain.Controls.Add(this.tpAction);
			this.tcMain.Location = new System.Drawing.Point(4, 4);
			this.tcMain.Name = "tcMain";
			this.tcMain.Padding = new System.Drawing.Point(3, 3);
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new System.Drawing.Size(513, 489);
			this.tcMain.TabIndex = 8;
			// 
			// tpContent
			// 
			this.tpContent.BackColor = System.Drawing.SystemColors.Control;
			this.tpContent.Controls.Add(this.tblContent);
			this.tpContent.Location = new System.Drawing.Point(4, 25);
			this.tpContent.Name = "tpContent";
			this.tpContent.Padding = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.tpContent.Size = new System.Drawing.Size(505, 460);
			this.tpContent.TabIndex = 0;
			this.tpContent.Text = "[Button_Content]";
			// 
			// tblContent
			// 
			this.tblContent.ColumnCount = 3;
			this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
			this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
			this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
			this.tblContent.Controls.Add(this.btnResource, 2, 5);
			this.tblContent.Controls.Add(this.Label9, 0, 0);
			this.tblContent.Controls.Add(this.Label7, 0, 2);
			this.tblContent.Controls.Add(this.txtText, 1, 2);
			this.tblContent.Controls.Add(this.Label8, 0, 3);
			this.tblContent.Controls.Add(this.btnImageDisc, 2, 4);
			this.tblContent.Controls.Add(this.pnlImagePreview, 1, 3);
			this.tblContent.Controls.Add(this.btnImageRemove, 2, 3);
			this.tblContent.Controls.Add(this.lblTextRatio, 0, 8);
			this.tblContent.Controls.Add(this.pnlTextRatio, 1, 8);
			this.tblContent.Controls.Add(this.chkSuperimpose, 0, 10);
			this.tblContent.Controls.Add(this.pnlLayout, 0, 9);
			this.tblContent.Controls.Add(Label3, 0, 1);
			this.tblContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblContent.Location = new System.Drawing.Point(3, 6);
			this.tblContent.Margin = new System.Windows.Forms.Padding(0);
			this.tblContent.Name = "tblContent";
			this.tblContent.RowCount = 12;
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblContent.Size = new System.Drawing.Size(499, 451);
			this.tblContent.TabIndex = 10;
			// 
			// btnResource
			// 
			this.btnResource.AutoSize = true;
			this.btnResource.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnResource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnResource.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnResource.Location = new System.Drawing.Point(310, 156);
			this.btnResource.Margin = new System.Windows.Forms.Padding(2);
			this.btnResource.Name = "btnResource";
			this.btnResource.Size = new System.Drawing.Size(187, 25);
			this.btnResource.TabIndex = 15;
			this.btnResource.Text = "[Button_ImageFromResource]";
			this.btnResource.UseVisualStyleBackColor = true;
			this.btnResource.Click += new System.EventHandler(this.btnResource_Click);
			// 
			// Label9
			// 
			this.Label9.AutoSize = true;
			this.tblContent.SetColumnSpan(this.Label9, 3);
			this.Label9.Location = new System.Drawing.Point(3, 0);
			this.Label9.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
			this.Label9.Name = "Label9";
			this.Label9.Size = new System.Drawing.Size(159, 17);
			this.Label9.TabIndex = 9;
			this.Label9.Text = "[Button_ContentExplain]";
			// 
			// Label7
			// 
			this.Label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Label7.AutoSize = true;
			this.Label7.Location = new System.Drawing.Point(34, 38);
			this.Label7.Name = "Label7";
			this.Label7.Size = new System.Drawing.Size(92, 17);
			this.Label7.TabIndex = 0;
			this.Label7.Text = "[Button_Text]";
			this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtText
			// 
			this.tblContent.SetColumnSpan(this.txtText, 2);
			this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtText.Location = new System.Drawing.Point(132, 41);
			this.txtText.Multiline = true;
			this.txtText.Name = "txtText";
			this.txtText.Size = new System.Drawing.Size(364, 52);
			this.txtText.TabIndex = 1;
			this.txtText.TextChanged += new System.EventHandler(this.txtText_TextChanged);
			// 
			// Label8
			// 
			this.Label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.Label8.AutoSize = true;
			this.Label8.Location = new System.Drawing.Point(23, 96);
			this.Label8.Name = "Label8";
			this.tblContent.SetRowSpan(this.Label8, 4);
			this.Label8.Size = new System.Drawing.Size(103, 17);
			this.Label8.TabIndex = 2;
			this.Label8.Text = "[Button_Image]";
			this.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnImageDisc
			// 
			this.btnImageDisc.AutoSize = true;
			this.btnImageDisc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnImageDisc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnImageDisc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnImageDisc.Location = new System.Drawing.Point(310, 127);
			this.btnImageDisc.Margin = new System.Windows.Forms.Padding(2);
			this.btnImageDisc.Name = "btnImageDisc";
			this.btnImageDisc.Size = new System.Drawing.Size(187, 25);
			this.btnImageDisc.TabIndex = 5;
			this.btnImageDisc.Text = "[Button_ImageFromDisc]";
			this.btnImageDisc.UseVisualStyleBackColor = true;
			this.btnImageDisc.Click += new System.EventHandler(this.btnImageDisc_Click);
			// 
			// pnlImagePreview
			// 
			this.pnlImagePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlImagePreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlImagePreview.Image = null;
			this.pnlImagePreview.Location = new System.Drawing.Point(132, 99);
			this.pnlImagePreview.Name = "pnlImagePreview";
			this.tblContent.SetRowSpan(this.pnlImagePreview, 5);
			this.pnlImagePreview.Size = new System.Drawing.Size(173, 154);
			this.pnlImagePreview.TabIndex = 3;
			this.pnlImagePreview.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlImagePreview_MouseClick);
			// 
			// btnImageRemove
			// 
			this.btnImageRemove.AutoSize = true;
			this.btnImageRemove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnImageRemove.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnImageRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnImageRemove.Location = new System.Drawing.Point(310, 98);
			this.btnImageRemove.Margin = new System.Windows.Forms.Padding(2);
			this.btnImageRemove.Name = "btnImageRemove";
			this.btnImageRemove.Size = new System.Drawing.Size(187, 25);
			this.btnImageRemove.TabIndex = 4;
			this.btnImageRemove.Text = "[Button_ImageRemove]";
			this.btnImageRemove.UseVisualStyleBackColor = true;
			this.btnImageRemove.Click += new System.EventHandler(this.btnImageRemove_Click);
			// 
			// lblTextRatio
			// 
			this.lblTextRatio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTextRatio.AutoSize = true;
			this.lblTextRatio.Location = new System.Drawing.Point(5, 256);
			this.lblTextRatio.Name = "lblTextRatio";
			this.lblTextRatio.Size = new System.Drawing.Size(121, 34);
			this.lblTextRatio.TabIndex = 10;
			this.lblTextRatio.Text = "[Button_TextRatio]";
			// 
			// pnlTextRatio
			// 
			this.tblContent.SetColumnSpan(this.pnlTextRatio, 2);
			this.pnlTextRatio.Controls.Add(this.sldTextRatio);
			this.pnlTextRatio.Controls.Add(this.rdoTextRatioSet);
			this.pnlTextRatio.Controls.Add(this.rdoTextRatioDefault);
			this.pnlTextRatio.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTextRatio.Location = new System.Drawing.Point(132, 259);
			this.pnlTextRatio.Name = "pnlTextRatio";
			this.pnlTextRatio.Size = new System.Drawing.Size(364, 36);
			this.pnlTextRatio.TabIndex = 12;
			// 
			// sldTextRatio
			// 
			this.sldTextRatio.AutoSize = false;
			this.sldTextRatio.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sldTextRatio.Enabled = false;
			this.sldTextRatio.LargeChange = 10;
			this.sldTextRatio.Location = new System.Drawing.Point(175, 0);
			this.sldTextRatio.Maximum = 100;
			this.sldTextRatio.Name = "sldTextRatio";
			this.sldTextRatio.Size = new System.Drawing.Size(189, 36);
			this.sldTextRatio.TabIndex = 11;
			this.sldTextRatio.TickFrequency = 10;
			this.sldTextRatio.Value = 50;
			this.sldTextRatio.ValueChanged += new System.EventHandler(this.sldTextRatio_ValueChanged);
			// 
			// rdoTextRatioSet
			// 
			this.rdoTextRatioSet.AutoSize = true;
			this.rdoTextRatioSet.Dock = System.Windows.Forms.DockStyle.Left;
			this.rdoTextRatioSet.Location = new System.Drawing.Point(161, 0);
			this.rdoTextRatioSet.Name = "rdoTextRatioSet";
			this.rdoTextRatioSet.Size = new System.Drawing.Size(14, 36);
			this.rdoTextRatioSet.TabIndex = 1;
			this.rdoTextRatioSet.TabStop = true;
			this.rdoTextRatioSet.UseVisualStyleBackColor = true;
			this.rdoTextRatioSet.CheckedChanged += new System.EventHandler(this.rdoTextRatioSet_CheckedChanged);
			// 
			// rdoTextRatioDefault
			// 
			this.rdoTextRatioDefault.AutoSize = true;
			this.rdoTextRatioDefault.Dock = System.Windows.Forms.DockStyle.Left;
			this.rdoTextRatioDefault.Location = new System.Drawing.Point(0, 0);
			this.rdoTextRatioDefault.Name = "rdoTextRatioDefault";
			this.rdoTextRatioDefault.Size = new System.Drawing.Size(161, 36);
			this.rdoTextRatioDefault.TabIndex = 0;
			this.rdoTextRatioDefault.TabStop = true;
			this.rdoTextRatioDefault.Text = "[Button_RatioDefault]";
			this.rdoTextRatioDefault.UseVisualStyleBackColor = true;
			// 
			// chkSuperimpose
			// 
			this.chkSuperimpose.AutoSize = true;
			this.tblContent.SetColumnSpan(this.chkSuperimpose, 3);
			this.chkSuperimpose.Location = new System.Drawing.Point(3, 381);
			this.chkSuperimpose.Name = "chkSuperimpose";
			this.chkSuperimpose.Size = new System.Drawing.Size(167, 21);
			this.chkSuperimpose.TabIndex = 14;
			this.chkSuperimpose.Text = "[Button_Superimpose]";
			this.chkSuperimpose.UseVisualStyleBackColor = true;
			this.chkSuperimpose.CheckedChanged += new System.EventHandler(this.chkSuperimpose_CheckedChanged);
			// 
			// pnlLayout
			// 
			this.pnlLayout.AutoScroll = true;
			this.pnlLayout.AutoSizeControls = false;
			this.tblContent.SetColumnSpan(this.pnlLayout, 3);
			this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlLayout.Horizontal = true;
			this.pnlLayout.Location = new System.Drawing.Point(3, 301);
			this.pnlLayout.MinimumItemHeight = 12;
			this.pnlLayout.Name = "pnlLayout";
			this.pnlLayout.Padding = new System.Windows.Forms.Padding(3);
			this.pnlLayout.SelectedColour = System.Drawing.Color.Orange;
			this.pnlLayout.SelectedTextColour = System.Drawing.Color.Empty;
			this.pnlLayout.Size = new System.Drawing.Size(493, 74);
			this.pnlLayout.TabIndex = 13;
			this.pnlLayout.TabStop = true;
			// 
			// tpStyle
			// 
			this.tpStyle.BackColor = System.Drawing.SystemColors.Control;
			this.tpStyle.Controls.Add(this.ctrStyle);
			this.tpStyle.Controls.Add(this.Label1);
			this.tpStyle.Controls.Add(this.cmbStyle);
			this.tpStyle.Controls.Add(this.lblExplain);
			this.tpStyle.Controls.Add(this.lnkShare);
			this.tpStyle.Controls.Add(this.lnkDefaultStyle);
			this.tpStyle.Location = new System.Drawing.Point(4, 25);
			this.tpStyle.Name = "tpStyle";
			this.tpStyle.Padding = new System.Windows.Forms.Padding(3);
			this.tpStyle.Size = new System.Drawing.Size(505, 460);
			this.tpStyle.TabIndex = 1;
			this.tpStyle.Text = "[Button_Style_Background]";
			// 
			// ctrStyle
			// 
			this.ctrStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrStyle.Location = new System.Drawing.Point(8, 68);
			this.ctrStyle.Margin = new System.Windows.Forms.Padding(5);
			this.ctrStyle.Name = "ctrStyle";
			this.ctrStyle.Size = new System.Drawing.Size(452, 384);
			this.ctrStyle.TabIndex = 6;
			// 
			// lnkDefaultStyle
			// 
			this.lnkDefaultStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkDefaultStyle.Location = new System.Drawing.Point(4, 64);
			this.lnkDefaultStyle.Name = "lnkDefaultStyle";
			this.lnkDefaultStyle.Size = new System.Drawing.Size(456, 392);
			this.lnkDefaultStyle.TabIndex = 7;
			this.lnkDefaultStyle.TabStop = true;
			this.lnkDefaultStyle.Text = "[Button_Style_EditDefault]";
			this.lnkDefaultStyle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkDefaultStyle.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDefaultStyle_LinkClicked);
			// 
			// tpAction
			// 
			this.tpAction.Controls.Add(this.tblAction);
			this.tpAction.Location = new System.Drawing.Point(4, 25);
			this.tpAction.Name = "tpAction";
			this.tpAction.Size = new System.Drawing.Size(505, 460);
			this.tpAction.TabIndex = 2;
			this.tpAction.Text = "[Button_Action]";
			this.tpAction.UseVisualStyleBackColor = true;
			// 
			// tblAction
			// 
			this.tblAction.ColumnCount = 1;
			this.tblAction.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblAction.Controls.Add(Label2, 0, 0);
			this.tblAction.Controls.Add(this.ctrActions, 0, 1);
			this.tblAction.Controls.Add(this.chkDisplayFromAction, 0, 3);
			this.tblAction.Controls.Add(this.pnlActionSelected, 0, 2);
			this.tblAction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblAction.Location = new System.Drawing.Point(0, 0);
			this.tblAction.Name = "tblAction";
			this.tblAction.RowCount = 4;
			this.tblAction.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblAction.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblAction.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblAction.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblAction.Size = new System.Drawing.Size(505, 460);
			this.tblAction.TabIndex = 2;
			// 
			// ctrActions
			// 
			this.ctrActions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrActions.Location = new System.Drawing.Point(3, 20);
			this.ctrActions.Mode = SAW.ctrActionSelect.Modes.Button;
			this.ctrActions.Name = "ctrActions";
			this.ctrActions.SelectedAction = nullAction1;
			this.ctrActions.Size = new System.Drawing.Size(499, 354);
			this.ctrActions.TabIndex = 0;
			this.ctrActions.SelectionChanged += new System.EventHandler(this.ctrActions_AfterSelect);
			// 
			// chkDisplayFromAction
			// 
			this.chkDisplayFromAction.AutoSize = true;
			this.chkDisplayFromAction.Location = new System.Drawing.Point(3, 436);
			this.chkDisplayFromAction.Name = "chkDisplayFromAction";
			this.chkDisplayFromAction.Size = new System.Drawing.Size(201, 21);
			this.chkDisplayFromAction.TabIndex = 2;
			this.chkDisplayFromAction.Text = "[Button_DisplayFromAction]";
			this.chkDisplayFromAction.UseVisualStyleBackColor = true;
			// 
			// pnlActionSelected
			// 
			this.pnlActionSelected.AutoSize = true;
			this.pnlActionSelected.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlActionSelected.Controls.Add(this.lblActionSelected);
			this.pnlActionSelected.Controls.Add(this.ctrActionColour);
			this.pnlActionSelected.Controls.Add(this.txtActionKey);
			this.pnlActionSelected.Controls.Add(this.chkActionKeyAuto);
			this.pnlActionSelected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlActionSelected.Location = new System.Drawing.Point(0, 377);
			this.pnlActionSelected.Margin = new System.Windows.Forms.Padding(0);
			this.pnlActionSelected.MinimumSize = new System.Drawing.Size(100, 10);
			this.pnlActionSelected.Name = "pnlActionSelected";
			this.pnlActionSelected.Size = new System.Drawing.Size(505, 56);
			this.pnlActionSelected.TabIndex = 3;
			// 
			// lblActionSelected
			// 
			this.lblActionSelected.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblActionSelected.AutoSize = true;
			this.lblActionSelected.Location = new System.Drawing.Point(3, 6);
			this.lblActionSelected.Name = "lblActionSelected";
			this.lblActionSelected.Size = new System.Drawing.Size(159, 17);
			this.lblActionSelected.TabIndex = 0;
			this.lblActionSelected.Text = "[Button_SelectedAction]";
			// 
			// ctrActionColour
			// 
			this.ctrActionColour.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctrActionColour.Location = new System.Drawing.Point(168, 3);
			this.ctrActionColour.Name = "ctrActionColour";
			this.ctrActionColour.Size = new System.Drawing.Size(75, 21);
			this.ctrActionColour.TabIndex = 1;
			this.ctrActionColour.Visible = false;
			this.ctrActionColour.UserChangedColour += new System.EventHandler(this.ctrActionColour_UserChangedColour);
			// 
			// txtActionKey
			// 
			this.txtActionKey.Location = new System.Drawing.Point(249, 3);
			this.txtActionKey.Name = "txtActionKey";
			this.txtActionKey.Size = new System.Drawing.Size(119, 23);
			this.txtActionKey.TabIndex = 2;
			this.txtActionKey.Visible = false;
			this.txtActionKey.TextChanged += new System.EventHandler(this.txtActionKey_TextChanged);
			this.txtActionKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtActionKey_KeyDown);
			this.txtActionKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtActionKey_KeyPress);
			// 
			// chkActionKeyAuto
			// 
			this.chkActionKeyAuto.AutoSize = true;
			this.chkActionKeyAuto.Location = new System.Drawing.Point(3, 32);
			this.chkActionKeyAuto.Name = "chkActionKeyAuto";
			this.chkActionKeyAuto.Size = new System.Drawing.Size(176, 21);
			this.chkActionKeyAuto.TabIndex = 3;
			this.chkActionKeyAuto.Text = "[Button_ActionKeyAuto]";
			this.chkActionKeyAuto.UseVisualStyleBackColor = true;
			this.chkActionKeyAuto.Visible = false;
			this.chkActionKeyAuto.CheckedChanged += new System.EventHandler(this.chkActionKeyAuto_CheckedChanged);
			// 
			// mnuImage
			// 
			this.mnuImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExportImage});
			this.mnuImage.Name = "mnuImage";
			this.mnuImage.Size = new System.Drawing.Size(155, 26);
			// 
			// mnuExportImage
			// 
			this.mnuExportImage.Name = "mnuExportImage";
			this.mnuExportImage.Size = new System.Drawing.Size(154, 22);
			this.mnuExportImage.Text = "[Export_Image]";
			this.mnuExportImage.Click += new System.EventHandler(this.mnuExportImage_Click);
			// 
			// frmButton
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(521, 532);
			this.Controls.Add(this.tcMain);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 350);
			this.Name = "frmButton";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Button_Title]";
			this.Load += new System.EventHandler(this.frmButton_Load);
			this.tcMain.ResumeLayout(false);
			this.tpContent.ResumeLayout(false);
			this.tblContent.ResumeLayout(false);
			this.tblContent.PerformLayout();
			this.pnlTextRatio.ResumeLayout(false);
			this.pnlTextRatio.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sldTextRatio)).EndInit();
			this.tpStyle.ResumeLayout(false);
			this.tpAction.ResumeLayout(false);
			this.tblAction.ResumeLayout(false);
			this.tblAction.PerformLayout();
			this.pnlActionSelected.ResumeLayout(false);
			this.pnlActionSelected.PerformLayout();
			this.mnuImage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.ComboBox cmbStyle;
		private System.Windows.Forms.Label lblExplain;
		private System.Windows.Forms.LinkLabel lnkShare;
		private System.Windows.Forms.TabControl tcMain;
		private System.Windows.Forms.TabPage tpContent;
		private System.Windows.Forms.TabPage tpStyle;
		private System.Windows.Forms.Label Label8;
		private System.Windows.Forms.TextBox txtText;
		private System.Windows.Forms.Label Label7;
		private PreviewPanel pnlImagePreview;
		private System.Windows.Forms.Button btnImageDisc;
		private System.Windows.Forms.Button btnImageRemove;
		private System.Windows.Forms.Label Label9;
		private System.Windows.Forms.TableLayoutPanel tblContent;
		private System.Windows.Forms.Label lblTextRatio;
		private System.Windows.Forms.TrackBar sldTextRatio;
		private System.Windows.Forms.Panel pnlTextRatio;
		private System.Windows.Forms.RadioButton rdoTextRatioSet;
		internal System.Windows.Forms.RadioButton rdoTextRatioDefault;
		private System.Windows.Forms.CheckBox chkSuperimpose;
		private SelectionPanel pnlLayout;
		private System.Windows.Forms.TabPage tpAction;
		private ctrActionSelect ctrActions;
		private System.Windows.Forms.TableLayoutPanel tblAction;
		private System.Windows.Forms.CheckBox chkDisplayFromAction;
		internal System.Windows.Forms.FlowLayoutPanel pnlActionSelected;
		private System.Windows.Forms.Label lblActionSelected;
		private ColourPicker ctrActionColour;
		private System.Windows.Forms.Button btnResource;
		private TextBoxAllKeys txtActionKey;
		private System.Windows.Forms.CheckBox chkActionKeyAuto;
		private System.Windows.Forms.LinkLabel lnkDefaultStyle;
		private System.Windows.Forms.ContextMenuStrip mnuImage;
		private System.Windows.Forms.ToolStripMenuItem mnuExportImage;
		private ctrButtonStyleEdit ctrStyle;
		private System.ComponentModel.IContainer components;
	}
	
}
