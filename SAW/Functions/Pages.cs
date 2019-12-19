using System.Windows.Forms;

namespace SAW.Functions
{
	static class PageVerbs
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.PageNext, new PageNext());
			Verb.Register(Codes.PagePrevious, new PagePrevious());
			Verb.Register(Codes.PageDelete, new PageDelete());
			Verb.Register(Codes.PageClear, new PageClear());
			Verb.Register(Codes.PageInsertAfter, new PageAdd());
			Verb.Register(Codes.PageDuplicate, new PageAdd() { Duplicate = true });
			Verb.Register(Codes.PageInsertBefore, new PageAdd() { Offset = 0 });
			Verb.Register(Codes.PageMoveDown, new PageMoveDown());
			Verb.Register(Codes.PageMoveUp, new PageMoveUp());
			Verb.Register(Codes.EditPaper, new EditPaper());
			Verb.Register(Codes.PageSize, new PageSize());
			Verb.Register(Codes.SetOrigin, new SetOrigin());
		}
	}

	class PageNext : Verb
	{// adds another page if at the end
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPageIndex >= CurrentDocument.Count - 1)
			{
				if (CurrentDocument.AddPage(CurrentPage, transaction, true) == null)
					return;
			}
			Editor.DisplayPage(CurrentPageIndex + 1);
		}

		public override bool IsApplicable(EditableView pnlView) => !CurrentDocument.IsPaletteWithin;
		public override bool IncludeOnContextMenu => false;
	}

	class PagePrevious : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPageIndex > 0)
				Editor.DisplayPage(CurrentPageIndex - 1);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentPageIndex > 0 && !CurrentDocument.IsPaletteWithin;
		public override bool IncludeOnContextMenu => false;
	}

	class PageDelete : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentDocument.Count < 2)
				return;
			// deleting the last page would bugger things up completely
			// Only confirm with user if the page is not empty
			if (!CurrentPage.IsEmpty)
			{
				if (MessageBox.Show(Strings.Item("Confirm_DeletePage"), RootApplication.AppName, MessageBoxButtons.OKCancel) != DialogResult.OK)
					return;
			}
			CurrentDocument.DeletePage(CurrentPageIndex, transaction);
			// Usually we will display the page following this one, which now has the same page index, unless there isn't one
			int newIndex = CurrentPageIndex;
			if (newIndex >= CurrentDocument.Count)
				newIndex = CurrentDocument.Count - 1;
			Editor.DisplayPage(newIndex);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentDocument.Count > 1 && !CurrentDocument.IsPaletteWithin;
	}

	class PageClear : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.ConcludeOngoing();
			if (MessageBox.Show(Strings.Item("Confirm_ClearPage"), RootApplication.AppName, MessageBoxButtons.OKCancel) == DialogResult.OK) CurrentPage.Clear(transaction);
		}

		public override bool ImmediateRefresh => true;
		public override bool IsApplicable(EditableView pnlView) => !CurrentPage.IsEmpty;
		public override bool IncludeOnContextMenu => false;
	}

	class PageAdd : Verb
	{
		public int Offset = 1;
		public bool Duplicate;

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentDocument.AddPage(CurrentPage, transaction, true, CurrentPageIndex + Offset, Duplicate) != null)
				Editor.DisplayPage(CurrentPageIndex + Offset);
		}

		public override bool IsApplicable(EditableView pnlView) => !CurrentDocument.IsPaletteWithin;
	}

	class PageMoveDown : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPageIndex >= CurrentDocument.Count - 1)
				return;
			CurrentDocument.MovePage(CurrentPageIndex, CurrentPageIndex + 2, transaction);
			Editor.DisplayPage(CurrentPageIndex + 1);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentPageIndex < CurrentDocument.Count - 1 && !CurrentDocument.IsPaletteWithin;
	}

	class PageMoveUp : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPageIndex == 0)
				return;
			CurrentDocument.MovePage(CurrentPageIndex, CurrentPageIndex - 1, transaction);
			Editor.DisplayPage(CurrentPageIndex - 1);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentPageIndex > 0 && !CurrentDocument.IsPaletteWithin;
	}

	class EditPaper : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			bool sizeChanged;
			if (frmPaper.EditPaper(CurrentPage, transaction, Editor, out sizeChanged) == DialogResult.OK)
			{
				CurrentPage.NotifySettingsChanged(Shape.EnvironmentChanges.Paper);
				pnlView.InvalidateAll();
				Editor.EnableGridSnap();
				Editor.MatchWindowToSet();
				if (sizeChanged)
					Globals.OnCurrentPageSizeChanged();
			}
		}

		public override bool IsApplicable(EditableView pnlView) => !CurrentDocument.IsPaletteWithin;
		public override bool IncludeOnContextMenu => false;
		public override bool MightOpenModalDialog => true;
	}

	class PageSize : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (frmPageSize.Display(Globals.Root.CurrentDocument, transaction) != DialogResult.OK)
				return;
			CurrentPage.NotifySettingsChanged(Shape.EnvironmentChanges.Paper);
			Globals.OnCurrentPageSizeChanged();
		}
	}

	class SetOrigin : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.StartSetOrigin();
		}

		public override bool IsApplicable(EditableView pnlView)
			=> Globals.Root.CurrentConfig.ShowArea(Config.Show_InfoMeasurement, true) && !CurrentDocument.IsPaletteWithin;
		public override bool IncludeOnContextMenu => false;
	}
}
