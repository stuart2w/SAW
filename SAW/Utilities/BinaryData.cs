using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Linq;
// ReSharper disable UnusedVariable

namespace SAW
{
	public enum FileMarkers : byte
	{
		// the shape codes are also used here.
		// Note that the file itself has one marker indicating what the file is.  Each object also has a type marker.  So a file containing a single object has two
		MaximumShape = 149, // any code less than this is a shape
		ButtonStyle = 228,
		Splash = 229, // just a random Byte
		Page = 230,
		Paper = 231,
		GraphPaper = 233,
		Document = 234,
		SharedBitmap = 235,
		ShapeList = 236, // used with copy and paste
		Config = 237,
		Omitted = 239, // used if Nothing is passed to Write (Datum)
		ErrorReport = 240, // only used on the file, not to indicate any objects; the beginning of the file actually contains the document, and the rest is custom streamed
		SharedResource = 241,
		ConfigDocumentFile = 243, // file contains the document, which only contains a configuration (and any related resources)
		ConfigDelta = 244, // file containing changes to configs - several config objects WITHOUT documents
		Undetermined = 255 // can be used when the type of object does not matter
	}

	public sealed class DataUtilities
	{
		public const int FILEVERSION = 130;
		public const int MINIMUMSUPPORTEDVERSION = 120;
		public const float MINIMUMCOMPATIBLEVERSION = 700.0f; // earliest version which can open files written by this

		//VERSIONS....
		// these start at higher numbers for compatibility with Splash during early development
		// 109: XX: Container added Border
		// 110: XX: Added document.Units
		// 120: XX: Changed Config to support any number of ButtonStyles
		// 121: XX: Added Document.ReverseRendering
		// 122: XX: Added Document.SAW stuff and Page.HelpSAWID
		// 123: XX: Added Scriptable.RepeatTimeout
		// 124: XX: Added Item.ConceptID
		// 125: XX: Added SVG option to MemoryImage
		// 126: XX: Added DesktopScreens to SAW header
		// 127: XX: SharedResource now calls base classes (oops)
		// 128: XX: Added SAWItem.GraphicOnlyOnHighlight
		// 129: XX: Added HighlightStyle to Scriptable and removed various Highlight styles from Item.  Moved various other fields from Item to Scriptable
		// 130: XX: Added Page.RecentRotationPoints
	}

	public class DataWriter : System.IO.BinaryWriter
	{

		public int Version = 0;
		private string[] m_Strings;
		private const byte NUMBERSTRINGS = 15;
		private const byte REPEATMARKER = 128;
		private int m_NextString = 1;
		// strings used are 1..n (0 not used)
		// each 'Buffered' string written is preceeded by a byte. 0=empty - string itself not written.
		// 1..n = new string, normal BinaryWriter string to follow
		// 128+n = repeat of string number n

		public DataWriter(string file, FileMarkers type, int dataVersion = -1, Bitmap thumbnail = null) : base(new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write), System.Text.Encoding.Unicode)
		{
			try
			{
				WriteByte((byte)type);
				if (dataVersion < 0)
					dataVersion = DataUtilities.FILEVERSION;
				Version = dataVersion;
				InitialiseFile(thumbnail, type);
			}
			catch
			{
				base.Close();
				throw;
			}
		}

		public DataWriter(System.IO.Stream stream, FileMarkers type, int dataVersion = -1, Bitmap thumbnail = null) : base(stream, System.Text.Encoding.Unicode)
		{
			WriteByte((byte)type);
			if (dataVersion < 0)
				dataVersion = DataUtilities.FILEVERSION;
			Version = dataVersion;
			InitialiseFile(thumbnail, type);
		}

		private void InitialiseFile(Bitmap thumbnail, FileMarkers type)
		{
			Write(Version);
			WriteByte(NUMBERSTRINGS);
			m_Strings = new string[NUMBERSTRINGS + 1]; // +1 since they are numbered 1..N
			for (int index = 1; index <= NUMBERSTRINGS; index++)
			{
				m_Strings[index] = "";
			}
			if (Version < DataUtilities.MINIMUMSUPPORTEDVERSION)
				throw new UserException("[File_TooOld]");
			Debug.Assert(DataUtilities.MINIMUMSUPPORTEDVERSION >= 60); // assumed throughout now
			Write(SoftwareVersion.Version); // software version
			Write(DataUtilities.MINIMUMCOMPATIBLEVERSION); // minimum version to read this file
			WriteOptionalPNG(thumbnail);
		}

		/// <summary>just as a convenience this encapsulates the cast</summary>
		public void WriteByte(int i)
		{
			base.Write(Convert.ToByte(i));
		}

		/// <summary>Saves space by only writing identical strings once.  Does not distinguish null and ""</summary>
		public void WriteBufferedString(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				WriteByte(0);
				return;
			}
			for (int index = 1; index <= NUMBERSTRINGS; index++)
			{
				if (text == m_Strings[index])
				{
					WriteByte(index + REPEATMARKER);
					return;
				}
			}
			WriteByte(m_NextString);
			m_Strings[m_NextString] = text;
			base.Write(text);
			m_NextString += 1;
			if (m_NextString > NUMBERSTRINGS)
				m_NextString = 1;
		}

		/// <summary>Distinguishes null and "" on readback</summary>
		public void WriteOptionalString(string text)
		{
			Write(text != null);
			if (text != null)
				Write(text);
		}

		#region Lists
		public void Write(List<int> list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Count));
			Debug.Assert(list.Count < short.MaxValue);
			for (int index = 0; index <= list.Count - 1; index++)
			{
				base.Write(list[index]);
			}
		}

		public void Write(int[] list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Length));
			Debug.Assert(list.Length < short.MaxValue);
			for (int index = 0; index <= list.Length - 1; index++)
			{
				base.Write(list[index]);
			}
		}

		public void Write(List<string> list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Count));
			Debug.Assert(list.Count < short.MaxValue);
			for (int index = 0; index <= list.Count - 1; index++)
			{
				base.Write(list[index]);
			}
		}

		public void Write(string[] list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Count()));
			Debug.Assert(list.Count() < short.MaxValue);
			for (int index = 0; index <= list.Count() - 1; index++)
			{
				base.Write(list[index]);
			}
		}

		public void Write(List<PointF> list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Count));
			Debug.Assert(list.Count < short.MaxValue);
			for (int index = 0; index <= list.Count - 1; index++)
			{
				Write(list[index]);
			}
		}

		public void Write(PointF[] list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Length));
			Debug.Assert(list.Length < short.MaxValue);
			for (int index = 0; index <= list.Length - 1; index++)
			{
				Write(list[index]);
			}
		}

		#endregion

		#region Structure/Class types
		public void Write(Rectangle rct)
		{
			base.Write(rct.X);
			base.Write(rct.Y);
			base.Write(rct.Width);
			base.Write(rct.Height);
		}

		public void Write(RectangleF rct)
		{
			base.Write(rct.X);
			base.Write(rct.Y);
			base.Write(rct.Width);
			base.Write(rct.Height);
		}

		public void Write(PointF pt)
		{
			base.Write(pt.X);
			base.Write(pt.Y);
		}

		public void Write(SizeF sz)
		{
			base.Write(sz.Width);
			base.Write(sz.Height);
		}

		public void Write(Size sz)
		{
			base.Write(sz.Width);
			base.Write(sz.Height);
		}

		public void WriteOptional(RectangleF rct)
		{
			if (rct.IsEmpty)
				base.Write(false);
			else
			{
				base.Write(true);
				base.Write(rct.X);
				base.Write(rct.Y);
				base.Write(rct.Width);
				base.Write(rct.Height);
			}
		}

		public void Write(Color col)
		{
			base.Write(col.ToArgb());
		}

		public void Write(Guid x)
		{
			Write(x.ToByteArray());
		}

		public void WriteOptionalPNG(Bitmap image, System.Drawing.Imaging.ImageFormat format = null)
		{
			// writes a bitmap which can be nothing.  Note that this cannot be upgraded to cope with Image, because there is no proper support for saving WMF in .net
			if (image == null)
				Write(false);
			else
			{
				Write(true);
				using (System.IO.MemoryStream buffer = new System.IO.MemoryStream())
				{
					image.Save(buffer, format ?? System.Drawing.Imaging.ImageFormat.Png);
					Write(buffer.Length);
					Write(buffer.GetBuffer(), 0, (int)buffer.Length);
				}

			}
		}

#if APPLICATION
		public void WriteOptionalMemoryImage(MemoryImage image)
		{
			if (image == null)
				Write(false);
			else
			{
				Write(true);
				image.Save(this);
			}
		}
#endif
		#endregion

		#region Data objects

		// data objects are written into memory stream first so that we can get the length.  I want to use the same stream repeatedly,
		// for efficiency.  However, we actually need to use a DataWriter object for the benefit of the Datums
		// it doesn't matter that objects can be nested; the object writer will itself create sub versions as needed
		private DataWriter m_ObjectWriter;

		public void Write(Datum data)
		{
			if (data == null)
				WriteByte((byte)FileMarkers.Omitted);
			else
			{
				byte type = data.TypeByte;
				Debug.Assert(type > 0, "Writing data object which has returned TypeByte 0; class = " + data.GetType().Name);
				if (m_ObjectWriter == null)
					m_ObjectWriter = new DataWriter(this);
				m_ObjectWriter.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
				m_ObjectWriter.m_NextString = m_NextString;
				data.Save(m_ObjectWriter);
				int length = (int)m_ObjectWriter.BaseStream.Position;
				WriteByte(type);
				Write(length);
				Write(((System.IO.MemoryStream)m_ObjectWriter.BaseStream).GetBuffer(), 0, length);
				m_NextString = m_ObjectWriter.m_NextString;
			}
		}

		private DataWriter(DataWriter container) : base(new System.IO.MemoryStream(500), System.Text.Encoding.Unicode)
		{
			// version used to create a memory stream for temporarily writing objects
			Version = container.Version;
			// note this does not call InitialiseFile
			//m_aStrings = Nothing ' this will have strange effects because the reader will work as a single reader not nested ones
			m_Strings = container.m_Strings;
		}

		public void WriteDatumList(IList listOfDatum)
		{
			// contents must all be Datum.  Cannot easily be declared as List<Datum> because often the caller will have a List<Shape>
			// but can't call this method Write, since unlike VB this gets called for byte[]
			if (listOfDatum == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			Debug.Assert(listOfDatum.Count < short.MaxValue);
			base.Write(Convert.ToInt16(listOfDatum.Count));
			for (int index = 0; index <= listOfDatum.Count - 1; index++)
			{
				Write((Datum)listOfDatum[index]);
			}
		}

		public void Write(DatumList list)
		{
			if (list == null)
			{
				base.Write(Convert.ToInt16(0));
				return;
			}
			base.Write(Convert.ToInt16(list.Count));
			Debug.Assert(list.Count < short.MaxValue);
			foreach (Datum data in list.Values)
			{
				Write(data);
			}
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			m_ObjectWriter?.Dispose(disposing);
		}

	}

	public class DataReader : IDisposable
	{
		//Inherits IO.BinaryReader
		// This no longer inherits, because it needs to decide the text encoding for constructing the BinaryReader

		public int Version;
		public FileMarkers FileType;
		// these are read from the file header:
		public float SoftwareVersion = 12; // (ie 0.12.0)
		public float MinimumSoftwareVersion = 12;
		public Image Thumbnail; // loaded during the constructor if there is one in the file
								// will be disposed by Dispose.  If the GUI wants to keep the image separately it should assign Nothing to this

		private System.IO.BinaryReader Reader;
		private System.IO.BinaryReader UnusedReader; // see Initialise function
		private string[] m_Strings;
		private const byte REPEATMARKER = 128;
		private int m_NumberStrings = 7;
		private string m_File;

		public DataReader(string file, FileMarkers type)
		{
			m_File = file;
			Reader = new System.IO.BinaryReader(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read), System.Text.Encoding.Unicode);
			try
			{
				InitialiseFile(type);
			}
			catch
			{
				Reader.Close();
				throw;
			}
		}

		public DataReader(System.IO.Stream fs, FileMarkers type)
		{
			m_File = "[Stream]";
			Reader = new System.IO.BinaryReader(fs, System.Text.Encoding.Unicode);
			try
			{
				InitialiseFile(type);
			}
			catch
			{
				Reader.Close();
				throw;
			}
		}
		private void InitialiseFile(FileMarkers type)
		{
			// Shared between the constructors
			FileType = (FileMarkers)Reader.ReadByte();
			if (FileType != type && type != FileMarkers.Undetermined)
				throw new FileTypeException("Wrong file type marker: " + FileType);
			Version = Reader.ReadInt32();
			Debug.WriteLineIf(Version == 71, "Opening file version 71: this should only happen for the latest v1 files (this number was also used for the earliest v2 files");
			if (Version < DataUtilities.MINIMUMSUPPORTEDVERSION)
				throw new UserException("[File_TooOld]", true);
			m_NumberStrings = ReadByte();
			m_Strings = new string[m_NumberStrings + 1];
			SoftwareVersion = ReadSingle();
			MinimumSoftwareVersion = ReadSingle();
			//if (Globals.Root != null && MinimumSoftwareVersion > SAW.SoftwareVersion.Version) // AM is nothing when loading initial configs
			//	throw new UserException(Strings.Item("File_Too_New").Replace("%0", SAW.SoftwareVersion.VersionStringFromNumber(MinimumSoftwareVersion)));
			Thumbnail = ReadOptionalPNG();
		}

		public string ReadBufferedString()
		{
			int b = ReadByte();
			if (b == 0)
				return "";
			if (b >= REPEATMARKER)
				return m_Strings[b - REPEATMARKER];
			m_Strings[b] = ReadString();
			return m_Strings[b];
		}

		public string ReadOptionalString()
		{
			return ReadBoolean() ? ReadString() : null;
		}

		public Bitmap ReadOptionalPNG()
		{
			// reads image, which might have been nothing - see matching method in DataWriter
			if (!ReadBoolean())
				return null;
			// file contains a thumbnail
			long length = ReadInt64();
			byte[] buffer = new byte[length + 1];
			this.Read(buffer, 0, (int)length);
			System.IO.MemoryStream strm = new System.IO.MemoryStream(buffer);
			return new Bitmap(strm);
		}

#if APPLICATION
		public MemoryImage ReadOptionalMemoryImage()
		{
			if (!ReadBoolean())
				return null;
			return new MemoryImage(this);
		}
#endif

		public void ExtractEmbeddedFile(string saveFile)
		{
			// saves a file which was included using writer.EmbedFile
			int length = ReadInt32();
			byte[] buffer = new byte[length - 1 + 1];
			Reader.BaseStream.Read(buffer, 0, length);
			System.IO.File.WriteAllBytes(saveFile, buffer);
		}

		#region Basic types which just call BinaryReader
		public byte ReadByte()
		{
			return Reader.ReadByte();
		}

		public bool ReadBoolean()
		{
			return Reader.ReadBoolean();
		}

		public int ReadInt32()
		{
			return Reader.ReadInt32();
		}

		public short ReadInt16()
		{
			return Reader.ReadInt16();
		}

		public uint ReadUint32()
		{
			return Reader.ReadUInt32();
		}

		public long ReadInt64()
		{
			return Reader.ReadInt64();
		}

		public float ReadSingle()
		{
			return Reader.ReadSingle();
		}

		public string ReadString()
		{
			return Reader.ReadString();
		}

		public void Read(byte[] buffer, int index, int count)
		{
			Reader.Read(buffer, index, count);
		}

		public byte[] ReadBytes(int count)
		{
			return Reader.ReadBytes(count);
		}

		#endregion

		#region Lists
		public List<int> ReadListInteger()
		{
			int last = Reader.ReadInt16() - 1; // for loop possibly more efficient if we stored the UBound instead of the Count
			List<int> list = new List<int>(last + 1);
			for (int index = 0; index <= last; index++)
			{
				list.Add(Reader.ReadInt32());
			}
			return list;
		}

		public List<string> ReadListString()
		{
			int last = Reader.ReadInt16() - 1; // for looper possibly more efficient if we stored the UBound instead of the Count
			List<string> list = new List<string>(last + 1);
			for (int index = 0; index <= last; index++)
			{
				list.Add(Reader.ReadString());
			}
			return list;
		}

		public List<PointF> ReadListPoints()
		{
			int last = Reader.ReadInt16() - 1; // for looper possibly more efficient if we stored the UBound instead of the Count
			List<PointF> list = new List<PointF>(last + 1);
			for (int index = 0; index <= last; index++)
			{
				list.Add(ReadPointF());
			}
			return list;
		}

		#endregion

		#region Class types

		public Rectangle ReadRectangle()
		{
			return new Rectangle(Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32());
		}

		public RectangleF ReadRectangleF()
		{
			return new RectangleF(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
		}

		public RectangleF ReadRectangleFOptional()
		{
			if (Reader.ReadBoolean())
				return ReadRectangleF();
			return RectangleF.Empty;
		}

		public PointF ReadPointF()
		{
			return new PointF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		public SizeF ReadSizeF()
		{
			return new SizeF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		public Size ReadSize()
		{
			return new Size(Reader.ReadInt32(), Reader.ReadInt32());
		}

		public Color ReadColour()
		{
			Color c = Color.FromArgb(Reader.ReadInt32());
			if (c.A == 0)
				return Color.Empty; // need to do this otherwise it returns IsEmpty = false
			return c;
		}

		public Guid ReadGuid()
		{
			byte[] bytes = new byte[16];
			Read(bytes, 0, 16);
			return new Guid(bytes);
		}
		#endregion

		#region Data objects
		// from version 40 the length of each object is written after the type byte and before the data.
		// This might allow all versions of the software to load the data, assuming that any additions are on the end of the record
		public Dictionary<Guid, Datum> LoadedObjects = new Dictionary<Guid, Datum>(); // list of Datum loaded by ID
																					  // LoadedObjects will survive the Close command
		public Datum ReadData(FileMarkers expectedType = FileMarkers.Undetermined)
		{
			FileMarkers type = (FileMarkers)ReadByte();
			if (type == FileMarkers.Omitted)
				return null; // note that Omitted does not store the length
			var expectedLength = ReadInt32();
			int startPosition = (int)Reader.BaseStream.Position;
			if (expectedType != FileMarkers.Undetermined && expectedType != type)
				throw new InvalidDataException("Expected datatype: " + expectedType);
			// in practice all the data will be derived from Datum
			var create = Datum.Create(type);
			if (create.TypeByte == (byte)FileMarkers.Document && LoadedObjects.Count > 0)
			{
				// in case of documents need to isolate the created object IDs.  But not if this was the root object (hence second condition)
				Dictionary<Guid, Datum> old = LoadedObjects;
				try
				{
					LoadedObjects = new Dictionary<Guid, Datum>();
					create.Load(this);
				}
				finally
				{
					LoadedObjects = old;
				}
			}
			else
				create.Load(this);
			if (expectedLength > -1)
			{
				int actualLength = (int)(Reader.BaseStream.Position - startPosition);
				if (actualLength > expectedLength)
					throw new InvalidDataException("Reading datatype " + type + " used " + actualLength + ", whereas the record was only actually " + expectedLength + " bytes long");
				else if (actualLength < expectedLength)
				{
					Debug.WriteLine("Discarding " + (expectedLength - actualLength) + " bytes when reading datatype: " + type);
					Reader.BaseStream.Seek(expectedLength - actualLength, System.IO.SeekOrigin.Current);
				}
			}
			if (!create.ID.IsEmpty() && !create.ID.Equals(ButtonStyle.idSelectionDefault)) // ButtonStyles can use empty IDs for those which don't need referencing
			{
				if (!LoadedObjects.ContainsKey(create.ID))
					LoadedObjects.Add(create.ID, create);
				else
				{
					Guid old = create.ID;
					create.ID = Guid.NewGuid();
					if (FileType != FileMarkers.ErrorReport)
						Utilities.LogSubError("Object ID repeated in file: type=" + create.TypeByte + ", ID changed from: " + old + ", to: " + create.ID + ", file=" + m_File);
				}
			}
			return create;
		}

		public List<Shape> ReadShapeList()
		{
			int last = Reader.ReadInt16() - 1; // for loop possibly more efficient if we stored the UBound instead of the Count
			List<Shape> list = new List<Shape>(last + 1);
			for (int index = 0; index <= last; index++)
			{
				Shape shp = (Shape)ReadData(); // shapes can set status deleted to not load
				if (shp.Status != Shape.StatusValues.Deleted)
					list.Add(shp);
			}
			return list;
		}

		public List<Datum> ReadDataList(FileMarkers eRequireType = FileMarkers.Undetermined)
		{
			// like the above, apart from the return type.  Also if eRequireType is changed to any other type, then only that type object will be permitted
			int last = Reader.ReadInt16() - 1;
			List<Datum> list = new List<Datum>(last + 1);
			for (int index = 0; index <= last; index++)
			{
				list.Add(ReadData(eRequireType));
			}
			return list;
		}
		#endregion

		public class FileTypeException : Exception
		{

			public FileTypeException(string message) : base(message)
			{
			}

		}

		public void Dispose()
		{
			Reader.Close();
			UnusedReader?.Close();
			UnusedReader = null;
		}

		public void Close()
		{
			Dispose();
		}
	}

	public class InvalidDataException : Exception
	{
		public InvalidDataException(string message) : base(message)
		{
		}
	}

}
