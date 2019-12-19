
namespace SAW
{
	public partial class frmFont : System.Windows.Forms.Form
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
			this.Label1 = new System.Windows.Forms.Label();
			base.Load += new System.EventHandler(frmFont_Load);
			this.Disposed += new System.EventHandler(frmFont_Disposed);
			this.lstFont = new System.Windows.Forms.ListBox();
			this.lstFont.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstFont_DrawItem);
			this.lstFont.SelectedIndexChanged += new System.EventHandler(this.lstFont_SelectedIndexChanged);
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.GroupBox1 = new System.Windows.Forms.GroupBox();
			this.lblSample = new System.Windows.Forms.Label();
			this.chkItalic = new System.Windows.Forms.CheckBox();
			this.chkItalic.CheckedChanged += new System.EventHandler(this.Control_Change);
			this.chkBold = new System.Windows.Forms.CheckBox();
			this.chkBold.CheckedChanged += new System.EventHandler(this.Control_Change);
			this.chkStrikeout = new System.Windows.Forms.CheckBox();
			this.chkStrikeout.CheckedChanged += new System.EventHandler(this.Control_Change);
			this.chkUnderline = new System.Windows.Forms.CheckBox();
			this.chkUnderline.CheckedChanged += new System.EventHandler(this.Control_Change);
			this.Label2 = new System.Windows.Forms.Label();
			this.cmbSize = new System.Windows.Forms.ComboBox();
			this.cmbSize.TextChanged += new System.EventHandler(this.Control_Change);
			this.GroupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.Label1.Location = new System.Drawing.Point(5, 4);
			this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(87, 17);
			this.Label1.TabIndex = 0;
			this.Label1.Text = "[Font_Label]";
			//
			//lstFont
			//
			this.lstFont.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.lstFont.FormattingEnabled = true;
			this.lstFont.ItemHeight = 18;
			this.lstFont.Location = new System.Drawing.Point(5, 33);
			this.lstFont.Margin = new System.Windows.Forms.Padding(4);
			this.lstFont.Name = "lstFont";
			this.lstFont.Size = new System.Drawing.Size(201, 220);
			this.lstFont.TabIndex = 1;
			//
			//btnOK
			//
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(299, 273);
			this.btnOK.Margin = new System.Windows.Forms.Padding(4);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(136, 32);
			this.btnOK.TabIndex = 9;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(456, 272);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(147, 33);
			this.btnCancel.TabIndex = 10;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//GroupBox1
			//
			this.GroupBox1.Controls.Add(this.lblSample);
			this.GroupBox1.Location = new System.Drawing.Point(312, 128);
			this.GroupBox1.Margin = new System.Windows.Forms.Padding(4);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Padding = new System.Windows.Forms.Padding(4);
			this.GroupBox1.Size = new System.Drawing.Size(288, 123);
			this.GroupBox1.TabIndex = 6;
			this.GroupBox1.TabStop = false;
			this.GroupBox1.Text = "[Font_Sample]";
			//
			//lblSample
			//
			this.lblSample.Location = new System.Drawing.Point(11, 30);
			this.lblSample.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblSample.Name = "lblSample";
			this.lblSample.Size = new System.Drawing.Size(267, 79);
			this.lblSample.TabIndex = 0;
			this.lblSample.Text = "AaBbYyZz";
			this.lblSample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			//chkItalic
			//
			this.chkItalic.AutoSize = true;
			this.chkItalic.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.chkItalic.Location = new System.Drawing.Point(312, 24);
			this.chkItalic.Margin = new System.Windows.Forms.Padding(4);
			this.chkItalic.Name = "chkItalic";
			this.chkItalic.Size = new System.Drawing.Size(99, 21);
			this.chkItalic.TabIndex = 5;
			this.chkItalic.Text = "[Font_Italic]";
			this.chkItalic.UseVisualStyleBackColor = true;
			//
			//chkBold
			//
			this.chkBold.AutoSize = true;
			this.chkBold.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.chkBold.Location = new System.Drawing.Point(312, 48);
			this.chkBold.Margin = new System.Windows.Forms.Padding(4);
			this.chkBold.Name = "chkBold";
			this.chkBold.Size = new System.Drawing.Size(110, 21);
			this.chkBold.TabIndex = 6;
			this.chkBold.Text = "[Font_Bold]";
			this.chkBold.UseVisualStyleBackColor = true;
			//
			//chkStrikeout
			//
			this.chkStrikeout.AutoSize = true;
			this.chkStrikeout.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.chkStrikeout.Location = new System.Drawing.Point(312, 96);
			this.chkStrikeout.Margin = new System.Windows.Forms.Padding(4);
			this.chkStrikeout.Name = "chkStrikeout";
			this.chkStrikeout.Size = new System.Drawing.Size(127, 21);
			this.chkStrikeout.TabIndex = 8;
			this.chkStrikeout.Text = "[Font_Strikeout]";
			this.chkStrikeout.UseVisualStyleBackColor = true;
			//
			//chkUnderline
			//
			this.chkUnderline.AutoSize = true;
			this.chkUnderline.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.chkUnderline.Location = new System.Drawing.Point(312, 72);
			this.chkUnderline.Margin = new System.Windows.Forms.Padding(4);
			this.chkUnderline.Name = "chkUnderline";
			this.chkUnderline.Size = new System.Drawing.Size(132, 21);
			this.chkUnderline.TabIndex = 7;
			this.chkUnderline.Text = "[Font_Underline]";
			this.chkUnderline.UseVisualStyleBackColor = true;
			//
			//Label2
			//
			this.Label2.AutoSize = true;
			this.Label2.Location = new System.Drawing.Point(219, 4);
			this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(79, 17);
			this.Label2.TabIndex = 2;
			this.Label2.Text = "[Font_Size]";
			//
			//cmbSize
			//
			this.cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
			this.cmbSize.FormattingEnabled = true;
			this.cmbSize.Location = new System.Drawing.Point(219, 33);
			this.cmbSize.Margin = new System.Windows.Forms.Padding(4);
			this.cmbSize.Name = "cmbSize";
			this.cmbSize.Size = new System.Drawing.Size(73, 226);
			this.cmbSize.TabIndex = 4;
			//
			//frmFont
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(612, 317);
			this.Controls.Add(this.cmbSize);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.chkUnderline);
			this.Controls.Add(this.chkStrikeout);
			this.Controls.Add(this.chkBold);
			this.Controls.Add(this.chkItalic);
			this.Controls.Add(this.GroupBox1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.lstFont);
			this.Controls.Add(this.Label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmFont";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Font_Title]";
			this.GroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.Label Label1;
		private System.Windows.Forms.ListBox lstFont;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.GroupBox GroupBox1;
		private System.Windows.Forms.Label lblSample;
		private System.Windows.Forms.CheckBox chkItalic;
		private System.Windows.Forms.CheckBox chkBold;
		private System.Windows.Forms.CheckBox chkStrikeout;
		private System.Windows.Forms.CheckBox chkUnderline;
		private System.Windows.Forms.Label Label2;
		private System.Windows.Forms.ComboBox cmbSize;
	}
	
}
