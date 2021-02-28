using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace SAW
{
	/// <summary>Shared by SharedImage and SharedResource.  Either of these can be stored in document SharedResource list, with CRC detecting duplicates.
	/// These should be stored in data objects as SharedReference&gt;SharedImage&lt; etc.</summary>
	public abstract class SharedBase : Datum
	{
		public int CRC { get; set; }
	}


	/// <summary>Represents the raster data for an image which can be shared between several data objects
	/// the data is considered invariant, and is not backed up in the undo buffer.  Only CopyFrom with depth >= Duplicate actually
	/// makes a copy of the data.  CRC is used to detect duplicates - only one copy of each is stored in the document
	/// Also supports referencing one of the application resources; in which case the actual image is not stored in here.  It is loaded from the resources as needed</summary>
	public class SharedImage : SharedBase
	{

		private MemoryImage m_Image;
		private string m_ResourceName = ""; // if this is defined, then m_Image is Nothing, and CRC is based upon the name not the image data

		#region Constructors
		public SharedImage()
		{
		}

		public SharedImage(MemoryImage image, int CRC = 0)
		{
			// CRC can be provided if already calculated - to avoid recalculating it
			m_Image = image;
			if (CRC == 0)
				CRC = image.CalcCRC();
			this.CRC = CRC;
		}

		/// <summary>it is assumed that the document has already been checked for any duplicates</summary>
		public static SharedImage CreateFromFile(string file, int CRC = 0)
		{
			if (CRC == 0)
				CRC = CRCCalc.Calc(file);
			return new SharedImage { m_Image = new MemoryImage(file), CRC = CRC };
		}

		public static SharedImage CreateFromStream(Stream stream, bool isSVG, int CRC = 0)
		{
			SharedImage create = new SharedImage();
			create.m_Image = new MemoryImage(stream, isSVG);
			if (CRC == 0)
				CRC = create.m_Image.CalcCRC();
			create.CRC = CRC;
			return create;
		}

		public static SharedImage CreateForResource(string resourceID)
		{
			SharedImage image = new SharedImage();
			image.m_ResourceName = resourceID;
			// Note that the CRC must NOT use string.GetHashCode because that can vary between machines.  The value is stored in the data and must be reproducible
			image.CRC = CRCCalc.Calc(System.Text.Encoding.Unicode.GetBytes(resourceID));
			return image;
		}

		/// <summary>Can implicitly accept a SharedReference&lt;SharedImage&gt; wherever a SharedImage is expected</summary>
		public static implicit operator SharedImage(SharedReference<SharedImage> reference) => reference?.Content;

		#endregion

		#region Datum
		public override void Load(DataReader reader)
		{
			base.Load(reader);
			CRC = reader.ReadInt32();
			if (reader.Version < 76 || reader.ReadBoolean()) // Version 76 onwards start with a boolean indicating what is stored in the object
				m_Image = new MemoryImage(reader);
			else
				m_ResourceName = reader.ReadString();
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			writer.Write(CRC);
			if (writer.Version >= 76)
			{
				// We start with a boolean indicating whether it is an image (true) or string (false)
				if (m_Image != null)
				{
					writer.Write(true);
					m_Image.Save(writer);
				}
				else
				{
					writer.Write(false);
					writer.Write(m_ResourceName);
				}
			}
			else
				m_Image.Save(writer);
		}

		public override byte TypeByte
		{ get { return (byte)FileMarkers.SharedBitmap; } }

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			SharedImage sharedImage = (SharedImage)other;
			if (depth >= CopyDepth.Duplicate)
			{
				m_Image = sharedImage.m_Image?.Clone();
				CRC = sharedImage.CRC;
			}
			m_ResourceName = sharedImage.m_ResourceName;
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{ }

		#endregion

		public Image GetNetImage(int sizeIfResource = 32)
		{
			// once the image is created it is not disposed at the moment.  If it is being drawn on-screen it will be needed repeatedly
			if (!string.IsNullOrEmpty(m_ResourceName))
				return GUIUtilities.VariableSizeImage(m_ResourceName, "", sizeIfResource); // RM.GetObject(m_strResourceName)
			return m_Image.GetNetImage();
		}

		/// <summary>Returns true if this takes minimal memory (i.e. it is a link to a resource image)</summary>
		public bool IsLightweight
		{ get { return !string.IsNullOrEmpty(m_ResourceName); } }

		/// <summary>Only permitted if IsLightweight.  Returns the resource name to use</summary>
		public string ResourceName
		{
			get
			{
				if (string.IsNullOrEmpty(m_ResourceName))
					throw new InvalidOperationException();
				return m_ResourceName;
			}
		}

		/// <summary>Only permitted if not IsLightweight</summary>
		public MemoryImage MemoryImage
		{
			get
			{
				if (!string.IsNullOrEmpty(m_ResourceName))
					throw new InvalidOperationException();
				return m_Image;
			}
		}

		/// <summary>Returns image size.  Might avoid reloading it if it was previously loaded.  Caller should provide a preferred size in case this is a resource available in various sizes</summary>
		public Size GetSize(int sizeIfResource = 32)
		{ // atm this just relies on the caching inside MemoryImage - there is no further logic here
			if (!string.IsNullOrEmpty(m_ResourceName))
				return GetNetImage(sizeIfResource).Size;
			return m_Image.Size; // might be cached internally in the image
		}

		public void Release()
		{
			m_Image?.Release();
		}
	}

	/// <summary>For any data that needs to be embedded within the file, other than images.  Is a combination of SharedImage and MemoryImage.
	/// This has no understanding of type - it is just a data buffer</summary>
	public class SharedResource : SharedBase
	{
		public byte[] Buffer;
		private MemoryStream m_strm;
		/// <summary>Original filename if known.  The file extension is important even if [filename?] not known</summary>
		public string Filename { get; private set; }

		#region Creators
		public static SharedResource CreateFromFile(string file, int CRC = 0)
		{
			SharedResource resource = new SharedResource();
			resource.Buffer = File.ReadAllBytes(file);
			resource.Filename = Path.GetFileName(file);
			if (CRC == 0)
				CRC = CRCCalc.Calc(resource.Buffer);
			resource.CRC = CRC;
			return resource;
		}

		public static SharedResource CreateFromStream(Stream strm, string fileExtension, int intCRC = 0)
		{
			Debug.Assert(fileExtension.StartsWith("."));
			SharedResource resource = new SharedResource();
			int length = (int)strm.Length;
			strm.Seek(0, SeekOrigin.Begin);
			resource.Buffer = new byte[length - 1 + 1];
			strm.Read(resource.Buffer, 0, length);
			// we don't keep the original stream, just the buffer we have filled from it
			if (intCRC == 0)
				intCRC = CRCCalc.Calc(resource.Buffer);
			resource.CRC = intCRC;
			resource.Filename = "auto" + fileExtension;
			return resource;
		}

		public static SharedResource CreateFromBuffer(byte[] buffer, string fileExtension)
		{
			Debug.Assert(fileExtension.StartsWith("."));
			SharedResource resource = new SharedResource();
			resource.Buffer = buffer;
			resource.CRC = CRCCalc.Calc(resource.Buffer);
			resource.Filename = "auto" + fileExtension;
			return resource;
		}

		#endregion

		#region Datum
		public override void Load(DataReader reader)
		{
			if (reader.Version >= 127)
				base.Load(reader);
			CRC = reader.ReadInt32();
			int length = reader.ReadInt32();
			Buffer = reader.ReadBytes(length);
			Filename = reader.ReadString();
		}

		public override void Save(DataWriter writer)
		{
			if (writer.Version >= 127)
				base.Save(writer);
			writer.Write(CRC);
			writer.Write(Buffer.Length);
			writer.Write(Buffer);
			writer.Write(Filename);
		}

		public override bool IdenticalTo(Datum other)
		{
			return CRC == ((SharedResource)other).CRC;
		}

		public override void CopyFrom(Datum other, CopyDepth depth, Mapping mapID)
		{
			SharedResource otherResource = (SharedResource)other;
			CRC = otherResource.CRC;
			Buffer = otherResource.Buffer;
			Filename = otherResource.Filename;
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{ }

		#endregion

		public void SaveContent(string file)
		{
			File.WriteAllBytes(file, Buffer);
		}

		public Stream GetStream()
		{
			if (m_strm == null)
			{
				if (Buffer == null)
					throw new ObjectDisposedException("SharedResource");
				m_strm = new MemoryStream(Buffer);
			}
			m_strm.Seek(0, SeekOrigin.Begin);
			return m_strm;
		}

		public override byte TypeByte
		{
			get { return (byte)FileMarkers.SharedResource; }
		}

	}
}
