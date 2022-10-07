using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Drawing2D;

// if adding, update Shape.TransformationType and related
namespace SAW.Shapes
{
	// transformation are implemented as virtual shapes.  In Splash these can be used as tools which modify the existing content rather than being added to the page themselves.
	// in SAW this functionality isn't used, but the remainder of the transformation logic is applicable

	public abstract class Transformation : Shape
	{
		// base class for all of the transformations.  This actually does most of the work; the individual versions mostly just need to set the matrix
		// can also be used without verbs to apply changes when user (eg) drags/drops.

		protected PointF m_Initial = PointF.Empty; // the start point of the transformation.  Some transformations require further points
		protected Matrix m_Transformation = new Matrix();

		public static bool CopyMode;

		protected Transformation(Modes mode)
		{
			if (mode == Modes.UseSystem)
				m_Mode = CopyMode ? Modes.Copy : Modes.Move;
			else
				m_Mode = mode;
		}

		#region List of affected shapes
		// we keep both old and new copies of the shapes, linked through a private class
		protected class Victim
		{
			public readonly Shape Original;
			public Shape Transformed;

			public Victim(Shape original)
			{
				Original = original;
				Transformed = (Shape)original.Clone(Mapping.Ignore);
			}

		}

		protected List<Victim> m_Shapes = new List<Victim>();

		protected void NewCopy()
		{
			Debug.Assert(m_Mode == Modes.Copy);
			// this can be called to make multiple copies.  This is used after the first copy has been added to the page
			// and goes through the list of shapes making new copies
			foreach (Victim victim in m_Shapes)
			{
				victim.Transformed = (Shape)victim.Original.Clone(Mapping.Ignore);
			}
		}

		protected void DisposeList()
		{
			// disposes of copies in list
			if (m_Shapes == null)
				return;
			foreach (Victim victim in m_Shapes)
			{
				victim.Transformed.Dispose();
			}
			m_Shapes.Clear();
		}

		/// <summary>Returns the transformed copy of the given original or Null</summary>
		public Shape Transformed(Shape original) => m_Shapes.Where(v => v.Original == original).Select(v => v.Transformed).FirstOrDefault();

		#endregion

		#region Modes and protection
		protected Modes m_Mode; // base class should set this during construction if applicable
		public enum Modes
		{
			Copy,
			Move,
			UseSystem // the default - use whatever is selected in the GUI.  The others can be specified by (eg) GrabSpots
		}

		public Modes Mode => m_Mode;

		public void ChangeMode(Modes mode, Page page)
		{
			m_Mode = mode;
			if (m_Shapes.Count > 0)
				page.MovingSelection = m_Mode == Modes.Move;
		}

		public abstract AllowedActions RequiredAllowed { get; }

		protected abstract string RequiredAllowedMessage { get; } // returns the error string if shapes did not

		#endregion

		#region Verbs
		// these are often done in the subclasses.  Default implementations for some of the functions are here

		public override VerbResult Start(ClickPosition position)
		{
			if (position.RequestedSnap == SnapModes.Grid)
				m_Initial = position.Page.Paper.SnapPoint2(position.Exact);
			else
				m_Initial = position.Exact;
			return StartSelection(position.Snapped, position.Page, position.Transaction);
			// NB objPosition.Snapped isn't snapped yet, as we needed to use exact position to do selection
		}

		protected virtual VerbResult StartSelection(PointF pt, Page page, Transaction transaction)
		{
			// implements the step where the shape(s) to be moved are selected.  The default is that this is during the Start function
			// but some transformations allow an external focus to be defined first
			if (page.SelectedCount == 0)
				return VerbResult.Rejected;
			// nothing selected.  The page should have selected the shape that the user clicked on if nothing was already selected
			// previously this check that the user had clicked within the bounding box of one of the selected shapes.  But because pt is already snapped
			// this is inaccurate; so now this is done within the View instead
			bool inappropriateShapes = false; // true if the selection included any shapes not appropriate for the current transformation
			foreach (Shape shp in page.SelectedShapes)
			{
				if ((shp.Allows & RequiredAllowed) == RequiredAllowed)
					m_Shapes.Add(new Victim(shp));
				else
					inappropriateShapes = true;
			}
			if (inappropriateShapes)
			{
				string message = RequiredAllowedMessage;
				if (!string.IsNullOrEmpty(message))
				{
					MessageBox.Show(message);
					return VerbResult.Rejected;
				}
				// note that if there is a message, we will stop if any shape fails.  If we can provide a message will continue with any permitted shapes
			}
			if (m_Shapes.Count == 0)
				return VerbResult.Rejected;
			page.MovingSelection = m_Mode == Modes.Move;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position) => VerbResult.Destroyed;

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			return DoTransform(position, false);
		}

		public override VerbResult Complete(ClickPosition position)
		{
			Float(position);
			return DoTransform(position, true);
		}

		public override VerbResult CompleteRetrospective() => VerbResult.TransformationComplete;

		#endregion

		#region transformation functions

		/// <summary>Applies the transform to the one shape - updating the provided copy (which may well BE a COPY).
		/// By default it just shape.ApplyTransformation, but can be overridden</summary>
		public virtual void DoTransform(Shape shape)
		{
			shape.ApplyTransformation(this);
		}

		protected VerbResult DoTransform(ClickPosition position, bool complete)
		{
			// should be called once the transformation is finally applied to the data
			// complete = true if one copy is made and the transaction is finished.  If false, then further copies can be made.  Has no effect if not copying
			// call MatrixChanged again first if necessary
			Transaction transaction = position.Transaction;
			Debug.Assert(position.Transaction != null);
			if (m_Mode == Modes.Copy)
			{
				foreach (Victim victim in m_Shapes)
				{
					transaction.Create(victim.Transformed);
					victim.Transformed.Status = StatusValues.Creating;
					position.Page.AddNew(victim.Transformed, transaction);
				}
				if (!complete)
				{
					// but allow continuation with new shapes
					if (m_Shapes.Count == 1)
					{
						Victim victim = m_Shapes[0];
						if ((victim.Original.Flags & GeneralFlags.NoColourOnTransformCopy) == 0)
						{
							FillStyleC fill = (FillStyleC)victim.Transformed.StyleObjectForParameter(Parameters.FillColour);
							if (fill != null)
							{
								int oldValue = fill.ParameterValue(Parameters.FillColour);
								fill.Colour = Color.FromArgb(Globals.ParameterValue(Parameters.FillColour));
								victim.Transformed.NotifyStyleChanged(Parameters.FillColour, oldValue, Globals.ParameterValue(Parameters.FillColour));
							} // not group
						}
					}
					NewCopy();
					Float(position);
					return VerbResult.Spawn; // indicates that the current transaction should be saved; creating the new shape
				}
				else
				{
					m_Shapes.Clear(); // so that we do not dispose of the new copies
					return VerbResult.TransformationComplete;
				}
			}
			else
			{
				// update the original shapes
				foreach (Victim victim in m_Shapes)
				{
					transaction.Edit(victim.Original);
					victim.Original.Status = StatusValues.Moved;
					DoTransform(victim.Original);
				}
				m_Shapes.Clear(); // so that we do not dispose of the new copies
				return VerbResult.TransformationComplete;
			}
		}

		/// <summary>the individual transformations should call this (probably during Float) as they alter the matrix</summary>
		protected void MatrixChanged()
		{
			foreach (Victim victim in m_Shapes)
			{
				victim.Transformed.CopyFrom(victim.Original, CopyDepth.Transform, null);
				DoTransform(victim.Transformed);
			}
			m_Bounds = RectangleF.Empty;
		}

		// the matrix requires an array of points; we keep 2 element array in stock in order to avoid continuously creating and destroying short arrays
		private PointF[] m_TempPoints = new PointF[2];
		public void TransformPoint(ref PointF pt)
		{
			m_TempPoints[0] = pt;
			m_Transformation.TransformPoints(m_TempPoints);
			pt = m_TempPoints[0];
		}

		public void TransformPoints(List<PointF> points)
		{
			PointF[] asArray = points.ToArray();
			m_Transformation.TransformPoints(asArray);
			for (int index = 0; index <= points.Count - 1; index++)
			{
				points[index] = asArray[index];
			}
		}

		public void TransformPoints(PointF[] points)
		{
			m_Transformation.TransformPoints(points);
		}

		public void TransformVector(ref SizeF vector)
		{
			m_TempPoints[0] = new PointF(vector.Width, vector.Height);
			m_Transformation.TransformVectors(m_TempPoints);
			vector = new SizeF(m_TempPoints[0]);
		}

		public void TransformRectangle(ref RectangleF rct)
		{
			Size ignore = new Size(1, 1);
			TransformRectangle(ref rct, ref ignore);
		}

		public void TransformRectangle(ref RectangleF rct, ref Size mirrorSize)
		{
			// szMirror is updated to show whether the rectangle was actually reflected.  It should be Size (+-1, +-1)
			// (it doesn't matter whether the Size starts positive or negative, the sign is inverted if the transformation would invert the rectangle in that direction)
			m_TempPoints[0] = rct.Location; // top left
			m_TempPoints[1] = new PointF(rct.Right, rct.Bottom);
			m_Transformation.TransformPoints(m_TempPoints);
			// the complication is that the points may no longer be in order
			rct.X = Math.Min(m_TempPoints[0].X, m_TempPoints[1].X);
			rct.Y = Math.Min(m_TempPoints[0].Y, m_TempPoints[1].Y);
			if (m_TempPoints[1].X < m_TempPoints[0].X)
				mirrorSize.Width = -mirrorSize.Width;
			if (m_TempPoints[1].Y < m_TempPoints[0].Y)
				mirrorSize.Height = -mirrorSize.Height;
			rct.Width = Math.Abs(m_TempPoints[1].X - m_TempPoints[0].X);
			rct.Height = Math.Abs(m_TempPoints[1].Y - m_TempPoints[0].Y);
		}

		/// <summary>not sure how to do this using the matrix - in theory the matrix must define separate values for each direction </summary>
		public abstract void TransformScalar(ref float length);

		public abstract void TransformAngle(ref float angle);

		public abstract void TransformDirection(ref int direction);
		// some shapes use a direction (1 or -1) as a sort of winding direction

		public void TransformPath(GraphicsPath path)
		{
			path.Transform(m_Transformation);
		}

		#endregion

		#region Irrelevant functions
		// transformations are never stored on the page or in the file, so many functions do not apply
		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			Debug.Fail("Function not applicable to transformation");
		}

		protected internal override void Load(DataReader reader)
		{
			base.Load(reader);
			Debug.Fail("Function not applicable to transformation");
		}

		protected internal override void Save(DataWriter writer)
		{
			base.Save(writer);
			Debug.Fail("Function not applicable to transformation");
		}

		protected internal override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			Debug.Fail("Function not applicable to transformation");
			return true;
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			Debug.Fail("Should not be transforming a transformation!");
		}

		#endregion

		#region Coordinates
		protected override RectangleF CalculateBounds()
		{
			RectangleF bounds = RectangleF.Empty;
			foreach (Victim victim in m_Shapes)
			{
				Geometry.Extend(ref bounds, victim.Transformed.Bounds);
			}
			return bounds;
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			// always includes shadow (mostly xforms are drawn with; doesn't matter if occasionally this invalidates too much)
			RectangleF bounds = RectangleF.Empty;
			foreach (Victim victim in m_Shapes)
			{
				Geometry.Extend(ref bounds, victim.Transformed.RefreshBounds(true));
			}
			return bounds;
		}

		public PointF MiddleOriginalShapes()
		{
			float X = 0;
			float Y = 0;
			int count = 0;
			foreach (Victim victim in m_Shapes)
			{
				X += victim.Original.Middle().X;
				Y += victim.Original.Middle().Y;
				count += 1;
			}
			return new PointF(X / count, Y / count);
		}

		protected PointF MiddleTransformedShapes()
		{
			float X = 0;
			float Y = 0;
			int count = 0;
			foreach (Victim victim in m_Shapes)
			{
				X += victim.Transformed.Middle().X;
				Y += victim.Transformed.Middle().Y;
				count += 1;
			}
			return new PointF(X / count, Y / count);
		}

		#endregion

		#region Graphics
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{ }

		internal override void DrawShadow(Canvas gr, float scale)
		{ }

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			if (Globals.Root.CurrentConfig.ReadBoolean(Config.Moving_Shadow, true))
				foreach (Victim victim in m_Shapes)
				{
					victim.Transformed.DrawShadow(gr, scale);
				}
			// note: deliberately separate loops
			foreach (Victim victim in m_Shapes)
			{
				victim.Transformed.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				DisposeList();
		}

		public virtual void SetGrabTransform(PointF startPoint, PointF current)
		{
			// used if this is being used to support a grab-spot drag.  Sets the matrix for the given movement
			throw new InvalidOperationException("Transformation of type " + this.GetType() + " does not support grab spot adjustment");
		}

		internal override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			if (m_Shapes.Count != 1)
				return new List<UserSocket>();
			return m_Shapes[0].Transformed.GetPointsWhichSnapWhenMoving();
		}

		public virtual bool NextChoosePerformsSelection()
		{
			// must return true if the next Start/Choose verb is the one which selects the shapes to operate on
			// the view does most of this selection
			return m_Initial.IsEmpty;
		}

		protected internal override void Diagnostic(System.Text.StringBuilder output)
		{
			base.Diagnostic(output);
			output.Append("Initial = ");
			PointDiagnostic(output, m_Initial);
			output.AppendLine("Matrix = ");
			DiagnosticMatrix(output, m_Transformation);
		}

		internal static void DiagnosticMatrix(System.Text.StringBuilder output, Matrix matrix)
		{
			// appends a description of the matrix to the output
			output.Append("{");
			output.Append(matrix.Elements[0].ToString("0.###")).Append(",");
			output.Append(matrix.Elements[1].ToString("0.###")).AppendLine(",");
			output.Append(matrix.Elements[2].ToString("0.###")).Append(",");
			output.Append(matrix.Elements[3].ToString("0.###")).AppendLine(",");
			output.Append(matrix.Elements[4].ToString("0.###")).Append(",");
			output.Append(matrix.Elements[5].ToString("0.###")).AppendLine("}");
		}

		/// <summary>true when shapes actually being moved (suppresses drawing of highlight, which might be in wrong place)
		/// false when just placed focal point</summary>
		public abstract bool Active();

	}

	public class TransformMove : Transformation
	{

		public TransformMove(Modes mode) : base(mode)
		{ }

		public TransformMove(float deltaX, float deltaY) : base(Modes.Move)
		{
			// used when we want to move some shapes - this makes a transformation with the desired offset which can be passed to Shape.ApplyTransformation
			m_Transformation = new Matrix();
			m_Transformation.Translate(deltaX, deltaY);
		}

		public TransformMove(Shape shape) : base(Modes.Copy)
		{
			m_Shapes.Add(new Victim(shape));
			m_Initial = shape.Middle();
		}

		public override VerbResult Start(ClickPosition position)
		{
			if (position.RequestedSnap == SnapModes.Angle)
				position.View.InvalidateAll(); // because of the green lines
			return base.Start(position);
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (position.RequestedSnap == SnapModes.Angle)
				position.View.InvalidateAll(StaticView.InvalidationBuffer.Current); // because of the green lines
			return base.Cancel(position);
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (position.RequestedSnap == SnapModes.Angle)
				position.View.InvalidateAll(StaticView.InvalidationBuffer.Current); // because of the green lines
			return base.Complete(position);
		}

		public override VerbResult Choose(ClickPosition position)
		{
			if (position.RequestedSnap == SnapModes.Angle)
				position.View.InvalidateAll(StaticView.InvalidationBuffer.Current); // because of the green lines
			return base.Choose(position);
		}

		#region Basics
		public override Shapes ShapeCode
		{ get { return Shapes.TransformMove; } }

		protected virtual void Constrain(ref SizeF transform)
		{
			// allows the vertical or horizontal ones to modify the vector
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (m_Mode == Modes.Copy)
			{
				list.Add(new Prompt(ShapeVerbs.Choose, "TransformAny_ChooseCopy", "TransformMove_ChooseCopy"));
				list.Add(new Prompt(ShapeVerbs.Complete, "TransformAny_FinishCopy", "TransformMove_FinishCopy"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformCopy", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformAny_FinishMove", "TransformMove_FinishMove"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
			}
			return list;
		}

		public override AllowedActions RequiredAllowed => AllowedActions.TransformMove;
		protected override string RequiredAllowedMessage => ""; // no message available for this one
		public override bool Active() => true;

		#endregion

		#region Positioning and SetGrabTransform

		public override VerbResult Float(ClickPosition position)
		{
			// if using shape to shape snapping, then initially place using the exact coordinates, and then we'll snap the vertices from there to a nearby shapes
			// otherwise use objPosition.Snapped as the target, which might be snapped to grid if appropriate
			SizeF vector = m_Initial.VectorTo(position.SnapMode == SnapModes.Shape ? position.Exact : position.Snapped);
			//Debug.WriteLine(m_ptInitial.ToString + " >> " + objPosition.Snapped.ToString)
			Constrain(ref vector);
			m_Transformation = new Matrix();
			m_Transformation.Translate(vector.Width, vector.Height);
			base.MatrixChanged();

			if (position.SnapMode == SnapModes.Shape)
			{
				List<UserSocket> vertices = GetPointsWhichSnapWhenMoving();
				Shape singleMovingShape = null;
				if (m_Shapes.Count == 1)
					singleMovingShape = m_Shapes[0].Transformed;
				if (vertices != null && vertices.Count > 0)
					Snap(vertices, position.Zoom, position.Page, singleMovingShape, MiddleOriginalShapes(), MiddleTransformedShapes());
			}

			return VerbResult.Continuing;
		}

		public override void SetGrabTransform(PointF startPoint, PointF current)
		{
			SizeF vector = startPoint.VectorTo(current);
			Constrain(ref vector);
			m_Transformation = new Matrix();
			m_Transformation.Translate(vector.Width, vector.Height);
		}

		public void SetDelta(float X, float Y)
		{
			m_Transformation.Reset();
			m_Transformation.Translate(X, Y);
		}

		public void SetDelta(SizeF offset)
		{
			m_Transformation.Reset();
			m_Transformation.Translate(offset.Width, offset.Height);
		}

		#endregion

		#region Snapping

		protected internal override SnapModes SnapNext(SnapModes requested) => requested;
		protected internal override PointF DoSnapAngle(PointF newPoint) => Geometry.AngleSnapPoint(newPoint, m_Initial, Geometry.ANGLE90);

		internal bool Snap(List<UserSocket> vertices, float scale, Page page, Shape singleMovingShape, PointF originalCentre, PointF currentCentre)
		{
			// ptOriginalCentre should be the centre of the original shape(s) (this is only used if auto rotation is required)
			// Other parameters as below
			SizeF vectorResult = SizeF.Empty;
			float rotationResult = 0;
			bool rotate = false;
			if (singleMovingShape != null)
				rotate = (singleMovingShape.Allows & AllowedActions.TransformRotate) > 0; // If multiple shapes then we never rotate
			bool success = Snap(vertices, scale, page, singleMovingShape, currentCentre, rotate, ref vectorResult, ref rotationResult);
			if (success)
			{
				// snap is acceptable, process it...
				m_Transformation.Translate(vectorResult.Width, vectorResult.Height);
				if (rotationResult != 0)
				{
					m_Transformation.RotateAt(Geometry.Degrees(rotationResult), originalCentre);
					//m_sngAutoRotationAngle += sngRotationResult;
				}
				base.MatrixChanged();
				page.ClearTargets();
			}
			return success;
		}

		internal static bool Snap(List<UserSocket> vertices, float scale, Page page, Shape singleMovingShape, PointF currentCentre, bool autoRotate, ref SizeF vectorResult, ref float rotationResult)
		{
			// this function extracts from EditableView the functionality to perform shape snap between the shape being moved and anything on the page
			// Returns true if snapping should be performed, false if too far from any targets
			// if only one shape is being moved, objSingleMovingShape should be that shape (used to check for dropping identical shapes on top of each other)
			// use nothing if multiple shapes being moved.  This is also used to avoid snapping to self with single shapes (e.g. in verb TidyShape)
			// resulting value is returned in szVectorResult, rotationResult (Returning these rather than the target allows this to be public)
			// These only defined if the result value is true
			List<TargetGroup> groups = new List<TargetGroup>();
			// organise all the possible targets, grouping the ones with the same data vector and rotation
			// started using a dictionary, but had problems with rounding errors in the vector giving different hash codes
			// and also different vectors (especially -ve of each other) giving the same hash code.  Aaargh
			foreach (UserSocket socket in vertices) // Caller has already checked for null/empty list etc
				foreach (Target target in page.GenerateIndependentTargets(socket, scale))
				{
					SizeF vector = socket.Centre.VectorTo(target.Position);
					if (target.Type != Target.Types.PageEdgeH && target.Type != Target.Types.PageEdgeV && target.Shape != singleMovingShape)
					{
						if (target.RotationRequired == 0 || autoRotate)
						{
							// having some problems with the vectors not been quite equal, so try rounding it
							// (it is important for the identical shape check below that we group all of the same data together)
							if (Math.Abs(target.RotationRequired) > Geometry.NEGLIGIBLEANGLESMALL)
							{
								// Need to adjust the vector; we assume rotation about the centre of the moving sockets
								PointF ptRotated = socket.Centre.RotateBy(target.RotationRequired, currentCentre);
								vector = ptRotated.VectorTo(target.Position);
							}
							TargetGroup existing = null;
							foreach (TargetGroup test in groups)
							{
								if (Math.Abs(test.Vector.Width - vector.Width) < 0.1f
									&& Math.Abs(test.Vector.Height - vector.Height) < 0.1f
									&& Math.Abs(test.AssumedRotation - target.RotationRequired) < 0.1f)
								{
									existing = test;
									break;
								}
							}
							if (existing != null)
								existing.Add(target);
							else
								groups.Add(new TargetGroup(vector, target, target.RotationRequired));
						} // rotation allowed
					} // not snapping to objSingleMovingShape
				} // next target for this socket
			page.ClearTargets();
			// need to find the best modified distance
			TargetGroup closestGroup = null;
			float closestDistance = float.MaxValue; // the modified this to is for the best one so far
			foreach (TargetGroup target in groups)
			{
				if (target.Valid)
				{
					float distance = target.AdjustedDistance();
					if (distance < closestDistance && target.Distance() < Target.ActivationMaximumDistanceMoveShape * GUIUtilities.MillimetreSize)
					{
						// Second condition is a sanity check just to avoid a situation where multiple hits cause a shape to jump all the way way across the page
						// by not counting it here, a target with lower priority, but outside the adjusted distance can still be considered
						closestDistance = distance;
						closestGroup = target;
					}
				}
			}
			if (closestGroup == null)
				return false;
			if (closestGroup.AdjustedDistance() < Target.ActivationThresholdMoveShape * GUIUtilities.MillimetreSize)
			{
				// check if every vertex on the moving shape is snapping onto a vertex on the same target shape, i.e. if we are overlaying identical shapes
				// we want to try and block this
				if (singleMovingShape != null)
				{
					foreach (Shape shape in closestGroup.GetDistinctTargetShapes())
					{
						// Check if this shape matches every point
						if (shape.ShapeCode == singleMovingShape.ShapeCode) // it is only a problem if they are the same type
						{
							// we can't check the data is completely identical, because we don't actually care which order the vertices are listed
							// i.e. we don't want to drop a shape on an identical one if they are technically mirror images (but have symmetry
							// so that these look identical).  So we just check if every moving vertex has hit this same target shape with this vector
							bool matches = true;
							if (shape.ShapeCode == Shapes.Group && !shape.Bounds.ApproxEqual(singleMovingShape.Bounds))
							{
								// for groups we need to check the bounds as well as there might be a very small number of sockets (1!) which could align
								// although the shapes are very different
								matches = false;
							}
							else
							{
								if (vertices.Any(vertex => !closestGroup.ContainsVertexCentre(vertex.Centre + closestGroup.Vector, shape)))
									matches = false;
							}
							if (matches)
								return false;
						}
					}
				}
				// also check if this would cause any invalid socket-socket matches
				if (closestGroup.Mismatch)
				{
					// just draw the failed targets
					List<Target> failed = closestGroup.Targets.Where(target => target.Mismatch).ToList();
					Debug.Assert(failed.Count > 0);
					page.SetDrawnTargets(failed);
					return false;
				}

				vectorResult = closestGroup.Vector;
				rotationResult = closestGroup.AssumedRotation;
				return true;
			}
			return false;
		}

		private class TargetGroup
		{
			// we are more interested in organising the list of targets by the offset which would need to be applied to the transformation
			// than by the actual shapes they hit or the distances.  We also consider Rotation now - the group contains targets requiring the same
			// vector (after modifying for rotation!) and rotation, if any
			public SizeF Vector;
			public List<Target> Targets = new List<Target>();
			private int TotalPriority = 0;
			public bool Valid = false; // true if one or more of the targets are valid (i.e. not socket-socket where they are incompatible)
			public bool Mismatch = false; // true if one or more are not valid
										  // if the entire group is mismatched it can be ignored; if it is a mixture of valid and invalid target, we might want to display it in some way
			public float AssumedRotation = 0; // the rotation which would be required to get the sockets to line up if doing this snap

			public TargetGroup(SizeF vector, Target target, float rotation)
			{
				Vector = vector;
				Targets.Add(target);
				TotalPriority += (int)target.Priority;
				if (target.Mismatch)
					Mismatch = true;
				else
					Valid = true;
				AssumedRotation = rotation;
			}

			public void Add(Target target)
			{
				Targets.Add(target);
				TotalPriority += (int)target.Priority;
				if (target.Mismatch)
					Mismatch = true;
				else
					Valid = true;
				Debug.Assert(Math.Abs(target.RotationRequired - AssumedRotation) < 0.01);
			}

			public float AdjustedDistance()
			{
				// in the same way that targets have a distance which is real distance - priority, this returns the distance minus the total priority
				return Vector.Length() - TotalPriority / (float)Math.Sqrt(Targets.Count);
				// we need to avoid simply adding priorities, because this tends select moving a shape directly on top of a similar shape,
				// even when the one being moved is currently outside and the best solution is to tessellate them
			}

			public float Distance()
			{
				return Vector.Length();
			}

			public IEnumerable<Shape> GetDistinctTargetShapes()
			{
				// returns a list of all the shapes we are snapping to, but only counting vertices snapping
				Dictionary<Shape, Shape> shapes = new Dictionary<Shape, Shape>();
				foreach (Target target in Targets)
				{
					if (target.Type == Target.Types.Vertex || target.Type == Target.Types.Centre)
					{
						if (!shapes.ContainsKey(target.Shape))
							shapes.Add(target.Shape, target.Shape);
					}
				}
				return shapes.Values;
			}

			public bool ContainsVertexCentre(PointF targetPoint, Shape targetShape)
			{
				// Returns true if there is a target in the list at the given position for the given shape which is a vertex or centre
				foreach (Target target in Targets)
				{
					if (target.Shape == targetShape && targetPoint.ApproxEqual(target.Position))
						return true;
				}
				return false;
			}
		}

		#endregion

		#region Transforming values
		// no change unless rotation angle is set
		public override void TransformScalar(ref float length)
		{ }
		public override void TransformAngle(ref float angle)
		{ }
		public override void TransformDirection(ref int direction)
		{ }

		#endregion

	}

	#region TransformMoveVH
	public class TransformMoveVH : TransformMove
	{

		public TransformMoveVH(Modes mode) : base(mode)
		{ }

		public override Shapes ShapeCode
		{ get { return Shapes.TransformMoveVH; } }

		protected override void Constrain(ref SizeF transform)
		{
			if (Math.Abs(transform.Width) > Math.Abs(transform.Height))
				// predominantly in the X direction
				transform.Height = 0;
			else
				transform.Width = 0;
		}

	}
	#endregion

	#region Focal
	public abstract class TransformFocal : Transformation
	{
		// Base class for transformations which need to pick a focus point before starting
		// unlike the movement transformations, the initial click does not need to be inside the selected shapes
		// mybase.m_Initial is the focus point
		// m_ptStart is now the position clicked to select the shape and trigger the xform

		protected PointF m_Start = PointF.Empty;
		protected bool m_StartFixed = false;

		protected TransformFocal(Modes mode) : base(mode)
		{ }

		public override bool Active() => m_StartFixed;

		#region Standard implementation of most verbs
		public override VerbResult Start(ClickPosition position)
		{
			// nothing selected.
			// all selected shapes are affected;
			m_Initial = position.Snapped;
			return VerbResult.Continuing;
		}

		public override VerbResult Cancel(ClickPosition position)
		{
			if (m_StartFixed)
			{
				// reverts to choosing the start point; thus also dumps the list of selected shapes
				m_StartFixed = false;
				position.Page.MovingSelection = false;
				base.DisposeList();
				return VerbResult.Continuing;
			}
			return base.Cancel(position);
		}

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			if (!m_StartFixed)
			{
				// this is the point which must be inside one of the victims
				if (position.Snapped.Equals(m_Initial))
					return VerbResult.Rejected; // this will cause all sorts of problem with rotation, and probably with other transformations
				VerbResult result = base.StartSelection(position.Snapped, position.Page, position.Transaction);
				if (result == VerbResult.Continuing)
					m_StartFixed = true;
				return result;
			}
			if (m_Shapes.Count == 0)
			{
				Utilities.LogSubError("No shapes in xform list");
				return VerbResult.Destroyed;
			}
			return DoTransform(position, false);
		}

		public override VerbResult Complete(ClickPosition position)
		{
			if (!m_StartFixed)
				return VerbResult.Rejected;
			return Choose(position);
		}

		public override VerbResult CompleteRetrospective()
		{
			if (!m_StartFixed)
				return VerbResult.Rejected;
			if (m_Shapes.Count == 0)
			{
				Utilities.LogSubError("No shapes in xform list");
				return VerbResult.Destroyed;
			}
			return VerbResult.TransformationComplete;
		}

		public override VerbResult Float(ClickPosition position)
		{
			if (!m_StartFixed)
			{
				m_Start = position.Snapped;
				return VerbResult.Continuing;
			}
			return FloatFinal(position.Snapped);
		}

		protected abstract VerbResult FloatFinal(PointF pt);
		// subclasses should override this to implement floating the final point, i.e. setting the transformation matrix
		#endregion

		internal const int FOCUSSIZE = 3;

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			base.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			// we need to draw the focal point as well
			// we don't use the objresources.mainpen logic from the base class, because we don't need to bother with faded versions etc
			using (Stroke pn = gr.CreateStroke(Color.FromArgb(150, Color.Orange), Geometry.TRANSPARENTLINE))
			{
				gr.DrawLine(m_Initial.X - FOCUSSIZE, m_Initial.Y - FOCUSSIZE, m_Initial.X + FOCUSSIZE, m_Initial.Y + FOCUSSIZE, pn);
				gr.DrawLine(m_Initial.X + FOCUSSIZE, m_Initial.Y - FOCUSSIZE, m_Initial.X - FOCUSSIZE, m_Initial.Y + FOCUSSIZE, pn);
			}

		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			RectangleF bounds = base.RefreshBounds(withShadow);
			Geometry.Extend(ref bounds, Geometry.RectangleFromPoint(m_Initial, FOCUSSIZE + 1));
			return bounds;
		}

		public override bool NextChoosePerformsSelection()
		{
			if (m_Initial.IsEmpty)
				return false; // haven't even started yet
			if (m_StartFixed)
				return false;
			return true; // it is fixing the start point which picks up the shape
		}

	}
	#endregion

	#region TransformRotate
	/// <summary>Performs rotation;  implementation as tool is not used in SAW.
	/// Unlike most transforms this also has some static state which changes the default behaviour (see rotation palette)</summary>
	public class TransformRotate : TransformFocal
	{

		/// <summary>Angle of rotation in radians</summary>
		private float m_Radians;

		/// <summary>If false all rotation is done about the given point.</summary>
		public static bool RotateAboutCentres = true;
		/// <summary>Point on the page to rotate about if RotateAboutCentres == false</summary>
		public static PointF RotationPoint;
		/// <summary>Whether text objects should be included when rotating</summary>
		public static bool IncludeText = false;
		/// <summary>Set during a single transform if there is only text and we probably do want to act on it (eg user is manipulating the grabhandle on that text).
		/// If true then IncludeText flag is ignored and text always rotated. </summary>
		private bool IgnoreTextFlag;

		public TransformRotate(Modes mode) : base(mode)
		{ }

		public TransformRotate(PointF focus, float radians) : base(Modes.Move)
		{
			// version for when the code wants to modify shapes
			m_Radians = radians;
			m_Initial = focus;
			m_Transformation = new Matrix();
			m_Transformation.RotateAt(Geometry.Degrees(m_Radians), m_Initial);
		}

		#region Basic information
		public override Shapes ShapeCode => Shapes.TransformRotate;

		protected internal override SnapModes SnapNext(SnapModes requested)
		{
			// This now angle snaps if grid snapping is turned on (many users won't find angle snapping, but would probably be hoping for some sort of snapping if grid snapping is turned on)
			if ((requested == SnapModes.Angle || requested == SnapModes.Grid) && m_StartFixed)
				return SnapModes.Angle;
			return SnapModes.Off;
		}

		protected internal override PointF DoSnapAngle(PointF newPoint) => Geometry.AngleSnapPointRelative(newPoint, m_Initial, m_Start);

		public override AllowedActions RequiredAllowed => AllowedActions.TransformRotate;
		protected override string RequiredAllowedMessage => Strings.Item("Cannot_Rotate");

		#endregion

		protected override VerbResult FloatFinal(PointF pt)
		{
			SetGrabTransform(m_Start, pt);
			base.MatrixChanged();
			return VerbResult.Continuing;
		}

		protected override VerbResult StartSelection(PointF pt, Page page, Transaction transaction)
		{
			IgnoreTextFlag = page.SelectedShapes.All(ShapeCountsAsText); // if only text is selected then ignore the text only flag
			if (!RotateAboutCentres)
				page.RotationPointUsed = true;
			return base.StartSelection(pt, page, transaction);
		}

		#region Grabs and prompts

		public static TransformRotate CreateForGrabMove(PointF centre, PointF start, IEnumerable<Shape> selected)
		{
			return new TransformRotate(Modes.Move)
			{
				m_Initial = centre,
				m_Start = start,
				m_StartFixed = true,
				IgnoreTextFlag = selected.All(ShapeCountsAsText)
			};
			// not actually essential that this is stored as it is provided each time
		}

		public override void SetGrabTransform(PointF startPoint, PointF current)
		{
			float startAngle = Geometry.VectorAngle(m_Initial, startPoint);
			float currentAngle = Geometry.VectorAngle(m_Initial, current);
			m_Radians = Geometry.AngleBetween(startAngle, currentAngle);
			m_Transformation = new Matrix();
			m_Transformation.RotateAt(Geometry.Degrees(m_Radians), m_Initial);
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (!m_StartFixed)
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformRotate_Select", "TransformRotate_Focus"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
				// the move text makes reasonable sense for cancelling  focus point
			}
			else if (m_Mode == Modes.Copy)
			{
				list.Add(new Prompt(ShapeVerbs.Choose, "TransformAny_ChooseCopy", "TransformRotate_Choose"));
				list.Add(new Prompt(ShapeVerbs.Complete, "TransformAny_FinishCopy", "TransformRotate_FinishCopy"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformCopy", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformAny_FinishMove", "TransformRotate_FinishMove"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
			}
			return list;
		}

		#endregion

		#region Doing transforms

		public override void DoTransform(Shape shape)
		{ // this differs in that each shape might be rotated about its own centre - ie each using a different matrix
			if (!IncludeText && !IgnoreTextFlag && ShapeCountsAsText(shape))
				return;
			if (!RotateAboutCentres)
			{
				Page page = shape.FindPage(); // doing this per shape isn't necessary, but is harmless and this is the easiest place to put it
				if (page.RecentRotationPoints.Any())
					page.RotationPointUsed = true;
				base.DoTransform(shape);
				return;
			}
			// need to create a new matrix for this shape which rotates about its centre:
			Matrix originalMatrix = m_Transformation;
			m_Transformation = new Matrix();
			m_Transformation.RotateAt(Geometry.Degrees(m_Radians), shape.Centre);
			shape.ApplyTransformation(this);
			m_Transformation = originalMatrix;
		}

		public override void TransformAngle(ref float angle)
		{
			angle += m_Radians;
		}

		public override void TransformDirection(ref int direction)
		{
			// no change.  Updating the base of vertices of (e.g.) a polygon will rotate the entire shape; we do not need to change the winding direction
		}

		public override void TransformScalar(ref float length)
		{
			// No change
		}

		#endregion

		/// <summary>True if the given shape counts as text for the IncludeText flag (currently only FloatingLabel, but that might change)</summary>
		internal static bool ShapeCountsAsText(Shape s) => s is FloatingLabel;


	}
	#endregion

	#region TransformScale
	public class TransformScale : TransformFocal
	{

		private float m_Scale; // although 0 indicates not valid.  Can be negative
		/// <summary>If true each shape is scaled from its own centre </summary>
		private bool m_ShapeCentres;

		public TransformScale(Modes mode) : base(mode)
		{ }

		public TransformScale(PointF focus, float scale, bool useShapeCentres) : base(Modes.Move)
		{
			// version for when the code wants to modify shapes
			m_Initial = focus;
			m_StartFixed = true;
			m_Scale = scale;
			m_ShapeCentres = useShapeCentres;
			m_Transformation = new Matrix();
			m_Transformation.Translate(m_Initial.X * (1 - m_Scale), m_Initial.Y * (1 - m_Scale));
			m_Transformation.Scale(m_Scale, m_Scale);
		}

		#region Basic information

		public override Shapes ShapeCode => Shapes.TransformScale;
		protected internal override SnapModes SnapNext(SnapModes requested) => SnapModes.Off;
		public override AllowedActions RequiredAllowed => AllowedActions.TransformScale;
		protected override string RequiredAllowedMessage => Strings.Item("Cannot_Resize");

		#endregion

		public override void DoTransform(Shape shape)
		{
			if (!m_ShapeCentres)
				base.DoTransform(shape);
			else
			{
				// need to create a new matrix for this shape which focuses on its centre:
				Matrix originalMatrix = m_Transformation;
				m_Transformation = new Matrix();
				m_Transformation.Translate(shape.Centre.X * (1 - m_Scale), shape.Centre.Y * (1 - m_Scale));
				m_Transformation.Scale(m_Scale, m_Scale);
				shape.ApplyTransformation(this);
				m_Transformation = originalMatrix;
			}
		}

		private const float MAXIMUMSHRINKAGE = 100f;
		protected override VerbResult FloatFinal(PointF pt)
		{
			// can't use SetGrabTransform, because we want to reject some values
			SizeF start = m_Initial.VectorTo(m_Start);
			// need current scalar in direction of initial vector (or negative if opposite)
			float startLength = start.Length();
			float currentLength = Geometry.ProjectionScalar(m_Initial.VectorTo(pt), start); // Geometry.Scalar(m_ptInitial, pt)
			if (currentLength == 0 || Math.Abs(currentLength) < startLength / MAXIMUMSHRINKAGE)
				return VerbResult.Rejected;
			// we not only reject scaling to nothing, but also scaling to almost nothing which would leave the shape impossible to display meaningfully
			m_Scale = currentLength / startLength;
			m_Transformation = new Matrix();
			m_Transformation.Translate(m_Initial.X * (1 - m_Scale), m_Initial.Y * (1 - m_Scale));
			m_Transformation.Scale(m_Scale, m_Scale);
			base.MatrixChanged();
			return VerbResult.Continuing;
		}

		public override void SetGrabTransform(PointF startPoint, PointF current)
		{
			SizeF start = m_Initial.VectorTo(m_Start);
			float startLength = Geometry.DistanceBetween(m_Initial, m_Start);
			float currentLength = Geometry.ProjectionScalar(m_Initial.VectorTo(current), start);
			m_Transformation = new Matrix();
			if (currentLength == 0 || Math.Abs(currentLength) < startLength / MAXIMUMSHRINKAGE)
			{
				// we not only reject scaling to nothing, but also scaling to almost nothing which would leave the shape impossible to display meaningfully
				// Leaves identity matrix in place if we don't like it
				m_Scale = 1;
				return;
			}
			m_Scale = currentLength / startLength;
			m_Transformation.Translate(m_Initial.X * (1 - m_Scale), m_Initial.Y * (1 - m_Scale));
			m_Transformation.Scale(m_Scale, m_Scale);
		}

		public static TransformScale CreateForGrabMove(PointF centre, PointF start)
		{
			TransformScale create = new TransformScale(Modes.Move);
			Debug.Assert(!centre.IsEmpty);
			create.m_Initial = centre;
			create.m_Start = start; // not actually essential that this is stored as it is provided each time
			create.m_StartFixed = true;
			return create;
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (!m_StartFixed)
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformScale_Select", "TransformScale_Focus"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
				// the move text makes reasonable sense for cancelling  focus point
			}
			else if (m_Mode == Modes.Copy)
			{
				list.Add(new Prompt(ShapeVerbs.Choose, "TransformAny_ChooseCopy", "TransformScale_Choose"));
				list.Add(new Prompt(ShapeVerbs.Complete, "TransformAny_FinishCopy", "TransformScale_FinishCopy"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformCopy", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformAny_FinishMove", "TransformScale_FinishMove"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
			}
			return list;
		}

		#region Making scalar modifications
		public override void TransformAngle(ref float angle)
		{
			// No change
		}

		public override void TransformDirection(ref int direction)
		{
			// No change to winding direction required
		}

		public override void TransformScalar(ref float length)
		{
			length = length * Math.Abs(m_Scale);
		}

		#endregion

	}
	#endregion

	#region TransformLinearScale
	public class TransformLinearScale : TransformFocal
	{

		private float m_Scale; // although 0 indicates not valid.  Can be negative
		private bool m_Horizontal;

		public TransformLinearScale(Modes mode) : base(mode)
		{ }

		public TransformLinearScale(PointF focus, float scale, bool horizontal) : base(Modes.Move)
		{
			// version for when the code wants to modify shapes
			m_Initial = focus;
			m_StartFixed = true;
			m_Scale = scale;
			m_Horizontal = horizontal;
			SetTransformation();
		}

		public override Shapes ShapeCode => Shapes.TransformLinearScale;
		protected internal override SnapModes SnapNext(SnapModes requested) => SnapModes.Off;
		public override AllowedActions RequiredAllowed => AllowedActions.TransformLinearStretch;
		protected override string RequiredAllowedMessage => Strings.Item("Cannot_Resize");

		private const float MAXIMUMSHRINKAGE = 100;
		protected override VerbResult FloatFinal(PointF pt)
		{
			throw new InvalidOperationException();
			// not required as this is only used for GrabSpots
			//' can't use SetGrabTransform, because we want to reject some values
		}

		public override void SetGrabTransform(PointF startPoint, PointF current)
		{
			SizeF startSize = m_Initial.VectorTo(m_Start);
			float startLength = Geometry.DistanceBetween(m_Initial, m_Start);
			float currentLength = Geometry.ProjectionScalar(m_Initial.VectorTo(current), startSize);
			if (currentLength == 0 || Math.Abs(currentLength) < startLength / MAXIMUMSHRINKAGE)
			{
				// we not only reject scaling to nothing, but also scaling to almost nothing which would leave the shape impossible to display meaningfully
				// Leaves identity matrix in place if we don't like it
				m_Transformation = new Matrix();
				m_Scale = 1;
				return;
			}
			m_Scale = currentLength / startLength;
			SetTransformation();
		}

		private void SetTransformation()
		{
			m_Transformation = new Matrix();
			if (m_Horizontal)
			{
				m_Transformation.Translate(m_Initial.X * (1 - m_Scale), 0);
				m_Transformation.Scale(m_Scale, 1);
			}
			else
			{
				m_Transformation.Translate(0, m_Initial.Y * (1 - m_Scale));
				m_Transformation.Scale(1, m_Scale);
			}
		}

		public static TransformLinearScale CreateForGrabMove(PointF centre, PointF start)
		{
			TransformLinearScale create = new TransformLinearScale(Modes.Move);
			Utilities.ErrorAssert(!centre.IsEmpty);
			Utilities.ErrorAssert(Math.Abs(centre.Y - start.Y) < Geometry.NEGLIGIBLE || Math.Abs(centre.X - start.X) < Geometry.NEGLIGIBLE, "TransformLinearScale - start points should be either horizontally or vertically aligned");
			create.m_Initial = centre;
			create.m_Start = start; // not actually essential that this is stored as it is provided each time
			create.m_Horizontal = Math.Abs(centre.Y - start.Y) < Geometry.NEGLIGIBLE;
			create.m_StartFixed = true;
			return create;
		}

		#region Making scalar modifications
		public override void TransformAngle(ref float angle)
		{
			// No change
		}

		public override void TransformDirection(ref int direction)
		{
			// No change to winding direction required
		}

		public override void TransformScalar(ref float length)
		{
			Debug.Fail("TransformLinearScale: TransformScalar should not be used; shapes which rely on this should not include TransformLinearScale in Allows");
		}

		#endregion

	}
	#endregion

	#region TransformMirror - 2 point version
	public class TransformMirror : Transformation
	{
		// the first attempt using a single point to place the reflection line wasn't terribly popular
		// so trying using two points; selecting the shape first and then placing each point
		// note that this is not focal as both points are placed AFTER selecting the shape

		//Private m_ptCentre As PointF ' the centre of the shape(s) being moved
		private PointF m_Focus = PointF.Empty; // this and the following point define the reflection line
		private PointF m_Second = PointF.Empty; // When this is empty we are still placing the focus.  This is defined once the focus is set and the line is being rotated

		protected bool m_InitialFixed;
		// m_ptInitial is the point inside the shape UNLIKE FOCAL

		public TransformMirror(Modes mode) : base(mode)
		{
		}

		public TransformMirror(PointF ptLineA, PointF ptLineB) : base(Modes.Move)
		{
			// version for when the code wants to modify shapes
			m_InitialFixed = true;
			m_Focus = ptLineA;
			m_Second = ptLineB;
			SetMatrix(Geometry.VectorAngle(ptLineA, ptLineB));
		}

		internal override List<Prompt> GetPrompts()
		{
			List<Prompt> list = new List<Prompt>();
			if (m_Second.IsEmpty)
			{
				list.Add(new Prompt(ShapeVerbs.Complete | ShapeVerbs.Choose, "TransformMirror_Choose", "TransformMirror_Choose"));
				list.Add(new Prompt(ShapeVerbs.Cancel, "CancelTransformMove", "CancelAll"));
			}
			else
			{
				list.Add(new Prompt(ShapeVerbs.Complete, "TransformMirror_Finish", "TransformMirror_Finish" + m_Mode));
				list.Add(new Prompt(ShapeVerbs.Cancel, "TransformMirror_Cancel1", "TransformMirror_Cancel1"));
			}
			return list;
		}

		#region Basics

		public override Shapes ShapeCode => Shapes.TransformMirror;
		protected internal override SnapModes SnapNext(SnapModes requested) => requested;
		public override AllowedActions RequiredAllowed => AllowedActions.TransformMirror;
		public override bool Active() => !m_Second.IsEmpty;
		protected override string RequiredAllowedMessage => Strings.Item("Cannot_Reflect");

		#endregion

		#region Verbs and Positioning

		private const float SNAPANGLETOLERANCE = Geometry.PI / 36; // 5° each way
		public override VerbResult Float(ClickPosition position)
		{
			// If Not m_InitialFixed Then Return VerbResult.Continuing
			if (m_Second.IsEmpty)
			{
				// Still placing the focus point
				m_Focus = position.Snapped;
				m_Bounds = CalculateBounds();
			}
			else
			{
				if (Geometry.DirtyDistance(m_Focus, position.Snapped) < Geometry.MILLIMETRE)
					return VerbResult.Rejected; // reflection line/point so close to original shape it is meaningless
				m_Second = position.Snapped;
				// doesn't matter which way along the line this goes
				float angle = Geometry.VectorAngle(m_Focus, m_Second);
				// the angle of the reflection line.  The reflection matrix for a line passing through the origin is then: (A=angle)
				// ( cos2A, sin2A
				//   sin2A, -cos2A)
				// Which forms the top part of the transformation matrix.  The offset part of the transformation matrix can be calculated by translating
				// an arbitrary point onto the reflection line, applying this and translating are back again

				// But first we check if we should snap the angle
				float[] angles = null;
				if (position.SnapMode == SnapModes.Off)
				{ }
				else if (position.SnapMode == SnapModes.Shape)
				{
					// snap to angles relevant to the original shape
					if (m_Shapes.Count == 1)
						angles = m_Shapes[0].Original.GetRelevantAngles();
				}
				else if (position.SnapMode == SnapModes.Grid)
					// snap to any angles relevant to the paper
					angles = position.Page.Paper.GetRelevantAngles(true);
				else if (position.SnapMode == SnapModes.Angle)
				{
					// keep as nothing - point was already snapped by GUI
				}
				else
					Debug.Fail("Unknown snap mode");
				if (angles != null && angles.Length > 0)
				{
					Debug.Assert(angle == Geometry.NormaliseAngle(angle));
					foreach (float relevant in angles)
					{
						for (int ninety = 0; ninety <= 3; ninety++) // try all 90° rotations; the actual direction of the reflection line is not important
						{
							// and if there was a line in the base subject it makes sense to snap either parallel to this or orthogonal to it
							float test = Geometry.NormaliseAngle(relevant + ninety * Geometry.ANGLE90);
							if (Math.Abs(angle - test) < SNAPANGLETOLERANCE)
							{
								// close enough, use this angle rather than the exact one that the user set
								angle = test;
								m_Second = m_Focus + Geometry.ScalarToVector(10, angle); // must also replace the vector for the line which will be drawn
								break;
							}
						}
					}
				}
				SetMatrix(angle);
			}
			return VerbResult.Continuing;
		}

		private void SetMatrix(float angle)
		{
			// angle is angle of line away from m_ptFocus
			angle = angle - Geometry.PI / 2; // Misc.PI / 2 because .net measures clockwise from x-axis
			float cos = (float)Math.Cos(2 * angle);
			float sin = (float)Math.Sin(2 * angle);
			m_Transformation = new Matrix(cos, sin, sin, -cos, m_Focus.X * (1 - cos) - m_Focus.Y * sin, m_Focus.Y * (1 + cos) - m_Focus.X * sin);
			base.MatrixChanged();

		}

		public override VerbResult Choose(ClickPosition position)
		{
			Float(position);
			if (m_Second.IsEmpty)
			{
				if (m_Shapes.Count == 0)
				{
					Utilities.LogSubError("No shapes in xform list");
					return VerbResult.Destroyed;
				}
				m_Focus = position.Snapped;
				// until the mouse moves will place the default reflection line at right angles to the vector from the (first) shape to the reflection line
				SizeF vector = m_Shapes[0].Original.Middle().VectorTo(m_Focus);
				m_Second = m_Focus + vector.Perpendicular(1); // doesn't actually matter which direction we rotate this
				m_Bounds = CalculateBounds();
				return VerbResult.Continuing;
			}
			else
			{
				if (Geometry.DirtyDistance(m_Focus, position.Snapped) < 1)
					return VerbResult.Rejected;
				return DoTransform(position, true); // always complete
			}
		}

		public override VerbResult Complete(ClickPosition position) => Choose(position);
		public override VerbResult CompleteRetrospective() => VerbResult.Rejected;

		public override VerbResult Cancel(ClickPosition position)
		{
			if (!m_Second.IsEmpty)
			{
				m_Second = PointF.Empty;
				m_Bounds = CalculateBounds();
				foreach (Victim victim in m_Shapes)
				{
					victim.Transformed.CopyFrom(victim.Original, CopyDepth.Transform, null);
				}
				return VerbResult.Continuing;
			}
			return base.Cancel(position);
		}

		protected internal override PointF DoSnapAngle(PointF newPoint)
		{
			if (m_Second.IsEmpty)
				return newPoint;
			return Geometry.AngleSnapPoint(newPoint, m_Focus);
		}

		#endregion

		protected override RectangleF CalculateBounds()
		{
			// the reflection line is infinite
			if (!m_Second.IsEmpty)
				return new RectangleF(-10000, -10000, 20000, 20000);
			RectangleF bounds = Geometry.RectangleFromPoint(m_Focus, TransformFocal.FOCUSSIZE + 0.5f);
			Geometry.Extend(ref bounds, base.CalculateBounds());
			return bounds;
		}

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			if (!m_Second.IsEmpty)
				return new RectangleF(-10000, -10000, 20000, 20000);
			return base.RefreshBounds(withShadow);
		}

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			if (m_Mode == Modes.Move && !m_Second.IsEmpty)
			{
				// with most transformations when moving the original shape is not shown, but for the reflection it doesn't make much sense without the original shape
				// so it is displayed somewhat still...
				foreach (Victim victim in m_Shapes)
				{
					victim.Original.Draw(gr, scale, coordScale, view, buffer, fillAlpha / 2, edgeAlpha / 2, reverseRenderOrder);
				}
			}
			base.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			if (!m_Second.IsEmpty)
			{
				using (Stroke pn = gr.CreateStroke(Color.FromArgb(150, Color.Red), 0.6f * scale))
				{
					pn.DashStyle = DashStyle.Dash;
					SizeF temp = m_Focus.VectorTo(m_Second);
					temp = temp.ChangeLength(1000); // makes an effectively infinite line
					gr.DrawLine(m_Focus - temp, m_Focus + temp, pn);
				}

			}
		}

		#region Transforming values
		public override void TransformScalar(ref float length)
		{ }
		public override void TransformAngle(ref float angle)
		{
			angle = Geometry.PI - angle;
		}
		public override void TransformDirection(ref int direction)
		{
			direction = -direction;
		}

		#endregion

	}
	#endregion

	#region Affine

	/// <summary>Implementation of Transformation that just applies any affine matrix.  This does not behave as a user tool like the others, 
	/// but rather is a placeholder for applying transforms to the contents</summary>
	public class TransformAffine : Transformation
	{
		public TransformAffine(Modes mode, Matrix matrix) : base(mode)
		{
			m_Transformation = matrix;
		}

		#region Not applicable as this isn't a shape
		public override Shapes ShapeCode => throw new InvalidOperationException();
		public override VerbResult Float(ClickPosition position) => throw new InvalidOperationException();
		public override AllowedActions RequiredAllowed => AllowedActions.TransformLinearStretch | AllowedActions.TransformMove | AllowedActions.TransformScale;
		protected override string RequiredAllowedMessage => throw new InvalidOperationException();
		public override bool Active() => true; // but not actually applicable as this isn't a shape?

		#endregion

		public override void TransformScalar(ref float length)
		{ // only used by circles
			Utilities.LogSubError("TransformAngle ignored for TransformAffine");
		}

		public override void TransformAngle(ref float angle)
		{ // only used by arcs
			Utilities.LogSubError("TransformAngle ignored for TransformAffine");
		}

		public override void TransformDirection(ref int direction)
		{
			var elements = m_Transformation.Elements;
			if (elements[0] * elements[3] < 0) // m11 * m22 - checking for reflections in X or Y
				direction = -direction;// not sure that's 100% correct, but it's close enough for this
		}

	}

	#endregion

}
