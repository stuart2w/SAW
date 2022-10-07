namespace SAW.GUI.Dialogs
{
	internal partial class frmYocto
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.lstRelays = new System.Windows.Forms.ListBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtSerial = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtState = new System.Windows.Forms.TextBox();
			this.tblDetails = new System.Windows.Forms.TableLayoutPanel();
			this.btnTest = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.m_tmrState = new System.Windows.Forms.Timer(this.components);
			this.tblDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(103, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "[Yocto_Relays]";
			// 
			// lstRelays
			// 
			this.lstRelays.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lstRelays.FormattingEnabled = true;
			this.lstRelays.ItemHeight = 16;
			this.lstRelays.Location = new System.Drawing.Point(12, 38);
			this.lstRelays.Name = "lstRelays";
			this.lstRelays.Size = new System.Drawing.Size(392, 276);
			this.lstRelays.TabIndex = 1;
			this.lstRelays.SelectedIndexChanged += new System.EventHandler(this.lstRelays_SelectedIndexChanged);
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnClose.Location = new System.Drawing.Point(318, 320);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(86, 31);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "[Close]";
			this.btnClose.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(178, 29);
			this.label2.TabIndex = 3;
			this.label2.Text = "[Yocto_Name]";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtName
			// 
			this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtName.Location = new System.Drawing.Point(187, 3);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(178, 23);
			this.txtName.TabIndex = 4;
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			// 
			// label3
			// 
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(3, 29);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(178, 29);
			this.label3.TabIndex = 5;
			this.label3.Text = "[Yocto_Serial]";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtSerial
			// 
			this.txtSerial.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSerial.Location = new System.Drawing.Point(187, 32);
			this.txtSerial.Name = "txtSerial";
			this.txtSerial.ReadOnly = true;
			this.txtSerial.Size = new System.Drawing.Size(178, 23);
			this.txtSerial.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(178, 29);
			this.label4.TabIndex = 7;
			this.label4.Text = "[Yocto_CurrentState]";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtState
			// 
			this.txtState.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtState.Location = new System.Drawing.Point(187, 61);
			this.txtState.Name = "txtState";
			this.txtState.ReadOnly = true;
			this.txtState.Size = new System.Drawing.Size(178, 23);
			this.txtState.TabIndex = 8;
			// 
			// tblDetails
			// 
			this.tblDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.tblDetails.ColumnCount = 2;
			this.tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tblDetails.Controls.Add(this.label2, 0, 0);
			this.tblDetails.Controls.Add(this.txtState, 1, 2);
			this.tblDetails.Controls.Add(this.txtName, 1, 0);
			this.tblDetails.Controls.Add(this.label4, 0, 2);
			this.tblDetails.Controls.Add(this.label3, 0, 1);
			this.tblDetails.Controls.Add(this.txtSerial, 1, 1);
			this.tblDetails.Controls.Add(this.btnTest, 0, 3);
			this.tblDetails.Controls.Add(this.btnSave, 1, 3);
			this.tblDetails.Location = new System.Drawing.Point(420, 38);
			this.tblDetails.Name = "tblDetails";
			this.tblDetails.RowCount = 5;
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tblDetails.Size = new System.Drawing.Size(368, 276);
			this.tblDetails.TabIndex = 9;
			// 
			// btnTest
			// 
			this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnTest.AutoSize = true;
			this.btnTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnTest.Location = new System.Drawing.Point(83, 90);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(98, 27);
			this.btnTest.TabIndex = 9;
			this.btnTest.Text = "[Yocto_Test]";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// btnSave
			// 
			this.btnSave.AutoSize = true;
			this.btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnSave.Enabled = false;
			this.btnSave.Location = new System.Drawing.Point(187, 90);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(102, 27);
			this.btnSave.TabIndex = 10;
			this.btnSave.Text = "[Yocto_Save]";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefresh.Location = new System.Drawing.Point(12, 320);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(163, 31);
			this.btnRefresh.TabIndex = 10;
			this.btnRefresh.Text = "[Yocto_Refresh]";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// m_tmrState
			// 
			this.m_tmrState.Interval = 300;
			this.m_tmrState.Tick += new System.EventHandler(this.m_tmrState_Tick);
			// 
			// frmYocto
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(800, 365);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.tblDetails);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.lstRelays);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(816, 1100);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(816, 0);
			this.Name = "frmYocto";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Verb_YoctoTest]";
			this.tblDetails.ResumeLayout(false);
			this.tblDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox lstRelays;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtSerial;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtState;
		private System.Windows.Forms.TableLayoutPanel tblDetails;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.Timer m_tmrState;
		private System.Windows.Forms.Button btnSave;
	}
}