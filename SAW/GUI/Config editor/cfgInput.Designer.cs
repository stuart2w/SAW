namespace SAW
{
	internal partial class cfgInput
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
			System.Windows.Forms.Label Label44;
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.chkReverse = new System.Windows.Forms.CheckBox();
			this.chkUseSwap = new System.Windows.Forms.CheckBox();
			this.chkShowHelp = new System.Windows.Forms.CheckBox();
			this.chkSoundClick = new System.Windows.Forms.CheckBox();
			this.chkRepeatPressStop = new SAW.WrappingCheckBox();
			Label44 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label44
			// 
			Label44.AutoSize = true;
			Label44.BackColor = System.Drawing.Color.PowderBlue;
			Label44.Dock = System.Windows.Forms.DockStyle.Top;
			Label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label44.Location = new System.Drawing.Point(3, 0);
			Label44.Name = "Label44";
			Label44.Size = new System.Drawing.Size(310, 20);
			Label44.TabIndex = 5;
			Label44.Text = "[SAW_Settings_Input]";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(Label44, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.chkReverse, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.chkUseSwap, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.chkShowHelp, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.chkSoundClick, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.chkRepeatPressStop, 0, 5);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(316, 325);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// chkReverse
			// 
			this.chkReverse.AutoSize = true;
			this.chkReverse.Location = new System.Drawing.Point(3, 23);
			this.chkReverse.Name = "chkReverse";
			this.chkReverse.Size = new System.Drawing.Size(190, 17);
			this.chkReverse.TabIndex = 0;
			this.chkReverse.Text = "[SAW_Settings_ReverseSwitches]";
			this.chkReverse.UseVisualStyleBackColor = true;
			this.chkReverse.CheckedChanged += new System.EventHandler(this.chkReverse_CheckedChanged);
			// 
			// chkUseSwap
			// 
			this.chkUseSwap.AutoSize = true;
			this.chkUseSwap.Location = new System.Drawing.Point(3, 46);
			this.chkUseSwap.Name = "chkUseSwap";
			this.chkUseSwap.Size = new System.Drawing.Size(153, 17);
			this.chkUseSwap.TabIndex = 1;
			this.chkUseSwap.Text = "[SAW_Settings_UseSwap]";
			this.chkUseSwap.UseVisualStyleBackColor = true;
			this.chkUseSwap.CheckedChanged += new System.EventHandler(this.chkUseSwap_CheckedChanged);
			// 
			// chkShowHelp
			// 
			this.chkShowHelp.AutoSize = true;
			this.chkShowHelp.Location = new System.Drawing.Point(3, 69);
			this.chkShowHelp.Name = "chkShowHelp";
			this.chkShowHelp.Size = new System.Drawing.Size(156, 17);
			this.chkShowHelp.TabIndex = 2;
			this.chkShowHelp.Text = "[SAW_Settings_ShowHelp]";
			this.chkShowHelp.UseVisualStyleBackColor = true;
			this.chkShowHelp.CheckedChanged += new System.EventHandler(this.chkShowHelp_CheckedChanged);
			// 
			// chkSoundClick
			// 
			this.chkSoundClick.AutoSize = true;
			this.chkSoundClick.Location = new System.Drawing.Point(3, 92);
			this.chkSoundClick.Name = "chkSoundClick";
			this.chkSoundClick.Size = new System.Drawing.Size(161, 17);
			this.chkSoundClick.TabIndex = 3;
			this.chkSoundClick.Text = "[SAW_Settings_SoundClick]";
			this.chkSoundClick.UseVisualStyleBackColor = true;
			this.chkSoundClick.CheckedChanged += new System.EventHandler(this.chkSoundClick_CheckedChanged);
			// 
			// chkRepeatPressStop
			// 
			this.chkRepeatPressStop.AutoSize = true;
			this.chkRepeatPressStop.Location = new System.Drawing.Point(3, 115);
			this.chkRepeatPressStop.Name = "chkRepeatPressStop";
			this.chkRepeatPressStop.Size = new System.Drawing.Size(190, 17);
			this.chkRepeatPressStop.TabIndex = 4;
			this.chkRepeatPressStop.Text = "[SAW_Settings_RepeatPressStop]";
			this.chkRepeatPressStop.UseVisualStyleBackColor = true;
			this.chkRepeatPressStop.CheckedChanged += new System.EventHandler(this.chkRepeatPressStop_CheckedChanged);
			// 
			// cfgInput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "cfgInput";
			this.Size = new System.Drawing.Size(316, 325);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.CheckBox chkReverse;
		private System.Windows.Forms.CheckBox chkUseSwap;
		private System.Windows.Forms.CheckBox chkShowHelp;
		private System.Windows.Forms.CheckBox chkSoundClick;
		private WrappingCheckBox chkRepeatPressStop;
	}
}
