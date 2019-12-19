using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SAW
{
	public class SelectionBox : Shape
	{
		// this is the "rubber band" box which can be used to select multiple shapes
		// it is never added to the page, by Canonbie m_shpCurrent within pnlView

		private PointF m_Start;
		private PointF m_End;
		public bool MultiMode = false; // true if shift was pressed when starting, which doesn't deselct other shapes
		private List<Shape> m_OriginalSelection; // shapes selected at start (only if Shifted)

		#region Information
		public override Shapes ShapeCode => Shapes.SelectionBox;
		public override SnapModes SnapNext(SnapModes requested) => SnapModes.Off;
		public override GeneralFlags Flags => base.Flags | GeneralFlags.NotWithinContainer;
		public override AllowedActions Allows => AllowedActions.None; // not actually used?

		#endregion

		#region Verbs

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			if (!MultiMode)
				position.Page.DeselectAll();
			else
				position.Page.SelectOnly(m_OriginalSelection);
			return VerbResult.Destroyed;
		}

		// the selection is done continuously as the box is floated; therefore as soon as the user clicks Choose or Complete the box is removed
		public override VerbResult Choose(EditableView.ClickPosition position) => VerbResult.Destroyed;
		public override VerbResult Complete(EditableView.ClickPosition position) => VerbResult.Destroyed;
		public override VerbResult CompleteRetrospective() => VerbResult.Destroyed;

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			m_End = position.Snapped;
			m_Bounds = RectangleF.Empty;
			EnsureBounds();
			IEnumerable<Shape> colSelected = (from s in position.Page where  m_Bounds.Contains(s.MinimalBounds) select s).ToList();
			if (MultiMode)
				colSelected = colSelected.Union(m_OriginalSelection);
			position.Page.SelectOnly(colSelected.ToList());
			return VerbResult.Continuing;
		}

		public override VerbResult Start(EditableView.ClickPosition position)
		{
			m_Start = position.Snapped;
			m_End = m_Start;
			if (MultiMode)
			{
				m_OriginalSelection = new List<Shape>();
				m_OriginalSelection.AddRange(position.Page.SelectedShapes);
			}
			return VerbResult.Continuing;
		}

		#endregion

		protected override RectangleF CalculateBounds()
		{
			RectangleF rctBounds = new RectangleF(m_Start.X, m_Start.Y, 1, 1);
			Geometry.Extend(ref rctBounds, m_End);
			return rctBounds;
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Debug.Fail("there is no reason to be using SelectionBox.CopyFrom");
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			Debug.Fail("There is no reason to be using SelectionBox.HitTestDetailed");
			return false; // there is no reason to be testing for this
		}

		#region Graphics
		// ignores the PrepareXXX commands (we are not interested in the alpha values anyway and just creates the pen and brush within the function)
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			// highlight the background area in pale yellow
			using (var br = resources.Graphics.CreateFill(Color.FromArgb(30, 100, 200, 100)))
			{
				using (var pn = resources.Graphics.CreateStroke(Color.Red, 1))
				{
					pn.DashStyle = DashStyle.Dash;
					gr.Rectangle(Bounds, pn, br);
				}
			}

		}

		#endregion

	}
}
