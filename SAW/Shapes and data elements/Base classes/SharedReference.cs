using System;

namespace SAW.Shapes
{
	/// <summary>Holds GUID and reference to SharedBase - used for loading when only GUID is known temporarily.  Use .Content to access the media object.
	/// Resources are all stored in document.  Whenever a SharedReference is expected a SharedImage or SharedResource object can be provided directly - the SharedReference wrapper is automatic</summary>
	public class SharedReference<T> where T : Datum
	{
		private T m_Data;
		private Guid m_id;

		/// <summary>True if DereferenceOnLoad failed</summary>
		public bool Failed { get; private set; }

		public SharedReference()
		{ }

		public SharedReference(T data)
		{
			m_Data = data;
			m_id = m_Data?.ID ?? Guid.Empty;
		}

		public static SharedReference<T> FromGUID(Guid ID)
		{
			if (ID.IsEmpty())
				return null;
			return new SharedReference<T>() { m_Data = default(T), m_id = ID }; // default(T) will be null, but can't use null here
		}

		/// <summary>The actual media object: SharedImage or SharedResource</summary>
		public T Content
		{
			get { return m_Data; }
			set
			{
				m_Data = value;
				m_id = m_Data?.ID ?? Guid.Empty;
			}
		}

		/// <summary>The ID of the media object.  During load this may be defined before the actual data is available</summary>
		public Guid ID
		{
			get { return m_id; }
			set
			{
				m_id = value;
				m_Data = default(T);
			}
		}

		internal void DereferenceOnLoad(Document doc)
		{
			if (m_id.IsEmpty()) return;
			if (m_Data != null) return; // already dereferenced (this can be called like this when loading a legacy AM document)
			m_Data = doc.FindExistingSharedResource<T>(m_id);
			if (m_Data == null)
			{
				Globals.Root.Log.WriteLine("Failed to dereference SharedResource: " + m_id);
				Failed = true;
			}
		}

		internal void UpdateIDsReferencesChanged()
		{
			m_id = m_Data?.ID ?? Guid.Empty;
		}

		public static implicit operator SharedReference<T>(T data) => data == null ? null : new SharedReference<T>(data);

		/// <summary>Mainly intended to allow foo?.HasContent which will also be false if foo == null</summary>
		public bool HasContent => m_Data != null;

		internal static bool ReferencesAreEqual(SharedReference<T> a, SharedReference<T> b)
		{
			if ((a == null) ^ (b == null))
				return false; // only 1 undef
			if (a == null)
				return true; // both undef
			return a.ID.Equals(b.ID);
		}

		internal SharedReference<T> Clone()
		{
			if (Failed)
				return null;
			if (m_Data == null)
				return FromGUID(m_id);
			return new SharedReference<T>(m_Data);
		}

		public override string ToString()
		{
			if (m_Data == null)
				return m_id.ToString();
			return $"{m_Data.GetType()} ({m_id})";
		}

	}
}