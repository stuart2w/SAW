using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SAW.CCF;
using SAW.GUI.Dialogs;
using SAW.Shapes;

namespace SAW.Functions
{
	internal static class SAWVerbs
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.HidePopup, new ShowHidePopup() { Show = false });
			Verb.Register(Codes.ShowPopup, new ShowHidePopup() { Show = true });
			Verb.Register(Codes.ShowAllPopups, new ShowHideAllPopups() { Show = true });
			Verb.Register(Codes.HideAllPopups, new ShowHideAllPopups() { Show = false });
			Verb.Register(Codes.SetPromptItem, new SetPromptItem());
			Verb.Register(Codes.ShowGraphicSelection, new ShowHideGraphicSelection() { Show = true });
			Verb.Register(Codes.HideGraphicSelection, new ShowHideGraphicSelection() { Show = false });
			Verb.Register(Codes.ShowTextSelection, new ShowHideTextSelection() { Show = true });
			Verb.Register(Codes.HideTextSelection, new ShowHideTextSelection() { Show = false });
			Verb.Register(Codes.GridWizard, new GridWizard());
			Verb.Register(Codes.ViewPredictionWords, new ViewPredictionWords());
			Verb.Register(Codes.EditItemBounds, new EditItemBounds());
			Verb.Register(Codes.EditWindowBounds, new EditWindowBounds());
			Verb.Register(Codes.CopyScripts, new CopyScripts());
			Verb.Register(Codes.CopyPresentation, new CopyPresentation());
			Verb.Register(Codes.CCFUpdate, new CCFUpdate());
			Verb.Register(Codes.SaveSettings, new SaveSettings());
			Verb.Register(Codes.LoadSettings, new LoadSettings());
			Verb.Register(Codes.ImportIRM, new ImportIRM());
			Verb.Register(Codes.ExportIRM, new ExportIRM());
			Verb.Register(Codes.MakeActive, new MakeActive());
			Verb.Register(Codes.MakeInactive, new MakeInactive());
			Verb.Register(Codes.SAWManual, (source, pnlView, Transaction) => { Globals.Root.ShellOpen(Path.Combine(Globals.Root.InternalFolder, "SAW Manual.pdf")); });
			Verb.Register(Codes.YoctoTest, (SourceFilter, pnlView, Transaction) => { frmYocto.Display(); }).MightOpenDialogValue = true;
		}

	}

	#region Pop ups

	internal class ShowHidePopup : Verb
	{
		public bool Show;
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			foreach (var s in CurrentPage.SelectedShapes.OfType<Scriptable>().Where(s => s.Popup && s.Shown != Show))
			{
				// only called for items that need changing
				transaction.Edit(s);
				s.Shown = Show;
			}
		}

		public override bool AutoRefreshAfterTrigger => true;
		public override bool IsApplicable(EditableView pnlView)
			=> CurrentPage.SelectedShapes.OfType<Scriptable>().Any(s => s.Popup && s.Shown != Show);
	}

	internal class ShowHideAllPopups : Verb
	{
		public bool Show;

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			CurrentPage.Iterate(s =>
			{
				var scriptable = s as Scriptable;
				if ((scriptable?.Popup ?? false) && scriptable.Shown != Show)
				{
					transaction.Edit(scriptable);
					scriptable.Shown = Show;
				}
			});
		}

		public override bool AutoRefreshAfterTrigger => true;
	}
	#endregion

	#region SetPromptItem

	internal class SetPromptItem : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage.SelectedCount != 1)
				return;
			var selected = CurrentPage.SelectedShapes.FirstOrDefault();
			if (!(selected is Scriptable))
				return;
			int helpID = (selected as Scriptable).SAWID;
			if (GUIUtilities.QuestionBox(Strings.Item("SAW_Edit_SetHelpConfirm", helpID.ToString()), MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			transaction.Edit(CurrentDocument);
			CurrentPage.HelpSAWID = helpID;
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentPage.SelectedCount == 1 && CurrentPage.SelectedShapes.FirstOrDefault() is Scriptable;
	}
	#endregion

	#region Show/Hide text or graphic

	internal class ShowHideGraphicSelection : Verb
	{
		public bool Show;
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			foreach (var item in (from Scriptable s in CurrentPage.SelectedShapes.OfType<Scriptable>() where s.Element is Item select (s.Element as Item)))
			{
				transaction.Edit(item);
				item.GraphicShown = Show;
			}
		}

		public override bool AutoRefreshAfterTrigger => true;
		public override bool IsApplicable(EditableView pnlView) => CurrentPage.SelectedCount > 0;
	}

	internal class ShowHideTextSelection : Verb
	{
		public bool Show;
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			foreach (var item in (from Scriptable s in CurrentPage.SelectedShapes.OfType<Scriptable>() where s.Element is Item select (s.Element as Item)))
			{
				transaction.Edit(item);
				item.TextShown = (Show);
			}
		}

		public override bool AutoRefreshAfterTrigger => true;
		public override bool IsApplicable(EditableView pnlView) => CurrentPage.SelectedCount > 0;
	}
	#endregion

	#region Prediction words

	internal class ViewPredictionWords : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			try
			{ BladeWrapper.Blade.EditUserWordList(); }
			catch (Exception e)
			{
				Utilities.LogSubError(e);
				MessageBox.Show(e.Message);
			}
		}
	}
	#endregion

	#region Grid wizard

	internal class GridWizard : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			frmGridWizard.Display((CurrentPage.SelectedShapes.FirstOrDefault() as IShapeContainer) ?? CurrentPage, CurrentDocument);
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			return CurrentPage.SelectedCount == 0 || (CurrentPage.SelectedCount == 1 && CurrentPage.SelectedShapes.All(s => s is Scriptable && ((Scriptable)s).Element is Item));
			// All() function is odd after checking there is 1, but it's just a neat way of embedding multiple conditions without repeated calls to .SelectShapes.FirstOrDefault
		}

	}
	#endregion

	#region Item/Window bounds

	internal class EditItemBounds : Verb
	{

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			var selected = SelectedItem;
			if (selected == null)
				return;
			frmEditBounds.EditItemBounds(selected);
		}

		public override bool IsApplicable(EditableView pnlView) => SelectedItem != null;

		/// <summary>Returns selected SAW Item, if only one selected (unwrapping the Selectable as needed)</summary>
		private static Item SelectedItem
		{
			get
			{
				var list = Globals.Root.CurrentPage.SelectedShapes;
				if (list.Count != 1)
					return null;
				var first = list.First();
				first = (first as Scriptable)?.Element ?? first; // unwrap to the Item if it's scriptable
				return (first as Item); // Item or null
			}
		}

	}

	internal class EditWindowBounds : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentDocument.IsPaletteWithin)
			{ // not appropriate for palettes - and will fail as EditWindowBounds tries to access the SAWHeader
				(new PageSize()).Trigger(source, pnlView, transaction);
				return;
			}
			frmEditBounds.EditWindowBounds();
		}

	}

	#endregion

	#region Copy and Cut

	internal class CopyScripts : Verb
	{

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Scriptable scriptable = pnlView.CurrentPage.SelectedShapes.First() as Scriptable;
			if (scriptable == null)
				return;
			// write the list of shapes into a byte buffer; this way we control the serialisation
			DataObject data = new DataObject();
			using (MemoryStream buffer = new MemoryStream(1000))
			using (DataWriter writer = new DataWriter(buffer, (FileMarkers)Shape.Shapes.Scriptable))
			{
				writer.Write(scriptable);
				data.SetData("Splash scripts", false, buffer.GetBuffer());
			}

			IndentStringBuilder output = new IndentStringBuilder();
			// store as text as well
			foreach (Scriptable.ScriptTypes type in Enum.GetValues(typeof(Scriptable.ScriptTypes)))
			{
				Script script = scriptable.GetScript(type);
				script?.WriteExportText(output, Strings.Item("SAW_ScriptType_" + type));
			}

			if (output.Length > 0)
				data.SetText(output.ToString());

			// store result on clipboard
			Clipboard.SetDataObject(data);

		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (pnlView.OngoingShape != null)
				return false;
			return CurrentPage != null && CurrentPage.SelectedCount == 1 && CurrentPage.SelectedShapes.First() is Scriptable;
		}

		#region Static implementation of pasting back in again

		/// <summary>Used by the Paste verb to do the paste in this case</summary>
		public static void Paste(Transaction transaction)
		{
			DataObject data = (DataObject)Clipboard.GetDataObject();
			byte[] buffer = (byte[])data.GetData("Splash scripts", false);
			if (buffer == null)
			{
				Debug.Fail("Deserialisation failed");
				return;
			}
			using (MemoryStream stream = new MemoryStream(buffer, false))
			using (DataReader reader = new DataReader(stream, (FileMarkers)Shape.Shapes.Scriptable))
			{
				Scriptable copied = (Scriptable)reader.ReadData((FileMarkers)Shape.Shapes.Scriptable);
				foreach (Scriptable target in CurrentPage.SelectedShapes.OfType<Scriptable>())
				{
					transaction.Edit(target);
					target.RepeatTimeout = copied.RepeatTimeout;
					foreach (Scriptable.ScriptTypes type in Enum.GetValues(typeof(Scriptable.ScriptTypes)))
					{
						target.SetScript(type, copied.GetScript(type));
					}
				}
			}
		}

		public static bool PasteApplicable(DataObject clipboardData)
		{
			if (CurrentPage == null)
				return false;
			if (!CurrentPage.SelectedShapes.Any())
				return false;
			return CurrentPage.SelectedShapes.All(s => s is Scriptable);
		}

		#endregion

	}

	internal class CopyPresentation : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Item item = SelectedItem;
			if (item == null)
				return;
			// write the list of shapes into a byte buffer; this way we control the serialisation
			DataObject data = new DataObject();
			using (MemoryStream buffer = new MemoryStream(1000))
			using (DataWriter writer = new DataWriter(buffer, (FileMarkers)Shape.Shapes.SAWItem))
			{
				writer.Write(item);
				data.SetData("Splash presentation", false, buffer.GetBuffer());
			}
			// No text is stored in this case

			Clipboard.SetDataObject(data);
			Debug.WriteLine("Copied presentation with image: " + item.Image.ID);
		}

		/// <summary>Returns the currently selected Item, unwrapping the Scriptable if that is selected, or null if nothing is selected, or something else is selected</summary>
		private static Item SelectedItem
		{
			get
			{
				Shape item = Globals.Root.CurrentPage?.SelectedShapes.FirstOrDefault();
				item = (item as Scriptable)?.Element ?? item;
				return item as Item;
			}
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (pnlView.OngoingShape != null)
				return false;
			return CurrentPage != null && SelectedItem != null;
		}

		#region Static implementation of pasting back in again

		/// <summary>Used by the Paste verb to do the paste in this case</summary>
		public static void Paste(Transaction transaction, EditableView pnlView)
		{
			DataObject data = (DataObject)Clipboard.GetDataObject();
			byte[] buffer = (byte[])data.GetData("Splash presentation", false);
			if (buffer == null)
			{
				Debug.Fail("Deserialisation failed");
				return;
			}
			using (MemoryStream stream = new MemoryStream(buffer, false))
			using (DataReader reader = new DataReader(stream, (FileMarkers)Shape.Shapes.SAWItem))
			{
				Item copied = (Item)reader.ReadData((FileMarkers)Shape.Shapes.SAWItem);
				copied.UpdateReferencesObjectsCreated(CurrentDocument, reader);
				//copied.UpdateReferencesIDsChanged(hashIDChanges, Globals.Root.CurrentDocument);
				foreach (Shape target in CurrentPage.SelectedShapes)
				{
					Item itemTarget = (Item)((target as Scriptable)?.Element ?? target); // unwraps the Scriptable if that was selected
					transaction.Edit(itemTarget);
					if (string.IsNullOrEmpty(itemTarget.LabelText))
						itemTarget.LabelText = copied.LabelText;
					itemTarget.FillStyle?.CopyFrom(copied.FillStyle);
					itemTarget.LineStyle?.CopyFrom(copied.LineStyle);
					itemTarget.TextStyle?.CopyFrom(copied.TextStyle);
					itemTarget.CopyPresentationFrom(copied, true);
					Scriptable scriptableTarget = target as Scriptable ?? (Scriptable)(target as Item).Parent;
					scriptableTarget.CopyPresentationFrom((Scriptable)copied.Parent, true);
				}
			}
			pnlView.InvalidateData(CurrentPage.SelectedRefreshBoundary(), StaticView.InvalidationBuffer.All);
		}

		public static bool PasteApplicable(DataObject clipboardData)
		{
			if (CurrentPage == null)
				return false;
			if (!CurrentPage.SelectedShapes.Any())
				return false;
			return CurrentPage.SelectedShapes.All(s => s is Item || (s as Scriptable)?.Element is Item); // each selected item must either be a SAWItem or a scriptable containing this
		}

		#endregion

	}

	#endregion

	#region CCF

	/// <summary>Update entire selection set using Concept Coding Framework</summary>
	internal class CCFUpdate : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			MultiUpdate update = new MultiUpdate();
			if (!frmMultiUpdateOptions.Display(CurrentPage.SelectedShapes.OfType<Item>().Any(), update))
				return;
			IEnumerable<Item> list;
			if (update.OnlySelected)
				list = CurrentPage.SelectedShapes.OfType<Item>();
			else
				list = Shape.FlattenList(CurrentPage.Shapes.ToList(), Shape.FlatListPurpose.Translate).OfType<Item>();
			update.Start();
			try
			{
				foreach (Item item in list)
				{
					string result = update.DoLookup(item.LabelText, item.ConceptID);
					if (string.IsNullOrEmpty(result))
						continue;
					transaction.Edit(item);
					switch (update.PerformFunction)
					{
						case MultiUpdate.Functions.ChangeSymbols:
							string imagePath = Path.Combine(Connection.LocalPath, result);
							SharedImage image = (SharedImage)Globals.Root.CurrentDocument.AddSharedResourceFromFile(imagePath, transaction);
							item.Image = new SharedReference<SharedImage>(image);
							item.ImageName = Path.GetFileName(imagePath);
							break;
						case MultiUpdate.Functions.ChangeText:
							item.LabelText = result;
							if ((item.Parent is Scriptable scriptable) && scriptable.OutputAsDisplay)
								scriptable.OutputText = result;
							break;
						case MultiUpdate.Functions.RemoveInfo:
							item.ConceptID = null;
							break;
						default: throw new ArgumentException("update.PerformFunction");
					}
				}
			}
			finally
			{
				update.Completed();
			}
		}

		public override bool AutoRefreshAfterTrigger => true;

		/// <summary>This is disabled in advanced graphics mode to reduce clutter (and it seems very unlikely it would be used with a very graphical set)</summary>
		public override bool IsApplicable(EditableView pnlView) => !Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics);

	}

	#endregion

	#region Settings

	public class SaveSettings : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string existing = "";
			if (Globals.Root.ManualSettingsFile != null)
				existing = Path.GetFileName(Globals.Root.ManualSettingsFile);
			string file = FileDialog.ShowSave(FileDialog.Context.OtherUserDoc, ExtensionFilter, existing);
			if (string.IsNullOrEmpty(file))
				return;
			Globals.Root.ManualSettingsFile = file;
			Config.UserUser.SaveTo(file);
		}

		/// <summary>File dialog filter for config files. Based on Config.Extension</summary>
		public static string ExtensionFilter => "*" + Config.Extension + "|*" + Config.Extension;
		public override bool AbandonsCurrent => true;
		public override bool IncludeOnContextMenu => false;

	}

	public class LoadSettings : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string existing = "";
			if (Globals.Root.ManualSettingsFile != null)
				existing = Path.GetFileName(Globals.Root.ManualSettingsFile);
			string file = FileDialog.ShowOpen(FileDialog.Context.OtherUserDoc, SaveSettings.ExtensionFilter, existing);
			if (string.IsNullOrEmpty(file))
				return;
			Globals.Root.ManualSettingsFile = file;
			Globals.Root.SaveUserConfigs();
			try
			{
				var config = Config.FromFile(file, Config.Levels.User);
				config.EnsureUserResources();
				Config.UserUser = config;
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
				MessageBox.Show(ex.Message);
			}
			Globals.OnSettingsChanged();
		}

		public override bool AbandonsCurrent => true;
		public override bool IncludeOnContextMenu => false;

	}

	#endregion

	#region Make (in)active

	internal class MakeActive : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			IShapeContainer container = CurrentPage.SelectionContainer();
			if (container == null)
			{
				MessageBox.Show(Strings.Item("Container_Mismatch"));
				return;
			}

			transaction.Edit((Datum)container);
			List<Shape> created = new List<Shape>();
			int nextID = (from p in Globals.Root.CurrentDocument.Pages select p.FindHighestUsedID()).Max() + 1;
			foreach (Shape s in CurrentPage.SelectedShapes)
			{
				transaction.Edit(s);
				Scriptable scriptable = new Scriptable(s);
				transaction.Create(scriptable);
				scriptable.Parent = container;
				scriptable.SAWID = nextID++;
				container.Contents.Remove(s);
				container.Contents.Add(scriptable);
				// note this makes no attempt to maintain Z order.  Edited items will be brought to the front (but are likely to maintain relative order)
				created.Add(scriptable);
			}
			container.FinishedModifyingContents(transaction);
			// it should still be the contained elements which are listed as selected I think
			CurrentPage.SelectOnly(created);
		}

		public override bool HideFromContextMenuIfUnavailable => true;

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount == 0)
				return false;
			if (CurrentPage.SelectionContainer() == null) // Trigger requires that all items are in same parent
				return false;
			return CurrentPage.SelectedShapes.All(s => !(s is Scriptable) && !(s.Parent is Scriptable));
		}

	}

	internal class MakeInactive : Verb
	{
		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			IShapeContainer container = CurrentPage.SelectionContainer();
			if (container == null)
			{
				MessageBox.Show(Strings.Item("Container_Mismatch"));
				return;
			}

			transaction.Edit((Datum)container);
			List<Shape> moved = new List<Shape>();
			foreach (Scriptable scriptable in CurrentPage.SelectedShapes)
			{
				transaction.Delete(scriptable);
				Shape shape = scriptable.Element;
				transaction.Edit(shape);
				shape.Parent = container;
				container.Contents.Remove(scriptable);
				container.Contents.Add(shape);
				moved.Add(shape);
			}
			container.FinishedModifyingContents(transaction);
			CurrentPage.SelectOnly(moved);

		}

		public override bool HideFromContextMenuIfUnavailable => true;

		public override bool IsApplicable(EditableView pnlView)
		{
			if (CurrentPage.SelectedCount == 0)
				return false;
			if (CurrentPage.SelectionContainer() == null) // Trigger requires that all items are in same parent
				return false;
			// doesn't allow traditional SAW buttons to make made inert
			return CurrentPage.SelectedShapes.All(s => (s is Scriptable scriptable) && !(scriptable.Element is Item));
		}
	}

	#endregion

}
