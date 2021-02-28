namespace Switches.Engines
{
	/// <summary>single switch user scan: press and hold to iterate; releasing selects</summary>
	public class UserSingle : Engine
	{

		protected override void Switch1Changed(bool down)
		{
			if (down)
				//OnIterate(); // and will automatically repeat // WRONG - this isn't supposed to provide an iteration when first pressed as the first item cannot be selected
				m_Timer.Start(ScanTime, OnIterate, Timings.ScanTime);
			else
			{
				m_Timer.Cancel();
				OnTrigger();
				OnEndSelect();
			}
		}

		public override Timings RelevantTimings => Timings.ScanTime | Timings.AcceptanceTime | Timings.PostActivation; // no repeats are possible

		public override Methods Method => Methods.UserSingle;

	}
}