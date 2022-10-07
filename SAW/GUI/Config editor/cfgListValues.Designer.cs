using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	internal partial class cfgListValues : ConfigPage
	{
		
		//UserControl overrides dispose to clean up the component list.
		[DebuggerNonUserCode()]protected override void Dispose(bool disposing)
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
		[DebuggerStepThrough()]private void InitializeComponent()
		{
			System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel6;
			this.lblTitle = new System.Windows.Forms.Label();
			this.btnListRemove = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtAddKey = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtAddValue = new System.Windows.Forms.TextBox();
			this.btnSet = new System.Windows.Forms.Button();
			this.pnlValues = new System.Windows.Forms.Panel();
			this.lstValues = new System.Windows.Forms.ListView();
			this.colKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			FlowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
			FlowLayoutPanel6.SuspendLayout();
			this.pnlValues.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.BackColor = System.Drawing.Color.PowderBlue;
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(0, 0);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(571, 20);
			this.lblTitle.TabIndex = 3;
			this.lblTitle.Text = "Values";
			// 
			// FlowLayoutPanel6
			// 
			FlowLayoutPanel6.Controls.Add(this.btnListRemove);
			FlowLayoutPanel6.Controls.Add(this.label1);
			FlowLayoutPanel6.Controls.Add(this.txtAddKey);
			FlowLayoutPanel6.Controls.Add(this.label2);
			FlowLayoutPanel6.Controls.Add(this.txtAddValue);
			FlowLayoutPanel6.Controls.Add(this.btnSet);
			FlowLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Bottom;
			FlowLayoutPanel6.Location = new System.Drawing.Point(0, 524);
			FlowLayoutPanel6.Name = "FlowLayoutPanel6";
			FlowLayoutPanel6.Size = new System.Drawing.Size(571, 32);
			FlowLayoutPanel6.TabIndex = 5;
			// 
			// btnListRemove
			// 
			this.btnListRemove.Location = new System.Drawing.Point(3, 3);
			this.btnListRemove.Name = "btnListRemove";
			this.btnListRemove.Size = new System.Drawing.Size(93, 29);
			this.btnListRemove.TabIndex = 0;
			this.btnListRemove.Text = "Remove";
			this.btnListRemove.UseVisualStyleBackColor = true;
			this.btnListRemove.Click += new System.EventHandler(this.btnListRemove_Click);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(102, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Key to add:";
			// 
			// txtAddKey
			// 
			this.txtAddKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtAddKey.Location = new System.Drawing.Point(169, 7);
			this.txtAddKey.Name = "txtAddKey";
			this.txtAddKey.Size = new System.Drawing.Size(100, 20);
			this.txtAddKey.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(280, 11);
			this.label2.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(70, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Value to add:";
			// 
			// txtAddValue
			// 
			this.txtAddValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtAddValue.Location = new System.Drawing.Point(356, 7);
			this.txtAddValue.Name = "txtAddValue";
			this.txtAddValue.Size = new System.Drawing.Size(100, 20);
			this.txtAddValue.TabIndex = 4;
			// 
			// btnSet
			// 
			this.btnSet.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnSet.Location = new System.Drawing.Point(462, 6);
			this.btnSet.Name = "btnSet";
			this.btnSet.Size = new System.Drawing.Size(63, 23);
			this.btnSet.TabIndex = 5;
			this.btnSet.Text = "Set";
			this.btnSet.UseVisualStyleBackColor = true;
			this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
			// 
			// pnlValues
			// 
			this.pnlValues.Controls.Add(this.lstValues);
			this.pnlValues.Controls.Add(this.lblTitle);
			this.pnlValues.Controls.Add(FlowLayoutPanel6);
			this.pnlValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlValues.Location = new System.Drawing.Point(0, 0);
			this.pnlValues.Name = "pnlValues";
			this.pnlValues.Size = new System.Drawing.Size(571, 556);
			this.pnlValues.TabIndex = 64;
			// 
			// lstValues
			// 
			this.lstValues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colKey,
            this.colValue,
            this.colDescription});
			this.lstValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstValues.FullRowSelect = true;
			this.lstValues.HideSelection = false;
			this.lstValues.Location = new System.Drawing.Point(0, 20);
			this.lstValues.MultiSelect = false;
			this.lstValues.Name = "lstValues";
			this.lstValues.ShowGroups = false;
			this.lstValues.Size = new System.Drawing.Size(571, 504);
			this.lstValues.TabIndex = 4;
			this.lstValues.UseCompatibleStateImageBehavior = false;
			this.lstValues.View = System.Windows.Forms.View.Details;
			// 
			// colKey
			// 
			this.colKey.Text = "Key";
			this.colKey.Width = 160;
			// 
			// colValue
			// 
			this.colValue.Text = "Value";
			this.colValue.Width = 160;
			// 
			// colDescription
			// 
			this.colDescription.Text = "Description";
			this.colDescription.Width = 260;
			// 
			// cfgListValues
			// 
			this.Controls.Add(this.pnlValues);
			this.Name = "cfgListValues";
			this.Size = new System.Drawing.Size(571, 556);
			FlowLayoutPanel6.ResumeLayout(false);
			FlowLayoutPanel6.PerformLayout();
			this.pnlValues.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private Panel pnlValues;
		private ListView lstValues;
		private ColumnHeader colKey;
		private ColumnHeader colValue;
		private ColumnHeader colDescription;
		private Button btnListRemove;
		private Label label1;
		private TextBox txtAddKey;
		private Label label2;
		private TextBox txtAddValue;
		private Button btnSet;
		private Label lblTitle;
	}
	
}
