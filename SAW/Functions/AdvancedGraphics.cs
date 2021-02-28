using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAW.Functions
{
	internal static class AdvancedGraphics
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.ConvertToPath, new ConvertToPath());
			Verb.Register(Codes.RemoveVertex, new VertexChange());
			Verb.Register(Codes.AddVertex, new VertexChange());
			Verb.Register(Codes.ConvertToBezier, new VertexChange());
			Verb.Register(Codes.ConvertToLine, new VertexChange());
			Verb.Register(Codes.SmoothVertex, new VertexChange());
			Verb.Register(Codes.CornerVertex, new VertexChange());
			Verb.Register(Codes.MakeMask, new MakeMask());
			Verb.Register(Codes.RemoveMask, new RemoveMask());
		}

	}

	#region Convert to path
	internal class ConvertToPath : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage.SelectedCount != 1)
				return;
			Shape shape = CurrentPage.SelectedShapes.First();
			GraphicsPath path = shape.ConvertToPath();
			if (path == null)
			{
				Utilities.LogSubError("ConvertToPath returned null from: " + shape.ShapeCode);
				MessageBox.Show(Strings.Item("ConvertPath_Null"));
				return;
			}
			GenericPath create = new GenericPath(path);
			create.DeriveStyles(new[] { shape }); // {} because it takes a list of shapes
			transaction.Edit((Datum)shape.Parent);
			transaction.Create(create);
			IShapeContainer container = (IShapeContainer)shape.Parent;
			container.Contents.Remove(shape);
			container.Contents.Add(create);
			container.FinishedModifyingContents(transaction);
			create.Parent = shape.Parent;
			transaction.Delete(shape);
			CurrentPage.SelectOnly(create);
		}

		public override bool AutoRefreshAfterTrigger => true;

		public override bool IsApplicable(EditableView pnlView)
		{
			return Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics) && CurrentPage.SelectedCount == 1 && CurrentPage.SelectionAnyAllows(Shape.AllowedActions.ConvertToPath);
		}

		

	}
	#endregion

	#region Vertex change - several different operations amending a single vertex or line
	/// <summary>Various vertex verbs which are all actually done in the shape </summary>
	internal class VertexChange : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Shape shape = CurrentPage.SelectedShapes.First();
			transaction.Edit(shape);
			Shape.VerbResult result = shape.OtherVerb(new EditableView.ClickPosition(CurrentPage.SelectedPath.Position, CurrentPage, pnlView.Zoom, Shape.SnapModes.Off, Shape.SnapModes.Off, pnlView, source), Code);
			if (result == Shape.VerbResult.Unexpected || result == Shape.VerbResult.Unchanged || result == Shape.VerbResult.Rejected)
				transaction.Cancel();
			pnlView.ForceUpdateGrabSpots();
		}

		public override bool AutoRefreshAfterTrigger => true;

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount != 1)
				return false;
			if (CurrentPage.SelectedPath == null)
				return false;
			if (!CurrentPage.SelectedShapes.First().VertexVerbApplicable(Code, CurrentPage.SelectedPath))
				return false;
			switch (Code)
			{
				case Codes.AddVertex:
				case Codes.ConvertToBezier:
				case Codes.ConvertToLine:
					return CurrentPage.SelectedPath.Type == Target.Types.Line;
				case Codes.RemoveVertex:
				case Codes.SmoothVertex:
				case Codes.CornerVertex:
					return CurrentPage.SelectedPath.Type == Target.Types.Vertex;
				default: throw new InvalidOperationException();
			}
		}

	}
	#endregion

	#region Make mask
	/// <summary>Makes a masked image from an image and a polygon </summary>
	internal class MakeMask : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			IShapeContainer container = CurrentPage.SelectionContainer();
			if (container == null)
			{
				MessageBox.Show(Strings.Item("Container_Mismatch"));
				return;
			}

			transaction.Edit((Datum)container);
			Lined mask = FindSourceShape();
			ImportedImage image = FindSourceImage();
			transaction.Edit(mask);
			transaction.Edit(image);
			(mask as Pencil).ForceClosure(transaction);
			if (mask is Curve curve && !curve.Closed())
				throw new UserException("[Mask_Not_Closed]");
			MaskedImage result = new MaskedImage(image, mask);
			transaction.Create(result);

			int index = Math.Max(mask.Z, image.Z); // this will be the Z-order of the new group
			result.Parent = container;
			container.Contents.Insert(index + 1, result); // This index will be valid as the individual shapes are still in the list
			container.Contents.Remove(mask);
			container.Contents.Remove(image);
			container.FinishedModifyingContents(transaction, null);
			CurrentPage.SelectOnly(result);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics))
				return false;
			if (CurrentPage.SelectedCount != 2)
				return false;
			if (FindSourceShape() == null || FindSourceImage() == null)
				return false; // they can't return the same item, so if both are defined that must encompass both selected shapes
			return true;
		}

		private ImportedImage FindSourceImage()
		{
			return (from s in CurrentPage.SelectedShapes where s is ImportedImage select (s as ImportedImage)).FirstOrDefault();
		}

		private Lined FindSourceShape()
		{ // at the moment this only returns Filled shapes - all of which are permitted.  It is defined as Lined as we might want to accept PolyLine ?
			return (from s in CurrentPage.SelectedShapes where s is Filled select (s as Filled)).FirstOrDefault();
		}

		public override bool HideFromContextMenuIfUnavailable => true;

	}

	/// <summary>Reverses the process of MakeMask, turning a MaskedImage back into a simple shape and image</summary>
	internal class RemoveMask : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{

			MaskedImage masked = (MaskedImage)CurrentPage.SelectedShapes.First();
			IShapeContainer container = (IShapeContainer)masked.Parent;
			transaction.Edit((Datum)container);
			transaction.Edit(masked.MaskShape);
			transaction.Edit(masked.Image);
			transaction.Delete(masked);

			masked.Image.Parent = container;
			masked.MaskShape.Parent = container;
			container.Contents.Insert(masked.Z + 1, masked.Image); // This index will be valid as the individual shapes are still in the list
			container.Contents.Insert(masked.Z + 2, masked.MaskShape);
			container.Contents.Remove(masked);
			container.FinishedModifyingContents(transaction, null);
			CurrentPage.SelectOnly(masked.MaskShape); // arbitrarily seemd better to leave the shape selected
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount != 1) return false;
			return (CurrentPage.SelectedShapes.First().ShapeCode == Shape.Shapes.MaskedImage);
		}

		public override bool HideFromContextMenuIfUnavailable => true;

	}

	#endregion

}
