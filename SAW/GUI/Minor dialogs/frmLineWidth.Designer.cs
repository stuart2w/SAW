
namespace SAW
{
	public partial class frmLineWidth : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
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
			System.Windows.Forms.Label Label1;
			this.btnCancel = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmLineWidth_Load);
			this.btnOK = new System.Windows.Forms.Button();
			this.nudWidth = new System.Windows.Forms.NumericUpDown();
			this.nudWidth.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nudWidth_KeyUp);
			this.nudWidth.ValueChanged += new System.EventHandler(this.nudWidth_ValueChanged);
			this.Label2 = new System.Windows.Forms.Label();
			this.pnlSample = new System.Windows.Forms.Panel();
			this.pnlSample.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSample_Paint);
			Label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize) this.nudWidth).BeginInit();
			this.SuspendLayout();
			//
			//Label1
			//
			Label1.AutoSize = true;
			Label1.Location = new System.Drawing.Point(8, 16);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(87, 17);
			Label1.TabIndex = 4;
			Label1.Text = "[Line_Width]";
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(240, 56);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(5);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(136, 39);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//btnOK
			//
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(96, 57);
			this.btnOK.Margin = new System.Windows.Forms.Padding(5);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(133, 39);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//nudWidth
			//
			this.nudWidth.DecimalPlaces = 1;
			this.nudWidth.Increment = new decimal(new int[] {1, 0, 0, 65536});
			this.nudWidth.Location = new System.Drawing.Point(104, 16);
			this.nudWidth.Maximum = new decimal(new int[] {25, 0, 0, 0});
			this.nudWidth.Minimum = new decimal(new int[] {1, 0, 0, 65536});
			this.nudWidth.Name = "nudWidth";
			this.nudWidth.Size = new System.Drawing.Size(72, 23);
			this.nudWidth.TabIndex = 5;
			this.nudWidth.Value = new decimal(new int[] {1, 0, 0, 0});
			//
			//Label2
			//
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(176, 16);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(28, 17);
			this.Label2.TabIndex = 6;
			this.Label2.Text = "[pt]";
			//
			//pnlSample
			//
			this.pnlSample.Location = new System.Drawing.Point(216, 8);
			this.pnlSample.Name = "pnlSample";
			this.pnlSample.Size = new System.Drawing.Size(160, 40);
			this.pnlSample.TabIndex = 7;
			//
			//frmLineWidth
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(388, 108);
			this.Controls.Add(this.pnlSample);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.nudWidth);
			this.Controls.Add(Label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmLineWidth";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Line_Width]";
			((System.ComponentModel.ISupportInitialize) this.nudWidth).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.NumericUpDown nudWidth;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.Panel pnlSample;
	}
	
}
