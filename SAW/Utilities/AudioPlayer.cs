using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace SAW
{

	/// <summary>Can play WAV or MP3 (unlike System.Media.SoundPlayer)</summary>
	public class AudioPlayer : IDisposable
	{
		private System.Media.SoundPlayer Player = new System.Media.SoundPlayer();
		// media player doesn't support detecting if it is playing, but we can create the effect by playing sync in background: https://stackoverflow.com/questions/27392396/how-to-know-when-soundplayer-has-finished-playing-a-sound
		// however that appeared to block the UI completely :-( 

		public bool Playing
		{
			get => m_playing || waveOut.PlaybackState == PlaybackState.Playing;
			private set => m_playing = value;
		}

		/// <summary>Plays the stream Async, optionally making a callback when ended - which is Invoked on the UI thread of the given control</summary>
		public void PlayAsync(Stream stream, Action callback = null, Control callbackThreadOwner = null)
		{
			PlayAsync(stream, callback, callbackThreadOwner, false);
		}

		WaveOut waveOut = new WaveOut();
		/// <summary>Whether WAV is playing.  MP3 can be detected from the actual player</summary>
		private bool m_playing;

		/// <summary>Plays sound async.  Callback is triggered whenever there is a state change.  HOWEVER Playing state may not work accurately for WAV sounds</summary>
		public void PlayAsync(Stream stream, Action callback, Control callbackThreadOwner, bool disposeStream)
		{
			if (callback != null && callbackThreadOwner == null)
				throw new ArgumentException("PlayAsync requires callbackThreadOwner if callback specified");
			Playing = true;
			Task.Run(() =>
			{
				// stop existing playback of either type:
				waveOut.Stop();
				Player.Stop();

				byte[] header = new byte[4];
				stream.Read(header, 0, 4);
				stream.Seek(0, SeekOrigin.Begin);
				if (BitConverter.ToInt32(header, 0) == 0x46464952)
				{ // WAV
					Player.Stream = stream;
					if (callback != null)
					{ // this only used within item editor
						Player.PlaySync();
					}
					else
					{ // PlaySync seems to block UI, even tho we're on background thread.  as long as there is no callback needed, we can just leave it playing unattended
						Player.Play();
						disposeStream = false;
					}
				}
				else
				{ // assumed MP3
					var reader = new Mp3FileReader(stream);
					waveOut.Init(reader);
					waveOut.Play();
					if (callback != null && !callbackThreadOwner.IsDisposed)
						callbackThreadOwner.Invoke(callback);
					if (disposeStream || callback != null) // need to run some part of code below at end of playback
						while (waveOut.PlaybackState == PlaybackState.Playing)
						{
							Thread.Sleep(100);
						}
				}
				Playing = false;
				if (disposeStream)
					stream.Dispose();
				if (callback != null && !callbackThreadOwner.IsDisposed)
					callbackThreadOwner.Invoke(callback);
			});
		}

		//public void PlayAsync(string file, Action callback = null, Control callbackThreadOwner = null)
		//{
		//	var stream = new FileStream(file, FileMode.Open);
		//	PlayAsync(stream, callback, callbackThreadOwner, true);
		//}

		public void Stop()
		{
			Player?.Stop(); // stops WAV, if any
			waveOut?.Stop();
			Playing = false; // will stop an MP3 due to check in loop
		}

		public void Dispose()
		{
			Stop();
			Player?.Dispose();
			Player = null;
			waveOut?.Dispose();
			waveOut = null;
		}
	}
}
