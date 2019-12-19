using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Some P/Invoke definitions for commands.  Some individual commands contain their own definitions which are not so widely useful</summary>
	public static class Windows
	{

		#region Positioning and ShowWindow
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hwnd, out Windows.RECT lpRect);

		[DllImport("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint wFlags);
		public const uint SWP_NOSIZE = 0x1;
		public const uint SWP_NOMOVE = 0x2;
		public const uint SWP_NOZORDER = 0x4;
		public const uint SWP_NOACTIVATE = 0x10;
		public const uint SWP_ASYNCWINDOWPOS = 0x4000;
		//Private Const SWP_NOOWNERZORDER As UInt32 = &H200
		public const int HWND_TOP = 0;

		public const int SW_HIDE = 0;
		public const int SW_SHOWNOACTIVATE = 4;
		public const int SW_MAXIMIZE= 3;
		public const int SW_MINIMIZE= 6;
		public const int SW_RESTORE = 9;
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


		//#region SetWindowPos P/Invoke
		//[DllImport("user32", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
		//private static extern int SetWindowPos(int hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint wFlags);
		//private const uint SWP_NOSIZE = 0x1;
		//private const uint SWP_NOMOVE = 0x2;
		//private const uint SWP_NOACTIVATE = 0x10;
		////Private Const SWP_NOOWNERZORDER As UInt32 = &H200
		//private const int HWND_TOP = 0;
		//#endregion

		// message consts
		public const int WM_MOUSEACTIVATE = 0x21;
		public const int MA_NOACTIVATE = 3;
		public const int WM_ACTIVATE = 0x6;

		#endregion

		#region Window styles
		internal const uint WS_OVERLAPPED = 0x00000000;
		internal const uint WS_POPUP = 0x80000000;
		internal const uint WS_CHILD = 0x40000000;
		internal const uint WS_MINIMIZE = 0x20000000;
		internal const uint WS_VISIBLE = 0x10000000;
		internal const uint WS_DISABLED = 0x08000000;
		internal const uint WS_CLIPSIBLINGS = 0x04000000;
		internal const uint WS_CLIPCHILDREN = 0x02000000;
		internal const uint WS_MAXIMIZE = 0x01000000;
		internal const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
		internal const uint WS_BORDER = 0x00800000;
		internal const uint WS_DLGFRAME = 0x00400000;
		internal const uint WS_VSCROLL = 0x00200000;
		internal const uint WS_HSCROLL = 0x00100000;
		internal const uint WS_SYSMENU = 0x00080000;
		internal const uint WS_THICKFRAME = 0x00040000;
		internal const uint WS_GROUP = 0x00020000;
		internal const uint WS_TABSTOP = 0x00010000;

		internal const uint WS_MINIMIZEBOX = 0x00020000;
		internal const uint WS_MAXIMIZEBOX = 0x00010000;

		internal const uint WS_TILED = WS_OVERLAPPED;
		internal const uint WS_ICONIC = WS_MINIMIZE;
		internal const uint WS_SIZEBOX = WS_THICKFRAME;

		//Extended Window Styles

		internal const uint WS_EX_DLGMODALFRAME = 0x00000001;
		internal const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
		internal const uint WS_EX_TOPMOST = 0x00000008;
		internal const uint WS_EX_ACCEPTFILES = 0x00000010;
		internal const uint WS_EX_TRANSPARENT = 0x00000020;

		//#if(WINVER >= 0x0400)
		internal const uint WS_EX_MDICHILD = 0x00000040;
		internal const uint WS_EX_TOOLWINDOW = 0x00000080;
		internal const uint WS_EX_WINDOWEDGE = 0x00000100;
		internal const uint WS_EX_CLIENTEDGE = 0x00000200;
		internal const uint WS_EX_CONTEXTHELP = 0x00000400;

		internal const uint WS_EX_RIGHT = 0x00001000;
		internal const uint WS_EX_LEFT = 0x00000000;
		internal const uint WS_EX_RTLREADING = 0x00002000;
		internal const uint WS_EX_LTRREADING = 0x00000000;
		internal const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
		internal const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;

		internal const uint WS_EX_CONTROLPARENT = 0x00010000;
		internal const uint WS_EX_STATICEDGE = 0x00020000;
		internal const uint WS_EX_APPWINDOW = 0x00040000;

		internal const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
		internal const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
		//#endif /* WINVER >= 0x0400 */

		//#if(_WIN32_WINNT >= 0x0500)
		internal const uint WS_EX_LAYERED = 0x00080000;
		//#endif /* _WIN32_WINNT >= 0x0500 */

		//#if(WINVER >= 0x0500)
		internal const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
		internal const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
														//#endif /* WINVER >= 0x0500 */

		//#if(_WIN32_WINNT >= 0x0500)
		internal const uint WS_EX_COMPOSITED = 0x02000000;
		internal const uint WS_EX_NOACTIVATE = 0x08000000;
#endregion

		#region Set/GetWindowLong
		// https://www.pinvoke.net/default.aspx/user32/SetWindowLong.html
		public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, (int)nIndex, dwNewLong);
			return new IntPtr(SetWindowLong32(hWnd, (int)nIndex, dwNewLong.ToInt32()));
		}

		public static IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongFlags nIndex)
		{
			if (IntPtr.Size == 8)
				return GetWindowLongPtr64(hWnd, (int)nIndex);
			return GetWindowLong32(hWnd, (int)nIndex);
		}


		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		public enum WindowLongFlags : int
		{
			GWL_EXSTYLE = -20,
			GWLP_HINSTANCE = -6,
			GWLP_HWNDPARENT = -8,
			GWL_ID = -12,
			GWL_STYLE = -16,
			GWL_USERDATA = -21,
			GWL_WNDPROC = -4,
			DWLP_USER = 0x8,
			DWLP_MSGRESULT = 0x0,
			DWLP_DLGPROC = 0x4
		}
		#endregion

		#region RECT
		/// <summary>Sadly cannot use .net Rectangle directly</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			//http://pinvoke.net/default.aspx/Structures/RECT.html
			public int Left, Top, Right, Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}

			public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

			public int X
			{
				get { return Left; }
				set { Right -= (Left - value); Left = value; }
			}

			public int Y
			{
				get { return Top; }
				set { Bottom -= (Top - value); Top = value; }
			}

			public int Height
			{
				get { return Bottom - Top; }
				set { Bottom = value + Top; }
			}

			public int Width
			{
				get { return Right - Left; }
				set { Right = value + Left; }
			}

			public Point Location
			{
				get { return new Point(Left, Top); }
				set { X = value.X; Y = value.Y; }
			}

			public Size Size
			{
				get { return new Size(Width, Height); }
				set { Width = value.Width; Height = value.Height; }
			}

			public static implicit operator Rectangle(RECT r)
			{
				return new Rectangle(r.Left, r.Top, r.Width, r.Height);
			}

			public static implicit operator RECT(Rectangle r)
			{
				return new RECT(r);
			}

			public static bool operator ==(RECT r1, RECT r2)
			{
				return r1.Equals(r2);
			}

			public static bool operator !=(RECT r1, RECT r2)
			{
				return !r1.Equals(r2);
			}

			public bool Equals(RECT r)
			{
				return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
			}

			public override bool Equals(object obj)
			{
				if (obj is RECT)
					return Equals((RECT)obj);
				else if (obj is Rectangle)
					return Equals(new RECT((Rectangle)obj));
				return false;
			}

			public override int GetHashCode()
			{
				return ((Rectangle)this).GetHashCode();
			}

			public override string ToString()
			{
				return String.Format(CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
			}
		}
		#endregion

		#region Other Window functions
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		internal const int MAXTITLE = 255;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int _GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

		#endregion


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int GetLastError();

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetFocus();

		#region Memory

		// https://stackoverflow.com/questions/1553336/how-can-i-get-the-total-physical-memory-in-c

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct MEMORYSTATUSEX
		{
			internal uint dwLength;
			internal uint dwMemoryLoad;
			internal ulong ullTotalPhys;
			internal ulong ullAvailPhys;
			internal ulong ullTotalPageFile;
			internal ulong ullAvailPageFile;
			internal ulong ullTotalVirtual;
			internal ulong ullAvailVirtual;
			internal ulong ullAvailExtendedVirtual;
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

		#endregion

		[DllImport("shell32.dll", EntryPoint = "ShellExecuteA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int ShellExecute(IntPtr hwnd, int lpOperationNULL, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);


		[DllImport("user32.dll")]
		public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

		public struct GUITHREADINFO
		{
			public int cbSize;
			public int flags;
			public IntPtr hwndActive;
			public IntPtr hwndFocus;
			public IntPtr hwndCapture;
			public IntPtr hwndMenuOwner;
			public IntPtr hwndMoveSize;
			public IntPtr hwndCaret;
			public Rectangle rcCaret;

		}

		#region Process iteration

		[Flags]
		internal enum SnapshotFlags : uint
		{
			HeapList = 0x00000001,
			Process = 0x00000002,
			Thread = 0x00000004,
			Module = 0x00000008,
			Module32 = 0x00000010,
			All = (HeapList | Process | Thread | Module),
			Inherit = 0x80000000,
			NoHeaps = 0x40000000

		}

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESSENTRY32
		{
			public uint dwSize;
			public uint cntUsage;
			public uint th32ProcessID;
			public IntPtr th32DefaultHeapID;
			public uint th32ModuleID;
			public uint cntThreads;
			public uint th32ParentProcessID;
			public int pcPriClassBase;
			public uint dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExeFile;
		};

		[DllImport("kernel32.dll")]
		internal static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool QueryFullProcessImageName([In]IntPtr hProcess, [In]int dwFlags, [Out]StringBuilder lpExeName, ref int lpdwSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

		[Flags]
		internal enum ProcessAccessFlags : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VirtualMemoryOperation = 0x00000008,
			VirtualMemoryRead = 0x00000010,
			VirtualMemoryWrite = 0x00000020,
			DuplicateHandle = 0x00000040,
			CreateProcess = 0x000000080,
			SetQuota = 0x00000100,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x00001000,
			Synchronize = 0x00100000
		}

		[DllImport("kernel32.dll")]
		internal static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		#endregion

		#region Process creation

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern bool CreateProcess(
			string lpApplicationName,
			string lpCommandLine,
			ref SECURITY_ATTRIBUTES lpProcessAttributes,
			ref SECURITY_ATTRIBUTES lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		// alternate versions allowing nulls for 2 params (via IntPtr.zero)

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern bool CreateProcess(
			string lpApplicationName,
			string lpCommandLine,
			IntPtr NULL_lpProcessAttributes,
			IntPtr NULL_lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		[StructLayout(LayoutKind.Sequential)]
		internal struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			//public unsafe byte* lpSecurityDescriptor;  // <- true version, removed to make this compile (not actually used here)
			public int bInheritHandle;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct STARTUPINFO
		{
			public Int32 cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public Int32 dwX;
			public Int32 dwY;
			public Int32 dwXSize;
			public Int32 dwYSize;
			public Int32 dwXCountChars;
			public Int32 dwYCountChars;
			public Int32 dwFillAttribute;
			public Int32 dwFlags;
			public Int16 wShowWindow;
			public Int16 cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		internal const int STARTF_USEPOSITION = 0x00000004;
		internal const int STARTF_USESIZE = 0x00000002;


		[StructLayout(LayoutKind.Sequential)]
		internal struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}

		[DllImport("user32.dll")]
		internal static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

		internal const uint INFINITE = 0xFFFFFFFF;

		#endregion

		internal delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

		[DllImport("kernel32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr hObject);

		#region Keys

		[DllImport("user32.dll")]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);


		[DllImport("User32.dll")]
		internal static extern short VkKeyScan(char ch);
		
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		internal static extern int ToUnicodeEx(Keys wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

		[DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
		internal static extern uint SendInput(uint nInput, tagINPUT[] pInputs, int cbInput);

		public const uint MAPVK_VK_TO_VSC = 0;

		[DllImport("user32.dll", EntryPoint = "MapVirtualKey")]
		public static extern uint MapVirtualKey(uint intKey, uint intMapType);

		//<DllImport(WinUser, EntryPoint:="keybd_event")> _
		//Public Shared Sub keybd_event(ByVal intKey As Byte, ByVal intScan As Byte, ByVal intFlags As UInt32, ByVal ignore As IntPtr)
		//End Sub

		[DllImport("user32.dll", EntryPoint = "GetKeyState")]
		public static extern short GetKeyState(int key);
		// top bit set means pressed.  Bottom bit set means toggled (Capslock etc only)

		#region Enumerations

		internal enum INPUTTYPE
		{
			MOUSE = 0,
			KEYBOARD = 1,
			HARDWARE = 2
		}

		[Flags()]
		public enum MOUSEEVENTF
		{
			MOVE = 0x1, // mouse move
			LEFTDOWN = 0x2, // left button down
			LEFTUP = 0x4, // left button up
			RIGHTDOWN = 0x8, // right button down
			RIGHTUP = 0x10, // right button up
			MIDDLEDOWN = 0x20, // middle button down
			MIDDLEUP = 0x40, // middle button up
			XDOWN = 0x80, // x button down
			XUP = 0x100, // x button down
			WHEEL = 0x800, // wheel button rolled
			HWHEEL = 0x1000, // hwheel button rolled
			MOVE_NOCOALESCE = 0x2000, // do not coalesce mouse moves
			VIRTUALDESK = 0x4000, // map to entire virtual desktop
			ABSOLUTE = 0x8000 // absolute move
		}

		[Flags()]
		internal enum KEYEVENTF
		{
			EXTENDEDKEY = 0x1,
			KEYUP = 0x2,
			Unicode = 0x4,
			SCANCODE = 0x8
		}

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		internal struct tagHARDWAREINPUT
		{
			public int uMsg;
			public short wParamL;
			public short wParamH;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct tagKEYBDINPUT
		{
			public short wVk;
			public short wScan;
			public Windows.KEYEVENTF dwFlags;
			public uint time;
			public UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct tagMOUSEINPUT
		{
			public int dx;
			public int dy;
			public int mouseData;
			public Windows.MOUSEEVENTF dwFlags;
			public uint time;
			public UIntPtr dwExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct tagINPUT
		{

			public tagINPUT(Windows.INPUTTYPE eType)
			{
				dwType = eType;
				union = new UnionTag();
			}

			public Windows.INPUTTYPE dwType;
			public UnionTag union;
			[StructLayout(LayoutKind.Explicit)]
			public struct UnionTag
			{
				[FieldOffset(0)]
				public tagMOUSEINPUT mi;
				[FieldOffset(0)]
				public tagKEYBDINPUT ki;
				[FieldOffset(0)]
				public tagHARDWAREINPUT hi;
			}
		}

		#endregion


		#endregion

		#region Clipboard and WMV
		// for putting metafiles on clipboard: http://support.microsoft.com/kb/323530

		internal class ClipboardMetafileHelper
		{
			[DllImport("user32.dll", EntryPoint = "OpenClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool OpenClipboard(IntPtr hWnd);
			[DllImport("user32.dll", EntryPoint = "EmptyClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool EmptyClipboard();
			[DllImport("user32.dll", EntryPoint = "SetClipboardData", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern IntPtr SetClipboardData(int uFormat, IntPtr hWnd);
			[DllImport("user32.dll", EntryPoint = "CloseClipboard", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool CloseClipboard();
			[DllImport("gdi32.dll", EntryPoint = "CopyEnhMetaFileA", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, IntPtr hNULL);

			[DllImport("gdi32.dll", EntryPoint = "CopyEnhMetaFileW", CharSet = CharSet.Unicode)]
			public static extern uint CopyEnhMetaFileW(IntPtr hEMF, string strFilename);


			[DllImport("gdi32.dll", EntryPoint = "DeleteEnhMetaFile", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern bool DeleteEnhMetaFile(IntPtr hemfSrc);

			[DllImport("user32.dll", EntryPoint = "GetClipboardOwner", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
			public static extern IntPtr GetClipboardOwner();

			// Metafile mf is set to a state that is not valid inside this function.
			public static bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile mf)
			{
				var bResult = false;
				var hEMF = mf.GetHenhmetafile();
				if (!hEMF.Equals(new IntPtr(0)))
				{
					var hEMF2 = CopyEnhMetaFile(hEMF, new IntPtr(0));
					if (!hEMF2.Equals(new IntPtr(0)))
					{
						if (OpenClipboard(hWnd))
						{
							// even if the clipboard is opened with the same owner and not emptied, this still seems to overwrite any previous data added by .net
							//If OpenClipboard(GetClipboardOwner()) Then
							if (EmptyClipboard())
							{
								var hRes = SetClipboardData(14, hEMF2);
								bResult = hRes.Equals(hEMF2);
								CloseClipboard();
							}
						}
					}
					DeleteEnhMetaFile(hEMF);
				}
				return bResult;
			}
		}

		#endregion
	}
}
