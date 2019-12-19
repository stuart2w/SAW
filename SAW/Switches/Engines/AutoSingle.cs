namespace Switches.Engines
{
	/// <summary>the simple one switch: iteration is continuous, switch selects</summary>
	public class AutoSingle : Engine
	{
	
		protected override void Switch1Changed(bool down)
		{
			CanStartRepeat = down;
			if (down)
			{
				m_Timer.Cancel();
				OnTrigger();
			}
			else
			{
				OnEndSelect();
				// holding the switch can repeat; scanning only starts once released
				m_Timer.Start(ScanTime, base.OnIterate);
			}
		}
		
		public override void StartScanning()
		{
			base.StartScanning();
			m_Timer.Start(ScanTime, base.OnIterate);
		}
		
		public override Timings RelevantTimings
		{get{return Timings.ScanTime | Timings.FirstRepeat | Timings.SubsequentRepeat | Timings.AcceptanceTime | Timings.PostActivation; }}
		
		public override Methods Method
		{get{return Methods.AutoSingle;}}
		
	}
}