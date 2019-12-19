
namespace SAW
{
	
	public partial class ctrButtonStyleEdit : System.Windows.Forms.UserControl
	{

		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;

		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.pnlSimple = new System.Windows.Forms.Panel();
			this.pnlShape = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoRectangular = new System.Windows.Forms.RadioButton();
			this.rdoRounded = new System.Windows.Forms.RadioButton();
			this.rdoEllipse = new System.Windows.Forms.RadioButton();
			this.Label5 = new System.Windows.Forms.Label();
			this.ctrTextColour = new SAW.ColourPanel();
			this.pnlFillStyle = new SAW.ButtonPanel();
			this.ctrFillColour = new SAW.ColourPanel();
			this.Label4 = new System.Windows.Forms.Label();
			this.pnlBorderStyle = new SAW.ButtonPanel();
			this.ctrBorderColour = new SAW.ColourPanel();
			this.Label3 = new System.Windows.Forms.Label();
			this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.Label6 = new System.Windows.Forms.Label();
			this.rdoNormal = new System.Windows.Forms.RadioButton();
			this.rdoHighlight = new System.Windows.Forms.RadioButton();
			this.rdoSelected = new System.Windows.Forms.RadioButton();
			this.Panel1 = new System.Windows.Forms.Panel();
			this.cmbBackground = new System.Windows.Forms.ComboBox();
			this.Label2 = new System.Windows.Forms.Label();
			this.pnlSoftwareImages = new System.Windows.Forms.FlowLayoutPanel();
			this.lblSoftware = new System.Windows.Forms.Label();
			this.Label7 = new System.Windows.Forms.Label();
			this.pnlSoftwarePreview = new System.Windows.Forms.Panel();
			this.pnlCustomImage = new System.Windows.Forms.Panel();
			this.pnlCustomTable = new System.Windows.Forms.TableLayoutPanel();
			this.Label1 = new System.Windows.Forms.Label();
			this.pnlImageIndex = new System.Windows.Forms.FlowLayoutPanel();
			this.lblImageIndex = new System.Windows.Forms.Label();
			this.btnLeft = new System.Windows.Forms.Button();
			this.btnRight = new System.Windows.Forms.Button();
			this.btnAddImage = new System.Windows.Forms.Button();
			this.btnRemoveImage = new System.Windows.Forms.Button();
			this.pnlPreview = new System.Windows.Forms.Panel();
			this.chkSlice = new System.Windows.Forms.CheckBox();
			this.lblSlicePrompt = new System.Windows.Forms.Label();
			this.pnlSliceEdit = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoEditSlice = new System.Windows.Forms.RadioButton();
			this.rdoPreviewSlice = new System.Windows.Forms.RadioButton();
			this.lnkCopy9 = new System.Windows.Forms.LinkLabel();
			this.pnlSimple.SuspendLayout();
			this.pnlShape.SuspendLayout();
			this.FlowLayoutPanel1.SuspendLayout();
			this.Panel1.SuspendLayout();
			this.pnlSoftwareImages.SuspendLayout();
			this.pnlCustomImage.SuspendLayout();
			this.pnlCustomTable.SuspendLayout();
			this.pnlImageIndex.SuspendLayout();
			this.pnlSliceEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlSimple
			// 
			this.pnlSimple.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSimple.Controls.Add(this.pnlShape);
			this.pnlSimple.Controls.Add(this.Label5);
			this.pnlSimple.Controls.Add(this.ctrTextColour);
			this.pnlSimple.Controls.Add(this.pnlFillStyle);
			this.pnlSimple.Controls.Add(this.ctrFillColour);
			this.pnlSimple.Controls.Add(this.Label4);
			this.pnlSimple.Controls.Add(this.pnlBorderStyle);
			this.pnlSimple.Controls.Add(this.ctrBorderColour);
			this.pnlSimple.Controls.Add(this.Label3);
			this.pnlSimple.Location = new System.Drawing.Point(48, 72);
			this.pnlSimple.Name = "pnlSimple";
			this.pnlSimple.Size = new System.Drawing.Size(476, 328);
			this.pnlSimple.TabIndex = 3;
			// 
			// pnlShape
			// 
			this.pnlShape.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlShape.Controls.Add(this.rdoRectangular);
			this.pnlShape.Controls.Add(this.rdoRounded);
			this.pnlShape.Controls.Add(this.rdoEllipse);
			this.pnlShape.Location = new System.Drawing.Point(8, 112);
			this.pnlShape.Name = "pnlShape";
			this.pnlShape.Size = new System.Drawing.Size(462, 24);
			this.pnlShape.TabIndex = 7;
			// 
			// rdoRectangular
			// 
			this.rdoRectangular.AutoSize = true;
			this.rdoRectangular.Checked = true;
			this.rdoRectangular.Location = new System.Drawing.Point(3, 3);
			this.rdoRectangular.Name = "rdoRectangular";
			this.rdoRectangular.Size = new System.Drawing.Size(160, 21);
			this.rdoRectangular.TabIndex = 0;
			this.rdoRectangular.TabStop = true;
			this.rdoRectangular.Text = "[Button_Rectangular]";
			this.rdoRectangular.UseVisualStyleBackColor = true;
			this.rdoRectangular.CheckedChanged += new System.EventHandler(this.rdoRectangular_CheckedChanged);
			// 
			// rdoRounded
			// 
			this.rdoRounded.AutoSize = true;
			this.rdoRounded.Location = new System.Drawing.Point(169, 3);
			this.rdoRounded.Name = "rdoRounded";
			this.rdoRounded.Size = new System.Drawing.Size(141, 21);
			this.rdoRounded.TabIndex = 1;
			this.rdoRounded.Text = "[Button_Rounded]";
			this.rdoRounded.UseVisualStyleBackColor = true;
			this.rdoRounded.CheckedChanged += new System.EventHandler(this.rdoRectangular_CheckedChanged);
			// 
			// rdoEllipse
			// 
			this.rdoEllipse.AutoSize = true;
			this.rdoEllipse.Location = new System.Drawing.Point(316, 3);
			this.rdoEllipse.Name = "rdoEllipse";
			this.rdoEllipse.Size = new System.Drawing.Size(134, 21);
			this.rdoEllipse.TabIndex = 2;
			this.rdoEllipse.Text = "[Button_Elliptical]";
			this.rdoEllipse.UseVisualStyleBackColor = true;
			this.rdoEllipse.CheckedChanged += new System.EventHandler(this.rdoRectangular_CheckedChanged);
			// 
			// Label5
			// 
			this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label5.Location = new System.Drawing.Point(56, 252);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(136, 48);
			this.Label5.TabIndex = 6;
			this.Label5.Text = "[Button_TextColour]";
			this.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ctrTextColour
			// 
			this.ctrTextColour.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.ctrTextColour.DisplayAdvanced = true;
			this.ctrTextColour.Location = new System.Drawing.Point(200, 236);
			this.ctrTextColour.Name = "ctrTextColour";
			this.ctrTextColour.Size = new System.Drawing.Size(200, 86);
			this.ctrTextColour.TabIndex = 5;
			this.ctrTextColour.UseSettings = true;
			this.ctrTextColour.VariableValue = 100;
			// 
			// pnlFillStyle
			// 
			this.pnlFillStyle.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.pnlFillStyle.DoubleBuffered = false;
			this.pnlFillStyle.Location = new System.Drawing.Point(208, 156);
			this.pnlFillStyle.Name = "pnlFillStyle";
			this.pnlFillStyle.Size = new System.Drawing.Size(176, 84);
			this.pnlFillStyle.TabIndex = 3;
			this.pnlFillStyle.UseArrowKeys = false;
			// 
			// ctrFillColour
			// 
			this.ctrFillColour.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.ctrFillColour.DisplayAdvanced = true;
			this.ctrFillColour.Location = new System.Drawing.Point(0, 156);
			this.ctrFillColour.Name = "ctrFillColour";
			this.ctrFillColour.Size = new System.Drawing.Size(200, 86);
			this.ctrFillColour.TabIndex = 4;
			this.ctrFillColour.UseSettings = true;
			this.ctrFillColour.VariableValue = 100;
			// 
			// Label4
			// 
			this.Label4.AutoSize = true;
			this.Label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label4.Location = new System.Drawing.Point(8, 136);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(160, 17);
			this.Label4.TabIndex = 3;
			this.Label4.Text = "[Button_Background]";
			// 
			// pnlBorderStyle
			// 
			this.pnlBorderStyle.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.pnlBorderStyle.DoubleBuffered = false;
			this.pnlBorderStyle.Location = new System.Drawing.Point(208, 24);
			this.pnlBorderStyle.Name = "pnlBorderStyle";
			this.pnlBorderStyle.Size = new System.Drawing.Size(240, 84);
			this.pnlBorderStyle.TabIndex = 2;
			this.pnlBorderStyle.UseArrowKeys = false;
			// 
			// ctrBorderColour
			// 
			this.ctrBorderColour.Blend = SAW.ButtonPanel.BlendDirection.Off;
			this.ctrBorderColour.DisplayAdvanced = true;
			this.ctrBorderColour.Location = new System.Drawing.Point(0, 20);
			this.ctrBorderColour.Name = "ctrBorderColour";
			this.ctrBorderColour.Size = new System.Drawing.Size(208, 92);
			this.ctrBorderColour.TabIndex = 1;
			this.ctrBorderColour.UseSettings = true;
			this.ctrBorderColour.VariableValue = 100;
			// 
			// Label3
			// 
			this.Label3.AutoSize = true;
			this.Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label3.Location = new System.Drawing.Point(8, 0);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(123, 17);
			this.Label3.TabIndex = 0;
			this.Label3.Text = "[Button_Border]";
			// 
			// FlowLayoutPanel1
			// 
			this.FlowLayoutPanel1.Controls.Add(this.Label6);
			this.FlowLayoutPanel1.Controls.Add(this.rdoNormal);
			this.FlowLayoutPanel1.Controls.Add(this.rdoHighlight);
			this.FlowLayoutPanel1.Controls.Add(this.rdoSelected);
			this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.FlowLayoutPanel1.Location = new System.Drawing.Point(0, 32);
			this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			this.FlowLayoutPanel1.Size = new System.Drawing.Size(558, 32);
			this.FlowLayoutPanel1.TabIndex = 9;
			this.FlowLayoutPanel1.WrapContents = false;
			// 
			// Label6
			// 
			this.Label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.Label6.AutoSize = true;
			this.Label6.Location = new System.Drawing.Point(3, 8);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(141, 17);
			this.Label6.TabIndex = 0;
			this.Label6.Text = "[Button_EditingState]";
			// 
			// rdoNormal
			// 
			this.rdoNormal.Appearance = System.Windows.Forms.Appearance.Button;
			this.rdoNormal.AutoSize = true;
			this.rdoNormal.Checked = true;
			this.rdoNormal.Location = new System.Drawing.Point(150, 3);
			this.rdoNormal.Name = "rdoNormal";
			this.rdoNormal.Size = new System.Drawing.Size(120, 27);
			this.rdoNormal.TabIndex = 1;
			this.rdoNormal.TabStop = true;
			this.rdoNormal.Text = "[Button_Normal]";
			this.rdoNormal.UseVisualStyleBackColor = true;
			this.rdoNormal.CheckedChanged += new System.EventHandler(this.rdoNormal_CheckedChanged);
			// 
			// rdoHighlight
			// 
			this.rdoHighlight.Appearance = System.Windows.Forms.Appearance.Button;
			this.rdoHighlight.AutoSize = true;
			this.rdoHighlight.Location = new System.Drawing.Point(276, 3);
			this.rdoHighlight.Name = "rdoHighlight";
			this.rdoHighlight.Size = new System.Drawing.Size(130, 27);
			this.rdoHighlight.TabIndex = 2;
			this.rdoHighlight.Text = "[Button_Highlight]";
			this.rdoHighlight.UseVisualStyleBackColor = true;
			this.rdoHighlight.CheckedChanged += new System.EventHandler(this.rdoNormal_CheckedChanged);
			// 
			// rdoSelected
			// 
			this.rdoSelected.Appearance = System.Windows.Forms.Appearance.Button;
			this.rdoSelected.AutoSize = true;
			this.rdoSelected.Location = new System.Drawing.Point(412, 3);
			this.rdoSelected.Name = "rdoSelected";
			this.rdoSelected.Size = new System.Drawing.Size(130, 27);
			this.rdoSelected.TabIndex = 3;
			this.rdoSelected.Text = "[Button_Selected]";
			this.rdoSelected.UseVisualStyleBackColor = true;
			this.rdoSelected.CheckedChanged += new System.EventHandler(this.rdoNormal_CheckedChanged);
			// 
			// Panel1
			// 
			this.Panel1.Controls.Add(this.cmbBackground);
			this.Panel1.Controls.Add(this.Label2);
			this.Panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.Panel1.Location = new System.Drawing.Point(0, 0);
			this.Panel1.Name = "Panel1";
			this.Panel1.Padding = new System.Windows.Forms.Padding(3);
			this.Panel1.Size = new System.Drawing.Size(558, 32);
			this.Panel1.TabIndex = 2;
			// 
			// cmbBackground
			// 
			this.cmbBackground.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBackground.FormattingEnabled = true;
			this.cmbBackground.Location = new System.Drawing.Point(176, 3);
			this.cmbBackground.Name = "cmbBackground";
			this.cmbBackground.Size = new System.Drawing.Size(379, 24);
			this.cmbBackground.TabIndex = 1;
			this.cmbBackground.SelectedIndexChanged += new System.EventHandler(this.cmbBackground_SelectedIndexChanged);
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Dock = System.Windows.Forms.DockStyle.Left;
			this.Label2.Location = new System.Drawing.Point(3, 3);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(173, 17);
			this.Label2.TabIndex = 0;
			this.Label2.Text = "[Button_BackgroundType]";
			this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// pnlSoftwareImages
			// 
			this.pnlSoftwareImages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSoftwareImages.Controls.Add(this.lblSoftware);
			this.pnlSoftwareImages.Controls.Add(this.Label7);
			this.pnlSoftwareImages.Controls.Add(this.pnlSoftwarePreview);
			this.pnlSoftwareImages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.pnlSoftwareImages.Location = new System.Drawing.Point(8, 176);
			this.pnlSoftwareImages.Name = "pnlSoftwareImages";
			this.pnlSoftwareImages.Size = new System.Drawing.Size(352, 216);
			this.pnlSoftwareImages.TabIndex = 10;
			// 
			// lblSoftware
			// 
			this.lblSoftware.AutoSize = true;
			this.lblSoftware.Location = new System.Drawing.Point(3, 0);
			this.lblSoftware.Name = "lblSoftware";
			this.lblSoftware.Size = new System.Drawing.Size(155, 17);
			this.lblSoftware.TabIndex = 0;
			this.lblSoftware.Text = "[Button_SoftwareLabel]";
			// 
			// Label7
			// 
			this.Label7.AutoSize = true;
			this.Label7.Location = new System.Drawing.Point(3, 41);
			this.Label7.Margin = new System.Windows.Forms.Padding(3, 24, 3, 0);
			this.Label7.Name = "Label7";
			this.Label7.Size = new System.Drawing.Size(167, 17);
			this.Label7.TabIndex = 1;
			this.Label7.Text = "[Button_SoftwareSample]";
			// 
			// pnlSoftwarePreview
			// 
			this.pnlSoftwarePreview.Location = new System.Drawing.Point(3, 61);
			this.pnlSoftwarePreview.Name = "pnlSoftwarePreview";
			this.pnlSoftwarePreview.Size = new System.Drawing.Size(64, 64);
			this.pnlSoftwarePreview.TabIndex = 2;
			this.pnlSoftwarePreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSoftwarePreview_Paint);
			// 
			// pnlCustomImage
			// 
			this.pnlCustomImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlCustomImage.Controls.Add(this.pnlCustomTable);
			this.pnlCustomImage.Location = new System.Drawing.Point(32, 112);
			this.pnlCustomImage.Name = "pnlCustomImage";
			this.pnlCustomImage.Size = new System.Drawing.Size(456, 328);
			this.pnlCustomImage.TabIndex = 4;
			// 
			// pnlCustomTable
			// 
			this.pnlCustomTable.ColumnCount = 2;
			this.pnlCustomTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlCustomTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlCustomTable.Controls.Add(this.Label1, 0, 0);
			this.pnlCustomTable.Controls.Add(this.pnlImageIndex, 0, 1);
			this.pnlCustomTable.Controls.Add(this.btnAddImage, 0, 2);
			this.pnlCustomTable.Controls.Add(this.btnRemoveImage, 1, 2);
			this.pnlCustomTable.Controls.Add(this.pnlPreview, 0, 3);
			this.pnlCustomTable.Controls.Add(this.chkSlice, 1, 3);
			this.pnlCustomTable.Controls.Add(this.lblSlicePrompt, 1, 5);
			this.pnlCustomTable.Controls.Add(this.pnlSliceEdit, 1, 4);
			this.pnlCustomTable.Controls.Add(this.lnkCopy9, 1, 6);
			this.pnlCustomTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlCustomTable.Location = new System.Drawing.Point(0, 0);
			this.pnlCustomTable.Name = "pnlCustomTable";
			this.pnlCustomTable.RowCount = 8;
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlCustomTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlCustomTable.Size = new System.Drawing.Size(454, 326);
			this.pnlCustomTable.TabIndex = 0;
			this.pnlCustomTable.Resize += new System.EventHandler(this.pnlCustomTable_Resize);
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.pnlCustomTable.SetColumnSpan(this.Label1, 2);
			this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Label1.Location = new System.Drawing.Point(3, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(157, 17);
			this.Label1.TabIndex = 0;
			this.Label1.Text = "[Button_CustomPrompt]";
			// 
			// pnlImageIndex
			// 
			this.pnlImageIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlImageIndex.AutoSize = true;
			this.pnlImageIndex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlCustomTable.SetColumnSpan(this.pnlImageIndex, 2);
			this.pnlImageIndex.Controls.Add(this.lblImageIndex);
			this.pnlImageIndex.Controls.Add(this.btnLeft);
			this.pnlImageIndex.Controls.Add(this.btnRight);
			this.pnlImageIndex.Location = new System.Drawing.Point(3, 20);
			this.pnlImageIndex.Name = "pnlImageIndex";
			this.pnlImageIndex.Size = new System.Drawing.Size(448, 34);
			this.pnlImageIndex.TabIndex = 1;
			// 
			// lblImageIndex
			// 
			this.lblImageIndex.AutoSize = true;
			this.lblImageIndex.Location = new System.Drawing.Point(3, 0);
			this.lblImageIndex.Name = "lblImageIndex";
			this.lblImageIndex.Size = new System.Drawing.Size(51, 17);
			this.lblImageIndex.TabIndex = 0;
			this.lblImageIndex.Text = "Label8";
			// 
			// btnLeft
			// 
			this.btnLeft.Enabled = false;
			this.btnLeft.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnLeft.Location = new System.Drawing.Point(60, 3);
			this.btnLeft.Name = "btnLeft";
			this.btnLeft.Size = new System.Drawing.Size(28, 28);
			this.btnLeft.TabIndex = 16;
			this.btnLeft.Text = "<";
			this.btnLeft.UseVisualStyleBackColor = true;
			this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
			// 
			// btnRight
			// 
			this.btnRight.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnRight.Location = new System.Drawing.Point(94, 3);
			this.btnRight.Name = "btnRight";
			this.btnRight.Size = new System.Drawing.Size(28, 28);
			this.btnRight.TabIndex = 15;
			this.btnRight.Text = ">";
			this.btnRight.UseVisualStyleBackColor = true;
			this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
			// 
			// btnAddImage
			// 
			this.btnAddImage.AutoSize = true;
			this.btnAddImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnAddImage.Location = new System.Drawing.Point(3, 60);
			this.btnAddImage.Name = "btnAddImage";
			this.btnAddImage.Size = new System.Drawing.Size(145, 27);
			this.btnAddImage.TabIndex = 2;
			this.btnAddImage.Text = "[Button_ImageLoad]";
			this.btnAddImage.UseVisualStyleBackColor = true;
			this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);
			// 
			// btnRemoveImage
			// 
			this.btnRemoveImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemoveImage.AutoSize = true;
			this.btnRemoveImage.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnRemoveImage.Location = new System.Drawing.Point(286, 60);
			this.btnRemoveImage.Name = "btnRemoveImage";
			this.btnRemoveImage.Size = new System.Drawing.Size(165, 27);
			this.btnRemoveImage.TabIndex = 3;
			this.btnRemoveImage.Text = "[Button_ImageRemove]";
			this.btnRemoveImage.UseVisualStyleBackColor = true;
			this.btnRemoveImage.Click += new System.EventHandler(this.btnRemoveImage_Click);
			// 
			// pnlPreview
			// 
			this.pnlPreview.Location = new System.Drawing.Point(3, 93);
			this.pnlPreview.Name = "pnlPreview";
			this.pnlCustomTable.SetRowSpan(this.pnlPreview, 5);
			this.pnlPreview.Size = new System.Drawing.Size(181, 139);
			this.pnlPreview.TabIndex = 4;
			this.pnlPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPreview_Paint);
			this.pnlPreview.DoubleClick += new System.EventHandler(this.pnlPreview_DoubleClick);
			this.pnlPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlPreview_MouseDown);
			this.pnlPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlPreview_MouseMove);
			this.pnlPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlPreview_MouseUp);
			this.pnlPreview.Resize += new System.EventHandler(this.pnlPreview_Resize);
			// 
			// chkSlice
			// 
			this.chkSlice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkSlice.AutoSize = true;
			this.chkSlice.Location = new System.Drawing.Point(230, 93);
			this.chkSlice.Name = "chkSlice";
			this.chkSlice.Size = new System.Drawing.Size(221, 21);
			this.chkSlice.TabIndex = 5;
			this.chkSlice.Text = "[Button_Use9Slice]";
			this.chkSlice.UseVisualStyleBackColor = true;
			this.chkSlice.CheckedChanged += new System.EventHandler(this.chkSlice_CheckedChanged);
			// 
			// lblSlicePrompt
			// 
			this.lblSlicePrompt.AutoSize = true;
			this.lblSlicePrompt.Location = new System.Drawing.Point(230, 177);
			this.lblSlicePrompt.Name = "lblSlicePrompt";
			this.lblSlicePrompt.Size = new System.Drawing.Size(172, 17);
			this.lblSlicePrompt.TabIndex = 7;
			this.lblSlicePrompt.Text = "[Button_9SliceEditPrompt]";
			// 
			// pnlSliceEdit
			// 
			this.pnlSliceEdit.AutoSize = true;
			this.pnlSliceEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlSliceEdit.Controls.Add(this.rdoEditSlice);
			this.pnlSliceEdit.Controls.Add(this.rdoPreviewSlice);
			this.pnlSliceEdit.Location = new System.Drawing.Point(230, 120);
			this.pnlSliceEdit.Name = "pnlSliceEdit";
			this.pnlSliceEdit.Size = new System.Drawing.Size(168, 54);
			this.pnlSliceEdit.TabIndex = 8;
			// 
			// rdoEditSlice
			// 
			this.rdoEditSlice.AutoSize = true;
			this.rdoEditSlice.Checked = true;
			this.rdoEditSlice.Location = new System.Drawing.Point(3, 3);
			this.rdoEditSlice.Name = "rdoEditSlice";
			this.rdoEditSlice.Size = new System.Drawing.Size(137, 21);
			this.rdoEditSlice.TabIndex = 0;
			this.rdoEditSlice.TabStop = true;
			this.rdoEditSlice.Text = "[Button_SliceEdit]";
			this.rdoEditSlice.UseVisualStyleBackColor = true;
			this.rdoEditSlice.CheckedChanged += new System.EventHandler(this.rdoEditSlice_CheckedChanged);
			// 
			// rdoPreviewSlice
			// 
			this.rdoPreviewSlice.AutoSize = true;
			this.rdoPreviewSlice.Location = new System.Drawing.Point(3, 30);
			this.rdoPreviewSlice.Name = "rdoPreviewSlice";
			this.rdoPreviewSlice.Size = new System.Drawing.Size(162, 21);
			this.rdoPreviewSlice.TabIndex = 1;
			this.rdoPreviewSlice.Text = "[Button_SlicePreview]";
			this.rdoPreviewSlice.UseVisualStyleBackColor = true;
			this.rdoPreviewSlice.CheckedChanged += new System.EventHandler(this.rdoEditSlice_CheckedChanged);
			// 
			// lnkCopy9
			// 
			this.lnkCopy9.AutoSize = true;
			this.lnkCopy9.Location = new System.Drawing.Point(230, 195);
			this.lnkCopy9.Name = "lnkCopy9";
			this.lnkCopy9.Size = new System.Drawing.Size(135, 17);
			this.lnkCopy9.TabIndex = 9;
			this.lnkCopy9.TabStop = true;
			this.lnkCopy9.Text = "[Button_Copy9Slice]";
			this.lnkCopy9.Visible = false;
			this.lnkCopy9.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCopy9_LinkClicked);
			// 
			// ctrButtonStyleEdit
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.pnlSimple);
			this.Controls.Add(this.pnlCustomImage);
			this.Controls.Add(this.pnlSoftwareImages);
			this.Controls.Add(this.FlowLayoutPanel1);
			this.Controls.Add(this.Panel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ctrButtonStyleEdit";
			this.Size = new System.Drawing.Size(558, 458);
			this.pnlSimple.ResumeLayout(false);
			this.pnlSimple.PerformLayout();
			this.pnlShape.ResumeLayout(false);
			this.pnlShape.PerformLayout();
			this.FlowLayoutPanel1.ResumeLayout(false);
			this.FlowLayoutPanel1.PerformLayout();
			this.Panel1.ResumeLayout(false);
			this.Panel1.PerformLayout();
			this.pnlSoftwareImages.ResumeLayout(false);
			this.pnlSoftwareImages.PerformLayout();
			this.pnlCustomImage.ResumeLayout(false);
			this.pnlCustomTable.ResumeLayout(false);
			this.pnlCustomTable.PerformLayout();
			this.pnlImageIndex.ResumeLayout(false);
			this.pnlImageIndex.PerformLayout();
			this.pnlSliceEdit.ResumeLayout(false);
			this.pnlSliceEdit.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label Label5;
		private ColourPanel ctrTextColour;
		private ButtonPanel pnlFillStyle;
		private ColourPanel ctrFillColour;
		private System.Windows.Forms.Label Label4;
		private ButtonPanel pnlBorderStyle;
		private ColourPanel ctrBorderColour;
		private System.Windows.Forms.Label Label3;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
		internal System.Windows.Forms.Label Label6;
		private System.Windows.Forms.RadioButton rdoNormal;
		private System.Windows.Forms.RadioButton rdoHighlight;
		private System.Windows.Forms.RadioButton rdoSelected;
		private System.Windows.Forms.Panel Panel1;
		private System.Windows.Forms.ComboBox cmbBackground;
		private System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.FlowLayoutPanel pnlShape;
		private System.Windows.Forms.RadioButton rdoRectangular;
		private System.Windows.Forms.RadioButton rdoRounded;
		private System.Windows.Forms.RadioButton rdoEllipse;
		private System.Windows.Forms.FlowLayoutPanel pnlSoftwareImages;
		private System.Windows.Forms.Label lblSoftware;
		private System.Windows.Forms.Label Label7;
		private System.Windows.Forms.Panel pnlSoftwarePreview;
		private System.Windows.Forms.Panel pnlCustomImage;
		private System.Windows.Forms.TableLayoutPanel pnlCustomTable;
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.FlowLayoutPanel pnlImageIndex;
		private System.Windows.Forms.Button btnLeft;
		private System.Windows.Forms.Button btnRight;
		private System.Windows.Forms.Button btnAddImage;
		private System.Windows.Forms.Button btnRemoveImage;
		private System.Windows.Forms.Panel pnlPreview;
		private System.Windows.Forms.CheckBox chkSlice;
		internal System.Windows.Forms.Label lblSlicePrompt;
		private System.Windows.Forms.Label lblImageIndex;
		internal System.Windows.Forms.FlowLayoutPanel pnlSliceEdit;
		private System.Windows.Forms.RadioButton rdoEditSlice;
		private System.Windows.Forms.RadioButton rdoPreviewSlice;
		private System.Windows.Forms.LinkLabel lnkCopy9;
		private System.Windows.Forms.Panel pnlSimple;

	}

}
