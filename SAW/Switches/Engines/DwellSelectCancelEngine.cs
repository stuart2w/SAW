namespace Switches.Engines
{
	/// <summary>as dwell select; second switch cancels</summary>
	public class DwellSelectCancelEngine : EngineTwo
	{

		#region Matching dwell select
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
			// in case of short/long switch, check if we should wait for current press to elapse
			int intDelay = Switch2.RecommendWaitTrigger;
			if (intDelay > 0)
				m_Timer.Start(intDelay, Dwell); // come back here later, once the switch is released
			else
			{
				OnTrigger();
				OnEndSelect();
			}
		}
		
		public override Timings RelevantTimings
		{ // scan time is used for repeats if held down
			get{return Timings.DwellSelect | Timings.ScanTime | Timings.AcceptanceTime | Timings.PostActivation; }
		}
		#endregion
		
		protected override void Switch2Changed(bool down)
		{
			if (down)
				m_Timer.Cancel();
		}
		
		public override Methods Method
		{get{return Methods.DwellSelectWithCancel;}}
	}
}