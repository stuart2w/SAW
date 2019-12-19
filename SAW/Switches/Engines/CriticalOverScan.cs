namespace Switches.Engines
{
	public class CriticalOverScan : Engine
	{

		private int m_ReverseIterations = 0; // if the user does excessive reverse iterations, that cancels the selection
		private const int MAXIMUMREVERSE = 5;


		protected override void Switch1Changed(bool down)
		{
			if (down)
			{
				m_Direction = -1;
				m_ReverseIterations = 0;
			}
			else
			{
				if (m_Direction >= 0)
					return; // must have cancelled; releasing the switch now does nothing
				OnTrigger();
				OnEndSelect();
				// and start iterating forwards again
				m_Direction = +1;
			}
			// must reset the timer in both cases; when reversing because ScanTime is now different
			m_Timer.Start(ScanTime, base.OnIterate);
		}

		protected override void OnIterate()
		{
			if (m_Direction < 0)
			{
				m_ReverseIterations += 1;
				if (m_ReverseIterations > MAXIMUMREVERSE)
				{
					m_Direction = 1;
					m_Timer.Start(ScanTime, base.OnIterate);
					return; // does not fire the actual iterate event this time
				}
			}
			base.OnIterate();
		}

		protected override int ScanTime
		{ get { return ConfiguredTiming(m_Direction < 0 ? Timings.CriticalReverse : Timings.ScanTime); } }

		public override void StartScanning()
		{
			base.StartScanning();
			m_Direction = 1;
			OnIterate();
		}

		public override Timings RelevantTimings // no repeats are possible
		{ get { return Timings.ScanTime | Timings.CriticalReverse | Timings.AcceptanceTime | Timings.PostActivation; } }

		public override Methods Method
		{ get { return Methods.CriticalOverScan; } }

		public override int DefaultTiming(Timings timing)
		{
			switch (timing)
			{
				case Timings.ScanTime:
					return 350;
			}
			return base.DefaultTiming(timing);
		}

	}
}