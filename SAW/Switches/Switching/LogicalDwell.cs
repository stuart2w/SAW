namespace Switches.Switching
{
	/// <summary>Like normal logical switch, but uses Dwell time instead of usual acceptance time</summary>
	public class LogicalDwell: Logical
	{
		public LogicalDwell(ISwitch source, Engine engine) : base(source, engine)
		{
		}

		protected override int AcceptanceTime => m_Engine.ConfiguredTiming(Engine.Timings.DwellSelect);
		public override Engine.Timings RequiredTimings => Engine.Timings.DwellSelect | Engine.Timings.PostActivation;
	}
}
