using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace SAW
{
	/// <summary>Miscellaneous extension methods</summary>
	static class Extensions
	{

		public static bool IsEmpty(this Guid id) => id.Equals(Guid.Empty);
		public static bool IsZero(this IntPtr p) => p.Equals(IntPtr.Zero);
		/// <summary>NOTE: this does not mean it is an invalid IntPtr - rather that it equals HANDLE_INVALID_HANDLE in C#</summary>
		public static bool IsInvalid(this IntPtr p) => p.Equals(InvalidIntPtr);

		private static IntPtr InvalidIntPtr = IntPtr.Subtract(IntPtr.Zero, 1);

		/// <summary>Equivalent to overloaded version taking 4 points, but accepting a Length = 4 array instead</summary>
		public static void AddBezier(this GraphicsPath path, PointF[] points)
		{
			if (points.Length != 4)
				throw new ArgumentException();
			path.AddBezier(points[0], points[1], points[2], points[3]);
		}

		public static PointF[] GetSingleCurve(this GraphicsPath path, int index)
		{
			Debug.Assert((path.PathTypes[index + 1] & Lined.PATHTYPEMASK) == Lined.PATHBEZIER); // index+1 because first point in curve has type of line leading to it (previous line) or 0 for start
			PointF[] points = path.PathPoints;
			PointF[] P = new PointF[4];
			P[0] = points[index];
			P[1] = points[index + 1];
			P[2] = points[index + 2];
			P[3] = points[index + 3];
			return P;
		}

		/// <summary>Clones the list, returning a new list object (referencing the same content objects - these are not cloned)</summary>
		public static List<T> Clone<T>(this List<T> list)
		{
			List<T> newList = new List<T>();
			newList.AddRange(list);
			return newList;
		}

		/// <summary>Replaces all of the given characters.  Intended for making sure that names are legal filenames</summary>
		public static string ReplaceCharacters(this string str, char[] remove, char replaceWith = '-')
		{
			if (str == null)
				return "";
			foreach (char ch in remove)
			{
				str = str.Replace(ch, replaceWith);
			}
			return str;
		}

		/// <summary>Returns the bounding rectangle of the image, always with location = 0,0 (this is just a convenience for various graphics functions)</summary>
		public static Rectangle Bounds(this Image image)
		{
			return new Rectangle(0, 0, image.Width, image.Height);
		}

		/// <summary>True if string in form [blah_blah] and can be translated by strings.translate</summary>
		internal static bool IsTranslatable(this string str)
		{
			return str != null && str.StartsWith("[") && str.EndsWith("]");
		}

		/// <summary>Returns the key of the max VALUE in the dictionary.
		/// Returns default(K) if list is empty - which will be null for classes</summary>
		public static K GetMaxKey<K>(this Dictionary<K, int> dict)
		{ // https://stackoverflow.com/questions/2805703/good-way-to-get-the-key-of-the-highest-value-of-a-dictionary-in-c-sharp
			if (!dict.Any()) return default(K);
			return dict.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
			// there is no easy way to do this for a generic value since it requires that V1 > V2 is possible, and no simple where constraint specifies numbers
		}

		/// <summary>Returns the key of the max VALUE in the dictionary </summary>
		public static K GetMaxKey<K>(this Dictionary<K, float> dict)
		{ // https://stackoverflow.com/questions/2805703/good-way-to-get-the-key-of-the-highest-value-of-a-dictionary-in-c-sharp
			if (!dict.Any()) return default(K);
			return dict.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
		}

		#region Keycode conversion
		// an alternative to these will be needed if not under Windows.  They are needed in frmMain.SimulateKey; other uses might be avoidable

		public static Keys ToKeyData(this char ch)
		{
			int VK = Windows.VkKeyScan(ch);
			// bottom byte returns the VK code; top byte it is: 1 = Shift, 2 = Control, 4 =Alt (8 = Hankaku - but I think I can ignore that one!)
			Keys eKey = (Keys)(VK & 255);
			if ((VK & 0x100) > 0)
				eKey |= Keys.Shift;
			if ((VK & 0x200) > 0)
				eKey |= Keys.Control;
			if ((VK & 0x400) > 0)
				eKey |= Keys.Alt;
			return eKey;
		}

		public static char ToCharacter(this Keys key)
		{
			// Gets (hopefully) the character typed by the key
			// Alternative is: http://stackoverflow.com/questions/6214326/translate-keys-to-char
			//' The problem is that the converter returns things like "Shift+X"

			byte[] keyStates = new byte[256];
			const byte keyPressed = 0x80;
			keyStates[(int)(key & Keys.KeyCode)] = keyPressed;
			keyStates[(int)Keys.ShiftKey] = (byte)((key & Keys.Shift) == Keys.Shift ? keyPressed : 0);
			keyStates[(int)Keys.ControlKey] = (byte)((key & Keys.Control) == Keys.Control ? keyPressed : 0);
			keyStates[(int)Keys.Menu] = (byte)((key & Keys.Alt) == Keys.Alt ? keyPressed : 0);

			StringBuilder sb = new StringBuilder(10);
			if (Windows.ToUnicodeEx(key, 0, keyStates, sb, sb.Capacity, 0, InputLanguage.CurrentInputLanguage.Handle) == 1)
				return sb[0];
			return '\0'; // Could not be converted
		}

		#endregion

		#region Control extensions

		/// <summary>Sets the value, ignoring any errors (which are raised if out of range) </summary>
		public static void SafeSetValue(this NumericUpDown nud, float value)
		{
			try
			{
				nud.Value = (decimal)value;
			}
			catch
			{
			}
		}

		internal static void FillKeyValuePair(this ComboBox cmb, List<KeyValuePair<int, string>> entries)
		{
			cmb.ValueMember = "Key";
			cmb.DisplayMember = "Value";
			cmb.DataSource = entries;
			// Must be after ValueMember is set, because this may raise a SelectedIndexChanged event
			// and if the form tries to read the Value it would be meaningless unless ValueMember has already been set
		}

		#endregion

	}
}
