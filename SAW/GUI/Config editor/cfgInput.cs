using System;

namespace SAW
{
	public partial class cfgInput : ConfigPage
	{
		public cfgInput()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			chkReverse.Checked = m_Applied.ReadBoolean(Config.Reverse_Switches);
			chkUseSwap.Checked = m_Applied.ReadBoolean(Config.Use_Swap_Switch);
			chkShowHelp.Checked = m_Applied.ReadBoolean(Config.SAW_Prompts, true);
			chkSoundClick.Checked = m_Applied.ReadBoolean(Config.Sound_Click, true);
			chkRepeatPressStop.Checked = m_Applied.ReadBoolean(Config.Repeat_PressStop);
			m_Filling = false;
		}

		private void chkReverse_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Reverse_Switches, chkReverse.Checked);
			WrittenToCurrent();
		}

		private void chkUseSwap_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Use_Swap_Switch, chkUseSwap.Checked);
			WrittenToCurrent();
		}

		private void chkShowHelp_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.SAW_Prompts, chkShowHelp.Checked);
			WrittenToCurrent();
		}

		private void chkSoundClick_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Sound_Click, chkSoundClick.Checked);
			WrittenToCurrent();
		}

		private void chkRepeatPressStop_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Repeat_PressStop, chkRepeatPressStop.Checked);
			WrittenToCurrent();
		}
	}
}
