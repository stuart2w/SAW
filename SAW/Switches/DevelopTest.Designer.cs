

namespace Switches
{
	public partial class DevelopTest : System.Windows.Forms.Form
	{
		
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
			this.Label1 = new System.Windows.Forms.Label();
			this.cmbMethods = new System.Windows.Forms.ComboBox();
			this.Label2 = new System.Windows.Forms.Label();
			this.rdoOne = new System.Windows.Forms.RadioButton();
			this.rdoTwo = new System.Windows.Forms.RadioButton();
			this.Label3 = new System.Windows.Forms.Label();
			this.tblTimings = new System.Windows.Forms.TableLayoutPanel();
			this.CtrEditTiming1 = new Switches.ctrEditTiming();
			this.pnlTest = new Switches.GUI.TestPanel();
			this.ctrSwitch1 = new Switches.ctrSelectSwitch();
			this.ctrSwitch0 = new Switches.ctrSelectSwitch();
			this.ctrDiagram = new Switches.GUI.Diagram();
			this.tblTimings.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(0, 32);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(122, 17);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "Scanning method:";
			// 
			// cmbMethods
			// 
			this.cmbMethods.DisplayMember = "DisplayName";
			this.cmbMethods.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMethods.FormattingEnabled = true;
			this.cmbMethods.Location = new System.Drawing.Point(8, 56);
			this.cmbMethods.Name = "cmbMethods";
			this.cmbMethods.Size = new System.Drawing.Size(296, 24);
			this.cmbMethods.TabIndex = 4;
			this.cmbMethods.ValueMember = "Method";
			this.cmbMethods.SelectedIndexChanged += new System.EventHandler(this.cmbMethods_SelectedIndexChanged);
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(0, 88);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(67, 17);
			this.Label2.TabIndex = 5;
			this.Label2.Text = "Switches:";
			// 
			// rdoOne
			// 
			this.rdoOne.AutoSize = true;
			this.rdoOne.Checked = true;
			this.rdoOne.Location = new System.Drawing.Point(8, 8);
			this.rdoOne.Name = "rdoOne";
			this.rdoOne.Size = new System.Drawing.Size(95, 21);
			this.rdoOne.TabIndex = 8;
			this.rdoOne.TabStop = true;
			this.rdoOne.Text = "One switch";
			this.rdoOne.UseVisualStyleBackColor = true;
			this.rdoOne.CheckedChanged += new System.EventHandler(this.rdoOne_CheckedChanged);
			// 
			// rdoTwo
			// 
			this.rdoTwo.AutoSize = true;
			this.rdoTwo.Location = new System.Drawing.Point(120, 8);
			this.rdoTwo.Name = "rdoTwo";
			this.rdoTwo.Size = new System.Drawing.Size(109, 21);
			this.rdoTwo.TabIndex = 9;
			this.rdoTwo.Text = "Two switches";
			this.rdoTwo.UseVisualStyleBackColor = true;
			this.rdoTwo.CheckedChanged += new System.EventHandler(this.rdoTwo_CheckedChanged);
			// 
			// Label3
			// 
			this.Label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Label3.Location = new System.Drawing.Point(312, 456);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(56, 40);
			this.Label3.TabIndex = 11;
			this.Label3.Text = "Test area:";
			this.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// tblTimings
			// 
			this.tblTimings.ColumnCount = 2;
			this.tblTimings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tblTimings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tblTimings.Controls.Add(this.CtrEditTiming1, 1, 0);
			this.tblTimings.Location = new System.Drawing.Point(12, 193);
			this.tblTimings.Name = "tblTimings";
			this.tblTimings.RowCount = 10;
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblTimings.Size = new System.Drawing.Size(294, 303);
			this.tblTimings.TabIndex = 12;
			// 
			// CtrEditTiming1
			// 
			this.CtrEditTiming1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CtrEditTiming1.Location = new System.Drawing.Point(138, 4);
			this.CtrEditTiming1.Margin = new System.Windows.Forms.Padding(4);
			this.CtrEditTiming1.MinimumSize = new System.Drawing.Size(100, 32);
			this.CtrEditTiming1.Name = "CtrEditTiming1";
			this.CtrEditTiming1.Size = new System.Drawing.Size(152, 33);
			this.CtrEditTiming1.TabIndex = 0;
			// 
			// pnlTest
			// 
			this.pnlTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlTest.BackColor = System.Drawing.Color.White;
			this.pnlTest.Engine = null;
			this.pnlTest.Location = new System.Drawing.Point(376, 456);
			this.pnlTest.Name = "pnlTest";
			this.pnlTest.Size = new System.Drawing.Size(616, 40);
			this.pnlTest.TabIndex = 10;
			// 
			// ctrSwitch1
			// 
			this.ctrSwitch1.Enabled = false;
			this.ctrSwitch1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrSwitch1.Location = new System.Drawing.Point(8, 152);
			this.ctrSwitch1.Margin = new System.Windows.Forms.Padding(4);
			this.ctrSwitch1.Name = "ctrSwitch1";
			this.ctrSwitch1.Number = 1;
			this.ctrSwitch1.Size = new System.Drawing.Size(304, 34);
			this.ctrSwitch1.TabIndex = 7;
			this.ctrSwitch1.UserChangedSwitch += new System.EventHandler(this.ctrSwitch1_UserChangedSwitch);
			// 
			// ctrSwitch0
			// 
			this.ctrSwitch0.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrSwitch0.Location = new System.Drawing.Point(8, 112);
			this.ctrSwitch0.Margin = new System.Windows.Forms.Padding(4);
			this.ctrSwitch0.Name = "ctrSwitch0";
			this.ctrSwitch0.Number = 0;
			this.ctrSwitch0.Size = new System.Drawing.Size(304, 34);
			this.ctrSwitch0.TabIndex = 6;
			this.ctrSwitch0.UserChangedSwitch += new System.EventHandler(this.ctrSwitch0_UserChangedSwitch);
			// 
			// ctrDiagram
			// 
			this.ctrDiagram.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctrDiagram.Location = new System.Drawing.Point(312, 0);
			this.ctrDiagram.Name = "ctrDiagram";
			this.ctrDiagram.Size = new System.Drawing.Size(680, 448);
			this.ctrDiagram.TabIndex = 0;
			// 
			// DevelopTest
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(998, 503);
			this.Controls.Add(this.tblTimings);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.pnlTest);
			this.Controls.Add(this.rdoTwo);
			this.Controls.Add(this.rdoOne);
			this.Controls.Add(this.ctrSwitch1);
			this.Controls.Add(this.ctrSwitch0);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.cmbMethods);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.ctrDiagram);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "DevelopTest";
			this.ShowIcon = false;
			this.Text = "Switch logic test screen";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.tblTimings.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private Switches.GUI.Diagram ctrDiagram;
		internal System.Windows.Forms.Label Label1;
		private Switches.ctrEditTiming CtrEditTiming1;
		private System.Windows.Forms.ComboBox cmbMethods;
		private System.Windows.Forms.Label Label2;
		private Switches.ctrSelectSwitch ctrSwitch0;
		private Switches.ctrSelectSwitch ctrSwitch1;
		private System.Windows.Forms.RadioButton rdoTwo;
		private System.Windows.Forms.RadioButton rdoOne;
		private GUI.TestPanel pnlTest;
		private System.Windows.Forms.Label Label3;
		private System.Windows.Forms.TableLayoutPanel tblTimings;
	}
	
}
