using System.Collections.Generic;
using System;
using System.Speech.Synthesis;

namespace SAW
{
	/// <summary>
	/// we always have one of these classes, which can be accessed via Globals.Root.Speech
		/// it is done this way rather than having shared members, so that we can control the moment it is constructed during the start up</summary>
	internal class Speech
	{
		private bool m_Failed; // must be true if there are no voices
		/// <summary>list of all the available voice names </summary>
		public List<string> Voices { get; } = new List<string>();
		private string m_SelectedVoice = "x";
		private readonly SpeechSynthesizer Speaker;

		public Speech()
		{
			try
			{
				Speaker = new SpeechSynthesizer();
				Globals.Root.Log.WriteLine("Current culture=" + System.Globalization.CultureInfo.CurrentCulture.Name);
				foreach (InstalledVoice voice in Speaker.GetInstalledVoices())
				{
					Voices.Add(voice.VoiceInfo.Name);
					Globals.Root.Log.WriteLine("Voice found: " + voice.VoiceInfo.Name);
				}
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
			}
			if (Voices.Count == 0)
				m_Failed = true; // will include errors which this from constructing
		}

		/// <summary>Notification that settings have changed and this may need to update the engine</summary>
		/// <param name="appliedConfig">Supplied so that frmEditConfig can use what is displayed (which may not be current user/teacher)</param>
		internal void SettingsChanged(AppliedConfig appliedConfig)
		{
			if (m_Failed)
				return;
			string voice = appliedConfig.ReadString(Config.Speech_Voice);
			if (!string.IsNullOrEmpty(voice) && voice == m_SelectedVoice)
			{
				// we just reapply the other settings, although they may not have changed.  I think these are probably quite fast
				ApplySettings(appliedConfig);
				return;
			}
			// wrong voice is selected; but we will only select the correct voice when it is needed
			m_SelectedVoice = "x";
		}

		private void EnsureVoice()
		{
			if (m_SelectedVoice != "x" || m_Failed)
				return;
			string voice = Globals.Root.CurrentConfig.ReadString(Config.Speech_Voice);
			int index = Voices.IndexOf(voice);
			if (index < 0)
				index = 0; // if the requested voice is not available, then just select any voice
			Speaker.SelectVoice(Voices[index]);
			m_SelectedVoice = Voices[index];
			ApplySettings(Globals.Root.CurrentConfig);
		}

		private void ApplySettings(AppliedConfig appliedConfig)
		{
			//applies the settings other than the voices selection
			if (m_Failed)
				return;
			try
			{// volume still 0-100
				Speaker.Volume = appliedConfig.ReadInteger(Config.Speech_Volume, 100);
				int rate100 = appliedConfig.ReadInteger(Config.Speech_Speed, 50); // note default was 70 in V2-
				Speaker.Rate = rate100 / 5 - 10; // this is on a -10 to +10 scale
												 // no pitch available
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
			}
		}

		public void Speak(string text)
		{
			if (m_Failed)
				return;
			try
			{
				EnsureVoice();
				Speaker.SpeakAsync(text);
			}
			catch (Exception ex)
			{ Utilities.LogSubError(ex); }
		}

		public void StopAll()
		{
			try
			{
				Speaker?.SpeakAsyncCancelAll();
			}
			catch (Exception ex)
			{ Utilities.LogSubError(ex); }
		}

	}

}
