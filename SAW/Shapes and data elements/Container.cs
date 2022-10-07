using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Linq;


namespace SAW.Shapes
{
	/// <summary>A shape which contains other shapes that are still movable and independent - but will move with the container (and be deleted if the container is deleted) </summary>
	public class Container : Filled, IShapeContainer, IShapeTarget
	{

		protected List<Shape> m_Shapes = new List<Shape>();
		public Borders BorderShape;

		public enum Borders : byte
		{
			Rectangle,
			Rounded,
			Ellipse
		}

		#region Information

		public override Shapes ShapeCode => Shapes.Container;
		internal override LabelModes LabelMode => LabelModes.NotSupported;
		public override GeneralFlags Flags => base.Flags | GeneralFlags.ProtectBounds;
		protected override LineLogics LineLogic => BorderShape == Borders.Rectangle ? LineLogics.Custom : LineLogics.UsePath;

		// Unlike a group most functionality does not necessarily propagate to the internal shapes
		public override AllowedActions Allows
		{
			get
			{
				const AllowedActions allows = AllowedActions.PermittedArea | AllowedActions.Tidy | AllowedActions.TransformMove;
				// Some others can be allowed if the contained shapes will support it
				AllowedActions test = AllowedActions.MirrorFlipOnly | AllowedActions.TransformLinearStretch | AllowedActions.TransformScale;
				if (m_Shapes != null) // Note that this isaccessed by Lined during construction, and the list will be Nothing at that point
				{
					// Even though m_colShapes is constructed automatically, and this test looks redundant
					foreach (Shape shape in m_Shapes)
					{
						test = test & shape.Allows;
					}
				}
				return allows | test;
			}
		}

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (requested == SnapModes.Angle)
				return SnapModes.Off;
			return base.SnapNext(requested);
		}

		#endregion

		#region Verbs
		// first click place any quarter; second the opposite corner
		// most of this is copied from Button (puppy class cannot inherit; it is easier to inherit this from Filled to get all the styling)
		private static PointF g_StartPoint; // in order to allow drawing out any corner first
		private const float MINSIZE = 5;
		public override VerbResult Start(ClickPosition position)
		{
			g_StartPoint = position.Snapped;
			Debug.Assert(LineStyle != null);
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			m_Bounds = new RectangleF(g_StartPoint.X, g_StartPoint.Y, MINSIZE, MINSIZE);
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Float(ClickPosition position)
		{
			RectangleF rct = Geometry.RectangleFromPoints(g_StartPoint, position.Snapped);
			if (rct.Width < MINSIZE || rct.Height < MINSIZE)
				return VerbResult.Rejected;
			DiscardPath();
			m_Bounds = rct;
			m_Refresh = RectangleF.Empty;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(ClickPosition position) => Complete(position);

		public override VerbResult Complete(ClickPosition position)
		{
			VerbResult result = Float(position);
			if (result == VerbResult.Rejected)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult CompleteRetrospective()
		{
			// shouldn't really get this far - the initial Choose click will have completed - if allowed
			Utilities.LogSubError("Container.CompleteRetrospective not expected");
			return VerbResult.Continuing;
		}

		internal override bool DefaultStylesApplied()
		{
			bool changed = base.DefaultStylesApplied();
			if (!FillStyle.Colour.IsEmpty)
			{
				FillStyle.Colour = Color.Empty;
				changed = true;
			}
			return changed;
		}

		#endregion

		#region Data - copied from Group with only minor amendments
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Container container = (Container)other;
			m_Bounds = container.m_Bounds;
			if (depth >= CopyDepth.Undo)
			{
				Overflow = container.Overflow;
				BorderShape = container.BorderShape;
			}
			if (depth == CopyDepth.Transform && m_Shapes.Count == container.m_Shapes.Count)
			{
				// excludes first time -when we must clone the shapes to get new copies to store the differing coords
				for (int index = 0; index <= m_Shapes.Count - 1; index++)
				{// not sure inner condition ever executes?
					if (m_Shapes[index] != container.m_Shapes[index])
						m_Shapes[index].CopyFrom(container.m_Shapes[index], depth, mapID);
				}
			}
			else if (depth == CopyDepth.Duplicate)
			{
				// Duplicate fully duplicates all of the contained items
				foreach (Shape shape in container.m_Shapes)
				{
					// need to check it hasn't already been cloned.  This might happen when container is selected along with contents
					if (mapID.ContainsKey(shape.ID))
						continue;
					Shape create = (Shape)shape.Clone(mapID);
					create.Parent = this;
					m_Shapes.Add(create);
				}
			}
			else
			{
				// XXXundo requires all shapes are copied, because they could later be edited by styling changes
				// mustn't create new instances as this causes problems when dragging (the GUI may also be reversing changes to the instance referenced by the GUI)
				// The list of references is updated.  It's the shape themselves are changed, they will need to be separately added to a transaction
				m_Shapes.Clear();
				m_Shapes.AddRange(container.m_Shapes);
			}
		}

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Bounds = reader.ReadRectangleF();
			if (reader.Version >= 109)
			{
				Overflow = (OverflowModes)reader.ReadByte();
				BorderShape = (Borders)reader.ReadByte();
			}
			m_Shapes = reader.ReadShapeList();
			foreach (Shape shape in m_Shapes)
			{
				shape.Parent = this;
			}
			if (ShapeCode == Shapes.Container)
				FinishedModifyingContents(null);
			// Fills in the Z order fields (which are not saved)
			// Derived shapes should call this at the end of their own Load functions
			// (It cannot safely be done here, because Flow rearranges the contents which won't work until the Flow data is loaded)
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Bounds);
			if (writer.Version >= 109)
			{
				writer.Write((byte)Overflow);
				writer.Write((byte)BorderShape);
			}
			writer.WriteDatumList(m_Shapes);
		}

		protected internal override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			foreach (Shape shape in m_Shapes)
			{
				shape.UpdateReferencesObjectsCreated(document, reader);
			}
		}

		protected internal override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			foreach (Shape shape in m_Shapes)
			{
				shape.UpdateReferencesIDsChanged(mapID, document);
			}
		}

		internal override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			foreach (Shape shape in m_Shapes)
			{
				shape.Iterate(fn);
			}
		}

		protected override bool IdenticalToShape(Shape other)
		{
			if (!base.IdenticalToShape(other))
				return false;
			Container container = (Container)other;
			if (BorderShape != container.BorderShape)
				return false;
			if (m_Shapes.Count != container.m_Shapes.Count)
				return false;
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				if (!m_Shapes[index].IdenticalTo(container.m_Shapes[index]))
					return false;
			}
			return true;
		}

		protected internal override void WriteExportText(IndentStringBuilder output)
		{
			base.WriteExportText(output);
			foreach (Shape shape in Contents)
				shape.WriteExportText(output);
		}

		#endregion

		#region Coordinates
		protected RectangleF m_Refresh; // because the refresh bounds can be quite slow to calculate, they are cached

		/// <summary>Sets the bounding box for this item.  Transaction is optional and should be provided only if you want other affected shapes included in the transaction</summary>
		public virtual void SetBounds(RectangleF bounds, Transaction transaction = null)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
				throw new ArgumentException();
			m_Bounds = bounds;
			m_Refresh = RectangleF.Empty;
		}

		protected override RectangleF CalculateBounds() => throw new InvalidOperationException($"Bounds of {this} not set");

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			List<UserSocket> sockets = new List<UserSocket>();
			foreach (Shape shape in m_Shapes)
			{
				List<UserSocket> col = shape.GetPointsWhichSnapWhenMoving();
				if (col != null)
					sockets.AddRange(col);
			}
			if (BorderShape == Borders.Rectangle)
			{
				sockets.Add(UserSocket.CreateForPoint(Bounds.BottomLeft()));
				sockets.Add(UserSocket.CreateForPoint(Bounds.BottomRight()));
				sockets.Add(UserSocket.CreateForPoint(Bounds.TopRight()));
				sockets.Add(UserSocket.CreateForPoint(Bounds.Location));
			}
			return sockets;
		}

		internal override bool StartGrabMove(GrabMovement grab)
		{
			// Must enrol the individual shapes in the movement, so that backup copies of them are created
			if (!base.StartGrabMove(grab))
				return false;
			if (GrabTransformsContents(grab))
			{
				foreach (Shape shp in m_Shapes)
				{
					if (!shp.StartGrabMove(grab))
						return false;
				}
			}
			return true;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			// Can't use the standard ApplyTransformation, because we don't want to move the individual shapes
			// If the individual shapes are supposed to be moved, they also need to be restored to their original position via GrabMove which will move them
			switch (move.GrabType)
			{
				case GrabTypes.Move:
				case GrabTypes.Radius:
				case GrabTypes.EdgeMoveH:
				case GrabTypes.EdgeMoveV:
				case GrabTypes.CornerResize:
					// Would usually be: this.ApplyTransformation(move.Transform).  Relevant contents pasted below:
					move.Transform.TransformRectangle(ref m_Bounds);
					base.ApplyTransformation(move.Transform); // Possibly useful because it does some decaching
					m_Refresh = RectangleF.Empty;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (GrabTransformsContents(move))
				foreach (Shape shape in m_Shapes)
				{
					shape.GrabMove(move); // note must NOT be DoGrabMove - the outer GrabMove does the restoration of the original coordinates
				}
		}

		protected virtual bool GrabTransformsContents(GrabMovement move) => move.GrabType == GrabTypes.Move || move.ControlKey;

		public override void ApplyTransformation(Transformation transformation)
		{
			transformation.TransformRectangle(ref m_Bounds);
			base.ApplyTransformation(transformation); // Possibly useful because it does some decaching
			foreach (Shape shape in m_Shapes)
			{
				shape.ApplyTransformation(transformation);
			}
			m_Refresh = RectangleF.Empty;
		}

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			if (LineLogic == LineLogics.UsePath)
				return base.HitTestDetailed(clickPoint, scale, true);
			return true;// clicking anywhere within the bounds is sufficient for rectangle
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			// from Group, but omitting rotation which is never allowed
			// always works as rectangle, regardless of border shape
			AllowedActions allows = Allows;
			// this is potentially somewhat slow to calculate
			// But if Control is not pressed, then we don't update the contained shapes, and any GrabSpot is allowed
			if ((Control.ModifierKeys & Keys.Control) == 0)
				allows = AllowedActions.TransformMove | AllowedActions.TransformScale | AllowedActions.TransformLinearStretch;
			if ((allows & AllowedActions.TransformMove) == 0)
				return null;
			List<GrabSpot> list = new List<GrabSpot>();
			if ((allows & AllowedActions.TransformScale) > 0)
				AddBoundingGrabSpots(list, scale, (Control.ModifierKeys & Keys.Control) == 0);
			foreach (GrabSpot g in list)
			{
				if (g.GrabType != GrabTypes.Move)
				{
					List<Prompt> newList = new List<Prompt>(g.Prompts);
					newList.Add(new Prompt(ShapeVerbs.Info, "Prompts_ContainerControl"));
					g.Prompts = newList.ToArray();
				}
			}
			return list;
			// DoGrabMove implementation in base Shape works OK since it is transformation based, and we don't do any single vertex changes
		}

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			if (LineLogic == LineLogics.Custom)
			{
				// Copied from BoundsDefines
				List<Target> targets = new List<Target>();
				AddLineTargets(this, targets, floating, m_Bounds.GetPoints(), true);
				targets.Add(new Target(this, Centre, Target.Types.Centre, floating));
				return targets;
			}
			else
				return base.GenerateTargetsDefault(floating, true);
		}

		protected override void CreatePath()
		{
			switch (BorderShape)
			{
				case Borders.Ellipse:
					m_Path = new System.Drawing.Drawing2D.GraphicsPath();
					m_Path.AddEllipse(m_Bounds);
					break;
				case Borders.Rounded:
					m_Path = GUIUtilities.CreateRoundedRectPath(m_Bounds, Math.Min(m_Bounds.Width, m_Bounds.Height) / 6);
					break;
				default:
					m_Path = new System.Drawing.Drawing2D.GraphicsPath();
					m_Path.AddRectangle(m_Bounds);
					break;
			}
		}

		/// <summary>Moves the child element to be within this one, if it protrudes out to any side</summary>
		internal void BringChildWithinBounds(Shape child, Transaction transaction)
		{
			float X = 0;
			float Y = 0;
			transaction.Edit(child);
			if (child.Bounds.Right > Bounds.Right)
				X = Bounds.Right - child.Bounds.Right;
			if (child.Bounds.Bottom > Bounds.Bottom)
				Y = Bounds.Bottom - child.Bounds.Bottom;
			if (child.Bounds.X < Bounds.X)
				X = Bounds.X - child.Bounds.X;
			if (child.Bounds.Y < Bounds.Y)
				Y = Bounds.Y - child.Bounds.Y;
			if (X != 0 || Y != 0)
			{
				var xform = new TransformMove(X, Y);
				child.ApplyTransformation(xform);
			}
		}

		#endregion

		#region IShapeParent and similar
		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			base.AddToFlatList(list, purpose);
			foreach (Shape shape in m_Shapes)
			{
				shape.AddToFlatList(list, purpose);
			}
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			// just passes along the notification to its container
			if ((affected & ChangeAffects.Bounds) > 0)
				m_Refresh = RectangleF.Empty; // not sure if this necessary or not
			Parent.NotifyIndirectChange(shape, affected);
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			// just passes along the notification to its container
			if ((affected & ChangeAffects.Bounds) > 0)
				m_Refresh = RectangleF.Empty; // not sure if this necessary or not
			Parent.NotifyIndirectChange(shape, affected, area);
		}

		internal override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			base.NotifyEnvironmentChanged(change);
			foreach (Shape shape in m_Shapes)
			{
				shape.NotifyEnvironmentChanged(change);
			}
		}

		public List<Shape> Contents => m_Shapes;

		public virtual void FinishedModifyingContents(Transaction transaction, GrabMovement move = null)
		{
			// can be used if the Z-order is changed to reset from first principles
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				m_Shapes[index].Z = index;
				Debug.Assert(m_Shapes[index].Parent == this);
			}
			m_Refresh = RectangleF.Empty;
		}

		// Enumerators should only enumerate the direct contents.  The page will use a complex enumerator wrapping these if it wants to navigate within containers
		public IEnumerable<Shape> Reverse => new Page.ReverseEnumerator(m_Shapes);

		IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() => m_Shapes.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => m_Shapes.GetEnumerator();

		public bool Accept(Shape shape)
		{
			if ((shape.Flags & GeneralFlags.NotWithinContainer) > 0)
				return false;
			if (!(shape is IShapeParent))
				return true;
			return !IsWithin((IShapeParent)shape);
		}

		// This only relevant for shapes which don't implement IShapeContainer
		public void DropNotContainer(Shape shape, Transaction objTransaction) => throw new InvalidOperationException();

#if DEBUG
		public void CheckZ(Shape shp)
		{
			Debug.Assert(shp == null || shp.GetClass() == Classes.Measure || shp.Z == m_Shapes.IndexOf(shp), "Shape Z-index wrong");
		}
#endif

		public virtual bool MoveWithin(List<Shape> shapes, PointF target, GrabMovement move, Transaction transaction)
		{
			// The standard container doesn't adjust the positions of shapes at all
			return false;
		}

		public bool AllowClick(PointF target)
		{
			if (!m_Bounds.Contains(target) && Overflow == OverflowModes.Clip)
				return false;
			return Container.AllowClick(target); // Continue up to my parents, in case we have containers within containers
		}

		public IShapeContainer AsParentContainer => this;

		#endregion

		#region Sockets
		// Unlike Group, this doesn't store the list of sockets, nor does it make the sockets refer to this shape.
		// Instead it simply returns the sockets from the internal shapes

		internal override List<Socket> GetSockets()
		{
			List<Socket> sockets = new List<Socket>();
			foreach (Shape shape in m_Shapes)
			{
				sockets.AddRange(shape.GetSockets());
			}
			return sockets;
		}

		#endregion

		#region Graphics
		// This isn't really used by this class, but it respects the Clip setting in the graphics (and which could actually be used in this class eventually)
		public enum OverflowModes : byte
		{
			/// <summary>Content overflows bounding box without limit</summary>
			Overflow = 0,
			/// <summary>Shapes which don't fit are hidden offscreen.  Only complete shapes will be displayed</summary>
			Hide,
			/// <summary>Shapes are positioned without limit, but the graphics are clipped the bounds of this</summary>
			Clip
		}
		/// <summary>In V2 this was only used by Flow.  In v3 it is now stored by the data of this class</summary>
		public OverflowModes Overflow = OverflowModes.Overflow;
		/// <summary>True is a green dotted border is shown when dragging onto this shape</summary>
		/// <remarks>Always true for this base class at the moment.  Can be edited for Flow</remarks>
		public bool ShowHighlight = true;

		// Overrides which propagate to the contained shapes:
		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			base.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			//if (Overflow == OverflowModes.Clip)
			//{
			//	if (LineLogic == LineLogics.Custom)
			//		gr.IntersectClip(Bounds);
			//	else
			//		gr.IntersectClip(EnsurePath(false));
			//}
			foreach (Shape shp in (reverseRenderOrder ? (m_Shapes as IEnumerable<Shape>).Reverse() : m_Shapes)) // must cast to Enumerable to avoid the List.Reverse which edits the actual list
			{
				shp.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			}
			//if (Overflow == OverflowModes.Clip)
			//	gr.RestoreClip();
		}

		/// <summary>Draws a highlighter indicating that the shape be moved/created will be inside this container</summary>
		/// <remarks>DrawHighlight, which is used for selected shapes also highlights the contents.  This draws the same, but only this one shape</remarks>
		public void DrawHover(Canvas gr, float scale, float coordScale)
		{
			// Adapted from base.DrawHighlight
			if (!ShowHighlight)
				return;
			DrawResources resources = new DrawResources(gr, scale, coordScale);
			//PrepareHighlight(objResources) ' adapted to:
			float width = LineStyle.Width + GUIUtilities.HIGHLIGHTEXTRAWIDTH; // using generic UI scaling, ignoring graphics, but this should only be used in regular UI, and not printed
			if (WireFrame)
				width = 2 * Geometry.INCH / (resources.Graphics.DpiX * resources.Scale);
			resources.MainPen = resources.Graphics.CreateStroke(Color.Green, width);
			resources.MainPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round; // otherwise points can stick out for miles, busting the refreshbounds
			resources.MainPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
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

		/// <summary>this both highlight the contained shapes and this shape itself</summary>
		internal override void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			base.DrawHighlight(gr, scale, coordScale, singleElement);
			//if (Overflow == OverflowModes.Clip)
			//{
			//	if (LineLogic == LineLogics.Custom)
			//		gr.IntersectClip(Bounds);
			//	else
			//		gr.IntersectClip(EnsurePath(false));
			//}
			foreach (Shape shp in m_Shapes)
			{
				shp.DrawHighlight(gr, scale, coordScale, singleElement);
			}
			//if (Overflow == OverflowModes.Clip)
			//	gr.RestoreClip();
		}

		// Methods which draw the container itself:
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.MainBrush != null || resources.MainPen != null)
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

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			if (!withShadow && !m_Refresh.IsEmpty)
				return m_Refresh;
			RectangleF refreshBounds = base.RefreshBoundsFromBounds(withShadow);
			foreach (Shape shape in m_Shapes)
			{
				Geometry.Extend(ref refreshBounds, shape.RefreshBounds(withShadow));
			}
			if (!withShadow)
				m_Refresh = refreshBounds;
			return refreshBounds;
		}

		internal override bool PerformLinkedChanges(Transaction transaction, frmMain editor)
		{
			// Need to check if it is one of the contents which has been edited; this might require updating the refresh bounds
			bool changed = base.PerformLinkedChanges(transaction, editor);
			foreach (Change change in transaction)
			{
				if (!change.IsDelete && change.Previous is Shape)
				{
					if (((Shape)change.Previous).IsWithin(this))
						m_Refresh = RectangleF.Empty; // This does not count as changing this shape
				}
			}
			return changed;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// Copied from BoundsDefines
			PointF[] points = m_Bounds.GetPoints();
			PointF start = points[target.ShapeIndex % points.Length];
			PointF end = points[(target.ShapeIndex + 1) % points.Length]; // mod makes sure last line loops back to (0), rather than trying to access (4)
			DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, start, end);
		}

		#endregion

	}

	/// <summary>A version of Container which also automatically positions the contained elements in 1 or more lines/columns.
	/// Not currently used in SAW</summary>
	public class Flow : Container, IAutoSize
	{

		// A container where the contents are automatically laid out in a line (or several lines)
		// Moving the edges of this never stretches the contents; however the contents will be automatically positioned

		public enum Directions : byte
		{
			Right = 0, // These ones only work on a single line
			Left,
			RightThenDown,
			LeftThenDown,
			RightThenUp,
			LeftThenUp,
			Up,
			Down,
			UpThenRight,
			DownThenRight,
			UpThenLeft,
			DownThenLeft
		}
		public const byte LASTHORIZONTALDIRECTION = (byte)Directions.LeftThenUp;
		public Directions Direction = Directions.RightThenDown;
		public StringAlignment Alignment = StringAlignment.Center; // Near is the low side of the transverse direction
		public bool DragWithin = true;
		/// <summary>Padding within the edge of the container</summary>
		public float Padding = 0.5F;
		/// <summary>Space to leave between shapes. This is not applied before the first shape or after the last shape.  Can be negative?</summary>
		public float ShapeSeparation;

		#region Information
		public override Shapes ShapeCode => Shapes.Flow;

		public override AllowedActions Allows => AllowedActions.PermittedArea | AllowedActions.TransformMove | AllowedActions.TransformScale | AllowedActions.TransformLinearStretch;

		#endregion

		#region Editing settings

		internal override string DoubleClickText() => Strings.Item("Flow_Settings");

		internal override void DoDoubleClick(EditableView view, ClickPosition.Sources source)
		{
			Transaction transaction = new Transaction();
			transaction.Edit(this);
			if (frmFlow.Display(this) == DialogResult.OK)
			{
				Status = StatusValues.Moved;
				FinishedModifyingContents(transaction);
				Globals.Root.StoreNewTransaction(transaction, true);
			}
		}

		#endregion

		#region Dragging and GrabSpots etc
		public override bool MoveWithin(List<Shape> shapes, PointF target, GrabMovement move, Transaction transaction)
		{
			// This needs to update the Z-order of the shapes.  The actual positioning will be updated by FinishedModifyingContents
			// Returns true if any changes have been made
			if (!DragWithin)
				return false;
			int insert = FindInsertionIndex(target);
			move.CustomPositioning = true; // Prevents the normal transform being applied.  The shapes will be positioned by this
			bool contiguous = true; // Whether the shapes of a continuous range of Z indices (if not there will always need to be moved)
			shapes.Sort(new Page.ZSort());
			for (int index = 0; index <= shapes.Count - 2; index++)
			{
				if (shapes[index].Z != shapes[index + 1].Z - 1)
					contiguous = false;
			}
			if (contiguous && insert >= shapes[0].Z && insert <= shapes.Last().Z + 1) //  the +1 is correct.  IntInsert is the item to insert before
				return false;
			// shapes already at the correct index
			// Shapes must be moved.  First remove them from the current list
			// Also need to update the insertion point is it is higher than the shapes (and if not continuous may be higher than some of them)
			foreach (Shape shape in shapes)
			{
				Debug.Assert(shape.Parent == this);
				m_Shapes.RemoveAt(shape.Z);
				if (insert > shape.Z)
					insert -= 1;
			}
			m_Shapes.InsertRange(insert, shapes);
			FinishedModifyingContents(transaction, move);
			return true;
		}

		private int FindInsertionIndex(PointF target)
		{
			// returns the index at which a shape dropped at the given point would be inserted
			float lengthways = Lengthways(target);
			float transverse = Transverse(target);
			// Find which line this falls on.  If before the first line or after the last line it will be "on" those lines
			int line = 0;
			while (line < m_Lines.Count - 1 && transverse > m_Lines[line].Width)
			{
				transverse -= m_Lines[line].Width + ShapeSeparation;
				line += 1;
			}
			int index = m_Lines[line].FirstIndex;
			int maximum = m_Shapes.Count; // Maximum insertion position - first index of the end of this line
			if (line < m_Lines.Count - 1)
				maximum = m_Lines[line + 1].FirstIndex;
			while (index < maximum && lengthways > Length(m_Shapes[index]) / 2)
			{
				// note the /2 which means it if we are beyond the midpoint of the shape we move to the next position (unlike lines)
				lengthways -= Length(m_Shapes[index]) + ShapeSeparation;
				index += 1;
			}
			return index; // note that this can be off the end of the list
		}

		protected override bool GrabTransformsContents(GrabMovement move) // This one ignores the control key; the bounding GrabSpots never affect the contents
		{
			return move.GrabType == GrabTypes.Move;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			base.DoGrabMove(move);
			FinishedModifyingContents(move.Transaction);
		}

		#endregion

		#region Arranging content
		// Some information about the lines into which the shapes are laid out is kept.  This is not stored in the file, but will be updated
		// because FinishedModifyingContents is called during Load
		private struct Line
		{
			public int FirstIndex;
			public float Width;
			public float Length;
			//Public Hidden As Boolean
		}
		private readonly List<Line> m_Lines = new List<Line>();

		public override void FinishedModifyingContents(Transaction transaction, GrabMovement move = null)
		{
			// transaction does not need to be provided
			DoPositioning(transaction, move, null);
		}

		/// <summary>Functionality for FinishedModifyingContents extracted, also allowing an extra function to run after the initial placement</summary>
		/// <param name="move"></param>
		/// <param name="fnOverallPosition">Specifies an optional function which returns a vector translation to apply to all of the calculated sizes.
		/// (Used by AutoSize which may need to move this object at the end, and doesn't want to have to translate all of the shapes a second time)</param>
		/// <param name="transaction">Optional - can be null</param>
		private void DoPositioning(Transaction transaction, GrabMovement move, GetOverallTranslation fnOverallPosition)
		{
			m_Lines.Clear();
			float lengthways = 0; // Current position at which the next shape will be placed (give or take).  These are within the padding
			float transverse = 0;
			float lineWidth = 0; // width of widest shape so far on the current line
			bool changed = false; // is set to true if any shapes are actually moved
			bool offScreen = false; // true once we have run out of space and other shapes should not be displayed (only if overflow is hide)
			float availableLength = Math.Max(Length(this) - Padding * 2, 2); // amount of space available on each line
			int lineFirst = 0; // first shape on current line
			if (IsSingleLine && Overflow != OverflowModes.Hide)
				availableLength = float.MaxValue; // Will ensure everything stays on one line
												  // First pass work out where the shapes go (if not immediately applied to the shapes, because transforming shapes is relatively slow,
												  // this may need some adjustment at the end of lines)
			PointF[] positions = new PointF[m_Shapes.Count - 1 + 1];
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				Utilities.ErrorAssert(m_Shapes[index].Parent == this);
				Shape shape = m_Shapes[index];
				shape.Z = index;
				if (lengthways > 0 && lengthways + Length(shape) > availableLength + Geometry.NEGLIGIBLE)
				{
					// First condition avoids this triggering at the beginning of a line - we always fit one item even if it is too big
					Utilities.ErrorAssert(index > lineFirst);
					transverse += lineWidth + ShapeSeparation;
					if (Overflow == OverflowModes.Hide)
					{
						if (IsSingleLine)
							offScreen = true;
						if (transverse > Width(this) - Padding * 2 + Geometry.NEGLIGIBLE && lineFirst > 0)
							offScreen = true; // The previous line does not fit completely and should be hidden (But not applied to first line!)
					}
					FinishLine(lineFirst, index - 1, positions, lineWidth, lengthways - ShapeSeparation, offScreen);
					lineWidth = 0;
					lengthways = 0;
					lineFirst = index;
				}
				positions[index] = Position(lengthways, transverse, shape);
				if (offScreen)
					positions[index].X -= 10000;
				lineWidth = Math.Max(lineWidth, Width(shape));
				lengthways += Length(shape) + ShapeSeparation;
			}
			// Complete the last line: check it fits, and process alignment
			if (Overflow == OverflowModes.Hide && transverse + lineWidth > Width(this) - Padding * 2 + Geometry.NEGLIGIBLE && lineFirst > 0)
				offScreen = true; // The previous line does not fit completely and should be hidden (But not applied to first line!)
			FinishLine(lineFirst, m_Shapes.Count - 1, positions, lineWidth, lengthways - ShapeSeparation, offScreen);

			SizeF adjust = SizeF.Empty;
			if (fnOverallPosition != null)
				adjust = fnOverallPosition.Invoke();
			TransformMove transform = new TransformMove(0, 0);
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				Shape shape = m_Shapes[index];
				SizeF offset = (positions[index] + adjust).Subtract(shape.Bounds.Location); // How far this shape needs to be moved
				if (!offset.IsEmpty)
				{
					transaction?.Edit(shape);
					transform.SetDelta(offset); // This resets the transformation, and does not accumulate the changes
					shape.ApplyTransformation(transform);
					changed = true;
				}
			}
			if (changed)
				Parent?.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded); // Otherwise other moved shapes might not be repainted.  Second condition is needed when loading
			if (move != null)
				move.CustomPositioning = true;
			m_Refresh = RectangleF.Empty;
		}

		private delegate SizeF GetOverallTranslation();

		/// <summary>Part of FinishedModifyingContents.  Called at the end of each line to sort out the alignment</summary>
		private void FinishLine(int first, int last, PointF[] positions, float lineWidth, float length, bool hidden)
		{
			// This doesn't need to know sngTransverse etc from the above function, because it can work by just adjusting the shape positions
			if (IsSingleLine)
				lineWidth = Math.Max(lineWidth, Width(this)); // improves centring on single line versions
			for (int index = first; index <= last; index++)
			{
				if (hidden)
					positions[index].X -= 10000; // this might have been done in the original function, but isn't for the line which first overflows
				else
				{
					switch (Direction)
					{
						case Directions.Right:
						case Directions.RightThenDown:
						case Directions.Left:
						case Directions.LeftThenDown:
							switch (Alignment)
							{
								case StringAlignment.Center:
									positions[index].Y += (lineWidth - m_Shapes[index].Bounds.Height) / 2;
									break;
								case StringAlignment.Far:
									positions[index].Y += lineWidth - m_Shapes[index].Bounds.Height;
									break;
							}
							break;
						case Directions.RightThenUp:
						case Directions.LeftThenUp:
							switch (Alignment)
							{
								case StringAlignment.Center:
									positions[index].Y -= (lineWidth - m_Shapes[index].Bounds.Height) / 2;
									break;
								case StringAlignment.Near:
									positions[index].Y -= lineWidth - m_Shapes[index].Bounds.Height;
									break;
							}
							break;
						case Directions.Up:
						case Directions.UpThenRight:
						case Directions.Down:
						case Directions.DownThenRight:
							switch (Alignment)
							{
								case StringAlignment.Center:
									positions[index].X += (lineWidth - m_Shapes[index].Bounds.Width) / 2;
									break;
								case StringAlignment.Far:
									positions[index].X += lineWidth - m_Shapes[index].Bounds.Width;
									break;
							}
							break;
						case Directions.UpThenLeft:
						case Directions.DownThenLeft:
							switch (Alignment)
							{
								case StringAlignment.Center:
									positions[index].X -= (lineWidth - m_Shapes[index].Bounds.Width) / 2;
									break;
								case StringAlignment.Near:
									positions[index].X -= lineWidth - m_Shapes[index].Bounds.Width;
									break;
							}
							break;
						default:
							throw new InvalidOperationException();
					}
				}
			}
			m_Lines.Add(new Line { FirstIndex = first, Width = lineWidth, Length = length }); //, .Hidden = bolHidden
		}

		/// <summary>Returns dimension of shape bounds in the flow direction</summary>
		private float Length(Shape shp)
		{
			// Note that this and Width below also work for this container (so far!)
			return Length(shp.Bounds.Size);
		}

		/// <summary>Returns dimension of SizeF in the flow direction</summary>
		private float Length(SizeF size)
		{
			if ((byte)Direction <= LASTHORIZONTALDIRECTION)
				return size.Width;
			return size.Height;
		}

		private float Width(Shape shape) => Width(shape.Bounds.Size);

		/// <summary>Returns dimension of SizeF orthogonal to the flow direction</summary>
		private float Width(SizeF size)
		{
			if ((byte)Direction <= LASTHORIZONTALDIRECTION)
				return size.Height;
			return size.Width;
		}

		/// <summary>Returns the lengthways coordinate WITHIN the container of the given point.  Sense may be inverted for 'backward' directions</summary>
		private float Lengthways(PointF point)
		{
			switch (Direction)
			{
				case Directions.Right:
				case Directions.RightThenDown:
				case Directions.RightThenUp:
					return point.X - m_Bounds.X - Padding;
				case Directions.Left:
				case Directions.LeftThenDown:
				case Directions.LeftThenUp:
					return m_Bounds.Right - point.X - Padding;
				case Directions.Down:
				case Directions.DownThenLeft:
				case Directions.DownThenRight:
					return point.Y - m_Bounds.Y - Padding;
				case Directions.Up:
				case Directions.UpThenLeft:
				case Directions.UpThenRight:
					return m_Bounds.Bottom - point.Y - Padding;
				default:
					throw new InvalidOperationException();
			}
		}

		/// <summary>Returns the transverse coordinate WITHIN the container of the given point</summary>
		private float Transverse(PointF point)
		{
			if ((byte)Direction <= LASTHORIZONTALDIRECTION)
				return point.Y - m_Bounds.Y - Padding;
			return point.X - m_Bounds.X - Padding;
		}

		/// <summary>Part of FinishedModifyingContents</summary>
		/// <remarks>Input positions are in the filling direction.  Which corner of the shape is specified depends on the direction</remarks>
		private PointF Position(float lengthways, float transverse, Shape shape)
		{
			switch (Direction)
			{
				case Directions.Right:
				case Directions.RightThenDown:
					return new PointF(m_Bounds.X + lengthways + Padding, m_Bounds.Y + transverse + Padding);
				case Directions.Left:
				case Directions.LeftThenDown:
					return new PointF(m_Bounds.Right - lengthways - Padding - shape.Bounds.Width, m_Bounds.Y + transverse + Padding);
				case Directions.RightThenUp:
					return new PointF(m_Bounds.X + lengthways + Padding, m_Bounds.Bottom - transverse - Padding - shape.Bounds.Height);
				case Directions.LeftThenUp:
					return new PointF(m_Bounds.Right - lengthways - Padding - shape.Bounds.Width, m_Bounds.Bottom - transverse - Padding - shape.Bounds.Height);
				case Directions.Down:
				case Directions.DownThenRight:
					return new PointF(m_Bounds.X + transverse + Padding, m_Bounds.Y + lengthways + Padding);
				case Directions.Up:
				case Directions.UpThenRight:
					return new PointF(m_Bounds.X + transverse + Padding, m_Bounds.Bottom - lengthways - Padding - shape.Bounds.Height);
				case Directions.DownThenLeft:
					return new PointF(m_Bounds.Right - transverse - Padding - shape.Bounds.Width, m_Bounds.Y + lengthways + Padding);
				case Directions.UpThenLeft:
					return new PointF(m_Bounds.Right - transverse - Padding - shape.Bounds.Width, m_Bounds.Bottom - lengthways - Padding - shape.Bounds.Height);
				default:
					throw new InvalidOperationException("Unexpected Flow direction: " + Direction);
			}
		}

		private bool IsSingleLine
		{
			get
			{
				switch (Direction)
				{
					case Directions.Right:
					case Directions.Left:
					case Directions.Up:
					case Directions.Down:
						return true;
					default:
						return false;
				}
			}
		}

		public bool Horizontal => (byte)Direction <= LASTHORIZONTALDIRECTION;

		internal override bool PerformLinkedChanges(Transaction transaction, frmMain editor)
		{
			bool changed = base.PerformLinkedChanges(transaction, editor);
			// Base class has already checked if any of the contents are included, and responds by clearing m_rctRefresh
			// (Doesn't matter that base class ignores deletes, because they will already have triggered an update of this)
			if ((changed || m_Refresh.IsEmpty) && !transaction.Contains(this))
			{
				FinishedModifyingContents(transaction);
				changed = true;
			}
			return changed;
		}

		#endregion

		#region Data
		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.WriteByte((byte)Direction);
			writer.WriteByte((byte)Alignment);
			if (writer.Version < 109)// now stored by base class instead
				writer.WriteByte((byte)Overflow);
			writer.Write(DragWithin);
			writer.Write(ShowHighlight);
			writer.Write(Padding);
			writer.Write(ShapeSeparation);
		}

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			Direction = (Directions)reader.ReadByte();
			Alignment = (StringAlignment)reader.ReadByte();
			if (reader.Version < 109) // now stored by base class instead
				Overflow = (OverflowModes)reader.ReadByte();
			DragWithin = reader.ReadBoolean();
			ShowHighlight = reader.ReadBoolean();
			Padding = reader.ReadSingle();
			ShapeSeparation = reader.ReadSingle();
			Debug.Assert(ShapeCode == Shapes.Flow); // if there are any derived types they should make this call:
			FinishedModifyingContents(null);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Flow flow = (Flow)other;
			if (depth >= CopyDepth.Undo)
			{
				Direction = flow.Direction;
				Alignment = flow.Alignment;
				DragWithin = flow.DragWithin;
				ShowHighlight = flow.ShowHighlight;
				Padding = flow.Padding;
				ShapeSeparation = flow.ShapeSeparation;
			}
		}

		#endregion

		public SizeF AutoSize(SizeF target)
		{
			if (target.IsEmpty)
				throw new ArgumentException();
			float length = Length(target); // requested length in the flow direction.  It is easier if this is specified, then we just set the transverse size based on # rows
			if (length == 0 && m_Shapes.Count > 0) // if 0 elements the other code will work (sets 10x10)
			{
				// if the number of rows is specified we have only a limited implementation.  This is an odd case.  In practice AutoSize is only used in palettes, which always specify
				// the horizontal (or both).  So this would be a vertical flow.  The assumption is that all elements are the same size.  We actually use the largest size, so the size will
				// be excessive if they vary, but at least everything is visible.
				SizeF largest = new SizeF(Math.Max(m_Shapes.Max(x => x.Bounds.Width), 1), Math.Max(m_Shapes.Max(x => x.Bounds.Height), 1)); // math.max needed to prevent div zero just in case only 0-size elements
				int lines = (int)Math.Max(Width(target) / Width(largest), 1); // number of lines which fit
				int perLine = (int)Math.Ceiling((float)m_Shapes.Count() / lines);
				// we use szTarget where specified because (a) we're supposed to and (b) if, eg, RTL in a palette using the actual size will tend to left align the buttons
				if (Horizontal)
					m_Bounds.Size = new SizeF(perLine * largest.Width, target.Height);
				else
					m_Bounds.Size = new SizeF(target.Width, perLine * largest.Height);
				m_Bounds.Y = -m_Bounds.Height;
				DoPositioning(null, null, null); // doesn't update size to fit actual objects - we stick with the one we've just calculated
			}
			else
			{
				m_Bounds.Size = target; // only one part of size actually applicable;  other is set to 'infinite'...
				if (Horizontal)
					m_Bounds.Height = 10000;
				else
					m_Bounds.Width = 10000;
				if (m_Shapes.Count == 0)
				{
					if (Horizontal)
						m_Bounds.Height = 10;
					else
						m_Bounds.Width = 10; // use a small default (0 not valid)
				}
				else
					DoPositioning(null, null, AutoSizeCalculationComplete);
				// AutoSizeCalculationComplete does the update of my size
			}
			return Bounds.Size;
		}

		public override void SetBounds(RectangleF bounds, Transaction transaction = null)
		{
			base.SetBounds(bounds, transaction);
			DoPositioning(transaction, null, null);
		}

		private SizeF AutoSizeCalculationComplete()
		{
			// is called by DoPositioning once the calculation is complete, and before updating the positions of the contents
			float transverse = m_Lines.Sum(x => x.Width); // required size in the other direction
			transverse += (m_Lines.Count - 1) * ShapeSeparation + Padding * 2;
			transverse = Math.Max((int)transverse, 10); // Max in particular in case this is empty; don't want to set 0
			float length = Math.Max(10, m_Lines.Max(x => x.Length)); // longest line
			SizeF adjustment; // how much this shape has been moved (the calculated positions of the contents need to be updated by this)
			if (Horizontal)
			{
				switch (Direction)
				{
					case Directions.Right:
					case Directions.RightThenDown:
						adjustment = new SizeF(0, -transverse - m_Bounds.Y);
						break;
					case Directions.RightThenUp:
						adjustment = new SizeF(0, -m_Bounds.Bottom);
						break;
					case Directions.LeftThenUp:
						adjustment = new SizeF(length - m_Bounds.Width, -m_Bounds.Bottom);
						break;
					default: // normal left...
						adjustment = new SizeF(length - m_Bounds.Width, -transverse - m_Bounds.Y);
						break;
				}
				m_Bounds.Height = transverse;
				// changed to use actual required width.  Works better with auto zoom in SizeFromSize in PaletteView
				Debug.Assert(m_Lines.Count > 0);
				m_Bounds.Width = m_Lines.Max(x => x.Length) + Padding * 2;
				m_Bounds.Y = -m_Bounds.Height;
			}
			else
			{
				switch (Direction)
				{
					case Directions.Up:
					case Directions.UpThenRight:
						adjustment = new SizeF(0, -m_Bounds.Bottom);
						break;
					case Directions.UpThenLeft:
						adjustment = new SizeF(transverse - m_Bounds.Width, -m_Bounds.Bottom);
						break;
					case Directions.DownThenLeft:
						adjustment = new SizeF(transverse - m_Bounds.Width, -length - m_Bounds.Y);
						break;
					default: // down right
						adjustment = new SizeF(0, -length - m_Bounds.Y);
						break;
				}
				m_Bounds.Width = transverse;
				m_Bounds.Height = length;
				m_Bounds.Y = -m_Bounds.Height;
			}
			return adjustment;
		}

	}
}
 