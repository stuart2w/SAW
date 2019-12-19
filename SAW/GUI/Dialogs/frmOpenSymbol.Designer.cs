namespace SAW
{
	partial class frmOpenSymbol
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
			this.pnlImages = new System.Windows.Forms.FlowLayoutPanel();
			this.btnSearch = new System.Windows.Forms.Button();
			this.txtText = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.lnkOpenSymbols = new System.Windows.Forms.LinkLabel();
			this.lblInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pnlImages
			// 
			this.pnlImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlImages.AutoScroll = true;
			this.pnlImages.Location = new System.Drawing.Point(13, 46);
			this.pnlImages.Margin = new System.Windows.Forms.Padding(4);
			this.pnlImages.Name = "pnlImages";
			this.pnlImages.Size = new System.Drawing.Size(559, 292);
			this.pnlImages.TabIndex = 9;
			this.pnlImages.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImages_Paint);
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearch.Location = new System.Drawing.Point(385, 13);
			this.btnSearch.Margin = new System.Windows.Forms.Padding(4);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(104, 30);
			this.btnSearch.TabIndex = 8;
			this.btnSearch.Text = "[Search]";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// txtText
			// 
			this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtText.Location = new System.Drawing.Point(13, 13);
			this.txtText.Margin = new System.Windows.Forms.Padding(4);
			this.txtText.Name = "txtText";
			this.txtText.Size = new System.Drawing.Size(364, 20);
			this.txtText.TabIndex = 7;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(455, 363);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(117, 34);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(316, 363);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(121, 34);
			this.btnOK.TabIndex = 5;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			// 
			// lnkOpenSymbols
			// 
			this.lnkOpenSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkOpenSymbols.AutoSize = true;
			this.lnkOpenSymbols.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lnkOpenSymbols.Location = new System.Drawing.Point(425, 342);
			this.lnkOpenSymbols.Name = "lnkOpenSymbols";
			this.lnkOpenSymbols.Size = new System.Drawing.Size(147, 17);
			this.lnkOpenSymbols.TabIndex = 10;
			this.lnkOpenSymbols.TabStop = true;
			this.lnkOpenSymbols.Text = "www.opensymbols.org";
			this.lnkOpenSymbols.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkOpenSymbols_LinkClicked);
			// 
			// lblInfo
			// 
			this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblInfo.ForeColor = System.Drawing.Color.Red;
			this.lblInfo.Location = new System.Drawing.Point(13, 346);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(296, 46);
			this.lblInfo.TabIndex = 11;
			this.lblInfo.Visible = false;
			// 
			// frmOpenSymbol
			// 
			this.AcceptButton = this.btnSearch;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(585, 401);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.lnkOpenSymbols);
			this.Controls.Add(this.pnlImages);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.txtText);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmOpenSymbol";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[SAW_Edit_OS]";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel pnlImages;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.TextBox txtText;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.LinkLabel lnkOpenSymbols;
		private System.Windows.Forms.Label lblInfo;
	}
}