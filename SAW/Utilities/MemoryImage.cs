using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Svg;

namespace SAW
{
	/// <summary>The actual data for an image, buffered in memory.  No Datum or sharing logic (see SharedImage).
	/// Also supports SVG images now - so base image may not be a .net image at all.</summary>
	public class MemoryImage
	{
		// in several places we want to persist a bitmap into a stream - but intrinsically .net only supports doing this if the entire stream is the bitmap
		// it cannot load several bitmaps as part of a data file
		// the only solution is to load the bitmap data into a memory buffer and generate a stream from that
		// this object does not derive from Datum, and has no CRC.  The functionality for sharing between several other objects is done in a separate class above
		// these are kept separate, because sometimes we want to use a MemoryImage without it being a proper data object, just a thumbnail stored by something else
		// this can support WMF files, because we don't usually attempt to save the image itself into the stream; the original data buffer is always kept
		// This is not IDisposable because usually be shared and garbage collection is needed to tidy up.  ForceDispose can be used to remove one of these if that is known to be safe

		/// <summary>the image file stored in a memory buffer.  This is always defined</summary>
		public byte[] Buffer;
		private Image m_Image; // the image and stream are created as needed
		private MemoryStream m_Stream;
		private SvgDocument m_SVG;
		/// <summary>true if initialised from Null.  GetImage returns a missing image</summary>
		public readonly bool IsNull;
		/// <summary>True if the image is actually SVG.  When written to file an initial byte is 1 for SVG, 0 otherwise (to allow for future options)</summary>
		public readonly bool IsSVG;

		/// <summary>Set by GetImage.  Allows query of size later if image is unloaded</summary>
		private Size m_sz;

		/// <summary>Default size used if we need to convert SVG/WMF to a bitmap</summary>
		internal static readonly Size DEFAULTBITMAPSIZE = new Size(64, 64);

		#region Normal constructors
		public MemoryImage(DataReader reader)
		{
			int length = reader.ReadInt32();
			if (length == 0)
			{
				Buffer = null;
				IsNull = true;
			}
			else
			{
				if (reader.Version >= 125)
					IsSVG = reader.ReadByte() == 1; // not a bool to allow for other options in future
				Buffer = new byte[length];
				reader.Read(Buffer, 0, length);
			}
		}

		public MemoryImage(string file)
		{
			Buffer = File.ReadAllBytes(file);
			IsSVG = (string.Compare(Path.GetExtension(file).ToLowerInvariant(), ".svg", true) == 0);
		}

		/// <summary>creates from an existing image</summary>
		public MemoryImage(Bitmap bitmap)
		{
			// 
			IsSVG = false;
			m_Image = bitmap;
			if (bitmap == null)
				IsNull = true;
			else
			{
				m_Stream = new MemoryStream(5000);
				m_Image.Save(m_Stream, ImageFormat.Png);
				Buffer = m_Stream.GetBuffer();
			}
		}

		/// <summary>Reads the entire stream into the buffer.</summary>
		public MemoryImage(Stream stream, bool isSVG)
		{
			IsSVG = isSVG;
			int length = (int)stream.Length;
			if (length == 0) // image was missing when saved
			{
				Buffer = null;
				IsNull = true;
			}
			else
			{
				stream.Seek(0, SeekOrigin.Begin);
				Buffer = new byte[length - 1 + 1];
				stream.Read(Buffer, 0, length);
				// we don't keep the original stream, just the buffer we have filled from it
			}
		}

		public MemoryImage(Stream stream, bool isSVG, int start, int length)
		{
			IsSVG = isSVG;
			stream.Seek(start, SeekOrigin.Begin);
			Buffer = new byte[length - 1 + 1];
			stream.Read(Buffer, 0, length);
			// we don't store the original stream, just the buffer we have filled from it
		}

		public void Save(DataWriter writer)
		{
			if (IsNull)
				writer.Write(0);
			else
			{
				if (Buffer == null)
					throw new ObjectDisposedException("MemoryImage");
				writer.Write(Buffer.Length);
				if (writer.Version >= 125)
					writer.WriteByte(IsSVG ? 1 : 0);
				writer.Write(Buffer);
			}
		}

		///// <summary>Uses  these bytes as the buffer, representing an image file in memory.  Note this uses the parameter array and does not copy it</summary>
		//public MemoryImage(byte[] objBuffer)
		//{
		//	Buffer = objBuffer;
		//}

		#endregion

		#region Image data handling
		/// <summary>Returns a version of the image as a .net Image - rendering SVG into a bitmap if necessary.  Only use where a net image is required</summary>
		/// <param name="requireBitmap">If true then a vector Image (WMF) will not be returned, instead a Bitmap will be generated - but a new one is generated for each call.  See also ChangeToBitmap</param>
		public Image GetNetImage(bool requireBitmap = false)
		{
			if (IsSVG)
				return GenerateBitmap(DEFAULTBITMAPSIZE);
			if (m_Image == null)
			{
				if (IsNull)
					return Resources.AM.Missing_128;
				if (Buffer == null)
					throw new ObjectDisposedException("MemoryImage");
				m_Stream = new MemoryStream(Buffer);
				m_Image = Image.FromStream(m_Stream);
				m_sz = m_Image.Size;
			}
			if (requireBitmap && !(m_Image is Bitmap))
				return GenerateBitmap(DEFAULTBITMAPSIZE);
			return m_Image;
		}

		/// <summary>Returns the size, without loading the image if it was previously loaded</summary>
		public Size Size
		{
			get
			{
				if (!m_sz.IsEmpty)
					return m_sz;
				if (IsSVG)
				{
					EnsureSVG();
					return new Size((int)m_SVG.ViewBox.Width, (int)m_SVG.ViewBox.Height);
				}
				return GetNetImage().Size;
			}
		}

		/// <summary>If this is a metafile or SVG, it is changed (permanently) to a bitmap.  Previous references to the image may become invalid.  Does nothing if already a bitmap</summary>
		public void ChangeToBitmap(Size sz)
		{
			if (!IsSVG && GetNetImage() is Bitmap)
				return;// already is

			Bitmap bmp = GenerateBitmap(sz);
			m_Stream = new MemoryStream();
			bmp.Save(m_Stream, ImageFormat.Png);
			m_Image = bmp;
			Debug.WriteLine("MemoryImage converted to bitmap");
		}

		/// <summary>Creates a bitmap from the data (if it was WMF or SVG)</summary>
		private Bitmap GenerateBitmap(Size sz)
		{
			Debug.Assert(IsSVG || !(GetNetImage() is Bitmap)); // if it already is the code below will work, but there's something wrong (and it's inefficient!)
			if (IsSVG)
			{
				GUIUtilities.CalcDestSize(Size, ref sz);
				// we don't rely on Draw maintaining aspect:ratio, since we can only fix one coord.  if we fixed width and the image happened to be very tall it would result in a huge image
				var x = m_SVG.Draw(sz.Width, sz.Height);
#if DEBUG
				x.Save(@"d:\temp\svg draw.png");
#endif
				return x;
			}
			Bitmap bmp = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb);
			using (Graphics gr = Graphics.FromImage(bmp))
			{
				gr.Clear(Color.Transparent);
				Rectangle rect = new Rectangle(Point.Empty, sz);
				GUIUtilities.CalcDestRect(sz, ref rect);
				gr.DrawImage(m_Image, rect);
			}
			return bmp;
		}

		/// <summary>Returns true if the image (potentially) has transparent parts</summary>
		public bool CanHaveAlpha
		{
			get
			{
				if (IsSVG)
					return true;
				switch ((GetNetImage() as Bitmap)?.PixelFormat ?? PixelFormat.Format32bppArgb) // if not bitmap will return true
				{
					case PixelFormat.Format24bppRgb:
					case PixelFormat.Format16bppRgb555:
					case PixelFormat.Format16bppRgb565:
						return false;
					default:
						return true;
				}
			}
		}

		/// <summary>Ensures m_SVG is defined - IF this is SVG.  Cannot be called otherwise</summary>
		private void EnsureSVG()
		{
			Debug.Assert(IsSVG);
			if (m_SVG != null)
				return;
			m_Stream = new MemoryStream(Buffer);
			m_SVG = SvgDocument.Open<SvgDocument>(m_Stream);
		}

		#endregion

		#region Rendering (into .net graphics - used by the NetCanvas;  most rendering should be through the canvas objects, not this)

		public void Draw(Graphics gr, RectangleF rect, ImageAttributes attributes = null)
		{
			if (IsSVG)
			{
				EnsureSVG();
				Draw(gr, rect, new RectangleF(m_SVG.ViewBox.MinX, m_SVG.ViewBox.MinY, m_SVG.ViewBox.Width, m_SVG.ViewBox.Height), attributes);
				// using this seemed problematic (corrupting gr state and/or not inheriting it properly into the SVG renderer.  And it turns out to be unnecessary;  the used option is just easier)
				//using (ISvgRenderer render = SvgRenderer.FromGraphics(gr))
				//{
				//render.Transform = gr.Transform;
				//render.ScaleTransform(rect.Width / m_SVG.Width, rect.Height / m_SVG.Height);
				//render.TranslateTransform(rect.Left, rect.Top);
				//m_SVG.Draw(render);
				//}
				// and then using this to get the size seemed to fail for some;  so just uses the full version below, which works
				// m_SVG.Draw(gr, new SizeF(rect.Width, rect.Height));
			}
			else
				gr.DrawImage(GetNetImage(), rect, attributes);
		}

		public void Draw(Graphics gr, RectangleF rect, RectangleF source, ImageAttributes attributes = null)
		{
			if (IsSVG)
			{
				EnsureSVG();
				var state = gr.Save();
				gr.TranslateTransform(rect.Left, rect.Top, MatrixOrder.Prepend);
				RectangleF viewbox = m_SVG.ViewBox;
				// we must prevent SVG applying any scaling as it's in the wrong sequence, and must be BEFORE the source adjustments
				// therefore we perform the output scale now: (and below m_SVG.Draw uses the viewbox size, so will apply a (1,1) scaling)
				gr.ScaleTransform(rect.Width / viewbox.Width, rect.Height / viewbox.Height);
				gr.ScaleTransform(viewbox.Width / source.Width, viewbox.Height / source.Height);
				gr.TranslateTransform(-(source.X - viewbox.X), -(source.Y - viewbox.Y));
				m_SVG.Draw(gr, new SizeF(viewbox.Width, viewbox.Height));
				gr.Restore(state);
				//gr.DrawRectangle(Pens.Salmon, rect.X, rect.Y, rect.Width, rect.Height);
			}
			else
				gr.DrawImage(GetNetImage(), rect, source, attributes);
		}

		public void Draw(Graphics gr, PointF[] destinationPoints, RectangleF source, ImageAttributes attributes = null)
		{
			if (IsSVG)
			{
				/* for a matrix (a,b,c,d,e,f) the 3 points render to:
				 pt0 = (0,0,1) -> (e,f)
				 pt1 = (w,0,1) -> (aw+e, bw+f)
				 pt2 = (0,h,1) -> (ch+e, dh+f)
				 Therefore we can deduce the matrix for rendering this image - to be prepended on any world matrix
				 BUT even better, Matrix has a constructor for it :-)
				 */
				EnsureSVG();
				gr.ResetClip();
				var state = gr.Save();
				var pt0 = destinationPoints[0];
				Matrix m = new Matrix(source, destinationPoints);
				gr.MultiplyTransform(m, MatrixOrder.Prepend);
				m_SVG.Draw(gr);
				gr.Restore(state);
				//gr.DrawLines(Pens.Blue, new[] { destinationPoints[0], destinationPoints[1], new PointF(destinationPoints[1].X, destinationPoints[2].Y), destinationPoints[2] });
			}
			else
				gr.DrawImage(GetNetImage(), destinationPoints, source, GraphicsUnit.Pixel, attributes);
		}

		#endregion

		/// <summary>once the image is created it is not disposed at the moment.  If it is being drawn on-screen it will be needed repeatedly</summary>
		private MemoryImage(byte[] buffer, bool isSVG)
		{
			// Only used for the Clone method
			Buffer = buffer;
			IsNull = (buffer == null);
			IsSVG = isSVG;
		}

		public MemoryImage Clone()
		{
			return new MemoryImage((byte[])Buffer?.Clone(), IsSVG);
		}

		public int CalcCRC()
		{
			if (Buffer == null)
				return 0;
			return CRCCalc.Calc(Buffer);
		}

		/// <summary>Usually images are just kept in memory, but for large ones they can be released.  This also disposes the last Image object returned by GetImage</summary>
		/// <remarks>Only used for page backgrounds - they were causing trouble if very large ones all kept in memory</remarks>
		public void Release()
		{
			m_Image?.Dispose();
			m_Image = null;
		}

	}
}