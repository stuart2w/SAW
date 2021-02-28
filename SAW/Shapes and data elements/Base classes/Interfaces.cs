using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAW
{
	/// <summary>Each shape has a parent property, which should point to one of these.  At the moment it will be a page or a group.
	/// Was "IShapeContainer", which may now be used for dynamic containers</summary>
	[Flags]
	public enum ChangeAffects
	{
		RepaintNeeded = 1,
		Bounds = 2,
		/// <summary>Indicates that previous GrabSpots are no longer valid and must be regenerated</summary>
		GrabSpots = 4,
//		Intersections = 8,
		//IterateRainbowOnTransform = 16,
		Diagnostic = 32,
		/// <summary>GUI will update applicability and refill the palettes</summary>
		StyleInformation = 64,
		/// <summary>Should only be fired in response to verbs which might not be expected to change the state (e.g. some keys)
		/// at the moment the GUI does not check that this object is responsible for prompts, so firing this excessively could cause excessive updates</summary>
		UpdatePrompts = 128,
		MoveMouse = 0x100,
		ApplyDefaultStyles = 0x200,
		/// <summary>Tells GUI to update with the buttons which performs functions are enabled or disabled</summary>
		/// <remarks>Actually used by NumberLine because it also triggers the change of colour of the counting button</remarks>
		UpdateVerbButtonApplicability = 0x400,
		/// <summary>Sets or clears (empty rect provided) a DATA rectangle over which floating palette forms cannot go</summary>
		SetPaletteExclusion = 0x800,

		Test = 0x8000
	}

	public interface IShapeParent
	{
		// this allows the shape to send some notifications back out, usually about GUI type things which need updating
		// NOTE: only unexpected changes need to be notified.  The GUI will automatically update graphics, intersections, GrabSpots as changes are made
		// a shape only needs to send a notification if it does something out of the ordinary; e.g. a timed update to itself
		// or responses to double click (where there is no real default)

		void NotifyIndirectChange(Shape shape, ChangeAffects affected);
		void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area); // Not allowed Optional sadly with RectangleF
																						 // rctArea only used for some updates - can be used for Repaint to repaint just part of screen
																						 // first function must be identical to passing (,,RectangleF.Empty) to second version
	}

	/// <summary>Implemented only by shapes (and Page) which expose the shapes that they contain, especially to mouse clicking.
	/// Other things, such as Group, can contain shapes which they encapsulates, without implementing this interface</summary>
	/// <remarks>Shapes implementing this must also implement IShapeParent and IShapeTarget</remarks>
	public interface IShapeContainer : IEnumerable<Shape>, IShapeParent
	{

		/// <summary>Enumerates the shapes in reverse order</summary>
		/// <remarks>Mainly for the benefit of Page for which it may be fairly efficient to a a numerate them all first to reverse them (it returns a Page.ReverseEnumerator which is also its own IEnumerable)</remarks>
		IEnumerable<Shape> Reverse { get; }

		/// <summary>Should be called after caller has finished making edits to Contents</summary>
		/// <param name="transaction">Transaction being used.  Can be nothing if this is not under transaction control</param>
		/// <param name="move">If this is actually happening during a grab movement the control object can be provided.  This will be nothing when this function is genuinely called at the end of some update.</param>
		/// <remarks>Some containers may want to edit the shapes further</remarks>
		void FinishedModifyingContents(Transaction transaction, Shape.GrabMovement move = null);

		/// <summary> List of shapes (excludes measures if a page)</summary>
		/// <returns>The internal List of contents</returns>
		/// <remarks>Note that if this is edited ResetZ should be called if necessary</remarks>
		List<Shape> Contents { get; }

		/// <summary>Used when dragging shapes to allow shape to be repositioned by the container when being dragged within the container</summary>
		/// <param name="shapes">The shapes which are being moved</param>
		/// <param name="target"></param>
		/// <param name="move"></param>
		/// <param name="transaction"></param>
		/// <returns>True if any changes were made, in which case FinishedModifyingContents will later be called</returns>
		/// <remarks>Will be ignored unless the container wants to position the shapes precisely (e.g. Flow).  All of the shapes will already be inside the container (the View will already have put them in here)</remarks>
		bool MoveWithin(List<Shape> shapes, PointF target, Shape.GrabMovement move, Transaction transaction);

		/// <summary>Returns true if contained shapes allowed to respond to clicks at this coordinate.  
		/// Is called on the parent of the deepest shape and should recurse up to parents</summary>
		/// <remarks>Will return false is the container implements some sort of clipping graphics</remarks>
		bool AllowClick(PointF target);

		/// <summary>Nominally returns this shape but can be changed as shims.  Call X.Parent.AsParentContainer to get MEANINGFUL container of X</summary>
		IShapeContainer AsParentContainer{ get;}


#if DEBUG
		/// <summary>Should verify that the Z-index of the given shape is set correctly</summary>
		void CheckZ(Shape shp);
#endif
	}

	/// <summary>Implemented by any shape which lights up when something is dropped on it</summary>
	public interface IShapeTarget
	{
		/// <summary>Draws a highlight indicating that the shape be moved/created will be inside this container.  Not valid for page</summary>
		/// <remarks>DrawHighlight, which is used for selected shapes also highlights the contents.  This draws the same, but only this one shape</remarks>
		void DrawHover(Canvas gr, float scale, float coordScale);

		/// <summary>Returns true if the given shape can be dropped into this shape</summary>
		bool Accept(Shape shape);

		/// <summary>Called at the end of moving shapes if it/they have been dropped on a target which is not also a container</summary>
		/// <param name="shape">A shape, for which Accept returns true</param>
		/// <param name="objTransaction"></param>
		/// <remarks>This will never be called if the shape implements IShapeContainer, and such shapes should just throw an exception in this method</remarks>
		void DropNotContainer(Shape shape, Transaction objTransaction);

	}

	/// <summary>Implemented by any shapes which can automatically resize themselves</summary>
	/// <remarks>Currently just Flow.  However this is in a separate interface to allow for more complex resizing later</remarks>
	public interface IAutoSize
	{
		/// <summary>Shape should resize self to match</summary>
		/// <param name="target">Requested size.  Either Width or Height, OR NEITHER, may be 0 indicating that the OTHER item is a maximum.
		/// Eg (100,0) indicates that the width is fixed at 100, and the height should be chosen to match; the width SHOULD be 100, but can be smaller if appropriate
		/// If both defined it is a recommendation and any value can be chosen.</param>
		/// <returns>Returns the size  after resizing (i.e. = Bounds.Size, but it is easier to return it is in this instance it is clunky for the caller to access the shape)</returns>
		SizeF AutoSize(SizeF target);
		/// <summary>Tells shape to change its current size</summary>
		void SetBounds(RectangleF bounds, Transaction transaction);
	}
}
