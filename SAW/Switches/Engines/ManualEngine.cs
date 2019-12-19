namespace Switches.Engines
{
	/// <summary>Two switches, first one iterates, second one triggers.  Includes single step version</summary>
	public class ManualEngine : EngineTwo
	{
	
		private bool m_SingleStep;
		
		public ManualEngine(bool singleStep)
		{
			m_SingleStep = singleStep;
		}
		
		protected override void Switch1Changed(bool down)
		{
			if (down)
				OnIterate(); // will automatically repeat until the timer is cancelled
			else
				m_Timer.Cancel();
		}
		
		protected override void Switch2Changed(bool down)
		{
			CanStartRepeat = down;
			if (down)
				OnTrigger();
			else
			{
				m_Timer.Cancel();
				OnEndSelect();
			}
		}

		protected override bool IterateCanRepeat
		{get{return ! m_SingleStep;}}
		
		public override Timings RelevantTimings
		{
			get
			{
				var result = Timings.FirstRepeat | Timings.SubsequentRepeat | Timings.AcceptanceTime | Timings.PostActivation;
				if (!m_SingleStep)
					result = result | Timings.ScanTime;
				return result;
			}
		}
		
		public override Methods Method
		{
			get{return m_SingleStep ? Methods.ManualTwoSingleStep : Methods.ManualTwo;}
		}
		
		public override bool AcceptShortLongSwitches
		{get{return m_SingleStep;}}
		
	}
}