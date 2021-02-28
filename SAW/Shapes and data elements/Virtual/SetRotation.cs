using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAW
{
	public class SetRotation : Shape
	{
		// dummy shape used when the user is setting the origin for reported coordinates

		private PointF m_Selected = PointF.Empty;

		#region Information

		public override Shapes ShapeCode => Shapes.SetRotation;

		public PointF SelectedPoint => m_Selected;

		public override GeneralFlags Flags => base.Flags | GeneralFlags.NotWithinContainer | GeneralFlags.AlwaysResetToolAfterCreation;

		public override SnapModes SnapNext(SnapModes requested) =>  SnapModes.Shape;

		#endregion

		#region Verbs
		// this is automatically started by the verb, therefore floating does most of the work

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			m_Selected = position.Snapped;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(EditableView.ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			m_Selected = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(EditableView.ClickPosition position) => Complete(position);

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			m_Selected = position.Snapped;
			m_Bounds = RectangleF.Empty;
			return VerbResult.Custom;
		}

		public override VerbResult CompleteRetrospective() => VerbResult.Custom;

		#endregion

		protected override RectangleF CalculateBounds() => Geometry.RectangleFromPoint(m_Selected, DRAWSIZE);

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => false;

		private const float DRAWSIZE = 8;

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			resources.MainPen = resources.Graphics.CreateStroke(Color.Gray, Geometry.THINLINE);
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.MainPen != null)
			{
				gr.DrawLine(m_Selected.X - DRAWSIZE, m_Selected.Y + DRAWSIZE, m_Selected.X + DRAWSIZE, m_Selected.Y - DRAWSIZE, resources.MainPen);
				gr.DrawLine(m_Selected.X - DRAWSIZE, m_Selected.Y - DRAWSIZE, m_Selected.X + DRAWSIZE, m_Selected.Y + DRAWSIZE, resources.MainPen);
			}
		}


	}
}
