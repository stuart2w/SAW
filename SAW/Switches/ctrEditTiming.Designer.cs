namespace Switches
{
	public partial class ctrEditTiming : System.Windows.Forms.UserControl
	{
		
		//UserControl overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.Windows.Forms.Label lblS;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrEditTiming));
			this.txtTime = new System.Windows.Forms.TextBox();
			this.btnSet = new System.Windows.Forms.Button();
			lblS = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblS
			// 
			lblS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			lblS.AutoSize = true;
			lblS.Location = new System.Drawing.Point(84, 10);
			lblS.Name = "lblS";
			lblS.Size = new System.Drawing.Size(15, 17);
			lblS.TabIndex = 4;
			lblS.Text = "s";
			// 
			// txtTime
			// 
			this.txtTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.txtTime.Location = new System.Drawing.Point(4, 6);
			this.txtTime.Margin = new System.Windows.Forms.Padding(4);
			this.txtTime.Name = "txtTime";
			this.txtTime.Size = new System.Drawing.Size(76, 23);
			this.txtTime.TabIndex = 2;
			this.txtTime.TextChanged += new System.EventHandler(this.txtTime_TextChanged);
			// 
			// btnSet
			// 
			this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSet.Image = ((System.Drawing.Image)(resources.GetObject("btnSet.Image")));
			this.btnSet.Location = new System.Drawing.Point(100, 2);
			this.btnSet.Margin = new System.Windows.Forms.Padding(4);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(32, 32);
			this.btnSet.TabIndex = 3;
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSet_MouseDown);
			this.btnSet.MouseLeave += new System.EventHandler(this.btnSet_MouseLeave);
			this.btnSet.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnSet_MouseUp);
			// 
			// ctrEditTiming
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(lblS);
			this.Controls.Add(this.btnSet);
			this.Controls.Add(this.txtTime);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(100, 32);
			this.Name = "ctrEditTiming";
			this.Size = new System.Drawing.Size(283, 33);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.TextBox txtTime;
		
	}
	
}
