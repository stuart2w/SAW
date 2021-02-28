using System;
using System.Diagnostics;


namespace SAW
{
	public partial class cfgSpeech
	{
		public cfgSpeech()
		{
			InitializeComponent();
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			// this is only displayed when the panel is actually selected
			Debug.Assert(!m_Filling);
			m_Filling = true;
			try
			{
				// we only fill the list of voices when needed
				ctrVolume.Value = m_Config.ReadInteger(Config.Speech_Volume, 100);
				ctrSpeed.Value = m_Config.ReadInteger(Config.Speech_Speed, 70);
				if (cmbSpeechVoice.Items.Count == 0)
				{
					foreach (string voice in Globals.Root.Speech.Voices)
					{
						cmbSpeechVoice.Items.Add(voice);
					}
				}
				try
				{
					cmbSpeechVoice.Text = m_Config.ReadString(Config.Speech_Voice);
				}
				catch
				{
				}
			}
			finally
			{
				m_Filling = false;
			}
		}
		
		public void cmbSpeechVoice_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Speech_Voice, cmbSpeechVoice.Text);
			Globals.Root.Speech.SettingsChanged(m_Applied);
			WrittenToCurrent();
		}

		public void ctrVolume_Scroll(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Speech_Volume, ctrVolume.Value);
			WrittenToCurrent();
		}

		public void ctrSpeed_Scroll(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Config.Write(Config.Speech_Speed, ctrSpeed.Value);
			WrittenToCurrent();
		}

		public void btnSpeechTest_Click(object sender, EventArgs e)
		{
			Globals.Root.Speech.SettingsChanged(m_Applied); // in order to apply any changes to volume, speed, pitch
			Globals.Root.Speech.Speak(Strings.Item("Config_SpeechTestText"));
			WrittenToCurrent();
		}

	}

}
