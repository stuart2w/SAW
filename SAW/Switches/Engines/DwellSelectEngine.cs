namespace Switches.Engines
{
	/// <summary>one switch, dwelling selects. can't make selection until iterated once.  Mostly single step although holding the switch will repeat scan</summary>
	public class DwellSelectEngine : Engine
	{

		protected override void Switch1Changed(bool down)
		{
			if (down)
			{
				m_Timer.Cancel(); // ensure dwell timer stopped regardless of scan repeats
				OnIterate();
			}
			else
				m_Timer.Start(ConfiguredTiming(Timings.DwellSelect), Dwell);
		}

		private void Dwell()
		{
			OnTrigger();
			OnEndSelect();
		}

		public override Timings RelevantTimings
		{// scan time is used for repeats if held down
			get { return Timings.DwellSelect | Timings.ScanTime | Timings.AcceptanceTime | Timings.PostActivation; }
		}

		public override Methods Method
		{ get { return Methods.DwellSelect; } }

	}
}