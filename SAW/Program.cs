using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMenu());
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
			try
			{
				Globals.Root.SaveUserConfigs();
				Globals.Root.SaveSystemConfig(); // needed due to current user; also file paths may have changed
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
			}
		}

	}
}
