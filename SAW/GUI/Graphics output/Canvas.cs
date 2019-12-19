using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SAW
{
	public abstract class Canvas : IDisposable
	{

		// I decided to do the graphics through an indirect layer.  On some experimentation, the speed difference appears to be negligible
		// when drawing lines or filling rectangles.  Perhaps 1% if each pen is used for 10 rectangles.  1.5% if used for one rectangle
		// This allows both some more complex, e.g. compound pens, and the possibility of writing to PDF

		#region Resource creation
		public abstract Stroke CreateStroke(Color col, float width = 1);
		public abstract Fill CreateFill(Color colour);
		public abstract Fill CreateLinearGradientFill(PointF A, PointF B, Color colA, Color colB);
		/// <summary>Scale parameter determines how large the image is drawn</summary>
		public abstract Fill CreateTextureBrush(SharedImage image, float scale);
		public abstract Fill CreateHatchBrush(Color foreground, Color background, Shape.FillStyleC.Patterns pattern);
		public abstract Fill CreateRadialGradientBrush(PointF centre, float radius, Color centreColour, Color edge, RectangleF bounds);

		public abstract Fill WhiteBrush { get; }
		public abstract Fill BlackBrush { get; }
		/// <summary>Used for some text</summary>
		public abstract Fill BlueBrush { get; }
		#endregion

		#region Information
		public abstract float DpiX { get; }
		public abstract float DpiY { get; }
		public virtual SmoothingMode SmoothingMode { get; set; }
		public virtual InterpolationMode InterpolationMode { get; set; }
		public virtual GraphicsUnit PageUnit { get; set; }
		#endregion

		#region Transformations and clipping
		/// <summary>Must be matched with a call to EndTransform (SVG puts all the transformed elements into a group)
		/// Transforms, clips and grouping must nest correctly - no overlapping allowed</summary>
		public abstract void MultiplyTransform(Matrix matrix, MatrixOrder order);
		/// <summary>Must be matched with a call to EndTransform (SVG puts all the transformed elements into a group)
		/// Transforms, clips and grouping must nest correctly - no overlapping allowed</summary>
		public abstract void TranslateTransform(float X, float Y);
		public abstract void EndTransform();

		/// <summary>Is permitted to return an area which is too large: only to be used for improving performance</summary>
		public abstract RectangleF ClipBounds { get; }
		/// <summary>RestoreClip must be called after calling this, to restore the original region. Transforms, clips and grouping must nest correctly - no overlapping allowed</summary>
		public abstract void IntersectClip(RectangleF rect);
		/// <summary>Called after IntersectClip to restore the original area</summary>
		public abstract void RestoreClip();

		/// <summary>Like IntersectClip(rect) but clips to any path.  Not supported by PDF.  RestoreClip must still be called.
		/// Further clipping inside this complex path is not supported (and will crash in SVG)</summary>
		public abstract void IntersectClip(GraphicsPath path);

		/// <summary>Groups all output until EndGroup; ignored by most output types.  Transforms, clips and grouping must nest correctly - no overlapping allowed</summary>
		public virtual void StartGroup()
		{
		}
		public virtual void EndGroup()
		{
		}
		#endregion

		#region Text
		public abstract SizeF MeasureString(string text, Font font, float width = 100000, StringFormat format = null);
		public abstract RectangleF MeasureTextExact(string text, Font font, RectangleF bounds, StringFormat format, int includeCharacters = int.MaxValue);

		public abstract void DrawString(string text, Font font, Fill fill, RectangleF layoutRect, StringFormat format);
		public abstract void DrawString(string text, Font font, Fill fill, PointF start, StringFormat format = null);

		#endregion

		#region Shapes
		public abstract void Path(GraphicsPath path, Stroke stroke = null, Fill fill = null);
		public abstract void Rectangle(RectangleF rct, Stroke stroke = null, Fill fill = null);
		public void Rectangle(float X1, float Y1, float X2, float Y2, Stroke stroke = null, Fill fill = null)
		{
			Rectangle(new RectangleF(X1, Y1, X2, Y2), stroke, fill);
		}
		public abstract void Ellipse(RectangleF rct, Stroke stroke = null, Fill fill = null);
		public abstract void Polygon(PointF[] points, Stroke stroke, Fill fill);
		public abstract void DrawLine(PointF ptA, PointF ptB, Stroke stroke);
		public abstract void DrawLine(float X1, float Y1, float X2, float Y2, Stroke stroke);
		public abstract void DrawBezier(PointF pt0, PointF pt1, PointF pt2, PointF pt3, Stroke stroke);
		public abstract void DrawLines(PointF[] points, Stroke stroke);

		public abstract void DrawArc(RectangleF bounds, float startAngle, float endAngle, Stroke stroke = null);
		public abstract void FillPie(RectangleF circleRect, float startAngle, float endAngle, Fill fill = null);
		#endregion

		#region Other
		// The image can be specified in 3 ways.  cannot just use .net images because PDF/SVG can't really use these.  We need access to the underlying data
		// likewise there are 3 different ways of specifying the coordinates
		public abstract void DrawImage(MemoryImage image, RectangleF destination, ImageAttributes attributes = null);
		public abstract void DrawImage(string resourceName, int preferredSize, RectangleF destination, ImageAttributes attributes = null);
		public void DrawImage(SharedImage image, int preferredSize, RectangleF destination, ImageAttributes attributes = null)
		{
			if (!image.IsLightweight)
				DrawImage(image.MemoryImage, destination, attributes);
			else
				DrawImage(image.ResourceName, preferredSize, destination, attributes);
		}

		public abstract void DrawImage(MemoryImage image, RectangleF destination, RectangleF sourceRect, ImageAttributes attributes = null);
		public abstract void DrawImage(string resourceName, int preferredSize, RectangleF destination, RectangleF sourceRect, ImageAttributes attributes = null);
		public void DrawImage(SharedImage image, int preferredSize, RectangleF destination, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			if (!image.IsLightweight)
				DrawImage(image.MemoryImage, destination, sourceRect, attributes);
			else
				DrawImage(image.ResourceName, preferredSize, destination, sourceRect, attributes);
		}

		public abstract void DrawImage(MemoryImage image, PointF[] destinationPoints, RectangleF sourceRect, ImageAttributes attributes = null);
		public abstract void DrawImage(string resourceName, int preferredSize, PointF[] destinationPoints, RectangleF sourceRect, ImageAttributes attributes = null);
		public void DrawImage(SharedImage image, int preferredSize, PointF[] destinationPoints, RectangleF sourceRect, ImageAttributes attributes = null)
		{
			if (!image.IsLightweight)
				DrawImage(image.MemoryImage, destinationPoints, sourceRect, attributes);
			else
				DrawImage(image.ResourceName, preferredSize, destinationPoints, sourceRect, attributes);
		}
		#endregion

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

	}

	#region Abstract Stroke and Fill classes
	public abstract class Stroke : IDisposable
	{

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		public abstract DashStyle DashStyle { get; set; }
		public abstract float[] DashPattern { get; set; }
		public abstract LineJoin LineJoin { get; set; }
		public abstract Color Color { get; }
		public abstract LineCap EndCap { get; set; }
		public abstract float MiterLimit { get; set; }

	}

	public abstract class Fill : IDisposable
	{

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>Returns true if this is not solid (most brushes other than colour assume they are not transparent)</summary>
		public abstract bool IsTransparent { get; }

	}
	#endregion
}
