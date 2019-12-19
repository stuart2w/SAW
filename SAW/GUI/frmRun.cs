using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Switches.Switching;

namespace SAW
{
	public partial class frmRun : Form
	{
		private Document m_Document;
		private Page m_Page;

		#region Construct start and dispose
		public frmRun()
		{
			InitializeComponent();
			SetHiddenTitle();
			Globals.Root.CurrentDocumentChanged += CurrentDocumentChanged;
			Globals.Root.CurrentPageChanged += CurrentPageChanged;
		}

		private void SetHiddenTitle()
		{
			FormBorderStyle = Globals.Root.CurrentConfig.ReadBoolean(Config.Hide_Title) ? FormBorderStyle.None : FormBorderStyle.Sizable;
		}

		private void CurrentDocumentChanged()
		{
			DisplayDocument(Globals.Root.CurrentDocument);
		}

		private void CurrentPageChanged()
		{
			m_Page = Globals.Root.CurrentPage;
			MainView.DisplayPage(m_Page, m_Document);
		}

		protected override CreateParams CreateParams
		{ // https://stackoverflow.com/questions/11143905/how-can-i-prevent-a-form-from-stealing-focus-while-still-receiving-mouse-clicks
			get
			{
				CreateParams p = base.CreateParams;
				p.ExStyle |= (int)(Windows.WS_EX_NOACTIVATE | Windows.WS_EX_TOPMOST);//  | Windows. WS_EX_TOOLWINDOW
				p.Parent = IntPtr.Zero; // not entirely relevant, but maybe still useful?
				return p;
			}
		}

		// didnt help
		//protected override void OnActivated(EventArgs e)
		//{
		//	base.OnActivated(e);
		//	Windows.SetWindowLongPtr(Handle, Windows.WindowLongFlags.GWL_EXSTYLE,
		//		new IntPtr((int)Windows.GetWindowLongPtr(Handle, Windows.WindowLongFlags.GWL_EXSTYLE) | Windows.WS_EX_NOACTIVATE));
		//}

		protected override void Dispose(bool disposing)
		{
			Globals.Root.CurrentPageChanged -= CurrentPageChanged;
			Globals.Root.CurrentDocumentChanged -= CurrentDocumentChanged;
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>Starts running with the given set</summary>
		public void Start(Document document)
		{
			SetHiddenTitle();
			DisplayDocument(document);
			MainView.StartScan();
		}

		/// <summary>Called when run mode starts, but also if set is changed while running</summary>
		public void DisplayDocument(Document document)
		{
			m_Document = document;
			m_Page = document.Pages.First();
			this.WindowState = FormWindowState.Normal;

			MainView.DisplayPage(m_Page, document);
			StringBuilder title = new StringBuilder();
			title.Append(Strings.Item("App"));
			title.Append(" (v").Append(SoftwareVersion.VersionString).Append(")");
			title.Append(" - ").Append(m_Document.FileTitle());
			this.Text = title.ToString();
			Rectangle targetPosition = m_Document.SAWHeader.GetAdjustedWindowBounds();
			if (!targetPosition.IsEmpty)
			{
				GUIUtilities.SetFormClientBounds(this, targetPosition);
				Debug.WriteLine($"frmRun target position={targetPosition}, using ClientRectangle={ClientRectangle}, resulting panel={RectangleToScreen(MainView.Bounds)}");
			}
		}

		public void Stop()
		{
			MainView.StopScan();
		}

		private void frmRun_FormClosing(object sender, FormClosingEventArgs e)
		{ // not called when reverting to user mode as the form is just hidden
		  // rather only called if the user explicitly closes it - which must quit (the editor form will still exist hidden)
		  //Debug.WriteLine(Switches.Switching.PhysicalSwitch.Switch(0).Type);
			if (e.CloseReason == CloseReason.UserClosing
				&& (PhysicalSwitch.Switch(0)?.Type ?? PhysicalSwitch.Types.Null) == PhysicalSwitch.Types.Pointer)
			{
				this.TopMost = false; // if true the question won't be visible!
				var response = GUIUtilities.QuestionBox("[SAW_Run_CloseConfirm]", MessageBoxButtons.YesNo);
				this.TopMost = true;
				if (response != DialogResult.Yes)
				{
					e.Cancel = true;
					return;
				}
			}
			MainView.StopScan();
			if (!Globals.Root.Closing)
				Globals.Root.CloseApplication();
		}

		#endregion

		#region System menu

		private const int WM_SYSCOMMAND = 0x112;
		private const int MF_STRING = 0x0;
		private const int MF_SEPARATOR = 0x800;

		// P/Invoke declarations
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

		//[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		//private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);


		// ID for the About item on the system menu
		private int SYSMENU_CUSTOM_ID = 0x1;

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			IntPtr sysMenu = GetSystemMenu(this.Handle, false);// Get a handle to a copy of this form's system (window) menu
			AppendMenu(sysMenu, MF_SEPARATOR, 0, string.Empty);
			AppendMenu(sysMenu, MF_STRING, SYSMENU_CUSTOM_ID,  Strings.Item( "SAW_Run_Return"));
		}


		private const int WM_NCLBUTTONDBLCLK = 0xA3;

		[DebuggerStepThrough()]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{ // went back to editing on click on sysmenu icon (top left), but Mats thought that was too easy to do by mistake
			  //case WM_SYSCOMMAND:
			  //	if (m.WParam.ToInt32() == 0xf093)
			  //	{
			  //		Globals.Root.ShowEditScreen(m_Document, true);
			  //		return; // otherwise this gets disposed
			  //	}
			  //	break;
			  //case WM_NCLBUTTONDBLCLK:
			  //	// https://social.msdn.microsoft.com/Forums/vstudio/en-US/47152d98-4221-4af2-a467-c1cd73757409/capture-form-title-bar-double-click-event
			  //	Debug.WriteLine("NC area: " + m.WParam.ToString("x"));
			  //	if (m.WParam.ToInt32() == 3)
			  //	{
			  //		Globals.Root.ShowEditScreen(m_Document, true);
			  //		return;
			  //	}
			  //	break;
				case WM_SYSCOMMAND:
					if ((int)m.WParam == SYSMENU_CUSTOM_ID)
					{
						Globals.Root.ShowEditScreen(m_Document, true);
						return;
					}
					break;
				case Windows.WM_MOUSEACTIVATE:
				case Windows.WM_ACTIVATE:
					m.Result = (IntPtr)Windows.MA_NOACTIVATE;
					return; // no call to base WndProc
			}
			base.WndProc(ref m);
		}

		#endregion

	}
}
