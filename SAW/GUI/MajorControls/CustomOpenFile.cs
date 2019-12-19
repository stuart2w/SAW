using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

namespace SAW
{
	internal sealed class FilePreviewPanel : Panel
	{
		private string m_File = ""; // currently selected file
		private Image m_Preview ;

		public FilePreviewPanel()
		{
			Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular, GraphicsUnit.Point);
			this.BorderStyle = BorderStyle.Fixed3D;
			this.Paint += FilePreviewPanel_Paint;
			this.Resize += FilePreviewPanel_Resize;
		}

		public string File
		{
			get { return m_File; }
			set
			{
				if (m_File == value)
					return;
				m_File = value;
				m_Preview?.Dispose();
				m_Preview = Document.GetFilePreview(m_File); // Nothing
				Invalidate();
			}
		}

		private void FilePreviewPanel_Paint(object sender, PaintEventArgs e)
		{
			if (!string.IsNullOrEmpty(m_File)) // is no file has been selected yet, then we don't display anything
			{
				if (m_Preview == null)
					e.Graphics.DrawString(Strings.Item("Preview_Unavailable"), Font, Brushes.LightGray, new RectangleF(0, 0, Width, Height), GUIUtilities.StringFormatCentreCentre);
				else
				{
					Rectangle imageRect = new Rectangle(0, 0, Width, Height);
					GUIUtilities.CalcDestRect(m_Preview.Width, m_Preview.Height, ref imageRect);
					e.Graphics.DrawImage(m_Preview, imageRect);
				}
			}
		}

		public void FileSelectionChanged(string file)
		{
			File = file;
		}

		private void FilePreviewPanel_Resize(object sender, EventArgs e)
		{
			Invalidate();
		}

	}

	// from here and translated: http://msdn.microsoft.com/en-us/library/ms996463.aspx
	namespace ExtensibleDialogs
	{

		#region Native methods
		/// <summary>
		/// Defines the shape of hook procedures that can be called by the OpenFileDialog
		/// </summary>
		internal delegate IntPtr OfnHookProc(IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam);

		/// <summary>
		/// Values that can be placed in the OPENFILENAME structure, we don't use all of them
		/// </summary>
		internal class OpenFileNameFlags
		{
			public const Int32 ReadOnly = 0x00000001;
			public const Int32 OverWritePrompt = 0x00000002;
			public const Int32 HideReadOnly = 0x00000004;
			public const Int32 NoChangeDir = 0x00000008;
			public const Int32 ShowHelp = 0x00000010;
			public const Int32 EnableHook = 0x00000020;
			public const Int32 EnableTemplate = 0x00000040;
			public const Int32 EnableTemplateHandle = 0x00000080;
			public const Int32 NoValidate = 0x00000100;
			public const Int32 AllowMultiSelect = 0x00000200;
			public const Int32 ExtensionDifferent = 0x00000400;
			public const Int32 PathMustExist = 0x00000800;
			public const Int32 FileMustExist = 0x00001000;
			public const Int32 CreatePrompt = 0x00002000;
			public const Int32 ShareAware = 0x00004000;
			public const Int32 NoReadOnlyReturn = 0x00008000;
			public const Int32 NoTestFileCreate = 0x00010000;
			public const Int32 NoNetworkButton = 0x00020000;
			public const Int32 NoLongNames = 0x00040000;
			public const Int32 Explorer = 0x00080000;
			public const Int32 NoDereferenceLinks = 0x00100000;
			public const Int32 LongNames = 0x00200000;
			public const Int32 EnableIncludeNotify = 0x00400000;
			public const Int32 EnableSizing = 0x00800000;
			public const Int32 DontAddToRecent = 0x02000000;
			public const Int32 ForceShowHidden = 0x10000000;
		};

		/// <summary>
		/// Values that can be placed in the FlagsEx field of the OPENFILENAME structure
		/// </summary>
		internal class OpenFileNameFlagsEx
		{
			public const Int32 NoPlacesBar = 0x00000001;
		};

		/// <summary>
		/// A small subset of the window messages that can be sent to the OpenFileDialog
		/// These are just the ones that this implementation is interested in
		/// </summary>
		internal class WindowMessage
		{
			public const UInt16 InitDialog = 0x0110;
			public const UInt16 Size = 0x0005;
			public const UInt16 Notify = 0x004E;
		};

		/// <summary>
		/// The possible notification messages that can be generated by the OpenFileDialog
		/// We only look for CDN_SELCHANGE
		/// </summary>
		internal class CommonDlgNotification
		{
			private const UInt16 First = unchecked((UInt16)(0 - 601));

			public const UInt16 InitDone = First - 0x0000;
			public const UInt16 SelChange = First - 0x0001;
			public const UInt16 FolderChange = First - 0x0002;
			public const UInt16 ShareViolation = First - 0x0003;
			public const UInt16 Help = First - 0x0004;
			public const UInt16 FileOk = First - 0x0005;
			public const UInt16 TypeChange = First - 0x0006;
			public const UInt16 IncludeItem = First - 0x0007;
		}

		/// <summary>
		/// Messages that can be send to the common dialogs
		/// We only use CDM_GETFILEPATH
		/// </summary>
		internal class CommonDlgMessage
		{
			private const UInt16 User = 0x0400;
			private const UInt16 First = User + 100;

			public const UInt16 GetFilePath = First + 0x0001;
		};

		/// <summary>
		/// See the documentation for OPENFILENAME
		/// </summary>
		internal struct OpenFileName
		{
			public Int32 lStructSize;
			public IntPtr hwndOwner;
			public IntPtr hInstance;
			public IntPtr lpstrFilter;
			public IntPtr lpstrCustomFilter;
			public Int32 nMaxCustFilter;
			public Int32 nFilterIndex;
			public IntPtr lpstrFile;
			public Int32 nMaxFile;
			public IntPtr lpstrFileTitle;
			public Int32 nMaxFileTitle;
			public IntPtr lpstrInitialDir;
			public IntPtr lpstrTitle;
			public Int32 Flags;
			public Int16 nFileOffset;
			public Int16 nFileExtension;
			public IntPtr lpstrDefExt;
			public Int32 lCustData;
			public OfnHookProc lpfnHook;
			public IntPtr lpTemplateName;
			public IntPtr pvReserved;
			public Int32 dwReserved;
			public Int32 FlagsEx;
		};

		/// <summary>
		/// Part of the notification messages sent by the common dialogs
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		internal struct NMHDR
		{
			[FieldOffset(0)]public IntPtr hWndFrom;
			[FieldOffset(4)]public UInt16 idFrom;
			[FieldOffset(8)]public UInt16 code;
		};

		/// <summary>
		/// Part of the notification messages sent by the common dialogs
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		internal struct OfNotify
		{
			[FieldOffset(0)]public NMHDR hdr;
			[FieldOffset(12)]public IntPtr ipOfn;
			[FieldOffset(16)]public IntPtr ipFile;
		};

		/// <summary>
		/// Win32 window style constants
		/// We use them to set up our child window
		/// </summary>
		internal class DlgStyle
		{
			public const Int32 DsSetFont = 0x00000040;
			public const Int32 Ds3dLook = 0x00000004;
			public const Int32 DsControl = 0x00000400;
			public const Int32 WsChild = 0x40000000;
			public const Int32 WsClipSiblings = 0x04000000;
			public const Int32 WsVisible = 0x10000000;
			public const Int32 WsGroup = 0x00020000;
			public const Int32 SsNotify = 0x00000100;
		};

		/// <summary>
		/// Win32 "extended" window style constants
		/// </summary>
		internal class ExStyle
		{
			public const Int32 WsExNoParentNotify = 0x00000004;
			public const Int32 WsExControlParent = 0x00010000;
		};

		/// <summary>
		/// An in-memory Win32 dialog template
		/// Note: this has a very specific structure with a single static "label" control
		/// See documentation for DLGTEMPLATE and DLGITEMTEMPLATE
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal class DlgTemplate
		{
			// The dialog template - see documentation for DLGTEMPLATE
			public Int32 style = DlgStyle.Ds3dLook | DlgStyle.DsControl | DlgStyle.WsChild | DlgStyle.WsClipSiblings | DlgStyle.SsNotify;
			public Int32 extendedStyle = ExStyle.WsExControlParent;
			public Int16 numItems = 1;
			public Int16 x = 0;
			public Int16 y = 0;
			public Int16 cx = 0;
			public Int16 cy = 0;
			public Int16 reservedMenu = 0;
			public Int16 reservedClass = 0;
			public Int16 reservedTitle = 0;

			// Single dlg item, must be dword-aligned - see documentation for DLGITEMTEMPLATE
			public Int32 itemStyle = DlgStyle.WsChild;
			public Int32 itemExtendedStyle = ExStyle.WsExNoParentNotify;
			public Int16 itemX = 0;
			public Int16 itemY = 0;
			public Int16 itemCx = 0;
			public Int16 itemCy = 0;
			public Int16 itemId = 0;
			public UInt16 itemClassHdr = 0xffff;    // we supply a constant to indicate the class of this control
			public Int16 itemClass = 0x0082;    // static label control
			public Int16 itemText = 0x0000; // no text for this control
			public Int16 itemData = 0x0000; // no creation data for this control
		};

		/// <summary>
		/// The rectangle structure used in Win32 API calls
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		};

		/// <summary>
		/// The point structure used in Win32 API calls
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct POINT
		{
			public int X;
			public int Y;
		};

		/// <summary>
		/// Contains all of the p/invoke declarations for the Win32 APIs used in this sample
		/// </summary>
		public class NativeMethods
		{

			[DllImport("User32.dll", CharSet = CharSet.Unicode)]
			internal static extern IntPtr GetDlgItem(IntPtr hWndDlg, Int16 Id);

			[DllImport("User32.dll", CharSet = CharSet.Unicode)]
			internal static extern IntPtr GetParent(IntPtr hWnd);

			[DllImport("User32.dll", CharSet = CharSet.Unicode)]
			internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

			[DllImport("User32.dll", CharSet = CharSet.Unicode)]
			internal static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, StringBuilder buffer);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			internal static extern int GetWindowRect(IntPtr hWnd, ref RECT rc);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			internal static extern int GetClientRect(IntPtr hWnd, ref RECT rc);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

			[DllImport("user32.dll", CharSet = CharSet.Unicode)]
			internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool repaint);

			[DllImport("ComDlg32.dll", CharSet = CharSet.Unicode)]
			internal static extern bool GetOpenFileName(ref OpenFileName ofn);

			[DllImport("ComDlg32.dll", CharSet = CharSet.Unicode)]
			internal static extern Int32 CommDlgExtendedError();
		}
		#endregion

		#region Extensible dialog
		public class OpenFileDialog : IDisposable
		{
			// The maximum number of characters permitted in a path
			private const int _MAX_PATH = 260;

			// The "control ID" of the content window inside the OpenFileDialog
			// See the accompanying article to learn how I discovered it
			private const int _CONTENT_PANEL_ID = 0x0461;

			// A constant that determines the spacing between panels inside the OpenFileDialog
			private const int _PANEL_GAP_FACTOR = 3;

			/// <summary>
			/// Clients can implement handlers of this type to catch "selection changed" events
			/// </summary>
			public delegate void SelectionChangedHandler(string path);

			/// <summary>
			/// This event is fired whenever the user selects an item in the dialog
			/// </summary>
			public event SelectionChangedHandler SelectionChanged;

			// unmanaged memory buffers to hold the file name (with and without full path)
			private IntPtr _fileNameBuffer;
			private IntPtr _fileTitleBuffer;

			// the OPENFILENAME structure, used to control the appearance and behaviour of the OpenFileDialog
			private OpenFileName _ofn;

			// user-supplied control that gets placed inside the OpenFileDialog
			private Control _userControl;

			// unmanaged memory buffer that holds the Win32 dialog template
			private IntPtr _ipTemplate;

			/// <summary>
			/// Sets up the data structures necessary to display the OpenFileDialog
			/// </summary>
			/// <param name="defaultExtension">The file extension to use if the user doesn't specify one (no "." required)</param>
			/// <param name="fileName">You can specify a filename to appear in the dialog, although the user can change it</param>
			/// <param name="filter">See the documentation for the OPENFILENAME structure for a description of filter strings</param>
			/// <param name="userControl">Any Windows Forms control, it will be placed inside the OpenFileDialog</param>
			public OpenFileDialog(string defaultExtension, string fileName, string filter, Control userControl)
			{
				// Need two buffers in unmanaged memory to hold the filename
				// Note: the multiplication by 2 is to allow for Unicode (16-bit) characters
				_fileNameBuffer = Marshal.AllocCoTaskMem(2 * _MAX_PATH);
				_fileTitleBuffer = Marshal.AllocCoTaskMem(2 * _MAX_PATH);

				// Zero these two buffers
				byte[] zeroBuffer = new byte[2 * (_MAX_PATH + 1)];
				for (int i = 0; i < 2 * (_MAX_PATH + 1); i++) zeroBuffer[i] = 0;
				Marshal.Copy(zeroBuffer, 0, _fileNameBuffer, 2 * _MAX_PATH);
				Marshal.Copy(zeroBuffer, 0, _fileTitleBuffer, 2 * _MAX_PATH);

				// Create an in-memory Win32 dialog template; this will be a "child" window inside the FileOpenDialog
				// We have no use for this child window, except that its presence allows us to capture events when
				// the user interacts with the FileOpenDialog
				_ipTemplate = BuildDialogTemplate();

				// Populate the OPENFILENAME structure
				// The flags specified are the minimal set to get the appearance and behaviour we need
				_ofn.lStructSize = Marshal.SizeOf(_ofn);
				_ofn.lpstrFile = _fileNameBuffer;
				_ofn.nMaxFile = _MAX_PATH;
				_ofn.lpstrDefExt = Marshal.StringToCoTaskMemUni(defaultExtension);
				_ofn.lpstrFileTitle = _fileTitleBuffer;
				_ofn.nMaxFileTitle = _MAX_PATH;
				_ofn.lpstrFilter = Marshal.StringToCoTaskMemUni(filter);
				_ofn.Flags = OpenFileNameFlags.EnableHook | OpenFileNameFlags.EnableTemplateHandle | OpenFileNameFlags.EnableSizing | OpenFileNameFlags.Explorer;
				_ofn.hInstance = _ipTemplate;
				_ofn.lpfnHook = new OfnHookProc(MyHookProc);

				// copy initial file name into unmanaged memory buffer
				UnicodeEncoding ue = new UnicodeEncoding();
				byte[] fileNameBytes = ue.GetBytes(fileName);
				Marshal.Copy(fileNameBytes, 0, _fileNameBuffer, fileNameBytes.Length);

				// keep a reference to the user-supplied control
				_userControl = userControl;
			}

			/// <summary>
			/// The finalizer will release the unmanaged memory, if I should forget to call Dispose
			/// </summary>
			~OpenFileDialog()
			{
				Dispose(false);
			}

			/// <summary>
			/// Display the OpenFileDialog and allow user interaction
			/// </summary>
			/// <returns>true if the user clicked OK, false if they clicked cancel (or close)</returns>
			public bool Show(Form frmOwner)
			{
				return NativeMethods.GetOpenFileName(ref _ofn);
			}

			/// <summary>
			/// Builds an in-memory Win32 dialog template.  See documentation for DLGTEMPLATE.
			/// </summary>
			/// <returns>a pointer to an unmanaged memory buffer containing the dialog template</returns>
			private IntPtr BuildDialogTemplate()
			{
				// We must place this child window inside the standard FileOpenDialog in order to get any
				// notifications sent to our hook procedure.  Also, this child window must contain at least
				// one control.  We make no direct use of the child window, or its control.

				// Set up the contents of the DLGTEMPLATE
				DlgTemplate template = new DlgTemplate();

				// Allocate some unmanaged memory for the template structure, and copy it in
				IntPtr ipTemplate = Marshal.AllocCoTaskMem(Marshal.SizeOf(template));
				Marshal.StructureToPtr(template, ipTemplate, true);
				return ipTemplate;
			}

			/// <summary>
			/// The hook procedure for window messages generated by the FileOpenDialog
			/// </summary>
			/// <param name="hWnd">the handle of the window at which this message is targeted</param>
			/// <param name="msg">the message identifier</param>
			/// <param name="wParam">message-specific parameter data</param>
			/// <param name="lParam">mess-specific parameter data</param>
			/// <returns></returns>
			public IntPtr MyHookProc(IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam)
			{
				if (hWnd == IntPtr.Zero)
					return IntPtr.Zero;

				// Behaviour is dependant on the message received
				switch (msg)
				{
					// We're not interested in every possible message; just return a NULL for those we don't care about
					default:
					{
						return IntPtr.Zero;
					}

					// WM_INITDIALOG - at this point the OpenFileDialog exists, so we pull the user-supplied control
					// into the FileOpenDialog now, using the SetParent API.
					case WindowMessage.InitDialog:
					{
						IntPtr hWndParent = NativeMethods.GetParent(hWnd);
						NativeMethods.SetParent(_userControl.Handle, hWndParent);
						return IntPtr.Zero;
					}

					// WM_SIZE - the OpenFileDialog has been resized, so we'll resize the content and user-supplied
					// panel to fit nicely
					case WindowMessage.Size:
					{
						FindAndResizePanels(hWnd);
						return IntPtr.Zero;
					}

					// WM_NOTIFY - we're only interested in the CDN_SELCHANGE notification message:
					// we grab the currently-selected filename and fire our event
					case WindowMessage.Notify:
					{
						IntPtr ipNotify = new IntPtr(lParam);
						OfNotify ofNot = (OfNotify)Marshal.PtrToStructure(ipNotify, typeof(OfNotify));
						UInt16 code = ofNot.hdr.code;
						if (code == CommonDlgNotification.SelChange)
						{
							// This is the first time we can rely on the presence of the content panel
							// Resize the content and user-supplied panels to fit nicely
							FindAndResizePanels(hWnd);

							// get the newly-selected path
							IntPtr hWndParent = NativeMethods.GetParent(hWnd);
							StringBuilder pathBuffer = new StringBuilder(_MAX_PATH);
							UInt32 ret = NativeMethods.SendMessage(hWndParent, CommonDlgMessage.GetFilePath, _MAX_PATH, pathBuffer);
							string path = pathBuffer.ToString();

							// copy the string into the path buffer
							UnicodeEncoding ue = new UnicodeEncoding();
							byte[] pathBytes = ue.GetBytes(path);
							Marshal.Copy(pathBytes, 0, _fileNameBuffer, pathBytes.Length);

							// fire selection-changed event
							SelectionChanged?.Invoke(path);
						}
						return IntPtr.Zero;
					}
				}
			}

			/// <summary>
			/// Layout the content of the OpenFileDialog, according to the overall size of the dialog
			/// </summary>
			/// <param name="hWnd">handle of window that received the WM_SIZE message</param>
			private void FindAndResizePanels(IntPtr hWnd)
			{
				// The FileOpenDialog is actually of the parent of the specified window
				IntPtr hWndParent = NativeMethods.GetParent(hWnd);

				// The "content" window is the one that displays the filenames, tiles, etc.
				// The _CONTENT_PANEL_ID is a magic number - see the accompanying text to learn
				// how I discovered it.
				IntPtr hWndContent = NativeMethods.GetDlgItem(hWndParent, _CONTENT_PANEL_ID);

				Rectangle rcClient = new Rectangle(0, 0, 0, 0);
				Rectangle rcContent = new Rectangle(0, 0, 0, 0);

				// Get client rectangle of dialog
				RECT rcTemp = new RECT();
				NativeMethods.GetClientRect(hWndParent, ref rcTemp);
				rcClient.X = rcTemp.left;
				rcClient.Y = rcTemp.top;
				rcClient.Width = rcTemp.right - rcTemp.left;
				rcClient.Height = rcTemp.bottom - rcTemp.top;

				// The content window may not be present when the dialog first appears
				if (hWndContent != IntPtr.Zero)
				{
					// Find the dimensions of the content panel
					RECT rc = new RECT();
					NativeMethods.GetWindowRect(hWndContent, ref rc);

					// Translate these dimensions into the dialog's coordinate system
					POINT topLeft;
					topLeft.X = rc.left;
					topLeft.Y = rc.top;
					NativeMethods.ScreenToClient(hWndParent, ref topLeft);
					POINT bottomRight;
					bottomRight.X = rc.right;
					bottomRight.Y = rc.bottom;
					NativeMethods.ScreenToClient(hWndParent, ref bottomRight);
					rcContent.X = topLeft.X;
					rcContent.Width = bottomRight.X - topLeft.X;
					rcContent.Y = topLeft.Y;
					rcContent.Height = bottomRight.Y - topLeft.Y;

					// Shrink content panel's width
					int width = rcClient.Right - rcContent.Left;
					rcContent.Width = width / 2 + _PANEL_GAP_FACTOR;
					NativeMethods.MoveWindow(hWndContent, rcContent.Left, rcContent.Top, rcContent.Width, rcContent.Height, true);
				}

				// Position the user-supplied control alongside the content panel
				Rectangle rcUser = new Rectangle(rcContent.Right + 2 * _PANEL_GAP_FACTOR, rcContent.Top, rcClient.Right - rcContent.Right - 3 * _PANEL_GAP_FACTOR, rcContent.Bottom - rcContent.Top);
				NativeMethods.MoveWindow(_userControl.Handle, rcUser.X, rcUser.Y, rcUser.Width, rcUser.Height, true);
			}

			/// <summary>
			/// returns the path currently selected by the user inside the OpenFileDialog
			/// </summary>
			public string SelectedPath
			{
				get
				{
					return Marshal.PtrToStringUni(_fileNameBuffer);
				}
			}

			#region IDisposable Members

			public void Dispose()
			{
				Dispose(true);
			}

			/// <summary>
			/// Free any unamanged memory used by this instance of OpenFileDialog
			/// </summary>
			/// <param name="disposing">true if called by Dispose, false otherwise</param>
			public void Dispose(bool disposing)
			{
				if (disposing)
				{
					GC.SuppressFinalize(this);
				}

				Marshal.FreeCoTaskMem(_fileNameBuffer);
				Marshal.FreeCoTaskMem(_fileTitleBuffer);
				Marshal.FreeCoTaskMem(_ipTemplate);
			}

			#endregion
		}
		#endregion

	}

}
