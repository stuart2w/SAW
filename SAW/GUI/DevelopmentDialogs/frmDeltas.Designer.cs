// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Linq;
// End of VB project level imports


namespace SAW
{
	public partial class frmDeltas : System.Windows.Forms.Form
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
			System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
			System.Windows.Forms.ColumnHeader ColumnHeader1;
			System.Windows.Forms.ColumnHeader ColumnHeader2;
			System.Windows.Forms.ColumnHeader ColumnHeader3;
			System.Windows.Forms.ColumnHeader ColumnHeader4;
			System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel1;
			this.lstDeltas = new System.Windows.Forms.ListBox();
			this.lstValues = new System.Windows.Forms.ListView();
			this.chkDisplayUnchanged = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.lblID = new System.Windows.Forms.Label();
			this.dlgSave = new System.Windows.Forms.SaveFileDialog();
			TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			FlowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			TableLayoutPanel1.SuspendLayout();
			FlowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// TableLayoutPanel1
			// 
			TableLayoutPanel1.ColumnCount = 1;
			TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			TableLayoutPanel1.Controls.Add(this.lstDeltas, 0, 0);
			TableLayoutPanel1.Controls.Add(this.lstValues, 0, 2);
			TableLayoutPanel1.Controls.Add(this.chkDisplayUnchanged, 0, 3);
			TableLayoutPanel1.Controls.Add(FlowLayoutPanel1, 0, 4);
			TableLayoutPanel1.Controls.Add(this.lblID, 0, 1);
			TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			TableLayoutPanel1.Name = "TableLayoutPanel1";
			TableLayoutPanel1.RowCount = 6;
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			TableLayoutPanel1.Size = new System.Drawing.Size(600, 534);
			TableLayoutPanel1.TabIndex = 1;
			// 
			// lstDeltas
			// 
			this.lstDeltas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstDeltas.FormattingEnabled = true;
			this.lstDeltas.Location = new System.Drawing.Point(3, 3);
			this.lstDeltas.Name = "lstDeltas";
			this.lstDeltas.Size = new System.Drawing.Size(594, 222);
			this.lstDeltas.TabIndex = 1;
			this.lstDeltas.SelectedIndexChanged += new System.EventHandler(this.lstDeltas_SelectedIndexChanged);
			// 
			// lstValues
			// 
			this.lstValues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            ColumnHeader1,
            ColumnHeader2,
            ColumnHeader3,
            ColumnHeader4});
			this.lstValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstValues.FullRowSelect = true;
			this.lstValues.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lstValues.HideSelection = false;
			this.lstValues.Location = new System.Drawing.Point(3, 251);
			this.lstValues.MultiSelect = false;
			this.lstValues.Name = "lstValues";
			this.lstValues.Size = new System.Drawing.Size(594, 222);
			this.lstValues.TabIndex = 2;
			this.lstValues.UseCompatibleStateImageBehavior = false;
			this.lstValues.View = System.Windows.Forms.View.Details;
			this.lstValues.SelectedIndexChanged += new System.EventHandler(this.lstValues_SelectedIndexChanged);
			// 
			// ColumnHeader1
			// 
			ColumnHeader1.Text = "Key";
			ColumnHeader1.Width = 150;
			// 
			// ColumnHeader2
			// 
			ColumnHeader2.Text = "Old value";
			ColumnHeader2.Width = 100;
			// 
			// ColumnHeader3
			// 
			ColumnHeader3.Text = "New value";
			ColumnHeader3.Width = 100;
			// 
			// ColumnHeader4
			// 
			ColumnHeader4.Text = "Descriptor";
			// 
			// chkDisplayUnchanged
			// 
			this.chkDisplayUnchanged.AutoSize = true;
			this.chkDisplayUnchanged.Location = new System.Drawing.Point(3, 479);
			this.chkDisplayUnchanged.Name = "chkDisplayUnchanged";
			this.chkDisplayUnchanged.Size = new System.Drawing.Size(290, 17);
			this.chkDisplayUnchanged.TabIndex = 3;
			this.chkDisplayUnchanged.Text = "List all values (including those which have not changed)";
			this.chkDisplayUnchanged.UseVisualStyleBackColor = true;
			this.chkDisplayUnchanged.CheckedChanged += new System.EventHandler(this.lstDeltas_SelectedIndexChanged);
			// 
			// FlowLayoutPanel1
			// 
			FlowLayoutPanel1.AutoSize = true;
			FlowLayoutPanel1.Controls.Add(this.btnCancel);
			FlowLayoutPanel1.Controls.Add(this.btnDelete);
			FlowLayoutPanel1.Controls.Add(this.btnSave);
			FlowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			FlowLayoutPanel1.Location = new System.Drawing.Point(0, 499);
			FlowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			FlowLayoutPanel1.Name = "FlowLayoutPanel1";
			FlowLayoutPanel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			FlowLayoutPanel1.Size = new System.Drawing.Size(600, 35);
			FlowLayoutPanel1.TabIndex = 4;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(503, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(94, 29);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnDelete
			// 
			this.btnDelete.Enabled = false;
			this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnDelete.Location = new System.Drawing.Point(359, 3);
			this.btnDelete.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(121, 29);
			this.btnDelete.TabIndex = 1;
			this.btnDelete.Text = "Delete value";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// btnSave
			// 
			this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSave.Location = new System.Drawing.Point(216, 3);
			this.btnSave.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(128, 29);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "Save delta file";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// lblID
			// 
			this.lblID.AutoSize = true;
			this.lblID.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblID.Location = new System.Drawing.Point(3, 228);
			this.lblID.Name = "lblID";
			this.lblID.Size = new System.Drawing.Size(594, 20);
			this.lblID.TabIndex = 6;
			// 
			// dlgSave
			// 
			this.dlgSave.DefaultExt = "delta";
			this.dlgSave.Filter = "*.delta|*.delta";
			this.dlgSave.RestoreDirectory = true;
			// 
			// frmDeltas
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(600, 534);
			this.Controls.Add(TableLayoutPanel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 400);
			this.Name = "frmDeltas";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Configuration deltas";
			TableLayoutPanel1.ResumeLayout(false);
			TableLayoutPanel1.PerformLayout();
			FlowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ListBox lstDeltas;
		private System.Windows.Forms.ListView lstValues;
		private System.Windows.Forms.CheckBox chkDisplayUnchanged;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.SaveFileDialog dlgSave;
		private System.Windows.Forms.Label lblID;
	}
	
}
