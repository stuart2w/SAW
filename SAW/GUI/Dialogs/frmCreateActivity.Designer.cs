
namespace SAW
{
	public partial class frmCreateActivity : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label11;
			System.Windows.Forms.Label Label12;
			System.Windows.Forms.Label Label4;
			this.btnOK = new System.Windows.Forms.Button();
			base.Load += new System.EventHandler(frmCreateActivity_Load);
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			this.btnCancel = new System.Windows.Forms.Button();
			this.FlowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.txtName = new System.Windows.Forms.TextBox();
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.pnlActivityIcon = new System.Windows.Forms.Panel();
			this.btnChangeActivityIcon = new System.Windows.Forms.Button();
			this.btnChangeActivityIcon.Click += new System.EventHandler(this.btnChangeActivityIcon_Click);
			this.pnlPreviewActivityIcon = new PreviewPanel();
			this.Panel1 = new System.Windows.Forms.Panel();
			this.FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.txtDisplayName = new System.Windows.Forms.TextBox();
			Label1 = new System.Windows.Forms.Label();
			Label2 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			Label11 = new System.Windows.Forms.Label();
			Label12 = new System.Windows.Forms.Label();
			Label4 = new System.Windows.Forms.Label();
			this.FlowLayoutPanel2.SuspendLayout();
			this.pnlActivityIcon.SuspendLayout();
			this.Panel1.SuspendLayout();
			this.FlowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			//Label1
			//
			Label1.AutoSize = true;
			Label1.Location = new System.Drawing.Point(3, 0);
			Label1.Name = "Label1";
			Label1.Size = new System.Drawing.Size(163, 17);
			Label1.TabIndex = 0;
			Label1.Text = "[Activity_Create_Explain]";
			//
			//Label2
			//
			Label2.AutoSize = true;
			Label2.BackColor = System.Drawing.Color.LightGoldenrodYellow;
			Label2.ForeColor = System.Drawing.Color.Red;
			Label2.Location = new System.Drawing.Point(3, 17);
			Label2.Name = "Label2";
			Label2.Size = new System.Drawing.Size(171, 17);
			Label2.TabIndex = 3;
			Label2.Text = "[Activity_Create_Warning]";
			//
			//Label3
			//
			Label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			Label3.AutoSize = true;
			Label3.Location = new System.Drawing.Point(3, 6);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(155, 17);
			Label3.TabIndex = 0;
			Label3.Text = "[Activity_Create_Name]";
			//
			//Label11
			//
			Label11.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			Label11.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			Label11.Location = new System.Drawing.Point(8, 80);
			Label11.Name = "Label11";
			Label11.Size = new System.Drawing.Size(376, 56);
			Label11.TabIndex = 0;
			Label11.Text = "[Config_Activity_Icon_Note]";
			Label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
			//
			//Label12
			//
			Label12.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			Label12.Location = new System.Drawing.Point(8, 8);
			Label12.Name = "Label12";
			Label12.Size = new System.Drawing.Size(379, 40);
			Label12.TabIndex = 2;
			Label12.Text = "[Config_Activity_Icon]";
			Label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			//Label4
			//
			Label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			Label4.AutoSize = true;
			Label4.Location = new System.Drawing.Point(3, 6);
			Label4.Name = "Label4";
			Label4.Size = new System.Drawing.Size(209, 17);
			Label4.TabIndex = 0;
			Label4.Text = "[Activity_Create_Display_Name]";
			//
			//btnOK
			//
			this.btnOK.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(342, 0);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(99, 32);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(454, 0);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 32);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//FlowLayoutPanel2
			//
			this.FlowLayoutPanel2.Controls.Add(Label3);
			this.FlowLayoutPanel2.Controls.Add(this.txtName);
			this.FlowLayoutPanel2.Location = new System.Drawing.Point(3, 37);
			this.FlowLayoutPanel2.Name = "FlowLayoutPanel2";
			this.FlowLayoutPanel2.Size = new System.Drawing.Size(541, 34);
			this.FlowLayoutPanel2.TabIndex = 0;
			//
			//txtName
			//
			this.txtName.Location = new System.Drawing.Point(164, 3);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(284, 23);
			this.txtName.TabIndex = 0;
			//
			//pnlActivityIcon
			//
			this.pnlActivityIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlActivityIcon.Controls.Add(Label11);
			this.pnlActivityIcon.Controls.Add(this.btnChangeActivityIcon);
			this.pnlActivityIcon.Controls.Add(Label12);
			this.pnlActivityIcon.Controls.Add(this.pnlPreviewActivityIcon);
			this.pnlActivityIcon.Location = new System.Drawing.Point(3, 117);
			this.pnlActivityIcon.Name = "pnlActivityIcon";
			this.pnlActivityIcon.Size = new System.Drawing.Size(541, 149);
			this.pnlActivityIcon.TabIndex = 2;
			//
			//btnChangeActivityIcon
			//
			this.btnChangeActivityIcon.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnChangeActivityIcon.AutoSize = true;
			this.btnChangeActivityIcon.Location = new System.Drawing.Point(208, 48);
			this.btnChangeActivityIcon.Name = "btnChangeActivityIcon";
			this.btnChangeActivityIcon.Size = new System.Drawing.Size(174, 27);
			this.btnChangeActivityIcon.TabIndex = 2;
			this.btnChangeActivityIcon.Text = "[Activity_Create_Browse]";
			this.btnChangeActivityIcon.UseVisualStyleBackColor = true;
			//
			//pnlPreviewActivityIcon
			//
			this.pnlPreviewActivityIcon.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.pnlPreviewActivityIcon.Image = null;
			this.pnlPreviewActivityIcon.Location = new System.Drawing.Point(395, 8);
			this.pnlPreviewActivityIcon.Name = "pnlPreviewActivityIcon";
			this.pnlPreviewActivityIcon.Size = new System.Drawing.Size(132, 132);
			this.pnlPreviewActivityIcon.TabIndex = 0;
			//
			//Panel1
			//
			this.Panel1.Controls.Add(this.btnCancel);
			this.Panel1.Controls.Add(this.btnOK);
			this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel1.Location = new System.Drawing.Point(0, 441);
			this.Panel1.MinimumSize = new System.Drawing.Size(500, 0);
			this.Panel1.Name = "Panel1";
			this.Panel1.Size = new System.Drawing.Size(554, 35);
			this.Panel1.TabIndex = 4;
			//
			//FlowLayoutPanel1
			//
			this.FlowLayoutPanel1.Controls.Add(Label1);
			this.FlowLayoutPanel1.Controls.Add(Label2);
			this.FlowLayoutPanel1.Controls.Add(this.FlowLayoutPanel2);
			this.FlowLayoutPanel1.Controls.Add(this.pnlActivityIcon);
			this.FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.FlowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			this.FlowLayoutPanel1.Size = new System.Drawing.Size(554, 476);
			this.FlowLayoutPanel1.TabIndex = 8;
			//
			//txtDisplayName
			//
			this.txtDisplayName.Location = new System.Drawing.Point(218, 3);
			this.txtDisplayName.Name = "txtDisplayName";
			this.txtDisplayName.Size = new System.Drawing.Size(284, 23);
			this.txtDisplayName.TabIndex = 1;
			//
			//frmCreateActivity
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(554, 476);
			this.Controls.Add(this.Panel1);
			this.Controls.Add(this.FlowLayoutPanel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmCreateActivity";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Activity_Create_Title]";
			this.FlowLayoutPanel2.ResumeLayout(false);
			this.FlowLayoutPanel2.PerformLayout();
			this.pnlActivityIcon.ResumeLayout(false);
			this.pnlActivityIcon.PerformLayout();
			this.Panel1.ResumeLayout(false);
			this.FlowLayoutPanel1.ResumeLayout(false);
			this.FlowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel Panel1;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel2;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Panel pnlActivityIcon;
		private System.Windows.Forms.Button btnChangeActivityIcon;
		private PreviewPanel pnlPreviewActivityIcon;
		private System.Windows.Forms.TextBox txtDisplayName;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
	}
	
}
