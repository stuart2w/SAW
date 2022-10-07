using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace SAW.Shapes
{
	/// <summary>Curve drawn freehand</summary>
	public class Pencil : Curve
	{

		// Used only when creating the shape:
		private List<PointF> m_Targets; // if this is not nothing, it indicates the shape is still being drawn
		private const int MAXIMUMTARGETS = 200;
		private static DateTime g_dtLastTargetAdded = DateTime.Now; // only used when drawing.  The time when the last point was added

		public Pencil() : base(false)
		{
		}

		#region Information

		public override Shapes ShapeCode => Shapes.Pencil;

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (requested != SnapModes.Shape)
				return SnapModes.Off;
			return SnapModes.Shape; // can snap the end point to a shape
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			list.Add(new Prompt(ShapeVerbs.Complete, "Pencil_Finish", "Pencil_Finish"));
			list.Add(new Prompt(ShapeVerbs.Complete, "Pencil_FinishClose", "Pencil_FinishClose"));
			if (m_Targets.Count < 2)
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			else
				list.Add(new Prompt(ShapeVerbs.Cancel | ShapeVerbs.NotDrag, "Pencil_Cancel", "Pencil_Cancel"));
			return list;
		}
		#endregion

		#region Verbs
		// was 4.8/6/3 in v2 (already more time than v1?)
		private const float TARGETSEPARATION = 6; // distance between targets, they are placed as soon as the user has moved this far
		private const float ADDMMPERSECOND = 10; // but adds extra distance if the user is moving slowly, i.e. placing the points more closely if the user moves slowly
		private const float MAXIMUMTIMEDISTANCE = 5; // the maximum contribution time can make to the above.
													 //I.e. giving a minimum physical separation of TARGETSEPARATION - MAXIMUMTIMEDISTANCE
		private const float ADDMMPERDEGREE = 0.05f; // ie 90deg = 4.5 extra (90° is the maximum considered)

		public override VerbResult Start(ClickPosition position)
		{
			m_Targets = new List<PointF>();
			m_Targets.Add(position.Snapped);
			Debug.Assert(LineStyle != null && FillStyle != null);
			LineStyle.SetDefaults();
			FillStyle.Colour = Color.Empty;
			FillStyle.Pattern = FillStyleC.Patterns.Solid;
			g_dtLastTargetAdded = DateTime.Now;
			return VerbResult.Continuing;
		}

		public override VerbResult Float(ClickPosition pt)
		{
			float distance = (float)DateTime.Now.Subtract(g_dtLastTargetAdded).TotalMilliseconds * ADDMMPERSECOND / 1000;
			distance = Math.Max(0, Math.Min(distance, MAXIMUMTIMEDISTANCE)); // this is now the time component.  Above could give <0 as cancel sets ref time in future
																			 //Debug.WriteLine("Time component = " + sngDistance.ToString)
			distance += Geometry.DirtyDistance(pt.Exact, m_Targets[m_Targets.Count - 1]);
			if (m_Targets.Count > 1 && distance > 1)
			{
				// also consider angular change.  Not considered when the distance is very small, otherwise the angle is largely spurious
				var previous = Geometry.VectorAngle(m_Targets[m_Targets.Count - 2], m_Targets[m_Targets.Count - 1]);
				var current = Geometry.VectorAngle(m_Targets[m_Targets.Count - 1], pt.Exact);
				var angularChange = Geometry.AbsoluteAngularDifference(previous, current);
				angularChange = Math.Min((int)Geometry.Degrees(angularChange), 90); // limited to 90°, mainly to filter out false positives
				Debug.WriteLine("Distance = " + distance + " + " + angularChange * ADDMMPERDEGREE);
				distance += angularChange * ADDMMPERDEGREE;
			}
			if (distance >= TARGETSEPARATION && m_Targets.Count < MAXIMUMTARGETS)
			{
				Debug.WriteLine("Placed point");
				m_Targets.Add(pt.Exact);
				Debug.Assert(!(m_Targets[m_Targets.Count - 1] == m_Targets[m_Targets.Count - 2]));
				m_Bounds = CalculateBounds();
				g_dtLastTargetAdded = DateTime.Now;
				if (m_Targets.Count == 2)
					Parent.NotifyIndirectChange(this, ChangeAffects.UpdatePrompts);
				if (m_Targets.Count > 2)
					CreateCurves(false);
				return VerbResult.Continuing;
			}
			else
				return VerbResult.Unchanged;
		}

		public override VerbResult Choose(ClickPosition position) => Complete(position);

		public override VerbResult Complete(ClickPosition position)
		{
			// we don't use Float, because we want to force this last point to be added even if it is a bit close to the previous point
			// (if we use the minimum distance logic than the line can noticeably fall short of where the user stopped)
			if (m_Targets.Count < MAXIMUMTARGETS)
			{
				// but if the finishing point is very close to the previous point it will muck up the angle geometry, in which case move the previous point instead
				if (Geometry.DirtyDistance(position.Snapped, m_Targets[m_Targets.Count - 1]) <= TARGETSEPARATION / 3)
					m_Targets[m_Targets.Count - 1] = position.Snapped;
				else
					m_Targets.Add(position.Snapped);
				m_Bounds = RectangleF.Empty;
			}
			return CompleteRetrospective();
		}

		public override VerbResult CompleteRetrospective()
		{
			if (m_Targets.Count < 2)
				return VerbResult.Destroyed;
			CreateCurves(true);
			return VerbResult.Completed;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (m_Targets.Count < 2)
				return VerbResult.Destroyed;
			m_Targets.RemoveAt(m_Targets.Count - 1);
			Parent.NotifyIndirectChange(this, ChangeAffects.MoveMouse, new RectangleF(m_Targets[m_Targets.Count - 1], new SizeF(0, 0)));
			if (m_Targets.Count < 2)
				Parent.NotifyIndirectChange(this, ChangeAffects.UpdatePrompts);
			g_dtLastTargetAdded = DateTime.Now.AddSeconds(2); // don't really want to use time trigger after this unless user very slow
			m_Sections = null; // recalculate entire curve
			CreateCurves(false);
			return VerbResult.Continuing;
		}

		#endregion

		#region Coordinates
		protected override RectangleF CalculateBounds()
		{
			if (m_Targets != null)
			{
				RectangleF bounds = RectangleF.Empty;
				foreach (PointF target in m_Targets)
				{
					Geometry.Extend(ref bounds, target);
				}
				return bounds;
			}
			return base.BoundsOfPathDetailed();
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			return m_Targets != null ? base.RefreshBoundsFromBounds(withShadow) : base.RefreshBoundsFromPath(withShadow);
		}

		// limits used when deciding where to split the curve:
		private const float MAXIMUMSINGLEANGLECHANGE = Geometry.PI / 2; // this is an absolute applied at any applicable point
		private const float MAXIMUMCUMULATIVEANGLECHANGE = Geometry.ANGLE90 * 1.6f; // otherwise the limit is on the cumulative angle change
		private const float TARGETEQUALSANGLE = Geometry.ANGLE90 / 10; // but every target adds a small angle even if it is straight

		// there is intrinsically therefore a maximum length, because there is a maximum number of targets (currently 16)

		private struct Section
		{
			public int StartsAt;
			public bool Discontinuity;

			public Section(int starts, bool discontinuity)
			{
				StartsAt = starts;
				Discontinuity = discontinuity;
			}
		}

		List<Section> m_Sections;

		private void CreateCurves(bool final)
		{
			// final = true if the shape has been completed; otherwise it is still ongoing and we mustn't corrupt the end
			// close, if appropriate... (Only happens if the user has finished; if the user is still moving we always treat it as open
			// partly because this probably looks better, but also closing the curve would corrupt the start point)
			m_Closed = Geometry.DirtyDistance(m_Targets[0], m_Targets[m_Targets.Count - 1]) < CLOSURETHRESHOLD && final;
			if (m_Closed)
			{
				if (Geometry.DirtyDistance(m_Targets[0], m_Targets[m_Targets.Count - 1]) < CLOSURETHRESHOLD / 2)
					// if very close just move the last point back to the start
					m_Targets[m_Targets.Count - 1] = m_Targets[0];
				else
					// otherwise add an extra target back at the beginning
					m_Targets.Add(m_Targets[0]);
			}

			// First choose how many Bezier curves to use.  We split the line up either by distance (there was a maximum length for each section)
			// or if the user turns through more than 90° at any one point, or if the cumulative angle change over the set of points exceeds a threshold
			Section last; // in various places we need to do significant processing on the last stored section
						  // storing it separately is probably quicker than looking it up in the list, and also makes the code more readable.  This is updated whenever a new item is added
			if (m_Sections == null || m_Sections.Count < 2)
			{
				m_Sections = new List<Section>();
				last = new Section(0, true);
				m_Sections.Add(last);
			}
			else
			{
				// The last Section object will mark the end of the curve; we need to remove this and reprocess from the previous one (marking the beginning of the last curve)
				m_Sections.RemoveAt(m_Sections.Count - 1);
				last = m_Sections.Last();
				//objLast = New Section(m_Sections.Last.StartsAt, False)
				//m_Sections(m_Sections.Count - 1) = objLast
			}
			int index = last.StartsAt + 1; // The vertex we are processing
			int firstSection = m_Sections.Count - 1; // this is the first curve which needs to be recalculated

			float cumulativeAngle = 0; // sum of the angle changes at each vertex in the current curve
			while (index < m_Targets.Count - 1) // we don't need to process the last one; this always counts as a break!
			{
				PointF current = m_Targets[index];
				float angle = Geometry.AngleBetween(Geometry.VectorAngle(m_Targets[index - 1], current), Geometry.VectorAngle(current, m_Targets[index + 1]));
				if (angle > Geometry.ANGLE180)
					angle = Geometry.ANGLE360 - angle;
				cumulativeAngle += angle + TARGETEQUALSANGLE;
				if (angle > MAXIMUMSINGLEANGLECHANGE)
				{
					// Must split here, we keep this point as a vertex because it seems to be a point rather than a smooth curve here
					// however if we only have one (or two?) vertices in the current section, we might want to extend the previous section
					if (index > 2 && index <= last.StartsAt + 2 && last.Discontinuity == false)
					{
						// this is the first target after the last curve ended; and the last break wasn't itself a discontinuity. just extend the previous section up to this point instead
						last = new Section(last.StartsAt, true);
						m_Sections[m_Sections.Count - 1] = last;
					}
					else
					{
						// otherwise break here as normal
						last = new Section(index, true);
						m_Sections.Add(last);
					}
					cumulativeAngle = 0;
				}
				else if (cumulativeAngle >= MAXIMUMCUMULATIVEANGLECHANGE)
				{
					// Must split here due to length/cumulative angle change
					last = new Section(index, false);
					m_Sections.Add(last);
					cumulativeAngle = 0;
				}
				index += 1;
			}
			// add a break at the end, unless this leaves a very short segment in which case we might extend the previous one to bring it to the end
			if (index > 2 && index <= last.StartsAt + 2 && last.Discontinuity == false)
				m_Sections[m_Sections.Count - 1] = new Section(m_Targets.Count - 1, true);
			else
				m_Sections.Add(new Section(m_Targets.Count - 1, true)); // last target point also counts as a break
																		// objLast no longer maintained below this

			// if closed, check if the closure point should be smoothed
			if (m_Closed)
			{
				PointF current = m_Targets[0];
				float angle = Geometry.AngleBetween(Geometry.VectorAngle(m_Targets[m_Targets.Count - 2], current), Geometry.VectorAngle(current, m_Targets[1]));
				// it is m_Targets.Count - 2 because we have already added a repeat of the first point at the end of the list
				if (angle > Geometry.ANGLE180)
					angle = Geometry.ANGLE360 - angle;
				//Debug.WriteLine("Closure angle: " + sngAngle.ToString)
				if (angle < MAXIMUMSINGLEANGLECHANGE)
				{
					m_Sections[0] = new Section(m_Sections[0].StartsAt, false);
					m_Sections[m_Sections.Count - 1] = new Section(m_Sections.Last().StartsAt, false);
				}
			}

			CreateActualCurves(firstSection);

			// and finally ensure continuity except where we deliberately have a point
			// we need to make sure the control bars at each point form a line with the point
			// ideally we'd have chosen the best lines above using this constraint but that would need 4 versions (neither,A,B or both constrained)
			// which would be VERY tedious - this seems to work well enough
			Utilities.ErrorAssert(Vertices.Count == m_Sections.Count);
			for (index = m_Closed ? 0 : 1; index <= m_Sections.Count - (final ? 2 : 3); index++) // if not final then we don't smooth the join between the last 2 curves because this may yet change
			{
				if (!m_Sections[index].Discontinuity)
				{
					int previousControl = index * 2 - 1; // index number of previous control point
					if (index == 0)
						previousControl = m_ControlPoints.Count - 1;
					// Calculate how the vertex would need to be moved to get it back on the line between the control points
					PointF closest = Geometry.ClosestPointOnLine(m_ControlPoints[previousControl], m_ControlPoints[index * 2], Vertices[index]);
					SizeF vector = Vertices[index].VectorTo(closest);
					// now move the vertex by half of this and the control points half of this in the opposite direction
					vector = vector.MultiplyBy(0.5f);
					Vertices[index] += vector;
					if (index == 0)
						Vertices[Vertices.Count - 1] = Vertices[0]; // if closed must update both first and last
					m_ControlPoints[previousControl] -= vector;
					m_ControlPoints[index * 2] -= vector;
				}
			}

#if DEBUG
			if (!ShowTargets && final)
				m_Targets = null;
#endif
			DiscardPath();
			m_Bounds = CalculateBounds();
			m_DefinedVertices = Vertices.Count;
			if (m_Closed)
				m_DefinedVertices -= 1;
		}

		private void CreateActualCurves(int firstSection)
		{
			// creates the actual curves from the sections.  Parameter gives the first section which needs to be recalculated
			if (Vertices.Count > firstSection + 1)
			{
				// There is one point per vertex plus an extra one on the end.  The point at the end of the last given curve at the beginning of the new curve can remain (it does not move)
				Vertices.RemoveRange(firstSection + 1, Vertices.Count - firstSection - 1);
				// There is no need for a separate condition for the control points as they are always created with the vertices
				m_ControlPoints.RemoveRange(firstSection * 2, m_ControlPoints.Count - firstSection * 2);
			}
			else if (Vertices.Count == 0)
				Vertices.Add(m_Targets[0]);
			for (int index = firstSection; index <= m_Sections.Count - 2; index++)
			{
				int start = m_Sections[index].StartsAt; // index of first point
				int last = m_Sections[index + 1].StartsAt;
				PointF[] points = new PointF[last - start + 1]; // put the points for this curve into an array
				for (int point = start; point <= last; point++)
				{
					points[point - start] = m_Targets[point];
				}
				Vertices.Add(m_Targets[last]); // end point of this curve

				// Choose some sensible starting values for the control points
				PointF A;
				PointF B;
				if (last == start + 1)
				{
					// somewhat degenerate, no intermediate points available at all - this appears to be just a straight line
					Debug.WriteLine("Curve " + index + " has no intermediate points");
					A = m_Targets[start];
					B = m_Targets[last];
				}
				else if (last == start + 2)
				{
					// not exactly degenerate, but I'm not sure the code below would really work for this
					A = m_Targets[start + 1];
					AddPoint(ref A, m_Targets[start + 1], 2);
					AddPoint(ref A, m_Targets[start], -1);
					AddPoint(ref A, m_Targets[start + 2], -1);
					B = A;
				}
				else
				{
					// take a sort of weighted average of the points
					// BIT ROPEY BUT WILL DO
					float total = 0; // the total weighting applied (will be the same for both ptA and ptB [although the weighting at each point is different for A and B, it is symmetrical is the total worked out the same for each])
					A = PointF.Empty;
					B = PointF.Empty;
					float third = (last - start) / 3f; // a nominal index one third of the way through; the control points are nominally spaced at thirds of the curve
					const float Factor = 0.6f;
					for (int point = start; point <= last; point++)
					{
						float weighting;
						if (point - start <= third)
							weighting = Factor + third / 2 - Math.Abs(point - start - third);
						else
							weighting = Factor + (third - Math.Abs(point - start - third)) * 0.5f;
						total += weighting;
						AddPoint(ref A, m_Targets[point], weighting);
						if (point - start <= third * 2)
							weighting = Factor + (third - Math.Abs(point - start - third * 2)) * 0.5f;
						else
							weighting = Factor + third / 2 - Math.Abs(point - start - third * 2);
						AddPoint(ref B, m_Targets[point], weighting);
					}
					A = new PointF(A.X / total, A.Y / total);
					B = new PointF(B.X / total, B.Y / total);
				}

				// Now refine the points properly.  A crude version of IGLS
				PointF[] P = { m_Targets[start], A, B, m_Targets[last] }; // the postulated Bezier curve
				float step = 2; // distance to try moving in each direction
				const float MINIMUMSTEP = 0.1f; // stop the refinement once we are moving in steps this small
				bool complete = false;
				int iterations = 0;
				while (!complete)
				{
					float currentError = CalculateError(P, points);
					float[] improvement = new float[4]; // the improvement available by changing each of the degrees of freedom.  Stored as a positive number
					float[] best = new float[4]; // best direction in which to change for each intParameter
					for (int parameter = 0; parameter <= 3; parameter++)
					{
						// step through the degrees of freedom (both X and Y for each of the 2 intermediate control points.  The endpoints are fixed)
						int modifyIndex = parameter / 2 + 1; // the index within P which we are modifying: 1 or 2
						SizeF positiveSize;
						if (parameter % 2 == 0)
							positiveSize = new SizeF(step, 0);
						else
							positiveSize = new SizeF(0, step);
						P[modifyIndex] += positiveSize;
						float positive = CalculateError(P, points);
						P[modifyIndex] -= positiveSize;
						P[modifyIndex] -= positiveSize;
						float negative = CalculateError(P, points);
						P[modifyIndex] += positiveSize; // put back the original value
						if (positive >= currentError && negative >= currentError)
						{
							improvement[parameter] = 0; // neither direction improves
							best[parameter] = 0;
						}
						else if (positive >= currentError && negative < currentError)
						{
							improvement[parameter] = currentError - negative; // only negative direction is an improvement
							best[parameter] = -step;
						}
						else if (positive < currentError && negative >= currentError)
						{
							improvement[parameter] = currentError - positive; // only positive direction is an improvement
							best[parameter] = step;
						}
						else
						{
							// both directions improve, check which gives the best improvement
							// in these cases we reduce the vector somewhat, as it is not so certain which way to move
							if (positive < negative)
							{
								improvement[parameter] = currentError - positive; // only positive direction is an improvement
								best[parameter] = step / 2;
							}
							else
							{
								improvement[parameter] = currentError - negative; // only positive direction is an improvement
								best[parameter] = -step / 2;
							}
						}
					}
					float totalImprovement = 0;
					for (int intParameter = 0; intParameter <= 3; intParameter++)
					{
						totalImprovement += improvement[intParameter];
					}
					if (totalImprovement == 0)
					{
						// No improvement found moving in these steps
						step /= 2;
						if (step < MINIMUMSTEP)
							complete = true;
					}
					else
					{
						// move in the direction found
						P[1] = new PointF(P[1].X + best[0] * improvement[0] / totalImprovement, P[1].Y + best[1] * improvement[1] / totalImprovement);
						P[2] = new PointF(P[2].X + best[2] * improvement[2] / totalImprovement, P[2].Y + best[3] * improvement[3] / totalImprovement);
					}
					iterations += 1;
					if (iterations > 100)
						complete = true; // safety valve
				} // not complete
				  //Debug.WriteLine("Iterations used: " + intIterations.ToString)

				if (P[1].ApproxEqual(P[0]))
				{
					// not sure why, but have generated this.  Splatter doesn't like it
					Globals.Root.Log.WriteLine("Pencil P1 = P0, adjusting");
					P[1] = Geometry.Interpolate(P[0], P[3], 0.25F);
				}
				if (P[2].ApproxEqual(P[3]))
				{
					Globals.Root.Log.WriteLine("Pencil P2 = P3, adjusting");
					P[2] = Geometry.Interpolate(P[0], P[3], 0.75F);
				}
				m_ControlPoints.Add(P[1]);
				m_ControlPoints.Add(P[2]);
			}
		}

		private float CalculateError(PointF[] P, PointF[] points)
		{
			// calculates the sum of the squares of the distances from the points to the line
			float error = 0;
			// the first and last points in points should be the endpoints of the curve itself, so we don't need to check how far these are from the line!
			Debug.Assert(P[0] == points[0] && P[3] == points[points.Length - 1]);
			for (int index = 1; index <= points.Length - 1 - 1; index++)
			{
				PointF pt = points[index];
				float T = Bezier.FindNearestApproachToPoint(pt, P);
				PointF curve = Bezier.GetPoint(P, T);
				error += (pt.X - curve.X) * (pt.X - curve.X) + (pt.Y - curve.Y) * (pt.Y - curve.Y);
			}
			return error;
		}

		private static void AddPoint(ref PointF modify, PointF add, float weighting)
		{
			modify = new PointF(modify.X + add.X * weighting, modify.Y + add.Y * weighting);
		}

		protected override void SetControlPoints(int firstVertex, int lastVertex = int.MaxValue)
		{
			// MyBase.SetControlPoints(intFirstVertex, intLastVertex)
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			// we must implement the functionality at the base class, because we cannot use the version of this in Curve
			// however if we disallow single vertex movement there is almost no base functionality to be fitted
			m_Targets = null;
			// not really needed; but at the moment I have been leaving the targets in place for diagnostics
			switch (move.GrabType)
			{
				case GrabTypes.Move:
					this.ApplyTransformation(move.Transform); // will transform the control points
					m_Bounds = RectangleF.Empty;
					break;
				case GrabTypes.SingleVertex:
					PointF newPoint = Vertices[move.ShapeIndex];
					move.Transform.TransformPoint(ref newPoint);
					// we need to check that this point does not collide with the previous or next ones before actually storing it
					if (move.ShapeIndex > 0 && newPoint.ApproxEqual(Vertices[move.ShapeIndex - 1]))
						return;
					if (move.ShapeIndex < Vertices.Count - 1 && newPoint.ApproxEqual(Vertices[move.ShapeIndex + 1]))
						return;
					Vertices[move.ShapeIndex] = newPoint;
					if (move.GrabType == GrabTypes.SingleVertex && m_Closed && move.ShapeIndex == 0)
						// if moving the initial point, we must also move the closing point
						Vertices[Vertices.Count - 1] = Vertices[0];
					// and we move the previous and next control points by a similar amount
					if (move.ShapeIndex > 0 || m_Closed)
					{
						int previous = move.ShapeIndex * 2 - 1;
						if (move.ShapeIndex == 0)
							previous = m_ControlPoints.Count - 1;
						PointF temp = m_ControlPoints[previous];
						move.Transform.TransformPoint(ref temp);
						m_ControlPoints[previous] = temp;
#if DEBUG
						if (m_ControlPoints[previous].ApproxEqual(Vertices[move.ShapeIndex]))
						{
							// splatter won't like this.  Shouldn't have been created in first place, but Andrew's doc had this
							m_ControlPoints[previous] = Geometry.Interpolate(Vertices[move.ShapeIndex], Vertices[move.ShapeIndex - 1 % Vertices.Count], 0.25F);
						}
#endif
					}
					if (move.ShapeIndex < m_DefinedVertices - 1 || m_Closed)
					{
						int control = move.ShapeIndex * 2 % (Vertices.Count * 2 - 2);
						PointF temp = m_ControlPoints[control];
						move.Transform.TransformPoint(ref temp);
						m_ControlPoints[control] = temp;
#if DEBUG
						if (m_ControlPoints[control].ApproxEqual(Vertices[move.ShapeIndex]))
							m_ControlPoints[control] = Geometry.Interpolate(Vertices[move.ShapeIndex], Vertices[(move.ShapeIndex + 1) % Vertices.Count], 0.25F);
#endif
					}
					m_Bounds = RectangleF.Empty;
					break;
				case GrabTypes.Rotate:
					base.DoGrabMove(move);
					break;
				default:
					Debug.Fail("Unexpected GrabSpot movement type in Pencil curve: " + move.GrabType);
					break;
			}
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType != GrabTypes.SingleVertex)
				base.DoGrabAngleSnap(move);
		}

		/// <summary>Ensures this line is closed.</summary>
		public void ForceClosure(Transaction transaction)
		{
			if (m_Closed)
				return;
			transaction.Edit(this);
			PointF mid = Geometry.MidPoint(Vertices[0], Vertices.Last());
			// closure is added as a straight line, by placing the control points on a line from the current last point with start
			Vertices.Add(Vertices[0]);
			m_ControlPoints.Add(mid);
			m_ControlPoints.Add(mid);
			m_Closed = true;
		}

		#endregion

		#region Graphics - Now debug only

#if DEBUG
		public static bool ShowTargets = false; // for diagnostics we can leave the targets behind
		protected override void PrepareDraw(DrawResources objResources)
		{
			base.PrepareDraw(objResources);
			if (m_Targets != null && ShowTargets)
			{
				float pixel = Geometry.INCH / objResources.Graphics.DpiX; // size of a pixel in graphics coords
																		  // doesn't actually adjust for scaling, but I think it works OK like that
				objResources.CustomPen = objResources.Graphics.CreateStroke(LineStyle.Colour, pixel * 3);
				objResources.CustomPen.EndCap = System.Drawing.Drawing2D.LineCap.Round; // does both ends
			}
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			base.InternalDraw(gr, resources);
			if (resources.CustomPen != null) // draw the points
			{
				float pixel = Geometry.INCH / gr.DpiX; // size of a pixel in graphics coords
				foreach (PointF ptTarget in m_Targets)
				{
					gr.DrawLine(ptTarget, new PointF(ptTarget.X + pixel, ptTarget.Y), resources.CustomPen);
				}
			}

			//If Not m_bolClosed Then MyBase.DrawArrowheads(objResources)
		}
#endif

		#endregion

		#region Data
		// This is needed because in the base class closure is implicit in the shape type

		protected internal override void Load(DataReader reader)
		{
			m_Closed = reader.ReadBoolean();
			base.Load(reader);
		}

		protected internal override void Save(DataWriter writer)
		{
			writer.Write(m_Closed);
			base.Save(writer);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			m_Closed = ((Pencil)other).m_Closed;
		}
		#endregion

	}
}