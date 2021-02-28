using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Drawing2D;
using SAW.Functions;

// this file contains the abstract base classes for the various shapes
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace SAW
{
	public abstract partial class Shape : Datum, IDisposable
	{
		// Implements Runtime.Serialization.IDeserializationCallback
		// base class for any shape (including measuring tools on the picture)

		#region Shape types and construction and copying; classes

		public enum Shapes
		{
			// most of these numbers are kept compatible with AccessMaths 4 (see LegacyData.RecordTypes)
			// The numbers cannot be changed as they are stored in data, but the order of items is not significant, so gaps can be filled in later
			// Although this is integer it is also returned to TypeByte, so 0-255 is permitted for genuine shapes
			Null = 0, // used in tool selection to mean that there is no tool, not even the selector
			/// <summary>Only used when changing tools - uses Selector if available, otherwise Null.  ie forces current tool to be deselected</summary>
			SelectorOrNull = 1,
			Selector = 2, // can't actually create one of these - it is the selection arrow
			SelectionBox = 3,
			Line = 4,
			Orthogonal = 5,
			Arrow = 6,
			Square = 7,
			Rectangle = 8,
			Parallelogram = 9,
			Triangle = 10,
			Isosceles = 11,
			Equilateral = 12,
			Polygon = 13,
			IrregularPolygon = 14,
			Circle = 16,
			Ellipse = 17,
			Semicircle = 18,
			FreeText = 19, // closest to old one
			OrthogonalRectangle = 20,
			Rhombus = 21,
			Image = 37,
			PolyLine = 45,
			MaskedImage = 46,

			Flow = 74,
			Container = 75,
			Scissors = 77,
			Button = 78,
			/// <summary>Not in UserShapes, and cannot be created by the user.  The shape is created directly by the view, and cannot be created by CreateShape
			/// It is never selected as the current tool </summary>
			SetOrigin = 80,
			/// <summary>Not in UserShapes, and cannot be created by the user.  The shape is created directly by the UI, and cannot be created by CreateShape
			/// It is never selected as the current tool </summary>
			SetRotation = 81,
			UserSocket = 83,
			Pencil = 86,
			ClosedCurve = 88,
			Connector = 89,
			GenericPath = 91,
			/// <summary>Diverges from Splash.  A text label beside another object that it moves with </summary>
			FloatingLabel = 94,  // Splash label is 95 and virtual
			Curve = 96,
			TextLine = 97,
			Group = 99,
			// if adding... update Create function and GetClass function.  Note creatable shapes (I.e. ones which can be stored in document) must be 0-99 at the moment - see Datum.Create

			TransformMove = 100, //was 31. TransformationType returns true for numbers 100-110
			TransformMoveVH = 101, // was 32 and 33
			TransformCopy = 102, // Not often needed because TransformMove will do this in copy mode.  All of the other transformations there is only one and it changes between move/copy modes
			TransformRotate = 104,
			TransformScale = 106,
			TransformMirror = 108,
			TransformLinearScale = 110, // not actually Creatable using CreateShape, only used for GrabSpots
										// Cannot be added to tools panel; these are automatically created
			SAWItem = 113,
			Scriptable = 114,

			Undefined = 255,

			// not used in SAW, but included so that various code compiles:
			Pie = 38,
			Arc = 15,
		}

		// shapes visible to the user (omits anything which cannot be added to the toolbar)
		// In the order that we want them displayed, unless in folders etc...
		public static Shapes[] UserShapes = {Shapes.Selector, Shapes.Scriptable, Shapes.SelectionBox,
			Shapes.Line, Shapes.Orthogonal, Shapes.Arrow, Shapes.PolyLine, Shapes.Connector,
			Shapes.Square, Shapes.Rectangle, Shapes.OrthogonalRectangle, Shapes.Parallelogram, Shapes.Rhombus,
			Shapes.Triangle, Shapes.Isosceles, Shapes.Equilateral, Shapes.Polygon, Shapes.IrregularPolygon,
			Shapes.Circle, Shapes.Ellipse, Shapes.Semicircle, Shapes.Curve, Shapes.ClosedCurve, Shapes.Pencil, Shapes.GenericPath,
			Shapes.FreeText, Shapes.TextLine, Shapes.FloatingLabel,Shapes.UserSocket,Shapes.Button,
			Shapes.Container, Shapes.Flow};
		// In no particular order (and see also GetClass)
		public static Shapes[] Transformations = {Shapes.TransformCopy, Shapes.TransformLinearScale, Shapes.TransformMirror,
			Shapes.TransformMove, Shapes.TransformMoveVH, Shapes.TransformRotate, Shapes.TransformScale};

		public abstract Shapes ShapeCode { get; }

		protected Shape()
		{
			// ReSharper disable once VirtualMemberCallInConstructor
			switch (LabelMode)
			{
				case LabelModes.Always:
				case LabelModes.IntrinsicOnly:
				case LabelModes.IntrinsicTextPlusLabel:
					TextStyle = new TextStyleC();
					break;
			}
		}

		public static Shape CreateShape(Shapes shape)
		{
			// constructs an empty object for the given type of shape
			switch (shape)
			{
				case Shapes.Line: return new Line();
				case Shapes.Orthogonal: return new Orthogonal();
				case Shapes.Square: return new Square();
				case Shapes.Rectangle: return new RectangleShape();
				case Shapes.OrthogonalRectangle: return new OrthogonalRectangle();
				case Shapes.Parallelogram: return new Parallelogram();
				case Shapes.Selector: throw new ArgumentException("Cannot create \'Selector\' shape");
				case Shapes.Triangle: return new Triangle();
				case Shapes.Isosceles: return new Isosceles();
				case Shapes.Equilateral: return new Equilateral();
				case Shapes.Polygon: return new Polygon();
				case Shapes.IrregularPolygon: return new IrregularPolygon();
				case Shapes.Rhombus: return new Rhombus();
				case Shapes.Pencil: return new Pencil();
				case Shapes.PolyLine: return new PolyLine();
				case Shapes.Circle: return new Circle();
				case Shapes.Ellipse: return new Ellipse();
				case Shapes.Semicircle: return new Semicircle();
				case Shapes.Arrow: return new Arrow();
				case Shapes.SelectionBox: return new SelectionBox();
				case Shapes.Image: return new ImportedImage();
				case Shapes.Group: return new Group();

				// new shapes...
				case Shapes.Flow: return new Flow();
				case Shapes.Container: return new Container();
				case Shapes.Button: return new ButtonShape();
				case Shapes.UserSocket: return new UserSocket();
				case Shapes.ClosedCurve: return new Curve(true);
				case Shapes.Connector: return new Connector();
				case Shapes.GenericPath: return new GenericPath();
				case Shapes.FreeText: return new FreeText();
				case Shapes.TextLine: return new TextLine();
				case Shapes.Curve: return new Curve(false);
				case Shapes.MaskedImage: return new MaskedImage();
				case Shapes.FloatingLabel: return new FloatingLabel();

				// transformations ...
				case Shapes.TransformMove: return new TransformMove(Transformation.Modes.UseSystem);
				case Shapes.TransformMoveVH: return new TransformMoveVH(Transformation.Modes.UseSystem);
				case Shapes.TransformCopy: return new TransformMove(Transformation.Modes.Copy);
				case Shapes.TransformRotate: return new TransformRotate(Transformation.Modes.UseSystem);
				case Shapes.TransformScale: return new TransformScale(Transformation.Modes.UseSystem);
				case Shapes.TransformMirror: return new TransformMirror(Transformation.Modes.UseSystem);

				//SAW:
				case Shapes.SAWItem: return new Item();
				case Shapes.Scriptable: return new Scriptable();

				default:
					throw new ArgumentException("Unexpected shape: " + shape);
			}
		}

		public static Classes GetClass(Shapes shape)
		{
			// Note that this needs to be a shared function because we want to use this when doing configuration etc (i.e. without a shape)
			// it would have been nice if this could have been overridable
			if ((int)shape >= 100 && (int)shape <= 110)
				return Classes.Transformation;
			switch (shape)
			{
				case Shapes.SelectionBox:
				case Shapes.Selector:
					return Classes.Selection;
				case Shapes.SetOrigin:
					return Classes.Modifier; // Shapes.Crayon including this would prevent it receiving styles.  Seems to work OK as a real shape
				case Shapes.Null: return Classes.Null;
				default:
					return Classes.Real;
			}
		}


		/// <summary>Only applicable to UserShapes.  Returns true if the option to include the shape should not generally be shown in configuration</summary>
		internal static bool HideShapeType(Shapes shape)
		{
			Debug.Assert(UserShapes.Contains(shape));
			switch (shape)
			{
				case Shapes.Button:
				case Shapes.Scriptable:
					return true;
				default:
					return false;
			}
		}

		/// <summary>Returns true if the shape is the selector which selects on clicking (or any equivalent.  Does not include SelectionBox and similar)</summary>
		internal static bool ShapeIsDirectSelector(Shapes shape)
		{
			switch (shape)
			{
				case Shapes.Selector:
					return true;
				default: return false;
			}
		}

		internal virtual void InitialiseFreeStanding()
		{
			// shape not used in document; Start will not be called.  Some shapes need to do some init
		}

		internal bool TransformationType() => GetClass(ShapeCode) == Classes.Transformation;

		internal static bool TransformationTypeWhichSnapsFirstPoint(Shapes type)
		{
			// returns true if parameter is one of the transforms which places a focal point first (which snaps;  others select a shape and so don't snap the initial point)
			switch (type)
			{
				case Shapes.TransformRotate:
				case Shapes.TransformScale:
					return true;
				default:
					return false;
			}
		}

		internal enum FlatListPurpose
		{
			Style,
			/// <summary>must include all shapes</summary>
			DiagnosticAll,
			/// <summary>Includes all shapes, including some within groups.  This is more than the page iterator</summary>
			Translate,
			/// <summary>only lists shapes with textures (for editor to detect those in use in document)</summary>
			HasTexture,
			/// <summary>lists all shapes which are exposed to GUI;  eg includes those in containers.  Same as page iterator?</summary>
			ExposedShapes
		}

		internal static List<Shape> FlattenList(List<Shape> list, FlatListPurpose purpose)
		{
			// if the parameter is a list of top-level shapes (e.g. selected shapes on the page) then this returns a list of all shapes
			// within that list as a single list, including shapes within groups.  Where there is a group both the group itself and its contents appear in the output list
			// the purpose parameter allows containers to decide whether children should be declared or not
			// output may not contain Nothing entries
			List<Shape> newList = new List<Shape>();
			foreach (Shape shape in list)
			{
				shape.AddToFlatList(newList, purpose);
			}
			Debug.Assert(newList.All(X => X != null));
			return newList;
		}

		internal virtual void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			// See FlattenList.  Sadly can't be protected because then the container will not be able to call this again on its children
			if (purpose == FlatListPurpose.HasTexture)
				return;
			list.Add(this);
		}

		public enum Classes
		{
			Real, // Draws a genuine shape. 
			Transformation,
			Selection, // selector and selectionbox
			Modifier, // modifies existing shapes; label 
			Measure,
			Activity, // HundredGrid and NumberLine (HundredGrid is not strictly activity sub-class)
			Null
		}

		public Classes GetClass() => GetClass(ShapeCode);

		#endregion

		#region Bounding, hit test and transformation

		protected RectangleF m_Bounds = RectangleF.Empty; // the stored copy of the bounds - can be set to empty if it has not yet been calculated

		// will be set to empty after every edit
		protected abstract RectangleF CalculateBounds();

		public RectangleF Bounds
		{
			get
			{
				if (m_Bounds.IsEmpty)
					m_Bounds = CalculateBounds();
				return m_Bounds;
			}
		}

		/// <summary>this is usually the same as Bounds, but where that can be misleadingly large this should be smaller
		/// This is used by the selection box, where the bounding rectangle must fall within the selection box.  Since Bounds can be slightly bigger than the shape
		/// for some curves, this returns a smaller bounding box containing just the actual plotted vertices; the curve might go outside these bounds</summary>
		public virtual RectangleF MinimalBounds
		{
			get { return Bounds; }
		}

		/// <summary>return a rectangle which can be used to invalidate this forcing a redraw - slightly larger than nominal bounds</summary>
		/// <param name="withShadow">True if this should be expanded enough to include the Shadow used when moving shapes</param>
		public virtual RectangleF RefreshBounds(bool withShadow = false)
		{
			RectangleF rct = Bounds;
			rct.Inflate(1, 1);
			rct.Width += 1; // because rectangles may omit far edge, whereas drawing a line along it manually wouldn't
			rct.Height += 1;
			// extend for typing bounds box...
			if (HasText(true) && LabelPosition == LabelPositions.RotatedRectangle && HasCaret) // use animation to detect cursor flash
			{
				// I think the other positions are OK.  If label is within bounds by definition that is included in the above
				Geometry.Extend(ref rct, RectangleLabelGetPoints());
				//Dim aPoints As PointF() = RectangleLabelGetPoints()
				//For index As Integer = 0 To UBound(aPoints)
				//	Geometry.Extend(rct, aPoints(index))
				//Next
				rct.Inflate(1, 1); // allow for border width
			}
			return rct;
		}

		protected void EnsureBounds()
		{
			if (m_Bounds.Equals(RectangleF.Empty))
				m_Bounds = CalculateBounds();
		}

		public abstract bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled);
		// should return true if this point hits this shape.  We actually do some quite accurate tests - the previous version of AccessMaths did so
		// we can assume that the caller has already checked the bounding rectangle for speed, so this just has to do detailed checks
		// and can just return true if hitting the bounding rectangle is sufficient
		// the graphics scale is provided in scale so we can adjust any tolerances (which are usually specified in data coordinates)

		public virtual void ApplyTransformation(Transformation transformation)
		{
			// should apply the given matrix to all of its coordinates
			// each class should call through to the base class
			Debug.Assert(!(this is BoundsDefines)); // Not actually critical now with the condition on the line below
			if ((Flags & GeneralFlags.ProtectBounds) == 0)
				m_Bounds = RectangleF.Empty; // it is not valid to transform this because this rectangle is larger than the constituent shape
											 // if the shape is, for example, rotated the bounding rectangle could actually get smaller (or larger or...)
			bool reformatText = ((transformation.RequiredAllowed & (AllowedActions.TransformScale | AllowedActions.TransformRotate | AllowedActions.TransformLinearStretch)) > 0);
			ClearTextCache(reformatText); // text Matrices ALWAYS change however; param shows whether the text within TEXT COORDS also has to be reformatted
		}

		/// <summary>Returns the centre of the bounding rectangle as a default. May be overridden to return Empty if not meaningful</summary>
		public virtual PointF Centre
		{
			get
			{
				EnsureBounds();
				return new PointF((m_Bounds.Left + m_Bounds.Right) / 2, (m_Bounds.Top + m_Bounds.Bottom) / 2);
			}
		}

		/// <summary>Returns a nominal centre of the shape which looks "right" to the user, always defined</summary>
		/// <remarks>usually = Centre, but...
		/// always returns a point, using bounding box if Centre = Empty
		/// also changed by some shapes to return a more middle value if the technical centre is on the edge of the shape (eg Pie, Semi-circle) </remarks>
		public virtual PointF Middle()
		{
			PointF pt = Centre;
			EnsureBounds();
			if (pt.IsEmpty)
				pt = new PointF((m_Bounds.Left + m_Bounds.Right) / 2, (m_Bounds.Top + m_Bounds.Bottom) / 2);
			return pt;
		}

		internal virtual float[] GetRelevantAngles()
		{
			// should return any angles applicable to the subject, e.g. the angles of lines in the object
			// used for snapping the angle of a reflection transformation
			// can return null if none
			return null;
		}

		[Flags()]
		internal enum EnvironmentChanges
		{
			Settings = 1,
			User = 2, // and in this case Settings should always be set as well
			Paper = 4, // paper has been modified (particularly grid, although might actually be called for just a colour change)

			ParentReassigned = 8 // not solely called for every change the parent, e.g. when shape first being created
								 // it is called for things like grouping and ungrouping
		}

		internal virtual void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			// Called, but only on current page, if user mode changes or configuration is edited
		}

		/// <summary>Returns the possible edits given the selected part of the shape.  Base class only acts on Centre, regardless of the provided target.
		/// The shape must support Grab movement for any grab spots that are returned - the usual grab dragging functions will be used to perform any data changes that the user makes </summary>
		/// <remarks>For EdgeMoveH/V spots, the editor will use the Focus, if set, to do Width/Height logic.  Leave Focus=PointF.Empty to show X/Y values rather than W/H to user</remarks>
		public virtual (GrabSpot[], string[]) GetEditableCoords(Target selectedElement)
		{
			if (selectedElement is Target.ForGrabSpot forGrabSpot)
				switch (forGrabSpot.Grab.GrabType)
				{
					case GrabTypes.EdgeMoveH: return (new[] { forGrabSpot.Grab }, new[] { "[Width]" });
					case GrabTypes.EdgeMoveV: return (new[] { forGrabSpot.Grab }, new[] { "[Height]" });
				}
			return (new[] { new GrabSpot(this, GrabTypes.Move, Centre) }, new[] { "[SAW_Coord_Centre]" });
		}

		#endregion

		#region Verbs

		// the main actions that the user can perform when drawing a shape
		public enum VerbResult
		{
			Continuing = 1, // action accepted, modifying the shape continues
			Unchanged, // like Continuing, but indicates that no change was made and no redrawing is necessary
			Completed, // accepted, the shape is now complete and cannot accept any further addition
			Destroyed, // cancel change was accepted, the shape has been completely cancelled
			Rejected, // this verb was not applicable
			Unexpected, // the shape is in an unexpected state for this verb - essentially a programme error.  Should be displayed to the user as Rejected,

			// but the calling code should assert/log (the shape itself won't to avoid repetition)
			Spawn, // like Completed, but the shape would like to immediately generate another shape (which will be in state Continuing)

			// the Spawn method will be called by View to get the new shape shortly.  Start will not be called on the new shape
			// behaviour is different for transformations - this causes the GUI to store the current transaction, but keep processing (used when storing a nonfinal copy)
			TransformationComplete, // like complete but for transformations - no solidify and the transformation itself is destroyed
			Custom = 9, // view must undertake custom actions.  If used by a completed Measure in response to Cancel, it unfixes so that it becoems current again

			Substitute = 11 // this shape should be removed, and another shape substituted in its position
							// Spawn method is used to get the new shape.  The replacement shape can be a new one.  At the moment the new one is treated as complete rather than ongoing
		}

		public abstract VerbResult Choose(EditableView.ClickPosition position);
		public abstract VerbResult Start(EditableView.ClickPosition position); // like Choose, but this is used for the first point - which can usually be plummeted a different place in the class hierarchy
		public abstract VerbResult Complete(EditableView.ClickPosition position);

		public abstract VerbResult CompleteRetrospective(); // previously provided point should be considered the last one

		// note that CompletePerspective can be called even if the last Choose returned Completed (Pie chart requires this)
		public abstract VerbResult Cancel(EditableView.ClickPosition position);

		public abstract VerbResult Float(EditableView.ClickPosition position); // mostly returns Continuing or Unchanged
																			   // bolSnapToShapes is true if shape snapping is on; in most cases the point will already have been snapped by the view

		/// <summary>Used only for certain verbs which can be triggered both when the shape is ongoing (being created) and later</summary>
		/// <param name="position">should report the current mouse position, although this is not generally used.
		/// The variable is used to get the Page and Zoom sometimes however</param>
		/// <param name="code">Verbs are Increment, Decrement, CountUpOne, CountDownOne, and all vertex editing</param>
		/// <returns>Return Rejected if this verb is not used by this shape</returns>
		/// <remarks>AllowVerbWhenComplete may be required in order to receive verbs once completed</remarks>
		public virtual VerbResult OtherVerb(EditableView.ClickPosition position, Functions.Codes code)
		{
			Debug.Assert(code == Functions.Codes.Increment || code == Functions.Codes.Decrement ||
						 code >= Functions.Codes.AddVertex && code <= Functions.Codes.CornerVertex);
			return VerbResult.Rejected;
		}

		public virtual Shape Spawn()
		{
			// if the shape returned Spawn in response to the Choose/Complete/CompleteRetrospective verbs than the editor will call this afterwards to get the new shape
			// that must be created
			Debug.Fail("Spawn not expected for shape type: " + ShapeCode);
			return null;
		}

		public virtual VerbResult CombinedKey(Keys key, char ch, Page page, bool simulated, EditableView fromView)
		{
			// the GUI combines the KeyDown and KeyPress events and sends one call to this. The focal shape gets this call before any other part of the GUI
			// returning Unexpected from this just means the key has been ignored, it is not really an error condition like it is with the main verbs
			// note that returning Unchanged now means that the key is swallowed
			// any return value except Unexpected/Rejected (and possibly Custom) indicates that the key has been processed
			if (HasCaret)
			{
				// check keycode first (these are more specific - the character handler will just deal with all keys eventually)
				if (key == Keys.Escape)
				{
					CaretLose(); //  will stop typing - note doesn't actually deselect on page
					Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
					return VerbResult.Continuing;
				}
				else if (key == Keys.Delete)
				{
					TextTypeDelete(true); // Delete now deletes forwards, except at end where it deletes back
					return VerbResult.Continuing;
					// For all of the following if shift is pressed then the selection is moved instead
				}
				else if (key == Keys.Left || key == (Keys.Left | Keys.Shift))
				{
					CaretMove(-1, (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}
				else if (key == Keys.Right || key == (Keys.Right | Keys.Shift))
				{
					CaretMove(Convert.ToInt32(+1), (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}
				else if (key == Keys.Up || key == (Keys.Up | Keys.Shift))
				{
					CaretMoveLine(-1, (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}
				else if (key == Keys.Down || key == (Keys.Down | Keys.Shift))
				{
					CaretMoveLine(1, (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}
				else if (key == Keys.Home || key == (Keys.Home | Keys.Shift))
				{
					CaretHomeEnd(-1, (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}
				else if (key == Keys.End || key == (Keys.End | Keys.Shift))
				{
					CaretHomeEnd(1, (key & Keys.Shift) > 0);
					return VerbResult.Continuing;
				}

				// now try the character.  should these be able to start typing automatically
				if (ch == '\0' || !HasText(true))
					return VerbResult.Unexpected;
				// ignore the key if we don't have a cursor (otherwise this shape will swallow all sorts of functional keys (e.g. shape selection) whenever it is selected)
				if (m_Label == null)
					m_Label = "";
				if (ch == '\b')
					return TextTypeDelete(false); // Delete is above (that gets no character)
				else if (ch == '\r')
					return TextType("\r");
				else if (ch >= '\u0020')
					return TextType(ch.ToString());
			}
			return VerbResult.Unexpected;
		}

		/// <summary>if the user clicks with the selector on an existing shape this is called.  Ignored by most shapes.  Selection is done by the view</summary>
		public virtual VerbResult ClickExisting(EditableView.ClickPosition position)
		{
			if (HasText(false) && HasCaret)
			{
				TextLocation location = TextHitLocation(position.Exact);
				if (location.IsValid)
					SetCaretLocation(location, true);
			}
			return VerbResult.Unchanged;
		}

		/// <summary>Return true if shape still wants to recieve this verb when completed.  Only main 5 allowed and ChooseExisting</summary>
		public virtual bool AllowVerbWhenComplete(Functions.Codes code)
		{
			Debug.Assert(code >= Functions.Codes.Choose && code <= Functions.Codes.ChooseExisting);
			return false;
		}

		/// <summary>Called after shape has been completed, and any animation complete, and added to page.  Will only rarely need to be overridden</summary>
		public virtual void OnConclude(Transaction transaction)
		{
		}

		/// <summary>Called when the shape is selected or not within the page.  Most shapes don't need to react to this; display and invalidation is done by the page</summary>
		public virtual void NotifySelectedChanged(bool selected)
		{
			// The selection state is not recorded by the shape typically.  Most shapes don't know that they are selected; they appear selected because DrawHighlight is called a
			// this is used by NumberArcLine, which selects sections of itself
		}

		#endregion

		#region Drawing

		internal static readonly Color HIGHLIGHTCOLOUR1 = Color.FromArgb(130, 220, 220, 80);
		internal static Color HIGHLIGHTCOLOUR2 = Color.FromArgb(110, 255, 110, 110);
		internal static Color CurrentHighlightColour = Color.FromArgb(130, 220, 220, 80); // is animated between 2 colours above

		protected internal sealed class DrawResources : IDisposable // sealed due to limited implementation of IDisposable
		{
			// we would like to split the creation of the pens and the actual drawing, because mostly we can create different pens
			// in the base class for drawing the shape or drawing the highlight, but leave the actual graphics code the same.
			// I started off by storing the pens and brushes as members, but this is becoming quite inefficient (almost every shape has at least three
			// references to potentially unmanaged objects).  So now this class is used to remember them during the lifespan of the Draw function
			public int FillAlpha; // and we may as well store some other information in here
			public int EdgeAlpha;
			public readonly float Scale;
			/// <summary>Scale factor applied to correct for leaving graphics object in pixels, giving nominal mm drawing.  Excludes the effect of Scale</summary>
			public readonly float CoordScale;
			/// <summary>The number of graphics units to use for 1 mm.  (Typically 1 in Splash)</summary>
			public readonly float Millimetre;

			public bool ViewIsFocal; // true if the view we are drawing in is focal.  Also false drawing shadows etc - in fact almost anything except main buffer just puts false here

			// Note that HasCaret remains true when the view has lost the focus.
			public bool IsTypingShape; // true if this is the shaping towards which typing is directed (NG for example doesn't use Caret)

			// NOTE: none of the above values are set when drawing highlight (they just default to 255, 255, 1, false)
			public Canvas Graphics; // although this is still usually passed as a parameter to the drawing functions

			// it isn't passed to the Prepare functions, which occasionally needs it for scaling purposes; not worth going back removing the gr parameter
			// from the drawing functions (especially as objResources.Graphics is rather long winded inside those functions! so the gr alias is quite useful)
			/// <summary>Is currently drawing the highlight</summary>
			public bool Highlight;

			/// <summary>If defined then preferably only this element should be drawn.  Only used with highlight.  If not supported whole shape should be drawn.
			/// (Used with editing single vertices to show selection;  if single vertex editing doesn't make sense in a shape it works fine that the whole shape is shown as highlighted)</summary>
			public Target SingleElement;

			public Stroke MainPen;
			public Fill MainBrush;
			public Fill TextBrush;
			public Stroke TextBoundsPen;
			public Stroke CustomPen;
			public Fill CustomBrush;
			public Font Font; // the default in this class is to store the font permanently; if other shapes want to do so temporarily it can be placed here
			public StaticView.InvalidationBuffer Buffer; // not really needed, but can be useful for diagnosing graphics problems to set breakpoints/assertions only for certain buffers

			public DrawResources(float scale, int fillAlpha, int edgeAlpha, bool viewIsFocal, bool isTypingShape, Canvas gr, StaticView.InvalidationBuffer buffer, float coordScale)
			{
				FillAlpha = fillAlpha;
				EdgeAlpha = edgeAlpha;
				Scale = scale;
				ViewIsFocal = viewIsFocal;
				IsTypingShape = isTypingShape;
				Highlight = false;
				Graphics = gr;
				Buffer = buffer;
				CoordScale = coordScale;
				Millimetre = gr.DpiX * Geometry.MILLIMETRE / Geometry.INCH / coordScale;
			}

			/// <summary>Highlight constructor</summary>
			public DrawResources(Canvas gr, float scale, float coordScale)
			{
				// Highlight version
				FillAlpha = 255;
				EdgeAlpha = 255;
				Scale = scale;
				ViewIsFocal = false;
				Highlight = true;
				Graphics = gr;
				Buffer = StaticView.InvalidationBuffer.Selection;
				CoordScale = coordScale;
				Millimetre = gr.DpiX * Geometry.MILLIMETRE / Geometry.INCH / coordScale;
			}

			#region Scaling info - matches same commands in GUIUtilities
			/// <summary>There is a matching static copy in GUIUtilities which assumes the scaling for the main UI and can be used for refresh measurement (might be wrong when printing) </summary>
			public float SHADOWXOFFSET
			{ get { return 1 * Millimetre; } }

			/// <summary>There is a matching static copy in GUIUtilities which assumes the scaling for the main UI and can be used for refresh measurement (might be wrong when printing) </summary>
			public float SHADOWYOFFSET
			{ get { return 2 * Millimetre; } }

			/// <summary>the amount to increase the pen width when drawing the selection highlight
			/// There is a matching static copy in GUIUtilities which assumes the scaling for the main UI and can be used for refresh measurement (might be wrong when printing) </summary>
			public float HIGHLIGHTEXTRAWIDTH
			{ get { return 1.1f * Millimetre; } }
			/// <summary>There is a matching static copy in GUIUtilities which assumes the scaling for the main UI and can be used for refresh measurement (might be wrong when printing) </summary>
			public float SHADOWEXTRAWIDTH
			{ get { return 1 * Millimetre; } }

			/// <summary>There is a matching static copy in GUIUtilities which assumes the scaling for the main UI and can be used for refresh measurement (might be wrong when printing) </summary>
			public float SELECTIONBORDERWIDTH
			{ get { return (Geometry.THINLINE + 0.5f) * Millimetre + HIGHLIGHTEXTRAWIDTH; } }

			#endregion

			#region IDisposable Support

			private bool m_IsDisposed;

			public void Dispose()
			{
				if (!m_IsDisposed)
				{
					MainPen?.Dispose();
					MainBrush?.Dispose();
					TextBrush?.Dispose();
					TextBoundsPen?.Dispose();
					CustomPen?.Dispose();
					CustomBrush?.Dispose();
					Font?.Dispose();
				}
				m_IsDisposed = true;
				//GC.SuppressFinalize(Me)
			}

			#endregion

		}

		internal virtual void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			// intAlpha should be applied to the fill (presumably if we are using transparent colours later, the transparency should be adjusted accordingly
			// e.g. transparency * intAlpha/255).  But at the moment only solid colours are used
			// this is used when shapes are being moved/initially placed
			// not usually overridden - it is usually best to override the ones below
			// pnlView can be nothing - used for focus checking etc; so when printing, use Nothing
			// bolIsFocal is true if this shape is the one where typing will currently go
			// note gr.PageUnit will be Pixel (ish), but there is a scale factor applied so that drawing is done nominally in mm (because PageUnit=mm fails under Mono)
			// however does cause some problems whereby the system's conversion of points to our coords will be wrong
			// coordScale is the scale factor thus applied
			bool viewIsFocal = false;
			bool isTypingShape = false;
			if (view != null)
			{
				viewIsFocal = view.Focused;
				isTypingShape = (view.TypingShape() == this);
			}
			DrawResources resources = new DrawResources(scale, fillAlpha, edgeAlpha, viewIsFocal, isTypingShape, gr, buffer, coordScale);
			PrepareDraw(resources);
			try
			{
				InternalDraw(gr, resources);
				DrawLabel(gr, resources);
			}
			finally
			{
				resources.Dispose();
				ReleaseDraw();
			}
		}

		internal virtual void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			// draw the transparent copy with highlight border for the shape
			// selected shapes are drawn three times:
			// - they are drawn normally in the background
			// - then there is a separate buffer over the top in which Draw is called with a low alpha value
			//   (which makes a shape hidden behind others faintly visible)
			// - followed by DrawHighlight (which draws the yellow/orange glowing highlight)
			// not usually overridden
			DrawResources resources = new DrawResources(gr, scale, coordScale);
			if (singleElement != null && singleElement.Type != Target.Types.GrabSpot)
				resources.SingleElement = singleElement;
			PrepareHighlight(resources);
			try
			{
				InternalDraw(gr, resources);
			}
			finally
			{
				resources.Dispose();
				ReleaseDraw();
			}
		}

		internal virtual void DrawShadow(Canvas gr, float scale)
		{
			DrawResources resources = new DrawResources(scale, 255, 255, false, false, gr, StaticView.InvalidationBuffer.SelectionHighlightOnly, 1); // CoordScale won't matter - only really used for text
			if (PrepareShadow(resources))
			{
				try
				{
					gr.TranslateTransform(resources.SHADOWXOFFSET, resources.SHADOWYOFFSET);
					InternalDraw(gr, resources);
				}
				finally
				{
					gr.EndTransform();
					resources.Dispose();
					ReleaseDraw();
				}
			}
		}

		protected virtual void PrepareDraw(DrawResources resources) // create any pens or brushes
		{
			DefaultPrepareDrawForText(resources);
		}

		/// <summary>Default Implementation of PrepareDraw from the Shape class.  Extracted so that subclasses can access it separately from intermediate layers
		/// All classes should either call this or base.PrepareDraw</summary>
		protected void DefaultPrepareDrawForText(DrawResources resources)
		{
			if (!HasText(false))
				return;
			int edge = resources.EdgeAlpha;
			if (string.IsNullOrEmpty(m_Label) && LabelMode != LabelModes.IntrinsicTextPlusLabel && LabelMode != LabelModes.IntrinsicOnly && this.ShapeCode != Shapes.FreeText
			) // faint cursor bit odd for free text: there's nothing else to indicate it is there
				edge /= 3;
			if (edge == 0)
				return; // can crash trying to draw text with A=0, so this is essential
			Color c = TextStyle.Colour;
			if (c.A == 0)
				c = GetDefaultTextColour(); // c.a=0 is safer than IsEmpty
			resources.TextBrush = resources.Graphics.CreateFill(Color.FromArgb(edge * c.A / 255, c));
			if (LabelMode != LabelModes.IntrinsicOnly && resources.ViewIsFocal && HasCaret && LabelPosition != LabelPositions.TextShape)
			{
				// intrinsic only shapes and specific Text shapes can draw the typing bounds themselves if they want to
				resources.TextBoundsPen = resources.Graphics.CreateStroke(Color.LightGray, 1); // THINLINE)
				resources.TextBoundsPen.DashStyle = DashStyle.Dot;
			}
		}

		protected virtual void PrepareHighlight(DrawResources resources) // prepare to draw the shape highlighted.  It is released again using the standard ReleaseDraw
		{
			// all classes must call through to this base, even though it doesn't do anything yet
		}

		protected virtual bool PrepareShadow(DrawResources resources)
		{
			// called if the page wants this to draw a shadow before drawing the actual shape.  Return true if this is supported
			// in which case this will be followed by InternalDraw and ReleaseDraw
			// DrawShadow will apply a transformation so that the shadow appears in the correct place; the shape should just draw in the usual way
			return false;
		}

		// note that during InternalDraw the shape should not assume that any pens or brushes necessarily exist - there are different Prepare functions used to draw in different circumstances

		protected abstract void InternalDraw(Canvas gr, DrawResources resources);

		protected virtual void ReleaseDraw() // release the pens and brushes
		{
			// all classes must call through to the base
		}

		protected virtual Color GetDefaultTextColour()
		{
			// used if colour is empty
			return Color.Black;
		}

		#endregion

		#region Basic information, parentage, status, flags, Allows

		// AutostartType and Classes in Shape types region

		/// <summary>used by the container (whether page or Group) to remember the order of the shapes</summary>
		/// <remarks>
		/// (in order to avoid slow array searches).  Does not need to be stored when the shape is saved (to file, clipboard string, or undo buffer)
		/// because this can be arbitrarily rebuilt from the list stored by the container
		/// </remarks>
		internal int Z;

		public IShapeParent Parent;

		// also  maintained by the container.  Not stored in data, but should be stored in the undo buffer
		/// <summary>Parent cast to IShapeContainer for convenience.</summary>
		/// <remarks>Will fail if not inside a IShapeContainer; however this is mainly used for selected shapes which cannot be encapsulated inside something else
		/// (shapes directly within a stack or group etc are not generally accessible from outside)
		/// Note that this is the Parent object; AsContainer returns (nominally) this object as a container</remarks>
		public IShapeContainer Container
		{
			get
			{
				if (Parent == null)
				{
					Utilities.LogSubError("Shape.Parent is nothing in container", false, true);
					Globals.NotifyInvalidCurrentState(); // object is probably
				}
				return (IShapeContainer)Parent;
			}
		}

		/// <summary>Returns IShapeContainer for this item, if it supports it.  The interface might not be on this object (to support shapes which optionally accept contents).
		/// Note that AsContainer returns (nominally) this object; Container returns the parent of this object</summary>
		public virtual IShapeContainer AsContainer
		{
			get { return this as IShapeContainer; }
		}

		/// <summary>Returns the IShapeTarget interface for this shape if it supports it.  Interface might not be on this object (to support shapes which optionally accept content, or delegate it)</summary>
		public virtual IShapeTarget AsTarget => this as IShapeTarget;

		/// <summary>Test if shape is within the parent, with any number 0+ of intervening levels</summary>
		public bool IsWithin(IShapeParent parent)
		{
			Shape test = this;
			while (test != null)
			{
				if (test.Parent == parent)
					return true;
				if (!(test.Parent is Shape))
					return false;
				test = (Shape)test.Parent;
			}
			return true;
		}

		/// <summary>Test if shape is within any shape in list with any number of intervening levels.  Unlike IsWithin(IShapeContainer) this can only accept shapes as candidate containers </summary>
		public bool IsWithin(List<Shape> list) // note must not be declared IEnumerable<Shape> - since IShapeParent inherits from that, so the other could not be called unambiguously
		{
			Shape test = this;
			while (true)
			{
				if (!(test.Parent is Shape parentShape)) // if parent is page then it must have failed
					return false;
				if (list.Contains(test.Parent as Shape))
					return true;
				test = parentShape;
			}
		}

		public virtual bool IsFilled => false;

		/// <summary>True if the shape is rendered.  may be false due to !Shown</summary>
		public virtual bool IsVisible => true;

		#region Diagnostic etc
		internal static string DiagnosticFormat = "0.#";

		[Conditional("DEBUG")]
		public virtual void Diagnostic(System.Text.StringBuilder output)
		{
			// shape should write any/all diagnostic information into output
			// start with call to mybase
			output.Append("Shape = ").Append(ShapeCode).Append("; ");
			output.AppendLine(this.GetHashCode().ToString("x"));
			output.Append("Z = ").AppendLine(Z.ToString());
			RectangleDiagnostic(output, m_Bounds);
			output.Append("ID = ").AppendLine(ID.ToString());
			output.Append("Parent = ").AppendLine(((Datum)Parent)?.ID.ToString() ?? "** UNDEFINED **");
			output.Append("Status =").AppendLine(Status.ToString());
			if (!string.IsNullOrEmpty(m_Label))
				output.Append("Label = ").AppendLine(m_Label);
			if (m_Links != null)
			{
				output.AppendLine("Linked to: ");
				foreach (Link objLink in m_Links)
				{
					if (objLink.Shape == null)
						output.AppendLine("NULL");
					else
					{
						output.Append(objLink.Shape.ShapeCode).Append("/").Append(objLink.Shape.ID);
						output.Append(" [").Append(objLink.Index.ToString()).AppendLine("]");
					}
				}
				output.AppendLine("------");
			}
		}

		[Conditional("DEBUG")]
		protected void RectangleDiagnostic(System.Text.StringBuilder output, RectangleF rct)
		{
			output.Append("{").Append(rct.X.ToString(DiagnosticFormat)).Append(", ").Append((-rct.Bottom).ToString(DiagnosticFormat));
			output.Append(" x ").Append(rct.Width.ToString(DiagnosticFormat)).Append(", ").Append(rct.Height.ToString(DiagnosticFormat));
			output.AppendLine("}");
		}

		[Conditional("DEBUG")]
		protected void PointDiagnostic(System.Text.StringBuilder output, PointF pt)
		{
			output.Append("(").Append(pt.X.ToString(DiagnosticFormat)).Append(", ").Append((-pt.Y).ToString(DiagnosticFormat));
			output.AppendLine(")");
		}

		[Conditional("DEBUG")]
		protected void PathDiagnostic(System.Text.StringBuilder output, GraphicsPath pth)
		{
			foreach (var element in PathElement.WithinPath(pth))
			{
				if (element.IsStart)
				{
					output.Append("Starts @ ");
					PointDiagnostic(output, element.Points[0]);
				}
				if (element.Type == Lined.PATHLINE)
				{
					output.Append("Line to ");
					if (element.IsClosure)
						output.Append("(closure) ");
					PointDiagnostic(output, element.Points[1]);
				}
				else if (element.Type == Lined.PATHBEZIER)
				{
					output.AppendLine("Bezier...");
					output.Append("  ");
					PointDiagnostic(output, element.Points[1]);
					output.Append("  ");
					PointDiagnostic(output, element.Points[2]);
					output.Append("  ");
					PointDiagnostic(output, element.Points[3]);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>Returns string describing the shape which can be displayed in the status line.
		///  ongoing = true if the shape is still being drawn; otherwise it is assumed to be static
		/// shapes can return "" if there isn't much meaningful to describe</summary>
		public virtual string StatusInformation(bool ongoing)
		{
			return "";
		}

		#endregion

		internal Page FindPage()
		{
			// Navigates up using Parent objects until it finds a page
			IShapeParent parent = Parent;
			while (!(parent is Page))
			{
				if (parent == null)
					return null;
				if (parent is EditableView editableView)
					return editableView.CurrentPage;
				if (!(parent is Shape))
					return null;
				parent = ((Shape)parent).Parent;
			}
			return (Page)parent;
		}

		#region Flags and Allows
		[Flags()]
		public enum GeneralFlags
		{
			None = 0,
			/// <summary>Shape should include this if the current shape should be stored even if the user apparently abandons without completing it</summary>
			/// <remarks>
			/// (usually used by the text shapes which will store the shape so far)
			/// CompleteRetrospective will be called by the View
			/// </remarks>
			StoreIfAbandoned = 1,
			/// <summary>Set if the shape wants to be continually refreshed during creation, even if the user has not moved the mouse</summary>
			/// <remarks>(used by the number grid as it draws the selection type boundary, which must pulsate)</remarks>
			ContinuousRefreshDuringCreation = 2,
			/// <summary>Some shapes always reset to selector, even if not configured that way</summary>
			AlwaysResetToolAfterCreation = 4,
			/// <summary>A few shapes never reset, even if so configured (number grid)</summary>
			DisallowResetToolAfterCreation = 8,
			/// <summary>New shapes should not be left selected</summary>
			NoSelectAfterCreate = 16,
			/// <summary>Used by splatter sometimes; it causes new shapes to be added behind everything else</summary>
			AddAtBack = 32,
			/// <summary>Included if transform copies should not have their colours modified (groups and NumberLine - which uses a different style class so would crash)</summary>
			NoColourOnTransformCopy = 64,
			/// <summary>An existing, selected shape should be asked for prompts just as if drawing a new shape</summary>
			PromptExistingAsNew = 128,
			/// <summary>Default for DrawNewText is to draw a different prompt if the text is empty (and it hides the cursor). This blocks that behaviour, and just starts with a cursor (simple text uses this)</summary>
			NoEmptyLabelMessage = 256,
			/// <summary>If set DoDoubleClick is run after creating each (eg to open editing screen).  It is essential that the double-click sets Shape.LastDoubleClickResult</summary>
			DoubleClickAfterCreation = 0x200,
			/// <summary>Bounds should not be cleared, they are critical.  This flag not used everywhere at the moment</summary>
			ProtectBounds = 0x800,
			/// <summary>Set if this cannot be put inside containers (e.g. selection box)</summary>
			NotWithinContainer = 0x1000,
			/// <summary>Should be returned by Page.FindSingleActivity (currently only NumberLine)</summary>
			Activity = 0x2000,
			/// <summary>If this flag is included in the shape no intersection testing methods will be called on the shape, with this shape as a parameter.
			/// (Equivalent to overriding all the methods and leaving them empty)</summary>
			NoIntersections = 0x4000,
			/// <summary>Choose events should be sent on mouse down not mouse up</summary>
			/// <remarks>(Used by eraser which is auto-started, but if there are pixels the mouse down might be effectively the start of a drag).</remarks>
			ChooseFallingEdge = 0x8000,
			/// <summary>Set on any shape which requires a fixed number of entries in m_Links (basically Connector).  If so any failed link is replaced with null rather than being deleted from list</summary>
			NumberLinksFixed = 0x20000,
			/// <summary>Usually a new shape is added to the page if it is Degenerate.  This flag ignores that check</summary>
			IgnoreDegenerateWhenCreating = 0x40000
		}
		public static bool LastDoubleClickResult; // true if OK, false if cancel; only used at the moment if DoubleClickAfterCreation

		public virtual GeneralFlags Flags
		{ get { return GeneralFlags.None; } }

		public bool Flag(GeneralFlags eFlag)
		{
			return (Flags & eFlag) > 0;
		}

		// actions are always listed as allowed even if they are somewhat redundant, e.g. rotating a circle
		// this is because many of the transformations work around an external focus point, therefore moving the shape somewhat
		[Flags()]
		public enum AllowedActions
		{
			/// <summary>entire shape can be moved</summary>
			TransformMove = 1,
			/// <summary>adjusting X and Y size by the same amount</summary>
			TransformScale = 2,
			TransformRotate = 4,
			TransformMirror = 8,
			TransformLinearStretch = 16,
			/// <summary>Merge outlines to make a new complex shape</summary>
			Merge = 32,
			Group = 64,
			/// <summary>if set receives key presses.  These don't necessarily turn into text</summary>
			Typing = 128,
			/// <summary>This is mainly used internally to work out whether we need to store the arrowhead data.</summary>
			Arrowheads = 256,
			/// <summary>shape can be tidied (to grid etc) after creation.  Defaults set in Sequential</summary>
			Tidy = 0x1000,
			PermittedArea = 0x2000, // if included this shape can be used to specify where the user can create content
									// default is on for Filled shapes.  The actual Protect flag indicates whether this shape is an allowed area; this just shows whether the shape supports this
			MirrorFlipOnly = 0x4000, // yes if the shape can be flipped horizontally or vertically, but cannot be arbitrarily mirrored (which effectively requires rotation)
									 // This is ignored if TransformMirror is set.  If checking if flipping is allowed, then check for either this or TransformMirror
			/// <summary>Shape can be converted to an GenericPath</summary>
			ConvertToPath = 0x8000,
			/// <summary>Can be converted to pixels.</summary>
			/// <remarks>Currently no distinction between explicit user request and auto action when Always_Pixels - so this is rejected for some shapes
			/// which could in theory allow pixelation on user request</remarks>
			MoveToPixels = 0x10000,

			None = 0,
			All = 0xFFFFFF,
			/// <summary> = All - Arrowheads - PaletteShapeStyleOnly - Tidy - PermittedArea</summary>
			Standard = All - Arrowheads - Tidy - PermittedArea - MirrorFlipOnly - ConvertToPath,
			Transformations = 31
		}

		public virtual AllowedActions Allows
		{
			get
			{
				if (!HasText(true))
					return AllowedActions.Standard & ~AllowedActions.Typing;
				return AllowedActions.Standard;
			}
		}
		#endregion

		/// <summary>a shape should return true if it is empty and can be discarded
		/// this mechanism is used to tidy up some text items, in particular NumberGrid cells which have been left behind empty</summary>
		internal virtual bool Degenerate
		{ get { return false; } }

		internal virtual string Description
		{
			get
			{
				if (!string.IsNullOrEmpty(LabelText))
					return "\"" + LabelText + "\"";
				return Strings.Item("Shape_" + ShapeCode);
			}
		}

		/// <summary>Used for SAW export.  Writes the description of this to the output.  Only relevant to SAW items, emits nothing for simple graphical elements</summary>
		public virtual void WriteExportText(IndentStringBuilder output)
		{ }

		#endregion

		#region Snapping, sockets

		public enum SnapModes : byte
		{
			Off,
			Grid,
			Shape, // snaps to lines vertices and intersections
			Socket, // snap to only the sockets returned by shapes
			Angle // snap to 15deg increments
		}

		/// <summary>Should return the snapping mode to use for the current point.  Mode for Start controlled by EditableView.EffectiveSnap</summary>
		/// <param name="requested">Should be neither Off nor Socket (the request is not made if Off)</param>
		public virtual SnapModes SnapNext(SnapModes requested)
		{
			Debug.Assert(requested == SnapModes.Off || requested == SnapModes.Grid || requested == SnapModes.Shape || requested == SnapModes.Angle, "Unexpected SnapMode in Shape.SnapNext - if adding modes must check all overrides of this function");
			return requested;
		}

		public virtual PointF DoSnapAngle(PointF newPoint)
		{
			// only called in SnapMode = Angle (and shouldn't be called if this rejects that mode under SnapNext)
			// must return the modified ptNew
			Debug.Fail("DoSnapAngle was not implemented");
			return newPoint;
		}

		internal struct Socket
		{
			// a location at which the connecting line can attach.  Also used for shape snapping groups together
			public Shape Shape;
			public int Index; // used by the creating shape to identify which of the possible sockets this is
			public const int AUTOMATIC = -1;
			// by convention Index = -1 indicates the centre of the shape, meaning that the user is not specifying the attachment point
			// which will be automatically chosen as the one nearest to the other end.  If the shape is asked for the position of the socket it always returns the centre of the shape
			public PointF Position;

			public Socket(Shape shape, int index, PointF position)
			{
				Shape = shape;
				Index = index;
				Position = position;
			}

			public static Socket Empty = new Socket(null, 0, PointF.Empty);

			public bool IsEmpty
			{ get { return Shape == null && Index == 0 && Position.IsEmpty; } }

		}

		internal virtual PointF SocketPosition(int index)
		{
			// returns PointF.empty if unavailable.  Should return centre of shape if -1
			if (index == -1)
				return Middle();
			return PointF.Empty;
		}

		internal virtual List<Socket> GetSockets()
		{
			// Can return nothing if none.  This returns actual sockets for the connector tool
			return null;
		}

		internal virtual SizeF SocketExitVector(int index)
		{
			// must return a vector that a connector can use to exit the shape from this socket.  The length of the vector is not important
			// only the angle is used
			Debug.Fail("SocketExitVector not implemented in " + ShapeCode);
			return new SizeF(1, 0); // 0, 0 would probably crash
		}

		public virtual bool Tidy(SnapModes mode, Page page)
		{
			// shouldn't be called with Off/Socket
			// only called for shapes with Allows And Tidy
			// returns true if shape was altered
			Utilities.LogSubError("Tidy not implemented for " + this.ShapeCode);
			return false;
		}

		#endregion

		#region Datum - mostly must be overridden
		// the Load and Save functions will be overridden at intermediate classes - the final classes should call MyBase.Load etc

		public override byte TypeByte => (byte)ShapeCode;

		private protected List<Guid> m_IDsForLinks; // only used when loading. Ids which need to be attached to objects later
		public override void Load(DataReader reader)
		{
			// reads label info.  Overridden functions must all call Mybase.Load...
			base.Load(reader);
			bool label = reader.ReadBoolean();
			if (label)
			{
				m_Label = reader.ReadString();
				if (string.IsNullOrEmpty(m_Label))
					m_Label = null;
				else if (reader.Version < 105)
				{
					// there were still some naked LF in swedish resources, which then didn't type correctly
					m_Label = m_Label.Replace("\n", "\r");
				}
				TextStyle = TextStyleC.Read(reader);
			}
			int links = reader.ReadInt32();
			if (links > 0)
			{
				m_Links = new List<Link>();
				m_IDsForLinks = new List<Guid>();
				// ReSharper disable RedundantAssignment
				for (int index = 1; index <= links; index++)
				{
					// ReSharper restore RedundantAssignment
					m_IDsForLinks.Add(reader.ReadGuid());
					m_Links.Add(new Link(null, reader.ReadInt32()));
				}
			}
			reader.ReadInt32(); // protect
			Status = StatusValues.Complete;
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			if (HasText(false)) // this will always be true for text types
			{
				// the shape needs some text support; it is possible it is intrinsic text and the label is still blank
				// there is no real point separating the two parts; it is efficient enough to just filter out all of the bog standard shapes with no text whatsoever
				writer.Write(true);
				writer.Write(m_Label ?? "");
				TextStyle.Write(writer);
			}
			else
				writer.Write(false);
			if (m_Links != null)
			{
				writer.Write(m_Links.Count);
				foreach (Link objLink in m_Links)
				{
					writer.Write(objLink.Shape?.ID ?? Guid.Empty);
					writer.Write(objLink.Index);
				}
			}
			else
				writer.Write(0);
			writer.Write(0xcf); // protect
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Shape shape = (Shape)other;
			//Debug.WriteLine("Copy from " + other.GetType() + ", depth=" + depth + ", ID=" + other.ID);
			if (depth >= CopyDepth.Undo)
			{
				// copy text.  TextStyle may be undefined in either shape
				if (shape.TextStyle != null)
				{
					if (TextStyle == null || TextStyle == shape.TextStyle)
						TextStyle = new TextStyleC();
					TextStyle.CopyFrom(shape.TextStyle);
				}
				Parent = shape.Parent;
				Z = shape.Z;
				if (shape.m_Links == null)
					m_Links = null;
				else
				{
					m_Links = new List<Link>();
					m_Links.AddRange(shape.m_Links);
				}
			}
			else
			{
				if (TextStyle == null)
					TextStyle = shape.TextStyle;
				m_Links = shape.m_Links;
			}
			m_Label = shape.m_Label; // may be nothing
		}

		/// <summary>Updates reference to other, NON-CONTAINED objects</summary>
		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			if (m_IDsForLinks == null)
				return;
			int index = 0;
			while (index <= m_IDsForLinks.Count - 1)
			{
				Guid idOther = m_IDsForLinks[index];
				if (idOther.Equals(Guid.Empty))
				{
					// is a reference to a Nothing shape.  M_colLinks already contains entry with shape = nothing as that is used as a placeholder when loading
				}
				else if (reader.LoadedObjects.ContainsKey(idOther))
				{
					Shape shape = (Shape)reader.LoadedObjects[idOther];
					m_Links[index] = new Link(shape, m_Links[index].Index);
				}
				else
				{
					if ((Flags & GeneralFlags.NumberLinksFixed) == 0)
					{
						// otherwise the link to Nothing will be left in place
						m_IDsForLinks.RemoveAt(index);
						m_Links.RemoveAt(index);
						index -= 1; // will be increased below
					}
					Debug.Fail("Failed to dereference link from object " + ShapeCode + "/" + ID + " to " + idOther);
				}
				index += 1;
			}
			m_IDsForLinks = null; // not needed once the references have been resolved
		}

		/// <summary>Updates reference to other, NON-CONTAINED objects</summary>
		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			if (m_Links == null) return;
			for (int index = 0; index <= m_Links.Count - 1; index++)
			{
				Shape shape = m_Links[index].Shape;
				if (shape != null) // a link to a Nothing shape is allowed, and will not be affected by this
				{
					if (mapID.ContainsKey(shape.ID))
					{
						// The target object is in the list of new IDs
						shape = (Shape)mapID[shape.ID];
						m_Links[index] = new Link(shape, m_Links[index].Index);
					}
					else
					{
						// the target object does not exist in the new list;  - set shape to nothing
						m_Links[index] = new Link(null, 0);
					}
				}
			}
		}

		public sealed override bool IdenticalTo(Datum other)
		{
			// want a comparison in base clase to check they are even the same shape!
			// also want to detect shapes which have not overridden, so this calls another method!
			Shape shape = (Shape)other;
			if (ShapeCode != shape.ShapeCode)
				return false;
			if (!IdenticalForPaletteDelta)
			{
				if (!m_Bounds.Equals(shape.m_Bounds))
					return false;
			}
			if (m_Label != shape.m_Label)
				return false;
			if (TextStyle != null && !TextStyle.IdenticalTo(shape.TextStyle))
				return false;
			// doesn't change links, since it seems unlikely there will be any in the things this is currently used for
			return IdenticalToShape(shape);
		}

		protected virtual bool IdenticalToShape(Shape other)
		{
			Debug.Fail("IdenticalToShape not implemented for shape: " + ShapeCode);
			return false;
		}

		#endregion

		#region Targets
		/// <summary>can return nothing if no points are available
		/// The source position is expressed as a socket to allow angle/gender/class constraints.  Most shapes ignore this and just use .Centre</summary>
		internal virtual List<Target> GenerateTargets(UserSocket floating)
		{
			return null;
		}

		internal virtual void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// the pen can be modified by this function - it will be discarded after each target
			// intActivePhase is < 0 if not active
			Debug.Fail("A shape has returned a line Target, without overriding DrawLineTarget");
		}

		internal virtual List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			// when moving this shape (either dragging or using the transform tool), this returns a list of vertices to which shape snap can be applied
			// points are expressed as UserSockets;  extra info is redundant for regular shape snap, but is used when snapping sockets in one group
			// to sockets in another group; including angle matching
			// default is nothing, which means there are no such points
			return null;
		}

		#endregion

		/// <summary>Returns the equivalent graphics path for the shape. May return null if not supported</summary>
		public virtual GraphicsPath ConvertToPath() => null;

		#region Vertex editing
		/// <summary>Returns whether the particular verb can be applied to this shape, given the target for where the user clicked.
		/// If true the verb will be sent to OtherVerb.  The caller will already have checked that objTarget is of the right basic type.
		/// This needs to report true if the shape supports the functionality, and if the segment is appropriate in other ways (e.g. not ConvertToBezier if it is already a Bezier)</summary>
		public virtual bool VertexVerbApplicable(Functions.Codes code, Target target)
		{
			Debug.Assert(target.Shape == this);
			Debug.Assert(target.Type == Target.Types.Line || target.Type == Target.Types.Vertex
						 || (target.Type == Target.Types.GrabSpot && (target as Target.ForGrabSpot)?.Grab.GrabType == GrabTypes.Move));
			switch (code)
			{
				case Functions.Codes.AddVertex:
				case Functions.Codes.ConvertToBezier:
				case Functions.Codes.ConvertToLine:
					//Debug.Assert(target.Type == Target.Types.Line);
					break;
				case Functions.Codes.RemoveVertex:
				case Functions.Codes.SmoothVertex:
				case Functions.Codes.CornerVertex:
					//Debug.Assert(target.Type == Target.Types.Vertex);
					break;
				default:
					Debug.Fail("Unexpected verb in VertexVerbApplicable.  Check all overridden functions");
					break;
			}
			return false;
		}

		#endregion


		#region IDisposable Support
		// cannot rely on this being called.  This is only used in the entire document is disposed (otherwise even deleted shapes will exist in the undo buffer)

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_Font?.Dispose();
				m_Font = null;
			}
		}

		~Shape()
		{
			Dispose(false);
		}

		#endregion

		#region Style classes and support
		// styling information stored in one or more of the following classes for each shape.  Each shape can only have one instance of each class
		// the styles can be updated using the same parameter system that the GUI uses (see IParameterGUI)
		public abstract class StyleBase
		{
			public abstract int ParameterValue(Parameters parameter);
			public abstract void SetParameterValue(int value, Parameters parameter);

			public abstract void CopyFrom(StyleBase other);
			public virtual void AddReferences(Action<Datum> fnAdd, Mapping mapID)
			{
			}

			public virtual void UpdateReferencesObjectsCreated(Document document, DataReader reader)
			{
			}

			/// <summary>Must return the parameters which are used by this object.  Individual shapes can still
			/// not return this object for those parameters, but this object must accept ParameterValue on these parameters</summary>
			public abstract Parameters[] ApplicableParameters();

			public abstract bool IdenticalTo(StyleBase other);
		}
		// *** if adding new parameters, remember to update StyleObjectForParameter in the derived classes

		#region Text
		public class TextStyleC : StyleBase
		{

			public float Size;
			public Color Colour; // Empty means use default
			public StringAlignment Alignment;
			public StringAlignment VerticalAlignment;
			public string Face = "";
			public FontStyle Style = FontStyle.Regular;

			public void SetDefaults()
			{
				Size = 16;
				Colour = Color.Empty;
				Alignment = StringAlignment.Center;
				VerticalAlignment = StringAlignment.Center;
				Face = "";
				Style = FontStyle.Regular;
			}

			public void Write(DataWriter writer)
			{
				writer.Write(Size);
				writer.Write(Colour);
				writer.Write(Convert.ToByte(Alignment)); // although it is declared as an integer enum, the only values which we exist are 0, 1, 2
				writer.Write(Convert.ToByte(VerticalAlignment));
				writer.Write(Face);
				writer.Write(Convert.ToInt32(Style));
			}

			public static TextStyleC Read(DataReader reader)
			{
				TextStyleC style = new TextStyleC();
				style.Size = reader.ReadSingle();
				style.Colour = reader.ReadColour();
				style.Alignment = (StringAlignment)reader.ReadByte();
				style.VerticalAlignment = (StringAlignment)reader.ReadByte();
				style.Face = reader.ReadString(); // is the font is not available it seems to revert to MS Sans Serif
				style.Style = (FontStyle)reader.ReadInt32();
				return style;
			}

			public override void CopyFrom(StyleBase other)
			{
				TextStyleC style = (TextStyleC)other;
				Size = style.Size;
				Colour = style.Colour;
				Alignment = style.Alignment;
				VerticalAlignment = style.VerticalAlignment;
				Face = style.Face;
				Style = style.Style;
			}

			// will respond to LineColour - specialist text shapes allow this to also control text colour
			public override int ParameterValue(Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.TextColour:
					case Parameters.LineColour:
						return Colour.ToArgb();
					case Parameters.TextAlignment:
						return (int)Alignment;
					case Parameters.TextVerticalAlignment:
						return (int)VerticalAlignment;
					case Parameters.FontSize:
						return (int)(Size * 100); // to make it an integer
					case Parameters.FontFace:
						return frmFont.IndexOfFontFace(Face);
					case Parameters.FontStyle:
						return (int)Style;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						return 0;
				}
			}
			public override void SetParameterValue(int value, Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.TextColour:
					case Parameters.LineColour:
						Colour = Color.FromArgb(value);
						break;
					case Parameters.TextAlignment:
						Alignment = (StringAlignment)value;
						break;
					case Parameters.TextVerticalAlignment:
						VerticalAlignment = (StringAlignment)value;
						break;
					case Parameters.FontSize:
						Size = value / 100f;
						break;
					case Parameters.FontFace:
						Face = frmFont.FontFaceFromIndex(value);
						break;
					case Parameters.FontStyle:
						Style = (FontStyle)value;
						break;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						break;
				}
			}

			public void ApplyFont(Font font)
			{
				// stores settings based upon the given .net font (doesn't affect all settings)
				Face = font.Name;
				Size = font.SizeInPoints;
				Style = font.Style;
			}

			public static Color DefaultTextColourOn(Color background)
			{
				if (background.A < 200)
					return Color.Black;
				if (background.B + background.G * 2 + background.R < 4 * 90) // dark colour

					return Color.White;
				return Color.Black;
			}

			private static Parameters[] Applicable = { Parameters.TextColour, Parameters.TextAlignment, Parameters.TextVerticalAlignment, Parameters.FontSize, Parameters.FontStyle, Parameters.FontFace };
			public override Parameters[] ApplicableParameters()
			{
				return Applicable;
			}

			public override bool IdenticalTo(StyleBase other)
			{
				TextStyleC objTextStyle = (TextStyleC)other;
				if (Size != objTextStyle.Size)
					return false;
				if (!Colour.Equals(objTextStyle.Colour))
					return false;
				if (Alignment != objTextStyle.Alignment)
					return false;
				if (VerticalAlignment != objTextStyle.VerticalAlignment)
					return false;
				if (Face != objTextStyle.Face)
					return false;
				if (Style != objTextStyle.Style)
					return false;
				return true;
			}

			/// <summary>Creates the font with configured styles.  </summary>
			/// <param name="size">Size must be provided because in most cases there are coordinate conversions causing scaling of the size</param>
			public Font CreateFont(float size)
			{
				var style = Style;
				if (string.IsNullOrEmpty(Face))
					return new Font(FontFamily.GenericSansSerif, size, style);
				return new Font(Face, size, style);
			}

			public FontFamily GetFontFamily()
			{
				if (string.IsNullOrEmpty(Face))
					return FontFamily.GenericSansSerif;
				return new FontFamily(Face);
			}

		}

		/// <summary>ensures that the given reference is defined and points to the correct default for this style
		/// if it points to a font which does not match, that font is disposed
		/// Note equation does its own version of this - if new properties are added, that may need to be updated as well</summary>
		/// <param name="coordScale">sizeReduce used because most graphics are drawn with an extra scale factor applied - page unit is pixels, but most graphics
		/// logic working on mm.  We need to reduce the requested size by this factor
		/// Explicitly specify sizeAdjust = 1 to get a size in points.  omitted gives default correction factor determined by looking at current document (ideally page really)
		/// Specifying parameter may be faster, and is provided as rendering functions are given it</param>
		public void EnsureFont(float coordScale = -1)
		{
			if (coordScale < 0)
				coordScale = Globals.Root.CurrentDocument.ApproxUnitScale;// Geometry.MILLIMETRE * GUIUtilities.SystemDPI / Geometry.INCH;
			float sizeNeeded = TextStyle.Size / coordScale;
			if (m_Font != null)
			{
				if (m_Font.SizeInPoints != sizeNeeded || m_Font.Style != TextStyle.Style || !string.IsNullOrEmpty(TextStyle.Face) && TextStyle.Face != m_Font.Name)
				{
					//Debug.WriteLine("Disposing of " + m_objFont.Name + " " + m_objFont.Size.ToString("0.0"));
					m_Font.Dispose();
					m_Font = null;
				}
			}
			if (m_Font == null)
				m_Font = TextStyle.CreateFont(sizeNeeded);
		}

		#endregion

		#region Line
		public class LineStyleC : StyleBase
		{
			public Color Colour;
			/// <summary>Actual graphical width.  Not *100</summary>
			public float Width;
			// although we use the .net patterns (which seem to match the Windows API ones) we only use these for drawing if the line is reasonably wide
			// if the line is only one pixel wide (after scaling?) the spacing is so small as to make the line look almost continuous
			// therefore we sometimes generate custom patterns
			public DashStyle Pattern; // = Drawing2D.DashStyle.Solid

			public void SetDefaults()
			{
				Colour = Color.Black;
				Width = Convert.ToSingle(ParameterSupport.StandardLineWidths[2] / 100); // Geometry.SINGLELINE
				Pattern = DashStyle.Solid;
			}

			public void Write(DataWriter writer)
			{
				Debug.Assert(Width >= 0);
				if (Width < 0)
					Width = 0; // otherwise it won't load back.  Splash 2 didn't permit 0
				writer.Write(Colour);
				writer.Write(Width);
				writer.Write((int)Pattern);
			}

			public static LineStyleC Read(DataReader reader)
			{
				LineStyleC newStyle = new LineStyleC();
				newStyle.Colour = reader.ReadColour();
				newStyle.Width = reader.ReadSingle();
				newStyle.Pattern = (DashStyle)reader.ReadInt32();
				// mainly used as a check that the data has not become un-synchronised
				if (newStyle.Width < 0 || newStyle.Width > 100)
					throw new InvalidDataException("LineStyleC.Width out of bounds");
				return newStyle;
			}

			public override void CopyFrom(StyleBase other)
			{
				LineStyleC objLineStyle = (LineStyleC)other;
				Width = objLineStyle.Width;
				Colour = objLineStyle.Colour;
				Pattern = objLineStyle.Pattern;
			}

			internal float[] GenerateCustomDashPattern()
			{
				// .net requires an array of singles giving the mark - space lengths
				float[] list;
				switch (Pattern)
				{
					case DashStyle.Dash:
						list = new float[] { 4, 2 };
						break;
					case DashStyle.DashDot:
						list = new float[] { 4, 2, 2, 2 };
						break;
					case DashStyle.DashDotDot:
						list = new float[] { 4, 2, 2, 2, 2, 2 };
						break;
					case DashStyle.Dot:
						list = new float[] { 2, 2 };
						break;
					default:
						list = new float[] { 1 };
						Debug.Fail("Unexpected parameter to GenerateCustomDashPattern");
						break;
				}
				const float ratio = 1.5f;
				for (int index = 0; index <= list.Length - 1; index++)
				{
					list[index] *= ratio;
				}
				return list;
			}

			public override int ParameterValue(Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.LineColour: return Colour.ToArgb();
					case Parameters.LinePattern: return (int)Pattern;
					case Parameters.LineWidth: return (int)(Width * 100); // the GUI parameter only uses integers
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						return 0;
				}
			}
			public override void SetParameterValue(int value, Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.LineColour:
						Colour = Color.FromArgb(value);
						break;
					case Parameters.LinePattern:
						Pattern = (DashStyle)value;
						break;
					case Parameters.LineWidth:
						Width = value / 100f;
						break;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						break;
				}
			}

			public override bool Equals(object obj)
			{
				if (!(obj is LineStyleC))
					return false;
				LineStyleC lineStyle = (LineStyleC)obj;
				if (Width != lineStyle.Width)
					return false;
				if (Pattern != lineStyle.Pattern)
					return false;
				if (!Colour.Equals(lineStyle.Colour))
					return false;
				return true;
			}

			private static readonly Parameters[] Applicable = { Parameters.LineColour, Parameters.LinePattern, Parameters.LineWidth };
			public override Parameters[] ApplicableParameters() => Applicable;

			public override bool IdenticalTo(StyleBase other)
			{
				LineStyleC lineStyle = (LineStyleC)other;
				if (!Colour.Equals(lineStyle.Colour))
					return false;
				if (Width != lineStyle.Width)
					return false;
				if (Pattern != lineStyle.Pattern)
					return false;
				return true;
			}

		}
		#endregion

		#region Fill
		public class FillStyleC : StyleBase
		{
			public Color Colour; // or Empty if not filled
			public enum Patterns
			{
				// mostly we will use the System.Drawing.Drawing values, but this only specifies hatches.  We need values for solid and empty
				Solid = -1,
				Empty = -2,
				Vertical = HatchStyle.Vertical,
				Horizontal = HatchStyle.Horizontal,
				ForwardDiagonal = HatchStyle.ForwardDiagonal,
				BackwardDiagonal = HatchStyle.BackwardDiagonal,
				Cross = HatchStyle.Cross,
				DiagonalCross = HatchStyle.DiagonalCross,
				// If adding new options, may need to update PDF.CreateHatchingBitmap
				Texture,
				Last = Texture
			}
			public Patterns Pattern;
			public static Color DEFAULTCOLOUR = Color.FromArgb(111, 111, 255);
			private SharedImage m_Texture; // image to use for background, usually nothing
			private Guid m_idTexture = Guid.Empty;

			public void SetDefaults()
			{
				Colour = DEFAULTCOLOUR;
				Pattern = Patterns.Solid;
			}

			public void Write(DataWriter writer)
			{
				writer.Write(Colour);
				writer.Write((int)Pattern);
				if (Pattern == Patterns.Texture)
					writer.Write(m_idTexture);
				Debug.Assert(Pattern >= (Patterns)(-2) && Pattern <= Patterns.Last);
			}

			public static FillStyleC Read(DataReader reader)
			{
				FillStyleC create = new FillStyleC();
				create.Colour = reader.ReadColour();
				create.Pattern = (Patterns)reader.ReadInt32();
				if (create.Pattern == Patterns.Texture)
					create.m_idTexture = reader.ReadGuid();
				if (create.Pattern < (Patterns)(-2) || create.Pattern > Patterns.Last)
					throw new InvalidDataException("FillStyleC.Pattern out of bounds");
				return create;
			}

			public override void CopyFrom(StyleBase other)
			{
				FillStyleC fillStyle = (FillStyleC)other;
				Colour = fillStyle.Colour;
				Pattern = fillStyle.Pattern;
				m_Texture = fillStyle.m_Texture;
				m_idTexture = fillStyle.m_idTexture;
			}

			public override int ParameterValue(Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.FillColour:
						return Colour.ToArgb();
					case Parameters.FillPattern:
						return (int)Pattern;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						return 0;
				}
			}

			public override void SetParameterValue(int value, Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.FillColour:
						Colour = Color.FromArgb(value);
						if (Pattern == Patterns.Texture)
							Pattern = Patterns.Solid;
						break;
					case Parameters.FillPattern:
						Pattern = (Patterns)value;
						if (Pattern != Patterns.Texture)
						{
							m_idTexture = Guid.Empty;
							m_Texture = null;
						}
						break;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						break;
				}
			}

			public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
			{
				if (Pattern == Patterns.Texture)
				{
					m_Texture = document.FindExistingSharedResource<SharedImage>(m_idTexture);
					if (m_Texture == null) // either m_idTexture was empty or texture not found
						Pattern = Patterns.Solid;
				}
			}

			public override void AddReferences(Action<Datum> fnAdd, Mapping mapID)
			{
				if (m_Texture != null)
				{
					fnAdd.Invoke(m_Texture);
					mapID.AddUnchangedObject(m_Texture);
				}
			}

			public SharedImage Texture
			{
				get { return m_Texture; }
				set
				{
					m_Texture = value;
					if (m_Texture == null)
					{
						m_idTexture = Guid.Empty;
						if (Pattern == Patterns.Texture)
							Pattern = Patterns.Empty;
					}
					else
					{
						m_idTexture = m_Texture.ID;
						Pattern = Patterns.Texture;
					}
				}
			}

			public override bool Equals(object obj)
			{
				if (!(obj is FillStyleC))
					return false;
				FillStyleC objFillStyle = (FillStyleC)obj;
				if (Pattern != objFillStyle.Pattern)
					return false;
				if (!m_idTexture.Equals(objFillStyle.m_idTexture))
					return false;
				if (!Colour.Equals(objFillStyle.Colour))
					return false;
				return true;
			}

			private static Parameters[] Applicable = new Parameters[] { Parameters.FillColour, Parameters.FillPattern };
			public override Parameters[] ApplicableParameters()
			{
				return Applicable;
			}

			public override bool IdenticalTo(StyleBase other)
			{
				FillStyleC objFillStyle = (FillStyleC)other;
				if (!Colour.Equals(objFillStyle.Colour))
					return false;
				if (Pattern != objFillStyle.Pattern)
					return false;
				if (!m_idTexture.Equals(objFillStyle.m_idTexture))
					return false;
				return true;
			}
		}
		#endregion

		public virtual StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			// Returns nothing if this styling information does not apply to this shape
			// Second parameter is true if this call to apply the default styles (shapes, in particular button, can lie and return Nothing if they don't want certain default styles to be applied)
			// (This is likely to apply to certain styles and not others, so styling overall is not blocked)
			switch (parameter)
			{
				case Parameters.TextColour:
				case Parameters.FontSize:
					if (HasText(false))
						return TextStyle;
					break;
				case Parameters.TextAlignment: // actual text shapes might want to overwrite this to block access to alignments
					if (HasText(false))
						return TextStyle;
					break;
				case Parameters.TextVerticalAlignment:
					if (HasText(false) && LabelPosition != LabelPositions.Line)
						return TextStyle;
					break;
			}
			return null;
		}

		public virtual void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			// the GUI calls this after modifying the style (NOT when applying the initial style at about the same time as the Start verb)
			if (parameter == Parameters.FontSize) // also used if font itself is changed
			{
				if (HasText(true) && LabelPosition != LabelPositions.Bounds && LabelPosition != LabelPositions.RotatedRectangle && (Flags & GeneralFlags.ProtectBounds) == 0)
				{
					// many labels are constrained within a certain rectangle, so changing the font does not change the bounding box at all
					// if changes to intrinsic text will change the bounding box, it is up to the shape itself to override this method
					m_Bounds = RectangleF.Empty;
				}
				FormatText();
				if (HasCaret)
					CaretRealculateCoordinates(false);
			}
			else if (parameter == Parameters.TextAlignment && HasText(true))
			{
				if (HasCaret)
					CaretRealculateCoordinates(false);
				RepositionAllFragments(); // blocks will need moving left or right at least.  But FormatText is not required
			}
			else if (parameter == Parameters.TextVerticalAlignment)
			{
				if (LabelPosition == LabelPositions.RotatedRectangle || LabelPosition == LabelPositions.Bounds)
					CheckTextVerticalPositions(true); // likewise, but only rectangle might adjust vertically
			}
		}

		/// <summary>This is called after the GUI applies the default styles; a few objects might want to force different values</summary>
		/// <remarks>must return true if it has made any changes</remarks>
		internal virtual bool DefaultStylesApplied()
		{
			return false;
		}

		public void CopyAllStylesFrom(Shape other)
		{
			//	Debug.Assert(other.Shape = Me.Shape) ' cutting now uses this, and the shape types might not match
			for (Parameters style = Parameters.FirstStyle; style <= Parameters.LastStyle; style++)
			{
				StyleBase styleBase = StyleObjectForParameter(style);
				if (styleBase != null)
				{
					StyleBase objOtherStyle = other.StyleObjectForParameter(style);
					if (objOtherStyle != null)
						styleBase.SetParameterValue(objOtherStyle.ParameterValue(style), style);
				}
			}
		}

		#endregion

		#region Grab spots and related
		// The graphics code for these is in EditableView

		#region Enum and Classes
		public enum GrabTypes
		{
			Undefined,
			/// <summary>moves entire shape</summary>
			Move,
			/// <summary>rotates entire shape about centre</summary>
			Rotate,
			/// <summary>moves a vertex independently of others</summary>
			SingleVertex,
			/// <summary>changes radius of circles and similar</summary>
			Radius,
			/// <summary>not actually movable; this is displayed as a fixed point in order to clarify the behaviour of the other points</summary>
			Fixed,
			/// <summary>move vertically</summary>
			EdgeMoveV,
			/// <summary>move horizontally</summary>
			EdgeMoveH,
			/// <summary>always maintains aspect ratio</summary>
			CornerResize,
			/// <summary>behaviour determined by shape</summary>
			Invisible,
			/// <summary>only used when moving (there is never a GrabSpot of this type).  Used when dragging out a part of the text to select it</summary>
			TextSelection,
			/// <summary>Grab handle in traditional Bezier editing style.  ShapeParameter will indicate whether it is first (+1) or second (-1) withn pathelement</summary>
			Bezier,
			/// <summary>Similar to Bezier, but not currently usable.  Drawn in grey</summary>
			BezierInactive

		}

		public class GrabSpot
		{
			public readonly GrabTypes GrabType;
			/// <summary>Position of the GrabSpot itself (before movement when inherited in GrabMovement)</summary>
			public PointF Position;
			/// <summary>Used by shape if necessary to further specify exact item being moved</summary>
			public int ShapeIndex;
			/// <summary>Can also be used by the shape.  Not passed to constructor (is only rarely used as GrabSpots are usually at corners so don't need to specify part way down a line)</summary>
			/// <remarks>Used in path to record whether GrabSpot is handle before or after vertex</remarks>
			public float ShapeParameter;
			/// <summary>only used for certain types, e.g. rotation (this isn't really used during the move, but the graphics needs to know the centre of rotation)
			/// Include for grab handles to record the position of the vertex it came from</summary>
			public PointF Focus;
			/// <summary>true if we just snap the mouse position for shape snap (usually for SingleVertex, but ruler also uses this)</summary>
			public bool SimpleShapeSnap;
			/// <summary>original shape on which this GrabSpot appeared; maybe nothing.  GrabSpot will act on all selected shapes, not just this shape.</summary>
			public readonly Shape Shape;
			/// <summary>Prompts to use when the mouse is hovering over this.  Will be set automatically for some types in the constructor and can be reassigned.  May be Nothing</summary>
			internal Prompt[] Prompts;

			internal const float DRAWSIZE = 1; // half diameter of a GrabSpot rectangle
			internal const float HITTOLERANCE = 3; // allowed distance from centre to count as a hit (uses DirtyDistance)
			internal const float HITTOLERANCEINVISIBLE = 9; // much larger because it cannot be seen.  However there is a second requirement that the mouse is inside the shape for these
			internal const int MAXIMUMACTIVEPHASE = 6; // animation information for spot hovered over
			internal const int PHASEINTERVAL = 75; // in ms
			internal const float PHASEGROWSIZE = 0.15f;

			public GrabSpot(Shape shape, GrabTypes type, PointF pt, int shapeIndex = 0)
			{
				Shape = shape;
				GrabType = type;
				Position = pt;
				ShapeIndex = shapeIndex;
				SimpleShapeSnap = (type == GrabTypes.SingleVertex);
				switch (type)
				{
					case GrabTypes.Move:
						Prompts = new[] { new Prompt(ShapeVerbs.Choose, "Move") };
						break;
					case GrabTypes.CornerResize:
						Prompts = new[] { new Prompt(ShapeVerbs.Choose, "CornerResize") };
						break;
					case GrabTypes.EdgeMoveH:
						Prompts = new[] { new Prompt(ShapeVerbs.Choose, "StretchH") };
						//Focus = 
						break;
					case GrabTypes.EdgeMoveV:
						Prompts = new[] { new Prompt(ShapeVerbs.Choose, "StretchV") };
						break;
					case GrabTypes.Rotate:
						Prompts = new[] { new Prompt(ShapeVerbs.Choose, "Rotate") };
						break;
				}
			}

			public GrabSpot(Shape shape, GrabTypes type, PointF pt, PointF ptFocus) : this(shape, type, pt)
			{
				Focus = ptFocus;
			}
		}

		public class GrabMovement : GrabSpot, IDisposable
		{

			// contains information about a GrabSpot that is being moved
			// the Position in the base class becomes the starting point of the drag (i.e. mouse position, not restrict position of the GrabSpot - the View changes this)

			/// <summary>Change to make.  Does not use shape list within this.  </summary>
			/// <remarks>theoretically not necessarily needed for every GrabType - but they all use it at the moment</remarks>
			public Transformation Transform;
			// MyBase.Position refers to the original GrabSpot position
			/// <summary>Original position grid snapped</summary>
			public PointF PositionGridSnapped;
			/// <summary>Initial mouse position, exact.  See Current for current position</summary>
			public PointF PositionMouseUnsnapped;
			/// <summary>whether the points used when generating the transformations should be snapped (Current.Snapped is the snapped point)</summary>
			public SnapModes SnapMode;
			/// <summary>the current location both snapped and unsnapped.  View willnot be defined</summary>
			public EditableView.ClickPosition Current;
			/// <summary>True if the control key was pressed when the GrabSpots were created</summary>
			/// <remarks>This can change their behaviour for some shapes, and it is important to use the value from when they were created otherwise there may be errors due to the inconsistency.</remarks>
			public bool ControlKey;
			/// <summary>Is set to true by Flow if it positions the shapes (In which case the normal transformation is not applied)</summary>
			public bool CustomPositioning;
			public readonly Transaction Transaction;
			/// <summary>Use OriginalShape to read.  Is copy of a shape before it was transformed; allowing it to be restored and the entire matrix to be applied again.</summary>
			/// <remarks>Wanted to use Transaction.GetObjectPrevious instead, but when shapes pulled from stack they are already in Tx as a new object, with no previous.</remarks>
			private readonly Dictionary<Shape, Shape> OriginalShapes = new Dictionary<Shape, Shape>();

			/// <summary>Page can be null and will be inferred from grabSpot shape</summary>
			public GrabMovement(GrabSpot grabSpot, Page page, SnapModes snapMode, Transaction transaction) : base(grabSpot.Shape, grabSpot.GrabType, grabSpot.Position)
			{
				ShapeParameter = grabSpot.ShapeParameter;
				// the entire Current object is replaced when the mouse moves, so we can just fill in dummy values for the the zoom etc
				Current = new EditableView.ClickPosition(grabSpot.Position, page ?? grabSpot.Shape.FindPage(), 1, snapMode, snapMode, null, EditableView.ClickPosition.Sources.Irrelevant);
				ShapeIndex = grabSpot.ShapeIndex;
				Focus = grabSpot.Focus;
				SnapMode = snapMode;
				PositionGridSnapped = Position;
				Transaction = transaction;
				switch (GrabType)
				{
					case GrabTypes.Rotate:
						if (SnapMode != SnapModes.Angle)
							SnapMode = SnapModes.Off;
						break;
					case GrabTypes.Radius:
					case GrabTypes.EdgeMoveH:
					case GrabTypes.EdgeMoveV:
					case GrabTypes.CornerResize:
						if (SnapMode == SnapModes.Angle)
							SnapMode = SnapModes.Off;
						break;
				}
			}

			public void DoGrabAngleSnapMultiShapes()
			{
				// called when multiple shapes involved.  Only possible for move
				switch (GrabType)
				{
					case GrabTypes.Move:
						Current.Snapped = Geometry.AngleSnapPoint(Current.Exact, Position);
						break;
					default:
						Debug.Fail("GrabSpot.DoGrabAngleSnapMultiShapes, unexpected grab type: " + this.GrabType);
						break;
				}
			}

			public void Dispose()
			{
				Transform?.Dispose();
				GC.SuppressFinalize(this);
			}

			/// <summary>If true the original shape is not drawn (Page.MovingSelection) and the current shape is restored from the backup before each transformation</summary>
			public bool UseMovingLogic
			{ get { return GrabType != GrabTypes.TextSelection; } }

			public void IncludeShape(Shape shp)
			{
				if (OriginalShapes.ContainsKey(shp))
					return; // can happen when ctrl-A pressed to select a container - both container and individual shapes are in original list, and contents call thru to here twice (container enumerates contents and adds them)
				Transaction.Edit(shp);
				Shape backup = CreateShape(shp.ShapeCode);
				backup.CopyFrom(shp, CopyDepth.Undo, null); // initial copy must be deepish - to get copies of all embedded objects.  Can use Transform from now on
				OriginalShapes.Add(shp, backup);
			}

			/// <summary>Gets backup copy of given shape with coordinates valid before any movement</summary>
			public Shape OriginalShape(Shape shp)
			{
				return OriginalShapes[shp];
				//Return Transaction.GetObjectPrevious(shp)
			}

			public void SetGrabMoveTransform(Page page, GrabSpot grabSpotHit)
			{
				RectangleF bounds = page.SelectedBounds(true);
				switch (GrabType)
				{
					case Shape.GrabTypes.Move:
					case Shape.GrabTypes.SingleVertex:
					case Shape.GrabTypes.Invisible:
					case Shape.GrabTypes.Bezier:
						Transform = new TransformMove(Transformation.Modes.Move);
						break;
					case Shape.GrabTypes.Rotate:
						Transform = TransformRotate.CreateForGrabMove(grabSpotHit.Focus, Position, page.SelectedShapes);
						break;
					case Shape.GrabTypes.Radius:
						Transform = TransformScale.CreateForGrabMove(grabSpotHit.Focus, Position);
						break;
					case Shape.GrabTypes.CornerResize:
						PointF focus1 = new PointF(bounds.Right - grabSpotHit.Position.X + bounds.X, bounds.Bottom - grabSpotHit.Position.Y + bounds.Y);
						Transform = TransformScale.CreateForGrabMove(focus1, grabSpotHit.Position);
						break;
					case Shape.GrabTypes.EdgeMoveV:
					case Shape.GrabTypes.EdgeMoveH:
						Debug.Assert(!Focus.IsEmpty); // should be set when creating grabspot
						Transform = TransformLinearScale.CreateForGrabMove(Focus, grabSpotHit.Position);
						break;
					case Shape.GrabTypes.TextSelection:
						Transform = null;
						break;
					case Shape.GrabTypes.BezierInactive: // this doesn't actually do any dragging, rather it prompts to convert to a path - should have been done by UI
					default:
						throw new ArgumentException("GrabType");
				}
			}


		}
		#endregion

		/// <summary>Called for each shape in Grab.</summary>
		/// <returns>if any shape involved in the grab movement returns false it is abandoned</returns>
		internal virtual bool StartGrabMove(GrabMovement grab)
		{
			if (grab.GrabType == GrabTypes.Undefined)
				throw new InvalidOperationException("Can\'t StartGrabMove(undefined)");
			if (grab.GrabType == GrabTypes.Fixed)
				throw new InvalidOperationException("Can\'t StartGrabMove(Fixed)");
			grab.IncludeShape(this);
			return true;
		}

		internal void GrabMove(GrabMovement move)
		{
			// this is called each time the mouse moves to adjust the position
			// objMove.OriginalShapes is the backup buffer returned from StartGrabMove.
			// general technique is to restore backup and apply Transform from scratch.  We don't keep applying change from previous position to now as this is more liable to
			// accumulate errors.
			if (move.UseMovingLogic)
			{
				this.CopyFrom(move.OriginalShape(this), CopyDepth.Transform, null);
				if ((Flags & GeneralFlags.ProtectBounds) == 0)
					m_Bounds = RectangleF.Empty;
			}
			DoGrabMove(move);
			if (move.UseMovingLogic)
				ClearTextCache(move.GrabType != GrabTypes.Move); // on move the formatting will remain unchanged
		}

		protected internal virtual void DoGrabMove(GrabMovement move)
		{
			// Does the body of the GrabMove function allowing individual shapes to override some of the movement while keeping the framework
			switch (move.GrabType)
			{
				case GrabTypes.Undefined:
					throw new InvalidOperationException("GrabType=undefined not permitted");
				case GrabTypes.Move:
				case GrabTypes.Rotate:
				case GrabTypes.Radius:
					move.Transform.DoTransform(this);
					break;
				case GrabTypes.Bezier:
				case GrabTypes.BezierInactive:
				case GrabTypes.SingleVertex:
					throw new InvalidOperationException("GrabMove must be overridden if SingleVertex GrabSpots/Bezier are used");
				case GrabTypes.EdgeMoveH:
				case GrabTypes.EdgeMoveV:
				case GrabTypes.CornerResize:
					move.Transform.DoTransform(this);
					break;
				case GrabTypes.Fixed:
					break;
				case GrabTypes.TextSelection:
					if (HasText(false) && HasCaret)
					{
						Debug.Assert(m_Fragments != null); // The next line will correct if this is Nothing, but it implies we are doing too much work - the cache has been cleared unnecessarily by some of the dragging logic
						EnsureTextFormatted();
						TextLocation location = TextHitLocation(move.Current.Exact);
						if (location.IsValid)
							SelectTextTo(location);
					}
					else
						Utilities.LogSubError("DoGrabMove::TextSelection when the shape does not have the Caret");
					break;
				default:
					throw new ArgumentException("Unexpected GrabType: " + move.GrabType);
			}
		}

		internal virtual void DoGrabAngleSnap(GrabMovement move)
		{
			// called when angular snapping is enabled
			switch (move.GrabType)
			{
				case GrabTypes.CornerResize:
				case GrabTypes.EdgeMoveH:
				case GrabTypes.EdgeMoveV:
				case GrabTypes.Radius:
					Debug.Fail("Angle snap not expected for grab type: " + move.GrabType);
					break;
				case GrabTypes.Rotate:
					move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, move.Focus);
					break;
				case GrabTypes.Move:
					move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, move.PositionGridSnapped, Convert.ToSingle(Geometry.ANGLE90));
					break;
				default:
					Debug.Fail("DoGrabAngleSnap wasn\'t overridden to cover grab type: " + move.GrabType);
					break;
			}
		}

		internal virtual List<GrabSpot> GetGrabSpots(float scale)
		{
			// returns all possible specialist grab spots for this shape
			// Can return nothing  rather than empty list if none
			// not called if HasCaret - see caller
			return null;
		}

		/// <summary>adds 8 GrabSpots on the bounding rectangle allowing moving the corners and edges, Depending on the Allows property
		/// 0 is the top left, and they count clockwise from there
		/// if allowAll then Allows is ignored, and all possible GrabSpots are added</summary>
		/// <remarks>Implemented in static AddBoundingGrabSpotsForRectangle to which rectangle and Allows need to be supplied</remarks>
		private protected void AddBoundingGrabSpots(List<GrabSpot> list, float scale, bool allowAll = false)
		{
			EnsureBounds();
			AddBoundingGrabSpotsForRectangle(list, m_Bounds, Allows, scale, allowAll, this);
		}

		/// <summary>adds 8 GrabSpots on the bounding rectangle allowing moving the corners and edges, Depending on the Allows given
		/// 0 is the top left, and they count clockwise from there
		/// if allowAll then allows is ignored, and all possible GrabSpots are added</summary>
		/// <remarks>Used by AddBoundingGrabSpots and also view for multi-shape version</remarks>
		internal static void AddBoundingGrabSpotsForRectangle(List<GrabSpot> list, RectangleF bounds, AllowedActions allows, float scale, bool allowAll = false, Shape shape = null)
		{
			if ((allows & AllowedActions.TransformScale) > 0 || allowAll)
			{
				// If this is not set, calling this function was a bit pointless.  However it does drop through to here for Jigsaw.Piece at the moment
				list.Add(new GrabSpot(shape, GrabTypes.CornerResize, new PointF(bounds.X, bounds.Y), 0));
				list.Add(new GrabSpot(shape, GrabTypes.CornerResize, new PointF(bounds.Right, bounds.Y), 2));
				list.Add(new GrabSpot(shape, GrabTypes.CornerResize, new PointF(bounds.Right, bounds.Bottom), 4));
				list.Add(new GrabSpot(shape, GrabTypes.CornerResize, new PointF(bounds.X, bounds.Bottom), 6));
			}
			// and the GrabSpots in the middle of the lines...
			if ((allows & AllowedActions.TransformLinearStretch) > 0 || allowAll) // not all shapes can stretch in one direction
			{
				float minimum = Convert.ToSingle(12 / Math.Sqrt(scale)); // How long it needs to be to display intermediate GrabSpots; changes somewhat with scale
																		 // Will also check for each GrabSpot that the edge it is on is not too short (if the edge is very short it just gets too crowded)
				if (bounds.Width > minimum) // width is OK; include the up and down ones
				{
					list.Add(new GrabSpot(shape, GrabTypes.EdgeMoveV, new PointF(bounds.X + bounds.Width / 2, bounds.Y), 1) { Focus = new PointF(bounds.X + bounds.Width / 2, bounds.Bottom) });
					list.Add(new GrabSpot(shape, GrabTypes.EdgeMoveV, new PointF(bounds.X + bounds.Width / 2, bounds.Bottom), 5) { Focus = new PointF(bounds.X + bounds.Width / 2, bounds.Y) });
				}
				if (bounds.Height > minimum)
				{
					list.Add(new GrabSpot(shape, GrabTypes.EdgeMoveH, new PointF(bounds.Right, bounds.Y + bounds.Height / 2), 3) { Focus = new PointF(bounds.X, bounds.Y + bounds.Height / 2) });
					list.Add(new GrabSpot(shape, GrabTypes.EdgeMoveH, new PointF(bounds.X, bounds.Y + bounds.Height / 2), 7) { Focus = new PointF(bounds.Right, bounds.Y + bounds.Height / 2) });
				}
			}
		}

		protected const float ROTATIONGRABSPOTOFFSET = 9; // maximum offset from the centre of the shape
		protected void AddStandardRotationGrabSpot(List<GrabSpot> spots)
		{
			// adds a rotational spot to the right of the centre
			Debug.Assert((Allows & AllowedActions.TransformRotate) > 0, "Adding rotation grabspot to shape with Allow Rotate = false");
			EnsureBounds();
			PointF centre = Middle();
			float offset = GUIUtilities.MillimetreSize * ROTATIONGRABSPOTOFFSET;
			PointF pt;
			Page page = FindPage();
			if (page.RecentRotationPoints.Any() && !TransformRotate.RotateAboutCentres) // SAW8: position is specified, use that regardless of this shape
			{
				pt = page.RecentRotationPoints.First();
				pt.X += offset * 2;
			}
			if (Math.Abs(m_Bounds.Right - centre.X - offset) < 2)
			{
				// usual position would roughly coincide with the right-hand edge of the bounding box - which will usually have something drawn on it
				pt = new PointF(m_Bounds.Right + 2, centre.Y);
			}
			else
				pt = new PointF(centre.X + offset, centre.Y);
			spots.Add(new GrabSpot(this, GrabTypes.Rotate, pt, centre));
		}

		/// <summary>Adds a rotation handle right of the centre point.  Shapes themselves should use non-static version which has some collision detection </summary>
		internal static void AddStandardRotationGrabSpotAt(List<GrabSpot> spots, PointF centre, Page page)
		{
			float offset = GUIUtilities.MillimetreSize * ROTATIONGRABSPOTOFFSET;
			if (page.RecentRotationPoints.Any() && !TransformRotate.RotateAboutCentres)
			{
				centre = page.RecentRotationPoints.First();
				offset *= 2;
			}
			PointF pt = new PointF(centre.X + offset, centre.Y);
			spots.Add(new GrabSpot(null, GrabTypes.Rotate, pt, centre));
		}

		/// <summary>Can optionally start a custom GrabMovement when the mouse is used directly, rather than the usual one</summary>
		/// <returns>Usually Nothing, otherwise the GrabMovement to use instead of the default</returns>
		/// <remarks>Used for text selection dragging</remarks>
		internal virtual GrabMovement GetCustomGrabMove(EditableView.ClickPosition current, EditableView.ClickPosition click, Transaction transaction)
		{
			if (HasText(true) && HasCaret && click != null) // Last condition should not be needed; just for safety
			{
				TextLocation location = TextHitLocation(click.Exact);
				if (location.IsValid)
					SetCaretLocation(location, true);
				return new GrabMovement(new GrabSpot(this, GrabTypes.TextSelection, current.Exact), current.Page, SnapModes.Off, transaction);
			}
			return null;
		}

		#endregion

		#region Label
		/// <summary>only defined if shape has text</summary>
		public TextStyleC TextStyle;
		protected string m_Label;
		/// <summary>Create by calling TextStyle.EnsureFont(m_objFont)</summary>
		protected Font m_Font;

		#region Behaviour and information properties
		// position of the main text label, ignoring any intrinsic text
		protected enum LabelPositions
		{
			Bounds, // label draws within bounding rectangle
			TextShape, // this shape is a text shape and draws the label independently
			RotatedRectangle, // drawn inside a rectangle, but it can be rotated.  It is specified by 4 points, the graphics automatically corrects to keep the text upright
							  // it is assumed that the points all fall within the bounding box, and the text is clipped to these points, therefore the label cannot affect the bounding box
			Line // text is drawn along a line
		}
		protected virtual LabelPositions LabelPosition
		{ get { return LabelPositions.Bounds; } }

		public enum LabelModes
		{
			NotSupported, // Can never have any text
						  // at the moment the Measures return NotSupported as they do their own text styling internally
			Allowed, // the default, can have a text label added, but does not start with any text
			Always, // the shape should always be treated as having a label from the moment it is created; effectively these are the intrinsic text shapes
			IntrinsicTextPlusLabel, // the shape draws text and so wants to configure the font style, it can also add a text label in addition to this
			/// <summary>the shape draws intrinsic text and wants the styling information for this, but cannot add a text label</summary>
			IntrinsicOnly //
		}

		/// <summary>Returns how the shape can use labels, or not. 
		/// NOTE: can be called before the shape is constructed, show should not rely on state</summary>
		public virtual LabelModes LabelMode
		{ get { return LabelModes.Allowed; } }

		public bool HasText(bool labelOnly)
		{
			// Parameter determines whether any text, including intrinsic text is counted, or whether we only count the typeable label
			// the text might still be empty even if the shape has a label; we distinguish between not being labelled and having an empty label
			// (at least until the data is saved [and in some cases deselect], when empty labels will end up being discarded)
			if (labelOnly)
				return m_Label != null || LabelMode == LabelModes.Always; // NB "" can be true (for a while - won't be saved as a label tho)
			switch (LabelMode)
			{
				case LabelModes.IntrinsicTextPlusLabel:
				case LabelModes.Always:
				case LabelModes.IntrinsicOnly:
					return true;
				case LabelModes.NotSupported:
					return false;
				case LabelModes.Allowed:
					return m_Label != null;
				default:
					Debug.Fail("Unexpected LabelMode");
					return false;
			}
		}

		internal virtual string LabelText
		{
			[DebuggerStepThrough()]
			get { return m_Label; }
			set
			{
				Debug.Assert(string.IsNullOrEmpty(value) || LabelMode != LabelModes.IntrinsicOnly);
				Debug.Assert(string.IsNullOrEmpty(value) || !value.Contains('\n'), "Shape LabelText should use vbCr not vbCrLf");
				if (value == null && m_Label != null && (LabelPosition == LabelPositions.RotatedRectangle || LabelPosition == LabelPositions.Line))
				{
					// must refresh INCLUDING the text bounding box, which might go outside shape
					Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
				}
				m_Label = value;
				if ((Flags & GeneralFlags.ProtectBounds) == 0)
					m_Bounds = RectangleF.Empty;
				FormatText();
				if (HasCaret)
					SetCaretCharacter(Convert.ToInt32(value?.Length ?? 0), false);
			}
		}

		/// <summary>Returns true if the shape supports typing an arbitrary text label.  I.e. accepts typing in the general sense.
		/// Some other controls may accept "typing" in the sense of processing certain keys, such as NumberGrid</summary>
		public bool SupportsTextLabel
		{
			get
			{
				switch (LabelMode)
				{
					case LabelModes.NotSupported:
					case LabelModes.IntrinsicOnly:
						return false;
					default:
						return true;
				}
			}
		}
		#endregion

		protected virtual void DrawLabel(Canvas gr, DrawResources resources)
		{
			// usually called after InternalDraw.  Never drawn during highlight
			// specialist text shapes override this entire function, usually replicating the first formatting and label parts
			switch (LabelMode)
			{
				case LabelModes.NotSupported:
				case LabelModes.IntrinsicOnly:
					return;
			}
			if ((!string.IsNullOrEmpty(m_Label) || HasCaret) && resources.TextBrush != null)
			{
				EnsureFont(resources.CoordScale);
				DrawNewText(gr, resources, RectangleF.Empty); // theoretical bounds drawn here - easier to state in Page coords than text coords
			}

			if (resources.TextBoundsPen != null)
			{
				switch (LabelPosition)
				{
					case LabelPositions.TextShape:
						throw new InvalidOperationException("Shape must override DrawLabel if LabelPosition = TextShape");
					case LabelPositions.Bounds:
						EnsureBounds();
						gr.Rectangle(new RectangleF(m_Bounds.X, m_Bounds.Y, m_Bounds.Width, m_Bounds.Height), resources.TextBoundsPen);
						break;
					case LabelPositions.RotatedRectangle:
						// rearrange the points so that the text flows in a sensible direction
						PointF[] aPoints_1 = RectangleLabelRearrangePoints(RectangleLabelGetPoints());
						gr.DrawLines(aPoints_1, resources.TextBoundsPen);
						gr.DrawLine(aPoints_1[3], aPoints_1[0], resources.TextBoundsPen);
						break;
					case LabelPositions.Line:
						PointF[] aPoints = LineLabelGetTextRectangle(LineLabelGetPoints());
						gr.DrawLines(aPoints, resources.TextBoundsPen);
						gr.DrawLine(aPoints[3], aPoints[0], resources.TextBoundsPen);
						break;
					default:
						throw new InvalidOperationException("Unexpected LabelPosition");
				}
			}
		}

		public virtual void CreateLabel(TextStyleC currentTextStyle)
		{
			if (!SupportsTextLabel)
			{
				Debug.WriteLine("Shape does not support labels");
				return;
			}
			if (m_Label == null)
				m_Label = "";
			if (TextStyle == null)
			{
				TextStyle = new TextStyleC();
				TextStyle.CopyFrom(currentTextStyle);
			}
			if (LabelPosition == LabelPositions.Line)
			{
				if ((Flags & GeneralFlags.ProtectBounds) == 0)
					m_Bounds = RectangleF.Empty; // in this mode the text will extend the bounding rectangle
				FormatText();
			}
			CaretGain();
		}

		#region LabelPosition = RotatedRectangle
		/// <summary>the points are the four corners of a rectangular label (NOT necessarily orthogonal)
		/// this returns the points reorganised into clockwise winding order with the first point where the text should begin</summary>
		protected PointF[] RectangleLabelRearrangePoints(PointF[] points)
		{
			if (Geometry.TurnDirection(points[0].VectorTo(points[1]), points[0].VectorTo(points[3])) < 0)
			{
				// turns anticlockwise.  We need to flip the baseline and bottom line
				Utilities.Swap(ref points[0], ref points[3]);
				Utilities.Swap(ref points[1], ref points[2]);
			}

			// choose the vertex which is the furthest to the top left
			float minimumValue = float.MaxValue;
			int start = -1;
			for (int index = 0; index <= 3; index++)
			{
				float value = points[index].Y + points[index].X;
				if (value < minimumValue)
				{
					minimumValue = value;
					start = index;
				}
			}
			// usually this would be the start point.  However if the shape is very thin we might bias towards doing it the other way
			int next = (start + 1) % 4;
			int previous = start - 1;
			if (previous < 0)
				previous = 3;
			float lineLength = Geometry.DistanceBetween(points[start], points[next]);
			float transverseLength = Geometry.DistanceBetween(points[start], points[previous]);
			if (transverseLength >= lineLength * 3)
			{
				// next condition makes sure that the text is flowing downhill
				if (points[next].Y < points[start].Y)
					start = next;
				else
					start = previous;
			}
			return new[] { points[start], points[(start + 1) % 4], points[(start + 2) % 4], points[(start + 3) % 4] };
		}

		/// <summary>shape should return the 4 points at the vertices.  They can wind clockwise or anticlockwise the graphics automatically chooses which one should be top left</summary>
		protected virtual PointF[] RectangleLabelGetPoints()
		{
			throw new InvalidOperationException("Shape.RectangleLabelGetPoints has not been overridden where Shape.LabelPosition = RotatedRectangle");
		}

		#endregion

		#region LabelPosition = Line
		// places the text so that one line of text appears above the line, and a second below, if necessary
		protected virtual PointF[] LineLabelGetPoints()
		{
			// shape should return the 2 points at the vertices.  The order is ignored, unless the points have equal X coordinate
			// in which case the text is drawn in the order that the points are provided (because it might be important for vertical axes to control which way the text goes)
			throw new InvalidOperationException("Shape.LineLabelGetPoints has not been overridden where Shape.LabelPosition = Line");
		}

		private PointF[] LineLabelGetTextRectangle(PointF[] linePoints)
		{
			// takes the line points returned by LineLabelGetPoints, and returns the bounding rectangle which can be given to DrawTextInArbitraryRectangle
			// text is always drawn left to right.  If the points are one above the other it is drawn in the order that they are provided
			if (linePoints[0].X > linePoints[1].X)
			{
				// swap the points so that they read left to right
				PointF temp = linePoints[0];
				linePoints[0] = linePoints[1];
				linePoints[1] = temp;
			}

			float height = (float)(TextStyle.Size * 1.1 * Geometry.POINTUNIT / Geometry.MILLIMETRE); // now measured in data coordinates
																									 // the vector in the downward direction...
			SizeF transverse = linePoints[0].VectorTo(linePoints[1]).Perpendicular(1);
			transverse = transverse.ChangeLength(height);
			return new PointF[]
			{
				linePoints[0] - transverse,
				linePoints[1] - transverse,
				linePoints[1] + transverse,
				linePoints[0] + transverse
			};
		}

		protected void ExtendBoundsToAccountForLineLabel(RectangleF bounds)
		{
			// should be used in the CalculateBounds of derived classes using LabelPosition = Line
			if (HasText(true))
				Geometry.Extend(ref bounds, LineLabelGetTextRectangle(LineLabelGetPoints()));
		}

		#endregion

		#endregion

		#region New text
		// v2 - we do a lot of formatting mainly in order to support carets
		// all text work is done in a virtual coordinate space;  a matrix transforms back to page space for drawing
		// this is because .net can't draw rotated text (but also for my sanity - I don't fancy doing the layout in rotated space)
		// the text is always horizontal within text space.  Organised into a series of fragments, which are probably lines
		// vertically above each other - BUT DONT NEED TO BE, they can be disconnected (E.g. the Equation tool overrides and extends this)
		// Origin of text space is not necessarily top left - it can be any useful reference;  probably centre of text when it is centred
		// default base class uses bounding box of shape as outer bounds, unrotated

		#region Fragments
		protected List<Fragment> m_Fragments; // = nothing by default (most shapes have no text)

		/// <summary>stores one 'line' of text </summary>
		internal protected class Fragment
		{
			/// <summary>will exclude trailing Whitespace</summary>
			public string Text;
			/// <summary>NOTE: in text space</summary>
			public RectangleF Bounds;
			/// <summary>index of this within the list - just used for faster lookup than calling IndexOf</summary>
			public int Index;
			// the remainder are not used in equations
			/// <summary>index into Label of this Text.  Must be 0 for first </summary>
			public int StartOffset;
			/// <summary>true if this fragment ends with vbCr (ie forces new line, even if there is no more text)</summary>
			public bool HardBreak; // true if this fragment ends with vbCr (ie forces new line, even if there is no more text)
			/// <summary>only set on last - true if more text has been discarded</summary>
			public bool Truncated; // only set on last - true if more text has been discarded

			/// <summary>finds X location of character (ie coord to left of intCharacter). eg intCharacter = 0 will return 0</summary>
			/// <remarks>NOTE THIS IS EXACT - whereas formatting uses MeasureString (cos it needs to know how many characters fit)</remarks>
			public virtual float CharacterOffsetWithin(int characterOffset, Shape shape)
			{
				if (characterOffset < 0)
					characterOffset = 0;
				if (characterOffset > Text.Length)
					characterOffset = Text.Length;
				RectangleF size = NetCanvas.MeasurementInstance.MeasureTextExact(Text, shape.TextFragmentFont(this), BigRectangle, GUIUtilities.StringFormatFindCharacter, characterOffset);
				//Debug.WriteLine("Text ('" + Text + "') measured: " + rctSize.ToString + " @ scale=" + NetCanvas.MeasurementInstance.Transform.Elements(0).ToString + ", with font=" + shp.TextFragmentFont(Me).Size.ToString)
				return size.Right; // note not width - X since rectangle.X may be >0 indicating dead space to left; but this dead space IS drawn, so needs to be included
			}

		}

		protected virtual void RepositionAllFragments()
		{
			// called if alignment changes - moves blocks, but does not recheck their sizes or the text flow - as this won't have changed
			foreach (Fragment fragment in m_Fragments)
			{
				PlaceFragment(fragment);
			}
			CheckTextVerticalPositions(false);
		}

		protected virtual bool PlaceFragment(Fragment newFragment, float extraLineSpace = 0)
		{
			// newFragment has been completely filled in, including the Index and Bounds width and height, but not Bounds.X or .Y; it is USUALLY not yet in m_Fragments
			// this function must set the location of the rectangle based upon the existing fragments
			// this can be called later to reposition (eg on alignment change) if the actual block sizes won't have changed
			// if overridden such that they are not vertically stacked, FormatTextChangedHeight may also need to be overridden
			// returns true if more fragments the same height will be placeable
			if (newFragment.Index == 0)
			{
				newFragment.Bounds.Y = 0;
				if (LabelPosition == LabelPositions.Line)
					newFragment.Bounds.Y = -newFragment.Bounds.Height;
			}
			else
			{
				Debug.Assert(newFragment.Index <= m_Fragments.Count);
				// default is to go below the last existing fragment
				newFragment.Bounds.Y = m_Fragments[newFragment.Index - 1].Bounds.Bottom + extraLineSpace;
			}
			// X position depends on alignment
			switch (TextStyle.Alignment)
			{
				case StringAlignment.Center:
					newFragment.Bounds.X = (TextAllowedWidth - newFragment.Bounds.Width) / 2;
					break;
				case StringAlignment.Near:
					newFragment.Bounds.X = 0; // most shapes start with 0 on left
					break;
				case StringAlignment.Far:
					newFragment.Bounds.X = TextAllowedWidth - newFragment.Bounds.Width;
					break;
				default:
					throw new ArgumentException("Invalid TextStyle.Alignment: " + TextStyle.Alignment);
			}
			float top = newFragment.Index == 0 ? newFragment.Bounds.Top : m_Fragments[0].Bounds.Top;
			switch (LabelPosition)
			{
				case LabelPositions.Line:
					return newFragment.Index == 0; // only 2 can be placed, so if this is 2nd no more
				case LabelPositions.TextShape:
					return true;
				case LabelPositions.Bounds:
					return Bounds.Height - (newFragment.Bounds.Bottom - top) >= newFragment.Bounds.Height;
				case LabelPositions.RotatedRectangle:
					PointF[] points = RectangleLabelRearrangePoints(RectangleLabelGetPoints());
					return Geometry.DistanceBetween(points[0], points[3]) - (newFragment.Bounds.Bottom - top) >= newFragment.Bounds.Height;
				default:
					throw new InvalidOperationException("Unexpected LabelPosition: " + LabelPosition);
			}
		}

		protected int FragmentFromCharacter(int character)
		{
			// returns fragment containing the given character.  Returns 0 if no fragments (caller should really have dealt with that already)
			int fragment = 0;
			if (m_Fragments != null)
			{
				while (fragment < m_Fragments.Count - 1 && m_Fragments[fragment + 1].StartOffset <= character)
				{
					fragment += 1;
				}
			}
			return fragment;
		}

		/// <summary>= TextStyle.VerticalAlignment, but can be overridden to force alignment without changing data if necessary </summary>
		protected virtual StringAlignment EffectiveTextVerticalAlignment
		{ get { return TextStyle?.VerticalAlignment ?? StringAlignment.Center; } }

		protected virtual void CheckTextVerticalPositions(bool verticalAlignChanged, float extraLineSpace = 0)
		{
			// called on exit from FormatText (with false) if it changed the total text height (actually the Bottom of the last fragment)
			// or on V align changed (with true) - this needs to execute even if new style is Near
			if ((LabelPosition == LabelPositions.RotatedRectangle || LabelPosition == LabelPositions.Bounds) && m_Fragments.Count > 0 &&
				(EffectiveTextVerticalAlignment != StringAlignment.Near || verticalAlignChanged))
			{
				// Near is top aligned - which happens automatically
				// for all others need to adjust the Y positions
				// this assumes the fragments have been placed one above the other
				float height = m_Fragments[m_Fragments.Count - 1].Bounds.Bottom - m_Fragments[0].Bounds.Top;
				float available;
				if (LabelPosition == LabelPositions.RotatedRectangle)
				{
					PointF[] aPts = RectangleLabelRearrangePoints(RectangleLabelGetPoints());
					available = Geometry.DistanceBetween(aPts[0], aPts[3]); // vertical space available to us
				}
				else
					available = Bounds.Height;
				float Y; // where the first must be
				switch (EffectiveTextVerticalAlignment)
				{
					case StringAlignment.Center:
						Y = Math.Max(0, available - height) / 2;
						break;
					case StringAlignment.Far:
						Y = Math.Max(0, available - height);
						break;
					case StringAlignment.Near:
						Y = 0; // only gets here if style changed; didn't need all the calcs for this techincally, but not worth filtering out
						break;
					default:
						throw new ArgumentException("Unexpected TextStyle.VerticalAlignment: " + TextStyle.VerticalAlignment);
				}
				if (m_Fragments[0].Bounds.Top != Y)
				{
					for (int index = 0; index <= m_Fragments.Count - 1; index++)
					{
						m_Fragments[index].Bounds.Y = Y;
						Y += m_Fragments[index].Bounds.Height + extraLineSpace;
					}
					InvalidateTextRectangle(CalculateTextBounds());
					if (HasCaret && Caret.IsValid)
						CaretRealculateCoordinates(false);
				}
			}
		}

		protected virtual Font TextFragmentFont(Fragment fragment, float sizeReduce = -1)
		{
			// this is a stub used by Equation.  Returns the font used in a particular fragment
			return m_Font;
		}
		#endregion

		#region Measurement and layout
		/// <summary>arbitrary (static) rectangle used for 'unlimited' text measurement</summary>
		internal static readonly RectangleF BigRectangle = new RectangleF(0, 0, 10000, 1000);

		protected void FormatTextFromCharacterPosition(int position)
		{
			// Calls FormatPosition; the parameter in this case is a character position into the text
			FormatText(FragmentFromCharacter(position));
		}

		protected virtual void FormatText(int fromFragmentIndex = 0)
		{
			Debug.Assert(m_Label == null || !m_Label.Contains("\r\n"), "Text should be vbcr only, not vbcrlf");
			if (m_Label == null && (!HasCaret || Flag(GeneralFlags.NoEmptyLabelMessage)))
			{
				m_Fragments = null;
				return;
			}
			string remaining;
			int start = 0;
			float bottom; // vertical end of layout - remembered to detect if it changes (need to know this for some vertical alignments)
			if (m_Fragments == null || m_Fragments.Count == 0)
			{
				remaining = m_Label;
				bottom = 0;
			}
			else
			{
				bottom = m_Fragments[m_Fragments.Count - 1].Bounds.Bottom;
				if (fromFragmentIndex == 0)
					remaining = m_Label;
				else
				{
					fromFragmentIndex = Math.Min(fromFragmentIndex, m_Fragments.Count - 1);
					start = m_Fragments[fromFragmentIndex].StartOffset;
					while (start >= m_Label.Length && fromFragmentIndex > 0)
					{
						fromFragmentIndex -= 1;
						start = m_Fragments[fromFragmentIndex].StartOffset;
					}
					remaining = m_Label.Substring(start);
				}
			}
			if (string.IsNullOrEmpty(m_Label) && (Flags & GeneralFlags.NoEmptyLabelMessage) == 0)
			{
				remaining = Strings.Item("Empty_Text");
				fromFragmentIndex = 0; // because fragments don't match actual text, best to keep this safe
				start = 0;
			}

			// ensure that various things have been created (these are only created as needed)
			if (m_Fragments == null)
				m_Fragments = new List<Fragment>();
			if (m_Fragments.Count > fromFragmentIndex)
				m_Fragments.RemoveRange(fromFragmentIndex, m_Fragments.Count - fromFragmentIndex);
			if (string.IsNullOrEmpty(remaining))
			{
				if (m_Fragments.Count == 0) // want to leave one empty fragment rather than empty list
				{
					Fragment fragment = new Fragment();
					PlaceFragment(fragment);
					fragment.Text = "";
					m_Fragments.Add(fragment);
					if (bottom != 0)
						CheckTextVerticalPositions(false); // it wasn't 0 before - must have deleted all the text
				}
				return;
			}
			EnsureFont();

			// intFromFragment is the next fragment to create.  strRemaining is the text to place.  intStart is the index within the outer text
			// adds a fudge factor to remove an apparent left and right margin (which doesn't really exist)
			SizeF layoutSize = new SizeF(TextAllowedWidth + 2, 1000);
			RectangleF invalid = new RectangleF();
			while (remaining.Length > 0)
			{
				int chars;
				int lines;
				string test = remaining;
				int breakIndex = remaining.IndexOf('\r');
				if (breakIndex >= 0) // need to limit the measurement to the first enforced line break (otherwise MeasureString happily measures several lines)
				{
					test = remaining.Substring(0, breakIndex);
					if (breakIndex == 0)
						test = " "; // if left as empty we get empty bounds - which does some funny and bad stuff in CaretUpdateCoords (if it's the only line)
				}
				SizeF sz = NetCanvas.MeasurementInstance.Underlying.MeasureString(test, m_Font, layoutSize, GUIUtilities.StringFormatMeasureLine, out chars, out lines);
				Debug.Assert(lines <= 1);
				if (chars == 0)
					chars = 1; // otherwise we get stuck!
				Fragment fragment = new Fragment()
				{
					Text = remaining.Substring(0, chars),
					StartOffset = start,
					Index = m_Fragments.Count,
					HardBreak = breakIndex == chars
				};
				fragment.Bounds.Size = sz; // position will still be 0,0 at the moment
				bool moreVerticalSpace = PlaceFragment(fragment);
				Geometry.Extend(ref invalid, fragment.Bounds);
				m_Fragments.Add(fragment);

				// now consume any final line break
				if (chars == breakIndex) // stopped at a line break
					chars += 1;
				else
				{
					// second length test needed due to Mac stuff above
					while (chars < test.Length && chars < remaining.Length && char.IsWhiteSpace(test[chars]))
					{
						chars += 1;
					}
				}
				start += chars;
				remaining = remaining.Substring(chars);
				if (!moreVerticalSpace)
				{
					// there is no room to place any more text, so stop
					remaining = "";
					fragment.Truncated = true;
				}
				else if (remaining.Length == 0 && fragment.HardBreak)
				{
					// special case if last line ends with vbcrlf we need to add another empty line
					float height = fragment.Bounds.Height;
					fragment = new Fragment();
					fragment.Text = "";
					fragment.StartOffset = start;
					fragment.Bounds.Size = new SizeF(0.1F, height);
					fragment.Index = m_Fragments.Count;
					PlaceFragment(fragment);
					m_Fragments.Add(fragment);
				}
			}
			if (m_Fragments[m_Fragments.Count - 1].Bounds.Bottom != bottom)
				CheckTextVerticalPositions(false);
			if (!invalid.IsEmpty)
				InvalidateTextRectangle(invalid);
		}

		/// <summary>finds which character position is at given X coord.  Ie character before which to insert caret if clicked here</summary>
		/// <param name="fragmentIndex">Index of fragment to search within</param>
		/// <param name="offset">X coordinate of position, relative to start of fragment.  This is updated to the text value at the returned location</param>
		/// <returns>always returns an offset 0 ... length even if coordinates way off end</returns>
		protected int FindCharacterWithinFragment(int fragmentIndex, ref float offset)
		{
			if (offset <= Geometry.NEGLIGIBLE)
			{
				offset = 0;
				return 0;
			}
			Fragment fragment = m_Fragments[fragmentIndex];
			if (offset >= fragment.Bounds.Width - Geometry.NEGLIGIBLE)
			{
				offset = fragment.Bounds.Width;
				return fragment.Text.Length;
			}
			// The code below doesn't work well with 1 character, because the test for MethodString returning 1 instead of 0 if never reached due to the Text.Length test
			// However for this case we can simply see which half of the box the point is in
			if (fragment.Text.Length == 1)
			{
				if (offset > fragment.Bounds.Width / 2)
				{
					offset = fragment.Bounds.Width;
					return fragment.Text.Length;
				}
				offset = 0;
				return 0;
			}
			Font font = TextFragmentFont(m_Fragments[fragmentIndex]);
			int chars;
			NetCanvas.MeasurementInstance.Underlying.MeasureString(fragment.Text, font, new SizeF(offset, 100), GUIUtilities.StringFormatFindCharacter, out chars, out _);
			// this will return WHOLE characters which fit. need to check which is closest position - ie if one more char NEARLY fitted
			if (chars >= fragment.Text.Length)
			{
				offset = fragment.Bounds.Width;
				return chars;
			}

			// and also since MeasureString is completely bollocks - need to measure again properly to get the exact position
			RectangleF rctA = NetCanvas.MeasurementInstance.MeasureTextExact(fragment.Text, font, BigRectangle, GUIUtilities.StringFormatFindCharacter, chars);
			RectangleF rctB = NetCanvas.MeasurementInstance.MeasureTextExact(fragment.Text, font, BigRectangle, GUIUtilities.StringFormatFindCharacter, chars + 1);
			if (Math.Abs(rctA.Right - offset) > Math.Abs(rctB.Right - offset))
			{
				// second was closer
				offset = rctB.Right;
				return chars + 1;
			}
			// unfortunately MeasureString also tends to return 1 if text v short
			if (chars == 1 && rctB.Right >= offset * 2) // but we can easily check if this one char is too wide for 1 to make sense
			{
				offset = 0;
				return 0;
			}
			offset = rctA.Right;
			return chars;
		}

		protected virtual float TextAllowedWidth
		{
			get // maximum permitted width for text fragments.  May be infinite (actually V large) for some unrestricted types
			{
				switch (LabelPosition)
				{
					case LabelPositions.Bounds:
						return Bounds.Width;
					case LabelPositions.Line:
						PointF[] aPts_1 = LineLabelGetPoints();
						return Geometry.DistanceBetween(aPts_1[0], aPts_1[1]);
					case LabelPositions.RotatedRectangle:
						PointF[] aPts = RectangleLabelRearrangePoints(RectangleLabelGetPoints());
						return Geometry.DistanceBetween(aPts[0], aPts[1]);
					// TextShape should have overridden this
					default:
						throw new InvalidOperationException("Unexpected LabelPosition");
				}
			}
		}

		/// <summary>Measured in text coordinates.</summary>
		protected RectangleF CalculateTextBounds()
		{
			// Not sure if it would be better to store this? At the moment I think is only needed to draw the border if that is shown
			if (m_Fragments == null)
				return RectangleF.Empty;
			return Geometry.UnionRectangles(from f in m_Fragments select f.Bounds);
		}

		/// <summary>returns true if ptPage within any text rectangle</summary>
		/// <param name="pointOnPage">Point in page coordinates</param>
		/// <remarks>Used as implementation of HitTestDetailed by text shapes</remarks>
		protected bool TextHitTest(PointF pointOnPage)
		{
			if (m_Fragments == null)
				return false;
			if (m_PageToTextTransform == null)
				SetTextTransforms();
			PointF[] points = { pointOnPage };
			m_PageToTextTransform.TransformPoints(points);
			return m_Fragments.Any(objFragment => objFragment.Bounds.Contains(points[0]));
		}

		/// <summary>Returns fragment index and character set of within text</summary>
		/// <param name="pointOnPage">Point in page coordinates</param>
		/// <returns>TextLocation to which ptPage refers.  Returns TextLocation.Empty if the point is not within any Fragment</returns>
		/// <remarks>Used as implementation of HitTestDetailed by text shapes.  Note that currently this returns Empty unless the point is actually inside a fragment.  Points off the left or right will return Empty.</remarks>
		protected virtual TextLocation TextHitLocation(PointF pointOnPage)
		{
			if (m_Fragments == null)
				return TextLocation.Empty;
			if (string.IsNullOrEmpty(m_Label))
				return new TextLocation(0, 0, this, 0); // needed in case [Empty_Text] is displayed in the fragments
			if (m_PageToTextTransform == null)
				SetTextTransforms();
			PointF[] points = { pointOnPage };
			m_PageToTextTransform.TransformPoints(points);
			PointF pointInText = points[0];
			for (int index = 0; index <= m_Fragments.Count - 1; index++)
			{
				Fragment objFragment = m_Fragments[index];
				if (objFragment.Bounds.Contains(pointInText))
				{
					float X = pointInText.X - objFragment.Bounds.X;
					int character = FindCharacterWithinFragment(index, ref X);
					return new TextLocation(index, character, this, X);
				}
			}
			return TextLocation.Empty;
		}

		/// <summary>Triggers FormatText if this contains text which is not been laid out in Fragments yet</summary>
		protected void EnsureTextFormatted()
		{
			if (m_Fragments != null) return;
			if (!string.IsNullOrEmpty(m_Label) || HasCaret && (Flags & GeneralFlags.NoEmptyLabelMessage) == 0)
			{
				FormatText();
				if (HasCaret)
					CaretRealculateCoordinates(false); //refresh may be spurious, but this will usually only happen when entire shape is being redrawn anyway
			}
		}

		/// <summary>Clears transforms and usually the entire formatting</summary>
		/// <param name="clearFormat"></param>
		/// <remarks>EnsureTextFormatted will recreate Fragments;  transforms must be tested for Nothing before use and call SetTextTransforms if needed.
		/// Overridable for Equation; that doesn't need to clear the transforms, and MUSTN'T clear fragments</remarks>
		protected virtual void ClearTextCache(bool clearFormat = true)
		{
			m_PageToTextTransform?.Dispose();
			m_PageToTextTransform = null;
			m_TextToPageTransform?.Dispose();
			m_TextToPageTransform = null;
			if (clearFormat)
				m_Fragments = null;
		}

		#endregion

		#region TextLocation
		protected struct TextLocation : IComparable<TextLocation>
		{
			// Note that this becomes invalid if the text is reformatted.  Would like many of the properties to be readonly, but they can't be because of Assign (which is needed by equation editing)
			// and also some are updated later to match changes to content (eg Fragment or FragmentIndex to reflect changes to fragments)

			/// <summary>Index of fragment, or -1 if not valid</summary>
			public int FragmentIndexIndex;
			/// <summary>Character index WITHIN fragment before which the point occurs.  Is always 0...Length (not limited to Length -1)</summary>
			public readonly int CharacterOffset;
			/// <summary>Character index within overall text (possibly meaningless for an equation)</summary>
			public int CharacterTotal;
			/// <summary>Fragment object, or Nothing</summary>
			public Fragment Fragment;
			/// <summary>Actual coordinate or PointF.Empty (if FragmentIndex less than 0).  Usually the top of the line</summary>
			public PointF Location
			{
				get
				{
					if (FragmentIndexIndex < 0)
						return new PointF(-10000, -10000); // this case will occur because we check the location of CaretDrawn when it is not valid
					if (!m_LocationDefined)
					{
						if (Shape.m_Fragments == null || FragmentIndexIndex >= Shape.m_Fragments.Count)
						{
							return new PointF(-10000, -10000);
						}
						Fragment = Shape.m_Fragments[FragmentIndexIndex];
						m_Location = new PointF(Fragment.Bounds.X + Fragment.CharacterOffsetWithin(CharacterOffset, Shape), Fragment.Bounds.Y);
						LineHeight = Fragment.Bounds.Height; // useful when font size changes; that will trigger a reposition, and doing this will fix the size
						m_LocationDefined = true;
					}
					return m_Location;
				}
			}
			private PointF m_Location; // Can be defined upon construction if none, otherwise is only defined when needed (to avoid unnecessary measurement)
			private bool m_LocationDefined; // Must remember this separately from m_ptLocation, because Empty may well be a valid location
			public Shape Shape;

			/// <summary>Height of Fragment; but sometimes updated later (e.g. if this is used as the caret)</summary>
			/// <remarks>Can be set to 0 in the Caret to hide it</remarks>
			public float LineHeight; // can be updated later

			// These remember the meaning of the location, making it easier to update the stored values (especially in equation)
			public enum Purposes
			{
				Other = 0, // will therefore be the default value
				Caret,
				Selection,
				SelectionRange0,
				SelectionRange1
			}
			public Purposes Purpose; // Locations are still considered equal if they differ only on this field

			public static TextLocation Empty = new TextLocation(-1, 0, null);
			public static TextLocation Zero = new TextLocation() { m_LocationDefined = true }; // A location with all indices and coordinates = 0, but counting as valid; Fragment will not be set

			//Private Sub New()
			//	' all fields will be Nothing / 0
			//End Sub

			/// <summary>Creates location based on fragment and character index and X coordinate within fragment (i.e. data produced by FindCharacterWithinFragment)
			/// pixelOffset can be omitted, but that is slower since it must measure</summary>
			public TextLocation(int fragmentIndex, int characterOffset, Shape shp, float pixelOffset = -1)
			{
				FragmentIndexIndex = fragmentIndex;
				CharacterOffset = characterOffset;
				Shape = shp;
				CharacterTotal = 0;
				m_Location = PointF.Empty;
				m_LocationDefined = false;
				LineHeight = 0;
				Purpose = Purposes.Other;
				Fragment = null;
				if (fragmentIndex >= 0)
				{
					Fragment = shp.m_Fragments[FragmentIndexIndex];
					CharacterTotal = Fragment.StartOffset + characterOffset;
					if (pixelOffset >= 0)
					{
						m_LocationDefined = true;
						m_Location = new PointF(Fragment.Bounds.X + pixelOffset, Fragment.Bounds.Y);
						// Otherwise it will be measured when needed
					}
					LineHeight = Fragment.Bounds.Height;
				}
			}

			/// <summary>Creates a location based on fragment and character index; if X coordinate is known use the overload which uses fragment index instead</summary>
			[DebuggerStepThrough()]
			public TextLocation(Fragment fragment, int characterOffset, Shape shp)
			{
				Fragment = fragment;
				FragmentIndexIndex = fragment.Index;
				CharacterOffset = characterOffset;
				Shape = shp;
				CharacterTotal = Fragment.StartOffset + characterOffset;
				LineHeight = Fragment.Bounds.Height;
				m_Location = PointF.Empty;
				m_LocationDefined = false;
				Purpose = Purposes.Other;
			}

			/// <summary>Creates location for the given text character within the overall shape.  (Slow)</summary>
			/// <remarks>This version is relatively slow because it must find the fragment and measure the coordinates</remarks>
			public TextLocation(int totalCharacter, Shape shp)
			{
				CharacterTotal = totalCharacter;
				FragmentIndexIndex = shp.FragmentFromCharacter(totalCharacter);
				Shape = shp;
				m_Location = PointF.Empty;
				m_LocationDefined = false;
				Purpose = Purposes.Other;
				CharacterOffset = 0;
				LineHeight = 0;
				Fragment = null;
				if (FragmentIndexIndex >= 0 && shp.m_Fragments != null)
				{
					Fragment = shp.m_Fragments[FragmentIndexIndex];
					CharacterOffset = totalCharacter - Fragment.StartOffset;
					LineHeight = Fragment.Bounds.Height;
					if (Fragment.Truncated && CharacterOffset > Fragment.Text.Length)
					{
						// Is actually off the end; this produces no coordinate
						m_Location.X = -10000;
						LineHeight = 0;
						m_LocationDefined = true;
					}
				}
			}

			public bool IsValid
			{
				get
				{
					return FragmentIndexIndex >= 0 && Shape != null && (FragmentIndexIndex == 0 || FragmentIndexIndex < Shape.m_Fragments.Count);
					// FragmentIndex = 0 in case they are not defined
				}
			}

			/// <summary>Causes Location to be recalculated on next access (assuming text has been reformatted, but the character offsets are unchanged)</summary>
			public void ResetLocation()
			{
				m_LocationDefined = false;
			}

			public int CompareTo(TextLocation other)
			{
				// Not terribly interested in the case where either is not valid.  Any invalid one will count as before the start of the text, which is probably OK
				int result = FragmentIndexIndex.CompareTo(other.FragmentIndexIndex);
				if (result == 0)
					result = CharacterOffset.CompareTo(other.CharacterOffset);
				return result;
			}

			public override bool Equals(object obj)
			{
				// Note that this deliberately ignores Purpose
				if (!(obj is TextLocation))
					return false;
				return CompareTo((TextLocation)obj) == 0;
			}

			public override int GetHashCode()
			{
				// ReSharper disable once NonReadonlyMemberInGetHashCode
				return (FragmentIndexIndex * 1000 + CharacterOffset).GetHashCode() ^ Location.GetHashCode();
			}

			public override string ToString()
			{
				if (FragmentIndexIndex < 0)
					return "Empty";
				return "Char=" + CharacterTotal + " @" + CharacterOffset + " in fragment " + Fragment;
			}

			/// <summary>Returns same structure but with Purpose field changed.</summary>
			/// <remarks>This is just a syntactical convenience</remarks>
			public TextLocation SetPurpose(Purposes ePurpose)
			{
				TextLocation newLocation = (TextLocation)MemberwiseClone();
				newLocation.Purpose = ePurpose;
				return newLocation;
			}

		}
		#endregion

		protected void DrawNewText(Canvas gr, DrawResources resources, RectangleF boundsRectOrEmpty)
		{
			// doesn't draw bounding area (objResources.TextBoundsPen)
			// rctBoundsRectorEmpty is optional - only provide (in text coords) if this should support the TextBoundsPen
			EnsureTextFormatted();
			if (m_TextToPageTransform == null)
				SetTextTransforms();

			// the drawing transformation is not just m_TextToPageTransform, because there is a transformation applied to all page coordinates to deal with scaling and scrolling
			//Dim objOldTransform As Matrix = Nothing
			if (!m_TextToPageTransform.IsIdentity)
			{
				//objOldTransform = gr.Transform.Clone() ' keep the original transformation so it can be restored
				gr.MultiplyTransform(m_TextToPageTransform, MatrixOrder.Prepend); // text to page must be done before page to screen, therefore Prepend
			}

			if (HasSelection && resources.Buffer == StaticView.InvalidationBuffer.Base)
				DrawTextSelection(gr, resources);

			bool emptyPrompt = string.IsNullOrEmpty(m_Label) && (Flags & GeneralFlags.NoEmptyLabelMessage) == 0;
			// true if showing different prompt
			// note that if text is empty, the fragments might not be
			if (resources.TextBrush != null)
			{
				if (!string.IsNullOrEmpty(m_Label) || emptyPrompt)
				{
					if (m_Fragments != null)
						foreach (Fragment objFragment in m_Fragments)
						{
							if (objFragment != null)
								gr.DrawString(objFragment.Text, m_Font, resources.TextBrush, objFragment.Bounds, GUIUtilities.StringFormatRender);
						}
				}

				if (g_CaretShape == this && g_CaretAnimateState && !emptyPrompt && resources.ViewIsFocal)
					gr.Rectangle(Caret.Location.X - CARETLEFTOFFSET, Caret.Location.Y, Convert.ToSingle(CARETWIDTH), Caret.LineHeight, null, resources.TextBrush);
				// if not g_bolCaretAnimateState then nil: other state is hidden
			}
			if (resources.TextBoundsPen != null && !boundsRectOrEmpty.IsEmpty)
				gr.Rectangle(boundsRectOrEmpty, resources.TextBoundsPen);

			if (!m_TextToPageTransform.IsIdentity)
				gr.EndTransform();
		}

		#region Caret/focus
		// I was going to put the caret info in the View (or as an object maintained by the view) but that just makes coding from here awkward
		// don't want this to be instance info - just cluttering up Shape - of which there can be thousands.
		// so although I don't like globals, for the caret it probably DOES make sense - there is only one within an app after all
		/// <summary>Shape containing caret</summary>
		private protected static Shape g_CaretShape;
		/// <summary>only defined if g_CaretShape is defined (otherwise the value is meaningless).  Note that this can be Empty even if the Caret exists (if this contains no text)</summary>
		protected static TextLocation Caret = TextLocation.Empty.SetPurpose(TextLocation.Purposes.Caret);
		/// <summary>currently drawn position.  This is based on Invalidation not called to Draw (i.e. this is the last position for which we did CaretInvalidateMe)</summary>
		private protected static TextLocation CaretDrawn = TextLocation.Empty;
		/// <summary>whether currently drawn</summary>
		private protected static bool g_CaretAnimateState = true;
		/// <summary>true skips next animation</summary>
		private protected static bool g_CaretSkipAnimate;
		private protected const float CARETWIDTH = 0.7f;
		/// <summary>Left edge of caret rectangle/symbol relative to actual location</summary>
		private protected const float CARETLEFTOFFSET = 0.25f;

		internal static void CaretDestroy()
		{
			if (g_CaretShape != null)
			{
				g_CaretShape.CaretLose();
			}
		}

		internal virtual void CaretLose()
		{
			if (this == g_CaretShape)
			{
				Globals.StoreEvent("CaretLose");
				//Debug.WriteLine("CaretLose: " + Me.ToString)
				ClearSelection();
				if (g_CaretAnimateState)
					CaretInvalidate(Caret); // no need to invalidate if in hidden state!
				g_CaretShape = null;
				Caret = TextLocation.Empty;
				Caret.Purpose = TextLocation.Purposes.Caret;
				CaretDrawn = TextLocation.Empty;
				if (string.IsNullOrEmpty(m_Label) && (Flags & GeneralFlags.NoEmptyLabelMessage) == 0)
					FormatText();
				Parent?.NotifyIndirectChange(this, ChangeAffects.GrabSpots);
			}
		}

		internal static void CaretAnimate() // called continuously by GUI - ignore if no caret
		{
			if (g_CaretShape == null)
				return;
			Debug.Assert(Caret.Purpose == TextLocation.Purposes.Caret && SelectionLocation.Purpose == TextLocation.Purposes.Selection && SelectionRange[0].Purpose == TextLocation.Purposes.SelectionRange0 && SelectionRange[1].Purpose == TextLocation.Purposes.SelectionRange1);
			if (g_CaretSkipAnimate)
				g_CaretSkipAnimate = false;
			else
			{
				CaretInvalidate(Caret);
				g_CaretAnimateState = !g_CaretAnimateState;
			}
		}

		protected static void CaretInvalidate(TextLocation caret) // whoever has the caret
		{
			if (caret.LineHeight == 0)
				return;
			Debug.Assert(g_CaretShape != null);
			RectangleF rct = new RectangleF(caret.Location.X - CARETLEFTOFFSET, caret.Location.Y, CARETWIDTH + 0.4f, caret.LineHeight);
			// seems to need some extra top and right
			rct.Inflate(0.2F, 0.2F);
			g_CaretShape.InvalidateTextRectangle(rct);
			//Debug.WriteLine("Inval caret: " + g_shpCaret.TextRectangleToPageRectangleContaining(rct).ToString)
		}

		/// <summary>acts on a shape; forces this to own caret</summary>
		internal virtual void CaretGain()
		{
			if (g_CaretShape == this)
				return;
			EnsureTextFormatted();
			CaretDestroy();
			Globals.StoreEvent("CaretGain in " + ID);
			g_CaretAnimateState = true; // always starts drawn
			Caret.LineHeight = 0; // will force calculation
			int character = 0;
			if (m_Label == null)
			{
				Globals.StoreEvent("CaretGain with Label=nothing");
			}
			g_CaretShape = this;
			if (character == 0 && (Flags & GeneralFlags.NoEmptyLabelMessage) == 0) // ie text empty
				FormatText(); // prompt has just appeared
			SetCaretCharacter(character, true);
			Parent?.NotifyIndirectChange(this, ChangeAffects.GrabSpots);
		}

		/// <summary>Positions the caret before the given character</summary>
		protected void SetCaretCharacter(int character, bool forceDisplay)
		{
			Caret = new TextLocation(character, this) { Purpose = TextLocation.Purposes.Caret };
			ClearSelection(false);
			CaretUpdateCoords(forceDisplay);
		}

		/// <summary>Positions the caret at the given location.  Will Invalidate</summary>
		protected void SetCaretLocation(TextLocation location, bool forceDisplay)
		{
			Caret = location;
			Caret.Purpose = TextLocation.Purposes.Caret;
			ClearSelection();
			CaretUpdateCoords(forceDisplay);
		}

		/// <summary>Called when the caret indices are correct, but the coordinates may have changed.  Will Invalidate</summary>
		protected void CaretRealculateCoordinates(bool forceDisplay)
		{
			if (Caret.IsValid)
				Caret.ResetLocation();
			CaretUpdateCoords(forceDisplay);
		}

		private void CaretUpdateCoords(bool forceDisplay)
		{
			// Although the Location is part of the Caret object, it is not necessarily calculated until it is accessed
			// this will do so, so that the coordinate is stored.  It will also invalidate if the coordinate has changed
			// if forceDisplay, it sets state to forced on so that it displays immediately.  This is used when cursoring around
			// (looks odd otherwise)
			Debug.Assert(g_CaretShape == this && Caret.IsValid);
			EnsureTextFormatted();
			// Special case: can have a single 0-height item if no chars - so last bracket is needed
			if (m_Fragments == null || m_Fragments.Count == 0 || m_Fragments.Count == 1 && m_Fragments[0].Bounds.Height == 0)
			{
				Caret = TextLocation.Zero.SetPurpose(TextLocation.Purposes.Caret);
				if (CaretDrawn.LineHeight == 0) // Avoid measuring if possible; this is relatively slow
				{
					// need to measure height to draw caret initially before any text is present.  Later can just use measured line height
					EnsureFont();
					SizeF sz = NetCanvas.MeasurementInstance.MeasureString("Ay", m_Font, format: GUIUtilities.StringFormatFindCharacter);
					Caret.LineHeight = sz.Height;
				}
				else
					Caret.LineHeight = CaretDrawn.LineHeight;
			}
			else
			{
				Caret.Fragment = m_Fragments[Caret.FragmentIndexIndex];
				Caret.LineHeight = Caret.Fragment.Bounds.Height;
			}

			if (!Caret.Location.Equals(CaretDrawn.Location) || !CaretDrawn.IsValid || Caret.LineHeight != CaretDrawn.LineHeight)
			{
				// Note that the conditions must be in this order; it is essential that we access Caret.Location
				if (CaretDrawn.IsValid)
					CaretInvalidate(CaretDrawn);
				CaretInvalidate(Caret);
				CaretDrawn = Caret;
			}
			if (forceDisplay)
			{
				g_CaretAnimateState = true;
				g_CaretSkipAnimate = true; // stops it turning off again right away in case timer is about to fire in just a moment
			}
		}

		internal bool HasCaret
		{ get { return this == g_CaretShape; } }

		internal static Shape CaretShape
		{ get { return g_CaretShape; } }

		/// <summary>Optionally called at some point early in a transaction to record the caret state</summary>
		/// <remarks>This is not called by default for all transactions, therefore foremost transactions the caret is not restored</remarks>
		internal static void StoreCaretState(Transaction transaction)
		{
			transaction.CaretState = new Tuple<Shape, TextLocation, TextLocation>(g_CaretShape, Caret, SelectionLocation);
		}

		internal static void RestoreCaretState(Transaction transaction)
		{
			if (transaction.CaretState == null)
				return; // position was not specified in this transaction
			Tuple<Shape, TextLocation, TextLocation> objState = (Tuple<Shape, TextLocation, TextLocation>)transaction.CaretState;
			if (objState.Item1 == null)
				CaretDestroy();
			else
			{
				objState.Item1.CaretGain();
				objState.Item1.SetCaretCharacter(objState.Item2.CharacterTotal, true); // can't use SetCaretLocaton since location.Fragment object may not be valid any longer
																					   // selection not restored atm.  Not sure that there is  an appropriate function, but also it may be better that the selection is not restored
			}
		}

		#endregion

		#region Selection
		/// <summary>If the user has selected a range of text, this is the other end to which they have selected</summary>
		/// <remarks>This is specifically the point to which the user has tried to select.  A slightly different range might actually be specified in SelectionRange</remarks>
		protected static TextLocation SelectionLocation = TextLocation.Empty.SetPurpose(TextLocation.Purposes.Selection);
		/// <summary>Represents the actual range lit up.</summary>
		/// <remarks>In some cases this can be greater than the range specified by the user (especially for equations, which may need to expand to a meaningful range</remarks>
		protected static TextLocation[] SelectionRange = { TextLocation.Empty.SetPurpose(TextLocation.Purposes.SelectionRange0), TextLocation.Empty.SetPurpose(TextLocation.Purposes.SelectionRange1) };

		/// <summary>Sets the selection to Empty, usually invalidating the old one.  Invalidation can be skipped if the oldest known to be broken</summary>
		protected void ClearSelection(bool invalidateOld = true)
		{
			Debug.Assert(this == g_CaretShape);
			if (invalidateOld && SelectionLocation.IsValid)
				InvalidateSelection();
			SelectionLocation = TextLocation.Empty.SetPurpose(TextLocation.Purposes.Selection);
			SelectionRange[0] = TextLocation.Empty.SetPurpose(TextLocation.Purposes.SelectionRange0);
			SelectionRange[1] = TextLocation.Empty.SetPurpose(TextLocation.Purposes.SelectionRange1);
		}

		protected virtual void InvalidateSelection()
		{
			Debug.Assert(this == g_CaretShape);
			if (!HasSelection)
				return;
			RectangleF invalid = new RectangleF();
			for (int fragment = SelectionRange[0].FragmentIndexIndex; fragment <= SelectionRange[1].FragmentIndexIndex; fragment++)
			{
				Geometry.Extend(ref invalid, SelectionBoundsWithinFragment(fragment));
			}
			InvalidateTextRectangle(invalid);
		}

		protected virtual void SelectTextTo(TextLocation location)
		{
			Debug.Assert(this == g_CaretShape && Caret.IsValid);
			if (SelectionLocation.IsValid)
				InvalidateSelection();
			SelectionLocation = location.SetPurpose(TextLocation.Purposes.Selection);
			switch (Caret.CompareTo(SelectionLocation))
			{
				case -1:
					SelectionRange[0] = Caret;
					SelectionRange[1] = SelectionLocation;
					break;
				case 0: // The selection and caret are in the same place, so nothing is actually selected
					SelectionLocation = TextLocation.Empty;
					return;
				case 1:
					SelectionRange[0] = SelectionLocation;
					SelectionRange[1] = Caret;
					break;
			}
			SelectionRange[0].Purpose = TextLocation.Purposes.SelectionRange0;
			SelectionRange[1].Purpose = TextLocation.Purposes.SelectionRange1;
			InvalidateSelection();
		}

		public bool HasSelection
		{ get { return g_CaretShape == this && SelectionLocation.IsValid; } }

		protected virtual void DrawTextSelection(Canvas gr, DrawResources resources)
		{
			// Part of DrawNewText; the graphics has only been transformed into private text coordinates
			using (Fill br = gr.CreateFill(Color.LightBlue))
			{
				for (int fragment = SelectionRange[0].FragmentIndexIndex; fragment <= SelectionRange[1].FragmentIndexIndex; fragment++)
				{
					gr.Rectangle(SelectionBoundsWithinFragment(fragment), null, br);
				}
			}

		}

		/// <summary>Returns the selection area within this fragment, which may be part of the fragment if it is partially selected</summary>
		/// <param name="fragmentIndex">Fragment index, must be one which is within the selection range</param>
		/// <returns>Selected rectangle in text coordinates</returns>
		protected RectangleF SelectionBoundsWithinFragment(int fragmentIndex)
		{
			RectangleF bounds = m_Fragments[fragmentIndex].Bounds;
			if (fragmentIndex == SelectionRange[0].FragmentIndexIndex)
			{
				// Drawing the first fragment; it may not be entirely selected
				float offset = SelectionRange[0].Location.X - bounds.X;
				if (offset > 0)
				{
					bounds.X += offset;
					bounds.Width -= offset;
				}
			}
			if (fragmentIndex == SelectionRange[1].FragmentIndexIndex) // Likewise with last fragment
			{
				float offset = bounds.Right - SelectionRange[1].Location.X;
				if (offset > 0)
					bounds.Width -= offset;
			}
			return bounds;
		}

		#endregion

		#region Editing, caret movement
		/// <summary>Performs backspace or delete action.</summary>
		/// <param name="forwards">True to delete to the right of the caret (Delete rather than Backspace)</param>
		/// <remarks>If there is a selection the masses deleted and the direction is irrelevant</remarks>
		protected virtual VerbResult TextTypeDelete(bool forwards)
		{
			Debug.Assert(HasCaret);
			if (m_Label == null)
				return VerbResult.Unchanged;
			int position;
			int fragmentIndex;
			if (HasSelection)
			{
				fragmentIndex = SelectionRange[0].FragmentIndexIndex;
				m_Label = m_Label.Remove(Convert.ToInt32(SelectionRange[0].CharacterTotal), Convert.ToInt32(SelectionRange[1].CharacterTotal - SelectionRange[0].CharacterTotal));
				position = SelectionRange[0].CharacterTotal;
			}
			else
			{
				if (forwards)
					position = Caret.CharacterTotal == m_Label.Length ? Caret.CharacterTotal - 1 : Caret.CharacterTotal;
				else
					position = Caret.CharacterTotal - 1;
				if (position < 0)
					return VerbResult.Unchanged; // param can be -1 if backspace at start
				fragmentIndex = FragmentFromCharacter(position);
				m_Label = m_Label.Remove(position, 1);
			}
			// may need to format from previous line, as this can change line breaking.
			if (fragmentIndex > 0 && m_Fragments != null && m_Fragments[fragmentIndex - 1].HardBreak == false) // but no need if there was a return on end of previous
				fragmentIndex -= 1;
			Caret = TextLocation.Empty; // otherwise the formatting may try and update it
			FormatText(fragmentIndex);
			SetCaretCharacter(position, false);
			switch (LabelPosition)
			{
				case LabelPositions.TextShape:
				case LabelPositions.Line:
					m_Bounds = CalculateBounds();
					break;
			}
			return VerbResult.Continuing;
		}

		public virtual VerbResult TextType(string text)
		{
			// inserts the given text for typing
			try
			{
				Debug.Assert(HasCaret);
				text = text.Replace("\r\n", "\r");
				int fragmentIndex;
				int character;
				if (m_Label == null)
					m_Label = "";
				if (HasSelection)
				{
					fragmentIndex = SelectionRange[0].FragmentIndexIndex;
					m_Label = m_Label.Remove(SelectionRange[0].CharacterTotal, SelectionRange[1].CharacterTotal - SelectionRange[0].CharacterTotal);
					character = SelectionRange[0].CharacterTotal;
				}
				else
				{
					fragmentIndex = Caret.FragmentIndexIndex;
					character = Caret.CharacterTotal;
				}
				m_Label = m_Label.Insert(character, text);
				// may need to format from previous line, as this can change line breaking.
				if (fragmentIndex > 0 && m_Fragments != null && m_Fragments[fragmentIndex - 1].HardBreak == false) // but no need if there was a return on end of previous
				{
					// Not m_colFragments Is Nothing should be redundant, but we had an error in TextLine setting it to null and it blew up here.  It's save enough to skip this condition
					// and no point formatting for this, as it will be formatted below anyway
					fragmentIndex -= 1;
				}
				FormatText(fragmentIndex);
				SetCaretCharacter(character + text.Length, false);
				switch (LabelPosition)
				{
					case LabelPositions.TextShape:
					case LabelPositions.Line:
						m_Bounds = CalculateBounds(); // RectangleF.Empty
						break;
				}
			}
			catch (IndexOutOfRangeException)
			{
				// had errors here, but can't see why.
				Utilities.LogSubError("IndexOutOfRangeException in TextType.  HasSelection=" + HasSelection + ", Caret.CharacterTotal=" + Caret.CharacterTotal + ", label length=" + m_Label.Length);
				CaretHomeEnd(1, false);
				return VerbResult.Rejected;
			}
			return VerbResult.Continuing;
		}

		/// <summary>Returns either Caret or Selection location depending on parameter.  If there is no Selection that will also return the Caret (rather than an undefined location)</summary>
		private TextLocation Location(bool selection)
		{
			if (!selection || !HasSelection)
				return Caret;
			return SelectionLocation;
		}

		/// <summary>Moves the Caret (or selection) left or right</summary>
		/// <param name="delta">-1 or +1 usually.  This can put the position out of bounds; this function will check</param>
		/// <param name="moveSelection"></param>
		protected virtual void CaretMove(int delta, bool moveSelection)
		{
			Debug.Assert(HasCaret);
			if (m_Label == null)
				return;
			int position = Location(moveSelection).CharacterTotal + delta;
			if (position < 0)
				position = 0;
			if (position > m_Label.Length)
				position = m_Label.Length;
			if (!moveSelection)
				SetCaretCharacter(position, true);
			else
				SelectTextTo(new TextLocation(position, this));
		}

		/// <summary>Moves caret/selection between lines (or fragments specifically)</summary>
		/// <returns>Returns true if successfully moved something</returns>
		protected virtual bool CaretMoveLine(int delta, bool moveSelection)
		{
			Debug.Assert(HasCaret);
			if (m_Label == null)
				return false;
			if (Location(moveSelection).Fragment == null)
				return false;
			int newFragment = Location(moveSelection).Fragment.Index + delta;
			// check if moving too far:
			if (newFragment < 0)
				return false;
			if (newFragment >= m_Fragments.Count)
				return false;
			float X = Location(moveSelection).Location.X - m_Fragments[newFragment].Bounds.X; // position of caret WITHIN NEW FRAGMENT
																							  // (copes if fragments mis-aligned)
			int charIndex = FindCharacterWithinFragment(newFragment, ref X); // character WITHIN fragment
																			 // however... we don't want to put the caret after any trailing white space on the end of the target line, as that would
																			 // actually draw it on the beginning of the next
			if (charIndex == m_Fragments[newFragment].Text.Length)
				while (charIndex > 0 && char.IsWhiteSpace(m_Fragments[newFragment].Text[charIndex - 1]))
				{
					charIndex -= 1;
				}
			// Cannot use the faster form (i.e. using the testing information to form a TextLocation, because the character is adjusted above)
			charIndex = m_Fragments[newFragment].StartOffset + charIndex;
			if (!moveSelection)
				SetCaretCharacter(charIndex, true);
			else
				SelectTextTo(new TextLocation(charIndex, this));
			return true;
		}

		protected virtual void CaretHomeEnd(int direction, bool moveSelection)
		{
			Debug.Assert(HasCaret);
			if (m_Label == null || m_Fragments == null)
				return;
			Fragment fragment = Location(moveSelection).Fragment;
			if (fragment == null) // this should be because the text is empty - in which case Home/End does nothing anyway.  But even if text not empty, the code below would crash
				return;
			int character; //= Location(bolMoveSelection).CharacterTotal
			if (direction < 0)
				character = fragment.StartOffset;
			else
			{
				character = fragment.StartOffset + fragment.Text.Length;
				// however... we don't want to put the caret after any trailing white space on the end of the target line, as that would
				// actually draw it on the beginning of the next
				if (fragment.Text.Length > 0)
				{
					while (character > fragment.StartOffset && char.IsWhiteSpace(m_Label[character - 1]))
					{
						character -= 1;
					}
				}
			}
			if (!moveSelection)
				SetCaretCharacter(character, true);
			else
				SelectTextTo(new TextLocation(character, this));
		}

		#endregion

		#region Transformations
		protected Matrix m_PageToTextTransform; // these are created as needed; they will always either both be defined or both be Nothing
		protected Matrix m_TextToPageTransform;

		// will probably be MustOverride once all shapes have been moved to the new text logic
		protected virtual void SetTextTransforms()
		{
			//both the matrices MUST be defined at the end of this function
			switch (LabelPosition)
			{
				case LabelPositions.Bounds:
					SetTextTransformsImpl(Bounds.Location);
					break;
				case LabelPositions.Line:
					PointF[] aPts_1 = LineLabelGetPoints();
					SetTextTransformsImpl(aPts_1[0], Geometry.VectorAngle(aPts_1[0], aPts_1[1]));
					break;
				case LabelPositions.RotatedRectangle:
					PointF[] aPts = RectangleLabelRearrangePoints(RectangleLabelGetPoints());
					SetTextTransformsImpl(aPts[0], Geometry.VectorAngle(aPts[0], aPts[1]));
					break;
				// LabelPosition = TextShape this should have been overridden
				default:
					throw new InvalidOperationException("Unexpected LabelPosition");
			}
		}

		protected void SetTextTransformsImpl(PointF origin, float angle = Geometry.PI / 2)
		{
			//the normal system for the TextToPage (used in drawing) will be to Translate to the origin and rotate by the angle
			// default ANGLE90 gives horizontal  (0 is vertically up)
			m_TextToPageTransform = new Matrix();
			m_TextToPageTransform.Translate(origin.X, origin.Y);
			if (Math.Abs(angle - Geometry.ANGLE90) > Geometry.NEGLIGIBLEANGLE)
				m_TextToPageTransform.Rotate(Geometry.DotNetAngle(Geometry.NormaliseAngle(angle)));
			m_PageToTextTransform = m_TextToPageTransform.Clone();
			m_PageToTextTransform.Invert();
		}

		/// <summary>returns smallest page rectangle entirely containing page rectangle - page one may be much larger if text rotated</summary>
		/// <remarks>Overridden for speed in FreeText which doesn't need to use transforms</remarks>
		protected virtual RectangleF TextRectangleToPageRectangleContaining(RectangleF rct)
		{
			if (LabelPosition == LabelPositions.Bounds)
			{
				// doesn't rotate, so can be done the simple way
				rct.Offset(Bounds.Location);
				return rct;
			}
			PointF[] aPoints = rct.GetPoints();
			if (m_TextToPageTransform == null)
				SetTextTransforms();
			m_TextToPageTransform.TransformPoints(aPoints);
			RectangleF rctNew = new RectangleF();
			foreach (PointF pt in aPoints)
			{
				Geometry.Extend(ref rctNew, pt);
			}
			return rctNew;
		}

		/// <summary>Invalidates rectangle quoted in text coordinates</summary>
		protected void InvalidateTextRectangle(RectangleF invalidate)
		{
			if (Parent == null)
				return; // can happen with new spawned text areas
			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded, TextRectangleToPageRectangleContaining(invalidate));
		}

		#endregion

		/// <summary>Defaults true.  Mainly intended for function keys, and especially for palettes that work by sending keys - so that buttons can be greyed out at times.</summary>
		/// <remarks>Always safe to return true, although maybe not perfect</remarks>
		public virtual bool IsKeyApplicable(char ch) => true;

		/// <summary>Defaults true.  Mainly intended for function keys, and especially for palettes that work by sending keys - so that buttons can be greyed out at times.</summary>
		/// <remarks>Always safe to return true, although maybe not perfect</remarks>
		public virtual bool IsKeyApplicable(Keys eKey) => true;

		#endregion

		#region Double-click
		// this happens when the user double clicks a shape and the selector is enabled
		// text within shapes not really supported in SAW now - not much need for it, and this function is used as a way of adding scripting

		/// <summary>Should return Nothing if not supported; can return "" to enable double-click with no menu entry</summary>
		internal virtual string DoubleClickText()
		{
			if (Parent is Scriptable)
				return null;
			//if (SupportsTextLabel)
			//{
			//	if (!HasText(true))
			//		return Strings.Item("Add_Text");
			//	else
			//	{
			//		if (!string.IsNullOrEmpty(m_Label) && AnimationController.HasAnimation(this))
			//			return Strings.Item("Edit_Text_Popup");
			//		return Strings.Item("Edit_Text");
			//	}
			//}
			return ""; // no need to have it on menu - there is an existing entry for it Strings.Item("Verb_MakeActive");
		}

		internal virtual void DoDoubleClick(EditableView view, EditableView.ClickPosition.Sources source)
		{// in SAW this converts any dumb element into active content (with a Scriptable around it)
			Verb verb = Verb.Find(Codes.MakeActive);
			if (!verb.IsApplicable(view))
				return;

			if (GUIUtilities.QuestionBox(Strings.Item("SAW_Edit_AutoActive"), MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;

			Globals.Root.PerformAction(verb);
			if (Parent is Scriptable scriptable)
			{
				scriptable.DoDoubleClick(view, source);
			}
			//if (!SupportsTextLabel)
			//	return;
			//if (HasCaret && !string.IsNullOrEmpty(m_Label))
			//{ // Splash revised text in popup, but not so useful here
			//}
			//else
			//{
			//	CreateLabel((TextStyleC)Globals.StyleParameterDefaultObject(Parameters.TextColour));
			//	Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded | ChangeAffects.StyleInformation);
			//}
		}

		/// <summary>Returns true if it is possible to DoDoubleClick with the given set of items selected.  This is called on first item.  Default is to only return true if 1 item (this) is selected</summary>
		internal virtual bool CanDoubleClickWith(IEnumerable<Shape> selection)
		{
			return selection.Count() == 1;
		}

		#endregion

		#region Links
		// a shape can store a list of links to other shapes (assumed to be on the same page)
		// if it depends on those shapes, or needs to react to changes to the shapes
		// Each reference contains the target shape and an integer index - the meaning of the index is up to the derived class
		// The list itself will be nothing unless the shape actually uses it.  An empty list implies the shape is interested but nothing is linked
		protected readonly struct Link
		{
			public readonly Shape Shape; // can be nothing - it is permissible to store a link to Nothing
			public readonly int Index;

			public Link(Shape shape, int index = 0)
			{
				Shape = shape;
				Index = index;
			}

			internal Link(Socket socket)
			{
				Shape = socket.Shape;
				Index = socket.Index;
			}

		}

		protected List<Link> m_Links;

		public enum StatusValues
		{
			Creating,
			Complete,
			/// <summary>or edited in such a way that any linked shape needs to update its position</summary>
			Moved,
			/// <summary>Can also be set during Load to indicate it is corrupt and should not be added to the page</summary>
			Deleted,
			/// <summary>setting this causes this object to respond to PerformLinkedChanges regardless of any links</summary>
			RequiresLinkUpdate
		}

		internal StatusValues Status = StatusValues.Complete;
		// not stored in data.  Set by GUI
		// used when objects link - if Moved other objects which link to this still need to be notified
		// not always set on creation (can be no links in!).  Usually set by gui NOT internally (cos lots of shapes but only a few GUI places need to set this)
		// storing the transaction resets status to Complete

		protected void AddLink(Shape shape, int index = 0)
		{
			if (m_Links == null) m_Links = new List<Link>();
			Debug.Assert(shape == null || !HasLinkTo(shape), "Code is not currently safe against a shape adding links to the same other shape multiple times");
			Debug.Assert(shape != this, "Shape is adding link to self");
			m_Links.Add(new Link(shape, index));
		}

		protected void AddLinks(List<Shape> shapes)
		{
			// A quick way of adding multiple links; all will have Index = 0.  Some shapes don't use Index so can use this function
			if (m_Links == null) m_Links = new List<Link>();
			foreach (Shape shape in shapes)
			{
				Debug.Assert(shape != this, "Shape is adding link to self");
				Debug.Assert(!HasLinkTo(shape), "Code is not currently safe against a shape adding links to the same other shape multiple times");
				m_Links.Add(new Link(shape));
			}
		}

		protected bool HasLinkTo(Shape shape)
		{
			return m_Links?.Any(objLink => objLink.Shape == shape) ?? false;
		}

		/// <summary>make any changes to this shape caused by changes to the other shapes that it links to</summary>
		/// <param name="transaction"></param>
		/// <param name="editor"></param>
		/// <returns>return true if this shape has changed (potentially). Can just return true if the other shape status triggers an update, do not need to check whether the values in this shape actually change</returns>
		/// <remarks>The status value in the other shapes is used to detect when an update is needed.
		/// Must do objTransaction.Edit (me) if changing.
		/// Default implementation calls OnLinkedChanged if one or more linked shapes have changed
		/// NOTE: this requires that shapes which we link to do not themselves have further links; the function below will not correctly cascade</remarks>
		internal virtual bool PerformLinkedChanges(Transaction transaction, frmMain editor)
		{
			bool changed;
			if (Status == StatusValues.RequiresLinkUpdate)
				changed = true;
			else
			{
				if (m_Links == null)
					return false;
				changed = m_Links.Any(link => link.Shape != null && link.Shape.Status != StatusValues.Complete);
			}
			if (changed)
			{
				transaction.Edit(this);
				OnLinkedChanged();
			}
			return changed;
		}

		protected virtual void OnLinkedChanged()
		{
			// if a shape changes itself it must use the parent notification to trigger any necessary refreshes
			// this shape has already been added to the transaction, so does not need to do anything transactional
			Debug.Fail("OnLinkedChanged not implemented");
		}

		// also code in Load/Save

		#endregion

		#region Prompts

		[Flags()]
		public enum ShapeVerbs
		{
			Start = 1,
			Choose = 2,
			Complete = 4,
			Cancel = 8,
			Increment = 16,
			Decrement = 32,
			Type = 64,
			// non-prefixed ones should be kept at the end; code below uses test >= Info
			Info = 128, // used by the GUI (e.g. hover).  NOTE if this is used "Prompts_" is not prefixed onto the TextID and ImageID within a Prompt
			Warning = 256, // likewise without prefix, used by GUI
			NotDrag = 512, // only used in prompting; indicates to ignore the prompt if dragging
		}

		internal class Prompt
		{
			public readonly ShapeVerbs Verbs;
			private readonly string TextID;
			private readonly string ImageID;
			private readonly string SecondTextID;
			// now supports possibility of items not retrieved by ID (eg PaletteShapes)
			private readonly string CustomText;
			private readonly Image CustomImage;

			public Prompt(ShapeVerbs verbs, string textID, string imageID, string secondTextID = "")
			{
				Verbs = verbs;
				TextID = textID;
				ImageID = imageID;
				SecondTextID = secondTextID;
				Debug.Assert(verbs >= ShapeVerbs.Info || !textID.StartsWith("Prompts_") && !imageID.StartsWith("Prompts_"), "Prompt text and image IDs should not include the \'Prompts_\' prefix, unless the Verb = Info");
			}

			public Prompt(ShapeVerbs verbs, string sharedID)
			{
				Verbs = verbs;
				TextID = sharedID;
				ImageID = sharedID;
				if (Strings.Exists(sharedID + "_SubText"))
					SecondTextID = sharedID + "_SubText";
				else
					SecondTextID = "";
				Debug.Assert(verbs >= ShapeVerbs.Info || !sharedID.StartsWith("Prompts_"), "Prompt text and image IDs should not include the \'Prompts_\' prefix, unless the Verb >= Info");
			}

			public Prompt(ShapeVerbs verbs, string customText, Image image, string secondTextID = "")
			{
				Verbs = verbs;
				CustomText = customText;
				CustomImage = image;
				SecondTextID = secondTextID;
			}

			public override bool Equals(object obj)
			{
				Prompt prompt = obj as Prompt;
				if (Verbs != prompt?.Verbs)
					return false;
				if (TextID != prompt.TextID)
					return false;
				if (ImageID != prompt.ImageID)
					return false;
				if (SecondTextID != prompt.SecondTextID)
					return false;
				if (CustomText != prompt.CustomText)
					return false;
				if (CustomImage != prompt.CustomImage)
					return false;
				return true;
			}

			public override int GetHashCode()
			{
				return Verbs.GetHashCode() ^ TextID.GetHashCode()
					^ (ImageID?.GetHashCode() ?? 0)
					^ (SecondText?.GetHashCode() ?? 0)
					^ (CustomText?.GetHashCode() ?? 0)
					^ (CustomImage?.GetHashCode() ?? 0);
			}

			public static bool ListsEqual(List<Prompt> A, List<Prompt> B)
			{
				if (A == null || B == null)
					return false;
				if (A.Count != B.Count)
					return false;
				for (int index = 0; index <= A.Count - 1; index++)
				{
					if (!A[index].Equals(B[index]))
						return false;
				}
				return true;
			}

			/// <summary>Returns the actual text (rather than translation ID) from the main text.  Does not return the secondary text</summary>
			public string FullText
			{
				get
				{
					if (!string.IsNullOrEmpty(CustomText))
						return CustomText;
					if ((Verbs & ~ShapeVerbs.NotDrag) < ShapeVerbs.Info)
						return Strings.Item("Prompts_" + TextID);
					return Strings.Item(TextID);
				}
			}

			// the returned image should not be disposed - it may have come from a PaletteShap stored thumbnail
			public Image Image
			{
				get
				{
					if (CustomImage != null)
						return CustomImage;
					if ((Verbs & ~ShapeVerbs.NotDrag) < ShapeVerbs.Info)
						return (Image)GUIUtilities.RM.GetObject("Prompts_" + ImageID);
					if (string.IsNullOrEmpty(ImageID))
						return null;
					return (Image)GUIUtilities.RM.GetObject(ImageID);
				}
			}

			public bool HasSecondText => !string.IsNullOrEmpty(SecondTextID);
			public string SecondText => Strings.Item(SecondTextID);
		}

		internal virtual List<Prompt> GetPrompts()
		{
			return new List<Prompt>();
		}

		#endregion

		#region Sorting

		internal static int XSort(Shape A, Shape B) => A.Bounds.X.CompareTo(B.Bounds.X);

		internal static int YSort(Shape A, Shape B) => A.Bounds.Y.CompareTo(B.Bounds.Y);

		/// <summary>Sorts by Y then X</summary>
		internal static int YXSort(Shape A, Shape B)
		{
			int result = A.Bounds.Y.CompareTo(B.Bounds.Y);
			if (result == 0)
				result = A.Bounds.X.CompareTo(B.Bounds.X);
			return result;
		}

		#endregion

	}

}
