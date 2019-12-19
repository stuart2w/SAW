namespace SAW.CommandEditors
{
	partial class MoveWindowEditor
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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblOpen = new System.Windows.Forms.Label();
			this.txtApplication = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.chkUseOutput = new System.Windows.Forms.CheckBox();
			this.btnRunningApps = new System.Windows.Forms.Button();
			this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
			this.ctxRunningApplications = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtX = new SAW.CommandEditors.IntegerTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtY = new SAW.CommandEditors.IntegerTextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblOpen, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtApplication, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnBrowse, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.chkUseOutput, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnRunningApps, 0, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(310, 297);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblOpen
			// 
			this.lblOpen.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblOpen, 2);
			this.lblOpen.Location = new System.Drawing.Point(3, 0);
			this.lblOpen.Name = "lblOpen";
			this.lblOpen.Size = new System.Drawing.Size(207, 13);
			this.lblOpen.TabIndex = 0;
			this.lblOpen.Text = "[SAW_CommandEdit_ApplicationToMove]";
			// 
			// txtApplication
			// 
			this.txtApplication.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtApplication.Location = new System.Drawing.Point(3, 16);
			this.txtApplication.Name = "txtApplication";
			this.txtApplication.Size = new System.Drawing.Size(274, 20);
			this.txtApplication.TabIndex = 1;
			this.txtApplication.TextChanged += new System.EventHandler(this.txtApplication_TextChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(283, 16);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(24, 23);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// chkUseOutput
			// 
			this.chkUseOutput.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkUseOutput, 2);
			this.chkUseOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkUseOutput.Location = new System.Drawing.Point(3, 45);
			this.chkUseOutput.Name = "chkUseOutput";
			this.chkUseOutput.Size = new System.Drawing.Size(304, 17);
			this.chkUseOutput.TabIndex = 9;
			this.chkUseOutput.Text = "[SAW_CommandEdit_ApplicatonUseOutput]";
			this.chkUseOutput.UseVisualStyleBackColor = true;
			this.chkUseOutput.CheckedChanged += new System.EventHandler(this.chkUseOutput_CheckedChanged);
			// 
			// btnRunningApps
			// 
			this.btnRunningApps.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnRunningApps.AutoSize = true;
			this.btnRunningApps.Location = new System.Drawing.Point(57, 68);
			this.btnRunningApps.Name = "btnRunningApps";
			this.btnRunningApps.Size = new System.Drawing.Size(165, 23);
			this.btnRunningApps.TabIndex = 10;
			this.btnRunningApps.Text = "[SAW_CommandEdit_Running]";
			this.btnRunningApps.UseVisualStyleBackColor = true;
			this.btnRunningApps.Click += new System.EventHandler(this.btnRunningApps_Click);
			// 
			// ctxRunningApplications
			// 
			this.ctxRunningApplications.Name = "ctxRunningApplications";
			this.ctxRunningApplications.Size = new System.Drawing.Size(61, 4);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.txtX);
			this.flowLayoutPanel1.Controls.Add(this.label2);
			this.flowLayoutPanel1.Controls.Add(this.txtY);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 94);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(310, 26);
			this.flowLayoutPanel1.TabIndex = 11;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "X:";
			// 
			// txtX
			// 
			this.txtX.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtX.Location = new System.Drawing.Point(26, 3);
			this.txtX.Name = "txtX";
			this.txtX.Size = new System.Drawing.Size(52, 20);
			this.txtX.TabIndex = 1;
			this.txtX.Value = 0;
			this.txtX.TextChanged += new System.EventHandler(this.txtX_TextChanged);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(84, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Y:";
			// 
			// txtY
			// 
			this.txtY.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtY.Location = new System.Drawing.Point(107, 3);
			this.txtY.Name = "txtY";
			this.txtY.Size = new System.Drawing.Size(52, 20);
			this.txtY.TabIndex = 3;
			this.txtY.Value = 0;
			this.txtY.TextChanged += new System.EventHandler(this.txtY_TextChanged);
			// 
			// MoveWindowEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "MoveWindowEditor";
			this.Size = new System.Drawing.Size(310, 297);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblOpen;
		private System.Windows.Forms.TextBox txtApplication;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.CheckBox chkUseOutput;
		private System.Windows.Forms.OpenFileDialog dlgOpen;
		private System.Windows.Forms.Button btnRunningApps;
		private System.Windows.Forms.ContextMenuStrip ctxRunningApplications;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private IntegerTextBox txtX;
		private System.Windows.Forms.Label label2;
		private IntegerTextBox txtY;
	}
}
