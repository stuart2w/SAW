using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SAW.Shapes
{
	/// <summary>Text which is created based upon a baseline.  The base class's Label is used for the text </summary>
	public class TextLine : Shape
	{

		protected PointF[] Vertices = new PointF[2];

		public TextLine()
		{ }

		public TextLine(RectangleF bounds, string text):this(bounds.Location, bounds.TopRight(), text)
		{ }

		public TextLine(PointF baseStart, PointF baseEnd, string text)
		{
			Vertices[0] = baseStart;
			Vertices[1] = baseEnd;
			m_BaseLineFixed = true;
			TextStyle.SetDefaults();
			Status = StatusValues.Complete;
			m_Label = text;
		}

		#region Basics

		public override Shapes ShapeCode => Shapes.TextLine;
		internal override LabelModes LabelMode => LabelModes.Always;
		protected override LabelPositions LabelPosition => LabelPositions.TextShape;
		public override AllowedActions Allows => (base.Allows & ~AllowedActions.Merge) | AllowedActions.Tidy;

		public override GeneralFlags Flags
		{
			get
			{
				GeneralFlags flags = base.Flags;
				if (!string.IsNullOrEmpty(m_Label) && m_BaseLineFixed)
					flags = flags | GeneralFlags.StoreIfAbandoned;
				if (!m_BaseLineFixed)
					flags = flags | GeneralFlags.NoEmptyLabelMessage;
				return flags;
			}
		}

		internal override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			switch (parameter)
			{
				case Parameters.TextVerticalAlignment:
					// must block access to the vertical alignment which is not supported here
					return null;
				case Parameters.LineColour:
					return TextStyle; // allow LineColour to control text colour
				default:
					return base.StyleObjectForParameter(parameter, applyingDefault);
			}
		}

		internal override bool DefaultStylesApplied()
		{
			if (TextStyle.Alignment != StringAlignment.Near)
			{
				TextStyle.Alignment = StringAlignment.Near;
				return true;
			}
			return false;
		}
		
		private bool m_LastPromptWasEmpty;

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (!m_BaseLineFixed)
			{
				list.Add(new Prompt(ShapeVerbs.Choose, "TextLine_Choose", "TextLine_Choose"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelAll", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Type, "TextLine_Type", "TextLine_Type"));
				list.Add(new Prompt(ShapeVerbs.Type, "TextLine_Flow", "TextLine_Flow"));
				if (!string.IsNullOrEmpty(m_Label))
					list.Add(new Prompt(ShapeVerbs.Complete, "TextLine_Finish", "TextLine_Finish"));
				m_LastPromptWasEmpty = string.IsNullOrEmpty(m_Label);
			}
			return list;
		}

		#endregion

		#region Verbs

		protected bool m_BaseLineFixed = true;
		public override VerbResult Start(ClickPosition position)
		{
			PointF pt = position.Snapped;
			Vertices[0] = pt;
			Vertices[1] = pt;
			m_BaseLineFixed = false;
			Debug.Assert(TextStyle != null);
			TextStyle.SetDefaults();
			TextStyle.Alignment = StringAlignment.Near;
			return VerbResult.Continuing;
		}

		public override VerbResult Float(ClickPosition position)
		{
			PointF pt = position.Snapped;
			if (!m_BaseLineFixed)
			{
				if (pt.ApproxEqual(Vertices[0]))
					return VerbResult.Rejected;
				Vertices[1] = pt;
				m_Bounds = CalculateBounds();
				// otherwise we are still typing text, nothing actually moves
				ClearTextCache();
			}
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (!m_BaseLineFixed)
				return VerbResult.Destroyed;
			if (!string.IsNullOrEmpty(m_Label))
				return Complete(position);
			m_BaseLineFixed = false;
			CaretLose();
			return VerbResult.Continuing;
		}

		public override VerbResult Choose(ClickPosition position)
		{
			if (!m_BaseLineFixed)
			{
				if (Float(position) == VerbResult.Rejected)
					return VerbResult.Rejected;
				m_BaseLineFixed = true;
				if (m_Label == null)
					m_Label = "";
				FormatText(0); // will format the EmptyText prompt
				CaretGain();
				m_Bounds = CalculateBounds();
				return VerbResult.Continuing;
			}
			else // will complete the shape, fixing the text
			{
				if (string.IsNullOrEmpty(m_Label))
					return VerbResult.Rejected;
				CaretLose();
				return VerbResult.Completed;
			}
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (!m_BaseLineFixed)
				return Choose(position);
			return CompleteRetrospective();
		}

		public override VerbResult CompleteRetrospective()
		{
			if (string.IsNullOrEmpty(m_Label))
				return VerbResult.Rejected;
			m_BaseLineFixed = true;
			CaretLose();
			return VerbResult.Completed;
		}

		protected internal override VerbResult CombinedKey(Keys key, char ch, Page page, bool simulated, EditableView fromView)
		{
			// ignore the key if we don't have a cursor (otherwise this shape will swallow all sorts of functional keys (e.g. shape selection) whenever it is selected)
			if (!HasCaret)
				return VerbResult.Unexpected;
			if (!m_BaseLineFixed)
			{
				Utilities.LogSubError("TextLine has caret without base line fixed");
				return VerbResult.Unexpected;
			}
			VerbResult result = base.CombinedKey(key, ch, page, simulated, fromView);
			if (ch != '\0')
			{
				m_Bounds = CalculateBounds();
				if (m_LastPromptWasEmpty != string.IsNullOrEmpty(m_Label))
					Parent.NotifyIndirectChange(this, ChangeAffects.UpdatePrompts);
			}
			return result;
		}

		#endregion

		#region Data

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			TextLine textLine = (TextLine)other;
			Vertices[0] = textLine.Vertices[0];
			Vertices[1] = textLine.Vertices[1];
			ClearTextCache();
		}

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_BaseLineFixed = true; // must be in order to store in data
			Vertices = reader.ReadListPoints().ToArray();
			reader.ReadSingle(); // m_sngTextHeight = ' no longer needed
			ClearTextCache();
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(Vertices);
			// no longer relevant: m_TextHeight
			writer.Write(8);
		}

		protected internal override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			output.Append("V0: ");
			PointDiagnostic(output, Vertices[0]);
			output.Append("V1: ");
			PointDiagnostic(output, Vertices[1]);
		}

		#endregion

		#region Graphics

		protected override void PrepareHighlight(DrawResources resources)
		{
			base.PrepareHighlight(resources);
			// note because this is a TextShape (LabelPosition) the standard PrepareDraw doesn't set a dotted TextBoundsPen
			resources.TextBoundsPen = resources.Graphics.CreateStroke(CurrentHighlightColour, resources.HIGHLIGHTEXTRAWIDTH);
		}

		protected override void DrawLabel(Canvas gr, DrawResources resources)
		{
			if (resources.TextBrush != null || resources.CustomPen != null)
			{
				EnsureFont(resources.CoordScale);
				base.DrawNewText(gr, resources, RectangleF.Empty); // empty because internal draw will draw the highlight border
			}
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (!m_BaseLineFixed)
			{
				// need to draw the baseline
				using (Stroke pn = gr.CreateStroke(Color.Gray, Geometry.THINLINE))
				{
					gr.DrawLine(Vertices[0], Vertices[1], pn);
				}

			}
			else if (resources.TextBoundsPen != null)
			{
				// calculate bounding rect in text coords
				RectangleF rct = new RectangleF(0, 0, Geometry.DistanceBetween(Vertices[0], Vertices[1]), 0);
				if (m_Fragments != null && m_Fragments.Count > 0)
					rct.Height = m_Fragments[m_Fragments.Count - 1].Bounds.Bottom;
				base.DrawNewText(gr, resources, rct); // will just draw the bounding rect (since the text brush should be null)
			}
		}

		#endregion

		#region Other coordinates
		protected override RectangleF CalculateBounds()
		{
			if (!m_BaseLineFixed)
			{
				RectangleF total = new RectangleF(Vertices[0], SizeF.Empty);
				Geometry.Extend(ref total, Vertices[1]);
				return total;
			}
			else
			{
				// base would use text - but we need to include the corners for the highlighter
				EnsureTextFormatted();
				float height = 0;
				if (m_Fragments != null && m_Fragments.Count > 0)
					height = m_Fragments[m_Fragments.Count - 1].Bounds.Bottom - m_Fragments[0].Bounds.Top;
				RectangleF rct = new RectangleF();
				Geometry.Extend(ref rct, Vertices[0]);
				Geometry.Extend(ref rct, Vertices[1]);
				if (height > 0)
				{
					SizeF sz = Vertices[0].VectorTo(Vertices[1]).Perpendicular(1); // down vector
					sz = sz.ChangeLength(height);
					Geometry.Extend(ref rct, Vertices[0] + sz);
					Geometry.Extend(ref rct, Vertices[1] + sz);
				}
				return rct;
			}
		}

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => base.TextHitTest(clickPoint);

		internal override void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			base.NotifyStyleChanged(parameter, oldValue, newValue); // at the moment this call is redundant as all it does is sometimes clear the bounding rectangle
																	// but for consistency should be included
			if (parameter == Parameters.FontSize) // also used if font itself is changed
			{
				m_Bounds = RectangleF.Empty;
				// we also update the length of the baseline in proportion to the change in the font
				SizeF vector = Vertices[0].VectorTo(Vertices[1]);
				vector = vector.MultiplyBy((float)newValue / oldValue);
				Vertices[1] = Vertices[0] + vector;
				// moving vertex will have moved any GrabSpots
				FormatText();
				Parent.NotifyIndirectChange(this, ChangeAffects.GrabSpots);
			}
		}

		protected internal override PointF DoSnapAngle(PointF newPoint) => Geometry.AngleSnapPoint(newPoint, Vertices[0]);

		public override bool Tidy(SnapModes mode, Page page)
		{
			List<PointF> vertices = new List<PointF>(Vertices); // function below needs a list not an array
			bool changed = Lined.TidyVertices(vertices, this, mode, page, 1);
			if (changed)
			{
				Vertices = vertices.ToArray();
				m_Bounds = RectangleF.Empty;
				ClearTextCache();
			}
			return changed;
		}

		protected override float TextAllowedWidth => Geometry.DistanceBetween(Vertices[0], Vertices[1]);

		protected override void SetTextTransforms()
		{
			if (Vertices[0].ApproxEqual(Vertices[1]))
				base.SetTextTransformsImpl(Vertices[0], Geometry.ANGLE90); // use horizontal (asserts getting angle for 0 length)
			else
				base.SetTextTransformsImpl(Vertices[0], Geometry.VectorAngle(Vertices[0], Vertices[1]));
		}

		#endregion

		#region Grabs and transforms

		internal override List<GrabSpot> GetGrabSpots(float scale)
		{
			List<GrabSpot> list = new List<GrabSpot>();
			list.Add(new GrabSpot(this, GrabTypes.Fixed, Vertices[0], 0));
			list.Add(new GrabSpot(this, GrabTypes.SingleVertex, Vertices[1], 1));
			return list;
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			switch (move.GrabType)
			{
				case GrabTypes.SingleVertex:
					PointF ptNew = Vertices[1];
					move.Transform.TransformPoint(ref ptNew);
					if (ptNew.ApproxEqual(Vertices[0]))
						return;
					if (HasCaret)
						CaretRealculateCoordinates(false);
					Vertices[1] = ptNew;
					ClearTextCache();
					m_Bounds = CalculateBounds();
					break;
				default:
					base.DoGrabMove(move);
					break;
			}
		}

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			if (move.GrabType != GrabTypes.SingleVertex)
				base.DoGrabAngleSnap(move);
			else
				move.Current.Snapped = Geometry.AngleSnapPoint(move.Current.Exact, Vertices[0]);
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformPoint(ref Vertices[0]);
			transformation.TransformPoint(ref Vertices[1]);
			ClearTextCache();
		}

		#endregion

	}

	internal class FreeText : Shape
	{
		// not really used in SAW at this time.  Left in the code for the moment.

		#region Basics

		public override Shapes ShapeCode => Shapes.FreeText;
		internal override LabelModes LabelMode => LabelModes.Always;
		protected override LabelPositions LabelPosition => LabelPositions.TextShape;

		public override GeneralFlags Flags
		{
			get
			{
				if (!string.IsNullOrEmpty(m_Label))
					return GeneralFlags.StoreIfAbandoned | base.Flags | GeneralFlags.NoEmptyLabelMessage;
				return base.Flags | GeneralFlags.NoEmptyLabelMessage;
			}
		}

		internal override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			switch (parameter)
			{
				case Parameters.TextVerticalAlignment:
					// must block access to the vertical alignment which is not supported here
					return null;
				case Parameters.LineColour:
					return TextStyle; // allow LineColour to control text colour
				default:
					return base.StyleObjectForParameter(parameter, applyingDefault);
			}
		}

		private bool m_LastPromptWasEmpty;

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			list.Add(new Prompt(ShapeVerbs.Type, "FreeText_Type", "FreeText_Type"));
			list.Add(new Prompt(ShapeVerbs.Type, "FreeText_Return", "FreeText_Enter"));
			if (!string.IsNullOrEmpty(m_Label))
				list.Add(new Prompt(ShapeVerbs.Complete, "FreeText_Finish", "FreeText_Finish"));
			m_LastPromptWasEmpty = string.IsNullOrEmpty(m_Label);
			return list;
		}

		internal override string LabelText
		{
			get { return base.LabelText; }
			set
			{
				base.LabelText = value;
				m_Bounds = RectangleF.Empty;
			}
		}

		public override AllowedActions Allows => base.Allows & ~(AllowedActions.TransformMirror | AllowedActions.TransformRotate);

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			if (requested == SnapModes.Angle)
				requested = SnapModes.Off;
			return requested;
		}

		#endregion

		#region Graphics

		protected override void PrepareHighlight(DrawResources resources)
		{
			base.PrepareHighlight(resources);
			resources.TextBoundsPen = resources.Graphics.CreateStroke(CurrentHighlightColour, resources.HIGHLIGHTEXTRAWIDTH);
			resources.TextBoundsPen.LineJoin = LineJoin.Round; // otherwise points can stick out for miles, busting the refreshbounds
		}

		protected override void DrawLabel(Canvas gr, DrawResources resources)
		{
			if (resources.TextBrush != null)
			{
				EnsureFont(resources.CoordScale);
				DrawNewText(gr, resources, RectangleF.Empty);
			}
		}

		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			if (resources.TextBoundsPen != null) // used for the highlight
				gr.Rectangle(Bounds, resources.TextBoundsPen);
		}

		#endregion

		#region Verbs
		public override VerbResult Start(ClickPosition position)
		{
			m_StartPoint = position.Snapped;
			if (position.SnapMode == SnapModes.Grid)
			{
				// The normal snapping would leave the text centred on the horizontal line, but we would rather than the text is drawn approximately within the cells of the grid
				// therefore it needs to be moved up or down.  We don't need to recalculate the snapping from scratch; we can see which way to move by whether the exact point is above or below the snapped point
				var interval = position.Page.Paper.YInterval;
				if (position.Exact.Y < position.Snapped.Y)
					m_StartPoint.Y -= interval / 2;
				else
					m_StartPoint.Y += interval / 2;
			}
			Debug.Assert(TextStyle != null);
			TextStyle.SetDefaults();
			TextStyle.Alignment = StringAlignment.Near;
			m_Label = "";
			CaretGain();
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (!string.IsNullOrEmpty(m_Label))
				return Complete(position);
			return VerbResult.Destroyed;
		}

		private FreeText m_Spawn;
		public override VerbResult Choose(ClickPosition position)
		{
			// can be moved if no text yet.  However if the focus was not in the drawing area, we just ignore it
			// the click will have just moved the focus back into the drawing area
			//Debug.WriteLine("f: " + objPosition.WasFocused.ToString)
			if (!position.WasFocused)
				return VerbResult.Continuing;
			if (string.IsNullOrEmpty(m_Label))
			{
				m_StartPoint = position.Snapped;
				m_Bounds = CalculateBounds();
				return VerbResult.Continuing;
			}
			else
			{
				m_Spawn = new FreeText();
				m_Spawn.Start(position);
				m_Spawn.TextStyle.CopyFrom(this.TextStyle);
				m_Spawn.CaretGain();
				return VerbResult.Spawn;
			}
		}

		protected internal override Shape Spawn()
		{
			FreeText obj = m_Spawn;
			m_Spawn = null;
			return obj;
		}

		public override VerbResult Complete(ClickPosition position) => CompleteRetrospective();

		public override VerbResult CompleteRetrospective()
		{
			if (string.IsNullOrEmpty(m_Label))
				return VerbResult.Destroyed;
			CaretLose();
			return VerbResult.Completed;
		}

		public override VerbResult Float(ClickPosition position) => VerbResult.Unchanged;

		protected internal  override VerbResult CombinedKey(Keys key, char ch, Page page, bool simulated, EditableView fromView)
		{
			// ignore the key if we don't have a cursor (otherwise this shape will swallow all sorts of functional keys (e.g. shape selection) whenever it is selected)
			switch (key)
			{
				case Keys.Escape:
					return CompleteRetrospective();
			}
			if (!HasCaret)
				return VerbResult.Unexpected;
			var eResult = base.CombinedKey(key, ch, page, simulated, fromView);
			if (m_LastPromptWasEmpty != string.IsNullOrEmpty(m_Label))
				Parent.NotifyIndirectChange(this, ChangeAffects.UpdatePrompts);
			return eResult;
		}

		#endregion

		#region Coordinates
		private PointF m_StartPoint;
		/// <summary>Height of first line; used for V centre.  Not stored in data, and is approximated before text is created.  Set by PlaceFragment(0)</summary>
		private float m_LineHeight;

		protected override RectangleF CalculateBounds()
		{
			EnsureTextFormatted();
			return base.TextRectangleToPageRectangleContaining(CalculateTextBounds());
		}

		private void SetDefaultLineHeight()
		{
			EnsureFont();
			m_LineHeight = NetCanvas.MeasurementInstance.MeasureString("Ay", m_Font, format: GUIUtilities.StringFormatCentreLeft).Height;
		}

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled) => true;  // anything in Bounds rect is a hit

		public override void ApplyTransformation(Transformation transformation)
		{
			base.ApplyTransformation(transformation);
			transformation.TransformPoint(ref m_StartPoint);
			// just clearing m_szTextWithoutCaret and m_Bounds causes problems at the end of a move transformation when the bounds is used
			// to calculate the clipping area, but can only be set after the drawing starts and the graphics is available
			if (!m_Bounds.IsEmpty)
				transformation.TransformRectangle(ref m_Bounds); // the second parameter can be ignored; we don't want to invert the text
		}

		internal override void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			base.NotifyStyleChanged(parameter, oldValue, newValue);
			// at the moment this call is redundant as all it does is sometimes clear the bounding rectangle but for consistency should be included
			if (parameter == Parameters.FontSize) // also used if font itself is changed
			{
				m_Bounds = RectangleF.Empty;
				Parent.NotifyIndirectChange(this, ChangeAffects.GrabSpots);
			}
			else if (parameter == Parameters.TextAlignment)
				m_Bounds = CalculateBounds();
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			RectangleF rctBounds = Bounds; // shadow never used, so won't bother with the base class
			rctBounds.Inflate(1, 1);
			return rctBounds;
		}

		internal override List<GrabSpot> GetGrabSpots(float scale) => new List<GrabSpot>();

		protected override void SetTextTransforms()
		{
			// first line is centred
			if (m_LineHeight == 0)
				SetDefaultLineHeight();
			base.SetTextTransformsImpl(new PointF(m_StartPoint.X, m_StartPoint.Y - m_LineHeight / 2));
		}

		protected override float TextAllowedWidth => 10000;  // doesn't wrap

		protected override bool PlaceFragment(Fragment newFragment, float extraLineSpace = 0)
		{
			Debug.Assert(extraLineSpace == 0); // not relevant to this type (only SAW atm)
											   // bit different to usual - not placing within a rect; instead all placed around 0-point on X axis
			if (newFragment.Index == 0)
			{
				newFragment.Bounds.Location = new PointF(0, 0); // technically redundant as it would have been initialised to this anyway
				if (m_LineHeight != newFragment.Bounds.Height)
				{
					m_LineHeight = newFragment.Bounds.Height;
					ClearTextCache(false);
				}
			}
			else
			{
				// default is to go below the last existing fragment
				newFragment.Bounds.Y = m_Fragments[newFragment.Index - 1].Bounds.Bottom;
			}
			// X position depends on alignment
			switch (TextStyle.Alignment)
			{
				case StringAlignment.Center:
					newFragment.Bounds.X = -newFragment.Bounds.Width / 2;
					break;
				case StringAlignment.Near:
					newFragment.Bounds.X = 0;
					break;
				case StringAlignment.Far:
					newFragment.Bounds.X = -newFragment.Bounds.Width;
					break;
				default:
					throw new ArgumentException("Invalid TextStyle.Alignment: " + TextStyle.Alignment);
			}
			return true;
		}

		protected override RectangleF TextRectangleToPageRectangleContaining(RectangleF rct)
		{
			if (m_LineHeight == 0)
				SetDefaultLineHeight();
			PointF TL = new PointF(m_StartPoint.X, m_StartPoint.Y - m_LineHeight / 2);
			rct.Offset(TL);
			return rct;
		}

		public override GraphicsPath ConvertToPath()
		{ // not actually used atm;  not working
			GraphicsPath path = new GraphicsPath();
			foreach (var fragment in m_Fragments)
			{
				path.AddString(fragment.Text, TextStyle.GetFontFamily(), 0, TextStyle.Size, fragment.Bounds.Location, StringFormat.GenericDefault);
			}
			return path;
		}

		#endregion

		#region Data
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			FreeText freeText = (FreeText)other;
			m_StartPoint = freeText.m_StartPoint;
			m_Bounds = freeText.m_Bounds;
		}

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			m_StartPoint = reader.ReadPointF();
			reader.ReadSizeF(); //m_szTextWithoutCaret =
			m_Fragments = null;
			ClearTextCache();
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(m_StartPoint);
			writer.Write(SizeF.Empty); // ignored completely now
		}

		#endregion

	}

}
