using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace SAW
{
		/// <summary>Base class for any type where the bounding rectangle defines the coordinates</summary>
	public abstract class BoundsDefines : Shape
	{

		public override void ApplyTransformation(Transformation transformation)
		{
			// the base class just clears the bounding rectangle - we do not want this to happen!
			transformation.TransformRectangle(ref m_Bounds);
			ClearTextCache(); // base class does a slightly more efficient version, but in practice this is probably only used for buttons which have quite simple text
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			return true; // clicking anywhere within the bounds counts as a hit
		}

		protected override RectangleF CalculateBounds()
		{
			Debug.Fail("Any BoundsDefines shape should not be calling CalculateBounds - m_Bounds is the formal definition of the position");
			return m_Bounds;
		}

		// override to true to get this to do targets for bounds
		protected virtual bool AutoTargets => false;

		// base does Centre and Middle

		public override List<Target> GenerateTargets(UserSocket floating)
		{
			if (!AutoTargets)
				return null;
			List<Target> targets = new List<Target>();
			Lined.AddLineTargets(this, targets, floating, m_Bounds.GetPoints(), true);
			targets.Add(new Target(this, Centre, Target.Types.Centre, floating));
			return targets;
		}

		public override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			if (!AutoTargets)
				return null;
			List<UserSocket> list = new List<UserSocket>();
			foreach (PointF pt in m_Bounds.GetPoints())
			{
				list.Add(UserSocket.CreateForPoint(pt));
			}
			list.Add(UserSocket.CreateForPoint(Middle()));
			return list;
		}

		public override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			if (!AutoTargets)
				return;
			PointF[] points = m_Bounds.GetPoints();
			PointF start = points[target.ShapeIndex % points.Length];
			PointF end = points[(target.ShapeIndex + 1) % points.Length]; // mod makes sure last line loops back to (0), rather than trying to access (4)
			Lined.DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, start, end);
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Bounds = reader.ReadRectangleF();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Bounds);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			// By default this isn't enabled - is only usable if the overriding class adds Tidy to the Allows return value
			switch (mode)
			{
				case SnapModes.Grid:
					switch (page.Paper.PaperType)
					{
						case Paper.Papers.Plain:
							return false; // cannot tidy up to an isometric grid
					}
					break;
				case SnapModes.Shape: // will be attempted below
					break;
				default:
					return false; // mainly intended for angle; where the shape is by definition aligned on the grid
			}
			List<PointF> points = new List<PointF>();
			points.AddRange(m_Bounds.GetPoints());
			Lined.TidyVertices(points, this, mode, page, 3);
			// but that might not have created an orthogonal rectangle.  So calculate the complete rectangle bounding these.  For tidying to grid
			// that should just return the same points again
			m_Bounds = RectangleF.Empty;
			Geometry.Extend(ref m_Bounds, points);
			return true;
		}

		public override GeneralFlags Flags => base.Flags | GeneralFlags.ProtectBounds;
	}


	/// <summary>Implements positioning for a shape which is basically rectangular but can be rotated.  (Derived shapes used BoundsDefines previously)</summary>
	public abstract class AbstractRectangle : Shape
	{

		/// <summary>defined around rectangle from top left: TL, TR, BR, BL</summary>
		protected PointF[] Vertices = new PointF[4];

		public override AllowedActions Allows
		{ get { return base.Allows | AllowedActions.Tidy; } }

		#region Data
		public override void Load(DataReader reader)
		{
			base.Load(reader);
			if (reader.Version < 90)
			{
				var rct = reader.ReadRectangleF();
				Vertices[0] = rct.Location;
				Vertices[1] = rct.TopRight();
				Vertices[2] = rct.BottomRight();
				Vertices[3] = rct.BottomLeft();
			}
			else
				Vertices = reader.ReadListPoints().ToArray();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			if (writer.Version < 90)
				writer.Write(m_Bounds); // this isn't strictly correct as the bounds may be larger than the image; but the old file has no way of representing a rotated image so this is probably the closest possible
			else
				writer.Write(Vertices);
		}


		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			AbstractRectangle rectangle = (AbstractRectangle)other;
			Vertices = (PointF[])rectangle.Vertices.Clone();
		}

		#endregion

		#region Coordinates

		protected override RectangleF CalculateBounds()
		{
			RectangleF bounds = new RectangleF();
			Geometry.Extend(ref bounds, Vertices);
			return bounds;
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformPoints(Vertices);
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			// The requirement is that the vector to the point must be to the right of the vector from any vertex to the next.  If all four satisfy this the point must be inside the box
			for (int index = 0; index <= 3; index++)
			{
				if (Vertices[index].VectorTo(Vertices[index % 4]).CrossProduct(Vertices[index].VectorTo(clickPoint)) < 0)
					return false;
			}
			return true;
		}

		/// <summary>Defines the vertices based on a standard rectangle</summary>
		protected void SetRectangle(RectangleF rct)
		{
			Vertices = rct.GetPoints();
			m_Bounds = RectangleF.Empty;
		}

		/// <summary>Returns true if the layout is a simple rectangle, i.e. not rotated</summary>
		protected bool IsOrthogonal()
		{
			return Vertices[0].ApproxEqual(Bounds.Location) && Vertices[1].ApproxEqual(Bounds.TopRight());
			// Not sure if it's actually necessary to check more than one point
		}

		#endregion

		public override bool Tidy(SnapModes mode, Page page)
		{
			List<PointF> col = new List<PointF>(Vertices);
			float transverse = Geometry.DistanceBetween(Vertices[1], Vertices[2]); // the transverse distance
			if (!Lined.TidyVertices(col, this, mode, page, 3))
				return false;
			// make sure it is rectangular again!...
			// the base class tidies the points independently and could end up with a parallelogram, this ensures that it remains a rectangle.  Copied from rectangle, but removing part of it which snapped the transverse size (not so applicable here we probably want to maintain the ratio for images?)
			Vertices = col.ToArray();
			int direction = Geometry.TurnDirection(Vertices[0], Vertices[1], Vertices[2]);
			Vertices[2] = Vertices[1] + Vertices[0].VectorTo(Vertices[1]).Perpendicular(direction).ChangeLength(transverse);
			Vertices[3] = Vertices[0] + Vertices[1].VectorTo(Vertices[2]);
			m_Bounds = CalculateBounds();
			return true;
			// This doesn't perform very well when aligning with other shapes; the initial function will tidy the individual points, but usually one will become misaligned again
			// when correcting the rectangle.  It would be better to do a proper moving snap, but I'm not sure if it's really worth the effort
		}

		#region Targets and related
		/// <summary>Overrides to true to the targets for the bounding edges</summary>
		protected virtual bool AutoTargets => false;

		public override List<Target> GenerateTargets(UserSocket floating)
		{
			if (!AutoTargets)
				return null;
			List<Target> targets = new List<Target>();
			Lined.AddLineTargets(this, targets, floating, Vertices, true);
			targets.Add(new Target(this, Centre, Target.Types.Centre, floating));
			return targets;
		}

		public override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			if (!AutoTargets)
				return null;
			List<UserSocket> list = (from pt in Vertices select UserSocket.CreateForPoint(pt)).ToList();
			list.Add(UserSocket.CreateForPoint(Middle()));
			return list;
		}

		public override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			if (!AutoTargets)
				return;
			PointF start = Vertices[target.ShapeIndex % 4];
			PointF end = Vertices[(target.ShapeIndex + 1) % 4]; // mod makes sure last line loops back to (0), rather than trying to access (4)
			Lined.DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, start, end);
		}

		#endregion

	}
}