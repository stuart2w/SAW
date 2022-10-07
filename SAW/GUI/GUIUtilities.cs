using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Supported by any custom controls which can be triggered using the space bar (bit like Button.PerformClick) </summary>
	internal interface IInvokeable
	{
		void PerformClick();
	}

	internal static class GUIUtilities
	{
		public static ResourceManager RM = new ResourceManager("SAW.AM", Assembly.GetExecutingAssembly());
		public static ImageAttributes DrawImage40Percent = new ImageAttributes(); // can be used to draw 40% alpha bitmap
		public static ImageAttributes DrawImageDisableNearGrey = new ImageAttributes(); // can be used to draw images grey-ish
		public static ImageAttributes DrawImageGreenTint = new ImageAttributes(); // used by palette headers to show focus
		public static ImageAttributes DrawImageNormal = new ImageAttributes();
		public static StringFormat StringFormatTopCentre;
		public static StringFormat StringFormatTopRight;
		public static StringFormat StringFormatTopLeft;
		public static StringFormat StringFormatCentreCentre;
		public static StringFormat StringFormatCentreLeft;
		public static StringFormat StringFormatCentreLeftNoWrap;
		public static StringFormat StringFormatCentreRight;

		/// <summary>used by new text to measure line break; single line, word break, no trailing spaces; no hot keys</summary>
		public static StringFormat StringFormatMeasureLine;

		/// <summary>used by new text to find characters; single line, char breaking, includes trailing spaces; no hot keys</summary>
		public static StringFormat StringFormatFindCharacter;

		/// <summary>Used to render single line of text which has already been formatted.  NoClip due to fractional errors (which otherwise lose the last char)
		/// Centred as that reduces effect of measurement differences in SVG/PDF</summary>
		public static StringFormat StringFormatRender;

		internal static Font ShortcutFont; // all buttons share the font for the shortcut
		internal static Font ShortcutFontLarge; // ... but want a larger version as well
		internal static Font ShortcutFontVerySmall; // and a few don't fit so need an even smaller one
		internal static StringFormat ShortcutFormat;
		public static Pen FocusPen;
		public static bool MouseSwapped = false; // true if the mouse buttons are reversed.  Checked during frmMain start-up, and not re-checked and thereafter
		public static int SystemDPI; // we assume that the X and Y are the same.  This is set by frmMenu.Load
		public static float SystemDPIRelative; //= SystemDPI / 96 (also set by frmMenu)

		static GUIUtilities()
		{
			// r->r, g->g, b->b, a->a reduced
			DrawImage40Percent.SetColorMatrix(new ColorMatrix
			{
				Matrix00 = 1,
				Matrix11 = 1,
				Matrix22 = 1,
				Matrix33 = 0.4F
			}, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			// also lower alpha
			DrawImageDisableNearGrey.SetColorMatrix(new ColorMatrix
			{
				Matrix00 = 0.3F,
				Matrix01 = 0.2F,
				Matrix02 = 0.2F,
				Matrix10 = 0.2F,
				Matrix11 = 0.3F,
				Matrix12 = 0.2F,
				Matrix20 = 0.2F,
				Matrix21 = 0.2F,
				Matrix22 = 0.3F,
				Matrix40 = 0.3F,
				Matrix41 = 0.3F,
				Matrix42 = 0.3F,
				Matrix33 = 0.6F
			}, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			ColorMatrix tempMatrix = new ColorMatrix
			{
				Matrix00 = 0.9F,
				Matrix01 = 0,
				Matrix02 = 0,
				Matrix10 = 0,
				Matrix11 = 1,
				Matrix12 = 0,
				Matrix20 = 0,
				Matrix21 = 0,
				Matrix22 = 0.9F,
				Matrix40 = 0,
				Matrix41 = 0.05F,
				Matrix42 = 0,
				Matrix33 = 1
			};
			DrawImageGreenTint.SetColorMatrix(tempMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			DrawImageGreenTint.SetColorMatrix(tempMatrix, ColorMatrixFlag.Default, ColorAdjustType.Brush);

			StringFormatTopCentre = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
			StringFormatTopRight = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
			StringFormatTopLeft = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

			StringFormatCentreCentre = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; // NumberGrid requires this to have nothing else set
			StringFormatCentreLeft = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center }; // NumberGrid requires this to have nothing else set
			StringFormatCentreLeftNoWrap = (StringFormat)StringFormatCentreLeft.Clone();
			StringFormatCentreLeftNoWrap.FormatFlags = StringFormatFlags.NoWrap;
			StringFormatCentreLeftNoWrap.Trimming = StringTrimming.EllipsisCharacter;
			StringFormatCentreRight = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center }; // NumberGrid requires this to have nothing else set

			ShortcutFontVerySmall = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Pixel);
			ShortcutFont = new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel);
			ShortcutFontLarge = new Font("Arial", 16, FontStyle.Bold, GraphicsUnit.Pixel);
			ShortcutFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };

			StringFormatMeasureLine = new StringFormat(StringFormatFlags.NoWrap) { HotkeyPrefix = HotkeyPrefix.None, Trimming = StringTrimming.Word };

			StringFormatFindCharacter = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.NoWrap)
			{
				HotkeyPrefix = HotkeyPrefix.None,
				Trimming = StringTrimming.Character
			};
			// note can't set None as this doesn't clip - it just returns the number of chars in the text

			// used to render one line (some probs with mis-measurement of "Dillenia UPC Bold Italic" still)
			StringFormatRender =
				new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox)
				{
					HotkeyPrefix = HotkeyPrefix.None,
					Trimming = StringTrimming.None,
					Alignment = StringAlignment.Near,           // needs to be cos otherwise trailing spaces mess up the start point (they were not included in the measurement?)
					LineAlignment = StringAlignment.Center
				};

			FocusPen = new Pen(Color.Gray) { DashStyle = DashStyle.Dot };

			// also work out total screen bounds, used to check palettes on screen
			foreach (Screen screen in Screen.AllScreens)
			{
				Geometry.Extend(ref g_TotalScreenBounds, screen.Bounds);
			}
		}

		#region Colours, alpha

		/// <summary>Gets ImageAttributes with a colour matrix applying a transparency</summary>
		public static ImageAttributes GetImageAttrForAlpha(int alpha)
		{
			ColorMatrix y = new ColorMatrix();
			y.Matrix00 = 1; // r->r
			y.Matrix11 = 1; // g->g
			y.Matrix22 = 1; // b->b
			y.Matrix33 = alpha / 255f; // a->a
			ImageAttributes attributes = new ImageAttributes();
			attributes.SetColorMatrix(y, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			return attributes;
		}

		/// <summary>Gets ImageAttributes with a colour matrix applying a transparency and making it grey</summary>
		public static ImageAttributes GetImageAttrForGreyAlpha(int alpha)
		{
			ColorMatrix y = new ColorMatrix
			{
				Matrix00 = 0.33F,
				Matrix01 = 0.33F,
				Matrix02 = 0.33F,
				Matrix10 = 0.33F,
				Matrix11 = 0.33F,
				Matrix12 = 0.33F,
				Matrix20 = 0.33F,
				Matrix21 = 0.33F,
				Matrix22 = 0.33F,
				Matrix33 = alpha / 255f
			};
			ImageAttributes attributes = new ImageAttributes();
			attributes.SetColorMatrix(y, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			return attributes;
		}

		/// <summary>Specifies the value is for a matrix in a form which can be written in line</summary>
		/// <remarks>The actual matrix uses an array of arrays, but the {{}, {}, {}} creates a two-dimensional array</remarks>
		public static void SetMatrix(this ColorMatrix matrix, float[,] sng)
		{
			for (int X = 0; X <= 3; X++)
			{
				for (int Y = 0; Y <= 4; Y++)
				{
					matrix[Y, X] = sng[Y, X];
				}
			}
		}

		/// <summary>this function can be used repeatedly to step a colour in stages towards a destination colour (e.g. used on a timer to update a colour progressively)</summary>
		public static Color StepColour(this Color current, Color destination, int step)
		{
			return Color.FromArgb(StepElement(current.A, destination.A, step), StepElement(current.R, destination.R, step), StepElement(current.G, destination.G, step),
				StepElement(current.B, destination.B, step));
		}

		private static int StepElement(int current, int destination, int step)
		{
			switch (Math.Sign(destination - current))
			{
				case 0:
					return current;
				case -1:
					if (current - step <= destination)
						return destination;
					return current - step;
				default:
					if (current + step >= destination)
						return destination;
					return current + step;
			}
		}

		/// <summary>Updates the colour, averaging it with another</summary>
		public static Color MixWith(this Color a, Color b)
		{
			return Color.FromArgb((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2);
		}

		/// <summary>Updates the colour, mixing it with another in a varying percentage.  percentageA is the percentage of the first colour to keep</summary>
		public static Color MixWith(this Color a, Color b, int percentageA)
		{
			int percentageB = 100 - percentageA;
			return Color.FromArgb((a.R * percentageA + b.R * percentageB) / 100, (a.G * percentageA + b.G * percentageB) / 100, (a.B * percentageA + b.B * percentageB) / 100);
		}

		public static Color ColorFromWin32RGB(int RGB)
		{
			return Color.FromArgb(RGB & 0xff, (RGB >> 8) & 0xff, (RGB >> 16) & 0xff);
		}

		public static int ToWin32RGB(this Color colour)
		{
			return colour.R + (colour.G << 8) + (colour.B << 16);
		}

		#endregion

		public static GraphicsPath CreateRoundedRectPath(RectangleF r, float radius)
		{
			// as below; done separately; this version probably more generally applicable
			GraphicsPath path = new GraphicsPath();
			float d = radius * 2;
			path.AddLine(r.Left + d, r.Top, r.Right - d, r.Top);
			path.AddArc(RectangleF.FromLTRB(r.Right - d, r.Top, r.Right, r.Top + d), -90, 90);
			path.AddLine(r.Right, r.Top + d, r.Right, r.Bottom - d);
			path.AddArc(RectangleF.FromLTRB(r.Right - d, r.Bottom - d, r.Right, r.Bottom), 0, 90);
			path.AddLine(r.Right - d, r.Bottom, r.Left + d, r.Bottom);
			path.AddArc(RectangleF.FromLTRB(r.Left, r.Bottom - d, r.Left + d, r.Bottom), 90, 90);
			path.AddLine(r.Left, r.Bottom - d, r.Left, r.Top + d);
			path.AddArc(RectangleF.FromLTRB(r.Left, r.Top, r.Left + d, r.Top + d), 180, 90);
			path.CloseFigure();
			return path;
		}

		public static GraphicsPath CreateRoundedRectPath(int X, int Y, int size, int radius, int inset = 0)
		{
			// Rounded parts are always created square at the moment
			// can create somewhat within the specified square if inset is defined
			GraphicsPath path = new GraphicsPath();
			radius = Math.Min(radius, size / 2); // could be 0
			inset = Math.Min(inset, size / 2);
			int i = inset + radius;
			path.AddLine(X + i, Y + inset, X + size - i, Y + inset);
			if (radius > 0)
				path.AddArc(X + size - i - radius - 1, Y + inset, radius * 2, radius * 2, 270, 90);

			path.AddLine(X + size - inset, Y + i, X + size - inset, Y + size - i);
			if (radius > 0)
				path.AddArc(X + size - i - radius - 1, Y + size - i - radius - 1, radius * 2, radius * 2, 0, 90);

			path.AddLine(X + size - i, Y + size - inset, X + i, Y + size - inset);
			if (radius > 0)
				path.AddArc(X + inset, Y + size - i - radius - 1, radius * 2, radius * 2, 90, 90);

			path.AddLine(X + inset, Y + size - i, X + inset, Y + i);
			if (radius > 0)
				path.AddArc(X + inset, Y + inset, radius * 2, radius * 2, 180, 90);

			return path;
		}

		#region Finding image resources

		/// <summary>the name is in two parts around any size postfix; 32 pixels never has a postfix</summary>
		/// <param name="firstPart">Compulsory part.  Can be complete name of image, in which case size is on the end.  Should not have with _</param>
		/// <param name="secondPart">Optional part to appear after the size (or immediately afterPart if size is 32).  Should usually start with _</param>
		/// <param name="requestedSize"></param>
		/// <returns>Image of different size, or smallest image larger than that, or any image failing that</returns>
		/// <remarks>The size postfix is added between the parts (one of them can be "" if desired)
		/// (it is done this way because I can only get Illustrator to save with the size in the middle, and the art board name on the end)</remarks>
		public static Image VariableSizeImage(string firstPart, string secondPart, int requestedSize)
		{
			Debug.Assert(!firstPart.EndsWith("_"));
			int attempt = requestedSize;
			while (true)
			{
				Image image = (Image)RM.GetObject(firstPart + ShapeImageSizePostfix(attempt) + secondPart);
				if (image != null)
					return image;
				if (attempt <= 23)
					attempt = 24;
				else if (attempt <= 31)
					attempt = 32;
				else if (attempt <= 47)
					attempt = 48;
				else if (attempt <= 63)
					attempt = 64;
				else if (attempt <= 127)
					attempt = 128;
				else
					break;
			}

			// now try shrinking
			attempt = requestedSize;
			while (true)
			{
				// this one changes size first as requested size was tried above
				if (attempt > 128)
					attempt = 128;
				else if (attempt > 64)
					attempt = 64;
				else if (attempt > 48)
					attempt = 48;
				else if (attempt > 32)
					attempt = 32;
				else if (attempt > 24)
					attempt = 24;
				else
					return null;
				Image image = (Image)RM.GetObject(firstPart + ShapeImageSizePostfix(attempt) + secondPart);
				if (image != null)
					return image;
			}
		}

		public static string VariableSizeImageResourceID(string firstPart, string secondPart, int requestedSize)
		{
			// As above, but returns the resource ID not the actual image
			// Is less efficient to implement the above function by calling this one, as this creates the image and then discards it
			int attempt = requestedSize;
			while (true)
			{
				string imageName = firstPart + ShapeImageSizePostfix(attempt) + secondPart;
				Image image = (Image)RM.GetObject(imageName);
				if (image != null)
					return imageName;
				if (attempt <= 23)
					attempt = 24;
				else if (attempt <= 31)
					attempt = 32;
				else if (attempt <= 47)
					attempt = 48;
				else if (attempt <= 63)
					attempt = 64;
				else if (attempt <= 127)
					attempt = 128;
				else if (attempt == 128)
					break;
			}
			// now try shrinking
			attempt = requestedSize;
			while (true)
			{
				// this one changes size first as requested size was tried above
				if (attempt > 128)
					attempt = 128;
				else if (attempt > 64)
					attempt = 64;
				else if (attempt > 48)
					attempt = 48;
				else if (attempt > 32)
					attempt = 32;
				else if (attempt > 24)
					attempt = 24;
				else
					return "";
				string imageName = firstPart + ShapeImageSizePostfix(attempt) + secondPart;
				Image image = (Image)RM.GetObject(imageName);
				if (image != null)
					return imageName;
			}
		}

		public static Image VariableSizeImageRoundButton(string firstPart, string secondPart, int size)
		{
			// exactly as above, but the round buttons use different sizes
			// ReSharper disable RedundantAssignment
			for (int index = 0; index <= 5; index++) // the maximum number of attempts we can have in order to try every possible size
			{
				// ReSharper restore RedundantAssignment
				// if the requested size fails we try larger sizes first
				string name = firstPart + "_" + size + secondPart; //ShapeImageSizePostfix(size)
				Image image = (Image)RM.GetObject(name);
				if (image != null)
					return image;
				if (size <= 31)
					size = 32;
				else if (size <= 37)
					size = 38;
				else if (size <= 47)
					size = 48;
				else if (size <= 79)
					size = 80;
				else if (size == 256)
					size = 32;
				else
					size = 256;
			}
			return null;
		}

		public static Image ShapeImage(Shape.Shapes shape, int size)
		{
			return VariableSizeImage("TOOLIMG", "_" + shape.ToString().ToUpper(), size);
		}

		public static string ShapeImageResourceID(Shape.Shapes shape, int size)
		{
			// As above, but returns the resource ID Is available
			return VariableSizeImageResourceID("TOOLIMG", "_" + shape.ToString().ToUpper(), size);
		}

		public static string ShapeImageSizePostfix(int size)
		{
			// the images are/will be stored and 32, 48 and 64 pixels
			// 32 has no postfix
			if (size == 32)
				return "";
			return "_" + size;
		}

		#endregion

		#region Measuring - CalcDestRect

		public static void CalcDestRect(int imageWidth, int imageHeight, ref Rectangle dest, float distortLimit = 1)
		{
			// calculates where to draw an image within a destination rectangle - shrinking the image as needed
			float ratioImage = (float)imageWidth / imageHeight;
			float ratioDest = (float)dest.Width / dest.Height;
			Debug.Assert(distortLimit > 0.999, "distortLimit should be >=1");
			if (dest.Width > imageWidth && dest.Height > imageHeight)
			{
				// special case - image smaller than target - draw it centred, don't expand it
				dest.Inflate(-(dest.Width - imageWidth) / 2, -(dest.Height - imageHeight) / 2);
			}
			else
			{
				int oldDimension;
				if (ratioDest / ratioImage > distortLimit)
				{
					// dest too wide
					oldDimension = dest.Width;
					dest.Width = (int)(dest.Height * ratioImage);
					dest.X += (oldDimension - dest.Width) / 2;
				}
				if (ratioImage / ratioDest > distortLimit)
				{
					// dest too high
					oldDimension = dest.Height;
					dest.Height = (int)(dest.Width / ratioImage);
					dest.Y += (oldDimension - dest.Height) / 2;
				}
			}
		}

		public static void CalcDestRect(Size imageSize, ref Rectangle dest, float distortLimit = 1)
		{
			CalcDestRect(imageSize.Width, imageSize.Height, ref dest, distortLimit);
		}

		public static void CalcDestRect(int imageWidth, int imageHeight, ref RectangleF dest, float distortLimit = 1)
		{
			// calculates where to draw an image within a destination rectangle - shrinking the image as needed
			float ratioImage = (float)imageWidth / imageHeight;
			float ratioDest = dest.Width / dest.Height;
			Debug.Assert(distortLimit > 0.999, "sistortLimit should be >=1");
			if (dest.Width > imageWidth && dest.Height > imageHeight)
			{
				// special case - image smaller than target - draw it centred, don't expand it
				dest.Inflate(-(dest.Width - imageWidth) / 2, -(dest.Height - imageHeight) / 2);
			}
			else
			{
				float oldDimension;
				if (ratioDest / ratioImage > distortLimit)
				{
					// dest too wide
					oldDimension = dest.Width;
					dest.Width = dest.Height * ratioImage;
					dest.X += (oldDimension - dest.Width) / 2;
				}
				if (ratioImage / ratioDest > distortLimit)
				{
					// dest too high
					oldDimension = dest.Height;
					dest.Height = dest.Width / ratioImage;
					dest.Y += (oldDimension - dest.Height) / 2;
				}
			}
		}

		public static void CalcDestRect(Size imageSize, ref RectangleF dest, float distortLimit = 1)
		{
			CalcDestRect(imageSize.Width, imageSize.Height, ref dest, distortLimit);
		}

		public static void CalcDestSize(int imageWidth, int imageHeight, ref int destWidth, ref int destHeight, float distortLimit = 1)
		{
			Rectangle rect = new Rectangle(0, 0, destWidth, destHeight);
			CalcDestRect(imageWidth, imageHeight, ref rect, distortLimit);
			destWidth = rect.Width;
			destHeight = rect.Height;
		}

		public static void CalcDestSize(Size imageSize, ref Size destSize, float distortLimit = 1)
		{
			int width = destSize.Width;
			int height = destSize.Height;
			CalcDestSize(imageSize.Width, imageSize.Height, ref width, ref height, distortLimit);
			destSize = new Size(width, height);
		}

		#endregion

		#region Focus

		public static Control CurrentFocus;

		internal static Control GetFocusControl()
		{
			// modified
			var ctr = CurrentFocus;
			if (ctr != null && ctr.ContainsFocus)
				return ctr;
			try
			{
				IntPtr focusHandle = Windows.GetFocus(); // returns null if handle is not to a .NET control
				if (!focusHandle.Equals(IntPtr.Zero))
					return Control.FromHandle(focusHandle);
				return null;
			}
			catch (Exception ex)
			{
				// will fail on non-Windows
				Utilities.LogSubError("GetFocusControl failed: " + ex.Message);
				return null;
			}
		}

		internal static List<Control> BuildTabStopList(Control container)
		{
			// builds a list of controls with tabstops inside container, including nested controls
			List<Control> list = new List<Control>();
			List<Control> temp = new List<Control>();
			foreach (Control ctr in container.Controls)
			{
				temp.Add(ctr);
			}
			temp.Sort(new TabSort());
			foreach (Control control in temp)
			{
				if (control.TabStop)
					list.Add(control);
				if (control.Controls.Count > 0)
				{
					List<Control> sub = BuildTabStopList(control);
					if (sub.Count > 0)
						list.AddRange(sub);
				}
			}
			return list;
		}

		private class TabSort : IComparer<Control>
		{

			public int Compare(Control x, Control y)
			{
				return x.TabIndex.CompareTo(y.TabIndex);
			}

		}

		internal static void IterateTabStop(Control container, bool forward)
		{
			// control should include line:
			//Protected Overrides Function ProcessTabKey(ByVal forward As Boolean) As Boolean
			//    IterateTabStop(Me, forward)
			//    Return True
			//End Function
			List<Control> list = BuildTabStopList(container);
			if (list.Count == 0)
				return;
			int direction = forward ? 1 : -1;
			int currentIndex = list.IndexOf(GetFocusControl());
			Debug.WriteLine("GetFocusControl is " + GetFocusControl() + ", index = " + currentIndex);
			int test = (currentIndex + direction + list.Count) % list.Count;
			while (test != currentIndex)
			{
				Control ctr = list[test];
				if (ctr.Enabled && ctr.TabStop)
					break;
				test = (test + direction) % list.Count;
				if (currentIndex < 0)
					currentIndex = 0; // don't want to set this before the loop, because it would always stop at 0 even if disabled;  without this however it could go into an infinite loop
			}
			list[test].Focus();
		}

		internal static void FocusIfNotInPalette(Control ctr)
		{
			// give focus to the control, it is NOT inside a palette form.  It is focused if it is inside a palette accordion which is part of the main form
			Form frm = ctr.FindForm();
			if (frm is frmPalette)
				return;
			ctr.Focus();
		}

		#endregion

		#region RectangleF graphics

		// there are some graphics commands which do not accept RectangleF parameters, but do accept a list of singles
		// we cannot use Rectangle.Round and the integer version - because we are working in millimetres and whole millimetres is rather a lot
		// therefore it is convenient to define a RectangleF version (the 4 single version is rather long-winded!)

		public static void FillPie(this Graphics gr, Brush br, RectangleF rct, float startAngle, float sweepAngle)
		{
			if (rct.Width == 0 || rct.Height == 0)
				return;
			gr.FillPie(br, rct.X, rct.Y, rct.Width, rct.Height, startAngle, sweepAngle);
		}

		//likewise there isn't a DrawImage with ImageAttr where the source is specified as a rectangle
		// and it MUST be a rectangle as it needs to come from img.GetBounds - 0,0,img.Width, img.Height
		public static void DrawImage(this Graphics gr, Image img, RectangleF dest, ImageAttributes attributes)
		{
			// bizarrely there isn't even a version with ImageAttributes which accepts RectangleF as the destination.  Must specify a parallelogram
			GraphicsUnit temp = GraphicsUnit.Pixel;
			RectangleF source = img.GetBounds(ref temp);
			// bizarrely there is no version with ImageAttributes which accepts RectangleF as the destination.  Must specify a parallelogram
			// in TL, TR, BL order
			PointF[] destPoints = { new PointF(dest.Left, dest.Top), new PointF(dest.Right, dest.Top), new PointF(dest.Left, dest.Bottom) };
			gr.DrawImage(img, destPoints, source, GraphicsUnit.Pixel, attributes);
		}

		// fewer DrawImage with rct, rct - but more with points, so rectangle versions provided:
		public static void DrawImage(this Graphics gr, Image img, RectangleF dest, RectangleF source, ImageAttributes attributes)
		{
			PointF[] ptDest = { new PointF(dest.Left, dest.Top), new PointF(dest.Right, dest.Top), new PointF(dest.Left, dest.Bottom) };
			gr.DrawImage(img, ptDest, source, GraphicsUnit.Pixel, attributes);
		}

		public static void DrawImage(this Graphics gr, Image img, Rectangle dest, Rectangle rctSource, ImageAttributes attributes)
		{
			Point[] ptDest = { new Point(dest.Left, dest.Top), new Point(dest.Right, dest.Top), new Point(dest.Left, dest.Bottom) };
			gr.DrawImage(img, ptDest, rctSource, GraphicsUnit.Pixel, attributes);
		}

		public static void DrawImage(this Graphics gr, Image img, Rectangle dest, ImageAttributes attributes)
		{
			// bizarrely there isn't even a version with ImageAttributes which accepts RectangleF as the destination.  Must specify a parallelogram
			GraphicsUnit temp = GraphicsUnit.Pixel;
			RectangleF source = img.GetBounds(ref temp);
			gr.DrawImage(img, dest, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
		}

		#endregion

		#region Scaling

		public static void ScaleDPI(Control ctr)
		{
			// we need to adjust the dialogs if the screen DPI changes, because this changes how much space the fonts  require
			// (even though we use fixed size fonts).  Setting the AutoScaleMode to DPI is just useless, this makes most controls bigger
			// but doesn't move them so that they end up on top of each other.  It seems to assume that everything is inside table layout panels
			// (whereas ironically we don't actually need to adjust anything inside a table layout panel, because inside these panels
			// we usually use AutoSize on the individual controls, which will do everything we need.  We only actually need AutoScale
			// on controls outside these panels)

			// we adjust the position and size.  We ignore margin and padding - these are usually only a couple of pixels, so might well
			// round off to the same number anyway.  And even if they could be increased, it's possibly better to keep smaller to save space
			// when the user has opted to make everything bigger
			if (ctr is Form form)
				Debug.Assert(form.AutoScaleMode == AutoScaleMode.None, "Form does not have AutoScaleMode=None"); // they should be set to this to work correctly
			if (SystemDPI == 96)
				return; // everything is designed at 96 DPI (Windows default)

			ctr.SuspendLayout();
			ctr.Width = ctr.Width * SystemDPI / 96;
			ctr.Height = ctr.Height * SystemDPI / 96;
			if (!(ctr is Form))
			{
				ctr.Left = ctr.Left * SystemDPI / 96;
				ctr.Top = ctr.Top * SystemDPI / 96;
			}
			// the layout panels are adjusted above, i.e. the entire panel is made larger, but internally they should sort themselves out
			// and we don't need to adjust the sub controls
			if (!(ctr is TableLayoutPanel) && !(ctr is FlowLayoutPanel) && !(ctr is ButtonPanel)) // latter cos it will have set size from its ButtonSize
			{
				// (allowing ButtonPanel to be affected here would mean button size depended on when they were added)
				foreach (Control objSub in ctr.Controls)
				{
					ScaleDPI(objSub);
				}
			}
			ctr.ResumeLayout();
		}

		/// <summary>Matching version in DrawResources for actual output.  This version for general UI (ie on screen only) based on system scale</summary>
		public static float SHADOWXOFFSET
		{
			get { return 1 * MillimetreSize; }
		}

		/// <summary>Matching version in DrawResources for actual output.  This version for general UI (ie on screen only) based on system scale</summary>
		public static float SHADOWYOFFSET
		{
			get { return 2 * MillimetreSize; }
		}

		/// <summary>the amount to increase the pen width when drawing the selection highlight
		/// Matching version in DrawResources for actual output.  This version for general UI (ie on screen only) based on system scale</summary>
		public static float HIGHLIGHTEXTRAWIDTH
		{
			get { return 1.1f * MillimetreSize; }
		}

		/// <summary>Matching version in DrawResources for actual output.  This version for general UI (ie on screen only) based on system scale</summary>
		public static float SHADOWEXTRAWIDTH
		{
			get { return 1 * MillimetreSize; }
		}

		/// <summary>Matching version in DrawResources for actual output.  This version for general UI (ie on screen only) based on system scale</summary>
		public static float SELECTIONBORDERWIDTH
		{
			get { return (Geometry.THINLINE + 0.5f) * MillimetreSize + HIGHLIGHTEXTRAWIDTH; }
		}

		/// <summary>An amount extra to refresh to be sure to cover miscellaneous large refresh borders (eg green drop indicator) </summary>
		public static float LARGEEXTRAREFRESH
		{
			get { return 5 * MillimetreSize; }
		}

		/// <summary>Graphical size to use to get 1 mm in output IN CURRENT DOCUMENT.  Assigned when document changed</summary>
		public static float MillimetreSize;

		#endregion

		#region Images
		public static Bitmap ImageOfControl(Control control)
		{
			Bitmap bmp = new Bitmap(control.Width, control.Height);
			Graphics gr = Graphics.FromImage(bmp);
			gr.Clear(Color.Transparent);
			control.DrawToBitmap(bmp, new Rectangle(0, 0, control.Width, control.Height));
			gr.Dispose();
			return bmp;
		}

		/// <summary>Returns the file extension (with .) for an image format, or null if not known</summary>
		public static string FileExtensionFromEncoder(this ImageFormat format)
		{
			try
			{
				return ImageCodecInfo.GetImageEncoders()
					.First(x => x.FormatID == format.Guid)
					.FilenameExtension
					.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
					.First()
					.Trim('*')
					.ToLower();
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion

		#region Positioning and bounds

		private static Rectangle g_TotalScreenBounds = Rectangle.Empty;

		public static Rectangle CheckRectOnScreen(Rectangle rct)
		{
			Debug.Assert(!g_TotalScreenBounds.IsEmpty);
			// We check that the given rectangle is ENTIRELY within the bounds of the overall rectangle containing the screens
			// note that combined screen works on total screen area rather than working area.  Possibly change this
			if (rct.Right > g_TotalScreenBounds.Right)
				rct.X = g_TotalScreenBounds.Right - rct.Width;
			if (rct.Bottom > g_TotalScreenBounds.Bottom)
				rct.Y = g_TotalScreenBounds.Bottom - rct.Height;
			if (rct.X < g_TotalScreenBounds.Left)
				rct.X = g_TotalScreenBounds.Left;
			if (rct.Y < g_TotalScreenBounds.Top)
				rct.Y = g_TotalScreenBounds.Top;
			// however if there are displays of different sizes this may not be entirely covered by screens
			// so we also need to check that the rectangle is at least partly within one of the screens
			foreach (Screen screen in Screen.AllScreens)
			{
				Rectangle overlap = Rectangle.Intersect(screen.WorkingArea, rct);
				if (overlap.Width > 50 && overlap.Height > 50)
					return rct; // rectangle is OK
			}
			// main exit from the function was above.  We only drop here if the rectangle is not really on any of the screens
			// finds the screen mostly containing this:
			Screen closest = Screen.FromRectangle(rct);
			// Now bring the rectangle completely within this screen
			Rectangle target = closest.WorkingArea; // not sure how fast this function is, so caching the result
			if (rct.Right > target.Right)
				rct.X = target.Right - rct.Width;
			if (rct.Bottom > target.Bottom)
				rct.Y = target.Bottom - rct.Height;
			if (rct.X < target.Left)
				rct.X = target.Left;
			if (rct.Y < target.Top)
				rct.Y = target.Top;
			return rct;
		}

		/// <summary>Like CheckRectOnScreen but checks rectangle sufficiently on screen to be suitable for a form position (allows it to go outside screen quite a bit as long as title accessible to drag) </summary>
		public static Rectangle CheckFormRectOnScreen(Rectangle rct)
		{
			Screen screen = Screen.FromPoint(rct.Centre());
			Rectangle working = screen.WorkingArea;
			rct.Y = Math.Max(working.Y, rct.Y); // TOP of form needs to be on screen.  It can protrude off bottom
			rct.Y = Math.Min(working.Bottom - 20, rct.Y);
			int centreX = rct.Centre().X;
			if (centreX < working.X) // if more than half outside on X, then it is brought fully inside.  Otherwise left overlapping a bit
				rct.X = working.X;
			if (centreX > working.Right)
				rct.X = working.Right - rct.Width;
			return rct;
		}

		/// <summary>For a form which has not yet been displayed, this configures it to appear beside the given rectangle (e.g. so that an editor can appear beside the object it is editing)</summary>
		public static void PositionNewFormBeside(Form frmNew, Rectangle beside)
		{
			Rectangle screen = Screen.FromRectangle(beside).WorkingArea;
			int encroach = 100; // Fraction of the target which can be covered
			frmNew.StartPosition = FormStartPosition.Manual;
			do
			{
				if (screen.Right - beside.Right + beside.Width / encroach >= frmNew.Width)
				{
					frmNew.Left = Math.Min(screen.Right - frmNew.Width, beside.Right);
					frmNew.Top = Math.Min(screen.Bottom - frmNew.Height, beside.Top);
					return;
				}
				if (screen.Bottom - beside.Bottom + beside.Height / encroach >= frmNew.Height)
				{
					frmNew.Left = Math.Min(screen.Right - frmNew.Width, beside.Left);
					frmNew.Top = Math.Min(screen.Bottom - frmNew.Height, beside.Bottom);
					return;
				}
				if (beside.Left - screen.Left + beside.Width / encroach >= frmNew.Width)
				{
					frmNew.Left = Math.Max(screen.Left, beside.Left - frmNew.Width);
					frmNew.Top = Math.Min(screen.Bottom - frmNew.Height, beside.Top);
					return;
				}
				if (beside.Top - screen.Top + beside.Height / encroach >= frmNew.Height)
				{
					frmNew.Left = Math.Min(screen.Right - frmNew.Width, beside.Left);
					frmNew.Top = Math.Min(screen.Top, beside.Top - frmNew.Height);
					return;
				}
				encroach /= 4;
			} while (!(encroach < 4));
			// Cannot realistically get the form beside the target;
			frmNew.StartPosition = FormStartPosition.CenterScreen;
		}

		/// <summary>Formats the rectangle for display to the user, with origin at the bottom left</summary>
		public static string RectangleToUserString(RectangleF rct, Page page, string format = "0")
		{
			if (page == null)
				return "???";
			StringBuilder output = new StringBuilder();
			output.Append("X=").Append(rct.X.ToString(format)).Append(" Y=").Append((-page.Bounds.Top + rct.Y).ToString(format)); // note Y origin at top
			output.Append(" ").Append(Strings.Item("Width_Abbrev")).Append("=").Append(rct.Width.ToString(format));
			output.Append(" ").Append(Strings.Item("Height_Abbrev")).Append("=").Append(rct.Height.ToString(format));
			return output.ToString();
		}

		/// <summary>Sets the form bounds so that the CLIENT bounds go to the given screen coordinates</summary>
		public static void SetFormClientBounds(Form frm, Rectangle target)
		{
			Point pt = frm.PointToScreen(Point.Empty);
			target.X -= (pt.X - frm.Left);
			target.Y -= (pt.Y - frm.Top);
			target.Width += (frm.Width - frm.ClientSize.Width);
			target.Height += (frm.Height - frm.ClientSize.Height);
			frm.Bounds = target;
		}

		#endregion

		public static DialogResult QuestionBox(string translateable, MessageBoxButtons buttons)
		{
			return MessageBox.Show(Strings.Translate(translateable), Strings.Item("App"), buttons);
		}

		internal static Control YoungestChildUnderMouse(Control within)
		{
			Control control = YoungestChildAtDesktopPoint(within, Cursor.Position);
			if (control == within)
				return null;
			// this would have been returned is it was outside the top level Form anyway
			//The sub function deliberately returns containers which the point is within, it dismisses all children of a container
			// but we don't really need to return the form itself if it missed all the controls on the form
			return control;
		}

		private static Control YoungestChildAtDesktopPoint(Control topControl, Point desktopPoint)
		{
			Control control = topControl.GetChildAtPoint(topControl.PointToClient(desktopPoint), GetChildAtPointSkip.Invisible);
			if (control is TabPage && topControl is TabControl tabControl)
			{
				// .net seems to return the wrong page - forcibly select the one which is displayed
				control = tabControl.SelectedTab;
			}
			if (control == null)
				return topControl;
			if (!control.HasChildren)
				return control;
			return YoungestChildAtDesktopPoint(control, desktopPoint);
		}

		#region Keys

		private static readonly KeysConverter g_KeyConverter = new KeysConverter();

		public static string KeyDescription(Keys key)
		{
			StringBuilder output = new StringBuilder();
			if ((key & Keys.Shift) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_ShiftModifier"));
			if ((key & Keys.Control) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_ControlModifier"));
			if ((key & Keys.Alt) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_AltModifier"));
			key = key & ~(Keys.Shift | Keys.Control | Keys.Alt);
			if (key == Keys.None)
				return ""; // regardless of modifiers
			else if (key >= Keys.D0 && key <= Keys.D9)
			{
				output.Append((char)key);
				// will let the NumPad ones report the enum name for the moment
			}
			else
			{
				string textID = "Key_" + key;
				if (Strings.Exists(textID))
					output.Append(Strings.Item(textID));
				else
				{
					// KeyConverter not actually so useful, so we only use for keys we haven't already translated
					// http://stackoverflow.com/questions/10288829/how-to-get-shortcut-text-from-system-windows-forms-keys-code
					output.Append(g_KeyConverter.ConvertToString(key)); // eKey.ToString)
				}
			}
			return output.ToString();
		}

		/// <summary>this returns a much shorter description than KeyDescription</summary>
		public static string KeyShortDescription(Keys key)
		{
			StringBuilder output = new StringBuilder();
			//If (eKey And Keys.Shift) > 0 Then output.Append(ChrW(&H2191))
			//If (eKey And Keys.Alt) > 0 Then output.Append(Chr(&HAA))
			if ((key & Keys.Shift) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_ShiftModifier_Short"));
			if ((key & Keys.Alt) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_AltModifier_Short"));
			if ((key & Keys.Control) > 0)
				output.Append(Strings.ItemNoTranslateMode("Key_ControlModifier_Short"));
			key = key & ~(Keys.Shift | Keys.Control | Keys.Alt);
			if (key == Keys.None)
				return ""; // regardless of modifiers
			else if (key >= Keys.D0 && key <= Keys.D9)
				output.Append((char)key);
			else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
				output.Append((key - Keys.NumPad0).ToString());
			else if (key == Keys.Oemcomma)
				output.Append(",");
			else if (key == Keys.OemPeriod)
				output.Append(".");
			else if (key == Keys.OemMinus)
				output.Append("-");
			else
			{
				string textID = "Key_" + key;
				if (Strings.Exists(textID))
					output.Append(Strings.Item(textID));
				else
					output.Append(key);
			}
			return output.ToString();
		}

		#endregion

	}

}
