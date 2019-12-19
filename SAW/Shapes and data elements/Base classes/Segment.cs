using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace SAW
{
	/// <summary>represents a part of a shape, which is EITHER a line or a bezier curve
	/// also remembers details of part of shape it came from for iteration</summary>
	public class Segment
	{

		public PointF[] P; // must be either PointF(1) or PointF(3+3n for n=0..)
		public Shape Shape;
		public int Index; // usually which line within shape it came from
		public float Parameter; // used by source shape
		public bool Forward; // used by shape when iterating to know which way we are going
		private Intersection m_EndIntersection; // if ends at a vertex, then is a dummy intersection, and ShapeB is null
												// should be nothing if and only if end of this segment is the end of a non-closed shape

		public Intersection EndIntersection
		{
			get { return m_EndIntersection; }
			set
			{
				m_EndIntersection = value.DisambiguateSameShape(Index); //If(Forward, Index, Index - 1))
			}
		}

		public Segment(Shape shape, PointF A, PointF B, int index, bool forward, Intersection end)
		{
			Shape = shape;
			P = new[] { A, B };
			Debug.Assert(!P[0].ApproxEqual(P[1]), "Line segment - degenerate in " + Shape.ShapeCode + ", index=" + index);
			Index = index;
			Forward = forward;
			m_EndIntersection = end;
			if (end != null)
			{
				Debug.Assert(end.ShapeA == shape || end.ShapeB == shape, "Segment end intersection - neither shape in the intersection is the shape for this segment");
				m_EndIntersection = end.DisambiguateSameShape(Index); //If(Forward, Index, Index - 1))
																	  // if the intersection is with another line in this shape this ensures that the store intersection has the segment line in A
			}
		}

		public Segment(Shape shape, PointF[] line, int index, float param, bool forward, Intersection end)
		{
			// can contain any number of Bezier segments.  4 points define the first one, and 3 points more are needed for each subsequent one
			Shape = shape;
			if (line.Length < 4 || line.Length % 3 != 1)
				throw new ArgumentException("Segment Bezier-style constructor: P must be length 4+3n");
			P = line;
			// sometimes arcs return and essentially empty curve at the beginning, if we are starting from an intersection pretty much at the end of the arc
			// due to rounding errors
			if (P.Length > 4 && P[0].ApproxEqual(P[1]))
			{
				if (P[0].ApproxEqual(P[2]) && P[0].ApproxEqual(P[3]))
				{
					// remove these three points off the beginning
					for (int point = 0; point <= P.Length - 4; point++)
						P[point] = P[point + 3];
					Array.Resize(ref P, P.Length - 3);
				}
			}
			// likewise on the end
			if (P.Length > 4 && P[P.Length - 1].ApproxEqual(P[P.Length - 2]))
			{
				if (P[P.Length - 1].ApproxEqual(P[P.Length - 3]) && P[P.Length - 1].ApproxEqual(P[P.Length - 4]))
					Array.Resize(ref P, P.Length - 3);
			}

			Debug.Assert(!P[0].ApproxEqual(P[3]), "Bezier segment - first segment degenerate"); // NB we do allow first control point to be on P0
			Index = index;
			Parameter = param;
			Forward = forward;
			m_EndIntersection = end;
			if (end != null)
			{
				Debug.Assert(end.ShapeA == shape || end.ShapeB == shape, "Segment end intersection - neither shape in the intersection is the shape for this segment");
				m_EndIntersection = end.DisambiguateSameShape(Index); //If(Forward, Index, Index - 1))
			}
		}

		public void Draw(Graphics gr, Pen pn)
		{
			if (P.Length == 2)
				gr.DrawLine(pn, P[0], P[1]);
			else
			{
				for (int index = 0; index <= P.Length - 4; index += 3)
				{
					gr.DrawBezier(pn, P[index], P[index + 1], P[index + 2], P[index + 3]);
				}
			}
		}

		public static System.Drawing.Drawing2D.GraphicsPath CreatePathFromSegments(List<Segment> segments)
		{
			// returns the graphics path for this list of segments
			// don't really want to use GraphicsPath.AddLine/AddBezier, because these always add a line from
			// the last point is used to the beginning of this line.  However the segments are defined such that they connect up without these
			List<PointF> points = new List<PointF> { segments[0].P[0] };
			List<byte> types = new List<byte> { Lined.PATHSTART };
			for (int index = 0; index <= segments.Count - 1; index++)
			{
				Segment segment = segments[index];
				if (segment.IsLine)
				{
					points.Add(segment.P[1]);
					types.Add(Lined.PATHLINE);
				}
				else
				{
					for (int vertex = 1; vertex <= segment.P.Length - 1; vertex++)
					{
						points.Add(segment.P[vertex]);
						types.Add(Lined.PATHBEZIER);
					}
				}
			}
			if (types[types.Count - 1] == Lined.PATHLINE && points[points.Count - 1].ApproxEqual(points[0]))
			{
				// Last point is actually spurious since we will close the path anyway; it is a line back to the start, but the closure flag would do that
				types.RemoveAt(types.Count - 1);
				points.RemoveAt(points.Count - 1);
			}
			types[types.Count - 1] += Lined.PATHCLOSUREFLAG;
			return new System.Drawing.Drawing2D.GraphicsPath(points.ToArray(), types.ToArray(), System.Drawing.Drawing2D.FillMode.Alternate);
		}

		public static List<Shape> GetInvolvedShapes(List<Segment> segments)
		{
			// returns all distinct shapes involved in this list of segments
			List<Shape> shapes = new List<Shape>();
			foreach (Segment segment in segments)
			{
				if (!shapes.Contains(segment.Shape))
					shapes.Add(segment.Shape);
			}
			return shapes;
		}

		public static Shape TestForSingleShape(List<Segment> segments, List<Shape> otherShapesToCheck)
		{
			// tests if the the given segments actually represent a single existing shape.  If so, returns the shape; otherwise Nothing
			// only really need to test shapes which are involved.  Although it is theoretically possible, if every single line
			// is overlapped that there could be another shape which replicates everyone of these lines and is never involved, that is exceedingly unlikely
			// (especially because the tracing system tends to follow a single shape around)
			List<Shape> shapes = GetInvolvedShapes(segments);
			if (shapes.Count == 1)
			{
				// special case: this single shape will always match the list of segments, unless the shape has intersections with itself
				if (shapes[0].HasIntersectionWith(shapes[0]))
				{
					// see test case 4 for why this needs to continue to the test beneath - in this case the self-intersections are
					// all within the boundary, and the splatter DOES follow the shape.  Usually, of course, a shape which intersects itself would be internally
					// divided, and a splatter will only fill part of it
					// although case 4 still fails as the splatter has divided the boundary slightly differently where the lines overlap each other
					//Return null
				}
				else
					return shapes[0];
			}
			// For each of the shapes check if it can represent all the segments
			foreach (Shape shape in shapes)
			{
				if (segments.All(segment => shape.ContainsSegment(segment)))
					return shape;
			}
			// and again for the other list
			if (otherShapesToCheck != null)
			{
				foreach (Shape shape in otherShapesToCheck)
				{
					if (segments.All(segment => shape.ContainsSegment(segment)))
						return shape;
				}
			}
			return null;
		}

		#region Information
		public bool IsLine
		{ get { return P.Length == 2; } }

		public bool IsBezier
		{ get { return P.Length >= 4; } }

		public void Diagnostic(System.Text.StringBuilder output)
		{
			output.Append("Segment in ").Append(Shape.GetHashCode().ToString("x")).Append("(").Append(Index.ToString()).Append(",").Append(Parameter.ToString());
			output.Append(Forward ? "->" : "<-").AppendLine(")");
			for (int index = 0; index <= P.Length - 1; index++)
			{
				if (index <= 1 || index >= P.Length - 2)
				{
					output.Append("P[").Append(index.ToString()).Append("] = (").Append(P[index].X.ToString("0.##"));
					output.Append(", ").Append(P[index].Y.ToString("0.##")).AppendLine(")");
					if (index == 1 && P.Length > 4)
						output.AppendLine("...");
				}
			}
			if (m_EndIntersection == null)
				output.AppendLine("(No end intersection)");
			else
			{
				output.Append("ends ");
				m_EndIntersection.Diagnostic(output, Shape); //, Index)
			}
			output.AppendLine("StartAngle=" + this.StartAngle);
		}

		public PointF EndPoint
		{ get { return P[P.Length - 1]; } }

		#endregion

		#region Comparisons
		public bool EndMatches(Segment other)
		{
			// [LATER] - below has problems with concurrent lines - we can approach the end point down a different shape
			// I think it is sufficient to check if the coordinates are the same
			// Check if this segment ends in the same place as the other one
			// if the end is a genuine intersection (from the data) we can just check that the intersections are the same object
			// however if the end was a dummy intersection at a vertex, we will need to check the values
			return EndPoint.ApproxEqual(other.EndPoint);
		}

		public bool Equals(Segment other)
		{
			// returns true if these segments refer to the same part of the same shape
			if (Shape != other.Shape)
				return false;
			if (other.Index != Index || other.Parameter != Parameter)
				return false;
			if (Forward != other.Forward)
				return false;
			//Need to check not just that they refer to the same section of the shape, but also stop in the same place
			if (m_EndIntersection == null)
				return other.m_EndIntersection == null;
			if (other.m_EndIntersection == null)
				return false;
			if (other.m_EndIntersection == m_EndIntersection)
				return true;
			// they don't match if one or both are genuine intersections (in which case the object identity test above was sufficient)
			if (!m_EndIntersection.Termination || !other.m_EndIntersection.Termination)
				return false;
			return true; // if both single shape it implies they are at end of segment so end point matches
		}

		public bool EqualButOpposite(Segment other)
		{
			// return true if the segment is appear to refer to the same piece of line, but in opposite directions
			// checking the end in the same place is a bit complicated, because EndIntersection will refer to different parts of the line for each
			// and some of the index and parameter is somewhat shaped pendant, so this is difficult to compare
			// but I think in the way this is used, it is not necessary to filter too much on some of these details
			if (Shape != other.Shape)
				return false;
			if (other.Index != Index)
				return false; // parameter should probably differ
			if (Forward == other.Forward)
				return false; // Forward must be opposite
			if (m_EndIntersection != null)
				if (other.Parameter != m_EndIntersection.Parameter(Shape))
					return false;
			if (other.m_EndIntersection != null)
				if (Parameter != other.m_EndIntersection.Parameter(other.Shape))
					return false;
			return true;
		}
		#endregion

		#region Instantaneous vectors at end of line
		public SizeF StartVector()
		{
			// for a Bezier curves we can't quite take the exact instantaneous vector.  In the situation where 2 circles touch, for example
			// the control points for their approximate curves will head in exactly the same direction.  We want to modify the start vector fractionally in the direction of curvature
			// to get a better understanding of which of these vectors is which side of the other#
			if (P.Length == 2)
				return P[0].VectorTo(P[1]);
			else
			{
				// so we mix in a small amount of P(2) with P(1)
				// in a test case 3 we are still getting some problems because a larger curve is showing up as an angle inside the tighter curve
				// therefore going to use more P(2) with shorter curves
				float P3 = 0.001f + 5 / (10 + Geometry.DirtyDistance(P[0], P[3]));
				// curve nominal length = P to use...
				// 0 = 0.5001
				// 10 = 0.2501
				// 30 = 0.1251
				// 100 = 0.050
				// 300 = 0.032
				return P[0].VectorTo(Geometry.Interpolate(P[1], P[2], P3));
			}
		}

		public SizeF EndVector()
		{
			// Vector away from the endpoint
			if (P.Length == 2)
				return P[1].VectorTo(P[0]);
			else
			{
				float P3 = 0.001f + 5 / (10 + Geometry.DirtyDistance(P[P.Length - 1], P[P.Length - 4]));
				return P[P.Length - 1].VectorTo(Geometry.Interpolate(P[P.Length - 2], P[P.Length - 3], P3));
			}
		}

		public float StartAngle
		{ get { return StartVector().VectorAngle(); } }

		public float EndAngle
		{ get { return EndVector().VectorAngle(); } }

		#endregion

	}
}