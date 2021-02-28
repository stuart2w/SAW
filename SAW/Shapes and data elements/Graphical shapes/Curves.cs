using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text;


namespace SAW
{
	public class Curve : Sequential
	{
		// This is drawn as a Bezier curve, but the control points automatically calculated, the user cannot edit them.
		// Although they are not user changeable, the control points are stored in the data for speed
		protected List<PointF> m_ControlPoints = new List<PointF>();
		// there is a pair of control points for each complete line segment.  The number of control points = 2*(Vertices.Count -1)
		protected bool m_Closed;

		public Curve(bool closed)
		{
			m_Closed = closed;
			if (closed)
			{
				EndArrowhead = null;
				StartArrowhead = null;
			}
		}

		#region Basic info
		public override PointF Centre => base.CalculateCentreFromPoints();
		public override Shapes ShapeCode => !m_Closed ? Shapes.Curve : Shapes.ClosedCurve;
		protected override bool UseBaseline() => false;
		protected  internal  override bool Closed() => m_Closed;
		public override string StatusInformation(bool ongoing) => "";
		public override LabelModes LabelMode => LabelModes.NotSupported;
		public override AllowedActions Allows => base.Allows & ~(AllowedActions.Tidy | AllowedActions.PermittedArea);
		protected override LineLogics LineLogic => LineLogics.UsePath;

		public override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			switch (parameter)
			{
				case Parameters.FillColour:
				case Parameters.FillPattern:
					if (!m_Closed)
						return null;
					break;
			}
			return base.StyleObjectForParameter(parameter, applyingDefault);
		}

		public override bool IsFilled
		{
			get
			{
				if (!m_Closed)
					return false;
				return base.IsFilled;
			}
		}

		public override void Diagnostic(StringBuilder output)
		{
			base.Diagnostic(output);
			output.Append("Closed = ").AppendLine(m_Closed.ToString());
		}

		internal override List<Prompt> GetPrompts()
		{
			if (m_Closed)
				return GetPolyPointPrompts("ClosedCurve");
			return GetPolyPointPrompts("Curve");
		}

		#endregion

		#region Verbs

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			if (base.Start(position) == VerbResult.Continuing)
			{
				m_ControlPoints.Add(Vertices[0]);
				m_ControlPoints.Add(Vertices[1]);
				if (m_Closed)
				{
					Vertices.Add(Vertices[0]);
					m_ControlPoints.Add(Vertices[1]);
					m_ControlPoints.Add(Vertices[1]);
				}
				else
					FillStyle.Colour = Color.Empty;
				SetControlPoints(0);
				return VerbResult.Continuing;
			}
			return VerbResult.Destroyed;
		}

		public override VerbResult Float(EditableView.ClickPosition pt)
		{
			if (m_DefinedVertices >= Vertices.Count)
			{
				Debug.Fail("Curve: Float no points are undetermined");
				return VerbResult.Unchanged;
			}
			if (m_DefinedVertices == 0)
				return VerbResult.Continuing; // don't think this actually happen, but just in case
			if (Vertices[m_DefinedVertices].ApproxEqual(pt.Snapped))
				return VerbResult.Rejected;
			if (m_DefinedVertices >= 2 && !m_Closed && pt.Exact.ApproxEqual(Vertices[0]))
				// auto-close - complete will change to closed
				Vertices[m_DefinedVertices] = Vertices[0];
			else
				Vertices[m_DefinedVertices] = pt.Snapped;
			SetControlPoints(m_DefinedVertices - 1);
			if (m_Closed)
				SetControlPoints(0, 0);
			DiscardDerived();
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			if (position.Snapped.ApproxEqual(LastDefined))
				return VerbResult.Rejected;
			Float(position);
			FixVertex();
			return VerbResult.Continuing;
		}

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 2)
				return VerbResult.Rejected;
			// although in general this function might be called when the shape is already complete for this shape it should not (because Choose never returns completed)
			Debug.Assert(Vertices.Count == m_DefinedVertices + (m_Closed ? 2 : 1));
			Vertices.RemoveAt(m_DefinedVertices); // remove the postulated one off the end
			m_ControlPoints.RemoveRange(m_DefinedVertices * 2 - 2, 2);
			if (!m_Closed && Geometry.DirtyDistance(Vertices[Vertices.Count - 1], Vertices[0]) < CLOSURETHRESHOLD && m_DefinedVertices > 2) // last condition needed as one will be removed - although this point is close it needs to be left
			{
				//m_eLastSnap = SnapModes.Shape AndAlso
				// if end point close to start, then auto close, [XXXif in Shape snap]
				m_Closed = true;
				Vertices[Vertices.Count - 1] = Vertices[0];
				m_DefinedVertices -= 1; // because point back at start isn't counted in m_intDefinedVertices
				SetControlPoints(0); // must redo first ctrl point as closure affects this
			}
			else
				SetControlPoints(m_DefinedVertices - 1);
			m_Bounds = RectangleF.Empty;
			return VerbResult.Completed;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			if (m_DefinedVertices <= 1)
				return VerbResult.Destroyed;
			m_DefinedVertices -= 1;
			SetLength(m_DefinedVertices + 1);
			if (m_Closed)// the final, closing vertex has been renumbered.  This will have been deleted off the end of the list
				Vertices[Vertices.Count - 1] = Vertices[0];
			return VerbResult.Continuing;
		}

		public override bool VertexVerbApplicable(Functions.Codes code, Target target)
		{
			switch (code)
			{
				case Functions.Codes.AddVertex:return true;
				case Functions.Codes.RemoveVertex:return m_DefinedVertices > 2;
			}
			return base.VertexVerbApplicable(code, target);
		}

		public override VerbResult OtherVerb(EditableView.ClickPosition position, Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.AddVertex:
				case Functions.Codes.RemoveVertex:
					// Base functionality is OK, except that we also need to update the control points
					var result = base.OtherVerb(position, code);
					if (code == Functions.Codes.RemoveVertex)
						m_ControlPoints.RemoveRange(m_ControlPoints.Count - 2, 2);
					else
					{
						m_ControlPoints.Add(PointF.Empty);
						m_ControlPoints.Add(PointF.Empty);
					}
					this.SetControlPoints(0);
					return result;
			}
			return base.OtherVerb(position, code);
		}
		#endregion

		#region List of control points
		protected override void SetLength(int length)
		{
			if (m_Closed)
				length += 1;
			base.SetLength(length);
			// and adjust the control points array to match
			int required = (length - 1) * 2;
			while (m_ControlPoints.Count > required)
			{
				m_ControlPoints.RemoveAt(m_ControlPoints.Count - 1);
			}
			while (m_ControlPoints.Count < required)
			{
				m_ControlPoints.Add(Vertices[Vertices.Count - 1]);
			}
			DiscardDerived();
		}

		protected override void FixVertex()
		{
			//MyBase.FixVertex()
			if (!m_Closed)
			{
				Vertices.Add(Vertices[Vertices.Count - 1]);
				//  Vertices(Vertices.Count - 1) = Vertices(Vertices.Count - 2)
				m_ControlPoints.Add(Vertices[Vertices.Count - 2]);
				m_ControlPoints.Add(Vertices[Vertices.Count - 1]);
			}
			else
			{
				Vertices.Insert(Vertices.Count - 1, Vertices[Vertices.Count - 2]);
				// Vertices(Vertices.Count - 1) = Vertices(Vertices.Count - 2)
				m_ControlPoints.Insert(m_ControlPoints.Count - 2, Vertices[Vertices.Count - 3]);
				m_ControlPoints.Insert(m_ControlPoints.Count - 2, Vertices[Vertices.Count - 2]);
			}
			m_DefinedVertices += 1;
			DiscardDerived();
		}

		protected override void DiscardVertex()
		{
			if (!m_Closed)
			{
				Vertices.RemoveAt(Vertices.Count - 1);
				m_ControlPoints.RemoveRange(m_ControlPoints.Count - 2, 2);
			}
			else
			{
				Vertices.RemoveAt(Vertices.Count - 2);
				m_ControlPoints.RemoveRange(m_ControlPoints.Count - 4, 2);
			}
			m_DefinedVertices -= 1;
			DiscardDerived();
		}

		private const float CONTROLLENGTH = 0.4f; // length of control arm relative to distance P(n-1) -> P(n+1)

		protected virtual void SetControlPoints(int firstVertex, int lastVertex = int.MaxValue)
		{
			// intFirstVertex is the vertex at the beginning of the first curve that needs updating (during Float only the last couple will need updating)
			if (m_DefinedVertices <= 1)
			{
				// with two points we just get a straight line
				m_ControlPoints[0] = Geometry.MidPoint(Vertices[0], Vertices[1]);
				m_ControlPoints[1] = m_ControlPoints[0];
				if (m_Closed && m_ControlPoints.Count >= 4)
				{
					m_ControlPoints[2] = Geometry.MidPoint(Vertices[1], Vertices[2]);
					m_ControlPoints[3] = m_ControlPoints[2];
				}
			}
			else
			{
				if (!m_Closed)
				{
					// need to do all of the vertices except the first and last initially
					int vertex;
					for (vertex = Math.Max(firstVertex, 1); vertex <= Math.Min(lastVertex, Vertices.Count - 2); vertex++)
					{
						// the control points before and after this vertex are set based upon the vector between the fixed points before and after this one
						SizeF vector = Vertices[vertex - 1].VectorTo(Vertices[vertex + 1]);
						// szVector = Geometry.ScalarMultiply(szVector, 0.2) ' the total spread of the control points is one quarter of the vector between the fixed points
						// but we want to make the lengths of the grab handles different.  If the previous point was a long way off, and the following point
						// is quite close, we need to do the control points in a similar fashion (otherwise we are in danger of overshooting the following point)
						float sizePrevious = Geometry.DistanceBetween(Vertices[vertex - 1], Vertices[vertex]) + 1; // the +1 is just to avoid any possibility of division by zero (and any other strangeness in the extreme case)
						float sizeNext = Geometry.DistanceBetween(Vertices[vertex], Vertices[vertex + 1]) + 1;
						// the total spread of the control points is one quarter of the vector between the fixed points
						SizeF vectorBefore = vector.MultiplyBy(CONTROLLENGTH * sizePrevious / (sizePrevious + sizeNext));
						SizeF vectorAfter = vector.MultiplyBy(CONTROLLENGTH * sizeNext / (sizePrevious + sizeNext));
						m_ControlPoints[vertex * 2 - 1] = Vertices[vertex] - vectorBefore; // the control point before
						m_ControlPoints[vertex * 2] = Vertices[vertex] + vectorAfter;
					}
					// and then the control points at the very beginning and end are set using the start/end point and the following/previous control point
					if (firstVertex <= 1) // note that the end control points depend on a vertex one further out than usual
					{
						// (because they line up with the next control point, which depends on the vertex after that)
						SizeF vector = Vertices[0].VectorTo(m_ControlPoints[1]);
						vector = vector.MultiplyBy(CONTROLLENGTH / 2);
						m_ControlPoints[0] = Vertices[0] + vector;
					}
					if (lastVertex >= Vertices.Count - 2)
					{
						vertex = Vertices.Count - 1;
						SizeF finalVector = m_ControlPoints[vertex * 2 - 2].VectorTo(Vertices[vertex]); // control point after the preceding vertex to the final vertex
						finalVector = finalVector.MultiplyBy(CONTROLLENGTH / 2);
						m_ControlPoints[vertex * 2 - 2 + 1] = Vertices[vertex] - finalVector;
					}
				}
				else
				{
					// closed version
					for (int vertex = Math.Max(firstVertex, 0); vertex <= Math.Min(lastVertex, Vertices.Count - 1); vertex++)
					{
						// the control points before and after this vertex are set based upon the vector between the fixed points before and after this one
						int previous = vertex == 0 ? Vertices.Count - 2 : vertex - 1;
						int next = vertex == Vertices.Count - 1 ? 1 : vertex + 1;
						SizeF vector = Vertices[previous].VectorTo(Vertices[next]);
						// but we want to make the lengths of the grab handles different.  If the previous point was a long way off, and the following point
						// is quite close, we need to do the control points in a similar fashion (otherwise we are in danger of overshooting the following point)
						float sizePrevious = Geometry.DistanceBetween(Vertices[previous], Vertices[vertex]) + 1; // the +1 is just to avoid any possibility of division by zero (and any other strangeness in the extreme case)
						float sizeNext = Geometry.DistanceBetween(Vertices[vertex], Vertices[next]) + 1;
						// the total spread of the control points is one quarter of the vector between the fixed points
						SizeF vectorBefore = vector.MultiplyBy(CONTROLLENGTH * sizePrevious / (sizePrevious + sizeNext));
						SizeF vectorAfter = vector.MultiplyBy(CONTROLLENGTH * sizeNext / (sizePrevious + sizeNext));
						if (vertex > 0)
							m_ControlPoints[vertex * 2 - 1] = Vertices[vertex] - vectorBefore; // the control point before
						if (vertex < Vertices.Count - 1)
							m_ControlPoints[vertex * 2] = Vertices[vertex] + vectorAfter;
					}
				}
			}
			DiscardDerived();
		}

		#endregion

		#region Coordinates

		internal const float CLOSURETHRESHOLD = 8; // curve is closed if it finishes within this distance of the start

		protected override RectangleF CalculateBounds() => base.BoundsOfPathDetailed();

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			// this is an exception to the usual system where shapes can be treated as if filled sometimes.  I think for this shape it is probably more confusing
			return base.HitTestDetailed(clickPoint, scale, treatAsFilled && m_Closed);
		}

		private PointF GetPoint(int vertex, float T)
		{
			// Like the above function, but using a given arc from this object
			return Bezier.GetPoint(Vertices[vertex], Vertices[vertex + 1], m_ControlPoints[vertex * 2], m_ControlPoints[vertex * 2 + 1], T);
		}

		private void DiscardDerived()
		{
			// discards derived information.  Does not affect the ControlPoints, although these can be derived from the vertices
			m_Path?.Dispose();
			m_Path = null;
			DecacheArrowheads();
			m_Bounds = RectangleF.Empty;
		}

		protected override void CreatePath()
		{
			m_Path = new GraphicsPath();
			for (int vertex = 0; vertex <= Vertices.Count - 2; vertex++) // iterate through the vertices at the beginning of each curve
			{
				m_Path.AddBezier(Vertices[vertex], m_ControlPoints[vertex * 2], m_ControlPoints[vertex * 2 + 1], Vertices[vertex + 1]);
			}
			if (m_Closed)
				m_Path.CloseFigure();
		}

		private PointF[] GetSingleCurve(int index)
		{
			// gets the 4 standard Bezier points for a given section, in the standard order (end-control-control-end)
			return new PointF[] { Vertices[index], m_ControlPoints[index * 2], m_ControlPoints[index * 2 + 1], Vertices[index + 1] };
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			DiscardDerived();
			transformation.TransformPoints(m_ControlPoints);
		}

		internal override float[] GetRelevantAngles() => null;

		public override RectangleF MinimalBounds
		{
			get
			{
				RectangleF bounds = new RectangleF();
				for (int index = 0; index <= Vertices.Count - 1; index++)
				{
					Geometry.Extend(ref bounds, Vertices[index]);
				}
				return bounds;
			}
		}

		protected override PointF[] ArrowheadVector(bool end)
		{
			if (m_DefinedVertices < 2)
				return null;
			// we started using the vector from the last Bezier control point to the end, but quite often this control point is very near the end
			// which forces the arrowhead to be very small.  Using the midpoint of the two Bezier handles technically makes the vector slightly the wrong direction,
			// but looks better
			PointF[] points;
			if (end)
				points = new PointF[] { Geometry.MidPoint(m_ControlPoints[m_ControlPoints.Count - 1], m_ControlPoints[m_ControlPoints.Count - 2]), Vertices[Vertices.Count - 1] };
			else
				points = new PointF[] { Geometry.MidPoint(m_ControlPoints[0], m_ControlPoints[1]), Vertices[0] };
			// the above is making the arrowhead rather small when the last segment in the curve is quite short
			// trying adjusting it:
			points[0] = points[1] + points[1].VectorTo(points[0]).MultiplyBy(2);
			return points;
		}

		#endregion

		#region Targets, GrabSpots

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			// copied from Lined.GenerateDefaultTargets and modified
			List<Target> targets = new List<Target>();
			// add all of the vertices
			// note that using m_intDefinedVertices rather than Vertices.Count omits the last closing vertex if closed
			for (int index = 0; index <= m_DefinedVertices - 1; index++)
			{
				targets.Add(new Target(this, Vertices[index], Target.Types.Vertex, floating, Target.Priorities.Vertex, shapeIndex: index));
			}
			// check the lines
			for (int index = 0; index <= Vertices.Count - 2; index++)
			{
				// it is relatively quick to check if the point is close enough to the bounding rectangle for the arc
				RectangleF bounds = new RectangleF(Vertices[index].X, Vertices[index].Y, 0, 0);
				Geometry.Extend(ref bounds, Vertices[index + 1]);
				Geometry.Extend(ref bounds, m_ControlPoints[index * 2]);
				Geometry.Extend(ref bounds, m_ControlPoints[index * 2 + 1]);
				if (Geometry.DirtyDistance(floating.Centre, bounds) <= Target.MaximumInterestDistance)
				{
					float maximumError = MaximumTError(index) * 3; // doesn't need to be so precise, because the target point release be on the Bezier
																		 // said the user probably can't tell if it is a little bit short or long
					Target create = GenerateLineTargetOnOneBezier(floating, GetSingleCurve(index), targets, maximumError, index);
					if (create != null)
						targets.Add(create);
				}
			}
			return targets;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// Target.ShapeIndex is the index of the first vertex on this side
			// Target.ShapeParameter is the T-value at this point
			// we need to know what T-value to sweep through; this is estimated just assuming that the total curve is somewhat shorter than the distance between the base and control points
			int vertex = target.ShapeIndex;
			float basicLength = NominalLength(vertex);
			float sweep = Target.LINEDRAWLENGTH / basicLength; // the fraction of the curve we want to sweep through, approximately
			sweep = Math.Min(sweep, 0.05F); // but never one to sweep through a bit T range - we will draw the target as a line, so we need to keep a fairly short range to keep it approximately linear
			sweep = Math.Min(target.ShapeParameter, Math.Min(1 - target.ShapeParameter, sweep)); // but limit it so it does not go off the ends
			PointF start = GetPoint(vertex, target.ShapeParameter - sweep);
			PointF end = GetPoint(vertex, target.ShapeParameter + sweep);
			gr.DrawLine(pn, start, end);
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			// cannot use Lined.AddGrabSpotsForAllVertices because that would use Vertices.Count; one too many if closed
			for (int index = 0; index <= m_DefinedVertices - 1; index++)
			{
				list.Add(new GrabSpot(this, GrabTypes.SingleVertex, Vertices[index], index) { Prompts = new[] { new Prompt(ShapeVerbs.Choose, "MoveVertex_Independent", "MoveVertex") } });
			}
			if (Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics))
			{
				// add grab handles for the vertices.  These don't actually move in this shape; but will trigger an offer to degenerate into an GenericPath
				for (int index = 0; index <= m_DefinedVertices - (m_Closed ? 1 : 2); index++)
				{
					list.Add(new GrabSpot(this, GrabTypes.BezierInactive, m_ControlPoints[index * 2], index) { Focus = Vertices[index] });
					list.Add(new GrabSpot(this, GrabTypes.BezierInactive, m_ControlPoints[index * 2 + 1], index) { Focus = Vertices[index + 1] });
				}
			}
			base.AddStandardRotationGrabSpot(list);
			return list;
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			base.DoGrabMove(move);
			if (move.GrabType == GrabTypes.SingleVertex && m_Closed && move.ShapeIndex == 0)
			{
				// if moving the initial point, we must also move the closing point
				Vertices[Vertices.Count - 1] = Vertices[0];
			}
			this.SetControlPoints(0); //Math.Max(objMove.Index - 1, 0))
			m_Bounds = RectangleF.Empty;
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType != GrabTypes.SingleVertex)
				base.DoGrabAngleSnap(move);
		}

		internal override bool StartGrabMove(GrabMovement grab)
		{
			if (grab.SnapMode == SnapModes.Grid)
				grab.SnapMode = SnapModes.Off;
			return base.StartGrabMove(grab);
		}

		internal override List<Socket> GetSockets()
		{
			// basically like Lined.DefaultGetSockets, but we don't include the ones halfway down each line(!)
			// we also don't include an automatic one; although this might be useful, displayed it in the middle would be weird
			List<Socket> sockets = new List<Socket>();
			for (int vertex = 0; vertex <= Vertices.Count - 1; vertex++)
			{
				sockets.Add(new Socket(this, vertex * 2, Vertices[vertex]));
			}
			return sockets;

		}

		#endregion

		#region Support functions
		private float NominalLength(int arc)
		{
			// returns a nominal length for a given arc by just taking two thirds of total distance between the base points and control points
			return (Geometry.DistanceBetween(Vertices[arc], m_ControlPoints[arc * 2])
				+ Geometry.DistanceBetween(m_ControlPoints[arc * 2], m_ControlPoints[arc * 2 + 1])
				+ Geometry.DistanceBetween(m_ControlPoints[arc * 2 + 1], Vertices[arc + 1])) * 0.66f;
		}

		private float MaximumTError(int arc)
		{
			// How precisely T needs to be calculated when looking for targets or intersections within the given arc
			// it needs to be more precise if this arc is quite long
			return 1 / (25 + NominalLength(arc) * 1);
		}
		#endregion

		#region Graphics
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (Vertices.Count < 2)
				return;
			base.InternalDrawFromPath(gr, resources);
			if (!m_Closed)
				base.DrawArrowheads(resources);
		}

		#endregion

		#region Data
		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_ControlPoints);
		}

		public override void Load(DataReader reader)
		{
			// closed is set due to shape type
			base.Load(reader);
			m_ControlPoints = reader.ReadListPoints();
			if (m_Closed)
				m_DefinedVertices -= 1;
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Curve objCurve = (Curve)other;
			// these need to be copied for transformations, even though they can be derived and so aren't completely essential for undo
			m_ControlPoints.Clear();
			m_ControlPoints.AddRange(objCurve.m_ControlPoints);
			DiscardDerived();
		}

		#endregion

		#region Test code
#if DEBUG
		public void TestSplit()
		{
			List<PointF> newVertices = new List<PointF>();
			List<PointF> newControls = new List<PointF>();
			DiscardDerived();
			newVertices.Add(Vertices[0]);
			for (int index = 0; index <= Vertices.Count - 2; index++)
			{
				PointF[] aSplit = Bezier.Split(new PointF[] { Vertices[index], m_ControlPoints[index * 2], m_ControlPoints[index * 2 + 1], Vertices[index + 1] }, 0.3F);
				// the first point of the split Bezier has already been added to colNewVertices.  The control points and endpoints of each arc must be added...
				newVertices.Add(aSplit[3]);
				newVertices.Add(aSplit[6]);
				newControls.Add(aSplit[1]);
				newControls.Add(aSplit[2]);
				newControls.Add(aSplit[4]);
				newControls.Add(aSplit[5]);
			}
			Vertices = newVertices;
			m_ControlPoints = newControls;
		}

#endif

		#endregion
	}
}
