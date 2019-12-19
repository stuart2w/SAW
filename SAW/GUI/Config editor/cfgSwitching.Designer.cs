namespace SAW
{
	partial class cfgSwitching
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
			this.label1 = new System.Windows.Forms.Label();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.rdoMouse = new System.Windows.Forms.RadioButton();
			this.rdoOne = new System.Windows.Forms.RadioButton();
			this.rdoTwo = new System.Windows.Forms.RadioButton();
			this.lblScanType = new System.Windows.Forms.Label();
			this.cmbScanType = new System.Windows.Forms.ComboBox();
			this.lblSwitch1 = new System.Windows.Forms.Label();
			this.ctrSwitch1 = new Switches.ctrSelectSwitch();
			this.ctrSwitch2 = new Switches.ctrSelectSwitch();
			this.ctrTimingAcceptance = new Switches.ctrEditTiming();
			this.lblAcceptance = new System.Windows.Forms.Label();
			this.ctrTimingPostActivation = new Switches.ctrEditTiming();
			this.ctrTimingLongPress = new Switches.ctrEditTiming();
			this.ctrTimingScanTime = new Switches.ctrEditTiming();
			this.ctrTimingFirstRepeat = new Switches.ctrEditTiming();
			this.ctrTimingSubsequentRepeat = new Switches.ctrEditTiming();
			this.lblPostActivation = new System.Windows.Forms.Label();
			this.lblLongPress = new System.Windows.Forms.Label();
			this.lblScanTime = new System.Windows.Forms.Label();
			this.lblFirstRepeat = new System.Windows.Forms.Label();
			this.lblSubsequentRepeat = new System.Windows.Forms.Label();
			this.lblDwellSelect = new System.Windows.Forms.Label();
			this.lblCriticalReverse = new System.Windows.Forms.Label();
			this.ctrTimingDwellTime = new Switches.ctrEditTiming();
			this.ctrTimingCriticalReverse = new Switches.ctrEditTiming();
			Label44 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label44
			// 
			Label44.AutoSize = true;
			Label44.BackColor = System.Drawing.Color.PowderBlue;
			this.tableLayoutPanel1.SetColumnSpan(Label44, 2);
			Label44.Dock = System.Windows.Forms.DockStyle.Top;
			Label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label44.Location = new System.Drawing.Point(3, 0);
			Label44.Name = "Label44";
			Label44.Size = new System.Drawing.Size(615, 20);
			Label44.TabIndex = 24;
			Label44.Text = "[SAW_Settings_Switches]";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
			this.tableLayoutPanel1.Controls.Add(Label44, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblScanType, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.cmbScanType, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblSwitch1, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.ctrSwitch1, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.ctrSwitch2, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingAcceptance, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.lblAcceptance, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingPostActivation, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingLongPress, 1, 8);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingScanTime, 1, 9);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingFirstRepeat, 1, 10);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingSubsequentRepeat, 1, 11);
			this.tableLayoutPanel1.Controls.Add(this.lblPostActivation, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.lblLongPress, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.lblScanTime, 0, 9);
			this.tableLayoutPanel1.Controls.Add(this.lblFirstRepeat, 0, 10);
			this.tableLayoutPanel1.Controls.Add(this.lblSubsequentRepeat, 0, 11);
			this.tableLayoutPanel1.Controls.Add(this.lblDwellSelect, 0, 12);
			this.tableLayoutPanel1.Controls.Add(this.lblCriticalReverse, 0, 13);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingDwellTime, 1, 12);
			this.tableLayoutPanel1.Controls.Add(this.ctrTimingCriticalReverse, 1, 13);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 15;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(621, 542);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(211, 52);
			this.label1.TabIndex = 0;
			this.label1.Text = "[SAW_Settings_NumberSwitches]";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.rdoMouse);
			this.flowLayoutPanel1.Controls.Add(this.rdoOne);
			this.flowLayoutPanel1.Controls.Add(this.rdoTwo);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(220, 23);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(398, 46);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// rdoMouse
			// 
			this.rdoMouse.AutoSize = true;
			this.rdoMouse.Location = new System.Drawing.Point(3, 3);
			this.rdoMouse.Name = "rdoMouse";
			this.rdoMouse.Size = new System.Drawing.Size(138, 17);
			this.rdoMouse.TabIndex = 1;
			this.rdoMouse.TabStop = true;
			this.rdoMouse.Tag = "0";
			this.rdoMouse.Text = "[SAW_Settings_Mouse]";
			this.rdoMouse.UseVisualStyleBackColor = true;
			this.rdoMouse.CheckedChanged += new System.EventHandler(this.rdoSwitchCount_CheckedChanged);
			// 
			// rdoOne
			// 
			this.rdoOne.AutoSize = true;
			this.rdoOne.Location = new System.Drawing.Point(147, 3);
			this.rdoOne.Name = "rdoOne";
			this.rdoOne.Size = new System.Drawing.Size(144, 17);
			this.rdoOne.TabIndex = 2;
			this.rdoOne.TabStop = true;
			this.rdoOne.Tag = "1";
			this.rdoOne.Text = "[SAW_Settings_1Switch]";
			this.rdoOne.UseVisualStyleBackColor = true;
			this.rdoOne.CheckedChanged += new System.EventHandler(this.rdoSwitchCount_CheckedChanged);
			// 
			// rdoTwo
			// 
			this.rdoTwo.AutoSize = true;
			this.rdoTwo.Location = new System.Drawing.Point(3, 26);
			this.rdoTwo.Name = "rdoTwo";
			this.rdoTwo.Size = new System.Drawing.Size(144, 17);
			this.rdoTwo.TabIndex = 3;
			this.rdoTwo.TabStop = true;
			this.rdoTwo.Tag = "2";
			this.rdoTwo.Text = "[SAW_Settings_2Switch]";
			this.rdoTwo.UseVisualStyleBackColor = true;
			this.rdoTwo.CheckedChanged += new System.EventHandler(this.rdoSwitchCount_CheckedChanged);
			// 
			// lblScanType
			// 
			this.lblScanType.AutoSize = true;
			this.lblScanType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblScanType.Location = new System.Drawing.Point(3, 72);
			this.lblScanType.Name = "lblScanType";
			this.lblScanType.Size = new System.Drawing.Size(211, 27);
			this.lblScanType.TabIndex = 1;
			this.lblScanType.Text = "[SAW_Settings_ScanType]";
			this.lblScanType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cmbScanType
			// 
			this.cmbScanType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbScanType.FormattingEnabled = true;
			this.cmbScanType.Location = new System.Drawing.Point(220, 75);
			this.cmbScanType.Name = "cmbScanType";
			this.cmbScanType.Size = new System.Drawing.Size(277, 21);
			this.cmbScanType.TabIndex = 2;
			this.cmbScanType.SelectedIndexChanged += new System.EventHandler(this.cmbScanType_SelectedIndexChanged);
			// 
			// lblSwitch1
			// 
			this.lblSwitch1.AutoSize = true;
			this.lblSwitch1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSwitch1.Location = new System.Drawing.Point(3, 107);
			this.lblSwitch1.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.lblSwitch1.Name = "lblSwitch1";
			this.lblSwitch1.Size = new System.Drawing.Size(211, 34);
			this.lblSwitch1.TabIndex = 3;
			this.lblSwitch1.Text = "[SAW_Settings_Switches]";
			this.lblSwitch1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// ctrSwitch1
			// 
			this.ctrSwitch1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrSwitch1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrSwitch1.Location = new System.Drawing.Point(221, 103);
			this.ctrSwitch1.Margin = new System.Windows.Forms.Padding(4);
			this.ctrSwitch1.Name = "ctrSwitch1";
			this.ctrSwitch1.Number = 0;
			this.ctrSwitch1.Size = new System.Drawing.Size(396, 34);
			this.ctrSwitch1.TabIndex = 7;
			this.ctrSwitch1.UserChangedSwitch += new System.EventHandler(this.ctrSwitch1_UserChangedSwitch);
			// 
			// ctrSwitch2
			// 
			this.ctrSwitch2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrSwitch2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrSwitch2.Location = new System.Drawing.Point(221, 145);
			this.ctrSwitch2.Margin = new System.Windows.Forms.Padding(4);
			this.ctrSwitch2.Name = "ctrSwitch2";
			this.ctrSwitch2.Number = 1;
			this.ctrSwitch2.Size = new System.Drawing.Size(396, 34);
			this.ctrSwitch2.TabIndex = 8;
			this.ctrSwitch2.UserChangedSwitch += new System.EventHandler(this.ctrSwitch2_UserChangedSwitch);
			// 
			// ctrTimingAcceptance
			// 
			this.ctrTimingAcceptance.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingAcceptance.Location = new System.Drawing.Point(217, 203);
			this.ctrTimingAcceptance.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingAcceptance.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingAcceptance.Name = "ctrTimingAcceptance";
			this.ctrTimingAcceptance.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingAcceptance.TabIndex = 5;
			this.ctrTimingAcceptance.Tag = "1";
			this.ctrTimingAcceptance.UserChangedValue += new System.EventHandler(this.ctrTiming_UserChangedValue);
			// 
			// lblAcceptance
			// 
			this.lblAcceptance.AutoSize = true;
			this.lblAcceptance.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblAcceptance.Location = new System.Drawing.Point(3, 203);
			this.lblAcceptance.Name = "lblAcceptance";
			this.lblAcceptance.Size = new System.Drawing.Size(211, 33);
			this.lblAcceptance.TabIndex = 9;
			this.lblAcceptance.Tag = "1";
			this.lblAcceptance.Text = "label2";
			this.lblAcceptance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ctrTimingPostActivation
			// 
			this.ctrTimingPostActivation.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingPostActivation.Location = new System.Drawing.Point(217, 236);
			this.ctrTimingPostActivation.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingPostActivation.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingPostActivation.Name = "ctrTimingPostActivation";
			this.ctrTimingPostActivation.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingPostActivation.TabIndex = 10;
			this.ctrTimingPostActivation.Tag = "2";
			// 
			// ctrTimingLongPress
			// 
			this.ctrTimingLongPress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingLongPress.Location = new System.Drawing.Point(217, 269);
			this.ctrTimingLongPress.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingLongPress.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingLongPress.Name = "ctrTimingLongPress";
			this.ctrTimingLongPress.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingLongPress.TabIndex = 11;
			this.ctrTimingLongPress.Tag = "4";
			// 
			// ctrTimingScanTime
			// 
			this.ctrTimingScanTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingScanTime.Location = new System.Drawing.Point(217, 302);
			this.ctrTimingScanTime.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingScanTime.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingScanTime.Name = "ctrTimingScanTime";
			this.ctrTimingScanTime.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingScanTime.TabIndex = 12;
			this.ctrTimingScanTime.Tag = "8";
			// 
			// ctrTimingFirstRepeat
			// 
			this.ctrTimingFirstRepeat.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingFirstRepeat.Location = new System.Drawing.Point(217, 335);
			this.ctrTimingFirstRepeat.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingFirstRepeat.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingFirstRepeat.Name = "ctrTimingFirstRepeat";
			this.ctrTimingFirstRepeat.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingFirstRepeat.TabIndex = 13;
			this.ctrTimingFirstRepeat.Tag = "16";
			// 
			// ctrTimingSubsequentRepeat
			// 
			this.ctrTimingSubsequentRepeat.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingSubsequentRepeat.Location = new System.Drawing.Point(217, 368);
			this.ctrTimingSubsequentRepeat.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingSubsequentRepeat.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingSubsequentRepeat.Name = "ctrTimingSubsequentRepeat";
			this.ctrTimingSubsequentRepeat.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingSubsequentRepeat.TabIndex = 14;
			this.ctrTimingSubsequentRepeat.Tag = "32";
			// 
			// lblPostActivation
			// 
			this.lblPostActivation.AutoSize = true;
			this.lblPostActivation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPostActivation.Location = new System.Drawing.Point(3, 236);
			this.lblPostActivation.Name = "lblPostActivation";
			this.lblPostActivation.Size = new System.Drawing.Size(211, 33);
			this.lblPostActivation.TabIndex = 15;
			this.lblPostActivation.Tag = "2";
			this.lblPostActivation.Text = "label3";
			this.lblPostActivation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblLongPress
			// 
			this.lblLongPress.AutoSize = true;
			this.lblLongPress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblLongPress.Location = new System.Drawing.Point(3, 269);
			this.lblLongPress.Name = "lblLongPress";
			this.lblLongPress.Size = new System.Drawing.Size(211, 33);
			this.lblLongPress.TabIndex = 16;
			this.lblLongPress.Tag = "4";
			this.lblLongPress.Text = "label4";
			this.lblLongPress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblScanTime
			// 
			this.lblScanTime.AutoSize = true;
			this.lblScanTime.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblScanTime.Location = new System.Drawing.Point(3, 302);
			this.lblScanTime.Name = "lblScanTime";
			this.lblScanTime.Size = new System.Drawing.Size(211, 33);
			this.lblScanTime.TabIndex = 17;
			this.lblScanTime.Tag = "8";
			this.lblScanTime.Text = "label5";
			this.lblScanTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblFirstRepeat
			// 
			this.lblFirstRepeat.AutoSize = true;
			this.lblFirstRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblFirstRepeat.Location = new System.Drawing.Point(3, 335);
			this.lblFirstRepeat.Name = "lblFirstRepeat";
			this.lblFirstRepeat.Size = new System.Drawing.Size(211, 33);
			this.lblFirstRepeat.TabIndex = 18;
			this.lblFirstRepeat.Tag = "16";
			this.lblFirstRepeat.Text = "label6";
			this.lblFirstRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblSubsequentRepeat
			// 
			this.lblSubsequentRepeat.AutoSize = true;
			this.lblSubsequentRepeat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSubsequentRepeat.Location = new System.Drawing.Point(3, 368);
			this.lblSubsequentRepeat.Name = "lblSubsequentRepeat";
			this.lblSubsequentRepeat.Size = new System.Drawing.Size(211, 33);
			this.lblSubsequentRepeat.TabIndex = 19;
			this.lblSubsequentRepeat.Tag = "32";
			this.lblSubsequentRepeat.Text = "label7";
			this.lblSubsequentRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDwellSelect
			// 
			this.lblDwellSelect.AutoSize = true;
			this.lblDwellSelect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDwellSelect.Location = new System.Drawing.Point(3, 401);
			this.lblDwellSelect.Name = "lblDwellSelect";
			this.lblDwellSelect.Size = new System.Drawing.Size(211, 33);
			this.lblDwellSelect.TabIndex = 20;
			this.lblDwellSelect.Tag = "64";
			this.lblDwellSelect.Text = "label8";
			this.lblDwellSelect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblCriticalReverse
			// 
			this.lblCriticalReverse.AutoSize = true;
			this.lblCriticalReverse.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCriticalReverse.Location = new System.Drawing.Point(3, 434);
			this.lblCriticalReverse.Name = "lblCriticalReverse";
			this.lblCriticalReverse.Size = new System.Drawing.Size(211, 33);
			this.lblCriticalReverse.TabIndex = 21;
			this.lblCriticalReverse.Tag = "128";
			this.lblCriticalReverse.Text = "label9";
			this.lblCriticalReverse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ctrTimingDwellTime
			// 
			this.ctrTimingDwellTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingDwellTime.Location = new System.Drawing.Point(217, 401);
			this.ctrTimingDwellTime.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingDwellTime.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingDwellTime.Name = "ctrTimingDwellTime";
			this.ctrTimingDwellTime.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingDwellTime.TabIndex = 22;
			this.ctrTimingDwellTime.Tag = "64";
			// 
			// ctrTimingCriticalReverse
			// 
			this.ctrTimingCriticalReverse.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrTimingCriticalReverse.Location = new System.Drawing.Point(217, 434);
			this.ctrTimingCriticalReverse.Margin = new System.Windows.Forms.Padding(0);
			this.ctrTimingCriticalReverse.MinimumSize = new System.Drawing.Size(100, 33);
			this.ctrTimingCriticalReverse.Name = "ctrTimingCriticalReverse";
			this.ctrTimingCriticalReverse.Size = new System.Drawing.Size(404, 33);
			this.ctrTimingCriticalReverse.TabIndex = 23;
			this.ctrTimingCriticalReverse.Tag = "128";
			// 
			// cfgSwitching
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "cfgSwitching";
			this.Size = new System.Drawing.Size(621, 542);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rdoMouse;
		private System.Windows.Forms.RadioButton rdoOne;
		private System.Windows.Forms.RadioButton rdoTwo;
		private System.Windows.Forms.Label lblScanType;
		private System.Windows.Forms.ComboBox cmbScanType;
		private System.Windows.Forms.Label lblSwitch1;
		private Switches.ctrEditTiming ctrTimingAcceptance;
		private Switches.ctrSelectSwitch ctrSwitch1;
		private Switches.ctrSelectSwitch ctrSwitch2;
		private System.Windows.Forms.Label lblAcceptance;
		private Switches.ctrEditTiming ctrTimingPostActivation;
		private Switches.ctrEditTiming ctrTimingLongPress;
		private Switches.ctrEditTiming ctrTimingScanTime;
		private Switches.ctrEditTiming ctrTimingFirstRepeat;
		private Switches.ctrEditTiming ctrTimingSubsequentRepeat;
		private System.Windows.Forms.Label lblPostActivation;
		private System.Windows.Forms.Label lblLongPress;
		private System.Windows.Forms.Label lblScanTime;
		private System.Windows.Forms.Label lblFirstRepeat;
		private System.Windows.Forms.Label lblSubsequentRepeat;
		private System.Windows.Forms.Label lblDwellSelect;
		private System.Windows.Forms.Label lblCriticalReverse;
		private Switches.ctrEditTiming ctrTimingDwellTime;
		private Switches.ctrEditTiming ctrTimingCriticalReverse;
	}
}
