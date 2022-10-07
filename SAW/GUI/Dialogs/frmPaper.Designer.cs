using System;

namespace SAW
{
	internal partial class frmPaper : System.Windows.Forms.Form
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
			this.chkDotted = new System.Windows.Forms.CheckBox();
			this.nudSize = new System.Windows.Forms.NumericUpDown();
			this.nudSecondarySize = new System.Windows.Forms.NumericUpDown();
			this.chkDraw = new System.Windows.Forms.CheckBox();
			this.rdoPlain = new System.Windows.Forms.RadioButton();
			this.rdoGraph = new System.Windows.Forms.RadioButton();
			this.rdoSquare = new System.Windows.Forms.RadioButton();
			this.lblSize = new System.Windows.Forms.Label();
			this.lblSecondarySize = new System.Windows.Forms.Label();
			this.GroupBox1 = new System.Windows.Forms.GroupBox();
			this.pnlSample = new System.Windows.Forms.Panel();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.chkBigDots = new System.Windows.Forms.CheckBox();
			this.lnkBackgroundImage = new System.Windows.Forms.LinkLabel();
			this.lblGridColour = new System.Windows.Forms.Label();
			this.Label3 = new System.Windows.Forms.Label();
			this.chkPrintBackground = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.lnkRemoveBackground = new System.Windows.Forms.LinkLabel();
			this.rdoImage0 = new System.Windows.Forms.RadioButton();
			this.rdoImage1 = new System.Windows.Forms.RadioButton();
			this.rdoImage2 = new System.Windows.Forms.RadioButton();
			this.rdoImage3 = new System.Windows.Forms.RadioButton();
			this.rdoImage4 = new System.Windows.Forms.RadioButton();
			this.rdoImage5 = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lblImageModeHeader = new System.Windows.Forms.Label();
			this.lblImageNoLockHeader = new System.Windows.Forms.Label();
			this.ctrColour = new SAW.ColourPicker();
			this.ctrGridColour = new SAW.ColourPicker();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			((System.ComponentModel.ISupportInitialize)(this.nudSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecondarySize)).BeginInit();
			this.GroupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// chkDotted
			// 
			this.chkDotted.AutoSize = true;
			this.chkDotted.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.chkDotted.Location = new System.Drawing.Point(20, 204);
			this.chkDotted.Margin = new System.Windows.Forms.Padding(4);
			this.chkDotted.Name = "chkDotted";
			this.chkDotted.Size = new System.Drawing.Size(123, 21);
			this.chkDotted.TabIndex = 9;
			this.chkDotted.Text = "[Paper_Dotted]";
			this.chkDotted.UseVisualStyleBackColor = true;
			this.chkDotted.CheckedChanged += new System.EventHandler(this.chkDotted_CheckedChanged);
			// 
			// nudSize
			// 
			this.nudSize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudSize.Location = new System.Drawing.Point(115, 118);
			this.nudSize.Margin = new System.Windows.Forms.Padding(4);
			this.nudSize.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.nudSize.Name = "nudSize";
			this.nudSize.Size = new System.Drawing.Size(64, 23);
			this.nudSize.TabIndex = 18;
			this.nudSize.ValueChanged += new System.EventHandler(this.nudSize_ValueChanged);
			// 
			// nudSecondarySize
			// 
			this.nudSecondarySize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.nudSecondarySize.Location = new System.Drawing.Point(115, 147);
			this.nudSecondarySize.Margin = new System.Windows.Forms.Padding(4);
			this.nudSecondarySize.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.nudSecondarySize.Name = "nudSecondarySize";
			this.nudSecondarySize.Size = new System.Drawing.Size(64, 23);
			this.nudSecondarySize.TabIndex = 21;
			this.nudSecondarySize.ValueChanged += new System.EventHandler(this.nudSecondarySize_ValueChanged);
			// 
			// chkDraw
			// 
			this.chkDraw.AutoSize = true;
			this.chkDraw.Checked = true;
			this.chkDraw.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkDraw.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.chkDraw.Location = new System.Drawing.Point(20, 175);
			this.chkDraw.Margin = new System.Windows.Forms.Padding(4);
			this.chkDraw.Name = "chkDraw";
			this.chkDraw.Size = new System.Drawing.Size(113, 21);
			this.chkDraw.TabIndex = 8;
			this.chkDraw.Text = "[Paper_Draw]";
			this.chkDraw.UseVisualStyleBackColor = true;
			this.chkDraw.CheckedChanged += new System.EventHandler(this.chkDraw_CheckedChanged);
			// 
			// rdoPlain
			// 
			this.rdoPlain.AutoSize = true;
			this.rdoPlain.Location = new System.Drawing.Point(17, 23);
			this.rdoPlain.Margin = new System.Windows.Forms.Padding(4);
			this.rdoPlain.Name = "rdoPlain";
			this.rdoPlain.Size = new System.Drawing.Size(111, 21);
			this.rdoPlain.TabIndex = 1;
			this.rdoPlain.TabStop = true;
			this.rdoPlain.Text = "[Paper_Plain]";
			this.rdoPlain.UseVisualStyleBackColor = true;
			this.rdoPlain.CheckedChanged += new System.EventHandler(this.rdoPlain_CheckedChanged);
			// 
			// rdoGraph
			// 
			this.rdoGraph.AutoSize = true;
			this.rdoGraph.Location = new System.Drawing.Point(17, 81);
			this.rdoGraph.Margin = new System.Windows.Forms.Padding(4);
			this.rdoGraph.Name = "rdoGraph";
			this.rdoGraph.Size = new System.Drawing.Size(120, 21);
			this.rdoGraph.TabIndex = 6;
			this.rdoGraph.TabStop = true;
			this.rdoGraph.Text = "[Paper_Graph]";
			this.rdoGraph.UseVisualStyleBackColor = true;
			this.rdoGraph.CheckedChanged += new System.EventHandler(this.rdoPlain_CheckedChanged);
			// 
			// rdoSquare
			// 
			this.rdoSquare.AutoSize = true;
			this.rdoSquare.Location = new System.Drawing.Point(17, 52);
			this.rdoSquare.Margin = new System.Windows.Forms.Padding(4);
			this.rdoSquare.Name = "rdoSquare";
			this.rdoSquare.Size = new System.Drawing.Size(126, 21);
			this.rdoSquare.TabIndex = 3;
			this.rdoSquare.TabStop = true;
			this.rdoSquare.Text = "[Paper_Square]";
			this.rdoSquare.UseVisualStyleBackColor = true;
			this.rdoSquare.CheckedChanged += new System.EventHandler(this.rdoPlain_CheckedChanged);
			// 
			// lblSize
			// 
			this.lblSize.Location = new System.Drawing.Point(-19, 116);
			this.lblSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSize.Name = "lblSize";
			this.lblSize.Size = new System.Drawing.Size(131, 28);
			this.lblSize.TabIndex = 17;
			this.lblSize.Text = "Label2";
			this.lblSize.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblSecondarySize
			// 
			this.lblSecondarySize.Location = new System.Drawing.Point(-22, 147);
			this.lblSecondarySize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSecondarySize.Name = "lblSecondarySize";
			this.lblSecondarySize.Size = new System.Drawing.Size(137, 23);
			this.lblSecondarySize.TabIndex = 20;
			this.lblSecondarySize.Text = "Label2";
			this.lblSecondarySize.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// GroupBox1
			// 
			this.GroupBox1.Controls.Add(this.pnlSample);
			this.GroupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.GroupBox1.Location = new System.Drawing.Point(528, 61);
			this.GroupBox1.Margin = new System.Windows.Forms.Padding(4);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Padding = new System.Windows.Forms.Padding(4);
			this.GroupBox1.Size = new System.Drawing.Size(299, 360);
			this.GroupBox1.TabIndex = 27;
			this.GroupBox1.TabStop = false;
			this.GroupBox1.Text = "[Paper_Sample]";
			// 
			// pnlSample
			// 
			this.pnlSample.Location = new System.Drawing.Point(11, 27);
			this.pnlSample.Margin = new System.Windows.Forms.Padding(4);
			this.pnlSample.Name = "pnlSample";
			this.pnlSample.Size = new System.Drawing.Size(277, 325);
			this.pnlSample.TabIndex = 0;
			this.pnlSample.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSample_Paint);
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(12, 407);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(92, 38);
			this.btnOK.TabIndex = 26;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(112, 407);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(92, 38);
			this.btnCancel.TabIndex = 27;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// chkBigDots
			// 
			this.chkBigDots.AutoSize = true;
			this.chkBigDots.Location = new System.Drawing.Point(20, 232);
			this.chkBigDots.Name = "chkBigDots";
			this.chkBigDots.Size = new System.Drawing.Size(130, 21);
			this.chkBigDots.TabIndex = 10;
			this.chkBigDots.Text = "[Paper_BigDots]";
			this.chkBigDots.UseVisualStyleBackColor = true;
			this.chkBigDots.CheckedChanged += new System.EventHandler(this.chkBigDots_CheckedChanged);
			// 
			// lnkBackgroundImage
			// 
			this.lnkBackgroundImage.AutoSize = true;
			this.lnkBackgroundImage.Location = new System.Drawing.Point(5, 26);
			this.lnkBackgroundImage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkBackgroundImage.Name = "lnkBackgroundImage";
			this.lnkBackgroundImage.Size = new System.Drawing.Size(176, 17);
			this.lnkBackgroundImage.TabIndex = 15;
			this.lnkBackgroundImage.TabStop = true;
			this.lnkBackgroundImage.Text = "[Paper_BackgroundImage]";
			this.lnkBackgroundImage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBackgroundImage_LinkClicked);
			// 
			// lblGridColour
			// 
			this.lblGridColour.Location = new System.Drawing.Point(81, 262);
			this.lblGridColour.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblGridColour.Name = "lblGridColour";
			this.lblGridColour.Size = new System.Drawing.Size(139, 39);
			this.lblGridColour.TabIndex = 12;
			this.lblGridColour.Text = "[Paper_GridColour]";
			// 
			// Label3
			// 
			this.Label3.Location = new System.Drawing.Point(603, 13);
			this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(139, 39);
			this.Label3.TabIndex = 14;
			this.Label3.Text = "[Paper_Colour]";
			// 
			// chkPrintBackground
			// 
			this.chkPrintBackground.Location = new System.Drawing.Point(539, 429);
			this.chkPrintBackground.Margin = new System.Windows.Forms.Padding(4);
			this.chkPrintBackground.Name = "chkPrintBackground";
			this.chkPrintBackground.Size = new System.Drawing.Size(277, 24);
			this.chkPrintBackground.TabIndex = 25;
			this.chkPrintBackground.Text = "[Paper_Print_Background]";
			this.chkPrintBackground.UseVisualStyleBackColor = true;
			this.chkPrintBackground.CheckedChanged += new System.EventHandler(this.chkPrintBackground_CheckedChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(25, 314);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(193, 64);
			this.label5.TabIndex = 33;
			this.label5.Text = "[Paper_NoRun]";
			// 
			// lnkRemoveBackground
			// 
			this.lnkRemoveBackground.AutoSize = true;
			this.lnkRemoveBackground.Location = new System.Drawing.Point(149, 26);
			this.lnkRemoveBackground.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lnkRemoveBackground.Name = "lnkRemoveBackground";
			this.lnkRemoveBackground.Size = new System.Drawing.Size(152, 17);
			this.lnkRemoveBackground.TabIndex = 34;
			this.lnkRemoveBackground.TabStop = true;
			this.lnkRemoveBackground.Text = "[Paper_RemoveImage]";
			this.lnkRemoveBackground.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRemoveBackground_LinkClicked);
			// 
			// rdoImage0
			// 
			this.rdoImage0.AutoSize = true;
			this.rdoImage0.Image = global::Resources.AM.ImageFit_0;
			this.rdoImage0.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.rdoImage0.Location = new System.Drawing.Point(8, 186);
			this.rdoImage0.Name = "rdoImage0";
			this.rdoImage0.Size = new System.Drawing.Size(306, 48);
			this.rdoImage0.TabIndex = 35;
			this.rdoImage0.TabStop = true;
			this.rdoImage0.Tag = "0";
			this.rdoImage0.Text = "[Paper_BackgroundFitWithin]";
			this.rdoImage0.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.rdoImage0.UseVisualStyleBackColor = true;
			// 
			// rdoImage1
			// 
			this.rdoImage1.AutoSize = true;
			this.rdoImage1.Image = global::Resources.AM.ImageFit_1;
			this.rdoImage1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.rdoImage1.Location = new System.Drawing.Point(8, 234);
			this.rdoImage1.Name = "rdoImage1";
			this.rdoImage1.Size = new System.Drawing.Size(297, 48);
			this.rdoImage1.TabIndex = 36;
			this.rdoImage1.TabStop = true;
			this.rdoImage1.Tag = "1";
			this.rdoImage1.Text = "[Paper_BackgroundStretch]";
			this.rdoImage1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.rdoImage1.UseVisualStyleBackColor = true;
			// 
			// rdoImage2
			// 
			this.rdoImage2.AutoSize = true;
			this.rdoImage2.Image = global::Resources.AM.ImageFit_2;
			this.rdoImage2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.rdoImage2.Location = new System.Drawing.Point(8, 282);
			this.rdoImage2.Name = "rdoImage2";
			this.rdoImage2.Size = new System.Drawing.Size(316, 48);
			this.rdoImage2.TabIndex = 37;
			this.rdoImage2.TabStop = true;
			this.rdoImage2.Tag = "2";
			this.rdoImage2.Text = "[Paper_BackgroundFitOutside]";
			this.rdoImage2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.rdoImage2.UseVisualStyleBackColor = true;
			// 
			// rdoImage3
			// 
			this.rdoImage3.AutoSize = true;
			this.rdoImage3.Image = global::Resources.AM.ImageFit_3;
			this.rdoImage3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.rdoImage3.Location = new System.Drawing.Point(8, 330);
			this.rdoImage3.Name = "rdoImage3";
			this.rdoImage3.Size = new System.Drawing.Size(294, 48);
			this.rdoImage3.TabIndex = 38;
			this.rdoImage3.TabStop = true;
			this.rdoImage3.Tag = "3";
			this.rdoImage3.Text = "[Paper_BackgroundCentre]";
			this.rdoImage3.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.rdoImage3.UseVisualStyleBackColor = true;
			// 
			// rdoImage4
			// 
			this.rdoImage4.AutoSize = true;
			this.rdoImage4.Image = global::Resources.AM.ImageFit_4;
			this.rdoImage4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.rdoImage4.Location = new System.Drawing.Point(8, 378);
			this.rdoImage4.Name = "rdoImage4";
			this.rdoImage4.Size = new System.Drawing.Size(275, 48);
			this.rdoImage4.TabIndex = 39;
			this.rdoImage4.TabStop = true;
			this.rdoImage4.Tag = "4";
			this.rdoImage4.Text = "[Paper_BackgroundTile]";
			this.rdoImage4.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.rdoImage4.UseVisualStyleBackColor = true;
			// 
			// rdoImage5
			// 
			this.rdoImage5.Location = new System.Drawing.Point(8, 67);
			this.rdoImage5.Name = "rdoImage5";
			this.rdoImage5.Size = new System.Drawing.Size(296, 61);
			this.rdoImage5.TabIndex = 40;
			this.rdoImage5.TabStop = true;
			this.rdoImage5.Tag = "5";
			this.rdoImage5.Text = "[Paper_BackgroundLockAR]";
			this.rdoImage5.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lblImageNoLockHeader);
			this.groupBox2.Controls.Add(this.lblImageModeHeader);
			this.groupBox2.Controls.Add(this.rdoImage5);
			this.groupBox2.Controls.Add(this.lnkBackgroundImage);
			this.groupBox2.Controls.Add(this.rdoImage4);
			this.groupBox2.Controls.Add(this.lnkRemoveBackground);
			this.groupBox2.Controls.Add(this.rdoImage3);
			this.groupBox2.Controls.Add(this.rdoImage0);
			this.groupBox2.Controls.Add(this.rdoImage2);
			this.groupBox2.Controls.Add(this.rdoImage1);
			this.groupBox2.Location = new System.Drawing.Point(224, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(308, 448);
			this.groupBox2.TabIndex = 41;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "[Paper_ImageHeader]";
			// 
			// lblImageModeHeader
			// 
			this.lblImageModeHeader.AutoSize = true;
			this.lblImageModeHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblImageModeHeader.Location = new System.Drawing.Point(8, 63);
			this.lblImageModeHeader.Name = "lblImageModeHeader";
			this.lblImageModeHeader.Size = new System.Drawing.Size(195, 17);
			this.lblImageModeHeader.TabIndex = 41;
			this.lblImageModeHeader.Text = "[Paper_BackgroundMode]";
			// 
			// lblImageNoLockHeader
			// 
			this.lblImageNoLockHeader.Location = new System.Drawing.Point(10, 118);
			this.lblImageNoLockHeader.Name = "lblImageNoLockHeader";
			this.lblImageNoLockHeader.Size = new System.Drawing.Size(292, 65);
			this.lblImageNoLockHeader.TabIndex = 42;
			this.lblImageNoLockHeader.Text = "[Paper_BackgroundNoLockHeader]";
			this.lblImageNoLockHeader.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// ctrColour
			// 
			this.ctrColour.AllowTransparent = false;
			this.ctrColour.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctrColour.Location = new System.Drawing.Point(539, 13);
			this.ctrColour.Margin = new System.Windows.Forms.Padding(4);
			this.ctrColour.Name = "ctrColour";
			this.ctrColour.Size = new System.Drawing.Size(53, 28);
			this.ctrColour.TabIndex = 13;
			this.ctrColour.UserChangedColour += new System.EventHandler(this.ctrColour_UserChangedColour);
			// 
			// ctrGridColour
			// 
			this.ctrGridColour.AllowTransparent = false;
			this.ctrGridColour.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctrGridColour.Location = new System.Drawing.Point(17, 262);
			this.ctrGridColour.Margin = new System.Windows.Forms.Padding(4);
			this.ctrGridColour.Name = "ctrGridColour";
			this.ctrGridColour.Size = new System.Drawing.Size(53, 28);
			this.ctrGridColour.TabIndex = 11;
			this.ctrGridColour.UserChangedColour += new System.EventHandler(this.ctrGridColour_UserChangedColour);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.nudSecondarySize);
			this.groupBox3.Controls.Add(this.chkBigDots);
			this.groupBox3.Controls.Add(this.nudSize);
			this.groupBox3.Controls.Add(this.rdoPlain);
			this.groupBox3.Controls.Add(this.rdoGraph);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.rdoSquare);
			this.groupBox3.Controls.Add(this.lblSize);
			this.groupBox3.Controls.Add(this.ctrGridColour);
			this.groupBox3.Controls.Add(this.lblSecondarySize);
			this.groupBox3.Controls.Add(this.chkDotted);
			this.groupBox3.Controls.Add(this.chkDraw);
			this.groupBox3.Controls.Add(this.lblGridColour);
			this.groupBox3.Location = new System.Drawing.Point(4, 6);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(214, 394);
			this.groupBox3.TabIndex = 42;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "[Paper_Type]";
			// 
			// frmPaper
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(842, 466);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.ctrColour);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.chkPrintBackground);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.GroupBox1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPaper";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Paper_Title]";
			this.Load += new System.EventHandler(this.frmPaper_Load);
			((System.ComponentModel.ISupportInitialize)(this.nudSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudSecondarySize)).EndInit();
			this.GroupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.RadioButton rdoPlain;
		private System.Windows.Forms.RadioButton rdoGraph;
		private System.Windows.Forms.RadioButton rdoSquare;
		private System.Windows.Forms.Label lblSize;
		private System.Windows.Forms.Label lblSecondarySize;
		private System.Windows.Forms.GroupBox GroupBox1;
		private System.Windows.Forms.CheckBox chkDotted;
		private System.Windows.Forms.NumericUpDown nudSize;
		private System.Windows.Forms.NumericUpDown nudSecondarySize;
		private System.Windows.Forms.CheckBox chkDraw;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel pnlSample;
		private System.Windows.Forms.Label Label3;
		private ColourPicker ctrColour;
		private ColourPicker ctrGridColour;
		private System.Windows.Forms.Label lblGridColour;
		private System.Windows.Forms.LinkLabel lnkBackgroundImage;
		private System.Windows.Forms.CheckBox chkBigDots;
		private System.Windows.Forms.CheckBox chkPrintBackground;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.LinkLabel lnkRemoveBackground;
		private System.Windows.Forms.RadioButton rdoImage0;
		private System.Windows.Forms.RadioButton rdoImage1;
		private System.Windows.Forms.RadioButton rdoImage2;
		private System.Windows.Forms.RadioButton rdoImage3;
		private System.Windows.Forms.RadioButton rdoImage4;
		private System.Windows.Forms.RadioButton rdoImage5;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label lblImageModeHeader;
		private System.Windows.Forms.Label lblImageNoLockHeader;
		private System.Windows.Forms.GroupBox groupBox3;
	}
	
}
