using System;

namespace SAW
{
	public partial class cfgOutput : ConfigPage
	{
		public cfgOutput()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			chkSpeech.Checked = m_Applied.ReadBoolean(Config.Output_Speech, true);
			chkSpeechVisit.Checked = m_Applied.ReadBoolean(Config.Output_SpeechVisit, true);
			chkWAV.Checked = m_Applied.ReadBoolean(Config.Output_Sound, true);
			chkApplication.Checked = m_Applied.ReadBoolean(Config.Output_Send, true);
			chkHideTitle.Checked = m_Applied.ReadBoolean(Config.Hide_Title);
			try
			{
				nudHideDelay.Value = (decimal) m_Applied.ReadSingle(Config.HideTime_OnSend);
			}
			catch{} // errors will be out of range

			m_Filling = false;
		}

		private void chkSpeech_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Output_Speech, chkSpeech.Checked);
			WrittenToCurrent();
		}

		private void chkSpeechVisit_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Output_SpeechVisit, chkSpeechVisit.Checked);
			WrittenToCurrent();
		}

		private void chkWAV_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Output_Sound, chkWAV.Checked);
			WrittenToCurrent();
		}

		private void chkApplication_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Output_Send, chkApplication.Checked);
			WrittenToCurrent();
		}

		private void chkHideTitle_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Hide_Title, chkHideTitle.Checked);
			WrittenToCurrent();
		}

		private void nudHideDelay_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.HideTime_OnSend, (float) nudHideDelay.Value);
			WrittenToCurrent();
		}
	}
}
