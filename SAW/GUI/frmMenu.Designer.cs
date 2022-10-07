namespace SAW
{
	internal partial class frmMenu
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMenu));
			this.ttMain = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnDesign = new System.Windows.Forms.Button();
			this.btnOpen = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnExit = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tmrTick = new System.Windows.Forms.Timer(this.components);
			this.pctUpdate = new System.Windows.Forms.PictureBox();
			this.lnkUpdate = new System.Windows.Forms.LinkLabel();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctUpdate)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnDesign);
			this.groupBox1.Controls.Add(this.btnOpen);
			this.groupBox1.Location = new System.Drawing.Point(12, 61);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(284, 100);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "[SAW_Menu_Sets]";
			// 
			// btnDesign
			// 
			this.btnDesign.Location = new System.Drawing.Point(13, 19);
			this.btnDesign.Name = "btnDesign";
			this.btnDesign.Size = new System.Drawing.Size(260, 31);
			this.btnDesign.TabIndex = 1;
			this.btnDesign.Text = "[SAW_Menu_Design]";
			this.btnDesign.UseVisualStyleBackColor = true;
			this.btnDesign.Click += new System.EventHandler(this.btnDesign_Click);
			// 
			// btnOpen
			// 
			this.btnOpen.Location = new System.Drawing.Point(13, 56);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(260, 31);
			this.btnOpen.TabIndex = 2;
			this.btnOpen.Text = "[SAW_Menu_Open]";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(226, 49);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Menu_Prompt]";
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(179, 198);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(114, 23);
			this.btnExit.TabIndex = 1;
			this.btnExit.Text = "[SAW_Menu_Exit]";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::Resources.AM.SAW;
			this.pictureBox1.Location = new System.Drawing.Point(247, 6);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(49, 49);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// tmrTick
			// 
			this.tmrTick.Interval = 500;
			this.tmrTick.Tick += new System.EventHandler(this.tmrTick_Tick);
			// 
			// pctUpdate
			// 
			this.pctUpdate.Image = global::Resources.AM.info_32;
			this.pctUpdate.Location = new System.Drawing.Point(9, 184);
			this.pctUpdate.Name = "pctUpdate";
			this.pctUpdate.Size = new System.Drawing.Size(36, 36);
			this.pctUpdate.TabIndex = 3;
			this.pctUpdate.TabStop = false;
			this.pctUpdate.Visible = false;
			// 
			// lnkUpdate
			// 
			this.lnkUpdate.Location = new System.Drawing.Point(53, 182);
			this.lnkUpdate.Name = "lnkUpdate";
			this.lnkUpdate.Size = new System.Drawing.Size(120, 39);
			this.lnkUpdate.TabIndex = 4;
			this.lnkUpdate.TabStop = true;
			this.lnkUpdate.Text = "[SAW_Menu_Update]";
			this.lnkUpdate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lnkUpdate.Visible = false;
			this.lnkUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkUpdate_LinkClicked);
			// 
			// frmMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(308, 246);
			this.Controls.Add(this.lnkUpdate);
			this.Controls.Add(this.pctUpdate);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMenu";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[App]";
			this.Load += new System.EventHandler(this.frmMenu_Load);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctUpdate)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ToolTip ttMain;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnDesign;
		private System.Windows.Forms.Button btnOpen;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Timer tmrTick;
		private System.Windows.Forms.PictureBox pctUpdate;
		private System.Windows.Forms.LinkLabel lnkUpdate;
	}
}