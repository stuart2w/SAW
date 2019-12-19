using System;
using System.Linq;

namespace SAW.Functions
{
	abstract class SelectionVerb : Verb
	{
		public static void RegisterSelectionVerbs()
		{
			Register(Codes.SelectNone, new SelectNone());
			Register(Codes.SelectAll, new SelectAll());
			Register(Codes.SelectNext, new SelectNext());
			Register(Codes.SelectPrevious, new SelectPrevious());
		}

		public override bool IncludeOnContextMenu
		{ get { return false; } } // hides the entire selection sub menu

	}

	class SelectNone : SelectionVerb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Globals.Root.CurrentPage.DeselectAll();
		}

		public override bool IsApplicable(EditableView pnlView) => Globals.Root.CurrentPage.SelectedCount > 0 && pnlView.OngoingShape == null;
	}

	class SelectAll : SelectionVerb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Globals.Root.CurrentPage.SelectAll();
		}

		public override bool IsApplicable(EditableView pnlView) => Globals.Root.CurrentPage.Any() && pnlView.OngoingShape == null;
	}

	class SelectNext : SelectionVerb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.IterateSelectionTo(Globals.Root.CurrentPage.NextSelection(1));
		}

		public override bool IsApplicable(EditableView pnlView) 
			=> Globals.Root.CurrentPage.Count() > Math.Max(1, Globals.Root.CurrentPage.SelectedCount) && pnlView.OngoingShape == null;
	}

	class SelectPrevious : SelectionVerb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.IterateSelectionTo(Globals.Root.CurrentPage.NextSelection(-1));
		}

		public override bool IsApplicable(EditableView pnlView) 
			=> Globals.Root.CurrentPage.Count() > Math.Max(1, Globals.Root.CurrentPage.SelectedCount) && pnlView.OngoingShape == null;
	}
}
