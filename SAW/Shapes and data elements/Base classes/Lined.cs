using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Drawing.Drawing2D;

namespace SAW.Shapes
{
	public abstract class Lined : Shape
	{
		// base class for any shape which draws lines (whether straight or curved)
		// main functionality is storing the line style.  We also maintain a list of vertices - most shapes are simply drawn by joining these up,
		// but the vertices do not necessarily need to be exactly these - they could be something else abstract (e.g. centre of circle)
		// This now also has significant path support for shapes which create a graphics path representing the shape

		/// <summary>Styling information for the border line</summary>
		public LineStyleC LineStyle;

		#region Basics

		protected Lined()
		{
			// ReSharper disable once VirtualMemberCallInConstructor // should be safe since this function doesn't rely on objects date, so it won't matter that base classes won't be constructed
			int fix = FixedVerticesLength();
			if (fix <= 0)
				fix = 3;
			Vertices = new List<PointF>(fix);
			LineStyle = new LineStyleC();
			if ((Allows & AllowedActions.Arrowheads) > 0)
			{
				StartArrowhead = new ArrowheadC(false);
				EndArrowhead = new ArrowheadC(true);
			}
		}

		internal override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			switch (parameter)
			{
				case Parameters.LineColour:
				case Parameters.LinePattern:
				case Parameters.LineWidth:
					return LineStyle;
				case Parameters.ArrowheadEndSize:
				case Parameters.ArrowheadEndType:
					return EndArrowhead;
				case Parameters.ArrowheadStartSize:
				case Parameters.ArrowheadStartType:
					return StartArrowhead;
				default:
					return base.StyleObjectForParameter(parameter, applyingDefault);
			}
		}

		protected internal override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			output.AppendLine("m_intDefinedVertices = " + m_DefinedVertices);
			for (int index = 0; index <= Vertices.Count - 1; index++)
			{
				output.AppendLine("Vertices[" + index + "] = (" + Vertices[index].X.ToString(DiagnosticFormat) + ", " + Vertices[index].Y.ToString(DiagnosticFormat) + ")");
			}
			if (m_Path != null)
			{
				output.AppendLine();
				output.AppendLine("Path:");
				PathDiagnostic(output, m_Path);
			}
			if (m_ExteriorPath != null)
			{
				output.AppendLine();
				output.AppendLine("Exterior path:");
				PathDiagnostic(output, m_ExteriorPath);
			}
		}

		public override AllowedActions Allows
		{
			get
			{
				var allows = base.Allows | AllowedActions.Arrowheads;
				// cannot cope with arrowheads in paths
				if ((StartArrowhead == null || StartArrowhead.Style == ArrowheadC.Styles.None) && (EndArrowhead == null || EndArrowhead.Style == ArrowheadC.Styles.None))
					allows = allows | AllowedActions.ConvertToPath;
				return allows;
			}
		}

		internal override void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			base.NotifyStyleChanged(parameter, oldValue, newValue);
			if (StartArrowhead != null && StartArrowhead.Style != ArrowheadC.Styles.None || EndArrowhead != null && EndArrowhead.Style != ArrowheadC.Styles.None)
			{
				if ((Flags & GeneralFlags.ProtectBounds) == 0)
					m_Bounds = RectangleF.Empty;
			}
			if (parameter == Parameters.LineWidth)
			{
				DecacheArrowheads();
				if (oldValue >= WIDETHRESHOLD || newValue >= WIDETHRESHOLD)
				{
					// must update any linked splatters, and redo exterior path
					if (m_ExteriorPath != null)
					{
						m_ExteriorPath.Dispose();
						m_ExteriorPath = null;
					}
					if (Status == StatusValues.Complete)
						Status = StatusValues.Moved; // allows splatters to detect that this has (effectively) moved
				}
			}
		}

		protected internal virtual bool Closed()
		{
			// returns true if the line draws back to the start point
			// This was only in Sequential in version 1; it is useful however for cutting
			return true;
		}

		/// <summary>What method default implementations of things like intersections should use in this class: either treating the Vertices
		/// as a sequence of straight lines, or using m_objPath. Affects default implementations of all CheckIX methods, GetSegment, ContainsSegment</summary>
		protected enum LineLogics
		{
			VerticesAsLines,
			UsePath,
			/// <summary>Neither the path nor vertices as lines is appropriate: the shape must override all applicable methods (all CheckIX, GetSegment, ContainsSegment)</summary>
			Custom
		}
		/// <summary>What method default implementations of things like intersections should use in this class: either treating the Vertices
		/// as a sequence of straight lines, or using m_Path.  Default is VerticesAsLines: no need to override if this is appropriate</summary>
		protected virtual LineLogics LineLogic => LineLogics.VerticesAsLines;

		#endregion

		#region Default implemention of some base functions based on line list

		public override VerbResult Start(ClickPosition position)
		{
			PointF pt = position.Snapped;
			Debug.Assert(m_DefinedVertices == 0);
			Debug.Assert(LineStyle != null);
			LineStyle.SetDefaults();
			if ((Allows & AllowedActions.Arrowheads) > 0)
			{
				StartArrowhead.SetDefaults();
				EndArrowhead.SetDefaults();
			}
			Vertices.Clear();
			Vertices.Add(pt); // this is the point that the user has fixed and is part of the data.
			Vertices.Add(pt); // this is the floatingpoint which the user is still moving
			m_DefinedVertices = 1;
			m_Bounds = new RectangleF(pt.X, pt.Y, 0, 0);
			Debug.Assert((Flags & GeneralFlags.ProtectBounds) == 0);
			return VerbResult.Continuing;
		}

		public override VerbResult Float(ClickPosition position)
		{
			// move the "current" point.  Many subclasses which need to do further calculation can call this version first
			if (m_DefinedVertices >= Vertices.Count)
			{
				Debug.Fail("Lined: Float no points are undetermined");
				return VerbResult.Unchanged;
			}
			Vertices[m_DefinedVertices] = position.SnappedExcluding(Vertices[m_DefinedVertices - 1]); // avoids 0 length on grids (mainly for graphical reasons; actual 0-length rejected anyway)
			DiscardPath();
			m_Bounds = RectangleF.Empty;
			Debug.Assert((Flags & GeneralFlags.ProtectBounds) == 0);
			if (m_DefinedVertices < 2)
				DecacheArrowhead(false);
			DecacheArrowhead(true);
			return VerbResult.Continuing;
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			if (Vertices.Count > 0)
				transformation.TransformPoints(Vertices);
			DecacheArrowheads();
			DiscardPath();
		}

		/// <summary>generates the prompts where the shape consists of an arbitrary sequence, with Finish to place the last vertex</summary>
		private protected List<Prompt> GetPolyPointPrompts(string prefix)
		{
			List<Prompt> list = new List<Prompt>();
			if (m_DefinedVertices == 1)
			{
				// image isn't different at first point (not worth drawing all those images)
				// however, the text changes
				list.Add(new Prompt(ShapeVerbs.Choose, prefix + "_Choose1", prefix + "_Choose"));
				list.Add(new Prompt(ShapeVerbs.Choose | ShapeVerbs.Complete, "CancelAll", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Choose, prefix + "_Choose", prefix + "_Choose"));
				list.Add(new Prompt(ShapeVerbs.Complete, prefix + "_Finish", prefix + "_Finish"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "Various_Cancel1", prefix + "_Cancel1"));
			}
			return list;
		}

		/// <summary>generates the prompts where the shape consists a baseline mostly defining the shape.  use flip=true if remaining option is merely which side the rest of the shape is placed</summary>
		private protected List<Prompt> GetBaseLinePrompts(string prefix, bool flip)
		{
			List<Prompt> list = new List<Prompt>();
			if (m_DefinedVertices == 1)
			{
				if (Strings.Exists("Prompts_" + prefix + "_Choose1"))
					list.Add(new Prompt(ShapeVerbs.Choose, prefix + "_Choose1", prefix + "_Choose"));
				else
					// most shapes use the generic version...
					list.Add(new Prompt(ShapeVerbs.Choose, "BaseLine_Choose1", prefix + "_Choose"));
				list.Add(new Prompt(ShapeVerbs.Choose | ShapeVerbs.Complete, "CancelAll", "CancelAll"));
			}
			else
			{
				if (flip)
					list.Add(new Prompt(ShapeVerbs.Complete, "BaseLineFlip_Finish", prefix + "_Finish"));
				else
					list.Add(new Prompt(ShapeVerbs.Complete, prefix + "_Finish", prefix + "_Finish"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "BaseLine_Cancel1", prefix + "_Cancel1"));
			}
			return list;
		}

		protected internal override VerbResult OtherVerb(ClickPosition position, Functions.Codes code)
		{
			// Performs basic Add and Remove vertex.  This will only be called however if the derived class has reported that these are applicable (the default is inapplicable)
			Target target = position.Page.SelectedPath;
			switch (code)
			{
				case Functions.Codes.RemoveVertex:
					if (m_DefinedVertices <= 2)
						return VerbResult.Rejected;
					m_DefinedVertices -= 1;
					Vertices.RemoveAt(target.ShapeIndex);
					DiscardPath();
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
				case Functions.Codes.AddVertex:
					Vertices.Insert(target.ShapeIndex + 1, target.Position);
					m_DefinedVertices += 1;
					DiscardPath();
					m_Bounds = RectangleF.Empty;
					return VerbResult.Continuing;
			}
			return base.OtherVerb(position, code);
		}

		#endregion

		#region List of vertices and support therefore
		protected List<PointF> Vertices;

		/// <summary>the number of vertices which the user has so far placed. There might be further vertices in the list - the list can be filled with temporary points as the user moves the mouse</summary>
		protected int m_DefinedVertices;
		protected virtual int FixedVerticesLength()
		{
			// returns number of vertices in a complete shape, if this is fixed.  Otherwise -1
			return -1;
		}

		/// <summary>we quite often need to know the last point which has been placed so far </summary>
		internal PointF LastDefined => m_DefinedVertices <= 0 ? PointF.Empty : Vertices[m_DefinedVertices - 1];

		protected virtual void FixVertex()
		{
			// fixes the currently floating vertex and adds a new one at the same position
			// assumes there is one floating vertex
			Debug.Assert(m_DefinedVertices == Vertices.Count - 1, "Lined.Fix/DiscardVertex is only applicable if there is exactly 1 floating vertex on the end of the list");
			Vertices.Add(Vertices[Vertices.Count - 1]);
			Vertices[Vertices.Count - 1] = Vertices[Vertices.Count - 2];
			if (m_DefinedVertices < 2)
				DecacheArrowhead(false);
			DecacheArrowhead(true);
			DiscardPath();
			m_DefinedVertices += 1;
		}

		protected virtual void DiscardVertex()
		{
			// discard the final vertex and make a previous one floating
			Debug.Assert(m_DefinedVertices >= 2, "Lined.DiscardVertex should  not be used to remove the last vertex");
			Debug.Assert(m_DefinedVertices == Vertices.Count - 1, "Lined.Fix/DiscardVertex is only applicable if there is exactly 1 floating vertex on the end of the list");
			Vertices.RemoveAt(Vertices.Count - 1);
			m_DefinedVertices -= 1;
			m_Bounds = RectangleF.Empty;
			Debug.Assert((Flags & GeneralFlags.ProtectBounds) == 0);
			DiscardPath();
		}

		protected SizeF BaseVector()
		{
			// returns the vector between the first two vertices.  The used quite a lot in the derived classes
			return Vertices[0].VectorTo(Vertices[1]);
		}

		public virtual PointF CalculateCentreFromPoints()
		{
			// calculate the "centre" of the shape by averaging all the points
			float X = 0;
			float Y = 0;
			for (int index = 0; index <= Vertices.Count - 1; index++)
			{
				X += Vertices[index].X;
				Y += Vertices[index].Y;
			}
			return new PointF(X / Vertices.Count, Y / Vertices.Count);
		}

		protected virtual void SetLength(int length)
		{
			// force the length of the vertex array to be this value
			while (Vertices.Count > length)
			{
				Vertices.RemoveAt(Vertices.Count - 1);
			}
			while (Vertices.Count < length)
			{
				Debug.Assert(Vertices.Count > 0, "Sequential.SetLength will not work unless a least one vertex has been provided - this should have happened in the Start function");
				Vertices.Add(Vertices[Vertices.Count - 1]);
			}
		}

		protected internal override PointF DoSnapAngle(PointF newPoint)
		{
			PointF ptOrigin = Vertices[Math.Min(Math.Max(m_DefinedVertices - 1, 0), Vertices.Count - 1)]; // check for Vertices.Count needed for Ellipse (et al?)
			return Geometry.AngleSnapPoint(newPoint, ptOrigin);
		}

		/// <summary>Default implementation of Tidy which tidies V 0...intLast.  shapes will often want to avoid doing all vertices</summary>
		protected virtual bool TidyVertices(SnapModes modes, Page page, int last)
		{
			bool changed = TidyVertices(Vertices, this, modes, page, last);
			if (changed)
			{
				m_Bounds = RectangleF.Empty;
				DecacheArrowheads();
			}
			Debug.Assert((Flags & GeneralFlags.ProtectBounds) == 0);
			return changed;
		}

		/// <summary>this rather untidy shared version is done so that TextArea can use it (and AbstractRectangle).  Use instance method in derived classes.</summary>
		internal static bool TidyVertices(List<PointF> Vertices, Shape thisShape, SnapModes mode, Page page, int last)
		{
			bool changed = false;
			for (int index = 0; index <= last; index++)
			{
				PointF newPoint = Vertices[index];
				switch (mode)
				{
					case SnapModes.Grid:
						newPoint = page.Paper.SnapPoint2(newPoint);
						break;
					case SnapModes.Angle:
						if (index > 0)
						{
							if (index < last)
								// points before and after - use both to choose
								newPoint = Geometry.AngleSnapFromTwoPoints(newPoint, Vertices[index - 1], Vertices[index + 1], true);
							else
								newPoint = Geometry.AngleSnapPoint(newPoint, Vertices[index - 1]);
						}
						break;
					case SnapModes.Shape:
						newPoint = TidyPointToShapes(newPoint, page, thisShape);
						break;
					default:
						Debug.Fail("Unexpected snap mode in TidyVertices: " + mode);
						break;
				}
				if (newPoint != Vertices[index])
				{
					Vertices[index] = newPoint;
					changed = true;
				}
			}
			return changed;
		}

		internal static PointF TidyPointToShapes(PointF old, Page page, Shape thisShape)
		{
			List<Target> targets = page.GenerateIndependentTargets(UserSocket.CreateForPoint(old), 1);
			// unfortunately the first target(s) are likely to be on the shape we are moving
			for (int index = 0; index <= targets.Count - 1; index++)
			{
				if (targets[index].AdjustedDistance > Target.ActivationThreshold)
					return old; // too far away give up - targets sorted into increasing distance, so others wont work either
				if (targets[index].Shape != thisShape)
					return targets[index].Position;
			}
			return old;
		}

		#endregion

		#region Path
		/// <summary>this is used both for drawing and hit testing.  Used only during processing and not generally part of the data.  Derived classes are not required to implement CreatePath, but if they don't they must not use the functions which access it</summary>
		protected GraphicsPath m_Path;
		/// <summary>Created (by EnsurePath) only if IsWide.  Is created in this class using m_Path; derived classes do not need to update CreatePath for this.
		/// Represents the outline of a very wide line</summary>
		protected GraphicsPath m_ExteriorPath;

		protected const float WIDETHRESHOLD = 3; // mm - approx 9 ot 10pt
		/// <summary>True if this line is very wide, and we should use the outer path for intersections etc; i.e. it is not treated as a single line but more like a filled shape
		/// False if line completely transparent (eg if this is filled only - then irrelevant what the unused line thickness is set to)</summary>
		protected bool IsWide
		{
			get
			{
				if (LineStyle == null)
					return false;
				return LineStyle.Width > WIDETHRESHOLD && LineStyle.Colour.A > 0;
			}
		}

		/// <summary>Create path object(s) if they are not defined.  Not supported by some classes, and some keep them permanently to define the shape</summary>
		/// <param name="needExterior">Controls whether exterior path is created IF APPLICABLE.  Ignored if not IsWide</param>
		/// <returns>It returns the path for convenience to the caller (allowing this to be used in line in another statement).
		/// The return value is the exterior path if it was both requested and IsWide</returns>
		protected GraphicsPath EnsurePath(bool needExterior)
		{
			if (m_Path == null)
				CreatePath();
			if (needExterior && IsWide && m_ExteriorPath == null)
			{
				m_ExteriorPath = (GraphicsPath)m_Path.Clone();
				// Unfortunately the Widen does return a rather approximate path.  Specifically it tends to be made of straight lines which are slightly larger than the actual path
				// therefore we use a pen which is slightly too small which brings it inwards, and the override of Widen which increases the number of points used as an approximation
				using (Pen pn = new Pen(Color.Black, LineStyle.Width - 0.18f) { MiterLimit = 2 })
				{
					// Documentation says that this adds a new figure containing the outline; however it appears to replace the original line in fact
					// it also seems to always generate straight lines.  There is an override with a tension factor for curves, but I'm not sure if this actually does anything
					// (it may have created a different number of straight lines?)
					m_ExteriorPath.Widen(pn, Geometry.IdentityMatrix, 0.3F); // Smaller numbers for the flatness parameter given more points on the curve.  Default appears to be about 0.55
				}

				Debug.Assert(!m_ExteriorPath.PathTypes.Any(x => (x & PATHTYPEMASK) == PATHBEZIER));
				// Unfortunately the path created is not particularly good.  Particularly it makes no attempt to avoid overlapping itself on internal corners
				// see GraphicsPath.widen in documentation folder.  However it is possible to filter out the nonsense by looking for lines which cross themselves within a couple of
				// segments
				PointF[] points = m_ExteriorPath.PathPoints;
				// No need to check the types; they are all lines
				List<PointF> colNew = new List<PointF>();
				int index = 1;
				colNew.Add(points[0]); // the start point is always used
				PointF last = points[0];
				PointF test = points[1];
				Debug.WriteLine("Create external path: " + ID);
				while (index < points.Length)
				{
					// check if this line intersects any of the following ones.  We are checking the line leading up to index (because its start point was OK, we want to know if the endpoint needs to be changed)
					// There is no absolute way of knowing how far to look ahead.  Going to index +2 gets rid of the standard crossover that Wide produces at almost every internal
					// corner.  However it does leave some problems where the a curve genuinely reverses over itself.  Going further smooths some of these, but there is a risk
					// of losing genuine detail.  This doesn't really cope where a pencil does a complete reversal.  However if the line overlaps itself substantially
					// the relevant intersection could be an arbitrary number of points ahead.  The only solution here would be to do a proper tracing of the outline
					// but... that would potentially produce multiple figures in other cases where a line crosses itself after a proper loop
					bool shouldContinue = false;
					for (int testIndex = index + 1; testIndex <= Math.Min(index + 4, points.Length - 2); testIndex++) // IntTest is the point at the START of the other line we are checking
					{
						// We still need to make some checks to avoid straight lines which genuinely curve back on themselves.  The following isn't strictly logical
						// but if there is any segment is quite long, the chances are we are now outside the wide pen bounds
						if (Geometry.DirtyDistance(points[testIndex], points[testIndex - 1]) > LineStyle.Width * 2)
							break;
						var ptIntersection = Intersection.Line_LineIntersection(last, test, true, points[testIndex], points[testIndex + 1], true, false, false);
						if (!ptIntersection.IsEmpty)
						{
							// The line crosses itself; do the path up to the intersection instead of following the nonsense part
							// In fact there can be multiple nonsense (see bad case screenshot); so the intersection cannot be immediately added to the list
							// instead we must continue testing from the intersection
							test = ptIntersection;
							index = testIndex;
							shouldContinue = true;
							break;
						}
					}
					if (shouldContinue) continue;
					// No problems with this section.  But we do also filter out consecutive points which are almost equal; partly because these may cause an assertion later when fetching the segment
					if (!test.ApproxEqual(last))
						colNew.Add(test);
					last = test;
					index += 1;
					if (index < points.Length)
						test = points[index]; // Otherwise the loop won't repeat anyway
				}

				byte[] types = new byte[colNew.Count - 1 + 1];
				types[0] = 0;
				types[colNew.Count - 1] = PATHLINE + PATHCLOSUREFLAG;
				for (index = 1; index <= colNew.Count - 2; index++)
				{
					types[index] = PATHLINE;
				}
				m_ExteriorPath.Dispose();
				m_ExteriorPath = new GraphicsPath(colNew.ToArray(), types);
			}
			return needExterior && IsWide ? m_ExteriorPath : m_Path;
		}

		/// <summary>Must be overridden by any class which supports paths.  On entry m_Path will be null.  Should not be called directly use EnsurePath</summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		protected virtual void CreatePath()
		{
			throw new InvalidOperationException("Shape " + ShapeCode + " does not support Path - EnsurePath not implemented");
		}

		/// <summary>Returns path made on lines between points.  this is a shared function because it is also used by Ruler, which is not derived from this class</summary>
		internal static GraphicsPath GetLinearPath(IList<PointF> vertices, bool closed)
		{
			GraphicsPath create = new GraphicsPath();
			if (closed)
				create.StartFigure();
			for (int index = 0; index <= vertices.Count - 2; index++)
			{
				create.AddLine(vertices[index], vertices[index + 1]);
			}
			if (closed)
				create.CloseFigure();
			return create;
		}

		protected virtual void DiscardPath()
		{
			// can be overridden by any shape which needs the path as part of the definition of the shape, and therefore should not dispose it (but must still dispose exterior path)
			// it is the responsibility of the shapes to transform the path as necessary when moving etc
			m_Path?.Dispose();
			m_Path = null;
			m_ExteriorPath?.Dispose();
			m_ExteriorPath = null;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			// note should not use DiscardPath, because a few shapes will ignore that
			m_Path?.Dispose();
			m_Path = null;
			m_ExteriorPath?.Dispose();
			m_ExteriorPath = null;
		}

		public override GraphicsPath ConvertToPath()
		{
			EnsurePath(false);
			return (GraphicsPath)m_Path.Clone(); // Needs to be a copy because m_Path will be disposed at some point
		}

		#endregion

		#region Default implementations which use the path
		// any shape which did not create a path will need to override all of these

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			// uses a path to check if this point is actually inside the shape
			EnsurePath(false);
			if (treatAsFilled && !(this is Filled))
				treatAsFilled = false; // shapes which don't even support the possibility of filling never behave filled
			if (treatAsFilled || IsFilled || LineInvisible || HasText(true))
				return m_Path.IsVisible(clickPoint);
			using (Pen pn = new Pen(Color.Black, (LineStyle.Width + Line.LINEHITTOLERANCE) / (float)Math.Sqrt(scale)) { MiterLimit = MITRELIMIT })
			{
				return m_Path.IsOutlineVisible(clickPoint, pn);
			}
		}

		// 3 different ways of calculating the standard bounding box.  All versions do the arrowhead calculation
		protected RectangleF BoundsOfPathDetailed()
		{
			// calculates the correct bounding rectangle, including any curves.
			// Does not make any allowance for line thickness
			if (Vertices.Count == 0 && m_Path == null)
				return RectangleF.Empty;
			RectangleF bounds = EnsurePath(false).GetBounds();
			ExtendBoundsToIncludeArrowhead(ref bounds);
			return bounds;
		}

		protected RectangleF BoundsOfPathPoints()
		{
			// returns the bounding rectangle containing all the points on the path, might not include any curves going outside
			// shapes can override MinimalBounds and return this if necessary.  This is much slower than the default MinimalBounds
			// which just returns the cached bounding rectangle.  For shapes containing only lines the results will be the same anyway
			RectangleF bounds = new RectangleF();
			EnsurePath(false);
			Geometry.Extend(ref bounds, m_Path.PathPoints);
			ExtendBoundsToIncludeArrowhead(ref bounds);
			return bounds;
		}

		protected RectangleF BoundsOfVertices()
		{
			// return the bounding box including all the vertices.  Sometimes used as a simple get out when shapes are incomplete (e.g. still placing first baseline)
			if (Vertices.Count == 0)
				return RectangleF.Empty;
			RectangleF bounds = RectangleF.Empty;
			for (int index = 0; index <= Vertices.Count - 1; index++)
			{
				Geometry.Extend(ref bounds, Vertices[index]);
			}
			ExtendBoundsToIncludeArrowhead(ref bounds);
			return bounds;
		}

		// 2 different ways of calculating the refresh bounding box.  Usually the full Path one is needed
		// due to corners where lines join.  The only exception is if there are no joins, in which case the second, faster version could be used
		protected RectangleF RefreshBoundsFromPath(bool withShadow = false)
		{
			// calculates the correct bounding rectangle, including any curves.
			// Does not make any allows for line thickness
			if (Vertices.Count == 0 && m_Path == null)
				return RectangleF.Empty;
			// both conditions are needed, because more complex shapes may have no Vertices, but just define the path directly
			RectangleF bounds;
			EnsurePath(false);
			// https://groups.google.com/forum/#!topic/microsoft.public.dotnet.framework.drawing/0exwkutWaqQ
			using (Pen pn = new Pen(Color.Black, LineStyle.Width) { MiterLimit = MITRELIMIT }) // MITRELIMIT important, see above
			{
				bounds = m_Path.GetBounds(Geometry.IdentityMatrix, pn);
			}

			ExtendBoundsToIncludeArrowhead(ref bounds);
			if (withShadow)
			{
				bounds.Width += GUIUtilities.SHADOWXOFFSET + 1; // +1 because the shadow pen is that much wider than the usual pen
				bounds.Height += GUIUtilities.SHADOWXOFFSET + 1;
			}
			return bounds;
		}

		/// <summary>Calculates refresh bounds from normal bounds - ie by adding enough width for bounding rect</summary>
		protected RectangleF RefreshBoundsFromBounds(bool withShadow = false, float useLineWidth = -1)
		{
			RectangleF bounds = base.RefreshBounds();
			float width = useLineWidth > 0 ? useLineWidth : LineStyle.Width;
			if (withShadow)
				width += GUIUtilities.SHADOWEXTRAWIDTH;
			if (width > GUIUtilities.SHADOWEXTRAWIDTH)
				bounds.Inflate(width / 2, width / 2);
			if (withShadow)
			{
				bounds.Width += GUIUtilities.SHADOWXOFFSET * 2; // *2 because the shadow pen is that much wider than the usual pen
				bounds.Height += GUIUtilities.SHADOWXOFFSET * 2;
			}
			return bounds;
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			return RefreshBoundsFromPath(withShadow);
		}

		#endregion

		#region Drawing
		internal static Color SHADOWCOLOUR = Color.FromArgb(30, 0, 0, 0);
		internal static bool WireFrame = false; // on diagnostic menu - changes all drawing to 1-pixel wide (as close as pos) lines
		internal const float MITRELIMIT = 2; // makes a BIG difference to thick lines, especially calc bounds of (assumes pen width * mitrelimit required, and default is 10)

		protected void InternalDrawFromPath(Canvas gr, DrawResources resources)
		{
			if (Vertices.Count < 2 && m_Path == null)
				return;
			if (m_DefinedVertices < 2)
			{
				// must be manoeuvring the first line at the moment... ' just draw a line between them
				if (resources.MainPen != null)
				{
					if (Vertices[0].ApproxEqual(Vertices[1]))
						// if user has not move the mouse yet (particularly applies to separate keyboard cursor) at least or something visible…
						gr.DrawLine(new PointF(Vertices[0].X - 0.25f, Vertices[0].Y), new PointF(Vertices[1].X + 0.25f, Vertices[1].Y), resources.MainPen);
					else
						gr.DrawLine(Vertices[0], Vertices[1], resources.MainPen);
				}
			}
			else
			{
				EnsurePath(WireFrame); // exterior path only ever needed if drawing wire frame
				if (resources.SingleElement != null)
				{
					IEnumerable<PathElement> elements = GetPathNearTarget(resources.SingleElement);
					GraphicsPath partialPath = PathElement.CreatePath(elements);
					gr.Path(partialPath, resources.MainPen);
					partialPath.Dispose();
					return;
				}
				if (WireFrame && IsWide)
				{
					if (resources.MainPen != null)
						gr.Path(m_ExteriorPath, resources.MainPen);
				}
				else if (resources.MainBrush != null || resources.MainPen != null)
					gr.Path(m_Path, resources.MainPen, resources.MainBrush);
			}
		}

		/// <summary>Returns path for one target, for drawing selection.  If target is a line it should be that line segment.  If it is a vertex it should be the lines before and after.
		/// Implementation is not complete for all shapes as we're only really interested for GraphicsPath</summary>
		private protected virtual IEnumerable<PathElement> GetPathNearTarget(Target target)
		{
			EnsurePath(false);
			PathElement[] elements = PathElement.WithinPath(m_Path, true, true).ToArray();
			switch (target.Type)
			{
				case Target.Types.Line:
					yield return elements[target.ShapeIndex];
					break;
				case Target.Types.Vertex:
					if (target.ShapeIndex > 0)
						yield return elements[target.ShapeIndex - 1];
					else if (Closed())
						yield return elements.Last();
					if (target.ShapeIndex < elements.Length)
						yield return elements[target.ShapeIndex];
					else if (Closed())
						yield return elements[0];
					break;
				default: throw new ArgumentException("Only Line or Vertex targets permitted in GetPathNearTarget");
			}
		}

		protected override void PrepareDraw(DrawResources resources)
		{
			base.PrepareDraw(resources);
			DefaultPrepareLineDraw(resources, LineStyle);
		}

		/// <summary>it is extracted into a separate function so that subclasses can always access the default through any number of layers of other classes
		/// (e.g. Filled will override this, and subclasses of that might want to access this part, but replace the Filled part)</summary>
		protected void DefaultPrepareLineDraw(DrawResources resources, LineStyleC lineStyle)
		{//SAWItem overrides and modifies this, so may need to be updated if this is changed
			float width = lineStyle.Width;
			if (WireFrame)
				width = Geometry.INCH / (resources.Graphics.DpiX * resources.Scale);
			if (width > 0) // added v3
			{
				if (IsFilled && lineStyle.Colour.A == 0 && m_DefinedVertices < 2 && Status == StatusValues.Creating && this is Filled)
				{
					// special case if drawing first line only which is transparent, but is filled, then draw a line using the fill colour
					resources.MainPen = resources.Graphics.CreateStroke(((Filled)this).FillStyle.Colour, width * 2);
				}
				else if (resources.EdgeAlpha == 255)
					resources.MainPen = resources.Graphics.CreateStroke(lineStyle.Colour, width);
				else
					resources.MainPen = resources.Graphics.CreateStroke(Color.FromArgb(resources.EdgeAlpha, lineStyle.Colour), width);
				resources.MainPen.DashStyle = lineStyle.Pattern;
				if (lineStyle.Pattern != DashStyle.Solid && lineStyle.Width < 2)
				{
					// not sure if we should take account of scaling here.  It does not seem to matter too much.  Although a dashed 2px line
					// at 50% looks just as naff as a 1px at 100% I think it looks less wrong to the user, because at low scale you expect to lose some detail
					resources.MainPen.DashStyle = DashStyle.Custom;
					resources.MainPen.DashPattern = lineStyle.GenerateCustomDashPattern();
				}
				resources.MainPen.MiterLimit = MITRELIMIT;
			}
		}

		protected override bool PrepareShadow(DrawResources resources)
		{
			// can't use DefaultPrepareDraw as we need to ignore the usual colour
			// so I think it is better if the shadow is not dashed (partly because the dash length will differ from the line dash length
			// because the width of the line is different)
			resources.MainPen = resources.Graphics.CreateStroke(SHADOWCOLOUR, LineStyle.Width + resources.SHADOWEXTRAWIDTH);
			resources.MainPen.MiterLimit = MITRELIMIT;
			resources.MainPen.LineJoin = LineJoin.Round; // otherwise points can stick out for miles, busting the refreshbounds
			return true;
		}

		protected override void PrepareHighlight(DrawResources resources)
		{
			base.PrepareHighlight(resources);
			float width = LineStyle.Width + resources.HIGHLIGHTEXTRAWIDTH;
			if (WireFrame)
				width = 2 * Geometry.INCH / (resources.Graphics.DpiX * resources.Scale);
			resources.MainPen = resources.Graphics.CreateStroke(CurrentHighlightColour, width);
			resources.MainPen.MiterLimit = MITRELIMIT;
			resources.MainPen.LineJoin = LineJoin.Round; // otherwise points can stick out for miles, busting the refreshbounds
		}

		protected bool LineInvisible => LineStyle != null && LineStyle.Colour.A == 0;

		#endregion

		#region Data and Copy
		// m_intDefinedVertices is not saved.  The derived classes should be able to set this automatically.  It is usually fixed
		// once the shape has been completed
		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			Vertices = reader.ReadListPoints();
			LineStyle = LineStyleC.Read(reader);
			if ((Allows & AllowedActions.Arrowheads) > 0)
				ArrowheadC.Read(reader, out StartArrowhead, out EndArrowhead); // which checks the file version
			m_DefinedVertices = Vertices.Count;
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(Vertices);
			LineStyle.Write(writer);
			if ((Allows & AllowedActions.Arrowheads) > 0)
				ArrowheadC.Write(writer, StartArrowhead, EndArrowhead);
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			Lined lined = (Lined)other;
			if (depth >= CopyDepth.Undo)
			{
				if (LineStyle == null || LineStyle == lined.LineStyle)
					LineStyle = new LineStyleC();
				LineStyle.CopyFrom(lined.LineStyle);
				if (lined.StartArrowhead != null)
				{
					if (StartArrowhead == null || StartArrowhead == lined.StartArrowhead)
						StartArrowhead = new ArrowheadC(false);
					StartArrowhead.CopyFrom(lined.StartArrowhead);
				}
				if (lined.EndArrowhead != null)
				{
					if (EndArrowhead == null || EndArrowhead == lined.EndArrowhead)
						EndArrowhead = new ArrowheadC(true);
					EndArrowhead.CopyFrom(lined.EndArrowhead);
				}
			}
			else
			{
				if (LineStyle == null)
					LineStyle = lined.LineStyle;
				// Don't need separate arrowhead objects, can just assign to the same object (mostly)
				if (StartArrowhead == null || lined.StartArrowhead == null)
					StartArrowhead = lined.StartArrowhead;
				else
				{
					// if they are already separate objects we mustn't reattach them to the same object (in transformations CopyFrom/Transform is used
					// to copy the data from the original shape into any number of transformed copies)
					StartArrowhead.CopyFrom(lined.StartArrowhead);
				}
				if (EndArrowhead == null || lined.EndArrowhead == null)
					EndArrowhead = lined.EndArrowhead;
				else
					EndArrowhead.CopyFrom(lined.EndArrowhead);
			}
			m_DefinedVertices = lined.m_DefinedVertices;
			Vertices.Clear();
			Vertices.AddRange(lined.Vertices);
			m_Bounds = lined.m_Bounds;
			DiscardPath();
		}

		protected override bool IdenticalToShape(Shape other)
		{
			//If Not MyBase.IdenticalToShape(objOther) Then Return False
			Lined lined = (Lined)other;
			if (LineStyle == null != (lined.LineStyle == null))
				return false;
			if (LineStyle != null)
			{
				if (!LineStyle.IdenticalTo(lined.LineStyle))
					return false;
			}
			if (Vertices.Count != lined.Vertices.Count)
				return false;
			if (!IdenticalForPaletteDelta)
			{
				for (int index = 0; index <= Vertices.Count - 1; index++)
				{
					if (Vertices[index] != lined.Vertices[index])
						return false;
				}
			}
			return true;
		}
		#endregion

		#region Targets and grabspots etc
		// default implemention of targets uses the array of Vertices
		// Target.ShapeIndex will be the index of the first Vertex forming that side for Line targets
		private protected List<Target> GenerateTargetsDefault(UserSocket floatSocket, bool closed)
		{
			// default implemention of GenerateTargets - subclass must specify whether to treat the list of vertices as closed
			// centre is automatically included if it is not empty
			List<Target> targets = new List<Target>();
			AddLineTargets(this, targets, floatSocket, Vertices.ToArray(), closed);
			PointF centre = Centre;
			if (!centre.IsEmpty)
				targets.Add(new Target(this, centre, Target.Types.Centre, floatSocket, Target.Priorities.Low));
			return targets;
		}

		internal static void AddLineTargets(Shape shp, List<Target> targets, UserSocket floatSocket, PointF[] points, bool closed)
		{
			// adds targets for a set of lines
			// add all of the vertices
			for (int index = 0; index <= points.Length - 1; index++)
			{
				targets.Add(new Target(shp, points[index], Target.Types.Vertex, floatSocket, Target.Priorities.Vertex) { ShapeIndex = index });
			}
			int last = points.Length - 2; // vertex index at the beginning of last line
			if (closed)
				last += 1; // there is a line back to the beginning again
			for (int index = 0; index <= last; index++)
			{
				PointF end;
				if (index == points.Length - 1) // we are on the line back to the beginning
					end = points[0];
				else
					end = points[index + 1];
				Target create = GenerateTargetOnOneLine(shp, floatSocket, points[index], end, targets, index);
				if (create != null)
					targets.Add(create);
			}
		}

		/// <summary>Generates the target on a given line.  Returns nothing if not appropriate.  Caller must add to list</summary>
		/// <remarks>the existing targets must be provided, because the one on the line is ignored if it is very close to something else on this shape (especially intersection)
		/// note that it is best to add the intersections first!</remarks>
		private protected static Target GenerateTargetOnOneLine(Shape shp, UserSocket floatSocket, PointF ptLineA, PointF ptLineB, List<Target> existingTargets, int shapeIndex)
		{
			PointF closest = Geometry.ClosestPointOnLineLimited(ptLineA, ptLineB, floatSocket.Centre, true);
			if (!closest.IsEmpty)
			{
				// but if it is very near the end of the line then ignore it
				// (not least because if the user is close enough to trigger remember vertex would take priority and would hide the line target anyway)
				if (Geometry.DirtyDistance(closest, ptLineA) > Target.IGNOREOVERLAPPINGTHRESHOLD && Geometry.DirtyDistance(closest, ptLineB) > Target.IGNOREOVERLAPPINGTHRESHOLD)
				{
					if (ExistingTargetCloseTo(existingTargets, closest))
						return null; // something else (e.g. intersection) is already close to this
					return new Target(shp, closest, Target.Types.Line, floatSocket, shapeIndex: shapeIndex);
				}
			}
			return null;
		}

		private static bool ExistingTargetCloseTo(List<Target> targets, PointF test)
		{
			// returns true if the list of targets already contains something close to the given point
			// usually this means anyone will be ignored.  Therefore it is best to add the ones with high priority first
			return targets.Any(target => Geometry.DirtyDistance(target.Position, test) < Target.IGNOREOVERLAPPINGTHRESHOLD);
		}

		private protected Target GenerateLineTargetOnOneBezier(UserSocket floatSocket, PointF[] P, List<Target> existingTargets, float maximumError, int shapeIndex)
		{
			// generates the target on the given Bezier.  Return is nothing if not appropriate
			// the existing targets must be provided, because the one on the line is ignored if it is very close to something else on this shape (especially intersection)
			// the caller should first do a quick test whether the point is close enough to the bounding box of this arc before calling this function
			// the closest T value is found by binary search
			PointF mouse = floatSocket.Centre;
			float lowT = 0;
			float highT = 1;
			float lowDistance = Geometry.DistanceBetween(mouse, P[0]);
			float highDistance = Geometry.DistanceBetween(mouse, P[3]);
			PointF high = P[3];
			PointF low = P[0];
			while (highT - lowT > maximumError)
			{
				float half = (lowT + highT) / 2;
				PointF create = Bezier.GetPoint(P, half);
				float testDistance = Geometry.DistanceBetween(mouse, create);
				if (lowDistance > highDistance)
				{
					lowDistance = testDistance;
					lowT = half;
					low = create;
				}
				else
				{
					highDistance = testDistance;
					highT = half;
					high = create;
				}
			}
			// everything above is basically from Curve.FindNearestApproachToPoint, but is reproduced here because we don't just want the T-value,
			// but also the distances and points
			if ((lowDistance + highDistance) / 2 < Target.MaximumInterestDistance) // this average isn't really meaningful, but is close enough
			{
				// but if it is very near the end of the line then ignore it
				// (not least because if the user is close enough to trigger remember vertex would take priority and would hide the line target anyway)
				PointF result = Geometry.MidPoint(low, high);
				if (Geometry.DirtyDistance(result, P[0]) > Target.IGNOREOVERLAPPINGTHRESHOLD && Geometry.DirtyDistance(result, P[3]) > Target.IGNOREOVERLAPPINGTHRESHOLD)
				{
					if (ExistingTargetCloseTo(existingTargets, result))
						return null; // something else (e.g. intersection) is already close to this
					return new Target(this, result, Target.Types.Line, floatSocket, shapeIndex: shapeIndex) { ShapeParameter = (highT + lowT) / 2 };
				}
			}
			return null;
		}

		internal override void DrawLineTarget(Target target, Graphics gr, Pen pn, int activePhase)
		{
			// Target.ShapeIndex is the index of the first vertex on this side
			// we need to draw a pretty much fixed length in the direction of this vertex
			PointF end;
			if (target.ShapeIndex == Vertices.Count - 1) // this must be the closed line back to the beginning
				end = Vertices[0];
			else
				end = Vertices[target.ShapeIndex + 1];
			DrawLineTargetGivenPoints(gr, pn, activePhase, target.Position, Vertices[target.ShapeIndex], end);
		}

		internal static void DrawLineTargetGivenPoints(Graphics gr, Pen pn, int activePhase, PointF target, PointF ptLineA, PointF ptLineB)
		{
			// draws the target along a line, once the line has been determined (this is also used by some of the circular ones, e.g. Pie
			SizeF vector = ptLineA.VectorTo(ptLineB); // vector along this edge
			vector = vector.MultiplyBy(Target.LINEDRAWLENGTH * GUIUtilities.MillimetreSize / vector.Length()); // length changed to be half the maximum
			gr.DrawLine(pn, target.X - vector.Width, target.Y - vector.Height, target.X + vector.Width, target.Y + vector.Height);
		}

		private protected void AddGrabSpotsForAllVertices(List<GrabSpot> spots, Prompt usePrompt = null)
		{
			// all vertices can be moved independently
			for (int index = 0; index <= Vertices.Count - 1; index++)
			{
				spots.Add(new GrabSpot(this, GrabTypes.SingleVertex, Vertices[index], index) { Prompts = new[] { usePrompt } });
			}
		}

		protected void AddBaselineGrabSpot(List<GrabSpot> list)
		{
			list.Add(new GrabSpot(this, GrabTypes.Fixed, Vertices[0], 0));
			list.Add(new GrabSpot(this, GrabTypes.SingleVertex, Vertices[1], 1));
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			// This sort of assumes that the logic of the shape is Sequential.  However I think the only shapes which use this which are not Sequential are Line
			// and its derivatives, for which the logic here still works
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					PointF ptNew = Vertices[move.ShapeIndex];
					move.Transform.TransformPoint(ref ptNew);
					// we need to check that this point does not collide with the previous or next ones before actually storing it
					if (move.ShapeIndex > 0 && ptNew.ApproxEqual(Vertices[move.ShapeIndex - 1]))
						return;
					if (move.ShapeIndex < Vertices.Count - 1 && ptNew.ApproxEqual(Vertices[move.ShapeIndex + 1]))
						return;
					Vertices[move.ShapeIndex] = ptNew;
					if ((Flags & GeneralFlags.ProtectBounds) == 0)
						m_Bounds = RectangleF.Empty;
					if (move.ShapeIndex <= 1)
						DecacheArrowhead(false);
					if (move.ShapeIndex >= m_DefinedVertices - 2)
						DecacheArrowhead(true);
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
			DiscardPath();
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			return (from pt in Vertices select UserSocket.CreateForPoint(pt)).ToList();
		}

		#endregion

		#region Path flags

		internal const int PATHSTART = 0; // Start of a figure; should only be used on the first item
		internal const int PATHLINE = 1;
		internal const int PATHBEZIER = 3;
		internal const int PATHTYPEMASK = 7; // mask which selects the type of segment
		internal const int PATHCLOSUREFLAG = 0x80;

		#endregion

		#region Sockets
		// socket index is vertex*2 (even) or midpoint of line = vertex_at_start*2+1
		internal override PointF SocketPosition(int index)
		{
			// we don't need to know if the shape is closed - if intIndex implies closed then assume that it is
			if (index == -1)
				return Middle();
			if (index < 0 || index >= Vertices.Count * 2)
				return PointF.Empty;
			int vertex = index / 2;
			if (index % 2 == 1)
			{
				// mid point
				return Geometry.MidPoint(Vertices[vertex], Vertices[(vertex + 1) % Vertices.Count]);
				// second param will return vertex 0 if this is the closure line
			}
			return Vertices[vertex];
		}

		internal override SizeF SocketExitVector(int index)
		{
			if (index < 0)
				return new SizeF(1, 0); // just in case
			int vertex = index / 2;
			if (index % 2 == 1)
				// we want to exit perpendicular to the line if on a midpoint
				return LineSocketExitVector(Vertices[vertex], Vertices[(vertex + 1) % Vertices.Count]);

			// we want to exit at an angle that bisects the corner
			// get the two vectors away from this corner, with an arbitrary length
			SizeF szA = Vertices[vertex].VectorTo(Vertices[(vertex + 1) % Vertices.Count]);
			PointF previous;
			if (vertex == 0)
				previous = Vertices[Vertices.Count - 1];
			else
				previous = Vertices[vertex - 1];
			SizeF szB = Vertices[vertex].VectorTo(previous);
			szA = szA.ChangeLength(1); // if reset them both to the same length then adding or averaging will give the vector into this vertex
			szB = szB.ChangeLength(1);
			// and we now need to return the vector in the opposite direction, outwards from the shape
			SizeF exit = new SizeF(-szA.Width - szB.Width, -szA.Height - szB.Height);
			// the problem is if the lines were effectively parallel (which makes szExit 0,0) - treat the pair of lines as one
			if (exit.Length() < Geometry.NEGLIGIBLE)
				return LineSocketExitVector(Vertices[index], previous);
			return new SizeF(-szA.Width - szB.Width, -szA.Height - szB.Height);
		}

		/// <summary>default socket exit vector from middle of a line</summary>
		private protected SizeF LineSocketExitVector(PointF A, PointF B)
		{
			SizeF line = A.VectorTo(B);
			SizeF exit = line.Perpendicular(1); // the direction is not clear, we will need to just try
												// test whether a point in this direction is inside or outside the shape...
			PointF mid = A + line.MultiplyBy(0.5f);
			PointF test = mid + exit.ChangeLength(0.8f); // although 0.8 seems excessive, smaller values have been found to give incorrect answers sometimes (i.e. not detecting that the point is inside the path)
			if (HitTestDetailed(test, 1, true))
				// This point is inside the shape, we need to go in the other direction
				exit = line.Perpendicular(-1);
			return exit;
		}

		private protected List<Socket> DefaultGetSockets(bool closed, bool omitAutomatic = false)
		{
			List<Socket> sockets = new List<Socket>();
			if (!omitAutomatic)
				sockets.Add(new Socket(this, -1, Middle()));
			for (int vertex = 0; vertex <= Vertices.Count - 1; vertex++)
			{
				sockets.Add(new Socket(this, vertex * 2, Vertices[vertex]));
				if (closed || vertex < Vertices.Count - 1)
				{
					PointF ptEnd = Vertices[(vertex + 1) % Vertices.Count];
					sockets.Add(new Socket(this, vertex * 2 + 1, Geometry.MidPoint(Vertices[vertex], ptEnd)));
				}
			}
			return sockets;
		}

		#endregion

		#region Arrowheads
		// Derived classes which actually use this are: Curve, Line, PolyLine, (this), Connector
		public ArrowheadC StartArrowhead; // a random collection of the derived classes actually support these.  Check (Allows and Arrowheads)
		public ArrowheadC EndArrowhead; // both will be left as Nothing if not supported

		#region Style class
		public class ArrowheadC : StyleBase
		{
			// Both Write and Read always do the pair (end and start).  This class also caches the coordinates of the actual arrowhead
			// to save recalculate in every time it is drawn (or every time the bounding box is requested!).  These are not stored in the data, however

			public int Size; // exact meaning of this depends on the style.  1000 is default
			public const int DEFAULTSIZE = 1000;
			public enum Styles : byte
			{
				// numbers in data and cannot be changed
				None = 0,
				Version4 = 1, // simple V as version 4
				SimpleSolid, // simple triangle, solid
				VMedium, //  simple V, hollow
				RecessSolid, // as SimpleSolid with V notch in back
				MeasureBar, // bar across end as measurement
				DoubleSolid,
				LongSolid, // longer, thinner solid only end
				Fletching // only start - arrow fletching style
			}
			public Styles Style;
			public const float DEFAULTDRAWRATIO = 3 / 1000f; // multiplier to get length from tip to base of triangle from size
			private bool m_End; // remembers whether this is the end or start arrowhead; not stored in the file

			public ArrowheadC(bool end)
			{
				m_End = end;
			}

			public void SetDefaults()
			{
				Size = DEFAULTSIZE;
				Style = Styles.None;
				Points = null;
			}

			public static void Write(DataWriter writer, ArrowheadC start, ArrowheadC end)
			{
				if (start != null)
				{
					writer.Write(start.Size);
					writer.Write((byte)start.Style);
				}
				else
				{
					writer.Write(DEFAULTSIZE);
					writer.WriteByte(0);
				}
				if (end != null)
				{
					writer.Write(end.Size);
					writer.Write((byte)end.Style);
				}
				else
				{
					writer.Write(DEFAULTSIZE);
					writer.WriteByte(0);
				}
			}

			public static void Read(DataReader reader, out ArrowheadC start, out ArrowheadC end)
			{
				start = new ArrowheadC(false);
				end = new ArrowheadC(true);
				start.Size = reader.ReadInt32();
				start.Style = (Styles)reader.ReadByte();
				end.Size = reader.ReadInt32();
				end.Style = (Styles)reader.ReadByte();
			}

			public override void CopyFrom(StyleBase other)
			{
				ArrowheadC objArrowhead = (ArrowheadC)other;
				Size = objArrowhead.Size;
				Style = objArrowhead.Style;
				// m_bolEnd deliberately not copied
				Points = null;
			}

			internal override int ParameterValue(Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.ArrowheadStartSize:
					case Parameters.ArrowheadEndSize:
						return Size;
					case Parameters.ArrowheadStartType:
					case Parameters.ArrowheadEndType:
						return (int)Style;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						return 0;
				}
			}

			internal override void SetParameterValue(int value, Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.ArrowheadStartSize:
					case Parameters.ArrowheadEndSize:
						Size = value;
						break;
					case Parameters.ArrowheadStartType:
					case Parameters.ArrowheadEndType:
						Style = (Styles)value;
						break;
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						break;
				}
				Points = null;
			}

			public PointF[] Points = null; // to be reset to nothing whenever the cached values are no longer valid
			public void EnsurePoints(Lined shape, LineStyleC lineStyle)
			{
				if (Points != null || Style == Styles.None)
					return;
				PointF[] line = shape.ArrowheadVector(m_End);
				if (line == null)
					return;
				Debug.Assert(line.Length == 2);
				float length = Math.Min((int)Geometry.DistanceBetween(line[0], line[1]), Size * DEFAULTDRAWRATIO);
				// length from the tip of arrow to the base of the triangle
				float transverseRatio = 0.45F; // transverse half-size as a multiple of length
				int points = 3;
				switch (Style)
				{
					case Styles.Version4:
						transverseRatio = 0.7F;
						break;
					case Styles.SimpleSolid:
						break;
					case Styles.VMedium:
						break;
					case Styles.LongSolid:
						transverseRatio *= 0.5F;
						length *= 2;
						break;
					case Styles.DoubleSolid:
						points = 7; // need to include point where arrows join twice
						break;
					case Styles.Fletching:
						points = 6;
						length *= 1.5F;
						transverseRatio *= 0.7F; // longer, but not wider
						break;
					case Styles.RecessSolid:
						length *= 1.1F;
						points += 1;
						break;
					case Styles.MeasureBar:
						points = 2;
						break;
					default:
						Debug.Fail("Unexpected Arrowhead style: " + Style);
						break;
				}

				SizeF backward = line[1].VectorTo(line[0]).ChangeLength(length);
				if (lineStyle != null) // just checking in case this is used by some class which doesn't actually allow the line to be adjusted
				{
					length = length + lineStyle.Width + lineStyle.Width - Geometry.SINGLELINE;
					// makes the arrow head a little bit larger if the line is wide
					// and... need to move the arrow forwards slightly over the end of the line, otherwise the line can show thru (esp if large)
					line[1] += backward.ChangeLength(-lineStyle.Width * (Style == Styles.LongSolid ? 3.0F : 0.5F));
				}

				SizeF transverse = backward.Perpendicular(1).ChangeLength(length * transverseRatio);
				PointF[] newPoints = new PointF[points - 1 + 1];
				// we construct the default triangle first.  If the shape is very weird this can be replaced below
				PointF ptBase = line[1] + backward;
				newPoints[0] = ptBase + transverse;
				newPoints[1] = line[1];
				if (newPoints.Length >= 3)
					newPoints[2] = ptBase - transverse;
				switch (Style)
				{
					// for most shapes that was sufficient
					case Styles.Version4:
					case Styles.SimpleSolid:
					case Styles.VMedium:
					case Styles.LongSolid:
						break;
					case Styles.RecessSolid:
						newPoints[3] = ptBase + backward.MultiplyBy(-0.2f);
						break;
					case Styles.DoubleSolid:
						newPoints[3] = ptBase;
						newPoints[4] = newPoints[0] + backward;
						newPoints[5] = newPoints[2] + backward;
						newPoints[6] = newPoints[3];
						break;
					case Styles.Fletching:
						SizeF szOffset = backward.MultiplyBy(-0.2f); // make it slope toward 'end' of line (which is the start as far as user is concerned)
						newPoints[0] = line[1] + transverse + szOffset;
						newPoints[2] = line[1] - transverse + szOffset;
						newPoints[3] = newPoints[2] + backward;
						newPoints[4] = line[1] + backward;
						newPoints[5] = newPoints[0] + backward;
						break;
					case Styles.MeasureBar:
						newPoints[0] = line[1] + transverse;
						newPoints[1] = line[1] - transverse;
						break;
					default:
						Debug.Fail("Unexpected Arrowhead style: " + Style);
						return; // because drawing below will probably fail
				}
				Points = newPoints;
			}

			public void ExtendBounds(ref RectangleF bounds)
			{
				if (Points == null) return;
				foreach (PointF pt in Points)
				{
					Geometry.Extend(ref bounds, pt);
				}
			}

			private static Parameters[] Applicable = { Parameters.ArrowheadEndSize, Parameters.ArrowheadEndType };

			internal override Parameters[] ApplicableParameters()
			{
				return Applicable;
			}

			public override bool IdenticalTo(StyleBase other)
			{
				ArrowheadC arrowhead = (ArrowheadC)other;
				if (Size != arrowhead.Size)
					return false;
				if (Style != arrowhead.Style)
					return false;
				if (m_End != arrowhead.m_End)
					return false;
				return true;
			}
		}
		#endregion

		protected virtual PointF[] ArrowheadVector(bool end)
		{
			// Returned array must be 2 points.  The first is the position the arrow head is coming from (exact position not important, only the direction counts)
			// the second is the position of the head itself, i.e. the end of the line
			if (Vertices.Count < 2)
				return null;
			if (end)
				return new PointF[] { Vertices[Vertices.Count - 2], Vertices[Vertices.Count - 1] };
			return new PointF[] { Vertices[1], Vertices[0] };
		}

		// Will use objResources.MainPen if not filled.  If filled will create own brush, however will still only draw if objResources.MainPen is defined
		protected void DrawArrowheads(DrawResources resources)
		{
			if (resources.MainPen == null)
				return;
			Debug.Assert((Allows & AllowedActions.Arrowheads) > 0);
			if (StartArrowhead != null && StartArrowhead.Style != ArrowheadC.Styles.None)
			{
				StartArrowhead.EnsurePoints(this, LineStyle);
				DrawArrowhead(resources, StartArrowhead);
			}
			if (EndArrowhead != null && EndArrowhead.Style != ArrowheadC.Styles.None)
			{
				EndArrowhead.EnsurePoints(this, LineStyle);
				DrawArrowhead(resources, EndArrowhead);
			}
		}

		private void DrawArrowhead(DrawResources resources, ArrowheadC style)
		{
			// The points must already have been defined on the arrowhead
			// this function cannot be part of ArrowheadC because DrawResources is protected
			if (style.Points == null)
				return;

			bool solid = false;
			bool closed = false; // is automatically set if bolSolid is set
			switch (style.Style)
			{
				case ArrowheadC.Styles.SimpleSolid:
					solid = true;
					break;
				// not including a Case Else with assertion here, because most can be omitted
				case ArrowheadC.Styles.DoubleSolid:
				case ArrowheadC.Styles.Fletching:
				case ArrowheadC.Styles.LongSolid:
				case ArrowheadC.Styles.RecessSolid:
					solid = true;
					break;
			}
			if (solid)
				closed = true;

			using (GraphicsPath create = new GraphicsPath())
			{
				create.StartFigure();
				create.AddLines(style.Points);
				if (closed)
					create.CloseAllFigures();
				if (solid)
				{
					using (var br = resources.Graphics.CreateFill(resources.MainPen.Color))
					{
						resources.Graphics.Path(create, null, br);
					}
				}
				else
					resources.Graphics.Path(create, resources.MainPen);
			}

		}

		protected void DecacheArrowhead(bool end)
		{
			if (!end)
			{
				if (StartArrowhead != null)
					StartArrowhead.Points = null;
			}
			else
			{
				if (EndArrowhead != null)
					EndArrowhead.Points = null;
			}
		}

		protected void DecacheArrowheads()
		{
			// as above, but does both in one function call
			if (StartArrowhead != null)
				StartArrowhead.Points = null;
			if (EndArrowhead != null)
				EndArrowhead.Points = null;
		}

		protected void ExtendBoundsToIncludeArrowhead(ref RectangleF bounds)
		{
			// usually called within CalculateBounds, so the rectangle calculated so far is included
			if (StartArrowhead != null)
			{
				StartArrowhead.EnsurePoints(this, LineStyle);
				StartArrowhead.ExtendBounds(ref bounds);
			}
			if (EndArrowhead != null)
			{
				EndArrowhead.EnsurePoints(this, LineStyle);
				EndArrowhead.ExtendBounds(ref bounds);
			}
		}

		#endregion

	}

}
