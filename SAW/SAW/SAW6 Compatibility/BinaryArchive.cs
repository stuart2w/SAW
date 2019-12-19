using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SAW
{
	// old SAW used the C++ archive classes.  These reproduce the required functionality to read the old SAW files
	// NOTE this only relates to SAW data - most Splash classes are not saved in these files

	/// <summary>Implemented by any class which can be archived</summary>
	public interface IArchivable
	{
		void Read(ArchiveReader ar);
		void Write(ArchiveWriter ar);
	}

	/// <summary>Used to assemble a list of the CLASSES (not objects) which can appear in the file.  Each is numbered, and the number is used to identify the class of objects stored in the data</summary>
	internal class ArchiveClass
	{
		public string Name;
		public int Schema = -1;
		public Type TypeObject;
	}

	/// <summary>Implemented by both Reader and Writer</summary>
	public interface IArchiver
	{
		void NotifyType(string classNameInFile, Type t);

		/// <summary>SAW File version.  Minimum permitted is 5004</summary>
		int SAWVersion { get; set; }
		/// <summary>The height of the SAW page.  This is used to adjust the coordinate systems.  Old SAW had 0,0 at the top left.  Splash has it at the bottom left (and all Y coordinates are negative)</summary>
		int PageHeight { get; set; }

		string FileFolder { get; }
	}

	/// <summary>Reads from SAW6 file, implementing C++ archive mechanism</summary>
	public class ArchiveReader : BinaryReader, IArchiver
	{
		/// <summary>SAW file version.  Must be set by document reader - and won't be defined at very start.  Minimum permitted is 5004</summary>
		public int SAWVersion { get; set; }
		/// <summary>The height of the SAW page.  This is used to adjust the coordinate systems.  Old SAW had 0,0 at the top left.  Splash has it at the bottom left (and all Y coordinates are negative)</summary>
		public int PageHeight { get; set; }

		/// <summary>The folder containing external files referenced by the old SAW file</summary>
		public string FileFolder { get; }

		/// <summary>The document that the file is being read into.  This is created internally while reading the DOC object in the file</summary>
		public Document IntoDocument { get; set; }

		/// <summary>Splash Globals transaction used to encompass the entire read operation.  Transaction control isn't essential as the entire data could be discarded, but 
		/// some code invoked during the load expects a transaction</summary>
		public Transaction ReadTransaction { get; }

		public ArchiveReader(string file)
			: base(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.Default)
		{
			m_LoadArray.Add("null");
			FileFolder = Path.GetDirectoryName(file);
			ReadTransaction = new Transaction();
		}

		#region Read base types
		/// <summary>Mark the base class ReadBool as obsolete - Bools are not stored in this format.  Use ReadBool32 instead.</summary>
		/// <returns></returns>
		[Obsolete]
		public bool ReadBool()
		{ return base.ReadBoolean(); }
		public bool ReadBool32()
		{
			return (base.ReadInt32() != 0);
		}
		/// <summary>reads a string with a preceeding length.  I think they are all stored like that, although the arccore.cpp MFC code which
		/// defines the CArchive stuff includes an option for loading CR or LF terminated strings</summary>
		public string ReadStringL()
		{
			bool unicode;
			int length = AfxReadStringLength(out unicode);
			Debug.Assert(length < 1000);
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				str.Append(unicode ? Convert.ToChar(ReadUInt16()) : Convert.ToChar(ReadByte()));
			}
			return str.ToString();
		}
		/// <summary>Reads the rectangle, without adjusting the Y-coordinate for the page size</summary>
		public Rectangle ReadRectangleRaw()
		{
			Rectangle r = new Rectangle();
			r.X = ReadInt32();
			r.Y = ReadInt32();
			r.Width = ReadInt32() - r.X;
			r.Height = ReadInt32() - r.Y;
			return r;
		}
		/// <summary>Reads the rectangle, but changes the y-coordinate to compensate for the different coordinate systems</summary>
		public Rectangle ReadRectangleAdjusted()
		{
			Rectangle r = new Rectangle();
			r.X = ReadInt32();
			r.Y = ReadInt32();
			r.Width = ReadInt32() - r.X;
			r.Height = ReadInt32() - r.Y;
			r.Y -= PageHeight;
			return r;
		}
		public RectangleF ReadFloatRect()
		{ // stored in the file as [l t r b]
			RectangleF r = new RectangleF();
			r.X = base.ReadSingle();
			r.Y = base.ReadSingle();
			r.Width = base.ReadSingle() - r.Left;
			r.Height = base.ReadSingle() - r.Height;
			return r;
		}
		public Point ReadPoint()
		{
			Point pt = new Point();
			pt.X = ReadInt32();
			pt.Y = ReadInt32();
			return pt;
		}

		#endregion

		#region Read objects and strings
		/// <summary>Read a terminated string (as opposed to one with length preceding it)</summary>
		private string ReadStringTerm(int max)
		{
			if (max == 0) return "";
			byte b = ReadByte();
			string s = "";
			while (b != 10 && b != 13)
			{
				s += (char)b;
				if (s.Length < max)
					b = ReadByte();
				else
					b = 10; // will terminate loop without reading owt else
			}
			if (b == 13) // 0D: in this case also pull off the 0A
				Debug.Assert(ReadByte() == 10);
			return s;
		}

		private const UInt32 NullTag = 0;           // special tag indicating NULL ptrs
		private const UInt32 NewClassTag = 0xFFFF;      // special tag indicating new CRuntimeClass
		private const UInt32 ClassTag = 0x8000;      // 0x8000 indicates class tag (OR'd)
		private const UInt32 BigClassTag = 0x80000000; // 0x8000000 indicates big class tag (OR'd)
		private const UInt32 BigObjectTag = 0x7FFF;      // 0x7FFF indicates DWORD object tag

		private readonly Dictionary<string, ArchiveClass> m_HashClasses = new Dictionary<string, ArchiveClass>();
		private List<object> m_LoadArray = new List<object>();

		public IArchivable ReadObject()
		{
			uint tag = ReadUInt16();
			if (tag == BigObjectTag)
				tag = ReadUInt32(); // stored as 4-bytes instead
			else
			{
				if (tag != NewClassTag && (tag & ClassTag) > 0)
					tag = (tag - ClassTag) | BigClassTag;
			}
			if (tag != NewClassTag && (tag & BigClassTag) == 0)
			{
				// specifies an already stored object
				uint objectTag = tag - BigClassTag;
				if (objectTag >= m_LoadArray.Count)
					throw new InvalidOperationException("Object tag too large: " + objectTag);
				return (IArchivable)m_LoadArray[(int)objectTag];
			}
			// specifies a new class
			ArchiveClass objClass;
			if (tag == NewClassTag)
			{
				// new class followed by object
				int schemaNum = ReadInt16();
				int nameLength = ReadInt16();
				var className = ReadStringTerm(nameLength);
				if (!m_HashClasses.ContainsKey(className))
					throw new InvalidOperationException("Unknown class type deserialising archive: " + className);
				objClass = m_HashClasses[className];
				m_LoadArray.Add(objClass);
			}
			else
			{ // is existing class followed by object
				uint classIndex = tag - BigClassTag;
				if (classIndex == 0 || classIndex >= m_LoadArray.Count)
					throw new InvalidOperationException("Invalid existing class index " + classIndex);
				objClass = (ArchiveClass)m_LoadArray[(int)classIndex];
				Debug.Assert(objClass != null);
			}
			IArchivable create = (IArchivable)objClass.TypeObject.GetConstructor(new Type[] { }).Invoke(new object[] { });
			m_LoadArray.Add(create);
			create.Read(this);
			return create;
		}

		/// <summary>Used in some SAW objects for a 2 or 4 byte value</summary>
		public uint ReadCount()
		{
			var count = ReadUInt16();
			if (count != 0xFFFF)
				return count;
			return ReadUInt32();
		}

		#endregion

		#region Lists
		// This effectively replaces CObList the old SAW which did nothing that List<> doesn't do

		/// <summary>Loads the list of objects into a new list</summary>
		public List<T> ReadList<T>() where T : IArchivable
		{
			List<T> result = new List<T>();
			uint n = ReadCount();
			for (int i = 0; i < n; i++)
				result.Add((T)ReadObject());
			return result;
		}

		/// <summary>As ReadList(), but add to an existing list rather than creating a new one.  The list is not cleared first</summary>
		public void ReadList<T>(List<T> list) where T : IArchivable
		{
			uint n = ReadCount();
			for (int i = 0; i < n; i++)
				list.Add((T)ReadObject());
		}

		#endregion

		public void NotifyType(string classNameInFile, Type t)
		{
			if (classNameInFile == "")
			{// must autodetect name
				classNameInFile = t.Name;
				int nDot = classNameInFile.IndexOf('.'); // now remove any namespace stuff to leave just the final type
				if (nDot > -1)
					classNameInFile = classNameInFile.Substring(nDot + 1, classNameInFile.Length - nDot - 1);
				classNameInFile = "C" + classNameInFile; // because we don't have 'C' on the beginning of every type name
			}
			Debug.Assert(typeof(IArchivable).IsAssignableFrom(t)); // check t is derived from IArchivable
			ArchiveClass a = new ArchiveClass
			{
				Name = classNameInFile,
				TypeObject = t
			};
			m_HashClasses.Add(classNameInFile, a);
		}

		private int AfxReadStringLength(out bool unicode)
		{
			unicode = false;
			// First, try to read a one-byte length
			int length = ReadByte();
			if (length < 0xff)
				return length;
			length = ReadUInt16();
			if (length == 0xfffe)
			{ // if unicode, size will follow again as previously
				unicode = true;
				length = ReadByte();
				if (length < 0xff)
					return length;
				length = ReadUInt16();
			}
			// now intLength has been read as if a 2-byte length either way
			if (length < 0xffff)
				return length;

			return ReadInt32();
			// technically it supports 64-bit lengths on Win64 but that won't apply to us
		}

	}

	//internal class TestStream : Stream
	//{
	//	FileStream s;
	//	public TestStream(string file)
	//	{
	//		s = new FileStream(file, FileMode.Open, FileAccess.Read);
	//	}

	//	public override bool CanRead
	//	{
	//		get { return false; }
	//	}

	//	public override bool CanSeek
	//	{
	//		get { return false; }
	//	}

	//	public override bool CanWrite
	//	{
	//		get { return true; }
	//	}

	//	public override void Flush()
	//	{
	//		s.Flush();
	//	}

	//	public override long Length
	//	{
	//		get { return s.Length; }
	//	}

	//	public override long Position
	//	{
	//		get
	//		{
	//			return s.Position;
	//		}
	//		set
	//		{
	//			s.Position = value;
	//		}
	//	}

	//	public override int Read(byte[] buffer, int offset, int count)
	//	{
	//		throw new Exception("The method or operation is not implemented.");
	//	}

	//	public override long Seek(long offset, SeekOrigin origin)
	//	{
	//		return s.Seek(offset, origin);
	//	}

	//	public override void SetLength(long value)
	//	{
	//		s.SetLength(value);
	//	}

	//	public override void Write(byte[] buffer, int offset, int count)
	//	{
	//		byte[] bufRead = new byte[count];
	//		s.Read(bufRead, 0, count);
	//		for (int i = 0; i < count; i++)
	//		{
	//			if (bufRead[i] != buffer[offset + i])
	//			{
	//				throw new InvalidOperationException("Mistmatched write.");
	//			}
	//		}
	//	}
	//}

	/// <summary>Writes to SAW6 file, re-implementing C++ archive system for classes</summary>
	public class ArchiveWriter : BinaryWriter, IArchiver
	{
		public int SAWVersion { get; set; }
		/// <summary>The height of the SAW page.  This is used to adjust the coordinate systems.  Old SAW had 0,0 at the top left.  Splash has it at the bottom left (and all Y coordinates are negative)</summary>
		public int PageHeight { get; set; }
		public string FileFolder { get; private set; }

		public ArchiveWriter(string file, int version = 6006)
			: base(new FileStream(file, FileMode.Create, FileAccess.Write), Encoding.Default)
		{
			SAWVersion = version;
			FileFolder = Path.GetDirectoryName(file);
		}

		#region Write simple values
		public void WriteBool32(bool b)
		{
			base.Write(b ? 1 : 0);
		}
		public void WriteStringL(string v, bool unicode = false)
		{ // Writes a string with a preceeding length.  Think they are all stored like that, although the arccore.cpp MFC code which
		  // defines the CArchive stuff includes an option for loading CR or LF terminated strings
			if (v == null) v = "";
			AfxWriteStringLength(v.Length, unicode);
			Debug.Assert(v.Length < 1000);
			//System.Text.StringBuilder str = new StringBuilder();
			WriteStringBytes(v, unicode);
		}
		private void WriteStringBytes(string s, bool unicode = false)
		{
			byte[] bytes;
			if (!unicode)
				bytes = Encoding.ASCII.GetBytes(s);
			else
				bytes = Encoding.Unicode.GetBytes(s);
			base.Write(bytes);
			//for (int i = 0; i < s.Length; i++)
			//{
			//	base.Write(Convert.ToByte(s[i]));
			//}
		}

		public void WriteRaw(Rectangle r)
		{
			base.Write(r.X);
			base.Write(r.Y);
			base.Write(r.Right);
			base.Write(r.Bottom);
		}
		public void WriteAdjusted(Rectangle r)
		{
			base.Write(r.X);
			base.Write(r.Y + PageHeight);
			base.Write(r.Right);
			base.Write(r.Bottom + PageHeight);
		}
		public void WriteRaw(RectangleF r)
		{
			Write(r.X);
			Write(r.Y);
			Write(r.Right);
			Write(r.Bottom);
		}
		public void Write(Point p)
		{
			Write(p.X);
			Write(p.Y);
		}

		//public void DebugNext()
		//{
		//    String strNext = "";
		//    for (int i = 0; i < 16; i++)
		//    {
		//        strNext += i.ToString() + ",";
		//    }
		//    System.Diagnostics.Debug.WriteLine(strNext);
		//}

		//private string WriteStringTerm(int nMax)
		//{
		//    if (nMax == 0) return "";
		//    byte b = WriteByte();
		//    string s = "";
		//    while (b != 10 && b != 13)
		//    {
		//        s += (char)b;
		//        if (s.Length < nMax)
		//            b = WriteByte();
		//        else
		//            b = 10; // will terminate loop without Writeing owt else
		//    }
		//    if (b == 13) // 0D: in this case also pull off the 0A
		//        System.Diagnostics.Debug.Assert(WriteByte() == 10);
		//    return s;
		//}
		#endregion

		#region Objects and strings
		private const uint NullTag = 0;           // special tag indicating NULL ptrs
		private const uint NewClassTag = 0x7FFF;      // special tag indicating new CRuntimeClass
		private const uint ClassTag = 0x8000;      // 0x8000 indicates class tag (OR'd)
		private const uint BigClassTag = 0x80000000; // 0x8000000 indicates big class tag (OR'd)
		private const uint BigObjectTag = 0x7FFF;      // 0x7FFF indicates DWORD object tag
													   //private Hashtable _hashSchemaNames = new Hashtable(); // looks up class name from number

		private Dictionary<Type, ArchiveClass> m_HashClasses = new Dictionary<Type, ArchiveClass>();
		private List<IArchivable> m_WrittenObjects = new List<IArchivable>();
		private uint m_NextIndex = 1;

		private void WriteTag(uint n, bool isClass)
		{
			bool big = (n >= 0x10000);
			if (big)
			{
				if (isClass) n += BigClassTag;
				base.Write(Convert.ToUInt16(BigObjectTag));
				base.Write(n);
			}
			else
			{
				if (isClass) n += ClassTag;
				Debug.Assert(n < 0x10000);
				base.Write(Convert.ToUInt16(n));
			}
		}

		public void Write(IArchivable o)
		{
			int existingIndex = m_WrittenObjects.IndexOf(o);
			if (existingIndex >= 0)
				WriteTag((uint)existingIndex, false);
			else
			{
				// object not get written - check if class name has been written
				if (!m_HashClasses.ContainsKey(o.GetType()))
					throw new InvalidOperationException("Unexpected class type in ArchiveWriter" + o.GetType().ToString());
				ArchiveClass archiveClass = (ArchiveClass)m_HashClasses[o.GetType()];
				if (archiveClass.Schema == -1)
				{
					// not yet written this class
					WriteTag(NewClassTag, true);
					WriteTag(0, false);
					//WriteTag(_nNextSchema, false);
					archiveClass.Schema = (int)m_NextIndex;
					m_NextIndex++;
					// for some reason name is not written as a length+string - instead it is always a 16-bit length followed by string data
					// (whereas normal StringL is 8-byte length unless that 8-bits indicates longer)
					base.Write(Convert.ToUInt16(archiveClass.Name.Length));
					WriteStringBytes(archiveClass.Name);
				}
				else
					WriteTag((uint)archiveClass.Schema, true);
				m_NextIndex++; // object itself also counted in index
				m_WrittenObjects.Add(o);
				o.Write(this);
			}
		}

		public void WriteCount(int c)
		{
			if (c >= 0xffff)
			{
				base.Write(Convert.ToUInt16(0xffff));
				base.Write(c);
			}
			else
				base.Write(Convert.ToUInt16(c));
		}

		public void NotifyType(string classNameInFile, Type t)
		{
			if (classNameInFile == "")
			{// must autodetect name
				classNameInFile = t.Name;
				int dot = classNameInFile.IndexOf('.'); // now remove any namespace stuff to leave just the final type
				if (dot > -1)
					classNameInFile = classNameInFile.Substring(dot + 1, classNameInFile.Length - dot - 1);
				classNameInFile = "C" + classNameInFile; // because we don't have 'C' on the beginning of every type name
			}
			ArchiveClass create = new ArchiveClass
			{
				Name = classNameInFile,
				TypeObject = t,
				Schema = -1
			};
			m_HashClasses.Add(t, create);
		}

		void AfxWriteStringLength(int length, bool unicode)
		{
			if (unicode)
			{
				base.Write(Convert.ToByte(0xff));
				base.Write(Convert.ToUInt16(0xfffe));
			}

			if (length < 255)
			{
				base.Write((byte)length);
			}
			else if (length < 0xfffe)
			{
				base.Write(Convert.ToByte(0xff));
				base.Write(Convert.ToUInt16(length));
			}
			else
			{
				base.Write(Convert.ToByte(0xff));
				base.Write(Convert.ToUInt16(0xffff));
				base.Write(Convert.ToUInt32(length));
			}
		}

		#endregion

		#region Lists
		public void WriteList<T>(List<T> list) where T : IArchivable
		{
			WriteCount(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				Debug.Assert(list[i] != null);
				Write(list[i]);
			}
		}

		public void WriteFiltered<T>(List<T> list, Predicate<T> predicate) where T : IArchivable
		{
			var toWrite = list.Where(x => predicate(x)).ToList();
			WriteCount(toWrite.Count);
			foreach (var x in toWrite)
				Write(x);
		}

		#endregion

	}
}
