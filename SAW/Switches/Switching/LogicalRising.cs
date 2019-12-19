using SAW;

namespace Switches.Switching
{
	/// <summary>used as the Short switch where one switch is doing to functions.  This never repeats, it always sends Down followed by Up instantly
	/// m_bolState is effectively always false, except very briefly during the call to OnInputStateChanged which triggers this</summary>
	public class LogicalRising : Logical
	{

		/// <summary>true if it has been held down for acceptance time and not been cancelled by a long press</summary>
		private bool m_Valid;

		public LogicalRising(ISwitch source, Engine engine) : base(source, engine)
		{ }

		protected override void OnInputStateChanged(bool down)
		{
			if (m_Cooldown)
				return; // we can ignore this completely, because we don't send true rising edges outwards
			if (down)
				m_Timer.Start(AcceptanceTime, AcceptanceTimeElapsed);
			else
			{
				if (m_Valid) // was held for long enough
				{
					base.OnStateChanged(true);
					base.OnStateChanged(false);
					m_Valid = false;
					m_Timer.Start(m_Engine.ConfiguredTiming(Engine.Timings.PostActivation), CooledDown);
				}
				else
					m_Timer.Cancel();
			}
		}

		private void AcceptanceTimeElapsed()
		{
			m_Valid = true;
			// and then we just wait for the rising edge....
		}

		public override void Abandon()
		{
			base.Abandon();
			m_Valid = false;
		}

		protected override string TypeName => Strings.Item("Switch_Logic_RisingEdge");
		public override Engine.Timings RequiredTimings => Engine.Timings.AcceptanceTime | Engine.Timings.PostActivation;
	}
}