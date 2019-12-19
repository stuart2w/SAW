using System.Diagnostics;

namespace SAW
{
/// <summary>Some functions for formatting measurements for user display.  The name of the class is an artefact of the Splash code</summary>
	public class Measure
	{

		// configured globally
		public enum Units
		{
			mm = 0, // the names here should appear in strings.txt
			cm = 1,
			inch = 2,
			GridSquare = 3 // Reverts to centimetre is no grid
		}

		public static Units CurrentUnits()
		{
			return (Units)Globals.Root.CurrentConfig.ReadInteger("Units", 0);
		}

		public static float UnitSizeInMM(Units units)
		{
			// length of current unit in MM
			switch (units)
			{
				case Units.mm:
					return Geometry.MILLIMETRE;
				case Units.cm:
					return Geometry.MILLIMETRE * 10;
				case Units.inch:
					return Geometry.INCH;
				case Units.GridSquare:
					Page objPage = Globals.Root.CurrentPage;
					if (objPage == null)
						return Geometry.MILLIMETRE * 10;
					return objPage.Paper.UnitStep; // Also defaults to 1 cm
				default:
					Debug.Fail("Unexpected units: " + units);
					return 1;
			}
		}

		public static string UnitNumberFormat(Units units)
		{
			// format string to use when converting to numbers for various units (inches/cm use more decimal places)
			switch (units)
			{
				case Units.cm:
				case Units.inch:
					return "0.00";
				case Units.GridSquare:
					return "0";
				default:
#if DEBUG
					return "0.0";
#else
					return "0";
#endif
			}
		}

		public static float UnitSizeInMM()
		{
			return UnitSizeInMM(CurrentUnits());
		}

		public static string FormatLength(float length)
		{
			Units units = CurrentUnits();
			return (length / UnitSizeInMM(units)).ToString(UnitNumberFormat(units)) + " " + Strings.Item(units.ToString());
		}

		public static string FormatLengthWithoutUnits(float length)
		{
			// as above, but it is the name for the units
			Units units = CurrentUnits();
			return (length / UnitSizeInMM(units)).ToString(UnitNumberFormat(units));
		}

		public static string FormatAngle(float angle)
		{
			// at the moment we can't change the units on these; but it makes sense to direct any GUI through here just in case
			return Geometry.Degrees(angle).ToString("0.0") + "°";
		}

	}

}
