namespace SAW
{
	internal partial class frmActivityRename : System.Windows.Forms.Form
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
			System.Windows.Forms.Label Label3;
			System.Windows.Forms.Label Label4;
			this.txtName = new System.Windows.Forms.TextBox();
			base.Load += new System.EventHandler(frmActivityRename_Load);
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.txtDisplayName = new System.Windows.Forms.TextBox();
			this.txtDisplayName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			this.Panel1 = new System.Windows.Forms.Panel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.Label1 = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			Label4 = new System.Windows.Forms.Label();
			this.Panel1.SuspendLayout();
			this.TableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			//Label3
			//
			Label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			Label3.AutoSize = true;
			Label3.Location = new System.Drawing.Point(3, 23);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(155, 17);
			Label3.TabIndex = 0;
			Label3.Text = "[Activity_Create_Name]";
			//
			//Label4
			//
			Label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			Label4.AutoSize = true;
			Label4.Location = new System.Drawing.Point(3, 52);
			Label4.Name = "Label4";
			Label4.Size = new System.Drawing.Size(209, 17);
			Label4.TabIndex = 0;
			Label4.Text = "[Activity_Create_Display_Name]";
			//
			//txtName
			//
			this.txtName.Location = new System.Drawing.Point(259, 20);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(251, 23);
			this.txtName.TabIndex = 0;
			//
			//txtDisplayName
			//
			this.txtDisplayName.Location = new System.Drawing.Point(259, 49);
			this.txtDisplayName.Name = "txtDisplayName";
			this.txtDisplayName.Size = new System.Drawing.Size(251, 23);
			this.txtDisplayName.TabIndex = 1;
			//
			//Panel1
			//
			this.Panel1.Controls.Add(this.btnCancel);
			this.Panel1.Controls.Add(this.btnOK);
			this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel1.Location = new System.Drawing.Point(0, 224);
			this.Panel1.MinimumSize = new System.Drawing.Size(500, 0);
			this.Panel1.Name = "Panel1";
			this.Panel1.Size = new System.Drawing.Size(513, 35);
			this.Panel1.TabIndex = 5;
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(413, 0);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 32);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//btnOK
			//
			this.btnOK.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(301, 0);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(99, 32);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			//TableLayoutPanel1
			//
			this.TableLayoutPanel1.ColumnCount = 2;
			this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (50.0F)));
			this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float) (50.0F)));
			this.TableLayoutPanel1.Controls.Add(this.txtDisplayName, 1, 2);
			this.TableLayoutPanel1.Controls.Add(this.txtName, 1, 1);
			this.TableLayoutPanel1.Controls.Add(Label4, 0, 2);
			this.TableLayoutPanel1.Controls.Add(Label3, 0, 1);
			this.TableLayoutPanel1.Controls.Add(this.Label1, 0, 0);
			this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.TableLayoutPanel1.Name = "TableLayoutPanel1";
			this.TableLayoutPanel1.RowCount = 4;
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, (float) (20.0F)));
			this.TableLayoutPanel1.Size = new System.Drawing.Size(513, 224);
			this.TableLayoutPanel1.TabIndex = 6;
			//
			//Label1
			//
			this.Label1.AutoSize = true;
			this.TableLayoutPanel1.SetColumnSpan(this.Label1, 2);
			this.Label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Label1.Location = new System.Drawing.Point(3, 0);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(507, 17);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "[Activity_Rename_Explain]";
			//
			//frmActivityRename
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(513, 259);
			this.Controls.Add(this.TableLayoutPanel1);
			this.Controls.Add(this.Panel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmActivityRename";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Rename]";
			this.Panel1.ResumeLayout(false);
			this.TableLayoutPanel1.ResumeLayout(false);
			this.TableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.TextBox txtDisplayName;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Panel Panel1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
		private System.Windows.Forms.Label Label1;
	}
	
}
