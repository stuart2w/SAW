using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SAW.Functions
{
	internal static class ObjectEdits
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.SendBack, new ChangeZ());
			Verb.Register(Codes.SendBackOneStep, new ChangeZ());
			Verb.Register(Codes.BringFront, new ChangeZ());
			Verb.Register(Codes.BringFrontOneStep, new ChangeZ());
			Verb.Register(Codes.Texture, new SelectTexture());
			Verb.Register(Codes.FlipHorizontal, new QuickTransform());
			Verb.Register(Codes.FlipVertical, new QuickTransform());
			Verb.Register(Codes.RotateLeft, new QuickTransform());
			Verb.Register(Codes.RotateRight, new QuickTransform());
			Verb.Register(Codes.NudgeDown, new Nudge() { X = 0, Y = 1 });
			Verb.Register(Codes.NudgeUp, new Nudge() { X = 0, Y = -1 });
			Verb.Register(Codes.NudgeLeft, new Nudge() { X = -1, Y = 0 });
			Verb.Register(Codes.NudgeRight, new Nudge() { X = 1, Y = 0 });
			Verb.Register(Codes.TextLarger, new ChangeTextSize() { Delta = 1 });
			Verb.Register(Codes.TextSmaller, new ChangeTextSize() { Delta = -1 });
			Verb.Register(Codes.TidyAngle, new Tidy() { Mode = Shape.SnapModes.Angle });
			Verb.Register(Codes.TidyShape, new Tidy() { Mode = Shape.SnapModes.Shape });
			Verb.Register(Codes.TidyGrid, new Tidy() { Mode = Shape.SnapModes.Grid });
			Verb.Register(Codes.DoubleClick, new DoubleClick());
			Verb.Register(Codes.QuickAddButtons, (source, pnlView, tx) => { frmAddButtons.Display(Globals.Root.CurrentPage); }, true, view => Globals.Root.CurrentDocument.ActivityID.Equals(Activities.PaletteID));
			Verb.Register(Codes.FreeTextToTextLine, new FreeTextToLine());
			Verb.Register(Codes.TypeDegree, (source, pnlView, tx) => { Globals.Root.Editor.SimulateKey((char)176); }, abandonsCurrent: false);
			Verb.Register(Codes.SmallestHeight, new Alignment());
			Verb.Register(Codes.SmallestWidth, new Alignment());
			Verb.Register(Codes.LargestHeight, new Alignment());
			Verb.Register(Codes.LargestWidth, new Alignment());
			for (Codes code = Codes.AlignLeft; code <= Codes._AfterAlignment; code++)
				Verb.Register(code, new Alignment());
			Verb.Register(Codes.MakeChild, new MakeChild());
			Verb.Register(Codes.MoveOutOfContainer, new MoveOutOfContainer());

		}

	}

	internal class DoubleClick : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Globals.Root.CurrentPage.SelectedShapes[0].DoDoubleClick(pnlView, EditableView.ClickPosition.Sources.VerbButton);
		}

		public override bool AbandonsCurrent => false;// must not abandon, otherwise control+enter does not edit existing text

		public override bool IsApplicable(EditableView pnlView)
		{
			return CurrentPage.SelectedCount >= 1
				   && CurrentPage.SelectedShapes.First().CanDoubleClickWith(CurrentPage.SelectedShapes)
				   && CurrentPage.SelectedShapes[0].DoubleClickText() != null;
		}
	}

	internal class ChangeZ : Verb
	{

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			switch (Code)
			{
				case Codes.SendBack:
				case Codes.BringFront:
					CurrentPage.SendToFrontBack((Code == Codes.BringFront) ^ CurrentDocument.ReverseRenderOrder, transaction); // sense front/back is reversed if they render the wrong way round
					break;
				case Codes.BringFrontOneStep:
				case Codes.SendBackOneStep:
					CurrentPage.SendToFrontBackOneStep((Code == Codes.BringFrontOneStep) ^ CurrentDocument.ReverseRenderOrder, transaction);
					break;
			}
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount == 0)
				return false;
			// now too complex to test this typeof thing (whether already at front/back) m_Page.SelectedShapes[0].Z < m_Page.TotalCount - 1
			if (Code == Codes.BringFrontOneStep && CurrentPage.SelectedCount > 1)
				return false;
			if (Code == Codes.SendBackOneStep && CurrentPage.SelectedCount > 1)
				return false;
			return true; // now too complex to test this  m_Page.SelectedShapes[0].Z > 0
		}
	}

	/// <summary>Makes selected item a child of its previous sibling</summary>
	internal class MakeChild : Verb
	{

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Shape moving = CurrentPage.SelectedShapes.First();
			IShapeContainer oldContainer = moving.Container;
			int targetIndex = oldContainer.Contents.IndexOf(moving) - 1; // index of new container
			if (targetIndex < 0)
				return;
			IShapeContainer newContainer = oldContainer.Contents[targetIndex].AsContainer;
			transaction.Edit((Datum)oldContainer);
			transaction.Edit((Datum)newContainer);
			transaction.Edit(moving);
			oldContainer.Contents.Remove(moving);
			newContainer.Contents.Add(moving);
			moving.Parent = newContainer;

			// move child physically within
			(newContainer as Container)?.BringChildWithinBounds(moving, transaction);

			oldContainer.FinishedModifyingContents(transaction);
			newContainer.FinishedModifyingContents(transaction);

			moving.Parent.NotifyIndirectChange(moving, ChangeAffects.GrabSpots | ChangeAffects.Intersections);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount != 1)
				return false;
			// can't do it if it's listed first:
			if (CurrentPage.SelectedShapes.First().Z == 0)
				return false;
			return true;
		}

		// refresh can be needed if child gets moved
		public override bool AutoRefreshAfterTrigger => true;
	}

	/// <summary>Moves it out one level of containment so it becomes a sibling of its parent</summary>
	internal class MoveOutOfContainer : Verb
	{

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Shape moving = CurrentPage.SelectedShapes.First();
			IShapeContainer oldContainer = moving.Container;
			IShapeContainer newContainer = (oldContainer as Shape).Container.AsParentContainer;
			transaction.Edit((Datum)oldContainer);
			transaction.Edit((Datum)newContainer);
			transaction.Edit(moving);
			oldContainer.Contents.Remove(moving);
			newContainer.Contents.Add(moving);
			moving.Parent = newContainer;

			oldContainer.FinishedModifyingContents(transaction);
			newContainer.FinishedModifyingContents(transaction);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount != 1)
				return false;
			if (CurrentPage.SelectedShapes.First().Parent is Page)
				return false; // already at top level
			return true;
		}

		public override bool AutoRefreshAfterTrigger => true;

	}

	internal class SelectTexture : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			SharedImage texture = frmTexture.PickTexture(CurrentDocument);
			if (texture == null) return;
			texture = CurrentDocument.AddSharedResource(texture);
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				Shape.FillStyleC style = (Shape.FillStyleC)shape.StyleObjectForParameter(Parameters.FillPattern);
				if (style != null)
				{
					transaction.Edit(shape);
					style.Texture = texture;
				}
			}
			pnlView.InvalidateData(CurrentPage.SelectedRefreshBoundary(), StaticView.InvalidationBuffer.All);
		}

		public override bool AbandonsCurrent => false;
		public override bool IsApplicable(EditableView pnlView) => Globals.ParameterApplicable(Parameters.FillPattern);
		public override bool IncludeOnContextMenu => false;
		public override bool MightOpenModalDialog => true;
	}

	internal class QuickTransform : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage == null || CurrentPage.SelectedCount < 1)
				return;
			bool reflect = false;
			bool rotate = false;
			switch (Code)
			{
				case Codes.FlipHorizontal:
				case Codes.FlipVertical:
					reflect = true;
					break;
				case Codes.RotateLeft:
				case Codes.RotateRight:
					rotate = true;
					break;
				default:
					Utilities.LogSubError("Unexpected verb in DoQuickTransform");
					return;
			}
			RectangleF bounds = RectangleF.Empty; // not using Page.SelectedBounds as I think it might be better to use the minimal bounds
			SizeF totalSize = SizeF.Empty;
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				RectangleF shapeBounds = shape.MinimalBounds;
				Geometry.Extend(ref bounds, shapeBounds);
				if (reflect && (shape.Allows & (Shape.AllowedActions.TransformMirror | Shape.AllowedActions.MirrorFlipOnly)) == 0)
				{
					MessageBox.Show(Strings.Item("Cannot_Reflect"));
					return;
				}
				if (rotate && (shape.Allows & Shape.AllowedActions.TransformRotate) == 0)
				{
					MessageBox.Show(Strings.Item("Cannot_Rotate"));
					return;
				}
				totalSize += shapeBounds.Size;
			}
			Transformation transformation;
			PointF middle = new PointF((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
			switch (Code)
			{
				case Codes.FlipHorizontal:
					transformation = new TransformMirror(middle, middle + new SizeF(0, 100));
					break;
				// second point is arbitrary as long as it is on the necessary vertical line
				case Codes.FlipVertical:
					transformation = new TransformMirror(middle, middle + new SizeF(100, 0));
					break;
				case Codes.RotateLeft:
					transformation = new TransformRotate(middle, -Geometry.ANGLE90);
					break;
				case Codes.RotateRight:
					transformation = new TransformRotate(middle, Geometry.ANGLE90);
					break;
				default:
					Utilities.LogSubError("Unexpected verb in DoQuickTransform");
					return;
			}
			DoTransformForVerb(pnlView, transformation, transaction);
		}

		internal static void DoTransformForVerb(EditableView pnlView, Transformation transformation, Transaction transaction)
		{ // part of both DoQuickTransform and DoNudge
			RectangleF invalid = CurrentPage.SelectedRefreshBoundary();
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				transaction.Edit(shape);
				shape.ApplyTransformation(transformation);
				shape.Status = Shape.StatusValues.Moved;
				Geometry.Extend(ref invalid, shape.RefreshBounds());
			}
			Geometry.Extend(ref invalid, pnlView.ForceUpdateGrabSpots());
			pnlView.InvalidateData(invalid, StaticView.InvalidationBuffer.All);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			switch (Code)
			{
				case Codes.FlipHorizontal:
				case Codes.FlipVertical:
					return CurrentPage.SelectionAnyAllows(Shape.AllowedActions.TransformMirror) || CurrentPage.SelectionAnyAllows(Shape.AllowedActions.MirrorFlipOnly);
				// strictly speaking this is incorrect, because it requires either (every shape support either A) or (every to support B)
				// whereas every shape supporting (A or B) is sufficient.  But I doubt anyone will notice
				case Codes.RotateLeft:
				case Codes.RotateRight:
					return CurrentPage.SelectionAnyAllows(Shape.AllowedActions.TransformRotate);
				default: throw new InvalidOperationException();
			}
		}

	}

	internal class Nudge : Verb
	{
		public int X;
		public int Y;
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage == null || CurrentPage.SelectedCount < 1)
				return;
			float step = 1;
			// in SAW it defaults to 1; but splash always uses grid
			if (pnlView.SnapMode == Shape.SnapModes.Grid)
				step = CurrentPage.Paper.ScalarSnapStep(0);
			TransformMove transformation = new TransformMove(X * step, Y * step);
			QuickTransform.DoTransformForVerb(pnlView, transformation, transaction);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentPage.SelectionAllAllow(Shape.AllowedActions.TransformMove);
	}

	internal class ChangeTextSize : Verb
	{
		public int Delta;
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			float newValue = frmFont.AdjustSize(Editor.ParameterValue(Parameters.FontSize) / 100f, Delta);
			Editor.SetParameterValue((int)(newValue * 100), Parameters.FontSize);
			((ctrTextStyle)Palette.Item(Parameters.FontSize).Control).EnableButtons();
		}

		public override bool AbandonsCurrent => false;
		public override bool IsApplicable(EditableView pnlView)
			=> Globals.ParameterApplicable(Parameters.FontSize);
	}

	internal class Tidy : Verb
	{
		public Shape.SnapModes Mode;

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			bool changed = false;
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				transaction.Edit(shape); // must add to Tx before Tidy to get the previous state.  Only after Tidy do we know if it actually changed
				if (shape.Tidy(Mode, CurrentPage))
				{
					changed = true;
					CurrentPage.UpdateIntersectionsWith(shape, true);
				}
				else
					transaction.Disregard(shape);
			}
			if (changed)
				pnlView.InvalidateData(pnlView.ForceUpdateGrabSpots(), StaticView.InvalidationBuffer.Selection);
			// main invalidation done automatically by tx
		}

		public override bool AutoRefreshAfterTrigger => true;

		public override bool IsApplicable(EditableView pnlView)
		{
			switch (Code)
			{
				case Codes.TidyAngle: return CurrentPage.SelectionAnyAllows(Shape.AllowedActions.Tidy);
				case Codes.TidyShape: return CurrentPage.SelectionAnyAllows(Shape.AllowedActions.Tidy);
				case Codes.TidyGrid:
					return CurrentPage.SelectionAnyAllows(Shape.AllowedActions.Tidy) && CurrentPage.Paper.PaperType != Paper.Papers.Plain;
				default: throw new InvalidOperationException();
			}
		}

	}

	internal class FreeTextToLine : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			List<FreeText> shapes = CurrentPage.SelectedShapes.OfType<FreeText>().ToList(); // new copy so we don't have problems editing while iterating
			if (shapes.Count == 0)
				return;
			CurrentPage.DeleteSelected(transaction);
			List<Shape> newList = new List<Shape>();
			foreach (FreeText shp in shapes)
			{
				TextLine textLine = TextLine.FromFreeText(shp);
				transaction.Create(textLine);
				newList.Add(textLine);
				CurrentPage.AddNew(textLine, transaction);
			}
			CurrentPage.SelectOnly(newList);
		}

		public override bool AutoRefreshAfterTrigger => true;

		public override bool IsApplicable(EditableView pnlView)
			=> CurrentPage.SelectedCount > 0 && CurrentPage.SelectedShapes.All(shp => shp.ShapeCode == Shape.Shapes.FreeText);

		public override bool IncludeOnContextMenu => false;
	}

	internal class Alignment : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage == null || CurrentPage.SelectedCount < 2)
				return;
			bool resize = Code == Codes.EqualiseHeight || Code == Codes.EqualiseWidth || Code == Codes.SmallestWidth || Code == Codes.SmallestHeight || Code == Codes.LargestWidth || Code == Codes.LargestHeight;
			RectangleF bounds = RectangleF.Empty; // not using Page.SelectedBounds as I think it might be better to use the minimal bounds
			SizeF size = SizeF.Empty; // total of all shape sizes added up.  Used to equalise width/height.  If going to smallest/largest this is overwritten later
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				RectangleF rect = shape.MinimalBounds;
				Geometry.Extend(ref bounds, rect);
				if (resize && (shape.Allows & (Shape.AllowedActions.TransformScale | Shape.AllowedActions.TransformLinearStretch)) == 0)
				{
					MessageBox.Show(Strings.Item("Cannot_Resize"));
					return;
				}
				if (Code == Codes.EqualiseWidth && rect.Width < Geometry.NEGLIGIBLE || Code == Codes.EqualiseHeight && rect.Height < Geometry.NEGLIGIBLE)
				{
					// can't stretch a shape which is currently zero dimension in the direction we are changing; this would require infinite scale
					MessageBox.Show(Strings.Item("Cannot_Resize_Zero"));
					return;
				}
				size += rect.Size;
				Debug.Assert((shape.Allows & Shape.AllowedActions.TransformMove) > 0);
			}
			RectangleF refreshBoundary = CurrentPage.SelectedRefreshBoundary();
			List<Shape> shapes = new List<Shape>(CurrentPage.SelectedShapes); // can't use the actual m_Page.SelectedShapes because we might need to rearrange the list
			float spread = 0; // current coordinates for spread horizontal and vertical
			switch (Code)
			{
				case Codes.SpreadHorizontal:
					shapes.Sort(Shape.XSort);
					spread = bounds.Left;
					break;
				case Codes.SpreadVertical:
					shapes.Sort(Shape.YSort);
					spread = bounds.Top;
					break;
				case Codes.SmallestWidth:
					size.Width = (from Shape shape in CurrentPage.SelectedShapes select shape.MinimalBounds.Width).Min();
					break;
				case Codes.LargestWidth:
					size.Width = (from Shape shape in CurrentPage.SelectedShapes select shape.MinimalBounds.Width).Max();
					break;
				case Codes.SmallestHeight:
					size.Height = (from Shape shape in CurrentPage.SelectedShapes select shape.MinimalBounds.Height).Min();
					break;
				case Codes.LargestHeight:
					size.Height = (from Shape shape in CurrentPage.SelectedShapes select shape.MinimalBounds.Height).Max();
					break;
			}
			if (resize && (size.Width == 0 || size.Height == 0)) // Note we cannot use size.IsEmpty because that is only true if both are zero
			{
				MessageBox.Show(Strings.Item("Cannot_Resize_Zero"));
				return;
			}
			for (int index = 0; index <= CurrentPage.SelectedCount - 1; index++)
			{
				Shape shape = shapes[index];
				transaction.Edit(shape);
				// several of the actions below need the middle of the current shape
				RectangleF minimalBounds = shape.MinimalBounds;
				PointF middle = new PointF((minimalBounds.Left + minimalBounds.Right) / 2, (minimalBounds.Top + minimalBounds.Bottom) / 2);
				switch (Code)
				{
					case Codes.AlignLeft:
						shape.ApplyTransformation(new TransformMove(bounds.Left - shape.MinimalBounds.Left, 0));
						break;
					case Codes.AlignRight:
						shape.ApplyTransformation(new TransformMove(bounds.Right - shape.MinimalBounds.Right, 0));
						break;
					case Codes.AlignCentre:
						shape.ApplyTransformation(new TransformMove((bounds.Left + bounds.Right) / 2 - middle.X, 0));
						break;
					case Codes.AlignTop:
						shape.ApplyTransformation(new TransformMove(0, bounds.Top - shape.MinimalBounds.Top));
						break;
					case Codes.AlignBottom:
						shape.ApplyTransformation(new TransformMove(0, bounds.Bottom - shape.MinimalBounds.Bottom));
						break;
					case Codes.AlignMiddle:
						shape.ApplyTransformation(new TransformMove(0, (bounds.Top + bounds.Bottom) / 2 - middle.Y));
						break;
					case Codes.SmallestWidth:
					case Codes.LargestWidth: // these can be implemented in one because the required dimension is already set in size
					case Codes.EqualiseWidth: // Where as this will need a different scale calculation, but the rest is the same
						float widthScale = size.Width / shape.MinimalBounds.Width;
						if (Code == Codes.EqualiseWidth)
							widthScale = size.Width / CurrentPage.SelectedCount / shape.MinimalBounds.Width;
						if ((shape.Allows & Shape.AllowedActions.TransformLinearStretch) > 0)
							shape.ApplyTransformation(new TransformLinearScale(middle, widthScale, true));
						else
							shape.ApplyTransformation(new TransformScale(middle, widthScale));
						break;
					case Codes.SmallestHeight:
					case Codes.LargestHeight:
					case Codes.EqualiseHeight:
						float heightScale = size.Height / shape.MinimalBounds.Height;
						if (Code == Codes.EqualiseHeight)
							heightScale = size.Height / CurrentPage.SelectedCount / shape.MinimalBounds.Height;
						if ((shape.Allows & Shape.AllowedActions.TransformLinearStretch) > 0)
							shape.ApplyTransformation(new TransformLinearScale(middle, heightScale, false));
						else
							shape.ApplyTransformation(new TransformScale(middle, heightScale));
						break;
					case Codes.SpreadHorizontal:
						shape.ApplyTransformation(new TransformMove(spread - minimalBounds.Left, 0));
						spread += minimalBounds.Width + (bounds.Width - size.Width) / (shapes.Count - 1); // doesn't matter if top half of fraction is negative
						break;
					case Codes.SpreadVertical:
						shape.ApplyTransformation(new TransformMove(0, spread - minimalBounds.Top));
						spread += minimalBounds.Height + (bounds.Height - size.Height) / (shapes.Count - 1);
						break;
				}
				shape.Status = Shape.StatusValues.Moved;
				Geometry.Extend(ref refreshBoundary, shape.RefreshBounds());
				CurrentPage.UpdateIntersectionsWith(shape);
			}
			pnlView.InvalidateData(refreshBoundary, StaticView.InvalidationBuffer.All);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			switch (Code)
			{
				case Codes.SpreadHorizontal:
				case Codes.SpreadVertical:
					return CurrentPage.SelectedCount >= 3;
				case Codes.EqualiseHeight:
				case Codes.EqualiseWidth:
				case Codes.SmallestHeight:
				case Codes.SmallestWidth:
				case Codes.LargestHeight:
				case Codes.LargestWidth:
					return CurrentPage.SelectedCount >= 2;
			}
			return CurrentPage.SelectedCount >= 2;
		}

	}
}
