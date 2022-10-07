namespace SAW
{
	internal partial class ctrScriptableEdit
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
			this.tcMain = new System.Windows.Forms.TabControl();
			this.tpVisit = new System.Windows.Forms.TabPage();
			this.tpSelect = new System.Windows.Forms.TabPage();
			this.tpNext = new System.Windows.Forms.TabPage();
			this.tpPre = new System.Windows.Forms.TabPage();
			this.tpRepeat = new System.Windows.Forms.TabPage();
			this.tpPost = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ctrScriptEditor = new SAW.ctrScriptEdit();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.chkRepeats = new System.Windows.Forms.CheckBox();
			this.lblTimeout = new System.Windows.Forms.Label();
			this.txtTimeout = new System.Windows.Forms.TextBox();
			this.btnTestRepeat = new System.Windows.Forms.Button();
			this.tcMain.SuspendLayout();
			this.panel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tcMain
			// 
			this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tcMain.Controls.Add(this.tpVisit);
			this.tcMain.Controls.Add(this.tpSelect);
			this.tcMain.Controls.Add(this.tpNext);
			this.tcMain.Controls.Add(this.tpPre);
			this.tcMain.Controls.Add(this.tpRepeat);
			this.tcMain.Controls.Add(this.tpPost);
			this.tcMain.Location = new System.Drawing.Point(3, 3);
			this.tcMain.Name = "tcMain";
			this.tcMain.SelectedIndex = 0;
			this.tcMain.Size = new System.Drawing.Size(585, 390);
			this.tcMain.TabIndex = 0;
			this.tcMain.SelectedIndexChanged += new System.EventHandler(this.tcMain_SelectedIndexChanged);
			// 
			// tpVisit
			// 
			this.tpVisit.Location = new System.Drawing.Point(4, 22);
			this.tpVisit.Name = "tpVisit";
			this.tpVisit.Padding = new System.Windows.Forms.Padding(3);
			this.tpVisit.Size = new System.Drawing.Size(577, 364);
			this.tpVisit.TabIndex = 0;
			this.tpVisit.Text = "[SAW_ScriptType_Visit]";
			this.tpVisit.UseVisualStyleBackColor = true;
			// 
			// tpSelect
			// 
			this.tpSelect.Location = new System.Drawing.Point(4, 22);
			this.tpSelect.Name = "tpSelect";
			this.tpSelect.Padding = new System.Windows.Forms.Padding(3);
			this.tpSelect.Size = new System.Drawing.Size(577, 364);
			this.tpSelect.TabIndex = 1;
			this.tpSelect.Text = "[SAW_ScriptType_Select]";
			this.tpSelect.UseVisualStyleBackColor = true;
			// 
			// tpNext
			// 
			this.tpNext.Location = new System.Drawing.Point(4, 22);
			this.tpNext.Name = "tpNext";
			this.tpNext.Size = new System.Drawing.Size(577, 364);
			this.tpNext.TabIndex = 2;
			this.tpNext.Text = "[SAW_ScriptType_Next]";
			this.tpNext.UseVisualStyleBackColor = true;
			// 
			// tpPre
			// 
			this.tpPre.Location = new System.Drawing.Point(4, 22);
			this.tpPre.Name = "tpPre";
			this.tpPre.Size = new System.Drawing.Size(577, 364);
			this.tpPre.TabIndex = 3;
			this.tpPre.Text = "[SAW_ScriptType_PreRepeat]";
			this.tpPre.UseVisualStyleBackColor = true;
			// 
			// tpRepeat
			// 
			this.tpRepeat.Location = new System.Drawing.Point(4, 22);
			this.tpRepeat.Name = "tpRepeat";
			this.tpRepeat.Size = new System.Drawing.Size(577, 364);
			this.tpRepeat.TabIndex = 4;
			this.tpRepeat.Text = "[SAW_ScriptType_Repeat]";
			this.tpRepeat.UseVisualStyleBackColor = true;
			// 
			// tpPost
			// 
			this.tpPost.Location = new System.Drawing.Point(4, 22);
			this.tpPost.Name = "tpPost";
			this.tpPost.Size = new System.Drawing.Size(577, 364);
			this.tpPost.TabIndex = 5;
			this.tpPost.Text = "[SAW_ScriptType_PostRepeat]";
			this.tpPost.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.ctrScriptEditor);
			this.panel1.Location = new System.Drawing.Point(3, 25);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(585, 379);
			this.panel1.TabIndex = 1;
			// 
			// ctrScriptEditor
			// 
			this.ctrScriptEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctrScriptEditor.BackColor = System.Drawing.SystemColors.Control;
			this.ctrScriptEditor.Location = new System.Drawing.Point(0, 3);
			this.ctrScriptEditor.Name = "ctrScriptEditor";
			this.ctrScriptEditor.Size = new System.Drawing.Size(585, 370);
			this.ctrScriptEditor.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.flowLayoutPanel1.Controls.Add(this.chkRepeats);
			this.flowLayoutPanel1.Controls.Add(this.lblTimeout);
			this.flowLayoutPanel1.Controls.Add(this.txtTimeout);
			this.flowLayoutPanel1.Controls.Add(this.btnTestRepeat);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 399);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(585, 27);
			this.flowLayoutPanel1.TabIndex = 2;
			// 
			// chkRepeats
			// 
			this.chkRepeats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.chkRepeats.AutoSize = true;
			this.chkRepeats.Checked = true;
			this.chkRepeats.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRepeats.Location = new System.Drawing.Point(3, 3);
			this.chkRepeats.Name = "chkRepeats";
			this.chkRepeats.Size = new System.Drawing.Size(127, 23);
			this.chkRepeats.TabIndex = 0;
			this.chkRepeats.Text = "[SAW_Edit_Repeats]";
			this.chkRepeats.UseVisualStyleBackColor = true;
			this.chkRepeats.CheckedChanged += new System.EventHandler(this.chkRepeats_CheckedChanged);
			// 
			// lblTimeout
			// 
			this.lblTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblTimeout.AutoSize = true;
			this.lblTimeout.Location = new System.Drawing.Point(136, 0);
			this.lblTimeout.Name = "lblTimeout";
			this.lblTimeout.Size = new System.Drawing.Size(106, 29);
			this.lblTimeout.TabIndex = 3;
			this.lblTimeout.Text = "[SAW_Edit_Timeout]";
			this.lblTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtTimeout
			// 
			this.txtTimeout.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtTimeout.Location = new System.Drawing.Point(248, 4);
			this.txtTimeout.Name = "txtTimeout";
			this.txtTimeout.Size = new System.Drawing.Size(41, 20);
			this.txtTimeout.TabIndex = 1;
			this.txtTimeout.TextChanged += new System.EventHandler(this.txtTimeout_TextChanged);
			// 
			// btnTestRepeat
			// 
			this.btnTestRepeat.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnTestRepeat.AutoSize = true;
			this.btnTestRepeat.Location = new System.Drawing.Point(295, 3);
			this.btnTestRepeat.Name = "btnTestRepeat";
			this.btnTestRepeat.Size = new System.Drawing.Size(134, 23);
			this.btnTestRepeat.TabIndex = 2;
			this.btnTestRepeat.Text = "[SAW_Edit_TestRepeat]";
			this.btnTestRepeat.UseVisualStyleBackColor = true;
			// 
			// ctrScriptableEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.tcMain);
			this.Name = "ctrScriptableEdit";
			this.Size = new System.Drawing.Size(592, 435);
			this.tcMain.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tcMain;
		private System.Windows.Forms.TabPage tpVisit;
		private System.Windows.Forms.TabPage tpSelect;
		private System.Windows.Forms.TabPage tpNext;
		private System.Windows.Forms.TabPage tpPre;
		private System.Windows.Forms.TabPage tpRepeat;
		private System.Windows.Forms.TabPage tpPost;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.CheckBox chkRepeats;
		private System.Windows.Forms.TextBox txtTimeout;
		private System.Windows.Forms.Button btnTestRepeat;
		private System.Windows.Forms.Label lblTimeout;
		private SAW.ctrScriptEdit ctrScriptEditor;
	}
}
