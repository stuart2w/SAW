namespace SAW
{
	internal partial class frmRun
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRun));
			this.MainView = new SAW.RunView();
			this.SuspendLayout();
			// 
			// MainView
			// 
			this.MainView.BackColor = System.Drawing.Color.Gray;
			this.MainView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainView.Location = new System.Drawing.Point(0, 0);
			this.MainView.Name = "MainView";
			this.MainView.Size = new System.Drawing.Size(453, 354);
			this.MainView.TabIndex = 0;
			this.MainView.Text = "runView1";
			// 
			// frmRun
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(453, 354);
			this.Controls.Add(this.MainView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmRun";
			this.Text = "frmRun";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmRun_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		private SAW.RunView MainView;
	}
}