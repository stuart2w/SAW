using System;
using System.Drawing;
using System.Diagnostics;

// general bezier info https://pomax.github.io/bezierinfo/
namespace SAW
{
	/// <summary>Helper functions for Bezier curves.  Class cannot be instantiated</summary>
	public class Bezier
	{
		// mostly uses PointF(3) which is start-ctr1-ctr2-end, as returned by GetSingleCurve

		private Bezier()
		{
		}

		internal static RectangleF BoundingRect(PointF[] P)
		{
			RectangleF rect = new RectangleF(P[0], SizeF.Empty);
			Geometry.Extend(ref rect, P[1]);
			Geometry.Extend(ref rect, P[2]);
			Geometry.Extend(ref rect, P[3]);
			return rect;
		}

		/// <summary>returns a single point on the Bezier curve between ptStart and ptEnd; changing T from 0 to 1 returns all of the points</summary>
		internal static PointF GetPoint(PointF start, PointF end, PointF control1, PointF control2, float T)
		{
			// see http://en.wikipedia.org/wiki/B%C3%A9zier_curve
			Debug.Assert(T >= 0 && T <= 1);
			float X = start.X * (float)Math.Pow(1 - T, 3) + 3 * control1.X * T * (1 - T) * (1 - T) + 3 * control2.X * T * T * (1 - T) + end.X * (float)Math.Pow(T, 3);
			float Y = start.Y * (float)Math.Pow(1 - T, 3) + 3 * control1.Y * T * (1 - T) * (1 - T) + 3 * control2.Y * T * T * (1 - T) + end.Y * (float)Math.Pow(T, 3);
			return new PointF(X, Y);
		}

		/// <summary>returns a single point on the Bezier curve between ptStart and ptEnd; changing T from 0 to 1 returns all of the points</summary>
		internal static PointF GetPoint(PointF[] P, float T)
		{
			// see http://en.wikipedia.org/wiki/B%C3%A9zier_curve
			Debug.Assert(P.Length == 4);
			Debug.Assert(T >= 0 && T <= 1);
			float X = (float)(P[0].X * Math.Pow(1 - T, 3) + 3 * P[1].X * T * (1 - T) * (1 - T) + 3 * P[2].X * T * T * (1 - T) + P[3].X * Math.Pow(T, 3));
			float Y = (float)(P[0].Y * Math.Pow(1 - T, 3) + 3 * P[1].Y * T * (1 - T) * (1 - T) + 3 * P[2].Y * T * T * (1 - T) + P[3].Y * Math.Pow(T, 3));
			return new PointF(X, Y);
		}

		/// <summary>returns a nominal length for a given Bezier curve by just taking two thirds of total distance between the base points and control points.
		/// THIS IS NOT ACCURATE</summary>
		internal static float NominalLength(PointF[] P)
		{
			return (Geometry.DistanceBetween(P[0], P[1]) + Geometry.DistanceBetween(P[1], P[2]) + Geometry.DistanceBetween(P[3], P[3])) * 0.66f;
		}

		/// <summary>Text representation of a Bezier.  Used for diagnostics.</summary>
		internal static string GetString(PointF[] P)
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder(); // assuming points listed in sequential order
			output.Append("{");
			for (int index = 0; index <= P.Length - 1; index++)
			{
				if (index > 0)
					output.Append("; ");
				output.Append("(").Append(P[index].X.ToString("0.###")).Append(",");
				output.Append(P[index].Y.ToString("0.###")).Append(")");
			}
			output.Append("}");
			return output.ToString();
		}

		internal static PointF[] Reverse(PointF[] line)
		{
			PointF[] reversed = new PointF[4];
			for (int index = 0; index <= 3; index++)
			{
				reversed[index] = line[3 - index];
			}
			return reversed;
		}

		/// <summary>Returns closest T value to point by binary search</summary>
		internal static float FindNearestApproachToPoint(PointF test, PointF[] P, float maximumError = 0.001f)
		{
			float lowT = 0;
			float hiT = 1;
			float lowDistance = Geometry.DistanceBetween(test, P[0]);
			float hiDistance = Geometry.DistanceBetween(test, P[3]);
			float half = 0;
			while (hiT - lowT > maximumError)
			{
				half = (lowT + hiT) / 2;
				PointF newPoint = Bezier.GetPoint(P, half);
				float testDistance = Geometry.DistanceBetween(test, newPoint);
				if (lowDistance > hiDistance)
				{
					lowDistance = testDistance;
					lowT = half;
				}
				else
				{
					hiDistance = testDistance;
					hiT = half;
				}
			}
			return half;
		}

		#region deCasteljau Bezier splitting
		// this is used to split a Bezier into two sections.  See Bezier PDF, chapter 2
		// this can act on an arbitrary Bezier, because it is used iteratively during intersection testing
		internal static PointF[] Split(PointF[] P, float T)
		{
			// the numbering of the points is taken from the PDF notes.  P00, P03 are the endpoints, P01, P02 are the control points
			// T is the position within the Bezier at which to split it (assuming Bezier ranges from T=0 to T=1)
			PointF P10 = Geometry.Interpolate(P[0], P[1], T);
			PointF P11 = Geometry.Interpolate(P[1], P[2], T);
			PointF P12 = Geometry.Interpolate(P[2], P[3], T);
			PointF P20 = Geometry.Interpolate(P10, P11, T);
			PointF P21 = Geometry.Interpolate(P11, P12, T);
			PointF P30 = Geometry.Interpolate(P20, P21, T);
			// returns the points of the two arcs in the order vertex, control, control, shared vertex, control, control, vertex
			return new PointF[] { P[0], P10, P20, P30, P21, P12, P[3] };
		}

		/// <summary>just returns 4 points forming one of the split parts.  If the last parameter is true, it returns the part for the high T (P3-P6)
			/// otherwise the low T (P0-P3)</summary>
		internal static PointF[] Split(PointF[] P, float T, bool returnLatterPart)
		{
			PointF P10 = Geometry.Interpolate(P[0], P[1], T); // We need to calculate all the points whichever piece we are returning
			PointF P11 = Geometry.Interpolate(P[1], P[2], T);
			PointF P12 = Geometry.Interpolate(P[2], P[3], T);
			PointF P20 = Geometry.Interpolate(P10, P11, T);
			PointF P21 = Geometry.Interpolate(P11, P12, T);
			PointF P30 = Geometry.Interpolate(P20, P21, T);
			if (returnLatterPart)
				return new[] {P30, P21, P12, P[3]};
			return new[] {P[0], P10, P20, P30};
		}

		#endregion

	}
}
