using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Xml.Linq;
using System.Text;
using SAW.Shapes;

namespace SAW
{
	public class SVGCanvas : Canvas
	{

		private readonly XElement m_Root;
		private XElement m_Container; // Object into which new elements should be added (could be a g group)
		private readonly XElement m_Definitions;
		private readonly HashSet<string> m_Gradients = new HashSet<string>();
		private readonly HashSet<string> m_Patterns = new HashSet<string>();
		private readonly Stack<XElement> m_stkContainers = new Stack<XElement>();
		private readonly HashSet<string> m_Images = new HashSet<string>();
		private int m_NextID = 1; // used at the moment just by the gradients and clips
		private static readonly XNamespace xlinkNamespace = "http://www.w3.org/1999/xlink";

		public SVGCanvas(SizeF size)
		{
			// viewBox is given negative Y coordinate which means no transformation is needed
			m_Root = new XElement("svg", new XAttribute("width", size.Width + "mm"), new XAttribute("height", size.Height + "mm"), new XAttribute("viewBox", "0 -" + size.Height + " " + size.Width + " " + size.Height), new XAttribute("style", "stroke-miterlimit: 10; "));
			m_Definitions = new XElement("defs");
			m_Root.Add(m_Definitions);
			m_Root.Add(new XAttribute(XNamespace.Xmlns + "xlink", xlinkNamespace));
			m_Container = m_Root;
		}

		/// <summary>Returns entire SVG</summary>
		public override string ToString()
		{
			// DisableFormatting is required because whitespace within <text> is significant (without this the last line of right-aligned text is always further to the right than the other lines)
#if DEBUG
			return m_Root.ToString(); //(SaveOptions.DisableFormatting)
#else
			return m_Root.ToString(SaveOptions.DisableFormatting);
#endif
		}

		#region Stroke and Fill classes
		private class SVGStroke : Stroke
		{

			private readonly Color m_Colour;
			public float Width;
			private float[] m_Pattern;
			private DashStyle m_Style = DashStyle.Solid;
			private LineCap m_EndCap = LineCap.Flat;
			private float m_MiterLimit = 10.0F;
			private LineJoin m_Join = LineJoin.Miter;
			public const double DEFAULTMITER = 10.0F; // default used by .net pens (differs from SVG - added to SVG element to get global default - hope it takes effect)

			public SVGStroke(Color colour, float width = 1)
			{
				m_Colour = colour;
				Width = width;
			}

			public override Color Color
			{ get { return m_Colour; } }

			public override float[] DashPattern
			{ get { return m_Pattern; } set { m_Pattern = value; } }

			public override DashStyle DashStyle
			{
				get { return m_Style; }
				set { m_Style = value; }
			}

			public override LineCap EndCap
			{
				get { return m_EndCap; }
				set { m_EndCap = value; }
			}

			public override LineJoin LineJoin
			{
				get { return m_Join; }
				set { m_Join = value; }
			}

			public override float MiterLimit
			{
				get { return m_MiterLimit; }
				set { m_MiterLimit = value; }
			}

			public string GetStyle()
			{
				if (m_Colour.IsEmpty)
					return "stroke: none; ";
				StringBuilder sb = new StringBuilder(50);
				sb.AppendSVGColour(m_Colour, "stroke");
				if (Width != 1)
					sb.Append("stroke-width: ").Append(Width).Append("; ");
				// unfortunately illustrator does not correctly scale this due to viewBox.  Adding units doesn't help as the units are scaled by viewbox (so saying 1mm would actually give 3mm ish)
				// Firefox does render correctly, as does OpenDraw.
				// I guess only solution would be to add: http://stackoverflow.com/questions/22297653/stroke-width-within-viewbox

				if (m_Style != DashStyle.Solid)
				{
					sb.Append("stroke-dasharray: ");
					switch (m_Style)
					{
						case DashStyle.Dash: sb.Append("7 3 "); break;
						case DashStyle.Dot: sb.Append("2 3 "); break;// Even with this the dots look longer than the spaces
						case DashStyle.DashDot: sb.Append("7 3 2 3 "); break;
						case DashStyle.DashDotDot: sb.Append("7 3 2 3 2 3 "); break;
						case DashStyle.Custom: sb.Append(string.Join(" ", m_Pattern)); break;
						default:
							throw new ArgumentException("DashStyle");
					}
					sb.Append(";");
				}
				if (m_Join != LineJoin.Miter && m_Join != LineJoin.MiterClipped) // miter is default; miter-clipped not supported
				{
					sb.Append("stroke-linejoin: ");
					switch (m_Join)
					{
						case LineJoin.Bevel: sb.Append("bevel; "); break;
						case LineJoin.Round: sb.Append("round; "); break;
						default:
							throw new ArgumentException("LineJoin");
					}
				}
				if (m_EndCap != LineCap.Flat)
				{
					sb.Append("stroke-linecap: ");
					switch (m_EndCap)
					{
						case LineCap.Round:
						case LineCap.RoundAnchor:
							sb.Append("round; ");
							break;
						case LineCap.Square:
						case LineCap.SquareAnchor:
							sb.Append("square; ");
							break;
						default:
							Utilities.LogSubError("Unexpected stroke-linecap: " + m_EndCap);
							sb.Append("inherit; ");
							break;
					}
				}
				if (m_MiterLimit != 10.0F)
					sb.Append("stroke-miterlimit: ").Append(m_MiterLimit.ToString("0.#")).Append("; ");
				return sb.ToString();
			}
		}

		private abstract class SVGFill : Fill
		{

			/// <summary>Returns the contents of the style attribute relevant to the fill (there may also be stroke information added).  Must end in space</summary>
			public abstract string GetStyle();

		}

		private sealed class SVGSolidFill : SVGFill
		{

			private readonly Color m_Colour;

			public SVGSolidFill(Color colour)
			{
				m_Colour = colour;
			}

			public override bool IsTransparent
			{ get { return m_Colour.A < 255; } }

			public override string GetStyle()
			{
				StringBuilder sb = new StringBuilder(50);
				sb.AppendSVGColour(m_Colour, "fill");
				sb.Append(" ");
				return sb.ToString();
			}

		}

		private class SVGLinearGradient : SVGFill
		{

			private readonly Color m_ColourA;
			private readonly Color m_ColourB;
			private readonly SVGCanvas Canvas;
			private readonly PointF m_PointA;
			private readonly PointF m_PointB;
			/// <summary>assigned the first time that this is rendered (using the m_intNextID from the canvas) </summary>
			private int m_ID = -1;

			public SVGLinearGradient(Color colourA, Color colourB, SVGCanvas canvas, PointF pointA, PointF pointB)
			{
				m_ColourA = colourA;
				m_ColourB = colourB;
				this.Canvas = canvas;
				m_PointA = pointA;
				m_PointB = pointB;
			}

			public override string GetStyle()
			{
				if (m_ID < 0)
				{
					m_ID = Canvas.m_NextID;
					Canvas.m_NextID += 1;
				}
				string ID = "Gradient" + m_ID;
				if (!Canvas.m_Gradients.Contains(ID))
				{
					XElement gradient = new XElement("linearGradient", new XAttribute("id", ID), new XAttribute("gradientUnits", "userSpaceOnUse"), new XAttribute("x1", m_PointA.X), new XAttribute("y1", m_PointA.Y), new XAttribute("x2", m_PointB.X), new XAttribute("y2", m_PointB.Y), new XElement("stop", new XAttribute("offset", "0%"), new XAttribute("style", m_ColourA.ToSVGWithA("stop-color"))), new XElement("stop", new XAttribute("offset", "100%"), new XAttribute("style", m_ColourB.ToSVGWithA("stop-color"))));
					Canvas.m_Definitions.Add(gradient);
					Canvas.m_Gradients.Add(ID);
				}
				return "fill: url(#" + ID + "); ";
			}

			public override bool IsTransparent
			{ get { return m_ColourA.A < 255 || m_ColourB.A < 255; } }

		}

		private class SVGRadialGradient : SVGFill
		{

			private readonly Color m_CentreColour;
			private readonly Color m_EdgeColour;
			private readonly PointF m_CentrePoint;
			private readonly float m_Radius;
			private readonly SVGCanvas Canvas;
			private int m_ID = -1;

			public SVGRadialGradient(Color centreColour, Color edgeColour, PointF centrePoint, float radius, SVGCanvas canvas)
			{
				m_CentreColour = centreColour;
				m_EdgeColour = edgeColour;
				m_CentrePoint = centrePoint;
				m_Radius = radius;
				Canvas = canvas;
			}

			public override string GetStyle()
			{
				if (m_ID < 0)
				{
					m_ID = Canvas.m_NextID;
					Canvas.m_NextID += 1;
				}
				string gradientID = "Gradient" + m_ID;
				if (!Canvas.m_Gradients.Contains(gradientID))
				{
					XElement gradient = new XElement("radialGradient", new XAttribute("id", gradientID), new XAttribute("gradientUnits", "userSpaceOnUse"), new XAttribute("cx", m_CentrePoint.X), new XAttribute("cy", m_CentrePoint.Y), new XAttribute("r", m_Radius), new XElement("stop", new XAttribute("offset", "0%"), new XAttribute("style", m_CentreColour.ToSVGWithA("stop-color"))), new XElement("stop", new XAttribute("offset", "100%"), new XAttribute("style", m_EdgeColour.ToSVGWithA("stop-color"))));
					Canvas.m_Definitions.Add(gradient);
					Canvas.m_Gradients.Add(gradientID);
				}
				return "fill: url(#" + gradientID + "); ";
			}

			public override bool IsTransparent
			{ get { return m_CentreColour.A < 255 || m_EdgeColour.A < 255; } }

		}

		private class SVGHatched : SVGFill
		{

			private readonly Color m_BackgroundColour;
			private readonly Color m_ForegroundColour;
			private readonly Shape.FillStyleC.Patterns m_Pattern;
			private readonly SVGCanvas Canvas;

			public SVGHatched(Color backgroundColour, Color foregroundColour, Shape.FillStyleC.Patterns pattern, SVGCanvas canvas)
			{
				m_BackgroundColour = backgroundColour;
				m_ForegroundColour = foregroundColour;
				m_Pattern = pattern;
				Canvas = canvas;
			}

			public override string GetStyle()
			{
				string ID = "Pattern" + (int)m_Pattern + "_" + m_ForegroundColour.ToWebNumber();
				if (!Canvas.m_Patterns.Contains(ID))
				{
					XElement pattern = new XElement("pattern", new XAttribute("id", ID), new XAttribute("patternUnits", "userSpaceOnUse"), new XAttribute("x", "0"), new XAttribute("y", "0"), new XAttribute("width", "2"), new XAttribute("height", "2"));

					StringBuilder sb = new StringBuilder(50);
					sb.AppendSVGColour(m_ForegroundColour, "stroke");
					//If Width <> 1 Then sb.Append("stroke-width: ").Append(Width).Append("; ")

					XElement graphics = new XElement("g", new XAttribute("style", "stroke-width: 0.2; stroke-linecap: butt; fill: none; " + sb));
					pattern.Add(graphics);
					switch (m_Pattern)
					{
						case Shape.FillStyleC.Patterns.DiagonalCross:
							graphics.Add(new XElement("path", new XAttribute("d", "M0,0 l2,2")));
							graphics.Add(new XElement("path", new XAttribute("d", "M2,0 l-2,2")));
							break;
						case Shape.FillStyleC.Patterns.ForwardDiagonal:
							graphics.Add(new XElement("path", new XAttribute("d", "M0,0 l2,2")));
							break;
						case Shape.FillStyleC.Patterns.BackwardDiagonal:
							graphics.Add(new XElement("path", new XAttribute("d", "M2,0 l-2,2")));
							break;
						case Shape.FillStyleC.Patterns.Cross:
							graphics.Add(new XElement("path", new XAttribute("d", "M1,0 l0,2")));
							graphics.Add(new XElement("path", new XAttribute("d", "M0,1 l2,0")));
							break;
						case Shape.FillStyleC.Patterns.Horizontal:
							graphics.Add(new XElement("path", new XAttribute("d", "M0,1 l2,0")));
							break;
						case Shape.FillStyleC.Patterns.Vertical:
							graphics.Add(new XElement("path", new XAttribute("d", "M1,0 l0,2")));
							break;
						default:
							throw new InvalidOperationException("unexpected hatching pattern: " + m_Pattern);
					}
					Canvas.m_Definitions.Add(pattern);
					Canvas.m_Patterns.Add(ID);
				}
				return "fill: url(#" + ID + "); ";
			}

			public override bool IsTransparent
			{ get { return m_BackgroundColour.A < 255; } }

		}

		private class SVGTexture : SVGFill
		{

			private readonly string m_ImageID;
			private readonly Size m_ImageSize;
			private readonly SVGCanvas Canvas;

			public SVGTexture(string imageID, Size imageSize, Canvas canvas)
			{
				m_ImageID = imageID;
				m_ImageSize = imageSize;
				Canvas = (SVGCanvas)canvas;
			}

			public override string GetStyle()
			{
				string ID = "Texture" + m_ImageID;
				if (!Canvas.m_Patterns.Contains(ID))
				{
					const float scale = Geometry.INCH / 96;
					XElement pattern = new XElement("pattern",
						new XAttribute("id", ID),
						new XAttribute("patternUnits", "userSpaceOnUse"),
						new XAttribute("x", "0"),
						new XAttribute("y", "0"),
						new XAttribute("width", m_ImageSize.Width * scale),
						new XAttribute("height", m_ImageSize.Height * scale));

					pattern.Add(new XElement("use",
						new XAttribute(xlinkNamespace + "href", "#" + m_ImageID),
						new XAttribute("x", 0),
						new XAttribute("y", 0),
						new XAttribute("transform", "scale(" + scale + "," + scale + ")")));
					Canvas.m_Definitions.Add(pattern);
					Canvas.m_Patterns.Add(ID);
				}
				return "fill: url(#" + ID + "); ";
			}

			public override bool IsTransparent
			{ get { return false; } }

		}

		#endregion

		#region Stroke and Fill creation

		public override Fill CreateFill(Color colour)
		{
			return new SVGSolidFill(colour);
		}

		public override Fill CreateHatchBrush(Color foreground, Color background, Shape.FillStyleC.Patterns pattern)
		{
			return new SVGHatched(background, foreground, pattern, this);
		}

		public override Fill CreateLinearGradientFill(PointF A, PointF B, Color colA, Color colB)
		{
			return new SVGLinearGradient(colA, colB, this, A, B);
		}

		public override Fill CreateRadialGradientBrush(PointF centre, float radius, Color centreColour, Color edge, RectangleF bounds)
		{
			return new SVGRadialGradient(centreColour, edge, centre, radius, this);
		}

		public override Stroke CreateStroke(Color col, float width = 1.0f)
		{
			return new SVGStroke(col, width);
		}

		public override Fill CreateTextureBrush(SharedImage image, float scale)
		{
			var netImage = image.GetNetImage();
			string ID = PrepareImage((Bitmap)netImage, image.GetHashCode(), RectangleF.Empty);
			// This uses a different image key to the actual image insertion; but it is unlikely that the same image would be used both as a texture and an image
			// and it is not the end of the world if the image gets embedded twice
			return new SVGTexture(ID, netImage.Size, this);
		}

		private static readonly SVGSolidFill Black = new SVGSolidFill(Color.Black);
		private static readonly SVGSolidFill White = new SVGSolidFill(Color.White);
		private static readonly SVGSolidFill Blue = new SVGSolidFill(Color.Blue);
		public override Fill BlackBrush
		{ get { return Black; } }
		public override Fill WhiteBrush
		{ get { return White; } }
		public override Fill BlueBrush
		{ get { return Blue; } }

		#endregion

		#region Information
		public override float DpiX
		{ get { return 96; } } // Used to get single pixel lines; so probably best to use something resembling a screen resolution

		public override float DpiY
		{ get { return 96; } }

		#endregion

		#region Transformations and Clipping
		private RectangleF m_CurrentClip = new RectangleF(-10000, -10000, 20000, 20000); // cannot use BigRectangle because that doesn't include negative Y
		private readonly Stack<RectangleF> m_ClipStack = new Stack<RectangleF>();

		public override RectangleF ClipBounds
		{ get { return m_CurrentClip; } }

		public override void IntersectClip(RectangleF rect)
		{
			m_ClipStack.Push(m_CurrentClip);
			rect.Intersect(m_CurrentClip);

			XElement clip = new XElement("clipPath", new XAttribute("id", "clip" + m_NextID));
			clip.Add(new XElement("rect", new XAttribute("x", rect.X), new XAttribute("y", rect.Y), new XAttribute("width", rect.Width), new XAttribute("height", rect.Height)));
			m_Definitions.Add(clip);
			StartGroup(new XElement("g", new XAttribute("style", "clip-path: url(#clip" + m_NextID + ");")));
			m_NextID += 1;
		}

		public override void RestoreClip()
		{
			m_Container = m_stkContainers.Pop();
			m_CurrentClip = m_ClipStack.Pop();
		}

		public override void IntersectClip(GraphicsPath path)
		{
			m_ClipStack.Push(m_CurrentClip);
			// just assumed (hope) path is inside any existing clip
			m_CurrentClip = path.GetBounds();

			XElement clip = new XElement("clipPath", new XAttribute("id", "clip" + m_NextID));
			clip.Add(new XElement("path", new XAttribute("d", GetSVGPath(path))));
			m_Definitions.Add(clip);
			StartGroup(new XElement("g", new XAttribute("style", "clip-path: url(#clip" + m_NextID + ");")));
			m_NextID += 1;
		}


		public override void MultiplyTransform(Matrix matrix, MatrixOrder order)
		{
			StartGroup(new XElement("g", new XAttribute("transform", "matrix(" + string.Join(",", matrix.Elements) + ")")));
		}

		public override void TranslateTransform(float X, float Y)
		{
			StartGroup(new XElement("g", new XAttribute("transform", "translate(" + X + "," + Y + ")")));
		}

		public override void EndTransform()
		{
			m_Container = m_stkContainers.Pop();
		}


		public override void StartGroup()
		{
			StartGroup(new XElement("g"));
		}
		private void StartGroup(XElement groupElement)
		{
			m_Container.Add(groupElement);
			m_stkContainers.Push(m_Container);
			m_Container = groupElement;
		}

		public override void EndGroup()
		{
			m_Container = m_stkContainers.Pop();
		}

		#endregion

		#region Text
		public override void DrawString(string text, Font font, Fill fill, PointF start, StringFormat format = null)
		{
			StringFormat create;
			if (format == null)
				create = new StringFormat();
			else
				create = (StringFormat)format.Clone();
			// Presumably the alignment starts from the point, and does not centre on the point
			create.LineAlignment = StringAlignment.Near;
			create.Alignment = StringAlignment.Near;
			DrawString(text, font, fill, new RectangleF(start.X, start.Y, 10000, 10000), create);
		}

		public override void DrawString(string text, Font font, Fill fill, RectangleF layoutRect, StringFormat format)
		{
			// SVG doesn't do multi-line text, although it does do left centre and right alignment (and justification if we wanted it)
			// therefore we must measure the lines independently.
			// Right alignment isn't entirely working, but that is actually done in SVG, and we are providing the same right-hand coordinates
			// illustrator shows the text too small; (21 not 26), but that is probably mostly a viewport problem.  Firefox and OpenDraw are closer, but still seem
			// just a little smaller than expected
			if (string.IsNullOrEmpty(text))
				return;
			Graphics gr = NetCanvas.MeasurementInstance.Underlying;
			List<string> lines = new List<string>();

			float sizeConversion = font.Size / font.FontFamily.GetEmHeight(FontStyle.Regular); // this is the only way to determine the conversion from the font's internal units to reality
			float Y = layoutRect.Top + font.FontFamily.GetCellAscent(FontStyle.Regular) * sizeConversion;
			do
			{
				int chars;
				gr.MeasureString(text, font, layoutRect.Size, GUIUtilities.StringFormatMeasureLine, out chars, out _);
				if (chars == 0)
					chars = 1; // otherwise we get stuck!
				lines.Add(text.Substring(0, chars).TrimEnd(' '.ToString().ToCharArray()));
				text = text.Substring(chars);
			} while (!string.IsNullOrEmpty(text));
			float lineHeight = font.Size; // conversion from points to pixels hopefully
			switch (format.LineAlignment)
			{
				case StringAlignment.Near:
					break;
				case StringAlignment.Center:
					Y += (layoutRect.Height - lines.Count * (lineHeight + 1)) / 2; // centre vertically
					break;
				case StringAlignment.Far:
					Y += layoutRect.Height - lines.Count * (lineHeight + 1);
					break;
				default:
					throw new ArgumentException("objFormat.LineAlignment");
			}
			string anchor;
			float X; // X coordinate to which to anchor
			switch (format.Alignment)
			{
				case StringAlignment.Near:
					anchor = "start";
					X = layoutRect.X;
					break;
				case StringAlignment.Center:
					anchor = "middle";
					X = layoutRect.Centre().X;
					break;
				case StringAlignment.Far:
					anchor = "end";
					X = layoutRect.Right;
					break;
				default:
					throw new ArgumentException("format.Alignment");
			}
			StringBuilder style = new StringBuilder();
			style.Append("font-size: ").Append(font.Size.ToString()).Append("pt; text-anchor: ").Append(anchor).Append("; ");
			style.Append("font-family: ").Append(font.FontFamily.Name).Append("; ");
			if ((font.Style & FontStyle.Italic) > 0)
				style.Append("font-style:italic; ");
			if ((font.Style & FontStyle.Bold) > 0)
				style.Append("font-weight:bold; ");
			if ((font.Style & FontStyle.Underline) > 0)
				style.Append("text-decoration: underline; ");
			if ((font.Style & FontStyle.Strikeout) > 0)
				style.Append("text-decoration: line-through; ");
			XElement textElement = new XElement("text", new XAttribute("style", style.ToString()), new XAttribute("x", X), new XAttribute("y", Y));
			for (int line = 0; line <= lines.Count - 1; line++)
			{
				textElement.Add(new XElement("tspan", lines[line], new XAttribute("x", X), new XAttribute("y", Y)));
				Y += lineHeight + 1; // +1 empirically ??
			}
			m_Container.Add(textElement);
		}

		public override SizeF MeasureString(string text, Font font, float width = 100000f, StringFormat format = null)
		{
			throw new InvalidOperationException(); // I don't think this is going to be needed
		}

		public override RectangleF MeasureTextExact(string text, Font font, RectangleF bounds, StringFormat format, int includeCharacters = int.MaxValue)
		{
			throw new InvalidOperationException(); // I don't think this is going to be needed
		}


		#endregion

		#region Shapes
		private static XAttribute GetStyleAttribute(SVGStroke stroke, SVGFill fill)
		{
			// Returns the "style" attribute which can be added to most elements
			string str;
			if (fill == null)
				str = "fill:none; ";
			else
				str = fill.GetStyle() + " ";
			if (stroke != null)
				str += stroke.GetStyle();
			return new XAttribute("style", str);
		}

		public override void DrawArc(RectangleF circleRect, float startAngle, float endAngle, Stroke stroke = null)
		{
			StringBuilder path = new StringBuilder();
			float radius = circleRect.Width / 2;
			PointF pt1 = circleRect.Centre() + Geometry.ScalarToVector(radius, startAngle); // MyBase.GetArcCoord(circleRect.Centre, radius, sngStartAngle)
			path.Append("M").Append(pt1.X).Append(",").Append(pt1.Y).Append(" ");
			path.Append("A").Append(radius).Append(",").Append(radius).Append(" 0 ").Append(endAngle - startAngle > Geometry.PI ? "1" : "0").Append(" 1 ");
			PointF pt2 = circleRect.Centre() + Geometry.ScalarToVector(radius, endAngle);
			path.Append(pt2.X).Append(",").Append(pt2.Y);
			m_Container.Add(new XElement("path", GetStyleAttribute((SVGStroke)stroke, null), new XAttribute("d", path.ToString())));
		}

		public override void DrawBezier(PointF pt0, PointF pt1, PointF pt2, PointF pt3, Stroke stroke)
		{
			StringBuilder path = new StringBuilder();
			path.Append("M").Append(pt0.X).Append(",").Append(pt0.Y).Append(" ");
			path.Append("C").Append(pt1.X).Append(",").Append(pt1.Y).Append(" ");
			path.Append(pt2.X).Append(",").Append(pt2.Y).Append(" ");
			path.Append(pt3.X).Append(",").Append(pt3.Y);
			m_Container.Add(new XElement("path", GetStyleAttribute((SVGStroke)stroke, null), new XAttribute("d", path.ToString())));
		}

		public override void DrawLine(float X1, float Y1, float X2, float Y2, Stroke stroke)
		{
			m_Container.Add(
				new XElement("line", new XAttribute("x1", X1),
				new XAttribute("y1", Y1), new XAttribute("x2", X2), new XAttribute("y2", Y2),
				new XAttribute("style", ((SVGStroke)stroke).GetStyle())));
		}

		public override void DrawLine(PointF ptA, PointF ptB, Stroke stroke)
		{
			m_Container.Add(new XElement("line",
				new XAttribute("x1", ptA.X), new XAttribute("y1", ptA.Y),
				new XAttribute("x2", ptB.X), new XAttribute("y2", ptB.Y), GetStyleAttribute((SVGStroke)stroke, null)));
		}

		public override void DrawLines(PointF[] points, Stroke stroke)
		{
			StringBuilder path = new StringBuilder(); // the path in SVG format
			path.Append("M").Append(points[0].X).Append(",").Append(points[0].Y).Append(" ");
			for (int index = 1; index <= points.Length - 1; index++)
			{
				path.Append("L").Append(points[index].X).Append(",").Append(points[index].Y).Append(" ");
			}
			m_Container.Add(new XElement("path", GetStyleAttribute((SVGStroke)stroke, null), new XAttribute("d", path.ToString())));
		}

		public override void Ellipse(RectangleF circleRect, Stroke stroke = null, Fill fill = null)
		{
			m_Container.Add(new XElement("ellipse",
				new XAttribute("cx", circleRect.Centre().X),
				new XAttribute("cy", circleRect.Centre().Y),
				new XAttribute("rx", circleRect.Width / 2),
				new XAttribute("ry", circleRect.Height / 2),
				GetStyleAttribute((SVGStroke)stroke, (SVGFill)fill)));
		}

		public override void FillPie(RectangleF circleRect, float startAngle, float endAngle, Fill fill = null)
		{
			StringBuilder path = new StringBuilder();
			path.Append("M").Append(circleRect.Centre().X).Append(",").Append(circleRect.Centre().Y).Append(" ");
			float radius = circleRect.Width / 2;
			PointF pt1 = circleRect.Centre() + Geometry.ScalarToVector(radius, startAngle); // MyBase.GetArcCoord(circleRect.Centre, radius, sngStartAngle)
			path.Append("L").Append(pt1.X).Append(",").Append(pt1.Y).Append(" ");
			path.Append("A").Append(radius).Append(",").Append(radius).Append(" 0 ").Append(endAngle - startAngle > Geometry.PI ? "1" : "0").Append(" 1 ");
			PointF pt2 = circleRect.Centre() + Geometry.ScalarToVector(radius, endAngle);
			path.Append(pt2.X).Append(",").Append(pt2.Y).Append(" Z ");
			m_Container.Add(new XElement("path", GetStyleAttribute(null, (SVGFill)fill), new XAttribute("d", path.ToString())));
		}

		protected PointF GetArcCoord(PointF centre, float radius, float angleRadians)
		{
			return new PointF(centre.X + radius * (float)Math.Cos(angleRadians), centre.Y + radius * (float)Math.Sin(angleRadians)); // * 180 / Misc.PI
		}

		public override void Path(GraphicsPath path, Stroke stroke = null, Fill fill = null)
		{
			m_Container.Add(new XElement("path", GetStyleAttribute((SVGStroke)stroke, (SVGFill)fill), new XAttribute("d", GetSVGPath(path))));
		}

		/// <summary>Returns the string of the SVG path info (M L C and Z codes)</summary>
		private string GetSVGPath(GraphicsPath path)
		{
			StringBuilder output = new StringBuilder(); // the path in SVG format
			PointF[] points = path.PathPoints;
			byte[] types = path.PathTypes;
			for (int index = 0; index <= points.Length - 1; index++)
			{
				if ((types[index] & Lined.PATHTYPEMASK) == Lined.PATHSTART)
					output.Append("M").Append(points[index].X).Append(",").Append(points[index].Y).Append(" ");
				else if ((types[index] & Lined.PATHTYPEMASK) == Lined.PATHLINE)
					output.Append("L").Append(points[index].X).Append(",").Append(points[index].Y).Append(" ");
				else if ((types[index] & Lined.PATHTYPEMASK) == Lined.PATHBEZIER)
				{
					output.Append("C").Append(points[index].X).Append(",").Append(points[index].Y).Append(" ");
					output.Append(points[index + 1].X).Append(",").Append(points[index + 1].Y).Append(" ");
					output.Append(points[index + 2].X).Append(",").Append(points[index + 2].Y).Append(" ");
					index += 2; // The for next loop will increment it by one automatically
				}
				if ((types[index] & Lined.PATHCLOSUREFLAG) > 0)
					output.Append("Z "); // close path
			}
			return output.ToString();
		}

		public override void Polygon(PointF[] points, Stroke stroke, Fill fill)
		{
			StringBuilder path = new StringBuilder(); // the path in SVG format
			path.Append("M").Append(points[0].X).Append(",").Append(points[0].Y).Append(" ");
			for (int index = 1; index <= points.Length - 1; index++)
			{
				path.Append("L").Append(points[index].X).Append(",").Append(points[index].Y).Append(" ");
			}
			path.Append("Z "); // close path
			m_Container.Add(new XElement("path", GetStyleAttribute((SVGStroke)stroke, (SVGFill)fill), new XAttribute("d", path.ToString())));
		}

		public override void Rectangle(RectangleF rct, Stroke stroke = null, Fill fill = null)
		{
			m_Container.Add(new XElement("rect",
				new XAttribute("x", rct.X),
				new XAttribute("y", rct.Y),
				new XAttribute("width", rct.Width),
				new XAttribute("height", rct.Height),
				GetStyleAttribute((SVGStroke)stroke, (SVGFill)fill)));
		}

		#endregion

		#region Other
		// This always embeds the image as pixel data.  The definitions are shared, using the hash code of the MemoryImage more resource name to identify it
		// This currently ignores any transparency.  I'm not sure transparency is actually used in documents?  Only used when editing

		private string PrepareImage(Bitmap img, int imageKey, RectangleF source)
		{
			string ID = "img" + imageKey.ToString("x");
			if (!source.IsEmpty && !source.Equals(img.Bounds().ToRectangleF()))
				ID += "_" + source.GetHashCode();
			if (!m_Images.Contains(ID))
			{
				// This always resave the image.  This is rather inefficient if it was already a MemoryImage, but does ensure that it is in PNG format
				if (!source.IsEmpty && !source.Equals(img.Bounds().ToRectangleF()))
				{
					Bitmap newImage = new Bitmap((int)source.Width, (int)source.Height);
					using (Graphics gr = Graphics.FromImage(newImage))
					{
						gr.Clear(Color.Transparent);
						gr.DrawImage(img, newImage.Bounds().ToRectangleF(), source, GraphicsUnit.Pixel);
						img = newImage;
					}
				}
				using (System.IO.MemoryStream stream = new System.IO.MemoryStream(2000))
				{
					img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					AddImageToDefinitions(stream.ToArray(), ID, img.Size, "png");
				}
			}
			return ID;
		}

		private string PrepareMemoryImage(MemoryImage img, int imageKey, RectangleF source)
		{
			if (!img.IsSVG)
				return PrepareImage((Bitmap)img.GetNetImage(true), imageKey, source);

			// rest for SVG only, which partially repeats PrepareImage (above)
			string ID = "img" + imageKey.ToString("x");
			RectangleF bounds = new RectangleF(PointF.Empty, img.Size);
			if (!source.IsEmpty && !source.Equals(bounds))
				ID += "_" + source.GetHashCode();
			if (!m_Images.Contains(ID))
			{ // for SVG images can just retrieve the data directly from the memory image (no need to worry about formats)
				Debug.Assert(img.Buffer != null);
				AddImageToDefinitions(img.Buffer, ID, img.Size, "svg+xml");
			}
			return ID;
		}

		/// <summary>Adds the given image to definitions, where buffer is the byte data of the saved image</summary>
		private void AddImageToDefinitions(byte[] buffer, string ID, Size size, string imageFormat)
		{
			XElement element = new XElement("image",
				new XAttribute("id", ID),
				new XAttribute("width", size.Width),
				new XAttribute("height", size.Height),
				new XAttribute(xlinkNamespace + "href", "data:image/" + imageFormat + ";base64," + Convert.ToBase64String(buffer)));
			m_Images.Add(ID);
			m_Definitions.Add(element);
		}


		private void SimpleImage(string ID, SizeF original, RectangleF destination)
		{
			// renders the image in a rectangle.  The complication is that SVG ignores any width and height on the use element.  A transform is needed
			// to transform from the original size of the image to the desired size
			float scale = destination.Width / original.Width;
			Debug.Assert(Math.Abs(destination.Height / original.Height - scale) < Geometry.NEGLIGIBLE);
			// Unfortunately the destination coordinates also gets scaled, so we need to apply the reverse to the coordinates stored in the file
			m_Container.Add(new XElement("use",
				new XAttribute(xlinkNamespace + "href", "#" + ID),
				new XAttribute("x", destination.X / scale),
				new XAttribute("y", destination.Y / scale),
				new XAttribute("transform", "scale(" + scale + "," + scale + ")")));
		}

		private void ComplexImage(string ID, RectangleF source, PointF[] destinationPoints)
		{
			// Version of the above for rotated images
			// first the v useful matrix constructor can tell us what the required matrix is... :-)
			using (Matrix m = new Matrix(source, destinationPoints))
			m_Container.Add(new XElement("use",
				new XAttribute(xlinkNamespace + "href", "#" + ID),
				new XAttribute("x", 0),
				new XAttribute("y", 0),
				new XAttribute("transform", "matrix(" + m.Elements[0] + "," + m.Elements[1] + "," + m.Elements[2] + "," + m.Elements[3] + "," + m.Elements[4] + "," + m.Elements[5] + ")")));
		}


		public override void DrawImage(MemoryImage image, PointF[] destinationPoints, RectangleF sourceRect, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			var ID = PrepareMemoryImage(image, image.GetHashCode(), sourceRect);
			ComplexImage(ID, sourceRect, destinationPoints);
		}

		public override void DrawImage(MemoryImage image, RectangleF destination, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			string ID = PrepareMemoryImage(image, image.GetHashCode(), RectangleF.Empty);
			SimpleImage(ID, image.Size, destination);
		}

		public override void DrawImage(MemoryImage image, RectangleF destination, RectangleF sourceRect, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			if (image.IsSVG && sourceRect.Location.IsEmpty && sourceRect.Size.Equals(image.Size.ToSizeF()))
			{ // sourceRect param was actually redundant as it specifies the entire thing
			  // this check done for efficiency, since if rendering the entire thing we can use SVG, whereas code below generates an entire new image each time
				DrawImage(image, destination, attributes);
				return;
			}
			string ID = PrepareMemoryImage(image, image.GetHashCode(), sourceRect);
			SimpleImage(ID, sourceRect.Size, destination);
		}

		public override void DrawImage(string resourceName, int preferredSize, PointF[] destinationPoints, RectangleF sourceRect, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			string ID = PrepareImage((Bitmap)image, resourceName.GetHashCode(), RectangleF.Empty);
			ComplexImage(ID, sourceRect, destinationPoints);
		}

		public override void DrawImage(string resourceName, int preferredSize, RectangleF destination, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			string ID = PrepareImage((Bitmap)image, resourceName.GetHashCode(), RectangleF.Empty);
			SimpleImage(ID, image.Size, destination);
		}

		public override void DrawImage(string resourceName, int preferredSize, RectangleF destination, RectangleF sourceRect, System.Drawing.Imaging.ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			string ID = PrepareImage((Bitmap)image, resourceName.GetHashCode(), sourceRect);
			SimpleImage(ID, sourceRect.Size, destination);
		}

		#endregion

	}

	internal static class SVGExtensions
	{
		/// <summary>Adds style string for colour for this context</summary>
		/// <param name="col"></param>
		/// <param name="context">eg 'stroke' or 'fill'</param>
		/// <param name="sb"></param>
		/// <remarks>Includes final ;</remarks>
		public static StringBuilder AppendSVGColour(this StringBuilder sb, Color col, string context)
		{
			sb.Append(context).Append(": ");
			if (col.IsEmpty)
				sb.Append("none; ");
			else
			{
				sb.Append("#").Append((col.ToArgb() & 0xFFFFFF).ToString("x").PadLeft(6, '0')).Append("; ");
				if (col.A < 255)
					sb.Append(context).Append("-opacity: ").Append((col.A / 255).ToString("0.##")).Append("; ");
			}
			return sb;
		}

		/// <summary>Returns rrggbb string for colour (without #)</summary>
		public static string ToWebNumber(this Color colour)
		{
			return (colour.ToArgb() & 0xFFFFFF).ToString("x").PadLeft(6, '0');
		}

		/// <summary>Creates the complete "fill:xxx; fill-opacity:yyy;" for a given colour where context = "fill".
		/// context should include the "-color" if that is needed on the colour itself; it will automatically be omitted on the opacity</summary>
		public static string ToSVGWithA(this Color colour, string context)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(context).Append(": ");
			if (colour.IsEmpty)
				sb.Append("none; ");
			else
			{
				sb.Append("#").Append((colour.ToArgb() & 0xFFFFFF).ToString("x").PadLeft(6, '0')).Append("; ");
				if (colour.A < 255)
					sb.Append(context.Replace("-color", "")).Append("-opacity: ").Append((colour.A / 255).ToString("0.##")).Append("; ");
			}
			return sb.ToString();
		}
	}
}
