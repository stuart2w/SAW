using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;


namespace SAW
{
	public static class Geometry
	{
		// support for various geometry calculations

		public const float MILLIMETRE = 1; //= 96 / 25.4 ' number of data units in 1 mm
		public const float THINLINE = 0.2f;
		public const float SINGLELINE = 0.4f; // now using 0.4 as this is default in AM4 // 0.25 ' the thickness of a single thickness line in millimetres (i.e. roughly 1 pixel)
		public const float TRANSPARENTLINE = 0.8f; // width of a transparent line, e.g. transformation focus
												   /// <summary>length of 1 inch in data units (this is the exact value used by .net)</summary>
		public const float INCH = 25.4f;
		/// <summary>Points size in data unit/mm (ie mm per point)</summary>
		public const float POINTUNIT = INCH / 72; // expressed as data units, as above.
		public readonly static Matrix IdentityMatrix = new Matrix(); // automatically creates the identity one; this instance is only stored to avoid creating loads of pointless objects

		public const float PI = (float)Math.PI;

		#region Rectangles
			/// <summary>extends the rectangle to include this point (like RectangleF.Union, but using a PointF as the second parameter)
		/// modifies the rectangle it is applied to</summary>
		internal static void Extend(ref RectangleF rct, PointF pt)
		{
			if (rct.Equals(RectangleF.Empty)) // checking IsEmpty is not sufficient, because this returned rectangle is actually empty
			{
				if (pt.IsEmpty) // must give this some nominal size, because if we return rectangle (0, 0, 0, 0) it will be ignored next time this function is called
					// and treated as the empty rectangle
					rct = new RectangleF(pt, new SizeF(NEGLIGIBLE, NEGLIGIBLE));
				else
					rct = new RectangleF(pt, new SizeF(0, 0));
				return;
			}
			float difference;
			if (pt.X < rct.X)
			{
				difference = rct.X - pt.X;
				rct.Width += difference;
				rct.X -= difference;
			}
			else if (pt.X >= rct.Right)
				rct.Width += pt.X - rct.Right;
			if (pt.Y < rct.Y)
			{
				difference = rct.Y - pt.Y;
				rct.Height += difference;
				rct.Y -= difference;
			}
			else if (pt.Y >= rct.Bottom)
				rct.Height += pt.Y - rct.Bottom;
		}

		internal static void Extend(ref RectangleF rct, IEnumerable<PointF> points)
		{
			foreach (var pt in points)
			{
				Extend(ref rct, pt);
			}
		}

		/// <summary>Unions the rectangles, but allows starting with an empty rectangle.  Updates the given rectangle.</summary>
		internal static void Extend(ref RectangleF update, RectangleF add)
		{
			// the main motivation for this is that RectangleF.Union cannot union an empty rectangle with something else (it will include 0, 0)
			// whereas we would like to start with an empty rectangle and then add an arbitrary number of other rectangles to it
			// this function is also different in that we update the first parameter rather than returning a new rectangle;
			// just because this is the way that suits this code best
			if (update.Equals(RectangleF.Empty)) // not the same as IsEmpty !
				update = add;
			else if (!add.Equals(RectangleF.Empty))
				update = RectangleF.Union(update, add);
		}

		internal static void Extend(ref Rectangle update, Rectangle add)
		{
			if (update.Equals(Rectangle.Empty))
				update = add;
			else if (!add.Equals(Rectangle.Empty))
				update = Rectangle.Union(update, add);
		}

		internal static RectangleF UnionRectangles(IEnumerable<RectangleF> rectangles)
		{
			return UnionRectangles(rectangles, RectangleF.Empty);
		}
		internal static RectangleF UnionRectangles(IEnumerable<RectangleF> rectangles, RectangleF initial)
		{
			foreach (RectangleF rct in rectangles)
				Extend(ref initial, rct);
			return initial;
		}

		/// <summary>Returns a square with a given centre and 'radius' (ie half the length of one side)</summary>
		internal static RectangleF RectangleFromPoint(PointF centre, float radius)
		{
			return new RectangleF(centre.X - radius, centre.Y - radius, radius * 2, radius * 2);
		}

		internal static bool ContainsApprox(this RectangleF outer, RectangleF inner)
		{
			if (inner.X < outer.X - NEGLIGIBLE || inner.Y < outer.Y - NEGLIGIBLE)
				return false;
			if (inner.X > outer.Right + NEGLIGIBLE || inner.Y > outer.Bottom + NEGLIGIBLE)
				return false;
			if (inner.Right < outer.X - NEGLIGIBLE || inner.Bottom < outer.Y - NEGLIGIBLE)
				return false;
			if (inner.Right > outer.Right + NEGLIGIBLE || inner.Bottom > outer.Bottom + NEGLIGIBLE)
				return false;
			return true;
		}

		internal static bool ContainsApprox(this RectangleF outer, PointF pt)
		{
			if (pt.X < outer.X - NEGLIGIBLE || pt.X > outer.Right + NEGLIGIBLE)
				return false;
			if (pt.Y < outer.Y - NEGLIGIBLE || pt.Y > outer.Bottom + NEGLIGIBLE)
				return false;
			return true;
		}

		/// <summary>this is (almost) simply RectangleF.Intersect (rctA, rctB).IsEmpty
			/// however it is presumably faster as we do not actually need to calculate the intersection, just check if there is one
			/// in fact this is required to detect intersections if one rectangle has 0 width or height (used in Connector.PrimaryVectorConflicts)
			/// as that would return an empty rectangle</summary>
		internal static bool Intersects(this RectangleF A, RectangleF B)
		{
			if (A.Right < B.X)
				return false;
			if (A.X > B.Right)
				return false;
			if (A.Bottom < B.Y)
				return false;
			if (A.Y > B.Bottom)
				return false;
			return true;
		}

		internal static bool IntersectsApprox(this RectangleF A, RectangleF B)
		{
			// as above, but true if they are within NEGLIGIBLE of each other - used for intersections where near misses usually count as a hit
			if (A.Right < B.X - NEGLIGIBLE)
				return false;
			if (A.X > B.Right + NEGLIGIBLE)
				return false;
			if (A.Bottom < B.Y - NEGLIGIBLE)
				return false;
			if (A.Y > B.Bottom + NEGLIGIBLE)
				return false;
			return true;
		}

		/// <summary>returns the rectangle with corners at the 2 points - but they are not necessarily TL and BR, rather can be either pair of opposite corners listed in either order</summary>
		internal static RectangleF RectangleFromPoints(PointF A, PointF B)
		{
			return new RectangleF
			{
				X = Math.Min(A.X, B.X),
				Y = Math.Min(A.Y, B.Y),
				Width = Math.Abs(A.X - B.X),
				Height = Math.Abs(A.Y - B.Y)
			};
		}

		/// <summary>Returns the 4 corners of rct, always right-hand wound (TL, TR, BR, BL)</summary>
		internal static PointF[] GetPoints(this RectangleF rct)
		{
			return new[] { rct.Location, new PointF(rct.Right, rct.Top), new PointF(rct.Right, rct.Bottom), new PointF(rct.Left, rct.Bottom) };
		}

		/// <summary>returns the 3 corners as used by Windows graphics</summary>
		internal static PointF[] GetThreePoints(this RectangleF rct)
		{
			return new[] { rct.Location, new PointF(rct.Right, rct.Top), new PointF(rct.Left, rct.Bottom) };
		}

		internal static RectangleF ToRectangleF(this Rectangle rct)
		{
			return new RectangleF(rct.X, rct.Y, rct.Width, rct.Height);
		}

		internal static RectangleF MultiplyBy(this RectangleF rct, float scale)
		{
			return new RectangleF(rct.X * scale, rct.Y * scale, rct.Width * scale, rct.Height * scale);
		}

		/// <summary>Returns the same rectangle expressed with positive width and height (negative can be used to flip images when drawing)</summary>
		internal static RectangleF Normalise(this RectangleF rct)
		{
			var X = rct.X;
			var Y = rct.Y;
			if (rct.Width < 0)
				X += rct.Width;
			if (rct.Height < 0)
				Y += rct.Height;
			return new RectangleF(X, Y, Math.Abs(rct.Width), Math.Abs(rct.Height));
		}

		#endregion

		#region Basic vector/size stuff
		//extensions - sadly extension operators aren't possible :-(
		public static Size VectorTo(this Point pt1, Point pt2) // from 1 to 2, ie 2-1
		{
			return new Size(pt2.X - pt1.X, pt2.Y - pt1.Y);
		}

		public static SizeF VectorTo(this PointF pt1, PointF pt2) // from 1 to 2, ie 2-1
		{
			return new SizeF(pt2.X - pt1.X, pt2.Y - pt1.Y);
		}

		public static Size Subtract(this Point pt1, Point pt2) // from 2 to 1, ie 1-2
		{
			return new Size(pt1.X - pt2.X, pt1.Y - pt2.Y);
		}

		public static SizeF Subtract(this PointF pt1, PointF pt2) // from 2 to 1, ie 1-2
		{
			return new SizeF(pt1.X - pt2.X, pt1.Y - pt2.Y);
		}

		/// <summary>Scale factor (multiplies both width and height) by the given value</summary>
		internal static SizeF MultiplyBy(this SizeF vector, float multiplyBy)
		{
			return new SizeF(vector.Width * multiplyBy, vector.Height * multiplyBy);
		}

		/// <summary>Returns the diagonal length of the vector</summary>
		internal static float Length(this SizeF vector)
		{
			return (float)Math.Sqrt(vector.Width * vector.Width + vector.Height * vector.Height);
		}

		/// <summary>Returns the area of a rectangle implied by this vector (simply X*Y)</summary>
		internal static float Area(this SizeF vector)
		{
			return vector.Width * vector.Height;
		}

		internal static float Length(this Size vector)
		{
			// returns the length of the given vector
			return (float)Math.Sqrt(vector.Width * vector.Width + vector.Height * vector.Height);
		}

		internal static SizeF ChangeLength(this SizeF vector, float length)
		{
			float multiply = length / vector.Length();
			return new SizeF(vector.Width * multiply, vector.Height * multiply);
		}

		internal static SizeF ToSizeF(this Size sz)
		{
			return new SizeF(sz.Width, sz.Height);
		}

		internal static Size ToSize(this Point pt)
		{
			return new Size(pt.X,pt.Y);
		}


		/// <summary>Flips between landscape and horizontal</summary>
		public static SizeF Flip(this SizeF sz)
		{
			return new SizeF(sz.Height, sz.Width);
		}

		/// <summary>Returns the max of each dimension from this and parameter (may be one dimension from each size)</summary>
		internal static SizeF Max(this SizeF sz1, SizeF sz2)
		{
			return new SizeF(Math.Max(sz1.Width, sz2.Width), Math.Max(sz1.Height, sz2.Height));
		}

		// older

		internal static float DistanceBetween(PointF pt1, PointF pt2)
		{
			return (float)Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
		}

		internal static PointF MidPoint(PointF ptA, PointF ptB)
		{
			return new PointF((ptA.X + ptB.X) / 2, (ptA.Y + ptB.Y) / 2);
		}

		internal static PointF Interpolate(PointF ptA, PointF ptB, float T)
		{
			// gets the point a given fraction of the distance along the line from ptA to ptB, for T=0 to T=1
			return new PointF(ptA.X * (1 - T) + ptB.X * T, ptA.Y * (1 - T) + ptB.Y * T);
		}

		internal static float DirtyDistance(PointF ptA, PointF ptB)
		{
			// a very fast way of calculating approximate distance between.  This does not give an exact answer, it is just used for checking if things are "close"
			// where the threshold is arbitrary anyway
			float x = Math.Abs(ptA.X - ptB.X);
			float y = Math.Abs(ptA.Y - ptB.Y);
			if (x > y)
				return x + y / 2;
			else
				return y + x / 2;
		}

		internal static float DirtyDistance(PointF ptA, RectangleF rctB)
		{
			//ditto, but between the point and the edge of the rectangle.  Returns 0 if the point is inside the rectangle
			float x = 0;
			if (ptA.X < rctB.X)
				x = rctB.X - ptA.X;
			else if (ptA.X > rctB.Right)
				x = ptA.X - rctB.Right;
			float y = 0;
			if (ptA.Y < rctB.Y)
				y = rctB.Y - ptA.Y;
			else if (ptA.Y > rctB.Bottom)
				y = ptA.Y - rctB.Bottom;
			if (x > y)
				return x + y / 2;
			else
				return y + x / 2;
		}

		internal static float DirtyDistance(RectangleF rctA, RectangleF rctB)
		{
			float X = 0;
			if (rctA.Right < rctB.X)
				X = rctB.X - rctA.Right;
			else if (rctA.X > rctB.Right)
				X = rctA.X - rctB.Right;
			float y = 0;
			if (rctA.Bottom < rctB.Y)
				y = rctB.Y - rctA.Bottom;
			else if (rctA.Y > rctB.Bottom)
				y = rctA.Y - rctB.Bottom;
			if (X > y)
				return X + y / 2;
			else
				return y + X / 2;
		}

		#endregion

		#region More complex vector stuff
		internal static float DotProduct(this SizeF szA, SizeF szB)
		{
			// calculates the DotProduct of the two vectors (length A * length B * cos angle between)
			return szA.Width * szB.Width + szA.Height * szB.Height;
		}

		internal static float CrossProduct(this SizeF szA, SizeF szB)
		{
			// because we are only working in two dimensions we do not return the product as a vector - it will only have a component at right angles to the page
			// we are just returning the size (and sign) of this component
			return szA.Height * szB.Width - szA.Width * szB.Height;
		}

		/// <summary>returns direction of turn of 2 subsequent lines in a shape. 1 for clockwise; 0 for parallel</summary>
		internal static int TurnDirection(SizeF szA, SizeF szB)
		{
			return -Math.Sign(szA.CrossProduct(szB));
		}

		/// <summary>returns direction of turn of 2 lines. 1 for clockwise; 0 for parallel</summary>
		internal static int TurnDirection(PointF ptA, PointF ptB, PointF ptC)
		{
			return -Math.Sign(ptA.VectorTo(ptB).CrossProduct(ptB.VectorTo(ptC)));
		}

		internal static float ProjectionScalar(SizeF project, SizeF ontoDirection)
		{
			// calculates the projection of the first parameter in the direction given by the second parameter. (i.e. length of first parameter in the direction of the second)
			//The length of the second parameter does not affect the result
			return project.DotProduct(ontoDirection) / ontoDirection.Length();
		}

		internal static SizeF ProjectionVector(SizeF project, SizeF ontoDirection)
		{
			// calculates the projection of the first parameter in the direction given by the second parameter as a vector
			// i.e. the vector would be in direction szOntoDirection, and the magnitude is the projection of szProject in this direction
			float multiply = project.DotProduct(ontoDirection) / ontoDirection.DotProduct(ontoDirection);
			// this is actually ProjectionScalar (szProject, szOnto)/Scalar (szOnto) - but it is faster written this way
			return ontoDirection.MultiplyBy(multiply);
		}

		internal static float DistancePointToLine(PointF lineA, PointF lineB, PointF test)
		{
			// returns distance between ptTest and the line which passes through the other two parameters
			// see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html (second part)
			// calculate any vector perpendicular to the line
			// this is NOT limited to the ends of the line; the line is effectively infinite.
			// When this is used in hit testing, it probably doesn't matter much, because points well off the end of the line are usually outside the bounding box
			SizeF perpendicular = new SizeF(lineA.Y - lineB.Y, lineB.X - lineA.X);
			if (perpendicular.IsEmpty)
			{
				Debug.Fail("DistancePointToLine - line is empty");
				return DistanceBetween(test, lineA); // just returns the distance from the test to either of the (identical) points
			}
			// distance is projection of vector from test to any point on the line in the perpendicular direction
			float distance = ProjectionScalar(test.VectorTo(lineA), perpendicular);
			return Math.Abs(distance);
		}

		internal static float DistancePointToLineLimited(PointF lineA, PointF lineB, PointF test)
		{
			// returns distance between ptTest and the line which runs between the two parameters
			// this is slower than the version above, but does check we have not run off the end of the line
			PointF closest = ClosestPointOnLineLimited(lineA, lineB, test, false);
			return DistanceBetween(test, closest);
		}

		internal static PointF PerpendicularPoint(PointF A, PointF B, PointF move)
		{
			// positions ptMove so that it is perpendicular to the line from ptA to ptB.  I.e. ptA - ptB - ptMove will form a rectangle
			// the movable point is opposite ptB on the rectangle
			SizeF existing = A.VectorTo(B);
			SizeF vectorTo = B.VectorTo(move);
			// calculate the component of szMove in the direction of szExisting and subtract that from szExisting just leaving the perpendicular component
			SizeF projection = ProjectionVector(vectorTo, existing);
			return PointF.Add(B, vectorTo - projection);
		}

		internal static SizeF Perpendicular(this SizeF vector, int direction)
		{
			// returns the vector rotated through 90° - which direction depends on intdirection
			switch (direction)
			{
				case -1: // anticlockwise
					return new SizeF(vector.Height, -vector.Width);
				case 1: // clockwise
					return new SizeF(-vector.Height, vector.Width);
				default:
					Debug.Fail("Perpendicular: unexpected direction");
					return vector;
			}
		}

		internal static PointF ClosestPointOnLine(PointF lineA, PointF lineB, PointF start)
		{
			// returns the point on the line described by the first two parameters which is closest to ptStart
			// the returned point may be off the end of the line
			SizeF perpendicular = new SizeF(lineB.Y - lineA.Y, lineA.X - lineB.X); // a vector perpendicular to the line, no particular length
			SizeF vector = start.VectorTo(lineA); // a random vector from ptStart on to the line
			vector = ProjectionVector(vector, perpendicular); // the vector from the point to the line
			return PointF.Add(start, vector);
		}

		internal static PointF ClosestPointOnLineLimited(PointF lineA, PointF lineB, PointF start, bool emptyIfOutside)
		{
			// as above but only returns points between A and B
			// if the last parameter is true it returns PointF.Empty if the closest point is off the line
			PointF closest = ClosestPointOnLine(lineA, lineB, start);
			float tolerance = 0;
			if (emptyIfOutside)
				tolerance = NEGLIGIBLE;
			// if we are ignoring the point completely outside the line, we need to allow for a certain amount of rounding error
			// if we are just bringing the point back inside the line, then it is probably better to actually bring it back inside even if it is just a rounding error outside
			// so in this case the outside codeTriggers even if it is within a rounding error
			if (closest.X < lineA.X - tolerance && closest.X < lineB.X - tolerance || closest.X > lineA.X + tolerance && closest.X > lineB.X + tolerance || closest.Y < lineA.Y - tolerance && closest.Y < lineB.Y - tolerance || closest.Y > lineA.Y + tolerance && closest.Y > lineB.Y + tolerance)
			{
				// off end of line
				if (emptyIfOutside)
					return PointF.Empty;
				//Dim szDistanceClosest As Single = Scalar(ptStart, ptClosest)
				float distanceA = DistanceBetween(start, lineA);
				float distanceB = DistanceBetween(start, lineB);
				return distanceA < distanceB ? lineA : lineB;
			}
			return closest; // is within the line
		}

		/// <summary>checks whether ptTest is within line A-B *ASSUMING* that we have already tested it lies on the line,
		/// *OR* we can assume it must.  (Eg intersection on given Segment).  Used especially for Ant/Segment where rounding</summary>
		internal static bool PointWithinLineExtent(PointF lineA, PointF lineB, PointF test)
		{
			// errors can actually put the point slightly off the official line, so we can't strictly check that test is within Rectangle(ptA, ptB)
			// just check in X or Y direction - wherever line is most distinct
			Debug.Assert(!lineA.ApproxEqual(lineB), "PointWithinLineExtent doesn\'t work correctly if points equal");
			// not sure about the assert - why not???  And I was getting some errors on scissors, which might be due to this pausing the code in an invalid state
			if (Math.Abs(lineA.X - lineB.X) > Math.Abs(lineA.Y - lineB.Y))
				// X more useful
				return (test.X > lineA.X - NEGLIGIBLE || test.X > lineB.X - NEGLIGIBLE) && (test.X < lineA.X + NEGLIGIBLE || test.X < lineB.X + NEGLIGIBLE);
			else
				return (test.Y > lineA.Y - NEGLIGIBLE || test.Y > lineB.Y - NEGLIGIBLE) && (test.Y < lineA.Y + NEGLIGIBLE || test.Y < lineB.Y + NEGLIGIBLE);
		}
		#endregion

		#region Angles
		internal const float ANGLEUP = 0;
		internal const float ANGLERIGHT = PI / 2;
		internal const float ANGLEDOWN = PI;
		internal const float ANGLELEFT = 3 * PI / 2;
		internal const float ANGLE360 = 2 * PI;
		internal const float ANGLE180 = PI;
		internal const float ANGLE90 = PI / 2;
		internal const float ANGLE45 = PI / 4;
		internal const float ANGLESTEPDEFAULT = PI / 12; // 15 deg angle to snap to when angle snapping

		/// <summary>returns the direction of the vector measured clockwise from vertically up (we need to use this baseline because the isometric paper)</summary>
		internal static float VectorAngle(this SizeF vector)
		{
			Debug.Assert(vector.Width != 0 || vector.Height != 0);
			//float angle = (float)Math.Atan(vector.Width / -vector.Height);
			// the above started giving problems in SAW ?!? when height=0
			float angle = (float)Math.Atan2(vector.Width , -vector.Height);
			//Debug.WriteLine($"Initial VectorAngle({vector.Width},{vector.Height}={angle}");
			if (vector.Height > 0) // was >=0 with Math.Atan
				angle += PI;
			else if (vector.Width < 0)
				angle += PI * 2;
			return angle;
		}

		internal static float VectorAngle(PointF A, PointF B)
		{
			return A.VectorTo(B).VectorAngle();
		}

		internal static SizeF ScalarToVector(float scalar, float angle)
		{
			// the angle is measured clockwise from vertically up as usual
			float X = scalar * (float)Math.Sin(angle);
			float Y = scalar * -(float)Math.Cos(angle);
			return new SizeF(X, Y);
		}

		internal static float Degrees(float radians)
		{
			// all internal work is done in radians (because that is what System.Math uses)
			// however, degrees may be needed for displayspace (both to the user and the .net graphics uses degrees sometimes)
			return radians * 180 / PI;
		}

		internal static SizeF RotateBy(this SizeF vector, float angle)
		{
			// rotates the given vector by angle radians. CLOCKWISE
			float cos = (float)Math.Cos(angle);
			float sin = (float)Math.Sin(angle);
			return new SizeF(cos * vector.Width - sin * vector.Height, sin * vector.Width + cos * vector.Height);
		}

		internal static PointF RotateBy(this PointF pt, float angle)
		{
			float cos = (float)Math.Cos(angle);
			float sin = (float)Math.Sin(angle);
			return new PointF(cos * pt.X - sin * pt.Y, sin * pt.X + cos * pt.Y);
		}

		internal static PointF RotateBy(this PointF pt, float angle, PointF about)
		{
			// Does rotation about the given point
			float cos = (float)Math.Cos(angle);
			float sin = (float)Math.Sin(angle);
			SizeF vector = about.VectorTo(pt);
			return new PointF(about.X + cos * vector.Width - sin * vector.Height, about.Y + sin * vector.Width + cos * vector.Height);
		}

		/// <summary>Returns the angle between two angles; the order of the parameters does not matter. The answer is always between the 0 and pi (0° and 180°)</summary>
		internal static float AbsoluteAngularDifference(float A, float B)
		{
			float difference = Math.Abs(A - B);
			while (difference > ANGLE360)
				difference -= ANGLE360;
			if (difference > PI)
				difference = ANGLE360 - difference;
			return difference;
		}

		internal static float AngleBetween(float A, float B, bool prefer360 = false)
		{
			// returns the angle sweeping from A to B, always as a positive number
			// IE A = 355° and B = 5° then the answer is 10°
			// if bolPrefer360 and A and B are the same it returns 360°
			Debug.Assert(A >= 0 && B >= 0 && A <= ANGLE360 && B <= ANGLE360); // I think this is assumed by the logic below; the function could be rewritten to cope with other values
			float difference = B - A;
			if (Math.Abs(difference) < NEGLIGIBLEANGLE && prefer360)
				return ANGLE360;
			if (difference < 0)
				difference += ANGLE360;
			return difference;
		}

		/// <summary>returns the angle restated as a positive number less than 360° (although in radians still)</summary>
		internal static float NormaliseAngle(float angle)
		{
			while (angle < 0)
				angle += ANGLE360;
			while (angle > ANGLE360)
				angle -= ANGLE360;
			return angle;
		}

		/// <summary>As NormaliseAngle but from -180 to +180</summary>
		internal static float NormaliseAnglePlusMinus180(float angle)
		{
			while (angle < -ANGLE180)
				angle += ANGLE360;
			while (angle > ANGLE180)
				angle -= ANGLE360;
			return angle;
		}

		internal static float Radians(float degrees)
		{
			return degrees * PI / 180;
		}

		/// <summary>returns a direction (1 clockwise, -1 anticlockwise) based upon going from A to B
			/// the only complication arises when the angles are each side of 0 (e.g. A = 355° and B = 5°, in this case B > A and direction = 1)
			/// rather than returning the angle the long way round, this assumes that they must span</summary>
		internal static int DirectionFromSimilarAngles(float A, float B)
		{
			Debug.Assert(A >= 0 && B >= 0);
			Debug.Assert(A < ANGLE360);
			if (B < A - PI) // it is more than 180° behind
				B += ANGLE360;
			return Math.Sign(B - A);
		}

		/// <summary>converts the angle (counting clockwise from vertically up) to an angle used by .net graphics IN DEGREES (e.g. DrawArc)</summary>
		internal static float DotNetAngle(float angle)
		{
			return Degrees(NormaliseAngle(angle - PI / 2));
		}

		internal static float AngleStep()
		{
			return ANGLESTEPDEFAULT;
		}

		internal static PointF AngleSnapPoint(PointF pt, PointF origin, float stepAngle = -1)
		{
			if (stepAngle <= 0)
				stepAngle = AngleStep();
			Debug.Assert(!origin.IsEmpty); // usually means dont want snap
			if (pt.ApproxEqual(origin))
				return origin; // usually happens as shape is being drawn - floated point starts on top of previous
			float angle = VectorAngle(origin, pt);
			angle = (float)(Math.Round(angle / stepAngle) * stepAngle);
			return origin + ScalarToVector(DistanceBetween(pt, origin), angle);
		}

		internal static SizeF AngleSnapVector(SizeF sz, float stepAngle = -1)
		{
			if (stepAngle <= 0)
				stepAngle = AngleStep();
			if (sz.IsEmpty)
			{
				Debug.Fail("Cannot AngleSnapVector empty vector");
				return sz;
			}
			float angle = sz.VectorAngle();
			angle = (float)(Math.Round(angle / stepAngle) * stepAngle);
			return ScalarToVector(sz.Length(), angle);
		}

		internal static float AngleSnap(float angle, float stepAngle = -1)
		{
			if (stepAngle <= 0)
				stepAngle = AngleStep();
			return (float)Math.Round(angle / stepAngle) * stepAngle;
		}

		/// <summary>as AngleSnap, but doesn't make angle a multiple of 15;  instead difference between current vector and vector ptOrigin->ptRelative
			/// must be a multiple of 15</summary>
		internal static PointF AngleSnapPointRelative(PointF pt, PointF origin, PointF relative)
		{
			Debug.Assert(!origin.IsEmpty); // usually means dont want snap
			if (pt.ApproxEqual(origin))
				return origin; // usually happens as shape is being drawn - floated point starts on top of previous
			float angle = VectorAngle(origin, pt);
			float relativeAngle = VectorAngle(origin, relative);
			angle = (float)(Math.Round((angle - relativeAngle) / AngleStep()) * AngleStep() + relativeAngle);
			return origin + ScalarToVector(DistanceBetween(pt, origin), angle);
		}

		/// <summary>like AngleSnap, but can use two origins, trying to find an intersection 15 degrees from each
			/// first origin has priority.  If bolSecondOnlyIfClose then second only used if quite close to snapping angle - ignored if angle needs a lot of adjusting</summary>
		internal static PointF AngleSnapFromTwoPoints(PointF pt, PointF origin, PointF secondOrigin, bool secondOnlyIfClose)
		{
			Debug.Assert(!origin.IsEmpty); // usually means dont want snap
			if (pt.ApproxEqual(origin))
				return origin; // usually happens as shape is being drawn - floated point starts on top of previous
			float angle = VectorAngle(origin, pt);
			angle = (float)(Math.Round(angle / AngleStep()) * AngleStep());
			float angle2 = VectorAngle(secondOrigin, pt);
			float angle2Rounded = (float)(Math.Round(angle2 / AngleStep()) * AngleStep());
			// check if we need to ignore second - happens if both angles are the same (because we are v distant from 2 origins near each other
			// in which case there is no target 15deg from both), or if bolSecondOnlyIfClose
			if (Math.Abs(angle - angle2Rounded) < 0.05 || secondOnlyIfClose && Math.Abs(angle2 - angle2Rounded) > AngleStep() / 6)
			{
				// just return snapping to first origin
				return origin + ScalarToVector(DistanceBetween(pt, origin), angle);
			}
			// try to solve for both - generate two lines of arbitrary length in right directions and let intersection logic return the intersection
			PointF intersection = Intersection.Line_LineIntersection(origin, origin + ScalarToVector(100, angle), false, secondOrigin, secondOrigin + ScalarToVector(100, angle2Rounded), false, true, true, out _, out _);
			// can return Empty if failed.  Ignore last Byref - only used if parallel which these aren't
			if (!intersection.IsEmpty)
				return intersection;
			Debug.WriteLine("AngleSnapFromTwoPoint2 failed to resolve both angles");
			// just use first origin
			return origin + ScalarToVector(DistanceBetween(pt, origin), angle);
		}

		#endregion

		#region Approximate comparisons
		// these are used in case of rounding errors, which make directly comparing floatingpoint values dangerous if they are the result of several steps of calculationsa
		internal const float NEGLIGIBLE = 0.05f;
		internal const float NEGLIGIBLESMALL = 0.005f; // alternate used where we don't want to reject too much
		internal const float NEGLIGIBLEANGLE = 0.01f; // can't be too small as is used for protractor direction reset
		internal const float NEGLIGIBLEANGLESMALL = 0.0005f; // for angles where we want derived points to be approx equal

		/// <summary>this is not a particularly accurate check (treats X and Y separately)- this is used to check that the points are not effectively the same one
			/// especially when creating a shape.  It is dangerous to just check equality, because they could be fractionally different due to rounding errors</summary>
		internal static bool ApproxEqual(this PointF pt1, PointF pt2)
		{
			return Math.Abs(pt1.X - pt2.X) < NEGLIGIBLE && Math.Abs(pt1.Y - pt2.Y) < NEGLIGIBLE;
			// this assumes that we are using data coordinates
		}

		/// <summary>this assumes the line is infinite </summary>
		internal static bool PointApproxOnLine(PointF lineA, PointF lineB, PointF test)
		{
			return DistancePointToLine(lineA, lineB, test) < NEGLIGIBLE;
		}

		internal static bool LineApproxPerpendicular(PointF A, PointF B)
		{
			SizeF szVector = A.VectorTo(B);
			return Math.Abs(szVector.Width) < NEGLIGIBLE || Math.Abs(szVector.Height) < NEGLIGIBLE;
		}

		internal static bool ApproxEqual(this RectangleF rctA, RectangleF rctB)
		{
			if (Math.Abs(rctA.X - rctB.X) > NEGLIGIBLE || Math.Abs(rctA.Y - rctB.Y) > NEGLIGIBLE)
				return false;
			if (Math.Abs(rctA.Width - rctB.Width) > NEGLIGIBLE || Math.Abs(rctA.Height - rctB.Height) > NEGLIGIBLE)
				return false;
			return true;
		}

		#endregion

		#region Normalised lines
		public class NormalisedLine
		{
			// represents line expressed as Ax+By + C = 0, where (A^2 + B^2) = 1
			// Distance from line of a point (X,Y) is always then AX+BY+C; this returns a signed value with opposite signs for opposite sides of the line
			public float A;
			public float B;
			public float C;

			public NormalisedLine(PointF pt1, PointF pt2)
			{
				// initially calculate A, assuming B = 1 (not normalised yet so that (A^2 + B^2) = 1)
				if (pt2.X != pt1.X)
				{
					A = (pt1.Y - pt2.Y) / (pt2.X - pt1.X);
					// Temporarily set B equal to root magnitude of A,B
					B = (float)Math.Sqrt(A * A + 1);
					A /= B;
					B = 1 / B; // now (A^2 + B^2) = 1
					C = -(A * pt1.X + B * pt1.Y);
				}
				else
				{
					// but the above will fail due to division by zero if X1 = X2
					B = 0;
					A = 1;
					C = -pt1.X;
				}
			}

			public float DistanceTo(PointF test)
			{
				return A * test.X + B * test.Y + C;
			}

			public float Y(float X)
			{
				return -(C + A * X) / B;
			}

		}

		/// <summary>represents a line, and a record of a distance away from the line in each direction</summary>
		public class FatLine : NormalisedLine
		{
			public float Dmax = 0;
			public float Dmin = 0;

			public FatLine(PointF pt1, PointF pt2) : base(pt1, pt2)
			{}

			/// <summary>creates a FatLine through the points in the first two parameters, with the distance limits set so that the other two points are included
				/// (this version is used in Bezier intersection analysis)</summary>
			public FatLine(PointF start, PointF end, PointF include1, PointF include2) : base(start, end)
			{
				float D1 = DistanceTo(include1);
				float D2 = DistanceTo(include2);
				Dmax = Math.Max(0, Math.Max(D1, D2));
				Dmin = Math.Min(0, Math.Min(D1, D2));
			}

			public float Ymax(float X)
			{
				return (Dmax - C - A * X) / B;
			}
			public float Ymin(float X)
			{
				return (Dmin - C - A * X) / B;
			}

		}
		#endregion

		#region Miscellaneous extension methods
		public static PointF BottomRight(this RectangleF rct)
		{
			return new PointF(rct.Right, rct.Bottom);
		}

		public static PointF TopRight(this RectangleF rct)
		{
			return new PointF(rct.Right, rct.Y);
		}

		public static PointF BottomLeft(this RectangleF rct)
		{
			return new PointF(rct.X, rct.Bottom);
		}
		// no TopLeft since it is just Location

		internal static Point Centre(this Rectangle rct)
		{
			return new Point(rct.X + rct.Width / 2, rct.Y + rct.Height / 2);
		}

		internal static PointF Centre(this RectangleF rct)
		{
			return new PointF(rct.X + rct.Width / 2, rct.Y + rct.Height / 2);
		}

		internal static int BiggestDimension(this Rectangle rct)
		{
			return Math.Max(rct.Width, rct.Height);
		}

		internal static Rectangle ToRectangle(this RectangleF rct)
		{
			return new Rectangle((int)rct.X, (int)rct.Y, (int)rct.Width, (int)rct.Height);
		}

		internal static Point ToPoint(this PointF pt)
		{
			return new Point((int)pt.X, (int)pt.Y);
		}

		/// <summary>assumes no rotation or shearing; this is mainly used with our graphics which can be scaled and offset</summary>
		public static RectangleF TransformRectangle(this Matrix transform, RectangleF rct)
		{
			Debug.Assert(transform.Elements[1] == 0 && transform.Elements[2] == 0); // These are the 2 elements which link the X and Y and cause shearing or rotation
			PointF[] pt = { rct.Location, rct.BottomRight() };
			transform.TransformPoints(pt);
			return new RectangleF(Math.Min(pt[0].X, pt[1].X), Math.Min(pt[0].Y, pt[1].Y), Math.Abs(pt[0].X - pt[1].X), Math.Abs(pt[0].Y - pt[1].Y));
		}

		/// <summary>More general version of TransformRectangle - returns the rectangle CONTAINING the transformed rectangle (which may no longer be orthogonal)</summary>
		public static RectangleF TransformRectangleBounds(this Matrix transform, RectangleF rct)
		{
			PointF[] points = { rct.Location, rct.BottomRight(), rct.TopRight(), rct.BottomLeft() };
			transform.TransformPoints(points);
			return new RectangleF(Math.Min(points[0].X, points[1].X), Math.Min(points[0].Y, points[1].Y), Math.Abs(points[0].X - points[1].X), Math.Abs(points[0].Y - points[1].Y));
		}

		/// <summary>Extracts 4 points representing a Bezier segment from a longer array of points (eg for an entire path)</summary>
		public static PointF[] ExtractBezierSegment(this PointF[] points, int initialIndex)
		{
			PointF[] segment = new PointF[4];
			for (int index = 0; index <= 3; index++)
				segment[index] = points[initialIndex + index];
			return segment;
		}

		/// <summary>Extracts 4 points representing a Bezier segment from a longer array of points (eg for an entire path)</summary>
		public static PointF[] ExtractBezierSegment(this List<PointF> points, int initialIndex)
		{
			return points.GetRange(initialIndex, 4).ToArray();
		}

		public static void Translate(this Matrix matrix, SizeF vector)
		{
			matrix.Translate(vector.Width, vector.Height);
		}

		/// <summary>Returns a more useful string representation of a matrix for diagnostics.  ToString isn't very helpful</summary>
		public static string ToStringUseful(this Matrix matrix, bool hiPrecision = false)
		{
			var output = new System.Text.StringBuilder();
			var elements = matrix.Elements;
			output.Append('{');
			for (int i = 0; i < 6; i++)
			{
				if (i > 0)
					output.Append(", ");
				output.Append(elements[i].ToString(hiPrecision ? "0.##########" : "0.##"));
			}
			output.Append('}');
			return output.ToString();
		}

		#endregion

	}
}
