using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

// ReSharper disable UnusedMember.Local

namespace SAW
{
	/// <summary>Both represents information recorded about a desktop as well as several static methods for accessing Windows info about processes and windows</summary>
	public class Desktop
	{
		public readonly List<Program> Programs = new List<Program>();

		#region Processes
		/// <summary>Used as param to IterateRunningProcesses.   Return true to continue search.</summary>
		public delegate bool ProcessFunction(Windows.PROCESSENTRY32 pe32, string name);

		/// <summary>Iterates all processes in Windows, running the given function.  This is based on processes, NOT windows.  Some may be hidden/background</summary>
		public static void IterateRunningProcesses(ProcessFunction procFunction)
		{
			Windows.PROCESSENTRY32 pe32 = new Windows.PROCESSENTRY32();
			//  Take a snapshot of all processes in the system. 
			var hProcessSnap = Windows.CreateToolhelp32Snapshot(Windows.SnapshotFlags.Process, 0);
			if (hProcessSnap.IsInvalid())
				return;

			//  Fill in the size of the structure before using it. 
			pe32.dwSize = (uint)Marshal.SizeOf(typeof(Windows.PROCESSENTRY32));
			//  Walk the snapshot of the processes, and for each process, display information. 
			if (Windows.Process32First(hProcessSnap, ref pe32))
			{
				do
				{
					int nLength = 1024;
					StringBuilder szPath = new StringBuilder(nLength);
					// requires Vista or later:  (we no longer have the fall back that was in SAW6)
					IntPtr hProcess = Windows.OpenProcess(Windows.ProcessAccessFlags.QueryLimitedInformation, false, (int)pe32.th32ProcessID);
					if (!hProcess.IsZero()) // will fail for a few (eg System process is protected), but they're not going to be the process we're looking for.
					{
						if (Windows.QueryFullProcessImageName(hProcess, 0, szPath, ref nLength))
						{
							if (!procFunction(pe32, szPath.ToString(0, nLength)))
							{
								Windows.CloseHandle(hProcessSnap);
								Windows.CloseHandle(hProcess);
								return;
							}
						}
						Windows.CloseHandle(hProcess);
					}
				} while (Windows.Process32Next(hProcessSnap, ref pe32));
			}
			Windows.CloseHandle(hProcessSnap);
		}

		#endregion

		#region Program class representing 1 item

		/// <summary>Represents one app/window in a saved desktop</summary>
		public class Program
		{
			/// <summary>Effectively window name</summary>
			public string Name;

			/// <summary>Path of EXE</summary>
			public string Path;

			/// <summary>Location of window</summary>
			public Rectangle Bounds;

			public Program()
			{
			}

			public Program(XElement element)
			{
				Name = element.Element("Name").Value;
				Path = element.Element("Path").Value;
				int X = Utilities.IntVal(element.Element("Left").Value);
				int Y = Utilities.IntVal(element.Element("Top").Value);
				int bottom = Utilities.IntVal(element.Element("Bottom").Value);
				int right = Utilities.IntVal(element.Element("Right").Value);
				Bounds = Rectangle.FromLTRB(X, Y, right, bottom);
			}

			public XElement ToXML()
			{
				return new XElement("Program",
					new XElement("Name", Name),
					new XElement("Path", Path),
					new XElement("Left", Bounds.X),
					new XElement("Top", Bounds.Y),
					new XElement("Bottom", Bounds.Bottom),
					new XElement("Right", Bounds.Right));
			}
		}

		#endregion

		#region Desktop object / list of windows manipulation
		/// <summary>Fills Programs with all currently visible windows</summary>
		public void PopulateFromWindows()
		{
			Programs.Clear();
			if (!Windows.EnumDesktopWindows(IntPtr.Zero, EnumWindowsIntoProgramsList, IntPtr.Zero))
				throw new UserException("[SAW_Desktop_Failed]", true);
		}

		/// <summary>Part of PopulateFromWindows only</summary>
		private bool EnumWindowsIntoProgramsList(IntPtr hWnd, int lParam)
		{
			if (Windows.IsWindowVisible(hWnd))  // Store the window only if the window is visible
			{
				string name = GetWindowText(hWnd);
				if (string.IsNullOrEmpty(name))
					return true;  // we get a load of empty window names - usually followed by another for same app with title - don't really want multiple per app anyway

				uint processId;
				Windows.GetWindowThreadProcessId(hWnd, out processId);
				if (processId == Process.GetCurrentProcess().Id) // this window is ours!
					return true;

				string path = null;
				int length = 1024;
				StringBuilder pathBuilder = new StringBuilder(length);
				// requires Vista or later:  (we no longer have the fall back that was in SAW6)
				IntPtr processHandle = Windows.OpenProcess(Windows.ProcessAccessFlags.QueryLimitedInformation, false, (int)processId);
				if (!processHandle.IsZero()) // will fail for a few (eg System process is protected), but they're not going to be of any interest
				{
					if (Windows.QueryFullProcessImageName(processHandle, 0, pathBuilder, ref length))
						path = pathBuilder.ToString();
					Windows.CloseHandle(processHandle);
				}

				Windows.RECT bounds;
				Windows.GetWindowRect(hWnd, out bounds);
				if (!string.IsNullOrEmpty(path))
					Programs.Add(new Program() { Name = name, Path = path, Bounds = bounds });
				else
					Debug.WriteLine($"Could not find path for window: {name}\r\n");
			}

			return true;
		}

		private static IntPtr g_FoundHWND;
		private static bool EnumWindowsFindForProcess(IntPtr hWnd, int lParam)
		{ // the lParam was the one provided to EnumDesktopWindows, containing the process ID
			if (Windows.IsWindowVisible(hWnd))
			{
				uint processID;
				Windows.GetWindowThreadProcessId(hWnd, out processID); // returns thread (which we don't want) and the processID is in the out param
				if (processID == (uint)lParam)
				{
					g_FoundHWND = hWnd;
					//SetForegroundWindow(hWnd);
					Debug.WriteLine("Found window for process");
					return false; // can stop enumeration now
				}
			}
			return true;
		}

		/// <summary>Returns the first visible window for the process or IntPtr.Zero if none, with optional error message in out param </summary>
		public static IntPtr FindWindowForProcess(IntPtr processID, out string errorMessage)
		{
			errorMessage = null;
			g_FoundHWND = IntPtr.Zero;
			if (!Windows.EnumDesktopWindows(IntPtr.Zero, EnumWindowsFindForProcess, processID))
				errorMessage = $"FindWindowForProcess failed with code {Marshal.GetLastWin32Error()}.";
			return g_FoundHWND;
		}

		/// <summary>Saves the Programs list to the given file in XML format (compatible with SAW6)</summary>
		public void Save(string file)
		{
			XElement root = new XElement("Desktop");
			foreach (Program p in Programs)
				root.Add(p.ToXML());
			File.WriteAllText(file, root.ToString());
		}

		/// <summary>Loads an XML format desktop file and returns the object</summary>
		public static Desktop LoadFrom(string file)
		{
			XDocument document = XDocument.Load(file);
			Desktop create = new Desktop();
			foreach (XElement element in document.Root.Elements())
			{
				if (element.Name != "Program")
					throw new InvalidDataException("Desktop object should contain only program elements");
				create.Programs.Add(new Program(element));
			}
			return create;
		}

		/// <summary>Opens all the applications listed in the desktop, unless they are already open</summary>
		public void OpenApplications()
		{
			// get list of running applications to avoid opening them again
			Desktop running = new Desktop();
			running.PopulateFromWindows();
			foreach (Program open in Programs)
			{
				if (!running.Programs.Any(p => string.Compare(p.Path, open.Path, true) == 0))
					OpenProgram(open);
			}
		}

		private bool OpenProgram(Program program)
		{
			Windows.PROCESS_INFORMATION processInfo = new Windows.PROCESS_INFORMATION();
			Windows.STARTUPINFO startupInfo = new Windows.STARTUPINFO
			{
				cb = Marshal.SizeOf(typeof(Windows.STARTUPINFO)),
				dwFlags = Windows.STARTF_USEPOSITION | Windows.STARTF_USESIZE,
				dwX = program.Bounds.Left,
				dwY = program.Bounds.Top,
				dwXSize = program.Bounds.Width,
				dwYSize = program.Bounds.Height
			};

			if (!Windows.CreateProcess(program.Path, null, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref startupInfo, out processInfo)) return false;
			//Wait until process has started its main message loop
			Windows.WaitForInputIdle(processInfo.hProcess, Windows.INFINITE);
			//Close process and thread handles
			Windows.CloseHandle(processInfo.hThread);
			Windows.CloseHandle(processInfo.hProcess);
			return true;
		}

		#endregion

		private static string GetWindowText(IntPtr hWnd)
		{
			StringBuilder title = new StringBuilder(Windows.MAXTITLE);
			int length = Windows._GetWindowText(hWnd, title, title.Capacity + 1);
			title.Length = length;
			return title.ToString();
		}

		internal static int GetProcessID(string application)
		{
			application = application.ToLower();
			int ID = -1;
			IterateRunningProcesses((pe32, name) =>
			{
				if (name.ToLower().Contains(application))
				{
					ID = (int)pe32.th32ProcessID;
					return false;
				}
				return true;
			});
			Debug.WriteLineIf(ID == -1, "Could not find process ID for: " + application);
			return ID;

			//application = application.ToLower();
			//PROCESSENTRY32 pe32 = new PROCESSENTRY32();
			////  Take a snapshot of all processes in the system. 
			//var hProcessSnap = CreateToolhelp32Snapshot(SnapshotFlags.Process, 0);
			//if (hProcessSnap.IsInvalid())
			//	return 0;

			////  Fill in the size of the structure before using it. 
			//pe32.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
			////  Walk the snapshot of the processes, and for each process, display information. 
			//if (Process32First(hProcessSnap, ref pe32))
			//{
			//	do
			//	{
			//		int nLength = 1024;
			//		StringBuilder szPath = new StringBuilder(nLength);
			//		// requires Vista or later:  (we no longer have the fall back that was in SAW6)
			//		IntPtr hProcess = OpenProcess( ProcessAccessFlags.QueryLimitedInformation, false, (int) pe32.th32ProcessID); 
			//		if (!hProcess.IsZero()) // will fail for a few (eg System process is protected), but they're not going to be the process we're looking for.
			//		{
			//			if (QueryFullProcessImageName(hProcess, 0, szPath, ref nLength))
			//			{
			//				if (szPath.ToString(0,nLength).ToLower().Contains(application))
			//				{
			//					CloseHandle(hProcessSnap);
			//					CloseHandle(hProcess);
			//					return (int) pe32.th32ProcessID;
			//				}
			//			}
			//			CloseHandle(hProcess);
			//		}
			//	} while (Process32Next(hProcessSnap, ref pe32));
			//}
			//Debug.WriteLine("Could not find process ID for: "+ application);
			//CloseHandle(hProcessSnap);
			//return 0;
		}

	}
}
