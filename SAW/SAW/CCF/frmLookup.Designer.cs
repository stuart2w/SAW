namespace SAW.CCF
{
	partial class frmLookup
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
			this.components = new System.ComponentModel.Container();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtText = new System.Windows.Forms.TextBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.pnlImages = new System.Windows.Forms.FlowLayoutPanel();
			this.tmrReceive = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.cmbLanguage = new System.Windows.Forms.ComboBox();
			this.lblUpdateWord = new System.Windows.Forms.Label();
			this.cmbUpdateWord = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(245, 280);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(121, 34);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(384, 280);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(117, 34);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// txtText
			// 
			this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtText.Location = new System.Drawing.Point(8, 8);
			this.txtText.Margin = new System.Windows.Forms.Padding(4);
			this.txtText.Name = "txtText";
			this.txtText.Size = new System.Drawing.Size(248, 23);
			this.txtText.TabIndex = 2;
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearch.Location = new System.Drawing.Point(264, 8);
			this.btnSearch.Margin = new System.Windows.Forms.Padding(4);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(104, 30);
			this.btnSearch.TabIndex = 3;
			this.btnSearch.Text = "[Search]";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// pnlImages
			// 
			this.pnlImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlImages.Location = new System.Drawing.Point(8, 48);
			this.pnlImages.Margin = new System.Windows.Forms.Padding(4);
			this.pnlImages.Name = "pnlImages";
			this.pnlImages.Size = new System.Drawing.Size(488, 224);
			this.pnlImages.TabIndex = 4;
			this.pnlImages.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImages_Paint);
			// 
			// tmrReceive
			// 
			this.tmrReceive.Enabled = true;
			this.tmrReceive.Interval = 1000;
			this.tmrReceive.Tick += new System.EventHandler(this.tmrReceive_Tick);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(384, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "[Language]";
			// 
			// cmbLanguage
			// 
			this.cmbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbLanguage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmbLanguage.FormattingEnabled = true;
			this.cmbLanguage.Items.AddRange(new object[] {
            "en - English",
            "sv - Swedish",
            "nl - Dutch",
            "es - Spanish"});
			this.cmbLanguage.Location = new System.Drawing.Point(384, 16);
			this.cmbLanguage.Name = "cmbLanguage";
			this.cmbLanguage.Size = new System.Drawing.Size(120, 21);
			this.cmbLanguage.TabIndex = 6;
			// 
			// lblUpdateWord
			// 
			this.lblUpdateWord.AutoSize = true;
			this.lblUpdateWord.Location = new System.Drawing.Point(8, 272);
			this.lblUpdateWord.Name = "lblUpdateWord";
			this.lblUpdateWord.Size = new System.Drawing.Size(203, 17);
			this.lblUpdateWord.TabIndex = 7;
			this.lblUpdateWord.Text = "[CCF_Lookup_ChangeToForm]";
			// 
			// cmbUpdateWord
			// 
			this.cmbUpdateWord.FormattingEnabled = true;
			this.cmbUpdateWord.Location = new System.Drawing.Point(8, 296);
			this.cmbUpdateWord.Name = "cmbUpdateWord";
			this.cmbUpdateWord.Size = new System.Drawing.Size(208, 24);
			this.cmbUpdateWord.TabIndex = 8;
			this.cmbUpdateWord.SelectedIndexChanged += new System.EventHandler(this.cmbUpdateWord_SelectedIndexChanged);
			// 
			// frmLookup
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(512, 325);
			this.Controls.Add(this.cmbUpdateWord);
			this.Controls.Add(this.lblUpdateWord);
			this.Controls.Add(this.cmbLanguage);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pnlImages);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.txtText);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(528, 360);
			this.Name = "frmLookup";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[CCF_Full]";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtText;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.FlowLayoutPanel pnlImages;
		private System.Windows.Forms.Timer tmrReceive;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbLanguage;
		private System.Windows.Forms.Label lblUpdateWord;
		private System.Windows.Forms.ComboBox cmbUpdateWord;
	}
}