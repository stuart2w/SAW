using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SAW.Functions
{
	internal static class DrawingVerbs
	{

		public static void RegisterDrawingVerbs()
		{
			Verb.Register(Codes.Choose, new Choose());
			Verb.Register(Codes.ChooseExisting, new DrawingVerbDoneByPanel());
			Verb.Register(Codes.Cancel, new Cancel());
			Verb.Register(Codes.Complete, new DrawingVerbDoneByPanel());
			Verb.Register(Codes.Increment, new DrawingVerbDoneByPanel());
			Verb.Register(Codes.Decrement, new DrawingVerbDoneByPanel());
			Verb.Register(Codes.Up, new MouseMove() { X = 0, Y = -1 });
			Verb.Register(Codes.Right, new MouseMove() { X = 1, Y = 0 });
			Verb.Register(Codes.Down, new MouseMove() { X = 0, Y = 1 });
			Verb.Register(Codes.Left, new MouseMove() { X = -1, Y = 0 });
			Verb.Register(Codes.MouseSmallStep, new MouseStep() { Step = AppliedConfig.MouseSteps.Small });
			Verb.Register(Codes.MouseMediumStep, new MouseStep() { Step = AppliedConfig.MouseSteps.Medium });
			Verb.Register(Codes.MouseLargeStep, new MouseStep() { Step = AppliedConfig.MouseSteps.Large });
			Verb.Register(Codes.RestoreFocus, (source, pnlView, tx) => { pnlView.Focus(); });
			Verb.Register(Codes.PageUp, (source, pnlView, tx) => { pnlView.PageUpDown(-1); });
			Verb.Register(Codes.PageDown, (source, pnlView, tx) => { pnlView.PageUpDown(1); });
			Verb.Register(Codes.ScrollDown, (source, pnlView, tx) => { pnlView.DoScroll(new Size(0, 2)); });
			Verb.Register(Codes.ScrollUp, (source, pnlView, tx) => { pnlView.DoScroll(new Size(0, -2)); });
			Verb.Register(Codes.ScrollLeft, (source, pnlView, tx) => { pnlView.DoScroll(new Size(-2, 0)); });
			Verb.Register(Codes.ScrollRight, (source, pnlView, tx) => { pnlView.DoScroll(new Size(2, 0)); });
		}

	}

	internal abstract class DrawingVerb : Verb
	{
		public override bool IsApplicable(EditableView pnlView)
		{
			if (Code == Codes.Cancel && Editor.MovingPalette != null)
				return true;
			if (pnlView.OngoingShape != null)
				return true; // all verbs assumed active when creating a shape
							 // otherwise verb is only active if there is a single shape which allows verbs after completion...
			if (pnlView.Grabbing && Code == Codes.Cancel)
				return true;
			if (CurrentPage.SelectedCount != 1)
				return false;
			return CurrentPage.SelectedShapes[0].AllowVerbWhenComplete(Code);
		}
	}

	internal class Choose : Verb // not drawing verb - this one is always applicable
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (Editor.MovingPalette != null)
				Editor.CompletePaletteMove();
			Control over = GUIUtilities.YoungestChildUnderMouse(Editor);
			if (source == ClickPosition.Sources.Pad)
				over = pnlView; // if triggered by mouse in control pad, target must be drawing area, not living back to the control pad!
			if (over == pnlView)
				pnlView.TriggerVerb(Codes.Choose, source);
			else if (over is Button)
				((Button)over).PerformClick();
			else if (over is IInvokeable)
				((IInvokeable)over).PerformClick();
		}
	}

	internal class Cancel : DrawingVerb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (Editor.MovingPalette != null)
				Editor.AbortPaletteMove();
			else
				pnlView.TriggerVerb(Codes.Cancel, source);
		}
	}

	/// <summary>Just calls TriggerVerb on panel view - used for several verbs</summary>
	internal class DrawingVerbDoneByPanel : DrawingVerb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.TriggerVerb(Code, source);
		}
	
	}

	internal class MouseMove : Verb
	{
		public int X;
		public int Y;
		/// <summary>The current mouse step size in use</summary>
		public static int CurrentStepSize = 1;

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			// parameters are the amount to move always as a single unit.  Scale is applied inside this function
			// this uses screen coordinates which are top downwards for Y
			Debug.Assert(Math.Abs(X) + Math.Abs(Y) == 1);
			Size szChange = new Size(X * CurrentStepSize, Y * CurrentStepSize); // this has the step applied to
			pnlView.MoveCursor(szChange);
		}

	}

	internal class MouseStep : Verb
	{
		/// <summary>Current step size in use.  See also MouseMove.CurrentStepSize </summary>
		public static AppliedConfig.MouseSteps CurrentStep = AppliedConfig.MouseSteps.Medium;
		/// <summary>The step which this verb object configures </summary>
		public AppliedConfig.MouseSteps Step;

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			SetMouseStep(Step);
		}

		public static void SetMouseStep(AppliedConfig.MouseSteps eStep)
		{
			CurrentStep = eStep;
			if (Globals.Root.CurrentConfig == null)
			{
				switch (CurrentStep)
				{
					case AppliedConfig.MouseSteps.Large:
						MouseMove.CurrentStepSize = (int)Config.MouseStepDefaults.Large;
						break;
					case AppliedConfig.MouseSteps.Medium:
						MouseMove.CurrentStepSize = (int)Config.MouseStepDefaults.Medium;
						break;
					default:
						MouseMove.CurrentStepSize = (int)Config.MouseStepDefaults.Small;
						break;
				}
			}
			else
				MouseMove.CurrentStepSize = Globals.Root.CurrentConfig.MouseStep(eStep);
		}

	}

}
