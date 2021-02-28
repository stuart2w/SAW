using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmMenu : Form
	{
		private string m_InitialFile = "";
		/// <summary>Whether initial file is opened in run mode (correct for Release builds, but helpful to go to design for development)</summary>
		// ReSharper disable once MemberInitializerValueIgnored
		private bool m_InitialIsRun = true;
		public bool StartedHidden { get; private set; }

		// Debug mod is in here >>>
		public frmMenu()
		{
			// This form is loaded when the application is started.  Therefore this constructor is effectively the application start point
			using (Graphics gr = this.CreateGraphics()) // must be before AM created
			{
				GUIUtilities.SystemDPI = (int)gr.DpiX;
				GUIUtilities.SystemDPIRelative = GUIUtilities.SystemDPI / 96f;
			}

			Globals.Root = new RootApplication(this); // assignment is actually redundant since constructor does the same internally
			Globals.Root.Speech = new Speech();

			// now start this screen...
			InitializeComponent();

			string[] aArguments = Environment.GetCommandLineArgs();
			if (aArguments.Length > 1)
			{
				m_InitialFile = aArguments[1];
				try
				{
					if (!System.IO.File.Exists(m_InitialFile))
						m_InitialFile = "";
				}
				catch (Exception ex)
				{
					Utilities.LogSubError("Error checking initial file: " + ex.Message);
					m_InitialFile = "";
				}
			}

#if DEBUG
			// can be used in development to start with an initial file...
			m_InitialFile = "d:\\data\\ace\\saw\\test data\\test.saw7";
			m_InitialIsRun = false;

			//var switchForm = new Switches.DevelopTest();
			//switchForm.Show();
#endif

			if (!string.IsNullOrEmpty(m_InitialFile))
				StartedHidden = true;

			if (StartedHidden)
			{
				this.WindowState = FormWindowState.Normal;
				this.StartPosition = FormStartPosition.Manual;
				this.Left = -20000;
				this.ShowInTaskbar = false; // because it doesn't seem to be hidden properly in this case.  but only if not initial file ??
			}


#if !DEBUG
			try
			{
				if (Config.SystemConfig.ReadSingle(Config.Delta_Applied) < SoftwareVersion.Version)
				{
					try
					{
						Globals.Root.LogPermanent.WriteLine("Applying deltas from: " + Config.SystemConfig.ReadSingle(Config.Delta_Applied) + " >> " + SoftwareVersion.Version);
						Config.Delta.ApplyDeltas(Globals.Root.InternalFolder + System.IO.Path.DirectorySeparatorChar + "deltas");
						Globals.Root.CleanUpdate();
					}
					catch (Exception ex)
					{
						Utilities.LogSubError(ex);
					}
					Config.SystemConfig.Write(Config.Delta_Applied, SoftwareVersion.Version);
					Globals.Root.SaveSystemConfig();
					Globals.Root.SaveUserConfigs();
				}
			}
			catch (Exception ex)
			{
				Globals.Root.LogPermanent.WriteLine("Failed config delta update: " + ex);
				MessageBox.Show("Configuration update failed: " + ex.Message);
			}
#endif
		}

		private void frmMenu_Load(object sender, EventArgs e)
		{
			Strings.Translate(this, ttMain);
			SetWindowTitle();
			try
			{
				if (!string.IsNullOrEmpty(m_InitialFile))
				{
					if (m_InitialIsRun)
					{
						Globals.Root.LoadDocumentFromExternal(m_InitialFile);
						Globals.Root.ShowRunScreen();
					}
					else
						Globals.Root.ShowEditScreen(Document.FromFile(m_InitialFile, false), true);
				}
				else
					Globals.Root.StartupCheck(NotifyUpdateAvailable); // no point doing this if we skip the screen!
			}
			catch (UserException exU) // this handler catches errors from the initial document selection
			{
				MessageBox.Show(Strings.Translate(exU.Message));
				Unhide();
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				Unhide();
#if DEBUG
				MessageBox.Show(ex.ToString());
#else
				frmErrorReport.ReportErrorOnce(frmErrorReport.OnceErrorContexts.Startup, ex);
#endif
			}
		}

		/// <summary>Callback invoked when the startup check finds there is an update available</summary>
		private void NotifyUpdateAvailable()
		{
			Strings.AutoLink(lnkUpdate);
			lnkUpdate.Visible = true;
			pctUpdate.Visible = true;
		}

		public void Redisplay()
		{
			Show();
		}

		private void Unhide()
		{
			if (StartedHidden)
			{
				StartedHidden = false;
				this.Left = 0;
				this.WindowState = FormWindowState.Normal;
				this.ShowInTaskbar = true;
			}
		}

		private void SetWindowTitle()
		{ // partly copied from main
			StringBuilder title = new StringBuilder();
			title.Append(Strings.Item("App"));
			title.Append(" (v").Append(SoftwareVersion.VersionString).Append(")");
			this.Text = title.ToString();
		}

		#region Start and delayed calls
		private List<Action> m_DelayedCalls = new List<Action>();

		internal void RequestDelayedCall(Action fn)
		{
			// Root.RequestDelayedCall calls through to here
			m_DelayedCalls.Add(fn);
			tmrTick.Enabled = true;
		}

		public void tmrTick_Tick(object sender, EventArgs e)
		{
			if (m_DelayedCalls.Count > 0) // avoid creating extra list objects if nothing actually needs doing
			{
				var colTemp = m_DelayedCalls;
				m_DelayedCalls = new List<Action>(); // by reassigning protects it against list mod, or anything like modal messages which would otherwise cause crashes
				foreach (Action fn in colTemp)
				{
					fn.Invoke();
				}
			}
			tmrTick.Enabled = false;
		}

		#endregion

		private void btnExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			string filename = FileDialog.ShowOpen(FileDialog.Context.Document);
			if (string.IsNullOrEmpty(filename))
				return;
			Globals.Root.OpenFile(filename);
			if (Globals.Root.CurrentDocument != null) // triggers may be Null if it was a SAW6 document and the user rejected it after warnings
				Globals.Root.ShowEditScreen(Globals.Root.CurrentDocument);
		}

		private void btnDesign_Click(object sender, EventArgs e)
		{
			Document document = new Document(false);
			SizeF sz = Globals.Root.CurrentConfig.ReadSize(Config.Page_Size, Config.Page_Size_Default);
			document.Page(0).SetSize(sz);
			document.ActivityID = Activities.SAW6;
			document.SetDefaultGridAndSnapFromActivity(); // calls InitialiseForSAW
			Point screenCentre = Screen.PrimaryScreen.WorkingArea.Centre();
			document.SAWHeader.SetWindowBounds(new RectangleF(screenCentre.X - sz.Width / 2, screenCentre.Y - sz.Height / 2, sz.Width, sz.Height).ToRectangle()); // easier to construct as a float rect then convert than to cast every single field
			Globals.Root.SelectDocument(document);
			Globals.Root.ShowEditScreen(Globals.Root.CurrentDocument);
		}

		private void lnkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Globals.Root.StartUpgrade();
		}
	}
}
