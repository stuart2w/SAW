namespace SAW.CommandEditors
{
	partial class YoctoEditor
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
			this.tableMain = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.rdoAll = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbAction = new System.Windows.Forms.ComboBox();
			this.lblDuration = new System.Windows.Forms.Label();
			this.lblRelayNotFound = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cmbRelayID = new System.Windows.Forms.ComboBox();
			this.rdoSingle = new System.Windows.Forms.RadioButton();
			this.lblQueryDurationNote = new System.Windows.Forms.Label();
			this.txtDuration = new SAW.CommandEditors.IntegerTextBox();
			this.tableMain.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableMain
			// 
			this.tableMain.ColumnCount = 2;
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableMain.Controls.Add(this.label1, 0, 0);
			this.tableMain.Controls.Add(this.rdoAll, 1, 0);
			this.tableMain.Controls.Add(this.label2, 0, 3);
			this.tableMain.Controls.Add(this.cmbAction, 1, 3);
			this.tableMain.Controls.Add(this.lblDuration, 0, 4);
			this.tableMain.Controls.Add(this.txtDuration, 1, 4);
			this.tableMain.Controls.Add(this.lblRelayNotFound, 1, 2);
			this.tableMain.Controls.Add(this.panel1, 1, 1);
			this.tableMain.Controls.Add(this.lblQueryDurationNote, 1, 5);
			this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableMain.Location = new System.Drawing.Point(0, 0);
			this.tableMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableMain.Name = "tableMain";
			this.tableMain.RowCount = 7;
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableMain.Size = new System.Drawing.Size(298, 233);
			this.tableMain.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 39);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_CommandEdit_Yocto_Relay]";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// rdoAll
			// 
			this.rdoAll.AutoSize = true;
			this.rdoAll.Location = new System.Drawing.Point(92, 3);
			this.rdoAll.Name = "rdoAll";
			this.rdoAll.Size = new System.Drawing.Size(178, 17);
			this.rdoAll.TabIndex = 1;
			this.rdoAll.TabStop = true;
			this.rdoAll.Text = "[SAW_CommandEdit_Yocto_All]";
			this.rdoAll.UseVisualStyleBackColor = true;
			this.rdoAll.CheckedChanged += new System.EventHandler(this.rdoAll_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 82);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 39);
			this.label2.TabIndex = 3;
			this.label2.Text = "[SAW_CommandEdit_Yocto_Action]";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cmbAction
			// 
			this.cmbAction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAction.FormattingEnabled = true;
			this.cmbAction.Items.AddRange(new object[] {
            "[SAW_CommandEdit_Yocto_Pulse]",
            "[SAW_CommandEdit_Yocto_On]",
            "[SAW_CommandEdit_Yocto_Off]",
            "[SAW_CommandEdit_Yocto_Toggle]",
            "[SAW_CommandEdit_Yocto_Query]"});
			this.cmbAction.Location = new System.Drawing.Point(92, 85);
			this.cmbAction.Name = "cmbAction";
			this.cmbAction.Size = new System.Drawing.Size(203, 21);
			this.cmbAction.TabIndex = 4;
			this.cmbAction.SelectedIndexChanged += new System.EventHandler(this.cmbAction_SelectedIndexChanged);
			// 
			// lblDuration
			// 
			this.lblDuration.AutoSize = true;
			this.lblDuration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDuration.Location = new System.Drawing.Point(3, 121);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(83, 39);
			this.lblDuration.TabIndex = 5;
			this.lblDuration.Text = "[SAW_CommandEdit_Yocto_Duration]";
			this.lblDuration.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblRelayNotFound
			// 
			this.lblRelayNotFound.AutoSize = true;
			this.lblRelayNotFound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblRelayNotFound.ForeColor = System.Drawing.Color.Red;
			this.lblRelayNotFound.Location = new System.Drawing.Point(92, 69);
			this.lblRelayNotFound.Name = "lblRelayNotFound";
			this.lblRelayNotFound.Size = new System.Drawing.Size(203, 13);
			this.lblRelayNotFound.TabIndex = 7;
			this.lblRelayNotFound.Text = "[SAW_CommandEdit_Yocto_NotFound]";
			this.lblRelayNotFound.Visible = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cmbRelayID);
			this.panel1.Controls.Add(this.rdoSingle);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(89, 39);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(209, 30);
			this.panel1.TabIndex = 8;
			// 
			// cmbRelayID
			// 
			this.cmbRelayID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbRelayID.FormattingEnabled = true;
			this.cmbRelayID.Location = new System.Drawing.Point(23, 3);
			this.cmbRelayID.Name = "cmbRelayID";
			this.cmbRelayID.Size = new System.Drawing.Size(178, 21);
			this.cmbRelayID.TabIndex = 1;
			this.cmbRelayID.TextChanged += new System.EventHandler(this.cmbRelayID_TextChanged);
			// 
			// rdoSingle
			// 
			this.rdoSingle.AutoSize = true;
			this.rdoSingle.Location = new System.Drawing.Point(3, 3);
			this.rdoSingle.Name = "rdoSingle";
			this.rdoSingle.Size = new System.Drawing.Size(14, 13);
			this.rdoSingle.TabIndex = 0;
			this.rdoSingle.TabStop = true;
			this.rdoSingle.UseVisualStyleBackColor = true;
			this.rdoSingle.CheckedChanged += new System.EventHandler(this.rdoSingle_CheckedChanged);
			// 
			// lblQueryDurationNote
			// 
			this.lblQueryDurationNote.AutoSize = true;
			this.lblQueryDurationNote.Location = new System.Drawing.Point(92, 160);
			this.lblQueryDurationNote.Name = "lblQueryDurationNote";
			this.lblQueryDurationNote.Size = new System.Drawing.Size(203, 26);
			this.lblQueryDurationNote.TabIndex = 9;
			this.lblQueryDurationNote.Text = "[SAW_CommandEdit_Yocto_DisplayDurationNote]";
			// 
			// txtDuration
			// 
			this.txtDuration.Location = new System.Drawing.Point(92, 124);
			this.txtDuration.Name = "txtDuration";
			this.txtDuration.Size = new System.Drawing.Size(64, 20);
			this.txtDuration.TabIndex = 6;
			this.txtDuration.Text = "1000";
			this.txtDuration.Value = 1000;
			this.txtDuration.TextChanged += new System.EventHandler(this.txtDuration_TextChanged);
			// 
			// YoctoEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableMain);
			this.Name = "YoctoEditor";
			this.Size = new System.Drawing.Size(298, 233);
			this.tableMain.ResumeLayout(false);
			this.tableMain.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableMain;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rdoAll;
		private System.Windows.Forms.RadioButton rdoSingle;
		private System.Windows.Forms.ComboBox cmbRelayID;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbAction;
		private System.Windows.Forms.Label lblDuration;
		private IntegerTextBox txtDuration;
		private System.Windows.Forms.Label lblRelayNotFound;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblQueryDurationNote;
	}
}
