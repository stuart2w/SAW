namespace SAW.CommandEditors
{
	partial class OnOffEditor
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
			this.rdoOn = new System.Windows.Forms.RadioButton();
			this.rdoOff = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// rdoOn
			// 
			this.rdoOn.AutoSize = true;
			this.rdoOn.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoOn.Location = new System.Drawing.Point(3, 3);
			this.rdoOn.Name = "rdoOn";
			this.rdoOn.Size = new System.Drawing.Size(302, 17);
			this.rdoOn.TabIndex = 0;
			this.rdoOn.TabStop = true;
			this.rdoOn.Text = "[Script_CommandParam_On]";
			this.rdoOn.UseVisualStyleBackColor = true;
			this.rdoOn.CheckedChanged += new System.EventHandler(this.rdoOn_CheckedChanged);
			// 
			// rdoOff
			// 
			this.rdoOff.AutoSize = true;
			this.rdoOff.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoOff.Location = new System.Drawing.Point(3, 20);
			this.rdoOff.Name = "rdoOff";
			this.rdoOff.Size = new System.Drawing.Size(302, 17);
			this.rdoOff.TabIndex = 1;
			this.rdoOff.TabStop = true;
			this.rdoOff.Text = "[Script_CommandParam_Off]";
			this.rdoOff.UseVisualStyleBackColor = true;
			this.rdoOff.CheckedChanged += new System.EventHandler(this.rdoOff_CheckedChanged);
			// 
			// OnOffEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.rdoOff);
			this.Controls.Add(this.rdoOn);
			this.Name = "OnOffEditor";
			this.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
			this.Size = new System.Drawing.Size(305, 190);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton rdoOn;
		private System.Windows.Forms.RadioButton rdoOff;
	}
}
