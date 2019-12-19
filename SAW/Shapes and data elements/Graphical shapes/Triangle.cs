using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;

namespace SAW
{
	public class Triangle : Sequential
	{
		// Triangle class itself implements the scalene

		#region Information
		public override Shapes ShapeCode => Shapes.Triangle;
		protected override int FixedVerticesLength() => 3;
		public override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Triangle", false);

		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// I think that the base class Float is sufficient, except that it does not  set m_bolAcceptable
			var eResult = base.Float(position);
			m_Acceptable = m_DefinedVertices >= 2 && !VerticesFormLine(0);
			return eResult;
		}

		public override PointF Centre => base.CalculateCentreFromPoints();

		public override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddGrabSpotsForAllVertices(list, new Prompt(ShapeVerbs.Choose, "MoveVertex_Independent", "MoveVertex"));
			return list;
		}

		public override PointF DoSnapAngle(PointF newPoint)
		{
			if (m_DefinedVertices == 2)
				return Geometry.AngleSnapFromTwoPoints(newPoint, Vertices[1], Vertices[0], false);
			return base.DoSnapAngle(newPoint);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			int direction = base.TurnDirection();
			bool changed = base.TidyVertices(mode, page, 2);
			if (changed)
			{
				// just need to check that the vertices do not form a single line
				if (Geometry.PointApproxOnLine(Vertices[0], Vertices[1], Vertices[2]))
				{
					// move the point again to get it off the line
					if (direction == 0)
						direction = 1; // just in case it was already a single line
					SizeF vector = Vertices[0].VectorTo(Vertices[1]).Perpendicular(direction);
					Vertices[2] = Vertices[2] + vector.ChangeLength(page.Paper.UnitStep); // UnitStep always returns a valid value
				}
			}
			return changed;
		}

		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();

		// Load/Save/CopyFrom in base class are sufficient
	}

	public class Isosceles : Triangle
	{

		#region Information
		public override Shapes ShapeCode => Shapes.Isosceles;
		public override AllowedActions Allows => base.Allows & ~AllowedActions.TransformLinearStretch;
		public override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Isosceles", false);

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices >= 2)
			{
				// because the direction of the second line is fixed, it only makes sense to snap this to a grid point
				// if the first line was lined up with the grid.  Otherwise the second line is going diagonally and will probably never hit any grid points
				if (!Geometry.LineApproxPerpendicular(Vertices[0], Vertices[1]))
					return SnapModes.Off;
			}
			return base.SnapNext(requested);
		}

		public override string StatusInformation(bool ongoing)
		{
			if (ongoing)
				return base.StatusInformation(true); // display length of current edge
			return Strings.Item("Info_EqualEdge") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[1], Vertices[2])) + "  " + Strings.Item("Info_OddEdge") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[1], Vertices[0]));
		}
		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// let the base class take care of positioning the first line
			if (m_DefinedVertices < 2)
				return base.Float(position);
			Debug.Assert(Vertices.Count == 3);
			// rather like the rectangle we extend the vector which is perpendicular at to the baseline.  Just in this case, we have to start halfway down the baseline
			SizeF baseVector = BaseVector().MultiplyBy(0.5f); // actually half the base vector
			Vertices[2] = Geometry.PerpendicularPoint(Vertices[0], PointF.Add(Vertices[0], baseVector), position.Snapped);
			m_Bounds = CalculateBounds();
			m_Acceptable = !VerticesFormLine(0);
			DiscardPath();
			return VerbResult.Continuing;
		}

		public override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddBaselineGrabSpot(list);
			GrabSpot create = new GrabSpot(this, GrabTypes.Radius, Vertices[2], 2);
			create.Focus = Geometry.MidPoint(Vertices[0], Vertices[1]);
			list.Add(create);
			return list;
		}

		protected override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					if (move.ShapeIndex != 1)
					{
						Debug.Fail("Isosceles.DoGrabMove(SingleVertex): index must be 1");
						return;
					}
					if (move.Current.Snapped.ApproxEqual(Vertices[0]))
						return;
					int direction = base.TurnDirection(); // maintain the same winding direction, and transverse length (i.e. from baseline to third point)
					float transverseSize = Geometry.DistancePointToLine(Vertices[0], Vertices[1], Vertices[2]);
					Vertices[1] = move.Current.Snapped;
					SizeF transverseVector = BaseVector().Perpendicular(direction);
					transverseVector = transverseVector.ChangeLength(transverseSize); // the correct, new vector from the midpoint of the baseline
					Vertices[2] = PointF.Add(Geometry.MidPoint(Vertices[0], Vertices[1]), transverseVector);
					m_Bounds = CalculateBounds();
					break;
				case GrabTypes.Radius: // this moves the last point
					if (move.ShapeIndex != 2)
					{
						Debug.Fail("Isosceles.DoGrabMove(Radius): index must be 1");
						return;
					}
					PointF original = Vertices[2];
					SizeF baseVector = BaseVector().MultiplyBy(0.5f); // actually half the base vector
					Vertices[2] = Geometry.PerpendicularPoint(Vertices[0], Vertices[0] + baseVector, move.Current.Exact);
					if (VerticesFormLine(0))
						Vertices[2] = original; // cannot store this
					m_Bounds = CalculateBounds();
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
			DiscardPath();
		}

		public override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
			else
				base.DoGrabAngleSnap(move);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			int direction = base.TurnDirection();
			float transverseSize = Geometry.DistanceBetween(Geometry.MidPoint(Vertices[0], Vertices[1]), Vertices[2]);
			bool changed = base.TidyVertices(mode, page, 1);
			if (mode == SnapModes.Grid)
			{
				float newSize = page.Paper.SnapScalar(transverseSize);
				if (transverseSize > Geometry.NEGLIGIBLE && transverseSize != newSize)
				{
					transverseSize = newSize;
					changed = true;
				}
			}
			if (changed)
			{
				SizeF baseVector = BaseVector().MultiplyBy(0.5f); // actually half the base vector
				Vertices[2] = Vertices[0] + baseVector + baseVector.Perpendicular(direction).ChangeLength(transverseSize);
			}
			return changed;
		}

		// Load/Save/CopyFrom in base class are sufficient
	}

	public class Equilateral : Triangle
	{

		#region Information
		public override Shapes ShapeCode => Shapes.Equilateral;
		public override AllowedActions Allows => base.Allows & ~AllowedActions.TransformLinearStretch;
		public override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Equilateral", true);
		public override string StatusInformation(bool ongoing) => Strings.Item("Info_Edge") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[0], Vertices[1]));

		#endregion

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			// let the base class take care of positioning the first line
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// otherwise similar to Square, except that we work from half the base vector and need to adjust the perpendicular length
			Debug.Assert(Vertices.Count == 3);
			m_Acceptable = true;
			Vertices[2] = position.Snapped;
			PlaceThird(base.TurnDirection());
			DiscardPath();
			return VerbResult.Continuing;
		}

		private void PlaceThird(int direction)
		{
			SizeF initial = BaseVector(); // the vector for the initial baseline
			m_Acceptable = direction != 0;
			SizeF szTransverse = SizeF.Empty;
			if (direction != 0)
				szTransverse = initial.Perpendicular(direction); // the vector for this second side
			szTransverse = szTransverse.MultiplyBy((float)Math.Sqrt(3) / 2);
			Vertices[2] = Geometry.MidPoint(Vertices[0], Vertices[1]) + szTransverse; // PointF.Add(PointF.Add(Vertices(0), szInitial), szTransverse)
			m_Bounds = CalculateBounds();
		}

		public override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddBaselineGrabSpot(list);
			return list;
		}

		protected override void DoGrabMove(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
			{
				if (move.ShapeIndex != 1)
				{
					Debug.Fail("Equilateral.DoGrabMove(SingleVertex): index must be 1");
					return;
				}
				if (move.Current.Exact.ApproxEqual(Vertices[0]))
					return;
				int direction = base.TurnDirection(); // maintain the same winding direction
				Vertices[1] = move.Current.Snapped;
				PlaceThird(direction);
				DiscardPath();
			}
			else
				base.DoGrabMove(move);
		}

		public override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
			else
				base.DoGrabAngleSnap(move);
		}

		public override PointF DoSnapAngle(PointF newPoint)
		{
			if (m_DefinedVertices == 2)
				return newPoint;
			// dont try solving for third point - triangle is fixed anyway
			return base.DoSnapAngle(newPoint);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			int direction = base.TurnDirection();
			bool changed = base.TidyVertices(mode, page, 1);
			if (changed) 
				PlaceThird(direction);
			return changed;
		}

		// Load/Save/CopyFrom in base class are sufficient
	}
}
