using System;
using System.Windows.Forms;


namespace Switches
{
	/// <summary>wrapper for Timer used by state model.  Always fires once and stops.  To repeat the invokee must restart it
		/// NOTE can be Started with time N even if N is zero - it just fires immediately without using the timer</summary>
	public class StateTimer : IDisposable
	{
		// 

		private Timer m_Timer;
		/// <summary>TickCount when started</summary>
		private int m_StartAt;
		/// <summary>Requested duration</summary>
		private int m_Duration;
		private TimerFunction m_fnTimeUp;
		/// <summary>user of this timer can optionally record what the timer is for </summary>
		public Engine.Timings Meaning = 0;

		public delegate void TimerFunction();

		public StateTimer()
		{
			m_Timer = new Timer();
			m_Timer.Tick += m_Timer_Tick;
		}

		public void Start(int time, TimerFunction fnTimeUp, Engine.Timings meaning = 0)
		{
			m_Timer.Enabled = false;
			m_fnTimeUp = null;
			m_Duration = time;
			m_StartAt = Environment.TickCount;
			Meaning = meaning;
			if (time <= 0)
			{
				// fire now
				fnTimeUp.Invoke();
			}
			else
			{
				m_fnTimeUp = fnTimeUp;
				m_Timer.Interval = time;
				m_Timer.Enabled = true;
			}
		}

		/// <summary>if anything happening; safely ignored otherwise</summary>
		public void Cancel()
		{
			m_Timer.Enabled = false;
			m_fnTimeUp = null;
			m_Duration = 0;
			m_StartAt = Environment.TickCount;
			Meaning = 0;
		}

		private void m_Timer_Tick(object sender, EventArgs e)
		{
			m_Timer.Enabled = false;
			if (m_fnTimeUp == null)
				return;
			TimerFunction fnCurrent = m_fnTimeUp;
			m_fnTimeUp = null; // must be before the Invoke, which might change this
			fnCurrent.Invoke();
			// must not clear Meaning - Invoke might want to check it
		}

		public bool Running
		{ get { return m_Timer.Enabled && m_fnTimeUp != null; } }

		public float ElapsedFraction
		{
			get
			{
				// Returns the percentage currently elapsed as a number from 0 to 1.  Used for the diagnostic display
				if (m_Duration == 0 || m_fnTimeUp == null)
					return 0;
				int elapsed = Environment.TickCount - m_StartAt;
				if (elapsed >= m_Duration)
					return 1;
				return (float)elapsed / m_Duration;
			}
		}

		public int RemainingTime
		{
			get
			{
				if (m_Duration == 0 || m_fnTimeUp == null)
					return 0;
				int elapsed = Environment.TickCount - m_StartAt;
				if (elapsed >= m_Duration)
					return 0;
				return m_Duration - elapsed;
			}
		}

		#region  IDisposable Support

		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (this.disposedValue)
				return;
			if (disposing)
			{
				if (m_Timer != null)
				{
					m_Timer.Tick -= m_Timer_Tick;
					m_Timer.Dispose();
				}
				m_Timer = null;
			}
			this.disposedValue = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

	}

}
