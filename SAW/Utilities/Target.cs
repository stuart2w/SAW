using System;
using System.Drawing;
using System.Diagnostics;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Used initially for snap to shape, this is also used to indicate parts of a shape that have been selected for manual coordinate editing</summary>
	public class Target : IComparable<Target>
	{

		public const int ACTIVEMAXIMUMPHASE = 9; // the graphics of the active one is continually changed; a parameter is provided running from 0 to this value
		public const int ACTIVEANIMATIONINTERVAL = 75; // number of milliseconds between phases
		public const int MAXIMUMDRAWDISTANCE = 10; // targets are not permitted to draw further than this distance from Position
												   // this is to enable refreshing just the active one
		public const float LINEDRAWLENGTH = 5; // approximate length to draw line targets (can vary somewhat, e.g ellipse and Bezier just use a mostly arbitrary length)
		public const float IGNOREOVERLAPPINGTHRESHOLD = 2; // targets within this distance of each other are sometimes removed
														   // (in particular anything this close to the active one is removed to make it clearer; also Lined ignores the line if it is this close to vertex)
		public const float PHASESTEP = 0.3f; // distance to move in each phase

		public enum Priorities // the shape can force some of the returned targets to have priority
		{
			Low = 2,
			Standard = 5, // these numbers are subtracted from distance, so the numbers matter
			Vertex = 8,
			Intersection = 10
		}
		public enum Types
		{
			Line, // on a line, whether straight or curved
			Centre, // the centre of the shape (or some other important focus - does not necessarily need to be the absolute little centre, but usually will be)
			Vertex, // a joint of two lines within this shape
			Intersection,
			PageEdgeV, // works rather like line, but Shape will not be defined
			PageEdgeH, // we need to distinguish horizontal and vertical in order to know which direction to draw it
			/// <summary>Wraps a GrabSpot - must be derived class (which stores the GrabSpot) </summary>
			GrabSpot
		}
		public readonly Shape Shape;
		/// <summary>The exact coordinates of this snapping point</summary>
		public readonly PointF Position;
		public readonly Priorities Priority;
		public readonly Types Type;
		/// <summary>Distance between this target and the mouse when recorded.  This value is stored in order to be able to quickly sort of the targets into distance order</summary>
		private readonly float Distance;

		// the following values can be used by the shape
		public int ShapeIndex;
		/// <summary>in a Bezier curve this is the T-value</summary>
		public float ShapeParameter;
		/// <summary>only used Socket-Socket;  only used when moving a group - causes this position to be invalid even if other sockets would match on source shape.  If true can be ignored by most logic</summary>
		public bool Mismatch = false;
		/// <summary>Only Socket-Socket; and only set if Mismatch = false.  This is not 0 it means that some rotation was assumed to make the target work</summary>
		public float RotationRequired = 0;
		/// <summary>Only if Type = Intersection.  This is Angle90-Intersection.Certainty, i.e. always a positive number.  Note opposite sense to the way it is stored in the Intersection</summary>
		public float Uncertainty = 0;

		// a more convenient constructor for shapes when generating targets:
		// might also want to store the source socket eventually
		internal Target(Shape shape, PointF position, Types type, UserSocket source, Priorities priority = Priorities.Standard, float intersectionCertainty = Geometry.PI / 2, int shapeIndex = -1)
		{
			// ObjSource if the moving item for which targets have been requested
			// -1 is used as the default shape index to make it obvious when it was not defined (0 is usually valid)
			Shape = shape;
			Position = position;
			Priority = priority;
			Type = type;
			Debug.Assert(priority == Priorities.Intersection || intersectionCertainty == Geometry.ANGLE90); // certainty has no effect unless it is an intersection
			if (type == Types.Intersection)
				Uncertainty = Geometry.ANGLE90 - intersectionCertainty;
			if (source != null)
				Distance = Geometry.DistanceBetween(source.Centre, position); // otherwise Distance left as 0.  Not used for real snapping targets, but can be used for some dummy ones
			ShapeIndex = shapeIndex;
			if (type == Types.GrabSpot && !(this is ForGrabSpot))
				throw new ArgumentException(nameof(type));
		}

		public int CompareTo(Target other)
		{
			// means that sorting the list of targets will return them in order of ascending distance
			return AdjustedDistance.CompareTo(other.AdjustedDistance);
		}

		public float AdjustedDistance => Distance - ((float)Priority / 4 + Uncertainty) * GUIUtilities.MillimetreSize;
		public float ActualDistance => Distance;

		/// <summary>if the picture is displayed at more than 100% then the drawing code might draw larger than usual </summary>
		public RectangleF RefreshBounds(float scale)
		{
			float distance = 10;
			if (scale < 1)
				distance /= (float)Math.Sqrt(scale);
			distance *= GUIUtilities.MillimetreSize;
			return new RectangleF(Position.X - distance, Position.Y - distance, distance * 2, distance * 2);
		}

		#region Graphics
		internal static Color TARGETCOLOUR = Color.BlueViolet; // but is actually drawn with a varying amount of alpha
		internal static Color FAILEDTARGETCOLOUR = Color.Red;
		private const float DISPLAYDIMENSION = 2;

		public void Draw(Graphics gr, int activePhase, float scale)
		{
			// activePhase is < 0 if not active
			int alpha = 140;
			if (Type == Types.Line)
			{
				// you need to be somewhat closer to align to display the target (because these move around they are more distracting, don't want too many line targets)
				alpha -= (int)(Distance * 8 / Math.Sqrt(scale));
			}
			else
				alpha -= (int)(Distance * 3.3 / Math.Sqrt(scale));
			if (alpha <= 0)
				return; // too far away to bother with
			if (Mismatch)
			{
				// no matter what type it just draws a red cross which pulses (rather than rotating which a valid intersection does)
				using (Pen pn = new Pen(FAILEDTARGETCOLOUR, 0.7f / scale))
				{
					float dimension = (float)((DISPLAYDIMENSION - (ACTIVEMAXIMUMPHASE - activePhase) * 0.1) * 1.4 / Math.Sqrt(scale)); // we compromise on scaling
					gr.DrawLine(pn, Position.X - dimension, Position.Y - dimension, Position.X + dimension, Position.Y + dimension);
					gr.DrawLine(pn, Position.X - dimension, Position.Y + dimension, Position.X + dimension, Position.Y - dimension);
				}
			}
			else
			{
				Color col = Color.FromArgb(alpha, TARGETCOLOUR);
				using (Pen pn = new Pen(col, 0.7f / scale))
				{
					switch (Type)
					{
						case Types.Line:
						case Types.PageEdgeH:
						case Types.PageEdgeV:
							// pen width is not scaled - we really need to keep this in proportion to the line we are drawing over
							pn.Width = 1.5F;
							if (activePhase >= 0)
								pn.Width = 1 + (ACTIVEMAXIMUMPHASE - activePhase) / 3.5f;
							pn.Width *= GUIUtilities.MillimetreSize / (float)Math.Sqrt(scale);
							float length = LINEDRAWLENGTH * GUIUtilities.MillimetreSize;
							switch (Type)
							{
								case Types.PageEdgeH:
									gr.DrawLine(pn, Position.X - length, Position.Y, Position.X + length, Position.Y);
									break;
								case Types.PageEdgeV:
									gr.DrawLine(pn, Position.X, Position.Y - length, Position.X, Position.Y + length);
									break;
								case Types.Line:
									if (Shape is Lined)
										pn.Width += ((Lined)Shape).LineStyle.Width;
									Shape.DrawLineTarget(this, gr, pn, activePhase);
									break;
							}
							break;
						case Types.Centre:
							float dimension1 = DISPLAYDIMENSION * GUIUtilities.MillimetreSize / (float)Math.Sqrt(scale); // we compromise on scaling
							if (activePhase >= 0)
							{
								dimension1 = (DISPLAYDIMENSION / 2 + (ACTIVEMAXIMUMPHASE - activePhase)) * PHASESTEP / (float)Math.Sqrt(scale) * GUIUtilities.MillimetreSize;
								Debug.Assert(dimension1 < MAXIMUMDRAWDISTANCE / Math.Sqrt(scale) - 1);
							}
							gr.DrawEllipse(pn, Position.X - dimension1, Position.Y - dimension1, dimension1 * 2, dimension1 * 2);
							break;
						case Types.Vertex:
							float dimension2 = DISPLAYDIMENSION * GUIUtilities.MillimetreSize / (float)Math.Sqrt(scale);
							if (activePhase >= 0)
								dimension2 = (float)((DISPLAYDIMENSION / 2 + (ACTIVEMAXIMUMPHASE - activePhase)) * PHASESTEP / Math.Sqrt(scale)) * GUIUtilities.MillimetreSize;
							gr.DrawRectangle(pn, Position.X - dimension2, Position.Y - dimension2, dimension2 * 2, dimension2 * 2);
							break;
						case Types.Intersection:
							float dimension3 = (float)(DISPLAYDIMENSION * 1.4 * GUIUtilities.MillimetreSize / Math.Sqrt(scale));
							// we compromise on scaling.
							// offset the points if it is active, making the cross sort of rotate
							float offset = dimension3 * 2 * activePhase / (ACTIVEMAXIMUMPHASE + 1);
							gr.DrawLine(pn, Position.X - dimension3, Position.Y - dimension3 + offset, Position.X + dimension3, Position.Y + dimension3 - offset);
							gr.DrawLine(pn, Position.X - dimension3 + offset, Position.Y + dimension3, Position.X + dimension3 - offset, Position.Y - dimension3);
							break;
						default:
							Debug.Fail("Unexpected Target type in Target.Draw");
							break;
					}
				}

			}
		}

		#endregion

		#region Distances
		public static float MaximumApplicableDistance(float scale) => MaximumInterestDistance * (float)Math.Sqrt(scale);

		/// <summary>any point outside this distance is of no interest</summary>
		public static float MaximumInterestDistance => (ActivationThreshold + (float)Priorities.Intersection) * GUIUtilities.MillimetreSize;

		/// <summary>this is compared to the target Distance - Priority.</summary>
		public static float ActivationThreshold => 3 * GUIUtilities.MillimetreSize;

		/// <summary>reduced amount when snapping entire shapes (because multiple priorities can be subtracted from this)</summary>
		public static float ActivationThresholdMoveShape => 0.5F;
		// Not sure that I gets users will use this for moving shapes much, but we do have an increased value for them anyway

		/// <summary>also imposes an absolute maximum distance when moving a shape, to prevent a massive number of points triggering threshold at large distance</summary>
		public static int ActivationMaximumDistanceMoveShape => 4;

		#endregion

		public override string ToString() => $"{Type} target in {Shape} at {Position} index = {ShapeIndex}";

		/// <summary>True if these refer to the same thing (can be different target objects).  Doesn't check coordinates match - only that they are targetting the same part of the same shape</summary>
		/// <remarks>Doesn't override Equals because that requires GetHashCode is replaced, which is all a pain</remarks>
		public bool Matches(Target other)
		{
			if (other == null)
				return false;
			return Shape == other.Shape
				   && Type == other.Type
				   && ShapeIndex == other.ShapeIndex
				   && ShapeParameter == other.ShapeParameter;
		}

		/// <summary>Wraps a GrabSpot.  Not used in snapping, only in part path selection</summary>
		public sealed class ForGrabSpot : Target
		{
			public readonly Shape.GrabSpot Grab;

			public ForGrabSpot(Shape.GrabSpot grab) : base(grab.Shape, grab.Position, Types.GrabSpot, null)
			{
				Grab = grab;
			}
		}

		public static Target FromGrabSpot(Shape.GrabSpot grab)
		{
			switch (grab.GrabType)
			{
				case Shape.GrabTypes.SingleVertex:
					return new Target(grab.Shape, grab.Position, Types.Vertex, null, Priorities.Standard, Geometry.PI / 2, grab.ShapeIndex);
				case Shape.GrabTypes.Bezier:
					return new Target(grab.Shape, grab.Position, Types.Line, null, Priorities.Standard, Geometry.PI / 2,
						grab.ShapeIndex - (grab.ShapeParameter == -1 ? 2 : 1)); // shapeParameter indicates whether bezier is first (+1) or second (-1) in segment
				default:
					return new ForGrabSpot(grab);
			}
		}
	}



}
