
namespace SAW
{
	public partial class frmEditConfig : System.Windows.Forms.Form
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
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.Panel pnlBottom;
			this.txtSearch = new SAW.TextBoxAllKeys();
			this.Label56 = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.ttMain = new System.Windows.Forms.ToolTip(this.components);
			this.pnlSections = new SAW.SelectionPanel();
			this.tmrSearch = new System.Windows.Forms.Timer(this.components);
			this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
			pnlBottom = new System.Windows.Forms.Panel();
			pnlBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlBottom
			// 
			pnlBottom.Controls.Add(this.txtSearch);
			pnlBottom.Controls.Add(this.Label56);
			pnlBottom.Controls.Add(this.btnOK);
			pnlBottom.Controls.Add(this.btnCancel);
			pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			pnlBottom.Location = new System.Drawing.Point(0, 488);
			pnlBottom.Name = "pnlBottom";
			pnlBottom.Padding = new System.Windows.Forms.Padding(4, 6, 8, 4);
			pnlBottom.Size = new System.Drawing.Size(819, 41);
			pnlBottom.TabIndex = 48;
			// 
			// txtSearch
			// 
			this.txtSearch.AcceptsReturn = true;
			this.txtSearch.Dock = System.Windows.Forms.DockStyle.Left;
			this.txtSearch.Location = new System.Drawing.Point(65, 6);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(183, 23);
			this.txtSearch.TabIndex = 48;
			this.ttMain.SetToolTip(this.txtSearch, "[Config_Search_Tooltip]");
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyDown);
			this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
			this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
			// 
			// Label56
			// 
			this.Label56.AutoSize = true;
			this.Label56.Dock = System.Windows.Forms.DockStyle.Left;
			this.Label56.Location = new System.Drawing.Point(4, 6);
			this.Label56.Name = "Label56";
			this.Label56.Size = new System.Drawing.Size(61, 17);
			this.Label56.TabIndex = 47;
			this.Label56.Text = "[Search]";
			// 
			// btnOK
			// 
			this.btnOK.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnOK.Location = new System.Drawing.Point(515, 6);
			this.btnOK.Margin = new System.Windows.Forms.Padding(5);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(144, 31);
			this.btnOK.TabIndex = 45;
			this.btnOK.Text = "[OK]";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnCancel.Location = new System.Drawing.Point(659, 6);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(152, 31);
			this.btnCancel.TabIndex = 46;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// pnlSections
			// 
			this.pnlSections.AutoScroll = true;
			this.pnlSections.BackColor = System.Drawing.SystemColors.Window;
			this.pnlSections.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlSections.Location = new System.Drawing.Point(0, 0);
			this.pnlSections.MinimumItemHeight = 12;
			this.pnlSections.Name = "pnlSections";
			this.pnlSections.Padding = new System.Windows.Forms.Padding(3);
			this.pnlSections.SelectedColour = System.Drawing.SystemColors.Highlight;
			this.pnlSections.SelectedTextColour = System.Drawing.Color.White;
			this.pnlSections.Size = new System.Drawing.Size(168, 488);
			this.pnlSections.TabIndex = 64;
			this.pnlSections.SelectedIndexChanged += new System.EventHandler(this.pnlSections_SelectedIndexChanged);
			// 
			// tmrSearch
			// 
			this.tmrSearch.Interval = 600;
			this.tmrSearch.Tick += new System.EventHandler(this.tmrSearch_Tick);
			// 
			// frmEditConfig
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(819, 529);
			this.Controls.Add(this.pnlSections);
			this.Controls.Add(pnlBottom);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 400);
			this.Name = "frmEditConfig";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "[Config_Title]";
			pnlBottom.ResumeLayout(false);
			pnlBottom.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		internal System.Windows.Forms.ToolTip ttMain;
		private System.Windows.Forms.Timer tmrSearch;
		private System.Windows.Forms.FolderBrowserDialog dlgFolder;
		private System.Windows.Forms.Label Label56;
		private TextBoxAllKeys txtSearch;
		private SAW.SelectionPanel pnlSections;
		private System.ComponentModel.IContainer components;
	}
	
}
