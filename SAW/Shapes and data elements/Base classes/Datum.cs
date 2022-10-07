using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using SAW.Shapes;

namespace SAW
{
	public abstract class Datum
	{
		// Base class for anything which can be under transaction control (i.e. undo and redo)
		// by definition they can all be saved using binary data as well

		[System.Xml.Serialization.XmlAttribute()]
		public Guid ID = Guid.NewGuid(); // XML used by WorksheetList - so this class must use it sensibly

		/// <summary>represents depths at which CopyFrom must operate.  At the shallowest level many of the embedded objects can be left linked</summary>
		/// <remarks>(e.g. if this is only needed to undo a transform, then all the styling data can be left linked rather than a copy of the styling objects made)
		/// these must be defined in order of ascending depth</remarks>
		public enum CopyDepth
		{
			/// <summary>the copy only needs to be separate enough so that coordinate edits to one will be reversed when the copy is copied back into the original (for cancelling a transform)</summary>
			Transform,
			/// <summary>a copy for the undo buffer.  The copy needs to be deep enough that anything that might be edited by the user is separated out</summary>
			/// <remarks>The undo buffer has a separate copy and when the undo buffer is copied back into the original everything is restored
			/// notably embedded bitmaps don't need to copy the entire bitmap data, because the user cannot edit this</remarks>
			Undo,
			/// <summary>Must be a complete copy, the new object is being created as an independent object in the data.  There should be no shared referenced objects except those which are expected to be shared (e.g. Document.SharedBitmaps)
			/// Copying does not update any references; for a complete Clone the top level caller must update references at the end</summary>
			Duplicate,
			/// <summary>Behaviour depends on the shape; not expected by most shapes for which the behaviour is undefined (in practice will work like Duplicate probably)</summary>
			Custom
		}

		/// <summary>ID is never copied  Undo will never restore it either, and when cloning we want a different ID
		/// mapID is defined __if and only if__ eDepth = Duplicate</summary>
		/// <param name="depth"></param>
		/// <param name="mapID">if duplicating, the ID mapping for any sub objects should be added to that list</param>
		/// <param name="other"></param>
		public virtual void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			Debug.Assert((depth == CopyDepth.Duplicate) == (mapID != null));
			if (other==this)
			Debug.Assert(other != this);
			// will fail in various places in shapes (where it clears my list and copies the contents of the other list)
			// if this data object contains or references other _data_ objects it should just copy the reference.  for undo/redo changes to the other object will be applied separately
			// if this data object contains other objects which do not derive from Datum, it should make a deep copy of those, so that both copies of this Datum
			// operate independently.  This includes lists of other Datum - it must make a copy of the list, rather than copying the list reference, but the list can
			// then contain links to the original objects.
			// Some transient information, such as list of selected shapes within a page need not be copied (?  Maybe change this to maintain selection on undo?)
			// the second parameter can allow a partial copy, especially when all we are doing is backing up the coordinates
		}

		public static Datum Create(byte type)
		{
			return Create((FileMarkers)type);
		}

		public static Datum Create(FileMarkers type)
		{
			switch (type)
			{
#if APPLICATION
				case FileMarkers.Page:
					return new Page();
				case FileMarkers.Paper:
				case FileMarkers.GraphPaper:
					return Paper.CreateFromTypeCode(type);
				case FileMarkers.Document:
					return new Document(true);
				case FileMarkers.SharedBitmap:
					return new SharedImage();
				case FileMarkers.Config:
					return new Config();
				case FileMarkers.ButtonStyle:
					return new ButtonStyle();
				case FileMarkers.SharedResource:
					return new SharedResource();
#endif
				default:
					if (type <= FileMarkers.MaximumShape)
						return Shape.CreateShape((Shape.Shapes)type);
					else
						throw new ArgumentException("Datum.Create: unknown type code: " + type);
			}
		}

		protected internal virtual void Load(DataReader reader)
		{
			ID = reader.ReadGuid();
		}

		protected internal virtual void Save(DataWriter writer)
		{
			writer.Write(ID);
		}

		protected internal abstract byte TypeByte { get; }

		internal FileMarkers TypeByteAsFileMarker => (FileMarkers)TypeByte;

		/// <summary>Creates deep copy of object with new ID, adding itself to the mapping list.  Does not update references (this should be used within CopyFrom)</summary>
		public virtual Datum Clone(Mapping mapID)
		{
			if (mapID == null)
				mapID = Mapping.Ignore;
			//Debug.Assert(!mapID.Ignored); // may fail later doing duplicates.  But this condition would trigger for things like ShapeStack.CopyFrom
			Datum datum = Create(TypeByte);
			// objNew.ID = New Guid ' CopyFrom never updates the ID; this line not actually needed as the Datum constructor does this
			mapID.Add(ID, datum);
			datum.CopyFrom(this, CopyDepth.Duplicate, mapID);
			return datum;
		}

		public static DatumList CloneList(IEnumerable<Datum> original, Document document)
		{
			// makes a copy of all of the objects in the list, returning the new list
			// contained objects are cloned, but don't appear in the list (they do appear in the ID lookup table)
			// the new list will also contain any necessary objects referenced by these (e.g. textures)
			// links between objects will be updated if both halves are in the list, or broken if only the linking object (e.g. connector, paint splat)
			// is in the list
			DatumList newList = new DatumList();
			Mapping mapID = new Mapping(); // lookup table from old to new IDs
			foreach (Datum data in original.Where(x => x != null)) // will ignore any missing objects in list
			{
				if (!mapID.ContainsKey(data.ID)) // check is needed because containers will include contents, and original list could include both container and content if selected from page (select all especially)
					newList.Add(data.Clone(mapID));
			}
			foreach (Datum data in original.Where(x => x != null))
			{
				// It would have been much easier to just iterate colNew, but we can't because the line below might modify the list
				mapID[data.ID].AddRequiredReferencesRecurseToContained(newList.Add, mapID);
			}
			foreach (Datum shape in newList.Values)
			{
				shape.UpdateReferencesIDsChanged(mapID, document);
			}
			return newList;
		}

		/// <summary>AddRequiredReferences for this and all contained objects</summary>
		protected internal void AddRequiredReferencesRecurseToContained(Action<Datum> fnAdd, Mapping mapID)
		{
			this.Iterate(obj => obj.AddRequiredReferences(fnAdd, mapID));
		}

		/// <summary>Non-recursively invokes function for all non-contained objects which REQUIRED by this one (may not include loosely linked objects).
		/// Particularly includes shared bitmaps.  Object IDs must also appear in mapID</summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		protected internal virtual void AddRequiredReferences(Action<Datum> fnAdd, Mapping mapID)
		{
			// if this object references other objects, __which it does not contain__ they need to be added, if not already in the list
			// if the target object is effectively invariant, i.e. duplicates of this object would all refer to the same target
			// then it can just add the existing reference
			// If the target is loosely linked to the subject (e.g. connector connecting to a shape) then it should be ignored and not added to the list
			// The ID linking the objects will be updated by UpdateReferencesIDsChanged, or the link will be broken if the target is not already in the list
			// although, I don't think it is used at the moment, an object also has the opportunity of cloning the target object and adding it to the list
			// but must check first that the target is not already cloned (i.e. it's ID is not a key in mapID)
			// WARNING - AddRequiredReferences will not be called on the target object (because we are not iterating objList - we can't as it is to be updated,
			// but rather iterating what is effectively a copy of it before the first called to AddRequiredReferences)

			// There is no need to call this on sub objects; this is always called via Iterate
		}

		// can possibly merge both of the next now as they basically contain an ID->object map? (of course in one case IDs match, one they don't)

		/// <summary>called after loading.  Allows any objects which reference external objects not contained within self to relink to those objects
		/// each class must call this on sub classes</summary>
		protected internal abstract void UpdateReferencesObjectsCreated(Document document, DataReader reader);

		/// <summary>Called after copying a group of objects, where the copies have new ID numbers
		/// objects with links to external objects should update their IDs, or clear them if they don't appear in the dictionary
		/// key is the old ID, value is the new OBJECT
		/// Document is provided for objects which access shared resources, but may want to update to a new reference (if the entire document has been copied)</summary>
		protected internal virtual void UpdateReferencesIDsChanged(Mapping mapID, Document document)
		{
		}

		public delegate void DatumFunction(Datum obj);

		/// <summary>Calls fn on this and all items CONTAINED within it, to any depth</summary>
		internal virtual void Iterate(DatumFunction fn)
		{
			// Should be overridden in any derived classes which CONTAIN other objects.
			// Overriding objects should do base.Iterate and then call Iterate on each contained object
			// should only iterate to CONTAINED objects.  For linked objects, override AddRequiredReferences
			fn(this);
		}

		/// <summary>Naff, but the palette delta needs to ignore some changes, such as page size.  If this is true then IdenticalTo will change its behaviour
		/// to suit comparing 2 palettes to see whether they need to be included in the deltas</summary>
		public static bool IdenticalForPaletteDelta = false;

		/// <summary>Compares if they are value equal.  NOT IMPLEMENTED FOR ALL TYPES.  See IdenticalForPaletteDelta</summary>
		/// <remarks>This is only used for palette change detection in config deltas atm.  I'm not using .Equals so that I can detect where it is not implemented</remarks>
		public virtual bool IdenticalTo(Datum other)
		{
			Debug.Fail("IdenticalTo not implemented for type " + GetType());
			return false;
		}
	}

	#region ID mapping
	/// <summary>Stores mapping of object IDs to new object references when copying/cloning an object tree </summary>
	public class Mapping
	{

		private readonly Dictionary<Guid, Datum> List = new Dictionary<Guid, Datum>();

		/// <summary>if true, then this list is not needed and additions are just ignored</summary>
		public bool Ignored { get; private set; }

		/// <summary>Shared list for use where the mappings aren't actually needed .Function calls require one of these objects</summary>
		/// <remarks></remarks>
		public static Mapping Ignore = new Mapping() { Ignored = true };

		public void Add(Guid old, Datum add)
		{
			if (Ignored)
				return;
			List.Add(old, add);
		}

		/// <summary>Adds an entry for an object which is not being duplicated</summary>
		/// <remarks>This can be required for links to external objects because UpdateReferences and 10 to wipe all references to objects not in this list</remarks>
		public void AddUnchangedObject(Datum obj)
		{
			if (Ignored)
				return;
			if (!List.ContainsKey(obj.ID))
				List.Add(obj.ID, obj);
		}

		public bool ContainsKey(Guid ID)
		{
#if DEBUG
			// Think by making this #if it will be inlined in release build
			if (Ignored)
				throw new InvalidOperationException("Mapping.Ignore: cannot read from contents");
#endif
			return List.ContainsKey(ID);
		}

		public Datum this[Guid old]
		{
			get
			{
				if (!List.ContainsKey(old))
					return null;
				return List[old];
			}
		}
	}
	#endregion

	#region Datum list

	public class DatumList : Dictionary<Guid, Datum>
	{

		/// <summary>Will ignore calls of Add(null)</summary>
		public void Add(Datum data)
		{
			if (data == null)
				return;
			Debug.Assert(!data.ID.Equals(Guid.Empty));
			if (!base.ContainsKey(data.ID))
				base.Add(data.ID, data);
		}

		public void Add(IEnumerable<Datum> list)
		{
			foreach (Datum objData in list)
			{
				Add(objData);
			}
		}

		public void Add(List<Shape> list)
		{
			// can be more convenient than above - we have a lot of lists of shapes
			foreach (Shape data in list)
			{
				Add(data);
			}
		}

		public bool Contains(Datum data)
		{
			return ContainsKey(data.ID);
		}

		public void Remove(Datum data)
		{
			base.Remove(data.ID);
		}

	}
	#endregion
}
