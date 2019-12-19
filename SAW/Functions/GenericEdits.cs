using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SAW.Functions
{
	internal static class GenericEditVerbs
	{
		public static void RegisterVerbs()
		{
			Verb.Register(Codes.Redo, new Redo());
			Verb.Register(Codes.Undo, new Undo());
			Verb.Register(Codes.Paste, new Paste());
			Verb.Register(Codes.Copy, new CopyCut() { IsCut = false });
			Verb.Register(Codes.Cut, new CopyCut() { IsCut = true });
			Verb.Register(Codes.Delete, new Delete());
		}

	}

	#region Undo

	internal class Undo : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			bool ongoing = pnlView.OngoingShape != null;
			pnlView.ConcludeOngoing();
			if (ongoing)
				return; // we cancelled the current;  better to not also undo a previous - that can look like 2 operations
			if (CurrentDocument.UndoTransactions.Count == 0)
				return;
			CurrentPage.DeselectAll(); // in case undo deletes something in selected list
			Transaction actualTransaction = CurrentDocument.UndoTransactions[CurrentDocument.UndoTransactions.Count - 1];
			CurrentDocument.UndoTransactions.RemoveAt(CurrentDocument.UndoTransactions.Count - 1);
			actualTransaction.Undo();
			pnlView.InvalidateAll();
			CurrentDocument.RedoTransactions.Add(actualTransaction);

			if (CurrentDocument.Changed != actualTransaction.ChangedAtStart)
				CurrentDocument.Changed = actualTransaction.ChangedAtStart;
			Editor.IndirectEffectsUndoRedo(actualTransaction, true);
			Shape.RestoreCaretState(actualTransaction); // Ignored unless objTransaction.CaretState is defined
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			return CurrentDocument.UndoTransactions.Count > 0;
		}

		// doesn't abandon - does that internally within Trigger (does nothing else in that case.  also cancelling the previous looks odd)
		public override bool AbandonsCurrent => false;
		public override bool IncludeOnContextMenu => false;
	}
	#endregion

	#region Redo

	internal class Redo : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentDocument.RedoTransactions.Count == 0)
				return;
			pnlView.ConcludeOngoing();
			CurrentPage.DeselectAll(); // in case undo deletes something in selected list
			Transaction actualTransaction = CurrentDocument.RedoTransactions[CurrentDocument.RedoTransactions.Count - 1];
			CurrentDocument.RedoTransactions.RemoveAt(CurrentDocument.RedoTransactions.Count - 1);
			actualTransaction.Undo();
			pnlView.InvalidateAll();
			CurrentDocument.UndoTransactions.Add(actualTransaction);

			if (!CurrentDocument.Changed)
				CurrentDocument.Changed = true;
			Editor.IndirectEffectsUndoRedo(actualTransaction, false);
		}

		public override bool IsApplicable(EditableView pnlView) => CurrentDocument.RedoTransactions.Count > 0;
		public override bool IncludeOnContextMenu => false;
	}
	#endregion

	#region Paste

	internal class Paste : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			try
			{
				DataObject data = (DataObject)Clipboard.GetDataObject();
				if (data.GetDataPresent("Splash data", false))
					PasteSplashData(transaction, pnlView);
#if SAW && !SPLASH
				else if (data.GetDataPresent("Splash scripts", false))
					CopyScripts.Paste(transaction);
				else if (data.GetDataPresent("Splash presentation", false))
					CopyPresentation.Paste(transaction, pnlView);
#endif
				else if (data.ContainsImage())
				{
					Image image = data.GetImage();
					MemoryStream strmMemory = new MemoryStream(10000);
					image.Save(strmMemory, ImageFormat.Png);
					image.Dispose();
					ImportedImage objImported = new ImportedImage(strmMemory, CurrentDocument, transaction); //= ImportedImage.CreateForPaste(strmMemory)
					transaction.Create(objImported);
					objImported.PlaceAt(pnlView.ViewableArea().Centre());
					//  strmMemory.Dispose()
					CurrentPage.AddNew(objImported, transaction);
				}
				else if (ContainsFileImage(data))
				{
					ImportedImage imported = new ImportedImage(data.GetFileDropList()[0], CurrentDocument, transaction);
					transaction.Create(imported);
					imported.PlaceAt(pnlView.ViewableArea().Centre());
					CurrentPage.AddNew(imported, transaction);
					CurrentPage.SelectOnly(imported);
				}
				else if (data.ContainsText())
				{
					string text = data.GetText();
					if (!string.IsNullOrEmpty(text))
					{
						// try and apply it to a shape - only if there is one and only one selected
						Shape shape = pnlView.TypingShape();
						if (shape != null && shape.SupportsTextLabel) // Second condition is needed because some "typing" shapes only actually do custom processing of certain keys
						{
							transaction.Edit(shape);
							shape.TextType(text);
						}
						else if (CurrentPage.SelectedCount == 1)
						{
							shape = CurrentPage.SelectedShapes[0];
							if (shape.SupportsTextLabel)
							{
								// When pasting in without the cursor, the entire current text is replaced, if any
								transaction.Edit(shape);
								if (!shape.HasText(true))
									shape.CreateLabel((Shape.TextStyleC)Editor.StyleParameterDefaultObject(Parameters.FontFace));
								// StyleParameterDefaultObject returns the object for that param - which will be all the text styles
								shape.LabelText = text.Replace("\r\n", "\r");
							}
						}
					}
				}
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				Utilities.LogSubError(ex);
				transaction.Cancel(); // must be before message box
				MessageBox.Show(Strings.Item("Paste_Failed"));
			}
		}

		private void PasteSplashData(Transaction transaction, EditableView pnlView)
		{
			DataObject data = (DataObject)Clipboard.GetDataObject();
			byte[] buffer = (byte[])data.GetData("Splash data", false);
			// will be nothing if deserialisation failed
			if (buffer == null)
			{
				Debug.Fail("Deserialisation failed");
				return;
			}
			using (MemoryStream stream = new MemoryStream(buffer, false))
			using (DataReader reader = new DataReader(stream, FileMarkers.ShapeList))
			{
				List<Datum> list = reader.ReadDataList();
				// cannot insert objects directly as that would mean pasting twice inserted 2 objects with same ID
				Mapping hashIDChanges = new Mapping();
				DatumList newList = new DatumList();
				RectangleF bounds = RectangleF.Empty; // work out existing bounds of list
				bool includesShapes = false;
				foreach (Datum datum in list)
				{
					if (datum is Shape)
					{
						// assumed that all shapes need to be created into page
						Shape create = (Shape)datum.Clone(hashIDChanges);
						transaction.Create(create);
						newList.Add(create);
						Geometry.Extend(ref bounds, create.MinimalBounds); // minimal makes, for example, the snapping of axes better
						includesShapes = true;
					}
					else if (datum is SharedBase resource)
					{
						// no need to clone these as they are effectively invariant
						if (CurrentDocument.FindExistingSharedResource<Datum>(resource.ID) == null)
						{
							transaction.Edit(CurrentDocument);
							CurrentDocument.AddSharedResource(resource);
							// if ID exists in this document it is assumed to be same object
						}
					}
					else if (datum is ButtonStyle buttonStyle)
					{
						// as Shape, but don't add to page
						// if IsShared it is not cloned.  Means styles in multi docs will share ID, which is good as further pasting in will reuse existing style
						// does need to be added to document share
						ButtonStyle create;
						if (buttonStyle.IsShared)
						{
							create = buttonStyle;
							if (CurrentDocument.GetButtonStyle(create.ID) == null)
							{
								CurrentDocument.AddButtonStyle(create); // is new to this doc - either by cloning, or
								transaction.Create(create);
							}
						}
						else
						{
							create = (ButtonStyle)buttonStyle.Clone(hashIDChanges);
							transaction.Create(create);
						}
						newList.Add(create);
					}
					else
						Debug.Fail("Datum not processed by by Paste: " + datum.TypeByte);
				}
				// want to centre shapes within current area, rather than pasting at original position (which leaves them on top of each other and largely invisible)
				IShapeContainer container = CurrentPage;
				if (includesShapes)
				{
					if (CurrentPage.SelectedCount == 1 && CurrentPage.SelectedShapes.First().AsContainer != null)
					{
						container = CurrentPage.SelectedShapes.First().AsContainer;
						// but in SAW don't paste automatically into an empty container - chances are it's the item which was just copied!
						// instead paste into that container's parent
						if (!container.Any())
							container = (container as Shape).Container.AsParentContainer;
					}
					PointF target = pnlView.ViewableArea().Centre();
					// Or use cursor position if better
					PointF cursor = pnlView.CursorPositionLocal;
					// Automatically selects the drawing cursor if split cursors are used
					// or the container
					if (((Datum)container).TypeByte == (byte)Shape.Shapes.Container)
						target = ((Shape)container).Bounds.Centre();
					else if (cursor.X > 0 && cursor.Y > 0 && cursor.X < pnlView.Width && cursor.Y < pnlView.Height)
					{
						cursor = pnlView.MouseToData(cursor.ToPoint());
						// And also need to check the cursor is within the page.  the above just checked it was within the control (there may be some dead area displayed)
						if (cursor.X < pnlView.ViewableArea().Width && cursor.Y < pnlView.ViewableArea().Height)
							target = cursor;
					}
					target.X -= bounds.Width / 2;
					target.Y -= bounds.Height / 2; // where we want the top left to be
					if (pnlView.SnapMode == Shape.SnapModes.Grid)
						target = CurrentPage.Paper.SnapPoint2(target);
					var transform = new TransformMove(target.X - bounds.X, target.Y - bounds.Y);
					transaction.Edit((Datum)container);
					foreach (Shape shape in newList.Values.OfType<Shape>()) // of type needed in case other stuff was mixed in with it
					{
						container.Contents.Add(shape);
						shape.Parent = container;
						shape.ApplyTransformation(transform);
					}
					container.FinishedModifyingContents(transaction);
				}
				foreach (Datum shape in newList.Values)
				{
					try
					{
						// order changed
						shape.UpdateReferencesObjectsCreated(CurrentDocument, reader);
						shape.UpdateReferencesIDsChanged(hashIDChanges, Globals.Root.CurrentDocument);
					}
					catch (Exception ex)
					{
						Utilities.LogSubError(ex);
					}
				}
				List<Shape> newShapes = newList.Values.OfType<Shape>().ToList();
				foreach (Shape shape in newShapes)
				{
					CurrentPage.UpdateIntersectionsWith(shape, true);
				}
				if (newShapes.Count > 0 && container == CurrentPage) // don't select the shapes when pasting into a container - leaves the container selected, which allows for more pastes
					CurrentPage.SelectOnly(newShapes);
				pnlView.InvalidateData(CurrentPage.SelectedRefreshBoundary(), StaticView.InvalidationBuffer.All);
			}
		}

		private static bool ContainsFileImage(DataObject data)
		{
			// Checks in a clipboard data object contains a filename, which is an image
			if (!data.GetDataPresent(DataFormats.FileDrop))
				return false;
			StringCollection files = data.GetFileDropList();
			if (files.Count == 0)
				return false;
			string file = files[0];
			return Utilities.IsImageFile(file);
		}

		public override bool AbandonsCurrent
		{
			get
			{
				DataObject objData = (DataObject)Clipboard.GetDataObject();
				if (!objData.GetDataPresent("Splash data", false) && !objData.ContainsImage() && !ContainsFileImage(objData) && objData.ContainsText())
					// pasting text only shouldn't clear the current action, which might be typing into a shape
					return false;
				return true;
			}
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			DataObject data = (DataObject)Clipboard.GetDataObject();
			if (data != null)
			{
				if (data.GetDataPresent("Splash shapes", false) || data.ContainsImage() || ContainsFileImage(data))
					return true;
#if SAW && !SPLASH
				if (data.GetDataPresent("Splash scripts", false))
					return CopyScripts.PasteApplicable(data);
				if (data.GetDataPresent("Splash presentation", false))
					return CopyPresentation.PasteApplicable(data);
#endif
				if (data.ContainsText())
				{
					if (CurrentPage.SelectedCount == 1 && CurrentPage.SelectedShapes[0].SupportsTextLabel)
						return true;
					Shape shape = pnlView.TypingShape();
					if (shape != null && shape.SupportsTextLabel)
						return true; // Second condition is needed because some "typing" shapes only actually do custom processing of certain keys
				}
			}
			return false;
		}

		public override bool HideFromContextMenuIfUnavailable => false;
	}
	#endregion

	#region Copy and Cut

	internal class CopyCut : Verb
	{
		public bool IsCut;

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentPage == null || CurrentPage.SelectedCount == 0)
				return;
			// the items to put on the clipboard
			//  even if cutting, we put a different copy of the objects on the clipboard with different IDs (in case the user undoes which would restore the original objects)
			DatumList hash = Datum.CloneList(CurrentPage.SelectedShapes, Globals.Root.CurrentDocument); // copies shapes, changes IDs, updates links and includes references as needed

			// write the list of shapes into a byte buffer; this way we control the serialisation
			DataObject data = new DataObject();
			using (MemoryStream buffer = new MemoryStream(1000))
			{
				using (DataWriter writer = new DataWriter(buffer, FileMarkers.ShapeList))
				{
					List<Datum> list = new List<Datum>();
					list.AddRange(hash.Values);
					writer.WriteDatumList(list);
					data.SetData("Splash data", false, buffer.GetBuffer());
				}
			}

			Bitmap bitmap = (Bitmap)pnlView.CreateImage(false, false, true);
			// will have white background - transparent gets lost somewhere within clipboard
			data.SetImage(bitmap);
			//objBitmap.Dispose()' doesn't work if this done

			// http://stackoverflow.com/questions/35045500/copy-svg-image-stored-as-xml-string-to-windows-clipboard-as-image-svgxml-mime-t
			try
			{
				var strSVG = pnlView.CreateSVG(false, true); // True)
				byte[] aBytes = Encoding.UTF8.GetBytes(strSVG);
				MemoryStream objStream = new MemoryStream(aBytes);
				data.SetData("image/svg+xml", objStream);
				//System.IO.File.WriteAllText("g:\temp\svg.svg", strSVG)
			}
			catch (UserException u)
			{
				Globals.Root.Log.WriteLine("Cannot copy as SVG: " + u.Message);
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
			}

			//check for text
			string text = "";
			foreach (Shape shape in CurrentPage.SelectedShapes)
			{
				if (shape.HasText(true) && !String.IsNullOrEmpty(shape.LabelText))
				{
					text = shape.LabelText;
					break; // stop once valid text found
				}
			}
			if (!string.IsNullOrEmpty(text))
				data.SetText(text);

			// store result on clipboard
			Clipboard.SetDataObject(data);

			if (IsCut)
				CurrentPage.DeleteSelected(transaction);

			// This works to put the image on the clipboard as a metafile, but it ONLY appears as a metafile and the above ones are lost.  drat.
			// for putting metafiles on clipboard: http://support.microsoft.com/kb/323530
			//Dim objImage As Image = pnlView.CreateImage(True, False)
			//ClipboardMetafileHelper.PutEnhMetafileOnClipboard(Me.Handle, objImage)
			//objImage.Dispose()
			// saving as a file first, reloading and adding a byte array or memorystream didn't work either
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (pnlView.OngoingShape != null)
				return false; // if part way through a new shape can't delete any selected one (in practice probably only when doing Arcs on NaL otherwise there wouldn't be any current selection)
			return CurrentPage.SelectedCount > 0;
		}

		public override bool HideFromContextMenuIfUnavailable => false;
	}
	#endregion

	#region Delete

	internal class Delete : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			CurrentPage.DeleteSelected(transaction);
		}
		public override bool IsApplicable(EditableView pnlView)
		{
			if (pnlView.OngoingShape != null)
				return false; // if part way through a new shape can't delete any selected one (in practice probably only when doing Arcs on NaL otherwise there wouldn't be any current selection)
			return CurrentPage.SelectedCount > 0 ;
		}

		public override bool HideFromContextMenuIfUnavailable => false;
	}
	#endregion

}
