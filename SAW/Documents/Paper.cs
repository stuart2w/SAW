using System;
using System.Drawing;
using System.Diagnostics;

namespace SAW
{
	/// <summary>this is basically the grid - see also Page
	/// there is pretty much one-to-one relationship between a page object and a paper object - but it is still probably worth keeping the paper separate
	/// because it allows subclassing more easily</summary>
	public class Paper : Datum
	{

		#region Types and creation
		public enum Papers : byte
		{
			Plain,
			Ruled,
			Square,
			//Isometric,
			Graph=4,
			//IsometricH
		}

		public Papers PaperType { get; private set; }

		internal static Paper CreateFromTypeCode(FileMarkers type)
		{
			// this is only used when loading, or possibly undoing so no values need to be set sensibly
			switch (type)
			{
				case FileMarkers.GraphPaper:
					return new GraphPaper();
				case FileMarkers.Paper:
					return new Paper(Papers.Plain);
				default:
					throw new ArgumentException("Paper.CreateFromTypeCode, unexpected type code: " + type);
			}
		}

		public static Paper CreateNew(Papers type, Paper prototype)
		{
			// second parameter is existing paper from which we can copy values
			Paper create;
			switch (type)
			{
				case Papers.Graph:
					create = new GraphPaper();
					break;
				default:
					create = new Paper(type);
					break;
			}
			if (prototype != null)
			{
				create.CopyFrom(prototype, CopyDepth.Duplicate, Mapping.Ignore);
				create.PaperType = type;
			}
			create.CheckProperties();
			if (type != Papers.Graph)
			{
				// make the intervals a multiple of 5 (because the editing screen uses a numeric which steps in 5, except when using graph paper)
				// (and if the user switched from graph to something else they could be stuck with an odd number)
				// I haven't made this part of CheckProperties, because I don't want this to be applied to strictly
				// there is no technical reason why paper cannot have other intervals
				if (create.X > 0)
					create.X = Math.Max((float)(Math.Floor(create.X / 5) * 5), 5);
				if (create.X > 0)
					create.Y = Math.Max((float)(Math.Floor(create.Y / 5) * 5), 5);
			}

			return create;
		}

		protected Paper(Papers type)
		{
			PaperType = type;
		}

		protected virtual void CheckProperties()
		{
			// called by the constructor.  The values will have been filled in by copying the prototype paper - check that they are appropriate for this type of paper
			// will also be called if some public properties are changed.  So this should just check the properties
			switch (PaperType)
			{
				case Papers.Plain: // keeping the old values allows them to remain as the user changes back and forth between paper types
					break;
				case Papers.Ruled:
					X = 0;
					if (Y < 1)
						Y = DefaultSpacing;
					m_Dotted = false;
					break;
				case Papers.Square:
					if (Y < 1)
						Y = DefaultSpacing;
					if (X < 1)
						X = Y; // not default, if we change from ruled to square it uses that dimension in both directions
					break;
				default:
					Debug.Fail("Unexpected type in Paper.Initialise");
					break;
			}
			if (GridColour.IsEmpty)
				GridColour = DEFAULTGRIDCOLOUR;
			if (m_Dotted && GridColour.A < 255)
				GridColour = Color.FromArgb(255, GridColour);
		}

		#endregion

		#region Values
		public static float DefaultSpacing = 50; // this is updated by the editor as changes are made
												 // so that if we change to Plain paper and back again the original values will be restored
		protected static Color DEFAULTGRIDCOLOUR = Color.FromArgb(150, 100, 100, 100); // default doc colour is white

		public Color GridColour; // colour of the grid
								 //Public MinorColour As Color ' only needed for graph paper ' not edited yet either
		protected float X = 0; // space between snap points in X direction.  For graph paper this is the small step
		protected float Y = 0; // X, Y are 0 if not snapping in this direction
							   // except on Plain paper where the Y value can still be defined (so that as we change back to a different paper the old settings are kept)
		protected bool m_Dotted;

		public bool PrintBackground = true;

		public bool Dotted
		{
			get { return m_Dotted; }
			set
			{
				if (m_Dotted == value)
					return;
				m_Dotted = value;
				CheckProperties();
			}
		}

		public void SetIntervals(float x, float y = -1, int graphMultiple = -1)
		{
			X = x;
			if (y >= 0)
				Y = y;
			if (graphMultiple > 0)
				GraphMultiple = graphMultiple;
			CheckProperties();
		}

		public float XInterval
		{ get { return PaperType == Papers.Plain ? 0 : X; } }

		public float YInterval
		{ get { return PaperType == Papers.Plain ? 0 : Y; } }

		public int GraphMultiple { get; protected set; } = 1;

		public bool GridVisible { get; set; } = true;

		#endregion

		#region Snapping
		public virtual PointF SnapPoint2(PointF pt)
		{
			if (X == 0 && Y == 0 || PaperType == Papers.Plain)
				return pt;
			float ptX = pt.X;
			float ptY = -pt.Y; // making it +ve
			if (X > 0)
				ptX = Convert.ToInt32(ptX / X) * X; // NOTE cannot cast to (int), as that rounds differently
			if (Y > 0)
				ptY = Convert.ToInt32(ptY / Y) * Y;
			return new PointF(ptX, -ptY);
		}

		/// <summary>Returns to distance to use to snap a scalar length</summary>
		/// <param name="indicativeAngle">the parameter should be the direction in which we are working - this is not generally needed, but can be used
		/// to decide whether to use the X or Y units if both are used.  If the caller does not care, just use 0</param>
		/// <param name="strictAngle">if bolStrictAngle then this will return 0 if it does not snap in the direction given, but does in a different direction (only ruled paper at the moment)</param>
		public virtual float ScalarSnapStep(float indicativeAngle, bool strictAngle = false)
		{
			float step = X;
			switch (PaperType)
			{
				case Papers.Ruled:
					if (strictAngle && indicativeAngle != 0)
						return 0;
					step = Y; // there is no X increment
					break;
				case Papers.Plain:
					step = 0;
					break;
				case Papers.Square: // potentially X and Y different
				case Papers.Graph:
					if (Y > 0) // otherwise just use X if some reason Y is not set
					{
						float x = X * (float)Math.Sin(indicativeAngle);
						float y = Y * (float)Math.Cos(indicativeAngle);
						step = (float)Math.Sqrt(x * x + y * y);
					}
					break;
			}
			return step;
		}

		public float SnapScalar(float scalar, float indicativeAngle = 0)
		{
			float step = ScalarSnapStep(indicativeAngle);
			return Math.Max(step, (float)Math.Floor(scalar / step) * step);
		}

		private const float DEFAULTUNITSTEP = 10; // 1cm
												  /// <summary>Used by some shapes, especially text, to get a roughly 1cm step based on the paper size. ALWAYS returns a value even if non-snapping.  This is not used for snapping, just as a basic size</summary>
		public virtual float UnitStep
		{
			get
			{
				float step = ScalarSnapStep(0);
				if (step <= 0)
					return DEFAULTUNITSTEP;
				while (step < DEFAULTUNITSTEP / 2)
					step *= 2;
				while (step > DEFAULTUNITSTEP * 2)
					step /= 2;
				return step;
			}
		}

		/// <summary>Used when we want to grid snap an angle.  Should return angles relevant to this grid.</summary>
		/// <param name="first90">If bolFirst90 then the caller is going to check every 90° rotation anyway, so there's no point returning anything after the 0° to 89°</param>
		/// <remarks>should return the angles even if snap = false; the caller can check this</remarks>
		public virtual float[] GetRelevantAngles(bool first90)
		{
			if (first90)
				return new[] { 0, Geometry.PI / 4 }; // 45 can be useful
			float[] aAngles = new float[8];
			for (int index = 0; index <= 7; index++)
			{
				aAngles[index] = index * Geometry.PI / 4;
			}
			return aAngles;
		}
		#endregion

		#region Drawing
		// originally tried to draw the dotted grid by drawing the standard line using a pen with a suitable dot/dash interval
		// however there seem to be some errors in .net with dashed pens with scaling and/or different pages, and it was highly unreliable
		protected Stroke m_PenMinor;
		/// <summary>the alternative pen to be used every multiple of GraphInterval</summary>
		protected Stroke m_PenMajor;
		/// <summary>approximate number of pixels to draw dotted grids. </summary>
		public float DotSize = 1.5F;
		public const float STANDARDDOTSIZE = 1.5F;

		public virtual void Draw(Canvas gr, Page page, float zoom)
		{
			// zoom is just needed to sort out a view issues with the dotting patterns
			if (!GridVisible || PaperType == Papers.Plain)
				return;
			PreparePens(gr, zoom);
			try
			{
				DrawInternal(gr, page, zoom);
			}
			finally
			{
				m_PenMinor?.Dispose();
				m_PenMajor?.Dispose();
			}
		}

		protected virtual void PreparePens(Canvas gr, float zoom)
		{
			Debug.Assert(PaperType != Papers.Graph); // should have been overridden.  Isometric can however use this
			if (m_Dotted)
			{
				// we ensure that the pen is one pixel wide
				float pixel = DotSize * (Geometry.INCH / gr.DpiX) / (float)Math.Sqrt(zoom); // size of a pixel in graphics coords
				m_PenMajor = gr.CreateStroke(GridColour.MixWith(Color.Black), pixel); // we make the dots somewhat darker, otherwise they can disappear a bit
			}
			else
				m_PenMajor = gr.CreateStroke(GridColour, Geometry.THINLINE);
			m_PenMinor = null;
			GraphMultiple = 1; // just in case
		}

		protected virtual void DrawInternal(Canvas gr, Page page, float zoom)
		{
			// calcs use positive coords, and invert as drawing
			int index = 0;
			float position = 0;
			//dots are only drawn horizontally
			float pixel= DotSize / (float)Math.Sqrt(zoom); // size of a pixel in graphics coords in mm
			switch (gr.PageUnit)
			{
				case GraphicsUnit.Pixel:
				case GraphicsUnit.Display:
					break;
				case GraphicsUnit.Millimeter:
					pixel *= Geometry.INCH / gr.DpiX;
					break;
				default:
					Debug.Fail("Unexpected page units in Paper.DrawInternal");
					break;
			}

			if (Y > 0)
			{
				// draw lines
				while (position < page.Size.Height)
				{
					Stroke pn = index % GraphMultiple == 0 ? m_PenMajor : m_PenMinor;
					if (m_Dotted && (GraphMultiple == 1 || index % GraphMultiple > 0) && X > 0)
					{
						// draw the dotted line.  X >0 just a safety check; ruled paper should have rejected dotted
						float x = 0;
						while (x < page.Size.Width)
						{
							gr.DrawLine(x, -position, x + pixel, -position, pn);
							x += X;
						}
					}
					else
						gr.DrawLine(0, -position, page.Size.Width, -position, pn);
					position += Y;
					index += 1;
				}
			}

			index = 0;
			position = 0;

			if (m_Dotted && GraphMultiple == 1)
				return;
			if (X > 0)
			{
				while (position < page.Size.Width)
				{
					if (m_Dotted && index % GraphMultiple > 0)
					{
						// if this one would be dotted we can ignore it
					}
					else
					{
						Stroke pn = index % GraphMultiple == 0 ? m_PenMajor : m_PenMinor;
						gr.DrawLine(position, 0, position, -page.Size.Height, pn);
					}
					position += X;
					index += 1;
				}
			}
		}

		#endregion

		#region Datum
		public override void Load(DataReader reader)
		{
			base.Load(reader);
			PaperType = (Papers)reader.ReadByte();
			GridColour = reader.ReadColour();
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			GraphMultiple = reader.ReadInt32();
			m_Dotted = reader.ReadBoolean();
			GridVisible = reader.ReadBoolean();
			 reader.ReadBoolean(); // PrintGrid
			if (reader.Version >= 96)
				DotSize = reader.ReadSingle(); // otherwise will use the default value assigned with the field
			if (reader.Version >= 98)
				PrintBackground = reader.ReadBoolean();
			else
				PrintBackground = true;
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.WriteByte((byte)PaperType);
			writer.Write(GridColour);
			writer.Write(X);
			writer.Write(Y);
			writer.Write(GraphMultiple);
			writer.Write(m_Dotted);
			writer.Write(GridVisible);
			writer.Write(false);
			if (writer.Version >= 96)
				writer.Write(DotSize);
			if (writer.Version >= 98)
				writer.Write(PrintBackground);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Paper objPaper = (Paper)other;
			PaperType = objPaper.PaperType;
			GridColour = objPaper.GridColour;
			X = objPaper.X;
			Y = objPaper.Y;
			GraphMultiple = objPaper.GraphMultiple;
			m_Dotted = objPaper.m_Dotted;
			GridVisible = objPaper.GridVisible;
			DotSize = objPaper.DotSize;
		}

		public override byte TypeByte
		{ get { return (byte)FileMarkers.Paper; } }

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
		}

		public override bool IdenticalTo(Datum other)
		{
			Paper objPaper = (Paper)other;
			if (PaperType != objPaper.PaperType)
				return false;
			if (!GridColour.Equals(objPaper.GridColour))
				return false;
			if (X != objPaper.X)
				return false;
			if (Y != objPaper.Y)
				return false;
			if (GraphMultiple != objPaper.GraphMultiple)
				return false;
			if (PrintBackground != objPaper.PrintBackground)
				return false;
			if (m_Dotted != objPaper.m_Dotted)
				return false;
			if (GridVisible != objPaper.GridVisible)
				return false;
			return true;
		}
		#endregion

	}

	public class GraphPaper : Paper
	{
		// graphics can mostly use the basic graphics - it just needs to create different pens

		public GraphPaper() : base(Papers.Graph)
		{
		}

		protected override void CheckProperties()
		{
			if (Y < 1)
				Y = DefaultSpacing;
			if (X < 1)
				X = Y; // not default, if we change from ruled to square it uses that dimension in both directions
			if (GraphMultiple <= 1)
				GraphMultiple = 5;
			if (GridColour.IsEmpty)
				GridColour = DEFAULTGRIDCOLOUR;
		}

		protected override void PreparePens(Canvas gr, float zoom)
		{
			float pixel = DotSize * Geometry.INCH / gr.DpiX / (float)Math.Sqrt(zoom); // size of a pixel in graphics coords
			if (m_Dotted)
			{
				// we ensure that the pen is one pixel wide
				m_PenMinor = gr.CreateStroke(GridColour, pixel);
				// and the major line is thinner
				m_PenMajor = gr.CreateStroke(GridColour, Geometry.THINLINE);
			}
			else
			{
				// we make the minor line both narrower and fainter; narrower often fails due to rounding issues
				// when the paper is displayed at less than 100% (especially less than 50%) were evenly thick line just becomes one pixel
				m_PenMinor = gr.CreateStroke(Color.FromArgb(100, GridColour), Geometry.THINLINE);
				m_PenMajor = gr.CreateStroke(GridColour, Geometry.THINLINE * 2);
			}
		}

		public override byte TypeByte
		{ get { return (byte)FileMarkers.GraphPaper; } }

	}
}
