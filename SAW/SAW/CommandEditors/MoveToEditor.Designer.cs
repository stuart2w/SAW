namespace SAW.CommandEditors
{
	partial class MoveToEditor
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
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtX = new IntegerTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtY = new IntegerTextBox();
			this.btnTest = new System.Windows.Forms.Button();
			this.btnSelect = new System.Windows.Forms.Button();
			this.lblMessage = new System.Windows.Forms.Label();
			this.flowLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.txtX);
			this.flowLayoutPanel1.Controls.Add(this.label2);
			this.flowLayoutPanel1.Controls.Add(this.txtY);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(272, 26);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.Controls.Add(this.btnTest);
			this.flowLayoutPanel2.Controls.Add(this.btnSelect);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 26);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(272, 29);
			this.flowLayoutPanel2.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "X:";
			// 
			// txtX
			// 
			this.txtX.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtX.Location = new System.Drawing.Point(26, 3);
			this.txtX.Name = "txtX";
			this.txtX.Size = new System.Drawing.Size(52, 20);
			this.txtX.TabIndex = 1;
			this.txtX.TextChanged += new System.EventHandler(this.txtX_TextChanged);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(84, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Y:";
			// 
			// txtY
			// 
			this.txtY.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtY.Location = new System.Drawing.Point(107, 3);
			this.txtY.Name = "txtY";
			this.txtY.Size = new System.Drawing.Size(52, 20);
			this.txtY.TabIndex = 3;
			this.txtY.TextChanged += new System.EventHandler(this.txtY_TextChanged);
			// 
			// btnTest
			// 
			this.btnTest.Location = new System.Drawing.Point(3, 3);
			this.btnTest.Name = "btnTest";
			this.btnTest.Size = new System.Drawing.Size(75, 23);
			this.btnTest.TabIndex = 0;
			this.btnTest.Text = "[SAW_CommandEdit_Test]";
			this.btnTest.UseVisualStyleBackColor = true;
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// btnSelect
			// 
			this.btnSelect.Location = new System.Drawing.Point(84, 3);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new System.Drawing.Size(75, 23);
			this.btnSelect.TabIndex = 1;
			this.btnSelect.Text = "[SAW_CommandEdit_SelectPosition]";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
			// 
			// lblMessage
			// 
			this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblMessage.Location = new System.Drawing.Point(0, 55);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(272, 184);
			this.lblMessage.TabIndex = 2;
			this.lblMessage.Text = "[SAW_CommandEdit_MoveToPrompt]";
			this.lblMessage.Visible = false;
			// 
			// MoveToEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.flowLayoutPanel2);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Name = "MoveToEditor";
			this.Size = new System.Drawing.Size(272, 239);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveToEditor_MouseDown);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private IntegerTextBox txtX;
		private System.Windows.Forms.Label label2;
		private IntegerTextBox txtY;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Button btnTest;
		private System.Windows.Forms.Button btnSelect;
		private System.Windows.Forms.Label lblMessage;
	}
}
