namespace Switches.Switching
{
	public interface ISwitch
	{
		// shared between Logical and Physical switches, to assist with the display
		
		bool State {get;}

		event StateEventHandler StateChanged;
	}
}