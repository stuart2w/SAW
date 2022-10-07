
namespace SAW
{
	internal partial class cfgSpeech : ConfigPage
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
			System.Windows.Forms.Label Label44;
			System.Windows.Forms.Label Label45;
			System.Windows.Forms.Label Label46;
			System.Windows.Forms.Label Label47;
			this.pnlSpeech = new System.Windows.Forms.TableLayoutPanel();
			this.cmbSpeechVoice = new System.Windows.Forms.ComboBox();
			this.ctrSpeed = new System.Windows.Forms.TrackBar();
			this.ctrVolume = new System.Windows.Forms.TrackBar();
			this.btnSpeechTest = new System.Windows.Forms.Button();
			Label44 = new System.Windows.Forms.Label();
			Label45 = new System.Windows.Forms.Label();
			Label46 = new System.Windows.Forms.Label();
			Label47 = new System.Windows.Forms.Label();
			this.pnlSpeech.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ctrSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ctrVolume)).BeginInit();
			this.SuspendLayout();
			// 
			// Label44
			// 
			Label44.AutoSize = true;
			Label44.BackColor = System.Drawing.Color.PowderBlue;
			this.pnlSpeech.SetColumnSpan(Label44, 2);
			Label44.Dock = System.Windows.Forms.DockStyle.Top;
			Label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			Label44.Location = new System.Drawing.Point(3, 0);
			Label44.Name = "Label44";
			Label44.Size = new System.Drawing.Size(562, 20);
			Label44.TabIndex = 1;
			Label44.Text = "[Config_SpeechTitle]";
			// 
			// Label45
			// 
			Label45.AutoSize = true;
			Label45.Dock = System.Windows.Forms.DockStyle.Fill;
			Label45.Location = new System.Drawing.Point(3, 20);
			Label45.Name = "Label45";
			Label45.Size = new System.Drawing.Size(221, 27);
			Label45.TabIndex = 3;
			Label45.Text = "[Config_SpeechVoice]";
			Label45.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label46
			// 
			Label46.AutoSize = true;
			Label46.Dock = System.Windows.Forms.DockStyle.Fill;
			Label46.Location = new System.Drawing.Point(3, 47);
			Label46.Name = "Label46";
			Label46.Size = new System.Drawing.Size(221, 40);
			Label46.TabIndex = 4;
			Label46.Text = "[Config_SpeechVolume]";
			Label46.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label47
			// 
			Label47.AutoSize = true;
			Label47.Dock = System.Windows.Forms.DockStyle.Fill;
			Label47.Location = new System.Drawing.Point(3, 87);
			Label47.Name = "Label47";
			Label47.Size = new System.Drawing.Size(221, 40);
			Label47.TabIndex = 7;
			Label47.Text = "[Config_SpeechSpeed]";
			Label47.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// pnlSpeech
			// 
			this.pnlSpeech.ColumnCount = 2;
			this.pnlSpeech.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.pnlSpeech.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.pnlSpeech.Controls.Add(Label44, 0, 0);
			this.pnlSpeech.Controls.Add(Label45, 0, 2);
			this.pnlSpeech.Controls.Add(Label46, 0, 3);
			this.pnlSpeech.Controls.Add(this.cmbSpeechVoice, 1, 2);
			this.pnlSpeech.Controls.Add(Label47, 0, 4);
			this.pnlSpeech.Controls.Add(this.ctrSpeed, 1, 4);
			this.pnlSpeech.Controls.Add(this.ctrVolume, 1, 3);
			this.pnlSpeech.Controls.Add(this.btnSpeechTest, 1, 6);
			this.pnlSpeech.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlSpeech.Location = new System.Drawing.Point(0, 0);
			this.pnlSpeech.Name = "pnlSpeech";
			this.pnlSpeech.RowCount = 13;
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlSpeech.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.pnlSpeech.Size = new System.Drawing.Size(568, 499);
			this.pnlSpeech.TabIndex = 63;
			// 
			// cmbSpeechVoice
			// 
			this.cmbSpeechVoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSpeechVoice.FormattingEnabled = true;
			this.cmbSpeechVoice.Location = new System.Drawing.Point(230, 23);
			this.cmbSpeechVoice.Name = "cmbSpeechVoice";
			this.cmbSpeechVoice.Size = new System.Drawing.Size(335, 21);
			this.cmbSpeechVoice.TabIndex = 6;
			this.cmbSpeechVoice.SelectedIndexChanged += new System.EventHandler(this.cmbSpeechVoice_SelectedIndexChanged);
			// 
			// ctrSpeed
			// 
			this.ctrSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrSpeed.Location = new System.Drawing.Point(230, 90);
			this.ctrSpeed.Maximum = 100;
			this.ctrSpeed.Name = "ctrSpeed";
			this.ctrSpeed.Size = new System.Drawing.Size(335, 34);
			this.ctrSpeed.SmallChange = 5;
			this.ctrSpeed.TabIndex = 9;
			this.ctrSpeed.TickFrequency = 10;
			this.ctrSpeed.Scroll += new System.EventHandler(this.ctrSpeed_Scroll);
			// 
			// ctrVolume
			// 
			this.ctrVolume.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctrVolume.Location = new System.Drawing.Point(230, 50);
			this.ctrVolume.Maximum = 100;
			this.ctrVolume.Name = "ctrVolume";
			this.ctrVolume.Size = new System.Drawing.Size(335, 34);
			this.ctrVolume.SmallChange = 5;
			this.ctrVolume.TabIndex = 11;
			this.ctrVolume.TickFrequency = 10;
			this.ctrVolume.Scroll += new System.EventHandler(this.ctrVolume_Scroll);
			// 
			// btnSpeechTest
			// 
			this.btnSpeechTest.AutoSize = true;
			this.btnSpeechTest.Location = new System.Drawing.Point(230, 135);
			this.btnSpeechTest.Name = "btnSpeechTest";
			this.btnSpeechTest.Size = new System.Drawing.Size(150, 27);
			this.btnSpeechTest.TabIndex = 12;
			this.btnSpeechTest.Text = "[Config_SpeechTest]";
			this.btnSpeechTest.UseVisualStyleBackColor = true;
			this.btnSpeechTest.Click += new System.EventHandler(this.btnSpeechTest_Click);
			// 
			// cfgSpeech
			// 
			this.Controls.Add(this.pnlSpeech);
			this.Name = "cfgSpeech";
			this.Size = new System.Drawing.Size(568, 499);
			this.pnlSpeech.ResumeLayout(false);
			this.pnlSpeech.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ctrSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ctrVolume)).EndInit();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.TableLayoutPanel pnlSpeech;
		private System.Windows.Forms.ComboBox cmbSpeechVoice;
		private System.Windows.Forms.TrackBar ctrSpeed;
		private System.Windows.Forms.TrackBar ctrVolume;
		private System.Windows.Forms.Button btnSpeechTest;
		
	}
	
}
