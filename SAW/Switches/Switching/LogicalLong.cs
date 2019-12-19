using SAW;

namespace Switches.Switching
{
	/// <summary>implements the long trigger with one switch doing two functions</summary>
	public class LogicalLong : Logical
	{

		private LogicalRising ShortSwitch; // the switch doing the short part; we need a reference to this, to tell it to ignore the rising edge if this one has triggered

		public LogicalLong(ISwitch source, LogicalRising shortLogicSwitch, Engine engine) : base(source, engine)
		{
			ShortSwitch = shortLogicSwitch;
		}

		protected override int AcceptanceTime => m_Engine.ConfiguredTiming(Engine.Timings.LongPress);
		protected override string TypeName => Strings.Item("Switch_Logic_Long");
		public override Engine.Timings RequiredTimings => Engine.Timings.LongPress | Engine.Timings.PostActivation;

		protected override void Activate()
		{
			ShortSwitch?.Abandon();
			base.Activate();
		}

		public override int RecommendWaitTrigger
		{
			get
			{
				if (!m_Timer.Running)
					return 0; // only wait if physical is down and we're waiting for time out
				if (m_Cooldown)
					return 0; // and this means switch just released - it's not down
				return m_Timer.RemainingTime + 20; // +20 is just to make sure these fire in right order (granularity is only about 16)
			}
		}

	}
}