using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.Functions
{
	internal class ImportIRM : Verb
	{
		public const string Filter = "*.irm|*.irm";

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Multiple_Documents) && !Editor.CheckDiscardCurrent(false))
				return;
			string file = FileDialog.ShowOpen(FileDialog.Context.OtherUserDoc, Filter);
			if (string.IsNullOrEmpty(file))
				return;
			using (var operation = new Globals.Operation(this.DescriptionWithoutAccelerator()))
				try
				{
					ParseFile(file);
					Document document = GenerateDocument();
					if (operation.ConfirmSuccess())
						Globals.Root.SelectDocument(document);

				}
				catch (Exception ex) when (!Globals.Root.IsDebug)
				{
					MessageBox.Show(ex.Message);
				}
				finally
				{
					m_Elements = null;
				}
		}

		public override bool MightOpenModalDialog => true;

		private List<Element> m_Elements;

		/// <summary>Splits file into m_Elements</summary>
		private void ParseFile(string file)
		{
			m_Elements = new List<Element>();
			int lineNumber = 0;
			foreach (string line in File.ReadAllLines(file))
			{
				lineNumber++;
				string remaining = line.Trim();
				if (string.IsNullOrEmpty(remaining))
					continue;
				int bracket = remaining.IndexOf('[');
				while (bracket >= 0)
				{
					int endBracket = remaining.IndexOf(']', bracket + 1);
					if (endBracket < 0)
						throw new Exception($"Mismatched missing ']' in IR file at line {lineNumber}");
					string code = remaining.Substring(bracket + 1, endBracket - bracket - 1);
					int end = remaining.IndexOf('[', endBracket + 1); // location of first char not included in data for this element
					if (end < 0)
						end = remaining.Length;
					string data = "";
					if (end > endBracket + 1)
						data = remaining.Substring(endBracket + 1, end - endBracket - 1);
					m_Elements.Add(new Element(code, data.Trim(), lineNumber));
					if (end >= remaining.Length - 1)
						break;
					remaining = remaining.Substring(end);
					bracket = remaining.IndexOf('[');
				}
			}
		}

		private struct Element
		{ // represents one element in file, in form [Code]Data, up until EOL or the next [
			public readonly string Code;
			public readonly string Data;
			public readonly int Line;
			public Element(string code, string data, int line)
			{
				Code = code;
				Data = data;
				Line = line;
			}

			public Point ParsePoint()
			{
				var parts = Data.Split(',');
				if (parts.Length != 2)
					throw new Exception($"{Code} is not a valid point");
				int x, y;
				if (!int.TryParse(parts[0], out x) || !int.TryParse(parts[1], out y))
					throw new Exception($"{Code} is not a valid point");
				return new Point(x, y);
			}

			public void Throw(string message)
			{
				throw new Exception($"{Code} @ line {Line}: {message}");
			}

		}

		private int m_NextElement;
		private int m_NextID;

		/// <summary>Size of top section.  Coords of other panels are relative to the panel, so this must be added on. </summary>
		private SizeF m_TopSize;

		private Document GenerateDocument()
		{
			Document document = new Document(false);
			document.SetDefaultGridAndSnapFromActivity();
			int currentPanel = 0; // which panel is being read, or -1 for the global bit at the top, or -2 for none (have encountered END)
			Page page = document.Page(0);
			m_NextElement = 0;
			m_NextID = 1;
			m_TopSize = SizeF.Empty; //Can be left at 0,0 if main is not used and this is just a fractional file
			int lowestSepSeen = 0; // Y coord of lowest separator seen.  this is used to size the top area rather than FRMPIX (which just appears to be way too large)
			List<Item> panelButtons = new List<Item>();
			Transaction ignoreTransaction = new Transaction(); // won't be stored as we don't want to be able to undo, but some methods below require one
			while (m_NextElement < m_Elements.Count)
			{
				Element element = ReadElement();
				switch (element.Code.ToUpperInvariant())
				{
					case "MAIN":
						currentPanel = -1;
						break;
					case "SLED":
					case "SDEVICE":
						break; //  ignored
					case "FRMPIX":
						SizeF sz = element.ParsePoint().ToSize();
						SizeF total = new SizeF(Math.Max(sz.Width, m_TopSize.Width), sz.Height + m_TopSize.Height);
						page.SetSize(total, 0); // if this is the top, then mainsize will currently be 0
						if (currentPanel >= 0)
							TranslateContentsVertically(page, -sz.Height);
						break;
					case "SEP":
						if (currentPanel == -2)
							throw new Exception("SEP not expected here");
						Element endElement = ReadElement();
						if (endElement.Code.ToUpperInvariant() != "END")
						{
							Globals.NonFatalOperationalError("IRM [SEP] is not followed by [END] - ignored");
							m_NextElement--; // put endElement back in the pool
						}
						else
						{
							PointF p1 = element.ParsePoint();
							PointF p2 = endElement.ParsePoint();
							lowestSepSeen = Math.Max((int)p1.Y, lowestSepSeen);
							p1.Y += m_TopSize.Height - page.Size.Height;
							p2.Y += m_TopSize.Height - page.Size.Height;
							Line line = new Line(p1, p2);
							line.LineStyle.Colour = Color.Black;
							line.LineStyle.Width = 1;
							page.AddNew(line, null);
						}
						break;
					case "MOD":
						if (currentPanel != -1)
							throw new Exception("MOD only expected in MAIN");
						var data = ReadParameters(new[] { "SIZE", "PANEL" });
						if (!data.ContainsKey("SIZE") || !data.ContainsKey("PANEL"))
							throw new Exception("MOD is missing either SIZE or PANEL fields");
						Scriptable created = CreateButton(data["PANEL"].Data, element, data["SIZE"], page);
						created.SelectScript = new Script();
						panelButtons.Add((Item)created.Element);
						created.SelectScript.CommandList.Add(new CmdGotoPage(panelButtons.Count)); // index is 1-based
						created.SelectScript.RunDefault = false;
						page.AddNew(created, null);
						break;
					case "END":
						if (currentPanel == -1 && panelButtons.Count > 0)
							GeneratePanels(document, panelButtons, ignoreTransaction);
						if (currentPanel == -1)
						{// ending the top area, record its size, so we can adjust coords on the following pages which are relative to this
						 // (they appear to be relative to the separator, and FRMPIX has little effect, so we use that IFF [SEP] was seen)
							if (lowestSepSeen > 0)
							{
								m_TopSize = new SizeF(page.Size.Width, lowestSepSeen);
								// but content was positioned based on FRMPIX, so need to adjust it again to match a new top left
								foreach (Page p in document.Pages)
									TranslateContentsVertically(p, -lowestSepSeen + page.Size.Height);
							}
							else
								m_TopSize = page.Size;
						}
						currentPanel = -2;
						break;
					case "LBL":
						var labelParams = ReadParameters(new[] { "SIZE", "TEXT", "FONT" });
						if (!labelParams.ContainsKey("SIZE") || !labelParams.ContainsKey("TEXT"))
						{
							Globals.NonFatalOperationalError("Either SIZE or TEXT missing from LABEL at line " + element.Line);
							continue;
						}
						string labelText = labelParams["TEXT"].Data;
						if (string.IsNullOrEmpty(labelText)) // there are some empty labels in IRM which look silly when imported
							continue;
						RectangleF labelBounds = GetRect(element, labelParams["SIZE"], page);
						var text = new TextLine(labelBounds, labelText);
						if (labelParams.ContainsKey("FONT"))
						{
							float size;
							if (float.TryParse(labelParams["FONT"].Data, out size))
								text.StyleObjectForParameter(Parameters.FontSize).SetParameterValue((int)(Math.Max(size, 6) * 100), Parameters.FontSize);
						}
						page.AddNew(text, null);
						break;
					case "POS":
						var buttonParams = ReadParameters(new[] { "SIZE", "REMOTE", "COMMAND", "TEXT" });
						if (!buttonParams.ContainsKey("SIZE") || !buttonParams.ContainsKey("TEXT"))
						{
							Globals.NonFatalOperationalError("TEXT or SIZE missing from POS at line " + element.Line);
							continue;
						}
						Scriptable button = CreateButton(buttonParams["TEXT"].Data, element, buttonParams["SIZE"], page);
						button.SelectScript = new Script() { RunDefault = true };
						if (buttonParams.ContainsKey("REMOTE"))
							button.SelectScript.CommandList.Add(new NullCommand() { Comment = "REMOTE=" + buttonParams["REMOTE"].Data });
						if (buttonParams.ContainsKey("COMMAND"))
							button.SelectScript.CommandList.Add(new NullCommand() { Comment = "COMMAND=" + buttonParams["COMMAND"].Data });
						page.AddNew(button, null);
						break;
					default:
						Item panelButton = (from b in panelButtons where b.LabelText == element.Code select b).FirstOrDefault();
						if (panelButton == null)
							throw new Exception($"{element.Code} element not understood/expected");
						currentPanel = panelButtons.IndexOf(panelButton);
						page = document.Page(currentPanel);
						break;
				}
			}
			document.SAWHeader = new Header();
			return document;
			//Debug.WriteLine("Top size=" + m_TopSize);
			//foreach (var p in document.Pages)
			//	Debug.WriteLine("Page size=" + p.Size);

		}

		/// <summary>Returns the rectangle specified by the coords in the 2 elements.  The first is typically the element which introduces something and may have various codes</summary>
		private RectangleF GetRect(Element posElement, Element size, Page page)
		{
			Debug.Assert(size.Code == "SIZE");
			Point pt = posElement.ParsePoint();
			Point sz = size.ParsePoint();
			return new RectangleF(pt.X, pt.Y + m_TopSize.Height - page.Size.Height, sz.X, sz.Y);
		}

		/// <summary>Takes the first page and duplicates content onto other pages, also creating those pages </summary>
		private void GeneratePanels(Document document, List<Item> PanelButtons, Transaction transaction)
		{
			Page main = document.Page(0);
			for (int add = 1; add < PanelButtons.Count; add++)
			{
				Page extra = document.AddPage(main, transaction, false, -1, true);
			}
		}

		/// <summary>Reads all the next elements matching the list of allowed codes and puts them in a dictionary</summary>
		private Dictionary<string, Element> ReadParameters(string[] allowedCodes)
		{
			var result = new Dictionary<string, Element>();
			Element element = ReadElement();
			while (allowedCodes.Contains(element.Code))
			{
				result.Add(element.Code, element);
				element = ReadElement();
			}
			m_NextElement--; // put last, unused, item back
			return result;
		}

		private Scriptable CreateButton(string text, Element topLeft, Element size, Page page)
		{
			Item item = new Item();
			item.SetBounds(GetRect(topLeft, size, page));
			item.LabelText = text;
			item.HighlightLineStyle.Colour = Color.White;
			item.HighlightFillStyle.Colour = Color.Red;
			Scriptable script = new Scriptable(item);
			script.SAWID = m_NextID++;
			return script;
		}

		private Element ReadElement()
		{
			return m_Elements[m_NextElement++];
		}

		/// <summary>Shifts all the existing content on the given page up or down</summary>
		private void TranslateContentsVertically(Page page, float deltaY)
		{
			Transformation x = new TransformMove(0, deltaY);
			foreach (Shape s in page.Shapes)
			{
				s.ApplyTransformation(x);
			}

		}

	}

	internal class ExportIRM : Verb
	{
		private static bool g_WarningDone;
		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			if (!g_WarningDone)
				MessageBox.Show(Strings.Item("SAW_IRM_SaveNote"));
			g_WarningDone = true;

			string file = FileDialog.ShowSave(FileDialog.Context.OtherUserDoc, ImportIRM.Filter);
			if (string.IsNullOrEmpty(file))
				return;

			using (var operation = new Globals.Operation(this.DescriptionWithoutAccelerator()))
			{
				Document doc = Globals.Root.CurrentDocument;
				StringBuilder output = new StringBuilder();
				int mainHeight = 0;
				List<Scriptable> pageButtons =
					(from shape in doc.Page(0).Shapes
					 where shape is Scriptable s && s.SelectScript.CommandList.Any(c => c is CmdGotoPage)
					 orderby PageButtonDestination(shape as Scriptable)
					 select shape as Scriptable).ToList();
				for (int pageIndex = 0; pageIndex < doc.Pages.Count(); pageIndex++)
				{
					Page page = doc.Page(pageIndex);
					// buttons used to change pages:
					if (pageIndex == 0)
					{
						m_YOffset = (int)page.Size.Height;
						mainHeight = WritePageButtons(output, pageButtons, page);
					}
					m_YOffset = (int)page.Size.Height - mainHeight;

					string name = pageButtons.Count() > pageIndex ? pageButtons[pageIndex].Element.LabelText : "page" + (pageIndex + 1);
					output.Append('[').Append(name).AppendLine("]");
					foreach (Shape s in page.Shapes)
					{
						if (s.ShapeCode == Shape.Shapes.Scriptable)
						{
							Scriptable scriptable = (Scriptable)s;
							if (scriptable.SelectScript.CommandList.Any(c => c is CmdGotoPage))
								continue; // these are the copies of the page buttons
							WriteElement(output, "POS", s.Bounds.Location);
							WriteElement(output, "SIZE", s.Bounds.Size.ToPointF());
							WriteElement(output, "TEXT", scriptable.Element.LabelText);
							string remote = ExtractCommentValue(scriptable.SelectScript, "REMOTE");
							string command = ExtractCommentValue(scriptable.SelectScript, "COMMAND");
							WriteElement(output, "REMOTE", remote ?? "?");
							WriteElement(output, "COMMAND", command ?? "?", true);
						}
						else if (s.ShapeCode == Shape.Shapes.TextLine)
						{
							WriteElement(output, "LBL", s.Bounds.Location);
							WriteElement(output, "SIZE", s.Bounds.Size.ToPointF());
							WriteElement(output, "TEXT", s.LabelText, true);
						}
						// all other types ignored.  In particular separator line which was sort of generated automatically from button bounds
					}
					output.AppendLine("[END]");
					output.AppendLine();
				}
				if (operation.ConfirmSuccess())
					File.WriteAllText(file, output.ToString());
			}
		}

		private int WritePageButtons(StringBuilder output, List<Scriptable> pageButtons, Page page)
		{ // writes [MAIN] panel with the page change button.
		  // only done from buttons on p1 as they are shared in irm
		  // returns 
			output.AppendLine("[MAIN]");
			RectangleF bounds = Geometry.UnionRectangles(from button in pageButtons select button.Bounds);
			Geometry.Extend(ref bounds, page.Bounds.Location);
			bounds.Height *= 1.1f; // allow some for the separator - the actual line in the SAW document is ignored and the header size is determined from the buttons
			WriteElement(output, "FRMPIX", bounds.Size.ToPointF(), true);
			WriteElement(output, "SEP", bounds.BottomLeft());
			WriteElement(output, "END", bounds.BottomRight(), true);

			foreach (var button in pageButtons)
			{
				WriteElement(output, "MOD", button.Bounds.Location);
				WriteElement(output, "SIZE", button.Bounds.Size.ToPointF());
				WriteElement(output, "PANEL", button.Element.LabelText, true);
			}
			output.AppendLine("[END]");
			output.AppendLine();
			return (int)bounds.Height;
		}

		private void WriteElement(StringBuilder output, string code, string data, bool endLine = false)
		{
			output.Append('[').Append(code).Append(']').Append(data.Replace("[", "").Replace("\r", "").Replace("\n", ""));
			if (endLine)
				output.AppendLine();
			else
				output.Append('\t');
		}

		private int m_YOffset;
		private void WriteElement(StringBuilder output, string code, PointF pt, bool endLine = false)
		{
			output.Append('[').Append(code).Append(']').Append((int)pt.X).Append(',').Append((int)pt.Y + m_YOffset);
			if (endLine)
				output.AppendLine();
			else
				output.Append('\t');
		}

		public override bool MightOpenModalDialog => true;

		/// <summary>For a button navigating to a page, returns the page index</summary>
		private int PageButtonDestination(Scriptable button)
		{
			return (from c in button.SelectScript.CommandList where c is CmdGotoPage select c.GetParamAsInt(0)).FirstOrDefault();
		}

		/// <summary>Looks for command with comment REMOTE= or COMMAND= (matching code) and returns following text, or "" if none</summary>
		private string ExtractCommentValue(Script script, string code)
		{
			code += "=";
			string comment = (from c in script.CommandList where c.Comment?.Contains(code) ?? false select c.Comment).FirstOrDefault();
			if (comment == null)
				return "";
			int index = comment.IndexOf(code) + code.Length;
			if (index >= comment.Length)
				return "";
			return comment.Substring(index);
		}

	}
}
