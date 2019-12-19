using System;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace SAW
{

	public static class Utilities
	{
		#region Error handling
		internal static void LogSubError(Exception ex, bool permanent = false, bool withoutTrace = false, bool bolWithDistributorFire = false)
		{
			Debug.Fail(ex.Message);
			string message = withoutTrace ? ex.Message : ex.ToString();
			frmErrorReport.RecordSubError(message.Replace("\r\n", " / "));
			try
			{
				ILogger objLog = permanent == false ? Globals.Root.Log : Globals.Root.LogPermanent;
				objLog.WriteLine(message);
			}
			catch // will also catch AM or AM.Log = nothing
			{
			}
		}

		internal static void LogSubError(string message, bool permanent = false, bool bolWithDistributorFire = false)
		{
			Debug.Fail(message);
			frmErrorReport.RecordSubError(message);
			try
			{
				if (permanent)
					Globals.Root.LogPermanent.WriteLine(message);
				else
					Globals.Root.Log.WriteLine(message);
			}
			catch // will also catch AM or AM.Log = nothing
			{
			}
		}

		/// <summary>Can be used in place of Debug.Assert in places where asserting is awkward (e.g. painting, and some mouse movement where subsequent moves will crash unless the code is actually stopped.
		/// This is still ignored at runtime</summary>
		[Conditional("DEBUG")]
		public static void ErrorAssert(bool condition, string message = "ErrorAssert failed")
		{
			if (!condition) // **** do not remove this exception
				throw new InvalidOperationException(message);
		}

		[Conditional("DEBUG")]
		public static void ErrorFail(string message = "ErrorAssert failed")
		{
			// **** do not remove this exception
			throw new InvalidOperationException(message);
		}

		#endregion

		#region Misc utilities
		/// <summary>= AM.CurrentConfig.Low_Graphics, but can safely be called during start up when some of these objects haven't yet been defined</summary>
		internal static bool Low_Graphics_Safe()
		{
			if (Globals.Root?.CurrentConfig == null)
				return false;
			return Globals.Root.CurrentConfig.Low_Graphics;
		}

		/// <summary>= AM.CurrentConfig.ReadBoolean(Config.Key_Shortcuts, True), but can safely be called during start up when some of these objects haven't yet been defined</summary>
		internal static bool Key_Shortcuts_Safe()
		{
			if (Globals.Root?.CurrentConfig == null)
				return true;
			return Globals.Root.CurrentConfig.ReadBoolean(Config.Key_Shortcuts, true);
		}

		internal const int MB = 1024 * 1024;

		internal static string MemoryText(long fileSize, long referenceSize = -1)
		{
			// intReferenceSize, if provided, is used to determine the scaling to use
			if (referenceSize < 0)
				referenceSize = fileSize;
			if (referenceSize > MB * 10)
				return (fileSize / MB).ToString("0.0") + "MB";
			else if (referenceSize > MB)
				return (fileSize / MB).ToString("0.00") + "MB";
			else if (referenceSize > 1024 * 10)
				return (fileSize / 1024).ToString("0.0") + "kb";
			else if (referenceSize > 1024)
				return (fileSize / 1024).ToString("0.00") + "kb";
			else
				return fileSize + "bytes";
		}

		internal static bool IsImageFile(string file)
		{
			// Returns true if the filename has an extension indicating that it is an image
			switch (Path.GetExtension(file).ToLower())
			{
				case ".png":
				case ".gif":
				case ".bmp":
				case ".jpg":
				case ".jpeg":
				case ".wmf":
				case ".emf":
					return true;
				default:
					return false;
			}
		}

		public static bool DirectoryHasFiles(string folder)
		{
			if (Directory.GetFiles(folder).Any())
				return true;
			foreach (string sub in Directory.GetDirectories(folder))
			{
				if (DirectoryHasFiles(sub))
					return true;
			}
			return false;
		}

		/// <summary>Roughly equivalent of VB Val</summary>
		public static int IntVal(string text)
		{
			if (string.IsNullOrEmpty(text)) return 0;
			try
			{
				if (text.Contains('.') || text.Contains(','))
					return (int)float.Parse(text);
				return int.Parse(text);
			}
			catch (FormatException)
			{ return 0; } // deliberately ignores errors
		}

		public static void Swap<T>(ref T A, ref T B)
		{
			T C = A;
			A = B;
			B = C;
		}

		#endregion

	}
}
