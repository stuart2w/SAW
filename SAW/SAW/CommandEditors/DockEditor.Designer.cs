namespace SAW.CommandEditors
{
	partial class DockEditor
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
			this.rdoAbove = new System.Windows.Forms.RadioButton();
			this.rdoBelow = new System.Windows.Forms.RadioButton();
			this.rdoLeft = new System.Windows.Forms.RadioButton();
			this.rdoRight = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// rdoAbove
			// 
			this.rdoAbove.AutoSize = true;
			this.rdoAbove.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoAbove.Location = new System.Drawing.Point(3, 3);
			this.rdoAbove.Name = "rdoAbove";
			this.rdoAbove.Size = new System.Drawing.Size(238, 17);
			this.rdoAbove.TabIndex = 0;
			this.rdoAbove.TabStop = true;
			this.rdoAbove.Tag = "1";
			this.rdoAbove.Text = "[Script_CommandPart_Above]";
			this.rdoAbove.UseVisualStyleBackColor = true;
			this.rdoAbove.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// rdoBelow
			// 
			this.rdoBelow.AutoSize = true;
			this.rdoBelow.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoBelow.Location = new System.Drawing.Point(3, 20);
			this.rdoBelow.Name = "rdoBelow";
			this.rdoBelow.Size = new System.Drawing.Size(238, 17);
			this.rdoBelow.TabIndex = 1;
			this.rdoBelow.TabStop = true;
			this.rdoBelow.Tag = "2";
			this.rdoBelow.Text = "[Script_CommandPart_Below]";
			this.rdoBelow.UseVisualStyleBackColor = true;
			this.rdoBelow.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// rdoLeft
			// 
			this.rdoLeft.AutoSize = true;
			this.rdoLeft.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoLeft.Location = new System.Drawing.Point(3, 37);
			this.rdoLeft.Name = "rdoLeft";
			this.rdoLeft.Size = new System.Drawing.Size(238, 17);
			this.rdoLeft.TabIndex = 2;
			this.rdoLeft.TabStop = true;
			this.rdoLeft.Tag = "3";
			this.rdoLeft.Text = "[Script_CommandPart_Left]";
			this.rdoLeft.UseVisualStyleBackColor = true;
			this.rdoLeft.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// rdoRight
			// 
			this.rdoRight.AutoSize = true;
			this.rdoRight.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoRight.Location = new System.Drawing.Point(3, 54);
			this.rdoRight.Name = "rdoRight";
			this.rdoRight.Size = new System.Drawing.Size(238, 17);
			this.rdoRight.TabIndex = 3;
			this.rdoRight.TabStop = true;
			this.rdoRight.Tag = "4";
			this.rdoRight.Text = "[Script_CommandPart_Right]";
			this.rdoRight.UseVisualStyleBackColor = true;
			this.rdoRight.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
			// 
			// DockEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.rdoRight);
			this.Controls.Add(this.rdoLeft);
			this.Controls.Add(this.rdoBelow);
			this.Controls.Add(this.rdoAbove);
			this.Name = "DockEditor";
			this.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
			this.Size = new System.Drawing.Size(241, 257);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton rdoAbove;
		private System.Windows.Forms.RadioButton rdoBelow;
		private System.Windows.Forms.RadioButton rdoLeft;
		private System.Windows.Forms.RadioButton rdoRight;
	}
}
