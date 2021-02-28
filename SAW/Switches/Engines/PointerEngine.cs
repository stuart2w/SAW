using Switches.Switching;

namespace Switches.Engines
{
	/// <summary>Implements all kinds of pointer logic (direct, dwell and with average)
	/// Most of the logic is in the switch</summary>
	public class PointerEngine : Engine
	{
		private bool m_Dwell;
		private Methods m_Method;

		public PointerEngine(Methods method)
		{
			m_Dwell = method != Methods.DirectPointer;
			m_Method = method;
		}

		public override void Initialise()
		{
			// unlike most engines this will also create switches
			PhysicalSwitch.AssignSwitch(0, PhysicalSwitch.Types.Pointer, (int)m_Method);
			if (m_Method == Methods.DwellAveragePointer)
			{
				Logical.CreateSwitches(Logical.Number.PassThrough, this);
				(PhysicalSwitch.Switch(0) as PointerDwellAverageSwitch).Engine = this;
			}
			else
				Logical.CreateSwitches(m_Dwell ? Logical.Number.DwellOnly : Logical.Number.One, this);
			base.Initialise(); // must be AFTER switches assigned
		}

		protected override void OnIterate()
		{
			base.OnIterate();
		}

		protected override void Switch1Changed(bool down)
		{ // copied from AutoSingle atm
			CanStartRepeat = down;
			if (down)
			{
				m_Timer.Cancel();
				OnTrigger();
			}
			else
			{
				OnEndSelect();
			}
		}

		#region Info
		public override Timings RelevantTimings
		{
			get
			{
				Timings result = Timings.PostActivation;
				result |= m_Dwell ? Timings.DwellSelect : Timings.AcceptanceTime;
				return result;
			}
		}

		public override Methods Method => m_Method;

		public override int NumberSwitchInputs => 0;

		#endregion

	}
}
