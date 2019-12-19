using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW
{
	public class Item : Container
	{

		#region Enums and constants
		/// <summary>User can only change between Item and Escape.  These are largely ignored in SAW7</summary>
		public enum ItemTypes
		{ IT_Item, IT_Group, IT_TopItem, IT_StartUp, IT_Default, IT_Help, IT_Control, IT_ControlItem, IT_Escape }
		/// <summary>Only controls what styles are used for the item, if any</summary>
		public enum ItemDisplayTypes
		{
			/// <summary>This is the default for most "Items"</summary>
			IDT_Output,
			IDT_Open,
			IDT_Group,
			/// <summary>Indicates the item uses its internal settings</summary>
			IDT_Item, //3
			IDT_NotScanned, // 4
			IDT_Help, //5
			IDT_TopItem,
			IDT_Escape
			///// <summary>Defined in SAW but never used?</summary>
			//[Obsolete]
			//IDT_NoStyle
		}

		[Flags]
		public enum Alignment : uint
		{
			Left = 0,
			Centre = 1,
			Right = 2,
			HorizontalMask = 0xf, // actually ff in SAW
			Top = 0,
			Middle = 0x100,
			Bottom = 0x200,
			VerticalMask = 0xf00
		}

		public static ItemDisplayTypes DefaultItemDisplayTypeForType(ItemTypes type)
		{
			switch (type)
			{
				case ItemTypes.IT_Item: return ItemDisplayTypes.IDT_Output;
				case ItemTypes.IT_Group: return ItemDisplayTypes.IDT_Group;
				case ItemTypes.IT_TopItem: return ItemDisplayTypes.IDT_TopItem;
				case ItemTypes.IT_StartUp:
				case ItemTypes.IT_Default: return ItemDisplayTypes.IDT_Output; // not really applicable
				case ItemTypes.IT_Help: return ItemDisplayTypes.IDT_Help;
				case ItemTypes.IT_Control:
				case ItemTypes.IT_ControlItem: return ItemDisplayTypes.IDT_Output; // not really applicable
				case ItemTypes.IT_Escape: return ItemDisplayTypes.IDT_Escape;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		#endregion

		#region Properties
		public ButtonShape.States State;

		// content...
		public SharedReference<SharedImage> Image;

		public SharedReference<SharedResource> Sound;
		/// <summary>Only defined if loaded from an old SAW file.  Will be leaf filename</summary>
		public string ImageName;

		public string OutputText;
		public uint LineSpace;
		public bool OutputAsDisplay;
		public string SpeechText;
		public bool SpeechAsDisplay;
		public string PromptText;

		// logic...
		public ItemTypes ItemType = ItemTypes.IT_Item;

		// other appearance... (some in styling section)
		public FillStyleC HighlightFillStyle;
		public LineStyleC HighlightLineStyle;
		public TextStyleC HighlightTextStyle;
		/// <summary>SAW 7.02.x - image only renders when highlighted </summary>
		public bool GraphicOnlyOnHighlight;

		public string ConceptID;

		#endregion

		#region Button styling from Splash button - style object and layout
		public ItemDisplayTypes StyleType = ItemDisplayTypes.IDT_Output;
		/// <summary>Only defined if DisplayType = Item
		/// may be from document IFF Style.IsShared - otherwise saved with me.  Should not be nothing (except temporarily) - set in Load or Start</summary>
		private ButtonStyle m_Style;
		/// <summary>defined whether stored internally or in document.
		/// Irrelevant unless DisplayType = Item</summary>
		private Guid m_idStyle;

		/// <summary>-1 means use auto (not in SAW).  SAW worked the same way round - this is % which is text</summary>
		public float TextRatio = -1;
		private const float DEFAULTTEXTRATIO = 0.25f;
		/// <summary>Position of graphic relative to text;  although SAW only used 0-3</summary>
		public ButtonShape.Layouts Arrangement = ButtonShape.Layouts.Above;
		public Alignment GraphicAlign = Alignment.Centre | Alignment.Middle;
		public bool TextShown = true;
		public bool GraphicShown;

		private RectangleF GetTextArea()
		{
			if (!GraphicShown) // Shown flags aren't done by the stuff in ButtonShape
				return m_Bounds;
			return ButtonShape.GetTextArea(m_Bounds, TextRatio, Image, Arrangement);
		}

		private RectangleF GetImageArea()
		{
			if (!TextShown) // Shown flags aren't done by the stuff in ButtonShape
				return m_Bounds;
			if (TextRatio < 0.01f)
				EnsureTextFormatted();
			return ButtonShape.GetImageArea(m_Bounds, TextRatio, Image, Arrangement, LabelText, m_Fragments);
		}

		protected override PointF[] RectangleLabelGetPoints() => GetTextArea().GetPoints();

		public Alignment TextAlign
		{
			get
			{
				Alignment a = 0;
				switch (TextStyle.Alignment)
				{
					case StringAlignment.Center: a = Alignment.Centre; break;
					case StringAlignment.Near: a = Alignment.Left; break;
					case StringAlignment.Far: a = Alignment.Right; break;
				}
				switch (TextStyle.VerticalAlignment)
				{
					case StringAlignment.Center: a |= Alignment.Middle; break;
					case StringAlignment.Near: a |= Alignment.Top; break;
					case StringAlignment.Far: a |= Alignment.Bottom; break;
				}
				return a;
			}
			set
			{
				switch (value & Alignment.HorizontalMask)
				{
					case Alignment.Left: TextStyle.Alignment = StringAlignment.Near; break;
					case Alignment.Centre: TextStyle.Alignment = StringAlignment.Center; break;
					case Alignment.Right: TextStyle.Alignment = StringAlignment.Far; break;
				}
				switch (value & Alignment.VerticalMask)
				{
					case Alignment.Top: TextStyle.VerticalAlignment = StringAlignment.Near; break;
					case Alignment.Middle: TextStyle.VerticalAlignment = StringAlignment.Center; break;
					case Alignment.Bottom: TextStyle.VerticalAlignment = StringAlignment.Far; break;
				}
			}
		}

		protected override StringAlignment EffectiveTextVerticalAlignment
		{
			get
			{
				if (TextRatio < 0)
					switch (Arrangement)
					{ // auto text ratio leaves enough space for text, but text engine works in superimpose mode.  Must top/bottom align it to get in right place if it is working vertically
						case ButtonShape.Layouts.Above:
							return StringAlignment.Far;
						case ButtonShape.Layouts.Below:
							return StringAlignment.Near;
					}
				return base.EffectiveTextVerticalAlignment;
			}
		}

		#endregion

		#region Verb etc overrides (mostly few)

		public override bool DefaultStylesApplied()
		{
			base.DefaultStylesApplied();
			//if (Globals.Root.CurrentConfig.Compatibility == Config.CompatibilityModes.SAW6)
			//{
				FillStyle.Colour = Color.LightGoldenrodYellow;
				FillStyle.Pattern = FillStyleC.Patterns.Solid;
				TextStyle.Colour = Color.Black;
				TextStyle.Size = 10;
				LineStyle.Colour = Color.Black;
				LineStyle.Width = 1;
				LineStyle.Pattern = DashStyle.Solid;
			//}
			HighlightFillStyle.CopyFrom(FillStyle);
			HighlightTextStyle.CopyFrom(TextStyle);
			HighlightLineStyle.CopyFrom(LineStyle);
			HighlightLineStyle.Colour = Color.Red;
			HighlightLineStyle.Width = 3;
			if (HighlightLineStyle.Colour.Equals(LineStyle.Colour))
				HighlightTextStyle.Colour = Color.Red;
			return true;
		}

		#endregion

		#region Data
		// parts maintaining Style are just copied from Button

		public Item()
		{
			HighlightFillStyle = new FillStyleC();
			HighlightLineStyle = new LineStyleC();
			HighlightTextStyle = new TextStyleC();
			TextStyle.SetDefaults();
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			HighlightFillStyle.SetDefaults();
			HighlightLineStyle.SetDefaults();
			HighlightTextStyle.SetDefaults();
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			Image = SharedReference<SharedImage>.FromGUID(reader.ReadGuid());
			ImageName = reader.ReadBufferedString();
			Sound = SharedReference<SharedResource>.FromGUID(reader.ReadGuid());
			// State is not stored
			//if (Sound != null)
			//	Debug.WriteLine("Read sound: " + Sound.ID + " = " + (LabelText ?? ""));
			OutputText = reader.ReadBufferedString();
			OutputAsDisplay = reader.ReadBoolean();
			SpeechText = reader.ReadString();
			SpeechAsDisplay = reader.ReadBoolean();
			PromptText = reader.ReadString();
			WordlistDoesntFill = reader.ReadBoolean();
			ItemType = (ItemTypes)reader.ReadInt32();
			HighlightFillStyle = FillStyleC.Read(reader);
			HighlightLineStyle = LineStyleC.Read(reader);
			HighlightTextStyle = TextStyleC.Read(reader);
			StyleType = (ItemDisplayTypes)reader.ReadInt32();
			m_idStyle = reader.ReadGuid();
			TextRatio = reader.ReadSingle();
			Arrangement = (ButtonShape.Layouts)reader.ReadInt32();
			//TextAlign = (Alignment)reader.ReadInt32();
			GraphicAlign = (Alignment)reader.ReadInt32();
			TextShown = reader.ReadBoolean();
			GraphicShown = reader.ReadBoolean();
			if (reader.Version >= 124)
				ConceptID = reader.ReadOptionalString();
			if (reader.Version >= 128)
				GraphicOnlyOnHighlight = reader.ReadBoolean();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(Image?.ID ?? Guid.Empty);
			writer.WriteBufferedString(ImageName);
			writer.Write(Sound?.ID ?? Guid.Empty);
			//if (Sound != null)
			//	Debug.WriteLine("Write sound: " + Sound.ID + " = " + (LabelText ?? ""));
			writer.WriteBufferedString(OutputText);
			writer.Write(OutputAsDisplay);
			writer.Write(SpeechText ?? "");
			writer.Write(SpeechAsDisplay);
			writer.Write(PromptText ?? "");
			writer.Write(WordlistDoesntFill);
			writer.Write((int)ItemType);
			HighlightFillStyle.Write(writer);
			HighlightLineStyle.Write(writer);
			HighlightTextStyle.Write(writer);
			writer.Write((int)StyleType);
			writer.Write(m_idStyle);
			writer.Write(TextRatio);
			writer.Write((int)Arrangement);
			//writer.Write((int)TextAlign);
			writer.Write((int)GraphicAlign);
			writer.Write(TextShown);
			writer.Write(GraphicShown);
			if (writer.Version >= 124)
				writer.WriteOptionalString(ConceptID);
			if (writer.Version >= 128)
				writer.Write(GraphicOnlyOnHighlight);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Item item = (Item)other;
			//Debug.WriteLine("Item.CopyFrom, depth=" + depth + " ID=" + (Parent as Scriptable).SAWID + ", Parent match=" + (Parent == item.Parent));
			if (depth > CopyDepth.Transform)
			{
				CopyPresentationFrom(item, false);
				if (depth == CopyDepth.Duplicate && (!m_Style?.IsShared ?? false)) // unless cloning and this is non-shared
					m_Style = (ButtonStyle)m_Style.Clone(mapID);
				ConceptID = item.ConceptID;
			}
		}

		/// <summary>This is much of the implementation of CopyFrom, but also used by Functions.CopyPresentation, which does not copy the contents.
		/// This only copies items specific to this class, and omits any presentation from the base class.  If noOverwriteExisting then the image and various text items aren't copied if there is anything currently there</summary>
		public void CopyPresentationFrom(Item item, bool noOverwriteExisting)
		{
			if (!noOverwriteExisting || Image == null)
			{
				Image = item.Image?.Clone();
				ImageName = item.ImageName;
			}
			if (!noOverwriteExisting || Sound == null)
				Sound = item.Sound?.Clone();
			if (!noOverwriteExisting || string.IsNullOrEmpty(OutputText))
				OutputText = item.OutputText;
			OutputAsDisplay = item.OutputAsDisplay;
			if (!noOverwriteExisting || string.IsNullOrEmpty(SpeechText))
				SpeechText = item.SpeechText;
			SpeechAsDisplay = item.SpeechAsDisplay;
			if (!noOverwriteExisting || string.IsNullOrEmpty(PromptText))
				PromptText = item.PromptText;
			WordlistDoesntFill = item.WordlistDoesntFill;
			ItemType = item.ItemType;
			if (HighlightFillStyle == null) HighlightFillStyle = new FillStyleC();
			if (HighlightLineStyle == null) HighlightLineStyle = new LineStyleC();
			if (HighlightTextStyle == null) HighlightTextStyle = new TextStyleC();
			HighlightFillStyle.CopyFrom(item.HighlightFillStyle);
			HighlightLineStyle.CopyFrom(item.HighlightLineStyle);
			HighlightTextStyle.CopyFrom(item.HighlightTextStyle);
			StyleType = item.StyleType;
			m_idStyle = item.m_idStyle;
			m_Style = item.m_Style; // it is OK to share the object; it will be transacted separately
			TextRatio = item.TextRatio;
			Arrangement = item.Arrangement;
			//TextAlign = objItem.TextAlign; - now in text style
			GraphicAlign = item.GraphicAlign;
			TextShown = item.TextShown;
			GraphicShown = item.GraphicShown;
			GraphicOnlyOnHighlight = item.GraphicOnlyOnHighlight;

			// actually from base class, so this is repeated in a normal copy, but needed for external CopyPresentation
			BorderShape = item.BorderShape;
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			Image?.DereferenceOnLoad(document);
			Sound?.DereferenceOnLoad(document);
			if (m_Style == null)
			{
				m_Style = ButtonStyle.UserDefaultByID(m_idStyle); // Will still be nothing unless the ID was one of the defaults
				if (m_Style == null)
				{
					m_Style = document.GetButtonStyle(m_idStyle);
					if (m_Style == null)
					{
						Utilities.LogSubError("Cannot find document style (" + m_idStyle + ") used in button: " + ID, true);
						// can't function without it
						m_Style = new ButtonStyle();
						m_Style.Initialise();
						m_idStyle = m_Style.ID;
					}
					else
						Debug.Assert(m_Style.IsShared);
					m_Style.UpdateReferencesObjectsCreated(document, reader);
				}
			}
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			if (mapID.ContainsKey(m_idStyle))
			{
				m_idStyle = mapID[m_idStyle].ID;
				m_Style = document.GetButtonStyle(m_idStyle);
			}
			else if (m_Style != null && !m_Style.IsUserDefault) // first condition is just safety check - refetch if object available
			{
				m_idStyle = m_Style.ID;
				m_Style.UpdateReferencesIDsChanged(mapID, document);
			}
			Image?.UpdateIDsReferencesChanged();
			Sound?.UpdateIDsReferencesChanged();
		}

		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			if (m_Style != null && m_Style.IsShared && !m_Style.IsUserDefault)
			{
				fnAdd.Invoke(m_Style);
				mapID.AddUnchangedObject(m_Style);
			}
			fnAdd.Invoke(Image?.Content);
			fnAdd.Invoke(Sound?.Content);
		}

		// The image and style are not contained, it is only a shared reference - unless style is not shared
		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			if (m_Style != null && !m_Style.IsShared)
				m_Style.Iterate(fn);
		}

		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			base.AddToFlatList(list, purpose);
			if (purpose == FlatListPurpose.HasTexture)
			{
				// will be ignored by base class
				if (m_Style.FillStyle.Any(objStyle => objStyle.Texture != null))
					list.Add(this);
			}
		}

		public override void WriteExportText(IndentStringBuilder output)
		{
			base.WriteExportText(output);
			// this is not like the item number header since the Scriptable will already have done that
			output.Append("Display = \"").AppendEncoded(LabelText ?? "").AppendLine("\"");
			if (!string.IsNullOrEmpty(OutputText))
				output.Append("Output = \"").AppendEncoded(OutputText).AppendLine("\"");
			if (!string.IsNullOrEmpty(PromptText))
				output.Append("Help text = \"").AppendEncoded(PromptText).AppendLine("\"");
			if (Image != null)
				output.AppendLine("Has image");
			if (Sound != null)
				output.AppendLine("Has recorded sound");
			output.Append("Font size = ").AppendLine(TextStyle.Size);
			output.Append("Item type = ").AppendLine(ItemType.ToString());
			output.Append("Item display type = ").AppendLine(StyleType.ToString());
			if (OutputAsDisplay)
				output.AppendLine("Output as display");
			if (SpeechAsDisplay)
				output.AppendLine("Speech as display");
		}

		#endregion

		#region Info
		public override Shapes ShapeCode => Shapes.SAWItem;

		public override LabelModes LabelMode => LabelModes.Always;

		protected override LabelPositions LabelPosition => LabelPositions.RotatedRectangle;
		// not actually rotated, but easier to just use this than create a new CustomRect value

		/// <summary>True if this is considered a GroupItem (ItemType==IT_Group in SAW6).  Now automatic based on whether it has contents</summary>
		public bool IsGroup => Contents.Any();

		/// <summary>Whether it is an escape item.  User editable.  (ItemType==IT_Escape in SAW6, and that is still used in SAW7)</summary>
		public bool IsEscape
		{
			get { return ItemType == ItemTypes.IT_Escape; }
			set { ItemType = value ? ItemTypes.IT_Escape : ItemTypes.IT_Item; }
		}

		#endregion

		#region Editor Interaction

		private bool m_BeingEdited;
		public override string DoubleClickText()
		{
			return Strings.Item("SAW_Edit_Item");
		}

		public override void DoDoubleClick(EditableView view, EditableView.ClickPosition.Sources source)
		{
			m_BeingEdited = true; // blocks highlight
			try
			{
				// Scriptable can also be passed to editor, but can be null if not present
				LastDoubleClickResult = frmSAWItem.Display(view) == DialogResult.OK;
				if (LastDoubleClickResult)
					Status = StatusValues.Moved;
				else
					Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
			}
			finally
			{ m_BeingEdited = false; }
		}

		/// <summary>Called by editor dialog to force this to discard some cache items and repaint</summary>
		public void WasEdited()
		{
			DiscardPath();
			ClearTextCache();
			FormatText();
			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
		}

		#endregion

		#region Coordinates

		public override void SetBounds(RectangleF bounds, Transaction transaction = null)
		{
			base.SetBounds(bounds, transaction);
			base.ClearTextCache();
			if (Parent is Scriptable scriptable) // it will reset it's own bounds, but doesn't add itself to the tx as it doesn't have it
			{
				Debug.Assert(transaction != null);
				transaction?.Edit(scriptable);
			}
			Parent?.NotifyIndirectChange(this, ChangeAffects.Bounds);
		}

		protected override bool PlaceFragment(Fragment create, float extraLineSpace = 0)
		{
			// The override is needed in order to implement the line spacing.  The base class can do line spacing within the function, but has no access to the property of this object
			return base.PlaceFragment(create, LineSpace / 2f);
		}

		protected override void CheckTextVerticalPositions(bool bolVerticalAlignChanged, float extraLineSpace = 0)
		{
			// as above
			base.CheckTextVerticalPositions(bolVerticalAlignChanged, LineSpace / 2f);
		}

		#endregion

		#region Graphics

		protected override void PrepareDraw(DrawResources resources)
		{
			// next part from: DefaultPrepareDrawForText(resources);
			if (HasText(false))
			{
				if (resources.EdgeAlpha == 0)
					return; // can crash trying to draw text with A=0, so this is essential
				Color c = State == ButtonShape.States.Highlight ? HighlightTextStyle.Colour : TextStyle.Colour;
				if (c.A == 0)
					c = GetDefaultTextColour(); // c.a=0 is safer than IsEmpty
				resources.TextBrush = resources.Graphics.CreateFill(Color.FromArgb(resources.EdgeAlpha * c.A / 255, c));
			}

			bool useStyle = Config.UserUser.ReadBooleanEx(Config.SAW_EnableItemStyles) && StyleType != ItemDisplayTypes.IDT_Item;
			if (!useStyle)
			{
				LineStyleC lineStyle = State == ButtonShape.States.Highlight ? HighlightLineStyle : LineStyle;
				// from: DefaultPrepareLineDraw( ... ) without WireFrame, support for thin dashed patterns (see base), DisplayCentres
				float width = lineStyle.Width;
				if (State == ButtonShape.States.Highlight)
					width = width * 2 - 1; // highlight widths were greater in SAW 6
				if (width > 0)
				{
					if (IsFilled && lineStyle.Colour.A == 0 && m_DefinedVertices < 2 && Status == StatusValues.Creating)
					{
						// special case if drawing first line only which is transparent, then draw a line using the fill colour
						resources.MainPen = resources.Graphics.CreateStroke(this.FillStyle.Colour, width * 2);
					}
					else if (resources.EdgeAlpha == 255)
						resources.MainPen = resources.Graphics.CreateStroke(lineStyle.Colour, width);
					else
						resources.MainPen = resources.Graphics.CreateStroke(Color.FromArgb(resources.EdgeAlpha, lineStyle.Colour), width);
					resources.MainPen.DashStyle = lineStyle.Pattern;
					resources.MainPen.MiterLimit = MITRELIMIT;
				}
				else if (width == 0 && FillStyle.Pattern == FillStyleC.Patterns.Empty && Globals.Root.User == Users.Editor && Globals.Root.CurrentConfig.ReadBoolean(Config.Show_Hidden_When_Editing, true))
				{ // show invisible items when editing
					resources.MainPen = resources.Graphics.CreateStroke(Color.Gray, 2);
					resources.MainPen.DashStyle = DashStyle.Dot;
				}
				DefaultPrepareFillDraw(State == ButtonShape.States.Highlight ? HighlightFillStyle : FillStyle, resources, HasText(true));
			}
			else
			{
				var style = Config.UserUser.ButtonStyle[(int)StyleType + 2];
				style.PrepareResources(State, resources);
			}
		}

		protected override void PrepareHighlight(DrawResources resources)
		{
			if (m_BeingEdited)
				return;
			base.PrepareHighlight(resources);
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			bool useStyle = Config.UserUser.ReadBooleanEx(Config.SAW_EnableItemStyles) && StyleType != ItemDisplayTypes.IDT_Item;
			if (!useStyle || resources.Buffer == StaticView.InvalidationBuffer.Selection)
			{
				if (resources.MainBrush != null || resources.MainPen != null) // from Container
				{
					switch (BorderShape)
					{
						case Borders.Rectangle:
							gr.Rectangle(m_Bounds, resources.MainPen, resources.MainBrush);
							break;
						default:
							gr.Path(EnsurePath(false), resources.MainPen, resources.MainBrush);
							break;
					}
				}
			}
			else
			{
				var style = Config.UserUser.ButtonStyle[(int)StyleType + 2];
				style.Draw(gr, resources, m_Bounds, State);
			}
			if (Image != null && GraphicShown
				&& (!GraphicOnlyOnHighlight || State == ButtonShape.States.Highlight))
			{
				RectangleF imageArea = GetImageArea();
				RectangleF destination = imageArea;
				GUIUtilities.CalcDestRect(Image.Content.GetSize(), ref destination);
				switch (GraphicAlign & Alignment.HorizontalMask)
				{
					case Alignment.Left: destination.X = imageArea.X; break;
					case Alignment.Centre: break;
					case Alignment.Right: destination.X = imageArea.Right - destination.Width; break;
				}
				switch (GraphicAlign & Alignment.VerticalMask)
				{
					case Alignment.Top: destination.Y = imageArea.Y; break;
					case Alignment.Middle: break;
					case Alignment.Bottom: destination.Y = imageArea.Bottom - destination.Height; break;
				}
				gr.DrawImage(Image.Content, 0, destination);
			}
		}

		protected override void DrawLabel(Canvas gr, DrawResources objResources)
		{
			if (!TextShown)
				return;
			base.DrawLabel(gr, objResources);
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{ // must replace version in Container - to use a greater line width if highlighted
		  // actually sometimes we need to reflect what previous state was, so this just uses the larger of the 2 widths always (and highlight one renders with larger value than is stored)
			if (!withShadow && !m_Refresh.IsEmpty)
				return m_Refresh;
			RectangleF refreshBounds = base.RefreshBoundsFromBounds(withShadow, Math.Max(LineStyle.Width, HighlightLineStyle.Width * 2 - 1) + 1);
			// but then had to add the random +1 because an artefact was being left behind when highlight removed???
			foreach (Shape shape in m_Shapes)
			{
				Geometry.Extend(ref refreshBounds, shape.RefreshBounds(withShadow));
			}
			if (!withShadow)
				m_Refresh = refreshBounds;
			return refreshBounds;
		}


		#endregion

		#region Legacy - Read/Write is in here
		/// <summary>Object representing an Item in an old SAW file used when reading/writing a file.  Not stored in the Splash document once loaded</summary>
		public class LegacyItem : IArchivable
		{
			public Item Main;
			public Scriptable Scripting;
			public List<LegacyItem> SubItems = new List<LegacyItem>();     // list of sub-items as LegacyItem (or derived)

			public LegacyItem()
			{
			}

			/// <summary>For saving creates a LegacyItem with the Scriptable and optionally Item within it</summary>
			public LegacyItem(Scriptable existing)
			{
				Scripting = existing;
				Main = Scripting.Element as Item ?? new Item() { m_Bounds = new RectangleF(0, 0, 100, 100) }; // can't have empty rectangle as it will want it to save;  this only applies to the dummy items for default scripts
			}

			#region Exposing some item members for convenience

			public Script m_pVisitScript
			{
				[DebuggerStepThrough]
				get { return Scripting.VisitScript; }
			}

			public Script m_pNextScript
			{
				[DebuggerStepThrough]
				get { return Scripting.NextScript; }
			}

			public Script m_pSelectScript
			{
				[DebuggerStepThrough]
				get { return Scripting.SelectScript; }
			}

			#endregion

			#region SAW read/write

			public virtual void Read(ArchiveReader ar)
			{
				Main = new Item();

				Main.LineStyle.SetDefaults();
				Main.FillStyle.SetDefaults();
				Main.TextStyle.SetDefaults();
				Main.HighlightFillStyle = new FillStyleC();
				Main.HighlightLineStyle = new LineStyleC();
				Main.HighlightTextStyle = new TextStyleC();

				if (ar.SAWVersion >= 6100)
					Main.ConceptID = ar.ReadStringL();
				if (ar.SAWVersion >= 6001)
				{
					Main.FillStyle.Pattern = ar.ReadBool32() ? FillStyleC.Patterns.Solid : FillStyleC.Patterns.Empty;
					Main.HighlightFillStyle.Pattern = ar.ReadBool32() ? FillStyleC.Patterns.Solid : FillStyleC.Patterns.Empty;
				}
				string soundFile = ar.ReadStringL();
				var file = SAW6.LocateResourceFile(soundFile, SAW6.GetDefaultSoundsFolders(), ar.FileFolder);
				if (string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(soundFile))
					Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_UNRESOLVED_RESOURCE").Replace("%0", soundFile));
				else if (!string.IsNullOrEmpty(file))
					try
					{
						Main.Sound = (SharedResource)ar.IntoDocument.AddSharedResourceFromFile(file, ar.ReadTransaction, false);
					}
					catch (Exception ex)
					{
						Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_IMAGE_FAILED").Replace("%0", soundFile).Replace("%1", ex.Message));
					}


				Main.ItemType = (ItemTypes)ar.ReadInt32();
				Main.StyleType = (ItemDisplayTypes)ar.ReadInt32();
				Main.OutputAsDisplay = ar.ReadBool32();
				Main.SpeechAsDisplay = ar.ReadBool32();
				Main.TextRatio = (float)ar.ReadDouble();

				string idText = ar.ReadStringL(); // need to convert safely to number
				int id = 0;
				int.TryParse(idText, out id);

				Rectangle temp = ar.ReadRectangleAdjusted();
				if (temp.Width <= 0)
					temp.Width = 1; // just because Empty bounds causes problems - it is assumed to be undefined;  but this shape requires defined bounds
				if (temp.Height <= 0)
					temp.Height = 1;
				Main.SetBounds(temp);
				//Main.m_rcSavedBound = Main.Bounds;

				// should be after SetBounds which has extra logic if it has a parent
				Scripting = new Scriptable(Main)
				{
					SAWID = id,
					VisitScript = (Script)ar.ReadObject(),
					SelectScript = (Script)ar.ReadObject(),
					NextScript = (Script)ar.ReadObject()
				};
				if (ar.SAWVersion >= 6200)
				{
					Scripting.PreRepeatScript = (Script)ar.ReadObject();
					Scripting.RepeatScript = (Script)ar.ReadObject();
					Scripting.PostRepeatScript = (Script)ar.ReadObject();
					Scripting.RepeatTimeout = ar.ReadInt32();
				}

				Main.m_Label = ar.ReadStringL().Replace("\r\n", "\r");
				string graphic = ar.ReadStringL();
				file = SAW6.LocateResourceFile(graphic, SAW6.GetDefaultGraphicFolders(), ar.FileFolder);
				if (file == null && graphic.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase))
					file = SAW6.LocateResourceFile(Path.ChangeExtension(graphic, ".png"), SAW6.GetDefaultGraphicFolders(), ar.FileFolder);
				Debug.WriteLineIf(!string.IsNullOrEmpty(graphic), Scripting.SAWID + " has GFX: '" + graphic + "' which resolves to '" + (file ?? "?") + "'");
				if (string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(graphic))
					Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_UNRESOLVED_RESOURCE").Replace("%0", graphic));
				else if (!string.IsNullOrEmpty(file))
					try
					{
						var image = new MemoryImage(file);
						if (string.Equals(Path.GetExtension(file), ".bmp", StringComparison.InvariantCultureIgnoreCase))
						{ // for bitmaps do transparent purple stuff
							var old = image.GetNetImage();
							var create = new Bitmap(old.Width, old.Height);
							using (var gr = Graphics.FromImage(create))
							{
								var x = new ImageAttributes();
								x.SetColorKey(Color.FromArgb(255, 255, 0, 255), Color.FromArgb(255, 255, 0, 255));
								gr.DrawImage(old, new Rectangle(0, 0, old.Width, old.Height), x);
							}
							image = new MemoryImage(create);
						}
						Main.Image = ar.IntoDocument.AddSharedBitmapFromMemoryImage(image, ar.ReadTransaction);
					}
					catch (Exception ex)
					{
						Globals.NonFatalOperationalError(Strings.Item("SAW_FILE_IMAGE_FAILED").Replace("%0", graphic).Replace("%1", ex.Message));
					}


				Main.SpeechText = ar.ReadStringL();
				Main.PromptText = ar.ReadStringL();
				Main.OutputText = ar.ReadStringL();

				// If the speech is a wav file then move to sound file for previous versions.
				//CheckSpeechForSound();
				Scripting.AutoRepeat = ar.ReadBool32();
				Scripting.ResetSwap = ar.ReadBool32();

				Main.TextShown = ar.ReadBool32();
				Main.GraphicShown = ar.ReadBool32();

				LOGFONT logFont = new LOGFONT(); // old C++ style structure used in SAW5
				logFont.Read(ar);
				Main.TextStyle.Size = Math.Abs(72 * logFont.lfHeight / 96); // will be -ve usually in the LOGFONT to indicate character height. And LOGFONT uses some device independant size.  It should be adjusted by output DPI, but since this is in the data file we have to assume its for 96 DPI
				if (logFont.lfItalic != 0)
					Main.TextStyle.Style |= FontStyle.Italic;
				if (logFont.lfStrikeout != 0)
					Main.TextStyle.Style |= FontStyle.Strikeout;
				if (logFont.lfUnderline != 0)
					Main.TextStyle.Style |= FontStyle.Underline;
				if (logFont.lfWeight > 500)
					Main.TextStyle.Style |= FontStyle.Bold;
				Main.TextStyle.Face = logFont.FontName;

				Main.LineSpace = ar.ReadUInt32();
				Main.TextStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				Main.FillStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				Main.LineStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				Main.HighlightTextStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				Main.HighlightFillStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				Main.HighlightLineStyle.Colour = GUIUtilities.ColorFromWin32RGB(ar.ReadInt32());
				int border = (int)ar.ReadUInt32(); // /Doesn't have a "no" border, but we can just do this by setting the width to 0
				if (border == 0) // There is no need to have it possible to remove the border in either the width or the shape
				{
					Main.BorderShape = Borders.Rectangle;
					Main.LineStyle.Width = 0;
					ar.ReadUInt32(); // and ignore the width in the file
				}
				else
				{
					Main.BorderShape = (Borders)(border - 1);
					Main.LineStyle.Width = ar.ReadUInt32(); // 0-4
				}
				Main.HighlightLineStyle.Width = ar.ReadUInt32();
				Main.Arrangement = (ButtonShape.Layouts)ar.ReadUInt32(); // luckily seems to use same values (at least 0-3)
				Debug.Assert(Main.Arrangement >= 0 && Main.Arrangement <= (ButtonShape.Layouts)3);
				Main.TextAlign = (Alignment)ar.ReadUInt32();
				Main.GraphicAlign = (Alignment)ar.ReadUInt32();
				//Main.m_nTextArea = ar.ReadInt32();
				ar.ReadInt32(); // m_nTextArea was no longer used
				Scripting.Popup = ar.ReadBool32();
				Scripting.Shown = ar.ReadBool32();
				Scripting.NotVisited = ar.ReadBool32();
				Main.WordlistDoesntFill = ar.ReadBool32();

				//Debug.WriteLine(Main.LabelText + "=" + Main.FillStyle.Colour.ToString());

				ar.ReadList(SubItems);

				foreach (LegacyItem sub in SubItems)
				{
					Main.Contents.Add(sub.Scripting);
					sub.Scripting.Parent = Main;
				}
				Main.FinishedModifyingContents(new Transaction());
			}

			public virtual void Write(ArchiveWriter ar)
			{
				if (Main.ItemType == ItemTypes.IT_Item && Main.Contents.Any())
					Main.ItemType = ItemTypes.IT_Group;

				if (ar.SAWVersion >= 6100)
					ar.WriteStringL(Main.ConceptID, true);
				if (ar.SAWVersion >= 6001)
				{
					ar.WriteBool32(Main.FillStyle.Pattern != FillStyleC.Patterns.Empty);
					ar.WriteBool32(Main.FillStyle.Pattern != FillStyleC.Patterns.Empty);
				}

				if (Main.Sound?.HasContent ?? false)
				{ // must pick a unique filename in the save destination folder
					string name = Main.Sound?.Content.Filename;
					var extension = Path.GetExtension(name);
					name = Path.GetFileNameWithoutExtension(name); // strip off extension if ImageName had one
					int index = 0; // will actually start at 1, as it is incremented before test below
					string file;
					do
					{
						index += 1;
						file = Path.Combine(ar.FileFolder, name + " " + index + extension);
					} while (File.Exists(file));
					Main.Sound.Content.SaveContent(file);
					ar.WriteStringL(Path.GetFileName(file), true);
				}
				else
					ar.WriteStringL("");

				ar.Write((int)Main.ItemType);
				ar.Write((int)Main.StyleType);
				ar.WriteBool32(Main.OutputAsDisplay);
				ar.WriteBool32(Main.SpeechAsDisplay);

				ar.Write(Main.TextRatio <= 0 ? DEFAULTTEXTRATIO : (double)Main.TextRatio); // was a double in SAW
				ar.WriteStringL(Scripting.SAWID.ToString());
				var bounds = Main.Bounds.ToRectangle();
				//bounds.Y += ar.PageHeight;
				ar.WriteAdjusted(bounds); // must convert as SAW saves as integers.
				ar.Write(m_pVisitScript ?? new Script()); // SAW7 allows null scripts, SAW6 doesn't
				ar.Write(m_pSelectScript ?? new Script());
				ar.Write(m_pNextScript ?? new Script());
				if (ar.SAWVersion >= 6200)
				{
					ar.Write(Scripting.PreRepeatScript ?? new Script());
					ar.Write(Scripting.RepeatScript ?? new Script());
					ar.Write(Scripting.PostRepeatScript ?? new Script());
					ar.Write(Scripting.RepeatTimeout);
				}

				ar.WriteStringL((Main.m_Label ?? "").Replace("\r", "\r\n"), true);
				//Debug.WriteLine("save: " + (Main.m_Label ?? ""));
				if (Main.Image?.HasContent ?? false)
				{ // must pick a unique filename in the save destination folder
					string name = string.IsNullOrEmpty(Main.ImageName) ? "Image" : Main.ImageName;
					name = Path.GetFileNameWithoutExtension(name); // strip off extension if ImageName had one
					int index = 0; // will actually start at 1, as it is incremented before test below
					string extension = Main.Image.Content.MemoryImage.GetNetImage().RawFormat.FileExtensionFromEncoder();
					if (extension == null)
					{
						ar.WriteStringL("");
						Globals.NonFatalOperationalError("Cannot determine format to save image in " + this + " - it will be omitted");
					}
					else
					{
						string file;
						do
						{
							index += 1;
							file = Path.Combine(ar.FileFolder, name + " " + index + extension);
						} while (File.Exists(file));
						Main.Image.Content.MemoryImage.GetNetImage().Save(file);
						//Debug.WriteLine("Format = " + objFormat);
						ar.WriteStringL(Path.GetFileName(file), true);
					}
				}
				else
					ar.WriteStringL("");
				ar.WriteStringL(Main.SpeechText, true);
				ar.WriteStringL(Main.PromptText, true);
				ar.WriteStringL(Main.OutputText, true);
				ar.WriteBool32(Scripting.AutoRepeat);
				ar.WriteBool32(Scripting.ResetSwap);
				ar.WriteBool32(Main.TextShown);
				ar.WriteBool32(Main.GraphicShown);

				LOGFONT logFont = new LOGFONT();
				logFont.lfHeight = -(int)(Main.TextStyle.Size * 96 / 72);
				if ((Main.TextStyle.Style & FontStyle.Italic) > 0)
					logFont.lfItalic = 255; // not sure why these particular numbers.  They were obtained by inspecting old files.  Zero is always False, but True seems to vary
				if ((Main.TextStyle.Style & FontStyle.Strikeout) > 0)
					logFont.lfStrikeout = 1;
				if ((Main.TextStyle.Style & FontStyle.Underline) > 0)
					logFont.lfItalic = 1;
				logFont.lfWeight = (Main.TextStyle.Style & FontStyle.Bold) > 0 ? 700 : 400;
				logFont.Write(ar);

				ar.Write(Main.LineSpace);
				ar.Write(Main.TextStyle.Colour.ToWin32RGB());
				ar.Write(Main.FillStyle.Colour.ToWin32RGB());
				ar.Write(Main.LineStyle.Colour.ToWin32RGB());
				ar.Write(Main.HighlightTextStyle.Colour.ToWin32RGB());
				ar.Write(Main.HighlightFillStyle.Colour.ToWin32RGB());
				ar.Write(Main.HighlightLineStyle.Colour.ToWin32RGB());
				ar.Write((int)Main.BorderShape + 1);
				ar.Write((int)Main.LineStyle.Width);
				ar.Write((int)Main.HighlightLineStyle.Width);
				ar.Write((int)Main.Arrangement & 3); // &3 is just a bodge.  Removes the 128 superimpose flag.  Will give random result for any other values we have that SAW doesn't support
				ar.Write((uint)Main.TextAlign);
				ar.Write((uint)Main.GraphicAlign);
				ar.Write(0); // m_nTextArea, not used in SAW6
				ar.WriteBool32(Scripting.Popup);
				ar.WriteBool32(Scripting.Shown);
				ar.WriteBool32(Scripting.NotVisited);
				ar.WriteBool32(Main.WordlistDoesntFill);
				ar.WriteList(SubItems);
			}

			#endregion

			/// <summary>Returns the correct type of LegacyItem object (SAW 6 used different classes for some elements)</summary>
			public static LegacyItem CreateForScriptable(Scriptable scriptable)
			{
				if (!(scriptable.Element is Item))
				{
					Globals.NonFatalOperationalError("[SAW_FILE_SAW6_OtherElement]");
					return null;
				}
				switch ((scriptable.Element as Item).ItemType)
				{
					// Actually these don't seem to be used for the types within the document, only for the defaults in header.
					// and for those ->Serialize is directly invoked rather than using the object system.  SAW 6 fails to load these as objects with wrong class errors
					//case ItemTypes.IT_Escape: return new LegacyItem(scriptable);
					//case ItemTypes.IT_Group: return new GroupItem(scriptable);
					default: return new LegacyItem(scriptable);
				}
			}

			/// <summary>Adds all the contents of Main to SubItems.  Used when saving from current data to SAW6 file to generate the rest of the tree of legacy objects</summary>
			public void AddContents()
			{
				if (Main == null)
					return;
				foreach (Scriptable sub in Main.Contents.OfType<Scriptable>())
				{
					var create = CreateForScriptable(sub);
					SubItems.Add(create);
					create.AddContents();
				}
			}

		}

		/// <summary>although the EscapeItem and GroupItem classes are used by SAW, I don't think they actually make any difference
		/// (the only difference is in the default scripts when they are first created).  However the classes need to exist so they can serialise to/from SAW6 files correctly</summary>
		public class EscapeItem : LegacyItem
		{

			public EscapeItem()
			{ }

			public EscapeItem(Scriptable existing) : base(existing)
			{ }
		}

		public class GroupItem : LegacyItem
		{
			public GroupItem()
			{ }

			public GroupItem(Scriptable existing) : base(existing)
			{ }
		}

		#endregion

		#region Word prediction container support
		// all of this is only relevant for the container/child in a word list.

		/// <summary>Current scroll offset.  Only meaningful in the word list container</summary>
		private int m_WordListScroll;
		/// <summary>Total set of predictions currently displayed, which may be more than is displayed at once.</summary>
		private List<string> m_Predictions;

		/// <summary>Only relevant on prediction items within word list.  The index of the word currently displayed herein.  1-based; 0 for nothing displayed</summary>
		private int WordIndex;

		/// <summary>True if this is a custom item in a word prediction list which is not filled with predicted words.
		/// This IS saved in the data</summary>
		public bool WordlistDoesntFill;

		/// <summary>Only relevant if this is an/the word list container.  Fills the child items with the given predictions.</summary>
		public void ShowPredictions(List<string> predictions)
		{
			m_Predictions = predictions;
			m_WordListScroll = 0;
			UpdateWordPredictions();
		}

		private void UpdateWordPredictions()
		{
			int index = m_WordListScroll;
			foreach (Item child in WordListEntryChildren)
			{
				if (index >= m_Predictions.Count)
				{
					// no more predictions
					child.WordIndex = 0;
					child.LabelText = "";
					child.SpeechText = "";
				}
				else
				{
					child.WordIndex = index + 1;
					child.LabelText = m_Predictions[index];
					child.SpeechText = m_Predictions[index];
					child.MaximiseFontSize();
					index++;
				}
			}
			NotifyIndirectChange(this, ChangeAffects.RepaintNeeded, RefreshBounds());
		}

		/// <summary>Only relevant if this is a word list container.  Displays different items, assuming there were more than could be displayed at once</summary>
		public void ScrollWordList(CmdWordListScroll.Directions direction)
		{
			if (m_Predictions?.Count == 0)
			{ // if no list provided, or was empty, always reset to zero for the future
				m_WordListScroll = 0;
				return;
			}
			int pageSize = WordListEntryChildren.Count();
			int lastPosition = (m_Predictions.Count - 1) / pageSize * pageSize;
			// highest permitted value of WordListScroll
			// which is the highest value counting up in units of pageSize that still has predictions on the remaining page
			switch (direction)
			{
				case CmdWordListScroll.Directions.Top:
					m_WordListScroll = 0;
					break;
				case CmdWordListScroll.Directions.Up:
					m_WordListScroll = Math.Max(0, m_WordListScroll - pageSize);
					break;
				case CmdWordListScroll.Directions.Down:
					m_WordListScroll = Math.Min(lastPosition, m_WordListScroll + pageSize);
					break;
				case CmdWordListScroll.Directions.Bottom:
					m_WordListScroll = lastPosition;
					break;
				default: throw new ArgumentException(nameof(direction));
			}
			UpdateWordPredictions();
		}

		/// <summary>Returns child items which can be used to display word list entries</summary>
		private IEnumerable<Item> WordListEntryChildren
		{ get { return from s in Contents.OfType<Scriptable>() where s.Element is Item && !(s.Element as Item).WordlistDoesntFill select (Item)s.Element; } }

		private void MaximiseFontSize()
		{
			float trySize = Bounds.Height / Geometry.POINTUNIT + 2; // approx max size.  +2 added on for luck
			ClearTextCache();
			while (true)
			{
				TextStyle.Size = trySize;
				base.FormatText(0);
				if (CalculateTextBounds().Height <= Bounds.Height)
					break; // keep this
				trySize = trySize * 0.9f;
			}
		}

		#endregion

	}

}
