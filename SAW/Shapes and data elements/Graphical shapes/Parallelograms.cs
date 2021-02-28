using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

// this file includes the three versions: square, rectangle, parallelogram

namespace SAW
{
	public class Parallelogram : Sequential
	{

		#region Information

		public override Shapes ShapeCode => Shapes.Parallelogram;
		protected override int FixedVerticesLength() => 4;
		public override PointF Centre => base.CalculateCentreFromPoints();
		internal override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Parallelogram", false);

		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// let the base class take care of positioning the first line
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// need to calculate the coordinates of the fourth point.  We can add the 1>2 vector onto point 0
			Debug.Assert(Vertices.Count == 4);
			PointF pt = position.Snapped;
			if (pt.Equals(Vertices[2]))
				return VerbResult.Unchanged;
			Vertices[2] = pt;
			SizeF szVector = Vertices[1].VectorTo(Vertices[2]);
			Vertices[3] = PointF.Add(Vertices[0], szVector);
			m_Bounds = CalculateBounds();
			m_Acceptable = !VerticesFormLine(0);
			DiscardPath();
			return VerbResult.Continuing;
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddGrabSpotsForAllVertices(list);
			return list;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			// although any point can be moved, we cannot use the base class which assumes they are completely independent
			if (move.GrabType == GrabTypes.SingleVertex)
			{
				// the indices of the other points:
				int index = move.ShapeIndex;
				int previous = (index + 3) % 4;
				int next = (index + 1) % 4;
				int opposite = (index + 2) % 4;
				// check it is not degenerate:
				if (Geometry.PointApproxOnLine(Vertices[previous], Vertices[next], move.Current.Snapped))
					return;
				Vertices[index] = move.Current.Snapped;
				Vertices[opposite] = Vertices[previous] + Vertices[index].VectorTo(Vertices[next]);
				m_Bounds = CalculateBounds();
				DiscardPath();
			}
			else
				base.DoGrabMove(move);
		}

		public override string StatusInformation(bool ongoing)
		{
			if (ongoing)
				return base.StatusInformation(true);
			return Strings.Item("Info_Edges") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[0], Vertices[1])) + " & " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[2], Vertices[1]));
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			bool changed = base.TidyVertices(mode, page, 2);
			if (changed) Vertices[3] = Vertices[0] + Vertices[1].VectorTo(Vertices[2]);
			DiscardPath();
			return changed;
		}

		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();
	}

	public class RectangleShape : Parallelogram
	{

		#region Constructors
		public RectangleShape()
		{ }

		public RectangleShape(RectangleF rct)
		{
			// used by the splatter to create a rectangle bordering the page
			Vertices.Add(new PointF(rct.X, rct.Y));
			Vertices.Add(new PointF(rct.Right, rct.Y));
			Vertices.Add(new PointF(rct.Right, rct.Bottom));
			Vertices.Add(new PointF(rct.X, rct.Bottom));
			m_DefinedVertices = 4;
			LineStyle.SetDefaults(); // otherwise error reports cannot be loaded
			FillStyle.SetDefaults();
			FillStyle.Colour = Color.Transparent;
			base.Status = StatusValues.Complete;
		}
		#endregion

		#region Information
		public override Shapes ShapeCode => Shapes.Rectangle;

		internal override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Rectangle", false);

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices >= 2)
			{
				// because the direction of the second line is fixed, it only makes sense to snap this to a grid point
				// if the first line was lined up with the grid.  Otherwise the second line is going diagonally and will probably never hit any grid points
				// we do allow shape snapping, because snapping to a target point will create a rectangle which passes through that target
				if (requested == SnapModes.Shape || Geometry.LineApproxPerpendicular(Vertices[0], Vertices[1]))
					return requested;
				return SnapModes.Off;
			}
			return base.SnapNext(requested);
		}

		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// let the base class take care of positioning the first line
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// need to calculate the coordinates of the fourth point.  We can add the 1>2 vector onto point 0
			Debug.Assert(Vertices.Count == 4);
			PointF newPoint = Geometry.PerpendicularPoint(Vertices[0], Vertices[1], position.Snapped);
			if (newPoint.Equals(Vertices[2]))
				return VerbResult.Unchanged;
			if (newPoint.ApproxEqual(Vertices[1]))
				return VerbResult.Rejected;
			Vertices[2] = newPoint;
			SizeF vector = Vertices[1].VectorTo(Vertices[2]);
			Vertices[3] = PointF.Add(Vertices[0], vector);
			m_Bounds = CalculateBounds();
			m_Acceptable = !VerticesFormLine(0);
			DiscardPath();
			return VerbResult.Continuing;
		}

		#region Coordinates

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddGrabSpotsForAllVertices(list, new Prompt(ShapeVerbs.Choose, "CornerResize"));
			for (int index = 0; index <= 3; index++)
			{
				PointF mid = Geometry.MidPoint(Vertices[index], Vertices[(index + 1) % 4]);
				GrabSpot grabSpot = new GrabSpot(this, GrabTypes.Radius, mid, Middle()) { ShapeIndex = index, Prompts = new[] { new Prompt(ShapeVerbs.Choose, "MoveVertex", "MoveVertex") } };
				list.Add(grabSpot);
			}
			return list;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					{
						int index = move.ShapeIndex;
						int previous = (index + 3) % 4;
						int next = (index + 1) % 4;
						int opposite = (index + 2) % 4;
						// keep orientation unchanged.  Opposite point is only invariant.  Calc vectors from this to the 2 semi-moving points
						SizeF previousVector = Vertices[opposite].VectorTo(Vertices[previous]);
						SizeF nextVector = Vertices[opposite].VectorTo(Vertices[next]);
						SizeF movingVector = Vertices[opposite].VectorTo(move.Current.Snapped);
						previousVector = Geometry.ProjectionVector(movingVector, previousVector);
						nextVector = Geometry.ProjectionVector(movingVector, nextVector);
						if (previousVector.Length() < Geometry.THINLINE || nextVector.Length() < Geometry.THINLINE)
							return; // degenerate
						Vertices[next] = Vertices[opposite] + nextVector;
						Vertices[previous] = Vertices[opposite] + previousVector;
						Vertices[index] = Vertices[previous] + nextVector;
						m_Bounds = CalculateBounds();
					}
					break;
				case GrabTypes.Radius:
					{
						int vertex = move.ShapeIndex; // vertex at start of line
						int end = (move.ShapeIndex + 1) % 4; // and end of line
						int previous = (move.ShapeIndex + 3) % 4; // vertex at start of previous line leading to intVertex
						int opposite = (move.ShapeIndex + 2) % 4; // and at opposite corner; end of line leading from intEnd
						PointF newPoint = Geometry.ClosestPointOnLine(Vertices[previous], Vertices[vertex], move.Current.Snapped);
						SizeF newVector = Vertices[previous].VectorTo(newPoint); // new length of the sides which are being stretched
						if (newVector.Length() < Geometry.THINLINE)
							return;
						Vertices[vertex] = newPoint;
						Vertices[end] = Vertices[opposite] + newVector;
						m_Bounds = CalculateBounds();
					}
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
			DiscardPath();
		}

		protected override PointF[] RectangleLabelGetPoints() => Vertices.ToArray();

		protected override LabelPositions LabelPosition => LabelPositions.RotatedRectangle;

		public override bool Tidy(SnapModes mode, Page page)
		{
			// the base class tidies the points independently and could end up with a parallelogram, this ensures that it remains a rectangle
			float transverse = Geometry.DistanceBetween(Vertices[1], Vertices[2]); // the transverse distance
			bool changed = base.Tidy(mode, page);
			if (mode == SnapModes.Grid)
			{
				float newValue = page.Paper.SnapScalar(transverse, 0);
				if (newValue > Geometry.NEGLIGIBLE && newValue != transverse)
				{
					changed = true;
					transverse = newValue;
				}
			}
			if (changed)
			{
				int direction = Geometry.TurnDirection(Vertices[0], Vertices[1], Vertices[2]);
				Vertices[2] = Vertices[1] + Vertices[0].VectorTo(Vertices[1]).Perpendicular(direction).ChangeLength(transverse);
				Vertices[3] = Vertices[0] + Vertices[1].VectorTo(Vertices[2]);
			}
			DiscardPath();
			return changed;
		}
		#endregion

		// Load/Save/CopyFrom in base class are sufficient
	}

	public class OrthogonalRectangle : RectangleShape
	{
		// stored in data as a normal Rectangle (and pretends to be such).  Difference is in the way it is drawn out

		private PointF m_Start;

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			m_Start = position.Snapped;
			base.Start(position);
			Vertices.Add(m_Start);
			Vertices.Add(m_Start);
			m_DefinedVertices = 2; // sort of!
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			Float(position);
			if (m_Bounds.Width < 1 || m_Bounds.Height < 1)
				return VerbResult.Rejected;
			m_DefinedVertices = 4;
			return VerbResult.Completed;
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			DiscardPath();
			m_Bounds = Geometry.RectangleFromPoints(m_Start, position.Snapped);
			Vertices[0] = m_Bounds.Location;
			Vertices[1] = m_Bounds.TopRight();
			Vertices[2] = m_Bounds.BottomRight();
			Vertices[3] = m_Bounds.BottomLeft();
			if (m_Bounds.Width >= 1 && m_Bounds.Height >= 1)
			{
				m_Acceptable = true;
				return VerbResult.Continuing;
			}
			m_Acceptable = false;
			return VerbResult.Rejected;
		}

		public override VerbResult Complete(EditableView.ClickPosition position) => Choose(position);

		public override VerbResult CompleteRetrospective()
		{
			if (m_Bounds.Width > 1 && m_Bounds.Height > 1)
				return VerbResult.Completed;
			return VerbResult.Rejected;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Destroyed;

		internal override List<Prompt> GetPrompts()
		{
			return new List<Prompt>
			{
				new Prompt(ShapeVerbs.Complete, "OrthogonalRectangle_Finish", "OrthogonalRectangle_Finish"), 
				new Prompt(ShapeVerbs.Choose | ShapeVerbs.Complete, "CancelAll", "CancelAll")
			};
		}

		public override SnapModes SnapNext(SnapModes requested)
		{
			switch (requested)
			{
				case SnapModes.Grid:
				case SnapModes.Shape:
					return requested;
				default:
					return SnapModes.Off; // Angle snap doesn't make much sense here
			}
		}
	}


	public class Square : RectangleShape
	{

		#region Information
		public override Shapes ShapeCode => Shapes.Square;
		public override AllowedActions Allows => base.Allows & ~AllowedActions.TransformLinearStretch;
		internal override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Square", true);

		public override string StatusInformation(bool ongoing)
		{
			if (ongoing)
				return base.StatusInformation(true);
			return Strings.Item("Info_Edge") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[0], Vertices[1]));
		}

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices >= 2)
				return SnapModes.Off;
			// do not want to grid snap this point - we only care which side of the line it is.  If we snap it it will tend to be on the line giving no shape at all
			return base.SnapNext(requested);
		}

		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// let the base class take care of positioning the first line
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// need to calculate the coordinates of the fourth point.  We can add the 1>2 vector onto point 0
			Debug.Assert(Vertices.Count == 4);
			m_Acceptable = true;
			int oldTurn = base.TurnDirection();
			Vertices[2] = position.Snapped;
			int direction = base.TurnDirection();
			PlaceSquare(direction);
			if (oldTurn == direction && m_Acceptable)
				return VerbResult.Unchanged;
			DiscardPath();
			return VerbResult.Continuing;
		}

		private void PlaceSquare(int direction)
		{
			SizeF szInitial = BaseVector(); // the vector for the initial baseline
			SizeF szTransverse = SizeF.Empty; // the vector for this second side
			switch (direction)
			{
				case -1:
				case 1:
					szTransverse = szInitial.Perpendicular(direction);
					break;
				case 0: // the points are in a line
					szTransverse = new SizeF(0, 0);
					m_Acceptable = false;
					break;
				default:
					Debug.Fail("TurnDirection returned an unexpected value");
					break;
			}
			Vertices[2] = PointF.Add(Vertices[1], szTransverse);
			Vertices[3] = PointF.Add(Vertices[0], szTransverse);
			m_Bounds = CalculateBounds();
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddGrabSpotsForAllVertices(list);
			return list;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex || move.GrabType == GrabTypes.Radius) // not sure how I want to display them graphically yet, but the functionality will be the same
			{
				int index = move.ShapeIndex;
				int previous = (index + 3) % 4;
				int next = (index + 1) % 4;
				int opposite = (index + 2) % 4;
				PointF target = Geometry.ClosestPointOnLine(Vertices[opposite], Vertices[index], move.Current.Exact);
				if (target.ApproxEqual(Vertices[opposite]))
					return;
				Vertices[index] = target;
				SizeF vector = Vertices[opposite].VectorTo(Vertices[index]).MultiplyBy(0.5f);
				PointF centre = Vertices[opposite] + vector;
				vector = new SizeF(-vector.Height, vector.Width);
				Vertices[previous] = centre + vector;
				Vertices[next] = centre - vector;
				DiscardPath();
			}
			else
				base.DoGrabMove(move);
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
			else
				base.DoGrabAngleSnap(move);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			int direction = base.TurnDirection();
			bool changed = base.TidyVertices(mode, page, 1);
			if (changed)
				PlaceSquare(direction);
			DiscardPath();
			return changed;
		}

		// Load/Save/CopyFrom in base class are sufficient
	}

		public class Rhombus : Sequential
	{

		#region Info
		public override Shapes ShapeCode => Shapes.Rhombus;
		protected override int FixedVerticesLength() => 4;
		public override PointF Centre => base.CalculateCentreFromPoints();
		internal override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Rhombus", false);
		#endregion

		#region Verbs

		// let the base class take care of positioning the first line
		// however after choosing the end of that line becomes point 2 and point 1 is then moved (and point 3 is automatic)

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			PointF pt = position.Snapped;
			if (pt.ApproxEqual(LastDefined))
				return VerbResult.Rejected;
			DiscardPath();

			switch (m_DefinedVertices)
			{
				case 1:
					SetLength(4);
					Vertices[2] = pt;
					Vertices[1] = Geometry.MidPoint(Vertices[0], Vertices[2]);
					m_DefinedVertices = 2;
					Float(position);
					return VerbResult.Continuing;
				case 2:
					Float(position);
					if (Geometry.PointApproxOnLine(Vertices[0], Vertices[2], Vertices[1]))
						return VerbResult.Rejected;
					return VerbResult.Completed;
				default: throw new InvalidOperationException();
			}
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// actually floating point 1 in this case and 0 and 2 are fixed
			PointF pt = position.Snapped;
			if (pt.Equals(Vertices[1]))
				return VerbResult.Unchanged;

			// find point on base line closest to floating point.
			PointF closest = Geometry.ClosestPointOnLine(Vertices[0], Vertices[2], pt);
			// vector from that to float gives us the vector for the second axis (effectively it's ony the length that matters)
			PointF centre = Geometry.MidPoint(Vertices[0], Vertices[2]);
			SizeF halfSecondAxis = closest.VectorTo(pt);
			Vertices[1] = centre + halfSecondAxis;
			Vertices[3] = centre - halfSecondAxis;
			m_Acceptable = !Geometry.PointApproxOnLine(Vertices[0], Vertices[2], Vertices[1]);

			m_Bounds = CalculateBounds();
			DiscardPath();
			return VerbResult.Continuing;
		}

		#endregion

		#region Coords

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddGrabSpotsForAllVertices(list);
			return list;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			// although any point can be moved, we cannot use the base class which assumes they are completely independent
			if (move.GrabType == GrabTypes.SingleVertex)
			{
				PointF centre = Geometry.MidPoint(Vertices[0], Vertices[2]);
				if (move.Current.Snapped.ApproxEqual(centre))
					return;
				int index = move.ShapeIndex;
				// calculation is done on the vector from centre point to the moving point.  The direction of this is maintained (so this cannot rotate the rhombus)
				// (technically, it would be possible to do a rotation and update all 4 points, but it's probably a bit odd from a UI perspective)
				SizeF existingVector = centre.VectorTo(Vertices[index]);
				SizeF requested = centre.VectorTo(move.Current.Snapped);
				SizeF resultVector = Geometry.ProjectionVector(requested, existingVector);

				int opposite = (index + 2) % 4;
				// check it is not degenerate:
				Vertices[index] = centre + resultVector;
				Vertices[opposite] = centre - resultVector;
				m_Bounds = CalculateBounds();
				DiscardPath();
			}
			else
				base.DoGrabMove(move);
		}

		protected override RectangleF CalculateBounds() => BoundsOfVertices();

		#endregion

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			// when drawing base line draw it dashed as it's not actually one of the final lines
			if (resources.MainPen != null && m_DefinedVertices == 1)
				resources.MainPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
		}

	}

}
