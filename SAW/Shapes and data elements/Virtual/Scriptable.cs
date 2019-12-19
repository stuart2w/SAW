using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Linq;

namespace SAW
{
	/// <summary>Used to add scripting support to any existing element.  Always contains one and only one other shape</summary>
	public class Scriptable : Shape, IShapeContainer
	{
		/// <summary>The visual element for this - usually a SAW.Item, but can be anything</summary>
		public Shape Element { get; private set; }
		private IShapeContainer ElementAsContainer
		{ get { return Element as IShapeContainer; } }

		#region Properties supporting active functionality
		/// <summary>Was stored as string in SAW6, but appears to always behave as int, and scripts referencing an ID could only accept integers</summary>
		public int SAWID { get; set; }

		/// <summary>item is a popup and can be hidden</summary>
		public bool Popup;
		/// <summary>item is visible.  Default true</summary>
		public bool Shown = true;
		/// <summary>In milliseconds.  -1 indicates no repeat scripts. </summary>
		public int RepeatTimeout = -1;
		public bool NotVisited;
		/// <summary>The Auto Repeat of standard Select scripts;  not whether the repeat scripts are used;  this causes entire item to repeat</summary>
		public bool AutoRepeat;
		public bool ResetSwap;

		#endregion

		#region Scripts

		/// <summary>The 6 types of script; can be used as an index into the array of scripts</summary>
		public enum ScriptTypes
		{
			Visit = 0,
			Select,
			Next,
			PreRepeat,
			Repeat,
			PostRepeat
		}

		public readonly Script[] Scripts = new Script[6];

		public Script VisitScript
		{
			[DebuggerStepThrough]
			get { return Scripts[0]; }
			set { Scripts[0] = value; }
		}

		public Script SelectScript
		{
			[DebuggerStepThrough]
			get { return Scripts[1]; }
			set { Scripts[1] = value; }
		}

		public Script NextScript
		{
			[DebuggerStepThrough]
			get { return Scripts[2]; }
			set { Scripts[2] = value; }
		}

		public Script PreRepeatScript
		{
			[DebuggerStepThrough]
			get { return Scripts[3]; }
			set { Scripts[3] = value; }
		}

		public Script RepeatScript
		{
			[DebuggerStepThrough]
			get { return Scripts[4]; }
			set { Scripts[4] = value; }
		}

		public Script PostRepeatScript
		{
			[DebuggerStepThrough]
			get { return Scripts[5]; }
			set { Scripts[5] = value; }
		}

		public Script GetScript(ScriptTypes which)
		{
			return Scripts[(int)which];
		}

		public void SetScript(ScriptTypes which, Script value)
		{
			Scripts[(int)which] = value;
		}

		/// <summary>Gets the default script from the document based on whether the contained SAW item is a group, or an escape item, or neither</summary>
		public Script GetDefaultScript(ScriptTypes which, Document doc)
		{
			if ((Element as Item)?.IsGroup ?? false)
				return doc.DefaultGroupScripts.GetScript(which);
			if ((Element as Item)?.IsEscape ?? false)
				return doc.DefaultEscapeScripts.GetScript(which);
			return doc.DefaultItemScripts.GetScript(which);
		}

		/// <summary>True if this uses the 3 custom repeating scripts.  (Even if false the basic SAW script can repeat if AutoRepeat=true</summary>
		public bool HasRepeatingScript
		{ get { return RepeatTimeout >= 0; } }

		#endregion

		public Scriptable()
		{ }

		public Scriptable(Shape element)
		{
			Element = element;
			Element.Parent = this;
		}

		#region Information
		public override Shapes ShapeCode => Shapes.Scriptable;

		public override string Description => SAWID + ": " + Element.Description;

		public override void Diagnostic(StringBuilder output)
		{
			base.Diagnostic(output);
			output.Append("SAW ID=").AppendLine(SAWID.ToString());
		}

		public override string ToString() => Description;

		public override bool IsVisible => Shown && base.IsVisible;

		public override bool IsFilled => Element.IsFilled;

		public override AllowedActions Allows
		{get { return Element.Allows & ~(AllowedActions.Group | AllowedActions.ConvertToPath | AllowedActions.MoveToPixels | AllowedActions.Merge); }}

		public override GeneralFlags Flags
		{get{return (Element?.Flags ?? GeneralFlags.None) | GeneralFlags.DoubleClickAfterCreation;}}

		public override string StatusInformation(bool ongoing)
		{
			StringBuilder output = new StringBuilder();
			if (!ongoing)
				output.Append(Strings.Item("ID")).Append("=").Append(SAWID).Append("   ");
			var b = Bounds;
			output.Append(GUIUtilities.RectangleToUserString(b, FindPage()));
			//output.Append(Strings.Item("SAW_Edit_Bounds")).Append("={").Append(b.Left.ToString("0")).Append(" ").Append(b.Top.ToString("0")).Append(" ").Append(b.Width.ToString("0")).Append(" ").Append(b.Height.ToString("0")).Append("}");
			if (Popup)
				output.Append("  Is Popup");
			return output.ToString();
		}

		#endregion

		#region Verbs - just call through to content (creating a SAWItem) except DoubleClick and CompleteRetrospective can be used when created externally
		public override VerbResult Start(EditableView.ClickPosition position)
		{
			Element = new Item {Parent = this};
			SetUniqueID();
			return Element.Start(position);
		}

		public override VerbResult Cancel(EditableView.ClickPosition position)
		{
			m_Bounds = RectangleF.Empty;
			return Element.Cancel(position);
		}

		public override VerbResult Choose(EditableView.ClickPosition position)
		{
			m_Bounds = RectangleF.Empty;
			return Element.Choose(position);
		}

		public override VerbResult Float(EditableView.ClickPosition position)
		{
			m_Bounds = RectangleF.Empty;
			return Element.Float(position);
		}

		public override VerbResult Complete(EditableView.ClickPosition position)
		{
			m_Bounds = RectangleF.Empty;
			return Element.Complete(position);
		}

		public override VerbResult CompleteRetrospective()
		{
			m_Bounds = RectangleF.Empty;
			return Element.CompleteRetrospective();
		}

		public override string DoubleClickText() => Element.DoubleClickText();

		public override void DoDoubleClick(EditableView view, EditableView.ClickPosition.Sources source)
		{
			Element.DoDoubleClick(view, source);
		}

		public override bool CanDoubleClickWith(IEnumerable<Shape> selection)
		{ // a group of items (within scriptables) can be edited together
			return selection.All(shape => (shape as Scriptable)?.Element is Item);
		}

		#endregion

		#region Data

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(Element);
			writer.Write(SAWID);
			writer.Write(Popup);
			writer.Write(Shown);
			writer.Write(AutoRepeat);
			writer.Write(NotVisited);
			if (writer.Version >= 123)
				writer.Write(RepeatTimeout);
			// for each script a boolean is written first to indicate if it exists (true), or is null  (false)
			for (int i = 0; i < Scripts.Length; i++)
			{
				writer.Write(Scripts[i] != null);
				Scripts[i]?.Write(writer);
			}
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			Element = (Shape)reader.ReadData();
			if (Element != null)
				Element.Parent = this;
			SAWID = reader.ReadInt32();
			Popup = reader.ReadBoolean();
			Shown = reader.ReadBoolean();
			AutoRepeat = reader.ReadBoolean();
			NotVisited = reader.ReadBoolean();
			if (reader.Version >= 123)
				RepeatTimeout = reader.ReadInt32();
			for (int i = 0; i < Scripts.Length; i++)
			{
				if (reader.ReadBoolean())
				{
					Scripts[i] = new Script();
					Scripts[i].Read(reader);
				}
			}
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			// this is slightly flaky for Undo still, in that the scriptable must be put into the transaction FIRST
			// (otherwise it will refernce differing copies of the contained element)
			base.CopyFrom(other, depth, mapID);
			Scriptable scriptable = (Scriptable)other;
			//Debug.WriteLine("Scriptable.CopyFrom, depth=" + depth + " ID=" + scriptable.SAWID);
			// contained element.  Works like ShapeStack containment - Element is tightly bound within this
			if (depth == CopyDepth.Transform && Element != null)
			{
				// first time must copy reference, below
				if (Element != scriptable.Element)
					Element?.CopyFrom(scriptable.Element, depth, mapID);
			}
			else// if (depth == CopyDepth.Duplicate)
			{
				if (mapID?.ContainsKey(scriptable.Element.ID) ?? false)
					Element = (Shape)mapID[scriptable.Element.ID];
				else
					Element = (Shape)scriptable.Element.Clone(mapID ?? new Mapping()); // use actual mapID if there is one, otherwise need a real one as Container aspects of Item don't like Ignore
				Element.Parent = this;
			}
			//else
			//{
			//	Element = scriptable.Element;
			//}

			////else if (depth == CopyDepth.Duplicate)
			////{
			////	if (mapID == null)
			////		mapID = Mapping.Ignore;
			////	Element = (Shape)scriptable.Element.Clone(mapID);
			////	Element.Parent = this;
			////}
			//else
			//{
			//	// undo requires all shapes are copied, because they could later be edited by styling changes
			//	//Element = Shape.CreateShape(scriptable.Element.ShapeCode); // <- this is inadequate as it doesn't clone contents of the items which leaves some reference problems
			//	//Element.CopyFrom(scriptable.Element, depth, null);
			//	Element = (Shape)scriptable.Element.Clone(mapID ?? new Mapping()); // use actual mapID if there is one, otherwise need a real one as Container aspects of Item don't like Ignore
			//	Element.Parent = this;
			//}

			// other fields:
			if (depth > CopyDepth.Transform)
			{
				SAWID = scriptable.SAWID;
				Popup = scriptable.Popup;
				Shown = scriptable.Shown;
				AutoRepeat = scriptable.AutoRepeat;
				RepeatTimeout = scriptable.RepeatTimeout;
				for (int i = 0; i < Scripts.Length; i++)
					Scripts[i] = scriptable.Scripts[i]?.Clone();
			}
			m_Bounds = RectangleF.Empty; // because content may have moved - especially on Undo	
		}

		protected override bool IdenticalToShape(Shape other)
		{
			if (!base.IdenticalToShape(other))
				return false;
			if (!Element.IdenticalTo(other))
				return false;
			Scriptable scriptable = (Scriptable)other;
			if (Popup != scriptable.Popup || Shown != scriptable.Shown)
				return false;
			for (int i = 0; i < Scripts.Length; i++)
				if (Scripts[i] != null && scriptable.Scripts[i] != null && !Scripts[i].IdenticalTo(scriptable.Scripts[i]))
					return false;
			return true;
		}

		public void SetUniqueID()
		{
			SAWID = (from p in Globals.Root.CurrentDocument.Pages select p.FindHighestUsedID()).Max() + 1;
		}

		public override void WriteExportText(IndentStringBuilder output)
		{
			output.Append("Item ").AppendLine(SAWID);
			output.Indent += 2;
			output.Append("Bounds = ").AppendLine(Bounds.ToString());
			if (Popup) output.AppendLine("Popup");
			if (!Shown) output.AppendLine("Not shown");
			if (AutoRepeat) output.AppendLine("Auto repeat");
			if (RepeatTimeout >= 0) output.Append("Has custom repeating scripts with timeout = ").AppendLine(RepeatTimeout);
			if (ResetSwap) output.AppendLine("Reset swap");
			if (NotVisited) output.AppendLine("Not visited");
			for (int script = 0; script < 6; script++)
			{
				if (!(Scripts[script]?.IsEmpty ?? true))
					Scripts[script].WriteExportText(output, ((ScriptTypes)script) + " script");
			}
			Element?.WriteExportText(output);
			output.Indent -= 2;
			output.Append("End of Item ").AppendLine(SAWID);
		}

		#endregion

		#region Coordinates
		protected override RectangleF CalculateBounds() => Element.Bounds;

		public override RectangleF RefreshBounds(bool withShadow = false) => Element.RefreshBounds(withShadow);

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			if (!Shown)
				return false;
			return Element.HitTestDetailed(clickPoint, scale, treatAsFilled);
		}

		internal override void AddIntersection(Intersection intersection)
		{
			Element.AddIntersection(intersection);
		}

		public override void CheckIntersectionsWith(Shape shape)
		{
			Element.CheckIntersectionsWith(shape);
		}

		public override void CheckIntersectionsWithBezier(Shape shape, int shapeIndex, PointF[] Q)
		{
			Element.CheckIntersectionsWithBezier(shape, shapeIndex, Q);
		}

		public override void CheckIntersectionsWithLine(Shape shape, int shapeIndex, float shapeParameter, PointF A, PointF B, bool ignoreEnd)
		{
			Element.CheckIntersectionsWithLine(shape, shapeIndex, shapeParameter, A, B, ignoreEnd);
		}

		public override void CheckIntersectionsWithSelf()
		{
			Element.CheckIntersectionsWithSelf();
		}

		public override List<UserSocket> GetPointsWhichSnapWhenMoving()
		{
			return Element.GetPointsWhichSnapWhenMoving();
		}

		public override List<Target> GenerateTargets(UserSocket floating)
		{
			return Element.GenerateTargets(floating);
		}

		#endregion

		#region Graphics
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			// this would draw this item itself, which has no representation.  It is the Draw et cetera methods which draw the content item
		}

		public override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			if (!Shown)
				return;
			Element.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
		}

		public override void DrawHighlight(Canvas gr, float scale, float coordScale)
		{
			//if (!Shown)
			//	return;
			Element.DrawHighlight(gr, scale, coordScale);
		}

		#endregion

		#region Other delegated to content

		public override void InitialiseFreeStanding()
		{ // called by GUI if it creates one of these for info - Start will not have been called, so must create the content element
			base.InitialiseFreeStanding();
			Element = new Item();
			Element.InitialiseFreeStanding();
		}

		public override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false)
		{
			return Element?.StyleObjectForParameter(parameter, applyingDefault);
		}

		public override void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			Element?.NotifyStyleChanged(parameter, oldValue, newValue);
		}

		public override bool DefaultStylesApplied()
		{
			return Element.DefaultStylesApplied();
		}

		public override List<GrabSpot> GetGrabSpots(float scale)
		{
			return Element.GetGrabSpots(scale);
		}

		public override void DoGrabAngleSnap(GrabMovement move)
		{
			Element.DoGrabAngleSnap(move);
		}

		protected override void DoGrabMove(GrabMovement move)
		{
			//base.DoGrabMove(move);
			Element.GrabMove(move);
		}

		public override GrabMovement GetCustomGrabMove(EditableView.ClickPosition current, EditableView.ClickPosition click, Transaction transaction)
		{
			return Element.GetCustomGrabMove(current, click, transaction);
		}

		public override bool StartGrabMove(GrabMovement grab)
		{
			base.StartGrabMove(grab);
			return Element.StartGrabMove(grab);
		}

		public override void ApplyTransformation(Transformation transformation)
		{
			m_Bounds = RectangleF.Empty;
			base.ApplyTransformation(transformation);
			Element.ApplyTransformation(transformation);
		}

		#endregion

		#region IShapeParent and related items
		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			Parent.NotifyIndirectChange(shape, affected);
			if ((affected & ChangeAffects.Bounds) > 0)
				m_Bounds = Rectangle.Empty;
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			Parent.NotifyIndirectChange(shape, affected, area);
			if ((affected & ChangeAffects.Bounds) > 0)
				m_Bounds = Rectangle.Empty;
		}

		internal override void AddToFlatList(List<Shape> list, FlatListPurpose purpose)
		{
			// We don't call through to the base class which would add this in most cases; instead we have custom logic for whether this shape is included
			Element.AddToFlatList(list, purpose);
			switch (purpose)
			{
				case FlatListPurpose.ExposedShapes: // don't want to expose contained item itself - this shape substitutes for it
					if (list.Contains(Element))
						list.Remove(Element);
					break;
				case FlatListPurpose.HasTexture:
					break;
				default:
					list.Add(this);
					break;
			}
		}

		public override void Iterate(DatumFunction fn)
		{
			base.Iterate(fn);
			Element?.Iterate(fn); // may be null for default scripts
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
			base.UpdateReferencesObjectsCreated(document, reader);
			Element?.UpdateReferencesObjectsCreated(document, reader);
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			Element?.UpdateReferencesIDsChanged(mapID, document);
			// this function is called on each object when a group have been copied and pasted
			// we can use this opportunity to set the SAW ID
			SetUniqueID();
		}

		public override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			Element?.NotifyEnvironmentChanged(change);
		}


		#endregion

		#region IShapeContainer delegates to Element where it is IShapeContainer
		/// <summary>Empty list that can be returned as needed if Element is not a container</summary>
		private static readonly List<Shape> EmptyList = new List<Shape>();

		public IEnumerator<Shape> GetEnumerator()
		{
			if (!(Element is IShapeContainer))
				return EmptyList.GetEnumerator();
			return ElementAsContainer.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (!(Element is IShapeContainer))
				return EmptyList.GetEnumerator();
			return ((IEnumerable)ElementAsContainer).GetEnumerator();
		}

		public IEnumerable<Shape> Reverse
		{ get { return (Element as IShapeContainer)?.Reverse ?? EmptyList; } }

		public void FinishedModifyingContents(Transaction transaction, GrabMovement move = null)
		{
			ElementAsContainer?.FinishedModifyingContents(transaction, move);
		}

		public List<Shape> Contents => (Element as IShapeContainer)?.Contents;

		public bool MoveWithin(List<Shape> shapes, PointF target, GrabMovement move, Transaction transaction)
		{
			if (!(Element is IShapeContainer))
				return false;
			return ElementAsContainer.MoveWithin(shapes, target, move, transaction);
		}

		public bool AllowClick(PointF target) => Container.AllowClick(target);

		public IShapeContainer AsParentContainer => Container;

		public void CheckZ(Shape shp)
		{ // function is called on parent of shp - so it should be our element
			Debug.Assert(shp == Element); // no need to check index really
		}

		public override IShapeContainer AsContainer => Element.AsContainer;

		public override IShapeTarget AsTarget => Element.AsTarget;

		#endregion

	}
}