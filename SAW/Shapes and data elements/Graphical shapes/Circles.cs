using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;


namespace SAW.Shapes
{
	public abstract class Circular : Filled
	{

		public abstract float Radius { get; }

		public override AllowedActions Allows => (base.Allows & ~AllowedActions.TransformLinearStretch) | AllowedActions.Tidy;

		protected override LineLogics LineLogic => LineLogics.Custom;

		#region Path support
		// Few of the circular items use Path for drawing.  All will create paths however for cutting

		/// <summary>returns a series (0 or more) of joined bezier curves representing the arc part.  Will only return 0 Bezier if the if the arc is degenerate (both angles equal)</summary>
		protected abstract PointF[] GetPathForArc();

		protected PointF[] GetPathPointsForArc(float startAngle, float endAngle, int direction)
		{
			// return a series (1 or more) of joined bezier curves representing the given arc
			if (startAngle == endAngle)
				return new PointF[] { };
			using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
			{
				if (direction == 1)
				{
					path.AddArc(CircleRectangle(), Geometry.DotNetAngle(startAngle), Geometry.Degrees(Geometry.AngleBetween(startAngle, endAngle)));
					return path.PathPoints;
				}
				else
				{
					path.AddArc(CircleRectangle(), Geometry.DotNetAngle(endAngle), Geometry.Degrees(Geometry.AngleBetween(endAngle, startAngle)));
					PointF[] temp = path.PathPoints;
					Array.Reverse(temp);
					return temp;
				}
			}

		}

		#endregion

		internal override float[] GetRelevantAngles()
		{
			return null; // don't want any vertex-based information from the base class to be used
						 // although semicircle will override this again
		}

		protected RectangleF CircleRectangle()
		{
			Debug.Assert(ShapeCode != Shapes.Ellipse); // not valid for an ellipse
													   // the rectangle containing the entire circle for arcs
			PointF pt = Centre;
			float r = Radius;
			return new RectangleF(pt.X - r, pt.Y - r, r * 2, r * 2);
		}

		protected internal override string StatusInformation(bool ongoing) => Strings.Item("Info_Radius") + ": " + Measure.FormatLength(Radius);

		// doesn't check this is within the used arc - is just a point on the circumference
		protected PointF PointAtAngle(float angle) => Centre + Geometry.ScalarToVector(Radius, angle);

	}

	#region Circle
	public class Circle : Circular
	{
		// this inherits from Lined, so has an array of vertices in theory - we just use the first entry for the centre

		public Circle()
		{ }

		public Circle(PointF centre, float radius)
		{
			m_DefinedVertices = 2;
			m_Radius = radius;
			Vertices.Add(centre);
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
		}

		#region Information and data
		private float m_Radius;
		private const float DEFAULTRADIUS = 10;
		private const float MINIMUMRADIUS = 1;

		public override Shapes ShapeCode => Shapes.Circle;

		public override PointF Centre => Vertices[0];

		public override float Radius => m_Radius;

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt> { new Prompt(ShapeVerbs.Complete, "Circle_Finish", "Circle_Finish"), new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll") };
			return list;
		}

		#endregion

		#region Verbs
		//All verbs need to be overridden

		public override VerbResult Start(ClickPosition position)
		{
			// cannot use base class Start because that would create two points
			Debug.Assert(LineStyle != null && FillStyle != null);
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			Vertices.Add(position.Snapped);
			// if we are using a grid then set the radius to one grid step initially
			if (position.SnapMode == SnapModes.Grid)
			{
				float step = position.Page.Paper.ScalarSnapStep(0);
				m_Radius = step > Geometry.NEGLIGIBLE ? step : DEFAULTRADIUS;
			}
			else
				m_Radius = DEFAULTRADIUS;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			if (m_Radius < MINIMUMRADIUS)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult Complete(ClickPosition position) => Choose(position);

		public override VerbResult CompleteRetrospective()
		{
			// any choose should have completed it
			if (m_Radius < MINIMUMRADIUS)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult Float(ClickPosition position)
		{
			m_Radius = Geometry.DistanceBetween(Centre, position.Snapped);
			float step = position.ScalarSnapStep(0);
			if (step > 0)
			{
				// when shape snapping the point has already been snapped, but that is OK because ScalarSnapStep will return 0, so we will not attempt to snap again
				m_Radius = (float)(Math.Round(m_Radius / step) * step);
				if (m_Radius < step)
					m_Radius = step; // in practice if <, then it will be 0 - if using a grid there is no point allowing a 0 circle, which will only be rejected anyway
			}
			m_Bounds = RectangleF.Empty;
			DiscardPath();
			return VerbResult.Continuing;
		}

		protected internal override VerbResult OtherVerb(ClickPosition position, Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					int delta = code == Functions.Codes.Increment ? 1 : -1;
					if (m_Radius + delta < MINIMUMRADIUS)
						return VerbResult.Rejected;
					float step = position.ScalarSnapStep(0);
					if (step > 0)
						step = delta * step;
					else
						step = delta * Globals.Root.CurrentConfig.ReadSingle(Config.Radius_Step, 1);
					m_Radius += step;
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
				default:
					return VerbResult.Rejected;
			}
		}
		#endregion

		#region Coordinates

		protected override RectangleF CalculateBounds() => new RectangleF(Vertices[0].X - m_Radius, Vertices[0].Y - m_Radius, m_Radius * 2, m_Radius * 2);

		public override RectangleF RefreshBounds(bool withShadow = false) => base.RefreshBoundsFromBounds(true);

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (requested == SnapModes.Angle)
				return SnapModes.Off;
			return requested; // will use a scalar snap internally when working on a grid
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformScalar(ref m_Radius);
		}

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			float distance = Geometry.DistanceBetween(Centre, clickPoint);
			if (IsFilled || treatAsFilled)
				return distance <= m_Radius;
			else
				return Math.Abs(distance - m_Radius) < Line.LINEHITTOLERANCE;
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			bool changed = false;
			switch (mode)
			{
				case SnapModes.Grid:
					changed = base.TidyVertices(mode, page, 0); // will snap the centre point onto a shape if possible
					float radius = page.Paper.SnapScalar(m_Radius);
					if (radius != m_Radius)
						changed = true;
					m_Radius = radius;
					break;
				case SnapModes.Shape:
					changed = base.TidyVertices(mode, page, 0); // will snap the centre point onto a shape if possible
					List<Target> targets = page.GenerateIndependentTargets(UserSocket.CreateForPoint(Vertices[0]), 1);
					// unfortunately the first target(s) will be to close
					for (int index = 0; index <= targets.Count - 1; index++)
					{
						Target target = targets[index];
						if (target.Shape != this && target.ActualDistance >= m_Radius - Target.ActivationMaximumDistanceMoveShape)
						{
							if (target.ActualDistance > m_Radius + Target.ActivationMaximumDistanceMoveShape)
								break; // now too far away, ignore any further targets
							if (m_Radius != targets[index].ActualDistance)
								changed = true;
							m_Radius = targets[index].ActualDistance;
							break;
						}
					}
					break;
			}
			if (changed)
				m_Bounds = RectangleF.Empty;
			return changed;
		}

		protected override void CreatePath()
		{
			// However, this is supported in order to support cutting
			m_Path = new System.Drawing.Drawing2D.GraphicsPath();
			m_Path.AddEllipse(this.Bounds);
		}

		#endregion

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.MainBrush != null || resources.MainPen != null)
				gr.Ellipse(Bounds, resources.MainPen, resources.MainBrush);
		}

		#region BinaryData and Copy
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Radius = reader.ReadSingle();
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Radius);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Circle objCircle = (Circle)other;
			m_Radius = objCircle.m_Radius;
		}

		#endregion

		#region Targets, grab spots, sockets

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> targets = new List<Target>();
			targets.Add(new Target(this, Centre, Target.Types.Centre, floating));
			SizeF vector = Centre.VectorTo(floating.Centre);
			if (vector.Length() > m_Radius / 3) // don't try and add an edge target if we are very near the middle - this will look very strange/would cause problems if exactly over the centre
			{
				float angle = vector.VectorAngle();
				PointF pt = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, angle));
				// it will speed up the drawing if we remember the angle
				targets.Add(new Target(this, pt, Target.Types.Line, floating) { ShapeParameter = angle });
			}
			return targets;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// we want to calculate the angle to sweep the marker through to keep the length of the marker similar
			// L = desired length
			// circumference = 2PI*r
			// want this percentage of circumference: L / 2PI*r
			// 2PI radians per circle, so angle = L/r
			float sweep = Target.LINEDRAWLENGTH / m_Radius; // just use half the maximum as a convenient value (also keeps this well within the bounding box even allowing for pen width etc)
															// objTarget.ShapeParameter stored the angle of the target  (set during GenerateTargets)
			gr.DrawArc(pn, Bounds, Geometry.DotNetAngle(target.ShapeParameter + sweep / 2), -Geometry.Degrees(sweep));
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			EnsureBounds();
			List<GrabSpot> list = new List<GrabSpot>();
			list.Add(new GrabSpot(this, GrabTypes.Radius, new PointF(m_Bounds.Right, Centre.Y)) { Focus = Centre, Prompts = new[] { new Prompt(ShapeVerbs.Choose, "Circle_Resize") } });
			return list;
		}

		// sockets are numbered 0-3: top, right, bottom, left
		internal override List<Socket> GetSockets()
		{
			List<Socket> sockets = new List<Socket>
			{
				new Socket(this, Socket.AUTOMATIC, Middle()),
				new Socket(this, 0, new PointF(Centre.X, Centre.Y + m_Radius)),
				new Socket(this, 1, new PointF(Centre.X + m_Radius, Centre.Y)),
				new Socket(this, 2, new PointF(Centre.X, Centre.Y - m_Radius)),
				new Socket(this, 3, new PointF(Centre.X - m_Radius, Centre.Y))
			};
			return sockets;
		}

		internal override PointF SocketPosition(int index)
		{
			switch (index)
			{
				case -1: return Middle();
				case 0: return new PointF(Centre.X, Centre.Y + m_Radius);
				case 1: return new PointF(Centre.X + m_Radius, Centre.Y);
				case 2: return new PointF(Centre.X, Centre.Y - m_Radius);
				case 3: return new PointF(Centre.X - m_Radius, Centre.Y);
				default: return PointF.Empty;
			}
		}

		internal override SizeF SocketExitVector(int index)
		{
			if (index < 0)
				return new SizeF(1, 0); // should not be requesting this!
			return Vertices[0].VectorTo(SocketPosition(index));
		}
		#endregion

		protected override PointF[] GetPathForArc()
		{
			using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
			{
				path.AddEllipse(CircleRectangle());
				return path.PathPoints;
			}
		}

	}
	#endregion

	#region PartCircle-base class for Arc, semicircle, Pie
	public abstract class PartCircle : Circular
	{
		// base class for Arc and SemiCircle.  Contains some angular logic

		// the following values must be set by CalculateBounds.  They are not stored in the data.  Storing these speeds up drawing and hit testing a lot (I think!)
		protected float m_StartAngle; // the angle to the first point on the radius
		protected float m_EndAngle;
		protected float m_Radius;
		// if m_Bounds is RectangleF.empty then the values in these are undefined (they are not actually cleared)
		protected const float MINIMUMRADIUS = 2;

		protected int m_Direction = 0;
		// this IS stored in the data, however
		// for the arc,the arc can be drawn in either direction.  It is possible to draw nearly an entire 360°
		// so once the user moves off in one direction it maintains that direction way around.  The direction is reset to 0 if the user is approximately over the start line
		// likewise the semicircle can go either way.  This could have been achieved by simply flipping the points over, but that would make some strange results
		// if the user then cancelled.  Should be set back to 0 if the user cancels so that the baseline is being moved

		#region Circular implementation
		// this is needed by some of the functions in Geometry

		public override float Radius
		{
			get
			{
				EnsureBounds();
				return m_Radius;
			}
		}

		protected override PointF[] GetPathForArc() => GetPathPointsForArc(m_StartAngle, m_EndAngle, m_Direction);

		#endregion

		#region Geometry
		protected abstract void SetInformation();
		// derived class must set m_Radius, m_StartAngle, m_EndAngle

		protected override RectangleF CalculateBounds()
		{
			// this needs to do two things differently to the base class.  The base class function would calculate the bounds including all three points
			// we may need to extend this, as the arc can reach outside this box (e.g. in particular if the arc is most of a circle and the part not drawn is very small)
			// secondly this must define the radius and angle values which are stored for later use
			if (!BaseVector().IsEmpty)
				SetInformation();
			RectangleF bounds = BoundsOfVertices();
			if (!BaseVector().IsEmpty)
				bounds = ExpandRectangleToIncludeArc(bounds);
			return bounds;
		}

		protected RectangleF ExpandRectangleToIncludeArc(RectangleF rctBounds)
		{
			// rctBounds should already include any fixed points in the shape.  This will extend the rectangle where necessary to include the entire arc
			// this can be done by checking whether the arc includes the four orthogonal directions, and if so extending the rectangle up to centre + radius
			// if the arc does not include horizontal right (for example) then the fixed points will be sufficient to find the right hand edge of the bounding rectangle
			if (ArcIncludes(Geometry.ANGLEUP))
				Geometry.Extend(ref rctBounds, PointF.Add(Centre, new SizeF(0, -m_Radius)));
			if (ArcIncludes(Geometry.ANGLERIGHT))
				Geometry.Extend(ref rctBounds, PointF.Add(Centre, new SizeF(m_Radius, 0)));
			if (ArcIncludes(Geometry.ANGLEDOWN))
				Geometry.Extend(ref rctBounds, PointF.Add(Centre, new SizeF(0, m_Radius)));
			if (ArcIncludes(Geometry.ANGLELEFT))
				Geometry.Extend(ref rctBounds, PointF.Add(Centre, new SizeF(-m_Radius, 0)));
			return rctBounds;
		}

		protected bool ArcIncludes(float angle)
		{
			// is a given angle between start and end angles
			// the complication is that the end angle might be less than the start angle if going clockwise, or greater than the start angle if going anticlockwise
			if (m_Direction == 0)
				return false;
			bool invert = m_Direction == -1;
			float end = m_EndAngle;
			if (end < m_StartAngle + Geometry.NEGLIGIBLEANGLESMALL * m_Direction)
				end += Geometry.ANGLE360;
			// m_intDirection needed on negligible due to way second part below works.  If the angles are identical we want it to count as a complete 360 (for arc)
			// but increasing end when using negative direction would exclude the entire arc
			Debug.Assert(m_StartAngle >= 0 && m_EndAngle >= 0, "Arc: angles should be positive");
			if (!invert)
			{
				if (m_StartAngle <= angle + Geometry.NEGLIGIBLEANGLESMALL && angle - Geometry.NEGLIGIBLEANGLESMALL <= end)
					return true;
				angle += Geometry.ANGLE360; // try again at +360° in case we are starting at a high angle and wrapping round
				if (m_StartAngle <= angle + Geometry.NEGLIGIBLEANGLESMALL && angle - Geometry.NEGLIGIBLEANGLESMALL <= end)
					return true;
				return false;
			}
			else
			{
				if (m_StartAngle <= angle - Geometry.NEGLIGIBLEANGLESMALL && angle + Geometry.NEGLIGIBLEANGLESMALL <= end)
					return false;
				angle += Geometry.ANGLE360; // try again at +360° in case we are starting at a high angle and wrapping round
				if (m_StartAngle <= angle - Geometry.NEGLIGIBLEANGLESMALL && angle + Geometry.NEGLIGIBLEANGLESMALL <= end)
					return false;
				return true;
			}
		}

		internal static bool ArcIncludes(float startAngle, float endAngle, int direction, float test)
		{
			// a version of the above which can be used externally (e.g. in protractor)
			if (direction == 0)
				return false;
			bool invert = direction == -1;
			if (endAngle < startAngle)
				endAngle += Geometry.ANGLE360;
			if (endAngle == startAngle && direction == 1)
				endAngle += Geometry.ANGLE360;
			Debug.Assert(startAngle >= 0 && endAngle >= 0, "Arc: angles should be positive");
			if (!invert) // have had to split this because returning bolInvert fails when test=end or start - this is inside regardless of bolInvert
			{
				if (startAngle <= test && test <= endAngle)
					return true;
				test += Geometry.ANGLE360; // try again at +360° in case we are starting at a high angle and wrapping round
				if (startAngle <= test && test <= endAngle)
					return true;
				return false;
			}
			else
			{
				if (startAngle < test && test < endAngle)
					return false;
				test += Geometry.ANGLE360; // try again at +360° in case we are starting at a high angle and wrapping round
				if (startAngle < test && test < endAngle)
					return false;
				return true;
			}
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			if (!(transformation is TransformAffine))
			{
				transformation.TransformAngle(ref m_StartAngle);
				transformation.TransformAngle(ref m_EndAngle);
				transformation.TransformDirection(ref m_Direction);
				transformation.TransformScalar(ref m_Radius);
			}
		}

		public override PointF Middle()
		{
			// actual centre isn't very middle - it's usually a vertex, so calculate a point nearer the middle of the shape
			return Geometry.MidPoint(Centre, MidArcPoint());
		}

		protected PointF MidArcPoint()
		{
			// returns the point half way around the arc
			float midAngle = (m_StartAngle + m_EndAngle) / 2;
			if (m_Direction < 0)
			{
				if (m_StartAngle < m_EndAngle)
					midAngle = Geometry.NormaliseAngle(midAngle + Geometry.ANGLE180);
			}
			else if (m_Direction > 0)
			{
				if (m_EndAngle < m_StartAngle)
					midAngle = Geometry.NormaliseAngle(midAngle + Geometry.ANGLE180);
			}
			return Vertices[0] + Geometry.ScalarToVector(m_Radius, midAngle);
			// is this sufficient: ??
		}

		#endregion

		#region Targets, GrabSpots
		// Target.ShapeParameter will be the angle (as with a circle - see Circle for some drawing comments etc)
		// Generate should be overridden again by each class to add the other vertices
		// Target.ShapeIndex should be = -1 if on the arc, otherwise used by the derived class to specify which line it is on
		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			EnsureBounds(); // for angles
			List<Target> targets = new List<Target>();
			// the derived class should add the centre if it wants to - unnecessary on high as this is a line vertex anyway, and needs higher priority on semicircle
			//colTargets.Add(New Target(Me, Centre, Target.Types.Centre, ptMouse))
			SizeF vector = Centre.VectorTo(floating.Centre);
			if (vector.Length() > m_Radius / 3) // don't try and add an edge target if we are very near the middle - this will look very strange/would cause problems if exactly over the centre
			{
				float angle = vector.VectorAngle();
				if (ArcIncludes(angle))
				{
					PointF pt = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, angle));
					Target create = new Target(this, pt, Target.Types.Line, floating);
					create.ShapeParameter = angle; // it will speed up the drawing if we remember the angle
					create.ShapeIndex = -1;
					targets.Add(create);
				}
			}
			return targets;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// see circle
			Debug.Assert(target.ShapeIndex == -1, "PartCircle.DrawLineTarget only draws the line on the Arc - the derived class mustoverride this if it wants to draw a straight lines");
			float sweep = Target.LINEDRAWLENGTH / m_Radius;
			RectangleF bounds = new RectangleF(Centre.X - m_Radius, Centre.Y - m_Radius, m_Radius * 2, m_Radius * 2);
			gr.DrawArc(pn, bounds, Geometry.DotNetAngle(target.ShapeParameter + sweep / 2), -Geometry.Degrees(sweep));
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			EnsureBounds();
			List<GrabSpot> objList = new List<GrabSpot>();
			objList.Add(new GrabSpot(this, GrabTypes.Radius, MidArcPoint()) { Focus = Centre });
			base.AddStandardRotationGrabSpot(objList);
			return objList;
		}

		#endregion

		protected abstract void SetPointsFromAngles();
		// should be implemented to set Vertices (0..n) based upon Centre, radius, m_StartAngle
		// in practice Centre will not have changed (because this is derived from the points anyway).  This is called rather when the radius or angle is adjusted

		protected internal override VerbResult OtherVerb(ClickPosition position, Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					int delta = code == Functions.Codes.Increment ? 1 : -1;
					if (this.ShapeCode != Shapes.Arc) // arc does own condition in override
					{
						if (m_DefinedVertices < 2 || m_Direction == 0)
							return VerbResult.Rejected;
					}
					float step = position.ScalarSnapStep(0);
					if (step <= 0)
						step = delta * Globals.Root.CurrentConfig.ReadSingle(Config.Radius_Step, 1);
					else
						step *= delta;
					if (m_Radius + step < MINIMUMRADIUS)
						return VerbResult.Rejected;
					EnsureBounds();
					m_Radius += step;
					SetPointsFromAngles();
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
				default:
					return VerbResult.Rejected;
			}
		}

		protected internal override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			output.AppendLine("Start angle = " + m_StartAngle.ToString("0.###"));
			output.AppendLine("End angle = " + m_EndAngle.ToString("0.###"));
			output.AppendLine("Radius = " + m_Radius.ToString(DiagnosticFormat));
			output.AppendLine("Winding direction = " + m_Direction);
		}

		protected internal override string StatusInformation(bool ongoing)
		{
			string strStatus = base.StatusInformation(ongoing); // just radius
			switch (m_Direction)
			{
				case 0:
					break;
				case 1:
					strStatus += "  " + Strings.Item("Info_Angle") + ": " + Measure.FormatAngle(Geometry.AngleBetween(m_StartAngle, m_EndAngle));
					break;
				case -1:
					strStatus += "  " + Strings.Item("Info_Angle") + ": " + Measure.FormatAngle(Geometry.AngleBetween(m_EndAngle, m_StartAngle));
					break;
			}
			return strStatus;
		}

		#region BinaryData and Copy
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Direction = reader.ReadInt32();
			m_Bounds = RectangleF.Empty; // this is probably the case anyway, the just in case reloading an object
										 // it is essential for these objects to clear the bounds because this indicates that the various angles are invalid
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Direction);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			PartCircle partCircle = (PartCircle)other;
			m_Direction = partCircle.m_Direction;
			m_StartAngle = partCircle.m_StartAngle;
			m_EndAngle = partCircle.m_EndAngle;
			m_Radius = partCircle.m_Radius;
		}

		#endregion

	}
	#endregion

	#region Semicircle
	public class Semicircle : PartCircle
	{
		// the data is stored as the baseline (2 points) and a direction (in partcircle) specifying whether the arc is then clockwise or anticlockwise from there
		// NB start angle is at Vertices(1) (cos Start angle defined as BaseVector angle)

		#region Basic information
		public override Shapes ShapeCode => Shapes.Semicircle;

		protected override int FixedVerticesLength() => 2;

		public override PointF Centre
		{ get { return Geometry.MidPoint(Vertices[0], Vertices[1]); } }

		protected override LabelPositions LabelPosition => LabelPositions.Line;

		internal override List<Prompt> GetPrompts() => base.GetBaseLinePrompts("Semicircle", true);

		#endregion

		#region Verbs
		// the Start in Lined is OK
		public override VerbResult Cancel(ClickPosition position)
		{
			if (m_DefinedVertices == 1)
				return VerbResult.Destroyed; // was placing the baseline, the only fixed point was the start point
											 // otherwise must have been placing the arc: revert to placing the end of the baseline
			m_DefinedVertices = 1;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			if (m_DefinedVertices < 2)
			{
				// this places the end of the baseline
				if (Vertices[0].ApproxEqual(Vertices[1]))
					return VerbResult.Rejected;
				m_Direction = 1;
				m_DefinedVertices = 2;
				return VerbResult.Continuing;
			}
			else
			{
				// this places the actual circle
				if (m_Direction == 0)
					return VerbResult.Rejected;
				return VerbResult.Completed;
			}
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (m_DefinedVertices < 2)
				return Choose(position);
			Float(position);
			if (m_Direction == 0)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 2 || m_Direction == 0)
				return VerbResult.Rejected;
			return VerbResult.Completed;
		}

		public override VerbResult Float(ClickPosition position)
		{
			if (m_DefinedVertices == 1)
			{
				// placing the end of the baseline - just use the base class
				return base.Float(position);
			}
			else
			{
				m_Direction = Geometry.TurnDirection(Vertices[0], Vertices[1], position.Snapped);
				m_Bounds = RectangleF.Empty;
				base.DiscardPath();
			}
			return VerbResult.Continuing;
		}
		#endregion

		#region Coordinates and calculations

		protected override void CreatePath()
		{
			m_Path = new System.Drawing.Drawing2D.GraphicsPath();
			if (m_DefinedVertices < 2)
				m_Path.AddLine(Vertices[0], Vertices[1]);
			else
			{
				if (m_Direction == 1)
					m_Path.AddArc(CircleRectangle(), Geometry.DotNetAngle(m_StartAngle), Geometry.Degrees(Geometry.AngleBetween(m_StartAngle, m_EndAngle)));
				else
					m_Path.AddArc(CircleRectangle(), Geometry.DotNetAngle(m_EndAngle), Geometry.Degrees(Geometry.AngleBetween(m_EndAngle, m_StartAngle)));
			}
			m_Path.CloseFigure();
		}

		protected override void SetInformation()
		{
			if (BaseVector().IsEmpty)
				return;
			m_Radius = BaseVector().Length() / 2; // the points stored in the data are a diameter
			m_StartAngle = BaseVector().VectorAngle();
			m_EndAngle = Geometry.NormaliseAngle(m_StartAngle + Geometry.PI);
		}

		protected override void SetPointsFromAngles()
		{
			// mainly used when the radius has changed
			PointF ptCentre = Centre;
			Vertices[0] = PointF.Add(ptCentre, Geometry.ScalarToVector(m_Radius, m_EndAngle));
			Vertices[1] = PointF.Add(ptCentre, Geometry.ScalarToVector(m_Radius, m_StartAngle));
			m_Bounds = RectangleF.Empty;
		}

		protected override RectangleF CalculateBounds()
		{
			RectangleF rctBounds = base.CalculateBounds();
			base.ExtendBoundsToAccountForLineLabel(rctBounds); // is automatically ignored if there is no label
			return rctBounds;
		}

		protected override PointF[] LineLabelGetPoints() => Vertices.ToArray();

		internal override float[] GetRelevantAngles()
		{
			return new[] { Geometry.VectorAngle(Vertices[0], Vertices[1]) };
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			if (base.TidyVertices(mode, page, 1))
			{
				// centre is automatically defined from the vertices
				m_Radius = Geometry.DistanceBetween(Vertices[0], Vertices[1]) / 2;
				m_StartAngle = Geometry.VectorAngle(Vertices[0], Vertices[1]);
				m_EndAngle = Geometry.VectorAngle(Vertices[1], Vertices[0]);
				return true;
			}
			return false;
		}
		#endregion

		#region Targets

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> targets = base.GenerateTargets(floating);
			// the base class added "Centre", which in this case is the average of the vertices.  We just need to add both vertices themselves and the line
			targets.Add(new Target(this, Centre, Target.Types.Centre, floating, Target.Priorities.Vertex)); // use higher priority so that it overrides the line
			targets.Add(new Target(this, Vertices[0], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			targets.Add(new Target(this, Vertices[1], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			Target target = GenerateTargetOnOneLine(this, floating, Vertices[0], Vertices[1], targets, 1);
			if (target != null)
				targets.Add(target);
			return targets;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			if (target.ShapeIndex < 0)// base class (PartCircle draws the target on the edge)
				base.DrawLineTarget(target, gr, pn, activePhase);
			else
				DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, Vertices[0], Vertices[1]);
		}

		internal override List<Socket> GetSockets()
		{
			return new List<Socket> { new Socket(this, 0, Centre), new Socket(this, 1, Vertices[0]), new Socket(this, 2, Vertices[1]), new Socket(this, 3, MidArcPoint()) };
		}

		internal override PointF SocketPosition(int index)
		{
			switch (index)
			{
				case 0:
					return Centre;
				case 1:
				case 2:
					return Vertices[index - 1];
				case 3:
					return MidArcPoint();
				default:
					throw new ArgumentException(nameof(index));
			}
		}

		internal override SizeF SocketExitVector(int index)
		{
			switch (index)
			{
				case 0: return Geometry.ScalarToVector(1, (m_StartAngle + m_EndAngle) / 2 + Geometry.ANGLE180);
				case 1: return Geometry.ScalarToVector(1, m_EndAngle + Geometry.ANGLE45 * m_Direction);
				case 2: return Geometry.ScalarToVector(1, m_StartAngle + Geometry.ANGLE45 * -m_Direction);
				case 3: return Geometry.ScalarToVector(1, (m_StartAngle + m_EndAngle) / 2);
				default: throw new ArgumentException(nameof(index));
			}
		}

		#endregion

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			// the diameter is drawn whether still placing the diameter/radius or the entire shape is present
			base.InternalDrawFromPath(gr, resources);
		}

		// Load/Save/CopyFrom in base class are sufficient

	}
	#endregion

	#region Ellipse
	public class Ellipse : Filled
	{

		// the ellipse is complex because we need an ellipse which is not horizontal or vertical.
		//  .net, however (and Windows), only does orthogonal ones.  If the ellipse is arranged at an angle we will need to process it differently
		// was a series of points (to v0.03).  Now use 4 Besier curves.  Technically these aren't precise, but are much more accurate
		// than points - visually error looks as if it < 1 in 10000
		// if it is horizontal then we can do things faster by letting .net (/Windows) process everything
		// Vertices (0) and Vertices (1) form the "major" axis (well actually initial axis - the other axis can be longer)
		// m_szMinorAxis is the vector for HALF the minor axis (i.e. from centre of Major axis to the edge, and -m_szMinorAxis gets to the other edge)
		// m_intDefinedVertices is not literally a number of vertices:
		// = 3 once the minor axis is set

		private SizeF m_MinorAxis = new SizeF(0, 0);
		private PointF[] m_Bezier = new PointF[13]; // 4 curves (points: 0-3, 3-6, 6-9, 9-12)
													// m_Path from Lined is the path of these points. Also set by CalculateDynamicVertices
													// m_Path = nothing is used to test if m_ptBezier is defined.  This is sufficient to cope with deserialise I think - path will be null and both recalculated

		public Ellipse()
		{ }

		/// <summary>Creates the ellipse given 3 nominal points: the 2 ends of the major axis, and either of the points at the end of the minor axis.
		/// In practice the first 2 points can be the minor axis and it will work.  And the 3rd point is adjusted if not valid: the distance from the line to this point determines the half-width of the ellipse. </summary>
		public Ellipse(PointF majorAxisA, PointF majorAxisB, PointF minorAxis)
		{
			Start(new ClickPosition(majorAxisA));
			Choose(new ClickPosition(majorAxisB));
			Complete(new ClickPosition(minorAxis));
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			FillStyle.Colour = Color.Transparent;
		}

		/// <summary>Creates an orthogonal ellipse which fits within the given rectangle</summary>
		public Ellipse(RectangleF rect) : this(new PointF(rect.X, rect.Y + rect.Height / 2), new PointF(rect.Right, rect.Y + rect.Height / 2), new PointF(rect.X + rect.Width / 2, rect.Top))
		{ }

		#region Coordinate calculations
		private bool Orthogonal
		{ get { return Geometry.LineApproxPerpendicular(Vertices[0], Vertices[1]); } }

		internal const float KAPPA = 0.5522847498f;
		// see: http://www.whizkidtech.redprince.net/bezier/circle/
		private void CalculateDynamicVertices()
		{
			if (m_DefinedVertices < 2)
			{
				for (int index = 0; index <= 12; index++)
				{
					m_Bezier[index] = PointF.Empty;
				}
				m_Path?.Dispose();
				m_Path = null;
			}
			else
			{
				SizeF minorKappa = m_MinorAxis.MultiplyBy(KAPPA);
				SizeF majorKappa = Centre.VectorTo(Vertices[1]).MultiplyBy(KAPPA);
				m_Bezier[0] = Vertices[0];
				m_Bezier[1] = Vertices[0] + minorKappa;
				m_Bezier[2] = Centre + m_MinorAxis - majorKappa;
				m_Bezier[3] = Centre + m_MinorAxis; // end of curve 1, start of curve 2
				m_Bezier[4] = Centre + m_MinorAxis + majorKappa;
				m_Bezier[5] = Vertices[1] + minorKappa;
				m_Bezier[6] = Vertices[1];
				m_Bezier[7] = Vertices[1] - minorKappa;
				m_Bezier[8] = Centre - m_MinorAxis + majorKappa;
				m_Bezier[9] = Centre - m_MinorAxis;
				m_Bezier[10] = Centre - m_MinorAxis - majorKappa;
				m_Bezier[11] = Vertices[0] - minorKappa;
				m_Bezier[12] = Vertices[0]; // this is repetition of (0), but probably easier to include it to make last 4 points a valid bezier
				m_Path?.Dispose();
				m_Path = new System.Drawing.Drawing2D.GraphicsPath();
				for (int curve = 0; curve <= 3; curve++)
				{
					m_Path.AddBezier(m_Bezier[curve * 3], m_Bezier[curve * 3 + 1], m_Bezier[curve * 3 + 2], m_Bezier[curve * 3 + 3]);
				}
				m_Path.CloseFigure();
			}
		}

		protected override void CreatePath()
		{
			CalculateDynamicVertices();
		}

		private PointF EdgeAtAngleRelative(float nominalAngle)
		{
			// calculates the point on circumference at the given angle (counts clockwise from the direction of the major axis).
			// HOWEVER the angle is not actually accurate.  The formula below is basically the one for a circle, stretched differently in the major and minor directions
			// this stretch distorts the angle.  However the formula will give the outline of the ellipse as the angle is rotated from 0 to 360°
			Debug.Assert(m_DefinedVertices >= 2); // only meaningful once the baseline has been fixed
			float cos = (float)Math.Cos(nominalAngle);
			float sin = (float)Math.Sin(nominalAngle);
			SizeF major = BaseVector();
			// note that the major axis is the entire axis, whereas minor is only half the axis
			float X = major.Width * cos / 2 + m_MinorAxis.Width * -sin; // -sin because angle measured the wrong way
			float Y = major.Height * cos / 2 + m_MinorAxis.Height * -sin;
			return PointF.Add(Centre, new SizeF(X, Y));
		}

		private float GetNominalAngle(float absolute)
		{
			// gets the nominal angle which can be passed to EdgeAtAngleRelative, from the given accurate absolute angle
			// the parameter is measured in the world coordinates; the return value is clockwise from the major axis
			// this can be calculated by taking the trig values from the provided angle, and stretching them in proportion to the ellipse
			// and then converting back to an angle again
			float axis = BaseVector().VectorAngle();
			float relative = Geometry.NormaliseAngle(absolute - axis);
			float cos = (float)Math.Cos(relative);
			float sin = (float)Math.Sin(relative);
			float majorRatio = BaseVector().Length() / 2 / m_MinorAxis.Length(); // the ratio major: minor axis lengths
			cos /= majorRatio;
			float nominal = (float)Math.Atan(sin / cos);
			if (cos < 0)
				nominal += Geometry.PI;
			return nominal;
		}

		protected override RectangleF CalculateBounds()
		{
			if (m_DefinedVertices < 2 || m_MinorAxis.IsEmpty)
			{
				// we are just placing the baseline - the base class will do this OK
				return base.BoundsOfVertices();
			}
			else
			{
				if (Orthogonal)
				{
					// can be calculated faster without so much angular geometry... moving by the minor axis vector (which is stored as half of the axis)
					// away from the major axis baseline will give us the four corners
					RectangleF rctBounds = new RectangleF(Centre, new SizeF(1, 1));
					Geometry.Extend(ref rctBounds, PointF.Add(Vertices[0], m_MinorAxis));
					Geometry.Extend(ref rctBounds, PointF.Subtract(Vertices[0], m_MinorAxis));
					Geometry.Extend(ref rctBounds, PointF.Add(Vertices[1], m_MinorAxis));
					Geometry.Extend(ref rctBounds, PointF.Subtract(Vertices[1], m_MinorAxis));
					return rctBounds;
				}
				else
				{
					if (m_Path == null)
						CalculateDynamicVertices();
					return m_Path.GetBounds();
				}
			}
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformVector(ref m_MinorAxis);
			CalculateDynamicVertices();
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			if (m_DefinedVertices < 2) // because path not defined
				return base.RefreshBoundsFromBounds();
			else
				return base.RefreshBounds(withShadow);
		}
		#endregion

		#region Basic information

		public override Shapes ShapeCode
		{
			get { return Shapes.Ellipse; }
		}

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices < 2)
				return requested;
			if (Orthogonal || requested == SnapModes.Shape)
				return requested; // we can still snap if the minor axis is directly vertical or horizontal
								  // if it goes at an angle there is no way to meaningfully make it hit a grid line/point
								  // we allow snapping to shape because this might be useful; I am not convinced that the results will help the user much, so possibly or Shape can be removed
			return SnapModes.Off;
		}

		public override PointF Centre => Geometry.MidPoint(Vertices[0], Vertices[1]);

		protected override LabelPositions LabelPosition => LabelPositions.RotatedRectangle;

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (m_DefinedVertices == 1)
			{
				list.Add(new Prompt(ShapeVerbs.Complete, "Ellipse_Choose", "Ellipse_Choose"));
				list.Add(new Prompt(ShapeVerbs.Choose | ShapeVerbs.Complete, "CancelAll", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Complete, "Ellipse_Finish", "Ellipse_Finish"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "Ellipse_Cancel1", "Ellipse_Cancel1"));
			}
			return list;
		}

		protected internal override string StatusInformation(bool ongoing)
		{
			string status = Strings.Item("Info_MajorAxis") + ": ";
			float major = Geometry.DistanceBetween(Vertices[0], Vertices[1]);
			float minor = m_MinorAxis.Length() * 2; // double because the vector stored is only half the axis
			if (m_MinorAxis.IsEmpty || minor < major)
			{
				status += Measure.FormatLength(major);
				if (!m_MinorAxis.IsEmpty)
					status += "  " + Strings.Item("Info_MinorAxis") + ": " + Measure.FormatLength(minor);
				return status;
			}
			else
			{
				// minor axis is actually longer, describe that is the major axis
				return status + Measure.FormatLength(minor) + "  " + Strings.Item("Info_MinorAxis") + ": " + Measure.FormatLength(major);
			}
		}

		public override AllowedActions Allows => base.Allows | AllowedActions.Tidy;

		protected override LineLogics LineLogic => LineLogics.UsePath;

		#endregion

		#region Verbs
		// the base class Start is OK - it places 2 initial vertices at the start point

		public override VerbResult Cancel(ClickPosition position)
		{
			if (m_DefinedVertices < 2)
			{
				// we were still placing the baseline
				return VerbResult.Destroyed;
			}
			else
			{
				// we were placing the minor axis
				m_MinorAxis = new SizeF(0, 0);
				m_DefinedVertices = 1;
				m_Path = null;
				return VerbResult.Continuing;
			}
		}

		public override VerbResult Choose(ClickPosition position)
		{
			PointF pt = position.Snapped;
			if (m_DefinedVertices < 2)
			{
				// fixes the major axis
				if (pt.Equals(Vertices[0]))
					return VerbResult.Rejected;
				Vertices[1] = pt;
				m_DefinedVertices = 2;
				Float(position);
				return VerbResult.Continuing;
			}
			else
			{
				// fixes the ellipse
				Float(position);
				if (m_MinorAxis.Length() < LineStyle.Width)
					return VerbResult.Rejected; // the user is hovering directly over the major axis line, just giving a straight line
				m_DefinedVertices = 3;
				return VerbResult.Completed;
			}
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (m_DefinedVertices < 2)
				return Choose(position);
			return Choose(position);
		}

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 2 || m_MinorAxis.Length() < LineStyle.Width)
				return VerbResult.Rejected;
			m_DefinedVertices = 3;
			return VerbResult.Completed;
		}

		public override VerbResult Float(ClickPosition position)
		{
			if (m_DefinedVertices < 2)
				// still placing the baseline
				return base.Float(position);
			else
			{
				PointF edge = Geometry.PerpendicularPoint(Vertices[0], Centre, position.Snapped); // calculates the point the correct distance from the baseline, away from the centre
				m_MinorAxis = Centre.VectorTo(edge);
				// always stored left-handed: minor axis is 90deg left of major axis
				if (Geometry.TurnDirection(BaseVector(), m_MinorAxis) > 0)
					m_MinorAxis = m_MinorAxis.MultiplyBy(-1);
				CalculateDynamicVertices();
				m_Bounds = RectangleF.Empty;
				return VerbResult.Continuing;
			}
		}
		#endregion

		#region Targets and GrabSpots, sockets
		// we can add the ends of the axes as vertices.  We then also need to add a line target for the edge
		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> targets = new List<Target>();
			// centre
			targets.Add(new Target(this, Centre, Target.Types.Centre, floating));
			// Major axis:
			targets.Add(new Target(this, Vertices[0], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			targets.Add(new Target(this, Vertices[1], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			// minor axis:
			targets.Add(new Target(this, PointF.Add(Centre, m_MinorAxis), Target.Types.Vertex, floating, Target.Priorities.Vertex));
			targets.Add(new Target(this, PointF.Subtract(Centre, m_MinorAxis), Target.Types.Vertex, floating, Target.Priorities.Vertex));
			SizeF vector = Centre.VectorTo(floating.Centre);
			// check that we are not very close to the centre (in which case an edge target is not such a good idea)
			float distance = vector.Length();
			float minor = m_MinorAxis.Length();
			float major = BaseVector().Length() / 2;
			// we don't allow snapping to the edge if the ellipse is very lopsided - the method we use of taking the line from centre to the mouse
			// tends to look less intuitive this way.  You can also get a lot of points on top of each other
			if (minor < major * 8 && major < minor * 8)
			{
				if (distance > minor / 3 && distance > major / 3)
				{
					// distance must be greater than one third of each of the "radii".
					float angle = vector.VectorAngle();
					angle = GetNominalAngle(angle);
					PointF pt = EdgeAtAngleRelative(angle);
					Target target = new Target(this, pt, Target.Types.Line, floating);
					target.ShapeParameter = angle; // it will speed up the drawing if we remember the nominal angle (which can be passed to EdgeAtAngleRelative and CalculateVerticesOverAngularRange)
					targets.Add(target);
				}
			}
			return targets;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// Target.ShapeParameter is the angle of this target
			// need to calculate the angle across which to draw based upon the "radius" of this ellipse.  We need to achieve an approximately consistent linear size
			// use the largest of the axes as the "radius" (safer this way)
			float radius = Math.Max(m_MinorAxis.Length(), BaseVector().Length() / 2);
			float sweep = Target.LINEDRAWLENGTH / radius; // see Circle

			// just draw a straight line - seems accurate enough - cos target is quite wide and pulsing it will be hard to spot the inaccuracy
			// if the ellipse is v small (& therfore tightly curved) the vertices take priority and the line is hardly seen
			PointF A = EdgeAtAngleRelative(target.ShapeParameter - sweep / 2);
			PointF B = EdgeAtAngleRelative(target.ShapeParameter + sweep / 2);
			gr.DrawLine(pn, A, B);
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			base.AddBaselineGrabSpot(list);
			PointF centre = Geometry.MidPoint(Vertices[0], Vertices[1]);
			list.Add(new GrabSpot(this, GrabTypes.Radius, centre + m_MinorAxis, centre));
			list.Add(new GrabSpot(this, GrabTypes.Radius, centre - m_MinorAxis, centre));
			return list;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					if (Vertices[0].ApproxEqual(Vertices[1]))
						return;
					float transverseSize = m_MinorAxis.Length();
					Vertices[1] = move.Current.Snapped;
					SizeF transverse = BaseVector().Perpendicular(-1); // Float uses left hand by convention
					m_MinorAxis = transverse.ChangeLength(transverseSize);
					CalculateDynamicVertices();
					m_Bounds = CalculateBounds();
					break;
				case GrabTypes.Radius:
					SizeF oldMinor = m_MinorAxis;
					Float(move.Current);
					if (m_MinorAxis.Length() < Geometry.THINLINE)
						// is degenerate - forming a line; restore previous state
						m_MinorAxis = oldMinor;
					m_Bounds = CalculateBounds();
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
			else
				base.DoGrabAngleSnap(move);
		}

		internal override List<Socket> GetSockets()
		{
			// sockets are at the vertices only
			if (m_Path == null)
				CalculateDynamicVertices();
			if (m_Path == null)
				return null; // being drawn???? none available
			List<Socket> colSockets = new List<Socket>();
			colSockets.Add(new Socket(this, Socket.AUTOMATIC, Middle()));
			for (int index = 0; index <= 3; index++)
			{
				colSockets.Add(new Socket(this, index, m_Bezier[index * 3]));
			}
			return colSockets;
		}

		internal override PointF SocketPosition(int index)
		{
			if (index == -1)
				return Middle();
			if (index < 0 || index > 3)
				return PointF.Empty;
			if (m_Path == null)
				CalculateDynamicVertices();
			return m_Bezier[index * 3];
		}

		internal override SizeF SocketExitVector(int index)
		{
			if (index < 0)
				return new SizeF(1, 0); // should not be requesting this!
			return Centre.VectorTo(SocketPosition(index));
		}
		#endregion

		#region Deducing Bezier parameter from point
		// see handwritten notes.  If we assume that a Bezier curve is for a unit circle, going from the x-axis to the positive y-axis
		// Then the y-coordinate = at^3 + bt^2 + ct for the constants defined below
		// Unfortunately since this has 3 real roots solving to get T from Y can be done algebraically, but involves complex numbers at the intermediate step
		// which .net does not really handle.  So I am going to generate a lookup table from Y to T

		// the parameters from the cubic function for Y given T
		private const float a = 3 * KAPPA - 2;
		private const float b = 3 - 6 * KAPPA;
		private const float c = 3 * KAPPA;

		private static readonly float[] TfromY = new float[10001]; // The T value for Y*10,000
		private static float YfromT(float T)
		{
			return a * T * T * T + b * T * T + c * T;
		}
		static Ellipse()
		{
			// fill in the lookup table TfromY
			// iterate T, taking the average of all the T values which give the same Y value
			int currentY = 0; // at *10000 - i.e. index into look up table
			int values = 1; // number of T values which count towards the current Y
			float totalT = 0; // total of the T values which count towards the current Y
			for (float T = 0; T <= 1; T += 0.000001f)
			{
				int Y = (int)Math.Round(10000 * YfromT(T));
				if (Y == currentY)
				{
					values += 1;
					totalT += T;
				}
				else
				{
					// store the previous value
					while (currentY < Y)
					{
						TfromY[currentY] = totalT / values;
						currentY += 1;
					}
					values = 1;
					totalT = T;
				}
			}
			// and will need to store the final value
			TfromY[10000] = 1;
		}

		internal static float T_FromRelativeY(float Y)
		{
			// Used when deducing the T parameter for one of the Bezier curves from a point
			// Y is the distance travelled in the direction that the curve starts moving (i.e. along vector P0-P1)
			return TfromY[(int)Math.Round(Y * 10000f)];
		}

		private void EnsureIntersectionParameters(Intersection intersection)
		{
			// the line intersection code does not set the Bezier parameters in the intersection (because it does not really have the information at that time)
			// this fills in the missing information if necessary; it can be derived from the point of the intersection (with difficulty!)
			int line = 0;
			float parameter = 0;
			intersection.Parameters(this, ref line, ref parameter);
			// line index will be -1 for undefined
			if (line > -1)
				return;
			// get the location of this point along the major and minor axis away from the centre AS FRACTION OF AXIS LENGTH...
			// (ie coordinate if this shape was a unit circle)
			SizeF vector = Centre.VectorTo(intersection.Position);
			SizeF major = Vertices[0].VectorTo(Centre);
			float majorSize = Geometry.ProjectionScalar(vector, major) / major.Length();
			float minorSize = Geometry.ProjectionScalar(vector, m_MinorAxis) / m_MinorAxis.Length();
			// rounding errors can put the Y value slightly out of bounds; rather than checking both major and minor now, Y is updated at the end
			Debug.Assert(majorSize >= -1.0001 && majorSize <= 1.0001);
			Debug.Assert(minorSize >= -1.0001 && minorSize <= 1.0001);
			float Y; // the distance from this point along the Bezier on which it resides, in the direction in which the Bezier starts
					 // (see handwritten notes for the directions of the Bezier curves)
			if (majorSize <= 0)
			{
				if (minorSize > 0)
				{ line = 0; Y = minorSize; }
				else
				{ line = 3; Y = -majorSize; }
			}
			else
			{
				if (minorSize > 0)
				{ line = 1; Y = majorSize; }
				else
				{ line = 2; Y = -minorSize; }
			}
			if (Y > 1)
				Y = 1;
			Debug.Assert(Y >= 0);
			parameter = T_FromRelativeY(Y); // gets the Bezier parameter from this Y value
			intersection.SetIndex(line * 3, this); // *3 since it is a path index
			intersection.SetParameter(parameter, this);
		}

		#endregion

		#region Miscellaneous coordinates

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			Debug.Assert(m_DefinedVertices == 3);
			SizeF vector = Centre.VectorTo(clickPoint);
			float angle = vector.VectorAngle();
			PointF edge = EdgeAtAngleRelative(GetNominalAngle(angle));
			float radius = Centre.VectorTo(edge).Length(); // the "radius" in this direction
			if (treatAsFilled || IsFilled)
				return vector.Length() < radius; // the point is inside if the user clicked within this radius
			else
				return Math.Abs(vector.Length() - radius) < Line.LINEHITTOLERANCE + LineStyle.Width / 2;
		}

		protected override PointF[] RectangleLabelGetPoints()
		{
			// we line it up with the axes of the ellipse, but the bounding points are the rectangle containing the ellipse, which is not stored in this class
			PointF[] aPoints =
				{
				Vertices[0] - m_MinorAxis,
				Vertices[1] - m_MinorAxis,
				Vertices[1] + m_MinorAxis,
				Vertices[0] + m_MinorAxis
			};
			return aPoints;
		}

		internal override float[] GetRelevantAngles() => new[] { Geometry.VectorAngle(Vertices[0], Vertices[1]), m_MinorAxis.VectorAngle() };

		public override bool Tidy(SnapModes mode, Page page)
		{
			float length = m_MinorAxis.Length();
			bool changed = base.TidyVertices(mode, page, 1);
			// Not going to bother trying to extend the minor Axis to hit any targets, way too complicated
			if (mode == SnapModes.Grid && Orthogonal)
			{
				float newLength = page.Paper.SnapScalar(length);
				if (newLength > Geometry.NEGLIGIBLE && length != newLength)
				{
					length = newLength;
					changed = true;
				}
			}
			if (changed)
			{
				// need to recalculate the minor Axis whichever of the above items has made changes
				SizeF major = Vertices[0].VectorTo(Vertices[1]); // although this is the new major axis, it shouldn't have moved far enough to stop the direction working
				int direction = Geometry.TurnDirection(major, m_MinorAxis);
				m_MinorAxis = major.Perpendicular(direction).ChangeLength(length);
				m_Bounds = RectangleF.Empty;
				m_Path = null;
			}
			return changed;
		}

		#endregion

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (m_DefinedVertices < 2 || m_DefinedVertices < 3 && m_MinorAxis.Length() < LineStyle.Width)
			{
				// the user is placing the baseline (OR is placing ellipse and minor axis near nil) - just draw baseline
				if (resources.MainPen != null)
					gr.DrawLine(Vertices[0], Vertices[1], resources.MainPen);
			}
			else
			{
				if (Orthogonal)
				{
					if (resources.MainBrush != null || resources.MainPen != null)
						gr.Ellipse(Bounds, resources.MainPen, resources.MainBrush);
				}
				else
				{
					if (m_Path != null && (resources.MainBrush != null || resources.MainPen != null))
						gr.Path(m_Path, resources.MainPen, resources.MainBrush);
				}
			}
		}

		#region Load/Save
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_MinorAxis = reader.ReadSizeF();
			m_DefinedVertices = 3;
			CalculateDynamicVertices();
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_MinorAxis);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Ellipse objEllipse = (Ellipse)other;
			m_MinorAxis = objEllipse.m_MinorAxis;
			m_Bezier = (PointF[])objEllipse.m_Bezier.Clone();
			m_Path?.Dispose();
			if (objEllipse.m_Path == null)
				m_Path = null;
			else
				m_Path = (System.Drawing.Drawing2D.GraphicsPath)objEllipse.m_Path.Clone();
		}

		#endregion

	}

	#endregion

	#region Arc - shape in own right and also base for Pie
	public class Arc : PartCircle
	{
		// draws just part of a circle.  Initially the user draws from the centre to the edge
		// stores 3 points -(0) = centre; (1) = first point on radius; (2) = second point on radius

		protected bool m_First = true; // true if this is the first one created.  This is not stored in the data; it is only used when initially creating the shape
									   // in the arc, only the first can have the radius adjusted; in the Pie only the first cannot be completed to make a circle

		public Arc()
		{ }

		/// <summary>Creates an arc.  startAngle, endAngle are expressed IN RADIANS, from vertically up.  In addition direction indicates which of the 2 possible arcs between these angles is used.
		/// +1 indicates clockwise from start to end, -1 widdershins.</summary>
		/// <remarks>The angles can be positive or negative.  Use Geometry.Radians(angleInDegress) to convert from degrees</remarks>
		public Arc(PointF centre, float radius, float startAngle, float endAngle, int direction)
		{
			m_DefinedVertices = 3;
			Status = StatusValues.Complete;
			m_Radius = radius;
			m_StartAngle = Geometry.NormaliseAngle(startAngle);
			m_EndAngle = Geometry.NormaliseAngle(endAngle);
			Vertices.Add(centre);
			Vertices.Add(PointF.Empty);
			Vertices.Add(PointF.Empty);
			SetPointsFromAngles(); // defines the second two points
			if (direction != 1 && direction != -1)
				throw new ArgumentException("Direction must be -1 or 1");
			m_Direction = direction;
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			FillStyle.Colour = Color.Transparent; // not actually used anyway IIRC, but this would make more sense
		}

		#region Basic information
		public override Shapes ShapeCode => Shapes.Arc;
		public override PointF Centre => Vertices[0];
		public override bool IsFilled => false;
		protected override int FixedVerticesLength() => 3;

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices >= 2 && requested == SnapModes.Grid)
				return SnapModes.Off; // don't try and snap to grid points while moving around the radius!
			if (m_DefinedVertices == 1 && g_RadiusLocked && requested != SnapModes.Angle)
				return SnapModes.Off; // subsequent arcs also locked to radius
			return base.SnapNext(requested);
		}

		protected override void SetInformation()
		{
			m_Radius = Centre.VectorTo(Vertices[1]).Length();
			if (m_DefinedVertices < 2)
			{
				// still placing the baseline
				m_StartAngle = 0;
				m_EndAngle = 0;
			}
			else
			{
				m_StartAngle = Centre.VectorTo(Vertices[1]).VectorAngle();
				m_EndAngle = Centre.VectorTo(Vertices[2]).VectorAngle();
			}
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (m_DefinedVertices == 1)
			{
				if (!g_RadiusLocked)
					list.Add(new Prompt(ShapeVerbs.Complete, "Arc_Choose1", "Arc_Choose1"));
				else
				{
					if (m_First)
						list.Add(new Prompt(ShapeVerbs.Complete, "Arc_Choose1Locked", "Arc_Choose1"));
					else
						list.Add(new Prompt(ShapeVerbs.Complete, "Arc_Choose1NotFirst", "Arc_Choose1"));
					list.Add(new Prompt(ShapeVerbs.Increment | ShapeVerbs.Decrement, "Arc_IncDec", "Arc_IncDec"));
				}
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Choose, "Arc_Choose2", "Arc_Choose2"));
				list.Add(new Prompt(ShapeVerbs.Complete, "Arc_Finish", "Arc_Finish"));
				list.Add(new Prompt(ShapeVerbs.Increment | ShapeVerbs.Decrement, "Arc_IncDec", "Arc_IncDec"));
				if (m_First)
				{
					if (!g_RadiusLocked)
						list.Add(new Prompt(ShapeVerbs.Info, "Prompts_Arc_ManuallyLock", "TOOLIMG_48_ARC"));
					list.Add(new Prompt(ShapeVerbs.Cancel, "Arc_Cancel1", "Arc_Cancel1"));
				}
				else
					list.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			}
			return list;
		}

		public override PointF Middle()
		{
			//Return MidArcPoint()
			// can't use the above, because this is mainly used for the movement GrabSpot, and we already have a radius GrabSpot at MidArcPoint
			return Geometry.MidPoint(m_DefinedVertices < 2 ? Vertices[0] : Vertices[2], Vertices[1]);
			// when doing first line mid point is mid of line
		}

		public override AllowedActions Allows => (base.Allows | AllowedActions.Arrowheads) & ~AllowedActions.Tidy;

		protected internal override bool Closed() => false;

		/// <summary>the number of degrees away from the start that is required to generate Direction + -1</summary>
		/// <remarks>The angular label increases this somewhat (it is unlikely to want very large angles so often)</remarks>
		protected virtual float ZeroDirectionTolerance => 3;

		#endregion

		#region Radius lock

		protected static bool g_RadiusLocked = false; //all arcs except first get same radius until tool button is pressed again
		/// <summary>The radius used when radius locked</summary>
		internal static float FixedRadius = 10;
		protected static bool m_RadiusWasLocked = false; // remembers radius locked in case of cancellation
		internal static event NullEventHandler RadiusLockedChanged;

		public static bool RadiusLocked
		{
			get { return g_RadiusLocked; }
			set
			{
				if (value == g_RadiusLocked)
					return;
				g_RadiusLocked = value;
				RadiusLockedChanged?.Invoke();
			}
		}

		protected internal override VerbResult OtherVerb(ClickPosition position, Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					if (!RadiusLocked && m_DefinedVertices < 2)
						return VerbResult.Rejected;
					// must set base angle before calling fn below...
					m_StartAngle = Geometry.VectorAngle(Vertices[0], Vertices[1]);
					return base.OtherVerb(position, code);
				default:
					return VerbResult.Rejected;
			}
		}

		#endregion

		#region Verbs

		public override VerbResult Start(ClickPosition position)
		{
			if (g_RadiusLocked)
				m_Radius = FixedRadius;
			return base.Start(position);
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (m_DefinedVertices == 1)
				return VerbResult.Destroyed;
			// was placing the baseline, the only fixed point was the start point
			// otherwise must have been placing the arc: revert to placing the end of the baseline
			// ... except if this is not the first slice, then we do not want to revert back to modifying the radius line
			//If Not m_bolFirst Then Return VerbResult.Destroyed
			// actually we do, because the start point can be set even if the radius itself is not adjusted, so this is moved to Pie
			DiscardVertex();
			m_Direction = 0;
			RadiusLocked = m_RadiusWasLocked;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			switch (m_DefinedVertices)
			{
				case 1:
					// have just placed in the initial line
					if (position.Snapped.ApproxEqual(Vertices[0]))
						return VerbResult.Rejected;
					FixVertex(); // adds another floating vertex for the end of the arc
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
				case 2:
					EnsureBounds();
					m_DefinedVertices += 1;
					m_RadiusWasLocked = g_RadiusLocked;
					RadiusLocked = ShapeCode == Shapes.Arc; // cos Pie calls down to this and DOESN'T lock
					FixedRadius = m_Radius;
					if (Math.Abs(m_EndAngle - m_StartAngle) < Geometry.NEGLIGIBLE)
						return VerbResult.Substitute; // returns a circle instead
					if (m_Direction == 0) // must be after angle condition for Pie, which changes result, and this shape continues
						return VerbResult.Rejected;
					return VerbResult.Spawn;
				default:
					return VerbResult.Unexpected;
			}
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (m_DefinedVertices != 2)
				return Choose(position);
			Choose(position);
			// can't use CompleteRetrospective as that assumes that Spawn has already been dealt with
			m_DefinedVertices = 3;
			FixedRadius = m_Radius;
			return VerbResult.Completed;
		}

		public override VerbResult CompleteRetrospective()
		{
			if (!m_First)
				return VerbResult.Destroyed; // this does not complete the circle (like the pie), but it does terminate creating within the circle
											 // i.e. the arc created by the Choose triggered by the first mouse down will be allowed to stand, and this spawned shape is destroyed
			if (m_DefinedVertices != 2 || m_Direction == 0)
				return VerbResult.Rejected;
			m_DefinedVertices = 3;
			FixedRadius = m_Radius;
			return VerbResult.Completed;
		}

		public override VerbResult Float(ClickPosition position)
		{
			PointF pt = position.Snapped;
			DecacheArrowheads();
			DiscardPath();
			if (m_DefinedVertices < 2)
			{
				// placing first radial point
				if (m_First && !g_RadiusLocked)
					return base.Float(position); // the base class can cope with placing the FIRST radius
				else
				{
					// except in subsequent arcs we must maintain the radius
					SizeF vector = Centre.VectorTo(pt);
					if (vector.Length() < Geometry.NEGLIGIBLE)
						return VerbResult.Continuing; // ignore attempts to move over the centre spot
													  // this will result in an indeterminate angle
					float angle = vector.VectorAngle();
					Vertices[1] = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, angle));
					m_Bounds = CalculateBounds();
					return VerbResult.Continuing;
				}
			}
			else
			{
				EnsureBounds(); // for the radius and angle
				Debug.Assert(m_Path == null); 
				SizeF vector = Centre.VectorTo(pt);
				if (vector.Length() < Geometry.NEGLIGIBLE)
					return VerbResult.Continuing; // ignore attempts to move over the centre spot
				float angle = vector.VectorAngle();
				if (Geometry.AbsoluteAngularDifference(angle, m_StartAngle) < Geometry.Radians(ZeroDirectionTolerance))
				{
					// we are very close to the start line - either close it if very long, or set the direction to nil
					m_EndAngle = angle; // needed to get MidArcPoint to use current float point (cos start and end will be identical the second time this runs)
					if (m_Direction == 0 || Geometry.DistanceBetween(MidArcPoint(), Vertices[0]) < m_Radius) // second condition checks arc is currently short
						m_Direction = 0;
					else                        // we have gone near 360 - make it 360
						m_EndAngle = m_StartAngle;
					Vertices[2] = Vertices[1];
				}
				else
				{
					// we are far enough away from the start line to determine which way we are going if we were previously hovering over the start line
					if (m_Direction == 0)
						m_Direction = Geometry.DirectionFromSimilarAngles(m_StartAngle, angle);
					Vertices[2] = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, angle));
				}
				m_Bounds = CalculateBounds();
				return VerbResult.Continuing;
			}
		}

		protected internal override bool AllowVerbWhenComplete(Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					return true;
			}
			return base.AllowVerbWhenComplete(code);
		}

		#endregion

		#region Coords - HitTest, Sockets, Targets, GrabSpots

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			// path version certainly doesn't work properly when placing the last point
			return base.RefreshBoundsFromBounds(withShadow);
		}

		protected override void CreatePath()
		{
			m_Path = new System.Drawing.Drawing2D.GraphicsPath();
			m_Path.StartFigure();
			if (m_DefinedVertices < 2)
				m_Path.AddLine(Vertices[0], Vertices[1]);
			else
			{
				if (m_Direction == 1)
					m_Path.AddArc(CircleRectangle(), Geometry.DotNetAngle(m_StartAngle), Geometry.Degrees(Geometry.AngleBetween(m_StartAngle, m_EndAngle)));
				else
					m_Path.AddArc(CircleRectangle(), Geometry.DotNetAngle(m_EndAngle), Geometry.Degrees(Geometry.AngleBetween(m_EndAngle, m_StartAngle)));
			}
		}

		protected override void SetPointsFromAngles()
		{
			// this version can be triggered when adjusting radius and drawing baseline, in which case just V(1) is adjusted
			Vertices[1] = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, m_StartAngle));
			if (m_DefinedVertices > 1)
				Vertices[2] = PointF.Add(Centre, Geometry.ScalarToVector(m_Radius, m_EndAngle));
			m_Bounds = RectangleF.Empty;
			DiscardPath();
		}

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			List<Target> targets = base.GenerateTargets(floating);
			targets.Add(new Target(this, Vertices[0], Target.Types.Centre, floating, Target.Priorities.Low));
			targets.Add(new Target(this, Vertices[1], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			targets.Add(new Target(this, Vertices[2], Target.Types.Vertex, floating, Target.Priorities.Vertex));
			return targets;
		}

		internal override List<Socket> GetSockets()
		{
			List<Socket> list = new List<Socket>();
			list.Add(new Socket(this, 1, Vertices[1]));
			list.Add(new Socket(this, 2, Vertices[2]));
			if (Geometry.AngleBetween(m_StartAngle, m_EndAngle) > Geometry.ANGLE90 / 3)               // we only add the midpoint of the arc if it is not too small an angle
				list.Add(new Socket(this, 3, MidArcPoint()));
			return list;
		}

		//These functions support more socket positions than are reported above, because these also work with the Pie object
		internal override PointF SocketPosition(int index)
		{
			switch (index)
			{
				case -1:
					return Centre; // not actually used, but shapes are supposed to support this
				case 0:
				case 1:
				case 2:
					return Vertices[index];
				case 3:
					return MidArcPoint();
				case 4:
				case 5:
					return Geometry.MidPoint(Centre, Vertices[index - 3]);
				default:
					throw new InvalidOperationException();
			}
		}

		internal override SizeF SocketExitVector(int index)
		{
			switch (index)
			{
				case 0:
					return Geometry.ScalarToVector(1, (m_StartAngle + m_EndAngle) / 2 + Geometry.ANGLE180);
				case 1:
					return Geometry.ScalarToVector(1, m_StartAngle + Geometry.ANGLE45 * -m_Direction);
				case 2:
					return Geometry.ScalarToVector(1, m_EndAngle + Geometry.ANGLE45 * m_Direction);
				case 3:
					return Geometry.ScalarToVector(1, (m_StartAngle + m_EndAngle) / 2);
				case 4:
					return Vertices[0].VectorTo(Vertices[1]).Perpendicular(-m_Direction);
				case 5:
					return Vertices[0].VectorTo(Vertices[2]).Perpendicular(m_Direction);
				default:
					throw new ArgumentException("SocketExitVector: index");
			}
		}

		protected internal override PointF DoSnapAngle(PointF newPoint)
		{
			// different from base class - BOTH points use centre origin
			return Geometry.AngleSnapPoint(newPoint, Centre);
		}

		#endregion

		#region Drawing, arrowheads

		protected override void PrepareDraw(DrawResources resources)
		{
			DefaultPrepareDrawForText(resources);
			DefaultPrepareLineDraw(resources, LineStyle); // we don't really want the Filled version because that will create a brush
			if (m_DefinedVertices == 2)
			{
				// placing the second arc point.  In this case we draw both the radial lines faintly
				resources.CustomPen = resources.Graphics.CreateStroke(Color.FromArgb(LineStyle.Colour.A / 5, LineStyle.Colour), Geometry.THINLINE);
			}
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.MainPen == null)
				return;
			if (m_DefinedVertices < 2 || m_Direction == 0)                // while placing the initial radius just draw that...
				gr.DrawLine(Vertices[0], Vertices[1], resources.MainPen);
			else
			{
				// we must provide a bounding box for the arc.  This is not the bounding box of the shape (which is just the arc itself)
				// but the bounding box of the imaginary circle of which the arc is part
				EnsureBounds(); // to check that radius and angles are set
				RectangleF bounds = new RectangleF(Centre.X - m_Radius, Centre.Y - m_Radius, m_Radius * 2, m_Radius * 2);
				if (m_Direction == 1)
					gr.DrawArc(bounds, Geometry.DotNetAngle(m_StartAngle), Geometry.Degrees(Geometry.AngleBetween(m_StartAngle, m_EndAngle, true)), resources.MainPen);
				else
					gr.DrawArc(bounds, Geometry.DotNetAngle(m_EndAngle), Geometry.Degrees(Geometry.AngleBetween(m_EndAngle, m_StartAngle, true)), resources.MainPen);
				if (resources.CustomPen != null)
				{
					// we also draw the radial lines to help when creating the arc
					gr.DrawLine(Vertices[0], Vertices[1], resources.CustomPen);
					gr.DrawLine(Vertices[0], Vertices[2], resources.CustomPen);
				}
				if ((Allows & AllowedActions.Arrowheads) > 0)
					base.DrawArrowheads(resources); // not all derived classes allow them
			}
		}

		protected override PointF[] ArrowheadVector(bool isEnd)
		{
			if (Vertices.Count < 3)
				return null; // arc not present yet - no arrowheads
			PointF endPoint = Vertices[isEnd ? 2 : 1]; // the position of the arrow head itself
			float direction;
			if (isEnd)
				direction = m_EndAngle - m_Direction * Geometry.ANGLE90;
			else
				direction = m_StartAngle + m_Direction * Geometry.ANGLE90;
			// The important thing is the direction, but the length will cause the arrowhead to shrink if small
			// by using the radius it should ensure a reason the small arrowhead if it is a tight circle
			// can't use m_sngRadius as it is only defined by CalculateBounds - which will request the arrowhead vectors, giving an infinite loop
			float length = Vertices[0].VectorTo(Vertices[1]).Length();
			length = Math.Min(length / 2, Geometry.DistanceBetween(Vertices[1], Vertices[2]) / 2);
			PointF other = endPoint + Geometry.ScalarToVector(length, direction);
			return new[] { other, endPoint };
		}

		#endregion

		#region Spawning
		// also implemented by Pie, but rather differently.  The Pie immediately start drawing out the arc once it has spawned
		protected internal override Shape Spawn()
		{
			if (Math.Abs(m_EndAngle - m_StartAngle) < Geometry.NEGLIGIBLE)
			{
				// returning a circle in place of this rather than new arc
				Circle create = new Circle(Vertices[0], m_Radius);
				create.FillStyle.Colour = Color.Empty;
				create.LineStyle.CopyFrom(this.LineStyle);
				return create;
			}
			else
			{
				Arc create = new Arc();
				create.CopyFrom(this, CopyDepth.Duplicate, Mapping.Ignore);
				create.Vertices.RemoveAt(2); // ' only the centre point is fixed, and Vertices(1) is floating
				create.m_Radius = FixedRadius;
				create.m_DefinedVertices = 1; // the first point on the radius is incomplete, although the radius will be maintained
				create.m_First = false;
				return create;
			}
		}

		#endregion

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			if (Vertices.Count != 3)
			{
				this.Status = StatusValues.Deleted; // corrupt - won't be added to page
				Utilities.LogSubError("Omitting arc " + this.ID + " on load, since it is corrupt");
			}
		}

		// Load/Save/CopyFrom in base class are sufficient
	}
	#endregion

}
