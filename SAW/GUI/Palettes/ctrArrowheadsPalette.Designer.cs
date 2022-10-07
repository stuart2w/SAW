
namespace SAW
{
	internal partial class ctrArrowheadsPalette : PalettePanel
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
			System.Windows.Forms.TabPage TabPage1;
			System.Windows.Forms.TabPage TabPage2;
			this.pnlArrowheadStartStyle = new ButtonPanel();
			this.pnlArrowheadStartSize = new ButtonPanel();
			this.pnlArrowheadEndStyle = new ButtonPanel();
			this.pnlArrowheadEndSize = new ButtonPanel();
			this.pnlArrowheads = new System.Windows.Forms.TabControl();
			TabPage1 = new System.Windows.Forms.TabPage();
			TabPage2 = new System.Windows.Forms.TabPage();
			TabPage1.SuspendLayout();
			TabPage2.SuspendLayout();
			this.pnlArrowheads.SuspendLayout();
			this.SuspendLayout();
			//
			//TabPage1
			//
			TabPage1.BackColor = System.Drawing.SystemColors.Control;
			TabPage1.Controls.Add(this.pnlArrowheadStartStyle);
			TabPage1.Controls.Add(this.pnlArrowheadStartSize);
			TabPage1.Location = new System.Drawing.Point(4, 22);
			TabPage1.Name = "TabPage1";
			TabPage1.Padding = new System.Windows.Forms.Padding(3);
			TabPage1.Size = new System.Drawing.Size(176, 113);
			TabPage1.TabIndex = 0;
			TabPage1.Text = "[Arrowhead_Start]";
			//
			//pnlArrowheadStartStyle
			//
			this.pnlArrowheadStartStyle.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlArrowheadStartStyle.ButtonSize = 32;
			this.pnlArrowheadStartStyle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlArrowheadStartStyle.DoubleBuffered = false;
			this.pnlArrowheadStartStyle.Location = new System.Drawing.Point(3, 39);
			this.pnlArrowheadStartStyle.Name = "pnlArrowheadStartStyle";
			this.pnlArrowheadStartStyle.Size = new System.Drawing.Size(170, 71);
			this.pnlArrowheadStartStyle.TabIndex = 3;
			this.pnlArrowheadStartStyle.Text = "ButtonPanel1";
			//
			//pnlArrowheadStartSize
			//
			this.pnlArrowheadStartSize.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlArrowheadStartSize.ButtonSize = 32;
			this.pnlArrowheadStartSize.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlArrowheadStartSize.DoubleBuffered = false;
			this.pnlArrowheadStartSize.Location = new System.Drawing.Point(3, 3);
			this.pnlArrowheadStartSize.Name = "pnlArrowheadStartSize";
			this.pnlArrowheadStartSize.Size = new System.Drawing.Size(170, 36);
			this.pnlArrowheadStartSize.TabIndex = 2;
			this.pnlArrowheadStartSize.Text = "ButtonPanel1";
			//
			//TabPage2
			//
			TabPage2.BackColor = System.Drawing.SystemColors.Control;
			TabPage2.Controls.Add(this.pnlArrowheadEndStyle);
			TabPage2.Controls.Add(this.pnlArrowheadEndSize);
			TabPage2.Location = new System.Drawing.Point(4, 22);
			TabPage2.Name = "TabPage2";
			TabPage2.Padding = new System.Windows.Forms.Padding(3);
			TabPage2.Size = new System.Drawing.Size(176, 113);
			TabPage2.TabIndex = 1;
			TabPage2.Text = "[Arrowhead_End]";
			//
			//pnlArrowheadEndStyle
			//
			this.pnlArrowheadEndStyle.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlArrowheadEndStyle.ButtonSize = 32;
			this.pnlArrowheadEndStyle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlArrowheadEndStyle.DoubleBuffered = false;
			this.pnlArrowheadEndStyle.Location = new System.Drawing.Point(3, 39);
			this.pnlArrowheadEndStyle.Name = "pnlArrowheadEndStyle";
			this.pnlArrowheadEndStyle.Size = new System.Drawing.Size(170, 71);
			this.pnlArrowheadEndStyle.TabIndex = 1;
			this.pnlArrowheadEndStyle.Text = "ButtonPanel1";
			//
			//pnlArrowheadEndSize
			//
			this.pnlArrowheadEndSize.Blend = ButtonPanel.BlendDirection.Off;
			this.pnlArrowheadEndSize.ButtonSize = 32;
			this.pnlArrowheadEndSize.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlArrowheadEndSize.DoubleBuffered = false;
			this.pnlArrowheadEndSize.Location = new System.Drawing.Point(3, 3);
			this.pnlArrowheadEndSize.Name = "pnlArrowheadEndSize";
			this.pnlArrowheadEndSize.Size = new System.Drawing.Size(170, 36);
			this.pnlArrowheadEndSize.TabIndex = 0;
			this.pnlArrowheadEndSize.Text = "ButtonPanel1";
			//
			//pnlArrowheads
			//
			this.pnlArrowheads.Controls.Add(TabPage1);
			this.pnlArrowheads.Controls.Add(TabPage2);
			this.pnlArrowheads.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlArrowheads.Location = new System.Drawing.Point(0, 0);
			this.pnlArrowheads.Name = "pnlArrowheads";
			this.pnlArrowheads.SelectedIndex = 0;
			this.pnlArrowheads.Size = new System.Drawing.Size(184, 139);
			this.pnlArrowheads.TabIndex = 5;
			//
			//ctrArrowheadsPalette
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (13.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlArrowheads);
			this.MinimumSize = new System.Drawing.Size(184, 139);
			this.Name = "ctrArrowheadsPalette";
			this.Size = new System.Drawing.Size(184, 139);
			TabPage1.ResumeLayout(false);
			TabPage2.ResumeLayout(false);
			this.pnlArrowheads.ResumeLayout(false);
			this.ResumeLayout(false);
			
		}
		internal System.Windows.Forms.TabControl pnlArrowheads;
		internal ButtonPanel pnlArrowheadStartStyle;
		internal ButtonPanel pnlArrowheadStartSize;
		internal ButtonPanel pnlArrowheadEndStyle;
		internal ButtonPanel pnlArrowheadEndSize;
		
	}
	
}
