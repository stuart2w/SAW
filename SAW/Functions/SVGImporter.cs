using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Imports an SVG file to a document.  Not actually an action atm, although in that folder, and is just run in global app file load.  However
	/// it could be extracted as a separate verb, possibly</summary>
	internal class SVGImporter
	{
		/// <summary>Size of 1 inch in user units </summary>
		private float Inch = 96f;
		private Matrix m_CurrentMatrix;
		private Page m_OutputPage;
		private Document m_Document;

		public Document LoadSVG(string file)
		{
			XDocument svg = XDocument.Load(file);
			XElement root = svg.Root;
			if (root.Name.LocalName != "svg")
				throw new InvalidDataException("Document does not contain an <svg> root element");

			RectangleF viewBox = ParseRectangle(root.Attribute("viewBox"));
			float width = ParseUnit(root.Attribute("width")?.Value);
			float height = ParseUnit(root.Attribute("height")?.Value);
			// x and y aren't really needed?  we want document to end up at (0,0) anyway
			if (viewBox.IsEmpty)
				viewBox = new RectangleF(0, 0, width, height);
			m_CurrentMatrix = Geometry.TransformFromRects(viewBox, new RectangleF(0, -height, width, height));// note output has -H at top and 0 at bottom - which is standard for SAW

			m_Document = new Document(false);
			m_Document.SetDefaultsForNewDocument();
			m_OutputPage = m_Document.Page(0);
			m_OutputPage.SetSize(new SizeF(width, height), 0);

			ProcessContents(root);
			return m_Document;
		}

		private void ProcessContents(XElement parent)
		{
			foreach (XElement child in parent.Elements())
			{
				Matrix transform = ParseTransform(child.Attribute("transform")); // often null
				Matrix oldMatrix = null;
				if (transform != null)
				{
					oldMatrix = m_CurrentMatrix.Clone();
					m_CurrentMatrix.Multiply(transform);
				}
				switch (child.Name.LocalName)
				{
					case "g": // nothing needed?  transform done above, and we're not intending to actually make groups
						break;
					case "line": DoLine(child); break;
					case "rect": DoRect(child); break;
					case "polygon": DoPolygon(child); break;
					case "text": DoText(child); break;
					case "image": DoImage(child); break;
					default:
						Globals.NonFatalOperationalError($"Ignored SVG element: {child.Name.LocalName}");
						break;
				}
				ProcessContents(child);
				if (transform != null)
					m_CurrentMatrix = oldMatrix;
			}
		}

		#region Elements

		private void DoLine(XElement element)
		{
			PointF one = m_CurrentMatrix.TransformPoint(new PointF(ParseUnit(element.Attribute("x1")), ParseUnit(element.Attribute("y1"))));
			PointF two = m_CurrentMatrix.TransformPoint(new PointF(ParseUnit(element.Attribute("x2")), ParseUnit(element.Attribute("y2"))));
			Line line = new Line(one, two);
			DoLineStyle(element, line);
			m_OutputPage.AddNew(line);
		}

		private void DoRect(XElement element)
		{
			RectangleF rect = new RectangleF(ParseUnit(element.Attribute("x")), ParseUnit(element.Attribute("y")), ParseUnit(element.Attribute("width")), ParseUnit(element.Attribute("height")));
			float rx = ParseUnit(element.Attribute("rx"));
			float ry = ParseUnit(element.Attribute("ry"));
			Filled shape;
			if (rx != 0 || ry != 0)
			{
				GraphicsPath path = GUIUtilities.CreateRoundedRectPath(rect, rx);
				path.Transform(m_CurrentMatrix);
				shape = new GenericPath(path);
			}
			else
			{
				PointF[] transformed = rect.GetThreePoints(); // TL, TR, BL
				m_CurrentMatrix.TransformPoints(transformed);
				shape = new RectangleShape(transformed[0], transformed[1], transformed[2] + transformed[1].Subtract(transformed[0])); // note 3rd point differs from .net to us
			}
			DoLineStyle(element, shape);
			DoFillStyle(element, shape.FillStyle);
			m_OutputPage.AddNew(shape);
		}

		private void DoPolygon(XElement element)
		{
			// points do seem to be strictly spaces between points and , inside them, although there seem to be spurious spaces on the end sometimes
			PointF[] points = (from pointSource in element.Attribute("points").Value.Split(' ')
							   where !string.IsNullOrEmpty(pointSource)
							   select pointSource.Split(',') into parts
							   select new PointF(ParseUnit(parts[0]), ParseUnit(parts[1]))).ToArray();
			m_CurrentMatrix.TransformPoints(points);
			IrregularPolygon shape = new IrregularPolygon(points);
			DoLineStyle(element, shape);
			DoFillStyle(element, shape.FillStyle);
			m_OutputPage.AddNew(shape);
		}

		private static XNamespace xlink = "http://www.w3.org/1999/xlink";
		private void DoImage(XElement element)
		{// eg "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEwAAABoCAYAAABbqhYLAAAACXBIWXMAAAsSAAALEgHS3X78AAAA..."
			float width = ParseUnit(element.Attribute("width"));
			float height = ParseUnit(element.Attribute("height"));
			if (width == 0 || height == 0)
			{
				Globals.NonFatalOperationalError("Cannot interpret image, width and/or height not defined");
				return;
			}
			if (element.Attribute("opacity") != null)
				Globals.NonFatalOperationalError("Opacity ignored in image");

			string content = element.Attribute(xlink + "href")?.Value; // and that syntax refers to "xlink:href".  Cannot just use that text directly - it insists on the namespace object
			if (string.IsNullOrEmpty(content) || !content.ToLowerInvariant().StartsWith("data:"))
			{
				Globals.NonFatalOperationalError("Cannot interpret image, no xlink");
				return;
			}
			content = content.Substring(5); // removing "data:"
			string[] parts = content.Split(new[] { ';' }, 2); // split off the format part
			if (parts.Length < 2)
			{
				Globals.NonFatalOperationalError("Cannot interpret image: no mime type");
				return;
			}
			//string type = parts[0]; - deduced?
			parts = parts[1].Split(new[] { ',' }, 2); // encoding
			if (parts.Length < 2)
			{
				Globals.NonFatalOperationalError("Cannot interpret image: no encoding");
				return;
			}
			byte[] data;
			switch (parts[0].ToLowerInvariant())
			{
				case "base64":
					data = Convert.FromBase64String(parts[1]);
					break;
				default:
					Globals.NonFatalOperationalError($"Cannot interpret image: unexpected encoding {parts[0]}");
					return;
			}
			MemoryImage image = new MemoryImage(data, false);
			SharedImage shared = m_Document.AddSharedBitmapFromMemoryImage(image, null);
			ImportedImage shape = new ImportedImage(shared);
			PointF[] bounds = (new RectangleF(0, 0, width, height)).GetPoints(); // gets the 4 corners in the correct order for the shape
			m_CurrentMatrix.TransformPoints(bounds);
			shape.SetVertices(bounds);
			m_OutputPage.AddNew(shape);
		}

		private void DoText(XElement element)
		{
			//Shape.FillStyleC fill = new Shape.FillStyleC();
			//fill.SetDefaults();
			//DoFillStyle(element, fill);
			Shape.TextStyleC textStyle = new Shape.TextStyleC() { Colour = Color.Black, Alignment = StringAlignment.Near, Size = 12, VerticalAlignment = StringAlignment.Near };
			DoTextStyle(element, textStyle);
			PointF location = PointF.Empty;
			// first do text in the outer element, if any
			if (element.Nodes().OfType<XText>().Any())
				DoTextSpan(element, ref location, textStyle);
			foreach (XElement tspan in element.Elements("tspan"))
			{
				// this makes copies of all the styles, which is probably creating unnecessary objects
				Shape.TextStyleC spanStyle = new Shape.TextStyleC();
				spanStyle.CopyFrom(textStyle);
				DoTextStyle(tspan, spanStyle);
				DoTextSpan(tspan, ref location, spanStyle);
			}
		}

		/// <summary>Does text part shared between text itself and tspan's within</summary>
		private void DoTextSpan(XElement element, ref PointF location, Shape.TextStyleC style)
		{
			if (element.Attribute("x") != null)
				location.X = ParseUnit(element.Attribute("x"));
			if (element.Attribute("y") != null)
				location.Y = ParseUnit(element.Attribute("y"));
			if (element.Attribute("dx") != null)
				location.X += ParseUnit(element.Attribute("dx"));
			if (element.Attribute("dy") != null)
				location.Y += ParseUnit(element.Attribute("dy"));
			string text = element.Nodes().OfType<XText>().FirstOrDefault()?.Value ?? "";

			var font = style.CreateFont(style.Size);
			float ascent = style.Size * font.FontFamily.GetCellAscent(FontStyle.Regular) / font.FontFamily.GetEmHeight(FontStyle.Regular);
			//GetEmHeight needed as GetCellAscent is in design units
			// ascent now in points, need to convert to document pixels
			float Y = location.Y - ascent / 72 * Inch;

			Graphics gr = NetCanvas.MeasurementInstance.Underlying;
			RectangleF bounds = NetCanvas.MeasureTextExact(gr, text, font, new RectangleF(0, 0, 10000, 10000), GUIUtilities.StringFormatMeasureLine);
			float X = location.X - bounds.X;

			PointF[] points = { new PointF(X, Y), new PointF(X + bounds.Width + 4, Y) }; // extra bit added to prevent spurious line wraps
			m_CurrentMatrix.TransformPoints(points);
			TextLine shape = new TextLine(points[0], points[1], text);
			shape.TextStyle = style;
			m_OutputPage.AddNew(shape);

			points = new[] { location, location + new SizeF(bounds.Width, 0) };
			m_CurrentMatrix.TransformPoints(points);
			m_OutputPage.AddNew(new Line(points[0], points[1]) { LineStyle = new Shape.LineStyleC() { Colour = Color.Black, Width = 1 } });

			location.X += bounds.Width;
		}

		/// <summary>Does the stroke/line style for any shape supporting it</summary>
		private void DoLineStyle(XElement element, Lined shape)
		{
			shape.LineStyle.Colour = ParseColour(element.Attribute("stroke"));
			shape.LineStyle.Width = ParseUnit(element.Attribute("stroke-width"));
			// "stroke-linecap" and "stroke-miterlimit" both ignored
		}

		private void DoFillStyle(XElement element, Shape.FillStyleC style)
		{
			style.Colour = ParseColour(element.Attribute("fill")); // also accepts "none" setting transparent
			style.Pattern = Shape.FillStyleC.Patterns.Solid;
		}

		private void DoTextStyle(XElement element, Shape.TextStyleC style)
		{// note this one must not update the original, if values are not provided - used in spans within text which must inherit.  This is perhaps true of the others as well
			if (element.Attribute("fill") != null)
				style.Colour = ParseColour(element.Attribute("fill"));
			if (element.Attribute("font-size") != null)
				style.Size = ParseUnit(element.Attribute("font-size")) / Inch * 72;
			if (element.Attribute("font-family") != null)
				style.Face = element.Attribute("font-family").Value;
		}

		#endregion

		#region Attribute readers

		private RectangleF ParseRectangle(XAttribute attrib)
		{
			if (attrib == null)
				return RectangleF.Empty;
			float[] values = ParseNumberList(attrib).ToArray();
			if (values.Length != 4)
				throw new InvalidDataException("viewBox does not have 4 values");
			return new RectangleF(values[0], values[1], values[2], values[3]);
		}

		private IEnumerable<float> ParseNumberList(XAttribute attrib)
		{
			foreach (string S in attrib.Value.Split(',', ' '))
			{
				if (string.IsNullOrEmpty(S))
					continue;
				float N;
				if (!float.TryParse(S, out N))
					throw new InvalidDataException($"'{S}' is not a number in {attrib}/{attrib.Parent}");
				yield return N;
			}
		}
		private IEnumerable<float> ParseNumberList(string value)
		{
			foreach (string S in value.Split(',', ' '))
			{
				if (string.IsNullOrEmpty(S))
					continue;
				float N;
				if (!float.TryParse(S, out N))
					throw new InvalidDataException($"'{S}' is not a number in {value}");
				yield return N;
			}
		}

		/// <summary>Returns the size to use for the given value from the file, which can include units (eg '5' or '5cm').
		/// Returns ifMissing if missing, throws exception if invalid</summary>
		private float ParseUnit(XAttribute attribute, float ifMissing = 0) => ParseUnit(attribute?.Value, ifMissing);

		/// <summary>Returns the size to use for the given value from the file, which can include units (eg '5' or '5cm').
		/// Returns ifMissing if missing, throws exception if invalid</summary>
		private float ParseUnit(string original, float ifMissing = 0)
		{
			if (string.IsNullOrWhiteSpace(original))
				return ifMissing;
			original = original.Trim();
			int digits = 0;
			while (digits < original.Length && (char.IsDigit(original[digits]) || original[digits] == '.'))
				digits++;
			float value;
			if (!float.TryParse(original.Substring(0, digits), out value))
				throw new InvalidDataException($"Invalid number in measurement {original}");
			if (digits < original.Length)
			{
				string units = original.Substring(digits); // all remaining chars
				switch (units.ToLowerInvariant())
				{
					case "px": break; // these are the default anyway
					case "in": value = value * Inch; break;
					case "pt": value = value * Inch / 72; break;
					case "pc": value = value * Inch / 7.2f; break; // seems to be 10pt??
					case "mm": value = value * Inch / Geometry.INCH; break;
					case "cm": value = value * 10 * Inch / Geometry.INCH; break;
					case "em":
					case "ex": throw new NotImplementedException(); // need to be relative to font size
					default: throw new InvalidDataException($"Invalid unit of measure in {original}");
				}
			}
			return value;
		}

		private Color ParseColour(XAttribute attribute)
		{
			if (attribute == null)
				return Color.Transparent;
			string value = attribute.Value;
			if (value.StartsWith("#"))
			{
				int hex = int.Parse(value.Substring(1), System.Globalization.NumberStyles.HexNumber);
				if (value.Length == 4)
					return Color.FromArgb(255, ((hex >> 8) & 0xf) * 0x11, ((hex >> 4) & 0xf) * 0x11, ((hex >> 0) & 0xf) * 0x11); // * 0x11 since #XYZ is same as #XXYYZZ
				return Color.FromArgb(255, (hex >> 16) & 0xff, (hex >> 8) & 0xff, hex & 0xff);
			}
			if (value == "none") // not sure this exists as a colour, but is used to indicate no stroke/fill
				return Color.Transparent;
			// https://www.december.com/html/spec/colorsvghex.html
			// otherwise must be named colour.  They seem to pretty much match the .net ones however.  Hoping this works:
			return Color.FromName(value); // and luckily this does seem to be case-insensitive
		}

		private static Regex TransformPattern = new Regex(@"(?<name>[a-zA-Z]+)\((?<bracket>[0-9. ]+)\)");
		/// <summary>Parses the Transform which can be on various elements.  This is done by the main processing, not the individual element types </summary>
		private Matrix ParseTransform(XAttribute attribute)
		{
			if (attribute == null)
				return null;
			Matrix result = new Matrix();
			foreach (Match match in TransformPattern.Matches(attribute.Value))
			{
				string name = match.Groups["name"].Captures[0].Value;
				string bracket = match.Groups["bracket"].Captures[0].Value;
				float[] values = ParseNumberList(bracket).ToArray();
				switch (name.ToLowerInvariant())
				{
					case "translate":
						if (values.Length != 2)
							Globals.NonFatalOperationalError($"Unexpected number of values in transform: {name}");
						else
							result.Translate(values[0], values[1]);
						break;
					case "matrix":
						if (values.Length != 6)
							Globals.NonFatalOperationalError($"Unexpected number of values in transform: {name}");
						else
							result.Multiply(new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]));
						break;
					case "scale":
						if (values.Length != 1)
							Globals.NonFatalOperationalError($"Unexpected number of values in transform: {name}");
						else
							result.Scale(values[0], values[0]); // deliberately repeated.  I think SVG only specifies one value used in both dimensions
						break;
					case "rotate":
						if (values.Length == 1)
							result.Rotate(values[0]);
						else if (values.Length == 3)
							result.RotateAt(values[0], new PointF(values[1], values[2]));
						else
							Globals.NonFatalOperationalError($"Unexpected number of values in transform: {name}");
						break;
					default:
						Globals.NonFatalOperationalError("Tranform not recognised: " + name);
						return null;
				}
			}

			return result;
		}

		#endregion

	}
}
