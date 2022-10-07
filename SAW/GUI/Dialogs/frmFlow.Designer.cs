

namespace SAW
{
	internal partial class frmFlow : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label3;
			System.Windows.Forms.Label Label4;
			System.Windows.Forms.Label Label5;
			this.cmbDirection = new System.Windows.Forms.ComboBox();
			base.Load += new System.EventHandler(frmFlow_Load);
			this.cmbDirection.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbDirection_DrawItem);
			this.cmbDirection.SelectedIndexChanged += new System.EventHandler(this.cmbDirection_SelectedIndexChanged);
			this.Label2 = new System.Windows.Forms.Label();
			this.cmbAlignment = new System.Windows.Forms.ComboBox();
			this.cmbAlignment.SelectedIndexChanged += new System.EventHandler(this.cmbAlignment_SelectedIndexChanged);
			this.cmbOverflow = new System.Windows.Forms.ComboBox();
			this.cmbOverflow.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cmbOverflow_DrawItem);
			this.chkDragWithin = new System.Windows.Forms.CheckBox();
			this.chkShowHighlight = new System.Windows.Forms.CheckBox();
			this.nudPadding = new System.Windows.Forms.NumericUpDown();
			this.nudShapeSeparation = new System.Windows.Forms.NumericUpDown();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			Label1 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			Label4 = new System.Windows.Forms.Label();
			Label5 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize) this.nudPadding).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.nudShapeSeparation).BeginInit();
			this.SuspendLayout();
			//
			//cmbDirection
			//
			this.cmbDirection.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.cmbDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbDirection.FormattingEnabled = true;
			this.cmbDirection.ItemHeight = 32;
			this.cmbDirection.Location = new System.Drawing.Point(8, 32);
			this.cmbDirection.Name = "cmbDirection";
			this.cmbDirection.Size = new System.Drawing.Size(304, 38);
			this.cmbDirection.TabIndex = 0;
			this.cmbDirection.Tag = "";
			//
			//Label1
			//
			Label1.AutoSize = true;
			Label1.Location = new System.Drawing.Point(8, 8);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(108, 17);
			Label1.TabIndex = 1;
			Label1.Text = "[Flow_Direction]";
			//
			//Label2
			//
			this.Label2.Location = new System.Drawing.Point(8, 88);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(256, 40);
			this.Label2.TabIndex = 2;
			this.Label2.Text = "[Flow_Alignment]";
			this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//cmbAlignment
			//
			this.cmbAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAlignment.FormattingEnabled = true;
			this.cmbAlignment.Location = new System.Drawing.Point(272, 88);
			this.cmbAlignment.Name = "cmbAlignment";
			this.cmbAlignment.Size = new System.Drawing.Size(136, 24);
			this.cmbAlignment.TabIndex = 3;
			//
			//Label3
			//
			Label3.AutoSize = true;
			Label3.Location = new System.Drawing.Point(8, 136);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(107, 17);
			Label3.TabIndex = 4;
			Label3.Text = "[Flow_Overflow]";
			//
			//cmbOverflow
			//
			this.cmbOverflow.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.cmbOverflow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbOverflow.FormattingEnabled = true;
			this.cmbOverflow.ItemHeight = 32;
			this.cmbOverflow.Location = new System.Drawing.Point(8, 160);
			this.cmbOverflow.Name = "cmbOverflow";
			this.cmbOverflow.Size = new System.Drawing.Size(400, 38);
			this.cmbOverflow.TabIndex = 5;
			this.cmbOverflow.Tag = "";
			//
			//chkDragWithin
			//
			this.chkDragWithin.Location = new System.Drawing.Point(16, 208);
			this.chkDragWithin.Name = "chkDragWithin";
			this.chkDragWithin.Size = new System.Drawing.Size(384, 40);
			this.chkDragWithin.TabIndex = 6;
			this.chkDragWithin.Text = "[Flow_DragWithin]";
			this.chkDragWithin.UseVisualStyleBackColor = true;
			//
			//chkShowHighlight
			//
			this.chkShowHighlight.Location = new System.Drawing.Point(16, 248);
			this.chkShowHighlight.Name = "chkShowHighlight";
			this.chkShowHighlight.Size = new System.Drawing.Size(384, 40);
			this.chkShowHighlight.TabIndex = 7;
			this.chkShowHighlight.Text = "[Container_ShowHighlight]";
			this.chkShowHighlight.UseVisualStyleBackColor = true;
			//
			//Label4
			//
			Label4.Location = new System.Drawing.Point(16, 296);
			Label4.Name = "Label4";
			Label4.Size = new System.Drawing.Size(192, 40);
			Label4.TabIndex = 8;
			Label4.Text = "[Flow_Padding]";
			Label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//nudPadding
			//
			this.nudPadding.DecimalPlaces = 1;
			this.nudPadding.Location = new System.Drawing.Point(216, 296);
			this.nudPadding.Maximum = new decimal(new int[] {10, 0, 0, 0});
			this.nudPadding.Name = "nudPadding";
			this.nudPadding.Size = new System.Drawing.Size(120, 23);
			this.nudPadding.TabIndex = 9;
			//
			//Label5
			//
			Label5.Location = new System.Drawing.Point(16, 336);
			Label5.Name = "Label5";
			Label5.Size = new System.Drawing.Size(192, 40);
			Label5.TabIndex = 10;
			Label5.Text = "[Flow_ShapeSeparation]";
			Label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//nudShapeSeparation
			//
			this.nudShapeSeparation.DecimalPlaces = 1;
			this.nudShapeSeparation.Location = new System.Drawing.Point(216, 336);
			this.nudShapeSeparation.Maximum = new decimal(new int[] {50, 0, 0, 0});
			this.nudShapeSeparation.Minimum = new decimal(new int[] {5, 0, 0, -2147483648});
			this.nudShapeSeparation.Name = "nudShapeSeparation";
			this.nudShapeSeparation.Size = new System.Drawing.Size(120, 23);
			this.nudShapeSeparation.TabIndex = 11;
			//
			//btnOK
			//
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(184, 384);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(104, 32);
			this.btnOK.TabIndex = 12;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(304, 384);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(104, 32);
			this.btnCancel.TabIndex = 13;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//frmFlow
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(419, 425);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.nudShapeSeparation);
			this.Controls.Add(Label5);
			this.Controls.Add(this.nudPadding);
			this.Controls.Add(Label4);
			this.Controls.Add(this.chkShowHighlight);
			this.Controls.Add(this.chkDragWithin);
			this.Controls.Add(this.cmbOverflow);
			this.Controls.Add(Label3);
			this.Controls.Add(this.cmbAlignment);
			this.Controls.Add(this.Label2);
			this.Controls.Add(Label1);
			this.Controls.Add(this.cmbDirection);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmFlow";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[Flow_Settings]";
			((System.ComponentModel.ISupportInitialize) this.nudPadding).EndInit();
			((System.ComponentModel.ISupportInitialize) this.nudShapeSeparation).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.ComboBox cmbDirection;
		private System.Windows.Forms.ComboBox cmbOverflow;
		private System.Windows.Forms.NumericUpDown nudPadding;
		private System.Windows.Forms.CheckBox chkDragWithin;
		private System.Windows.Forms.CheckBox chkShowHighlight;
		private System.Windows.Forms.NumericUpDown nudShapeSeparation;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.ComboBox cmbAlignment;
	}
	
}
