using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;


namespace SAW
{
	public sealed class Change
	{
		// Represents a change to a single object within a transaction
		// if the change is a deletion then the 'current' object reference is kept in Previous (allowing the same reference to be restored on undo - this is crucial
		// so that lists containing the subject will reference it correctly) end Current is nothing.  If the change is an addition then Previous is nothing
		// if it is an edit then both objects are defined
		// note that this means restoring a delete is unable to restore any changes to that object already made during the same transaction
		// therefore objects deleted during a transaction currently be unchanged.  This isn't really a problem in the case of AccessMaths
		public Datum Current;
		public Datum Previous;

		#region Constructors
		private Change(Datum current, Datum previous)
		{
			Current = current;
			Previous = previous;
		}

		public static Change CreateEdit(Datum data)
		{
			Datum previous = Datum.Create(data.TypeByte);
			previous.CopyFrom(data, Datum.CopyDepth.Undo, null);
			return new Change(data, previous);
		}

		public static Change CreateDelete(Datum data)
		{
			return new Change(null, data);
		}

		public static Change CreateCreate(Datum newData)
		{
			return new Change(newData, null);
		}
		#endregion

		#region Information properties
		public bool IsDelete
		{ get { return Current == null; } }

		public bool IsCreate
		{ get { return Previous == null; } }

		/// <summary>Key to use when storing this in indexed transaction list - Current if it is defined</summary>
		public Datum Key
		{ get { return Current ?? Previous; } }

		public override string ToString()
		{
			return (Shape.Shapes)(Current ?? Previous).TypeByte + "/" + (Current ?? Previous).ID;
		}
		#endregion

		public void Undo()
		{
			if (IsDelete || IsCreate)
			{
				// Changes to the container should be sufficient, except for the status
				if (Previous is Shape)
					((Shape)Previous).Status = Shape.StatusValues.Complete;
			}
			else
			{
				// we need to keep a copy of the state after the edit (i.e. current) in case we need to redo
				// however the changes must be applied to the reference in Current
				Datum future = Datum.Create(Current.TypeByte);
				future.CopyFrom(Current, Datum.CopyDepth.Undo, null);
				Current.CopyFrom(Previous, Datum.CopyDepth.Undo, null);
				Previous = future;
				(Current as IShapeContainer)?.FinishedModifyingContents(null, null);
			}
		}

		public void Redo()
		{
			// this is actually logically identical to Undo; not sure if it will always remain like that
			if (IsDelete || IsCreate)
			{
				// Changes to the container should be sufficient
			}
			else
			{
				// again we need to keep the copy of the past state in case it is undone again.
				// Undo left the future state in the Previous reference
				Datum previous = Datum.Create(Current.TypeByte);
				previous.CopyFrom(Current, Datum.CopyDepth.Undo, null);
				Current.CopyFrom(Previous, Datum.CopyDepth.Undo, null);
				Previous = previous;
			}
		}

	}

	/// <summary>Represents changes to one or more objects which can be undone in a single batch</summary>
	public sealed class Transaction : IEnumerable<Change>
	{

		/// <summary>Key is Change.Key - current object if defined</summary>
		private readonly Dictionary<Datum, Change> m_Changes = new Dictionary<Datum, Change>();
		/// <summary>True if document was already Changed at the start.  filled in by frmMain as it stores Tx</summary>
		public bool ChangedAtStart = true;
		/// <summary>true if the transaction was cancelled, reversing any changes made so far
		/// this is not true if the transaction has been undone or redone; only cancelling during the transaction sets this, in which case it cannot be put into the undo buffer</summary>
		public bool Cancelled;
		/// <summary>the current page displayed when this transaction was made.  By implication the transaction affects shapes on this page.
		/// frmMain uses this to work out which pages need to be updated in the page list when undoing a transaction</summary>
		public Page CurrentPage = null;
		/// <summary>Set to true on the edit/create etc of a config object.  Also for the user default button styles</summary>
		public bool ContainsConfig;
		/// <summary>Must be set by the relevant GUI; not automatically detected.  frmMain.IndirectEffectsUndoRedo needs to know this.   (it won't matter if it also contains some other things)</summary>
		public bool IsPageSizeChange = false;
		/// <summary>is set to true if it contains changes that will need a repaint of the entire document (currently just changes to button styles)</summary>
		public bool RequiresDocumentRepaint = false;
		/// <summary>true once has been stored</summary>
		public bool Stored = false;
		/// <summary>true if doesn't cause document changed flag to be set</summary>
		public bool DocumentNotChanged = false;
		/// <summary>Just removes the assertion for this slightly odd condition</summary>
		public bool ExpectObjectCreatedAndDestroyed;
		/// <summary>Implemented privately within Shape and the type of this is not specified, except that a value of Nothing means not specified</summary>
		/// <remarks>The  caret position isn't always stored in a transaction (doing so meaningfully requires proper timing on when to capture its position)</remarks>
		public object CaretState;

		#region Constants
		/// <summary>The minimum number of buffers to keep, regardless of how much memory they seem to use</summary>
		public const int MINIMUMBUFFERS = 5;
		/// <summary>Maximum NOMINAL memory to use for undo buffers.  Assigned by static constructor as one 20th of machine's memory</summary>
		public static readonly int MAXIMUMMEMORY;
		/// <summary>Maximum number of buffers to keep the regardless of how little memory they have used</summary>
		public const int MAXIMUMBUFFERS = 100;
		#endregion

		#region System memory
		static Transaction()
		{
			Windows.MEMORYSTATUSEX statEX = new Windows.MEMORYSTATUSEX();
			statEX.dwLength = 2 * 4 + 7 * 8;
			Windows.GlobalMemoryStatusEx(ref statEX);

			// .Min in case machine has >80 GB! which would overflow an integer.  Actually limits to 500MB regardless (that seems plenty!)
			MAXIMUMMEMORY = (int)Math.Min(500000000, statEX.ullTotalPhys / 20);
			// new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory
		}


		#endregion

		#region Single object edits
		private void AddChange(Change change)
		{
			m_Changes.Add(change.Key, change);
			if (change.Key.TypeByteAsFileMarker == FileMarkers.Config)
				ContainsConfig = true;
			else if (change.Key.TypeByteAsFileMarker == FileMarkers.ButtonStyle && ((ButtonStyle)change.Key).IsUserDefault)
				ContainsConfig = true;
		}

		public void Edit(Datum data)
		{
			Debug.Assert(data != null);
			if (data == null)
				return;
			Debug.Assert(!((data as Scriptable)?.Element is Item) || !Contains((data as Scriptable).Element as Item) || Contains(data), "A Scriptable must be added to the transaction BEFORE its content");
			if (m_Changes.ContainsKey(data))
				Debug.Assert(!m_Changes[data].IsDelete); // can be ignored if it was created or previously edited
			else
				AddChange(Change.CreateEdit(data));
		}

		public void Create(Datum newData)
		{
			if (m_Changes.ContainsKey(newData))
				throw new InvalidOperationException("Transaction.StartCreate: this object reference is already in the transaction");
			AddChange(Change.CreateCreate(newData));
			if (newData is Shape)
				((Shape)newData).Status = Shape.StatusValues.Complete;
		}

		public void Delete(Datum deleted)
		{
			if (m_Changes.ContainsKey(deleted))
			{
				Change change = m_Changes[deleted];
				if (change.IsDelete)
				{
					// can just ignore this
				}
				else if (change.IsCreate)
				{
					// possible to cope with this, but odd.  If it was created and destroyed we can just remove it from the transaction and pretend it never existed
					Debug.Assert(ExpectObjectCreatedAndDestroyed, "Object created and destroyed inside same transaction"); // This can be ignored if dragging from the stack and back in
					m_Changes.Remove(deleted);
				}
				else
				{
					// any containers will have references to the Current entry.  Therefore if this transaction is reversed later that object reference is the one that we must restore
					// however we want to restore it to the state before this transaction (i.e. both edit and delete), therefore we need to copy the state from Previous now
					// (This changed in version 2; version once this correctly, but it didn't matter until dragging back into a stack was implemented)
					change.Current.CopyFrom(change.Previous, Datum.CopyDepth.Undo, null);
					change.Previous = change.Current;
					change.Current = null;
				}
			}
			else
				AddChange(Change.CreateDelete(deleted));
			if (deleted is Shape)
				((Shape)deleted).Status = Shape.StatusValues.Deleted;
		}

		public void Disregard(Datum data)
		{
			// removes a previous object from list - if it is known it won't actually have changed
			// to be used sparingly!
			if (m_Changes.ContainsKey(data))
				m_Changes.Remove(data);
			else
				Debug.Fail("Transaction.Disregard: object not in transaction");
		}

		#endregion

		#region Information
		public bool Contains(Datum data)
		{ return m_Changes.ContainsKey(data); }

		public bool Empty
		{ get { return m_Changes.Count == 0 || Cancelled; } }

		public RectangleF CalculateRefreshBounds()
		{
			// calculates the bounds for all shapes in this transaction, using both the before and after versions
			// assumes that all of the changed items are shapes, not any other data type
			RectangleF invalid = RectangleF.Empty;
			foreach (Change change in m_Changes.Values)
			{
				if (change.Previous is Shape)
					Geometry.Extend(ref invalid, ((Shape)change.Previous).RefreshBounds());
				if (change.Current is Shape)
					Geometry.Extend(ref invalid, ((Shape)change.Current).RefreshBounds());
			}
			return invalid;
		}

		/// <summary>Gets the previous copy of the given object; or nothing if it is new</summary>
		public Datum GetObjectPrevious(Datum obj)
		{
			if (!m_Changes.ContainsKey(obj))
			{
				Utilities.LogSubError("Transaction.GetObjectPrevious - requested object is not in transaction at all");
				return null;
			}
			return m_Changes[obj].Previous;
		}

		/// <summary>Increases the value returned by NominalMemory.  Currently only used when pixel data is edited</summary>
		public int ExtraNominalMemory = 0;

		/// <summary>Gets an entirely nominal estimate of the memory used by this.  This currently simply 100*number of objects, unless pixel layer of a page has been edited</summary>
		public int NominalMemory
		{ get { return m_Changes.Count * 100 + ExtraNominalMemory; } }

		#endregion

		#region Complete transaction status changes
		public void Undo()
		{
			Debug.Assert(Cancelled == false, "Undo transaction is \'Cancelled\' - a cancelled transaction should not be in the undo buffer");
			foreach (Change change in m_Changes.Values)
				change.Undo();
		}

		public void Redo()
		{
			foreach (Change change in m_Changes.Values)
				change.Redo();
		}

		public void Cancel()
		{
			Debug.Assert(Cancelled == false, "Cancelling a transaction for the second time; not sure that this is supported");
			foreach (Change change in m_Changes.Values)
				change.Undo();
			Cancelled = true;
		}

		public bool CheckForDegenerateShapes()
		{
			// checks if we are storing it changes to, or creating, any shapes which would better be discarded
			// if these exist then additions are simply removed from the transaction, and edits are changed to deletions
			// and we update the page to delete these objects
			// returns true if any were detected
			if (Cancelled)
				return false;
			bool detected = false;
			List<Change> remove = null; // list of changes which would be removed in the transaction.  The list is only created when needed
			Change pageChange = null; // if it is necessary to add the page to the transaction, then the change object for it is stored here
									  // the transaction list cannot be updated directly, because we are iterating it
			foreach (Change change in m_Changes.Values)
			{
				if (change.Current is Shape)
				{
					Shape shape = (Shape)change.Current;
					if (shape.Degenerate && shape.Parent is Page && !shape.HasCaret)
					{
						// we ignore it if the shape is not directly stored on the page.  If it is inside a group or stack or something it is too awkward to remove
						// (and this case would be very rare anyway, so it's safest just to leave them)
						if (change.Previous == null)
						{
							// shape is newly created; just remove the addition from the transaction
							if (remove == null)
								remove = new List<Change>();
							remove.Add(change);
						}
						else
							change.Current = null; // shape is being edited; change the record into a deletion
												   // and we need to remove it from the page
						Page page = (Page)shape.Parent;
						if (pageChange == null && !m_Changes.ContainsKey(page))
							pageChange = Change.CreateEdit(page);
						page.Delete(shape, new Transaction());
						// we cannot give the page this transaction
						// because it will crash if the page tries to add records, because we are currently iterating the list
						// however we have taken care of recording the necessary changes here, so we can just give it a dummy transaction which is then throw away
						detected = true;
					}
				}
			}
			// now we are no longer iterating the list, make any  necessary changes to it...
			if (remove != null)
			{
				foreach (Change change in remove)
				{
					m_Changes.Remove(change.Key);
				}
			}
			if (pageChange != null)
				m_Changes.Add(pageChange.Key, pageChange);
			return detected;
		}

		#endregion

		#region IEnumerable

		IEnumerator<Change> IEnumerable<Change>.GetEnumerator()
		{
			return m_Changes.Values.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return m_Changes.Values.GetEnumerator();
		}

		public RectangleF RefreshBounds()
		{
			// refreshes summed bounds of all shapes - before and after. Not particularly efficient
			RectangleF invalid = RectangleF.Empty;
			foreach (Change change in m_Changes.Values)
			{
				if (change.Current is Shape)
					Geometry.Extend(ref invalid, ((Shape)change.Current).RefreshBounds());
				if (change.Previous is Shape)
					Geometry.Extend(ref invalid, ((Shape)change.Previous).RefreshBounds());
			}
			return invalid;
		}

		#endregion
	}


}
