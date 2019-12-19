using Switches.Switching;

namespace Switches.Engines
{
	/// <summary>Base class for two switch engines
	/// mainly this just attaches the event handler for the second switch, and calls the abstract Switch2Changed when it changes</summary>
	public abstract class EngineTwo : Engine
	{

		protected Logical Switch2;

		public override void Initialise()
		{
			base.Initialise();
			Switch2 = Logical.Switch(1);
			Switch2.StateChanged += Switch2Changed;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && Switch2 != null)
			{
				Switch2.StateChanged -= Switch2Changed;
				Switch2 = null;
			}
		}

		protected abstract void Switch2Changed(bool down);

		public override int NumberSwitchInputs
		{ get { return 2; } }

		public override Logical[] GetInputs()
		{
			return new[] { Switch1, Switch2 };
		}

	}
}