using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace SAW.Functions
{
	public static class DocumentVerbs
	{
		public static void RegisterDocumentVerbs()
		{
			Verb.Register(Codes.NewDocument, new NewDocument());
			Verb.Register(Codes.Open, new OpenDocument());
			Verb.Register(Codes.Save, new SaveDocument());
			Verb.Register(Codes.SaveAs, new SaveDocument());
			Verb.Register(Codes.SaveAsWorksheet, new SaveDocument());
			Verb.Register(Codes.Print, new PrintDocument() { Direct = false });
			Verb.Register(Codes.PrintDirectly, new PrintDocument() { Direct = true });
			Verb.Register(Codes.PrintPreview, new PrintDocument() { Preview = true });
			Verb.Register(Codes.ExportEMF, new ExportEMF());
			Verb.Register(Codes.ExportSVG, new ExportSVG());
			Verb.Register(Codes.ExportJPEG, new ExportImage() { extension = "JPEG", format = ImageFormat.Jpeg });
			Verb.Register(Codes.ExportPNG, new ExportImage() { extension = "PNG", format = ImageFormat.Png });
			Verb.Register(Codes.ExportPageAsDocument, new ExportPageAsDocument());
			Verb.Register(Codes.ImportImage, new ImportImage());
			Verb.Register(Codes.Quit, (source, view, transaction) =>
			{
				if (Globals.Root.Editor.CheckDiscard(Globals.Root.CurrentDocument))
					Globals.Root.CloseApplication();
			});
			Verb.Register(Codes.CloseDocument, new CloseDocument());

			Verb.Register(Codes.Start, (source, pnlView, tx) => { Globals.Root.Editor.DoStartVerb(); });
			Verb.Register(Codes.StartupScript, (source, pnlView, tx) => { frmEditStartupScript.Display(Globals.Root.Editor); });
			Verb.Register(Codes.DefaultItemScript, (source, pnlView, tx) => { frmEditDefaultScripts.Display(Codes.DefaultItemScript, Globals.Root.Editor); });
			Verb.Register(Codes.DefaultEscapeScript, (source, pnlView, tx) => { frmEditDefaultScripts.Display(Codes.DefaultEscapeScript, Globals.Root.Editor); });
			Verb.Register(Codes.DefaultGroupScript, (source, pnlView, tx) => { frmEditDefaultScripts.Display(Codes.DefaultGroupScript, Globals.Root.Editor); });
			Verb.Register(Codes.ExportAsText, new ExportAsText());
			Verb.Register(Codes.UserUser, new UserUser());
			Verb.Register(Codes.UserTeacher, new UserTeacher());

			Verb.Register(Codes.Group, new NullVerb());
			Verb.Register(Codes.Ungroup, new NullVerb());

		}

	}

	#region Save
	public class SaveDocument : Verb
	{

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (Code == Codes.SaveAs)
				SaveAs(false, pnlView);
			else if (Code == Codes.SaveAsWorksheet)
				SaveAs(true, pnlView);
			else
				Save(pnlView);
		}

		public static void Save(EditableView pnlView)
		{
			if (CurrentDocument.IsPaletteWithin)
			{
				MessageBox.Show("Cannot_Save_Palette");
				return;
			}
			if (pnlView.Visible && pnlView.CurrentPage != null) // second condition essential.  If this is triggered by save question when closing application in run mode, it would be null and GetDocumentScreenCoords would crash
				CurrentDocument.SAWHeader.SetWindowBounds(pnlView.GetDocumentScreenCoords());
			if (CurrentDocument.NoSaveOverOriginalFile || string.IsNullOrEmpty(CurrentDocument.Filename)) // NoSaveOverOriginalFile will be true for SAW6 files
				SaveAs(false, pnlView);
			else
				SaveFile(pnlView); // so this can only happen for SAW7 format.  SaveAs handles saving in the old format

		}

		public static void SaveAs(bool worksheet, EditableView pnlView)
		{
			var strFilename = FileDialog.ShowSave(FileDialog.Context.Document, "SAW 7|*" + Document.StandardExtension + "|SAW 6|*.sss", Path.GetFileName(CurrentDocument.FilenameWithExtension(Document.StandardExtension)));
			if (string.IsNullOrEmpty(strFilename))
				return;
			CurrentDocument.SAWHeader.SetWindowBounds(pnlView.GetDocumentScreenCoords());
			if (strFilename.ToLower().EndsWith(".sss"))
			{
				using (var op = new Globals.Operation("[Save_SAW6]"))
				{
					var oldDoc = new SAW6Doc(CurrentDocument);

					using (var writer = new ArchiveWriter(strFilename, oldDoc.m_Header.Version))
					{
						oldDoc.Write(writer);
					}
					op.ConfirmSuccess(true);
				}
			}
			else
			{
				if (!CurrentDocument.IsPaletteWithin)
					CurrentDocument.Filename = strFilename;
				CurrentDocument.DisplayName = ""; // otherwise the original worksheet name remains in the title bar, rather than the filename
				SaveFile(pnlView);
			}

		}

		public static void SaveFile(EditableView pnlView)
		{
			pnlView.ConcludeOngoing();
			Bitmap thumbnail = CurrentDocument.Page(0).GenerateThumbnail2(Page.FILEDOCUMENTTHUMBNAILSIZE, CurrentDocument.ApproxUnitScale, 96);
			try
			{
				using (DataWriter writer = new DataWriter(CurrentDocument.Filename, FileMarkers.Splash, thumbnail: thumbnail))
				{
					writer.Write(CurrentDocument);
				}
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				MessageBox.Show(Strings.Item("Save_Failed").Replace("%0", ex.Message), Strings.Item("App"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return; // functions can detect this failed (e.g. save before close)by detecting now m_Document.Changed is still True
			}
			CurrentDocument.Changed = false;
			Config.UserUser.RememberFile(CurrentDocument.Filename);
			Editor.SetWindowTitle(); // in case filename was changed
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			return !CurrentDocument.IsPaletteWithin;
		}

		public override bool MightOpenModalDialog
		{ get { return true; } }

	}
	#endregion

	#region ExportAsText
	internal class ExportAsText : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string file = FileDialog.ShowSave(FileDialog.Context.UserExport, "*.txt|*.txt");
			if (string.IsNullOrEmpty(file))
				return;
			IndentStringBuilder output = new IndentStringBuilder();
			CurrentDocument.WriteExportText(output);
			File.WriteAllText(file, output.ToString());
		}

	}
	#endregion

	#region User
	internal class UserUser : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (CurrentDocument.IsPaletteWithin)
			{
				MessageBox.Show(Strings.Item("Palette_CannotSwitchUser"));
				return;
			}
			CurrentDocument.SAWHeader.SetWindowBounds(pnlView.GetDocumentScreenCoords());
			Config.UserCurrent.StorePalettePositions();
			Globals.Root.User = Users.User;
			Globals.Root.ShowRunScreen();
		}
	}

	internal class UserTeacher : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Globals.Root.User = Users.Editor;
		}
	}
	#endregion

	#region New
	public class NewDocument : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Document document = new Document(false); // must be before IF below for first branch
			document.Page(0).SetSize(Globals.Root.CurrentConfig.ReadSize(Config.Page_Size, Config.Page_Size_Default));
			if (CurrentDocument != null)
			{
				document.ActivityID = CurrentDocument.ActivityID;
				document.Page(0).SetSize(CurrentDocument.Page(0).PhysicalSize, CurrentDocument.Page(0).Margin);
			}
			document.SetDefaultGridAndSnapFromActivity();
			if (CurrentDocument.UserSettings != null && (CurrentDocument.UserSettings.Values.Count > 0) ||
				CurrentDocument.BothSettings != null && (CurrentDocument.BothSettings.Values.Count > 0))
			{
				if (GUIUtilities.QuestionBox(Strings.Item("NewDocument_CopySettings"), MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					Transaction objTransaction = new Transaction(); // settings methods below require a transaction, but we won't store this for undo
					if (CurrentDocument.UserSettings != null)
						document.GetCreateUserSettings(objTransaction).CopyFrom(CurrentDocument.UserSettings, Datum.CopyDepth.Duplicate, Mapping.Ignore);
					if (CurrentDocument.BothSettings != null)
						document.GetCreateBothSettings(objTransaction).CopyFrom(CurrentDocument.BothSettings, Datum.CopyDepth.Duplicate, Mapping.Ignore);
				}
			}
			if (Globals.Root.CurrentConfig.ReadBoolean(Config.Multiple_Documents) && !CurrentDocument.IsEmpty)
			{
				// create new index - current doc kept even if saved, as long as not totally empty
				Globals.Root.AddNewDocument(document);
			}
			else
			{
				if (!Globals.Root.Editor.CheckDiscardCurrent(false))
					return;
				Globals.Root.CurrentDocument = document;
			}
			Document.NextUntitledIndex += 1;
		}

	}
	#endregion

	#region Open
	public class OpenDocument : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.ConcludeOngoing();
			string path = FileDialog.ShowOpen(FileDialog.Context.Document, Document.LoadFilter());
			if (string.IsNullOrEmpty(path))
				return;
			Globals.Root.OpenFile(path);
		}

		public override bool MightOpenModalDialog => true;

	}
	#endregion

	#region Print
	/// <summary>Implements print (including directly) and preview.  Fields determine which is used </summary>
	internal class PrintDocument : Verb
	{

		private int m_PrintPageIndex; // the index of the next page to be printed 0-based
		private System.Drawing.Printing.PrintDocument m_PrintDocument = null;
		internal static bool PrintMeasures;
		public bool Direct;
		public bool Preview;

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (Preview)
				PrintPreview();
			else
				Print(pnlView);
		}

		private void Print(EditableView pnlView)
		{
			pnlView.ConcludeOngoing();
			try
			{
				m_PrintDocument = new System.Drawing.Printing.PrintDocument();
				m_PrintDocument.PrinterSettings.MaximumPage = CurrentDocument.Count;
				m_PrintDocument.PrinterSettings.MinimumPage = 1;
				m_PrintDocument.PrinterSettings.FromPage = 1;
				m_PrintDocument.PrinterSettings.ToPage = CurrentDocument.Count;
				m_PrintDocument.PrinterSettings.PrintRange = PrintRange.CurrentPage;
				m_PrintDocument.DefaultPageSettings.Landscape = CurrentPage.IsLandscape;
				m_PrintDocument.BeginPrint += m_PrintDocument_BeginPrint;
				m_PrintDocument.PrintPage += PrintDocument_PrintPage;
				if (!Direct)
				{
					var dlgPrint = new PrintDialog
					{
						AllowSelection = CurrentPage.SelectedCount > 0,
						Document = m_PrintDocument,
						AllowCurrentPage = true,
						AllowSomePages = true,
						UseEXDialog = true
					};
					if (dlgPrint.ShowDialog() != DialogResult.OK)
						return;
				}
				PrintMeasures = false;
				m_PrintDocument.Print();
			}
			catch (Exception EX)
			{
				Utilities.LogSubError(EX);
				MessageBox.Show(Strings.Item("Print_Error") + " " + EX.Message);
			}
			finally
			{
				m_PrintDocument.Dispose();
				m_PrintDocument = null;
			}
		}

		private void m_PrintDocument_BeginPrint(object sender, PrintEventArgs e)
		{
			switch (m_PrintDocument.PrinterSettings.PrintRange)
			{
				case PrintRange.AllPages:
					m_PrintPageIndex = 0;
					break;
				case PrintRange.CurrentPage:
				case PrintRange.Selection:
					m_PrintPageIndex = CurrentPageIndex;
					break;
				case PrintRange.SomePages:
					m_PrintPageIndex = m_PrintDocument.PrinterSettings.FromPage - 1;
					break;
			}
		}

		public void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
		{
			// we do not use the buffers when printing (the printer is probably higher-resolution!)
			Graphics gr = e.Graphics;
			Page page = CurrentDocument.Page(m_PrintPageIndex);
			e.PageSettings.Landscape = page.IsLandscape;
			gr.ResetTransform();
			gr.PageUnit = GraphicsUnit.Pixel; // GraphicsUnit.Millimeter ' it shows 600 DPI which makes the usual scaling conversion for mono impossible

			SizeF printable = e.PageSettings.PrintableArea.Size.MultiplyBy(Geometry.INCH / 100); // in mm.  Apparently e.MarginBounds or e.PageSettings.PrintableArea is in hundredths of an inch  (?)
			if (page.IsLandscape)
				printable = printable.Flip(); // But sadly e.PageSettings.PrintableArea does not account for orientation
			var pageSize = page.Size;
			// max scale that will fit:
			var scale = Math.Min(printable.Width / pageSize.Width, printable.Height / pageSize.Height);
			if (scale > 0.97f) // will not fit at full size (allow a bit <1 in case of slight margin issues)
				scale = 1; // don't shrink
			else
			{
				pageSize.Width *= scale; // update page size for centreing below
				pageSize.Height *= scale;
			}

			gr.ScaleTransform(scale * gr.DpiX / Geometry.INCH, scale * gr.DpiY / Geometry.INCH);
			gr.TranslateTransform(0, page.Size.Height - 1);
			// centre it
			gr.TranslateTransform(scale * (pageSize.Width - pageSize.Width) / 2, scale * (pageSize.Height - pageSize.Height) / 2);
			// No need to take account of the margins if we are centring it anyway
			//gr.TranslateTransform(m_objPage.Margin - INCH * e.PageSettings.HardMarginX / 100, m_objPage.Margin - INCH * e.PageSettings.HardMarginY / 100)
			gr.SmoothingMode = SmoothingMode.AntiAlias;
			gr.TextRenderingHint = TextRenderingHint.AntiAlias; // otherwise text on transparent buffer is naff

			using (NetCanvas canvas = new NetCanvas(gr, true))
			{
				page.DrawBackground(canvas, scale * gr.DpiX / Geometry.INCH, page.Paper.PrintBackground, false);
				if (page != CurrentPage)
					page.BackgroundImage?.Release(); // avoid overload if document has masses of large pages
				if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.Selection)
					page.DrawSelected(canvas, null, scale, gr.DpiX / Geometry.INCH, true);
				else
					page.DrawShapes(canvas, scale, gr.DpiX / Geometry.INCH, null, !PrintMeasures);
			}

			m_PrintPageIndex += 1;
			switch (m_PrintDocument.PrinterSettings.PrintRange)
			{
				case PrintRange.CurrentPage:
				case PrintRange.Selection:
					e.HasMorePages = false;
					break;
				default:
					e.HasMorePages = m_PrintPageIndex + 1 <= m_PrintDocument.PrinterSettings.ToPage; // +1 is the conversion from 0-based to 1-based
					break;
			}
		}

		private void PrintPreview()
		{
			try
			{
				m_PrintDocument = new System.Drawing.Printing.PrintDocument();
				m_PrintDocument.BeginPrint += m_PrintDocument_BeginPrint;
				m_PrintDocument.PrintPage += PrintDocument_PrintPage;
				m_PrintDocument.PrinterSettings.MaximumPage = CurrentDocument.Count;
				m_PrintDocument.PrinterSettings.MinimumPage = 1;
				m_PrintDocument.PrinterSettings.FromPage = 1;
				m_PrintDocument.PrinterSettings.ToPage = CurrentDocument.Count;
				m_PrintDocument.DefaultPageSettings.Landscape = CurrentPage.IsLandscape;

				m_PrintPageIndex = 0; // in case the user prints
				var dlgPreview = new PrintPreviewDialog
				{
					Document = m_PrintDocument,
					//AutoScrollMargin = new Size(0, 0),
					//AutoScrollMinSize = new Size(0, 0),
					ClientSize = new Size(400, 300),
					//Enabled = true,
					//UseAntiAlias = true
				};

				PrintMeasures = false;
				dlgPreview.ShowDialog();
			}
			finally
			{
				m_PrintDocument.Dispose();
				m_PrintDocument = null;
				m_PrintDocument.BeginPrint += m_PrintDocument_BeginPrint;
				m_PrintDocument.PrintPage += PrintDocument_PrintPage;
			}
		}

		public override bool IsApplicable(EditableView pnlView)
		{
			if (Code == Codes.PrintDirectly)
				return !CurrentPage.IsEmpty;
			return !CurrentDocument.IsEmpty;
		}

		public override bool MightOpenModalDialog => !Direct;
	}
	#endregion

	#region Export

	internal class ExportEMF : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			// see:  http://stackoverflow.com/questions/152729/gdi-c-how-to-save-an-image-as-emf
			string filename = FileDialog.ShowSave(FileDialog.Context.Image, "*.emf|*.emf");
			if (String.IsNullOrEmpty(filename))
				return;
			Image objImage = pnlView.CreateImage(true, true, false);
			IntPtr h = ((Metafile)objImage).GetHenhmetafile();
			Windows.ClipboardMetafileHelper.CopyEnhMetaFileW(h, filename);
			Windows.ClipboardMetafileHelper.DeleteEnhMetaFile(h);
			objImage.Dispose();
		}
		public override bool MightOpenModalDialog => true;
	}

	internal class ExportSVG : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string filename = FileDialog.ShowSave(FileDialog.Context.Image, "*.svg|*.svg");
			if (string.IsNullOrEmpty(filename))
				return;
			float scale = GUIUtilities.SystemDPI / Geometry.INCH; // convert from mm to nominal pixels (needed to get fonts the right size)
			using (SVGCanvas canvas = new SVGCanvas(CurrentPage.Size))
			{
				//canvas.TranslateTransform(0, m_objPage.Size.Height) - not needed because the SVG viewbox is created with a negative starting y-coordinate
				CurrentPage.DrawBackground(canvas, scale, true, true, false); // Not sure if the origin should be drawn or not
				CurrentPage.DrawShapes(canvas, scale, scale, pnlView);
				// no attempt to do pixels - should not be called if pixel data present
				var str = canvas.ToString();
				File.WriteAllText(filename, str);
#if DEBUG
				File.WriteAllText(filename.Replace(".svg", ".html"), "<html><head></head><body>" + str + "</body></html>");
#endif
			}

		}

		public override bool IsApplicable(EditableView pnlView)
		{
			return !CurrentPage.IsEmpty; // These options not terribly helpful if there is pixel data
		}

		public override bool MightOpenModalDialog => true;
	}

	internal class ExportImage : Verb
	{
		public string extension;
		public ImageFormat format;
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string filename = FileDialog.ShowSave(FileDialog.Context.Image, "*." + extension + "|*." + extension);
			if (string.IsNullOrEmpty(filename))
				return;
			const bool selection = false;// was param before, not sure if true was ever used?
			Image image = pnlView.CreateImage(false, !selection, selection);
			if (File.Exists(filename))
				File.Delete(filename);
			image.Save(filename, format);
			image.Dispose();
		}

		public override bool IsApplicable(EditableView pnlView) => !CurrentPage.IsEmpty;
		public override bool MightOpenModalDialog => true;
	}

	internal class ExportPageAsDocument : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string filename = FileDialog.ShowSave(FileDialog.Context.Document);
			if (string.IsNullOrEmpty(filename))
				return;
			List<Page> list = new List<Page> { CurrentPage };
			Document document = new Document(list); // constructs the document containing this page
			DatumList referenced = new DatumList();
			CurrentPage.AddRequiredReferencesRecurseToContained(referenced.Add, Mapping.Ignore);
			foreach (Datum datum in referenced.Values)
			{
				switch (datum.TypeByteAsFileMarker)
				{
					case FileMarkers.Paper:
						break;
					case FileMarkers.SharedBitmap:
						document.AddSharedResource((SharedImage)datum);
						break;
					default:
						Debug.Fail("Unexpected external reference from page");
						break;
				}
			}
			using (DataWriter writer = new DataWriter(filename, FileMarkers.Splash))
			{
				writer.Write(document);
			}

			// We don't dispose the document because that would dispose the pages; we can just let the garbage collector cleanup the document
		}

		public override bool IsApplicable(EditableView pnlView) => !CurrentPage.IsEmpty;
		public override bool MightOpenModalDialog => true;
	}
	#endregion

	#region ImportImage
	class ImportImage : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			pnlView.ConcludeOngoing();
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image);
			if (!string.IsNullOrEmpty(filename))
				pnlView.ImportImage(filename);
		}

		public override bool MightOpenModalDialog => true;
	}
	#endregion

	#region Close
	class CloseDocument : Verb
	{
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (!Editor.CheckDiscard(CurrentDocument))
				return;
			if (Globals.Root.User == Users.Editor && Editor.ctrDocuments.Count > 1 && Editor.ctrDocuments.Visible)
				Globals.Root.RemoveDocument(Globals.Root.DocumentIndex); // Will select a different document
			else
			{
				Globals.Root.RemoveDocument(Globals.Root.DocumentIndex);
				//m_Document = null; // to avoid triggering another save question if the user didn't save here, and then closes the menu page
				Globals.Root.ShowMenu();
			}
		}

	}

	#endregion

}
