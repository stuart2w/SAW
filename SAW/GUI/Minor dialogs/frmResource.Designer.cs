namespace SAW
{
	internal partial class frmResource : System.Windows.Forms.Form
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
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			base.Load += new System.EventHandler(frmResource_Load);
			System.Windows.Forms.Panel Panel1;
			System.Windows.Forms.Label Label1;
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.pnlImage = new System.Windows.Forms.TableLayoutPanel();
			this.lstImages = new System.Windows.Forms.ListView();
			this.lstImages.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lstImages_DrawItem);
			this.lstImages.DoubleClick += new System.EventHandler(this.lstStamps_DoubleClick);
			this.lstImages.SelectedIndexChanged += new System.EventHandler(this.lstImages_SelectedIndexChanged);
			this.imlSize = new System.Windows.Forms.ImageList(this.components);
			this.ctrPreview = new PreviewPanel();
			this.pnlSizes = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoAll = new System.Windows.Forms.RadioButton();
			this.rdo16 = new System.Windows.Forms.RadioButton();
			this.rdo32 = new System.Windows.Forms.RadioButton();
			this.rdo48 = new System.Windows.Forms.RadioButton();
			this.rdo64 = new System.Windows.Forms.RadioButton();
			this.rdo128 = new System.Windows.Forms.RadioButton();
			Panel1 = new System.Windows.Forms.Panel();
			Label1 = new System.Windows.Forms.Label();
			Panel1.SuspendLayout();
			this.pnlImage.SuspendLayout();
			this.pnlSizes.SuspendLayout();
			this.SuspendLayout();
			//
			//Panel1
			//
			Panel1.Controls.Add(this.btnOK);
			Panel1.Controls.Add(this.btnCancel);
			Panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			Panel1.Location = new System.Drawing.Point(469, 417);
			Panel1.Name = "Panel1";
			this.pnlImage.SetRowSpan(Panel1, 2);
			Panel1.Size = new System.Drawing.Size(150, 113);
			Panel1.TabIndex = 5;
			//
			//btnOK
			//
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(0, 57);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(150, 28);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnCancel.Location = new System.Drawing.Point(0, 85);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(150, 28);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//pnlImage
			//
			this.pnlImage.ColumnCount = 3;
			this.pnlImage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, (float) (0.0F)));
			this.pnlImage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (75.0F)));
			this.pnlImage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (25.0F)));
			this.pnlImage.Controls.Add(this.pnlSizes, 1, 1);
			this.pnlImage.Controls.Add(this.lstImages, 1, 2);
			this.pnlImage.Controls.Add(Label1, 1, 0);
			this.pnlImage.Controls.Add(Panel1, 2, 3);
			this.pnlImage.Controls.Add(this.ctrPreview, 1, 4);
			this.pnlImage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlImage.Location = new System.Drawing.Point(0, 0);
			this.pnlImage.Margin = new System.Windows.Forms.Padding(4);
			this.pnlImage.Name = "pnlImage";
			this.pnlImage.RowCount = 5;
			this.pnlImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlImage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float) (100.0F)));
			this.pnlImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlImage.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlImage.Size = new System.Drawing.Size(622, 533);
			this.pnlImage.TabIndex = 68;
			//
			//lstImages
			//
			this.pnlImage.SetColumnSpan(this.lstImages, 2);
			this.lstImages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstImages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.lstImages.HideSelection = false;
			this.lstImages.LargeImageList = this.imlSize;
			this.lstImages.Location = new System.Drawing.Point(4, 58);
			this.lstImages.Margin = new System.Windows.Forms.Padding(4);
			this.lstImages.MultiSelect = false;
			this.lstImages.Name = "lstImages";
			this.lstImages.OwnerDraw = true;
			this.lstImages.Size = new System.Drawing.Size(614, 352);
			this.lstImages.TabIndex = 1;
			this.lstImages.UseCompatibleStateImageBehavior = false;
			//
			//imlSize
			//
			this.imlSize.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imlSize.ImageSize = new System.Drawing.Size(48, 48);
			this.imlSize.TransparentColor = System.Drawing.Color.Transparent;
			//
			//ctrPreview
			//
			this.ctrPreview.Dock = System.Windows.Forms.DockStyle.Top;
			this.ctrPreview.Image = null;
			this.ctrPreview.Location = new System.Drawing.Point(3, 430);
			this.ctrPreview.Name = "ctrPreview";
			this.ctrPreview.Size = new System.Drawing.Size(460, 100);
			this.ctrPreview.TabIndex = 7;
			//
			//Label1
			//
			Label1.AutoSize = true;
			this.pnlImage.SetColumnSpan(Label1, 2);
			Label1.Dock = System.Windows.Forms.DockStyle.Fill;
			Label1.Location = new System.Drawing.Point(3, 0);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(616, 17);
			Label1.TabIndex = 69;
			Label1.Text = "[ResourceImage_Heading]";
			//
			//pnlSizes
			//
			this.pnlImage.SetColumnSpan(this.pnlSizes, 2);
			this.pnlSizes.Controls.Add(this.rdoAll);
			this.pnlSizes.Controls.Add(this.rdo16);
			this.pnlSizes.Controls.Add(this.rdo32);
			this.pnlSizes.Controls.Add(this.rdo48);
			this.pnlSizes.Controls.Add(this.rdo64);
			this.pnlSizes.Controls.Add(this.rdo128);
			this.pnlSizes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlSizes.Location = new System.Drawing.Point(3, 20);
			this.pnlSizes.Name = "pnlSizes";
			this.pnlSizes.Size = new System.Drawing.Size(616, 31);
			this.pnlSizes.TabIndex = 70;
			//
			//rdoAll
			//
			this.rdoAll.AutoSize = true;
			this.rdoAll.Checked = true;
			this.rdoAll.Location = new System.Drawing.Point(3, 3);
			this.rdoAll.Name = "rdoAll";
			this.rdoAll.Size = new System.Drawing.Size(156, 21);
			this.rdoAll.TabIndex = 0;
			this.rdoAll.TabStop = true;
			this.rdoAll.Tag = "0-256";
			this.rdoAll.Text = "[ResourceImage_All]";
			this.rdoAll.UseVisualStyleBackColor = true;
			//
			//rdo16
			//
			this.rdo16.AutoSize = true;
			this.rdo16.Location = new System.Drawing.Point(165, 3);
			this.rdo16.Name = "rdo16";
			this.rdo16.Size = new System.Drawing.Size(42, 21);
			this.rdo16.TabIndex = 1;
			this.rdo16.TabStop = true;
			this.rdo16.Tag = "0-20";
			this.rdo16.Text = "16";
			this.rdo16.UseVisualStyleBackColor = true;
			//
			//rdo32
			//
			this.rdo32.AutoSize = true;
			this.rdo32.Location = new System.Drawing.Point(213, 3);
			this.rdo32.Name = "rdo32";
			this.rdo32.Size = new System.Drawing.Size(42, 21);
			this.rdo32.TabIndex = 2;
			this.rdo32.TabStop = true;
			this.rdo32.Tag = "20-40";
			this.rdo32.Text = "32";
			this.rdo32.UseVisualStyleBackColor = true;
			//
			//rdo48
			//
			this.rdo48.AutoSize = true;
			this.rdo48.Location = new System.Drawing.Point(261, 3);
			this.rdo48.Name = "rdo48";
			this.rdo48.Size = new System.Drawing.Size(42, 21);
			this.rdo48.TabIndex = 3;
			this.rdo48.TabStop = true;
			this.rdo48.Tag = "40-56";
			this.rdo48.Text = "48";
			this.rdo48.UseVisualStyleBackColor = true;
			//
			//rdo64
			//
			this.rdo64.AutoSize = true;
			this.rdo64.Location = new System.Drawing.Point(309, 3);
			this.rdo64.Name = "rdo64";
			this.rdo64.Size = new System.Drawing.Size(42, 21);
			this.rdo64.TabIndex = 4;
			this.rdo64.TabStop = true;
			this.rdo64.Tag = "56-96";
			this.rdo64.Text = "64";
			this.rdo64.UseVisualStyleBackColor = true;
			//
			//rdo128
			//
			this.rdo128.AutoSize = true;
			this.rdo128.Location = new System.Drawing.Point(357, 3);
			this.rdo128.Name = "rdo128";
			this.rdo128.Size = new System.Drawing.Size(50, 21);
			this.rdo128.TabIndex = 5;
			this.rdo128.TabStop = true;
			this.rdo128.Tag = "96-256";
			this.rdo128.Text = "128";
			this.rdo128.UseVisualStyleBackColor = true;
			//
			//frmResource
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (8.0F), (float) (16.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(622, 533);
			this.Controls.Add(this.pnlImage);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 400);
			this.Name = "frmResource";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Stamp_Title]";
			Panel1.ResumeLayout(false);
			this.pnlImage.ResumeLayout(false);
			this.pnlImage.PerformLayout();
			this.pnlSizes.ResumeLayout(false);
			this.pnlSizes.PerformLayout();
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.ListView lstImages;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TableLayoutPanel pnlImage;
		private PreviewPanel ctrPreview;
		private System.Windows.Forms.ImageList imlSize;
		internal System.Windows.Forms.FlowLayoutPanel pnlSizes;
		private System.Windows.Forms.RadioButton rdoAll;
		private System.Windows.Forms.RadioButton rdo16;
		internal System.Windows.Forms.RadioButton rdo64;
		private System.Windows.Forms.RadioButton rdo32;
		private System.Windows.Forms.RadioButton rdo48;
		private System.Windows.Forms.RadioButton rdo128;
	}
	
}
