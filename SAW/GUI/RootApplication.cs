using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Media;
using System.Threading.Tasks;
using Microsoft.Win32;
using SAW.Repo2;

namespace SAW
{

	internal class RootApplication
	{

		/// <summary>main editing screen; only created when it is first opened</summary>
		public frmMain Editor { get; private set; }
		/// <summary>the menu screen, which opens first when the application is created</summary>
		internal frmMenu Menu;
		/// <summary>The run time, switching screen</summary>
		internal frmRun Run;

		/// <summary>must be suitable for use as a folder name, so some characters cannot be used</summary>
		public const string AppName = "SAW7";

		/// <summary>true if #DEBUG (easier to use in Catch When)</summary>
		public bool IsDebug = false;
		/// <summary>Log which is cleared each time the software is run.
		/// Initial NullLogger means that these won't crash if accessed before they are actually initialised, or in design-time code</summary>
		internal ILogger Log = new NullLogger();
		/// <summary>Semi-permanent log.  Don't write too much to this.</summary>
		internal ILogger LogPermanent = new NullLogger();
		internal SoundPlayer Sound = new SoundPlayer();
		public Speech Speech;

		public RootApplication(frmMenu frm)
		{
#if DEBUG
			IsDebug = true;
#endif
			Menu = frm;
			m_Documents.Add(null); // because the editor will just assign the current document into the current index

			// folders...
			EXEFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			InternalFolder = EXEFolder;

			SharedFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppName);
			ConfigFolder = SharedFolder;
			bool sharedCreated = false;
			// create shared folder and set permissions
			try
			{
				if (!System.IO.Directory.Exists(SharedFolder))
				{
					System.IO.Directory.CreateDirectory(SharedFolder);
					System.Security.AccessControl.DirectorySecurity dirSecurity = System.IO.Directory.GetAccessControl(SharedFolder);
					System.Security.Principal.SecurityIdentifier user = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinUsersSid, null);
					System.Security.AccessControl.FileSystemAccessRule newRule = new System.Security.AccessControl.FileSystemAccessRule(user, System.Security.AccessControl.FileSystemRights.FullControl,
						System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow);
					dirSecurity.AddAccessRule(newRule);
					System.IO.Directory.SetAccessControl(SharedFolder, dirSecurity);

					sharedCreated = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Cannot create required folder: " + SharedFolder + "\r\n" + "Error: " + ex);
				Application.Exit();
			}

#if DEBUG
			EXEFolder = "d:\\data\\ace\\SAW\\Installer\\installer files";
			InternalFolder = EXEFolder;
			// config certainly needs to write back to the installation folder
			ConfigFolder = "d:\\data\\ace\\SAW\\test data";
			SharedFolder = "d:\\data\\ace\\SAW\\test data";
#else
			// Temporary version using folder location for everything
			//SharedFolder = EXEFolder + System.IO.Path.DirectorySeparatorChar + "Data";
			//ConfigFolder = SharedFolder;
#endif

			Globals.Root = this;
			try
			{
				Log = new CLogFile(SharedFolder + System.IO.Path.DirectorySeparatorChar + "log.txt");
				LogPermanent = new CLogFile(SharedFolder + System.IO.Path.DirectorySeparatorChar + "permanent.txt", 100000);
				LogPermanent.TimeStampEx = TimeStamps.Date;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Could not create log files: " + ex.Message);
			}
			Log.WriteLine("Starting, version = " + SoftwareVersion.VersionString);
			Log.WriteLine("EXEFolder = " + EXEFolder);
			Log.WriteLine("SharedFolder = " + SharedFolder);
			Log.WriteLine("InternalFolder = " + InternalFolder);
			Log.WriteLine("OS = " + Environment.OSVersion);
			Log.WriteLine("64-bit = " + Environment.Is64BitOperatingSystem);
			Log.WriteLine("CLR = " + Environment.Version);
			Globals.CheckThemeOnStartup();

			if (sharedCreated)
			{
				// need to copy across from Splash 1 if applicable
				LogPermanent.WriteLine("Shared folder created");
			}

			Strings.Load();
			Log.WriteLine("SystemDPI = " + GUIUtilities.SystemDPI);


			Functions.Verb.CreateList(); // must before any configs loaded
			Functions.SAWVerbs.RegisterVerbs();

			// load configurations...
			Config.SystemConfig = LoadConfig("system" + Config.Extension, Config.Levels.System);
			Config.UserUser = LoadConfig("User" + Config.Extension, Config.Levels.User, "user_Default" + Config.Extension);
			Config.UserUser.EnsureUserResources();
			if (!System.IO.Directory.Exists(Activities.ActivityConfigFolder))
				System.IO.Directory.CreateDirectory(Activities.ActivityConfigFolder);
			// activities are now loaded as needed

#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += frmErrorReport.UnhandledException;
			Application.ThreadException += frmErrorReport.ThreadException;
#endif
			CurrentConfig = new AppliedConfig();
			CurrentConfig.AddConfigAtEnd(Config.SystemConfig);
			CurrentConfig.AddConfigWithPriority(Config.UserUser);
		}

		internal void CleanUpdate()
		{
			// called by frmMenu when it triggers deltas (which will be after an update)
			if (!System.IO.Directory.Exists(SharedFolder + System.IO.Path.DirectorySeparatorChar + "Update"))
				return;
			long total = 0;
			foreach (string file in System.IO.Directory.GetFiles(SharedFolder + System.IO.Path.DirectorySeparatorChar + "Update", "*.*"))
			{
				total += new System.IO.FileInfo(file).Length;
			}
			if (total > 30000000) // 30MB
			{
				// clean the folder out
				Globals.Root.LogPermanent.WriteLine("Update folder size " + (total / (1024 * 1024)).ToString("0.0") + "MB.  Cleaning");
				foreach (string file in System.IO.Directory.GetFiles(SharedFolder + System.IO.Path.DirectorySeparatorChar + "Update", "*.*"))
				{
					try
					{
						System.IO.File.Delete(file);
					}
					catch
					{
					}
				}
			}
		}

		#region Folders
		/// <summary>where the EXE is.  No trailing \.  Must be assumed to be readonly.  "Installer files" folder in DEBUG builds</summary>
		public string InternalFolder;
		/// <summary>shared data folder, writable by all users</summary>
		public string SharedFolder;
		/// <summary>folder where the configuration is stored.  Equals SharedFolder after installation, just different on a development machine (= "Installer Files")</summary>
		public string ConfigFolder;
		/// <summary>where other EXEs can be launched from. = InternalFolder on release.  = c:\program files... in development</summary>
		public string EXEFolder;
		public string ManualFolder
		{
			get
			{
#if DEBUG
				return "c:\\dropbox\\splash\\manuals\\release v2";
#else
				return InternalFolder + System.IO.Path.DirectorySeparatorChar + "manual";
#endif
			}
		}
		#endregion

		#region Screens and transferring
		private Timer m_tmrRefocus;
		/// <summary>if bolForceRefocus is used a timer checks the main window is actually focussed a second later
		/// this is needed if this is called during frmMenu_Load when it tends to end up with neither window focused</summary>
		public void ShowEditScreen(Document document, bool forceRefocus = false)
		{
			Globals.StoreEvent("ShowMainScreen bolForceRefocus=" + forceRefocus);
			if (Editor == null)
			{
				Editor = new frmMain();
				Globals.SetEditor(Editor);
			}
			User = Users.Editor;
			Run?.Stop(); // safe if not running
			Run?.Hide();
			m_DocumentIndex = 0;
			m_CurrentDocument = null; // sensible due to change in index, but also forces engine documentchanged event to fire which will pass this to main screen
			CurrentDocument = document; // Must be assigned before Editor.Display
			Editor.Display();
			Menu?.Hide();
			if (forceRefocus)
			{
				m_tmrRefocus = new Timer();
				m_tmrRefocus.Interval = 500;
				m_tmrRefocus.Enabled = true;
				m_tmrRefocus.Tick += Refocus;
			}
		}

		public void ShowRunScreen()
		{
			Editor?.Hide();
			Menu.Hide();
			if (Run == null)
				Run = new frmRun();
			Run.Show();
			Run.Start(CurrentDocument);
		}

		public void ShowMenu()
		{
			// caller should check that it was OK to discard any current document first
			Globals.StoreEvent("ShowMenu");
			if (User == Users.User)
				User = Users.Editor;
			Editor.LeavingScreen();
			Editor.Hide();
			Menu.Redisplay();
			Menu.BringToFront();
		}

		private void Refocus(object sender, EventArgs e)
		{
			m_tmrRefocus.Dispose();
			m_tmrRefocus = null;
			try
			{
				Editor.Activate();
				Editor.Focus();
				Editor.BringToFront();
			}
			catch
			{
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void ReopenMainScreen(Document document)
		{
			if (Editor != null)
			{
				Editor.Dispose();
				Editor = null;
				Globals.SetEditor(null);
			}
			ShowEditScreen(document);
		}

		public void MatchScreenSizes(Form old, Form newForm)
		{
			newForm.WindowState = old.WindowState;
			if (newForm.WindowState == FormWindowState.Minimized)
				newForm.WindowState = FormWindowState.Normal; // just in case!
			if (newForm.WindowState != FormWindowState.Maximized)
			{
				newForm.StartPosition = FormStartPosition.Manual;
				newForm.Bounds = old.Bounds;
			}
		}

		public bool Closing;
		public void CloseApplication()
		{
			// NOTE: this may be called more than once (once by Editor, again in frmMenu close event)
			Closing = true;
			Editor?.LeavingScreen();
			Application.Exit();
		}

		public Form CurrentMainScreen()
		{
			if (Run?.Visible == true)
				return Run;
			if (Editor?.Visible == true)
				return Editor;
			return Menu;
		}

		public void StartUpgrade()
		{
			// Called either from the link in the menu, or the menu on the main screen
			// caller should check first if it is OK to discard current documents (if any)
			frmUpdate frm = new frmUpdate();
			if (frm.ShowDialog() == DialogResult.OK)
				Application.Exit();
		}

		#endregion

		#region Configurations and Users
		public AppliedConfig CurrentConfig;
		/// <summary>Defined if the user has explicitly loaded/saved a settings file.  Must be null otherwise (not empty string)</summary>
		public string ManualSettingsFile;

		internal Config LoadConfig(string file, Config.Levels level, string defaultFile = "")
		{
			// defaultFile can be the name of the file used as a default - ie the one in InternalFolder which will be copied in.
			Debug.Assert(!file.StartsWith("\\"));
			if (level == Config.Levels.DocumentBoth || level == Config.Levels.ActivityBoth)
				throw new ArgumentException("LoadConfig (eLevel)");
			if (string.IsNullOrEmpty(defaultFile))
				defaultFile = file;
			string path = ConfigFolder + System.IO.Path.DirectorySeparatorChar + (level == Config.Levels.ActivityUser ? "Activities" + System.IO.Path.DirectorySeparatorChar : "");
			path += file;
			Log.WriteLine("Load config: " + path);
			if (System.IO.File.Exists(path))
			{
				try
				{
					Config config = Config.FromFile(path, level);
					if (level == Config.Levels.User)
						config.EnsureUserResources();
					return config;
				}
				catch (Exception ex) when (!IsDebug)
				{
					Globals.Root.LogPermanent.WriteLine("Failed to load configuration: " + file + ": " + ex.Message);
					Utilities.LogSubError(ex);
				}
			}
			try
			{
				path = InternalFolder + System.IO.Path.DirectorySeparatorChar + (level == Config.Levels.ActivityUser ? "Activities" + System.IO.Path.DirectorySeparatorChar : "") + defaultFile;
				if (System.IO.File.Exists(path))
				{
					// The default copies of these are installed by the installer, and they are then copied to the shared folder when first referenced
					Globals.Root.Log.WriteLine("Loading config from default at: " + path); // + ", code=" + (New StackTrace).ToString)
					Config config = Config.FromFile(path, level);
					if (level == Config.Levels.User)
					{
						// want to force ID change
						Mapping map = new Mapping();
						Document newDoc = (Document)config.Document.Clone(map);
						config = newDoc.UserSettings;
						if (level == Config.Levels.User)
							config.EnsureUserResources();
						newDoc.UpdateReferencesIDsChanged(map, newDoc);
					}
					// will be saved back to the config folder when it is saved
					return config;
				}
			}
			catch (Exception ex)
			{
				Globals.Root.LogPermanent.WriteLine("Failed to load DEFAULT configuration: " + file + ": " + ex.Message);
				Utilities.LogSubError(ex);
			}
			Config create = new Config();
			create.Document = new Document(true);
			create.Document.UserSettings = create;
			Utilities.LogSubError("Config not found (" + file + ").  Creating a blank one", true);
			if (level == Config.Levels.System)
				create.InitialiseAsDefaultSystem();
			if (level == Config.Levels.User)
				create.EnsureUserResources();
			create.AutoCreated = true;
			return create;
		}

		public Config GetCurrentConfig(Config.Levels level, Document document, Users user = Users.Undefined)
		{
			switch (level)
			{
				case Config.Levels.System:
					return Config.SystemConfig;
				case Config.Levels.ActivityUser:
					Document objActivity_1 = Activities.GetActivitySettings();
					return objActivity_1?.UserSettings;
				case Config.Levels.ActivityBoth:
					Document objActivity = Activities.GetActivitySettings();
					return objActivity?.BothSettings;
				case Config.Levels.DocumentUser:
					return document?.UserSettings;
				case Config.Levels.DocumentBoth:
					return document?.BothSettings;
				case Config.Levels.User:
					return Config.UserCurrent;
				default:
					Debug.Fail("Unexpected config level in AM.GetCurrentConfig");
					return null;
			}
		}

		public AppliedConfig GenerateAppliedConfig(Users user, Document document)
		{
			AppliedConfig create = new AppliedConfig();
			for (Config.Levels level = Config.Levels._First; level <= Config.Levels._Last; level++)
			{
				if (level != Config.Levels.DocumentUser && level != Config.Levels.ActivityUser || user == Users.User)
					create.AddConfigAtEnd(GetCurrentConfig(level, document, user));
			}
			return create;
		}

		public void SaveSystemConfig()
		{
			Config.SystemConfig.SaveTo(ConfigFolder + System.IO.Path.DirectorySeparatorChar + "system" + Config.Extension);
		}

		public void SaveUserConfigs()
		{
			// Saves all of the user configurations.
			// ignores config if .AutoCreated - this indicates it was loaded by error report display (not actually auto-generated)
			try
			{
#if DEBUG
				// for development machine - to avoid recent files getting added to the configuration which are used for the installation
				Config.UserUser.RecentFiles.Clear();
#endif
				// always saves to default location
				Config.UserUser.SaveTo(System.IO.Path.Combine(ConfigFolder, "User" + Config.Extension));
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
			}
		}

		public void SaveActivityConfig()
		{
			// this should be saved on exit from the config editing screen if it is changed
			Guid ID = Activities.ActivityID;
			if (ID.Equals(Guid.Empty))
				return;
			if (Config.ActivityConfig(ID) == null)
			{
				Utilities.LogSubError("Trying to save activity config (" + ID + "), but it is not loaded?!");
				return;
			}
			if (!Config.ActivityExists(ID, true))
			{
				Utilities.LogSubError("Cannot save activity config (" + ID + "), because it is an empty one which was automatically created");
				return;
			}
			Config.ActivityConfig(ID).UserSettings.SaveTo(ConfigFolder + System.IO.Path.DirectorySeparatorChar + "Activities" + System.IO.Path.DirectorySeparatorChar + ID + Config.Extension);
		}

		public void SaveAllActivityConfigs()
		{
			foreach (Document config in Config.ActivityConfigs())
			{
				config.UserSettings.SaveTo(ConfigFolder + System.IO.Path.DirectorySeparatorChar + "Activities" + System.IO.Path.DirectorySeparatorChar + config.ID + Config.Extension);
			}
		}

		public void AddNewActivity(Document newDoc)
		{
			Config.AddActivity(newDoc);
			newDoc.UserSettings.SaveTo(ConfigFolder + System.IO.Path.DirectorySeparatorChar + "Activities" + System.IO.Path.DirectorySeparatorChar + newDoc.ID + Config.Extension);
		}

		/// <summary>Where the user settings is stored - internally initially, but changed if the user explicitly opens/saves a file</summary>
		public string UserConfigPath
		{ get { return ManualSettingsFile ?? System.IO.Path.Combine(ConfigFolder, "User" + Config.Extension); } }

		public string CurrentUserDisplayname
		{ get { return Strings.Item("User"); } }

		#endregion

		#region Documents, CurrentDocument etc
		/// <summary>index of currently displayed within current document</summary>
		// support for multiple documents... (most of the GUI should just use CurrentDocument)
		private readonly List<Document> m_Documents = new List<Document>(); // There is always at least one entry, although it might be Nothing
		private int m_DocumentIndex = 0;
		private Document m_CurrentDocument;
		// Just equals m_Documents [m_DocumentIndex]; possibly not worth storing, but exists for historical reasons and does avoid a lot of function calls via CurrentDocument
		/// <summary>Fired whenever CurrentDocument changes; Engine.SettingsChanged is always raised after this, 
		/// but OnSettingsChanged is called first so that the correct configuration object is set globally during this event.
		/// Also followed by CurrentPageChanged in all cases</summary>
		public event NullEventHandler CurrentDocumentChanged;

		public Document Documents(int index) => m_Documents[index];
		public int DocumentsCount => m_Documents.Count;
		public int DocumentIndex => m_DocumentIndex;

		#region Functions which select a different document
		/// <summary>Assigning this opens a document at the current DocumentIndex, replacing the existing one </summary>
		public Document CurrentDocument
		{
			get { return m_CurrentDocument; }
			set
			{
				if (value == m_CurrentDocument)
					return;
				CloseDocument(m_CurrentDocument);
				SetCurrentDocument(value);
				m_Documents[m_DocumentIndex] = value;
				Globals.OnSettingsChanged(); // event is on timer, so comes after document
				CurrentDocumentChanged?.Invoke();
				CurrentPageChanged?.Invoke();
			}
		}

		public void LoadDocumentFromExternal(string file)
		{
			// Used when the user double clicks a file in Windows (comes from MyApplication_StartupNextInstance)
			try
			{
				if (Menu.Visible)
				{
					Document objDocument = Document.FromFile(file, true);
					// remembered because this is used in two places - opening a file double clicked in Windows (which should be remembered)
					// and from review resources (probably shouldn't be remembered, but only used by distributors)
					ShowEditScreen(objDocument);
				}
				else
					// if the edit is open we must ask nicely to show the document, in case it has an existing, unsaved document
					Editor.RequestLoadDocument(file);
			}
			catch (UserException)
			{
				throw; // In particular file too new uses a UserException which is reported tidily
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void AddNewDocument(Document document)
		{
			// adds a document to list and makes it the current one
			Debug.Assert(Globals.Root.CurrentConfig.ReadBoolean(Config.Multiple_Documents));
			if (m_CurrentDocument != null)
				m_CurrentDocument.CurrentPage = CurrentPageIndex; // remember current page in current doc
			m_Documents.Add(document);
			SetCurrentDocument(document);
			m_DocumentIndex = Globals.Root.m_Documents.Count - 1;
			Globals.OnSettingsChanged();
			CurrentDocumentChanged?.Invoke();
			CurrentPageChanged?.Invoke();
		}

		/// <summary>Selects one of the already open documents (in multiple document mode)</summary>
		public void SelectDocument(int index)
		{
			if (index < 0 || index >= m_Documents.Count)
			{
				Utilities.LogSubError("AM.SelectDocument - index out of bounds");
				return;
			}
			SetCurrentDocument(m_Documents[index]);
			m_DocumentIndex = index;
			Globals.OnSettingsChanged();
			CurrentDocumentChanged?.Invoke();
			CurrentPageChanged?.Invoke();
		}

		/// <summary>this version will select the given document, coping with it already being open, and deciding whether to open into a new tab (in multiple document mode)</summary>
		public void SelectDocument(Document document)
		{
			if (m_Documents.IndexOf(document) >= 0)
			{
				// Document is already open; select that tab
				SelectDocument(m_Documents.IndexOf(document));
			}
			else
			{
				bool asNewTab = false;
				if (Globals.Root.CurrentConfig.ReadBoolean(Config.Multiple_Documents) && m_CurrentDocument != null && (!m_CurrentDocument.IsEmpty || m_CurrentDocument.IsPaletteWithin || document.IsPaletteWithin))
				{
					// Last condition is needed because we don't want to replace a real document with a palette tab - a palette should always be in a new tab
					// penultimate condition is just because it looks a bit odd to automatically close the palette editor - better to treat these slightly differently
					asNewTab = true;
				}
				else
				{
					if (Editor != null && !Editor.CheckDiscardCurrent(false))
						return;
				}
				if (document.IsPaletteWithin)
					document.StartEditingPalette(); // reset to the design size
				if (asNewTab)                    // Open as a new tab
					AddNewDocument(document);
				else                    // Replace current document (either not in multi-document mode, or current document is empty and automatically discarded)
					CurrentDocument = document;
			}
		}

		/// <summary>Assigns m_CurrentDocument, and does other necessary bits</summary>
		private void SetCurrentDocument(Document document)
		{
			m_CurrentDocument = document;
			m_CurrentPageIndex = document?.CurrentPage ?? 0;
			if (document != null)
				GUIUtilities.MillimetreSize = GUIUtilities.SystemDPI * Geometry.MILLIMETRE / Geometry.INCH / document.ApproxUnitScale;
		}

		/// <summary>Opens the given Splash file either into a new slot, or in the current index.  Use ShellOpen for other files.
		/// See also Document.FromFile which does the actual loading</summary>
		public void OpenFile(string file, bool remember = true, bool noErrorUI = false)
		{
			frmErrorReport.SubErrors.Clear();
			Editor?.pnlView.ConcludeOngoing();
			try
			{
				var loaded = Document.FromFile(file, remember, noErrorUI);
				SelectDocument(loaded);
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				Log.WriteLine("Failed to load file: " + file);
				Utilities.LogSubError(ex);
				MessageBox.Show(Strings.Item("File_Cannot_Load").Replace("%0", ex.Message));
			}
		}

		#endregion

		public void ClearDocuments()
		{
			Globals.StoreEvent("ClearDocuments");
			foreach (Document document in m_Documents)
			{
				CloseDocument(document);
			}
			m_Documents.Clear();
			m_Documents.Add(null);
			m_CurrentDocument = null;
			m_CurrentPageIndex = 0;
			Globals.Root.m_DocumentIndex = 0;
			CurrentDocumentChanged?.Invoke();
			CurrentPageChanged?.Invoke();
		}

		public void RemoveDocument(int index)
		{
			Globals.StoreEvent("Remove document " + index + ", current = " + m_DocumentIndex);
			CloseDocument(m_Documents[index]);
			if (index == m_DocumentIndex)
			{
				// must change to another doc, this one is displayed
				if (m_Documents.Count == 1)
				{
					if (Editor != null && Editor.Visible)
						ShowMenu(); // No documents now available
				}
				else if (index < Globals.Root.DocumentsCount - 1) // Display the next one in the list if possible
					SelectDocument(index + 1);
				else
					Globals.Root.SelectDocument(Globals.Root.DocumentsCount - 2);
			}
			m_Documents.RemoveAt(index);
			if (m_DocumentIndex >= index)
				m_DocumentIndex -= 1; // doesn't change the document - this document has been renumbered by the removal of a previous one
		}

		private void CloseDocument(Document document)
		{
			// Does the actual close of a document.  Not necessarily used when application is shutting down
			// This does not check if this document is displayed; that is done by the above functions
			if (document == null)
				return;
			if (!document.IsPaletteWithin) // Don't want to dispose palettes just because we stop editing them!
			{
				// we need to check in case the document being closed contained any palettes which are open for editing, or displayed on-screen
				List<Datum> removedPalettes = new List<Datum>(); // List of all palette documents which are no longer valid; Will all be Document objects, type is just so that AddRange can be used...
				if (document.UserSettings != null)
					removedPalettes.AddRange(document.UserSettings.CustomPalettes.Values);
				if (document.BothSettings != null)
					removedPalettes.AddRange(document.BothSettings.CustomPalettes.Values);
				// Check if any are open for editing
				foreach (Document remove in removedPalettes)
				{
					int index = m_Documents.IndexOf(remove);
					if (index >= 0)
						RemoveDocument(index);
				}
				// And remove from the positioning system:
				foreach (Document objRemove in removedPalettes)
				{
					Palette.Deregister(objRemove.ID.ToString());
				}
				// This must be after the palette test above (which clears the config properties)
				document.Dispose();
			}
		}

		public bool DocumentIsOpen(Document document)
		{
			return m_Documents.Contains(document);
		}

		/// <summary>Always fires after CurrentDocumentChanged (if applicable)</summary>
		public event NullEventHandler CurrentPageChanged;
		private int m_CurrentPageIndex;

		public int CurrentPageIndex
		{
			get { return m_CurrentPageIndex; }
			set
			{
				m_CurrentPageIndex = value;
				CurrentPageChanged?.Invoke();
			}
		}

		public Page CurrentPage => m_CurrentDocument?.Page(CurrentPageIndex);

		#endregion

		public void RequestDelayedCall(Action fn)
		{
			// Any part of the application can use this to queue a method to be called imminently (usually because it cannot be called a
			// from the current location, e.g. during an iterator)
			Menu.RequestDelayedCall(fn);
		}

		#region User/Editor mode
		private Users m_eUser = Users.Editor;

		public Users User
		{
			get { return m_eUser; }
			set
			{
				if (value == m_eUser)
					return;
				Globals.StoreEvent("Set User=" + value);
				m_eUser = value;
				Globals.OnSettingsChanged();
				ButtonStyle.SetDefaultOnUserChange();
			}
		}

		#endregion

		#region global editing support

		public void StoreNewTransaction(Transaction transaction, bool autoRefresh = false)
		{
			if (Editor == null)
			{
				Debug.Fail("");
				return;
			}
			Editor.StoreNewTransaction(transaction, autoRefresh);
		}

		[DebuggerStepThrough()]
		public void PerformAction(Functions.Action action, EditableView.ClickPosition.Sources source = EditableView.ClickPosition.Sources.Irrelevant)
		{
			Editor.PerformAction(action, source);
		}
		#endregion

		#region ShellExecute

		internal void ShellOpen(string folder, Form source = null)
		{
			if (source == null)
				source = CurrentMainScreen();
			Windows.ShellExecute(source.Handle, 0, folder, "", "", 1);
		}

		internal void OpenURL(string URL, Form source)
		{
			if (!URL.ToLower().StartsWith("http://") && !URL.ToLower().StartsWith("https://"))
				return;
			Windows.ShellExecute(source.Handle, 0, URL, "", "", 0);
		}

		#endregion

		#region Start-up update check
		internal static bool UpdateAvailable;
		internal const string CONFIG_UPDATECHECK_STATE = "UpdateCheck_State";

		internal enum StartState
		{
			// we might store the status of the start-up check in the configuration, in case it fails critically
			NotStarted = 0,
			Started = 1,
			Complete = 2
		}

		internal void StartupCheck(Action callbackWhenAvailable)
		{
			// called by Menu.Load.  Main processing should be in a different thread
			try
			{
				StartState eState = (StartState)Config.SystemConfig.ReadInteger(CONFIG_UPDATECHECK_STATE);
				if (eState == StartState.Started)
				{
					Utilities.LogSubError("StartupCheck flagged as started.  Will not attempt this time");
					Config.SystemConfig.Write(CONFIG_UPDATECHECK_STATE, (int)StartState.NotStarted);
					return;
				}
				Config.SystemConfig.Write(CONFIG_UPDATECHECK_STATE, (int)StartState.Started);

				// do actual check in background
				Task.Run(() => DoStartupCheck(callbackWhenAvailable));
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
			}
			finally
			{
				Globals.Root.SaveSystemConfig();
			}
		}

		private void DoStartupCheck(Action callbackWhenAvailable)
		{
			try
			{
				Repo2SoapClient server = new Repo2SoapClient("Repo2Soap", Server.TechURL + "repo2.asmx");
				string latest = server.LastVersionB(Server.Software, Server.PRODUCT, SoftwareVersion.VersionString, "", false, Server.Language2, GetNetVersion(), GetMachineID());
				if (latest.StartsWith("w"))
					latest = latest.Substring(1); // ignore update-via-download flag;  all that's important is what version is available
												  // run the rest on UI thread
				Menu.Invoke(new Action(() =>
				{
					var parts = latest.Split('#');
#if !DEBUG
					for (int index = 1; index < parts.Length; index++)
					{
						// process any directives.They are in form "1.2.3#warn" - where each item between # is a command
						switch (parts[index].ToLower())
						{
							case "warn":
								const string SkipKey = "Skip_Upgrade_Message";
								var skip = Config.SystemConfig.ReadInteger(SkipKey, 0);
								if (skip > 0)
									Config.SystemConfig.Write(SkipKey, skip - 1);
								else
								{
									Config.SystemConfig.Write(SkipKey, 10);
									MessageBox.Show(Strings.Item("Update_Warn"));
								}
								SaveSystemConfig();
								break;
						}
					}
#endif
					latest = parts[0];

					if (string.IsNullOrEmpty(latest))
						return;
					float asNumber = SoftwareVersion.VersionNumberFromString(latest);
					UpdateAvailable = asNumber > SoftwareVersion.Version + 0.001; // see frmUpdate

					Config.SystemConfig.Write(CONFIG_UPDATECHECK_STATE, (int)StartState.Complete);
					Globals.Root.SaveSystemConfig();
					if (UpdateAvailable)
						callbackWhenAvailable.Invoke();
				}));
			}
			catch (Exception ex)
			{
				try
				{
#if !DEBUG
					Utilities.LogSubError(ex);
#endif
				}
				catch
				{
				}
			}
		}

		#endregion

		#region System info
		// modified from here: http://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx
		// ignores service packs, which I'm not interested in
		private static int g_NetVersion = -1; // value is cached once it has been calculated
		internal static int GetNetVersion()
		{
			if (g_NetVersion < 0)
			{
				using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\", false))
				{
					foreach (string versionKeyName in ndpKey.GetSubKeyNames())
					{
						if (!versionKeyName.StartsWith("v"))
							continue;
						RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
						string name = (string)versionKey.GetValue("Version", "");
						if (string.IsNullOrEmpty(name))
						{
							// v4 is in v4/client and v4/full
							// not detecting 4.5 (requires checking a different value inside this)
							RegistryKey subKey = versionKey.OpenSubKey("Client", false);
							if (subKey == null)
								continue;
							name = (string)subKey.GetValue("Version");
						}
						if (!string.IsNullOrEmpty(name))
						{
							Version version = new Version(name);
							int number = version.Major * 10 + version.Minor; // in the form "35"
							if (number > g_NetVersion)
								g_NetVersion = number;
						}
					}
				}

			}
			return g_NetVersion;
		}

		/// <summary>Not needed now</summary>
		internal static string GetMachineID()
		{
			return "";
		}

		#endregion

	}

	public enum Users
	{
		User = 0,
		Editor,
		Undefined = -1
	}




}
