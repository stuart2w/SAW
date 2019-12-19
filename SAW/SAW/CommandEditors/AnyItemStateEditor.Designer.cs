namespace SAW.CommandEditors
{
	partial class AnyItemStateEditor
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
			this.rdoThis = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoOther = new System.Windows.Forms.RadioButton();
			this.txtID = new ItemIDTextBox();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// rdoThis
			// 
			this.rdoThis.AutoSize = true;
			this.rdoThis.Dock = System.Windows.Forms.DockStyle.Top;
			this.rdoThis.Location = new System.Drawing.Point(0, 0);
			this.rdoThis.Name = "rdoThis";
			this.rdoThis.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.rdoThis.Size = new System.Drawing.Size(275, 17);
			this.rdoThis.TabIndex = 0;
			this.rdoThis.TabStop = true;
			this.rdoThis.Text = "[SAW_CommandEdit_This]";
			this.rdoThis.UseVisualStyleBackColor = true;
			this.rdoThis.CheckedChanged += new System.EventHandler(this.rdoThis_CheckedChanged);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.rdoOther);
			this.flowLayoutPanel1.Controls.Add(this.txtID);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 17);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(275, 211);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// rdoOther
			// 
			this.rdoOther.AutoSize = true;
			this.rdoOther.Location = new System.Drawing.Point(3, 3);
			this.rdoOther.Name = "rdoOther";
			this.rdoOther.Size = new System.Drawing.Size(159, 17);
			this.rdoOther.TabIndex = 1;
			this.rdoOther.TabStop = true;
			this.rdoOther.Text = "[SAW_CommandEdit_Other]";
			this.rdoOther.UseVisualStyleBackColor = true;
			this.rdoOther.CheckedChanged += new System.EventHandler(this.rdoOther_CheckedChanged);
			// 
			// txtID
			// 
			this.txtID.Enabled = false;
			this.txtID.Location = new System.Drawing.Point(168, 3);
			this.txtID.Name = "txtID";
			this.txtID.Size = new System.Drawing.Size(53, 20);
			this.txtID.TabIndex = 2;
			this.txtID.TextChanged += new System.EventHandler(this.txtID_TextChanged);
			// 
			// AnyItemState
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.rdoThis);
			this.Name = "AnyItemStateEditor";
			this.Size = new System.Drawing.Size(275, 228);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton rdoThis;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.RadioButton rdoOther;
		private ItemIDTextBox txtID;
	}
}
