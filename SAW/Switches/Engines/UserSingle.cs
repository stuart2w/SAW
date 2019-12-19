namespace Switches.Engines
{
	/// <summary>single switch user scan: press and hold to iterate; releasing selects</summary>
	public class UserSingle : Engine
	{

		protected override void Switch1Changed(bool down)
		{
			if (down)
				OnIterate(); // and will automatically repeat
			else
			{
				m_Timer.Cancel();
				OnTrigger();
				OnEndSelect();
			}
		}

		public override Timings RelevantTimings // no repeats are possible
		{ get { return Timings.ScanTime | Timings.AcceptanceTime | Timings.PostActivation; } }

		public override Methods Method
		{ get { return Methods.UserSingle; } }

	}
}