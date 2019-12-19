namespace SAW
{
	public partial class ctrTextStyle : System.Windows.Forms.UserControl
	{
		
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrTextStyle));
			this.btnChoose = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(ctrTextStyle_Load);
			this.Resize += new System.EventHandler(ctrTextStyle_Resize);
			this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click);
			this.btnSmaller = new System.Windows.Forms.Button();
			this.btnSmaller.Click += new System.EventHandler(this.btnSmaller_Click);
			this.btnLarger = new System.Windows.Forms.Button();
			this.btnLarger.Click += new System.EventHandler(this.btnLarger_Click);
			this.ctrColour = new ColourPicker();
			this.pnlAlignment = new ButtonPanel();
			this.pnlAll = new System.Windows.Forms.TableLayoutPanel();
			this.pnlAll.SuspendLayout();
			this.SuspendLayout();
			//
			//btnChoose
			//
			this.btnChoose.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnChoose.Location = new System.Drawing.Point(3, 3);
			this.btnChoose.Name = "btnChoose";
			this.btnChoose.Size = new System.Drawing.Size(96, 28);
			this.btnChoose.TabIndex = 0;
			this.btnChoose.Text = "[Palette_ChooseFont]";
			this.btnChoose.UseVisualStyleBackColor = true;
			//
			//btnSmaller
			//
			this.btnSmaller.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSmaller.Image = (System.Drawing.Image) (resources.GetObject("btnSmaller.Image"));
			this.btnSmaller.Location = new System.Drawing.Point(104, 3);
			this.btnSmaller.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.btnSmaller.Name = "btnSmaller";
			this.btnSmaller.Size = new System.Drawing.Size(28, 28);
			this.btnSmaller.TabIndex = 1;
			this.btnSmaller.UseVisualStyleBackColor = true;
			//
			//btnLarger
			//
			this.btnLarger.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnLarger.Image = Resources.AM.TextLarger;
			this.btnLarger.Location = new System.Drawing.Point(136, 3);
			this.btnLarger.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.btnLarger.Name = "btnLarger";
			this.btnLarger.Size = new System.Drawing.Size(28, 28);
			this.btnLarger.TabIndex = 2;
			this.btnLarger.UseVisualStyleBackColor = true;
			//
			//ctrColour
			//
			this.ctrColour.AllowEmpty = true;
			this.ctrColour.BlackWhite = true;
			this.ctrColour.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctrColour.DisplayCurrentColour = false;
			this.ctrColour.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrColour.Location = new System.Drawing.Point(169, 3);
			this.ctrColour.Name = "ctrColour";
			this.ctrColour.Size = new System.Drawing.Size(28, 28);
			this.ctrColour.TabIndex = 3;
			//
			//pnlAlignment
			//
			this.pnlAlignment.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlAlignment.ButtonSize = 32;
			this.pnlAll.SetColumnSpan(this.pnlAlignment, 4);
			this.pnlAlignment.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlAlignment.DoubleBuffered = false;
			this.pnlAlignment.Location = new System.Drawing.Point(3, 37);
			this.pnlAlignment.Name = "pnlAlignment";
			this.pnlAlignment.Size = new System.Drawing.Size(194, 29);
			this.pnlAlignment.TabIndex = 4;
			//
			//pnlAll
			//
			this.pnlAll.ColumnCount = 4;
			this.pnlAll.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (51.02041F)));
			this.pnlAll.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (16.32653F)));
			this.pnlAll.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (16.32653F)));
			this.pnlAll.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (16.32653F)));
			this.pnlAll.Controls.Add(this.btnChoose, 0, 0);
			this.pnlAll.Controls.Add(this.pnlAlignment, 0, 1);
			this.pnlAll.Controls.Add(this.ctrColour, 3, 0);
			this.pnlAll.Controls.Add(this.btnSmaller, 1, 0);
			this.pnlAll.Controls.Add(this.btnLarger, 2, 0);
			this.pnlAll.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlAll.Location = new System.Drawing.Point(0, 0);
			this.pnlAll.Margin = new System.Windows.Forms.Padding(0);
			this.pnlAll.Name = "pnlAll";
			this.pnlAll.RowCount = 2;
			this.pnlAll.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float) (50.0F)));
			this.pnlAll.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float) (50.0F)));
			this.pnlAll.Size = new System.Drawing.Size(200, 69);
			this.pnlAll.TabIndex = 5;
			//
			//ctrTextStyle
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (13.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlAll);
			this.Name = "ctrTextStyle";
			this.Size = new System.Drawing.Size(200, 69);
			this.pnlAll.ResumeLayout(false);
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnChoose;
		private ButtonPanel pnlAlignment;
		private System.Windows.Forms.Button btnSmaller;
		private System.Windows.Forms.Button btnLarger;
		private ColourPicker ctrColour;
		private System.Windows.Forms.TableLayoutPanel pnlAll;
		
	}
	
}
