using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using SAW.Functions;

namespace SAW
{
	/// <summary>Used to add scripting support to any existing element.  Always contains one and only one other shape</summary>
	public class Scriptable : Shape, IShapeContainer
	{
		/// <summary>The visual element for this - usually a SAW.Item, but can be anything</summary>
		public Shape Element { get; private set; }
		private IShapeContainer ElementAsContainer => Element as IShapeContainer;

		/// <summary>Shouldn't generally be used, but unavoidable for loading SAW6 files </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal void SetElement(Shape element)
		{
			Element = element;
			element.Parent = this;
		}

		#region Properties supporting active functionality

		public ButtonShape.States State;

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

		// fields that were in Item in SAW7
		public SharedReference<SharedResource> Sound;
		public string OutputText;
		public bool OutputAsDisplay;
		public string SpeechText;
		public bool SpeechAsDisplay;
		public string PromptText;

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
		{
			HighlightStyle = new HighlightStyleC();
			HighlightStyle.SetDefaults();
		}

		public Scriptable(Shape element) : this()
		{
			Element = element;
			Element.Parent = this;
		}

		#region Information
		public override Shapes ShapeCode => Shapes.Scriptable;

		internal override string Description => SAWID + ": " + Element.Description;

		public override void Diagnostic(StringBuilder output)
		{
			base.Diagnostic(output);
			output.Append("SAW ID=").AppendLine(SAWID.ToString());
		}

		public override string ToString() => Description;

		public override bool IsVisible => Shown && base.IsVisible;

		public override bool IsFilled => Element.IsFilled;

		public override AllowedActions Allows
		{ get { return Element.Allows & ~(AllowedActions.Group | AllowedActions.ConvertToPath | AllowedActions.MoveToPixels | AllowedActions.Merge); } }

		public override GeneralFlags Flags
		{ get { return (Element?.Flags ?? GeneralFlags.None) | GeneralFlags.DoubleClickAfterCreation; } }

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
			Element = new Item { Parent = this };
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

		#endregion

		#region Property editor stuff

		internal override string DoubleClickText()
		{
			if (Element is Item)
				return Strings.Item("SAW_Edit_Item");
			return Strings.Item("SAW_Edit_Scriptable");
		}

		internal override void DoDoubleClick(EditableView view, EditableView.ClickPosition.Sources source)
		{
			LastDoubleClickResult = frmSAWItem.Display(view) == DialogResult.OK;
			if (!LastDoubleClickResult)
				Parent.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded);
			// why was this added in v8 - causes it to be left this way usually, which is invalid?
			//else
			//	Status = StatusValues.Moved;
		}

		internal override bool CanDoubleClickWith(IEnumerable<Shape> selection)
		{ // a group of items (within scriptables) can be edited together
			bool hasItems = selection.Any(shape => (shape as Scriptable)?.Element is Item);
			if (!hasItems && selection.Count() != 1)
				return false;// if editing graphical objects then it's only really the scripts which are useful - so only one can be edited
							 // (since scripts aren't edited if more than 1 is selected)
			return selection.All(shape => (shape is Scriptable));
		}

		/// <summary>Called by editor dialog to force this to discard some cache items and repaint</summary>
		public void WasEdited()
		{
			// editor also calls on each Item, which will do a repaint, so this is redundant IFF the contained Element is Item.
			// but if it's another graphical object, then this is needed
			Parent?.NotifyIndirectChange(this, ChangeAffects.RepaintNeeded); // note ? is needed for property editor which can make a dummy Item with no parent
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
			if (writer.Version >= 129)
			{
				HighlightStyle.Save(writer);
				writer.Write(Sound?.ID ?? Guid.Empty);
				writer.WriteBufferedString(OutputText ?? "");
				writer.Write(OutputAsDisplay);
				writer.Write(SpeechText ?? "");
				writer.Write(SpeechAsDisplay);
				writer.Write(PromptText ?? "");
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
			if (reader.Version < 129)
			{
				if (Element is Item item)
				{
					HighlightStyle = item.LoadedV7Data.HighlightStyle;
					Sound = item.LoadedV7Data.Sound;
					OutputText = item.LoadedV7Data.OutputText;
					OutputAsDisplay = item.LoadedV7Data.OutputAsDisplay;
					SpeechText = item.LoadedV7Data.SpeechText;
					SpeechAsDisplay = item.LoadedV7Data.SpeechAsDisplay;
					PromptText = item.LoadedV7Data.PromptText;
				}
			}
			else
			{
				HighlightStyle = HighlightStyleC.Read(reader);
				Sound = SharedReference<SharedResource>.FromGUID(reader.ReadGuid());
				OutputText = reader.ReadBufferedString();
				OutputAsDisplay = reader.ReadBoolean();
				SpeechText = reader.ReadString();
				SpeechAsDisplay = reader.ReadBoolean();
				PromptText = reader.ReadString();
			}
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			// this is slightly flaky for Undo still, in that the scriptable must be put into the transaction FIRST
			// (otherwise it will refernce differing copies of the contained element)
			base.CopyFrom(other, depth, mapID);
			Scriptable scriptable = (Scriptable)other;
			if (depth > CopyDepth.Transform)
				CopyPresentationFrom(scriptable, false);
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

		public void CopyPresentationFrom(Scriptable item, bool noOverwriteExisting)
		{ // see Item
			if (HighlightStyle == null)
				HighlightStyle = new HighlightStyleC();
			HighlightStyle.CopyFrom(item.HighlightStyle);
			if (!noOverwriteExisting || Sound == null)
				Sound = item.Sound?.Clone();
			if (!noOverwriteExisting || string.IsNullOrEmpty(OutputText))
				OutputText = item.OutputText;
			OutputAsDisplay = item.OutputAsDisplay;
			if (!noOverwriteExisting || string.IsNullOrEmpty(SpeechText))
				SpeechText = item.SpeechText;
			SpeechAsDisplay = item.SpeechAsDisplay;
			if (!noOverwriteExisting || string.IsNullOrEmpty(PromptText))
				PromptText = item.PromptText;
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

		public override RectangleF RefreshBounds(bool withShadow = false)
		{
			RectangleF bounds = Element.RefreshBounds(withShadow);
			// mostly copied from Lined.RefreshBoundsFromBounds

			float width = HighlightStyle.LineWidth * 2;
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

		public override bool VertexVerbApplicable(Codes code, Target target)
		{
			if (target.Shape == Element) // can't call base as it asserts that it is the right shape
				return Element.VertexVerbApplicable(code, target);
			return base.VertexVerbApplicable(code, target);
		}

		public override bool HitTestDetailed(PointF clickPoint, float scale, bool treatAsFilled)
		{
			if (!Shown)
				return false;
			return Element.HitTestDetailed(clickPoint, scale, treatAsFilled);
		}
		internal override List<UserSocket> GetPointsWhichSnapWhenMoving() => Element.GetPointsWhichSnapWhenMoving();
		internal override List<Target> GenerateTargets(UserSocket floating) => Element.GenerateTargets(floating);
		public override (GrabSpot[], string[]) GetEditableCoords(Target selectedElement) => Element.GetEditableCoords(selectedElement);

		#endregion

		#region Highlight styling

		public HighlightStyleC HighlightStyle;

		internal override bool DefaultStylesApplied()
		{
			HighlightStyle.LineColour = Color.Red;
			HighlightStyle.TextColour = Color.Red;
			HighlightStyle.FillColour = Color.White;
			HighlightStyle.LineWidth = (((LineStyleC)Element.StyleObjectForParameter(Parameters.LineWidth)).Width + 1) / 2;
			return Element.DefaultStylesApplied();
		}

		public class HighlightStyleC : StyleBase
		{
			public Color LineColour;
			public Color FillColour;
			public Color TextColour;
			public float LineWidth;

			public override int ParameterValue(Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.LineColour: return LineColour.ToArgb();
					case Parameters.FillColour: return FillColour.ToArgb();
					case Parameters.TextColour: return TextColour.ToArgb();
					case Parameters.LineWidth: return (int)(LineWidth * 100); // the GUI parameter only uses integers
					default:
						Debug.Fail("Parameter not expected for this type of style object");
						return 0;
				}
			}

			public override void SetParameterValue(int value, Parameters parameter)
			{
				switch (parameter)
				{
					case Parameters.LineColour: LineColour = Color.FromArgb(value); break;
					case Parameters.FillColour: FillColour = Color.FromArgb(value); break;
					case Parameters.TextColour: TextColour = Color.FromArgb(value); break;
					case Parameters.LineWidth:
						LineWidth = value / 100f;
						break;
				}
			}

			public override void CopyFrom(StyleBase other)
			{
				HighlightStyleC highlight = (HighlightStyleC)other;
				LineColour = highlight.LineColour;
				FillColour = highlight.FillColour;
				TextColour = highlight.TextColour;
				LineWidth = highlight.LineWidth;
			}

			public override Parameters[] ApplicableParameters()
			{
				return new Parameters[] { Parameters.LineWidth, Parameters.LineColour, Parameters.FillColour, Parameters.TextColour };
			}

			public override bool IdenticalTo(StyleBase other)
			{
				HighlightStyleC highlight = (HighlightStyleC)other;
				return LineColour.Equals(highlight.LineColour)
				&& FillColour.Equals(highlight.FillColour)
				&& TextColour.Equals(highlight.TextColour)
				&& LineWidth == highlight.LineWidth;
			}

			/// <summary>Sets very generic defaults without knowing what the element is </summary>
			public void SetDefaults()
			{
				LineColour = Color.Red;
				FillColour = Color.White;
				TextColour = Color.Red;
				LineWidth = 2;
			}

			public static HighlightStyleC Read(DataReader reader)
			{
				HighlightStyleC newStyle = new HighlightStyleC();
				newStyle.FillColour = reader.ReadColour();
				newStyle.TextColour = reader.ReadColour();
				newStyle.LineColour = reader.ReadColour();
				newStyle.LineWidth = reader.ReadSingle();
				if (newStyle.LineWidth < 0 || newStyle.LineWidth > 100)
					throw new InvalidDataException("HighlightStyleC.Width out of bounds");
				return newStyle;
			}

			public void Save(DataWriter writer)
			{
				writer.Write(FillColour);
				writer.Write(TextColour);
				writer.Write(LineColour);
				writer.Write(LineWidth);
			}

			/// <summary>Creates an instance initialised with the current (non-highlight) styles stored in a shape</summary>
			public static HighlightStyleC FromShape(Shape shape)
			{
				var highlight = new HighlightStyleC();
				FillStyleC fill = (FillStyleC)shape.StyleObjectForParameter(Parameters.FillColour);
				if (fill != null)
					highlight.FillColour = fill.Colour;
				LineStyleC line = (LineStyleC)shape.StyleObjectForParameter(Parameters.LineColour);
				if (line != null)
				{
					highlight.LineColour = line.Colour;
					highlight.LineWidth = (line.Width + 1) / 2;
				}
				TextStyleC text = (TextStyleC)shape.StyleObjectForParameter(Parameters.TextColour);
				if (text != null)
					highlight.TextColour = text.Colour;
				return highlight;
			}

			/// <summary>Writes the styles from this object into the normal (non-highlight) styles in the shape</summary>
			public void ApplyToShape(Shape shape)
			{
				FillStyleC fill = (FillStyleC)shape.StyleObjectForParameter(Parameters.FillColour);
				if (fill != null)
					fill.Colour = FillColour;
				LineStyleC line = (LineStyleC)shape.StyleObjectForParameter(Parameters.LineColour);
				if (line != null)
				{
					line.Colour = LineColour;
					line.Width = LineWidth * 2 - 1;
				}
				TextStyleC text = (TextStyleC)shape.StyleObjectForParameter(Parameters.TextColour);
				if (text != null)
					text.Colour = TextColour;
			}

		}

		public override StyleBase StyleObjectForParameter(Parameters parameter, bool applyingDefault = false) => Element?.StyleObjectForParameter(parameter, applyingDefault);

		#endregion

		#region Graphics
		protected override void InternalDraw(Canvas gr, DrawResources resources)
		{
			// this would draw this item itself, which has no representation.  It is the Draw et cetera methods which draw the content item
		}

		internal override void Draw(Canvas gr, float scale, float coordScale, StaticView view, StaticView.InvalidationBuffer buffer, int fillAlpha = 255, int edgeAlpha = 255, bool reverseRenderOrder = false)
		{
			if (!Shown)
				return;
			HighlightStyleC actualStyles = null;
			if (State == ButtonShape.States.Highlight)
			{
				actualStyles = HighlightStyleC.FromShape(Element);
				HighlightStyle.ApplyToShape(Element);
			}
			Element.Draw(gr, scale, coordScale, view, buffer, fillAlpha, edgeAlpha, reverseRenderOrder);
			if (State == ButtonShape.States.Highlight)
				actualStyles.ApplyToShape(Element);
		}

		internal override void DrawHighlight(Canvas gr, float scale, float coordScale, Target singleElement)
		{
			// sounds perverse, but highlighting styles not done here - this highlight always draws in glowing yellow regardless of object settings
			Element.DrawHighlight(gr, scale, coordScale, singleElement);
		}

		#endregion

		#region Other delegated to content

		internal override void InitialiseFreeStanding()
		{ // called by GUI if it creates one of these for info - Start will not have been called, so must create the content element
			base.InitialiseFreeStanding();
			Element = new Item();
			Element.InitialiseFreeStanding();
		}

		public override void NotifyStyleChanged(Parameters parameter, int oldValue, int newValue)
		{
			Element?.NotifyStyleChanged(parameter, oldValue, newValue);
		}

		internal override List<GrabSpot> GetGrabSpots(float scale) => Element.GetGrabSpots(scale);

		internal override void DoGrabAngleSnap(GrabMovement move)
		{
			Element.DoGrabAngleSnap(move);
		}

		protected internal override void DoGrabMove(GrabMovement move)
		{
			//base.DoGrabMove(move);
			Element.GrabMove(move);
		}

		internal override GrabMovement GetCustomGrabMove(EditableView.ClickPosition current, EditableView.ClickPosition click, Transaction transaction)
			=> Element.GetCustomGrabMove(current, click, transaction);

		internal override bool StartGrabMove(GrabMovement grab)
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
			if ((affected & ChangeAffects.Bounds) > 0)
				m_Bounds = Rectangle.Empty;
			Parent.NotifyIndirectChange(shape, affected);
			if ((affected & ChangeAffects.GrabSpots) > 0 && shape == Element)// page only responds if the element with grabspots is mentioned - but grabspots here came from Element, even if they are technically on this shape
				Parent.NotifyIndirectChange(this, ChangeAffects.GrabSpots);
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
			Sound?.DereferenceOnLoad(document);
		}

		public override void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
			base.UpdateReferencesIDsChanged(mapID, document);
			Element?.UpdateReferencesIDsChanged(mapID, document);
			// this function is called on each object when a group have been copied and pasted
			// we can use this opportunity to set the SAW ID
			SetUniqueID();
			Sound?.UpdateIDsReferencesChanged();
		}

		internal override void NotifyEnvironmentChanged(EnvironmentChanges change)
		{
			Element?.NotifyEnvironmentChanged(change);
		}

		public override void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			base.AddRequiredReferences(fnAdd, mapID);
			fnAdd.Invoke(Sound?.Content);
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

		public IEnumerable<Shape> Reverse => (Element as IShapeContainer)?.Reverse ?? EmptyList;

		public void FinishedModifyingContents(Transaction transaction, GrabMovement move = null)
		{
			ElementAsContainer?.FinishedModifyingContents(transaction, move);
		}

		/// <summary>Returns the Item contents if it contains a SAW item, or an empty list </summary>
		public List<Shape> Contents => (Element as IShapeContainer)?.Contents ?? new List<Shape>();

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