using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;

namespace SAW
{
	/// <summary>Note, this is not the regular SAW item which is "Item".  This is a button on a palette or in the toolbox.  It may also be usable in the document in future if
	/// Scriptable is extended to be able to contain anything. </summary>
	public class ButtonShape : BoundsDefines
	{
		// for buttons in palettes
		// uses TextStyle defined in base class.  LineStyle and FillStyle not used - See Styling section

		public enum States
		{
			Normal = 0, // numbers critical - used as ButtonStyle array indexer
			Highlight = 1, // or Hover
			Selected = 2,
			Disabled = 10 // there is no display for this as such; it displays using Normal but with lower opacity
		}
		/// <summary>not saved - this always starts as Normal</summary>
		protected States m_State = States.Normal;
		private SharedReference<SharedImage> m_Image;
		public Action Action;
		public bool DisplayFromAction = true; // true if the display should be updated based on the action (only takes effect in the editor - the necessary display is stored in this object)
		private int m_LastAutoImageSize = 0; // size requested for the last image obtained automatically.  0 if not attempted.  -1 if failed
											 // Note that this is not the size of the actual image (this is used to detect when the request substantially changes, in which case it may be worth trying again)

		#region Edit support
		public Image GetImageForPreview()
		{// returns .net image object
			return m_Image?.Content?.GetNetImage();
		}

		public void SetImage(SharedImage img)
		{
			// image should already have been added to document
			m_Image = img;
		}

		public void SetPosition(RectangleF position)
		{
			m_Bounds = position;
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal TextStyleC GetTextStyle()
		{
			// editor wants direct access - base class returns no style if label is empty
			return TextStyle;
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal SharedImage GetImage()
		{
			return m_Image;
		}

		internal void SetStyleObject(ButtonStyle style)
		{
			BackgroundStyleObject = style;
			// although we can't have an empty style permanently, the editor can assign Nothing temporarily to do a references test
			m_StyleID = BackgroundStyleObject?.ID ?? Guid.Empty;
		}

		/// <summary>Returns the default image or text to use on a button based on its action</summary>
		/// <remarks>One of strText and image will be Nothing on exit</remarks>
		internal static void GetDisplayFromAction(Action action, out string text, out SharedImage image, Transaction transaction)
		{
			// Need to update the image with one based on the action.  To improve efficiency we try looking for a resource first
			string id = action.GetSampleImageResourceID();
			if (!string.IsNullOrEmpty(id))
			{
				image = Globals.Root.CurrentDocument.AddSharedBitmapForResource(id, transaction);
				text = "";
			}
			else if (action.Change == Parameters.Action_Character || action.Change == Parameters.Action_Text)
			{
				// don't try image for this - it would create a naff one
				text = (action as TextActionBase).Text;
				image = null;
			}
			else // no resource, see if we can create a bitmap dynamically (works, e.g., for PaletteShapes)
			{
				Bitmap bitmap = action.CreateSampleImage();
				image = null;
				if (bitmap != null)
				{
					MemoryImage objMemory = new MemoryImage(bitmap);
					image = Globals.Root.CurrentDocument.AddSharedBitmapFromMemoryImage(objMemory, transaction);
					text = "";
				}
				else if (action.Change == Parameters.Action_Key)
					// Cannot use the standard Action.DescriptionWithoutAccelerator in some cases because this complains about extraneous stuff: "Press (X)"
					text = GUIUtilities.KeyDescription((action as KeyAction).Key);
				else
					text = action.DescriptionWithoutAccelerator();
			} // Resource ID found ... else
		}

		private SharedImage CreateImageForAction(int size)
		{
			string id = Action.GetSampleImageResourceID();
			if (!string.IsNullOrEmpty(id))
			{
				Debug.Fail("CreateImageForAction shouldn\'t get a resource ID image.  Such an image is lightweight, and therefore should have been persisted and not re-generated automatically");
				return SharedImage.CreateForResource(id);
			}
			else if (Action.Change == Parameters.Action_Character || Action.Change == Parameters.Action_Text)
				return null;
			else
			{
				Bitmap bitmap = Action.CreateSampleImage(size);
				return new SharedImage(new MemoryImage(bitmap));
			}
		}
		#endregion

		#region Styling

		private Guid m_StyleID; // defined whether stored internally or in document
		private float m_TextRatio = -1; // means use auto
		[Flags]
		public enum Layouts
		{
			// location of image relative to text.  frmButton assumes these numbered 0-128
			Below = 0,
			Above,
			Right,
			Left,
			CentreBoth, // both fill button
			Superimpose = 128 // add this to make image fill, and text just gets its share within.  Text position unaffected
		}
		private Layouts m_Layout = Layouts.Above;

		private States DrawState
		{
			get // Returns a value of States which can be used as a style index
			{
				if (m_State == States.Disabled)
					return States.Normal;
				return m_State;
			}
		}

		public override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			if (BackgroundStyleObject == null)
				return null; // can happen when the main screen requests the current style for ongoing shape (and I think it might be a dummy ongoing shape) in order to update palettes
			switch (parameter)
			{
				case Parameters.LineColour:
				case Parameters.LinePattern:
				case Parameters.LineWidth:
					if (applyingDefault && BackgroundStyleObject.IsShared)
						return null;
					return BackgroundStyleObject.LineStyle[(int)DrawState];
				case Parameters.FillColour:
				case Parameters.FillPattern:
					if (applyingDefault && BackgroundStyleObject.IsShared)
						return null;
					return BackgroundStyleObject.FillStyle[(int)DrawState];
				default:
					return base.StyleObjectForParameter(parameter, applyingDefault);
			}
		}

		protected override Color GetDefaultTextColour()
		{
			return TextStyleC.DefaultTextColourOn(BackgroundStyleObject.FillStyle[(int)DrawState].Colour);
		}

		internal float TextRatio
		{
			get { return m_TextRatio; }
			set
			{
				m_TextRatio = value;
				// limit bounds:
				if (m_TextRatio < 0)
					m_TextRatio = -1;
				else if (m_TextRatio < 0.01)
					m_TextRatio = 0.01F;
				else if (m_TextRatio >= 0.99)
					m_TextRatio = 0.99F;
				ClearTextCache();
			}
		}

		internal Layouts Layout
		{
			get { return m_Layout; }
			set
			{
				m_Layout = value;
				TextStyle.VerticalAlignment = StringAlignment.Center;
				TextStyle.Alignment = StringAlignment.Center;
				// if text superimposed we want to move it right to the edge
				// layout is IMAGE position
				if (m_Layout == (Layouts.Below | Layouts.Superimpose))
					TextStyle.VerticalAlignment = StringAlignment.Near;
				else if (m_Layout == (Layouts.Above | Layouts.Superimpose))
					TextStyle.VerticalAlignment = StringAlignment.Far;
				else if (m_Layout == (Layouts.Left | Layouts.Superimpose))
					TextStyle.Alignment = StringAlignment.Near;
				else if (m_Layout == (Layouts.Right | Layouts.Superimpose))
					TextStyle.Alignment = StringAlignment.Far;
				ClearTextCache();
			}
		}

		public ButtonStyle BackgroundStyleObject { get; private set; }

		#endregion

		#region Info
		public override Shapes ShapeCode => Shapes.Button;

		public override GeneralFlags Flags
		{ get { return base.Flags | GeneralFlags.NoEmptyLabelMessage | GeneralFlags.NoColourOnTransformCopy | GeneralFlags.DoubleClickAfterCreation; } }

		public override AllowedActions Allows
		{get{return (base.Allows | AllowedActions.Tidy) & ~(AllowedActions.TransformRotate  | AllowedActions.TransformMirror | AllowedActions.Merge | AllowedActions.Arrowheads);}}

		public override LabelModes LabelMode => LabelModes.Always;

		protected override LabelPositions LabelPosition => LabelPositions.RotatedRectangle;
		// not actually rotated, but easier to just use this than create a new CustomRect value

		public override bool IsFilled => true;
		// although COULD be empty - Filled enables click inside (good even if not back filled!) and fade in on create (unimportant)

		public States State
		{
			get { return m_State; }
			set
			{
				if (m_State == value)
					return;
				m_State = value;
				Parent?.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
			}
		}

		/// <summary>Returns true if this should go to the selected state while being pressed with the mouse
		/// Returns false for any button which is permanently selectable - EG style which can be selected)</summary>
		public bool SelectDuringUserPress
		{ get { return !(Action is ValueSelectableAction); } }

		internal override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			List<Prompt> objList = new List<Prompt>();
			objList.Add(new Prompt(ShapeVerbs.Complete, "Button_Complete", "Button_Complete"));
			objList.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			return objList;
		}

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (requested == SnapModes.Angle) // not implemented, but is implicit anyway
				return SnapModes.Off;
			return base.SnapNext(requested);
		}

		public override string ToString() => $"Buttonshape (Action={Action})";

		internal override string Description => $"{Strings.Item("Shape_Button")} ({Action.DescriptionWithAccelerator()})";

		#endregion

		#region Verbs
		private static PointF g_ptStart; // in order to allow drawing out any corner first
		private const float MINSIZE = 2;
		public override VerbResult Start(EditableView.ClickPosition position)
		{
			// Style for button is most common one on this page, or last one selected
			BackgroundStyleObject = frmButton.GetStyleForNewButton(position.Page);
			m_StyleID = BackgroundStyleObject.ID;
			g_ptStart = position.Snapped;
			m_Bounds.Location = position.Snapped;
			m_Bounds.Size = new SizeF(MINSIZE, MINSIZE);
			TextStyle = new TextStyleC();
			TextStyle.SetDefaults();
			return VerbResult.Continuing;
		}

		internal override void InitialiseFreeStanding()
		{
			base.InitialiseFreeStanding();
			BackgroundStyleObject = new ButtonStyle();
			BackgroundStyleObject.Initialise();
			TextStyle = new TextStyleC();
			TextStyle.SetDefaults();
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			RectangleF rct = Geometry.RectangleFromPoints(g_ptStart, position.Snapped);
			if (rct.Width < MINSIZE || rct.Height < MINSIZE)
				return VerbResult.Rejected;
			m_Bounds = rct;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Choose(EditableView.ClickPosition position) => Complete(position);

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			VerbResult eResult = Float(position);
			if (eResult == VerbResult.Rejected)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult CompleteRetrospective()
		{
			// shouldn't really get this far - the initial Choose click will have completed - if allowed
			Utilities.LogSubError("Button.CompleteRetrospective not expected");
			return VerbResult.Continuing;
		}

		// Editing (and therefore double-click) only available in teacher mode (user mode can affect some of the controls in the editing screen, e.g. colour picker)
		internal override string DoubleClickText()
		{
			if (Globals.Root.User == Users.User)
				return "";
			return Strings.Item("Button_Edit");
		}

		internal override void DoDoubleClick(EditableView view, EditableView.ClickPosition.Sources source)
		{
			if (Globals.Root.User == Users.User)
				return;
			Transaction transaction = new Transaction();
			transaction.Edit(this);
			transaction.Edit(BackgroundStyleObject);
			LastDoubleClickResult = frmButton.Display(BackgroundStyleObject, this, transaction); // Must set this; if it is false and this was triggered on the creation of a button it will be destroyed again
			if (LastDoubleClickResult)
			{
				Globals.Root.StoreNewTransaction(transaction);
				if (Parent != null) 
					Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
			}
			else
				transaction.Cancel();
		}
		#endregion

		#region Graphics
		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			BackgroundStyleObject.PrepareResources(m_State, resources);
		}

		//protected override bool PrepareShadow(Shape.DrawResources objResources)
		//{
		//	return base.PrepareShadow(objResources);
		//}

		protected override void PrepareHighlight(DrawResources resources)
		{
			base.PrepareHighlight(resources);
			if (m_State == States.Disabled)
			{
				resources.EdgeAlpha /= 3;
				resources.FillAlpha /= 3;
			}
			resources.MainPen = resources.Graphics.CreateStroke(CurrentHighlightColour, BackgroundStyleObject.LineStyle[(int)DrawState].Width + resources.HIGHLIGHTEXTRAWIDTH);
			resources.MainPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.Highlight)
			{
				// Copied from ImportedImage
				Stroke pnHighlight = gr.CreateStroke(CurrentHighlightColour, resources.HIGHLIGHTEXTRAWIDTH);
				gr.Rectangle(m_Bounds, pnHighlight);
				pnHighlight.Dispose();
			}
			else
			{
				// background
				BackgroundStyleObject.Draw(gr, resources, m_Bounds, State); // easy!
																	// The alpha in objResources has already been adjusted by PrepareDraw
				if (DisplayFromAction && m_Image == null && m_LastAutoImageSize >= 0)
				{
					float size = Math.Max(Bounds.Width, Bounds.Height) - 1; // -1 because image area is always somewhat less about (see GetImageArea)
					size *= resources.CoordScale * resources.Scale;
					SharedImage image = CreateImageForAction((int)size);
					if (image == null)
						m_LastAutoImageSize = -1;
					else
					{
						m_LastAutoImageSize = image.GetSize().Width;
						m_Image = image;
						// The image is deliberately not added to the shared ones in the document, because we are not expecting to actually persist it
					}
				}
				if (m_Image?.HasContent ?? false)
				{
					RectangleF imageBounds = GetImageArea();
					int size = (int)(imageBounds.Width * resources.CoordScale * resources.Scale);
					if (DisplayFromAction && m_LastAutoImageSize > 0 && (m_LastAutoImageSize / size > 2 || (float)m_LastAutoImageSize / size < 0.5) && !m_Image.Content.IsLightweight)
					{
						// image was automatically obtained, but the requested size at that time is significantly different to the current size
						// check that the image itself is also substantially different to the original size (it might not have matched the original request)
						// Lightweight images don't do this, since they (at the moment) auto size internally
						SharedImage sharedImage = CreateImageForAction(size);
						if (sharedImage == null)
							m_LastAutoImageSize = -1;
						else if (sharedImage.GetSize().Width != m_Image.Content.GetSize().Width) // If you don't need to pass a size to GetImage here because that only makes a difference if the image is lightweight
						{
							m_LastAutoImageSize = sharedImage.GetSize().Width;
							m_Image = sharedImage;
							// The image is deliberately not added to the shared ones in the document, because we are not expecting to actually persist it
						}
					}
					if (m_Image.Content != null)
						GUIUtilities.CalcDestRect(m_Image.Content.GetSize(), ref imageBounds);
					if (resources.FillAlpha < 200 && !Utilities.Low_Graphics_Safe())
					{
						if (m_State == States.Disabled)
							gr.DrawImage(m_Image, size, imageBounds, GUIUtilities.GetImageAttrForGreyAlpha(resources.FillAlpha));
						else // this is unlikely
							gr.DrawImage(m_Image, size, imageBounds, GUIUtilities.GetImageAttrForAlpha(resources.FillAlpha));
					}
					else
						gr.DrawImage(m_Image, size, imageBounds);
				}
				// text drawn by DrawLabel which is called by the base class:
			}
		}

		protected override void DrawLabel(Canvas gr, DrawResources resources)
		{
			// We don't need all the complexity of the base class and formatting, and certainly no caret.  Also it is helpful to be able to translate the text
			// therefore this does the graphics itself
			if ((!string.IsNullOrEmpty(m_Label) || HasCaret) && resources.TextBrush != null)
			{
				EnsureFont(resources.CoordScale);
				gr.DrawString(Strings.Translate(LabelText), m_Font, resources.TextBrush, GetTextArea(), GUIUtilities.StringFormatCentreCentre);
			}
		}

		protected override void FormatText(int fromFragmentIndex = 0)
		{
			// Not needed, and can be called occasionally, e.g. when label is assigned
		}
		#endregion

		#region Data
		// Text is in label

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			ButtonShape button = (ButtonShape)other;
			m_Bounds = button.m_Bounds;
			m_State = button.m_State; // bit odd, but probably best copied.
			if (depth >= CopyDepth.Undo)
			{
				m_Layout = button.m_Layout;
				m_TextRatio = button.m_TextRatio;
				m_Image = button.m_Image?.Clone();
				if (depth == CopyDepth.Duplicate && button.BackgroundStyleObject != null && button.BackgroundStyleObject.IsShared == false)
				{
					BackgroundStyleObject = (ButtonStyle)button.BackgroundStyleObject.Clone(mapID);
					m_StyleID = BackgroundStyleObject.ID;
				}
				else
				{
					BackgroundStyleObject = button.BackgroundStyleObject; // share the object - it will be transacted separately if edited
					m_StyleID = button.m_StyleID;
				}
				Action = button.Action?.Clone();
				DisplayFromAction = button.DisplayFromAction;
			}
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			// State not saved
			m_State = States.Normal;
			m_Layout = (Layouts)reader.ReadInt32();
			m_TextRatio = reader.ReadSingle();
			m_Image = SharedReference<SharedImage>.FromGUID(reader.ReadGuid());
			m_StyleID = reader.ReadGuid();
			if (reader.ReadBoolean()) // is style local
			{
				BackgroundStyleObject = (ButtonStyle)reader.ReadData(FileMarkers.ButtonStyle);
				Debug.Assert(BackgroundStyleObject.IsShared == false);
			}
			if (reader.Version >= 74)
			{
				Action = Action.Read(reader);
				DisplayFromAction = reader.ReadBoolean();
			}
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write((int)m_Layout);
			writer.Write(m_TextRatio);
			if (ShouldSaveSharedImage())
				writer.Write(m_Image?.ID ?? Guid.Empty);
			else
				writer.Write(Guid.Empty);
			writer.Write(m_StyleID);
			if (BackgroundStyleObject != null && BackgroundStyleObject.IsShared == false)
			{
				writer.Write(true);
				writer.Write(BackgroundStyleObject);
			}
			else
				writer.Write(false);
			if (writer.Version >= 74)
			{
				Action.Write(writer);
				writer.Write(DisplayFromAction);
			}
		}

		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			if (BackgroundStyleObject != null && BackgroundStyleObject.IsShared && !BackgroundStyleObject.IsUserDefault)
			{
				fnAdd.Invoke(BackgroundStyleObject);
				mapID.AddUnchangedObject(BackgroundStyleObject);
			}
			// the image is not reported if we can recreate it from the action (unless it takes minimal memory, i.e. the image itself is a reference to a resource or something)
			if (m_Image != null && ShouldSaveSharedImage())
				fnAdd.Invoke(m_Image?.Content);
		}

		/// <summary>True if the image should actually be saved in the document</summary>
		private bool ShouldSaveSharedImage()
		{
			if (m_Image == null || !m_Image.HasContent)
				return false;
			return DisplayFromAction == false || Action.IsEmpty || m_Image.Content.IsLightweight;
		}

		// The image and style are not contained, it is only a shared reference - unless style is not shared
		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			if (BackgroundStyleObject != null && !BackgroundStyleObject.IsShared)
				BackgroundStyleObject.Iterate(fn);
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			m_Image?.DereferenceOnLoad(document);
			if (m_Image?.Failed ?? false)
				Utilities.LogSubError("Cannot find image used in button: " + m_Image.ID);
			if (BackgroundStyleObject == null)
			{
				BackgroundStyleObject = ButtonStyle.UserDefaultByID(m_StyleID); // Will still be nothing unless the ID was one of the defaults
				if (BackgroundStyleObject == null)
				{
					BackgroundStyleObject = document.GetButtonStyle(m_StyleID);
					if (BackgroundStyleObject == null)
					{
						Utilities.LogSubError("Cannot find document style (" + m_StyleID + ") used in button: " + ID, true);
						// can't function without it
						BackgroundStyleObject = new ButtonStyle();
						BackgroundStyleObject.Initialise();
						m_StyleID = BackgroundStyleObject.ID;
					}
					else
						Debug.Assert(BackgroundStyleObject.IsShared);
					BackgroundStyleObject.UpdateReferencesObjectsCreated(document, reader);
				}
			}
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			if (mapID.ContainsKey(m_StyleID))
			{
				m_StyleID = mapID[m_StyleID].ID;
				BackgroundStyleObject = document.GetButtonStyle(m_StyleID);
			}
			else if (BackgroundStyleObject != null && !BackgroundStyleObject.IsUserDefault) // first condition is just safety check - refetch if object available
			{
				m_StyleID = BackgroundStyleObject.ID;
				BackgroundStyleObject.UpdateReferencesIDsChanged(mapID, document);
			}
			// since these are invariant, no need to update much...
			m_Image?.UpdateIDsReferencesChanged();
		}

		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			base.AddToFlatList(list, purpose);
			if (purpose == FlatListPurpose.HasTexture)
			{
				// will be ignored by base class
				if (BackgroundStyleObject.FillStyle.Any(objStyle => objStyle.Texture != null))
					list.Add(this);
			}
		}

		protected override bool IdenticalToShape(Shape other)
		{
			ButtonShape objButtonShape = (ButtonShape)other;
			if (SharedReference<SharedImage>.ReferencesAreEqual(m_Image, objButtonShape.m_Image))
				return false;
			if (!Action.Equals(objButtonShape.Action))
				return false; // will do a value comparison as it is a structure
			if (DisplayFromAction != objButtonShape.DisplayFromAction)
				return false;
			if (!m_StyleID.Equals(objButtonShape.m_StyleID))
				return false;
			if (m_Layout != objButtonShape.m_Layout)
				return false;
			return true;
		}
		#endregion

		#region Grabspots, Coords, targets

		protected override PointF[] RectangleLabelGetPoints()
		{
			return GetTextArea().GetPoints();
		}

		private RectangleF GetTextArea()
		{
			return GetTextArea(m_Bounds, m_TextRatio, m_Image, m_Layout);
		}

		internal static RectangleF GetTextArea(RectangleF bounds, float textRatio, SharedReference<SharedImage> image, Layouts layout)
		{
			// superimpose flag extends the image under the text - has no effect on text area, which is still limited
			if (image == null)
				return bounds;
			if (textRatio < 0.01)
			{
				switch (layout & ~Layouts.Superimpose)
				{
					case Layouts.Above:
					case Layouts.Below:
						return bounds; // text drawn in entire space - to allow auto-sizing - image will try and draw beside
					default: // image made square or 50% whichever is smaller
						float imageRatio = Math.Min(0.5F, bounds.Height / bounds.Width);
						textRatio = 1 - imageRatio;
						break;
				}
			}
			textRatio = Math.Min(textRatio, 0.9F);
			RectangleF temp = bounds;
			//Debug.WriteLine("GetTextArea with layout=" + layout);
			switch (layout & ~Layouts.Superimpose)
			{
				case Layouts.CentreBoth:
					break;
				// note Layout name refers to IMAGE
				case Layouts.Above:
					temp.Y += bounds.Height * (1.0f - textRatio);
					temp.Height = bounds.Height * textRatio;
					break;
				case Layouts.Below:
					temp.Height = bounds.Height * textRatio;
					break;
				case Layouts.Left:
					temp.X += bounds.Width * (1.0f - textRatio);
					temp.Width = bounds.Width * textRatio;
					break;
				case Layouts.Right:
					temp.Width = bounds.Width * textRatio;
					break;
				default:
					Utilities.LogSubError("Unexpected ButtonShape.Layout: " + layout);
					break;
			}
			return temp;
		}

		private RectangleF GetImageArea()
		{
			if (m_TextRatio < 0.01f)
				EnsureTextFormatted();
			return GetImageArea(m_Bounds, m_TextRatio, m_Image, m_Layout, LabelText, m_Fragments);
		}

		internal static RectangleF GetImageArea(RectangleF bounds, float defaultTextRatio, SharedReference<SharedImage> image, Layouts layout, string LabelText, List<Fragment> fragments)
		{
			Debug.Assert(image != null);
			RectangleF basis = bounds;
			basis.Inflate(-0.6F, -0.6F); // roughly 2 pixels; stops this overlapping the border so badly

			if ((layout & Layouts.Superimpose) > 0 || layout == Layouts.CentreBoth || string.IsNullOrEmpty(LabelText)) //OrElse m_strLabel = "" Then
			{
				basis.Inflate(-0.8F, -0.8F); // if the image covers most of the button, we to leave more margin, otherwise it can be impossible to see the background colour (which gives selection state)
				return basis;
			}
			float textRatio = defaultTextRatio;
			if (defaultTextRatio < 0.01f)
			{
				switch (layout) // superimpose flag filtered out above
				{
					case Layouts.Above: // check size of text - image gets remainder, or 25% whichever is larger
					case Layouts.Below:
						float text = 0;
						if (fragments != null && fragments.Count > 0)
							text = fragments[fragments.Count - 1].Bounds.Bottom - fragments[0].Bounds.Top;
						textRatio = Math.Min(text / basis.Height, 0.75F);
						break;
					default: // image made square or 50% whichever is smaller
						float imageSize = Math.Min(0.5F, basis.Height / basis.Width);
						textRatio = 1 - imageSize;
						break;
				}
			}
			textRatio = Math.Min(textRatio, 0.9F);

			RectangleF temp = basis;
			switch (layout)
			{
				case Layouts.Above:
					temp.Height = temp.Height * (1.0f - textRatio);
					break;
				case Layouts.Below:
					temp.Y = temp.Y + temp.Height * textRatio;
					temp.Height = temp.Height * (1.0f - textRatio);
					break;
				case Layouts.Left:
					temp.Width = temp.Width * (1.0f - textRatio);
					break;
				case Layouts.Right:
					temp.X = temp.X + temp.Width * textRatio;
					temp.Width = temp.Width * (1.0f - textRatio);
					break;
					// else already asserted in GetTextArea
			}
			return temp;
		}

		protected override void CheckTextVerticalPositions(bool verticalAlignChanged, float extraLineSpace = 0)
		{
			//need to override as the V alignment isn't actually set as such
			if (TextTopBottom && m_Fragments.Count > 0)
			{
				// Near is top aligned - which happens automatically.  for all others need to adjust the Y positions
				float height = m_Fragments[m_Fragments.Count - 1].Bounds.Bottom - m_Fragments[0].Bounds.Top;
				float Y; // where the first must be
				if ((m_Layout & ~Layouts.Superimpose) == Layouts.Above) // remember Layout is IMAGE position
					Y = Bounds.Height - height;
				else
					Y = 0; // text top
				// rest copied from base...
				if (m_Fragments[0].Bounds.Top != Y)
				{
					for (int index = 0; index <= m_Fragments.Count - 1; index++)
					{
						m_Fragments[index].Bounds.Y = Y;
						Y += m_Fragments[index].Bounds.Height + extraLineSpace;
					}
					InvalidateTextRectangle(CalculateTextBounds());
				}
				if (HasCaret)
					CaretRealculateCoordinates(false);
			}
			else
				base.CheckTextVerticalPositions(verticalAlignChanged);
		}

		private bool TextTopBottom
		{
			get // returns true if text is pushed to edge
			{
				if (m_Layout == Layouts.Above || m_Layout == Layouts.Below)
					return m_TextRatio <= 0;
				else if (m_Layout == (Layouts.Above | Layouts.Superimpose) || m_Layout == (Layouts.Below | Layouts.Superimpose))
					return true;
				else
					return false;
			}
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> objList = new List<GrabSpot>();
			base.AddBoundingGrabSpots(objList, scale);
			return objList;
			// all these grabspots move using transforms
		}

		protected override bool AutoTargets => BackgroundStyleObject.BorderShape == ButtonStyle.BorderShapes.Rectangle;
		// otherwise no targets / snapping

		#endregion

	}

	public class ButtonStyle : Datum, IComparable<ButtonStyle>
	{

		// can be shared between buttons - because we may want a lot of buttons all looking the same, and it's quite
		// handy if they can be edited as one.  Shared buttons are stored in the document (If IsShared).  There is also one

		public Shape.LineStyleC[] LineStyle = new Shape.LineStyleC[3];
		public Shape.FillStyleC[] FillStyle = new Shape.FillStyleC[3];
		public Color[] TextColour = new Color[3]; // all other text style aspects in individual button
		public enum BorderShapes : byte
		{
			Rectangle,
			Rounded,
			Ellipse
		}
		public BorderShapes BorderShape;
		public enum ImageTypes : byte
		{
			None = 0, // only in this case are the LineStyle and FillStyle used
			Custom = 1, // uses CustomImage
			RoundButton, // some images built into code can be used
			GlassButton,
			Windows // rendered like a Windows button
		}
		public ImageTypes ImageType = ImageTypes.None;
		public ScalableImage[] CustomImage = new ScalableImage[3];
		public bool IsShared; // if true this is stored in the document and shared between buttons.  If false it is stored in the button itself
		public string Name = ""; // can be translateable - ie including []

		public void Initialise()
		{
			// must be called when creating new - to create objects in arrays.
			for (int index = 0; index <= 2; index++)
			{
				LineStyle[index] = new Shape.LineStyleC();
				LineStyle[index].SetDefaults();
				FillStyle[index] = new Shape.FillStyleC();
				FillStyle[index].SetDefaults();
				TextColour[index] = Color.Black;
			}
			FillStyle[0].Colour = Color.FromArgb(111, 111, 255);
			FillStyle[1].Colour = Color.FromArgb(55, 139, 255);
			FillStyle[2].Colour = Color.FromArgb(0, 255, 255);
		}

		public override byte TypeByte
		{ get { return (byte)FileMarkers.ButtonStyle; } }

		#region Data
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			ButtonStyle buttonStyle = (ButtonStyle)other;
			if (depth >= CopyDepth.Undo) // no change needed on Xform
			{
				for (int index = 0; index <= 2; index++)
				{
					if (LineStyle[index] == null)
						LineStyle[index] = new Shape.LineStyleC();
					if (FillStyle[index] == null)
						FillStyle[index] = new Shape.FillStyleC();
					LineStyle[index].CopyFrom(buttonStyle.LineStyle[index]);
					FillStyle[index].CopyFrom(buttonStyle.FillStyle[index]);
					TextColour[index] = buttonStyle.TextColour[index];
					if (buttonStyle.CustomImage[index] == null || buttonStyle.ImageType != ImageTypes.Custom)
					{
						// second condition avoids copying unnecessary images.  Custom images are kept if not selected, in case they are reselected - until the file is saved and reloaded
						// but don't want to start dumping copies of them everywhere
						CustomImage[index] = null;
					}
					else
					{
						if (depth == CopyDepth.Duplicate)
						{
							// duplicating a ButtonStyle is rare(?) so probably best to actually duplicate the images
							// if they're used they are the most of this style, and probably the only thing worth editing, so had better not share the objects
							CustomImage[index] = new ScalableImage();
							CustomImage[index].CopyFrom(buttonStyle.CustomImage[index], depth); //, mapID)
						}
						else
						{
							CustomImage[index] = buttonStyle.CustomImage[index]; // note contents are not changed - these are effectively invariant and Undo doesn't work on them
						}
					}
				}
				BorderShape = buttonStyle.BorderShape;
				ImageType = buttonStyle.ImageType;
				IsShared = buttonStyle.IsShared;
				Name = buttonStyle.Name;
				//If eDepth >= CopyDepth.Duplicate Then IsShared = False ' must NOT do this for copy/paste.  Think this will only be duplicated if expected
			}
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			ImageType = (ImageTypes)reader.ReadByte();
			for (int index = 0; index <= 2; index++)
			{
				LineStyle[index] = Shape.LineStyleC.Read(reader);
				FillStyle[index] = Shape.FillStyleC.Read(reader);
				TextColour[index] = reader.ReadColour();
				if (ImageType == ImageTypes.Custom)
				{
					CustomImage[index] = new ScalableImage();
					CustomImage[index].Load(reader);
				}
			}
			BorderShape = (BorderShapes)reader.ReadByte();
			IsShared = reader.ReadBoolean();
			Name = reader.ReadString();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.WriteByte((byte)ImageType); // must be before loop so that we know whether to load images or not
			if (LineStyle[0] == null || FillStyle[0] == null)
			{
				Utilities.LogSubError("Initialising ButtonStyle properties on saving - currently null");
				Initialise();
			}
			for (int index = 0; index <= 2; index++)
			{
				LineStyle[index].Write(writer);
				FillStyle[index].Write(writer);
				writer.Write(TextColour[index]);
				if (ImageType == ImageTypes.Custom)
					CustomImage[index].Save(writer);
			}
			writer.WriteByte((byte)BorderShape);
			writer.Write(IsShared);
			writer.Write(Name);
		}

		// note FillStyles can have references - so need to process these - but IDs never change for textures(?) so that one not needed
		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			foreach (Shape.FillStyleC objStyle in FillStyle)
			{
				objStyle.AddReferences(fnAdd, mapID);
			}
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			foreach (Shape.FillStyleC objStyle in FillStyle)
			{
				objStyle.UpdateReferencesObjectsCreated(document, reader);
			}
		}

		#endregion

		/// <summary>Defines the MainPen and MainBrush if needed by this style in the given state.  Will adjust both Alpha values in objResources if eState = Disabled</summary>
		/// <param name="state">Can include Disabled (which mostly uses Normal state)</param>
		/// <param name="resources"></param>
		internal void PrepareResources(ButtonShape.States state, Shape.DrawResources resources)
		{
			if (state == ButtonShape.States.Disabled)
			{
				resources.EdgeAlpha /= 3;
				resources.FillAlpha /= 3;
				state = ButtonShape.States.Normal; // so that it is an index into the styles list
			}
			if (ImageType != ImageTypes.None)
				return;

			Shape.LineStyleC style = LineStyle[(int)state];
			if (resources.EdgeAlpha > 0 && style.Colour.A > 0)
			{
				// note these create for image modes in case image missing
				if (resources.EdgeAlpha == 255)
					resources.MainPen = resources.Graphics.CreateStroke(style.Colour, style.Width);
				else
					resources.MainPen = resources.Graphics.CreateStroke(Color.FromArgb((resources.EdgeAlpha * style.Colour.A) / 255, style.Colour), style.Width);
				resources.MainPen.DashStyle = style.Pattern;
				if (style.Pattern != System.Drawing.Drawing2D.DashStyle.Solid && style.Width < 2)
				{
					// not sure if  we should take accountof scaling here.  It does not seem to matter too much.  Although a dashed 2px line
					// at 50% looks just as naff as a 1px at 100% I think it looks less wrong to the user, because at low scale you expect to lose some detail
					resources.MainPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
					resources.MainPen.DashPattern = style.GenerateCustomDashPattern();
				}
				resources.MainPen.MiterLimit = 2;
			}

			Filled.DefaultPrepareFillDraw(FillStyle[(int)state], resources, true);
			// Last parameter not reported correctly, because it is only used to allow/disallow textures
			// which we probably shouldn't have here anyway.  Through will allow textures if they are defined
		}

		internal void Draw(Canvas gr, Shape.DrawResources resources, RectangleF destination, ButtonShape.States state)
		{
			// Used to draw the background of a button.  Uses MainPen and MainBrush to draw
			// EState can be Disabled, but that is only valid for a Windows button (all of the other ones implement disabled by changing the alpha)
			if (resources.Highlight)
				return;
			ScalableImage image = null;
			var type = ImageType;
			if (type == ImageTypes.Windows)
			{
				if (!(gr is NetCanvas))
					type = ImageTypes.GlassButton; // Windows button can only be done within.net
				if (Globals.ClassicTheme)
					type = ImageTypes.RoundButton; // button type goes nuts if Windows has classic theme selected
			}
			if (type != ImageTypes.Windows)
			{
				if (state == ButtonShape.States.Disabled)
					state = ButtonShape.States.Normal;
				Debug.Assert(state >= 0 && (int)state <= 2);
			}
			if (type == ImageTypes.None)
			{ }
			else if (type == ImageTypes.Custom)
				image = CustomImage[(int)state];
			else if (type == ImageTypes.RoundButton)
				image = ScalableImage.RoundButton((int)state);
			else if (type == ImageTypes.GlassButton)
				image = ScalableImage.PaletteButton((int)state);
			else if (type == ImageTypes.Windows)
			{
				// The ButtonRenderer seems to partially ignore any transforms which have already been applied to the graphics.  In some ways this is good because it is integer only
				// Actually it gets worse; it seems to use the translation but not the scale EVEN IF the transform is reset first (!!?!!!!!!)
				// There is some difference depending on whether the transform is reset or not, but not much
				// however if the transform is reset the clipping goes nuts - I think it respects the clipping in reset coordinates, but continues to draw in its strange coordinates
				// therefore the button is never within the clipping rectangle and never gets drawn if the clipping is restricted to approximately just this button
				// AAAAAAAAAAAAAAAAAAAAAAAAAAARGH!
				Graphics grNet = ((NetCanvas)gr).Underlying;
				float S = grNet.Transform.Elements[0]; // these usually set: objResources.CoordScale * objResources.Scale, but CoordScale meaningless in thumbnails
				ButtonRenderer.DrawButton(grNet, new Rectangle((int)(destination.X * S - 1), (int)(destination.Y * S - 1), (int)(destination.Width * S + 2), (int)(destination.Height * S + 2)), false, WindowsPushButtonState(state));
				// focal highlight (penultimate parameter) however seems to draw differently, so ends up in the wrong place; so cannot use this
				return;
			}
			if (image != null) // doing it this way gives us a fallback; if there should be an image but it is missing, it will revert to the line method
			{
				System.Drawing.Drawing2D.InterpolationMode old = gr.InterpolationMode; // The glass buttons really need the better quality
				gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				image.Draw(gr, destination, resources.FillAlpha);
				gr.InterpolationMode = old;
			}
			else
			{
				if (resources.MainBrush != null || resources.MainPen != null)
				{
					switch (BorderShape)
					{
						case BorderShapes.Ellipse:
							gr.Ellipse(destination, resources.MainPen, resources.MainBrush);
							break;
						case BorderShapes.Rounded:
							float radius = (float)(Math.Sqrt(Math.Min(destination.Width, destination.Height)) * 0.8f);
							// looks odd, but gives something which is roughly elliptical at 2-3 mm
							using (System.Drawing.Drawing2D.GraphicsPath path = GUIUtilities.CreateRoundedRectPath(destination, radius))
							{
								if (resources.MainBrush != null)
									gr.Path(path, null, resources.MainBrush);
								if (resources.MainPen != null)
									gr.Path(path, resources.MainPen);
							}

							break;
						default:
							Debug.Assert(BorderShape == BorderShapes.Rectangle);
							gr.Rectangle(destination, resources.MainPen, resources.MainBrush);
							break;
					}
				}
			}
		}

		internal static System.Windows.Forms.VisualStyles.PushButtonState WindowsPushButtonState(ButtonShape.States state)
		{
			// Gets the windows style to use when rendering a button as a Windows button
			switch (state)
			{
				case ButtonShape.States.Normal: return System.Windows.Forms.VisualStyles.PushButtonState.Normal;
				case ButtonShape.States.Disabled: return System.Windows.Forms.VisualStyles.PushButtonState.Disabled;
				case ButtonShape.States.Highlight: return System.Windows.Forms.VisualStyles.PushButtonState.Hot;
				case ButtonShape.States.Selected: return System.Windows.Forms.VisualStyles.PushButtonState.Pressed;
				default:
					Utilities.LogSubError("Unexpected button state: " + state);
					return System.Windows.Forms.VisualStyles.PushButtonState.Normal;
			}
		}

		public int CompareTo(ButtonStyle other)
		{
			// Sort into name order
			return Strings.Translate(Name).CompareTo(Strings.Translate(other.Name));
		}

		public bool ContentEquals(object obj)
		{
			// does not check Name and IsShared.  This must not be Overrides Equals, because that messes up checking if an object is in a list (List.Contains uses Equals rather than checking for object references)
			if (!(obj is ButtonStyle))
				return false;
			ButtonStyle style = (ButtonStyle)obj;
			if (ImageType != style.ImageType)
				return false;
			if (BorderShape != style.BorderShape)
				return false;
			for (int index = 0; index <= 2; index++)
			{
				if (!TextColour[index].Equals(style.TextColour[index]))
					return false;
				if (ImageType == ImageTypes.Custom)
				{
					if (CustomImage[index] != style.CustomImage[index])
						return false;
				}
				else
				{
					if (!LineStyle[index].Equals(style.LineStyle[index]))
						return false;
					if (!FillStyle[index].Equals(style.FillStyle[index]))
						return false;
				}
			}
			return true;
		}

		#region User default
		// Because the buttons link directly to a style reference, in order to set a button to use a default style, we must have a single, unchanging default reference
		// which is declared here.  There is also a copy in each user's configuration, and the contents are copied in here as the user changes
		// We now have 2 defaults: one for action buttons and one for selection buttons.  Action buttons have Guid Empty, and selection ones have:
		public static readonly Guid idSelectionDefault = new Guid("00000000000000000000000000000001");
		public static ButtonStyle UserDefaultActionInstance = new ButtonStyle() { ID = Guid.Empty, IsShared = true, Name = "[Button_Style_Default_Action]", ImageType = ImageTypes.GlassButton };
		public static ButtonStyle UserDefaultSelectionInstance = new ButtonStyle() { ID = idSelectionDefault, IsShared = true, Name = "[Button_Style_Default_Selection]", ImageType = ImageTypes.RoundButton };

		/// <summary>Strictly returns whether this is any user default, not just the current one; but difference probably irrelevant</summary>
		public bool IsUserDefault
		{ get { return ID.IsEmpty() || ID.Equals(idSelectionDefault); } }

		/// <summary>Copies from the user config to the static references in this class
		/// This doesn't copy the SAW ones  - they are accessed differently.</summary>
		public static void SetDefaultOnUserChange()
		{
			Config config = Config.UserCurrent;
			EnsureConfigUserDefaultsSet(config);
			UserDefaultActionInstance.CopyFrom(config.ButtonStyle[0], CopyDepth.Undo, null);
			UserDefaultSelectionInstance.CopyFrom(config.ButtonStyle[1], CopyDepth.Undo, null);
		}

		/// <summary>Ensures Config.ButtonStyle[0] and [1] are defined.  ButtonStyle must have count>=2</summary>
		public static void EnsureConfigUserDefaultsSet(Config config)
		{
			Debug.Assert(config.ButtonStyle.Count >= 2);
			if (config.ButtonStyle[0] == null)
			{
				config.ButtonStyle[0] = new ButtonStyle { ID = Guid.Empty, IsShared = true, Name = "[Button_Style_Default_Action]" };
				config.ButtonStyle[0].Initialise();
				config.ButtonStyle[0].ImageType = ImageTypes.Windows;
			}
			if (config.ButtonStyle[1] == null)
			{
				config.ButtonStyle[1] = new ButtonStyle { ID = idSelectionDefault, IsShared = true, Name = "[Button_Style_Default_Selection]" };
				config.ButtonStyle[1].Initialise();
				config.ButtonStyle[1].ImageType = ImageTypes.RoundButton;
			}
		}

		/// <summary>Returns one of the shared UserDefault instances based on the ID, or nothing</summary>
		public static ButtonStyle UserDefaultByID(Guid ID)
		{
			if (ID.IsEmpty())
				return UserDefaultActionInstance;
			if (ID.Equals(idSelectionDefault))
				return UserDefaultSelectionInstance;
			return null;
		}

		#endregion

	}

	public class ScalableImage
	{
		// for button(style).  Records one or more images which can be drawn at different sizes
		// if multiple, then the nearest to the required size is drawn
		// not Datum, and not shared between styles as a SharedImage
		// (unlikely to be needed - only likely to be one style which uses a given image)

		public List<MemoryImage> Images = new List<MemoryImage>();
		public List<Size> Sizes = new List<Size>(); // the sizes of the images are remembered - can save generating the .net image object from the buffer
		public List<Rectangle> Slices = new List<Rectangle>(); // Any Slice which IsEmpty indicates 9-slice not used for that image
															   // Size and Slice not F versions as they are within image - which is integral

		#region Data
		public void Load(DataReader reader)
		{
			int count = reader.ReadInt32();
			while (count > 0)
			{
				Images.Add(new MemoryImage(reader));
				Sizes.Add(reader.ReadSize());
				Slices.Add(reader.ReadRectangle());
				count -= 1;
			}
		}

		public void Save(DataWriter writer)
		{
			if (m_Resources)
				throw new InvalidOperationException("Cannot save resource version of ScalableImage");
			writer.Write(Images.Count);
			for (int index = 0; index <= Images.Count - 1; index++)
			{
				Images[index].Save(writer);
				writer.Write(Sizes[index]);
				writer.Write(Slices[index]);
			}
		}

		public void CopyFrom(ScalableImage other, Datum.CopyDepth depth) //, ByVal mapID As Mapping)
		{
			if (depth >= Datum.CopyDepth.Duplicate)
			{
				Images = new List<MemoryImage>();
				Sizes = new List<Size>();
				Slices = new List<Rectangle>();
				for (int index = 0; index <= other.Images.Count - 1; index++)
				{
					Images.Add(other.Images[index].Clone());
					Sizes[index] = other.Sizes[index];
					Slices[index] = other.Slices[index];
				}
			}
		}

		#endregion

		public void AddImageFromDisc(string strFile)
		{
			MemoryImage img = new MemoryImage(strFile);
			Sizes.Add(img.Size);
			Images.Add(img); // having this after Sizes.Add means that an error in img.Size (which can load image and fail) leaves this in a consistent state
			Slices.Add(Rectangle.Empty);
		}

		#region Drawing
		internal void Draw(Canvas gr, RectangleF destination, int fillAlpha = 255)
		{
			if (!m_Resources && Images.Count == 0) // the resource versions don't have the images list
			{
				Utilities.LogSubError("Drawing ScalableImage which has no images");
				return;
			}
			SizeF target = destination.Size;
			if (gr.PageUnit == GraphicsUnit.Millimeter)
			{
				target.Width *= gr.DpiX / Geometry.INCH; // size in pixels
				target.Height *= gr.DpiY / Geometry.INCH;
			}
			else
				Utilities.ErrorAssert(gr.PageUnit == GraphicsUnit.Pixel || gr.PageUnit == GraphicsUnit.Display || gr is SVGCanvas); // don't know if this will work in SVG, but no point excepting
																																  // at the moment this ignores scaling; not sure if this is the best thing to do or not
			int image = BestMatch(target.ToSize());
			if (Slices[image].IsEmpty)
			{
				if (fillAlpha < 200 && !Utilities.Low_Graphics_Safe())
					gr.DrawImage(GetImage(image), (int)target.Width, destination, GUIUtilities.GetImageAttrForAlpha(fillAlpha));
				else
					gr.DrawImage(GetImage(image), (int)target.Width, destination);
			}
			else
				Draw9Slice(gr, destination, GetImage(image), Slices[image], fillAlpha);
		}

		public static void Draw9Slice(Canvas gr, RectangleF destination, SharedImage image, Rectangle slice, int fillAlpha = 255)
		{
			// draws a nine slice image.  RctSlice indicates the central, scaling part of the source image
			// first need to calculate the central scaling part in the destination
			RectangleF central = RectangleF.Empty; // central part WITHIN rctDestination; i.e. (0,0) is the top left of rctDestination
			float xScale = 1;
			float yScale = 1;
			if (gr.PageUnit == GraphicsUnit.Millimeter)
			{
				// need to work in editors which are pixel based as well as document which is mm based
				xScale = Geometry.INCH / gr.DpiX;
				yScale = Geometry.INCH / gr.DpiY;
			}
			central.X = slice.X * xScale;
			Size imageSize = image.GetSize(slice.BiggestDimension());
			float rightEdge = (imageSize.Width - slice.Right) * xScale; // size of the static bit on the right in gr units
			if (rightEdge + central.X > destination.Width)
			{
				// the central section has been squeezed out completely; must also reduce the left and right parts - this will keep them in proportion with the static parts in rctSlice
				central.Width = 0;
				central.X = destination.Width * slice.X / (slice.X + imageSize.Width - slice.Right);
			}
			else
				central.Width = destination.Width - rightEdge - central.X;

			central.Y = slice.Y * yScale;
			float bottomEdge = (imageSize.Height - slice.Bottom) * yScale; // size of the static bit on the Bottom in gr units
			if (bottomEdge + central.Y > destination.Height)
			{
				// the central section has been squeezed out completely; must also reduce the left and Bottom parts - this will keep them in proportion with the static parts in rctSlice
				central.Height = 0;
				central.Y = destination.Height * slice.Y / (slice.Y + imageSize.Height - slice.Bottom);
			}
			else
				central.Height = destination.Height - bottomEdge - central.Y;

			System.Drawing.Imaging.ImageAttributes attributes = null;
			if (fillAlpha < 200 && !Utilities.Low_Graphics_Safe())
				attributes = GUIUtilities.GetImageAttrForAlpha(fillAlpha);

			// we now know the necessary coordinates; we just need to draw the nine parts
			// TL, TC, TR
			DrawSingleSlice(gr, image, new RectangleF(destination.X, destination.Y, central.X, central.Y), new Rectangle(0, 0, slice.X, slice.Y), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.X, destination.Y, central.Width, central.Y), new Rectangle(slice.X, 0, slice.Width, slice.Y), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.Right, destination.Y, destination.Width - central.Right, central.Y), new Rectangle(slice.Right, 0, imageSize.Width - slice.Right, slice.Y), attributes);

			// Middle line:
			DrawSingleSlice(gr, image, new RectangleF(destination.X, destination.Y + central.Y, central.X, central.Height), new Rectangle(0, slice.Y, slice.X, slice.Height), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.X, destination.Y + central.Y, central.Width, central.Height), new Rectangle(slice.X, slice.Y, slice.Width, slice.Height), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.Right, destination.Y + central.Y, destination.Width - central.Right, central.Height), new Rectangle(slice.Right, slice.Y, imageSize.Width - slice.Right, slice.Height), attributes);

			// bottom line:
			DrawSingleSlice(gr, image, new RectangleF(destination.X, destination.Y + central.Bottom, central.X, destination.Height - central.Bottom), new Rectangle(0, slice.Bottom, slice.X, imageSize.Height - slice.Bottom), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.X, destination.Y + central.Bottom, central.Width, destination.Height - central.Bottom), new Rectangle(slice.X, slice.Bottom, slice.Width, imageSize.Height - slice.Bottom), attributes);
			DrawSingleSlice(gr, image, new RectangleF(destination.X + central.Right, destination.Y + central.Bottom, destination.Width - central.Right, destination.Height - central.Bottom), new Rectangle(slice.Right, slice.Bottom, imageSize.Width - slice.Right, imageSize.Height - slice.Bottom), attributes);
		}

		private static void DrawSingleSlice(Canvas gr, SharedImage image, RectangleF destination, Rectangle source, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			if (destination.Width <= 0 || destination.Height <= 0)
				return;
			// we can get some zero rectangles; it is possible for one of the edges not to be used; or if the destination is very small the central part disappears
			// And entirely spurious preferred size is used.  I think if this used a resource name would be for a specific size
			if (attributes != null)
				gr.DrawImage(image, 32, destination, source, attributes);
			else
				gr.DrawImage(image, 32, destination, source);
		}
		#endregion

		private int BestMatch(Size target)
		{
			// returns the index which best matches the target size
			if (target.Width == 0)
				target.Width = 1;
			if (target.Height == 0)
				target.Height = 1; // otherwise the code below would crash with division by zero errors
			float bestValue = float.PositiveInfinity;
			int bestIndex = -1;
			for (int index = 0; index <= Sizes.Count - 1; index++)
			{
				float error; // the ratio between the sizes, but always >1; i.e. larger numbers are bigger errors
				if (Sizes[index].Width >= target.Width)
					error = (float)Sizes[index].Width / target.Width;
				else
					error = (float)target.Width / Sizes[index].Width;
				if (Sizes[index].Height >= target.Height)
					error *= (float)Sizes[index].Height / target.Height;
				else
					error *= (float)target.Height / Sizes[index].Height;
				if (error < bestValue)
				{
					bestIndex = index;
					bestValue = error;
				}
			}
			return bestIndex;
		}

		/// <summary>Returns a SharedImage, not because it is stored in the document, but because we need to use either a MemoryImage or resource name</summary>
		protected virtual SharedImage GetImage(int index)
		{
			return new SharedImage(Images[index]);
		}

		#region Resource versions
		// We can return a version of these objects which returns the available RoundButton or PaletteButton images from the resources
		private bool m_Resources = false;

		private class ResourceVersion : ScalableImage
		{
			// This does not store the Images list; the Sizes list is filled in on construction
			// none of these used slices.  The sizes are passed to the constructor; it is essential that all these images actually exist
			private string m_ResourceName;
			public ResourceVersion(string resourceName, int[] sizes)
			{
				m_Resources = true;
				m_ResourceName = resourceName;
				Images = null;
				for (int index = 0; index <= sizes.Length - 1; index++)
				{
					Sizes.Add(new Size(sizes[index], sizes[index])); // all of the images are square
					Slices.Add(Rectangle.Empty);
				}
			}

			protected override SharedImage GetImage(int index)
			{
				return SharedImage.CreateForResource(m_ResourceName + "_" + Sizes[index].Width); // RM.GetObject(m_strResourceName + "_" + Sizes(index).Width.ToString)
																								 // either Width or Height could be used; all of these images are square
			}
		}

		#region Shared resource instances
		private static ResourceVersion[] g_RoundButton = new ResourceVersion[3];
		private static ResourceVersion[] g_PaletteButton = new ResourceVersion[3];

		private static string ImageSuffix(ButtonShape.States state)
		{
			switch (state)
			{
				case ButtonShape.States.Highlight: return "Hover";
				case ButtonShape.States.Selected: return "Select";
				default: return "";
			}
		}

		public static ScalableImage RoundButton(int index)
		{
			if (g_RoundButton[index] == null)
				g_RoundButton[index] = new ResourceVersion("RoundButton" + ImageSuffix((ButtonShape.States)index), new[] { 32, 38, 48, 80, 256 });
			return g_RoundButton[index];
		}

		public static ScalableImage PaletteButton(int index)
		{
			if (g_PaletteButton[index] == null)
				g_PaletteButton[index] = new ResourceVersion("CustomButton" + ImageSuffix((ButtonShape.States)index), new[] { 32 });
			return g_PaletteButton[index];
		}
		#endregion

		#endregion

	}
}
