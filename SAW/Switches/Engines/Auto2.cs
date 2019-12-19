namespace Switches.Engines
{
	/// <summary>switch one starts OR stops the iterator.  Switch 2 selects</summary>
	public class Auto2 : EngineTwo
	{

		public override Methods Method
		{ get { return Methods.AutoTwo; } }

		public override Timings RelevantTimings
		{ get { return Timings.ScanTime | Timings.FirstRepeat | Timings.SubsequentRepeat | Timings.AcceptanceTime | Timings.PostActivation; } }

		public override bool AcceptShortLongSwitches
		{ get { return false; } }

		protected override void Switch1Changed(bool down)
		{
			// only down matters - this starts or stops
			if (down)
			{
				if (m_Timer.Running && m_Timer.Meaning == Timings.ScanTime)
					m_Timer.Cancel(); // scanning - so stop
				else // start scanning
					m_Timer.Start(ConfiguredTiming(Timings.ScanTime), OnIterate, Timings.ScanTime);
			}
		}

		protected override void Switch2Changed(bool down)
		{
			CanStartRepeat = down; // this must not just be set in down if branch - an up during OnTrigger should cancel
			if (down)
				OnTrigger();
			else
			{
				m_Timer.Cancel(); // stop repeating
				OnEndSelect();
			}
		}

	}
}