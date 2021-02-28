using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAW
{
	public class FloatingLabel : TextLine
	{

		/// <summary>Distance from linked shape centre to Vertex 0</summary>
		private SizeF m_Offset;

		#region Info

		public override Shapes ShapeCode => Shapes.FloatingLabel;

		public override GeneralFlags Flags => base.Flags | GeneralFlags.AlwaysResetToolAfterCreation | GeneralFlags.NoIntersections;

		#endregion

		#region Verbs

		private const float PREFERSPACE = 200;
		private const float MINSPACE = 100;

		/// <summary>Not stored in data; only used when first being created</summary>
		bool m_AddingRight ;
		public override VerbResult Start(EditableView.ClickPosition position)
		{
			Shape hit = position.Page.HitTest(position.Exact, 1, Page.HitTestMode.StartSelectionFilled);
			if (hit == null)
			{
				MessageBox.Show(Strings.Item("FloatingLabel_NoShapeClicked"));
				return VerbResult.Rejected;
			}

			RectangleF hitBounds = hit.Bounds;
			RectangleF pageBounds = position.Page.Bounds;
			float spaceRight = pageBounds.Right - hitBounds.Right;
			float spaceLeft = hitBounds.Left - pageBounds.Left;

			m_AddingRight=true;
			float space = spaceRight;
			if (spaceRight < PREFERSPACE && spaceLeft > spaceRight)
			{
				m_AddingRight = false;
				space = spaceLeft;
			}
			float width = Math.Max(Math.Min(space, PREFERSPACE), MINSPACE);
			float X;
			if (m_AddingRight)
				X = Math.Min(hitBounds.Right, pageBounds.Right - width); // never goes right off page, Min will bring it left overlapping the shape if needed
			else
				X = Math.Max(hitBounds.Left - width, pageBounds.Left);

			Vertices[0] = new PointF(X, hitBounds.Centre().Y);
			Vertices[1] = new PointF(X + width, hitBounds.Centre().Y);

			m_BaseLineFixed = true;
			AddLink(hit);
			SetOffset();
			TextStyle.SetDefaults();
			Status = StatusValues.Complete;
			TextStyle.Alignment = StringAlignment.Near;
			m_Label = "";
			FormatText();
			CaretGain();

			return VerbResult.Continuing;
		}

		public override VerbResult Float(EditableView.ClickPosition position) => VerbResult.Continuing;

		// there's not actually much difference between cancelling and choosing
		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			if (!string.IsNullOrEmpty(m_Label))
				return Complete(position);
			return VerbResult.Destroyed;
		}

		public override VerbResult Choose(EditableView.ClickPosition position) => CompleteRetrospective();
		//{
		//	if (string.IsNullOrEmpty(m_Label))
		//		return VerbResult.Rejected;
		//	CaretLose();
		//	return VerbResult.Completed;
		//}

		public override VerbResult Complete(EditableView.ClickPosition position) => CompleteRetrospective();

		public override VerbResult CompleteRetrospective()
		{
			if (string.IsNullOrEmpty(m_Label))
				return VerbResult.Rejected;
			CaretLose();
			if (!m_AddingRight)
			{ // want to move it to crudely right align
				var widthUsed = CalculateTextBounds().Width;
				Vertices[0] = new PointF(Vertices[1].X - widthUsed - 1, Vertices[1].Y);
				m_Bounds =RectangleF.Empty;
				ClearTextCache();
			}
			return VerbResult.Completed;
		}

		#endregion

		#region Datum

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_Offset);
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			m_Offset = reader.ReadSizeF();
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			FloatingLabel floating = (FloatingLabel)other;
			m_Offset = floating.m_Offset;
		}

		#endregion

		#region Coordinates

		private void SetOffset()
		{
			if (m_Links?.Count > 0)
				m_Offset = Vertices[0].Subtract(m_Links[0].Shape.Centre);
		}

		protected override void OnLinkedChanged()
		{
			if (this.Status == StatusValues.Moved)
				return; // avoids recalculating twice if both shapes have changed - copied from Connector, but not sure if applicable?

			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded | ChangeAffects.Bounds);
			// record vector between the 2 points, so second can also be updated: (there is no need to keep this permanently in the data)
			SizeF line = Vertices[1].Subtract(Vertices[0]);
			Vertices[0] = m_Links[0].Shape.Centre + m_Offset;
			Vertices[1] = Vertices[0] + line;
			m_Bounds = CalculateBounds();
			Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
			ClearTextCache(false); // doesn't clear formatting in text space, only the xforms from page to text space
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			SetOffset();
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			//RectangleF textBounds = CalculateTextBounds();
			//PointF midLeft = Geometry.MidPoint(textBounds.Location, textBounds.BottomLeft());
			//PointF midRight = Geometry.MidPoint(textBounds.TopRight(), textBounds.BottomRight());
			//this.SetTextTransforms();
			//var points = new PointF[] { midLeft, midRight };
			//m_TextToPageTransform.TransformPoints(points);
			//list.Add(new GrabSpot(this, GrabTypes.Radius, points[0], points[1]));
			//list.Add(new GrabSpot(this, GrabTypes.Radius, points[1], points[0]));

			list.Add(new GrabSpot(this, GrabTypes.Radius, Vertices[0], Vertices[1]));
			list.Add(new GrabSpot(this, GrabTypes.Radius, Vertices[1], Vertices[0]));
			base.AddStandardRotationGrabSpot(list);
			return list;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			base.DoGrabMove(move);
		}

		#endregion


	}
}
