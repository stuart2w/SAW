namespace SAW.CCF
{
	partial class frmMultiUpdateOptions
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
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			System.Windows.Forms.Label label5;
			this.rdoSymbols = new System.Windows.Forms.RadioButton();
			this.rdoText = new System.Windows.Forms.RadioButton();
			this.rdoRemoveCCF = new System.Windows.Forms.RadioButton();
			this.chkOnlyCoded = new System.Windows.Forms.CheckBox();
			this.chkUpdateSelected = new System.Windows.Forms.CheckBox();
			this.cmbTextLanguage = new System.Windows.Forms.ComboBox();
			this.cmbPreferredSymbols = new System.Windows.Forms.ComboBox();
			this.cmbSecondarySymbols = new System.Windows.Forms.ComboBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.pnlSymbolOut = new System.Windows.Forms.Panel();
			this.pnlLanguageOut = new System.Windows.Forms.Panel();
			this.cmbLanguageOut = new System.Windows.Forms.ComboBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			this.pnlSymbolOut.SuspendLayout();
			this.pnlLanguageOut.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(16, 16);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(191, 17);
			label1.TabIndex = 0;
			label1.Text = "[CCF_Options_ActionPrompt]";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(24, 160);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(182, 17);
			label2.TabIndex = 6;
			label2.Text = "[CCF_Options_InLanguage]";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(16, 8);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(151, 17);
			label3.TabIndex = 8;
			label3.Text = "Preferred symbol type:";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(16, 40);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(148, 17);
			label4.TabIndex = 10;
			label4.Text = "Alternate symbol type:";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(40, 16);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(194, 17);
			label5.TabIndex = 8;
			label5.Text = "[CCF_Options_OutLanguage]";
			// 
			// rdoSymbols
			// 
			this.rdoSymbols.AutoSize = true;
			this.rdoSymbols.Checked = true;
			this.rdoSymbols.Location = new System.Drawing.Point(144, 16);
			this.rdoSymbols.Name = "rdoSymbols";
			this.rdoSymbols.Size = new System.Drawing.Size(217, 21);
			this.rdoSymbols.TabIndex = 1;
			this.rdoSymbols.TabStop = true;
			this.rdoSymbols.Text = "[CCF_Action_ChangeSymbols]";
			this.rdoSymbols.UseVisualStyleBackColor = true;
			this.rdoSymbols.CheckedChanged += new System.EventHandler(this.rdoFunction_CheckedChanged);
			// 
			// rdoText
			// 
			this.rdoText.AutoSize = true;
			this.rdoText.Location = new System.Drawing.Point(144, 40);
			this.rdoText.Name = "rdoText";
			this.rdoText.Size = new System.Drawing.Size(191, 21);
			this.rdoText.TabIndex = 2;
			this.rdoText.Text = "[CCF_Action_ChangeText]";
			this.rdoText.UseVisualStyleBackColor = true;
			this.rdoText.CheckedChanged += new System.EventHandler(this.rdoFunction_CheckedChanged);
			// 
			// rdoRemoveCCF
			// 
			this.rdoRemoveCCF.AutoSize = true;
			this.rdoRemoveCCF.Location = new System.Drawing.Point(144, 64);
			this.rdoRemoveCCF.Name = "rdoRemoveCCF";
			this.rdoRemoveCCF.Size = new System.Drawing.Size(190, 21);
			this.rdoRemoveCCF.TabIndex = 3;
			this.rdoRemoveCCF.Text = "[CCF_Action_RemoveInfo]";
			this.rdoRemoveCCF.UseVisualStyleBackColor = true;
			this.rdoRemoveCCF.CheckedChanged += new System.EventHandler(this.rdoFunction_CheckedChanged);
			// 
			// chkOnlyCoded
			// 
			this.chkOnlyCoded.AutoSize = true;
			this.chkOnlyCoded.Location = new System.Drawing.Point(16, 96);
			this.chkOnlyCoded.Name = "chkOnlyCoded";
			this.chkOnlyCoded.Size = new System.Drawing.Size(196, 21);
			this.chkOnlyCoded.TabIndex = 4;
			this.chkOnlyCoded.Text = "[CCF_Options_OnlyCoded]";
			this.chkOnlyCoded.UseVisualStyleBackColor = true;
			this.chkOnlyCoded.CheckedChanged += new System.EventHandler(this.chkOnlyCoded_CheckedChanged);
			// 
			// chkUpdateSelected
			// 
			this.chkUpdateSelected.AutoSize = true;
			this.chkUpdateSelected.Location = new System.Drawing.Point(16, 120);
			this.chkUpdateSelected.Name = "chkUpdateSelected";
			this.chkUpdateSelected.Size = new System.Drawing.Size(210, 21);
			this.chkUpdateSelected.TabIndex = 5;
			this.chkUpdateSelected.Text = "[CCF_Options_OnlySelected]";
			this.chkUpdateSelected.UseVisualStyleBackColor = true;
			// 
			// cmbTextLanguage
			// 
			this.cmbTextLanguage.FormattingEnabled = true;
			this.cmbTextLanguage.Items.AddRange(new object[] {
            "en - English",
            "sv - Swedish",
            "nl - Dutch",
            "es - Spanish"});
			this.cmbTextLanguage.Location = new System.Drawing.Point(200, 160);
			this.cmbTextLanguage.Name = "cmbTextLanguage";
			this.cmbTextLanguage.Size = new System.Drawing.Size(144, 24);
			this.cmbTextLanguage.TabIndex = 7;
			this.cmbTextLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbTextLanguage_SelectedIndexChanged);
			// 
			// cmbPreferredSymbols
			// 
			this.cmbPreferredSymbols.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPreferredSymbols.FormattingEnabled = true;
			this.cmbPreferredSymbols.Items.AddRange(new object[] {
            "aras",
            "bliss"});
			this.cmbPreferredSymbols.Location = new System.Drawing.Point(176, 8);
			this.cmbPreferredSymbols.Name = "cmbPreferredSymbols";
			this.cmbPreferredSymbols.Size = new System.Drawing.Size(144, 24);
			this.cmbPreferredSymbols.TabIndex = 9;
			// 
			// cmbSecondarySymbols
			// 
			this.cmbSecondarySymbols.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSecondarySymbols.FormattingEnabled = true;
			this.cmbSecondarySymbols.Items.AddRange(new object[] {
            "aras",
            "bliss",
            "(none)"});
			this.cmbSecondarySymbols.Location = new System.Drawing.Point(176, 40);
			this.cmbSecondarySymbols.Name = "cmbSecondarySymbols";
			this.cmbSecondarySymbols.Size = new System.Drawing.Size(144, 24);
			this.cmbSecondarySymbols.TabIndex = 11;
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(176, 264);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(91, 32);
			this.btnOK.TabIndex = 12;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(272, 264);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(91, 32);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// pnlSymbolOut
			// 
			this.pnlSymbolOut.Controls.Add(this.cmbPreferredSymbols);
			this.pnlSymbolOut.Controls.Add(label3);
			this.pnlSymbolOut.Controls.Add(label4);
			this.pnlSymbolOut.Controls.Add(this.cmbSecondarySymbols);
			this.pnlSymbolOut.Location = new System.Drawing.Point(8, 192);
			this.pnlSymbolOut.Name = "pnlSymbolOut";
			this.pnlSymbolOut.Size = new System.Drawing.Size(344, 72);
			this.pnlSymbolOut.TabIndex = 14;
			// 
			// pnlLanguageOut
			// 
			this.pnlLanguageOut.Controls.Add(this.cmbLanguageOut);
			this.pnlLanguageOut.Controls.Add(label5);
			this.pnlLanguageOut.Location = new System.Drawing.Point(0, 200);
			this.pnlLanguageOut.Name = "pnlLanguageOut";
			this.pnlLanguageOut.Size = new System.Drawing.Size(352, 56);
			this.pnlLanguageOut.TabIndex = 15;
			this.pnlLanguageOut.Visible = false;
			// 
			// cmbLanguageOut
			// 
			this.cmbLanguageOut.FormattingEnabled = true;
			this.cmbLanguageOut.Items.AddRange(new object[] {
            "en - English",
            "sv - Swedish",
            "nl - Dutch",
            "es - Spanish"});
			this.cmbLanguageOut.Location = new System.Drawing.Point(200, 16);
			this.cmbLanguageOut.Name = "cmbLanguageOut";
			this.cmbLanguageOut.Size = new System.Drawing.Size(144, 24);
			this.cmbLanguageOut.TabIndex = 9;
			// 
			// frmMultiUpdateOptions
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(378, 311);
			this.Controls.Add(this.pnlLanguageOut);
			this.Controls.Add(this.pnlSymbolOut);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.cmbTextLanguage);
			this.Controls.Add(label2);
			this.Controls.Add(this.chkUpdateSelected);
			this.Controls.Add(this.chkOnlyCoded);
			this.Controls.Add(this.rdoRemoveCCF);
			this.Controls.Add(this.rdoText);
			this.Controls.Add(this.rdoSymbols);
			this.Controls.Add(label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMultiUpdateOptions";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[CCF_Options_Title]";
			this.pnlSymbolOut.ResumeLayout(false);
			this.pnlSymbolOut.PerformLayout();
			this.pnlLanguageOut.ResumeLayout(false);
			this.pnlLanguageOut.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton rdoSymbols;
		private System.Windows.Forms.RadioButton rdoText;
		private System.Windows.Forms.RadioButton rdoRemoveCCF;
		private System.Windows.Forms.CheckBox chkOnlyCoded;
		private System.Windows.Forms.CheckBox chkUpdateSelected;
		private System.Windows.Forms.ComboBox cmbTextLanguage;
		private System.Windows.Forms.ComboBox cmbPreferredSymbols;
		private System.Windows.Forms.ComboBox cmbSecondarySymbols;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel pnlSymbolOut;
		private System.Windows.Forms.Panel pnlLanguageOut;
		private System.Windows.Forms.ComboBox cmbLanguageOut;
	}
}