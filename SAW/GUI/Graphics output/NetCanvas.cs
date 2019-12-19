using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace SAW
{
	/// <summary>Disposing this does not expose the underlying graphics.  It is not essential that this class is disposed</summary>
	/// <remarks></remarks>
	public class NetCanvas : Canvas
	{

		private Graphics gr;

		public readonly bool IsPrinting;

		public NetCanvas(Graphics gr, bool isPrinting = false)
		{
			this.gr = gr;
			IsPrinting = isPrinting;
		}

		protected override void Dispose(bool disposing)
		{
			Debug.Assert(m_rgnOriginalClip == null, "RestoreClip was not called after IntersectClip");
			base.Dispose(disposing);
			//If Not gr Is Nothing Then gr.Dispose()
			gr = null;
		}

		#region Stroke and Fill classes
		private class NetStroke : Stroke
		{

			public  Pen Pen;

			public override Color Color
			{ get { return Pen.Color; } }

			public override DashStyle DashStyle
			{
				get { return Pen.DashStyle; }
				set { Pen.DashStyle = value; }
			}

			public override LineJoin LineJoin
			{
				get { return Pen.LineJoin; }
				set { Pen.LineJoin = value; }
			}

			/// <summary>Both start and end.  .net allows them to differ, but SVG doesn't</summary>
			public override LineCap EndCap
			{
				get { return Pen.EndCap; }
				set { Pen.EndCap = value; Pen.StartCap = value; }
			}

			public override float MiterLimit
			{
				get { return Pen.MiterLimit; }
				set { Pen.MiterLimit = value; }
			}

			public override float[] DashPattern
			{
				get { return Pen.DashPattern; }
				set { Pen.DashPattern = value; }
			}

			protected override void Dispose(bool disposing)
			{
				Pen?.Dispose();
				Pen = null;
			}
		}

		private class NetFill : Fill
		{

			public readonly Brush Brush;

			public NetFill(Brush objBrush)
			{
				this.Brush = objBrush;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				Brush.Dispose();
			}

			public override bool IsTransparent
			{
				get
				{
					if (!(Brush is SolidBrush))
						return false;
					return ((SolidBrush)Brush).Color.A < 0xFF;
				}
			}
		}
		#endregion

		#region Stroke/Fill creation
		public override Fill CreateFill(Color colour)
		{
			return new NetFill(new SolidBrush(colour));
		}

		/// <summary>Static version of the usual Canvas.CreateFill</summary>
		public static Fill CreateSolidFill(Color col)
		{
			return new NetFill(new SolidBrush(col));
		}

		public override Fill CreateLinearGradientFill(PointF A, PointF B, Color colA, Color colB)
		{
			return new NetFill(new LinearGradientBrush(A, B, colA, colB));
		}

		public override Fill CreateTextureBrush(SharedImage image, float scale)
		{
			try
			{
				TextureBrush create = new TextureBrush(image.GetNetImage());
				Matrix matrix = create.Transform; // cannot do Brush.Transform.Scale because Transform returns a copy of the matrix
				matrix.Scale(scale, scale);
				create.Transform = matrix;
				return new NetFill(create);
			}
			catch (Exception ex)
			{
				// If this fails it returns a hatched brush
				Utilities.LogSubError(ex);
				return new NetFill(new HatchBrush(HatchStyle.DiagonalCross, Color.Red, Color.White));
			}
		}

		public override Fill CreateHatchBrush(Color foreground, Color background, Shape.FillStyleC.Patterns pattern)
		{
			// The patterns from the FillStyleC are compatible with the windows enum
			return new NetFill(new HatchBrush((HatchStyle)pattern, foreground, background));
		}

		public override Fill CreateRadialGradientBrush(PointF centre, float radius, Color centreColour, Color edge, RectangleF bounds)
		{
			// Last parameter just used by PDF
			using (GraphicsPath objPath = new GraphicsPath())
			{
				objPath.AddEllipse(Geometry.RectangleFromPoint(centre, radius));
				PathGradientBrush brush = new PathGradientBrush(objPath);
				brush.CenterColor = centreColour;
				brush.CenterPoint = centre;
				brush.SurroundColors = new[] { edge };
				return new NetFill(brush);
			}

		}


		public override Stroke CreateStroke(Color col, float width = 1)
		{
			return new NetStroke() { Pen = new Pen(col, width) };
		}

		/// <summary>Version (.net only) which can use any brush.  Not available in other outputs</summary>
		public Stroke CreateStroke(Brush br, float width = 1)
		{
			return new NetStroke() { Pen = new Pen(br, width) };
		}

		private static NetFill White = new NetFill(Brushes.White);
		private static NetFill Black = new NetFill(Brushes.Black);
		private static NetFill Blue = new NetFill(Brushes.Blue);
		public override Fill WhiteBrush => White;
		public override Fill BlackBrush => Black;
		public override Fill BlueBrush => Blue;

		#endregion

		#region Information
		public override float DpiX
		{
			get { return gr.DpiX; }
		}

		public override float DpiY
		{ get { return gr.DpiY; } }

		public Graphics Underlying
		{ get { return gr; } }

		public override GraphicsUnit PageUnit
		{
			get { return gr.PageUnit; }
			set { gr.PageUnit = value; }
		}

		public override InterpolationMode InterpolationMode
		{
			get { return gr.InterpolationMode; }
			set { gr.InterpolationMode = value; }
		}

		public override SmoothingMode SmoothingMode
		{
			get { return gr.SmoothingMode; }
			set { gr.SmoothingMode = value; }
		}
		#endregion

		#region Transformations and clipping
		private Region m_rgnOriginalClip;
		public override void IntersectClip(RectangleF rect)
		{
			if (m_rgnOriginalClip != null)
				throw new InvalidOperationException("Calls to IntersectClip cannot be nested");
			m_rgnOriginalClip = gr.Clip;

			gr.IntersectClip(rect);
		}

		public override void IntersectClip(GraphicsPath path)
		{
			if (m_rgnOriginalClip != null)
				throw new InvalidOperationException("Calls to IntersectClip cannot be nested");
			m_rgnOriginalClip = gr.Clip;
			using (var region = new Region(path))
				gr.IntersectClip(region);
		}

		public override void RestoreClip()
		{
			if (m_rgnOriginalClip == null)
				throw new InvalidOperationException("RestoreClip can only be used after IntersectClip");
			gr.Clip = m_rgnOriginalClip;
			m_rgnOriginalClip = null;
		}

		public override RectangleF ClipBounds
		{ get { return gr.ClipBounds; } }

		private readonly Stack<Matrix> m_stkMatrices = new Stack<Matrix>();
		public override void TranslateTransform(float X, float Y)
		{
			m_stkMatrices.Push(gr.Transform);
			gr.TranslateTransform(X, Y);
		}

		public override void MultiplyTransform(Matrix matrix, MatrixOrder order)
		{
			m_stkMatrices.Push(gr.Transform);
			gr.MultiplyTransform(matrix, order);
		}

		public override void EndTransform()
		{
			gr.Transform = m_stkMatrices.Pop();
		}
		#endregion

		#region Text
		public override SizeF MeasureString(string text, Font font, float width = 100000.0F, StringFormat format = null)
		{
			if (format != null)
			{
				return gr.MeasureString(text, font, (int)width, format);
			}
			return gr.MeasureString(text, font, (int)width);
		}

		/// <summary>Returns the exact rectangle in which the text would be drawn.  Note that it usually will not start at 0, 0 even if drawn top left, the graphics engine usually leaves a pixel or so space</summary>
		public override RectangleF MeasureTextExact(string text, Font font, RectangleF bounds, StringFormat format, int includeCharacters = int.MaxValue)
		{
			return MeasureTextExact(gr, text, font, bounds, format, includeCharacters);
		}

		public override void DrawString(string text, Font font, Fill fill, PointF start, StringFormat format = null)
		{
			gr.DrawString(text, font, ((NetFill)fill).Brush, start, format);
		}

		public override void DrawString(string text, Font font, Fill fill, RectangleF layoutRect, StringFormat format)
		{
			gr.DrawString(text, font, ((NetFill)fill).Brush, layoutRect, format);
		}

		/// <summary>Returns the exact rectangle in which the text would be drawn.  Note that it usually will not start at 0, 0 even if drawn top left, the graphics engine usually leaves a pixel or so space</summary>
		public static RectangleF MeasureTextExact(Graphics gr, string text, Font font, RectangleF rct, StringFormat format, int includeCharacters = int.MaxValue)
		{
			// There is a problem with MeasureCharacterRanges when the font is very small, which is often the case as the font size has been reduced so that we can work in virtual millimetres
			// This also only seems correct if the graphics had a ScaleTransform applied, but it needs to be the right amount.  The ScaleTransform alone is not enough to give the right answer with small font sizes
			// [Later] not sure that this is true.  The problem may just have been that the TextRenderingHint was set differently.  It is certainly the case that this affects the measurement,
			// as does the scale factor (although the measurement is not scaled, the scale factor does appear to be taken into account to determine exactly how characters are placed pixel wise)
			Debug.Assert(gr.TextRenderingHint == System.Drawing.Text.TextRenderingHint.AntiAlias);
			if (format == null) 
				format = new StringFormat();
			if (text.Length == 0)
				return new RectangleF(0, 0, 0, 0); // otherwise it crashes below
			format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, Math.Min(text.Length, includeCharacters)) });
			using (Region region = gr.MeasureCharacterRanges(text, font, rct, format)[0])
			{
				return region.GetBounds(gr);
			}

			//End If
		}

		#endregion

		#region Independent object for measuring text
		private static readonly Bitmap m_bmp;
		public static readonly NetCanvas MeasurementInstance; // used by the internal MeasureText, but not returned by the CreateGraphics (in case the caller manipulates it)

		static NetCanvas()
		{
			// must be after SystemDPI set (currently in frmMenu.Load)
			Debug.Assert(GUIUtilities.SystemDPI > 0);
			m_bmp = new Bitmap(1, 1); //, Imaging.PixelFormat.Format32bppArgb)
			m_bmp.SetResolution(GUIUtilities.SystemDPI, GUIUtilities.SystemDPI);
			MeasurementInstance = new NetCanvas(CreateGraphics(true));
		}

		public static Graphics CreateGraphics(bool scale = true)
		{
			Graphics gr = Graphics.FromImage(m_bmp);
			// See MeasureTextExact; which comes out rather inexact without this
			// In fact it may be necessary to generate this with the exact scaling used on the screen currently
			if (scale)
				gr.ScaleTransform(Geometry.MILLIMETRE * GUIUtilities.SystemDPI / Geometry.INCH, Geometry.MILLIMETRE * GUIUtilities.SystemDPI / Geometry.INCH); // so working in sort of mm
			gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			gr.SmoothingMode = SmoothingMode.AntiAlias;
			return gr;
		}

		#endregion

		#region Shapes
		public override void Path(GraphicsPath path, Stroke stroke = null, Fill fill = null)
		{
			if (fill != null) 
				gr.FillPath(((NetFill) fill).Brush, path);
			if (stroke != null) 
				gr.DrawPath(((NetStroke) stroke).Pen, path);
		}

		public override void Polygon(PointF[] points, Stroke stroke, Fill fill)
		{
			if (fill != null) 
				gr.FillPolygon(((NetFill) fill).Brush, points);
			if (stroke != null) 
				gr.DrawPolygon(((NetStroke) stroke).Pen, points);
		}

		public override void Rectangle(RectangleF rct, Stroke stroke = null, Fill fill = null)
		{
			if (fill != null) 
				gr.FillRectangle(((NetFill) fill).Brush, rct);
			if (stroke != null)
			{
				var pn = ((NetStroke)stroke).Pen;
				// And on the Mac, Graphics.DrawRectangle(pen, single, single, single, single) rounds all the numbers to integers which is a problem when working in mm. Oops
				gr.DrawRectangle(pn, rct.X, rct.Y, rct.Width, rct.Height);
			}
		}

		public override void Ellipse(RectangleF rct, Stroke stroke = null, Fill fill = null)
		{
			if (fill != null)
				gr.FillEllipse(((NetFill)fill).Brush, rct);
			if (stroke != null)
			{
				var pn = ((NetStroke)stroke).Pen;
				gr.DrawEllipse(pn, rct.X, rct.Y, rct.Width, rct.Height);
			}
		}

		public override void DrawLine(float X1, float Y1, float X2, float Y2, Stroke stroke)
		{
			Debug.Assert(stroke != null);
			var pn = ((NetStroke)stroke).Pen;
			gr.DrawLine(pn, (int)X1, (int)Y1, (int)X2, (int)Y2);
		}

		public override void DrawLine(PointF ptA, PointF ptB, Stroke stroke)
		{
			Debug.Assert(stroke != null);
			var pn = ((NetStroke)stroke).Pen;
			gr.DrawLine(pn, ptA, ptB);
		}

		public override void DrawLines(PointF[] points, Stroke stroke)
		{
			Debug.Assert(stroke != null);
			var pn = ((NetStroke)stroke).Pen;
			gr.DrawLines(pn, points);
		}

		public override void DrawBezier(PointF pt0, PointF pt1, PointF pt2, PointF pt3, Stroke stroke)
		{
			Debug.Assert(stroke != null);
			var pn = ((NetStroke)stroke).Pen;
			gr.DrawBezier(pn, pt0, pt1, pt2, pt3);
		}

		public override void DrawArc(RectangleF bounds, float startAngle, float endAngle, Stroke stroke = null)
		{
			Debug.Assert(stroke != null);
			var pn = ((NetStroke)stroke).Pen;
			gr.DrawArc(pn, bounds, startAngle, endAngle);
		}

		public override void FillPie(RectangleF circleRect, float startAngle, float endAngle, Fill fill = null)
		{
			Debug.Assert(fill != null);
			gr.FillPie(((NetFill)fill).Brush, circleRect, startAngle, endAngle);
		}
		#endregion

		#region Other
		public override void DrawImage(MemoryImage image, RectangleF destination, ImageAttributes attributes = null)
		{
			image.Draw(gr, destination, attributes);
		}
		public override void DrawImage(string resourceName, int preferredSize, RectangleF destination, ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			if (image == null)
			{
				image = GUIUtilities.VariableSizeImage("GreyQuestion", "", preferredSize);
				Globals.Root.Log.WriteLine("Cannot find resource image: " + resourceName);
			}
			gr.DrawImage(image, destination, attributes);
		}

		public override void DrawImage(MemoryImage image, RectangleF destination, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			image.Draw(gr, destination, sourceRect, attributes);
		}
		public override void DrawImage(string resourceName, int preferredSize, RectangleF destination, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			if (image == null)
			{
				image = GUIUtilities.VariableSizeImage("GreyQuestion", "", preferredSize);
				Globals.Root.Log.WriteLine("Cannot find resource image: " + resourceName);
			}
			gr.DrawImage(image, destination, sourceRect, attributes);
		}

		public override void DrawImage(MemoryImage image, PointF[] destinationPoints, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			image.Draw(gr, destinationPoints, sourceRect, attributes);
		}
		public override void DrawImage(string resourceName, int preferredSize, PointF[] destinationPoints, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			Image image = GUIUtilities.VariableSizeImage(resourceName, "", preferredSize);
			if (image == null)
			{
				image = GUIUtilities.VariableSizeImage("GreyQuestion", "", preferredSize);
				Globals.Root.Log.WriteLine("Cannot find resource image: " + resourceName);
			}
			gr.DrawImage(image, destinationPoints, sourceRect, GraphicsUnit.Pixel, attributes);
		}

		#endregion

	}
}
