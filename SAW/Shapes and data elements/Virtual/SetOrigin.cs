using System.Collections.Generic;
using System.Drawing;

namespace SAW.Shapes
{
	internal class SetOrigin : Shape
	{
		// dummy shape used when the user is setting the origin for reported coordinates

		private PointF m_Origin = PointF.Empty;

		#region Information

		public override Shapes ShapeCode => Shapes.SetOrigin;

		public PointF Origin => m_Origin;

		internal override List<Prompt> GetPrompts()
		{
			// if we are being asked, then the first vertex must have been placed
			List<Prompt> list = new List<Prompt>();
			list.Add(new Prompt(ShapeVerbs.Choose, "SetOrigin", "SetOrigin"));
			list.Add(new Prompt(ShapeVerbs.Cancel, "SetOrigin_Cancel", ""));
			return list;
		}

		public override GeneralFlags Flags => base.Flags | GeneralFlags.NotWithinContainer | GeneralFlags.AlwaysResetToolAfterCreation;

		protected internal override SnapModes SnapNext(SnapModes requested) => requested == SnapModes.Angle ? SnapModes.Off : base.SnapNext(requested);

		#endregion

		#region Verbs
		// this is automatically started by the verb, therefore floating does most of the work

		public override VerbResult Start(ClickPosition position)
		{
			m_Origin = position.Snapped;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Float(ClickPosition position)
		{
			m_Origin = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(ClickPosition position) => Complete(position);

		public override VerbResult Complete(ClickPosition position)
		{
			m_Origin = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Custom;
		}

		public override VerbResult CompleteRetrospective() => VerbResult.Custom;

		#endregion

		protected override RectangleF CalculateBounds() => Geometry.RectangleFromPoint(m_Origin, DRAWSIZE);

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => false;

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
