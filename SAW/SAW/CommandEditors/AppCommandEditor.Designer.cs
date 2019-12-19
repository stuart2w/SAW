namespace SAW.CommandEditors
{
	partial class AppCommandEditor
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
			this.lblParams = new System.Windows.Forms.Label();
			this.txtParams = new System.Windows.Forms.TextBox();
			this.lblParams2 = new System.Windows.Forms.Label();
			this.txtParams2 = new System.Windows.Forms.TextBox();
			this.lblAlternateID = new System.Windows.Forms.Label();
			this.txtAlternateID = new System.Windows.Forms.TextBox();
			this.chkUseOutput = new System.Windows.Forms.CheckBox();
			this.btnRunningApps = new System.Windows.Forms.Button();
			this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
			this.ctxRunningApplications = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Controls.Add(this.lblOpen, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtApplication, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnBrowse, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblParams, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.txtParams, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblParams2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.txtParams2, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.lblAlternateID, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.txtAlternateID, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.chkUseOutput, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnRunningApps, 0, 9);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 11;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
			this.lblOpen.Size = new System.Drawing.Size(204, 13);
			this.lblOpen.TabIndex = 0;
			this.lblOpen.Text = "[SAW_CommandEdit_ApplicatonToOpen]";
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
			// lblParams
			// 
			this.lblParams.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblParams, 2);
			this.lblParams.Location = new System.Drawing.Point(3, 65);
			this.lblParams.Name = "lblParams";
			this.lblParams.Size = new System.Drawing.Size(202, 13);
			this.lblParams.TabIndex = 3;
			this.lblParams.Text = "[SAW_CommandEdit_ApplicationParams]";
			// 
			// txtParams
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtParams, 2);
			this.txtParams.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtParams.Location = new System.Drawing.Point(3, 81);
			this.txtParams.Name = "txtParams";
			this.txtParams.Size = new System.Drawing.Size(304, 20);
			this.txtParams.TabIndex = 4;
			this.txtParams.TextChanged += new System.EventHandler(this.txtParams_TextChanged);
			// 
			// lblParams2
			// 
			this.lblParams2.AutoSize = true;
			this.lblParams2.Location = new System.Drawing.Point(3, 104);
			this.lblParams2.Name = "lblParams2";
			this.lblParams2.Size = new System.Drawing.Size(214, 13);
			this.lblParams2.TabIndex = 5;
			this.lblParams2.Text = "[SAW_CommandEdit_ApplicationAltParams]";
			// 
			// txtParams2
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtParams2, 2);
			this.txtParams2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtParams2.Location = new System.Drawing.Point(3, 120);
			this.txtParams2.Name = "txtParams2";
			this.txtParams2.Size = new System.Drawing.Size(304, 20);
			this.txtParams2.TabIndex = 6;
			this.txtParams2.TextChanged += new System.EventHandler(this.txtParams2_TextChanged);
			// 
			// lblAlternateID
			// 
			this.lblAlternateID.AutoSize = true;
			this.lblAlternateID.Location = new System.Drawing.Point(3, 143);
			this.lblAlternateID.Name = "lblAlternateID";
			this.lblAlternateID.Size = new System.Drawing.Size(190, 13);
			this.lblAlternateID.TabIndex = 7;
			this.lblAlternateID.Text = "[SAW_CommandEdit_ApplicationAltID]";
			// 
			// txtAlternateID
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.txtAlternateID, 2);
			this.txtAlternateID.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtAlternateID.Location = new System.Drawing.Point(3, 159);
			this.txtAlternateID.Name = "txtAlternateID";
			this.txtAlternateID.Size = new System.Drawing.Size(304, 20);
			this.txtAlternateID.TabIndex = 8;
			this.txtAlternateID.TextChanged += new System.EventHandler(this.txtAlternateID_TextChanged);
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
			this.btnRunningApps.Location = new System.Drawing.Point(57, 185);
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
			// AppCommandEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "AppCommandEditor";
			this.Size = new System.Drawing.Size(310, 297);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblOpen;
		private System.Windows.Forms.TextBox txtApplication;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label lblParams;
		private System.Windows.Forms.TextBox txtParams;
		private System.Windows.Forms.Label lblParams2;
		private System.Windows.Forms.TextBox txtParams2;
		private System.Windows.Forms.Label lblAlternateID;
		private System.Windows.Forms.TextBox txtAlternateID;
		private System.Windows.Forms.CheckBox chkUseOutput;
		private System.Windows.Forms.OpenFileDialog dlgOpen;
		private System.Windows.Forms.Button btnRunningApps;
		private System.Windows.Forms.ContextMenuStrip ctxRunningApplications;
	}
}
