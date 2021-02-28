using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;

namespace SAW
{
	public class UserSocket : Shape
	{

		private PointF m_Centre;
		private SizeF m_Exit; // or Empty if don't care; if it is set then the length should be DrawSize
		[Flags()]
		public enum OptionsEnum
		{
			Connector = 1,
			ShapeSnap = 2,
			Male = 4, // one or both genders should be set - indicates this acts as that gender
			Female = 8,
			Explicit = 16, // An explicit socket, i.e. specified as part of a group, or from jigsaw.  Not set for points automatically generated for shape snapping.  Is set by Load
			_All = 31,
			_Gender = 12
		}
		public OptionsEnum Options = OptionsEnum._All;
		public string Classification = "";

		/// <summary>With shape snapping on, during move all snappable bits of the moving shape are expressed as sockets
		/// this creates one for a standard vertex, which contains just the position with no other constraints</summary>
		public static UserSocket CreateForPoint(PointF pt)
		{
			return new UserSocket
			{
				m_Centre = pt,
				m_Exit = SizeF.Empty,
				Options = OptionsEnum.Male | OptionsEnum.Female | OptionsEnum.ShapeSnap
			};
		}

		#region Information

		public override Shapes ShapeCode => Shapes.UserSocket;
		public bool Grouped => Parent is Group; // returns true once this is inside a group
		public override LabelModes LabelMode => LabelModes.NotSupported;
		public override AllowedActions Allows => (base.Allows | AllowedActions.Tidy) & ~(AllowedActions.Merge | AllowedActions.Typing | AllowedActions.MoveToPixels);

		public override string StatusInformation(bool ongoing)
		{
			if (ongoing)
				return ""; // shouldn't be ongoing
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			if (!m_Exit.IsEmpty)
			{
				output.Append(Strings.Item("Info_Direction"));
				output.Append(Measure.FormatAngle(m_Exit.VectorAngle())).Append(" ");
			}
			if ((Options & (OptionsEnum.Male | OptionsEnum.Female)) == OptionsEnum.Female)
				output.Append(Strings.Item("Info_Female")).Append(" ");
			else if ((Options & (OptionsEnum.Male | OptionsEnum.Female)) == OptionsEnum.Male)
				output.Append(Strings.Item("Info_Male")).Append(" ");
			if (!string.IsNullOrEmpty(Classification))
				output.Append(Strings.Item("Sockets_Classification")).Append(" ").Append(Classification);
			return output.ToString();
		}

		#endregion

		#region Verbs - mostly irrelevant

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			m_Centre = position.Snapped;
			m_Exit = SizeF.Empty;
			return VerbResult.Completed;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult Choose(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult Complete(EditableView.ClickPosition position) => VerbResult.Unexpected;
		public override VerbResult CompleteRetrospective() => VerbResult.Unexpected;
		public override VerbResult Float(EditableView.ClickPosition position) => VerbResult.Unexpected;

		#endregion

		#region Coordinates
		private const float DrawSize = 3; // length of one of the shorter edges when drawing with exit line; half the size of the side when drawing without

		protected override RectangleF CalculateBounds()
		{
			RectangleF bounds = Geometry.RectangleFromPoint(m_Centre, 0);
			if (!Grouped)
			{
				if (m_Exit.IsEmpty)
					bounds.Inflate(DrawSize, DrawSize);
				else
				{
					// need to consider all six vertices used when drawing, except the one at m_Centre
					SizeF transverse = m_Exit.Perpendicular(1); // clockwise from the exit vector
					Geometry.Extend(ref bounds, m_Centre + m_Exit + m_Exit); // the exit vector line
																					  // And now the points around the [ shape working clockwise:
					Geometry.Extend(ref bounds, m_Centre + transverse);
					Geometry.Extend(ref bounds, m_Centre + transverse - m_Exit);
					Geometry.Extend(ref bounds, m_Centre - transverse - m_Exit);
					Geometry.Extend(ref bounds, m_Centre - transverse);
				}
			}
			return bounds;
		}

		public override RectangleF MinimalBounds => Geometry.RectangleFromPoint(m_Centre, 0);

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => !Grouped;
		// don't think this is actually called when grouped anyway

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			if (Grouped)
				return null; //!
			List<GrabSpot> list = new List<GrabSpot>();
			if (!m_Exit.IsEmpty) 
				base.AddStandardRotationGrabSpot(list);
			return list;
		}

		public override PointF Centre => m_Centre;

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			if (!m_Exit.IsEmpty)
				transformation.TransformVector(ref m_Exit);
			transformation.TransformPoint(ref m_Centre);
		}

		public SizeF ExitVector
		{
			get { return m_Exit; }
			set
			{
				if (value.IsEmpty)
					m_Exit = value;
				else
					m_Exit = value.ChangeLength(DrawSize);
				m_Bounds = RectangleF.Empty;
			}
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			return new List<UserSocket> {this};
		}

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			return new List<Target> {new Target(this, m_Centre, Target.Types.Centre, floating)};
		}

		internal override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			if ((change & EnvironmentChanges.ParentReassigned) > 0)
				m_Bounds = RectangleF.Empty; // bounds collapses if parent is group
		}

		[Obsolete()]
		public static PointF AverageCentre(List<UserSocket> list)
		{
			// returns the average centre point of the given sockets (used when doing auto rotation to decide where to rotate)
			float X = 0;
			float Y = 0;
			int count = 0;
			foreach (UserSocket socket in list)
			{
				X += socket.Centre.X;
				Y += socket.Centre.Y;
				count += 1;
			}
			return new PointF(X / count, Y / count);
		}

		public override bool Tidy(SnapModes mode, Page page)
		{
			PointF newPoint = m_Centre;
			switch (mode)
			{
				case SnapModes.Grid:
					newPoint = page.Paper.SnapPoint2(newPoint);
					break;
				case SnapModes.Shape:
					newPoint = Lined.TidyPointToShapes(newPoint, page, this);
					break;
				case SnapModes.Angle:
					SizeF newVector = Geometry.AngleSnapVector(m_Exit);
					if (newVector.Equals(m_Exit))
						return false;
					m_Exit = newVector;
					return true;
				default:
					Debug.Fail("Bad Tidy mode");
					break;
			}
			if (newPoint == m_Centre)
				return false;
			m_Centre = newPoint;
			return true;
		}

		#endregion

		#region graphics

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			if (Grouped)
				return; // does not draw if grouped
			// just overridden in order to avoid unnecessary resource allocation
			base.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
		}

		internal override void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			if (Grouped)
				return; // does not draw if grouped
			base.DrawHighlight(gr, scale, coordScale, singleElement);
		}

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			resources.MainPen = resources.Graphics.CreateStroke(Color.FromArgb(resources.EdgeAlpha, Color.Purple), Geometry.THINLINE * 3);
		}

		protected override void PrepareHighlight(DrawResources resources)
		{
			base.PrepareHighlight(resources);
			resources.MainPen = resources.Graphics.CreateStroke(CurrentHighlightColour, Geometry.THINLINE * 3 + resources.HIGHLIGHTEXTRAWIDTH);
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (Grouped)
				return; // does not draw if grouped
			if (resources.MainPen == null)
				return;
			if (m_Exit.IsEmpty)
			{
				using (var br = resources.Graphics.CreateFill(Color.Purple))
				{
					gr.Ellipse(Geometry.RectangleFromPoint(m_Centre, Geometry.THINLINE * 2), null, br);
					gr.Rectangle(Geometry.RectangleFromPoint(m_Centre, DrawSize), resources.MainPen);
				}
			}
			else
			{
				SizeF szTransverse = m_Exit.Perpendicular(1); // clockwise from the exit vector
																// exit line, or a triangular bit if it has gender
				if ((Options & (OptionsEnum.Male | OptionsEnum.Female)) == OptionsEnum.Male)
				{
					SizeF halfExit = m_Exit.MultiplyBy(0.5f);
					gr.DrawLine(m_Centre + szTransverse, m_Centre + halfExit, resources.MainPen);
					gr.DrawLine(m_Centre - szTransverse, m_Centre + halfExit, resources.MainPen);
				}
				else if ((Options & (OptionsEnum.Male | OptionsEnum.Female)) == OptionsEnum.Female)
				{
					SizeF halfExit = m_Exit.MultiplyBy(0.5f);
					gr.DrawLine(m_Centre + szTransverse, m_Centre - halfExit, resources.MainPen);
					gr.DrawLine(m_Centre - szTransverse, m_Centre - halfExit, resources.MainPen);
				}
				else
				{
					gr.DrawLine(m_Centre, m_Centre + m_Exit + m_Exit, resources.MainPen);
				}
				// and then the half square background
				gr.DrawLines(new PointF[] { m_Centre + szTransverse, m_Centre + szTransverse - m_Exit, m_Centre - szTransverse - m_Exit, m_Centre - szTransverse }, resources.MainPen);
			}
		}

		#endregion

		#region Data
		public override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Centre = reader.ReadPointF();
			m_Exit = reader.ReadSizeF();
			Options = (OptionsEnum)reader.ReadInt32();
			if (reader.Version < 75)
				Options = Options | OptionsEnum.Explicit;
			Classification = reader.ReadString();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Centre);
			writer.Write(m_Exit);
			writer.Write((int)Options);
			writer.Write(Classification);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			UserSocket socket = (UserSocket)other;
			m_Centre = socket.m_Centre;
			m_Exit = socket.m_Exit;
			Options = socket.Options;
			Classification = socket.Classification;
			m_Bounds = RectangleF.Empty;
		}
		#endregion

		#region Options and compatibility
		public bool OptionFlag(OptionsEnum flag) => (Options & flag) > 0;

		public void SetOptionFlag(bool value, OptionsEnum flag)
		{
			if (value)
				Options = Options | flag;
			else
				Options = Options & ~flag;
		}

		public bool GenderCompatible(UserSocket other)
		{
			if ((Options & OptionsEnum._Gender) == OptionsEnum._Gender)
				return true; // this shape doesn't care
			if ((other.Options & OptionsEnum._Gender) == OptionsEnum._Gender)
				return true; // other shape doesn't care
			if ((Options & OptionsEnum._Gender) == (other.Options & OptionsEnum._Gender))
				return false; // genders same- fails
			return true;
		}

		public bool ClassCompatible(UserSocket other)
		{
			return string.IsNullOrEmpty(Classification) || string.IsNullOrEmpty(other.Classification) || Classification == other.Classification;
		}

		public float RotationRequired(UserSocket other)
		{
			// rotation required of this socket to make it fit other socket. 0 means they match or one or both don't care about angle
			if (m_Exit.IsEmpty)
				return 0;
			if (other.ExitVector.IsEmpty)
				return 0;
			float angle = other.ExitVector.VectorAngle() - m_Exit.VectorAngle() - Geometry.ANGLE180;
			// angle180 on end, because they are expected to be OPPOSITE
			return Geometry.NormaliseAnglePlusMinus180(angle);
		}

		[Obsolete()]
		public bool AngleCompatibile(UserSocket other)
		{
			// vectors must be opposite (apart from rounding error), if both defined
			if (m_Exit.IsEmpty)
				return true;
			if (other.ExitVector.IsEmpty)
				return true;
			float angle = Geometry.AbsoluteAngularDifference(m_Exit.VectorAngle(), other.ExitVector.VectorAngle());
			if (angle > Geometry.ANGLE180 - Geometry.NEGLIGIBLEANGLESMALL)
				return true;
			return false;
		}
		#endregion

	}
}
