
namespace SAW
{
	public partial class frmPaletteDetails : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label2;
			System.Windows.Forms.Label Label3;
			System.Windows.Forms.Label Label4;
			System.Windows.Forms.Label Label5;
			this.btnOK = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmPaletteDetails_Load);
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtTitle = new System.Windows.Forms.TextBox();
			this.txtTitle.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.cmbPurpose = new System.Windows.Forms.ComboBox();
			this.cmbPurpose.SelectedIndexChanged += new System.EventHandler(this.txtName_TextChanged);
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.txtDescription.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.chkFlow = new System.Windows.Forms.CheckBox();
			this.txtSubTitle = new System.Windows.Forms.TextBox();
			this.lblDuplicate = new System.Windows.Forms.Label();
			Label1 = new System.Windows.Forms.Label();
			Label2 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			Label4 = new System.Windows.Forms.Label();
			Label5 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			//Label1
			//
			Label1.Location = new System.Drawing.Point(8, 8);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(392, 40);
			Label1.TabIndex = 0;
			Label1.Text = "[Palette_RequestTitle]";
			Label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			//Label2
			//
			Label2.AutoSize = true;
			Label2.Location = new System.Drawing.Point(8, 272);
			Label2.Name = "Label2";
			Label2.Size = new System.Drawing.Size(121, 17);
			Label2.TabIndex = 7;
			Label2.Text = "[Palette_Purpose]";
			//
			//Label3
			//
			Label3.Location = new System.Drawing.Point(8, 88);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(392, 40);
			Label3.TabIndex = 2;
			Label3.Text = "[Palette_RequestDescription]";
			Label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			//Label4
			//
			Label4.Location = new System.Drawing.Point(10, 156);
			Label4.Name = "Label4";
			Label4.Size = new System.Drawing.Size(392, 40);
			Label4.TabIndex = 4;
			Label4.Text = "[Palette_RequestSubTitle]";
			Label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			//Label5
			//
			Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.5F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			Label5.Location = new System.Drawing.Point(8, 232);
			Label5.Name = "Label5";
			Label5.Size = new System.Drawing.Size(392, 40);
			Label5.TabIndex = 6;
			Label5.Text = "[Palette_SubtitleExplain]";
			//
			//btnOK
			//
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(200, 392);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(96, 32);
			this.btnOK.TabIndex = 10;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(304, 392);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(96, 32);
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//txtTitle
			//
			this.txtTitle.Location = new System.Drawing.Point(8, 56);
			this.txtTitle.Name = "txtTitle";
			this.txtTitle.Size = new System.Drawing.Size(392, 23);
			this.txtTitle.TabIndex = 1;
			//
			//cmbPurpose
			//
			this.cmbPurpose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPurpose.FormattingEnabled = true;
			this.cmbPurpose.Location = new System.Drawing.Point(8, 296);
			this.cmbPurpose.Name = "cmbPurpose";
			this.cmbPurpose.Size = new System.Drawing.Size(392, 24);
			this.cmbPurpose.TabIndex = 8;
			//
			//txtDescription
			//
			this.txtDescription.Location = new System.Drawing.Point(8, 136);
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.Size = new System.Drawing.Size(392, 23);
			this.txtDescription.TabIndex = 3;
			//
			//chkFlow
			//
			this.chkFlow.Checked = true;
			this.chkFlow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkFlow.Location = new System.Drawing.Point(8, 328);
			this.chkFlow.Name = "chkFlow";
			this.chkFlow.Size = new System.Drawing.Size(392, 40);
			this.chkFlow.TabIndex = 9;
			this.chkFlow.Text = "[Palette_CreateFlow]";
			this.chkFlow.UseVisualStyleBackColor = true;
			//
			//txtSubTitle
			//
			this.txtSubTitle.Location = new System.Drawing.Point(10, 204);
			this.txtSubTitle.Name = "txtSubTitle";
			this.txtSubTitle.Size = new System.Drawing.Size(392, 23);
			this.txtSubTitle.TabIndex = 5;
			//
			//lblDuplicate
			//
			this.lblDuplicate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblDuplicate.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.5F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.lblDuplicate.Location = new System.Drawing.Point(0, 324);
			this.lblDuplicate.Name = "lblDuplicate";
			this.lblDuplicate.Size = new System.Drawing.Size(408, 64);
			this.lblDuplicate.TabIndex = 12;
			this.lblDuplicate.Text = "[Palette_Duplicate]";
			this.lblDuplicate.Visible = false;
			//
			//frmPaletteDetails
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(412, 426);
			this.Controls.Add(this.lblDuplicate);
			this.Controls.Add(Label5);
			this.Controls.Add(this.txtSubTitle);
			this.Controls.Add(Label4);
			this.Controls.Add(this.chkFlow);
			this.Controls.Add(this.txtDescription);
			this.Controls.Add(Label3);
			this.Controls.Add(this.cmbPurpose);
			this.Controls.Add(Label2);
			this.Controls.Add(this.txtTitle);
			this.Controls.Add(Label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPaletteDetails";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Palette_InfoTitle]";
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtTitle;
		private System.Windows.Forms.ComboBox cmbPurpose;
		private System.Windows.Forms.TextBox txtDescription;
		private System.Windows.Forms.CheckBox chkFlow;
		private System.Windows.Forms.TextBox txtSubTitle;
		private System.Windows.Forms.Label lblDuplicate;
	}
	
}
