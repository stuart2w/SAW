namespace SAW
{
	partial class frmEditBounds
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.lblOuterHeader = new System.Windows.Forms.Label();
			this.txtOuterX = new System.Windows.Forms.TextBox();
			this.txtOuterY = new System.Windows.Forms.TextBox();
			this.txtOuterHeight = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtOuterWidth = new System.Windows.Forms.TextBox();
			this.chkAdjustContents = new System.Windows.Forms.CheckBox();
			this.lblARlocked = new System.Windows.Forms.Label();
			this.txtX = new SAW.CommandEditors.IntegerTextBox();
			this.txtY = new SAW.CommandEditors.IntegerTextBox();
			this.txtWidth = new SAW.CommandEditors.IntegerTextBox();
			this.txtHeight = new SAW.CommandEditors.IntegerTextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.txtX, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtY, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.txtWidth, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.txtHeight, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.btnOK, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.label5, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblOuterHeader, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtOuterX, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.txtOuterY, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.txtOuterHeight, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 7);
			this.tableLayoutPanel1.Controls.Add(this.txtOuterWidth, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.chkAdjustContents, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.lblARlocked, 0, 6);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(487, 216);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "X";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 39);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 26);
			this.label2.TabIndex = 1;
			this.label2.Text = "Y";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(150, 26);
			this.label3.TabIndex = 2;
			this.label3.Text = "[Width]";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 91);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(150, 26);
			this.label4.TabIndex = 3;
			this.label4.Text = "[Height]";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnOK.Location = new System.Drawing.Point(166, 185);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(136, 28);
			this.btnOK.TabIndex = 8;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(159, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "[SAW_ItemBounds]";
			// 
			// lblOuterHeader
			// 
			this.lblOuterHeader.AutoSize = true;
			this.lblOuterHeader.Location = new System.Drawing.Point(315, 0);
			this.lblOuterHeader.Name = "lblOuterHeader";
			this.lblOuterHeader.Size = new System.Drawing.Size(25, 13);
			this.lblOuterHeader.TabIndex = 11;
			this.lblOuterHeader.Text = "???";
			// 
			// txtOuterX
			// 
			this.txtOuterX.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOuterX.Location = new System.Drawing.Point(315, 16);
			this.txtOuterX.Name = "txtOuterX";
			this.txtOuterX.ReadOnly = true;
			this.txtOuterX.Size = new System.Drawing.Size(150, 20);
			this.txtOuterX.TabIndex = 12;
			// 
			// txtOuterY
			// 
			this.txtOuterY.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOuterY.Location = new System.Drawing.Point(315, 42);
			this.txtOuterY.Name = "txtOuterY";
			this.txtOuterY.ReadOnly = true;
			this.txtOuterY.Size = new System.Drawing.Size(150, 20);
			this.txtOuterY.TabIndex = 13;
			// 
			// txtOuterHeight
			// 
			this.txtOuterHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOuterHeight.Location = new System.Drawing.Point(315, 94);
			this.txtOuterHeight.Name = "txtOuterHeight";
			this.txtOuterHeight.ReadOnly = true;
			this.txtOuterHeight.Size = new System.Drawing.Size(150, 20);
			this.txtOuterHeight.TabIndex = 15;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(322, 185);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(136, 28);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// txtOuterWidth
			// 
			this.txtOuterWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOuterWidth.Location = new System.Drawing.Point(315, 68);
			this.txtOuterWidth.Name = "txtOuterWidth";
			this.txtOuterWidth.ReadOnly = true;
			this.txtOuterWidth.Size = new System.Drawing.Size(150, 20);
			this.txtOuterWidth.TabIndex = 16;
			// 
			// chkAdjustContents
			// 
			this.chkAdjustContents.Checked = true;
			this.chkAdjustContents.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tableLayoutPanel1.SetColumnSpan(this.chkAdjustContents, 3);
			this.chkAdjustContents.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkAdjustContents.Location = new System.Drawing.Point(3, 120);
			this.chkAdjustContents.Name = "chkAdjustContents";
			this.chkAdjustContents.Size = new System.Drawing.Size(462, 35);
			this.chkAdjustContents.TabIndex = 17;
			this.chkAdjustContents.Text = "[SAW_ItemBounds_AdjustContents]";
			this.chkAdjustContents.UseVisualStyleBackColor = true;
			// 
			// lblARlocked
			// 
			this.lblARlocked.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblARlocked, 3);
			this.lblARlocked.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblARlocked.Location = new System.Drawing.Point(3, 158);
			this.lblARlocked.Name = "lblARlocked";
			this.lblARlocked.Size = new System.Drawing.Size(462, 24);
			this.lblARlocked.TabIndex = 18;
			this.lblARlocked.Text = "[SAW_ItemBounds_ARLocked]";
			// 
			// txtX
			// 
			this.txtX.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtX.Location = new System.Drawing.Point(159, 16);
			this.txtX.Name = "txtX";
			this.txtX.Size = new System.Drawing.Size(150, 20);
			this.txtX.TabIndex = 4;
			this.txtX.Text = "0";
			this.txtX.Value = 0;
			// 
			// txtY
			// 
			this.txtY.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtY.Location = new System.Drawing.Point(159, 42);
			this.txtY.Name = "txtY";
			this.txtY.Size = new System.Drawing.Size(150, 20);
			this.txtY.TabIndex = 5;
			this.txtY.Text = "0";
			this.txtY.Value = 0;
			// 
			// txtWidth
			// 
			this.txtWidth.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtWidth.Location = new System.Drawing.Point(159, 68);
			this.txtWidth.Name = "txtWidth";
			this.txtWidth.Size = new System.Drawing.Size(150, 20);
			this.txtWidth.TabIndex = 6;
			this.txtWidth.Text = "0";
			this.txtWidth.Value = 0;
			this.txtWidth.TextChanged += new System.EventHandler(this.txtWidth_TextChanged);
			// 
			// txtHeight
			// 
			this.txtHeight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtHeight.Location = new System.Drawing.Point(159, 94);
			this.txtHeight.Name = "txtHeight";
			this.txtHeight.Size = new System.Drawing.Size(150, 20);
			this.txtHeight.TabIndex = 7;
			this.txtHeight.Text = "0";
			this.txtHeight.Value = 0;
			// 
			// frmEditBounds
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(487, 216);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmEditBounds";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "frmEditBounds";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private CommandEditors.IntegerTextBox txtX;
		private CommandEditors.IntegerTextBox txtY;
		private CommandEditors.IntegerTextBox txtWidth;
		private CommandEditors.IntegerTextBox txtHeight;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtOuterX;
		private System.Windows.Forms.TextBox txtOuterY;
		private System.Windows.Forms.TextBox txtOuterHeight;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblOuterHeader;
		private System.Windows.Forms.TextBox txtOuterWidth;
		private System.Windows.Forms.CheckBox chkAdjustContents;
		private System.Windows.Forms.Label lblARlocked;
	}
}