namespace SAW.CommandEditors
{
	partial class WordListScrollEditor
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
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.rdoTop = new System.Windows.Forms.RadioButton();
			this.rdoUp = new System.Windows.Forms.RadioButton();
			this.rdoDown = new System.Windows.Forms.RadioButton();
			this.rdoBottom = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.rdoTop);
			this.flowLayoutPanel1.Controls.Add(this.rdoUp);
			this.flowLayoutPanel1.Controls.Add(this.rdoDown);
			this.flowLayoutPanel1.Controls.Add(this.rdoBottom);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(336, 225);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(174, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "[SAW_CommandEdit_ScrollPrompt]";
			// 
			// rdoTop
			// 
			this.rdoTop.AutoSize = true;
			this.rdoTop.Location = new System.Drawing.Point(3, 16);
			this.rdoTop.Name = "rdoTop";
			this.rdoTop.Size = new System.Drawing.Size(155, 17);
			this.rdoTop.TabIndex = 0;
			this.rdoTop.TabStop = true;
			this.rdoTop.Text = "[Script_CommandPart_Top]";
			this.rdoTop.UseVisualStyleBackColor = true;
			this.rdoTop.CheckedChanged += new System.EventHandler(this.rdoTop_CheckedChanged);
			// 
			// rdoUp
			// 
			this.rdoUp.AutoSize = true;
			this.rdoUp.Location = new System.Drawing.Point(3, 39);
			this.rdoUp.Name = "rdoUp";
			this.rdoUp.Size = new System.Drawing.Size(175, 17);
			this.rdoUp.TabIndex = 1;
			this.rdoUp.TabStop = true;
			this.rdoUp.Text = "[Script_CommandPart_PageUp]";
			this.rdoUp.UseVisualStyleBackColor = true;
			this.rdoUp.CheckedChanged += new System.EventHandler(this.rdoUp_CheckedChanged);
			// 
			// rdoDown
			// 
			this.rdoDown.AutoSize = true;
			this.rdoDown.Location = new System.Drawing.Point(3, 62);
			this.rdoDown.Name = "rdoDown";
			this.rdoDown.Size = new System.Drawing.Size(189, 17);
			this.rdoDown.TabIndex = 2;
			this.rdoDown.TabStop = true;
			this.rdoDown.Text = "[Script_CommandPart_PageDown]";
			this.rdoDown.UseVisualStyleBackColor = true;
			this.rdoDown.CheckedChanged += new System.EventHandler(this.rdoDown_CheckedChanged);
			// 
			// rdoBottom
			// 
			this.rdoBottom.AutoSize = true;
			this.rdoBottom.Location = new System.Drawing.Point(3, 85);
			this.rdoBottom.Name = "rdoBottom";
			this.rdoBottom.Size = new System.Drawing.Size(169, 17);
			this.rdoBottom.TabIndex = 3;
			this.rdoBottom.TabStop = true;
			this.rdoBottom.Text = "[Script_CommandPart_Bottom]";
			this.rdoBottom.UseVisualStyleBackColor = true;
			this.rdoBottom.CheckedChanged += new System.EventHandler(this.rdoBottom_CheckedChanged);
			// 
			// WordListScrollEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Name = "WordListScrollEditor";
			this.Size = new System.Drawing.Size(336, 225);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rdoTop;
		private System.Windows.Forms.RadioButton rdoUp;
		private System.Windows.Forms.RadioButton rdoDown;
		private System.Windows.Forms.RadioButton rdoBottom;
	}
}
