using System;
using System.Diagnostics;
using System.IO;

namespace SAW
{
	internal class UserException : Exception
	{
		// An exception which should be reported to the user if not handled, and should not trigger the error reporting mechanism
		// The message will be translated if it starts and ends []

		public UserException(string translateableText, bool breakpoint = false) : base(Strings.Translate(translateableText))
		{
			if (breakpoint)
			{ // leave breakpoint set here:
				short i = 0;
			}
		}
	}

	#region Logging

	public enum TimeStamps
	{
		None,
		Time,
		@Date
	}
	public interface ILogger
	{
		void WriteLine(string str);
		TimeStamps TimeStampEx { get; set; }
		int Indent { get; set; }
		string GetEntireContent();
	}

	internal class NullLogger : ILogger, IDisposable
	{
		// disposable is just implemented to maintain compatibility with the actual log objects

		public int Indent
		{
			get { return 0; }
			set { }
		}

		public TimeStamps TimeStampEx
		{
			get { return TimeStamps.None; }
			set { }
		}

		public void WriteLine(string str)
		{

		}

		public string GetEntireContent()
		{
			return "NullLogger";
		}

		#region  IDisposable Support
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion

	}

	public class CLogFile : ILogger, IDisposable
	{

		private int m_Indent;
		private TimeStamps m_TimeStamp; // = TimeStamps.None
		private StreamWriter m_fs;
		private readonly string m_Path;
		private bool m_Dead; // = false;

		public CLogFile(string path, int appendIfSmallerThan = 0)
		{
			m_Path = path;
			bool keep = false;
			if (appendIfSmallerThan > 0 && File.Exists(m_Path))
			{
				if (new FileInfo(m_Path).Length < appendIfSmallerThan)
					keep = true;
			}
			m_fs = new StreamWriter(m_Path, keep) { AutoFlush = true };
			m_TimeStamp = TimeStamps.Date;
#if DEBUG
			m_TimeStamp = TimeStamps.Time;
#endif
			if (keep == false && appendIfSmallerThan > 0)
				WriteLine("New file started. " + DateTime.Now.ToLongDateString());
		}

		public int Indent
		{
			get { return m_Indent; }
			set
			{
				m_Indent = value;
				if (m_Indent < 0)
				{
					m_Indent = 0;
					Debug.Fail("");
				}
			}
		}

		public TimeStamps TimeStampEx
		{
			get { return m_TimeStamp; }
			set { m_TimeStamp = value; }
		}

		public void WriteLine(string line)
		{
			if (m_Dead)
				return; // some finalisers write, and it is possible for the app exit to have probs without this
			try
			{
				switch (m_TimeStamp)
				{
					case TimeStamps.None:
						break;
					case TimeStamps.Time:
						m_fs.Write("[" + DateTime.Now.ToString("HH:mm:ss ") + Convert.ToString(Environment.TickCount % 10000) + "]");
						break;
					case TimeStamps.Date:
						m_fs.Write("[" + DateTime.Now.ToString("dd-MMM HH:mm:ss") + "]");
						break;
				}
				m_fs.WriteLine(new string(' ', m_Indent) + line);
				m_fs.Flush();
			}
			catch (Exception ex)
			{
				Debug.Fail("Logging failed: " + ex.Message);
				// can't really log the error!
			}
			Debug.WriteLine(line + "   [" + Environment.TickCount % 10000 + "]");
		}

		~CLogFile()
		{
			m_Dead = true;
		}

		public string GetEntireContent()
		{
			m_fs.Close();
			StreamReader fs = new StreamReader(m_Path);
			var strResult = fs.ReadToEnd();
			fs.Close();
			m_fs = new StreamWriter(new FileStream(m_Path, FileMode.Append));
			return strResult;
		}

		public void Dispose()
		{
			if (m_fs != null)
			{
				m_fs.Close();
				m_fs = null;
			}
		}

		public void Flush()
		{
			m_fs.Flush();
		}
	}
	#endregion

}