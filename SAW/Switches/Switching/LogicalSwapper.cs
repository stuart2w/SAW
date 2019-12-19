using SAW;

namespace Switches.Switching
{
	/// <summary>Implements switch reverse AND swap.  Takes the normal input and an alternate, and reacts to only one depending on whether currently swapped </summary>
	public class LogicalSwapper : Logical
	{
		public LogicalSwapper(ISwitch source, ISwitch alternateSource, Engine engine) : base(source, engine)
		{
			alternateSource.StateChanged += AlternateSource_StateChanged;
		}

		/// <summary>Set to true when switches are currently "swapped" (not reversed).  This does not check the Use Swap setting; and this should only be set if that is enabled</summary>
		public static bool Swap;

		/// <summary>True when the switches work in opposite sense - due to Reverse XOR swap switch </summary>
		public static bool IsInverted
		{
			get
			{ return Swap ^ Globals.Root.CurrentConfig.ReadBoolean(Config.Reverse_Switches); }
		}

		protected override void OnInputStateChanged(bool down)
		{
			if (!IsInverted || (!down && m_State)) // ignore normal switch when swapped, but always send UP signals if currently down (in case swap was set while user holding and the UP is important)
				base.OnInputStateChanged(down);
		}

		private void AlternateSource_StateChanged(bool down)
		{
			if (IsInverted || (!down && m_State))
				base.OnInputStateChanged(down);
		}
	}
}
