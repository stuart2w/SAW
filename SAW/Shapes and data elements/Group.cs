using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;

namespace SAW
{
	public class Group : Shape, IShapeParent
	{

		// a group of other shapes lumped together
		// m_Bounds forms part of the data for this
		// on the page the shapes do keep their own internal coordinates; the group is capable of repositioning them if needed

		public Group()
		{
			// used when loading
		}

		public Group(List<Shape> colShapes)
		{
			Contents.AddRange(colShapes);
			m_Bounds = RectangleF.Empty;
			foreach (Shape shape in Contents)
			{
				shape.Parent = this;
				shape.NotifyEnvironmentChanged(EnvironmentChanges.ParentReassigned);
			}
			ResetZ();
		}

		#region Information

		public override Shapes ShapeCode => Shapes.Group;

		public override AllowedActions Allows
		{
			get
			{
				AllowedActions result = AllowedActions.All & ~AllowedActions.Typing;
				foreach (Shape shape in Contents)
				{
					result &= shape.Allows;
				}
				return result;
			}
		}

		public List<Shape> Contents { get; private set; } = new List<Shape>();
		public override LabelModes LabelMode => LabelModes.NotSupported;
		public override GeneralFlags Flags => base.Flags | GeneralFlags.NoColourOnTransformCopy;

		#endregion

		#region Verbs - disallowed
		// none of the usual verbs are applicable to groups.  The group is created in a single step from a collection of selected shapes

		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult Choose(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult Complete(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult CompleteRetrospective() => VerbResult.Unexpected;
		public override VerbResult Float(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult Start(EditableView.ClickPosition position) => VerbResult.Unexpected;

		#endregion

		#region Load/Save/copy from
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Group otherGroup = (Group)other;
			if (depth == CopyDepth.Transform && Contents.Count == otherGroup.Contents.Count)
			{
				// excludes first time -when we must clone the shapes to get new copies to store the differing coords
				for (int index = 0; index <= Contents.Count - 1; index++)
				{
					Contents[index].CopyFrom(otherGroup.Contents[index], depth, mapID);
				}
			}
			else
			{
				// undo requires all shapes are copied, because they could later be edited by styling changes
				Contents.Clear();
				if (mapID == null)
					mapID = Mapping.Ignore;
				foreach (Shape shape in otherGroup.Contents)
				{
					Shape create = (Shape)shape.Clone(mapID);
					create.Parent = this;
					Contents.Add(create);
				}
			}
			m_Sockets = null;
			m_Bounds = RectangleF.Empty;
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			Contents = reader.ReadShapeList();
			foreach (Shape shape in Contents)
			{
				shape.Parent = this;
			}
			ResetZ();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.WriteDatumList(Contents);
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			foreach (Shape shape in Contents)
			{
				shape.UpdateReferencesObjectsCreated(document, reader);
			}
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			foreach (Shape shape in Contents)
			{
				shape.UpdateReferencesIDsChanged(mapID, document);
			}
		}

		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			foreach (Shape shape in Contents)
			{
				shape.Iterate(fn);
			}
		}

		protected override bool IdenticalToShape(Shape other)
		{
			if (!base.IdenticalToShape(other))
				return false;
			Group otherGroup = (Group)other;
			if (otherGroup.Contents.Count != Contents.Count) return false;
			for (int i = 0; i < Contents.Count; i++)
			{
				if (!Contents[i].IdenticalTo(otherGroup.Contents[i]))
					return false;
			}
			return true;
		}

		#endregion

		#region Coords, Grabspots, Targets, cut
		protected override RectangleF CalculateBounds()
		{
			RectangleF bounds = RectangleF.Empty;
			foreach (Shape shp in Contents)
			{
				Geometry.Extend(ref bounds, shp.Bounds);
			}
			return bounds;
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => true;  // clicking anywhere within the bounds is sufficient

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			foreach (Shape shp in Contents)
			{
				shp.ApplyTransformation(transformation);
			}
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			AllowedActions allows = Allows; // this is potentially somewhat slow to calculate
			if ((allows & AllowedActions.TransformMove) == 0)
				return null;
			List<GrabSpot> list = new List<GrabSpot>();
			if ((allows & AllowedActions.TransformRotate) > 0)
				base.AddStandardRotationGrabSpot(list);
			if ((allows & AllowedActions.TransformScale) > 0)
				base.AddBoundingGrabSpots(list, scale);
			return list;
		}

		public const float MAXIMUMSNAPROTATION = Geometry.ANGLE45 / 2; // most to autorotate a moving shape

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> list = new List<Target>();
			EnsureSocketList();
			foreach (UserSocket socket in m_Sockets)
			{
				if ((socket.Options & UserSocket.OptionsEnum.ShapeSnap) > 0)
				{
					// if they aren't compatible we still generate the shape, but mark it as failed
					Target newTarget = new Target(this, socket.Centre, Target.Types.Vertex, floating, Target.Priorities.Vertex);
					if (!floating.ClassCompatible(socket) || !floating.GenderCompatible(socket))
						newTarget.Mismatch = true;
					else
					{
						float angle = floating.RotationRequired(socket);
						if (Math.Abs(angle) > MAXIMUMSNAPROTATION)
							newTarget = null; // angle so far out it is not a match at all
						else if (angle != 0)
							// can autorotate
							newTarget.RotationRequired = angle;
					}
					if (newTarget != null)
						list.Add(newTarget);
				}
			}
			return list;
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			EnsureSocketList();
			if (m_Sockets.Count == 0)
				return null;
			return m_Sockets;
		}

		#endregion

		#region Sockets
		private List<UserSocket> m_Sockets; // this is calculated as needed, but then remembered
											// if any edit might change this just set it to nothing again (nothing means not checked; empty list means no UserSocket present)
											// socket indices are just the index into this array

		private void EnsureSocketList()
		{
			if (m_Sockets != null)
				return;
			m_Sockets = new List<UserSocket>();
			foreach (Shape shape in Contents)
			{
				if (shape.ShapeCode == Shapes.UserSocket && (((UserSocket)shape).Options & UserSocket.OptionsEnum.ShapeSnap) > 0)
					m_Sockets.Add((UserSocket)shape);
				else if (shape.ShapeCode == Shapes.Group)
				{
					((Group)shape).EnsureSocketList();
					foreach (UserSocket socket in ((Group)shape).m_Sockets)
					{
						m_Sockets.Add(socket);
					}
				}
			}
		}

		internal override List<Socket> GetSockets()
		{
			EnsureSocketList();
			List<Socket> list = new List<Socket>();
			if (m_Sockets.Count > 1)
				list.Add(new Socket(this, Socket.AUTOMATIC, Middle()));
			// only include the automatic choice socket if there are at least two
			// real sockets to choose from
			for (int index = 0; index <= m_Sockets.Count - 1; index++)
			{
				list.Add(new Socket(this, index, m_Sockets[index].Centre));
			}
			return list;
		}

		internal override PointF SocketPosition(int index)
		{
			EnsureSocketList();
			if (index < 0 || index >= m_Sockets.Count)
				return Middle();
			return m_Sockets[index].Centre;
		}

		internal override SizeF SocketExitVector(int index)
		{
			EnsureSocketList();
			if (index < 0 || index >= m_Sockets.Count)
			{
				Debug.Fail("Invalid socket index");
				return new SizeF(1, 0);
			}
			SizeF exit = m_Sockets[index].ExitVector;
			// we cannot return SizeF.Empty; if the user has not specified then exit vector we will deduce it based on the group
			if (exit.IsEmpty)
			{
				exit = Middle().VectorTo(m_Sockets[index].Centre);
				// and just in case the user has managed to place the socket right in the middle
				if (exit.Length() < Geometry.NEGLIGIBLE)
					exit = new SizeF(1, 0);
			}
			return exit;
		}
		#endregion

		#region Graphics

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			foreach (Shape shp in Contents)
			{
				shp.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			}
		}

		internal override void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			// draws the highlight of the entire group; the individual shapes are not marked as highlighted
			// we don't bother using the PrepareHighlight etc functions - since we have no functionality to share between this and standard drawing there is no point
			using (Stroke pnHighlight = gr.CreateStroke(CurrentHighlightColour, GUIUtilities.HIGHLIGHTEXTRAWIDTH))
			{
				gr.Rectangle(m_Bounds, pnHighlight);
			}

			foreach (Shape shp in Contents)
			{
				shp.DrawHighlight(gr, scale, coordScale, singleElement);
			}
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			// this is only called during DrawHighlight
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (Contents != null)
				{
					foreach (Shape shp in Contents)
					{
						shp.Dispose();
					}
				}
			}
		}

		#region IShapeParent and similar
		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			base.AddToFlatList(list, purpose);
			if (purpose == FlatListPurpose.ExposedShapes)
				return; // shapes inside this are not exposed
			foreach (Shape shape in Contents)
			{
				shape.AddToFlatList(list, purpose);
			}
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			// just passes along the notification to its container
			affected = affected & ~( ChangeAffects.GrabSpots);
			if (affected == 0 || Parent == null)
				return; // Parent can be null - if this is floating to be placed
			Parent.NotifyIndirectChange(shape, affected);
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			// just passes along the notification to its container
			affected = affected & ~(ChangeAffects.GrabSpots);
			if (affected == 0 || Parent == null)
				return;
			Parent.NotifyIndirectChange(shape, affected, area);
		}

		internal override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			base.NotifyEnvironmentChanged(change);
			foreach (Shape shape in Contents)
			{
				shape.NotifyEnvironmentChanged(change);
			}
		}

		private void ResetZ()
		{
			// can be used if the Z-order is changed to reset from first principles
			for (int index = 0; index <= Contents.Count - 1; index++)
			{
				Contents[index].Z = index;
				Debug.Assert(Contents[index].Parent == this);
			}
		}

		#endregion

	}
}
