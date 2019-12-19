namespace SAW.CommandEditors
{
	partial class KeysEditor
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblDelay = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.chkCapture = new System.Windows.Forms.CheckBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.chkGIDEI = new System.Windows.Forms.CheckBox();
			this.mnuKeys = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.lblWarning = new System.Windows.Forms.Label();
			this.tmrCheckScript = new System.Windows.Forms.Timer(this.components);
			this.txtDelay = new SAW.CommandEditors.IntegerTextBox();
			this.txtKeys = new SAW.CommandEditors.CapturingTextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.Controls.Add(this.lblDelay, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.txtDelay, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtKeys, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.chkCapture, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnAdd, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkGIDEI, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblWarning, 0, 5);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(357, 265);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblDelay
			// 
			this.lblDelay.AutoSize = true;
			this.lblDelay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDelay.Location = new System.Drawing.Point(3, 139);
			this.lblDelay.Name = "lblDelay";
			this.lblDelay.Size = new System.Drawing.Size(271, 26);
			this.lblDelay.TabIndex = 0;
			this.lblDelay.Text = "[SAW_CommandEdit_KeyDelay]";
			this.lblDelay.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label2, 2);
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(351, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "[SAW_CommandEdit_Keystrokes]";
			// 
			// chkCapture
			// 
			this.chkCapture.AutoSize = true;
			this.chkCapture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkCapture.Location = new System.Drawing.Point(3, 42);
			this.chkCapture.MinimumSize = new System.Drawing.Size(0, 44);
			this.chkCapture.Name = "chkCapture";
			this.chkCapture.Size = new System.Drawing.Size(271, 44);
			this.chkCapture.TabIndex = 4;
			this.chkCapture.Text = "[SAW_CommandEdit_CaptureKeys]";
			this.chkCapture.UseVisualStyleBackColor = true;
			this.chkCapture.CheckedChanged += new System.EventHandler(this.chkCapture_CheckedChanged);
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.AutoSize = true;
			this.btnAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnAdd.Location = new System.Drawing.Point(280, 42);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(74, 23);
			this.btnAdd.TabIndex = 5;
			this.btnAdd.Text = "[SAW_CommandEdit_AddKey]";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// chkGIDEI
			// 
			this.chkGIDEI.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.chkGIDEI, 2);
			this.chkGIDEI.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkGIDEI.Location = new System.Drawing.Point(3, 92);
			this.chkGIDEI.MinimumSize = new System.Drawing.Size(0, 44);
			this.chkGIDEI.Name = "chkGIDEI";
			this.chkGIDEI.Size = new System.Drawing.Size(351, 44);
			this.chkGIDEI.TabIndex = 6;
			this.chkGIDEI.Text = "[SAW_CommandEdit_KeyGIDEI]";
			this.chkGIDEI.UseVisualStyleBackColor = true;
			this.chkGIDEI.CheckedChanged += new System.EventHandler(this.chkGIDEI_CheckedChanged);
			// 
			// mnuKeys
			// 
			this.mnuKeys.Name = "mnuKeys";
			this.mnuKeys.Size = new System.Drawing.Size(61, 4);
			// 
			// lblWarning
			// 
			this.lblWarning.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblWarning, 2);
			this.lblWarning.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblWarning.ForeColor = System.Drawing.Color.Red;
			this.lblWarning.Location = new System.Drawing.Point(3, 165);
			this.lblWarning.Name = "lblWarning";
			this.lblWarning.Size = new System.Drawing.Size(351, 100);
			this.lblWarning.TabIndex = 7;
			// 
			// tmrCheckScript
			// 
			this.tmrCheckScript.Interval = 800;
			this.tmrCheckScript.Tick += new System.EventHandler(this.tmrCheckScript_Tick);
			// 
			// txtDelay
			// 
			this.txtDelay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDelay.Location = new System.Drawing.Point(280, 142);
			this.txtDelay.Name = "txtDelay";
			this.txtDelay.Size = new System.Drawing.Size(74, 20);
			this.txtDelay.TabIndex = 1;
			this.txtDelay.Text = "0";
			this.txtDelay.Value = 0;
			this.txtDelay.TextChanged += new System.EventHandler(this.txtDelay_TextChanged);
			// 
			// txtKeys
			// 
			this.txtKeys.AcceptsReturn = true;
			this.txtKeys.AcceptsTab = true;
			this.tableLayoutPanel1.SetColumnSpan(this.txtKeys, 2);
			this.txtKeys.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtKeys.Location = new System.Drawing.Point(3, 16);
			this.txtKeys.Name = "txtKeys";
			this.txtKeys.Size = new System.Drawing.Size(351, 20);
			this.txtKeys.TabIndex = 3;
			this.txtKeys.TextChanged += new System.EventHandler(this.txtKeys_TextChanged);
			this.txtKeys.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtKeys_KeyDown);
			this.txtKeys.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtKeys_KeyPress);
			// 
			// KeysEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "KeysEditor";
			this.Size = new System.Drawing.Size(357, 265);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label lblDelay;
		private IntegerTextBox txtDelay;
		private System.Windows.Forms.Label label2;
		private CapturingTextBox txtKeys;
		private System.Windows.Forms.CheckBox chkCapture;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.CheckBox chkGIDEI;
		private System.Windows.Forms.ContextMenuStrip mnuKeys;
		private System.Windows.Forms.Label lblWarning;
		private System.Windows.Forms.Timer tmrCheckScript;
	}
}
