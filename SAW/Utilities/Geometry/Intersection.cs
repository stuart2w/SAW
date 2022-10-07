using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using SAW.Shapes;

namespace SAW
{
	/// <summary>used to record where two shapes intersect (i.e. the bounding lines - we are not interested in the filled part of a shape)
	/// also used sometimes to record a position within a shape (and ShapeB is nothing in that case)</summary>
	public class Intersection
	{

		public PointF Position;
		public Shape ShapeA;
		public Shape ShapeB;
		// we also record information about where within the shapes the intersection occurred, to help with complex iterations
		// 2 pieces of information are stored for each shape; the meaning of these is determined by the shape implementation
		public int IndexA;
		public int IndexB;
		// would quite like to store the angles in a circular intersections, but not really worth it at the moment as the
		// circular intersection logic just calculates the points, and we would need to derive the angles from these which is not worth it is the angle is only needed
		// for complex iterations
		public float ParameterA;
		public float ParameterB;
		public float Certainty; // estimate of the accuracy of the intersection.  Expressed as the angle between the lines which cause it
								// (maximum 90°), however adjustments can be applied so this might be <0

		public Intersection(PointF position, Shape shpA, int indexA, float parameterA, Shape shpB, int indexB, float parameterB, float certainty)
		{
			// Debug.Assert(Not shpA Is shpB, "Intersection: shapes equal") now allowed - is basically same as other constructor anyway and check IX with self using path reverts to normal IX logic which calls this
			Position = position;
			ShapeA = shpA;
			ShapeB = shpB;
			IndexA = indexA;
			IndexB = indexB;
			ParameterA = parameterA;
			ParameterB = parameterB;
			Certainty = certainty;
		}

		// termination version
		public Intersection(PointF position, Shape shpA, int indexA, float parameterA)
		{
			Position = position;
			ShapeA = shpA;
			ShapeB = null;
			IndexA = indexA;
			IndexB = 0;
			ParameterA = parameterA;
			ParameterB = 0;
			Certainty = Geometry.ANGLE90;
		}

		/// <summary>version for intersections within one shape </summary>
		public Intersection(PointF position, Shape shpA, int indexA, float parameterA, int indexB, float parameterB, float certainty)
		{
			Position = position;
			ShapeA = shpA;
			ShapeB = shpA;
			IndexA = indexA;
			IndexB = indexB;
			ParameterA = parameterA;
			ParameterB = parameterB;
			Certainty = certainty;
		}

		/// <summary>Returns a copy but with A and B reversed </summary>
		public Intersection(Intersection existing)
		{
			Intersection intersection = existing;
			Position = intersection.Position;
			ShapeA = intersection.ShapeB;
			ShapeB = intersection.ShapeA;
			IndexA = intersection.IndexB;
			IndexB = intersection.IndexA;
			ParameterA = intersection.ParameterB;
			ParameterB = intersection.ParameterA;
			Certainty = intersection.Certainty;
		}

		/// <summary>if given one of the two shapes of which this is an intersection, the function returns the other one</summary>
		public Shape OtherShape(Shape shape)
		{
			if (shape == ShapeA)
				return ShapeB;
			Debug.Assert(shape == ShapeB);
			return ShapeA;
		}

		public bool ContainsShape(Shape shape)
		{
			return shape == ShapeA || shape == ShapeB;
		}

		/// <summary>returns true if one and only one of the shapes is nothing </summary>
		public bool Termination
		{ get { return (ShapeA == null) ^ (ShapeB == null); } }

		/// <summary>returns the params for the given shape
		/// optional parameter is only used if ShapeA is the same as ShapeB, in which case this is the index which we do want returned</summary>
		public void Parameters(Shape shape, ref int index, ref float parameter, int desiredIndexIfInternal = -1)
		{
			if (ShapeA == ShapeB && desiredIndexIfInternal >= 0)
			{
				Debug.Assert(shape == ShapeA);
				if (IndexB == desiredIndexIfInternal)
				{
					index = IndexB;
					parameter = ParameterB;
				}
				else
				{
					index = IndexA;
					parameter = ParameterA;
				}
			}
			else
			{
				if (shape == ShapeA)
				{
					index = IndexA;
					parameter = ParameterA;
				}
				else if (shape == ShapeB)
				{
					index = IndexB;
					parameter = ParameterB;
				}
				else
					throw new ArgumentException("Shape is not party to this intersection");
			}
		}

		public int Index(Shape shape)
		{
			if (shape == ShapeA)
				return IndexA;
			Debug.Assert(shape == ShapeB);
			return IndexB;
		}
		public void SetIndex(int value, Shape shape)
		{
			if (shape == ShapeA)
				IndexA = value;
			else
			{
				Debug.Assert(shape == ShapeB);
				IndexB = value;
			}
		}

		/// <summary>unlike checking the value from Index above, this will check both if this is an intersection within the same shape </summary>
		public bool IsIndex(Shape shape, int index)
		{
			return shape == ShapeA && index == IndexA || shape == ShapeB && index == IndexB;
		}

		public float Parameter(Shape shape)
		{
			if (shape == ShapeA)
				return ParameterA;
			Debug.Assert(shape == ShapeB);
			return ParameterB;
		}
		public void SetParameter(float value, Shape shape)
		{
			if (shape == ShapeA)
				ParameterA = value;
			else
			{
				Debug.Assert(shape == ShapeB);
				ParameterB = value;
			}
		}

		public void Diagnostic(System.Text.StringBuilder output, Shape fromShape)
		{
			// if this intersection involves this shape twice it will report the A values first and then the B values
			output.Append("@ (").Append(Position.X.ToString(Shape.DiagnosticFormat)).Append(",").Append(Position.Y.ToString(Shape.DiagnosticFormat)).Append(")(");
			int index = 0;
			float parameter = 0;
			Parameters(fromShape, ref index, ref parameter);
			output.Append(index.ToString()).Append(";").Append(parameter.ToString()).Append(")");
			output.Append(" with ");
			Shape otherShape = OtherShape(fromShape);
			if (otherShape == null)
				output.AppendLine("nothing");
			else if (otherShape == fromShape)
			{
				Debug.Assert(index == IndexA);
				output.Append(otherShape.GetHashCode().ToString("x")).Append("(");
				output.Append(IndexB.ToString()).Append(";").Append(ParameterB.ToString()).AppendLine(")");
			}
			else
			{
				output.Append(otherShape.GetHashCode().ToString("x")).Append("(");
				Parameters(otherShape, ref index, ref parameter);
				output.Append(index.ToString()).Append(";").Append(parameter.ToString()).AppendLine(")");
			}
		}

		/// <summary>if and only if this intersection is within a single shape it returns an intersection guaranteed to
		/// have the given index as ShapeA</summary>
		public Intersection DisambiguateSameShape(int indexWanted)
		{
			if (ShapeA != ShapeB)
				return this;
			if (indexWanted == IndexB)
				return new Intersection(this); // returns a version with A and B flipped over
			return this;
		}

		public Intersection FlipSameShape()
		{
			// returns this intersection, but with A and B reversed if and only if the intersection is within one shape
			if (ShapeA != ShapeB)
				return this;
			return new Intersection(this);
		}

		internal static float NormaliseCertainty(float certainty)
		{
			// the certainty is often just given as the angle between the two lines which created the intersection.  This can be much greater than 90°
			certainty = Math.Abs(certainty) % Geometry.ANGLE180;
			if (certainty > Geometry.ANGLE90)
				certainty = Geometry.ANGLE180 - certainty;
			return certainty;
		}

		#region Shared geometric calculations
		// certainty of any intersection is returned in the final ByRef parameter.  Not set if returns PointF.Empty
		// certainties return, any of these functions might not be normalised

		public static PointF Line_LineIntersection(PointF A, PointF B, PointF C, PointF D)
		{
			// version without the IgnoreEnd and Infinite parameters(sometimes more useful).  Mostly used by connector
			return Line_LineIntersection(A, B, false, C, D, false, false, false, out _, out _);
		}

		public static PointF Line_LineIntersection(PointF A, PointF B, PointF C, PointF D, ref float certainty)
		{
			// version without the IgnoreEnd and Infinite parameters(sometimes more useful).  Mostly used by connector
			return Line_LineIntersection(A, B, false, C, D, false, false, false, out _, out certainty);
		}


		public static PointF Line_LineIntersection(PointF A, PointF B, bool ignoreEndAB, PointF C, PointF D, bool ignoreEndCD, bool ABInfinite, bool CDInfinite)
		{
			return Line_LineIntersection(A, B, ignoreEndAB, C, D, ignoreEndCD, ABInfinite, CDInfinite, out _, out _);
		}

		/// <summary>checks for any intersection between the lines A-B and C-D.  Returns PointF.Empty if they do not intersect (or if they are the same line)</summary>
		public static PointF Line_LineIntersection(PointF A, PointF B, bool ignoreEndAB, PointF C, PointF D, bool ignoreEndCD, bool ABInfinite, bool CDInfinite, out PointF secondIntersection, out float certainty)
		{
			// secondIntersection can return a second point for overlapping, parallel lines.  Will always be empty if return value is empty
			// see handwritten notes for basic geometry
			// first we calculate the intersection point assuming infinite lines, then check whether this is off the end of either line
			// we need to be careful if any of the lines are exactly vertical, because this would cause division by zero
			secondIntersection = PointF.Empty;
			certainty = 0;
			PointF intersection;
			if (Math.Abs(A.X - B.X) < Geometry.NEGLIGIBLE)
			{
				if (Math.Abs(C.X - D.X) < Geometry.NEGLIGIBLE)
					// Return PointF.Empty ' both lines vertical - they are either parallel or the same line
					return Line_LineIntersectionBothVertical(A, B, ignoreEndAB, C, D, ignoreEndCD, ABInfinite, CDInfinite);
				else
				{
					// first line is vertical
					float slope = (D.Y - C.Y) / (D.X - C.X);
					intersection = new PointF(A.X, C.Y + slope * (A.X - C.X));
					// we also need to check whether the calculated point is within the extent of the vertical line.  The test at the end of the function would not be sufficient for that
					if (!ABInfinite && intersection.Y < A.Y && intersection.Y < B.Y)
						return PointF.Empty;
					if (!ABInfinite && intersection.Y > A.Y && intersection.Y > B.Y)
						return PointF.Empty;
					if (Math.Abs(slope) > 0.001f)
						certainty = (float)Math.Atan(1 / slope); // otherwise leave certainty as 0
				}
			}
			else if (Math.Abs(C.X - D.X) < Geometry.NEGLIGIBLE)
			{
				// second line is vertical
				float slope = (B.Y - A.Y) / (B.X - A.X);
				intersection = new PointF(C.X, A.Y + slope * (C.X - A.X));
				if (!CDInfinite && intersection.Y < C.Y && intersection.Y < D.Y)
					return PointF.Empty;
				if (!CDInfinite && intersection.Y > C.Y && intersection.Y > D.Y)
					return PointF.Empty;
				if (Math.Abs(slope) > 0.001f)
					certainty = (float)Math.Atan(1 / slope); // otherwise leave certainty as 0
			}
			else
			{
				float slopeA = (B.Y - A.Y) / (B.X - A.X);
				float slopeC = (D.Y - C.Y) / (D.X - C.X);
				if (Math.Abs(slopeA - slopeC) < 0.001)
				{
					// lines are effectively parallel.  leave certainty = 0
					return Line_LineIntersectionParallel(A, B, ignoreEndAB, C, D, ignoreEndCD, ABInfinite, CDInfinite, ref secondIntersection);
				}
				float X = (slopeA * A.X - slopeC * C.X - A.Y + C.Y) / (slopeA - slopeC);
				intersection = new PointF(X, C.Y + slopeC * (X - C.X));
				certainty = (float)Math.Abs(Math.Atan(slopeA) - Math.Atan(slopeC));
			}
			// now check if the point is actually within the bounds of each line
			// only necessary to check in one direction, either X or Y because we know that the point does fall on both lines if extended
			// however there can be a problem if the line is almost horizontal or vertical, so we need to choose between X and Y
			// using the one where the line has the greatest extent.  If not the NEGLIGIBLE below might be a large chunk of the line
			if (!ABInfinite)
			{
				if (Math.Abs(A.X - B.X) > Math.Abs(A.Y - B.Y))
				{
					if (intersection.X < A.X - Geometry.NEGLIGIBLE && intersection.X < B.X - Geometry.NEGLIGIBLE)
						return PointF.Empty;
					// the +/- NEGLIGIBLE is because rounding errors can put the intersection point slightly outside the lines if they meet at the end
					// (quite common with any sort of snapping)
					if (intersection.X > A.X + Geometry.NEGLIGIBLE && intersection.X > B.X + Geometry.NEGLIGIBLE)
						return PointF.Empty;
				}
				else
				{
					if (intersection.Y < A.Y - Geometry.NEGLIGIBLE && intersection.Y < B.Y - Geometry.NEGLIGIBLE)
						return PointF.Empty;
					if (intersection.Y > A.Y + Geometry.NEGLIGIBLE && intersection.Y > B.Y + Geometry.NEGLIGIBLE)
						return PointF.Empty;
				}
			}
			if (!CDInfinite)
			{
				if (Math.Abs(C.X - D.X) > Math.Abs(C.Y - D.Y))
				{
					if (intersection.X < C.X - Geometry.NEGLIGIBLE && intersection.X < D.X - Geometry.NEGLIGIBLE)
						return PointF.Empty;
					if (intersection.X > C.X + Geometry.NEGLIGIBLE && intersection.X > D.X + Geometry.NEGLIGIBLE)
						return PointF.Empty;
				}
				else
				{
					if (intersection.Y < C.Y - Geometry.NEGLIGIBLE && intersection.Y < D.Y - Geometry.NEGLIGIBLE)
						return PointF.Empty;
					if (intersection.Y > C.Y + Geometry.NEGLIGIBLE && intersection.Y > D.Y + Geometry.NEGLIGIBLE)
						return PointF.Empty;
				}
			}
			if (ignoreEndAB && B.ApproxEqual(intersection))
				return PointF.Empty;
			if (ignoreEndCD && D.ApproxEqual(intersection))
				return PointF.Empty;
			return intersection;
		}

		/// <summary>Part of the Line_LineIntersection function for when both lines are parallel</summary>
		private static PointF Line_LineIntersectionParallel(PointF A, PointF B, bool ignoreEndAB, PointF C, PointF D, bool ignoreEndCD, bool ABInfinite, bool CDInfinite, ref PointF secondIntersection)
		{
			// First check that they are actually concurrent (at least extended)
			float distance = Geometry.DistancePointToLine(A, B, C);
			if (distance > Geometry.NEGLIGIBLE / 10)
				return PointF.Empty; // lines are parallel.  Need to be more sensitive than just using Negligible.  See splatter test case nine
			if (ABInfinite)
			{
				if (CDInfinite)
					return PointF.Empty;
				return C; //AB subsumes CD - see below
			}
			else if (CDInfinite)
				return A;
			float maxA = Math.Max(A.X, B.X);
			float minA = Math.Min(A.X, B.X);
			float minC = Math.Min(C.X, D.X);
			float maxC = Math.Max(C.X, D.X);
			if (maxA < minC - Geometry.NEGLIGIBLE || minA > maxC + Geometry.NEGLIGIBLE)
				return PointF.Empty;
			// (the lines are on the same infinite line, but don't overlap)
			if (Math.Abs(maxA - minC) < Geometry.NEGLIGIBLE || Math.Abs(minA - maxC) < Geometry.NEGLIGIBLE)
			{
				// the lines just touch end to end
				// we need to be careful as to whether we are allowed to include the relevant endpoints
				if (A.ApproxEqual(C))
					return A;
				if (!ignoreEndAB && B.ApproxEqual(C))
					return B;
				if (!ignoreEndCD && A.ApproxEqual(D))
					return A;
				if (!ignoreEndAB && !ignoreEndCD && B.ApproxEqual(D))
					return B;
				return PointF.Empty;
			}
			if (minA > minC - Geometry.NEGLIGIBLE && maxA < maxC + Geometry.NEGLIGIBLE)
				// Line AB is entirely within CD.  It is better to return the start point of AB, as generally shapes containing multiple lines
				// consider the start of each line, and ignore the end
				return A;
			if (minC > minA - Geometry.NEGLIGIBLE && maxC < maxA + Geometry.NEGLIGIBLE)
				// likewise CD in AB
				return C;
			// otherwise the lines must overlap, with each extending beyond the end of the other
			// [later] now return both points - required for flipped pie text case 2
			// we arbitrarily return a point halfway along the overlap
			if (minA < minC)
			{
				// maxA and minC are within the other lines
				float Y = (maxA - A.X) / (C.X - A.X) * (C.Y - A.Y) + A.Y;
				secondIntersection = new PointF(maxA, Y);
				Y = (minC - A.X) / (C.X - A.X) * (C.Y - A.Y) + A.Y;
				return new PointF(minC, Y);
			}
			else
			{
				// minA and maxC are within the other lines
				float Y = (minA - A.X) / (C.X - A.X) * (C.Y - A.Y) + A.Y;
				secondIntersection = new PointF(minA, Y);
				Y = (maxC - A.X) / (C.X - A.X) * (C.Y - A.Y) + A.Y;
				return new PointF(maxC, Y);
			}
		}

		private static PointF Line_LineIntersectionBothVertical(PointF A, PointF B, bool ignoreEndAB, PointF C, PointF D, bool ignoreEndCD, bool ABInfinite, bool CDInfinite)
		{
			// Similar to the above function but for when both lines are vertical (when we have to use the Y coordinates to compare)
			if (Math.Abs(A.X - C.X) > Geometry.NEGLIGIBLE)
				return PointF.Empty; // lines are parallel
			if (ABInfinite)
			{
				if (CDInfinite)
					return PointF.Empty;
				return C;
			}
			else if (CDInfinite)
				return A;
			float maxA = Math.Max(A.Y, B.Y);
			float minA = Math.Min(A.Y, B.Y);
			float minC = Math.Min(C.Y, D.Y);
			float maxC = Math.Max(C.Y, D.Y);
			if (maxA < minC - Geometry.NEGLIGIBLE || minA > maxC + Geometry.NEGLIGIBLE)
				return PointF.Empty;
			// (the lines are on the same infinite line, but don't overlap)
			if (Math.Abs(maxA - minC) < Geometry.NEGLIGIBLE || Math.Abs(minA - maxC) < Geometry.NEGLIGIBLE)
			{
				// the lines just touch end to end
				// we need to be careful as to whether we are allowed to include the relevant endpoints
				if (A.ApproxEqual(C))
					return A;
				if (!ignoreEndAB && B.ApproxEqual(C))
					return B;
				if (!ignoreEndCD && A.ApproxEqual(D))
					return A;
				if (!ignoreEndAB && !ignoreEndCD && B.ApproxEqual(D))
					return B;
				return PointF.Empty;
			}
			if (minA > minC - Geometry.NEGLIGIBLE && maxA < maxC + Geometry.NEGLIGIBLE)
			{
				// Line AB is entirely within CD.  It is better to return the start point of AB, as generally shapes containing multiple lines
				// consider the start of each line, and ignore the end
				return A;
			}
			if (minC > minA - Geometry.NEGLIGIBLE && maxC < maxA + Geometry.NEGLIGIBLE)
				return C;
			// otherwise the lines must overlap, with each extending beyond the end of the other
			// we arbitrarily return a point halfway along the overlap
			float Y = (Math.Max(minA, minC) + Math.Min(maxA, maxC)) / 2f; // the x-coordinate of our nominal intersection
			return new PointF(A.X, Y);
		}

		public static bool Line_PointIntersection(PointF lineA, PointF lineB, PointF C)
		{
			// or basically is the point on the line.  This is slightly different to the Line hit test in that the point should be exactly on the line
			// although "exactly" allows NEGLIGIBLE error to allow for rounding problems
			// if the return value is true, then the intersection is at ptC (of course)
			if (Math.Abs(lineA.X - lineB.X) < Geometry.NEGLIGIBLE)
			{
				// line is vertical, slope calculation below will fail
				if (Math.Abs(C.X - lineA.X) > Geometry.NEGLIGIBLE)
					return false;
				if (C.Y < lineA.Y && C.Y < lineB.Y)
					return false;
				if (C.Y > lineA.Y && C.Y > lineB.Y)
					return false;
				return true;
			}
			// check extent first, this is probably fastest
			if (C.X < lineA.X && C.X < lineB.X)
				return false;
			if (C.X > lineA.X && C.X > lineB.X)
				return false;
			if (C.Y < lineA.Y && C.Y < lineB.Y)
				return false;
			if (C.Y > lineA.Y && C.Y > lineB.Y)
				return false;
			float slope = (lineB.Y - lineA.Y) / (lineB.X - lineA.X);
			float requiredY = lineA.Y + slope * (C.X - lineA.X);
			if (Math.Abs(C.Y - requiredY) > 0)
				return false;
			return true;
		}

		public static PointF[] Circle_CircleIntersection(PointF centreA, float radiusA, PointF centreB, float radiusB, ref float certainty)
		{
			// returns either two points or null
			// see scanned documentation.  That gives the coordinates assuming that the first circle is at the origin, and the second is on the x-axis
			// however it is easy to calculate the answer given this - because the formula gives the distance along the vector between the two circles
			// and the perpendicular distance in, which can easily be used to scale the vector between the circles to give the answer
			SizeF vector = centreA.VectorTo(centreB);
			float distance = vector.Length();
			if (distance < Geometry.NEGLIGIBLE)
				return null; // if the centres match they either don't touch or they are the same circle and touch everywhere
							 // the distance along the x-axis way from circle A if the two circles were on the x-axis
			float X = (distance * distance + radiusA * radiusA - radiusB * radiusB) / (distance * 2);
			float Ysq = radiusA * radiusA - X * X;
			// this is the square of the perpendicular part
			// if this has no roots then the circles do not touch
			if (Ysq < 0)
				return null;
			float Y = (float)Math.Sqrt(Ysq);
			SizeF unit = vector.MultiplyBy(1 / distance); // the unit vector from circle A to B
														  // now get the midpoint between the two intersections, i.e. add to the X component
			PointF middle = centreA + unit.MultiplyBy(X);
			// and rotate the unit vector to be perpendicular; it does not matter which way we go as we need both answers anyway
			unit = new SizeF(-unit.Height, unit.Width);
			// and now add both positive and negative perpendicular vectors to give the two points
			if (Y < Geometry.NEGLIGIBLE)
			{
				// the two points are pretty much the same place, just assume it is a single tangent
				certainty = -1;
				return new PointF[] { middle };
			}
			PointF[] result = new PointF[2];
			result[0] = middle + unit.MultiplyBy(Y);
			result[1] = middle + unit.MultiplyBy(-Y);
			certainty = NormaliseCertainty((float)Math.Atan(Y / X));
			return result;
		}

		public static List<PointF> Circle_LineIntersection(PointF centre, float radius, PointF lineA, PointF lineB, bool ignoreEndPoint, ref float certainty)
		{
			// see scanned documentation
			// returns a list of 0,1 or 2 points (number of intersections is uncertain because although the line would usually cross the circle twice,
			// the line is not infinite).  Can also return nothing if they do not intersect
			PointF relative = PointF.Subtract(lineA, new SizeF(centre)); // the first point on the line, restated with the centre of the circle as origin
			List<PointF> list = new List<PointF>();
			if (lineA.ApproxEqual(lineB))
				return list;
			// first do a check whether the line is tangential to the circle.  The code below is a bit erratic in this case
			// finding either 0,1 or 2 intersections at the same point, all depending on rounding errors
			float distance = Geometry.DistancePointToLine(lineA, lineB, centre);
			if (Math.Abs(distance - radius) < Geometry.NEGLIGIBLE)
			{
				// tangential.  Just a single intersection on the line
				certainty = 0;
				PointF intersect = Geometry.ClosestPointOnLineLimited(lineA, lineB, centre, true);
				if (!intersect.IsEmpty)
				{
					if (!ignoreEndPoint || !lineB.ApproxEqual(intersect))
						list.Add(intersect); // IsEmpty implies it is off the end of the line
				}
				else
				{
					// there can be a problem with the above with a polygon and a circle, where the circle crosses at a vertex.
					// the line leading up to the vertex will correctly ignore the intersection because it is at the end of the line
					// but if the line leading away from the intersection is almost tangential, then although ClosestPointOnLineLimited
					// allows some rounding error when testing if the point is off the end, the actual tangent point can be quite a long way from the end of the line
					// even though the line terminates almost on the circle (because the line is virtually parallel to the circle at this point,
					// and therefore the intersection is some way off).  Therefore this intersection would also be ignored because ClosestPointOnLineLimited
					// ignored the point.  So we need to check if either end of the line is actually very close to the radius, and if so
					// take that end of the line as an intersection
					if (Geometry.DistanceBetween(centre, lineA) < radius + Geometry.NEGLIGIBLE)
						list.Add(lineA);
					else if (Geometry.DistanceBetween(centre, lineB) < radius + Geometry.NEGLIGIBLE)
						list.Add(lineB);
				}
				return list;
			}
			if (Math.Abs(lineA.X - lineB.X) < Geometry.NEGLIGIBLE)
			{
				// line is vertical - calculating slope will give division by zero.  This needs to be done separately, but the case is not exactly complex!
				if (relative.X < -radius || relative.X > radius)
					return null; // does not intercept
				certainty = (float)Math.Acos(Math.Abs(relative.X) / radius);
				float Y = (float)Math.Sqrt(radius * radius - relative.X * relative.X);
				// note that this is still relative to the circle and both positive and negative roots should be used
				// need to check whether these are within the extent of the line however
				float Ylow = centre.Y - Y;
				float Yhigh = centre.Y + Y;
				// don't know which order the two line points are given in
				if (Ylow >= lineA.Y && Ylow <= lineB.Y || Ylow <= lineA.Y && Ylow >= lineB.Y)
				{
					if (!ignoreEndPoint || Math.Abs(lineB.Y - Ylow) > Geometry.NEGLIGIBLESMALL)
						list.Add(new PointF(lineA.X, Ylow));
				}
				if (Yhigh >= lineA.Y && Yhigh <= lineB.Y || Yhigh <= lineA.Y && Yhigh >= lineB.Y)
				{
					if (!ignoreEndPoint || Math.Abs(lineB.Y - Yhigh) > Geometry.NEGLIGIBLESMALL)
						list.Add(new PointF(lineA.X, Yhigh));
				}
				return list;
			}
			float slope = (lineB.Y - lineA.Y) / (lineB.X - lineA.X);
			// we need the three values to plug in to the standard quadratic formula
			float A = 1 + slope * slope;
			float B = 2 * (relative.Y * slope - relative.X * slope * slope);
			float C = slope * slope * relative.X * relative.X + relative.Y * relative.Y - radius * radius - 2 * slope * relative.X * relative.Y;
			float root = B * B - 4 * A * C; // the inside of the root bit of the quadratic formula
			if (root <= 0)
				return null; // we also ignore, for the moment, if the line is an exact tangent of the circle (=0)
							 // calculate the points...
			float X1 = Convert.ToSingle((-B + Math.Sqrt(root)) / (2 * A));
			float X2 = Convert.ToSingle((-B - Math.Sqrt(root)) / (2 * A));
			float Y1 = relative.Y + slope * (X1 - relative.X);
			float Y2 = relative.Y + slope * (X2 - relative.X);
			// adjust them back into actual coordinates (no longer relative to centre of the circle)
			X1 += centre.X;
			X2 += centre.X;
			Y1 += centre.Y;
			Y2 += centre.Y;
			// check that they are actually within the line, and add to list.  Only need to check the X dimension
			if (X1 >= lineA.X - Geometry.NEGLIGIBLE && X1 <= lineB.X + Geometry.NEGLIGIBLE || X1 <= lineA.X + Geometry.NEGLIGIBLE && X1 >= lineB.X - Geometry.NEGLIGIBLE)
			{
				if (!ignoreEndPoint || !lineB.ApproxEqual(new PointF(X1, Y1)))
					list.Add(new PointF(X1, Y1));
			}
			if (X2 >= lineA.X - Geometry.NEGLIGIBLE && X2 <= lineB.X + Geometry.NEGLIGIBLE || X2 <= lineA.X + Geometry.NEGLIGIBLE && X2 >= lineB.X - Geometry.NEGLIGIBLE)
			{
				if (!ignoreEndPoint || !lineB.ApproxEqual(new PointF(X2, Y2)))
					list.Add(new PointF(X2, Y2));
			}
			certainty = (float)Math.Asin(Math.Min(1, Math.Max(Geometry.DistanceBetween(new PointF(X1, Y1), new PointF(X2, Y2)), 0.01F) / (radius * 2)));
			// Math.Min(1,) needed as rounding errors every so often make it 1.000001
			return list;
		}

		public static PointF[] Rectangle_LineIntersection(RectangleF rect, PointF A, PointF B, bool ignoreEndAB = false)
		{
			// just for convenience; checks the line against each of the edges of the rectangle
			PointF[] rectangle = rect.GetPoints();
			List<PointF> intersections = new List<PointF>();
			for (int index = 0; index <= 3; index++)
			{
				PointF ptIntersection = Line_LineIntersection(A, B, ignoreEndAB, rectangle[index], rectangle[(index + 1) % 4], true, false, false);
				if (!ptIntersection.IsEmpty)
					intersections.Add(ptIntersection);
			}
			return intersections.ToArray();
		}

		#endregion

		#region Bezier intersections
		internal struct BezierParameterPair
		{
			// for the clipping functions below this remembers the pair of parameters at which they intersect
			public float Tp;
			public float Tq;
			public PointF Position;
			// it also now remembers the number of versions of FatClip which was required.  This is used as an estimate of the certainty
			public int Recursion;

			public BezierParameterPair(float P, float Q, PointF pt, int recursion)
			{
				Tp = P;
				Tq = Q;
				Position = pt;
				Recursion = recursion;
			}

			public BezierParameterPair(BezierParameterPair source, float P, float Q)
			{
				// used within the recursive clipping functions when unwinding and we calculating the parameters based upon the original curves
				Tp = P;
				Tq = Q;
				Position = source.Position;
				Recursion = source.Recursion;
			}
		}

		/// <summary>in data coordinates units
		/// any less than about 0.001 may fail to resolve, since actual coordinates may be up to about 1000, and we are only working using Single (8 sigfig?) </summary>
		private const float BEZIER_BEZIER_INTERSECTION_ACCURACY = 0.01f;

		internal static List<BezierParameterPair> FatClip(PointF[] P, PointF[] Q)
		{
			// First a sanity check, to check that the two curves are not identical, which would return an almost infinite number of matches
			// (impressively the code below works and does indeed return a huge number of intersections, potentially generating targets > every pixel)
			bool matches = true;
			for (int index = 0; index <= 3; index++)
			{
				if (!P[index].ApproxEqual(Q[index]) &&
					!P[index].ApproxEqual(Q[3 - index]))
				{
					// note that this also checks that they don't match, but in opposite directions
					matches = false;
					break;
				}
			}
			if (matches)
				return null; // if the curves are identical, then return no intersections
			return FatClip(P, Q, false, 0);
		}

		private static List<BezierParameterPair> FatClip(PointF[] P, PointF[] Q, bool failedPrevious, int depth)
		{
			// P and Q are arrays of 4 points each defining a Bezier curve (end-ctrl-ctrl-end), with parameter running from 0-1.  This uses the FatLine clipping
			// described in the Bezier PDF document to find the parameters within the curves at which they intersect. see ch7.7.2
			// returns the parameters for P,Q at which they intersect.  Return is nothing if they do not intersect
			// failedPrevious should be false when first called.  This is used internally to check if clipping both P and Q has failed to restrict much

			if (depth > 200)
			{
				Debug.WriteLine("Aborting FatClip due to excess iterations");
				return null;
			}
			// check if the answer we have is accurate enough
			var QLength = Bezier.NominalLength(Q);
			if (Bezier.NominalLength(P) < BEZIER_BEZIER_INTERSECTION_ACCURACY && QLength < BEZIER_BEZIER_INTERSECTION_ACCURACY)
			{
				// since both entire curves are considered accurate enough, we just return the parameters 0.5, 0.5.
				// As the recursion unwinds these will be adjusted backwards to the parameters of the original curves
				return new List<BezierParameterPair> { new BezierParameterPair(0.5f, 0.5f, Bezier.GetPoint(P, 0.5F), depth) };
			}

			// also need to check if Q very short (case if previous iteration had minimal T range).  If Q totally degenerate, Q0=Q1=Q2=Q3 then the
			// FatLine is nonsense
			if (QLength < BEZIER_BEZIER_INTERSECTION_ACCURACY / 10)
			{
				var T = Bezier.FindNearestApproachToPoint(Q[1], P, BEZIER_BEZIER_INTERSECTION_ACCURACY / 10);
				var pt = Bezier.GetPoint(P, T);
				if (Geometry.DistanceBetween(pt, Q[1]) > BEZIER_BEZIER_INTERSECTION_ACCURACY)
					return null; // not actually on line
				return new List<BezierParameterPair> { new BezierParameterPair(T, 0.5f, pt, depth) };
			}

			Geometry.FatLine line = new Geometry.FatLine(Q[0], Q[3], Q[1], Q[2]); // FatLine encompassing all of the points on Q
			PointF[] D = new PointF[4]; // (signed) distance function - as EXPLICIT bezier.  Distance from P to line at t - where t is same in D(t) and P(t)
										// the Y of each point is the distance; the X of each point just matches the parameter
			D[0] = new PointF(0, line.DistanceTo(P[0]));
			D[1] = new PointF(1 / 3F, line.DistanceTo(P[1]));
			D[2] = new PointF(2 / 3F, line.DistanceTo(P[2]));
			D[3] = new PointF(1, line.DistanceTo(P[3]));
			float lowBound = 1; // bounds of region WITHIN distance
			float hiBound = 0;
			// check each line forming convex hull of D (check all lines Dn - Dm, without checking which are external)
			// to see what limits they place on t for D to be within fatline
			FatLineClipDistance(D[0], D[1], line, ref lowBound, ref hiBound);
			FatLineClipDistance(D[0], D[2], line, ref lowBound, ref hiBound);
			FatLineClipDistance(D[0], D[3], line, ref lowBound, ref hiBound);
			FatLineClipDistance(D[1], D[2], line, ref lowBound, ref hiBound);
			FatLineClipDistance(D[1], D[3], line, ref lowBound, ref hiBound);
			FatLineClipDistance(D[2], D[3], line, ref lowBound, ref hiBound);
			if (hiBound < lowBound)// failed to find any intersection
				return null;
			else
			{
				// we can discard areas that we know to be outside the intersection region.  The intersection region should be in the middle,
				// with a discardable area above, below or both.  Occasionally there will be no discarded area
				// lowBound, hiBound is the range within which we have deduced that the P parameter must lie
				bool failedToRestrict = hiBound - lowBound > 0.8f;
				// Sederberg suggests that it neither iteration clips by more than 20%
				// we should try splitting the curve in case we are looking for multiple intersections
				//Debug.Write("Clip" + ImageIndex.ToString + ": " + lowBound.ToString("0.0000") + " -> " + hiBound.ToString("0.0000"))
				//Debug.WriteLine(";  P = " + Curve.GetString(P) + ";  Q = " + Curve.GetString(Q))
				if (failedPrevious && failedToRestrict)
				{
					// Both this iteration and the previous one had failed to improve things much, therefore split
					// I'm going to split the original curve, not Pnew, just to avoid the necessity of mapping all the T values back and forth
					// Debug.WriteLine("Failed to improve, splitting...")
					return FatClipSplit(P, Q, depth);
				}
#if DEBUG
				if (SaveDiagnostics)
					DrawFatClipDiagnosticDistanceImage(D, line, lowBound, hiBound);
#endif
				PointF[] Pnew = (PointF[])P.Clone();
				if (lowBound > 0)
				{
					PointF[] split = Bezier.Split(Pnew, lowBound);
					// has returned seven points describing to Bezier curves; we need to keep the last four, describing the region to keep
					Pnew = new[] { split[3], split[4], split[5], split[6] };
					// and need to adjust the high bound to be a parameter within this new curve
					hiBound = 1 - (1 - hiBound) / (1 - lowBound);
				}
				if (hiBound < 1)
				{
					PointF[] split = Bezier.Split(Pnew, hiBound);
					Pnew = new[] { split[0], split[1], split[2], split[3] };
				}
				// And now try clipping the other curve
#if DEBUG
				if (SaveDiagnostics)
					DrawFatClipDiagnosticImage(P, Q, line, Pnew);
#endif
				List<BezierParameterPair> intersections = FatClip(Q, Pnew, failedToRestrict, depth + 1);
				if (intersections == null)
					return null; // failure
				// the list can contain any number of intersections
				for (int index = 0; index <= intersections.Count - 1; index++)
				{
					// the Q parameter value we can just return:
					float Tq = intersections[index].Tp;
					float Tp = intersections[index].Tq; // however this will need to be adjusted back from the partial curve that was passed to the next iteration
														// to get the parameter for the original curve that we were given
					if (hiBound < 1)
						Tp = Tp * hiBound;
					if (lowBound > 0)
						Tp = Tp * (1 - lowBound) + lowBound;
					intersections[index] = new BezierParameterPair(intersections[index], Tp, Tq);
				}
				return intersections;
			}
		}

		private static List<BezierParameterPair> FatClipSplit(PointF[] P, PointF[] Q, int depth)
		{
			// implements the part of the above function where it looks like we might have multiple solutions, and split one of the curves into two.
			// Sederberg says to split the one with the greatest remaining parameter range, however the implementation doesn't remember the range within the original parameter
			// therefore I will just split the longest curve
			// the first part added in response to splatter test case 8.  If all of P Q share a y-coordinate (or possibly x-coordinate?)  But don't overlap it gets stuck
			// check the bounding rectangles overlap at all
			RectangleF rctP = new RectangleF(P[0], new SizeF(0, 0));
			RectangleF rctQ = new RectangleF(Q[0], new SizeF(0, 0));
			for (int index = 1; index <= 3; index++)
			{
				Geometry.Extend(ref rctP, P[index]);
				Geometry.Extend(ref rctQ, Q[index]);
			}
			if (!rctP.Intersects(rctQ))
				return null;
			List<BezierParameterPair> intersections = new List<BezierParameterPair>();
			if (Bezier.NominalLength(P) > Bezier.NominalLength(Q))
			{
				PointF[] split = Bezier.Split(P, 0.5F); // curve is always split into two equal parts
				List<BezierParameterPair> partialIntersections = FatClip(split.ExtractBezierSegment(0), Q, false, depth + 1); // New PointF()  {aSplit(0), aSplit(1), aSplit(2), aSplit(3)}, Q, False, intDepth + 1)
				if (partialIntersections != null)
				{
					for (int index = 0; index <= partialIntersections.Count - 1; index++)
					{
						intersections.Add(new BezierParameterPair(partialIntersections[index], partialIntersections[index].Tp / 2, partialIntersections[index].Tq));
					}
				}
				//Debug.WriteLine("Trying hi split of P")
				partialIntersections = FatClip(split.ExtractBezierSegment(3), Q, false, depth + 1); //  New PointF() { aSplit(3), aSplit(4), aSplit(5), aSplit(6)}, Q, False, intDepth + 1)
				if (partialIntersections != null)
				{
					for (int index = 0; index <= partialIntersections.Count - 1; index++)
					{
						intersections.Add(new BezierParameterPair(partialIntersections[index], partialIntersections[index].Tp / 2 + 0.5f, partialIntersections[index].Tq));
					}
				}
			}
			else
			{
				//Debug.WriteLine("Trying low split of Q")
				PointF[] split = Bezier.Split(Q, 0.5F); // curve is always split into two equal parts
				List<BezierParameterPair> partialIntersections = FatClip(P, new PointF[] { split[0], split[1], split[2], split[3] }, false, depth + 1);
				if (partialIntersections != null)
				{
					for (int index = 0; index <= partialIntersections.Count - 1; index++)
					{
						intersections.Add(new BezierParameterPair(partialIntersections[index], partialIntersections[index].Tp, partialIntersections[index].Tq / 2));
					}
				}
				//Debug.WriteLine("Trying hi split of Q")
				partialIntersections = FatClip(P, new PointF[] { split[3], split[4], split[5], split[6] }, false, depth + 1);
				if (partialIntersections != null)
				{
					for (int index = 0; index <= partialIntersections.Count - 1; index++)
					{
						intersections.Add(new BezierParameterPair(partialIntersections[index], partialIntersections[index].Tp, partialIntersections[index].Tq / 2 + 0.5f));
					}
				}
			}
			if (intersections.Count == 0)
				return null;
			return intersections;
		}

		private static void FatLineClipDistance(PointF Da, PointF Db, Geometry.FatLine line, ref float lowBound, ref float highBound)
		{
			//Da and Db are any two of the control points on the D Bezier curve; the last two parameters of the bounds we have placed so far on the intersection parameter
			if (Da.Y >= line.Dmin && Db.Y >= line.Dmin && Da.Y <= line.Dmax && Db.Y <= line.Dmax)
			{
				// both points within the distance tolerance.  This restricts how much we can discard to the limits of this line
				lowBound = Math.Min(Math.Min(Da.X, Db.X), lowBound);
				highBound = Math.Max(Math.Max(Da.X, Db.X), highBound);
				return;
			}
			if ((Da.Y < line.Dmin) ^ (Db.Y < line.Dmin))
			{
				// one point below min.  cross at X...
				float X = (Da.X * (Db.Y - line.Dmin) + Db.X * (line.Dmin - Da.Y)) / (Db.Y - Da.Y);
				// whether we are restricting the low or high bound depends on whether the line is ascending or descending
				if (Da.Y < line.Dmin)
				{
					lowBound = Math.Min(X, lowBound);
					if (Db.Y <= line.Dmax) // entire line lies below the maximum distance; we can restrict the other end as well
						highBound = Math.Max(Math.Max(Da.X, Db.X), highBound);
				}
				else
				{
					highBound = Math.Max(X, highBound);
					if (Da.Y <= line.Dmax)
						lowBound = Math.Min(Math.Min(Da.X, Db.X), lowBound);
				}
			}
			if ((Da.Y > line.Dmax) ^ (Db.Y > line.Dmax))
			{
				float X = (Da.X * (Db.Y - line.Dmax) + Db.X * (line.Dmax - Da.Y)) / (Db.Y - Da.Y);
				if (Db.Y > line.Dmax)
				{
					highBound = Math.Max(X, highBound);
					if (Da.Y >= line.Dmin)
						lowBound = Math.Min(Math.Min(Da.X, Db.X), lowBound);
				}
				else
				{
					lowBound = Math.Min(X, lowBound);
					if (Db.Y >= line.Dmin)
						highBound = Math.Max(Math.Max(Da.X, Db.X), highBound);
				}
			}
			// Otherwise if both points outside the line, it does not restrict at all
		}

		#region Testing functions for Bezier Bezier intersection
#if DEBUG
		public static bool SaveDiagnostics = false;
		private static int ImageIndex;
		private static void DrawFatClipDiagnosticImage(PointF[] P, PointF[] Q, Geometry.FatLine line, PointF[] Pnew)
		{
			const int SIZE = 1000;
			using (Bitmap bmp = new Bitmap(SIZE, SIZE))
			{
				using (Graphics gr = Graphics.FromImage(bmp))
				{
					gr.Clear(Color.White);
					RectangleF bounds = new RectangleF(P[0], new Size(0, 0));
					Geometry.Extend(ref bounds, Q[0]);
					for (int index = 1; index <= 3; index++)
					{
						Geometry.Extend(ref bounds, P[index]);
						Geometry.Extend(ref bounds, Q[index]);
					}
					float penWidth = (bounds.Width + bounds.Height) / SIZE;
					gr.ScaleTransform(SIZE / bounds.Width, SIZE / bounds.Height);
					gr.TranslateTransform(-bounds.Left, -bounds.Top);
					Pen pn = new Pen(Color.Black, penWidth);
					gr.DrawBezier(pn, P[0], P[1], P[2], P[3]);
					gr.DrawBezier(pn, Q[0], Q[1], Q[2], Q[3]);
					pn.Dispose();
					pn = new Pen(Color.FromArgb(100, Color.Blue), penWidth);
					gr.DrawLine(pn, 0, (int)line.Y(0), 1000, (int)line.Y(1000));
					gr.DrawLine(pn, 0, (int)line.Ymax(0), 1000, (int)line.Ymax(1000));
					gr.DrawLine(pn, 0, (int)line.Ymin(0), 1000, (int)line.Ymin(1000));
					pn.Dispose();
					pn = new Pen(Color.Red, penWidth);
					gr.DrawBezier(pn, Pnew[0], Pnew[1], Pnew[2], Pnew[3]);
				}

				bmp.Save("g:\\temp\\Bezier\\" + ImageIndex + ".png", System.Drawing.Imaging.ImageFormat.Png);
				ImageIndex += 1;
			}

		}

		private static void DrawFatClipDiagnosticDistanceImage(PointF[] D, Geometry.FatLine line, float lowBound, float hiBound)
		{
			// draws a diagnostic image for the distance function Bezier curve
			const int size = 1000;
			using (Bitmap bmp = new Bitmap(size, size))
			{
				using (Graphics gr = Graphics.FromImage(bmp))
				{
					gr.Clear(Color.White);
					RectangleF bounds = new RectangleF(D[0], new Size(0, 0));
					for (int index = 1; index <= 3; index++)
					{
						Geometry.Extend(ref bounds, D[index]);
					}
					Geometry.Extend(ref bounds, new PointF(0, line.Dmin));
					Geometry.Extend(ref bounds, new PointF(1, line.Dmax));
					const float penWidth = 0.01f; // (rctBounds.Width + rctBounds.Height) / intSize
					gr.ScaleTransform(size / bounds.Width, size / bounds.Height);
					gr.TranslateTransform(-bounds.Left, -bounds.Top);
					Pen pn = new Pen(Color.Black, penWidth);
					gr.DrawBezier(pn, D[0], D[1], D[2], D[3]);
					pn.Dispose();
					pn = new Pen(Color.FromArgb(140, Color.Blue), penWidth);
					gr.DrawLine(pn, 0, (int)line.Dmin, 1, (int)line.Dmin);
					gr.DrawLine(pn, 0, (int)line.Dmax, 1, (int)line.Dmax);
					pn.Dispose();
					pn = new Pen(Color.FromArgb(255, Color.Blue), penWidth);
					gr.DrawLine(pn, 0, 0, 1, 0);
					pn.Dispose();
					pn = new Pen(Color.FromArgb(90, Color.Green), penWidth);
					gr.DrawLine(pn, D[0], D[1]);
					gr.DrawLine(pn, D[0], D[2]);
					gr.DrawLine(pn, D[0], D[3]);
					gr.DrawLine(pn, D[1], D[2]);
					gr.DrawLine(pn, D[1], D[3]);
					gr.DrawLine(pn, D[2], D[3]);
					pn.Dispose();
					pn = new Pen(Color.Red, penWidth);
					gr.DrawLine(pn, (int)lowBound, -1000, (int)lowBound, 1000); // vertical lines at the minimum and maximum parameter bounds chosen
					gr.DrawLine(pn, (int)hiBound, -1000, (int)hiBound, 1000);
				}

				bmp.Save("g:\\temp\\Bezier\\" + ImageIndex + "d.png", System.Drawing.Imaging.ImageFormat.Png);
			}

		}

		public static void TestIntersect()
		{
			foreach (string file in System.IO.Directory.GetFiles("g:\\temp\\Bezier"))
			{
				System.IO.File.Delete(file);
			}

			PointF[] P = new PointF[4];
			P[0] = new PointF(0, 0);
			P[1] = new PointF(0, 500);
			P[2] = new PointF(500, 1000);
			P[3] = new PointF(1000, 1000);
			PointF[] Q = new PointF[4];
			Q[0] = new PointF(0, 1000);
			Q[1] = new PointF(500, 1000);
			Q[2] = new PointF(1000, 500);
			Q[3] = new PointF(1000, 0);

			List<BezierParameterPair> intersect = FatClip(P, Q, false, 0);
			if (intersect != null)
			{
				for (int index = 0; index <= intersect.Count - 1; index++)
				{
					Debug.WriteLine("Tp = " + intersect[index].Tp + " & Tq = " + intersect[index].Tq);
					Debug.WriteLine("Tp coordinates: " + Bezier.GetPoint(P, intersect[index].Tp));
					Debug.WriteLine("Tq coordinates: " + Bezier.GetPoint(Q, intersect[index].Tq));
				}
			}
			else
				Debug.WriteLine("No intersections found");
		}
#endif
		#endregion

		// although these return BezierParameterPair, both parameters are always set to the same thing
		internal static List<BezierParameterPair> Bezier_LineIntersection(PointF[] P, PointF lineA, PointF lineB)
		{
			// this is like the FatLine clip in many ways, using the same distance function.  returns list of T parameters where intersection occurs
			// P[] is a Bezier curve
			return Bezier_LineIntersection2(P, lineA, lineB, 0);
		}

		private static List<BezierParameterPair> Bezier_LineIntersection2(PointF[] P, PointF lineA, PointF lineB, int depth)
		{
			// check if line reaches bounding rectangle of P
			// this IS done during recursion - I think this is a sufficient test that refinement of the P parameter hasn't gone off the end of the line
			// must be before the completion test (as can get completion in a single step if EXTENDED line touches end of bezier)
			// the problem is this can reject some valid results when the lines are almost orthogonal and the previous step restricted hugely
			// but rounding errors give slightly the wrong coords - fixed by previous iteration detecting this
			RectangleF bounds = Bezier.BoundingRect(P);
			RectangleF lineRect = new RectangleF(lineA, SizeF.Empty);
			Geometry.Extend(ref lineRect, lineB);
			// sufficient to use line bound box as we are interested in whether P is off the end of the line,
			// the code below will deal with being to one side
			if (!bounds.Intersects(lineRect))
				return null;

			if (depth > 0 && Bezier.NominalLength(P) < BEZIER_BEZIER_INTERSECTION_ACCURACY)
			{
				// since both entire curves are considered accurate enough, we just return the parameters 0.5, 0.5.
				// As the recursion unwinds these will be adjusted backwards to the parameters of the original curves
				return new List<BezierParameterPair> {new BezierParameterPair(0.5f, 0.5f, Bezier.GetPoint(P, 0.5f), depth)};
			}

			Geometry.FatLine line = new Geometry.FatLine(lineA, lineB); // FatLine matching this line with Dmin=Dmax = 0
			PointF[] D =
			{			// (signed) distance function - as EXPLICIT bezier.  Distance from P to line at t - where t is same in D(t) and P(t)
			// the Y of each point is the distance; the X of each point just matches the parameter
				new PointF(0, line.DistanceTo(P[0])),
				new PointF(1 / 3f, line.DistanceTo(P[1])),
				new PointF(2 / 3f, line.DistanceTo(P[2])),
				new PointF(1, line.DistanceTo(P[3]))
			};
			float lowBound = 1; // bounds of region WITHIN distance
			float highBound = 0;
			// check each line forming convex hull of D (check all lines Dn - Dm, without checking which are external)
			// to see what limits they place on t for D to be within fatline
			FatLineClipDistance(D[0], D[1], line, ref lowBound, ref highBound);
			FatLineClipDistance(D[0], D[2], line, ref lowBound, ref highBound);
			FatLineClipDistance(D[0], D[3], line, ref lowBound, ref highBound);
			FatLineClipDistance(D[1], D[2], line, ref lowBound, ref highBound);
			FatLineClipDistance(D[1], D[3], line, ref lowBound, ref highBound);
			FatLineClipDistance(D[2], D[3], line, ref lowBound, ref highBound);
			// Debug.WriteLine("Bounded within: " + lowBound.ToString("0.###") + "->" + hiBound.ToString("0.###"))
			if (highBound < lowBound)
			{
				// failed to find any intersection
				return null;
			}
			else
			{
				// we can discard areas that we know to be outside the intersection region.  The intersection region should be in the middle,
				// with a discardable area above, below or both.  Occasionally there will be no discarded area
				// lowBound, hiBound is the range within which we have deduced that the P parameter must lie
				bool failedToRestrict = highBound - lowBound > 0.9;
				List<BezierParameterPair> intersection;
				if (failedToRestrict)
				{
					// failed to improve - multiple solutions presumably.
					// Try splitting curve and retrying
					intersection = new List<BezierParameterPair>();
					PointF[] split = Bezier.Split(P, 0.5F); // curve is always split into two equal parts
					List<BezierParameterPair> partialIntersection = Bezier_LineIntersection2(new[] { split[0], split[1], split[2], split[3] }, lineA, lineB, depth + 1);
					if (partialIntersection != null)
					{
						for (int index = 0; index <= partialIntersection.Count - 1; index++)
						{
							float newP = partialIntersection[index].Tp / 2;
							intersection.Add(new BezierParameterPair(partialIntersection[index], newP, newP));
						}
					}
					partialIntersection = Bezier_LineIntersection2(new[] { split[3], split[4], split[5], split[6] }, lineA, lineB, depth + 1);
					if (partialIntersection != null)
					{
						for (int index = 0; index <= partialIntersection.Count - 1; index++)
						{
							float newP = partialIntersection[index].Tp / 2;
							intersection.Add(new BezierParameterPair(partialIntersection[index], newP + 0.5f, newP + 0.5f));
						}
					}
					return intersection;
				}
				// DrawFatClipDiagnosticDistanceImage(D, objLine, lowBound, hiBound)
				PointF[] Pnew = (PointF[])P.Clone();
				if (lowBound > 0)
				{
					Pnew = Bezier.Split(Pnew, lowBound, true);
					// and need to adjust the high bound to be a parameter within this new curve
					highBound = 1 - (1 - highBound) / (1 - lowBound);
				}
				if (highBound < 1)
					Pnew = Bezier.Split(Pnew, highBound, false);
				// do an immediate check for good sufficient solution.  If v good iterating sometimes fails due to rounding errors causing the bounds to not intersect
				if (highBound < 0.01 && Bezier.NominalLength(Pnew) < BEZIER_BEZIER_INTERSECTION_ACCURACY)
				{
					// mainly second condition which is important;  but first is faster and checks that this iteration has restricted a lot
					// also check not off end of straight line... (can check any of Pnew as they are basically the same)
					if (Geometry.PointWithinLineExtent(lineA, lineB, Pnew[0]))
					{
						intersection = new List<BezierParameterPair>();
						intersection.Add(new BezierParameterPair(0.5f, 0.5f, Bezier.GetPoint(Pnew, 0.5F), depth));
					}
					else
						return null; // failed - crossing point would be off end of straight line
				}
				else
					intersection = Bezier_LineIntersection2(Pnew, lineA, lineB, depth + 1);
				// and now try again using this reduced curve
				if (intersection == null)
					return null; // failure
								 // the list can contain any number of intersections
				for (int index = 0; index <= intersection.Count - 1; index++)
				{
					// the Q parameter value we can just return:
					float Tp = intersection[index].Tp; // however this will need to be adjusted back from the partial curve that was passed to the next iteration
													   // to get the parameter for the original curve that we were given
					if (highBound < 1)
						Tp = Tp * highBound; // note that HiBound was adjusted above
					if (lowBound > 0)
						Tp = Tp * (1 - lowBound) + lowBound;
					intersection[index] = new BezierParameterPair(intersection[index], Tp, Tp);
				}
				return intersection;
			}
		}

		internal static float CertaintyFromDepth(int depth)
		{
			return Geometry.ANGLE90 - 0.15f * depth;
		}

		#endregion

	}

}
