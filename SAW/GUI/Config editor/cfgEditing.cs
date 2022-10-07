using System;

namespace SAW
{
	internal partial class cfgEditing : ConfigPage
	{

		public cfgEditing()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			chkToolbar.Checked = m_Applied.ReadBoolean(Config.Show_Toolbar, true);
			chkTools.Checked = m_Applied.ReadBoolean(Config.Show_Tools, true);
			chkToolbox.Checked = m_Applied.ReadBoolean(Config.ShowPaletteKey(Parameters.Tool), true);
			chkOutline.Checked = m_Applied.ReadBoolean(Config.ShowPaletteKey(Palette.Item("DocumentOutline")), true);
			chkPages.Checked = m_Applied.ReadBoolean(Config.Show_PageList, true);
			chkInfoBar.Checked = m_Applied.ReadBoolean(Config.Show_InfoMeasurement, true);
			chkShowHidden.Checked = m_Applied.ReadBoolean(Config.Show_Hidden_When_Editing, true);
			chkMultipleDocuments.Checked = m_Applied.ReadBoolean(Config.Multiple_Documents, false);
			chkResizeDocToWindow.Checked = m_Applied.ReadBoolean(Config.Resize_Document_ToWindow, true);
#if DEBUG
			chkMultipleDocuments.Visible = Level == Config.Levels.User; // currently off unless debug
#endif
			m_Filling = false;
		}

		#region Control events
		private void chkToolbox_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.ShowPaletteKey(Parameters.Tool), chkToolbox.Checked);
			//m_Config.Write(Config.ShowPaletteKey(Palette.Item(Config.ToolboxPaletteID)), chkToolbox.Checked);
			WrittenToCurrent();
		}

		private void chkOutline_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.ShowPaletteKey(Palette.Item("DocumentOutline")), chkOutline.Checked);
			WrittenToCurrent();
		}

		private void chkTools_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Show_Tools, chkTools.Checked);
			WrittenToCurrent();
		}

		private void chkToolbar_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Show_Toolbar, chkToolbar.Checked);
			WrittenToCurrent();
		}

		private void chkPages_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Show_PageList, chkPages.Checked);
			WrittenToCurrent();
		}

		private void chkInfoBar_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Show_InfoMeasurement, chkInfoBar.Checked);
			WrittenToCurrent();
		}

		private void chkShowHidden_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Show_Hidden_When_Editing, chkShowHidden.Checked);
			WrittenToCurrent();
		}

		private void chkMultipleDocuments_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Multiple_Documents, chkMultipleDocuments.Checked);
			WrittenToCurrent();
		}

		private void chkResizeDocToWindow_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Resize_Document_ToWindow, chkResizeDocToWindow.Checked);
			WrittenToCurrent();
		}

		#endregion

	}
}
