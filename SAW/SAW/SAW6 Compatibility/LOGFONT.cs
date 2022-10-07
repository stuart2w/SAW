namespace SAW
{
	/// <summary>Support for Win32 font object, used in old data</summary>
	internal class LOGFONT : IArchivable
	{
		public string FontName;
		public int lfHeight;
		public int lfWeight;
		public byte lfUnderline;
		public byte lfItalic;
		public byte lfStrikeout;

		public static LOGFONT CreateDefault()
		{
			return new LOGFONT { FontName = "Arial", lfHeight = -16 };
		}

		#region IArchivable Members

		public void Read(ArchiveReader ar)
		{
			FontName = ar.ReadStringL();
			lfHeight = ar.ReadInt32();
			lfWeight = ar.ReadInt32();
			lfUnderline = ar.ReadByte();
			lfItalic = ar.ReadByte();
			lfStrikeout = ar.ReadByte();
		}

		public void Write(ArchiveWriter ar)
		{
			ar.WriteStringL(FontName);
			ar.Write(lfHeight);
			ar.Write(lfWeight);
			ar.Write(lfUnderline);
			ar.Write(lfItalic);
			ar.Write(lfStrikeout);
		}

		#endregion
	}
}