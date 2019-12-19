namespace Switches.Switching
{
	/// <summary>Logical switch used as a placeholder with no timing. State change is always instantaneous</summary>
	public class LogicalPassThrough : Logical
	{
		public LogicalPassThrough(ISwitch source, Engine engine) : base(source, engine)
		{ }

		protected override void OnInputStateChanged(bool down)
		{
			OnStateChanged(down);
		}

		protected override int AcceptanceTime => 0;
		public override Engine.Timings RequiredTimings => 0;
	}
}
