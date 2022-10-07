using System;


namespace SAW
{
	internal partial class frmAddButtons : System.Windows.Forms.Form
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
			System.Windows.Forms.Label lblHeader;
			this.lblKey = new System.Windows.Forms.Label();
			base.Load += new System.EventHandler(frmAddButtons_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(frmAddButtons_KeyDown);
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			this.btnCancel = new System.Windows.Forms.Button();
			this.pnlKey = new System.Windows.Forms.FlowLayoutPanel();
			this.txtKey = new TextBoxAllKeys();
			this.txtKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtKey_KeyDown);
			this.txtKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtKey_KeyPress);
			this.ctrActions = new ctrActionSelect();
			this.ctrActions.SelectionChanged +=new EventHandler (this.ctrActions_AfterSelect);
			this.ctrActions.DoubleClick += new System.EventHandler(this.ctrActions_DoubleClick);
			this.txtText = new System.Windows.Forms.TextBox();
			this.txtText.TextChanged += new System.EventHandler(this.txtText_TextChanged);
			lblHeader = new System.Windows.Forms.Label();
			this.pnlKey.SuspendLayout();
			this.SuspendLayout();
			//
			//lblHeader
			//
			lblHeader.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			lblHeader.Location = new System.Drawing.Point(8, 0);
			lblHeader.Name = "lblHeader";
			lblHeader.Size = new System.Drawing.Size(512, 32);
			lblHeader.TabIndex = 5;
			lblHeader.Text = "[Button_AddButtons_Header]";
			//
			//lblKey
			//
			this.lblKey.AutoSize = true;
			this.lblKey.Location = new System.Drawing.Point(3, 0);
			this.lblKey.Name = "lblKey";
			this.lblKey.Size = new System.Drawing.Size(182, 17);
			this.lblKey.TabIndex = 0;
			this.lblKey.Text = "[Button_AddButtons_Press]";
			this.lblKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			//btnAdd
			//
			this.btnAdd.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnAdd.Enabled = false;
			this.btnAdd.Location = new System.Drawing.Point(293, 496);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(133, 32);
			this.btnAdd.TabIndex = 3;
			this.btnAdd.Text = "[Button_AddButtons_Add]";
			this.btnAdd.UseVisualStyleBackColor = true;
			//
			//btnCancel
			//
			this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles) (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnCancel.Location = new System.Drawing.Point(432, 496);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 32);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "[Close]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//pnlKey
			//
			this.pnlKey.Anchor = (System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.pnlKey.Controls.Add(this.lblKey);
			this.pnlKey.Controls.Add(this.txtKey);
			this.pnlKey.Controls.Add(this.txtText);
			this.pnlKey.Location = new System.Drawing.Point(8, 464);
			this.pnlKey.Name = "pnlKey";
			this.pnlKey.Size = new System.Drawing.Size(504, 24);
			this.pnlKey.TabIndex = 7;
			this.pnlKey.Visible = false;
			//
			//txtKey
			//
			this.txtKey.Location = new System.Drawing.Point(191, 3);
			this.txtKey.Name = "txtKey";
			this.txtKey.Size = new System.Drawing.Size(113, 23);
			this.txtKey.TabIndex = 1;
			//
			//ctrActions
			//
			this.ctrActions.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.ctrActions.Location = new System.Drawing.Point(8, 40);
			this.ctrActions.Mode = ctrActionSelect.Modes.Button;
			this.ctrActions.Name = "ctrActions";
			this.ctrActions.Size = new System.Drawing.Size(504, 416);
			this.ctrActions.TabIndex = 6;
			//
			//txtText
			//
			this.txtText.Location = new System.Drawing.Point(310, 3);
			this.txtText.Name = "txtText";
			this.txtText.Size = new System.Drawing.Size(122, 23);
			this.txtText.TabIndex = 2;
			this.txtText.Visible = false;
			//
			//frmAddButtons
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(523, 536);
			this.Controls.Add(this.pnlKey);
			this.Controls.Add(this.ctrActions);
			this.Controls.Add(lblHeader);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.btnCancel);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte) (0));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmAddButtons";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Verb_QuickAddButtons]";
			this.pnlKey.ResumeLayout(false);
			this.pnlKey.PerformLayout();
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnCancel;
		private ctrActionSelect ctrActions;
		private System.Windows.Forms.FlowLayoutPanel pnlKey;
		private TextBoxAllKeys txtKey;
		private System.Windows.Forms.Label lblKey;
		internal System.Windows.Forms.TextBox txtText;
	}
	
}
