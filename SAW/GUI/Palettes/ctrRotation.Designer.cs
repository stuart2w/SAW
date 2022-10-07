namespace SAW
{
	internal partial class ctrRotation
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
			this.tableMain = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.rdoCentres = new System.Windows.Forms.RadioButton();
			this.flowPoint = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoPoint = new System.Windows.Forms.RadioButton();
			this.txtX = new SAW.CommandEditors.IntegerTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtY = new SAW.CommandEditors.IntegerTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.flowDoRotate = new System.Windows.Forms.FlowLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.txtAngle = new SAW.CommandEditors.IntegerTextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.btnDoRotate = new System.Windows.Forms.Button();
			this.chkIncludeText = new System.Windows.Forms.CheckBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnSet = new System.Windows.Forms.Button();
			this.btnPreviousPoints = new System.Windows.Forms.Button();
			this.tmrTextChange = new System.Windows.Forms.Timer(this.components);
			this.tableMain.SuspendLayout();
			this.flowPoint.SuspendLayout();
			this.flowDoRotate.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableMain
			// 
			this.tableMain.ColumnCount = 1;
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableMain.Controls.Add(this.label1, 0, 0);
			this.tableMain.Controls.Add(this.rdoCentres, 0, 1);
			this.tableMain.Controls.Add(this.flowPoint, 0, 2);
			this.tableMain.Controls.Add(this.label3, 0, 4);
			this.tableMain.Controls.Add(this.flowDoRotate, 0, 5);
			this.tableMain.Controls.Add(this.chkIncludeText, 0, 6);
			this.tableMain.Controls.Add(this.flowLayoutPanel1, 0, 3);
			this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableMain.Location = new System.Drawing.Point(0, 0);
			this.tableMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableMain.Name = "tableMain";
			this.tableMain.RowCount = 8;
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableMain.Size = new System.Drawing.Size(258, 205);
			this.tableMain.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Rotate_About]";
			// 
			// rdoCentres
			// 
			this.rdoCentres.AutoSize = true;
			this.rdoCentres.Location = new System.Drawing.Point(3, 16);
			this.rdoCentres.Name = "rdoCentres";
			this.rdoCentres.Size = new System.Drawing.Size(136, 17);
			this.rdoCentres.TabIndex = 1;
			this.rdoCentres.TabStop = true;
			this.rdoCentres.Text = "[SAW_Rotate_Centres]";
			this.rdoCentres.UseVisualStyleBackColor = true;
			this.rdoCentres.CheckedChanged += new System.EventHandler(this.rdoCentres_CheckedChanged);
			// 
			// flowPoint
			// 
			this.flowPoint.AutoSize = true;
			this.flowPoint.Controls.Add(this.rdoPoint);
			this.flowPoint.Controls.Add(this.txtX);
			this.flowPoint.Controls.Add(this.label2);
			this.flowPoint.Controls.Add(this.txtY);
			this.flowPoint.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowPoint.Location = new System.Drawing.Point(0, 36);
			this.flowPoint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.flowPoint.Name = "flowPoint";
			this.flowPoint.Size = new System.Drawing.Size(258, 23);
			this.flowPoint.TabIndex = 2;
			// 
			// rdoPoint
			// 
			this.rdoPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.rdoPoint.AutoSize = true;
			this.rdoPoint.Location = new System.Drawing.Point(3, 3);
			this.rdoPoint.Name = "rdoPoint";
			this.rdoPoint.Size = new System.Drawing.Size(38, 17);
			this.rdoPoint.TabIndex = 0;
			this.rdoPoint.TabStop = true;
			this.rdoPoint.Text = "X=";
			this.rdoPoint.UseVisualStyleBackColor = true;
			this.rdoPoint.CheckedChanged += new System.EventHandler(this.rdoPoint_CheckedChanged);
			// 
			// txtX
			// 
			this.txtX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtX.Location = new System.Drawing.Point(47, 3);
			this.txtX.Name = "txtX";
			this.txtX.Size = new System.Drawing.Size(32, 20);
			this.txtX.TabIndex = 1;
			this.txtX.Text = "0";
			this.txtX.Value = 0;
			this.txtX.TextChanged += new System.EventHandler(this.txtXY_TextChanged);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(85, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(20, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Y=";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtY
			// 
			this.txtY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtY.Location = new System.Drawing.Point(111, 3);
			this.txtY.Name = "txtY";
			this.txtY.Size = new System.Drawing.Size(32, 20);
			this.txtY.TabIndex = 3;
			this.txtY.Text = "0";
			this.txtY.Value = 0;
			this.txtY.TextChanged += new System.EventHandler(this.txtXY_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 91);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "[SAW_Rotate_How]";
			// 
			// flowDoRotate
			// 
			this.flowDoRotate.AutoSize = true;
			this.flowDoRotate.Controls.Add(this.label4);
			this.flowDoRotate.Controls.Add(this.txtAngle);
			this.flowDoRotate.Controls.Add(this.label5);
			this.flowDoRotate.Controls.Add(this.btnDoRotate);
			this.flowDoRotate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowDoRotate.Location = new System.Drawing.Point(0, 104);
			this.flowDoRotate.Margin = new System.Windows.Forms.Padding(0);
			this.flowDoRotate.MinimumSize = new System.Drawing.Size(0, 23);
			this.flowDoRotate.Name = "flowDoRotate";
			this.flowDoRotate.Size = new System.Drawing.Size(258, 55);
			this.flowDoRotate.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(109, 26);
			this.label4.TabIndex = 0;
			this.label4.Text = "[SAW_Rotate_Angle]";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtAngle
			// 
			this.txtAngle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtAngle.Location = new System.Drawing.Point(118, 3);
			this.txtAngle.MinimumSize = new System.Drawing.Size(4, 20);
			this.txtAngle.Name = "txtAngle";
			this.txtAngle.Size = new System.Drawing.Size(36, 20);
			this.txtAngle.TabIndex = 1;
			this.txtAngle.Text = "0";
			this.txtAngle.Value = 0;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(157, 0);
			this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(11, 26);
			this.label5.TabIndex = 2;
			this.label5.Text = "°";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnDoRotate
			// 
			this.btnDoRotate.AutoSize = true;
			this.btnDoRotate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnDoRotate.Enabled = false;
			this.btnDoRotate.Location = new System.Drawing.Point(3, 29);
			this.btnDoRotate.Name = "btnDoRotate";
			this.btnDoRotate.Size = new System.Drawing.Size(106, 23);
			this.btnDoRotate.TabIndex = 3;
			this.btnDoRotate.Text = "[SAW_Rotate_Do]";
			this.btnDoRotate.UseVisualStyleBackColor = true;
			this.btnDoRotate.Click += new System.EventHandler(this.btnDoRotate_Click);
			// 
			// chkIncludeText
			// 
			this.chkIncludeText.AutoSize = true;
			this.chkIncludeText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkIncludeText.Location = new System.Drawing.Point(3, 162);
			this.chkIncludeText.Name = "chkIncludeText";
			this.chkIncludeText.Size = new System.Drawing.Size(252, 17);
			this.chkIncludeText.TabIndex = 6;
			this.chkIncludeText.Text = "[SAW_Rotate_IncludeText]";
			this.chkIncludeText.UseVisualStyleBackColor = true;
			this.chkIncludeText.CheckedChanged += new System.EventHandler(this.chkIncludeText_CheckedChanged);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.btnSet);
			this.flowLayoutPanel1.Controls.Add(this.btnPreviousPoints);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 62);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(258, 29);
			this.flowLayoutPanel1.TabIndex = 7;
			// 
			// btnSet
			// 
			this.btnSet.AutoSize = true;
			this.btnSet.Location = new System.Drawing.Point(3, 3);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(108, 23);
			this.btnSet.TabIndex = 3;
			this.btnSet.Text = "[SAW_Rotate_Set]";
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
			// 
			// btnPreviousPoints
			// 
			this.btnPreviousPoints.AutoSize = true;
			this.btnPreviousPoints.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnPreviousPoints.Enabled = false;
			this.btnPreviousPoints.Location = new System.Drawing.Point(117, 3);
			this.btnPreviousPoints.Name = "btnPreviousPoints";
			this.btnPreviousPoints.Size = new System.Drawing.Size(133, 23);
			this.btnPreviousPoints.TabIndex = 4;
			this.btnPreviousPoints.Text = "[SAW_Rotate_Previous]";
			this.btnPreviousPoints.UseVisualStyleBackColor = true;
			this.btnPreviousPoints.Click += new System.EventHandler(this.btnPreviousPoints_Click);
			// 
			// tmrTextChange
			// 
			this.tmrTextChange.Interval = 300;
			this.tmrTextChange.Tick += new System.EventHandler(this.tmrTextChange_Tick);
			// 
			// ctrRotation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableMain);
			this.MinimumSize = new System.Drawing.Size(200, 145);
			this.Name = "ctrRotation";
			this.Size = new System.Drawing.Size(258, 205);
			this.tableMain.ResumeLayout(false);
			this.tableMain.PerformLayout();
			this.flowPoint.ResumeLayout(false);
			this.flowPoint.PerformLayout();
			this.flowDoRotate.ResumeLayout(false);
			this.flowDoRotate.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableMain;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rdoCentres;
		private System.Windows.Forms.FlowLayoutPanel flowPoint;
		private System.Windows.Forms.RadioButton rdoPoint;
		private SAW.CommandEditors.IntegerTextBox txtX;
		private System.Windows.Forms.Label label2;
		private SAW.CommandEditors.IntegerTextBox txtY;
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.FlowLayoutPanel flowDoRotate;
		private System.Windows.Forms.Label label4;
		private SAW.CommandEditors.IntegerTextBox txtAngle;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnDoRotate;
		private System.Windows.Forms.Timer tmrTextChange;
		private System.Windows.Forms.CheckBox chkIncludeText;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button btnPreviousPoints;
	}
}
