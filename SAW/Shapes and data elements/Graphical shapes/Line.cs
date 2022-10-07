using System.Collections.Generic;
using System;
using System.Drawing;

namespace SAW.Shapes
{
	public class Line : Lined
	{
		// a line joining two points (may also be orthogonal)

		#region Basic information

		public Line()
		{ }

		public Line(PointF start, PointF end)
		{
			Vertices.Add(start);
			Vertices.Add(end);
			LineStyle.SetDefaults();
			m_DefinedVertices = 2;
			m_Bounds = RectangleF.Empty;
			Status = StatusValues.Complete;
		}

		protected override int FixedVerticesLength() => 2;
		public override Shapes ShapeCode => Shapes.Line;
		public override PointF Centre => PointF.Empty;
		protected override LabelPositions LabelPosition => LabelPositions.Line;
		protected override PointF[] LineLabelGetPoints() => Vertices.ToArray();

		protected internal override string StatusInformation(bool ongoing) => Strings.Item("Info_Length") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[0], Vertices[1]));

		public override AllowedActions Allows => base.Allows | AllowedActions.Tidy;

		protected internal override bool Closed() => false;

		#endregion

		#region Verbs
		// note that when the shape is started, the initial point is entered into the array twice to give the initial line that the user is moving
		public override VerbResult Choose(ClickPosition position)
		{
			return Complete(position);
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (Vertices.Count != 2 || m_DefinedVertices != 1)
				return VerbResult.Unexpected;
			Vertices[1] = position.Snapped;
			m_DefinedVertices = 2;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Completed;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (Vertices.Count != 2 || m_DefinedVertices != 1)
				return VerbResult.Unexpected;
			// this can only happen when we have one vertex specified - it will always cancel the entire shape
			return VerbResult.Destroyed;
		}

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 1)
				return VerbResult.Destroyed; // can happen if the user double clicks at the beginning to start the shape
			return VerbResult.Completed;
		}

		protected internal  override VerbResult OtherVerb(ClickPosition position, SAW.Functions.Codes code)
		{
			switch (code)
			{
				case SAW.Functions.Codes.Increment:
				case SAW.Functions.Codes.Decrement:
					int delta = code == SAW.Functions.Codes.Increment ? 1 : -1;
					float step = position.ScalarSnapStep(0);
					if (step <= 0)
						step = delta * Globals.Root.CurrentConfig.ReadSingle(Config.Radius_Step, 1);
					else
						step *= delta;
					SizeF vector = Vertices[0].VectorTo(Vertices[1]);
					float length = vector.Length();
					if (length + step < 5 || length < Geometry.NEGLIGIBLESMALL)
						return VerbResult.Rejected;
					Vertices[1] = Vertices[0] + vector.ChangeLength(length + step);
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
				default:
					return VerbResult.Rejected;
			}
		}

		#endregion

		#region sockets
		internal override List<Socket> GetSockets()
		{
			return base.DefaultGetSockets(false, true);
			// omits the centre = automatic socket, because there may also be a genuine socket in the centre of the line
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			return base.TidyVertices(mode, page, 1);
		}

		#endregion

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (Vertices.Count < 2)
				return;
			if (resources.MainPen != null)
			{
				if (Vertices[0].ApproxEqual(Vertices[1]))
				{
					// if user has not move the mouse yet (particularly applies to separate keyboard cursor) at least or something visible…
					gr.DrawLine(new PointF(Vertices[0].X - 0.25f, Vertices[0].Y), new PointF(Vertices[1].X + 0.25f, Vertices[1].Y), resources.MainPen);
				}
				else
					gr.DrawLine(Vertices[0], Vertices[1], resources.MainPen);
			}
			if ((Allows & AllowedActions.Arrowheads) > 0)
				base.DrawArrowheads(resources);
		}

		#region Coordinates
		public const float LINEHITTOLERANCE = 2;
		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			float distance = Geometry.DistancePointToLine(Vertices[0], Vertices[1], clickPoint);
			return distance < LineStyle.Width + LINEHITTOLERANCE / scale;
		}

		internal override List<Target> GenerateTargets(UserSocket floating) => base.GenerateTargetsDefault(floating, false);

		protected override RectangleF CalculateBounds()
		{
			RectangleF bounds = base.BoundsOfVertices();
			base.ExtendBoundsToAccountForLineLabel(bounds); // is automatically ignored if there is no label
															//  ExtendBoundsToIncludeArrowhead(rctBounds)
			return bounds;
		}

		public override RectangleF RefreshBounds(bool withShadow = false) => base.RefreshBoundsFromBounds(withShadow); // Can use the simpler version
		internal override float[] GetRelevantAngles() => new[] { Geometry.VectorAngle(Vertices[0], Vertices[1]) };

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[1 - move.ShapeIndex]);
			else
				base.DoGrabAngleSnap(move);
		}

		protected override void CreatePath()
		{
			m_Path = GetLinearPath(Vertices, false);
		}

		#endregion

		// Load/Save/CopyFrom in base class are sufficient

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddGrabSpotsForAllVertices(list);
			return list;
		}

		internal override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			List<Prompt> list = new List<Prompt>();
			list.Add(new Prompt(ShapeVerbs.Complete, ShapeCode + "_Finish", ShapeCode + "_Finish")); // Using Shape means that this also works for InfiniteLine
			list.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			return list;
		}

		internal void InitialiseArrowheadGUISample()
		{
			// Initialises this so that it can be used off the page
			// used both in RoundButton and CreateParameterSampleImage
			LineStyle.Colour = Color.Black;
			LineStyle.Width = 1;
			LineStyle.Pattern = System.Drawing.Drawing2D.DashStyle.Solid;
			Vertices.Clear();
			Vertices.Add(new PointF(4, 8));
			Vertices.Add(new PointF(26, 19));
			m_DefinedVertices = 2;
		}

		internal override (GrabSpot[], string[]) GetEditableCoords(Target selectedElement)
		{
			return (new[] { new GrabSpot(this, GrabTypes.SingleVertex, Vertices[0],0),
				new GrabSpot(this, GrabTypes.SingleVertex, Vertices[1],1)}, new[] { "[SAW_Coord_From]", "[SAW_Coord_To]" });
		}

	}

	/// <summary>a line which can only be drawn at angles of multiples of 45</summary>
	public class Orthogonal : Line
	{

		public override Shapes ShapeCode => Shapes.Orthogonal;

		internal override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			return new List<Prompt>
			{
				new Prompt(ShapeVerbs.Complete, "Line_Finish", "Orthogonal_Finish"),
				new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll")
			};
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (Vertices.Count != 2 || m_DefinedVertices != 1)
				return VerbResult.Unexpected;
			Float(position);
			m_DefinedVertices = 2;
			if (Vertices[0].ApproxEqual(Vertices[1]))
				return VerbResult.Destroyed;
			return VerbResult.Completed;
		}

		protected internal override SnapModes SnapNext(SnapModes requested) => SnapModes.Off;
		// the snapping will be done internally because otherwise it would cause strange interactions with the snapping to the necessary direction

		public override VerbResult Float(ClickPosition position)
		{
			AdjustPoint(1, position);
			return base.Float(position);
		}

		private bool AdjustPoint(int index, ClickPosition position)
		{
			// Used by both Float and DoGrabMove.  Returns true if the point is valid
			PointF pt = position.Snapped;
			// the main task is to restrict the line to the correct directions
			float angularStep = GetAngularStep(position.Page);
			SizeF vector = Vertices[1 - index].VectorTo(pt);
			if (vector.IsEmpty)
				return false;
			// the following calculations will fail if the two points are on top of each other
			// but there is no need to do any of this - the position of point(0) is a valid position (ish)
			// calculate the angle in radians of this line counting clockwise from vertically up (must count from vertically in order to match the isometric paper)
			float angle = vector.VectorAngle();
			float length = vector.Length();
			angle = (float)(Math.Round(angle / angularStep) * angularStep);
			// angle changed to the closest of the allowed angles
			// now snapped the length so that it will hit a grid point
			// the complication is if we are going diagonally on squared paper, in which case the incremental length is sqrt(2) * spacing.
			// the following function should take care of this
			float step = position.ScalarSnapStep(angle);
			if (step > 0)
				length = (float)Math.Round(length / step) * step; // make it a multiple of a grid unit
			vector = Geometry.ScalarToVector(length, angle);
			pt = PointF.Add(Vertices[1 - index], vector);
			// and finally we snap the resulting point back to the page grid - it should already be on this, but just in case
			// of any rounding errors
			if (position.RequestedSnap == SnapModes.Grid)
				position.Snapped = position.Page.Paper.SnapPoint2(pt);
			else
				position.Snapped = pt;
			// in order to avoid flickering it is worth checking if the target has actually moved - because this line is very constrained often it does not move at all, or and repainting a completely unchanged line is the most flickery situation
			return true;
		}

		protected virtual float GetAngularStep(Page objPage) => Geometry.PI / 4; // 1 eighth of a circle - ie any multiple of 45°

		protected internal override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					//we need to ignore the transformation matrix and apply our adjustment
					if (AdjustPoint(move.ShapeIndex, move.Current))
					{
						PointF pt = move.Current.Snapped;
						Vertices[move.ShapeIndex] = pt;
						m_Bounds = RectangleF.Empty;
					}
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			if (mode == SnapModes.Shape || mode == SnapModes.Angle)
				return false;
			return base.Tidy(mode, page);
		}

		// Load/Save/CopyFrom in base class are sufficient
	}

	public class Arrow : Line
	{
		// like the standard line, but draws an arrowhead on the end
		// all the positioning is the same, it is just the graphics which is different (and the bounds calculation possibly?)
		// I tried using the .net Pen.CustomEndCap, but did not have much success.  It is a pain to use, seemed to use xor logic if it intersects the line
		// (which is required!?), and would not help with calculating the expanded bounding rectangle
		// [LATER] - now this uses arrow heads and the only difference is when loading or creating
		// once saved this becomes a standard line.  It reports itself as a standard line

		public override Shapes ShapeCode => Shapes.Line;

		internal override bool DefaultStylesApplied()
		{
			EndArrowhead = new ArrowheadC(true)
			{
				Style = ArrowheadC.Styles.Version4,
				Size = ArrowheadC.DEFAULTSIZE
			};
			if (StartArrowhead != null)
				StartArrowhead.Style = ArrowheadC.Styles.None;
			base.DefaultStylesApplied();
			return true;
		}

		#region Data

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			DefaultStylesApplied();
		}

		#endregion

	}
}
