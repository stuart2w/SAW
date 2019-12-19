namespace SAW
{
	partial class ctrScriptEdit
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
			this.lstCommands = new System.Windows.Forms.ListBox();
			this.txtScript = new System.Windows.Forms.TextBox();
			this.btnUp = new System.Windows.Forms.Button();
			this.btnDown = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.cmbVisitType = new System.Windows.Forms.ComboBox();
			this.lblVisitItemID = new System.Windows.Forms.Label();
			this.txtVisitItemID = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.lblVisitDefaultInfo = new System.Windows.Forms.Label();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoList = new System.Windows.Forms.RadioButton();
			this.rdoText = new System.Windows.Forms.RadioButton();
			this.btnAbandonText = new System.Windows.Forms.Button();
			this.btnAdd = new System.Windows.Forms.Button();
			this.chkRunDefault = new System.Windows.Forms.CheckBox();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnDefaultInfo = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.ctrCommandEditor = new SAW.CommandEditors.CommandEditor();
			this.flowLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstCommands
			// 
			this.lstCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lstCommands.FormattingEnabled = true;
			this.lstCommands.IntegralHeight = false;
			this.lstCommands.Location = new System.Drawing.Point(3, 50);
			this.lstCommands.Name = "lstCommands";
			this.lstCommands.Size = new System.Drawing.Size(238, 236);
			this.lstCommands.TabIndex = 0;
			this.lstCommands.SelectedIndexChanged += new System.EventHandler(this.lstCommands_SelectedIndexChanged);
			// 
			// txtScript
			// 
			this.txtScript.AcceptsReturn = true;
			this.txtScript.AcceptsTab = true;
			this.txtScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtScript.Location = new System.Drawing.Point(3, 50);
			this.txtScript.Multiline = true;
			this.txtScript.Name = "txtScript";
			this.txtScript.Size = new System.Drawing.Size(477, 236);
			this.txtScript.TabIndex = 1;
			this.txtScript.TextChanged += new System.EventHandler(this.txtScript_TextChanged);
			// 
			// btnUp
			// 
			this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnUp.Enabled = false;
			this.btnUp.Location = new System.Drawing.Point(486, 44);
			this.btnUp.Name = "btnUp";
			this.btnUp.Size = new System.Drawing.Size(66, 23);
			this.btnUp.TabIndex = 2;
			this.btnUp.Text = "[SAW_Edit_Up]";
			this.btnUp.UseVisualStyleBackColor = true;
			this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
			// 
			// btnDown
			// 
			this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDown.Enabled = false;
			this.btnDown.Location = new System.Drawing.Point(486, 73);
			this.btnDown.Name = "btnDown";
			this.btnDown.Size = new System.Drawing.Size(66, 23);
			this.btnDown.TabIndex = 3;
			this.btnDown.Text = "[SAW_Edit_Down]";
			this.btnDown.UseVisualStyleBackColor = true;
			this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 7);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(87, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "[SAW_Edit_Visit]";
			// 
			// cmbVisitType
			// 
			this.cmbVisitType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cmbVisitType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbVisitType.FormattingEnabled = true;
			this.cmbVisitType.Location = new System.Drawing.Point(96, 3);
			this.cmbVisitType.Name = "cmbVisitType";
			this.cmbVisitType.Size = new System.Drawing.Size(121, 21);
			this.cmbVisitType.TabIndex = 5;
			this.cmbVisitType.SelectedIndexChanged += new System.EventHandler(this.cmbVisitType_SelectedIndexChanged);
			// 
			// lblVisitItemID
			// 
			this.lblVisitItemID.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblVisitItemID.AutoSize = true;
			this.lblVisitItemID.Location = new System.Drawing.Point(223, 7);
			this.lblVisitItemID.Name = "lblVisitItemID";
			this.lblVisitItemID.Size = new System.Drawing.Size(118, 13);
			this.lblVisitItemID.TabIndex = 6;
			this.lblVisitItemID.Text = "[SAW_Edit_VisitItemID]";
			// 
			// txtVisitItemID
			// 
			this.txtVisitItemID.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtVisitItemID.Location = new System.Drawing.Point(347, 3);
			this.txtVisitItemID.Name = "txtVisitItemID";
			this.txtVisitItemID.Size = new System.Drawing.Size(77, 20);
			this.txtVisitItemID.TabIndex = 7;
			this.txtVisitItemID.TextChanged += new System.EventHandler(this.txtVisitItemID_TextChanged);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.cmbVisitType);
			this.flowLayoutPanel1.Controls.Add(this.lblVisitItemID);
			this.flowLayoutPanel1.Controls.Add(this.txtVisitItemID);
			this.flowLayoutPanel1.Controls.Add(this.lblVisitDefaultInfo);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 289);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(477, 29);
			this.flowLayoutPanel1.TabIndex = 8;
			// 
			// lblVisitDefaultInfo
			// 
			this.lblVisitDefaultInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblVisitDefaultInfo.AutoSize = true;
			this.lblVisitDefaultInfo.Location = new System.Drawing.Point(430, 7);
			this.lblVisitDefaultInfo.Name = "lblVisitDefaultInfo";
			this.lblVisitDefaultInfo.Size = new System.Drawing.Size(16, 13);
			this.lblVisitDefaultInfo.TabIndex = 8;
			this.lblVisitDefaultInfo.Text = "---";
			this.lblVisitDefaultInfo.Visible = false;
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel2.Controls.Add(this.rdoList);
			this.flowLayoutPanel2.Controls.Add(this.rdoText);
			this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 0);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(477, 23);
			this.flowLayoutPanel2.TabIndex = 9;
			// 
			// rdoList
			// 
			this.rdoList.AutoSize = true;
			this.rdoList.Location = new System.Drawing.Point(3, 3);
			this.rdoList.Name = "rdoList";
			this.rdoList.Size = new System.Drawing.Size(129, 17);
			this.rdoList.TabIndex = 0;
			this.rdoList.TabStop = true;
			this.rdoList.Text = "[SAW_Edit_ScriptList]";
			this.rdoList.UseVisualStyleBackColor = true;
			this.rdoList.CheckedChanged += new System.EventHandler(this.rdoListOrText_CheckedChanged);
			// 
			// rdoText
			// 
			this.rdoText.AutoSize = true;
			this.rdoText.Location = new System.Drawing.Point(138, 3);
			this.rdoText.Name = "rdoText";
			this.rdoText.Size = new System.Drawing.Size(134, 17);
			this.rdoText.TabIndex = 1;
			this.rdoText.TabStop = true;
			this.rdoText.Text = "[SAW_Edit_ScriptText]";
			this.rdoText.UseVisualStyleBackColor = true;
			this.rdoText.CheckedChanged += new System.EventHandler(this.rdoListOrText_CheckedChanged);
			// 
			// btnAbandonText
			// 
			this.btnAbandonText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAbandonText.Enabled = false;
			this.btnAbandonText.Location = new System.Drawing.Point(486, 229);
			this.btnAbandonText.Name = "btnAbandonText";
			this.btnAbandonText.Size = new System.Drawing.Size(66, 57);
			this.btnAbandonText.TabIndex = 10;
			this.btnAbandonText.Text = "[SAW_Edit_AbandonText]";
			this.btnAbandonText.UseVisualStyleBackColor = true;
			this.btnAbandonText.Click += new System.EventHandler(this.btnAbandonText_Click);
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.Location = new System.Drawing.Point(486, 102);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(66, 23);
			this.btnAdd.TabIndex = 11;
			this.btnAdd.Text = "[SAW_Edit_Add]";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Visible = false;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// chkRunDefault
			// 
			this.chkRunDefault.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.chkRunDefault.AutoSize = true;
			this.chkRunDefault.Location = new System.Drawing.Point(3, 6);
			this.chkRunDefault.Name = "chkRunDefault";
			this.chkRunDefault.Size = new System.Drawing.Size(141, 17);
			this.chkRunDefault.TabIndex = 13;
			this.chkRunDefault.Text = "[SAW_Edit_RunDefault]";
			this.chkRunDefault.UseVisualStyleBackColor = true;
			this.chkRunDefault.CheckedChanged += new System.EventHandler(this.chkRunDefault_CheckedChanged);
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.Controls.Add(this.chkRunDefault);
			this.flowLayoutPanel3.Controls.Add(this.btnDefaultInfo);
			this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 21);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(430, 28);
			this.flowLayoutPanel3.TabIndex = 14;
			// 
			// btnDefaultInfo
			// 
			this.btnDefaultInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnDefaultInfo.AutoSize = true;
			this.btnDefaultInfo.Location = new System.Drawing.Point(150, 3);
			this.btnDefaultInfo.Name = "btnDefaultInfo";
			this.btnDefaultInfo.Size = new System.Drawing.Size(29, 23);
			this.btnDefaultInfo.TabIndex = 14;
			this.btnDefaultInfo.Text = "?";
			this.btnDefaultInfo.UseVisualStyleBackColor = true;
			this.btnDefaultInfo.Click += new System.EventHandler(this.btnDefaultInfo_Click);
			// 
			// btnDelete
			// 
			this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDelete.Enabled = false;
			this.btnDelete.Location = new System.Drawing.Point(486, 131);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(66, 23);
			this.btnDelete.TabIndex = 15;
			this.btnDelete.Text = "[SAW_Edit_Delete]";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Visible = false;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReset.Location = new System.Drawing.Point(486, 160);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(66, 23);
			this.btnReset.TabIndex = 16;
			this.btnReset.Text = "[SAW_Edit_Reset]";
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// ctrCommandEditor
			// 
			this.ctrCommandEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctrCommandEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ctrCommandEditor.Location = new System.Drawing.Point(247, 50);
			this.ctrCommandEditor.Name = "ctrCommandEditor";
			this.ctrCommandEditor.Size = new System.Drawing.Size(233, 230);
			this.ctrCommandEditor.TabIndex = 12;
			this.ctrCommandEditor.UserChanged += new System.EventHandler(this.ctrCommandEditor_UserChanged);
			// 
			// ctrScriptEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnDelete);
			this.Controls.Add(this.flowLayoutPanel3);
			this.Controls.Add(this.txtScript);
			this.Controls.Add(this.ctrCommandEditor);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.btnAbandonText);
			this.Controls.Add(this.flowLayoutPanel2);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.btnDown);
			this.Controls.Add(this.btnUp);
			this.Controls.Add(this.lstCommands);
			this.Name = "ctrScriptEdit";
			this.Size = new System.Drawing.Size(555, 318);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.flowLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstCommands;
		private System.Windows.Forms.TextBox txtScript;
		private System.Windows.Forms.Button btnUp;
		private System.Windows.Forms.Button btnDown;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbVisitType;
		private System.Windows.Forms.Label lblVisitItemID;
		private System.Windows.Forms.TextBox txtVisitItemID;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.RadioButton rdoList;
		private System.Windows.Forms.RadioButton rdoText;
		private System.Windows.Forms.Button btnAbandonText;
		private System.Windows.Forms.Button btnAdd;
		private CommandEditors.CommandEditor ctrCommandEditor;
		private System.Windows.Forms.CheckBox chkRunDefault;
		private System.Windows.Forms.Label lblVisitDefaultInfo;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.Button btnDefaultInfo;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnReset;
	}
}
