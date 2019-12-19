using System;
using System.Diagnostics;
using System.Reflection;

namespace SAW
{
	public class SoftwareVersion
	{
		// will use the assembly version to determine the version
		// however, for the auto update it is helpful if the version as a string collates correctly (and the assembly one doesn't:
		// "1.4.1" > "1.14.1")
		// this system allows minor versions up to 99, and builds up to 9, giving X.YY.Z  There is no limit on the major version
		// both string and number will collate correctly.
		// revision number is ignored

		/// <summary>Major version number, e.g. 1, 2</summary>
		public static byte MajorVersion
		{ get { return (byte)Assembly.GetExecutingAssembly().GetName().Version.Major; } }

		/// <summary>Version in the form 202.3</summary>
		public static float Version
		{
			get
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				float versionNumber = version.Major * 100 + version.Minor + (float)version.Build / 10;
				return versionNumber;
			}
		}

		/// <summary>Version string as displayed to the user</summary>
		public static string VersionString
		{
			get
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				return version.Major.ToString("0") + "." + version.Minor.ToString("00") + "." + version.Build.ToString("0");
			}
		}

		public static string VersionStringFromNumber(float asNumber)
		{
			return (asNumber / 100).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "." + (asNumber % 100).ToString("00.0");
			// NB Mod does work with singles (100.45 Mod 100 = 0.45)
		}

		public static float VersionNumberFromString(string asString)
		{
			string[] parts = asString.Split('.');
			float asNumber = int.Parse(parts[0]) * 100 + int.Parse(parts[1]);
			if (parts.Length == 3)
			{
				asNumber += int.Parse(parts[2]) / 10f; // should only be one character after the last decimal point
				Debug.Assert(parts[2].Length == 1);
			}
			return asNumber;
		}

	}

}
