using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SAW.Commands;
using SAW.Shapes;

namespace SAW
{
	public class Document : Datum, IDisposable
	{

		// A single Splash document, containing one or more pages.  This is what is stored in a data file

		#region Fields which are stored
		private List<Page> m_Pages = new List<Page>();
		internal Shape.SnapModes SnapMode = Shape.SnapModes.Off;
		public Config UserSettings; // settings which apply only in user mode
		public Config BothSettings;
		// settings which apply in BOTH user and editor mode

		// these only stored if explicitly set by the user...
		internal Shape.LineStyleC InitialLineStyle;
		internal Shape.FillStyleC InitialFillStyle;
		internal Shape.TextStyleC InitialTextStyle;

		/// <summary>Units in which coordinates stored in the document; must be pixels or millimetres at the moment</summary>
		public GraphicsUnit Units = GraphicsUnit.Pixel;

		/// <summary>If true the first item listed is rendered topmost (used in SAW)</summary>
		internal bool ReverseRenderOrder;

		/// <summary>(Was ActivityName) Intended for storage in the activity itself; NO LONGER copied to the derived document</summary>
		/// <remarks>Both Name and DisplayName are also used for palettes in PaletteTitle and PaletteDescription properties</remarks>
		internal string Name = "";
		/// <summary>Alternate version of Name which is used when displaying in run mode to the end user (not editor).
		/// E.g. Name might be "Infant Numbers" whereas the display version would be just "Numbers"</summary>
		/// <remarks>Both Name and DisplayName are also used for palettes in PaletteTitle and PaletteDescription properties</remarks>
		internal string DisplayName = "";

		/// <summary>Used with palettes.</summary>
		internal string SubTitle = "";

		#region SAW
		/// <summary>The default script for all normal items.  If an item has its own script that has an option determining whether this is run first </summary>
		public Scriptable DefaultItemScripts;
		/// <summary>The default script for all items marked as escape items.  If an item has its own script that has an option determining whether this is run first </summary>
		public Scriptable DefaultEscapeScripts;
		/// <summary>The default script for all group items - ie those containing sub-items.  If an item has its own script that has an option determining whether this is run first </summary>
		public Scriptable DefaultGroupScripts;
		/// <summary>Script run when the set is loaded/started in run mode</summary>
		public Script StartupScript;
		/// <summary>Optional and might not exist</summary>
		public SAW.Header SAWHeader;

		#endregion

		#endregion

		#region Fields not stored in data... (mostly public)
		// There are also some private fields in the regions below
		/// <summary>true if changed in memory since load </summary>
		private bool m_Changed;
		/// <summary>remembers where this was loaded from if anywhere</summary>
		public string Filename = "";

		/// <summary>not currently stored in the data, only used with multiple documents
		/// use AM.CurrentPageIndex for the current page in the GUI </summary>
		public int CurrentPage = 0; // 
		internal int UntitledIndex; // number to use for this document if calling it "Untitled N"
		internal static int NextUntitledIndex = 1; // above # for next new document created
		internal List<Transaction> UndoTransactions = new List<Transaction>(); // edits which have been made in the order that they occurred (last one is undone first)
		internal List<Transaction> RedoTransactions = new List<Transaction>();
		private bool m_ReferencesUpdated; // true once UpdateReferences has been called.  Only used for debugging
#if DEBUG
		internal int FileVersion; // binary version of file from which this was loaded
#endif

		public bool Changed
		{
			get { return m_Changed; }
			set
			{
				m_Changed = value;
				StateChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>Note this is invoked when Changed is assigned, even if it isn't a change.  This is deliberate as thelistener may need to refresh due to a name change</summary>
		public event EventHandler<EventArgs> StateChanged;

		#endregion

		#region Constructors, including shared functions

		public Document(bool empty)
		{
			if (!empty)
			{
				m_Pages.Add(new Page());
				m_ReferencesUpdated = true; // we are presumably making a new file
			}
			UntitledIndex = NextUntitledIndex; // OK to set this even if not Untitled; NextUntitledIndex only incremented by GUI if doing new doc
		}

		internal Document(IEnumerable<Page> pages)
		{
			// used when loading an AccessMaths 4 file
			m_Pages = pages.ToList();
			UntitledIndex = NextUntitledIndex; // OK to set this even if not Untitled; NextUntitledIndex only incremented by GUI if doing new doc
			m_ReferencesUpdated = true; // there are no references in an old version file
		}

		internal static Document FromReader(DataReader reader)
		{
			Document create = (Document)reader.ReadData(FileMarkers.Document);
			create.UpdateReferencesObjectsCreated(create, reader);
			return create;
		}

		/// <summary>Loads the document.  noErrorUI param is SAW only and suppresses any warning for an sss file load (this is used when it is loaded at run time</summary>
		public static Document FromFile(string file, bool remember, bool noErrorUI = false)
		{
			ConfigLevelHint = Config.Levels.DocumentUser;
			Document loaded;
			if (System.IO.Path.GetExtension(file).ToLower() == Document.SAW6Extension)
			{
				using (var op = new Globals.Operation("Load SAW6 file"))
				{
					using (var reader = new ArchiveReader(file))
					{
						SAW6Doc oldDocument = new SAW6Doc();
						oldDocument.Read(reader);
						loaded = reader.IntoDocument;
						loaded.InitialiseForSAW();
					}
					if (!noErrorUI && !op.ConfirmSuccess(true))
						return null;
				}
			}
			else
			{
				DataReader reader = new DataReader(file, FileMarkers.SAW); // will throw an exception if the file is too new
				loaded = (Document)reader.ReadData(FileMarkers.Document);
				if (reader.Version < 85 && !loaded.ActivityID.IsEmpty() && !loaded.IsPaletteWithin)
				{
					loaded.Name = ""; // Previously these stored the name of the activity from which it was derived; they are now used to store document names for automatically saved documents
					loaded.DisplayName = ""; // Cannot do this within Load as that would wipe the names of palettes
				}
				reader.Close();
				loaded.UpdateReferencesObjectsCreated(loaded, reader);
			}
			loaded.Filename = file;
			if (remember)
				Config.UserUser.RememberFile(file);
			return loaded;
		}

		/// <summary>Configures document for use in SAW, setting things in the Splash engine which are fixed/default differently in SAW.
		/// Called by SetDefaultGridAndSnapFromActivity</summary>
		public void InitialiseForSAW()
		{
			m_Pages[0].Paper = Paper.CreateNew(Paper.Papers.Square, m_Pages[0].Paper);
			m_Pages[0].Paper.Dotted = true;
			m_Pages[0].Paper.SetIntervals(5, 5);
			m_Pages[0].Paper.GridVisible = true;
			Units = GraphicsUnit.Pixel;
			ActivityID = Activities.SAW6;
			if (SAWHeader == null)
				SAWHeader = new Header() { Version = 6006 };
		}

		/// <summary>does initial settings for NEW (ie not loaded) document.  Also creates scripts</summary>
		public void SetDefaultsForNewDocument()
		{
			InitialiseForSAW();

			DefaultItemScripts = new Scriptable();
			DefaultItemScripts.VisitScript = new Script(new Command[] { new CmdHighlightItem(), new CmdSoundClick(),
				new CmdOutText( CmdOutText.Action.Say, CmdOutText.Source.SpeechText), new CmdDisplayPromptText() });
			DefaultItemScripts.SelectScript = new Script(new Command[] { new CmdNormalItem(), new CmdSoundClick(),
				new CmdOutText( CmdOutText.Action.Say, CmdOutText.Source.SpeechText), new CmdOutText( CmdOutText.Action.Send, CmdOutText.Source.OutputText) });
			DefaultItemScripts.NextScript = new Script(new Command[] { new CmdNormalItem() }, Script.VisitTarget.VisitTypes.Next);

			DefaultEscapeScripts = new Scriptable();
			DefaultEscapeScripts.VisitScript = new Script(new Command[] { new CmdHighlightItem(), new CmdSoundClick(),
				new CmdOutText( CmdOutText.Action.Say, CmdOutText.Source.SpeechText), new CmdDisplayPromptText() });
			DefaultEscapeScripts.SelectScript = new Script(new Command[] { new CmdNormalItem(), new CmdSoundClick() }, Script.VisitTarget.VisitTypes.Up);
			DefaultEscapeScripts.NextScript = new Script(new Command[] { new CmdNormalItem() }, Script.VisitTarget.VisitTypes.Next);

			DefaultGroupScripts = new Scriptable();
			DefaultGroupScripts.VisitScript = new Script(new Command[] { new CmdHighlightItem(), new CmdSoundClick(),
				new CmdOutText( CmdOutText.Action.Say, CmdOutText.Source.SpeechText), new CmdDisplayPromptText() });
			DefaultGroupScripts.SelectScript = new Script(new Command[] { new CmdNormalItem(), new CmdSoundClick() }, Script.VisitTarget.VisitTypes.Down);
			DefaultGroupScripts.NextScript = new Script(new Command[] { new CmdNormalItem() }, Script.VisitTarget.VisitTypes.Next);
		}

		#endregion

		#region Datum
		// Activity IDs have been moved into the activities region
		/// <summary>needed when converting OLD configurations prior to
		/// the split into editor and user separately.  We only want to perform split if this is a document or activity</summary>
		internal static Config.Levels ConfigLevelHint = Config.Levels.DocumentUser;

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			// Duplicate will leave some references shared stil (eg SharedImages).  Don't think there is a problem with these being shared in memory
			// or having same IDs in files - they are basically const anyway.  NOTE duplicate will only work with no pages (ie if this is for configs)
			// because duplicating a page is not really supported
			Document document = (Document)other;
			if (depth >= CopyDepth.Undo)
			{
				// the lists of pages and textures need to be backed up, but the  actual data objects don't need to be
				// they will be put in the transaction separately if they are changing
				m_Pages = new List<Page>();
				if (depth == CopyDepth.Duplicate)
				{
					foreach (Page page in document.m_Pages)
					{
						m_Pages.Add((Page)page.Clone(mapID));
					}
				}
				else
					m_Pages.AddRange(document.m_Pages);
				m_SharedResources = new DatumList();
				// The bitmaps themselves not duplicated, the other document has a reference to the same ones
				foreach (Datum image in document.m_SharedResources.Values)
				{
					m_SharedResources.Add(image.ID, image);
				}
				SnapMode = document.SnapMode;
				if (depth == CopyDepth.Duplicate && document.UserSettings != null)
				{
					UserSettings = (Config)document.UserSettings.Clone(mapID);
					UserSettings.Document = this;
				}
				else
					UserSettings = document.UserSettings; // if the settings themselves have been edited then they will be separately in the transaction
				if (depth == CopyDepth.Duplicate && document.BothSettings != null)
				{
					BothSettings = (Config)document.BothSettings.Clone(mapID);
					BothSettings.Document = this;
				}
				else
					BothSettings = document.BothSettings;
				ActivityIcon = document.ActivityIcon;
				ActivityID = document.ActivityID;
				Name = document.Name;
				DisplayName = document.DisplayName;
				Info = document.Info;
				InitialLineStyle = document.InitialLineStyle;
				InitialFillStyle = document.InitialFillStyle;
				InitialTextStyle = document.InitialTextStyle;
				Units = document.Units;
				m_SharedButtonStyles.Clear();
				foreach (ButtonStyle style in document.m_SharedButtonStyles.Values)
				{
					if (depth == CopyDepth.Duplicate)
						m_SharedButtonStyles.Add(style.Clone(mapID));
					else
						// no need to copy the ButtonStyle - it should be transacted separately if it was edited
						m_SharedButtonStyles.Add(style);
				}
				PaletteDesignSize = document.PaletteDesignSize;
				ReverseRenderOrder = document.ReverseRenderOrder;
				if (depth == CopyDepth.Duplicate)
				{
					DefaultEscapeScripts = document.DefaultEscapeScripts?.Clone(mapID) as Scriptable;
					DefaultGroupScripts = document.DefaultGroupScripts?.Clone(mapID) as Scriptable;
					DefaultItemScripts = document.DefaultItemScripts?.Clone(mapID) as Scriptable;
					StartupScript = document.StartupScript?.Clone();
				}
				else
				{
					DefaultEscapeScripts = document.DefaultEscapeScripts;
					DefaultGroupScripts = document.DefaultGroupScripts;
					DefaultItemScripts = document.DefaultItemScripts;
					StartupScript = document.StartupScript;
				}
				if (document.SAWHeader != null && SAWHeader == null)
					SAWHeader = new Header();
				SAWHeader?.CopyFrom(document.SAWHeader);
			}
		}

		// Load/Save are not shared since much of it is historical and not needed in SAW
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
#if DEBUG
			FileVersion = reader.Version;
#endif
			m_Pages.Clear();
			foreach (Page page in reader.ReadDataList(FileMarkers.Page))
			{
				m_Pages.Add(page);
			}
			foreach (SharedBase shared in reader.ReadDataList()) // cannot specify SharedBitmap as they might be SharedResource
			{
				m_SharedResources.Add(shared);
				//Debug.WriteLine("Loaded shared object: " + shared.ID);
			}
			SnapMode = (Shape.SnapModes)reader.ReadByte();
			UserSettings = (Config)reader.ReadData(FileMarkers.Config);
			if (UserSettings != null)
				UserSettings.Document = this;
			BothSettings = (Config)reader.ReadData(FileMarkers.Config);
			if (BothSettings != null)
			{
				BothSettings.Document = this;
			}

			reader.ReadInt32(); // protection
			reader.ReadString(); // password
			Info.Load(reader);
			ActivityID = reader.ReadGuid();
			Name = reader.ReadString();
			ActivityIcon = reader.ReadOptionalMemoryImage();
			reader.ReadBoolean(); // DerivedFromWorksheet
			bool hasStyleDefaults = reader.ReadBoolean();
			if (hasStyleDefaults)
			{
				InitialLineStyle = Shape.LineStyleC.Read(reader);
				InitialFillStyle = Shape.FillStyleC.Read(reader);
				InitialTextStyle = Shape.TextStyleC.Read(reader);
			}
			else
			{
				InitialLineStyle = null;
				InitialFillStyle = null;
				InitialTextStyle = null;
			}
			if (reader.Version < 120)
				throw new Exception("File versions <120 not applicable to SAW");
			DisplayName = reader.ReadString();
			m_SharedButtonStyles.Add(reader.ReadDataList(FileMarkers.ButtonStyle));
			if (hasStyleDefaults)
			{ // was marker style.  Redundant in SAW
				reader.ReadByte();
				reader.ReadByte();
			}
			PaletteDesignSize = reader.ReadSizeF();
			SubTitle = reader.ReadString();
			Units = (GraphicsUnit)reader.ReadInt32();
			if (reader.Version >= 121)
			{
				ReverseRenderOrder = reader.ReadBoolean();
				foreach (Page p in m_Pages)
					p.ReverseRenderOrder = ReverseRenderOrder;
			}
			if (reader.Version >= 122)
			{
				DefaultEscapeScripts = (Scriptable)reader.ReadData();
				DefaultGroupScripts = (Scriptable)reader.ReadData();
				DefaultItemScripts = (Scriptable)reader.ReadData();
				if (reader.ReadBoolean())
				{
					StartupScript = new Script();
					StartupScript.Read(reader);
				}
				if (reader.ReadBoolean())
				{
					SAWHeader = new Header();
					SAWHeader.Read(reader);
				}
			}
		}

		protected internal override void Save(DataWriter writer)
		{
			Debug.Assert(m_ReferencesUpdated,
				"Document.Save: m_ReferencesUpdated = false: it does not appear that UpdateReferencesObjectsCreated was called when the document was loaded" + "\r\n" +
				"IGNORE this when creating activity from document");
			TidySharedResources();
			base.Save(writer);
			writer.WriteDatumList(m_Pages);
			//foreach (var shared in m_SharedResources)
			//	Debug.WriteLine("Write shared: " + shared);
			writer.Write(m_SharedResources);
			writer.WriteByte((byte)SnapMode);
			writer.Write(UserSettings); // can be nothing
			writer.Write(BothSettings);
			writer.Write(0); // protection mode
			writer.Write(""); // password
			Info.Save(writer);
			writer.Write(ActivityID);
			writer.Write(Name);
			writer.WriteOptionalMemoryImage(ActivityIcon);
			writer.Write(false);
			bool defaults = InitialLineStyle != null && InitialFillStyle != null && InitialTextStyle != null;
			// they must all be defined or the code would crash below
			writer.Write(defaults);
			if (defaults)
			{
				InitialLineStyle.Write(writer);
				InitialFillStyle.Write(writer);
				InitialTextStyle.Write(writer);
			}
			writer.Write(DisplayName);
			writer.Write(m_SharedButtonStyles);
			if (defaults)
			{
				writer.WriteByte(0); // these 2 bytes were marker style.  Redundant in SAW
				writer.WriteByte(0);
				//writer.Write(0);
			}
			writer.Write(PaletteDesignSize);
			writer.Write(SubTitle);
			writer.Write((int)Units);
			if (writer.Version >= 121)
				writer.Write(ReverseRenderOrder);
			if (writer.Version >= 122)
			{
				writer.Write(DefaultEscapeScripts);
				writer.Write(DefaultGroupScripts);
				writer.Write(DefaultItemScripts);
				writer.Write(StartupScript != null);
				StartupScript?.Write(writer);
				writer.Write(SAWHeader != null);
				SAWHeader?.Write(writer);
			}
		}

		protected internal override byte TypeByte
		{ get { return (byte)FileMarkers.Document; } }

		protected internal override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			foreach (SharedBase image in m_SharedResources.Values)
			{
				image.UpdateReferencesObjectsCreated(document, reader);
			}
			foreach (Page page in m_Pages)
			{
				page.UpdateReferencesObjectsCreated(document, reader);
			}
			foreach (ButtonStyle style in m_SharedButtonStyles.Values)
			{
				style.UpdateReferencesObjectsCreated(document, reader);
			}
			UserSettings?.UpdateReferencesObjectsCreated(document, reader);
			BothSettings?.UpdateReferencesObjectsCreated(document, reader);
			DefaultEscapeScripts?.UpdateReferencesObjectsCreated(document, reader);
			DefaultItemScripts?.UpdateReferencesObjectsCreated(document, reader);
			DefaultGroupScripts?.UpdateReferencesObjectsCreated(document, reader);
			m_ReferencesUpdated = true;
		}

		protected internal override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			foreach (SharedImage image in m_SharedResources.Values)
			{
				image.UpdateReferencesIDsChanged(mapID, document);
			}
			foreach (Page page in m_Pages)
			{
				page.UpdateReferencesIDsChanged(mapID, document);
			}
			foreach (ButtonStyle style in m_SharedButtonStyles.Values)
			{
				style.UpdateReferencesIDsChanged(mapID, document);
			}
			UserSettings?.UpdateReferencesIDsChanged(mapID, document);
			BothSettings?.UpdateReferencesIDsChanged(mapID, document);
			DefaultEscapeScripts?.UpdateReferencesIDsChanged(mapID, document);
			DefaultItemScripts?.UpdateReferencesIDsChanged(mapID, document);
			DefaultGroupScripts?.UpdateReferencesIDsChanged(mapID, document);
			m_ReferencesUpdated = true;
		}

		internal override void Iterate(DatumFunction fn)
		{
			IterateEx(fn, true, true);
		}

		protected internal void IterateEx(DatumFunction fn, bool includeSharedBitmaps, bool includeSharedButtonStyles)
		{
			// for the document we need a version where we can omit the shared images, because when tidying bitmaps we use this function,
			// but only want to detect the images actually referenced elsewhere!
			// Note that this only works when called directly on self
			base.Iterate(fn);
			if (includeSharedBitmaps)
			{
				foreach (SharedImage image in m_SharedResources.Values)
				{ image.Iterate(fn); }
			}
			if (includeSharedButtonStyles)
			{
				foreach (ButtonStyle style in m_SharedButtonStyles.Values)
				{ style.Iterate(fn); }
			}
			foreach (Page page in m_Pages)
			{ page.Iterate(fn); }
			UserSettings?.Iterate(fn);
			BothSettings?.Iterate(fn);
			DefaultEscapeScripts?.Iterate(fn);
			DefaultGroupScripts?.Iterate(fn);
			DefaultItemScripts?.Iterate(fn);
		}

		public override bool IdenticalTo(Datum other)
		{
			// only checks values which would be stored in data
			Document document = (Document)other;
			if (SnapMode != document.SnapMode)
				return false;
			// i think we need to ignore the case where one of them is null (when making palette deltas getting this a bit)
			if (UserSettings != null && document.UserSettings != null && !UserSettings.IdenticalTo(document.UserSettings))
				return false;
			if (BothSettings != null && document.BothSettings != null && !BothSettings.IdenticalTo(document.BothSettings))
				return false;
			if (InitialFillStyle != null && !InitialFillStyle.IdenticalTo(document.InitialFillStyle))
				return false;
			if (InitialLineStyle != null && !InitialLineStyle.IdenticalTo(document.InitialLineStyle))
				return false;
			if (InitialTextStyle != null && !InitialTextStyle.IdenticalTo(document.InitialTextStyle))
				return false;
			if (!Info.Equals(document.Info))
				return false;
			if (DisplayName != document.DisplayName)
				return false;
			if (Name != document.Name)
				return false;
			if (SubTitle != document.SubTitle)
				return false;
			if (!ActivityID.Equals(document.ActivityID))
				return false;
			if (ActivityIcon != null && ActivityIcon.CalcCRC() != document.ActivityIcon.CalcCRC())
				return false;
			if (m_Pages.Count != document.m_Pages.Count)
				return false;
			for (int index = 0; index <= m_Pages.Count - 1; index++)
			{
				if (!m_Pages[index].IdenticalTo(document.m_Pages[index]))
					return false;
			}
			if (!DefaultEscapeScripts?.IdenticalTo(document.DefaultEscapeScripts) ?? true)
				return false;
			if (!DefaultGroupScripts?.IdenticalTo(document.DefaultGroupScripts) ?? true)
				return false;
			if (!DefaultItemScripts?.IdenticalTo(document.DefaultItemScripts) ?? true)
				return false;
			if (!StartupScript?.IdenticalTo(document.StartupScript) ?? true)
				return false;
			return true;
		}

		#endregion

		#region List of pages

		/// <summary>The number of pages in the document</summary>
		public int Count => m_Pages.Count;

		/// <summary>Returns one of the pages.  Note: first page is numbered 0 </summary>
		public Page Page(int index) => m_Pages[index];

		public int IndexOf(Page page) => m_Pages.IndexOf(page);

		/// <summary>source is the page to copy grid settings etc from (i.e. the currently displayed page).  If duplicate all content is also duplicated
		/// interactive should be true if this is UI triggered and can produce UI (question about background images)</summary>
		public Page AddPage(Page source, Transaction transaction, bool interactive, int atIndex = -1, bool duplicate = false)
		{
			Page create;
			if (!duplicate)
			{
				create = new Page(source);
				if (interactive && source.BackgroundImage != null)
				{
					var sz = source.BackgroundImage.GetSize(64);
					if (sz.Width > 100 && sz.Height > 100 && source.BackgroundImageMode != SAW.Page.BackgroundImageModes.Tile)
					{
						if (!frmNewPageBackground.Ask(source, create, transaction))
							return null;
					}
				}
			}
			else
			{
				Mapping mapID = new Mapping();
				create = (Page)source.Clone(mapID);
				DatumList ignore = new DatumList(); // We need to use the function below to make sure the mappings list is complete; it also updates a simple list of objects, which we don't actually need
				source.AddRequiredReferencesRecurseToContained(ignore.Add, mapID);
				create.UpdateReferencesIDsChanged(mapID, Globals.Root.CurrentDocument);
			}
			create.ReverseRenderOrder = ReverseRenderOrder;
			transaction.Edit(this);
			transaction.Create(create);
			transaction.Create(create.Paper);
			if (atIndex < 0 || atIndex >= m_Pages.Count)
				m_Pages.Add(create);
			else
				m_Pages.Insert(atIndex, create);
			return create;
		}

		internal void MovePage(int pageIndex, int before, Transaction transaction)
		{
			// to move to the end of intBefore = Count (i.e. before a non-existent page)
			if (pageIndex == before)
				return;
			transaction.Edit(this);
			transaction.IsPageSizeChange = true; // not actually a page size; however it will force ctrPages to completely refresh
			Page page = m_Pages[pageIndex];
			m_Pages.RemoveAt(pageIndex);
			if (before > pageIndex)
				before -= 1;
			m_Pages.Insert(before, page);
		}

		public void DeletePage(int index, Transaction transaction)
		{
			if (index < 0 || index >= m_Pages.Count)
				return;
			transaction?.Edit(this);
			transaction?.Delete(m_Pages[index]);
			// I don't think it is necessary to put the contents of the page into the transaction, because the shapes will still be referenced by the page object in the transaction
			m_Pages.RemoveAt(index);
		}

		/// <summary>All the contained pages for use in foreach() </summary>
		public IEnumerable<Page> Pages => m_Pages.AsReadOnly();

		#endregion

		#region Shared bitmaps and textures

		private DatumList m_SharedResources = new DatumList();

		/// <summary>Loads an image into the document for use in buttons within the document.
		/// Media is all stored centrally the document.  If the same item is loaded more than once only one copy is kept.</summary>
		/// <remarks>Calling this again for an image that is already loaded will return the existing copy</remarks>
		public SharedImage AddImageFromFile(string file) => (SharedImage)AddSharedResourceFromFile(file);

		/// <summary>Loads a sound file into the document for use in buttons within the document.
		/// Media is all stored centrally the document.  If the same item is loaded more than once only one copy is kept.</summary>
		/// <remarks>Calling this again for a sound that is already loaded will return the existing copy</remarks>
		public SharedResource AddSoundFromFile(string file) => (SharedResource)AddSharedResourceFromFile(file, null, false);

		/// <summary>Loads an image or sound into the document for use in buttons within the document.
		/// Media is all stored centrally the document.  If the same item is loaded more than once only one copy is kept.</summary>
		/// <param name="file">The filename containing the image or sound</param>
		/// <param name="transaction">Optional.  Only required if it is necessary to be able to undo these changes</param>
		/// <param name="isImage">Defaults to true.  Parameter must be included and false if loading any other resource</param>
		/// <returns>Returns a SharedImage or SharedResource</returns>
		internal SharedBase AddSharedResourceFromFile(string file, Transaction transaction = null, bool isImage = true)
		{
			if (string.IsNullOrEmpty(file))
				return null;
			int CRC = CRCCalc.Calc(file);
			SharedBase existing = FindSharedResourceByCRC<SharedBase>(CRC);
			if (existing != null)
				return existing;
			transaction?.Edit(this);
			SharedBase create;
			if (isImage)
				create = SharedImage.CreateFromFile(file, CRC);
			else
				create = SharedResource.CreateFromFile(file, CRC);
			m_SharedResources.Add(create);
			transaction?.Create(create);
			return create;
		}

		/// <summary>Extension must be provided IF AND ONLY IF it is not an image.</summary>
		internal SharedBase AddSharedResourceFromStream(System.IO.Stream stream, Transaction transaction, string fileExtensionIfNotImage = "", bool imageIsSVG = false)
		{
			int CRC = CRCCalc.Calc(stream);
			SharedBase existing = FindSharedResourceByCRC<SharedBase>(CRC);
			if (existing != null)
				return existing;
			transaction.Edit(this);
			SharedBase create;
			if (string.IsNullOrEmpty(fileExtensionIfNotImage))
				create = SharedImage.CreateFromStream(stream, imageIsSVG, CRC);
			else
				create = SharedResource.CreateFromStream(stream, fileExtensionIfNotImage);
			m_SharedResources.Add(create);
			transaction.Create(create);
			return create;
		}

		internal T FindSharedResourceByCRC<T>(int CRC) where T : SharedBase
		{
			return m_SharedResources.Values.OfType<T>().FirstOrDefault(x => x.CRC == CRC);
		}

		/// <summary>Return type will also be ISharedResource, but defining as Datum makes the ID accessible</summary>
		internal T FindExistingSharedResource<T>(Guid id) where T : Datum
		{
			if (m_SharedResources.ContainsKey(id))
				return (T)m_SharedResources[id];
			return null;
		}

		/// <summary>Returns the shared instance to use for the given memory data.  Can cope with transaction = null, but in this case undo wouldn't remove the image again</summary>
		internal SharedImage AddSharedBitmapFromMemoryImage(MemoryImage image, Transaction transaction)
		{
			int CRC = image.CalcCRC();
			SharedImage existing = FindSharedResourceByCRC<SharedImage>(CRC);
			if (existing != null)
				return existing;
			transaction?.Edit(this);
			SharedImage create = new SharedImage(image, CRC);
			m_SharedResources.Add(create);
			transaction?.Create(create);
			return create;
		}

		internal SharedImage AddSharedBitmapForResource(string id, Transaction transaction)
		{
			int CRC = CRCCalc.Calc(Encoding.Unicode.GetBytes(id));
			SharedImage existing = FindSharedResourceByCRC<SharedImage>(CRC);
			if (existing != null)
				return existing;
			transaction.Edit(this);
			SharedImage create = SharedImage.CreateForResource(id);
			m_SharedResources.Add(create);
			transaction.Create(create);
			return create;
		}

		/// <summary>Adds the object if not already in the shared resources, returning the copy to use (the param if not in resources, or the canonical object of that ID from the shared resources if the ID existed)</summary>
		internal T AddSharedResource<T>(T resource) where T : SharedBase
		{
			if (!m_SharedResources.Contains(resource))
				m_SharedResources.Add(resource);
			return resource;
		}

		internal void TidySharedResources()
		{
			// doesn't count as a transactional change, because this is in theory just tidying up missing links which should already have been included
			// now called by Save, to ensure we do all places where documents are saved (e.g. configuration documents)
			DatumList listNeeded = new DatumList();
			this.IterateEx(obj => obj.AddRequiredReferences(listNeeded.Add, Mapping.Ignore), false, true);
			List<Datum> listRemove = new List<Datum>(); // images which can be removed as they are no longer referenced
			foreach (var test in m_SharedResources.Values)
			{
				if (!listNeeded.Contains(test))
					listRemove.Add(test);
			}
			foreach (var data in listRemove)
			{
				m_SharedResources.Remove(data);
			}
			// We now also need to check for any shared bitmaps not already listed in this document.  ImportedImage automatically generates these
			// but because it is done within the shape object (e.g. when loading an old file) it can't necessarily notify the document - so there
			// can be some missing in m_colSharedImages as well as extras
			foreach (SharedImage needed in listNeeded.Values.OfType<SharedImage>())
			{
				if (!m_SharedResources.Contains(needed))
				{
					// But they should now be fewer of these, because the Stamp works correctly
					Debug.WriteLine("Detected shared image which was not yet in the document");
					m_SharedResources.Add(needed);
				}
			}
		}

		internal DatumList TexturesUsed()
		{
			// returns all textures used by shapes in doc
			DatumList list = new DatumList();
			foreach (Page page in m_Pages)
			{
				foreach (Shape shape in Shape.FlattenList(page.Shapes.ToList(), Shape.FlatListPurpose.HasTexture))
				{
					SharedImage texture = ((Filled)shape).FillStyle.Texture;
					if (texture != null)
					{
						if (!list.Contains(texture))
							list.Add(texture);
					}
				}
			}
			return list;
		}

		#endregion

		#region Shared button styles

		private readonly DatumList m_SharedButtonStyles = new DatumList();

		internal ButtonStyle GetButtonStyle(Guid id)
		{
			if (ButtonStyle.UserDefaultByID(id) != null)
				return ButtonStyle.UserDefaultByID(id);
			if (m_SharedButtonStyles.ContainsKey(id))
				return (ButtonStyle)m_SharedButtonStyles[id];
			return null;
		}

		internal IEnumerable<Datum> SharedButtonStyles()
		{
			return m_SharedButtonStyles.Values;
		}

		internal void AddButtonStyle(ButtonStyle style)
		{
			Debug.Assert(style.IsShared);
			if (m_SharedButtonStyles.Contains(style))
				Utilities.LogSubError("Adding shared style to document - already exists: " + style.ID);
			else
				m_SharedButtonStyles.Add(style);
		}

		internal void RemoveButtonStyle(ButtonStyle style)
		{
			Debug.Assert(!style.ID.IsEmpty(), "Trying to remove user default style from document!");
			if (m_SharedButtonStyles.Contains(style))
				m_SharedButtonStyles.Remove(style);
			style.IsShared = false;
		}

		internal ButtonStyle FindSharedButtonStyleByName(string name)
		{
			foreach (ButtonStyle style in m_SharedButtonStyles.Values)
			{
				if (string.Compare(style.Name, name, true) == 0 || string.Compare(Strings.Translate(style.Name), name, true) == 0)
					return style;
				// second condition is because we might have standard styles which are translatable at some point
			}
			return null;
		}

		#endregion

		#region IDisposable Support
		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			// effectively only done by AM.CloseDocument (aside from some temporary docs: prompt in frmMain and Worksheet folder scanning)
			if (!this.disposedValue)
			{
				if (disposing)
				{
					foreach (Page page in m_Pages)
					{
						page.Dispose();
					}
				}
			}
			this.disposedValue = true;
		}

		public void Dispose()
		{
			//Globals.Root.Log.WriteLine("Dispose document: " + ID);
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed => m_Pages.Count == 0 || m_Pages[0].IsDisposed;

		#endregion

		#region Activities
		// IDs now in Activities class

		/// <summary>This is always a bitmap and never a WMF or SVG </summary>
		internal MemoryImage ActivityIcon;

		internal Guid ActivityID { get; set; } = Guid.Empty;

		#endregion

		#region Settings

		public Config GetCreateUserSettings(Transaction transaction = null)
		{
			if (UserSettings == null)
			{
				if (transaction != null)
				{
					if (transaction.Empty)
						transaction.DocumentNotChanged = true; // prevents spurious save changes? message when editing (eg user) config via blank document
					transaction.Edit(this);
				}
				UserSettings = new Config() { Document = this };
				transaction?.Create(UserSettings);
			}
			return UserSettings;
		}

		public Config GetCreateBothSettings(Transaction transaction = null)
		{
			if (BothSettings == null)
			{
				if (transaction != null)
				{
					if (transaction.Empty)
						transaction.DocumentNotChanged = true; // prevents spurious save changes? message when editing (eg user) config via blank document
					transaction.Edit(this);
				}
				BothSettings = new Config() { Document = this };
				transaction?.Create(BothSettings);
			}
			return BothSettings;
		}

		internal void NotifySettingsChanged(Shape.EnvironmentChanges change)
		{
			// should be passed on down through the hierarchy of shapes
			foreach (Page page in m_Pages)
			{
				page.NotifySettingsChanged(change);
			}
		}

		#endregion

		/// <summary>Applies the necessary scaling to convert from document units to output pixels.  Graphics is needed to determine actual size of output pixel (ie DPI)</summary>
		public void ApplyUnitScale(Graphics gr)
		{
			switch (Units)
			{
				case GraphicsUnit.Millimeter:
					gr.ScaleTransform(gr.DpiX / Geometry.INCH, gr.DpiY / Geometry.INCH);
					break;
				case GraphicsUnit.Pixel:
					break;
				default:
					throw new InvalidOperationException("Unknown document units: " + Units);
			}
		}

		/// <summary>Applies the necessary scaling to convert from document units to output pixels.  DPI is needed to determine actual size of output pixel</summary>
		public void ApplyUnitScale(ref SizeF sz, float dpiX, float dpiY)
		{
			switch (Units)
			{
				case GraphicsUnit.Millimeter:
					sz.Width *= dpiX / Geometry.INCH;
					sz.Height *= dpiY / Geometry.INCH;
					break;
				case GraphicsUnit.Pixel:
					break;
				default:
					throw new InvalidOperationException("Unknown document units: " + Units);
			}
		}

		/// <summary>Ideally use ApplyUnitScale which does X and Y separately</summary>
		public float ApproxUnitScale
		{
			get
			{
				switch (Units)
				{
					case GraphicsUnit.Millimeter:
						return GUIUtilities.SystemDPI / Geometry.INCH;
					case GraphicsUnit.Pixel:
						return 1;
					default:
						throw new InvalidOperationException("Unknown document units: " + Units);
				}
			}
		}

		#region Saving, filenames and information

		public void Save(string file = null)
		{
			if (!string.IsNullOrEmpty(file))
				Filename = file;
			if (string.IsNullOrEmpty(Filename))
				throw new ArgumentException("file must be provided to Save() if document does not already have filename set");
			Bitmap thumbnail = Page(0).GenerateThumbnail2(SAW.Page.FILEDOCUMENTTHUMBNAILSIZE, ApproxUnitScale, 96);
			using (DataWriter writer = new DataWriter(Filename, FileMarkers.SAW, thumbnail: thumbnail))
			{
				writer.Write(this);
			}
		}

		internal const string WorksheetExtension = ".saw7";
		internal const string StandardExtension = ".saw7";
		internal const string SAW6Extension = ".sss";

		/// <summary>returns true if this was loaded from a worksheet file, and therefore should not be saved back over the same file.
		/// Saving will do a SaveAs, which COULD overwrite the original, but only if the user explicitly does so</summary>
		public bool NoSaveOverOriginalFile
		{
			get { return Filename.EndsWith(Document.SAW6Extension, StringComparison.InvariantCultureIgnoreCase); }
		}

		internal static string LoadPattern()
		{
			//; separated list of possible file extensions, can be used when building the filter (see below)
			return "*" + StandardExtension + ";*.sss"; // +";*" + WorksheetExtension
		}

		internal static string LoadFilter()
		{
			// they value which can be given to OpenDialog
			return "SAW file (" + LoadPattern() + ")|" + LoadPattern() + "|SVG(*.svg)|*.svg";
		}

		public string FilenameWithExtension(string extension)
		{
			// returns a filename, but with the extension modified if necessary
			if (string.IsNullOrEmpty(Filename))
				return "";
			Debug.Assert(extension.ToLower() == extension);
			if (System.IO.Path.GetExtension(Filename).ToLower() != extension)
				return Filename.Substring(0, Filename.Length - System.IO.Path.GetExtension(Filename).Length) + extension;
			else
				return Filename;
		}

		/// <summary>True if this is any extension used for splash user documents (excluding in total); parameter should be extensions in the way that IO.Path does them</summary>
		internal static bool IsSplashExtension(string extension)
		{
			Debug.Assert(string.IsNullOrEmpty(extension) || extension.StartsWith("."));
			return string.Compare(extension, StandardExtension, true) == 0 || string.Compare(extension, WorksheetExtension, true) == 0;
		}

		/// <summary>Leaf name used in title bar/tabs.  May be blank</summary>
		internal string FileTitle()
		{
			if (!string.IsNullOrEmpty(DisplayName))
				return DisplayName; // will be original worksheet name if set;  or palette name
			 if (!string.IsNullOrEmpty(Filename))
				return System.IO.Path.GetFileNameWithoutExtension(Filename);
			 return Strings.Item("New_Document").Replace("%0", UntitledIndex.ToString());
		}

		#endregion

		/// <summary>True if contains no content.  NOT VALID FOR NON-STANDARD DOCS (eg configs)</summary>
		public bool IsEmpty
		{
			get
			{
				if (m_Pages.Count == 0)
					return true;
				return m_Pages.Count == 1 && m_Pages[0].IsEmpty;
			}
		}

		/// <summary>extracts the preview from the file on disk</summary>
		internal static Image GetFilePreview(string file)
		{
			try
			{
				if (IsSplashExtension(System.IO.Path.GetExtension(file)))
				{
					using (DataReader reader = new DataReader(file, FileMarkers.Undetermined))
					{
						var imageResult = reader.Thumbnail; // which may be Nothing
						reader.Thumbnail = null; // prevents the reader from disposing it
						return imageResult;
					}

				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Preview failed to load image: " + ex.Message);
				return null;
			}
		}

		internal void WriteExportText(IndentStringBuilder output)
		{
			output.AppendLine("Header for SAW");
			output.Indent += 1;
			output.Append("Version = ").AppendLine(SoftwareVersion.VersionString);
			output.Append("SAW Bounds = ").AppendLine(SAWHeader.MainWindowBounds.ToString());
			output.Indent -= 1;
			output.Append("End Header for SAW");
			for (int page = 0; page < Pages.Count(); page++)
			{
				// The page number framework is only written if there are multiple pages
				if (Pages.Count() > 1)
				{
					output.Append("Page ").Append(page + 1).AppendLine("");
					output.Indent += 2;
				}
				m_Pages[page].WriteExportText(output);
				if (Pages.Count() > 1)
				{
					output.Indent -= 2;
					output.Append("End of Page ").Append(page + 1).AppendLine("");
				}
			}
		}

		/// <summary>Returns the next unused SAWID within the document (equal to highest used so far plus 1)</summary>
		public int NextUniqueID => (from p in Pages select p.FindHighestUsedID()).Max() + 1;

		#region Palettes

		/// <summary>if this is loaded as a palette within a config this is a reference to the config </summary>
		internal Config PaletteWithin = null;
		internal SizeF PaletteDesignSize;
		// only defined in v81 data.  Not really defined whether this includes margin or not, as the margin is always set to 0 for palettes

		/// <summary>Text displayed in the title of the palette at runtime.</summary>
		/// <remarks>Stored in Name</remarks>
		internal string PaletteTitle
		{
			get
			{
				Debug.Assert(IsPaletteWithin);
				return DisplayName;
			}
			set { DisplayName = value; }
		}

		/// <summary>Main part of description in the editing screen.  Can be translatable</summary>
		/// <remarks>SubTitle is appended in brackets if it is not empty</remarks>
		internal string PaletteDescription
		{
			get
			{
				Debug.Assert(IsPaletteWithin);
				return Name;
			}
			set
			{
				Name = value;
				Filename = value;
			}
		}

		/// <summary>Complete description display in the editing screen, including both PaletteDescription and SubTitle</summary>
		/// <remarks>Return value is already translated</remarks>
		internal string PaletteEditingFullDescription
		{
			get
			{
				if (string.IsNullOrEmpty(SubTitle))
					return Strings.Translate(PaletteDescription);
				return Strings.Translate(PaletteDescription) + " (" + Strings.Translate(SubTitle) + ")";
			}
		}

		/// <summary>If purpose was custom this correctly returns the a purpose with the document ID</summary>
		internal Palette.Purpose PalettePurpose
		{
			get
			{
				Debug.Assert(IsPaletteWithin);
				Palette.Purpose purpose = Info.Purpose;
				if (purpose.IsCustom)
					purpose = new Palette.Purpose(ID);
				return purpose;
			}
			set { Info.Purpose = new Palette.Purpose(value.ToInt32()); }
		}

		/// <summary>True if this is a palette within a config file</summary>
		internal bool IsPaletteWithin => PaletteWithin != null;

		/// <summary>Called (only for palettes) when the document is opened for editing; not when switching between tabs</summary>
		internal void StartEditingPalette()
		{
			if (m_Pages[0].IsSingleAutoSize && PaletteDesignSize.Width > 0 && PaletteDesignSize.Height > 0)
			{
				m_Pages[0].SetSize(PaletteDesignSize, 0);
				((IAutoSize)m_Pages[0].Shapes.First()).SetBounds(m_Pages[0].Bounds, null);
			}
		}

		internal class PaletteNameSort : IComparer<Document>
		{

			public int Compare(Document x, Document y) => Strings.Translate(x.Name).CompareTo(Strings.Translate(y.Name));

		}

		#endregion

		#region Document Info
		// in SAW this is only used for Palettes to identify them
		internal struct DocumentInfo
		{
			public Palette.Purpose Purpose;
			public string Author;

			public void Load(DataReader reader)
			{
				// for compatibility with Splash and early versions this has some dummy fields in the file
				Purpose = new Palette.Purpose(reader.ReadInt32());
				reader.ReadString();
				reader.ReadInt32();
				reader.ReadInt32();
				reader.ReadInt32();
				Author = ""; // to stop it being Nothing
				Author = reader.ReadString();
			}

			public void Save(DataWriter writer)
			{
				writer.Write(Purpose.ToInt32());
				writer.Write("");
				writer.Write(0);
				writer.Write(0);
				writer.Write(0);
				writer.Write(Author ?? "");
			}
		}

		internal DocumentInfo Info;

		#endregion

	}

}
