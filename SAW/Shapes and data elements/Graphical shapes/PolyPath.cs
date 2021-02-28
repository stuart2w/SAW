using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Abstract base class for shapes representing a GraphicsPath</summary>
	public abstract class PolyPath : Filled
	{
		// in SAW this is only used by GenericPath and could be merged;  the classes are separate for historical reasons (Splash has some other derivatives) and it's easier for me to leave them consistent

		#region Information

		protected PolyPath() => m_DefinedVertices = 2;// otherwise Lined will sulk

		protected override int FixedVerticesLength() => 0;

		public override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			if (m_Path == null)
				output.AppendLine("** No path defined **");
			else
			{
				for (int index = 0; index <= m_Path.PointCount - 1; index++)
				{
					output.Append("[" + index + "/" + m_Path.PathTypes[index] + "] ");
					switch (m_Path.PathTypes[index] & 7)
					{
						case 0: output.Append("Start"); break;
						case 1: output.Append("Line ends"); break;
						case 3: output.Append("Bezier to"); break;
						default: output.Append("???"); break;
					}
					output.Append(" @ ");
					PointDiagnostic(output, m_Path.PathPoints[index]);
				}
			}
		}

		internal override bool DefaultStylesApplied()
		{
			if (LineStyle == null)
				LineStyle = new LineStyleC();
			LineStyle.Colour = Color.Empty;
			return true;
		}

		public override AllowedActions Allows => base.Allows & ~AllowedActions.PermittedArea;
		protected override LineLogics LineLogic => LineLogics.UsePath;

		#endregion

		#region Verbs

		public override VerbResult Float(EditableView.ClickPosition pt) => VerbResult.Rejected;
		public override VerbResult Start(EditableView.ClickPosition position) => VerbResult.Rejected;
		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Rejected;
		public override VerbResult Choose(EditableView.ClickPosition position) => VerbResult.Rejected;
		public override VerbResult Complete(EditableView.ClickPosition position) => VerbResult.Rejected;
		public override VerbResult CompleteRetrospective() => VerbResult.Rejected;

		#endregion

		#region Data
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			PolyPath polyPath = (PolyPath)other;
			m_Path?.Dispose();
			m_Path = (GraphicsPath)polyPath.m_Path.Clone();
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			PointF[] points = reader.ReadListPoints().ToArray();
			int length = reader.ReadInt32();
			byte[] types = reader.ReadBytes(length);
			m_Path = new GraphicsPath(points, types, FillMode.Winding);
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Path.PathPoints);
			byte[] types = m_Path.PathTypes;
			writer.Write(types.Length);
			writer.Write(types);
		}

		#endregion

		#region Coordinates and Path
		// In the past this didn't really support closure - it was necessary to add an explicit line back to the beginning
		// it has been updated so it should support closure in the targets and intersections.
		// Targets only in GenericPath.  GrabSpots in this class, but Splatter can override them to block them again
		// Derived class should define CustomPen if it wants to draw the Bezier control points

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (m_Path == null)
				return;
			// can happen if there is an assertion being displayed during creation
			// doesn't use InternalDrawFromPath, cos it will EnsurePath
			if (resources.SingleElement != null)
			{
				IEnumerable<PathElement> elements = GetPathNearTarget(resources.SingleElement);
				GraphicsPath partialPath = PathElement.CreatePath(elements);
				gr.Path(partialPath, resources.MainPen);
				partialPath.Dispose();
				return;
			}
			if (WireFrame && IsWide)
			{
				if (resources.MainPen != null)
					gr.Path(m_ExteriorPath, resources.MainPen);
			}
			else if (resources.MainBrush != null || resources.MainPen != null)
				gr.Path(m_Path, resources.MainPen, resources.MainBrush);
		}

		protected override RectangleF CalculateBounds()
		{
			if (m_Path == null)
				return RectangleF.Empty;
			return base.BoundsOfPathDetailed();
		}

		public override RectangleF MinimalBounds => base.BoundsOfPathPoints();

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformPath(m_Path); // Lined does not transform the path, it just tries to discard it
			m_Bounds = RectangleF.Empty;
			m_Links = null; // we discard the automatic linkage if this shape is manually moved
		}

		public bool MatchesPath(GraphicsPath path)
		{
			// returns true if this shape effectively matches the given path.  The complication is that the paths might start in different places
			// I think they can only match if they are the same length
			var points = path.PathPoints;
			var myPoints = m_Path.PathPoints;
			var types = path.PathTypes;
			var myTypes = m_Path.PathTypes;
			if (points.Length != myPoints.Length)
				return false;
			// try at every offset
			for (int offset = 0; offset <= myPoints.Length - 1; offset++)
			{
				bool matches = true;
				for (int index = 0; index <= myPoints.Length - 1; index++)
				{
					int otherIndex = (index + offset) % myPoints.Length;
					if (!myPoints[index].ApproxEqual(points[otherIndex]) ||
						(myTypes[index] & PATHTYPEMASK) != (types[otherIndex] & PATHTYPEMASK) && index > 0 && otherIndex > 0)
					{
						// the And 7 filters out the start of figure/end of figure markers, just leaving the actual line/Bezier selection
						// AndAlso intIndex > 0 AndAlso intOtherIndex > 0 is needed because the first point in each path always has type 0
						matches = false;
						break;
					}
				}
				if (matches)
					return true;
			}
			return false;
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			if ((Allows & AllowedActions.TransformRotate) > 0)
				base.AddStandardRotationGrabSpot(list);
			base.AddBoundingGrabSpots(list, scale);
			return list;
		}

		protected override void DiscardPath()
		{
			// path should not be automatically disposed - it defines this shape!
			m_ExteriorPath?.Dispose();
			m_ExteriorPath = null;
		}

		private protected override IEnumerable<PathElement> GetPathNearTarget(Target target)
		{
			// target.ShapeIndex is now the point index at the start of the path element - not the index of the path element (ie it goes 0,3,6 for lots of bezier curves)
			PathElement[] elements = PathElement.WithinPath(m_Path, true, true).ToArray();
			// find the path element with this shape index
			int index = 0;
			while (elements[index].Index < target.ShapeIndex)
				index++;
			// index could be off the end of the path element array for the final vertex
			switch (target.Type)
			{
				case Target.Types.Line:
					yield return elements[index];
					break;
				case Target.Types.Vertex:
					if (index > 0)
						yield return elements[index - 1];
					else if (Closed())
						yield return elements.Last();
					if (index < elements.Count())
						yield return elements[index];
					else if (Closed())
						yield return elements[0];
					break;
				default: throw new ArgumentException("Only Line or Vertex targets permitted in GetPathNearTarget");
			}
		}

		#endregion

		#region Targets

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> targets = new List<Target>();

			PathElement last = PathElement.Invalid;
			PointF lastEnd = PointF.Empty; // because the points within the PathElements are re-used
			foreach (PathElement element in PathElement.WithinPath(m_Path))
			{
				targets.Add(new Target(this, element.Points[0], Target.Types.Vertex, floating, Target.Priorities.Vertex) { ShapeIndex = element.Index });
				if (element.IsClosure && element.IsDegenerate)
				{
					last = element;
					lastEnd = last.Endpoint;
					continue; // Has no length
				}
				if (element.Index > 0 && element.IsStart)
				{
					// We may need to add the point at the end of the last line of any previous figure, if it was not closed back to the beginning
					if (!last.IsClosure && last.Index > 0) // Second condition partly to check if it was assigned at all
						targets.Add(new Target(this, lastEnd, Target.Types.Vertex, floating, Target.Priorities.Vertex) { ShapeIndex = last.Index + (last.Type == PATHLINE ? 1 : 3) });
				}
				switch (element.Type)
				{
					case PATHLINE:
						Target target = GenerateTargetOnOneLine(this, floating, element.Points[0], element.Points[1], targets, element.Index);
						if (target != null)
							targets.Add(target);
						break;
					case PATHBEZIER:
						// First check that we are somewhere near this curve (the actual generate target is fairly slow)
						if (Geometry.DirtyDistance(floating.Centre, Bezier.BoundingRect(element.Points)) <= Target.MaximumInterestDistance)
						{
							Target create = GenerateLineTargetOnOneBezier(floating, element.Points, targets, 0.01F, element.Index);
							if (create != null)
								targets.Add(create);
						}
						break;
					default:
						Debug.Fail("Unexpected PolyPath path type");
						return targets; // because we don't know how much to increment index
				}
				last = element;
				lastEnd = last.Endpoint;
			}
			// We may need to add the point at the end of the last line of any previous figure, if it was not closed back to the beginning
			if (!last.IsClosure && last.Index > 0) // Second condition partly to check if it was assigned at all
				targets.Add(new Target(this, last.Endpoint, Target.Types.Vertex, floating, Target.Priorities.Vertex, shapeIndex: last.Index + (last.Type == PATHLINE ? 1 : 3)));
			// We never add the centre of the shape
			return targets;

		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// Target.ShapeIndex is the index of the first vertex on this segment
			PointF[] points = m_Path.PathPoints;
			int index = target.ShapeIndex;
			if (index == m_Path.PointCount - 1)
			{
				// Must be implicit closure
				Debug.Assert((m_Path.PathTypes.Last() & PATHCLOSUREFLAG) > 0);
				DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, points[index], points[0]);
			}
			else
			{
				if ((m_Path.PathTypes[index + 1] & PATHTYPEMASK) == PATHLINE)
				{
					DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, points[index], points[index + 1]);
				} // based on version in Curve
				else if ((m_Path.PathTypes[index + 1] & PATHTYPEMASK) == PATHBEZIER)
				{
					PointF[] P = { points[index], points[index + 1], points[index + 2], points[index + 3] };
					float basicLength = Bezier.NominalLength(P);
					float sweep = Target.LINEDRAWLENGTH / basicLength; // the fraction of the curve we want to sweep through, approximately
					sweep = Math.Min(sweep, 0.05F); // but never one to sweep through a bit T range - we will draw the target as a line, so we need to keep a fairly short range to keep it approximately linear
					sweep = Math.Min(target.ShapeParameter, Math.Min(1 - target.ShapeParameter, sweep)); // but limit it so it does not go off the ends
					PointF start = Bezier.GetPoint(P, target.ShapeParameter - sweep);
					PointF end = Bezier.GetPoint(P, target.ShapeParameter + sweep);
					gr.DrawLine(pn, start, end);
				}
			}
		}

		internal override List<Socket> GetSockets()
		{
			List<Socket> list = new List<Socket>();
			foreach (var element in PathElement.WithinPath(m_Path, true)) // Particularly important that closure is ignored if short.  Many of the paths generated by Splash
			{
				// have an explicit line segment closing them but might also be flagged with the figure closer flag at the end, which is actually mostly redundant
				list.Add(new Socket(this, element.Index, element.Points[0]));
				if (element.Type == PATHLINE) // ignore if very short
				{
					if (Geometry.DirtyDistance(element.Points[0], element.Points[1]) > 3)
						list.Add(new Socket(this, element.Index + 10000, Geometry.MidPoint(element.Points[0], element.Points[1])));
				} // No sockets added part way around curves
				else if (element.Type == PATHBEZIER)
				{
				}
				else
					throw new InvalidOperationException();
			}
			return list;
		}

		internal override SizeF SocketExitVector(int index)
		{
			// this roughly copied from the Lined version, but meaning of index changes.
			// angular one not exactly right for curves - it's taking the first grab handle as the line vector - but close enough
			if (index < 0)
				return new SizeF(1, 0);
			// just in case
			// index is path index, but plus 10000 if mid point of line
			PointF[] points = m_Path.PathPoints;
			if (index >= 10000)
			{
				index -= 10000;
				return LineSocketExitVector(points[index], points[(index + 1) % points.Length]);
			}
			else
			{
				// we want to exit at an angle that bisects the corner
				// get the two vectors away from this corner, with an arbitrary length
				SizeF sizeA = points[index].VectorTo(points[(index + 1) % points.Length]);
				PointF previous;
				if (index == 0)
					previous = points[points.Length - 2];
				else
					previous = points[index - 1];
				SizeF sizeB = points[index].VectorTo(previous);
				sizeA = sizeA.ChangeLength(1); // if reset them both to the same length then adding or averaging will give the vector into this vertex
				sizeB = sizeB.ChangeLength(1);
				// and we now need to return the vector in the opposite direction, outwards from the shape
				SizeF sizeExit = new SizeF(-sizeA.Width - sizeB.Width, -sizeA.Height - sizeB.Height);
				// the problem is if the lines were effectively parallel (which makes szExit 0,0) - treat the pair of lines as one
				if (sizeExit.Length() < Geometry.NEGLIGIBLE)
					return LineSocketExitVector(points[index], previous);
				return new SizeF(-sizeA.Width - sizeB.Width, -sizeA.Height - sizeB.Height);
			}
		}

		internal override PointF SocketPosition(int index)
		{
			// index is path index, but plus 10000 if mid point of line
			PointF[] points = m_Path.PathPoints;
			if (index >= 10000)
			{
				index -= 10000;
				return Geometry.MidPoint(points[index], points[(index + 1) % points.Length]);
			}
			else
				return points[index];
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			List<UserSocket> list = new List<UserSocket>();
			foreach (PathElement element in PathElement.WithinPath(m_Path, true))
			{
				// All of the actual "vertices" count as snappable points
				list.Add(UserSocket.CreateForPoint(element.Points[0]));
			}
			return list;
		}

		#endregion

	}

	/// <summary>A generic shape represented by a GraphicsPath.  This was originally only created by merging shapes, but is becoming progressively more functional.
	/// These still cannot be drawn directly or appear in the tools palette</summary>
	public class GenericPath : PolyPath
	{
		// This has no need to keep links to the original shapes - it can't anyway, because the original shapes are deleted

		public GenericPath() // version for serialisation etc
		{ }

		public GenericPath(GraphicsPath path)
		{
			m_Path = path;
			FillStyle.SetDefaults();
			LineStyle.SetDefaults();
		}

		#region Verbs

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			PointF pt = position.Snapped;
			Debug.Assert(LineStyle != null);
			LineStyle.SetDefaults();
			m_Path = new GraphicsPath(new PointF[] { pt, pt, pt, pt }, new byte[] { PATHSTART, PATHBEZIER, PATHBEZIER, PATHBEZIER });
			//if ((Allows & AllowedActions.Arrowheads) > 0)
			//{
			//	StartArrowhead.SetDefaults();
			//	EndArrowhead.SetDefaults();
			//}
			//Debug.Assert((Flags & GeneralFlags.ProtectBounds) == 0);
			m_DefinedVertices = 2;
			m_Bounds = new RectangleF(pt.X, pt.Y, 0, 0);
			return VerbResult.Continuing;
		}

		public override VerbResult Float(EditableView.ClickPosition pt)
		{
			PointF[] points = m_Path.PathPoints;
			byte[] types = m_Path.PathTypes;
			DoFloat(ref types, ref points, pt);
			m_Path.Dispose();
			m_Path = new GraphicsPath(points, types);
			m_Bounds = RectangleF.Empty;
			//Debug.WriteLine("End float, count=" + types.Length);
			return VerbResult.Continuing;
		}

		/// <summary>Does the main logic of float.  Extracted so that it can be used within Choose etc without creating a new path more than once</summary>
		private void DoFloat(ref byte[] types, ref PointF[] points, EditableView.ClickPosition pt)
		{
			bool wantBezier = Control.ModifierKeys != Keys.Shift;// note requires exactly shift, and not control or alt
			if (wantBezier)
			{
				if (!IsBezier(types))
					MakeBezier(ref types, ref points);
			}
			else
			{
				if (IsBezier(types))
					MakeLine(ref types, ref points);
			}
			points[points.Length - 1] = pt.Snapped;
			if (wantBezier)
				SetLastBezierControls(types, points);
			DecacheArrowhead(true);
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			PointF[] points = m_Path.PathPoints;
			byte[] types = m_Path.PathTypes;
			int newLength = types.Length - (IsBezier(types) ? 3 : 1);
			if (newLength <= 1)
				return VerbResult.Destroyed;
			Array.Resize(ref types, newLength);
			Array.Resize(ref points, newLength);
			DecacheArrowhead(true);
			m_Path.Dispose();
			m_Path = new GraphicsPath(points, types);
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			PointF[] points = m_Path.PathPoints;
			byte[] types = m_Path.PathTypes;
			DoFloat(ref types, ref points, position);
			bool wantBezier = Control.ModifierKeys != Keys.Shift;
			int oldLast = points.Length - 1; // index of current last point
			int newLength = points.Length + (wantBezier ? 3 : 1);
			Array.Resize(ref points, newLength);
			Array.Resize(ref types, newLength);
			points[newLength - 1] = points[oldLast];
			if (!wantBezier)
				types[newLength - 1] = PATHLINE;
			else
			{
				types[newLength - 1] = PATHBEZIER;
				types[newLength - 2] = PATHBEZIER;
				types[newLength - 3] = PATHBEZIER;
				SetLastBezierControls(types, points);
			}
			m_Path.Dispose();
			m_Path = new GraphicsPath(points, types);
			return VerbResult.Continuing;
		}

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			Choose(position);
			return CompleteRetrospective();
		}

		public override VerbResult CompleteRetrospective()
		{
			// discards the last floating one
			PointF[] points = m_Path.PathPoints;
			byte[] types = m_Path.PathTypes;
			int newLength = types.Length - (IsBezier(types) ? 3 : 1);
			if (newLength <= 1)
				return VerbResult.Destroyed; // shouldn't be the case as Choose should have been triggered before this
			Array.Resize(ref types, newLength);
			Array.Resize(ref points, newLength);
			m_Path.Dispose();
			m_Path = new GraphicsPath(points, types);
			return VerbResult.Completed;
		}

		internal override bool DefaultStylesApplied()
		{ // PolyPath has some custom logic that we don't want
			return false;
		}

		#endregion

		#region Editing support during verbs - all fns act on last segment only

		/// <summary>True if last segment is bezier </summary>
		private static bool IsBezier(byte[] types)
		{
			return ((types[types.Length - 1] & PATHTYPEMASK) == PATHBEZIER);
		}

		/// <summary>Changes arrays so that last segment is a bezier.  Assumes it is currently line</summary>
		private static void MakeBezier(ref byte[] types, ref PointF[] points)
		{
			Debug.Assert(!IsBezier(types));
			Array.Resize(ref types, types.Length + 2);
			Array.Resize(ref points, points.Length + 2);
			int last = types.Length - 1; // index of last point
			types[last] = PATHBEZIER;
			types[last - 1] = PATHBEZIER;
			types[last - 2] = PATHBEZIER;// changing the existing point
			points[last] = points[last - 2]; // actual end point now in new spot in list
			SetLastBezierControls(types, points);
		}

		private static void MakeLine(ref byte[] types, ref PointF[] points)
		{
			Debug.Assert(IsBezier(types));
			int last = types.Length - 3; // index of NEW last point
			points[last] = points[last + 2]; // copy current end point back to last position which will remain in array
			Array.Resize(ref types, types.Length - 2);
			Array.Resize(ref points, points.Length - 2);
			types[last] = PATHLINE;
		}

		/// <summary>Updates the 2 control handles within the last segment.  Ignored if last is line </summary>
		private static void SetLastBezierControls(byte[] types, PointF[] points)
		{
			if (!IsBezier(types))
				return;
			int last = types.Length - 1; // index of last point
										 // first handle is based on the vector from the previous REAL point (ie not grab handle) before this segment to the end point of this segment
										 // we must also reset the handle from the bezier, if it is a bezier
			if (types.Length == 4)
			{ // this is the only section, do as line for the moment
				points[last - 1] = Geometry.MidPoint(points[last - 3], points[last]);
				points[last - 2] = points[last - 1];
				return;
			}
			bool previousBezier = types[last - 3] == PATHBEZIER;
			int previousPoint = last - 3 - (previousBezier ? 3 : 1); // index of point at start of previous section
			SizeF longVector = points[previousPoint].VectorTo(points[last]);
			// this gives the direction for the vector for the grab handles.  The length we just set to 1/4 of the distance of this segment
			SizeF forwardVector = longVector.ChangeLength(Geometry.DistanceBetween(points[last - 3], points[last]) / 4);
			points[last - 2] = points[last - 3] + forwardVector; // sets the first grab handle in this segment; the forwards one from the start point
																 // the other grab handle is setto point to this
			points[last - 1] = Geometry.MidPoint(points[last], points[last - 2]);
			// and finally, if previous was bezier, then update the last grab handle in that previous segment, so that it forms a smooth join with the first one in this segment
			if (!previousBezier)
				return;
			SizeF reverseVector = longVector.ChangeLength(Geometry.DirtyDistance(points[last - 3], points[last - 6])); // actually this points forwards in direction of second segment, so subtracted below
			points[last - 4] = points[last - 3] - reverseVector;
		}

		#endregion

		#region Information

		public override Shapes ShapeCode => Shapes.GenericPath;

		public override AllowedActions Allows => base.Allows & ~AllowedActions.ConvertToPath; // Because it already is!

		#endregion

		public void DeriveStyles(IEnumerable<Shape> shapesUsed)
		{
			// the default styles from the GUI should already have been applied to the shape.  This will use the styles in the original objects if they don't conflict
			Color fill = Color.Empty;
			// meaning nothing used yet.  If still this at the end then Empty will be used for this shape
			// e.g. if we merge a load of lines, they would not be filled
			Color line = Color.Empty;
			DashStyle dash = (DashStyle)(-1);
			//  all of the genuine values are >= 0
			// fillpattern is always solid, unless the colour is empty.  Doesn't seem worth trying to check all the patterns
			float lineWidth = -1;
			Dictionary<Parameters, Parameters> conflicted = new Dictionary<Parameters, Parameters>(); // list of parameters where we have conflicts
			foreach (Shape shape in shapesUsed)
			{
				FillStyleC fillStyle = (FillStyleC)shape.StyleObjectForParameter(Parameters.FillColour);
				if (fillStyle != null && fillStyle.Colour.A > 0 && !conflicted.ContainsKey(Parameters.FillColour))
				{
					if (fill.IsEmpty)
						fill = fillStyle.Colour;
					else if (!fill.Equals(fillStyle.Colour))
						conflicted.Add(Parameters.FillColour, Parameters.FillColour);
				}
				LineStyleC lineStyle = (shape.StyleObjectForParameter(Parameters.LineColour) as LineStyleC);
				if (lineStyle != null && lineStyle.Colour.A > 0 && !conflicted.ContainsKey(Parameters.LineColour))
				{
					if (line.IsEmpty)
						line = lineStyle.Colour;
					else if (!line.Equals(lineStyle.Colour))
						conflicted.Add(Parameters.LineColour, Parameters.LineColour);
				}
				lineStyle = (shape.StyleObjectForParameter(Parameters.LinePattern) as LineStyleC);
				if (lineStyle != null && !conflicted.ContainsKey(Parameters.LinePattern))
				{
					if (dash < 0)
						dash = lineStyle.Pattern;
					else if (lineStyle.Pattern != dash)
						conflicted.Add(Parameters.LinePattern, Parameters.LinePattern);
				}
				lineStyle = (shape.StyleObjectForParameter(Parameters.LineWidth) as LineStyleC);
				if (lineStyle != null && !conflicted.ContainsKey(Parameters.LineWidth))
				{
					if (lineWidth < 0)
						lineWidth = lineStyle.Width;
					else if (lineStyle.Width != lineWidth)
						conflicted.Add(Parameters.LineWidth, Parameters.LineWidth);
				}
			}
			if (!conflicted.ContainsKey(Parameters.FillColour))
				FillStyle.Colour = fill;
			if (!conflicted.ContainsKey(Parameters.LineColour))
				LineStyle.Colour = line;
			if (!conflicted.ContainsKey(Parameters.LinePattern))
				LineStyle.Pattern = dash;
			if (!conflicted.ContainsKey(Parameters.LineWidth))
				LineStyle.Width = lineWidth;
		}

		#region Editing: GrabSpots and vertex editing
		// The GrabSpots for the handles have ShapeParameter = -1/+1 for handled before or after a vertex
		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			var list = base.GetGrabSpots(scale);
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics))
				return list;
			// can move individuals vertices and Bezier control points
			// cannot use PathElement.Within because we need to do stuff at the start of figures
			var index = 0;
			byte[] types = m_Path.PathTypes;
			PointF[] points = m_Path.PathPoints;
			int figureStart = 0;
			list.Add(new GrabSpot(this, GrabTypes.SingleVertex, points[index], index));
			while (index < types.Count() - 1)
			{
				switch (types[index + 1] & PATHTYPEMASK)
				{
					case PATHSTART:
						index += 1;
						figureStart = index;
						break;
					case PATHLINE:
						index += 1;
						break;
					case PATHBEZIER:
						list.Add(new GrabSpot(this, GrabTypes.Bezier, points[index + 1], index + 1) { Focus = points[index], ShapeParameter = 1 });
						list.Add(new GrabSpot(this, GrabTypes.Bezier, points[index + 2], index + 2) { Focus = points[index + 3], ShapeParameter = -1 });
						index += 3;
						break;
					default:
						Debug.Fail("Invalid path type");
						return list;
				}
				// Add the vertex at the end of this line/curve, UNLESS it has a closure back to the start point which is in the same place
				if ((types[index] & PATHCLOSUREFLAG) == 0 || !points[index].ApproxEqual(points[figureStart]))
					list.Add(new GrabSpot(this, GrabTypes.SingleVertex, points[index], index));
			}
			return list;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			byte[] types;
			PointF[] points;
			switch (move.GrabType)
			{
				case GrabTypes.Bezier:
					points = m_Path.PathPoints;
					types = m_Path.PathTypes;
					int index = move.ShapeIndex;
					Utilities.ErrorAssert(move.ShapeParameter != 0); // should be flagged -1/+1 to show whether it is the handle before or after a vertex
					int other = -1; // the index of the handle the other side of the vertex, if any (will be null if at the end of an open figure, or if the other side is a straight line)
					int vertex = -1; // the index of the vertex; only defined if intOther is defined
					if (move.ShapeParameter == 1)
					{
						vertex = index - 1;
						other = PreviousHandle(vertex);
					}
					else if (move.ShapeParameter == -1)
					{
						if (index == types.Length - 2 || (types[index + 2] & PATHTYPEMASK) == PATHSTART)
						{
							// This curve is at the end of a figure
							vertex = PathElement.FindFigureStart(types, index - 2);
							if ((types[index + 1] & PATHCLOSUREFLAG) > 0 && points[index + 1].ApproxEqual(points[vertex]) && (types[vertex + 1] & PATHTYPEMASK) == PATHBEZIER)
							{
								// Has a degenerate closure back to another curve which can be adjusted
								other = vertex + 1;
							}
						}
						else
						{
							vertex = index + 1;
							if ((types[index + 2] & PATHTYPEMASK) == PATHBEZIER)
								other = vertex + 1;
						}
					}
					if (other >= 0)
					{
						// the other handle only needs to be adjusted if they currently form a line
						var szA = points[other].VectorTo(points[vertex]);
						var szB = points[vertex].VectorTo(points[index]);
						if (Math.Abs(szA.CrossProduct(szB)) > Geometry.NEGLIGIBLEANGLE)
							other = -1; // handle don't currently form a line; no need to keep them smooth
					}
					PointF newPoint = points[index];
					move.Transform.TransformPoint(ref newPoint);
					// in this case there is very little sanity checking.  The user is allowed to place points on top of each other
					points[index] = newPoint;
					if (other >= 0)
					{
						// the length of the other handle is kept the same, but the angle is changed to the opposite of this one
						var sngLength = points[vertex].VectorTo(points[other]).Length();
						var sngAngle = points[vertex].VectorTo(points[index]).VectorAngle(); // Note that this VectorTo this to the other handle
						points[other] = points[vertex] + Geometry.ScalarToVector(-sngLength, sngAngle); // uses negative length rather than rotating the angle through 180°
					}
					ChangePath(types, points);
					break;
				case GrabTypes.SingleVertex:
					// Very similar, but we also want to move any previous or following grab handles (otherwise it is very weird!)
					points = m_Path.PathPoints;
					types = m_Path.PathTypes;
					var subsequent = PointF.Empty; // These will be left as PointF.Empty to indicate if they are not to be moved.  They may be present independently; i.e. either one might be present by itself
					int previous = -1; // index of the previous Bezier control (might not be the one before the grab handle due to closure)
					int alsoMove = -1; // second point which needs to be moved to the same place (see below)
					if ((types[move.ShapeIndex] & PATHTYPEMASK) == PATHBEZIER)
					{
						previous = move.ShapeIndex - 1;
					} // Also need to check for the special case if this figure is closed at the end with a degenerate closure.  In which case we need to move both points
					else if ((types[move.ShapeIndex] & PATHTYPEMASK) == PATHSTART)
					{
						// and possibly the previous point will be the one before the closure
						int end = PathElement.FindFigureEnd(types, move.ShapeIndex);
						if ((types[end] & PATHCLOSUREFLAG) > 0 && points[end].ApproxEqual(points[move.ShapeIndex]))
						{
							// Does contain a final point which is returned to the start point (presumably because it is a curve)
							alsoMove = end;
							if ((types[end] & PATHTYPEMASK) == PATHBEZIER) // probably true I would expect
								previous = end - 1;
						}
					}
					if (move.ShapeIndex < points.Count() - 1 && (types[move.ShapeIndex + 1] & PATHTYPEMASK) == PATHBEZIER)
						subsequent = points[move.ShapeIndex + 1];
					PointF create = points[move.ShapeIndex];
					move.Transform.TransformPoint(ref create);
					// in this case there is very little sanity checking.  The user is allowed to place points on top of each other
					points[move.ShapeIndex] = create;
					if (alsoMove >= 0)
						points[alsoMove] = create;
					if (previous >= 0)
					{
						PointF previousPoint = points[previous];
						move.Transform.TransformPoint(ref previousPoint);
						points[previous] = previousPoint;
					}
					if (!subsequent.IsEmpty)
					{
						move.Transform.TransformPoint(ref subsequent);
						points[move.ShapeIndex + 1] = subsequent;
					}
					ChangePath(types, points);
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
		}

		public override bool VertexVerbApplicable(Functions.Codes code, Target target)
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics))
				return false; // Even Add and Remove disabled for this shape, although allow other shapes
							  // because we only allow vertex movement at all in this particular shape with advanced graphics
			switch (code)
			{
				case Functions.Codes.RemoveVertex:
				case Functions.Codes.AddVertex:
					return true;
				case Functions.Codes.SmoothVertex: // Requires curves before and after
				case Functions.Codes.CornerVertex:
					byte[] types = m_Path.PathTypes;
					int previous = PreviousHandle(target.ShapeIndex);
					if (previous < 0)
						return false;
					// not a curve leading up to this
					// If this is at a closure it can be rejected, because if the closure is degenerate the start vertex of the figure would have been used
					if (target.ShapeIndex >= types.Length - 1 || (types[target.ShapeIndex + 1] & PATHTYPEMASK) != PATHBEZIER)
						return false; // Not a curve leading away from this
					int following = target.ShapeIndex + 1;
					PointF[] points = m_Path.PathPoints;
					SizeF sizeA = points[previous].VectorTo(points[target.ShapeIndex]);
					SizeF sizeB = points[target.ShapeIndex].VectorTo(points[following]);
					bool straight = Math.Abs(sizeA.CrossProduct(sizeB)) < Geometry.NEGLIGIBLEANGLE;
					return (code == Functions.Codes.SmoothVertex) ^ straight;
				case Functions.Codes.ConvertToBezier:
					if (target.Type != Target.Types.Line)
						return false;
					PathElement element1 = PathElement.GetElement(m_Path, target.ShapeIndex);
					return element1.IsLine;
				case Functions.Codes.ConvertToLine:
					if (target.Type != Target.Types.Line)
						return false;
					PathElement element = PathElement.GetElement(m_Path, target.ShapeIndex);
					return element.IsBezier;
			}
			return base.VertexVerbApplicable(code, target);
		}

		/// <summary>Returns the index of the point for the grab handle leading up to this vertex, or -1 if inappropriate in any way</summary>
		private int PreviousHandle(int vertex)
		{
			byte[] types = m_Path.PathTypes;
			if (vertex >= types.Length - 1)
				return -1; // is at the end
			if ((types[vertex + 1] & PATHTYPEMASK) != PATHBEZIER)
				return -1; // it is not a Bezier leading away from this vertex
			if ((types[vertex] & PATHTYPEMASK) == PATHSTART)
			{
				// This is the start of a figure
				int end = PathElement.FindFigureEnd(types, vertex);
				if ((types[end] & PATHCLOSUREFLAG) == 0)
					return -1; // path is not closed so this vertex is at the beginning
				if (!m_Path.PathPoints[vertex].ApproxEqual(m_Path.PathPoints[end]))
					return -1; // Is a genuine closure line, not a Bezier leading up to this point
				return end - 1; // closure flag doesn't create a line with any length.  Previous curve finishes coincident with this one so can use the last grab handle from that curve
			}
			if ((types[vertex] & PATHTYPEMASK) == PATHLINE)
				return -1; // it is a line leading up to this
			else if ((types[vertex] & PATHTYPEMASK) == PATHBEZIER)
				return vertex - 1;
			else
			{
				Utilities.LogSubError("Inappropriate path type");
				return -1;
			}
		}

		public override VerbResult OtherVerb(EditableView.ClickPosition position, Functions.Codes code)
		{
			Target target = position.Page.SelectedPath;
			switch (code)
			{
				case Functions.Codes.RemoveVertex:
					RemoveVertex(target.ShapeIndex);
					return VerbResult.Continuing;
				case Functions.Codes.SmoothVertex: // Requires curves before and after
				case Functions.Codes.CornerVertex:
					var previous = PreviousHandle(target.ShapeIndex);
					if (previous < 0)
						return VerbResult.Rejected; // not a curve leading up to this
													// If this is at a closure it can be rejected, because if the closure is degenerate the start vertex of the figure would have been used
					if (target.ShapeIndex >= m_Path.PointCount - 1 || (m_Path.PathTypes[target.ShapeIndex + 1] & PATHTYPEMASK) != PATHBEZIER)
						return VerbResult.Rejected; // Not a curve leading away from this
					var following = target.ShapeIndex + 1;
					var points = m_Path.PathPoints;
					var sizeA = points[previous].VectorTo(points[target.ShapeIndex]);
					var sizeB = points[target.ShapeIndex].VectorTo(points[following]);
					if (code == Functions.Codes.CornerVertex)
					{
						sizeA = sizeB.RotateBy(-Geometry.ANGLE45 / 2); // Any angle sufficient to stop it being a straight line.  It could be moved less than this, but I think it is helpful user can see it move as well (?)
						sizeB = sizeB.RotateBy(Geometry.ANGLE45 / 2);
						points[previous] = points[target.ShapeIndex] - sizeA; // - because the vector was measured in the other direction
						points[following] = points[target.ShapeIndex] + sizeB;
					}
					else
					{
						float angle = (sizeA + sizeB).VectorAngle(); // Angle from first handle to second.  They are both repositioned with their existing lengths using this angle away from the vertex
						points[previous] = points[target.ShapeIndex] + Geometry.ScalarToVector(-sizeA.Length(), angle);
						points[following] = points[target.ShapeIndex] + Geometry.ScalarToVector(sizeB.Length(), angle);
					}
					ChangePath(m_Path.PathTypes, points);
					return VerbResult.Continuing;
				case Functions.Codes.ConvertToLine:
				case Functions.Codes.ConvertToBezier:
					ConvertElementType(target.ShapeIndex, code == Functions.Codes.ConvertToBezier);
					return VerbResult.Continuing;
				case Functions.Codes.AddVertex:
					InsertVertex(target);
					return VerbResult.Continuing;
				default:
					return base.OtherVerb(position, code);
			}
		}

		private void RemoveVertex(int index)
		{
			// This works from the PathElement objects, much easier to manipulate these, and the efficiency of converting back and forth is irrelevant here
			// first split them up into a separate list for each figure; so that we can work on just the one figure
			List<List<PathElement>> figures = new List<List<PathElement>>();
			List<PathElement> figure = null;
			int figureIndex = -1; // the index of the figure that we will be working on
			int elementIndex = -1; // and the index of the element within the figure
			foreach (PathElement element in PathElement.WithinPath(m_Path, false, true))
			{
				if (element.IsStart)
				{
					figure = new List<PathElement>();
					figures.Add(figure);
				}
				if (element.Index == index)
				{
					figureIndex = figures.Count - 1;
					elementIndex = figure.Count; // not -1 because we haven't added this item yet
				}
				figure.Add(element);
			}
			bool done = false;
			if (figureIndex < 0)
			{
				// Could be removing the last vertex in an closed figure
				figure = (from f in figures where f.Last().EndIndex == index select f).FirstOrDefault();
				if (figure == null)
					throw new InvalidOperationException("Did not find element index");
				figureIndex = figures.IndexOf(figure);
				figure.RemoveAt(figure.Count - 1);
				done = true;
			}

			figure = figures[figureIndex];
			if (figure.Count == 2)
				figures.RemoveAt(figureIndex); // removes this figure completely; it has collapsed
			else if (!done)
			{
				var closed = figure.Last().IsClosure;
				var addClosure = false; // True if a closure needs to be added back at the end, usually because it has been removed first
				var previous = elementIndex - 1;
				if (elementIndex == 0) // removing first item
				{
					previous = figure.Count - 1;
					if (!closed) // then just remove the first vertex - simples!
					{
						figure.RemoveAt(0);
						done = true;
					}
					else if (figure.Last().IsDegenerate)
					{
						// If the closure is largely spurious, i.e. just a flag, it can be removed and added back afterwards.  The lines before and after this can be treated as normal
						figure.RemoveAt(figure.Count - 1);
						previous -= 1;
						addClosure = true;
					}
					else if (figure[0].IsLine)
					{
						// Can just remove the starting straight line.  The closure will now link to the next element
						figure.RemoveAt(0);
						done = true;
					}
					else // IsBezier
					{
						// If we treat the closure as a regular line the code below will turn it into a Bezier.  Then we can add a degenerate closure
						// Between the resulting curves
						addClosure = true;
						figure[previous] = new PathElement(PATHBEZIER, figure[previous].Points, figure[previous].Index, false, false); // clears existing Closure flag
					}
				}
				else if (elementIndex == figure.Count - 1) // Cannot be first and last because that would have triggered the no more than 2 elements
				{
					if (figure[previous].Endpoint.ApproxEqual(figure[0].Points[0]))
					{
						Utilities.LogSubError("for a degenerate closure which should be editing vertex 0 not the closure line");
						return;
					}
					if (!closed)
					{
						figure.RemoveAt(elementIndex);
						done = true;
					}
					else if (figure[previous].IsLine)
					{
						figure.RemoveAt(previous); // by removing the previous one, the closure will automatically close from the end of the segment before that
						done = true;
					}
					else
					{
						// Preceded by Bezier.  Can do as normal, creating a new Bezier to replace these 2, as long as we then add a degenerate closure
						addClosure = true;
						figure[previous] = new PathElement(PATHBEZIER, figure[previous].Points, figure[previous].Index, false, false); // clears existing Closure flag
					}
				}
				if (!done)
				{
					// Main processing, other than special cases handled above.  Depends on the combination of line and Bezier
					if (figure[previous].IsLine && figure[elementIndex].IsLine)
						figure[previous].Points[1] = figure[elementIndex].Points[1];
					else if (figure[previous].IsBezier && figure[elementIndex].IsBezier)
					{
						figure[previous].Points[2] = figure[elementIndex].Points[2]; // Keeps the first grab handle from first curve and second from second curve
						figure[previous].Points[3] = figure[elementIndex].Points[3];
					}
					else if (figure[previous].IsLine && figure[elementIndex].IsBezier)
					{
						// Creates curve using grab handles halfway along the straight-line and the grab handle from the end of the curve
						var bezier = new[] { figure[previous].Points[0], Geometry.MidPoint(figure[previous].Points[0], figure[previous].Points[1]), figure[elementIndex].Points[2], figure[elementIndex].Points[3] };
						figure[previous] = new PathElement(PATHBEZIER, bezier, figure[previous].Index, false, figure[previous].IsStart);
					}
					else // is Bezier-Line
					{
						// Likewise use a grab handle halfway along the line
						figure[previous].Points[3] = figure[elementIndex].Points[1];
						figure[previous].Points[2] = Geometry.MidPoint(figure[elementIndex].Points[0], figure[elementIndex].Points[1]);
					}
					figure.RemoveAt(elementIndex);
				}
				if (addClosure)
					figure.Add(new PathElement(PATHLINE, new[] { PointF.Empty, PointF.Empty }, 0, true, false)); // actual points (and everything else) are irrelevant here
				else if (closed)
					Debug.Assert(figure.Last().IsClosure);
			}

			MakePathFromElements(figures); // And now re-combine into a path
		}

		private void MakePathFromElements(List<List<PathElement>> figures)
		{
			List<byte> types = new List<byte>();
			List<PointF> points = new List<PointF>();
			foreach (var figure in figures)
			{
				var first = true; // cannot use the IsStart flag on the elements because it is not maintained by the editing code above
								  // however it is simply the first item in each collection which is the start of a figure
				foreach (var element in figure)
				{
					if (first)
					{
						types.Add(PATHSTART);
						points.Add(element.Points[0]);
						first = false;
					}
					if (element.IsClosure)
					{
						Debug.Assert(!element.IsStart);
						// note closure reported as separate line by iterator but this doesn't actually require any extra points, just change the type of the last one to include the closure
						types[types.Count - 1] |= PATHCLOSUREFLAG;
					}
					else
					{
						types.Add(element.Type);
						points.Add(element.Points[1]);
						if (element.IsBezier)
						{
							types.Add(element.Type);
							points.Add(element.Points[2]);
							types.Add(element.Type);
							points.Add(element.Points[3]);
						}
					}
				}
			}

			ChangePath(types.ToArray(), points.ToArray());
		}

		private void ConvertElementType(int index, bool toBezier)
		{
			// First part copied from RemoveVertex
			List<List<PathElement>> figures = new List<List<PathElement>>();
			List<PathElement> figure = null;
			int figureIndex = -1; // the index of the figure that we will be working on
			int elementIndex = -1; // and the index of the element within the figure
			foreach (var element in PathElement.WithinPath(m_Path, false, true))
			{
				if (element.IsStart)
				{
					figure = new List<PathElement>();
					figures.Add(figure);
				}
				if (element.Index == index)
				{
					figureIndex = figures.Count - 1;
					elementIndex = figure.Count; // not -1 because we haven't added this item yet
				}
				figure.Add(element);
			}
			if (figureIndex < 0)
				throw new InvalidOperationException("Did not find element index");
			figure = figures[figureIndex];
			// End copied from RemoveVertex

			PathElement old = figure[elementIndex];
			if (!toBezier)
				figure[elementIndex] = new PathElement(PATHLINE, new[] { old.Points[0], old.Points[3] }, old.Index, false, old.IsStart);
			else
			{
				// convert two Bezier.  Somewhat more complex.  Need to check if this is actually a closure, because that would mean adding a new closure on the end of this
				// Firstly create the new curve, which will not be flagged as closed regardless of whether the old line was a closure
				PointF[] points = { old.Points[0], PointF.Empty, PointF.Empty, old.Points[1] }; // The grab handles maybe more complex (?)
				points[1] = Geometry.Interpolate(old.Points[0], old.Points[1], 0.25F);
				points[2] = Geometry.Interpolate(old.Points[0], old.Points[1], 0.75F);
				figure[elementIndex] = new PathElement(PATHBEZIER, points, old.Index, false, old.IsStart);
				if (old.IsClosure)
				{
					// need to add a new degenerate closure on the end to close this curve back to the beginning
					figure.Add(new PathElement(PATHLINE, new[] { old.Points[1], old.Points[1] }, old.Index + 1, true, false)); // actually a lot of the parameters here are ignored because it is a closure
				}
			}

			MakePathFromElements(figures);
		}

		private void InsertVertex(Target target)
		{
			int index = target.ShapeIndex;
			// No need to analyse the path into elements, because this is the simplest of the updates
			List<byte> types = new List<byte>(m_Path.PathTypes);
			List<PointF> points = new List<PointF>(m_Path.PathPoints);
			if ((types[index] & PATHCLOSUREFLAG) > 0)
			{
				// assumed to not be degenerate (otherwise vertex zero should have been edited)
				types[index] -= PATHCLOSUREFLAG;
				types.Add(PATHLINE + PATHCLOSUREFLAG);
				points.Add(target.Position);
			}
			else if ((types[index + 1] & PATHTYPEMASK) == PATHLINE) // +1 index should be OK as we cannot be pointing to the last vertex of an open path: the index is supposed to indicate the start of an element
			{
				types.Insert(index + 1, PATHLINE);
				points.Insert(index + 1, target.Position); // Geometry.MidPoint(colPoints[index], colPoints(index + 1)))
			}
			else // is a Bezier
			{
				PointF[] newPoints = new[] { points[index], points[index + 1], points[index + 2], points[index + 3] };
				float T = Bezier.FindNearestApproachToPoint(target.Position, newPoints);
				T = Math.Max(Math.Min(T, 0.9F), 0.1F); // Limited to the range 0.1 to 0.9 - i.e. something reasonably sensible
				var newCurve = Bezier.Split(newPoints, T); // Has 7 points, But first and last are guaranteed to be first and last of the original curve
				points[index + 1] = newCurve[1];
				points[index + 2] = newCurve[2];
				points.Insert(index + 3, newCurve[3]);
				points.Insert(index + 4, newCurve[4]);
				points.Insert(index + 5, newCurve[5]);
				types.Insert(index + 3, PATHBEZIER);
				types.Insert(index + 4, PATHBEZIER);
				types.Insert(index + 5, PATHBEZIER); // This will leave the original third element on the end, which may have a closure flag
			}
			ChangePath(types.ToArray(), points.ToArray());
		}

		private void ChangePath(byte[] types, PointF[] points)
		{
			// Sadly there seems to be no way of updating an existing path; it is necessary to create a new one
			GraphicsPath create = new GraphicsPath(points, types, m_Path.FillMode);
			m_Path.Dispose();
			m_Path = create;
			m_Bounds = RectangleF.Empty;
		}

		public override (GrabSpot[], string[]) GetEditableCoords(Target selectedElement)
		{
			if (selectedElement == null || selectedElement.Type != Target.Types.Line && selectedElement.Type != Target.Types.Vertex)
				return base.GetEditableCoords(selectedElement);
			PathElement element = GetPathNearTarget(selectedElement).Last(); // last since for vertex we want the one after.  For line/bezier it only returns one anyway
			if (selectedElement.Type == Target.Types.Vertex)
				return (new[] { new GrabSpot(this, GrabTypes.SingleVertex, element.Points[0], element.Index) }, new[] { "[SAW_Coord_Vertex]" });
			if (element.IsBezier)
				return (new[]
						{
							new GrabSpot(this, GrabTypes.SingleVertex, element.Points[0], element.Index),
							new GrabSpot(this, GrabTypes.Bezier, element.Points[1], element.Index+1) {ShapeParameter=1},
							new GrabSpot(this, GrabTypes.Bezier, element.Points[2], element.Index+2) {ShapeParameter=-1},
							new GrabSpot(this, GrabTypes.SingleVertex, element.Points[3], element.Index+3),
						},
					new[] { "[SAW_Coord_From]", "[SAW_Coord_Handle1]", "[SAW_Coord_Handle2]", "[SAW_Coord_To]" });
			return (new[]
			{
				new GrabSpot(this, GrabTypes.SingleVertex, element.Points[0], element.Index),
				new GrabSpot(this, GrabTypes.SingleVertex, element.Points[1], element.Index+1)
			}, new[] { "[SAW_Coord_From]", "[SAW_Coord_To]" });// path element might return 4 element array, so need to trim it down
		}

		#endregion

		// data, load, save in the base class is sufficient

	}
}
