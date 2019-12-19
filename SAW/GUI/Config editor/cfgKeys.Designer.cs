using System;

namespace SAW
{
	public partial class cfgKeys : ConfigPage
	{
		
		//UserControl overrides dispose to clean up the component list.
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
			System.Windows.Forms.Label Label5;
			System.Windows.Forms.Label Label6;
			this.pnlActions = new System.Windows.Forms.Panel();
			this.tvActions = new SAW.ctrActionSelect();
			this.lblActionCurrentKeys = new System.Windows.Forms.Label();
			this.pnlKeys = new System.Windows.Forms.TableLayoutPanel();
			this.pnlCurrentKey = new System.Windows.Forms.FlowLayoutPanel();
			this.lblKeyCurrent = new System.Windows.Forms.Label();
			this.btnClearKey = new System.Windows.Forms.Button();
			this.txtKey = new SAW.TextBoxAllKeys();
			this.btnSetKey = new System.Windows.Forms.Button();
			this.btnKeysReset = new System.Windows.Forms.Button();
			this.FlowLayoutPanel8 = new System.Windows.Forms.FlowLayoutPanel();
			this.lblKeyWarning = new System.Windows.Forms.Label();
			this.lblKeyAction = new System.Windows.Forms.Label();
			Label3 = new System.Windows.Forms.Label();
			Label5 = new System.Windows.Forms.Label();
			Label6 = new System.Windows.Forms.Label();
			this.pnlActions.SuspendLayout();
			this.pnlKeys.SuspendLayout();
			this.pnlCurrentKey.SuspendLayout();
			this.FlowLayoutPanel8.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label3
			// 
			Label3.BackColor = System.Drawing.Color.PowderBlue;
			this.pnlKeys.SetColumnSpan(Label3, 5);
			Label3.Dock = System.Windows.Forms.DockStyle.Fill;
			Label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label3.Location = new System.Drawing.Point(3, 0);
			Label3.Name = "Label3";
			Label3.Size = new System.Drawing.Size(694, 24);
			Label3.TabIndex = 0;
			Label3.Text = "[Config_KeysTitle]";
			// 
			// Label5
			// 
			Label5.AutoSize = true;
			this.pnlKeys.SetColumnSpan(Label5, 5);
			Label5.Dock = System.Windows.Forms.DockStyle.Fill;
			Label5.Location = new System.Drawing.Point(3, 24);
			Label5.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			Label5.Name = "Label5";
			Label5.Size = new System.Drawing.Size(694, 13);
			Label5.TabIndex = 2;
			Label5.Text = "[Config_KeyPrompt]";
			// 
			// Label6
			// 
			Label6.AutoSize = true;
			Label6.Location = new System.Drawing.Point(3, 0);
			Label6.Name = "Label6";
			Label6.Size = new System.Drawing.Size(101, 13);
			Label6.TabIndex = 7;
			Label6.Text = "[Config_KeyCurrent]";
			// 
			// pnlActions
			// 
			this.pnlKeys.SetColumnSpan(this.pnlActions, 2);
			this.pnlActions.Controls.Add(this.tvActions);
			this.pnlActions.Controls.Add(this.lblActionCurrentKeys);
			this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlActions.Location = new System.Drawing.Point(363, 78);
			this.pnlActions.Name = "pnlActions";
			this.pnlKeys.SetRowSpan(this.pnlActions, 5);
			this.pnlActions.Size = new System.Drawing.Size(334, 525);
			this.pnlActions.TabIndex = 9;
			this.pnlActions.Resize += new System.EventHandler(this.pnlActions_Resize);
			// 
			// tvActions
			// 
			this.tvActions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvActions.Location = new System.Drawing.Point(0, 13);
			this.tvActions.Mode = SAW.ctrActionSelect.Modes.Key;
			this.tvActions.Name = "tvActions";
			this.tvActions.Size = new System.Drawing.Size(334, 512);
			this.tvActions.TabIndex = 1;
			this.tvActions.SelectionChanged += new System.EventHandler(this.tvActions_AfterSelect);
			// 
			// lblActionCurrentKeys
			// 
			this.lblActionCurrentKeys.AutoSize = true;
			this.lblActionCurrentKeys.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblActionCurrentKeys.Location = new System.Drawing.Point(0, 0);
			this.lblActionCurrentKeys.Name = "lblActionCurrentKeys";
			this.lblActionCurrentKeys.Size = new System.Drawing.Size(25, 13);
			this.lblActionCurrentKeys.TabIndex = 1;
			this.lblActionCurrentKeys.Text = "???";
			// 
			// pnlKeys
			// 
			this.pnlKeys.BackColor = System.Drawing.SystemColors.Control;
			this.pnlKeys.ColumnCount = 5;
			this.pnlKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.pnlKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.pnlKeys.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.pnlKeys.Controls.Add(Label3, 0, 0);
			this.pnlKeys.Controls.Add(Label5, 0, 2);
			this.pnlKeys.Controls.Add(this.pnlCurrentKey, 0, 5);
			this.pnlKeys.Controls.Add(this.txtKey, 0, 3);
			this.pnlKeys.Controls.Add(this.btnSetKey, 1, 3);
			this.pnlKeys.Controls.Add(this.btnKeysReset, 0, 8);
			this.pnlKeys.Controls.Add(this.FlowLayoutPanel8, 0, 4);
			this.pnlKeys.Controls.Add(this.pnlActions, 3, 4);
			this.pnlKeys.Controls.Add(this.lblKeyAction, 4, 3);
			this.pnlKeys.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlKeys.Location = new System.Drawing.Point(0, 0);
			this.pnlKeys.Name = "pnlKeys";
			this.pnlKeys.RowCount = 10;
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlKeys.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlKeys.Size = new System.Drawing.Size(700, 626);
			this.pnlKeys.TabIndex = 52;
			// 
			// pnlCurrentKey
			// 
			this.pnlKeys.SetColumnSpan(this.pnlCurrentKey, 2);
			this.pnlCurrentKey.Controls.Add(Label6);
			this.pnlCurrentKey.Controls.Add(this.lblKeyCurrent);
			this.pnlCurrentKey.Controls.Add(this.btnClearKey);
			this.pnlCurrentKey.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlCurrentKey.Location = new System.Drawing.Point(3, 104);
			this.pnlCurrentKey.Name = "pnlCurrentKey";
			this.pnlCurrentKey.Size = new System.Drawing.Size(334, 449);
			this.pnlCurrentKey.TabIndex = 50;
			// 
			// lblKeyCurrent
			// 
			this.lblKeyCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblKeyCurrent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblKeyCurrent.Location = new System.Drawing.Point(3, 13);
			this.lblKeyCurrent.Name = "lblKeyCurrent";
			this.lblKeyCurrent.Size = new System.Drawing.Size(301, 19);
			this.lblKeyCurrent.TabIndex = 8;
			// 
			// btnClearKey
			// 
			this.btnClearKey.AutoSize = true;
			this.btnClearKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnClearKey.Location = new System.Drawing.Point(3, 35);
			this.btnClearKey.Name = "btnClearKey";
			this.btnClearKey.Size = new System.Drawing.Size(101, 23);
			this.btnClearKey.TabIndex = 5;
			this.btnClearKey.Text = "[Config_ClearKey]";
			this.btnClearKey.UseVisualStyleBackColor = true;
			this.btnClearKey.Visible = false;
			this.btnClearKey.Click += new System.EventHandler(this.btnClearKey_Click);
			// 
			// txtKey
			// 
			this.txtKey.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtKey.ForeColor = System.Drawing.Color.Silver;
			this.txtKey.Location = new System.Drawing.Point(3, 45);
			this.txtKey.Name = "txtKey";
			this.txtKey.Size = new System.Drawing.Size(254, 26);
			this.txtKey.TabIndex = 3;
			this.txtKey.Text = "[Config_PressKey]";
			this.txtKey.TextChanged += new System.EventHandler(this.txtKey_TextChanged);
			this.txtKey.GotFocus += new System.EventHandler(this.txtKey_GotFocus);
			this.txtKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtKey_KeyDown);
			// 
			// btnSetKey
			// 
			this.btnSetKey.AutoSize = true;
			this.btnSetKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlKeys.SetColumnSpan(this.btnSetKey, 3);
			this.btnSetKey.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnSetKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSetKey.Location = new System.Drawing.Point(263, 45);
			this.btnSetKey.Name = "btnSetKey";
			this.btnSetKey.Size = new System.Drawing.Size(174, 27);
			this.btnSetKey.TabIndex = 4;
			this.btnSetKey.Text = "[Config_AssignKey]";
			this.btnSetKey.UseVisualStyleBackColor = true;
			this.btnSetKey.Click += new System.EventHandler(this.btnSetKey_Click);
			// 
			// btnKeysReset
			// 
			this.btnKeysReset.AutoSize = true;
			this.pnlKeys.SetColumnSpan(this.btnKeysReset, 2);
			this.btnKeysReset.Location = new System.Drawing.Point(3, 576);
			this.btnKeysReset.Name = "btnKeysReset";
			this.btnKeysReset.Size = new System.Drawing.Size(165, 27);
			this.btnKeysReset.TabIndex = 11;
			this.btnKeysReset.Text = "[Config_Keys_ResetAll]";
			this.btnKeysReset.UseVisualStyleBackColor = true;
			this.btnKeysReset.Click += new System.EventHandler(this.btnKeysReset_Click);
			// 
			// FlowLayoutPanel8
			// 
			this.FlowLayoutPanel8.AutoSize = true;
			this.pnlKeys.SetColumnSpan(this.FlowLayoutPanel8, 2);
			this.FlowLayoutPanel8.Controls.Add(this.lblKeyWarning);
			this.FlowLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlowLayoutPanel8.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.FlowLayoutPanel8.Location = new System.Drawing.Point(3, 78);
			this.FlowLayoutPanel8.Name = "FlowLayoutPanel8";
			this.FlowLayoutPanel8.Size = new System.Drawing.Size(334, 20);
			this.FlowLayoutPanel8.TabIndex = 14;
			// 
			// lblKeyWarning
			// 
			this.lblKeyWarning.AutoSize = true;
			this.lblKeyWarning.BackColor = System.Drawing.Color.PapayaWhip;
			this.lblKeyWarning.Location = new System.Drawing.Point(3, 0);
			this.lblKeyWarning.MinimumSize = new System.Drawing.Size(50, 20);
			this.lblKeyWarning.Name = "lblKeyWarning";
			this.lblKeyWarning.Size = new System.Drawing.Size(50, 20);
			this.lblKeyWarning.TabIndex = 10;
			this.lblKeyWarning.Visible = false;
			// 
			// lblKeyAction
			// 
			this.lblKeyAction.AutoSize = true;
			this.lblKeyAction.BackColor = System.Drawing.Color.White;
			this.lblKeyAction.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblKeyAction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblKeyAction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblKeyAction.ForeColor = System.Drawing.Color.Silver;
			this.lblKeyAction.Location = new System.Drawing.Point(443, 45);
			this.lblKeyAction.Margin = new System.Windows.Forms.Padding(3);
			this.lblKeyAction.Name = "lblKeyAction";
			this.lblKeyAction.Size = new System.Drawing.Size(254, 27);
			this.lblKeyAction.TabIndex = 51;
			this.lblKeyAction.Text = "[Config_KeyNoAction]";
			this.lblKeyAction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cfgKeys
			// 
			this.Controls.Add(this.pnlKeys);
			this.Name = "cfgKeys";
			this.Size = new System.Drawing.Size(700, 626);
			this.pnlActions.ResumeLayout(false);
			this.pnlActions.PerformLayout();
			this.pnlKeys.ResumeLayout(false);
			this.pnlKeys.PerformLayout();
			this.pnlCurrentKey.ResumeLayout(false);
			this.pnlCurrentKey.PerformLayout();
			this.FlowLayoutPanel8.ResumeLayout(false);
			this.FlowLayoutPanel8.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.TableLayoutPanel pnlKeys;
		private System.Windows.Forms.FlowLayoutPanel pnlCurrentKey;
		private System.Windows.Forms.Label lblKeyCurrent;
		private System.Windows.Forms.Button btnClearKey;
		private SAW.TextBoxAllKeys txtKey;
		private System.Windows.Forms.Button btnSetKey;
		private System.Windows.Forms.Button btnKeysReset;
		private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel8;
		private System.Windows.Forms.Label lblKeyWarning;
		internal SAW.ctrActionSelect tvActions;
		private System.Windows.Forms.Label lblActionCurrentKeys;
		private System.Windows.Forms.Label lblKeyAction;
		private System.Windows.Forms.Panel pnlActions;
		
	}
	
}
