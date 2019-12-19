using System.Collections.Generic;
using System.Drawing;

namespace SAW
{
	public class SetOrigin : Shape
	{
		// dummy shape used when the user is setting the origin for reported coordinates

		private PointF m_Origin = PointF.Empty;

		#region Information

		public override Shapes ShapeCode
		{ get { return Shapes.SetOrigin; } }

		public PointF Origin
		{ get { return m_Origin; } }

		public override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			List<Prompt> objList = new List<Prompt>();
			objList.Add(new Prompt(ShapeVerbs.Choose, "SetOrigin", "SetOrigin"));
			objList.Add(new Prompt(ShapeVerbs.Cancel, "SetOrigin_Cancel", ""));
			return objList;
		}

		public override GeneralFlags Flags
		{ get { return base.Flags | GeneralFlags.NotWithinContainer; } }

		public override SnapModes SnapNext(SnapModes requested)
		{
			if (requested == SnapModes.Angle)
				return SnapModes.Off;
			return base.SnapNext(requested);
		}
		#endregion

		#region Verbs
		// this is automatically started by the verb, therefore floating does most of the work

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			m_Origin = position.Snapped;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			return VerbResult.Destroyed;
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			m_Origin = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			return Complete(position);
		}

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			m_Origin = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Custom;
		}

		public override VerbResult CompleteRetrospective()
		{
			return VerbResult.Custom;
		}

		#endregion

		protected override RectangleF CalculateBounds()
		{
			return Geometry.RectangleFromPoint(m_Origin, DRAWSIZE);
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			return false;
		}

		private const float DRAWSIZE = 4;

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			resources.MainPen = resources.Graphics.CreateStroke(Color.Gray, Geometry.THINLINE);
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.MainPen != null)
			{
				gr.DrawLine(m_Origin.X - DRAWSIZE, m_Origin.Y, m_Origin.X + DRAWSIZE, m_Origin.Y, resources.MainPen);
				gr.DrawLine(m_Origin.X, m_Origin.Y - DRAWSIZE, m_Origin.X, m_Origin.Y + DRAWSIZE, resources.MainPen);
			}
		}


	}
}
