using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;


namespace SAW
{
	public class Connector : Lined
	{

		// this class always has exactly 2 Links which gives the locations where the ends are linked to
		// if the shape in the link is nothing, then that end is not attached to any shape and is free-floating
		// the Vertices array changes in length.  The first entry is always the start point, and the last at the end point,
		// but there can be a variable number of intermediate entries in order to connect up the line

		#region Information
		public override Shapes ShapeCode => Shapes.Connector;
		public override SnapModes SnapNext(SnapModes requested) => SnapModes.Socket;
		public override AllowedActions Allows => AllowedActions.Arrowheads ;
		protected internal override bool Closed() => false;
		public override GeneralFlags Flags => base.Flags | GeneralFlags.NumberLinksFixed;

		private bool EndsEqual
		{
			get
			{
				// returns true if both ends points to an actual shape, and it is the same socket on the same shape
				// or one or both are the indefinite socket on the shape
				Link A = m_Links[0];
				Link B = m_Links[1];
				if (A.Shape == null || B.Shape == null)
					return false;
				if (A.Shape != B.Shape)
					return false;
				return A.Index == B.Index || A.Index < 0 || B.Index < 0;
			}
		}

		// The start and end points  give priority to the connection to the other shape, if the end is connected.  If not it just reads from Vertices
		// these versions always resolve the indefinite socket
		private PointF StartPoint
		{
			get
			{
				if (m_Links[0].Shape != null)
					return m_Links[0].Shape.SocketPosition(ResolveIndefiniteSocket(0));
				return Vertices[0];
			}
		}

		private PointF FinishPoint
		{
			get
			{
				if (m_Links[1].Shape != null)
					return m_Links[1].Shape.SocketPosition(ResolveIndefiniteSocket(1));
				return Vertices[Vertices.Count - 1];
			}
		}

		private PointF EndPoint(int index, bool resolveIndefinite = true) // returns either StartPoint or FinishPoint
		{
			Debug.Assert(index == 0 || index == 1);
			if (m_Links[index].Shape != null)
			{
				int socketIndex = m_Links[index].Index;
				if (socketIndex < 0 && resolveIndefinite)
					socketIndex = ResolveIndefiniteSocket(socketIndex);
				// otherwise does ask for position -1, which will return the centre of the shape
				return m_Links[index].Shape.SocketPosition(socketIndex);
			}
			return index == 0 ? Vertices[0] : Vertices[Vertices.Count - 1];
		}


		internal override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			return new List<Prompt>
			{
				new Prompt(ShapeVerbs.Complete, "Connector_Finish", "Connector_Finish"),
				new Prompt(ShapeVerbs.Info, "Prompts_Connector_Hover", "Prompts_Connector_Hover"),
				new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll")
			};
		}

		#endregion

		#region Verbs
		private bool m_Acceptable = false;
		public override VerbResult Start(EditableView.ClickPosition position)
		{
			base.AddLink(null, 0);
			base.AddLink(null, 0);
			VerbResult result = base.Start(position);
			UpdateLink(0, position); // but we deliberately leave the other one floating; otherwise it would just point to the same socket
			m_Acceptable = false;
			m_DefinedVertices = 2; // not used by this class and confuses base class
			return result;
		}

		private void UpdateLink(int index, EditableView.ClickPosition position)
		{
			Socket socket = position.Page.NearestSocket(position.Exact);
			m_Links[index] = new Link(socket); // works even if  objSocket.IsEmpty
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			return VerbResult.Destroyed;
		}

		public override VerbResult Float(EditableView.ClickPosition pt)
		{
			Vertices[Vertices.Count - 1] = pt.Snapped;
			UpdateLink(1, pt);
			PositionLine();
			m_Acceptable = !EndsEqual && !Vertices[0].ApproxEqual(Vertices[Vertices.Count - 1]);
			m_Bounds = RectangleF.Empty;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			Float(position);
			return CompleteRetrospective();
		}

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			Float(position);
			return CompleteRetrospective();
		}

		public override VerbResult CompleteRetrospective()
		{
			if (!m_Acceptable)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		protected override void OnLinkedChanged()
		{
			if (this.Status == StatusValues.Moved)
				return; // avoids recalculating twice if both shapes have changed
			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded | ChangeAffects.Bounds);
			PositionLine();
			m_Bounds = CalculateBounds();
			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
		}
		#endregion

		#region Placing the connection line
		private const float EXITANGLETOLERANCE = Geometry.PI / 3; // 60°
		private const float EXITSTAGELENGTH = 5;
		private const float TRANSVERSECLEARANCE = 5; // clearance to leave around shape when stepping around it
		private void PositionLine()
		{
			// This builds up a list of points required at each of the beginning and end
			List<PointF> start = new List<PointF> { StartPoint };
			List<PointF> end = new List<PointF> { FinishPoint };
			SizeF vector = StartPoint.VectorTo(FinishPoint);
			if (vector.Length() < 1)
				return;
			float primary = PrimaryVector(vector).VectorAngle();
			ProcessExitVector(start, 0, primary);
			ProcessExitVector(end, 1, primary + Geometry.ANGLE180);

			// The remaining required vector after adding on the exit vectors...
			bool[] conflict = new bool[2];
			int index = 0;
			int step = 0;
			bool done = false;
			do
			{
				if (index == 0)
					vector = start[start.Count - 1].VectorTo(end[end.Count - 1]);
				else // opposite direction reverses the vector...
					vector = end[end.Count - 1].VectorTo(start[start.Count - 1]);
				SizeF primaryVector = PrimaryVector(vector);
				SizeF transverseGuaranteed = TransverseVector(vector, true);
				SizeF transverseNull = TransverseVector(vector, false);

				List<PointF> points = index == 0 ? start : end;
				List<PointF> other = index == 1 ? start : end;
				conflict[index] = PrimaryVectorConflicts(points, primaryVector, m_Links[index].Shape);
				conflict[1 - index] = PrimaryVectorConflicts(other, new SizeF(0, 0) - primaryVector, m_Links[1 - index].Shape);
				if (conflict[index])
				{
					// If the other shape is not conflicted, the best strategy can be to move the entire transverse distance now (assuming that is far enough to clear this shape)
					PointF pt = points[points.Count - 1] + transverseGuaranteed;
					if (!conflict[1 - index] && !PrimaryVectorConflicts(pt, primaryVector, m_Links[index].Shape))
					{
						// prime vector won't conflict once entire distance away
						points.Add(pt);
						done = true;
					}
					else
					{
						// can't go correct distance away - probably not far enough to clear this shape
						// therefore go far enough to clear this one
						pt = points[points.Count - 1] + GetRequiredTransverseToClearShape(points[points.Count - 1], m_Links[index].Shape, primaryVector, transverseGuaranteed);
						points.Add(pt);
					}
				}
				else if (!conflict[1 - index])
				{
					// neither now conflict - place the line as necessary
					// first we check if extending one of the previous vectors, which is in the transverse direction
					// will work.  This saves an extra wiggle (which does look quite silly) when the exits were, say, vertical and the current primary
					// is horizontal, but there is nothing constraining us from just extending the exit vectors with a single horizontal line
					SizeF last = LastVector(points);
					SizeF otherLast = LastVector(other);
					if (VectorsMatch(last, transverseGuaranteed))
					{
						if (VectorsMatch(new SizeF(0, 0) - otherLast, transverseGuaranteed))
						{
							// both match, ideal option is to move half long each
							points.Add(points[points.Count - 1] + transverseGuaranteed.MultiplyBy(0.5f));
							other.Add(other[other.Count - 1] + transverseGuaranteed.MultiplyBy(-0.5f));
						}
						else
							points.Add(points[points.Count - 1] + transverseGuaranteed);
						done = true;
					}
					else if (VectorsMatch(new SizeF(0, 0) - otherLast, transverseGuaranteed))
					{
						other.Add(other[other.Count - 1] + transverseGuaranteed.MultiplyBy(-1));
						done = true;
					}
					else
					{
						SizeF first = ChooseCrossStepPosition(index, points, other, primaryVector);
						if (!first.IsEmpty)
						{
							PointF pt = points[points.Count - 1] + first;
							points.Add(pt);
							if (!transverseNull.IsEmpty)
							{
								pt = pt + transverseNull;
								points.Add(pt);
							}
						}
						done = true;
					}
				}
				index = (index + 1) % 2;
				step += 1;
			} while (!(step >= 8 || done));

			// And now merge these lists to create the new list of vertices
			Vertices.Clear();
			Vertices.AddRange(start);
			end.Reverse();
			Vertices.AddRange(end);
			DecacheArrowheads();
			DiscardPath();
		}

		private void ProcessExitVector(List<PointF> points, int index, float primaryAngle)
		{
			// index is 0 or 1; specifying which end we are looking at
			Shape shape = m_Links[index].Shape; // the shape that this end is connected to
			if (shape == null)
				return; // just stops at this point, no processing needed if not attached to a shape
			SizeF exitVector = shape.SocketExitVector(ResolveIndefiniteSocket(index));
			float exitAngle = Geometry.NormaliseAngle(exitVector.VectorAngle());
			float difference = Math.Abs(Geometry.AngleBetween(exitAngle, Geometry.NormaliseAngle(primaryAngle)));
			if (difference < EXITANGLETOLERANCE || difference > Geometry.ANGLE360 - EXITANGLETOLERANCE)
			{
				// the line exits in roughly the right direction, we don't need to add an exit stage
			}
			else
			{
				exitVector = exitVector.ChangeLength(EXITSTAGELENGTH);
				PointF exitPoint = points[0] + exitVector;
				// need to see if this has cleared the shape boundary
				RectangleF bounds = shape.Bounds;
				if (bounds.Contains(exitPoint))
				{
					// find where the exit vector crosses the boundary
					//initially set the exit point a long distance away to test for the intersections.  I think we need to test each of the four edges of the bounding rectangle separately
					exitPoint = points[0] + exitVector.ChangeLength(1000);
					// top:
					PointF intersection = Intersection.Line_LineIntersection(points[0], exitPoint, new PointF(bounds.Left, bounds.Top), new PointF(bounds.Right, bounds.Top));
					// right:
					if (intersection.IsEmpty)
						intersection = Intersection.Line_LineIntersection(points[0], exitPoint, new PointF(bounds.Right, bounds.Top), new PointF(bounds.Right, bounds.Bottom));
					// bottom:
					if (intersection.IsEmpty)
						intersection = Intersection.Line_LineIntersection(points[0], exitPoint, new PointF(bounds.Left, bounds.Bottom), new PointF(bounds.Right, bounds.Bottom));
					// left:
					if (intersection.IsEmpty)
						intersection = Intersection.Line_LineIntersection(points[0], exitPoint, new PointF(bounds.Left, bounds.Top), new PointF(bounds.Left, bounds.Bottom));
					if (intersection.IsEmpty)
					{
						// huh
						Debug.Fail("Failed to find where exit vector leaves the bounding rectangle");
						// leave szExit with the default length
					}
					else
					{
						float length = Geometry.DistanceBetween(intersection, points[0]);
						exitVector = exitVector.ChangeLength(length + EXITSTAGELENGTH / 2); // second term gets us a bit outside the rectangle
					}
					exitPoint = points[0] + exitVector;
				}
				points.Add(exitPoint);
			}
		}

		/// <summary>returns a vector in just the X or Y directions, whichever is larger in the parameter</summary>
		private static SizeF PrimaryVector(SizeF vector)
		{
			if (Math.Abs(vector.Width) > Math.Abs(vector.Height))
				return new SizeF(vector.Width, 0);
			else
				return new SizeF(0, vector.Height);
		}

		private SizeF TransverseVector(SizeF vector, bool disallowNull)
		{
			// opposite of the above function, it returns the other part of the vector.
			// If the other part is 0 and disallowNull it will return a random component in the transverse direction
			// this is because we want to use this function to give a direction to head in if the forward direction is blocked.  So we need to return
			// a nonzero vector.  If the forwards direction directly towards the target has no transverse component then it doesn't matter which way we travel
			if (Math.Abs(vector.Width) > Math.Abs(vector.Height))
			{
				if (Math.Abs(vector.Height) > 0.01)
					return new SizeF(0, vector.Height);
				if (disallowNull)
					return new SizeF(0, 1);
				return new SizeF(0, 0);
			}
			else
			{
				if (Math.Abs(vector.Width) > 0.01)
					return new SizeF(vector.Width, 0);
				if (disallowNull)
					return new SizeF(1, 0);
				return new SizeF(0, 0);
			}
		}

		/// <summary>returns the last vector in the existing list of points. returns Empty if the list is a single point (i.e. no exit vector was required) </summary>
		private SizeF LastVector(List<PointF> points)
		{
			if (points.Count < 2)
				return SizeF.Empty;
			return points[points.Count - 2].VectorTo(points[points.Count - 1]);
		}

		private bool VectorsMatch(SizeF A, SizeF B)
		{
			// returns true if the two vectors are in the same direction; they must be horizontal or vertical
			// diagonal vectors will always return false
			if (A.Width != 0 && A.Height != 0)
				return false;
			return Math.Sign(A.Width) == Math.Sign(B.Width) && Math.Sign(A.Height) == Math.Sign(B.Height);
		}

		private bool PrimaryVectorConflicts(List<PointF> points, SizeF primary, Shape shape)
		{
			// returns true if moving along this primary vector from the end of the current chain of points will hit the bounding box for the shape
			if (points.Count == 1)
				return false;
			// if it contains only the start point, then that implies we have not used an exit vector
			// and so we think the line is heading in the right direction anyway.  However without the exit vector the main vector will always touch the bounding box
			// so the test below is completely misleading
			return PrimaryVectorConflicts(points[points.Count - 1], primary, shape);
		}

		private bool PrimaryVectorConflicts(PointF start, SizeF primary, Shape shape)
		{
			// as above but given a point to start from - can assume that any necessary exit vector has been applied
			if (shape == null)
				return false;
			Debug.Assert(primary.Height == 0 || primary.Width == 0); // and the code below relies on this fact to simplify the test
			RectangleF rctVector = new RectangleF(start, new SizeF(0, 0)); // primary) ' not sure if this is allowed
			Geometry.Extend(ref rctVector, start + primary);
			return rctVector.Intersects(shape.Bounds);
		}

		private SizeF GetRequiredTransverseToClearShape(PointF start, Shape shape, SizeF primary, SizeF transverse)
		{
			// gets the length of transverse needed to clear the bounds
			if (shape == null)
				return transverse;
			// above: should never have got here anyway as can't have conflicted in the first case
			// this fn actually rather boring - I think we need to write out each case...
			RectangleF shapeBounds = shape.Bounds;
			if (primary.Width != 0)
			{
				// going left or right
				if (transverse.Height > 0) // by going up
										   // nb bottom and top swapped because we're using upwards Y coords
					return new SizeF(0, shapeBounds.Bottom - start.Y + TRANSVERSECLEARANCE);
				else
					return new SizeF(0, shapeBounds.Top - start.Y - TRANSVERSECLEARANCE);
			}
			else
			{
				// going up or down
				if (transverse.Width < 0) // by going left
					return new SizeF(shapeBounds.Left - start.X - TRANSVERSECLEARANCE, 0);
				else
					return new SizeF(shapeBounds.Right - start.X + TRANSVERSECLEARANCE, 0);
			}
		}

		private const float EXTRASWEEP = 10;
		private SizeF ChooseCrossStepPosition(int index, List<PointF> points, List<PointF> other, SizeF primary)
		{
			// usually we do part of the primary, then the transverse, then the rest of the primary.  The decision is how far to go in
			// the primary direction before crossing.  Can determine by making a rectangle covering the possible swept area and seeing where it intersects the possible area
			RectangleF swept = new RectangleF(points[points.Count - 1], new SizeF(0, 0));
			Geometry.Extend(ref swept, other[other.Count - 1]);
			// also works best if we try a bit further - ie actually going backwards slightly at one primary end
			if (primary.Width != 0)
				swept.Inflate(EXTRASWEEP, 0);
			else
				swept.Inflate(0, EXTRASWEEP);
			RectangleF intersect = RectangleF.Empty;
			if (m_Links[index].Shape != null)
			{
				RectangleF exclude = m_Links[index].Shape.Bounds;
				exclude.Inflate(0.1F, 0.1F); // without this, especially when using a grid, we will sometimes leave a line lying along the edge of the shape
				intersect = RectangleF.Intersect(swept, exclude);
			}
			RectangleF intersectOther = RectangleF.Empty;
			if (m_Links[1 - index].Shape != null)
			{
				RectangleF exclude = m_Links[1 - index].Shape.Bounds;
				exclude.Inflate(0.1F, 0.1F);
				intersectOther = RectangleF.Intersect(swept, exclude);
			}
			SizeF first; // distance to travel along primary direction before doing transverse
			if (primary.Width != 0)
			{
				float minimum = swept.Left;
				float maximum = swept.Right;
				if (primary.Width > 0)
				{
					if (!intersect.IsEmpty)
						minimum = intersect.Right;
					if (!intersectOther.IsEmpty)
						maximum = intersectOther.Left;
				}
				else
				{
					if (!intersect.IsEmpty)
						maximum = intersect.Left;
					if (!intersectOther.IsEmpty)
						minimum = intersectOther.Right;
				}
				float position;
				if (maximum < minimum)
					// no solution, just go halfway
					position = swept.Left + swept.Width / 2;
				else
					position = (maximum + minimum) / 2;
				// sngDistance is now the X coordinate at which to go transverse
				if (primary.Width > 0)
					first = new SizeF(position - swept.Left - EXTRASWEEP, 0);
				else
					first = new SizeF(position - swept.Right + EXTRASWEEP, 0); // will give a negative number
			}
			else
			{// primary.Height <> 0
				float minimum = swept.Y;
				float maximum = swept.Bottom; // Actually top
				if (primary.Height > 0)
				{
					if (!intersect.IsEmpty)
						minimum = intersect.Bottom; // actually top
					if (!intersectOther.IsEmpty)
						maximum = intersectOther.Y;
				}
				else
				{
					if (!intersect.IsEmpty)
						maximum = intersect.Y;
					if (!intersectOther.IsEmpty)
						minimum = intersectOther.Bottom;
				}
				float position;
				if (maximum < minimum)
					//    ' no solution, just go halfway
					position = swept.Y + swept.Height / 2;
				else
					position = (maximum + minimum) / 2;
				// sngDistance is now the X coordinate at which to go transverse
				if (primary.Height > 0)
					first = new SizeF(0, position - swept.Y - EXTRASWEEP);
				else
					first = new SizeF(0, position - swept.Bottom + EXTRASWEEP); // will give a negative number
			}
			return first;
		}

		private int ResolveIndefiniteSocket(int end)
		{
			// returns the socket index at this end, but changing -1 ("don't care") to a specific socket
			int index = m_Links[end].Index;
			Shape shape = m_Links[end].Shape;
			if (index != -1 || shape == null)
				return index;
			PointF other = EndPoint(1 - end, false);
			// Need to get all the sockets in the shape at this end and see which is closest to the other End
			Socket closestSocket = Socket.Empty;
			float closest = float.MaxValue;
			foreach (Socket socket in shape.GetSockets())
			{
				if (socket.Index >= 0)
				{
					float distance = Geometry.DistanceBetween(socket.Position, other);
					if (distance < closest)
					{
						closest = distance;
						closestSocket = socket;
					}
				}
			}
			if (closestSocket.IsEmpty)
			{
				Debug.Fail("Failed to resolve indefinite socket");
				return 0; // just return some arbitrary number to try and continue
			}
			return closestSocket.Index;
		}
		#endregion

		#region Shape coordinate functions, GrabSpots
		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();
		public override RectangleF RefreshBounds(bool withShadow = false) => new Rectangle(0, -1000, 1000, 1000);

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			return new List<GrabSpot> {
				new GrabSpot(this, GrabTypes.SingleVertex, Vertices[0], 0),
				new GrabSpot(this, GrabTypes.SingleVertex, Vertices[Vertices.Count - 1], 1)
			};
		}

		protected  internal override void DoGrabMove(GrabMovement move)
		{
			// Cannot use the base version, because it will try and move vertices (1) rather than the last vertex
			if (move.GrabType != GrabTypes.SingleVertex)
			{
				Utilities.LogSubError("Connector: unexpected grab move type");
				return;
			}
			int index; // the index of the vertex; objMove.Index is the end index (0|1)
			switch (move.ShapeIndex)
			{
				case 0:
					index = 0;
					break;
				case 1:
					index = Vertices.Count - 1;
					break;
				default:
					Utilities.LogSubError("Connector: unexpected grab move index");
					return;
			}
			PointF newPoint = Vertices[index];
			move.Transform.TransformPoint(ref newPoint);
			Vertices[index] = newPoint;
			UpdateLink(move.ShapeIndex, move.Current);
			PositionLine();
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			// can be ignored
		}

		internal override bool StartGrabMove(GrabMovement grab)
		{
			base.StartGrabMove(grab);
			grab.SnapMode = SnapModes.Socket;
			return true;
		}

		protected override void CreatePath()
		{
			m_Path = GetLinearPath(Vertices, false);
		}

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			return base.GenerateTargetsDefault(floating, false);
		}

		#endregion

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			EnsurePath(false);
			base.InternalDrawFromPath(gr, resources);
			base.DrawArrowheads(resources);
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			// before 2.1.9 entries in m_Links could be deleted if they referenced deleted objects, which broke this - code assumes 2 entries
			while (m_Links.Count < 2)
			{
				base.AddLink(null, 0);
			}
		}
	}

}
