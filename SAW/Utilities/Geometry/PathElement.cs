using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Linq;


namespace SAW
{
	/// <summary>Use WithinPath to iterate the elements within a GraphicsPath.  Also a few other shared utility functions</summary>
	internal struct PathElement
	{
		/// <summary>Will only be PATHLINE or PATHBEZIER and without any flags</summary>
		public readonly byte Type;
		/// <summary>Same array will usually be reused in multiple elements and maybe longer than necessary (probably long enough for a Bezier even give it is a line)</summary>
		public readonly PointF[] Points;
		/// <summary>True is this (must be a line) is generated due to the closure flag</summary>
		public readonly bool IsClosure;
		/// <summary>True if the start of this element is the start of a figure</summary>
		public readonly bool IsStart;
		/// <summary>Index at start of this element</summary>
		public readonly int Index;

		internal PathElement(byte type, PointF[] points, int index, bool isClosure, bool isStart)
		{
			Type = type;
			Points = points;
			Index = index;
			IsClosure = isClosure;
			IsStart = isStart;
			Utilities.ErrorAssert(type == Lined.PATHLINE || type == Lined.PATHBEZIER || type == 255 && points == null); // last for Invalid only
		}

		internal PathElement(int type, PointF[] points, int index, bool isClosure, bool isStart)
			: this((byte)type, points, index, isClosure, isStart)
		{
		}

		/// <summary>Can be used to initialise the variable where required</summary>
		public static PathElement Invalid = new PathElement(255, null, -1, false, false);

		/// <summary>Iterates the elements within the past.  If ignoreShortClosure then the closure is not reported as a line if it is zero length</summary>
		public static IEnumerable<PathElement> WithinPath(GraphicsPath path, bool ignoreShortClosure = false, bool dontReusePointsArray = false)
		{
			// There may be degenerate closure segments which are not entirely spurious. If the shape contains curves but is closed, a line would be reported
			// between the end of the last curve and beginning of the first which are actually in the same point.  In some cases, such as intersection checking
			// this will, at best, create assertions
			return new PathEnumerator(path, ignoreShortClosure, dontReusePointsArray);
		}

		#region Derived Properties
		/// <summary>Last point in this element.  Not necessarily Points.Last as Points may be excessively long</summary>
		public PointF Endpoint
		{ get { return Points[Type == Lined.PATHLINE ? 1 : 3]; } }

		/// <summary>Last index within the path data used by this</summary>
		public int EndIndex
		{ get { return Index + (Type == Lined.PATHLINE ? 1 : 3); } }

		public bool IsLine
		{ get { return Type == Lined.PATHLINE; } }

		public bool IsBezier
		{ get { return Type == Lined.PATHBEZIER; } }

		/// <summary>True if this is zero length.  Usually used with closure line elements, but valid on any</summary>
		public bool IsDegenerate
		{
			get
			{
				if (!Points[0].ApproxEqual(Points[1]))
					return false;
				if (Type == Lined.PATHLINE)
					return true;
				return Points[3].ApproxEqual(Points[1]); // doesn't bother checking that the other grab handle is in the same place.  I think it pretty much must be
			}
		}
		#endregion

		#region Miscellaneous Shared utility functions
		/// <summary>Returns index of first point in figure containing point</summary>
		public static int FindFigureStart(GraphicsPath path, int point)
		{
			byte[] types = path.PathTypes;
			while (point > 0 && (types[point] & Lined.PATHTYPEMASK) != Lined.PATHSTART)
			{
				// no need to jump 3 for a bezier - all 3 points in a bezier have a type code, so can jus iterate one at a time - simple as that
				point -= 1;
			}
			return point;
		}

		/// <summary>Returns the number of lines/curves in the given figure.  figureStart must be the index of the first point in the figure.
		/// This does not include any closure line; it is the number of "vertices"</summary>
		public static int CountElementsInFigure(byte[] types, int figureStart)
		{
			Debug.Assert((types[figureStart] & Lined.PATHTYPEMASK) == Lined.PATHSTART);
			int count = 0;
			do
			{
				count += 1;
				switch (types[figureStart + 1] & Lined.PATHTYPEMASK)
				{
					case Lined.PATHSTART:
						return count - 1; // -1 because the +1 above would be the line to this point, which is inappropriate
					case Lined.PATHLINE:
						figureStart += 1;
						break;
					case Lined.PATHBEZIER:
						figureStart += 3;
						break;
				}
			} while (!(figureStart >= types[figureStart] - 1));
			return count;
		}

		/// <summary>Returns index of first point in figure containing point</summary>
		public static int FindFigureStart(byte[] types, int point)
		{
			while (point > 0 && (types[point] & Lined.PATHTYPEMASK) != Lined.PATHSTART)
				// no need to jump 3 for a bezier - all 3 points in a bezier have a type code, so can jus iterate one at a time - simple as that
				point -= 1;
			return point;
		}

		/// <summary>Returns the index of the last point in the path containing point (usually starting with point, but will work as long as point is the beginning of any segment)
		/// Returned point is the last element in PathPoints, and may not be at the start of a curve</summary>
		public static int FindFigureEnd(GraphicsPath path, int point)
		{
			byte[] aTypes = path.PathTypes;
			while (point < aTypes.Length - 1 && (aTypes[point + 1] & Lined.PATHTYPEMASK) != Lined.PATHSTART)
			{
				point += 1;
			}
			return point;
		}

		/// <summary>Returns the index of the last point in the path containing point (usually starting with point, but will work as long as point is the beginning of any segment)
		/// Returned point is the last element in PathPoints, and may not be at the start of a curve</summary>
		public static int FindFigureEnd(byte[] types, int point)
		{
			while (point < types.Length - 1 && (types[point + 1] & Lined.PATHTYPEMASK) != Lined.PATHSTART)
			{
				point += 1;
			}
			return point;
		}

		/// <summary>Returns a single element with the same information as the iterator.  Index must be the beginning of the element (or the point with the closure flag)</summary>
		public static PathElement GetElement(GraphicsPath path, int index)
		{
			// Approximately copied from the iterator Current, but changed a bit to find the figure start as needed
			byte[] types = path.PathTypes;
			PointF[] points = path.PathPoints;
			PointF[] resultPoint = new PointF[4];
			if ((types[index] & Lined.PATHCLOSUREFLAG) > 0)
			{
				// At the beginning of a closure line
				int start = index - ((types[index] & Lined.PATHTYPEMASK) == Lined.PATHLINE ? 1 : 3); // the start of this element
				start = FindFigureStart(types, start); // The start of the figure
				resultPoint[0] = points[index];
				resultPoint[1] = points[start];
				return new PathElement(Lined.PATHLINE, resultPoint, index, true, false);
			}
			else
			{
				int type = types[index + 1] & Lined.PATHTYPEMASK;
				resultPoint[0] = points[index];
				resultPoint[1] = points[index + 1];
				if (type == Lined.PATHBEZIER)
				{
					resultPoint[2] = points[index + 2];
					resultPoint[3] = points[index + 3];
				}
				int start = FindFigureStart(types, index);
				return new PathElement(type, resultPoint, index, false, index == start);
			}
		}

		/// <summary>Creates a GraphicsPath from this set of elements.  Doesn't handle closures at the moment.  Just appends all the points with the correct type bytes
		/// Will add straight lines between elements if they do not connect</summary>
		public static GraphicsPath CreatePath(IEnumerable<PathElement> elements)
		{
			List<PointF> points = new List<PointF>();
			List<byte> types = new List<byte>();
			points.Add(elements.First().Points[0]);
			types.Add(Lined.PATHSTART);
			foreach (PathElement e in elements)
			{
				if (!e.Points[0].ApproxEqual(points.Last()))
				{ // items are not continuous - add line between them
					points.Add(e.Points[0]);
					types.Add(Lined.PATHLINE);
				}
				points.Add(e.Points[1]);
				if (e.IsBezier)
				{
					points.Add(e.Points[2]);
					points.Add(e.Points[3]);
					types.Add(Lined.PATHBEZIER);
					types.Add(Lined.PATHBEZIER);
					types.Add(Lined.PATHBEZIER);
				}
				else
					types.Add(Lined.PATHLINE);
			}
			return new GraphicsPath(points.ToArray(), types.ToArray());
		}

		#endregion

		#region Iterator
		private class PathEnumerator : IEnumerator<PathElement>, IEnumerable<PathElement>
		{

			private readonly PointF[] m_Points;
			private readonly byte[] m_Types;
			private int m_Index;
			private PointF[] m_ResultPoint = new PointF[4]; // same array used full results, to avoid creating lots of tiny objects
			private readonly bool m_IgnoreShortClosure;
			private int m_FigureStart; // first index at the beginning of the current figure
			private readonly bool m_DontReusePointsArray;

			public PathEnumerator(GraphicsPath path, bool ignoreShortClosure, bool dontReusePointsArray)
			{
				m_Points = path.PathPoints;
				m_Types = path.PathTypes;
				m_Index = -1;
				m_IgnoreShortClosure = ignoreShortClosure;
				m_DontReusePointsArray = dontReusePointsArray;
			}

			public PathElement Current
			{
				get
				{
					if ((m_Types[m_Index] & Lined.PATHCLOSUREFLAG) > 0)
					{
						// At the beginning of a closure line
						Debug.Assert(!m_Points[m_Index].ApproxEqual(m_Points[m_FigureStart]) || !m_IgnoreShortClosure);
						m_ResultPoint[0] = m_Points[m_Index];
						m_ResultPoint[1] = m_Points[m_FigureStart];
						return new PathElement(Lined.PATHLINE, m_ResultPoint, m_Index, true, false);
					}
					else
					{
						int type = m_Types[m_Index + 1] & Lined.PATHTYPEMASK;
						m_ResultPoint[0] = m_Points[m_Index];
						m_ResultPoint[1] = m_Points[m_Index + 1];
						if (type == Lined.PATHBEZIER)
						{
							m_ResultPoint[2] = m_Points[m_Index + 2];
							m_ResultPoint[3] = m_Points[m_Index + 3];
						}
						return new PathElement(type, m_ResultPoint, m_Index, false, m_Index == m_FigureStart);
					}
				}
			}

			object IEnumerator.Current => Current;

			public bool MoveNext()
			{
				if (m_DontReusePointsArray)
					m_ResultPoint = new PointF[4]; // does reassign even if size unchanged m_Points = Array.CreateInstance(GetType(PointF), 4) ' New PointF() {PointF.Empty, PointF.Empty, PointF.Empty, PointF.Empty}
				if (m_Index < 0)
				{
					m_Index = 0; // Because Reset resets to a position BEFORE the first item
					m_FigureStart = 0;
					return true;
				}
				if (m_Index >= m_Points.Length - 1)
					return false; // off end.  N-1 is the start of the closure line, so there cannot possibly be anything after it
				if ((m_Types[m_Index] & Lined.PATHCLOSUREFLAG) > 0)
				{
					// Currently at the closure at the end of a figure
					m_Index += 1;
					m_FigureStart = m_Index;
					Debug.Assert(m_Types[m_Index] == Lined.PATHSTART);
				}
				else
				{
					int type = m_Types[m_Index + 1]; // checks the subsequent point, because I think the first point in the shape is type 0 regardless
					switch (type & Lined.PATHTYPEMASK)
					{
						case Lined.PATHLINE:
							m_Index += 1;
							break;
						case Lined.PATHBEZIER:
							m_Index += 3;
							break;
						default:
							Utilities.ErrorAssert(false, "Unexpected PolyPath path type");
							return false; // because we don't know how much to increment index
					}
				}
				if (m_Index == m_Points.Length - 1 || (m_Types[m_Index + 1] & Lined.PATHTYPEMASK) == Lined.PATHSTART)
				{
					// At the end of a figure: either at the last point, or the next point is marked as StartFigure
					if ((m_Types[m_Index] & Lined.PATHCLOSUREFLAG) > 0 && (!m_Points[m_Index].ApproxEqual(m_Points[m_FigureStart]) || !m_IgnoreShortClosure)) // Caller requested not to be given degenerate closure
																																							  // We have a closure back to the beginning, and have excluded the possibility of it being degenerate if the caller requested them to be omitted
						return true;
					else
					{
						if (m_Index == m_Points.Length - 1)
							return false;
						// Otherwise move to the beginning of the new figure
						m_Index += 1;
						m_FigureStart = m_Index;
					}
				}
				return true;
			}

			public void Reset()
			{
				m_Index = -1;
			}

			public void Dispose()
			{
			}

			#region Implements IEnumerable in order to return same object from PathElement.WithinElement

			IEnumerator<PathElement> IEnumerable<PathElement>.GetEnumerator()
			{
				return this;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this;
			}

			#endregion

		}
		#endregion

	}
}
