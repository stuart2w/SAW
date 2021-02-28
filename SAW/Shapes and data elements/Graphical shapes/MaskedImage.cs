using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace SAW
{
	public class MaskedImage : Shape, IShapeParent
	{
		public ImportedImage Image { get; private set; }

		public Lined MaskShape { get; private set; }

		public MaskedImage()
		{ }

		public MaskedImage(ImportedImage image, Lined mask)
		{
			Image = image;
			MaskShape = mask;
			MaskShape.Parent = this;
			Image.Parent = this;
			MaskShape.NotifyEnvironmentChanged(EnvironmentChanges.ParentReassigned);
			Image.NotifyEnvironmentChanged(EnvironmentChanges.ParentReassigned);
		}

		#region Info

		public override Shapes ShapeCode => Shapes.MaskedImage;

		#endregion

		#region Verbs - not applicable is it can't be drawn directly

		public override VerbResult Choose(EditableView.ClickPosition position) => throw new InvalidOperationException();
		public override VerbResult Start(EditableView.ClickPosition position) => throw new InvalidOperationException();
		public override VerbResult Complete(EditableView.ClickPosition position) => throw new InvalidOperationException();
		public override VerbResult CompleteRetrospective() => throw new InvalidOperationException();
		public override VerbResult Cancel(EditableView.ClickPosition position) => throw new InvalidOperationException();
		public override VerbResult Float(EditableView.ClickPosition position) => throw new InvalidOperationException();

		#endregion

		#region Datum

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			MaskedImage mask = (MaskedImage)other;
			// largely copied fom scriptable
			Debug.Assert((Image == null) == (MaskShape == null));
			if (depth == CopyDepth.Transform && MaskShape != null) // m_Mask and m_Image will either both be null or neither
			{
				// first time must copy reference, below
				if (MaskShape != mask.MaskShape)
					MaskShape?.CopyFrom(mask.MaskShape, depth, mapID);
				if (Image != mask.Image)
					Image?.CopyFrom(mask.Image, depth, mapID);
			}
			else// if (depth == CopyDepth.Duplicate)
			{
				if (mapID?.ContainsKey(mask.MaskShape.ID) ?? false)
					MaskShape = (Lined)mapID[mask.MaskShape.ID];
				else
					MaskShape = (Lined)mask.MaskShape.Clone(mapID ?? new Mapping()); // use actual mapID if there is one, otherwise need a real one as Container aspects of Item don't like Ignore
				MaskShape.Parent = this;

				if (mapID?.ContainsKey(mask.Image.ID) ?? false)
					Image = (ImportedImage)mapID[mask.Image.ID];
				else
					Image = (ImportedImage)mask.Image.Clone(mapID ?? new Mapping()); // use actual mapID if there is one, otherwise need a real one as Container aspects of Item don't like Ignore
				Image.Parent = this;
			}


			m_Bounds = RectangleF.Empty;
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(Image);
			writer.Write(MaskShape);
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			Image = (ImportedImage)reader.ReadData();
			MaskShape = (Lined)reader.ReadData();
			MaskShape.Parent = this;
			Image.Parent = this;
		}

		#endregion

		#region Coordinates

		protected override RectangleF CalculateBounds()
		{
			return MaskShape.Bounds;
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			return MaskShape.HitTestDetailed(clickPoint, scale, true);
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			MaskShape.ApplyTransformation(transformation);
			Image.ApplyTransformation(transformation);
		}

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			return MaskShape.GetGrabSpots(scale);
		}

		internal override bool StartGrabMove(GrabMovement grab)
		{
			switch (grab.GrabType)
			{
				case GrabTypes.SingleVertex:
				case GrabTypes.Bezier:
				case GrabTypes.BezierInactive:
					grab.IncludeShape(Image);
					grab.IncludeShape(MaskShape);
					break;
			}
			return base.StartGrabMove(grab);
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
				case GrabTypes.Bezier:
				case GrabTypes.BezierInactive:
					MaskShape.GrabMove(move);
					break;
				case GrabTypes.Fixed:// does nowt
					break;
				// these act just as base class would - use transform (which will be applied to both shapes)
				case GrabTypes.Move:
				case GrabTypes.Rotate:
				case GrabTypes.Radius:
				case GrabTypes.EdgeMoveV:
				case GrabTypes.EdgeMoveH:
				case GrabTypes.CornerResize:
				case GrabTypes.TextSelection:
					ApplyTransformation(move.Transform);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal override List<Target> GenerateTargets(UserSocket floating)
		{
			return MaskShape.GenerateTargets(floating);
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving() => MaskShape.GetPointsWhichSnapWhenMoving();

		#endregion

		#region Graphics

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			gr.IntersectClip(MaskShape.ConvertToPath());
			Image.Draw(gr, scale, coordScale, view, buffer, fillAlpha, 0, reverseRenderOrder);
			gr.RestoreClip();
			MaskShape.Draw(gr, scale, coordScale, view, buffer, 0, edgeAlpha, reverseRenderOrder);
		}

		internal override void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			MaskShape.DrawHighlight(gr, scale, coordScale, singleElement);
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{// not used as main functions replaced entirely
		}

		public override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{ // basically returns style for the shape, but inhibits fill styles as these won't work
			switch (parameter)
			{
				case Parameters.FillColour:
				case Parameters.FillPattern:
					return null;
			}
			return MaskShape.StyleObjectForParameter(parameter, applyingDefault);
		}

		#endregion

		#region Parent/containment

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			Parent.NotifyIndirectChange(shape, affected);
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			Parent.NotifyIndirectChange(shape, affected, area);
		}

		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			base.AddToFlatList(list, purpose);
			if (purpose == FlatListPurpose.ExposedShapes)
				return; // shapes inside this are not exposed
			MaskShape.AddToFlatList(list, purpose);
			Image.AddToFlatList(list, purpose);
		}

		internal override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			base.NotifyEnvironmentChanged(change);
			MaskShape.NotifyEnvironmentChanged(change);
			Image.NotifyEnvironmentChanged(change);
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			Image.UpdateReferencesObjectsCreated(document, reader);
			MaskShape.UpdateReferencesObjectsCreated(document, reader);
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			Image.UpdateReferencesIDsChanged(mapID, document);
			MaskShape.UpdateReferencesIDsChanged(mapID, document);
		}

		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			MaskShape.Iterate(fn);
			Image.Iterate(fn);
		}

		protected override bool IdenticalToShape(Shape other)
		{
			if (!base.IdenticalToShape(other))
				return false;
			MaskedImage otherMask = (MaskedImage)other;
			if (!MaskShape.IdenticalTo(otherMask.MaskShape))
				return false;
			if (!Image.IdenticalTo(otherMask.Image))
				return false;
			return true;
		}


		#endregion

	}
}
