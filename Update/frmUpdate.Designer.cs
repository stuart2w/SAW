using System.Windows.Forms;

namespace Update
{
	partial class frmUpdate: Form
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmUpdate));
			this.PictureBox1 = new System.Windows.Forms.PictureBox();
			this.lblPermissions = new System.Windows.Forms.Label();
			this.proMain = new System.Windows.Forms.ProgressBar();
			this.btnBegin = new System.Windows.Forms.Button();
			this.lblPrompt = new System.Windows.Forms.Label();
			this.tmrFront = new System.Windows.Forms.Timer(this.components);
			this.btnCancel = new System.Windows.Forms.Button();
			this.pctLogo = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pctLogo)).BeginInit();
			this.SuspendLayout();
			// 
			// PictureBox1
			// 
			this.PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
			this.PictureBox1.InitialImage = null;
			this.PictureBox1.Location = new System.Drawing.Point(12, 12);
			this.PictureBox1.Name = "PictureBox1";
			this.PictureBox1.Size = new System.Drawing.Size(48, 50);
			this.PictureBox1.TabIndex = 12;
			this.PictureBox1.TabStop = false;
			// 
			// lblPermissions
			// 
			this.lblPermissions.Location = new System.Drawing.Point(17, 79);
			this.lblPermissions.Name = "lblPermissions";
			this.lblPermissions.Size = new System.Drawing.Size(563, 96);
			this.lblPermissions.TabIndex = 11;
			this.lblPermissions.Visible = false;
			// 
			// proMain
			// 
			this.proMain.Location = new System.Drawing.Point(20, 178);
			this.proMain.Name = "proMain";
			this.proMain.Size = new System.Drawing.Size(560, 23);
			this.proMain.TabIndex = 10;
			// 
			// btnBegin
			// 
			this.btnBegin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnBegin.Location = new System.Drawing.Point(140, 210);
			this.btnBegin.Name = "btnBegin";
			this.btnBegin.Size = new System.Drawing.Size(160, 32);
			this.btnBegin.TabIndex = 9;
			this.btnBegin.Text = "[Install_Start]";
			this.btnBegin.UseVisualStyleBackColor = true;
			this.btnBegin.Click += new System.EventHandler(this.btnBegin_Click);
			// 
			// lblPrompt
			// 
			this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPrompt.Location = new System.Drawing.Point(68, 12);
			this.lblPrompt.Name = "lblPrompt";
			this.lblPrompt.Size = new System.Drawing.Size(442, 56);
			this.lblPrompt.TabIndex = 8;
			this.lblPrompt.Text = "[Install_Explain]";
			// 
			// tmrFront
			// 
			this.tmrFront.Enabled = true;
			this.tmrFront.Interval = 3000;
			this.tmrFront.Tick += new System.EventHandler(this.tmrFront_Tick);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(316, 210);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(160, 32);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Visible = false;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// pctLogo
			// 
			this.pctLogo.Image = ((System.Drawing.Image)(resources.GetObject("pctLogo.Image")));
			this.pctLogo.Location = new System.Drawing.Point(516, 10);
			this.pctLogo.Name = "pctLogo";
			this.pctLogo.Size = new System.Drawing.Size(64, 64);
			this.pctLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pctLogo.TabIndex = 14;
			this.pctLogo.TabStop = false;
			// 
			// frmWelcome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(592, 264);
			this.Controls.Add(this.PictureBox1);
			this.Controls.Add(this.lblPermissions);
			this.Controls.Add(this.proMain);
			this.Controls.Add(this.btnBegin);
			this.Controls.Add(this.lblPrompt);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.pctLogo);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmUpdate";
			this.Text = "SAW update";
			this.Load += new System.EventHandler(this.frmWelcome_Load);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pctLogo)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.PictureBox PictureBox1;
		internal System.Windows.Forms.Label lblPermissions;
		internal System.Windows.Forms.ProgressBar proMain;
		internal System.Windows.Forms.Button btnBegin;
		internal System.Windows.Forms.Label lblPrompt;
		internal System.Windows.Forms.Timer tmrFront;
		private System.Windows.Forms.Button btnCancel;
		internal System.Windows.Forms.PictureBox pctLogo;
	}
}

