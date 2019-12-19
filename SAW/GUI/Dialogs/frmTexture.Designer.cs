
namespace SAW
{
	public partial class frmTexture : System.Windows.Forms.Form
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
			this.components = new System.ComponentModel.Container();
			this.lstSystem = new System.Windows.Forms.ListView();
			this.lstSystem.Click += new System.EventHandler(this.lstTextures_Click);
			this.lstSystem.GotFocus += new System.EventHandler(this.lstSystem_GotFocus);
			this.lstSystem.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lstSystem_KeyPress);
			this.lstSystem.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lstDocument_DrawItem);
			this.btnCancel = new System.Windows.Forms.Button();
			this.Panel1 = new System.Windows.Forms.Panel();
			this.btnLoad = new System.Windows.Forms.Button();
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			this.tcSource = new System.Windows.Forms.TabControl();
			this.tpDocument = new System.Windows.Forms.TabPage();
			this.lstDocument = new System.Windows.Forms.ListView();
			this.lstDocument.Click += new System.EventHandler(this.lstTextures_Click);
			this.lstDocument.GotFocus += new System.EventHandler(this.lstDocument_GotFocus);
			this.lstDocument.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lstDocument_KeyPress);
			this.lstDocument.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lstDocument_DrawItem);
			this.tpSystem = new System.Windows.Forms.TabPage();
			this.imlForSize = new System.Windows.Forms.ImageList(this.components);
			this.Panel1.SuspendLayout();
			this.tcSource.SuspendLayout();
			this.tpDocument.SuspendLayout();
			this.tpSystem.SuspendLayout();
			this.SuspendLayout();
			//
			//lstSystem
			//
			this.lstSystem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstSystem.HideSelection = false;
			this.lstSystem.LargeImageList = this.imlForSize;
			this.lstSystem.Location = new System.Drawing.Point(3, 3);
			this.lstSystem.MultiSelect = false;
			this.lstSystem.Name = "lstSystem";
			this.lstSystem.OwnerDraw = true;
			this.lstSystem.Size = new System.Drawing.Size(618, 465);
			this.lstSystem.TabIndex = 0;
			this.lstSystem.UseCompatibleStateImageBehavior = false;
			//
			//btnCancel
			//
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnCancel.Location = new System.Drawing.Point(520, 4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(108, 28);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "[Cancel]";
			this.btnCancel.UseVisualStyleBackColor = true;
			//
			//Panel1
			//
			this.Panel1.Controls.Add(this.btnLoad);
			this.Panel1.Controls.Add(this.btnCancel);
			this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Panel1.Location = new System.Drawing.Point(0, 497);
			this.Panel1.Name = "Panel1";
			this.Panel1.Padding = new System.Windows.Forms.Padding(4);
			this.Panel1.Size = new System.Drawing.Size(632, 36);
			this.Panel1.TabIndex = 0;
			//
			//btnLoad
			//
			this.btnLoad.Dock = System.Windows.Forms.DockStyle.Left;
			this.btnLoad.Location = new System.Drawing.Point(4, 4);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(220, 28);
			this.btnLoad.TabIndex = 0;
			this.btnLoad.Text = "[Texture_Load]";
			this.btnLoad.UseVisualStyleBackColor = true;
			//
			//tcSource
			//
			this.tcSource.Controls.Add(this.tpDocument);
			this.tcSource.Controls.Add(this.tpSystem);
			this.tcSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcSource.Location = new System.Drawing.Point(0, 0);
			this.tcSource.Name = "tcSource";
			this.tcSource.SelectedIndex = 0;
			this.tcSource.Size = new System.Drawing.Size(632, 497);
			this.tcSource.TabIndex = 2;
			//
			//tpDocument
			//
			this.tpDocument.Controls.Add(this.lstDocument);
			this.tpDocument.Location = new System.Drawing.Point(4, 22);
			this.tpDocument.Name = "tpDocument";
			this.tpDocument.Padding = new System.Windows.Forms.Padding(3);
			this.tpDocument.Size = new System.Drawing.Size(624, 471);
			this.tpDocument.TabIndex = 0;
			this.tpDocument.Text = "[Texture_Document]";
			this.tpDocument.UseVisualStyleBackColor = true;
			//
			//lstDocument
			//
			this.lstDocument.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstDocument.HideSelection = false;
			this.lstDocument.LargeImageList = this.imlForSize;
			this.lstDocument.Location = new System.Drawing.Point(3, 3);
			this.lstDocument.MultiSelect = false;
			this.lstDocument.Name = "lstDocument";
			this.lstDocument.OwnerDraw = true;
			this.lstDocument.Size = new System.Drawing.Size(618, 465);
			this.lstDocument.TabIndex = 1;
			this.lstDocument.UseCompatibleStateImageBehavior = false;
			//
			//tpSystem
			//
			this.tpSystem.Controls.Add(this.lstSystem);
			this.tpSystem.Location = new System.Drawing.Point(4, 22);
			this.tpSystem.Name = "tpSystem";
			this.tpSystem.Padding = new System.Windows.Forms.Padding(3);
			this.tpSystem.Size = new System.Drawing.Size(624, 471);
			this.tpSystem.TabIndex = 1;
			this.tpSystem.Text = "[Texture_System]";
			this.tpSystem.UseVisualStyleBackColor = true;
			//
			//imlForSize
			//
			this.imlForSize.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imlForSize.ImageSize = new System.Drawing.Size(128, 128);
			this.imlForSize.TransparentColor = System.Drawing.Color.Transparent;
			//
			//frmTexture
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (13.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(632, 533);
			this.Controls.Add(this.tcSource);
			this.Controls.Add(this.Panel1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmTexture";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "[Verb_Texture]";
			this.Panel1.ResumeLayout(false);
			this.tcSource.ResumeLayout(false);
			this.tpDocument.ResumeLayout(false);
			this.tpSystem.ResumeLayout(false);
			this.ResumeLayout(false);
			
		}
		private System.Windows.Forms.ListView lstSystem;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel Panel1;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.TabControl tcSource;
		private System.Windows.Forms.TabPage tpDocument;
		private System.Windows.Forms.TabPage tpSystem;
		private System.Windows.Forms.ListView lstDocument;
		private System.Windows.Forms.ImageList imlForSize;
	}
	
}
