
namespace Switches
{
	public partial class ctrSelectSwitch : System.Windows.Forms.UserControl
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
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrSelectSwitch));
			this.lblNumber = new System.Windows.Forms.Label();
			this.btnSet = new System.Windows.Forms.Button();
			this.cmbType = new System.Windows.Forms.ComboBox();
			this.cmbSwitch = new System.Windows.Forms.ComboBox();
			this.lblAutoDetect = new System.Windows.Forms.Label();
			this.tmrDetect = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// lblNumber
			// 
			this.lblNumber.AutoSize = true;
			this.lblNumber.Location = new System.Drawing.Point(0, 5);
			this.lblNumber.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblNumber.Name = "lblNumber";
			this.lblNumber.Size = new System.Drawing.Size(24, 17);
			this.lblNumber.TabIndex = 5;
			this.lblNumber.Text = "#1";
			// 
			// btnSet
			// 
			this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSet.Image = ((System.Drawing.Image)(resources.GetObject("btnSet.Image")));
			this.btnSet.Location = new System.Drawing.Point(286, 0);
			this.btnSet.Margin = new System.Windows.Forms.Padding(4);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(32, 32);
			this.btnSet.TabIndex = 6;
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
			// 
			// cmbType
			// 
			this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbType.FormattingEnabled = true;
			this.cmbType.Location = new System.Drawing.Point(32, 4);
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new System.Drawing.Size(112, 24);
			this.cmbType.TabIndex = 7;
			this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
			// 
			// cmbSwitch
			// 
			this.cmbSwitch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmbSwitch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSwitch.Enabled = false;
			this.cmbSwitch.FormattingEnabled = true;
			this.cmbSwitch.Location = new System.Drawing.Point(152, 4);
			this.cmbSwitch.Name = "cmbSwitch";
			this.cmbSwitch.Size = new System.Drawing.Size(124, 24);
			this.cmbSwitch.TabIndex = 8;
			this.cmbSwitch.SelectedIndexChanged += new System.EventHandler(this.cmbSwitch_SelectedIndexChanged);
			// 
			// lblAutoDetect
			// 
			this.lblAutoDetect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAutoDetect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblAutoDetect.Location = new System.Drawing.Point(24, 0);
			this.lblAutoDetect.Name = "lblAutoDetect";
			this.lblAutoDetect.Size = new System.Drawing.Size(256, 32);
			this.lblAutoDetect.TabIndex = 9;
			this.lblAutoDetect.Text = "Press switch now";
			this.lblAutoDetect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblAutoDetect.Visible = false;
			// 
			// tmrDetect
			// 
			this.tmrDetect.Interval = 1000;
			this.tmrDetect.Tick += new System.EventHandler(this.tmrDetect_Tick);
			// 
			// ctrSelectSwitch
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.lblAutoDetect);
			this.Controls.Add(this.cmbSwitch);
			this.Controls.Add(this.cmbType);
			this.Controls.Add(this.btnSet);
			this.Controls.Add(this.lblNumber);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "ctrSelectSwitch";
			this.Size = new System.Drawing.Size(320, 34);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.Button btnSet;
		private System.Windows.Forms.Label lblNumber;
		private System.Windows.Forms.ComboBox cmbType;
		private System.Windows.Forms.ComboBox cmbSwitch;
		private System.Windows.Forms.Label lblAutoDetect;
		private System.Windows.Forms.Timer tmrDetect;
		private System.ComponentModel.IContainer components;
	}
	
}
