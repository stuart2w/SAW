using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;

// regular, irregular polygons and PolyLines...

namespace SAW.Shapes
{
	/// <summary>Regular polygon </summary>
	public class Polygon : Sequential
	{
		protected int m_Sides = 6;
		public const int MINIMUMSIDES = 3;
		public const int MAXIMUMSIDES = 20;
		public static int CURRENTSIDES = 6; // a number of sides to use for a new shape

		public static event NullEventHandler CurrentSidesChanged;// Raised when CURRENTSIDES is modified (because it is displayed in one of the buttons)

		public Polygon()
		{ }

		public Polygon(PointF A, PointF B, bool clockwise, int sides)
		{
			m_Sides = sides;
			//Vertices.Add(A);
			//Vertices.Add(B);
			Start(new ClickPosition(A));
			Choose(new ClickPosition(B));
			SetLength(m_Sides);
			PlacePoints(clockwise ? 1 : -1);
			m_Acceptable = true;
			Status = StatusValues.Complete;
			LineStyle.SetDefaults();
			FillStyle.SetDefaults();
			FillStyle.Colour = Color.Transparent;
		}

		#region Basic Information

		public override Shapes ShapeCode => Shapes.Polygon;

		protected override int FixedVerticesLength() => m_Sides;

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (m_DefinedVertices >= 2)
				return SnapModes.Off;
			// do not want to grid snap this point - we only care which side of the line it is.  If we snap it it will tend to be on the line giving no shape at all
			return base.SnapNext(requested);
		}

		public override PointF Centre => base.CalculateCentreFromPoints();

		public override AllowedActions Allows => base.Allows & ~AllowedActions.TransformLinearStretch;

		protected internal override bool AllowVerbWhenComplete(Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					return true;
				default:
					return false;
			}
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = base.GetBaseLinePrompts("Polygon", true);
			if (m_DefinedVertices > 1)
				list.Add(new Prompt(ShapeVerbs.Increment | ShapeVerbs.Decrement, "Polygon_IncDec", "Polygon_IncDec"));
			return list;
		}

		protected internal override string StatusInformation(bool ongoing)
		{
			if (ongoing)
				return base.StatusInformation(true);
			return Strings.Item("Info_Edge") + ": " + Measure.FormatLength(Geometry.DistanceBetween(Vertices[0], Vertices[1])) + "  " + Strings.Item("Info_Sides") + ": " + m_Sides;
		}

		#endregion

		#region Verbs and GrabSpots

		public override VerbResult Float(ClickPosition position)
		{
			if (m_DefinedVertices < 2)
				return base.Float(position);
			// If pt.Equals(m_aVertices(2)) Then Return RectangleF.Empty ' polygon is unchanged - no need to do the further calculations
			// CANT DO THAT DUE TO ChangeSizeVerb
			Vertices[2] = position.Snapped;
			m_Bounds = RectangleF.Empty; // invalidate the bounds
			int direction = base.TurnDirection();
			if (direction == 0)
			{
				m_Acceptable = false;
				for (int index = 2; index <= m_Sides - 1; index++)
				{
					Vertices[index] = Vertices[1];
				}
			}
			else
			{
				PlacePoints(direction);
				m_Acceptable = true;
			}
			return VerbResult.Continuing;
		}

		private void PlacePoints(int direction)
		{
			// generate the extra sides by rotating the base vector repeatedly
			float angle = Geometry.ANGLE360 / m_Sides * direction; // the amount to rotate the vector between each side
			SizeF vector = BaseVector();
			PointF pt = Vertices[1]; // set it to the previous point
			for (int index = 2; index <= m_Sides - 1; index++)
			{
				vector = vector.RotateBy(angle);
				pt = PointF.Add(pt, vector);
				Vertices[index] = pt;
			}
			DiscardPath();
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddStandardRotationGrabSpot(list);
			// the shape can be expanded at each vertex; vertices cannot be moved independently
			PointF centre = Centre; // because Centre function may not be all that fast
			for (int index = 0; index <= Vertices.Count - 1; index++)
			{
				list.Add(new GrabSpot(this, GrabTypes.Radius, Vertices[index], index) { Focus = centre });
			}
			return list;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.Radius)
			{
				PointF centre = Centre; // because Centre function may not be all that fast
				if (centre.ApproxEqual(move.Current.Snapped))
					return;
				// update the radius, changing all points
				float radius = Geometry.DistanceBetween(centre, move.Current.Snapped);
				for (int vertex = 0; vertex <= Vertices.Count - 1; vertex++)
				{
					Vertices[vertex] = centre + centre.VectorTo(Vertices[vertex]).ChangeLength(radius);
				}
				DiscardPath();
				m_Bounds = RectangleF.Empty;
			}
			else
				base.DoGrabMove(move);
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType == GrabTypes.SingleVertex)
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
			else
				base.DoGrabAngleSnap(move);
		}

		protected internal override VerbResult OtherVerb(ClickPosition position, Functions.Codes code)
		{
			switch (code)
			{
				case Functions.Codes.Increment:
				case Functions.Codes.Decrement:
					int delta = code == Functions.Codes.Increment ? 1 : -1;
					return ChangeSides(delta);
				default:
					return VerbResult.Rejected;
			}
		}

		public static void ChangeDefaultSides(int delta)
		{
			// called by GUI in response to INC/DEC when no shape is ongoing, but tool is selected
			int newSides = CURRENTSIDES + delta;
			if (newSides < MINIMUMSIDES || newSides > MAXIMUMSIDES)
				return;
			CURRENTSIDES = newSides;
			CurrentSidesChanged?.Invoke();
		}

		private VerbResult ChangeSides(int delta)
		{
			// shared between verb during drawing and key later...
			int newSides = m_Sides + delta;
			if (newSides < MINIMUMSIDES || newSides > MAXIMUMSIDES)
				return VerbResult.Rejected;
			m_Sides = newSides;
			CURRENTSIDES = m_Sides;
			CurrentSidesChanged?.Invoke();
			if (m_DefinedVertices > 1 && m_Acceptable) // if Baseline is fixed
			{
				// otherwise we do not have a complete set of postulated vertices yet
				SetLength(m_Sides);
				m_Bounds = RectangleF.Empty;
				int direction = base.TurnDirection(); // must be <>0 if m_bolAcceptable
				PlacePoints(direction);
				m_DefinedVertices = m_Sides;
				return VerbResult.Continuing;
			}
			return VerbResult.Continuing;
		}

		#endregion

		#region BinaryData
		// there is no need to override Save, but we do need to update m_Sides after loading
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Sides = Vertices.Count;
			m_Acceptable = true;
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Polygon polygon = (Polygon)other;
			m_Sides = polygon.m_Sides;
			m_Acceptable = polygon.m_Acceptable;
		}

		#endregion

		public override bool Tidy(SnapModes mode, Page page)
		{
			bool changed = false;
			int direction = base.TurnDirection();
			if (mode == SnapModes.Shape)
			{
				// Not used the standard TidyVertices because this would only adjust the first two points.  This is also a problem with some triangles and rectangle/squares
				// but it is not so noticeable is the point which are ignored are usually outnumbered by the active points
				// Instead we will use the full shape snapping functionality in the Move transformation
				TransformMove transform = new TransformMove(0, 0);
				if (transform.Snap(this.GetPointsWhichSnapWhenMoving(), 1, page, this, Centre, Centre))
				{
					base.ApplyTransformation(transform);
					changed = true;
				}
			}
			else
			{
				changed = base.TidyVertices(mode, page, 1);
				if (changed)
					PlacePoints(direction);
			}
			return changed;
		}

		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();

	}

	/// <summary>this is basically just a closed (and filled) PolyLine </summary>
	public class IrregularPolygon : Sequential
	{

		public IrregularPolygon()
		{ }

		public IrregularPolygon(IEnumerable<PointF> points)
		{
			Vertices = points.ToList();
			m_DefinedVertices = Vertices.Count();
			LineStyle.SetDefaults(); // otherwise error reports cannot be loaded
			FillStyle.SetDefaults();
			FillStyle.Colour = Color.Transparent;
		}

		public override Shapes ShapeCode => Shapes.IrregularPolygon;

		protected override bool UseBaseline() => false;

		internal override List<Prompt> GetPrompts() => GetPolyPointPrompts("IrregularPolygon");

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 3)
				return VerbResult.Rejected;
			SetLength(m_DefinedVertices);
			DiscardPath();
			return VerbResult.Completed;
		}

		internal override LabelModes LabelMode => LabelModes.NotSupported;

		public override bool Tidy(SnapModes mode, Page page) => base.TidyVertices(mode, page, Vertices.Count - 1);

		protected internal override string StatusInformation(bool ongoing)
		{
			return Strings.Item("Info_Sides") + ": " + m_DefinedVertices + "  " + base.StatusInformation(ongoing);
			// base does perimeter
		}

		protected internal override bool VertexVerbApplicable(Functions.Codes code, Target target)
		{
			switch (code)
			{
				case Functions.Codes.AddVertex:
					return true;
				case Functions.Codes.RemoveVertex:
					return m_DefinedVertices > 2;
			}
			return base.VertexVerbApplicable(code, target);
		}

		#region Coordinates: Target, GrabSpots

		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();

		public override PointF Centre => PointF.Empty;

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddGrabSpotsForAllVertices(list, new Prompt(ShapeVerbs.Choose, "MoveVertex_Independent", "MoveVertex"));
			base.AddStandardRotationGrabSpot(list);
			return list;
		}

		internal override List<Target> GenerateTargets(UserSocket floating) => base.GenerateTargetsDefault(floating, Closed());

		protected internal override PointF DoSnapAngle(PointF newPoint)
		{
			if (m_DefinedVertices == 1)
				return base.DoSnapAngle(newPoint);
			return Geometry.AngleSnapFromTwoPoints(newPoint, Vertices[m_DefinedVertices - 1], Vertices[0], true);
		}

		#endregion

		// Load/Save/CopyFrom in base class are sufficient
	}

	public class PolyLine : Lined
	{

		public PolyLine()
		{ }

		public PolyLine(IEnumerable<PointF> points)
		{
			Vertices = points.ToList();
			m_DefinedVertices = Vertices.Count();
			LineStyle.SetDefaults(); 
			Status = StatusValues.Complete;
		}

		#region Information

		public override Shapes ShapeCode => Shapes.PolyLine;

		internal override List<Prompt> GetPrompts() => GetPolyPointPrompts("PolyLine");

		internal override LabelModes LabelMode => LabelModes.NotSupported;

		public override AllowedActions Allows => base.Allows | AllowedActions.Tidy;

		protected internal override bool Closed() => false;

		#endregion

		#region Drawing

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			base.InternalDrawFromPath(gr, resources);
			base.DrawArrowheads(resources);
		}

		#endregion

		#region Verbs

		public override VerbResult CompleteRetrospective()
		{
			if (m_DefinedVertices < 2)
				return VerbResult.Rejected;
			// although in general this function might be called when the shape is already complete for this shape it should not (because Choose never returns completed)
			Debug.Assert(Vertices.Count == m_DefinedVertices + 1);
			Vertices.RemoveAt(Vertices.Count - 1); // remove the postulated one off the end
			return VerbResult.Completed;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			// unless overridden in a subclass this will remove one DEFINED vertex
			// and trim the vertex array to contain just the defined ones plus one floating
			// the vertex array may currently contain any number of other postulated ones
			if (m_DefinedVertices <= 1)
				return VerbResult.Destroyed;
			DiscardVertex();
			return VerbResult.Continuing;
		}

		public override VerbResult Complete(ClickPosition position)
		{
			Choose(position);
			return CompleteRetrospective();
		}

		public override VerbResult Choose(ClickPosition position)
		{
			PointF point = position.Snapped;
			if (point.ApproxEqual(LastDefined))
				return VerbResult.Rejected;
			Vertices[Vertices.Count - 1] = point;
			FixVertex();
			return VerbResult.Continuing;
		}

		protected internal override bool VertexVerbApplicable(Functions.Codes code, Target target)
		{
			switch (code)
			{
				case Functions.Codes.AddVertex: return true;
				case Functions.Codes.RemoveVertex: return m_DefinedVertices > 2;
			}
			return base.VertexVerbApplicable(code, target);
		}

		#endregion

		#region Coordinates

		protected override RectangleF CalculateBounds() => base.BoundsOfVertices();

		protected override void CreatePath()
		{
			m_Path = GetLinearPath(Vertices, false);
		}

		internal override List<Target> GenerateTargets(UserSocket floating) => base.GenerateTargetsDefault(floating, false);

		public override PointF Centre => PointF.Empty;

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			base.AddGrabSpotsForAllVertices(list);
			return list;
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			return base.TidyVertices(mode, page, Vertices.Count - 1);
		}

		#endregion

	}
}
