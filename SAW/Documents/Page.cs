using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

// ReSharper disable IsExpressionAlwaysFalse

namespace SAW
{
	public partial class Page : Datum, IDisposable, IShapeContainer
	{

		public delegate void ShapeNotifiedIndirectChangeEventHandler(Shape shape, ChangeAffects affected, RectangleF area);
		/// <summary>raised when a shape notifies about some indirect changes.  Some might be handled internally</summary>
		public event ShapeNotifiedIndirectChangeEventHandler ShapeNotifiedIndirectChange;

		#region Values
		/// <summary>size available to the software, excluding margin: readonly as Size</summary>
		private SizeF m_Size;
		/// <summary>physical size of the paper including margin.  Size is this minus Margin on each side</summary>
		public SizeF PhysicalSize;
		public float Margin = 7F; // Always 0 for palettes at the moment (editing screen enforces this, although it also largely irrelevant)
		public Color Colour = Color.White; // background colour of the paper
		public const float DEFAULTMARGIN = 7; // millimetres on every side
		public PointF Origin = PointF.Empty; // only affects coordinates displayed to the user

		private SharedReference<SharedImage> m_Background; // The image is assumed to be a bitmap if tiling
		public enum BackgroundImageModes : byte
		{
			FitWithin = 0,
			Stretch = 1, // Values are stored in data and cannot be changed
			FitOutside = 2,
			Centre = 3,
			Tile = 4,
			/// <summary>Forces set to keep AR matching image</summary>
			LockAR = 5
		}
		public BackgroundImageModes BackgroundImageMode = BackgroundImageModes.Stretch;

		/// <summary>True if first item is rendered on top.  Not stored in data, rather this is written from the same item in Document.  (But must be copied here as rendering code has no reference to the document)</summary>
		public bool ReverseRenderOrder;

#if SAW
		/// <summary>Or -1 for not set (was a string in SAW6 and stored in SAW6Doc header)</summary>
		public int HelpSAWID = -1;
#endif

		#endregion

		public Page()
		{
			Paper paper = Paper.CreateNew(Paper.Papers.Square, null);
			paper.SetIntervals(10, 10, 5);
			paper.GridColour = Color.LightGreen;
			Paper = paper;
			SetSize(A4); // with default margin
		}

		public Page(Page copySettingsFrom)
		{
			m_Size = copySettingsFrom.Size;
			PhysicalSize = copySettingsFrom.PhysicalSize;
			Margin = copySettingsFrom.Margin;
			Paper = Paper.CreateNew(copySettingsFrom.Paper.PaperType, copySettingsFrom.Paper);
			BackgroundImage = copySettingsFrom.BackgroundImage;
			BackgroundImageMode = copySettingsFrom.BackgroundImageMode;
		}

		#region Paper, page sizes etc, background

		public Paper Paper { get; set; }

		/// <summary>the bounding area of the page in data coordinates</summary>
		public RectangleF Bounds
		{
			[DebuggerStepThrough()]
			get { return new RectangleF(0, -Size.Height, Size.Width, Size.Height); }
		}

		public static SizeF A4 = new SizeF(210, 297);
		public static SizeF WorksheetSize = new SizeF(179, 253); // 85% of A4 (used landscape)
		public static List<SizeF> PossibleSizes = new List<SizeF>(); // size, portrait
		public static List<string> PossibleSizeNames = new List<string>(); // list must match the one above, this is the text ID of the name for that size

		/// <summary>Size available to the software, excluding margin</summary>
		public SizeF Size
		{
			[DebuggerStepThrough()]
			get { return m_Size; }
		}

		public void SetSize(SizeF includingMargin, float setMargin = 7)
		{
			Margin = setMargin > 0 ? setMargin : 0.0F;
			PhysicalSize = includingMargin;
			m_Size = new SizeF(PhysicalSize.Width - Margin * 2, PhysicalSize.Height - Margin * 2);
		}

		public void SetSizeExcludingMargin(SizeF excludingMargin)
		{
			SetSize(new SizeF(excludingMargin.Width + Margin, excludingMargin.Height + Margin), Margin);
		}

		static Page()
		{
			PossibleSizes.Add(A4);
			PossibleSizeNames.Add("A4");
			PossibleSizes.Add(new SizeF(147, 210));
			PossibleSizeNames.Add("A5");
			PossibleSizes.Add(WorksheetSize);
			PossibleSizeNames.Add("ScreenWorksheet_PaperSize");
		}

		public static bool IdentifySize(SizeF sz, out int listIndex, out bool landscape)
		{
			// Checks of the given size exists in PossibleSizes (either way round)
			// returns true if found, in which case the out parameters are defined.  IntListIndex is the index of this size in PossibleSizes
			// (but note that sz will not match PossibleSizes(intListIndex) if it is landscape)
			SizeF invert = sz.Flip();
			landscape = false;
			for (int index = 0; index <= PossibleSizes.Count - 1; index++)
			{
				Debug.Assert(PossibleSizes[index].Width <= PossibleSizes[index].Height, "Page.PossibleSizes should have been set up with each size in portrait");
				if (sz.Equals(PossibleSizes[index]))
				{
					listIndex = index;
					landscape = false;
					return true;
				}
				else if (invert.Equals(PossibleSizes[index]))
				{
					listIndex = index;
					landscape = true;
					return true;
				}
			}
			listIndex = -1;
			return false;
		}

		public void BringShapesWithinPage(Transaction transaction)
		{
			// this is called after the page size has been changed.  It moves all shapes back within the page
			// measures are always ignored when doing this
			RectangleF bounds = TotalBounds();
			if (bounds.Right <= Size.Width && bounds.Top >= -Size.Height)
				return; // everything already fits
						// first calculate the global shift, which gets things to fit as best as possible.  This will always move in complete grid steps
			float xGlobal = 0; // X will be negative if moving, never positive
			float yGlobal = 0; // Y opposite
			if (bounds.Right > Size.Width)
				xGlobal = Math.Max(-bounds.Left, Size.Width - bounds.Right); // Max ie least -ve
			if (-bounds.Top > Size.Height)
				yGlobal = Math.Min(-bounds.Bottom, -Size.Height - bounds.Top);
			float step = Paper.ScalarSnapStep(0, true);
			if (step != 0)
				xGlobal = (float)(Math.Round(xGlobal / step + 0.45 * Math.Sign(xGlobal)) * step);
			step = Paper.ScalarSnapStep(Geometry.ANGLE90, true);
			if (step != 0)
				yGlobal = (float)(Math.Round(yGlobal / step + 0.45f * Math.Sign(yGlobal)) * step);

			TransformMove move = new TransformMove(0, 0);
			if (bounds.Width <= Size.Width && bounds.Height <= Size.Height)
			{
				// There is enough space on the page for the shape to fit if they are all universally moved
				foreach (Shape shape in m_Shapes)
				{
					transaction.Edit(shape);
					move.SetDelta(xGlobal, yGlobal);
					shape.ApplyTransformation(move);
				}
			}
			else
			{
				// The shapes in their current configuration cannot all fit; move shapes individually as needed
				// in principle only shapes completely outside the bounds are moved, shapes overlapping the edge are left in place, because the user can pick them up up and move them individually
				// in practice we require at least 10% of the shape to be in bounds, because if it is only the odd pixel it might be effectively invisible
				// first calculate a global shift, much as above, to get them to fit as best as possible
				// then in addition to this each shape will be moved as much as necessary further to get it onto the new page
				foreach (Shape shape in m_Shapes)
				{
					RectangleF shapeBounds = shape.Bounds;
					float X = 0;
					float Y = 0;
					// it is only really necessary to test the top and right, as the left and bottom of the page are always at 0, 0
					if (shapeBounds.Left + xGlobal + shapeBounds.Width / 10 > Size.Width)
						X = Size.Width - shapeBounds.Width - shapeBounds.Left - xGlobal;
					if (shapeBounds.Bottom + yGlobal - shapeBounds.Height / 10 < -Size.Height)
						Y = -Size.Height + shapeBounds.Height - shapeBounds.Bottom - yGlobal;
					move.SetDelta(X + xGlobal, Y + yGlobal);
					transaction.Edit(shape);
					shape.ApplyTransformation(move);
				}
			}
			UpdateAllIntersections();
		}

		public bool IsLandscape => PhysicalSize.Width >= PhysicalSize.Height;

		public string SizeName
		{
			get
			{
				int listIndex;
				bool landscape;
				if (IdentifySize(Size, out listIndex, out landscape))         // it is a named size
					return PossibleSizeNames[listIndex] + (landscape ? Strings.Item("PageSize_Landscape") : Strings.Item("PageSize_Portrait"));
				else
					return Size.Width.ToString("0") + " x " + Size.Height.ToString("0") + Strings.Item("mm");
			}
		}

		public SharedImage BackgroundImage
		{
			get { return m_Background; }
			set { m_Background = value; }
		}

		/// <summary>True if the aspect ratio of the page should always be matched to the image.  But ALSO means resizing window always resizes content</summary>
		public bool LockARToBackgroundImage => m_Background!=null && BackgroundImageMode== BackgroundImageModes.LockAR;

		#endregion

		#region Graphics
		public delegate void RequestRefreshEventHandler(RectangleF dataRect); // fired when changes to the data will require a refreshing in any viewer.  Parameter is the invalid area in data coordinates
		public event RequestRefreshEventHandler RequestRefresh;

		public float AssumeRefreshScale = 1; // where critical is passed to each function (especially drawing).
											 // this can be used for some things which do auto refresh - and scale not always easily available

		public void DrawBackground(Canvas gr, float scale, bool background = true, bool grid = true, bool origin = true)
		{
			RectangleF bounds = new RectangleF(0, -Size.Height, Size.Width, Size.Height + 1); // +1 needed to stop some artifacts at bottom of page when zooming out (between page and grey, when zooming from being large enough there is no grey).  Not sure if it affects the image
			gr.StartGroup();
			if (background)
			{
				if (m_Background == null || !m_Background.HasContent || BackgroundImageMode == BackgroundImageModes.Centre || BackgroundImageMode == BackgroundImageModes.FitWithin || m_Background.Content.MemoryImage.CanHaveAlpha)
				{
					// Colour required if no image, or for some image modes
					Color col = Colour;
					using (var br = gr.CreateFill(col))
					{
						Debug.Assert(col.A == 255);
						gr.Rectangle(bounds, fill: br);
					}

				}
				if (m_Background?.HasContent ?? false)
					DrawBackgroundImage(gr, m_Background, BackgroundImageMode, bounds, Geometry.INCH / gr.DpiX);
			}
			if (grid)
				Paper.Draw(gr, this, scale);
			if (origin && !Origin.IsEmpty && Globals.Root.CurrentConfig.ReadBoolean(Config.Display_Origin))
			{
				Stroke pen;
				if (Paper.GridVisible)
				{
					pen = gr.CreateStroke(Color.LightGray, 3.5f * Geometry.THINLINE); // thicker if grid - otherwise not visible
					pen.DashStyle = DashStyle.Dot;
				}
				else
					pen = gr.CreateStroke(Color.LightGray, Geometry.THINLINE);
				gr.DrawLine(0, Origin.Y, Size.Width, Origin.Y, pen);
				gr.DrawLine(Origin.X, 0, Origin.X, -Size.Height, pen);
				pen.Dispose();
			}
			gr.EndGroup();
		}

		public static void DrawBackgroundImage(Canvas gr, SharedImage image, BackgroundImageModes mode, RectangleF bounds, float scale)
		{
			// This is shared because it might be useful for backgrounds within shapes, although textures do the brush creation in Shape.PrepareDraw
			var sz = image.GetSize();
			// The preferred size given to the drawing methods is largely arbitrary; the background image is unlikely to be a resource!
			SizeF imageSize;
			switch (mode)
			{
				case BackgroundImageModes.Tile:
					using (var brush = gr.CreateTextureBrush(image, scale)) //As New TextureBrush(objImage.GetImage)
						gr.Rectangle(bounds, null, brush);
					break;
				case BackgroundImageModes.Stretch: // Note that this changes the aspect ratio
					gr.DrawImage(image, 128, bounds);
					break;
				case BackgroundImageModes.Centre:
					imageSize = sz.ToSizeF().MultiplyBy(scale);
					// * INCH / gr.DpiX)	' RctBounds should be in millimetres, but image will be measured in pixels
					// the image is centred, regardless of whether it fits within the page or outside
					PointF corner1 = bounds.Centre() + imageSize.MultiplyBy(-0.5f); // is the top left corner
					gr.DrawImage(image, 128, new RectangleF(corner1, imageSize)); // Just using the DrawImage (..., PointF) gives some odd scaling
					break;
				case BackgroundImageModes.FitWithin:
				case BackgroundImageModes.LockAR:// in theory shouldn't matter how it's drawn in LockAR as AR should match.  But just in case this seems the safest option
					float ratio1 = Math.Min(bounds.Width / sz.Width, bounds.Height / sz.Height);
					imageSize = sz.ToSizeF().MultiplyBy(ratio1);
					PointF corner2 = bounds.Centre() + imageSize.MultiplyBy(-0.5f);
					gr.DrawImage(image, 128, new RectangleF(corner2, imageSize));
					break;
				case BackgroundImageModes.FitOutside: // Exactly as the above one, except for Max not Min
					float ratio = Math.Max(bounds.Width / sz.Width, bounds.Height / sz.Height);
					imageSize = sz.ToSizeF().MultiplyBy(ratio);
					PointF corner = bounds.Centre() + imageSize.MultiplyBy(-0.5f);
					gr.DrawImage(image, 128, new RectangleF(corner, imageSize));
					break;
				default:
					Debug.WriteLine("Unexpected BackgroundImageMode: " + mode);
					break;
			}
		}

		private static readonly Fill ErrorBrush = NetCanvas.CreateSolidFill(Color.FromArgb(100, Color.Tomato));
		public void DrawShapes(Canvas gr, float scale, float coordScale, StaticView view, bool omitMeasures = false, StaticView.InvalidationBuffer buffer = StaticView.InvalidationBuffer.Base)
		{
			//foreach (Shape shp in m_Shapes)
			foreach (Shape shape in (ReverseRenderOrder ? (m_Shapes as IEnumerable<Shape>).Reverse() : m_Shapes)) // must cast to Enumerable to avoid the List.Reverse which edits the actual list
			{
				if (gr.ClipBounds.IntersectsWith(shape.RefreshBounds()))
				{
					// Not sure whether intersection test is faster or not; most graphics will be ignored outside the clipping region, and for some shapes the refresh bounds is not so fast
					if (m_MovingSelection == false || !SelectedShapes.Contains(shape))
					{
						// draw normally
						try
						{
							if (SelectedShapes.Contains(shape))
								shape.Draw(gr, scale, coordScale, view, buffer, 230, 255, ReverseRenderOrder);
							else
								shape.Draw(gr, scale, coordScale, view, buffer, 255, 255, ReverseRenderOrder);
						}
						catch (Exception ex) when (!Globals.Root.IsDebug && gr is NetCanvas && !(gr as NetCanvas).IsPrinting) // errors drawing to PDF etc can be propagated.  Likewise when printing to paper
						{
							Debug.WriteLine(ex.ToString());
							if (shape.Bounds.Width > 0 && shape.Bounds.Height > 0)
								gr.Rectangle(shape.Bounds, fill: ErrorBrush);
						}
					}
					else // no need to draw this - the selection version is sufficient
					{
						// we are moving this shape to a different position - drawn with very low alpha
						// shp.Draw(gr, sngScale, 15, 25)
					}
				}
			}
		}

		public void DrawSelected(Canvas gr, StaticView view, float scale, float coordScale, bool print)
		{
			// if bolPrint then this draws the shapes pretty much as normal, just filters to the selected ones
			// note this is not called when a transform is moving the selected shapes - they draw within current buffer as part of transform
			// (and also DrawShapes wont have drawn the base shape since m_bolMovingSelection)
			if (print)
			{
				foreach (Shape shp in SelectedShapes)
				{
					shp.Draw(gr, scale, coordScale, view, StaticView.InvalidationBuffer.Base, 255, 255, ReverseRenderOrder);
				}
			}
			else
			{
				foreach (Shape shp in SelectedShapes)
				{
					try
					{
						if (m_MovingSelection)
						{
							// want to draw them a little more solid in this case as the background version not being drawn
							// but first of all we need to draw a shadow if supported
							if (Globals.Root.CurrentConfig.ReadBoolean(Config.Moving_Shadow, true))
								shp.DrawShadow(gr, scale);
							shp.Draw(gr, scale, coordScale, view, StaticView.InvalidationBuffer.Selection, 110, 100, ReverseRenderOrder);
						}
						else
						{
							shp.Draw(gr, scale, coordScale, view, StaticView.InvalidationBuffer.Selection, 45, 25, ReverseRenderOrder);
							shp.DrawHighlight(gr, scale, coordScale);
#if DEBUG
							if (Shape.DiagnoseIntersections)
								shp.DrawIntersections(gr, scale);
#endif
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.ToString());
						gr.Rectangle(shp.Bounds, fill: ErrorBrush);
					}
				}
			}
		}

		public void DrawSelectionBoundary(RectangleF bounds, Canvas gr)
		{
			if (bounds.IsEmpty)
				return;
			SmoothingMode oldSmoothing = gr.SmoothingMode;
			gr.SmoothingMode = SmoothingMode.None;
			using (Stroke pen = gr.CreateStroke(Color.Red, 1))
			{
				pen.DashStyle = DashStyle.Custom;
				pen.DashPattern = new float[] { 8, 6, 4, 6 };
				gr.Rectangle(bounds, pen);
			}

			gr.SmoothingMode = oldSmoothing;
		}

		#endregion

		#region Thumbnails
		// used by ctrPage...  Not affected by Undo or object copying, or stored in file
		public Bitmap Thumbnail; // may be arbitrarily discarded, external code should not keep references;  call EnsureThumbnail each time this is needed
		public static Size FILEDOCUMENTTHUMBNAILSIZE = new Size(400, 400); // the size of the thumbnail to store in files
		public Bitmap GenerateThumbnail2(Size limit, float docCoordScale, int dpi = 72)
		{
			// returned image will have aspect:ratio of page, therefore usually being smaller than szLimit
			// NB szLimit in pixels; I am in mm;  but that's OK as graphics drawing expects a scale factor to be applied so that it is drawing in nominal mm
			// DOES NOT UPDATE Thumbnail property
			float scale = Math.Min(limit.Width / Size.Width, limit.Height / Size.Height);
			Bitmap bmp = new Bitmap((int)(Size.Width * scale), (int)(Size.Height * scale), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			bmp.SetResolution(dpi, dpi);
			using (Graphics gr = Graphics.FromImage(bmp))
			{
				using (NetCanvas canvas = new NetCanvas(gr))
				{
					gr.TranslateTransform(0, bmp.Height);
					gr.SmoothingMode = SmoothingMode.AntiAlias;
					gr.Clear(Colour);
					gr.ScaleTransform(scale, scale);
					if (m_Background != null)
					{
						DrawBackgroundImage(canvas, m_Background, this.BackgroundImageMode, Bounds, Geometry.INCH / 96);
						// Must use 96 not DPI here, because this needs to scale the image in relation to the page.  The DPI has been fed into sngScale which is transforming the graphics
					}
					this.DrawShapes(canvas, scale, docCoordScale, null, true, StaticView.InvalidationBuffer.NotApplicable);
					return bmp;
				}
			}

		}

		public void EnsureThumbnail(Size sz, float docCoordScale)
		{
			if (Thumbnail != null)
			{
				if (Thumbnail.Width != sz.Width && Thumbnail.Height != sz.Height)
				{
					// wrong size
					Thumbnail.Dispose();
					Thumbnail = null;
				}
			}
			if (Thumbnail == null)
				Thumbnail = GenerateThumbnail2(sz, docCoordScale);
		}

		public void DiscardThumbnail()
		{
			Thumbnail?.Dispose();
			Thumbnail = null;
		}

		#endregion

		#region List of shapes and measures
		private List<Shape> m_Shapes = new List<Shape>();

		public enum HitTestMode
		{
			/// <summary>Starts behind the currently selected item(s), thus allowing selection to iterate through a stack of overlapping objects</summary>
			StartBehindSelectionAlways,
			/// <summary>As StartBehindSelectionAlways, but only if the click was within the bounds of the currently selected items</summary>
			StartBehindSelectionWithinBounds,
			/// <summary>Simple test: Start checking from the front</summary>
			StartFront,
			OnlySelection, // only checks already selected items
			StartSelectionFilled, // used by the label tool, this starts with the already selected item (if any) and treats all items as filled
			ActiveContent // as StartFront, but checking for anything which responds to mouse, and is not necessarily editable (i.e. ignores protection).  Currently checks for ButtonShape
		}

		public List<Shape> HitTestAll(PointF clickPoint, float scale, Shape.Shapes onlyType = Shape.Shapes.Undefined, bool treatAsFilled = false)
		{
			// scale should specify the display scale, so the tolerances can be adjusted
			// returns all of the shapes hit, starting at the front of the Z order
			// the shapes themselves usually have a tolerance on hitting a line.  If a shape is very narrow we need to apply a similar tolerance to the bounding box
			List<Shape> results = new List<Shape>();
			if (IsDisposed)
				return results;
			float minimumDimension = Line.LINEHITTOLERANCE / scale;
			foreach (Shape shpCurrent in new ComplexEnumerator(true ^ ReverseRenderOrder, true, m_Shapes).Where(s => s.IsVisible))
			{
				if (onlyType != Shape.Shapes.Undefined && shpCurrent.ShapeCode != onlyType)
					continue;

				RectangleF bounds = shpCurrent.Bounds;
				if (bounds.Width < minimumDimension)
					bounds.Inflate((minimumDimension - bounds.Width) / 2, 0);
				if (bounds.Height < minimumDimension)
					bounds.Inflate(0, (minimumDimension - bounds.Height) / 2);
				if (bounds.Contains(clickPoint))
				{
					if (shpCurrent.HitTestDetailed(clickPoint, scale, treatAsFilled) && shpCurrent.Container.AllowClick(clickPoint))
						results.Add(shpCurrent);
				}
			}
			return results;
		}

		/// <summary>Returns the target shape, if any at given point - finds front most</summary>
		/// <param name="clickPoint">Location at which to search</param>
		/// <param name="forShapes">List of shapes being moved/dropped. The targets are asked if they will accept these. Also t.Sleep the fifthhis list, and containers within them will not be counted. </param>
		/// <returns>Frontmost target, Nothing if no targets (does not return self)</returns>
		public IShapeTarget FindTarget(PointF clickPoint, List<Shape> forShapes)
		{
			return FindTarget(clickPoint, this, forShapes);
		}

		private IShapeTarget FindTarget(PointF clickPoint, IShapeContainer searchIn, List<Shape> forShapes)
		{
			foreach (Shape test in searchIn.Reverse)
			{
				if (test.AsTarget != null && test.Bounds.Contains(clickPoint))
				{
					if (forShapes != null && forShapes.Contains(test))
						continue;
					// test if it itself contains another container... (if it is a container)
					if (test.AsTarget != null)
					{
						var inner = FindTarget(clickPoint, (IShapeContainer)test, forShapes);
						if (inner != null)
							return (IShapeTarget)inner;
					}
					// Check if the container/target is willing to accept all these shapes
					var t = test.AsTarget; // to avoid lambda warning
					if (forShapes.All(s => t.Accept(s)))
						return test.AsTarget;
				}
			}
			return null;
		}

		private void ResetZ()
		{
			// can be used if the Z-order is changed to reset from first principles
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				m_Shapes[index].Z = index;
			}
		}

		public void FinishedModifyingContents(Transaction transaction, Shape.GrabMovement move = null)
		{
			ResetZ();
		}

		public void Clear(Transaction transaction)
		{
			if (m_Shapes.Count == 0)
				return;
			transaction.Edit(this);
			foreach (Shape shape in m_Shapes)
			{
				transaction.Delete(shape);
			}
			m_Shapes.Clear();
			if (SelectedShapes.Count > 0)
			{
				SelectedShapes.Clear();
				SelectionChanged?.Invoke();
			}
			RequestRefresh?.Invoke(new RectangleF(new PointF(0, -Size.Height), Size));
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public IEnumerable<Shape> Shapes
		{ // the list should not be modified externally.  This excludes measures.  The Page enumerator includes both
			[DebuggerStepThrough()]
			get { return m_Shapes; }
		}

		/// <summary>Sorts a list of shapes into ascending Z-order (i.e. front most at the end of the list)</summary>
		/// <remarks></remarks>
		internal class ZSort : IComparer<Shape>
		{
			// The complication is that the shapes may be within containers

			public int Compare(Shape x, Shape y)
			{
				if (x == y)
					return 0; // would get stuck below
				IShapeContainer objContainer = null;
				do
				{
					Shape xTop = ShapeBelowContainer(x, objContainer);
					Shape yTop = ShapeBelowContainer(y, objContainer);
					if (xTop != yTop)
						return xTop.Z.CompareTo(yTop.Z);
					// otherwise they are shapes within a shared container - try again looking within this container
					objContainer = (IShapeContainer)xTop;
				} while (true);
			}

			private Shape ShapeBelowContainer(Shape shape, IShapeContainer container)
			{
				// iterates up through containers to find shape below given container.  Param can be nothing for page
				while (!(shape.Parent is Page) && shape.Parent != container)
				{
					shape = (Shape)shape.Parent;
				}
				return shape;
			}

		}

		public Shape HitTest(PointF ptClick, float scale, HitTestMode mode, bool ignore = false)
		{ // last param just for Splash compatibility
			if (IsDisposed)
				return null;
			// scale should specify the display scale, so the tolerances can be adjusted
			// start scanning from the front most one (at the end of the list) if nothing is selected [...IFF NOT bolDontStartBehindSelection]
			// if something is selected start scanning behind that (so that subsequent clicks select items deeper into the stack)

			if (m_Shapes.Count == 0)
				return null;
			if (mode == HitTestMode.StartBehindSelectionWithinBounds && !SelectedBounds(true).Contains(ptClick))
				mode = HitTestMode.StartFront;
			bool treatAsFilled = false;
			do // Implements a second attempt with bolTreatAsFilled= True if the first pass fails
			{
				IEnumerable<Shape> iterator; // IEnumerable not IEnumerator so that we can pass it to for each
				Shape waitUntil = null; // Ignore shapes until we reach this one (this is used to define the starting point of the search)
				switch (mode)
				{
					case HitTestMode.StartBehindSelectionAlways: // if WithinBounds, the test for within bounds was done above
					case HitTestMode.StartBehindSelectionWithinBounds:
					case HitTestMode.StartSelectionFilled:
						// But behaviour below if slightly different if StartSelectionFilled
						iterator = new ComplexEnumerator(true ^ ReverseRenderOrder, true, m_Shapes, m_Shapes);
						if (SelectedShapes.Count > 0)
							waitUntil = SelectedShapes[0];
						if (mode == HitTestMode.StartSelectionFilled)
							treatAsFilled = true;
						break;
					case HitTestMode.StartFront:
					case HitTestMode.ActiveContent:
						iterator = new ComplexEnumerator(true ^ ReverseRenderOrder, true, m_Shapes); // Only a single, simple iteration is needed
						break;
					case HitTestMode.OnlySelection:
						iterator = ReverseRenderOrder ? (IEnumerable<Shape>)SelectedShapes : new ReverseEnumerator(SelectedShapes); // note that this does not step into contained items - it literally goes through SelectedShapes
						break;
					default:
						throw new ArgumentException("Page.HitTest - unknown eMode: " + mode);
				}
				iterator = iterator.Where(s => s.IsVisible);

				// the shapes themselves usually have a tolerance on hitting a line.  If a shape is very narrow we need to apply a similar tolerance to the bounding box
				float expand = Line.LINEHITTOLERANCE / scale * 0.75f;
				foreach (Shape current in iterator)
				{
					Debug.Assert(current != null);
					if (current == null)
						continue;
					if (waitUntil != null)
					{
						if (waitUntil != current)
							continue;
						waitUntil = null;
						if (mode != HitTestMode.StartSelectionFilled)
							continue; // StartSelectionFilled (only) does include this item
					}
					if (mode == HitTestMode.ActiveContent)
					{
						// this is done as a completely different check.  We need to ignore the protection, and then always treat as filled
						// and, we don't need to expand the bounds, but do want to check if the shape might have active stuff
						if (current is ButtonShape && current.Bounds.Contains(ptClick))
							return current;
					}
					else
					{
						{
							RectangleF bounds = current.Bounds;
							// now apply extra line tolerance always - shape can re-restrict if desired
							bounds.Inflate(expand, expand);
							if (bounds.Contains(ptClick))
							{
								if (current.HitTestDetailed(ptClick, scale, treatAsFilled))
								{
									// And finally check that the shape is not in some sort of container which is clipping it
									if (current.Container.AllowClick(ptClick))
										return current;
								}
							}
						}
					}
				}
				if (treatAsFilled)
					break;
				treatAsFilled = true; // try again treating as filled
			}
			while (true);
			return null;
		}

		public Shape FindSingleActivity()
		{
			// If there is just one activity on the page, or just one selected this is returned
			var col = this.Where(x => (x.Flags & Shape.GeneralFlags.Activity) > 0);
			if (!col.Any())
				return null;
			if (col.Count() >= 2)
			{
				col = SelectedShapes.Where(x => (x.Flags & Shape.GeneralFlags.Activity) > 0);
				if (col.Count() != 1)
					return null; // either multiple selected, or none selected but not all non-selected; either way no unambiguous single one to select
			}
			return col.First(); // Is now just one, either from the first search, or once we restrict to only selected ones
		}

		public bool Contains(Shape shape) => this.Any(shp => shp == shape);

		/// <summary>excludes measures, because it probably doesn't matter these stick off the edges when printing
		/// (and basically this is only used when changing the page size, and it just seems unlikely it would be more fluent if measures were included)</summary>
		public RectangleF TotalBounds()
		{
			return Geometry.UnionRectangles(from shape in new ComplexEnumerator(false, false, m_Shapes) select shape.Bounds);
		}

		public void ChangeShapeZ(Shape shape, int newZ, Transaction transaction)
		{
			// used when changing the order within the text flow.  This works literally within the main shapes list, and ignores containers
			// We need to allow intNew to be off the ends of the list (?)
			if (!m_Shapes.Contains(shape))
			{
				Utilities.LogSubError("ChangeShapeZ: shape not within page shape list");
				return;
			}
			transaction.Edit(this);
			Debug.Assert(shape.Z == m_Shapes.IndexOf(shape));
			m_Shapes.RemoveAt(shape.Z);
			if (newZ > shape.Z)
				newZ -= 1; // after removing shape, index will be one different
			m_Shapes.Insert(newZ, shape);
			ResetZ();
			Debug.Assert(shape.Z == m_Shapes.IndexOf(shape));
		}

		public List<Shape> Contents => m_Shapes;
		// The page doesn't adjust the positions of shapes at all
		public bool MoveWithin(List<Shape> shapes, PointF target, Shape.GrabMovement move, Transaction transaction) => false;
		public bool AllowClick(PointF target) => true;

		public IShapeContainer AsParentContainer => this;


		/// <summary>Returns true if this contains a single shape which is IAutoSize</summary>
		/// <remarks>Used to detect palettes which should flow</remarks>
		public bool IsSingleAutoSize => m_Shapes.Count == 1 && m_Shapes[0] is IAutoSize;

#if DEBUG
		public void CheckZ(Shape shp) => Debug.Assert(shp == null || shp.GetClass() == Shape.Classes.Measure || shp.Z == m_Shapes.IndexOf(shp), "Shape Z-index wrong");

		/// <summary>debug only test that Z index is still correct </summary>
		public void CheckZ(string message)
		{
			if (m_Shapes.Count == 0)
				return;
			Debug.Assert(m_Shapes.Last().Z == m_Shapes.Count - 1, message);
		}

#endif

#if SAW
		public Scriptable FindScriptableByID(int searchID)
		{
			Scriptable found = null;
			Iterate(element =>
			{
				if (element is Scriptable && (element as Scriptable).SAWID == searchID)
					found = (Scriptable)element;
			});
			return found;
		}

		public int FindHighestUsedID()
		{
			int highest = 0;
			Iterate(element =>
			{
				if (element is Scriptable)
					highest = Math.Max((element as Scriptable).SAWID, highest);
			});
			return highest;
		}

#endif
		#endregion

		#region Custom enumerators
		// default enumerators enumerate measures and shapes and work within containers

		/// <summary>Enumerator which lists both shapes and measures.  And importantly it also steps into containers.</summary>
		/// <remarks>
		/// Actually an arbitrary set of lists can be provided
		/// A container will be returned AFTER its contents (even in reverse)
		/// IEnumerable is also implemented so that this can be explicitly passed to "for each"
		/// </remarks>
		private class ComplexEnumerator : IEnumerator<Shape>, IEnumerable<Shape>
		{
			private IEnumerator<Shape> m_Enumerator;
			private readonly Stack<IEnumerator<Shape>> m_Stack = new Stack<IEnumerator<Shape>>(); // Previous enumerators which we will return to after exhausting the current one
			private readonly Stack<bool> m_StackMoveNextCalled = new Stack<bool>();
			// these usually partway through iterating (I.e. MoveNext has already been called)
			// However neither logic (calling MoveNext first or not) entirely works for both containers and the original lists
			private readonly bool m_OnlyVisible;

			public ComplexEnumerator(bool reverse, bool onlyVisible, params List<Shape>[] shapes)
			{
				m_OnlyVisible = onlyVisible;
				// Parameter is one or more sets of shapes to iterate.  When forwards they are iterated in the given order
				Debug.Assert(shapes.Length >= 1);
				// First at all desired iterators to the stack.  This lets us test if they are empty or missing
				if (reverse)
				{
					for (int index = shapes.Length - 1; index >= 0; index--)
					{
						if (shapes[index] != null && shapes[index].Count > 0)
							m_Stack.Push(new ReverseEnumerator(shapes[index]));
					}
				}
				else
				{
					for (int index = 0; index <= shapes.Length - 1; index++)
					{
						if (shapes[index] != null && shapes[index].Count > 0)
							m_Stack.Push(shapes[index].GetEnumerator());
					}
				}
				if (m_Stack.Count > 0)
				{
					m_Enumerator = m_Stack.Pop();
					// flag that MoveNext has not been called for these
					foreach (var unused in m_Stack)
					{
						m_StackMoveNextCalled.Push(false);
					}
				}
				else
					m_Enumerator = null; // no shapes in any of the lists
			}

			Shape IEnumerator<Shape>.Current => m_Enumerator.Current;
			object IEnumerator.Current => m_Enumerator.Current;

			public bool MoveNext()
			{
				if (m_Enumerator == null)
					return false; // Can happen if no shapes available in any of the lists
				do
				{
					if (!m_Enumerator.MoveNext())
					{
						// nothing else in this list; try any other lists in the back
						if (m_Stack.Count == 0)
							return false; // no more items in previous lists either
						m_Enumerator = m_Stack.Pop();
						if (m_StackMoveNextCalled.Pop())
							continue; // MoveNext has already been called on the previous one.  Continue drops to outer while loop at end
									  // Otherwise...
						m_Enumerator.MoveNext(); // and check if it is a container...
					}
					while (m_Enumerator.Current is IShapeContainer && (!m_OnlyVisible || m_Enumerator.Current.IsVisible)) // While loop is in case first item in the container is itself a container
					{
						// must move into the container
						IEnumerator<Shape> create = ((IShapeContainer)m_Enumerator.Current).GetEnumerator();
						if (create.MoveNext())
						{
							m_Stack.Push(m_Enumerator);
							m_StackMoveNextCalled.Push(true);
							m_Enumerator = create;
						}
						else
							break;
					}
				} // but if this item is hidden keep iterating...
				while (m_OnlyVisible && !m_Enumerator.Current.IsVisible);
				return true;
			}

			public void Reset()
			{
				throw new NotSupportedException(); // only used by COM
			}

			public void Dispose()
			{
			}

			IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() => this;
			IEnumerator IEnumerable.GetEnumerator() => this;
		}

		IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() => new ComplexEnumerator(false, false, m_Shapes);
		IEnumerator IEnumerable.GetEnumerator() => new ComplexEnumerator(false, false, m_Shapes);

		public class ReverseEnumerator : IEnumerator<Shape>, IEnumerable<Shape>
		{
			// list shapes in reverse order, given a list of shapes.  This is more efficient (I think) than the Linq .reverse which would have to iterate them first
			// Also supports IEnumerable so we can do "for each X in Page.Reverse"

			private readonly List<Shape> m_List;
			private int m_Index;

			public ReverseEnumerator(List<Shape> list)
			{
				m_List = list;
				m_Index = m_List.Count; // iterators always start before the first item
			}

			Shape IEnumerator<Shape>.Current => m_List[m_Index];
			object IEnumerator.Current => m_List[m_Index];

			public bool MoveNext()
			{
				m_Index -= 1;
				return m_Index >= 0;
			}

			public void Reset()
			{
				throw new NotSupportedException(); // only needed for COM
			}

			public void Dispose()
			{
			}

			IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() => this;
			IEnumerator IEnumerable.GetEnumerator() => this;
		}

		public IEnumerable<Shape> Reverse => new ReverseEnumerator(m_Shapes);

		#endregion

		#region List of selected shapes

		// second parameter is true if this area of the image needs to be completely redrawn.  Third is true if GrabSpots need to be updated(either if selected shapes have changed or have moved; false in a few cases where the content of the shape has changed, but it is in the same position)
		// GrabSpot update both changes the list of them and refreshes
		public delegate void RefreshSelectionEventHandler(RectangleF dataRect, bool dataChanged, bool updateGrabSpots); //  fired when the list of selected items changes
		public event RefreshSelectionEventHandler RefreshSelection;

		public event NullEventHandler SelectionChanged;

		private bool m_MovingSelection = false; // true while we are applying a Transformation with Mode = Move to the selection

		internal int SelectedCount
		{ get { return SelectedShapes?.Count ?? 0; } }

		public void ToggleSelection(Shape shape)
		{
			if (shape == null)
				return;
			if (SelectedShapes.Contains(shape))
				SelectedShapes.Remove(shape);
			else
			{
				SelectedShapes.Add(shape);
				SelectedShapes.Sort(new ZSort());
			}
			shape.NotifySelectedChanged(SelectedShapes.Contains(shape));
			RefreshSelection?.Invoke(shape.RefreshBounds(), false, true);
			SelectionChanged?.Invoke();
		}

		public void SelectOnly(Shape shape)
		{
#if DEBUG
			shape?.Container.CheckZ(shape); // this is not particularly relevant to this function; it's just a random place to make this check
#endif
			if (SelectedShapes.Count == 1 && SelectedShapes[0] == shape)
				return; // it and only it is already selected
			Debug.Assert(!m_MovingSelection, "Changing selection while moving shapes - is this correct?  Can cause problems with grab moves if selected shapes are not the ones in the original shapes of the grab move");
			RectangleF invalid = RectangleF.Empty;
			bool already = false; // true if the requested shape is already selected (in which case it is not added to the invalid area at the end)
			bool prompts = shape != null && (shape.Flags & Shape.GeneralFlags.PromptExistingAsNew) > 0 ||
							  SelectedShapes.Count == 1 && (SelectedShapes[0].Flags & Shape.GeneralFlags.PromptExistingAsNew) > 0;
			bool removed = false;
			foreach (Shape selected in SelectedShapes)
			{
				if (selected != shape)
				{
					Geometry.Extend(ref invalid, selected.RefreshBounds());
					selected.CaretLose();
					selected.NotifySelectedChanged(false);
					if (selected.Degenerate)
					{
						m_Shapes.Remove(selected); // technically there is a flaw here since this isn't transacted
						removed = true;
					}
				}
				else
					already = true;
			}
			SelectedShapes.Clear();
			if (shape != null)
			{
				SelectedShapes.Add(shape);
				if (!already)
				{
					Geometry.Extend(ref invalid, shape.RefreshBounds());
					shape.NotifySelectedChanged(true);
				}
			}
			if (removed)
				ResetZ();
			if (!invalid.IsEmpty)
			{
				// because the lines are wider when selected we need a bit more...
				invalid.Inflate(GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2, GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2);
			}
			RefreshSelection?.Invoke(invalid, false, true);
			SelectionChanged?.Invoke();
			if (prompts)
				ShapeNotifiedIndirectChange?.Invoke(null, ChangeAffects.UpdatePrompts, RectangleF.Empty);
		}

		public void SelectOnly(List<Shape> selected)
		{
			// first check if the list is actually changing; we do quite a lot of refreshing when the selection changes
			// so it is worth filtering out a lot of null updates
			if (selected.Count == SelectedShapes.Count)
			{
				if (selected.All(x => SelectedShapes.Contains(x)))
					return;
			}
			bool prompts = selected.Count == 1 && (selected[0].Flags & Shape.GeneralFlags.PromptExistingAsNew) > 0 ||
							  SelectedShapes.Count == 1 && (SelectedShapes[0].Flags & Shape.GeneralFlags.PromptExistingAsNew) > 0;
			Debug.Assert(!m_MovingSelection, "Changing selection while moving shapes - is this correct?  Can cause problems with grab moves if selected shapes are not the ones in the original shapes of the grab move");
			RectangleF invalid = RectangleF.Empty;
			// check the invalid area by looking for items which appear in either the current list or the new list
			bool removed = false;
			foreach (Shape shape in SelectedShapes.Where(x => !selected.Contains(x)))
			{
				Geometry.Extend(ref invalid, shape.RefreshBounds());
				shape.CaretLose();
				shape.NotifySelectedChanged(false);
				if (shape.Degenerate)
				{
					m_Shapes.Remove(shape);
					removed = true;
				}
			}
			foreach (Shape newShape in selected.Where(x => !SelectedShapes.Contains(x)))
			{
				Geometry.Extend(ref invalid, newShape.RefreshBounds());
				newShape.NotifySelectedChanged(true);
			}
			SelectedShapes.Clear();
			SelectedShapes.AddRange(selected);
			SelectedShapes.Sort(new ZSort());
			if (removed)
				ResetZ();
			if (!invalid.IsEmpty)
			{
				// because the lines are wider when selected we need a bit more...
				invalid.Inflate(GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2, GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2);
			}
			RefreshSelection?.Invoke(invalid, false, true);
			SelectionChanged?.Invoke();
			if (prompts)
				ShapeNotifiedIndirectChange?.Invoke(null, ChangeAffects.UpdatePrompts, RectangleF.Empty);
		}

		public void DeselectAll()
		{
			if (SelectedShapes.Count == 0)
				return;
			SelectOnly((Shape)null);
		}

		public void SelectAll()
		{
			List<Shape> list = (from shp in new ComplexEnumerator(false, false, m_Shapes) select shp).ToList();
			if (list.Count > 0)
				SelectOnly(list);
		}

		/// <summary>Returns area that must be repainted to update selection, which is greater than data bounds, or RectangleF.Empty if none selected</summary>
		internal RectangleF SelectedRefreshBoundary()
		{
			if (SelectedShapes == null)
			{
				Utilities.LogSubError("Page.SelectedRefreshBoundary: disposed");
				return RectangleF.Empty;
			}
			if (SelectedShapes.Count == 0)
				return RectangleF.Empty;
			RectangleF bounds = SelectedShapes[0].RefreshBounds(m_MovingSelection);
			for (int index = 1; index <= SelectedShapes.Count - 1; index++)
			{
				Geometry.Extend(ref bounds, SelectedShapes[index].RefreshBounds(m_MovingSelection));
			}
			// because the lines are wider when selected we need a bit more...
			bounds.Inflate(GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2, GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2);
			// we also need to allow for any GrabSpots
			return bounds;
		}

		/// <summary>returns true if it is selected.  Returns false without error if param is null</summary>
		public bool IsShapeSelected(Shape shape)
		{
			return shape != null && SelectedShapes.Contains(shape);
		}

		internal List<Shape> SelectedShapes { get; private set; } = new List<Shape>();

		internal RectangleF SelectedBounds(bool ignore)
		{
			return Geometry.UnionRectangles(from shape in SelectedShapes select shape.Bounds);
		}

		public bool MovingSelection
		{
			get { return m_MovingSelection; }
			set
			{
				if (value == m_MovingSelection)
					return;
				m_MovingSelection = value;
				RefreshSelection?.Invoke(SelectedRefreshBoundary(), true, true);
			}
		}

		/// <summary>returns true if one or more shapes in the selection list allows all of the flags in eRequired</summary>
		/// <remarks>always returns false if nothing selected</remarks>
		public bool SelectionAnyAllows(Shape.AllowedActions required)
		{
			return SelectedShapes.Any(shape => (shape.Allows & required) == required);
		}

		/// <summary>returns true if all shapes in the selection list allows all of the flags in eRequired</summary>
		/// <remarks>always returns false if nothing selected</remarks>
		public bool SelectionAllAllow(Shape.AllowedActions required)
		{
			return SelectedShapes.All(shape => (shape.Allows & required) == required);
		}

		public void FilterSelectionByAllows(Shape.AllowedActions required)
		{
			// remove any shapes which do not support the specified actions from the selection list
			List<Shape> remove = (from shape in SelectedShapes where (shape.Allows & required) < required select shape).ToList();
			// shapes to remove from selection; in some ways it would be easier to build a new selection list
			// but that wouldn't help detect the refresh boundary etc
			if (remove.Count == 0)
				return;
			RectangleF refresh = RectangleF.Empty;
			foreach (Shape shape in remove)
			{
				SelectedShapes.Remove(shape);
				Geometry.Extend(ref refresh, shape.RefreshBounds());
			}
			refresh.Inflate(GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2, GUIUtilities.HIGHLIGHTEXTRAWIDTH / 2);
			RefreshSelection?.Invoke(refresh, false, false);
			SelectionChanged?.Invoke();
		}

		/// <summary>Returns next shape when doing shift-F9 / ctrl-F9.  Complicated by the presence of containers; the next shape may not be in the same container</summary>
		public Shape NextSelection(int direction)
		{
			Debug.Assert(direction == 1 || direction == -1);
			if (m_Shapes.Count == 0)
				return null;
			Shape last = null;
			bool foundSelected = false; // will be true once we have passed the last selected shape
			if (SelectedShapes.Count > 0)
				last = direction > 0 ? SelectedShapes.Last() : SelectedShapes[0]; // last selected shape in the direction in which we are traversing
			else
				foundSelected = true; // first shape found by the iterator is our one
			foreach (Shape shape in new ComplexEnumerator(direction > 0, false, m_Shapes, m_Shapes))
			{
				// By lifting m_Shapes twice it means that we will loop through a second time (which can be needed if the currently selected item is near the end of the list)
				if (shape == last)
					foundSelected = true;
				else if (foundSelected)
					return shape;
			}
			return null; // no unprotected shapes found
		}

		/// <summary>Returns the container containing all of the selected shapes, or Nothing if they don't match</summary>
		/// <returns>The IShapeContainer which is the Parent of everything in SelectedShapes</returns>
		public IShapeContainer SelectionContainer()
		{
			if (SelectedShapes.Count == 0)
				return null;
			IShapeContainer container = (IShapeContainer)SelectedShapes[0].Parent;
			for (int index = 1; index <= SelectedShapes.Count - 1; index++)
			{
				if (SelectedShapes[index].Parent != container)
					return null;
			}
			return container;
		}

		#endregion

		#region Editing - changes to the actual data
		/// <summary>Sends all of the selected shapes to the front or back</summary>
		/// <param name="front">Direction to move</param>
		/// <param name="transaction">Transaction object to use.  This will call Edit as necessary</param>
		/// <remarks>Ignores measures.  Ignored if nothing else if selected.  Ignored, with user message if selection are in different containers.</remarks>
		public void SendToFrontBack(bool front, Transaction transaction)
		{
			// extract them from the main list, and then put them back in again
			if (SelectedShapes.Count == 0)
				return;
			IShapeContainer container = SelectionContainer();
			if (container == null)
			{
				MessageBox.Show(Strings.Item("Container_Mismatch"));
				return;
			}
			transaction.Edit((Datum)container);
			foreach (Shape shape in SelectedShapes)
				container.Contents.Remove(shape);
			if (front)
				container.Contents.AddRange(SelectedShapes);
			else
				container.Contents.InsertRange(0, SelectedShapes);
			container.FinishedModifyingContents(transaction, null);
			RefreshSelection?.Invoke(SelectedRefreshBoundary(), true, false);
		}

		public void SendToFrontBackOneStep(bool front, Transaction transaction)
		{
			// sends a single shape forwards or backwards one step
			if (SelectedShapes.Count != 1)
			{
				Utilities.LogSubError("SendToFrontBackOneStep: " + SelectedShapes.Count + " selected");
				return;
			}
			transaction.Edit(SelectedShapes[0].Container as Datum);
			int index = SelectedShapes[0].Z;
			List<Shape> col = SelectedShapes[0].Container.Contents;
			if (front)
			{
				if (index >= col.Count - 1)
					return;
				col.RemoveAt(index);
				col.Insert(index + 1, SelectedShapes[0]);
				SelectedShapes[0].Z += 1;
				col[index].Z -= 1;
			}
			else
			{
				if (index == 0)
					return;
				col.RemoveAt(index);
				col.Insert(index - 1, SelectedShapes[0]);
				SelectedShapes[0].Z -= 1;
				col[index].Z += 1;
			}
			SelectedShapes[0].Container.FinishedModifyingContents(transaction);
			// ResetZ would not be needed on this if the commented out lines were not included
			// however complex containers might want to move the shapes depending on the Z-order
			RefreshSelection?.Invoke(SelectedRefreshBoundary(), true, false);
		}

		public void AddNew(Shape newShape, Transaction transaction)
		{
			// adds a shape that the user has just finished drawing
			// objTransaction can be nothing if this is not a transactional change (e.g. loading old file version)
			// the object itself to be added to the transaction by the calling code
			if (transaction != null)
			{
				transaction.Edit(this);
				Debug.Assert(transaction.Contains(newShape), "Object to be added to the page is not in the transaction.  Has it been forgotten?");
			}
			newShape.Parent = this;
			{
				if ((newShape.Flags & Shape.GeneralFlags.AddAtBack) > 0)
				{
					newShape.Z = 0;
					m_Shapes.Insert(0, newShape);
					ResetZ();
				}
				else
				{
					newShape.Z = m_Shapes.Count;
					m_Shapes.Add(newShape);
				}
				CheckIntersectionsOneShape(newShape);
			}
			RequestRefresh?.Invoke(newShape.RefreshBounds());
		}

		public bool IsEmpty => m_Shapes.Count == 0;

		/// <summary>Lets all shapes make any changes required due to changes to shapes that they link to.  Will automatically be sent within containers but groups must pass on</summary>
		/// <remarks>
		/// The status value on the already changed shapes shows which ones might trigger changes
		/// NOTE: this requires that shapes which we link to do not themselves have further links; this function will not correctly cascade
		/// such cascading changes will only work at the moment if the shapes are stored in the correct order
		/// </remarks>
		public void PerformLinkedChanges(Transaction transaction, frmMain editor)
		{
			foreach (Shape shape in this)
			{
				shape.PerformLinkedChanges(transaction, editor);
			}
		}

		public void NotifySettingsChanged(Shape.EnvironmentChanges change)
		{
			// should be passed on down through the hierarchy of shapes
			foreach (Shape shape in m_Shapes)
			{
				shape.NotifyEnvironmentChanged(change);
			}
		}

#if DEBUG
		public bool ContainsChangedShapes()
		{
			return m_Shapes.Any(shape => shape.Status != Shape.StatusValues.Complete);
		}
#endif

		public void Delete(Shape shape, Transaction transaction)
		{
			// objTransaction can be nothing if this is not a transactional change (e.g. loading old file version)
			Debug.Assert(!shape.HasCaret);
			transaction?.Edit((Datum)shape.Container);
			shape.DeleteIntersections();
			shape.Container.Contents.Remove(shape);
			transaction?.Delete(shape);
			shape.Container.FinishedModifyingContents(transaction);
			if (SelectedShapes.Contains(shape))
			{
				RefreshSelection?.Invoke(shape.RefreshBounds(), true, true);
				SelectionChanged?.Invoke();
				SelectedShapes.Remove(shape);
			}
		}

		public void DeleteSelected(Transaction transaction)
		{
			if (SelectedShapes.Count == 0)
				return;
			RectangleF invalid = SelectedRefreshBoundary();
			HashSet<IShapeContainer> hashContainers = new HashSet<IShapeContainer>(); // all containers which are affected
			foreach (Shape shape in SelectedShapes)
			{
				if (!(shape.Container is Shape) || !SelectedShapes.Contains((Shape)shape.Container)) // only delete from container if it isn't being deleted (.Edit will assert if container is already deleted)
				{
					transaction.Edit((Datum)shape.Container);
					shape.DeleteIntersections();
					shape.Container.Contents.Remove(shape);
					if ((shape.Parent as Shape)?.Parent is Scriptable) // select scriptable if possible rather than any item within it
						hashContainers.Add(((Shape)shape.Parent).Container);
					else
						hashContainers.Add(shape.Container);
				}
				transaction.Delete(shape);
			}
			SelectedShapes.Clear();
			if (hashContainers.OfType<Shape>().Count() == 1) // if all shapes were within a container select it instead - intended for deleting arc on NaL - could be restricted to this case only
				SelectOnly(hashContainers.OfType<Shape>().First());
			// we don't change the Parent property in the shape, because undo would not restore this after a delete (deletes do not keep backup buffers)
			foreach (IShapeContainer container in hashContainers)
			{
				container.FinishedModifyingContents(transaction);
			}
			RefreshSelection?.Invoke(invalid, true, true);
			SelectionChanged?.Invoke();
		}

		#endregion

		#region Datum -load, save, etc
		public override void Load(DataReader reader)
		{
			base.Load(reader);
			PhysicalSize = reader.ReadSizeF();
			Margin = reader.ReadSingle();
			m_Size = new SizeF(PhysicalSize.Width - Margin * 2, PhysicalSize.Height - Margin * 2);
			Colour = reader.ReadColour();
			Paper = (Paper)reader.ReadData(); // assigning the public property will create the event handler ' cant say expected paper or Iso will fail
			reader.ReadInt32(); // flags
			m_Shapes = reader.ReadShapeList();
			foreach (Shape shape in m_Shapes)
			{
				shape.Parent = this;
			}
			CheckAllIntersections();
			foreach (Shape shape in m_Shapes)
			{
				shape.Status = Shape.StatusValues.Complete;
			}
			ResetZ();
			if (reader.Version >= 60)
				Origin = reader.ReadPointF();
			if (reader.Version >= 78)
			{
				m_Background = SharedReference<SharedImage>.FromGUID(reader.ReadGuid());
				BackgroundImageMode = (BackgroundImageModes)reader.ReadByte();
			}
			reader.ReadString();
			if (reader.Version >= 99)
				reader.ReadOptionalPNG();
			if (reader.Version >= 122)
				HelpSAWID = reader.ReadInt32();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(PhysicalSize);
			writer.Write(Margin);
			writer.Write(Colour);
			writer.Write(Paper);
			writer.Write(0); // flags
			writer.WriteDatumList(m_Shapes);
			if (writer.Version >= 60)
				writer.Write(Origin);
			if (writer.Version >= 78)
			{
				writer.Write(m_Background?.ID ?? Guid.Empty);
				writer.Write((byte)BackgroundImageMode);
			}
			writer.Write("");
			if (writer.Version >= 99)
				writer.WriteOptionalPNG(null);
			if (writer.Version >= 122)
				writer.Write(HelpSAWID);
		}

		public override byte TypeByte
		{ get { return (byte)FileMarkers.Page; } }

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			// selection information is not copied
			base.CopyFrom(other, depth, mapID);
			Page page = (Page)other;
			m_Size = page.Size;
			PhysicalSize = page.PhysicalSize;
			Margin = page.Margin;
			Colour = page.Colour;
			Origin = page.Origin;
			BackgroundImageMode = page.BackgroundImageMode;
			m_Background = page.m_Background;
			HelpSAWID = page.HelpSAWID;
			m_Shapes = new List<Shape>();
			if (depth >= CopyDepth.Duplicate)
			{
				Paper = (Paper)page.Paper.Clone(mapID);
				foreach (var shape in page.m_Shapes)
				{
					Shape clone = (Shape)shape.Clone(mapID);
					clone.Parent = this;
					m_Shapes.Add(clone);
				}
				ResetZ();
			}
			else
			{
				Paper = page.Paper;
				m_Shapes.AddRange(page.m_Shapes);
			}
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			Paper.UpdateReferencesObjectsCreated(document, reader);
			foreach (Shape shape in m_Shapes)
			{
				shape.UpdateReferencesObjectsCreated(document, reader);
			}
			m_Background?.DereferenceOnLoad(document);
			Debug.Assert(!(m_Background?.Failed ?? false));
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			Paper.UpdateReferencesIDsChanged(mapID, document);
			foreach (Shape shape in m_Shapes)
			{
				shape.UpdateReferencesIDsChanged(mapID, document);
			}
			m_Background?.UpdateIDsReferencesChanged();
		}

		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			fnAdd.Invoke(m_Background?.Content);
		}

		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			if (Paper == null || m_Shapes == null)
				return;
			Paper.Iterate(fn);
			foreach (Shape shape in m_Shapes)
			{
				shape.Iterate(fn);
			}
		}

		public override bool IdenticalTo(Datum other)
		{
			Page page = (Page)other;
			if (!IdenticalForPaletteDelta) // palette size can change as they are moved around the screen, so we need to ignore this
			{
				if (m_Size != page.m_Size)
					return false;
				if (PhysicalSize != page.PhysicalSize)
					return false;
			}
			if (Margin != page.Margin)
				return false;
			if (!Colour.Equals(page.Colour))
				return false;
			if (Origin != page.Origin)
				return false;
			if (!SharedReference<SharedImage>.ReferencesAreEqual(m_Background, page.m_Background))
				return false;
			if (!Paper.IdenticalTo(page.Paper))
				return false;
			if (m_Shapes.Count != page.m_Shapes.Count)
				return false;
			for (int index = 0; index <= m_Shapes.Count - 1; index++)
			{
				if (!m_Shapes[index].IdenticalTo(page.m_Shapes[index]))
					return false;
			}
			if (HelpSAWID != page.HelpSAWID)
				return false;
			// doesn't check the image layer.  unlikely to be needed atm where there is one
			return true;
		}

		public void WriteExportText(IndentStringBuilder output)
		{ // in this caseThe start and end of page written by the container, if needed
			if (m_Background != null)
			{
				output.AppendLine("Has background image");
				output.Append("Background render mode = ").AppendLine(BackgroundImageMode.ToString());
			}
			foreach (Shape shape in m_Shapes)
				shape.WriteExportText(output);
		}


		#endregion

		#region Targets (shape snapping)
		private List<Target> m_Targets = new List<Target>(); // GenerateTargets will only generate a single target; but when snapping a moving shape we might want to draw several intersections

		public void ClearTargets()
		{
			m_Targets.Clear();
		}

		/// <summary>all targets are generated from a (probably dummy) UserSocket.  For mouse UserSocket.Centre is the mouse position,
		/// and other values are default.  But this does allow us to specify angular or class/gender constraints (mostly used by Group when generating targets)</summary>
		/// <remarks>will only actually generate a single target - the one closest to the mouse</remarks>
		public void GenerateTargets(UserSocket moving, float scale)
		{
			m_Targets.Clear();
			m_Targets = GenerateIndependentTargets(moving, scale);
			if (m_Targets.Count == 0)
				return;
			// we now keep either just the first one, if it is close enough to activate; if not the list can be cleared
			if (m_Targets[0].AdjustedDistance < Target.ActivationThreshold)
			{
				// it is active
				if (m_Targets.Count > 1)
					m_Targets.RemoveRange(1, m_Targets.Count - 1);
			}
			else
				m_Targets.Clear();
		}

		/// <summary>Does not update the targets stored on the page; instead simply returns a list of targets near the area of interest</summary>
		public List<Target> GenerateIndependentTargets(UserSocket moving, float scale)
		{
			List<Target> targets = new List<Target>();
			try
			{
				// need to make larger if working in pixels...
				//scale = scale * (GUIUtilities.DpiX / Geometry.INCH) / Globals.Root.CurrentDocument.ApproxUnitScale;
				scale = scale * GUIUtilities.MillimetreSize;
				float maximumDistance = Target.MaximumApplicableDistance(scale); // we don't need to bother checking some of the objects which are a long way from the mouse
				foreach (Shape shape in this) //  does shapes and measures and within containers
				{
					if (!m_MovingSelection || !SelectedShapes.Contains(shape)) // don't allow a shape which is being moved to snap back onto itself!
					{
						if (Geometry.DirtyDistance(moving.Centre, shape.Bounds) <= maximumDistance)
						{
							List<Target> list = shape.GenerateTargets(moving);
							if (list != null)
								targets.AddRange(list);
						}
					}
				}
				// and finally check if we are near the edge of the page
				if (moving.Centre.X < maximumDistance)
					targets.Add(new Target(null, new PointF(-Geometry.NEGLIGIBLE, moving.Centre.Y), Target.Types.PageEdgeV, moving, Target.Priorities.Low));
				if (moving.Centre.X > Size.Width - maximumDistance)
					targets.Add(new Target(null, new PointF(Size.Width + Geometry.NEGLIGIBLE, moving.Centre.Y), Target.Types.PageEdgeV, moving, Target.Priorities.Low));
				if (moving.Centre.Y > -maximumDistance)
					targets.Add(new Target(null, new PointF(moving.Centre.X, Geometry.NEGLIGIBLE), Target.Types.PageEdgeH, moving, Target.Priorities.Low));
				if (moving.Centre.Y < -Size.Height + maximumDistance) // because all Y coordinates are negative

					targets.Add(new Target(null, new PointF(moving.Centre.X, -Size.Height - Geometry.NEGLIGIBLE), Target.Types.PageEdgeH, moving, Target.Priorities.Low));
				// and the origin (if moved)
				if (!Origin.IsEmpty && Geometry.DirtyDistance(Origin, moving.Centre) < maximumDistance)
					targets.Add(new Target(null, Origin, Target.Types.Vertex, moving, Target.Priorities.Intersection)); // higher priority than usual vertex
				targets.Sort(); // sorts them into ascending distance order
			}
			catch (Exception ex)
			{
				targets.Clear();
				Utilities.LogSubError(ex);
			}
			return targets;
		}

		public void SetDrawnTargets(List<Target> targets)
		{
			m_Targets = targets;
		}

		public void DrawTargets(Graphics gr, int activePhase, float scale)
		{
			// intActivePhase ranges from 0 to Target.ACTIVEMAXIMUMPHASE
			Debug.Assert(activePhase >= 0 && activePhase <= Target.ACTIVEMAXIMUMPHASE);
			if (!HasActiveTarget)
				return;
			foreach (Target target in m_Targets)
			{
				target.Draw(gr, activePhase, scale);
			}
		}

		public bool HasTargets
		{ get { return m_Targets.Count > 0; } }

		public bool HasActiveTarget
		{ get { return m_Targets.Count > 0; } }

		public Target ActiveTarget
		{
			get
			{
				// will always be the first one because they are sorted into order
				if (m_Targets.Count == 0)
					return null; // not really needed at runtime, but without this the debugger F9 has problems
				return m_Targets[0];
			}
		}

		public RectangleF TargetRefreshBoundary(float scale)
		{
			// area which needs to be refreshed for current targets.  These are drawn in 'current' buffer of view
			if (!HasActiveTarget)
				return RectangleF.Empty;
			return Geometry.UnionRectangles(from t in m_Targets select t.RefreshBounds(scale));
		}
		#endregion

		#region Sockets
		public List<Shape.Socket> Sockets = new List<Shape.Socket>(); // currently active sockets when moving mouse. object should always exist, but is usually empty
		private const float SOCKETDRAWRADIUS = 1.2f;

		/// <summary>returns list of sockets near given point - based on first shape this is over which has sockets
		/// ie always returns just sockets for one shape</summary>
		public List<Shape.Socket> FindSockets(PointF source, float scale)
		{
			float expand = Line.LINEHITTOLERANCE / scale / 2;
			foreach (Shape shape in new ComplexEnumerator(true, true, m_Shapes))
			{
				RectangleF bounds = shape.Bounds;
				bounds.Inflate(expand, expand);
				if (bounds.Contains(source))
				{
					if (shape.HitTestDetailed(source, scale, true))
					{
						List<Shape.Socket> sockets = shape.GetSockets();
						if (sockets != null && sockets.Count > 0)
							return sockets;
					}
				}
			}
			return new List<Shape.Socket>();
		}

		public RectangleF SocketRefreshBoundary()
		{
			//if (Sockets.Count == 0)
			//	return RectangleF.Empty;
			return Geometry.UnionRectangles(from socket in Sockets select Geometry.RectangleFromPoint(socket.Position, SOCKETDRAWRADIUS + 1));
			//RectangleF bounds = RectangleF.Empty;
			//foreach (Shape.Socket socket in Sockets)
			//{
			//	Geometry.Extend(ref bounds, Geometry.RectangleFromPoint(socket.Position, SOCKETDRAWRADIUS + 1));
			//}
			//return bounds;
		}

		public Shape.Socket NearestSocket(PointF mouse)
		{
			float nearestDistance = float.MaxValue;
			Shape.Socket nearest = Shape.Socket.Empty; // will be returned if no sockets active
			foreach (Shape.Socket objSocket in Sockets)
			{
				float distance = Geometry.DistanceBetween(mouse, objSocket.Position);
				if (distance < nearestDistance)
				{
					nearestDistance = distance;
					nearest = objSocket;
				}
			}
			return nearest;
		}

		public void DrawSockets(Graphics gr, float scale)
		{
			if (Sockets.Count == 0)
				return;
			float dimension = (float)(SOCKETDRAWRADIUS / Math.Sqrt(scale)); // we compromise on scaling
			Color col = Color.FromArgb(180, Target.TARGETCOLOUR);
			using (Pen pn = new Pen(col, 0.7F / scale))
			{
				foreach (Shape.Socket socket in Sockets)
				{
					gr.DrawEllipse(pn, socket.Position.X - dimension, socket.Position.Y - dimension, dimension * 2, dimension * 2);
				}
			}

		}

		#endregion

		#region Intersections
		private void CheckIntersectionsOneShape(Shape shape)
		{
			if ((shape.Flags & Shape.GeneralFlags.NoIntersections) > 0)
				return;
			foreach (Shape test in new ComplexEnumerator(false, false, m_Shapes)) // enumerate shapes in containers, no measures
			{
				if (test != shape && (test.Flags & Shape.GeneralFlags.NoIntersections) == 0)
				{
					if (shape.Bounds.Intersects(test.Bounds))
						test.CheckIntersectionsWith(shape);
				}
			}
			shape.CheckIntersectionsWithSelf();
		}

		private void DeleteAllIntersections()
		{
			foreach (Shape shape in new ComplexEnumerator(false, false, m_Shapes))
			{
				shape.DeleteIntersections();
			}
		}

		private void CheckAllIntersections()
		{
			// checks for intersections ab initio (e.g. after loading page).  Slow.
			// must first make a flat list of shapes - in order to iterate twice over
			List<Shape> all = Shape.FlattenList(m_Shapes, Shape.FlatListPurpose.ExposedShapes);
			for (int index = 1; index <= all.Count - 1; index++)
			{
				Shape shape = all[index];
				if ((shape.Flags & Shape.GeneralFlags.NoIntersections) > 0)
					continue;
				for (int otherIndex = 0; otherIndex <= index - 1; otherIndex++)
				{
					Shape otherShape = all[otherIndex];
					if ((otherShape.Flags & Shape.GeneralFlags.NoIntersections) == 0 && shape.Bounds.IntersectsApprox(otherShape.Bounds))
						shape.CheckIntersectionsWith(otherShape);
				}
				shape.CheckIntersectionsWithSelf();
			}
			if (m_Shapes.Count > 0)
				m_Shapes[0].CheckIntersectionsWithSelf(); // was excluded by loop above
		}

		public void UpdateIntersectionsWith(Shape shape, bool calledExplicitly = false)
		{
			// called eg if the shape is moved.  All intersections in shape removed and it is rechecked
			shape.DeleteIntersections();
			CheckIntersectionsOneShape(shape);
		}

		public void UpdateAllIntersections()
		{
			// inefficient, but occasionally necessary.  E.g. on undo/redo when shapes could have been added or removed
			DeleteAllIntersections();
			CheckAllIntersections();
		}

		#endregion

		#region IDisposable Support
		// cannot rely on this being called; this does not use the standard disposable pattern
		// this is ONLY used in the entire document is disposed (and one or two cases where dummy pages are created to do interim calculations)
		public void Dispose()
		{
			if (m_Shapes == null)
				return;
			Globals.StoreEvent("Page.Dispose");
			foreach (Shape shape in m_Shapes)
			{
				shape.Dispose();
			}
			// none of the measures need disposing
			m_Shapes = null;
			SelectedShapes = null;
		}

		public bool IsDisposed
		{
			[DebuggerStepThrough()]
			get
			{
				return m_Shapes == null;
			}
		}

		#endregion

		#region Shape parent
		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			Debug.Assert(affected != ChangeAffects.SetPaletteExclusion, "SetPaletteExclusion requires a rectangle to be specified, even if it is empty (to clear)");
			NotifyIndirectChange(shape, affected, RectangleF.Empty);
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			if (IsDisposed)
				return;
			if ((affected & ChangeAffects.Intersections) > 0)
			{
				UpdateIntersectionsWith(shape);
				affected -= ChangeAffects.Intersections;
			}
			if (affected > 0)
				// some changes that it cannot handled within the page
				ShapeNotifiedIndirectChange?.Invoke(shape, affected, area);
		}

		#endregion


	}

}
