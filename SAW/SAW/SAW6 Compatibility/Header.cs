using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>C++ SAW had the header information as a separate object.  Always one header per document</summary>
	public class Header : IArchivable
	{
		/// <summary>Not used in current version, nor stored in new files</summary>
		public short Version;
		public Point DesktopSize;
		public Rectangle MainWindowBounds { get; private set; }
		/// <summary>NOTE: NO LONGER USED.  Stored in Page now</summary>
		[Obsolete]
		public int PromptID;
		/// <summary>Doesn't appear to actually be used in SAW6 or maintained correctly (always 10000).  So unused here and just set to something valid if writing to SAW6</summary>
		public int NextControlID;

		/// <summary>Not from SAW6, optional.  Bounds of screens available when MainWindowBounds was set</summary>
		public Rectangle[] DesktopScreens;

		public const string SERIALISEHEADER = "SAW3.4 Serialized Binary File\r\n";

		public void Read(ArchiveReader ar)
		{
			string strSerializeID = ar.ReadStringL();
			if (strSerializeID != SERIALISEHEADER)
				throw new System.IO.IOException("File does not start with correct header - not a SAW6 file");
			Version = ar.ReadInt16();
			if (Version < 5004)
				throw new InvalidOperationException("Cannot read SAW files from before version 5.00.4");
			DesktopSize = ar.ReadPoint();
			MainWindowBounds = ar.ReadRectangleRaw();
			PromptID = -1;
			int.TryParse(ar.ReadStringL(), out PromptID);
			NextControlID = (int)ar.ReadUInt32();
		}

		public void Write(ArchiveWriter ar)
		{
			Debug.Assert(Version == 6200);
			ar.WriteStringL(SERIALISEHEADER, true);
			ar.Write(Version);
			ar.Write(DesktopSize);
			ar.WriteRaw(MainWindowBounds);
			ar.WriteStringL(PromptID < 0 ? "" : PromptID.ToString());
			ar.Write(NextControlID);
		}

		public void Read(DataReader reader)
		{ // Version not included in Splash format - file header contained that
			DesktopSize = new Point(reader.ReadInt32(), reader.ReadInt32());
			MainWindowBounds = reader.ReadRectangle();
			if (reader.Version >= 126)
			{
				int n = reader.ReadByte();
				if (n > 0)
					DesktopScreens = (from int i in Enumerable.Range(0, n) select reader.ReadRectangle()).ToArray();
				else
					DesktopScreens = null;
			}
		}

		public void Write(DataWriter writer)
		{
			writer.Write(DesktopSize.X);
			writer.Write(DesktopSize.Y);
			writer.Write(MainWindowBounds);
			if (writer.Version >= 126)
			{
				writer.WriteByte(DesktopScreens?.Length ?? 0);
				if (DesktopScreens != null)
					foreach (Rectangle rct in DesktopScreens)
						writer.Write(rct);
			}
		}

		public static Header CreateDefault()
		{
			Size sz = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
			return new Header
			{
				Version = 5025,
				DesktopSize = new Point(sz.Width, sz.Height),
				MainWindowBounds = new Rectangle(sz.Width / 10, sz.Height * 2 / 3, sz.Width * 8 / 10, sz.Height / 3)
			};
		}

		public void CopyFrom(Header other)
		{
			Version = other.Version;
			DesktopSize = other.DesktopSize;
			MainWindowBounds = other.MainWindowBounds;
			PromptID = other.PromptID;
			//NextControlID = other.NextControlID;
		}

		/// <summary>Sets MainWindowBounds, but also records current desktops</summary>
		public void SetWindowBounds(Rectangle bounds)
		{
			if (bounds.IsEmpty)
				throw new Exception("Empty window bounds: " + bounds);
			MainWindowBounds = bounds;
			DesktopScreens = (from s in Screen.AllScreens select s.WorkingArea).ToArray();
		}

		/// <summary>Returns MainWindowBounds, but adjusts if differs from screens as stored</summary>
		public Rectangle GetAdjustedWindowBounds()
		{
			// if not list was stored in the data then use current screens.  This will cause no adjustment, but will check it is in bounds
			var list = DesktopScreens ?? (from s in Screen.AllScreens select s.WorkingArea).ToArray();
			int screenIndex = 0; // will default to first in list if none found
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].Contains(MainWindowBounds.Centre()))
					screenIndex = i;
			}
			Rectangle screen = list[screenIndex];
			//Debug.WriteLine($"screenIndex={screenIndex}, screen bounds={screen}");
			//Debug.WriteLine($"MainWindowBounds={MainWindowBounds}");
			// express as fraction of source screen
			RectangleF fraction = new RectangleF((float)(MainWindowBounds.X - screen.X) / screen.Width,
				(float)(MainWindowBounds.Y - screen.Y) / screen.Height,
				(float)MainWindowBounds.Width / screen.Width,
				(float)MainWindowBounds.Height / screen.Height);
			//Debug.WriteLine($"fraction={fraction}");

			// current screen to use
			screen = Screen.FromPoint(MainWindowBounds.Centre()).WorkingArea; // if point is out of bounds it seems to return nearest screen
			//Debug.WriteLine($"Current screen {screenIndex}={screen}");
			Rectangle bounds;
			if (DesktopScreens != null) // doesn't make sense doing this unless the source screen is available (actually should cancel out to be no change)
				bounds = new Rectangle((int)(screen.X + screen.Width * fraction.X),
					(int)(screen.Y + screen.Height * fraction.Y),
					(int)(screen.Width * fraction.Width),
					(int)(screen.Height * fraction.Height));
			else
				bounds = MainWindowBounds;
			//Debug.WriteLine($"bounds={bounds}");
			bounds.X = Math.Max(bounds.X, screen.Left);
			bounds.Y = Math.Max(bounds.Y, screen.Top);
			bounds.X = Math.Min(bounds.X, Math.Max(screen.Right - bounds.Width, screen.Centre().X + screen.Width / 2 - bounds.Width / 2)); // Max part is in case set is wider than the screen; will go no further left than centred
			bounds.Y = Math.Min(bounds.Y, Math.Max(screen.Bottom - bounds.Height, screen.Centre().Y + screen.Height / 2 - bounds.Height / 2));
			return bounds;
		}

	}
}