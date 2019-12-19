using System.Diagnostics;
using Blade;

namespace SAW
{
	/// <summary>Wrapper for the Blade engine - maintaining a single instance, even when scan isn't running</summary>
	public sealed class BladeWrapper : Blade.INotification
	{

		private static Blade.Engine g_Blade;
		private static BladeWrapper g_Instance;  // instance of this class for callbacks
		public static Blade.Engine Blade
		{
			get
			{
				if (g_Blade == null)
				{
					g_Blade = new Blade.Engine();
					g_Instance = new BladeWrapper();
					g_Blade.Initialise(Globals.Root.SharedFolder + "\\basedata-%lang%.bin", g_Instance);
				}
				return g_Blade;
			}
		}

		// blade callbacks
		void INotification.Notify(Notification state)
		{
			Debug.WriteLine("Blade notification: " + state);
		}

		bool INotification.Learning(string text)
		{
			return true; // could return false to instruct Blade not to learn this text
		}
	}
}
